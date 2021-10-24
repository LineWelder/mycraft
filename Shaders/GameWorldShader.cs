using OpenGL;
using Mycraft.Graphics;

namespace Mycraft.Shaders
{
    public class GameWorldShader : ShaderProgram
    {
        private const string VERTEX_SOURCE =
@"#version 330 core

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 textureCoords;
layout(location = 2) in float light;

uniform vec3 chunkStart;
uniform mat4 view;
uniform mat4 projection;

out vec3 _position;
out float _distance;
out vec3 _textureCoords;
out float _light;

void main()
{
    _position = position;
    _textureCoords = textureCoords;
    _light = light;

    vec4 viewPosition = view * vec4(chunkStart + position, 1.0);
    _distance = length(viewPosition);

    gl_Position = projection * viewPosition;
}";

        private const string FRAGMENT_SOURCE =
@"#version 330 core

uniform sampler2DArray tex;
uniform float alpha;

uniform vec3 fogColor;
uniform float fogDistance;
uniform float fogDensity;

uniform vec3 lightMapScale;
uniform sampler3D lightMap;

in vec3 _position;
in float _distance;
in vec3 _textureCoords;
in float _light;

void main()
{
    vec3 lightMapCoords = (_position + vec3(1.0, 0.0, 1.0)) / lightMapScale;
    float lightMapSample = texture(lightMap, lightMapCoords).x;

    vec4 textureSample = texture(tex, _textureCoords);
    if (textureSample.a == 0.0) discard;
    vec3 color = textureSample.rgb * _light * lightMapSample;

    gl_FragColor = vec4(
        mix(
            color,
            fogColor,
            smoothstep(fogDistance, fogDistance + fogDensity, _distance)
        ),
        alpha
    );
}";

        public Vertex3f ChunkStart
        {
            set => Gl.Uniform3f(chunkStartLocation, 1, value);
        }

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

        public float Alpha
        {
            set => Gl.Uniform1f(alphaLocation, 1, value);
        }

        public Vertex3f FogColor
        {
            set => Gl.Uniform3f(fogColorLocation, 1, value);
        }

        public float FogDistance
        {
            set => Gl.Uniform1f(fogDistanceLocation, 1, value);
        }

        public float FogDensity
        {
            set => Gl.Uniform1f(fogDensityLocation, 1, value);
        }

        public Vertex3f LightMapScale
        {
            set => Gl.Uniform3f(lightMapScaleLocation, 1, value);
        }

        public int LightMap
        {
            set => Gl.Uniform1i(lightMapLocation, 1, value);
        }

        private readonly int chunkStartLocation;
        private readonly int viewLocation, projectionLocation;
        private readonly int textureLocation;
        private readonly int alphaLocation;

        private readonly int fogColorLocation;
        private readonly int fogDistanceLocation;
        private readonly int fogDensityLocation;

        private readonly int lightMapScaleLocation;
        private readonly int lightMapLocation;

        public GameWorldShader()
            : base(new int[] { 3, 3, 1 }, VERTEX_SOURCE, FRAGMENT_SOURCE)
        {
            chunkStartLocation = FindVariable("chunkStart");
            viewLocation = FindVariable("view");
            projectionLocation = FindVariable("projection");
            textureLocation = FindVariable("tex");
            alphaLocation = FindVariable("alpha");

            fogColorLocation = FindVariable("fogColor");
            fogDistanceLocation = FindVariable("fogDistance");
            fogDensityLocation = FindVariable("fogDensity");

            lightMapScaleLocation = FindVariable("lightMapScale");
            lightMapLocation = FindVariable("lightMap");
        }
    }
}
