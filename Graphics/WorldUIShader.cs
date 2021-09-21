using OpenGL;
using System;

namespace Mycraft.Graphics
{
    public class WorldUIShader : ShaderProgram
    {
        private static readonly string VERTEX_SOURCE =
@"#version 330 core

layout(location = 0) in vec3 position;
uniform mat4 model;
uniform mat4 vp;

void main()
{
    gl_Position = vp * model * vec4(position, 1.0);
}";

        private const string FRAGMENT_SOURCE =
@"#version 330 core
uniform vec3 color;

void main()
{
    gl_FragColor = vec4(color, 1.0);
}";

        public Vertex3f Color
        {
            set => Gl.Uniform3f(colorLocation, 1, value);
        }

        public Matrix4x4f Model
        {
            set => Gl.UniformMatrix4f(modelLocation, 1, false, value);
        }

        public Matrix4x4f VP
        {
            set => Gl.UniformMatrix4f(vpLocation, 1, false, value);
        }

        private readonly int colorLocation, modelLocation, vpLocation;

        public WorldUIShader()
            : base(VERTEX_SOURCE, FRAGMENT_SOURCE)
        {
            colorLocation = FindVariable("color");
            modelLocation = FindVariable("model");
            vpLocation = FindVariable("vp");
        }
    }
}
