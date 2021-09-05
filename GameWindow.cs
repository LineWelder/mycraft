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
        private VertexArray trangle;

        private readonly float[] vertices =
        {
             1f, -1f, 0f,
            -1f, -1f, 0f,
             0f,  1f, 0f
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

        private const float MOVEMENT_SPEED = .05f, ROTATION_SPEED = .3f;
        private readonly Camera camera;
        private readonly Input2d movementInput, rotationInput;

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

            camera = new Camera(new Vertex3f(0f, 0f, -5f), new Vertex2f(0f, 0f));
            movementInput = new Input2d(this, Keys.W, Keys.A, Keys.S, Keys.D);
            rotationInput = new Input2d(this, Keys.U, Keys.H, Keys.J, Keys.K);
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
            trangle = new VertexArray(vertices);
        }

        private void OnContextUpdate(object sender, GlControlEventArgs e)
        {
            camera.Rotate(ROTATION_SPEED * rotationInput.X, ROTATION_SPEED * rotationInput.Y);
            camera.MoveRelativeToYaw(MOVEMENT_SPEED * movementInput.Y, MOVEMENT_SPEED * movementInput.X);
            camera.Update();
        }

        private void Render(object sender, GlControlEventArgs e)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);

            Gl.UseProgram(program.glId);
            program.MVP = projection * camera.TransformMatrix;
            trangle.Draw(PrimitiveType.Triangles);
        }

        private void OnContextDestroyed(object sender, GlControlEventArgs e)
        {
            program.Dispose();
            trangle.Dispose();
        }
    }
}
