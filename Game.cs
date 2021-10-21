using System;
using OpenGL;

using Mycraft.GUI;
using Mycraft.Physics;
using Mycraft.Utils;
using Mycraft.World;
using Mycraft.WorldUI;
using Mycraft.Blocks;
using Mycraft.World.Generation;
using System.Windows.Forms;
using Mycraft.Graphics;

// TODO make seasons
// TODO make neighbour caching in getblock and getlight in chunk mesh generation

namespace Mycraft
{
    public class Game : IDisposable
    {
        private enum SkyState
        {
            Normal,
            UnderWater,
            Void
        }

        private const float MOVEMENT_ACCELERATION = 20f, MOVEMENT_SPEED = 3.7f;
        private const float MAX_ACCENDING_SPEED = 2f, ACCENDING_ACCELERATION = 4f;

        private Matrix4x4f projection;
        private SkyState skyState = (SkyState)(-1);
        private Block blockIn;

        private Origin origin;
        private GameWorld world;
        private GUIRectangle cross;
        private ParticleSystem particles;

        private SmoothChangingVertex2f playerMovement;
        private Player player;
        private Hotbar hotbar;

        public void Init()
        {
            // Configure the graphics

            Gl.LineWidth(2f);
            Gl.Enable(EnableCap.Multisample);
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Gl.UseProgram(Resources.GameWorldShader.glId);
            Resources.GameWorldShader.Alpha = .6f;
            Resources.GameWorldShader.FogDensity = 16f;
            Resources.GameWorldShader.LightMapScale = new Vertex3f(
                Chunk.SIZE, Chunk.HEIGHT, Chunk.SIZE
            );
            Resources.GameWorldShader.LightMap = 1;

            // Create the GUI

            cross = new GUIRectangle(
                new Vertex2i(),
                new Vertex2i()
            );

            hotbar = new Hotbar(
                new Vertex2i(), 0,
                0, new Block[]
                {
                    BlockRegistry.Stone,
                    BlockRegistry.Grass,
                    BlockRegistry.Dirt,
                    BlockRegistry.Log,
                    BlockRegistry.Leaves,
                    BlockRegistry.Water,
                    null, null, null, null
                }
            );

            // Create the game objects

            origin = new Origin();

            world = new GameWorld(new SimpleWorldGenerator());
            world.GenerateSpawnArea();

            playerMovement = new SmoothChangingVertex2f(new Vertex2f(), MOVEMENT_ACCELERATION);
            player = new Player(world, new Vertex3f(.5f, world.GetGroundLevel(0, 0) + 1f, .5f));
            world.Update(player.camera.Position, true);

            particles = new ParticleSystem(world, .2f, .5d);
        }

        public void MoveHotbarSelection(int delta)
        {
            int select = hotbar.Selected + delta;
            if (select < 0)
                hotbar.Selected = Hotbar.CAPACITY - 1;
            else
                hotbar.Selected = select % Hotbar.CAPACITY;
        }

        public void RotateCamera(float dpitch, float dyaw)
        {
            player.RotateCamera(dpitch, dyaw);
        }

        public void BreakBlock()
        {
            if (!player.Selection.IsSelected)
                return;

            Vertex3i position = player.Selection.Position;
            Block block = world.GetBlock(
                position.x,
                position.y,
                position.z
            );

            world.SetBlock(
                position.x,
                position.y,
                position.z,
                BlockRegistry.Air
            );

            particles.Spawn(
                (Vertex3f)position,
                (Vertex3f)position + new Vertex3f(1f, 1f, 1f),
                30, block
            );
        }

        public void PlaceBlock()
        {
            if (!player.Selection.IsSelected)
                return;

            if (!(hotbar.SelectedBlock is null))
            {
                Vertex3i placeBlockCoords = Block.GetNeighbour(
                    player.Selection.Position,
                    player.Selection.Side
                );

                if (!hotbar.SelectedBlock.HasCollider ||
                    !player.Intersects(
                        new AABB(
                            (Vertex3f)placeBlockCoords,
                            new Vertex3f(1f, 1f, 1f)
                        )
                    )
                )
                    world.SetBlock(
                        placeBlockCoords.x,
                        placeBlockCoords.y,
                        placeBlockCoords.z,
                        hotbar.SelectedBlock
                    );
            }
        }

        public void Resize(int width, int height)
        {
            projection = Matrix4x4f.Perspective(
                70, (float)width / height,
                .01f, GameWorld.UNLOAD_DISTANCE * Chunk.SIZE * 2f
            );

            Gl.UseProgram(Resources.GUIShader.glId);
            Resources.GUIShader.Projection = Matrix4x4f.Ortho2D(0f, width - 1, height - 1, 0f);

            // Update the GUI

            int pixelSize = height / 200;

            cross.Resize(
                new Vertex2i(
                    width / 2 - 6 * pixelSize,
                    height / 2 - 6 * pixelSize
                ),
                new Vertex2i(12 * pixelSize, 12 * pixelSize)
            );

            hotbar.Resize(
                new Vertex2i(
                    width / 2 - 91 * pixelSize,
                    height - 21 * pixelSize
                ),
                pixelSize
            );
        }

        public void Update(double deltaTime)
        {
            // Player movement

            int forwardInput = FuncUtils.GetInput1d(Keys.W, Keys.S);
            int horizontalInput = FuncUtils.GetInput1d(Keys.D, Keys.A);

            if (forwardInput != 0 || horizontalInput != 0)
                playerMovement.Value = new Vertex2f(
                    forwardInput, horizontalInput
                ).Normalized * MOVEMENT_SPEED;
            else
                playerMovement.Value = new Vertex2f();

            playerMovement.Update(deltaTime);
            player.MoveRelativeToYaw(
                (float)(playerMovement.Value.x * deltaTime),
                (float)(playerMovement.Value.y * deltaTime)
            );

            Vertex3f velocity = player.Velocity;

            if (FuncUtils.IsKeyPressed(Keys.Space))
            {
                if (player.IsGrounded)
                    velocity.y = 6f;

                if (player.IsInWater && velocity.y < MAX_ACCENDING_SPEED)
                    velocity.y += (float)(ACCENDING_ACCELERATION * deltaTime);
            }

            // Jump off the void

            if (player.Position.y < -64f && velocity.y < 0f)
                velocity.y *= -1f;

            player.Velocity = velocity;

            // Update the game objects

            player.Update(deltaTime);
            world.Update(player.camera.Position);
            particles.Update(deltaTime);

            // Update the sky state

            blockIn = world.GetBlock(
                (int)Math.Floor(player.camera.Position.x),
                (int)Math.Floor(player.camera.Position.y),
                (int)Math.Floor(player.camera.Position.z)
            );

            SkyState newSkyState;
            if (player.camera.Position.y < 0)
                newSkyState = SkyState.Void;
            else if (blockIn is LiquidBlock)
                newSkyState = SkyState.UnderWater;
            else
                newSkyState = SkyState.Normal;

            if (newSkyState == skyState)
                return;
            skyState = newSkyState;

            // Update the fog

            Gl.UseProgram(Resources.GameWorldShader.glId);

            if (skyState is SkyState.UnderWater)
                Resources.GameWorldShader.FogDistance = 16f;
            else
                Resources.GameWorldShader.FogDistance = GameWorld.LOAD_DISTANCE * 16f - 24f;

            // Update the sky color

            Vertex3f skyColor;
            switch (newSkyState)
            {
                case SkyState.Normal:
                    skyColor = new Vertex3f(0.53f, 0.81f, 0.98f);
                    break;

                case SkyState.UnderWater:
                    skyColor = new Vertex3f(0.53f, 0.81f, 0.98f);
                    break;

                case SkyState.Void:
                    skyColor = new Vertex3f(.05f, .05f, .05f);
                    break;

                default:
                    return;
            }

            Gl.ClearColor(skyColor.x, skyColor.y, skyColor.z, 1f);
            Resources.GameWorldShader.FogColor = skyColor;
        }

        public void Draw()
        {
            Matrix4x4f vp = projection * player.camera.TransformMatrix;

            // Draw the world

            Gl.UseProgram(Resources.GameWorldShader.glId);
            Gl.Enable(EnableCap.DepthTest);

            Resources.GameWorldShader.View = player.camera.TransformMatrix;
            Resources.GameWorldShader.Projection = projection;
            world.Draw();

            // Draw particles

            Gl.UseProgram(Resources.ParticleShader.glId);
            Resources.ParticleShader.View = player.camera.TransformMatrix;
            Resources.ParticleShader.Projection = projection;
            Resources.BlocksTexture.Bind();
            particles.Draw();

            // Draw UI stuff

            Gl.UseProgram(Resources.WorldUIShader.glId);
            Resources.WorldUIShader.VP = vp;
            player.Selection.Draw();
            origin.Draw();

            // Draw vignette

            Gl.Disable(EnableCap.DepthTest);
            Gl.Disable(EnableCap.CullFace);

            if (blockIn is LiquidBlock)
            {
                Gl.UseProgram(Resources.OverlayShader.glId);
                Gl.Enable(EnableCap.Blend);

                Resources.BlocksTexture.Bind();
                Vertex4f texture = Block.GetTextureCoords(blockIn.GetTexture(BlockSide.Top));
                using (VertexArray overlay = new VertexArray(
                    PrimitiveType.Quads, Resources.OverlayShader,
                    new float[]
                    {
                         1f,  1f,  texture.z, texture.y,
                         1f, -1f,  texture.z, texture.w,
                        -1f, -1f,  texture.x, texture.w,
                        -1f,  1f,  texture.x, texture.y
                    }
                )) overlay.Draw();
            }

            // Draw GUI

            Gl.UseProgram(Resources.GUIShader.glId);
            Gl.Disable(EnableCap.Blend);

            Resources.CrossTexture.Bind();
            cross.Draw();

            hotbar.Draw();
        }

        public void Dispose()
        {
            world.Dispose();
            origin.Dispose();
            player.Dispose();
            hotbar.Dispose();
        }
    }
}
