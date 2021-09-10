using OpenGL;
using System;

namespace Mycraft.Graphics
{
    public class OneColoredShader : ShaderProgram
    {
        private const string VERTEX_SOURCE =
@"#version 330 core

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 color;
uniform mat4 mvp;

void main()
{
    gl_Position = mvp * vec4(position, 1.0);
}";

        private const string FRAGMENT_SOURCE =
@"#version 330 core
uniform vec3 color;

void main()
{
    gl_FragColor = vec4(color, 1.0);
}";

        public readonly int mvpLocation;
        public readonly int colorLocation;

        public OneColoredShader()
            : base(VERTEX_SOURCE, FRAGMENT_SOURCE)
        {
            colorLocation = Gl.GetUniformLocation(glId, "color");

            if (colorLocation < 0)
                throw new InvalidOperationException("Color variable not found");

            mvpLocation = Gl.GetUniformLocation(glId, "mvp");
            if (mvpLocation < 0)
                throw new InvalidOperationException("MVP variable not found");
        }
    }
}
