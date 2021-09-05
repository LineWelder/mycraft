using Mycraft.Graphics;
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
             0.5f, -0.5f, 0.0f,
            -0.5f, -0.5f, 0.0f,
             0.0f,  0.5f, 0.0f
        };

        private readonly string vertexSource =
@"#version 330 core
layout(location = 0) in vec3 aPos;

void main()
{
    gl_Position = vec4(aPos, 1.0);
}";

        private readonly string fragmentSource =
@"#version 330 core

void main()
{
    gl_FragColor = vec4(1.0);
}";

        public GameWindow()
        {
            SuspendLayout();

            Name = "GameWindow";
            Text = "Mycraft";
            ClientSize = new Size(1920, 1080);

            glControl = new GlControl
            {
                Name = "GLControl",
                Dock = DockStyle.Fill,

                ColorBits = 24u,
                DepthBits = 0u,
                MultisampleBits = 0u,
                StencilBits = 0u
            };

            Resize += OnResized;
            glControl.ContextCreated += OnContextCreated;
            glControl.ContextDestroying += OnContextDestroyed;
            glControl.Render += Render;

            Controls.Add(glControl);
            ResumeLayout(false);
        }

        private void OnResized(object sender, EventArgs e)
        {
            Gl.Viewport(0, 0, ClientSize.Width, ClientSize.Height);
        }

        private void OnContextCreated(object sender, GlControlEventArgs e)
        {
            Gl.Viewport(0, 0, ClientSize.Width, ClientSize.Height);

            program = new ShaderProgram(vertexSource, fragmentSource);
            trangle = new VertexArray(vertices);
        }

        private void Render(object sender, GlControlEventArgs e)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);

            Gl.UseProgram(program.glId);
            trangle.Draw(PrimitiveType.Triangles);
        }

        private void OnContextDestroyed(object sender, GlControlEventArgs e)
        {
            program.Dispose();
            trangle.Dispose();
        }
    }
}
