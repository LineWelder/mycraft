using OpenGL;

namespace Mycraft.Graphics
{
    public class VignetteShader : ShaderProgram
    {
        private const string VERTEX_SOURCE =
@"#version 330 core

layout(location = 0) in vec2 position;
layout(location = 1) in vec2 textureCoords;

out vec2 _textureCoords;

void main()
{
    _textureCoords = textureCoords;
    gl_Position = vec4(position, 0.0, 1.0);
}";

        private const string FRAGMENT_SOURCE =
@"#version 330 core

uniform sampler2D tex;
in vec2 _textureCoords;

void main()
{
    gl_FragColor = vec4(
        texture(tex, _textureCoords).xyz,
        distance(vec2(0.5), _textureCoords) * 0.5 + 0.25
    );
}";

        public int Texture
        {
            set => Gl.Uniform1i(textureLocation, 1, value);
        }

        private readonly int textureLocation;

        public VignetteShader()
            : base(VERTEX_SOURCE, FRAGMENT_SOURCE)
        {
            textureLocation = FindVariable("tex");
        }
    }
}
