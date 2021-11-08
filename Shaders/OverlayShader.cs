using OpenGL;
using Mycraft.Graphics;

namespace Mycraft.Shaders
{
    public class OverlayShader : ShaderProgram
    {
        private const string VERTEX_SOURCE =
@"#version 330 core

layout(location = 0) in vec2 position;

out vec2 _textureCoords;

void main()
{
    _textureCoords = position * vec2(0.5, -0.5) + vec2(0.5);
    gl_Position = vec4(position, 0.0, 1.0);
}";

        private const string FRAGMENT_SOURCE =
@"#version 330 core

uniform sampler2DArray tex;
uniform float textureId;

in vec2 _textureCoords;

void main()
{
    gl_FragColor = vec4(
        texture(tex, vec3(_textureCoords, textureId)).xyz,
        distance(vec2(0.5), _textureCoords) * 0.5 + 0.25
    );
}";

        public int Texture
        {
            set => Gl.Uniform1i(textureLocation, 1, value);
        }

        public float TextureId
        {
            set => Gl.Uniform1f(textureIdLocation, 1, value);
        }

        private readonly int textureLocation;
        private readonly int textureIdLocation;

        public OverlayShader()
            : base(new int[] { 2 }, VERTEX_SOURCE, FRAGMENT_SOURCE)
        {
            textureLocation = FindVariable("tex");
            textureIdLocation = FindVariable("textureId");
        }
    }
}
