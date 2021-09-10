using OpenGL;
using System;

namespace Mycraft.Graphics
{
    public class ColoredShader : ShaderProgram
    {
        private static readonly string VERTEX_SOURCE =
@"#version 330 core

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 color;
uniform mat4 mvp;
out vec3 vtxColor;

void main()
{
    vtxColor = color;
    gl_Position = mvp * vec4(position, 1.0);
}";

        private const string FRAGMENT_SOURCE =
@"#version 330 core
in vec3 vtxColor;

void main()
{
    gl_FragColor = vec4(vtxColor, 1.0);
}";

        public Matrix4x4f MVP
        {
            set => Gl.UniformMatrix4f(mvpLocation, 1, false, value);
        }

        private readonly int mvpLocation;

        public ColoredShader()
            : base(VERTEX_SOURCE, FRAGMENT_SOURCE)
        {
            mvpLocation = Gl.GetUniformLocation(glId, "mvp");
            if (mvpLocation < 0)
                throw new InvalidOperationException("mvp variable not found");
        }
    }
}
