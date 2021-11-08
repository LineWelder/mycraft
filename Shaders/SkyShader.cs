using OpenGL;
using Mycraft.Graphics;

namespace Mycraft.Shaders
{
    public class SkyShader : ShaderProgram
    {
        private const string VERTEX_SOURCE =
@"#version 330 core

layout(location = 0) in vec2 position;

out vec2 _screenCoords;

void main()
{
    _screenCoords = position;
    gl_Position = vec4(position, 0.0, 1.0);
}";

        private const string FRAGMENT_SOURCE =
@"#version 330 core

uniform mat4 transformMatrix;
uniform vec3 skyColor;
uniform vec3 fogColor;

in vec2 _screenCoords;

void main()
{
    vec4 transformed = transformMatrix * vec4(_screenCoords, -1.0, 1.0);
    vec3 direction = normalize(transformed.xyz / transformed.w);

    gl_FragColor = vec4(
        mix(
            fogColor, skyColor,
            smoothstep(0.15, 0.5, abs(direction.y))
        ),
        1.0
    );
}";
        public Matrix4x4f TransformMatrix
        {
            set => Gl.UniformMatrix4f(transformMatrixLocation, 1, false, value);
        }

        public Vertex3f SkyColor
        {
            set => Gl.Uniform3f(skyColorLocation, 1, value);
        }

        public Vertex3f FogColor
        {
            set => Gl.Uniform3f(fogColorLocation, 1, value);
        }

        private readonly int transformMatrixLocation;
        private readonly int skyColorLocation;
        private readonly int fogColorLocation;

        public SkyShader()
            : base(new int[] { 2 }, VERTEX_SOURCE, FRAGMENT_SOURCE)
        {
            transformMatrixLocation = FindVariable("transformMatrix");
            skyColorLocation = FindVariable("skyColor");
            fogColorLocation = FindVariable("fogColor");
        }
    }
}
