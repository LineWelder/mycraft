using System;
using System.Windows.Forms;
using OpenGL;

using Mycraft.Blocks;
using Mycraft.Graphics;
using Mycraft.GUI;
using Mycraft.Physics;
using Mycraft.Utils;
using Mycraft.World;
using Mycraft.World.Generation;
using Mycraft.WorldUI;

// TODO make seasons
// TODO make neighbour caching in getblock and getlight in chunk mesh generation

namespace Mycraft
{
    public class Game : IDisposable
    {
        private const float MOUSE_SENSIVITY = .3f;
        private const float MOVEMENT_ACCELERATION = 20f, MOVEMENT_SPEED = 3.7f;
        private const float MAX_ACCENDING_SPEED = 2f, ACCENDING_ACCELERATION = 4f;
        private const float DAY_CYCLE_SPEED = .005f;

        private static readonly float[] screenQuad =
        {
            1f,  1f,
            1f, -1f,
           -1f, -1f,
           -1f,  1f,
        };

        private Matrix4x4f projection;
        private Block blockIn;

        private Origin origin;
        private ChunkBorders chunkBorders;
        private GameWorld world;
        private GUIRectangle cross;
        private ParticleSystem particles;

        private SmoothChangingVertex2f playerMovement;
        private Player player;
        private Hotbar hotbar;

        private float time = 0.3f;

        private GUIRectangle imageShowcase;
        private LightComputeShader computeShader;

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
                Chunk.SIZE + 1, Chunk.HEIGHT, Chunk.SIZE + 1
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
                    BlockRegistry.Sand,
                    BlockRegistry.Planks,
                    BlockRegistry.Torch,
                    BlockRegistry.RedFlower,
                    BlockRegistry.YellowFlower
                }
            );

            // Create the game objects

            origin = new Origin();
            chunkBorders = new ChunkBorders();

            world = new GameWorld(new SimpleWorldGenerator(1337));
            world.GenerateSpawnArea();

            playerMovement = new SmoothChangingVertex2f(new Vertex2f(), MOVEMENT_ACCELERATION);
            player = new Player(world, new Vertex3f(.5f, world.GetGroundLevel(0, 0) + 1f, .5f));
            world.ObservingCamera = player.camera;
            world.Update();

            particles = new ParticleSystem(world, .2f, .5d);

            imageShowcase = new GUIRectangle(new Vertex2i(), new Vertex2i());
            computeShader = new LightComputeShader();
            computeShader.Run();
        }

        public void MoveHotbarSelection(int delta)
        {
            int select = hotbar.Selected + delta;
            if (select < 0)
                hotbar.Selected = Hotbar.CAPACITY - 1;
            else
                hotbar.Selected = select % Hotbar.CAPACITY;
        }

        public void MouseInput(float dx, float dy)
        {
            player.RotateCamera(dx * MOUSE_SENSIVITY, -dy * MOUSE_SENSIVITY);
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

            Matrix4x4f guiProjection = Matrix4x4f.Ortho2D(0f, width - 1, height - 1, 0f);
            Gl.UseProgram(Resources.GUIShader.glId);
            Resources.GUIShader.Projection = guiProjection;
            Gl.UseProgram(Resources.BlockViewShader.glId);
            Resources.BlockViewShader.Projection = guiProjection;

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

            imageShowcase.Resize(
                new Vertex2i(
                    width / 2 - 16 * pixelSize,
                    height / 2 - 16 * pixelSize
                ),
                new Vertex2i(32 * pixelSize, 32 * pixelSize)
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
            particles.Update(deltaTime);
            chunkBorders.Update(player.camera.Position);
            world.Update();

            // Update the sky state

            time += (float)(DAY_CYCLE_SPEED * deltaTime);
            if (time > 1f)
                time = 0f;

            Gl.UseProgram(Resources.SkyShader.glId);
            Resources.SkyShader.Time = time;

            Vertex3f skyColor, fogColor;
            if (player.camera.Position.y < 0)
            {
                skyColor = new Vertex3f(.05f, .05f, .05f);
                fogColor = new Vertex3f(.05f, .05f, .05f);
            }
            else
            {
                double time_ = time;
                if (time_ > .5d)
                {
                    time_ = 1d - time_;
                }

                double mix = 1d / ( 1d + Math.Exp(-40d * (time_ - .25d)) );

                Vertex3f nightSkyColor = new Vertex3f(0.00f, 0.00f, 0.09f);
                Vertex3f daySkyColor   = new Vertex3f(0.43f, 0.77f, 0.98f);
                skyColor = nightSkyColor + (daySkyColor - nightSkyColor) * mix;

                Vertex3f nightFogColor = new Vertex3f(0.00f, 0.01f, 0.17f);
                Vertex3f dayFogColor   = new Vertex3f(0.53f, 0.81f, 0.98f);
                fogColor = nightFogColor + (dayFogColor - nightFogColor) * mix;

                double sunset = 1d / (1d + Math.Pow(40d * (time_ - .25d), 4d));

                Vertex3f sunsetColor = new Vertex3f(1.00f, 0.60f, 0.40f);
                fogColor += (sunsetColor - fogColor) * sunset;
            }

            Gl.ClearColor(fogColor.x, fogColor.y, fogColor.z, 1f);
            Resources.SkyShader.SkyColor = skyColor;
            Resources.SkyShader.FogColor = fogColor;
            
            Gl.UseProgram(Resources.GameWorldShader.glId);
            Resources.GameWorldShader.FogColor = fogColor;

            blockIn = world.GetBlock(
                (int)Math.Floor(player.camera.Position.x),
                (int)Math.Floor(player.camera.Position.y),
                (int)Math.Floor(player.camera.Position.z)
            );

            if (blockIn is LiquidBlock)
            {
                Resources.GameWorldShader.FogDistance = 16f;
            }
            else
            {
                Resources.GameWorldShader.FogDistance = GameWorld.LOAD_DISTANCE * 16f - 24f;
            }
        }

        public void Draw()
        {
            // Draw the sky

            Gl.UseProgram(Resources.SkyShader.glId);
            Resources.SkyShader.TransformMatrix = player.camera.InversedRotationMatrix * projection.Inverse;

            using (VertexArray skyQuad = new VertexArray(
                PrimitiveType.Quads, Resources.SkyShader,
                screenQuad
            )) skyQuad.Draw();

            // Draw UI stuff

            Gl.Enable(EnableCap.DepthTest);

            Gl.UseProgram(Resources.WorldUIShader.glId);
            Resources.WorldUIShader.VP = projection * player.camera.TransformMatrix;
            player.Selection.Draw();
            origin.Draw();
            chunkBorders.Draw();

            // Draw particles

            Gl.UseProgram(Resources.ParticleShader.glId);
            Resources.ParticleShader.View = player.camera.TransformMatrix;
            Resources.ParticleShader.Projection = projection;
            Resources.BlocksTexture.Bind();
            particles.Draw();

            // Draw the world

            Gl.UseProgram(Resources.GameWorldShader.glId);
            Resources.GameWorldShader.View = player.camera.TransformMatrix;
            Resources.GameWorldShader.Projection = projection;
            world.Draw();

            // Draw vignette

            Gl.Disable(EnableCap.DepthTest);
            Gl.Disable(EnableCap.CullFace);

            if (blockIn is LiquidBlock)
            {
                Gl.UseProgram(Resources.OverlayShader.glId);
                Gl.Enable(EnableCap.Blend);

                Resources.BlocksTexture.Bind();
                Resources.OverlayShader.TextureId = blockIn.GetTexture(BlockSide.Top);
                using (VertexArray overlay = new VertexArray(
                    PrimitiveType.Quads, Resources.OverlayShader,
                    screenQuad
                )) overlay.Draw();
            }

            // Draw GUI

            Gl.UseProgram(Resources.GUIShader.glId);
            Gl.Disable(EnableCap.Blend);

            // Resources.CrossTexture.Bind();
            // cross.Draw();
            computeShader.BindTexture();
            imageShowcase.Draw();

            hotbar.Draw();
        }

        public void Dispose()
        {
            world.Dispose();
            origin.Dispose();
            chunkBorders.Dispose();
            player.Dispose();
            cross.Dispose();
            hotbar.Dispose();
            
            imageShowcase.Dispose();
            computeShader.Dispose();
        }
    }
}
