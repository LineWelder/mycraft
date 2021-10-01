using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenGL;

using Mycraft.Blocks;
using Mycraft.GUI;
using Mycraft.Graphics;
using Mycraft.Physics;
using Mycraft.Utils;
using Mycraft.World;
using Mycraft.World.Generation;
using Mycraft.WorldUI;

// TODO make good in-water physics
// TODO make pretty methods for creating planes for mesh generation

namespace Mycraft
{
    public class GameWindow : Form
    {
        private readonly GlControl glControl;
        private readonly Stopwatch stopwatch;

        private Origin origin;
        private GameWorld world;
        private GUIRectangle cross;

        private Player player;
        private Hotbar hotbar;

        private ParticleSystem particles;

        private const float MOVEMENT_SPEED = 3.7f, MOUSE_SENSIVITY = .003f;
        private Matrix4x4f projection;

        public GameWindow()
        {
            SuspendLayout();

            Name = "GameWindow";
            Text = "Mycraft";
            KeyPreview = true;
            WindowState = FormWindowState.Maximized;

            glControl = new GlControl
            {
                Name = "GLControl",
                Dock = DockStyle.Fill,

                Animation = true,
                AnimationTimer = false,

                ColorBits = 24u,
                DepthBits = 8u,
                MultisampleBits = 4u,
                StencilBits = 0u
            };

            Resize += OnResized;
            glControl.MouseEnter += (o, e) => Cursor.Hide();
            glControl.MouseMove += OnMouseMove;
            glControl.MouseDown += OnMouseDown;
            glControl.MouseWheel += OnMouseWheel;
            glControl.ContextCreated += OnContextCreated;
            glControl.ContextDestroying += OnContextDestroyed;
            glControl.ContextUpdate += OnContextUpdate;
            glControl.Render += Render;

            Controls.Add(glControl);
            ResumeLayout(false);

            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            int select = hotbar.Selected - e.Delta / 120;
            if (select < 0)
                hotbar.Selected = Hotbar.CAPACITY - 1;
            else
                hotbar.Selected = select % Hotbar.CAPACITY;
        }

        private (float dx, float dy) GrabCursor()
        {
            Point screenLocation = Location;
            Point screenCenter = new Point(ClientSize.Width / 2, ClientSize.Height / 2);
            Point cursorPos = new Point(
                screenLocation.X + screenCenter.X,
                screenLocation.Y + screenCenter.Y
            );

            float dx = Cursor.Position.X - cursorPos.X;
            float dy = Cursor.Position.Y - cursorPos.Y;

            Cursor.Position = cursorPos;
            return (dx, dy);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var (dx, dy) = GrabCursor();
            player.RotateCamera(MOUSE_SENSIVITY * dx, -MOUSE_SENSIVITY * dy);
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (player.Selection.IsSelected)
            {
                if (e.Button == MouseButtons.Left)
                {
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
                        20, block
                    );
                }
                else if (e.Button == MouseButtons.Right)
                {
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
            }
        }
           
        private void OnResized(object sender, EventArgs e)
        {
            if (!Resources.AreLoaded) return;

            // Set the viewport and the projection

            Gl.Viewport(0, 0, ClientSize.Width, ClientSize.Height);
            projection = Matrix4x4f.Perspective(
                70, (float)ClientSize.Width / ClientSize.Height,
                .01f, GameWorld.UNLOAD_DISTANCE * Chunk.SIZE * 2f 
            );

            Gl.UseProgram(Resources.GUIShader.glId);
            Resources.GUIShader.Projection = Matrix4x4f.Ortho2D(0f, ClientSize.Width - 1, ClientSize.Height - 1, 0f);

            // Update the GUI

            int pixelSize = ClientSize.Height / 200;

            cross = new GUIRectangle(
                new Vertex2i(
                    ClientSize.Width / 2 - 6 * pixelSize,
                    ClientSize.Height / 2 - 6 * pixelSize
                ),
                new Vertex2i(12 * pixelSize, 12 * pixelSize)
            );

            hotbar = new Hotbar(
                new Vertex2i(
                    ClientSize.Width / 2 - 91 * pixelSize,
                    ClientSize.Height - 21 * pixelSize
                ),
                pixelSize,
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
        }

        private void OnContextCreated(object sender, GlControlEventArgs e)
        {
            Resources.LoadAll();

            // Configure the graphics

            Gl.LineWidth(2f);
            Gl.Enable(EnableCap.Multisample);
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Gl.UseProgram(Resources.GameWorldShader.glId);
            Resources.GameWorldShader.Alpha = .6f;

            // Screen stuff

            OnResized(null, null);
            Cursor.Hide();
            GrabCursor();

            // Create the game objects

            origin = new Origin();

            world = new GameWorld(new SimpleWorldGenerator());
            world.GenerateSpawnArea();

            player = new Player(world, new Vertex3f(.5f, world.GetGroundLevel(0, 0) + 1f, .5f));
            world.Update(player.camera.Position, true);

            particles = new ParticleSystem(world, .2f, .5d);
        }

        private void OnContextUpdate(object sender, GlControlEventArgs e)
        {
            double deltaTime = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();

            Text = $"Mycraft - UPS: {(int)Math.Floor(1d / deltaTime)}";

            // Player movement

            int forwardInput    = FuncUtils.GetInput1d(Keys.W, Keys.S);
            int horizontalInput = FuncUtils.GetInput1d(Keys.D, Keys.A);

            float movementAmount = (float)(deltaTime * MOVEMENT_SPEED);
            player.MoveRelativeToYaw(
                forwardInput * movementAmount,
                horizontalInput * movementAmount
            );

            Vertex3f velocity = player.Velocity;

            if (FuncUtils.IsKeyPressed(Keys.Space))
            {
                if (player.IsGrounded)
                    velocity.y = 6f;

                if (player.IsInWater)
                    velocity.y += (float)(20f * deltaTime);
            }

            // Jump off the void

            if (player.Position.y < -64f && velocity.y < 0f)
                velocity.y *= -1f;

            player.Velocity = velocity;

            // float speed = (float)(deltaTime * MOVEMENT_SPEED);
            // camera.MoveRelativeToYaw(
            //     speed * forwardInput,
            //     speed * horizontalInput
            // );
            // camera.Translate(0f, FuncUtils.GetInput1d(Keys.Space, Keys.LShiftKey) * speed, 0f);

            // Update the game objects

            player.Update(deltaTime);
            world.Update(player.camera.Position);
            particles.Update(deltaTime);

            // Update the graphics

            if (player.camera.Position.y < 0)
                Gl.ClearColor(.05f, .05f, .05f, 1f);
            else
                Gl.ClearColor(0.53f, 0.81f, 0.98f, 1f);
        }

        private void Render(object sender, GlControlEventArgs e)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4x4f vp = projection * player.camera.TransformMatrix;

            // Draw the world

            Gl.UseProgram(Resources.GameWorldShader.glId);
            Gl.Enable(EnableCap.DepthTest);

            Resources.GameWorldShader.MVP = vp;
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

            Resources.WorldUIShader.Model = Matrix4x4f.Identity;
            origin.Draw();

            // Draw vignette

            Gl.Disable(EnableCap.DepthTest);
            Gl.Disable(EnableCap.CullFace);

            Block block = world.GetBlock(
                (int)Math.Floor(player.camera.Position.x),
                (int)Math.Floor(player.camera.Position.y),
                (int)Math.Floor(player.camera.Position.z)
            );

            if (block is LiquidBlock)
            {
                Gl.UseProgram(Resources.OverlayShader.glId);

                Gl.Enable(EnableCap.Blend);

                Resources.BlocksTexture.Bind();
                Vertex4f texture = Block.GetTextureCoords(block.GetTexture(BlockSide.Top));
                using (VertexArray overlay = new VertexArray(
                    PrimitiveType.Quads, new int[] { 2, 2 },
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

        private void OnContextDestroyed(object sender, GlControlEventArgs e)
        {
            Resources.DisposeAll();
            world.Dispose();
            origin.Dispose();
            player.Dispose();
            hotbar.Dispose();
        }
    }
}
