using OpenGL;
using Mycraft.Graphics;

namespace Mycraft.Shaders
{
    public class ParticleShader : ShaderProgram
    {
        private const string VERTEX_SOURCE =
@"#version 330 core

layout(location = 0) in vec3 particlePosition;
layout(location = 1) in vec2 offset;
layout(location = 2) in vec3 textureCoords;

uniform mat4 view;
uniform mat4 projection;
out vec3 _textureCoords;

void main()
{
    _textureCoords = textureCoords;
    vec4 position = view * vec4(particlePosition, 1.0);
    gl_Position = projection * (position + vec4(offset, 0.0, 0.0));
}";

        private const string FRAGMENT_SOURCE =
@"#version 330 core

uniform sampler2DArray tex;
in vec3 _textureCoords;

void main()
{
    vec4 textureSample = texture(tex, _textureCoords);
    if (textureSample.a == 0)
        discard;

    gl_FragColor = textureSample;
}";

        public Matrix4x4f View
        {
            set => Gl.UniformMatrix4f(viewLocation, 1, false, value);
        }

        public Matrix4x4f Projection
        {
            set => Gl.UniformMatrix4f(projectionLocation, 1, false, value);
        }

        public int Texture
        {
            set => Gl.Uniform1i(textureLocation, 1, value);
        }

        private readonly int viewLocation;
        private readonly int projectionLocation;
        private readonly int textureLocation;

        public ParticleShader()
            : base(new int[] { 3, 2, 3 }, VERTEX_SOURCE, FRAGMENT_SOURCE)
        {
            textureLocation = FindVariable("tex");
            viewLocation = FindVariable("view");
            projectionLocation = FindVariable("projection");
        }
    }
}
