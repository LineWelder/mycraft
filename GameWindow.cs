using Mycraft.WorldUI;
using Mycraft.Utils;
using Mycraft.World;
using OpenGL;
using System;
using System.Drawing;
using System.Windows.Forms;
using Mycraft.Graphics;
using Mycraft.GUI;
using Mycraft.Physics;
using System.Collections.Generic;

namespace Mycraft
{
    public class GameWindow : Form
    {
        private readonly GlControl glControl;

        private Origin origin;
        private GameWorld world;
        private Selection selection;
        private GUIRectangle cross;

        private Box playerBoxGraphics;
        private FallingBox playerBox;

        private const float MOVEMENT_SPEED = .05f, MOUSE_SENSIVITY = .004f;
        private Camera camera;
        private Matrix4x4f projection;

        public GameWindow()
        {
            SuspendLayout();

            Name = "GameWindow";
            Text = "Mycraft";
            KeyPreview = true;
            ClientSize = new Size(1920, 1080);

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
            glControl.ContextCreated += OnContextCreated;
            glControl.ContextDestroying += OnContextDestroyed;
            glControl.ContextUpdate += OnContextUpdate;
            glControl.Render += Render;

            Controls.Add(glControl);
            ResumeLayout(false);
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

            Vertex2f rotation = camera.Rotation;
            rotation.x += MOUSE_SENSIVITY * dx;
            rotation.y += -MOUSE_SENSIVITY * dy;

            if (rotation.x >= 2f * (float)Math.PI)
                rotation.x -= 2f * (float)Math.PI;
            else if (rotation.x < 0)
                rotation.x += 2f * (float)Math.PI;

            if (rotation.y > .5f * (float)Math.PI)
                rotation.y = .5f * (float)Math.PI;
            else if (rotation.y < -.5f * (float)Math.PI)
                rotation.y = -.5f * (float)Math.PI;

            camera.Rotation = rotation;
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (selection.IsSelected)
            {
                if (e.Button == MouseButtons.Left)
                {
                    world.SetBlock(
                        selection.Position.x,
                        selection.Position.y,
                        selection.Position.z,
                        Block.Air
                    );
                    world.RegenerateMesh();
                }
                else if (e.Button == MouseButtons.Right)
                {
                    Vertex3i placeBlockCoords = GameWorld.GetNeighbour(selection.Position, selection.Side);
                    world.SetBlock(
                        placeBlockCoords.x,
                        placeBlockCoords.y,
                        placeBlockCoords.z,
                        Block.Test
                    );
                    world.RegenerateMesh();
                }
            }
        }

        private void OnResized(object sender, EventArgs e)
        {
            Gl.Viewport(0, 0, ClientSize.Width, ClientSize.Height);
            projection = Matrix4x4f.Perspective(70, (float)ClientSize.Width / ClientSize.Height, .01f, 100f);

            Gl.UseProgram(Resources.GUIShader.glId);
            Resources.GUIShader.Projection = Matrix4x4f.Ortho2D(0f, ClientSize.Width - 1, ClientSize.Height - 1, 0f);

            cross = new GUIRectangle(
                new Vertex2i(ClientSize.Width / 2 - 20, ClientSize.Height / 2 - 20),
                new Vertex2i(40, 40)
            );
        }

        private void OnContextCreated(object sender, GlControlEventArgs e)
        {
            Resources.LoadAll();

            OnResized(null, null);
            Cursor.Hide();
            GrabCursor();

            camera = new Camera(new Vertex3f(.5f, 3.5f, .5f), new Vertex2f(0f, 0f));

            origin = new Origin();
            selection = new Selection();

            Gl.LineWidth(2f);
            Gl.Enable(EnableCap.CullFace);
            Gl.Enable(EnableCap.Multisample);

            world = new GameWorld();
            world.GenerateSpawnArea();
            world.RegenerateMesh();

            playerBox = new FallingBox(world, new Vertex3f(.25f, 3f, -4.75f), new Vertex3f(.5f, .5f, .5f));
            playerBoxGraphics = new Box(new Vertex3f(0f, 0f, 0f), playerBox.Size, new Vertex3f(0f, 0f, 1f));
        }

        private void OnContextUpdate(object sender, GlControlEventArgs e)
        {
            int cameraForwardInput    = FuncUtils.GetInput1d(Keys.W, Keys.S);
            int cameraHorizontalInput = FuncUtils.GetInput1d(Keys.D, Keys.A);
            int cameraVerticalInput   = FuncUtils.GetInput1d(Keys.Space, Keys.LShiftKey);

            int boxForwardInput = FuncUtils.GetInput1d(Keys.U, Keys.J);
            int boxHorizontalInput = FuncUtils.GetInput1d(Keys.H, Keys.K);

            camera.MoveRelativeToYaw(MOVEMENT_SPEED * cameraForwardInput, MOVEMENT_SPEED * cameraHorizontalInput);
            camera.Translate(0f, MOVEMENT_SPEED * cameraVerticalInput, 0f);
            camera.UpdateTransformMatrix();

            if (RayCasting.Raycast(world, camera.Position, camera.Forward, out Hit hit))
                selection.Select(hit.blockCoords, hit.side);
            else
                selection.Deselect();

            if (camera.Position.y < 0)
                Gl.ClearColor(.05f, .05f, .05f, 1f);
            else
                Gl.ClearColor(0.53f, 0.81f, 0.98f, 1f);

            playerBox.Move(new Vertex3f(boxHorizontalInput, 0f, boxForwardInput) * MOVEMENT_SPEED);
            playerBox.Update();
            if (playerBox.IsGrounded && FuncUtils.IsKeyPressed(Keys.Y))
            {
                Vertex3f velocity = playerBox.Velocity;
                velocity.y = .1f;
                playerBox.Velocity = velocity;
            }
        }

        private void DrawPosition(Vertex3f position)
        {
            float[] vertices = {
                 -.1f,   0f,   0f,
                  .1f,   0f,   0f,
                   0f, -.1f,   0f,
                   0f,  .1f,   0f,
                   0f,   0f, -.1f,
                   0f,   0f,  .1f
            };

            Resources.WorldUIShader.Model = FuncUtils.TranslateBy(position);
            Resources.WorldUIShader.Color = new Vertex3f(0f, 0f, 0f);
            using (VertexArray vao = new VertexArray(PrimitiveType.Lines, new int[] { 3 }, vertices))
                vao.Draw();
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

            // Draw UI stuff
            Gl.UseProgram(Resources.WorldUIShader.glId);
            Resources.WorldUIShader.VP = vp;
            selection.Draw();

            Resources.WorldUIShader.Model = Matrix4x4f.Identity;
            origin.Draw();

            Resources.WorldUIShader.Model = FuncUtils.TranslateBy(playerBox.Position);
            playerBoxGraphics.Draw();

            // Draw GUI
            Gl.UseProgram(Resources.GUIShader.glId);
            Gl.Disable(EnableCap.DepthTest);

            Resources.CrossTexture.Bind();
            cross.Draw();
        }

        private void OnContextDestroyed(object sender, GlControlEventArgs e)
        {
            Resources.DisposeAll();
            world.Dispose();
            origin.Dispose();
            selection.Dispose();
        }
    }
}
