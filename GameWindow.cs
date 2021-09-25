using Mycraft.WorldUI;
using Mycraft.Utils;
using Mycraft.World;
using OpenGL;
using System;
using System.Drawing;
using System.Windows.Forms;
using Mycraft.GUI;
using Mycraft.Physics;
using System.Diagnostics;
using Mycraft.Blocks;
using Mycraft.World.Generation;
using Mycraft.Graphics;

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
        private Selection selection;
        private GUIRectangle cross;

        private Hotbar hotbar;

        private FallingBox playerBox;
        private ParticleSystem particles;

        private const float MOVEMENT_SPEED = 3.7f, MOUSE_SENSIVITY = .003f;
        private Camera camera;
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
            const float HALF_PI = .5f * (float)Math.PI;

            var (dx, dy) = GrabCursor();

            Vertex2f rotation = camera.Rotation;
            rotation.x = FuncUtils.FixRotation(rotation.x + MOUSE_SENSIVITY * dx);
            rotation.y = FuncUtils.Clamp(-HALF_PI, rotation.y - MOUSE_SENSIVITY * dy, HALF_PI);

            camera.Rotation = rotation;
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (selection.IsSelected)
            {
                if (e.Button == MouseButtons.Left)
                {
                    Vertex3i position = selection.Position;
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
                        Vertex3i placeBlockCoords = Block.GetNeighbour(selection.Position, selection.Side);
                        
                        if (!hotbar.SelectedBlock.HasCollider ||
                            !playerBox.Intersects(
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

            Gl.Viewport(0, 0, ClientSize.Width, ClientSize.Height);
            projection = Matrix4x4f.Perspective(
                70, (float)ClientSize.Width / ClientSize.Height,
                .01f, GameWorld.UNLOAD_DISTANCE * Chunk.SIZE * 2f 
            );

            Gl.UseProgram(Resources.GUIShader.glId);
            Resources.GUIShader.Projection = Matrix4x4f.Ortho2D(0f, ClientSize.Width - 1, ClientSize.Height - 1, 0f);

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

            Gl.UseProgram(Resources.GameWorldShader.glId);
            Resources.GameWorldShader.Alpha = .6f;

            OnResized(null, null);
            Cursor.Hide();
            GrabCursor();

            origin = new Origin();
            selection = new Selection();

            Gl.LineWidth(2f);
            Gl.Enable(EnableCap.Multisample);
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            world = new GameWorld(new SimpleWorldGenerator());
            world.GenerateSpawnArea();

            playerBox = new FallingBox(
                world,
                new Vertex3f(.25f, world.GetGroundLevel(0, 0) + 1f, .25f),
                new Vertex3f(.75f, 1.7f, .75f)
            );
            camera = new Camera(new Vertex3f(.5f, 20.5f, 1.5f), new Vertex2f(0f, 0f));

            world.Update(camera.Position, true);

            particles = new ParticleSystem(world, .2f, .5d);
        }

        private void OnContextUpdate(object sender, GlControlEventArgs e)
        {
            double deltaTime = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();

            Text = $"Mycraft - UPS: {(int)Math.Floor(1d / deltaTime)}";

            int forwardInput    = FuncUtils.GetInput1d(Keys.W, Keys.S);
            int horizontalInput = FuncUtils.GetInput1d(Keys.D, Keys.A);

            playerBox.Move(camera.RelativeToYaw(
                forwardInput,
                horizontalInput
            ) * (deltaTime * MOVEMENT_SPEED));
            playerBox.Update(deltaTime);
            if ((playerBox.IsGrounded || playerBox.IsInWater) && FuncUtils.IsKeyPressed(Keys.Space))
            {
                Vertex3f velocity = playerBox.Velocity;
                velocity.y = 6f;
                playerBox.Velocity = velocity;
            }

            if (playerBox.Position.y < -64f)
            {
                Vertex3f velocity = playerBox.Velocity;
                velocity.y *= -1f;
                playerBox.Velocity = velocity;
            }
            
            // float speed = (float)(deltaTime * MOVEMENT_SPEED);
            // camera.MoveRelativeToYaw(
            //     speed * forwardInput,
            //     speed * horizontalInput
            // );
            // camera.Translate(0f, FuncUtils.GetInput1d(Keys.Space, Keys.LShiftKey) * speed, 0f);
            
            camera.Position = playerBox.Position + new Vertex3f(.375f, 1.5f, .375f);
            camera.UpdateTransformMatrix();

            world.Update(camera.Position);

            if (RayCasting.Raycast(world, camera.Position, camera.Forward, out Hit hit))
                selection.Select(hit.blockCoords, hit.side);
            else
                selection.Deselect();

            if (camera.Position.y < 0)
                Gl.ClearColor(.05f, .05f, .05f, 1f);
            else
                Gl.ClearColor(0.53f, 0.81f, 0.98f, 1f);

            particles.Update(deltaTime);
        }

        private void Render(object sender, GlControlEventArgs e)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4x4f vp = projection * camera.TransformMatrix;

            // Draw the world
            Gl.UseProgram(Resources.GameWorldShader.glId);
            Gl.Enable(EnableCap.DepthTest);

            Resources.GameWorldShader.MVP = vp;
            world.Draw();

            // Draw particles
            Gl.UseProgram(Resources.ParticleShader.glId);
            Resources.ParticleShader.View = camera.TransformMatrix;
            Resources.ParticleShader.Projection = projection;
            Resources.BlocksTexture.Bind();
            particles.Draw();

            // Draw UI stuff
            Gl.UseProgram(Resources.WorldUIShader.glId);
            Resources.WorldUIShader.VP = vp;
            selection.Draw();

            Resources.WorldUIShader.Model = Matrix4x4f.Identity;
            origin.Draw();

            // Draw vignette
            Gl.Disable(EnableCap.DepthTest);
            Gl.Disable(EnableCap.CullFace);

            Block block = world.GetBlock(
                (int)Math.Floor(camera.Position.x),
                (int)Math.Floor(camera.Position.y),
                (int)Math.Floor(camera.Position.z)
            );

            if (block is LiquidBlock)
            {
                Gl.UseProgram(Resources.VignetteShader.glId);

                Gl.Enable(EnableCap.Blend);

                Resources.BlocksTexture.Bind();
                Vertex4f texture = Block.GetTextureCoords(block.GetTexture(BlockSide.Top));
                using (VertexArray vignette = new VertexArray(
                    PrimitiveType.Quads, new int[] { 2, 2 },
                    new float[]
                    {
                         1f,  1f,  texture.z, texture.y,
                         1f, -1f,  texture.z, texture.w,
                        -1f, -1f,  texture.x, texture.w,
                        -1f,  1f,  texture.x, texture.y
                    }
                )) vignette.Draw();
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
            selection.Dispose();
            hotbar.Dispose();
        }
    }
}
