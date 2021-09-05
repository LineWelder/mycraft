using OpenGL;
using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mycraft
{
    public class GameWindow : Form
    {
        private readonly GlControl glControl;

        private readonly float[] vertices =
        {
             0.5f, -0.5f, 0.0f,
            -0.5f, -0.5f, 0.0f,
             0.0f,  0.5f, 0.0f
        };

        private uint program, vao, vbo;

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

        private uint CreateShader(ShaderType shaderType, string source)
        {
            uint id = Gl.CreateShader(shaderType);
            Gl.ShaderSource(id, new string[] { source });
            Gl.CompileShader(id);

            Gl.GetShader(id, ShaderParameterName.CompileStatus, out int сompileStatus);
            if (сompileStatus == 0)
            {
                Gl.GetShader(id, ShaderParameterName.InfoLogLength, out int infoLogLength);

                StringBuilder errorString = new StringBuilder(infoLogLength);
                Gl.GetShaderInfoLog(id, infoLogLength, out _, errorString);

                throw new InvalidOperationException($"Shader compiling error: {errorString}");
            };

            return id;
        }

        private void OnContextCreated(object sender, GlControlEventArgs e)
        {
            Gl.Viewport(0, 0, ClientSize.Width, ClientSize.Height);

            uint vertex = CreateShader(ShaderType.VertexShader,
@"#version 330 core
layout(location = 0) in vec3 aPos;

void main()
{
    gl_Position = vec4(aPos, 1.0);
}"
            );

            uint fragment = CreateShader(ShaderType.FragmentShader,
@"#version 330 core

void main()
{
    gl_FragColor = vec4(1.0);
}"
            );

            program = Gl.CreateProgram();

            Gl.AttachShader(program, vertex);
            Gl.AttachShader(program, fragment);
            Gl.LinkProgram(program);

            Gl.GetProgram(program, ProgramProperty.LinkStatus, out int linkStatus);
            if (linkStatus == 0)
            {
                Gl.GetProgram(program, ProgramProperty.InfoLogLength, out int infoLogLength);

                StringBuilder errorString = new StringBuilder(infoLogLength);
                Gl.GetShaderInfoLog(program, infoLogLength, out _, errorString);

                throw new InvalidOperationException($"Shader compiling error: {errorString}");
            };

            Gl.DeleteShader(vertex);
            Gl.DeleteShader(fragment);

            vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            vbo = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(sizeof(float) * vertices.Length), vertices, BufferUsage.StaticDraw);

            Gl.VertexAttribPointer(0, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);
            Gl.EnableVertexAttribArray(0);
        }

        private void Render(object sender, GlControlEventArgs e)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);

            Gl.UseProgram(program);
            Gl.BindVertexArray(vao);

            Gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
        }

        private void OnContextDestroyed(object sender, GlControlEventArgs e)
        {
            Gl.DeleteProgram(program);
            Gl.DeleteVertexArrays(vao);
            Gl.DeleteBuffers(vbo);
        }
    }
}
