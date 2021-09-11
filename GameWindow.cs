﻿using Mycraft.WorldUI;
using Mycraft.Utils;
using Mycraft.World;
using OpenGL;
using System;
using System.Drawing;
using System.Windows.Forms;
using Mycraft.Graphics;

namespace Mycraft
{
    public class GameWindow : Form
    {
        private readonly GlControl glControl;

        private Origin origin;
        private GameWorld world;
        private Selection selection;

        private const float MOVEMENT_SPEED = .05f, MOUSE_SENSIVITY = .002f;
        private readonly Camera camera;
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
            glControl.MouseMove += OnMouseMove;
            glControl.MouseDown += OnMouseDown;
            glControl.ContextCreated += OnContextCreated;
            glControl.ContextDestroying += OnContextDestroyed;
            glControl.ContextUpdate += OnContextUpdate;
            glControl.Render += Render;

            Controls.Add(glControl);
            Cursor.Hide();
            ResumeLayout(false);

            camera = new Camera(new Vertex3f(.5f, 3.5f, .5f), new Vertex2f(0f, 0f));
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
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
            camera.Rotate(MOUSE_SENSIVITY * dx, -MOUSE_SENSIVITY * dy);

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
        }

        private void OnContextCreated(object sender, GlControlEventArgs e)
        {
            Resources.LoadAll();

            OnResized(null, null);

            origin = new Origin();
            selection = new Selection();

            Gl.ClearColor(0.53f, 0.81f, 0.98f, 1f);
            Gl.LineWidth(2f);
            Gl.Enable(EnableCap.CullFace);
            Gl.Enable(EnableCap.Multisample);

            Gl.UseProgram(Resources.TexturedShader.glId);
            Resources.TexturedShader.Texture = 0;

            world = new GameWorld();
            world.GenerateSpawnArea();
            world.RegenerateMesh();
        }

        private void OnContextUpdate(object sender, GlControlEventArgs e)
        {
            int cameraForwardInput    = FuncUtils.GetInput1d(Keys.W, Keys.S);
            int cameraHorizontalInput = FuncUtils.GetInput1d(Keys.D, Keys.A);
            int cameraVerticalInput   = FuncUtils.GetInput1d(Keys.Space, Keys.LShiftKey);

            camera.MoveRelativeToYaw(MOVEMENT_SPEED * cameraForwardInput, MOVEMENT_SPEED * cameraHorizontalInput);
            camera.Translate(0f, MOVEMENT_SPEED * cameraVerticalInput, 0f);
            camera.UpdateTransformMatrix();

            if (RayCasting.Raycast(world, camera.Position, camera.Forward, out Hit hit))
                selection.Select(hit.blockCoords, hit.side);
            else
                selection.Deselect();
        }

        private void DrawCamera(Camera camera)
        {
            float[] positionVertices = {
                 -.1f,   0f,   0f,
                  .1f,   0f,   0f,
                   0f, -.1f,   0f,
                   0f,  .1f,   0f,
                   0f,   0f, -.1f,
                   0f,   0f,  .1f
            };

            Vertex3f look = camera.Forward;
            float[] lookVertices = {
                0f,     0f,     0f,
                look.x, look.y, look.z
            };

            Resources.WorldUIShader.Model = Matrix4x4f.Translated(
                camera.Position.x,
                camera.Position.y,
                camera.Position.z
            );

            Resources.WorldUIShader.Color = new Vertex3f(0f, 0f, 0f);
            using (VertexArray positionGraphics = new VertexArray(PrimitiveType.Lines, new int[] { 3 }, positionVertices))
                positionGraphics.Draw();

            Resources.WorldUIShader.Color = new Vertex3f(1f, 0f, 0f);
            using (VertexArray lookGraphics = new VertexArray(PrimitiveType.Lines, new int[] { 3 }, lookVertices))
                lookGraphics.Draw();
        }

        private void Render(object sender, GlControlEventArgs e)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4x4f vp = projection * camera.TransformMatrix;

            // Draw the world
            Gl.UseProgram(Resources.TexturedShader.glId);
            Gl.Enable(EnableCap.DepthTest);
            Resources.TexturedShader.MVP = vp;
            world.Draw();

            // Draw UI stuff
            Gl.UseProgram(Resources.WorldUIShader.glId);
            
            Resources.WorldUIShader.VP = vp;
            Resources.WorldUIShader.Model = Matrix4x4f.Identity;
            origin.Draw();
            selection.Draw();

            Resources.WorldUIShader.VP = Matrix4x4f.Identity;
            Resources.WorldUIShader.Model = Matrix4x4f.Identity;
            Resources.WorldUIShader.Color = new Vertex3f(.1f, .1f, .1f);
            Gl.Disable(EnableCap.DepthTest);
            using (VertexArray cursor = new VertexArray(
                PrimitiveType.Lines, new int[] { 3 }, new float[]
                {
                    -.05f,  .0f,  .0f,
                     .05f,  .0f,  .0f,
                     .0f,  -.07f, .0f,
                     .0f,   .07f, .0f
                })
            )
                cursor.Draw();
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
