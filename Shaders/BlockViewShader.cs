using OpenGL;
using Mycraft.Graphics;

namespace Mycraft.Shaders
{
    public class BlockViewShader : ShaderProgram
    {
        private const string VERTEX_SOURCE =
@"#version 330 core

layout(location = 0) in vec2 position;
layout(location = 1) in vec3 textureCoords;
layout(location = 2) in float light;

uniform mat4 projection;
out vec3 _textureCoords;
out float _light;

void main()
{
    _textureCoords = textureCoords;
    _light = light;
    gl_Position = projection * vec4(position, 0.0, 1.0);
}";

        private const string FRAGMENT_SOURCE =
@"#version 330 core

uniform sampler2DArray tex;
in vec3 _textureCoords;
in float _light;

void main()
{
    vec4 textureSample = texture(tex, _textureCoords);
    if (textureSample.a == 0) discard;

    gl_FragColor = vec4(textureSample.rgb * _light, 1.0);
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

        public BlockViewShader()
            : base(new int[] { 2, 3, 1 }, VERTEX_SOURCE, FRAGMENT_SOURCE)
        {
            textureLocation = FindVariable("tex");
            projectionLocation = FindVariable("projection");
        }
    }
}
