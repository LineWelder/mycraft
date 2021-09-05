using Mycraft.Graphics;
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

        private ShaderProgram program;
        private VertexArray triangle, origin;

        private readonly float[] triangleVertices =
        {
             1f, -1f, 0f,
            -1f, -1f, 0f,
             0f,  1f, 0f
        };

        private readonly float[] originVertices =
        {
            0f, 0f, 0f,
            1f, 0f, 0f,
            0f, 0f, 0f,
            0f, 1f, 0f,
            0f, 0f, 0f,
            0f, 0f, 1f
        };

        private readonly string vertexSource =
@"#version 330 core

layout(location = 0) in vec3 position;
uniform mat4 mvp;

void main()
{
    gl_Position = mvp * vec4(position, 1.0);
}";

        private readonly string fragmentSource =
@"#version 330 core

void main()
{
    gl_FragColor = vec4(1.0);
}";

        private Matrix4x4f projection;

        private const float MOVEMENT_SPEED = .05f, ROTATION_SPEED = .05f;
        private readonly Camera camera;

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
                DepthBits = 0u,
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

            program = new ShaderProgram(vertexSource, fragmentSource);
            triangle = new VertexArray(PrimitiveType.Triangles, triangleVertices);
            origin = new VertexArray(PrimitiveType.Lines, originVertices);
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
            camera.Update();
        }

        private void Render(object sender, GlControlEventArgs e)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);

            Gl.UseProgram(program.glId);
            program.MVP = projection * camera.TransformMatrix;
            origin.Draw();
            triangle.Draw();
        }

        private void OnContextDestroyed(object sender, GlControlEventArgs e)
        {
            program.Dispose();
            triangle.Dispose();
        }
    }
}
