using Mycraft.Graphics;
using OpenGL;

namespace Mycraft.Shaders
{
    public class GUIShader : ShaderProgram
    {
        private const string VERTEX_SOURCE =
@"#version 330 core

layout(location = 0) in vec2 position;
layout(location = 1) in vec2 textureCoords;

uniform mat4 projection;
out vec2 _textureCoords;

void main()
{
    _textureCoords = textureCoords;
    gl_Position = projection * vec4(position, 0.0, 1.0);
}";

        private const string FRAGMENT_SOURCE =
@"#version 330 core

uniform sampler2D tex;
in vec2 _textureCoords;

void main()
{
    vec4 col = texture(tex, _textureCoords);
    if (col.a == 0.0) discard;
    gl_FragColor = col;
}";

        public Matrix4x4f Projection
        {
            set => Gl.UniformMatrix4f(projectionLocation, 1, false, value);
        }

        public int Texture
        {
            set => Gl.Uniform1i(textureLocation, 1, value);
        }

        private readonly int projectionLocation;
        private readonly int textureLocation;

        public GUIShader()
            : base(VERTEX_SOURCE, FRAGMENT_SOURCE)
        {
            textureLocation = FindVariable("tex");
            projectionLocation = FindVariable("projection");
        }
    }
}
