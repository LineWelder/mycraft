﻿using Mycraft.Graphics;
using Mycraft.Utils;
using OpenGL;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mycraft
{
    public class GameWindow : Form
    {
        private readonly GlControl glControl;

        private ColoredShader coloredShader;
        private TexturedShader texturedShader;
        private VertexArray quad, origin;
        private Texture testTexture;

        private readonly float[] quadVertices =
        {
            0f, 0f, 0f, 1f, 1f,
            0f, 1f, 0f, 1f, 0f,
            1f, 1f, 0f, 0f, 0f,
            1f, 0f, 0f, 0f, 1f
        };

        private readonly float[] originVertices =
        {
            0f, 0f, 0f, 1f, 0f, 0f,
            1f, 0f, 0f, 1f, 0f, 0f,
            0f, 0f, 0f, 0f, 1f, 0f,
            0f, 1f, 0f, 0f, 1f, 0f,
            0f, 0f, 0f, 0f, 0f, 1f,
            0f, 0f, 1f, 0f, 0f, 1f
        };

        private const float MOVEMENT_SPEED = .05f, ROTATION_SPEED = .03f;
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
                MultisampleBits = 0u,
                StencilBits = 0u
            };

            Resize += OnResized;
            glControl.ContextCreated += OnContextCreated;
            glControl.ContextDestroying += OnContextDestroyed;
            glControl.ContextUpdate += OnContextUpdate;
            glControl.Render += Render;

            Controls.Add(glControl);
            ResumeLayout(false);

            camera = new Camera(new Vertex3f(0f, 0f, 5f), new Vertex2f(0f, 0f));
        }

        private void OnResized(object sender, EventArgs e)
        {
            Gl.Viewport(0, 0, ClientSize.Width, ClientSize.Height);
            projection = Matrix4x4f.Perspective(70, (float)ClientSize.Width / ClientSize.Height, .01f, 100f);
        }

        private void OnContextCreated(object sender, GlControlEventArgs e)
        {
            OnResized(null, null);

            coloredShader = new ColoredShader();
            texturedShader = new TexturedShader();

            quad = new VertexArray(PrimitiveType.Quads, quadVertices, new int[] { 3, 2 });
            origin = new VertexArray(PrimitiveType.Lines, originVertices, new int[] { 3, 3 });

            Gl.ClearColor(0.53f, 0.81f, 0.98f, 1f);
            Gl.Enable(EnableCap.DepthTest);

            Gl.UseProgram(texturedShader.glId);
            texturedShader.Texture = 0;

            testTexture = new Texture(@"resources\textures\test_texture.png");
            testTexture.Bind();
        }

        private void OnContextUpdate(object sender, GlControlEventArgs e)
        {
            int cameraYawInput        = FuncUtils.GetInput1d(Keys.K, Keys.H);
            int cameraPitchInput      = FuncUtils.GetInput1d(Keys.U, Keys.J);
            int cameraForwardInput    = FuncUtils.GetInput1d(Keys.W, Keys.S);
            int cameraHorizontalInput = FuncUtils.GetInput1d(Keys.D, Keys.A);
            int cameraVerticalInput   = FuncUtils.GetInput1d(Keys.E, Keys.Q);

            camera.Rotate(ROTATION_SPEED * cameraYawInput, ROTATION_SPEED * cameraPitchInput);
            camera.MoveRelativeToYaw(MOVEMENT_SPEED * cameraForwardInput, MOVEMENT_SPEED * cameraHorizontalInput);
            camera.Translate(0f, MOVEMENT_SPEED * cameraVerticalInput, 0f);
            camera.UpdateTransformMatrix();
        }

        private void Render(object sender, GlControlEventArgs e)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4x4f mvp = projection * camera.TransformMatrix;

            Gl.UseProgram(texturedShader.glId);
            texturedShader.MVP = mvp;
            quad.Draw();

            Gl.UseProgram(coloredShader.glId);
            coloredShader.MVP = mvp;
            origin.Draw();
        }

        private void OnContextDestroyed(object sender, GlControlEventArgs e)
        {
            texturedShader.Dispose();
            coloredShader.Dispose();
            quad.Dispose();
            origin.Dispose();
            testTexture.Dispose();
        }
    }
}
