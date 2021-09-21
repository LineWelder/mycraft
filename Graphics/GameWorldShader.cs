using OpenGL;
using System;

namespace Mycraft.Graphics
{
    public class GameWorldShader : ShaderProgram
    {
        private const string VERTEX_SOURCE =
@"#version 330 core

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 textureCoords;
layout(location = 2) in float light;
uniform mat4 mvp;
out vec2 _textureCoords;
out float _light;

void main()
{
    _textureCoords = textureCoords;
    _light = light;
    gl_Position = mvp * vec4(position, 1.0);
}";

        private const string FRAGMENT_SOURCE =
@"#version 330 core

uniform sampler2D tex;
in vec2 _textureCoords;
in float _light;

void main()
{
    gl_FragColor = texture(tex, _textureCoords) * _light;
}";

        public Matrix4x4f MVP
        {
            set => Gl.UniformMatrix4f(mvpLocation, 1, false, value);
        }

        public int Texture
        {
            set => Gl.Uniform1i(textureLocation, 1, value);
        }

        private readonly int mvpLocation;
        private readonly int textureLocation;

        public GameWorldShader()
            : base(VERTEX_SOURCE, FRAGMENT_SOURCE)
        {
            textureLocation = Gl.GetUniformLocation(glId, "tex");
            if (textureLocation < 0)
                throw new InvalidOperationException("tex variable not found");

            mvpLocation = Gl.GetUniformLocation(glId, "mvp");
            if (mvpLocation < 0)
                throw new InvalidOperationException("mvp variable not found");
        }
    }
}
