using Mycraft.Graphics;
using OpenGL;
using System;
using System.Text;

namespace Mycraft.Utils
{
    public class ComputeShader
    {
        private const string LIGHTING_SOURCE =
@"#version 430

layout(local_size_x = 1, local_size_y = 1) in;
layout(rgba8, binding = 0) uniform image2D image;

#define LIGHT_DECREASE 1.0 / 16.0

float getLight(ivec2 coords)
{
    vec4 pixel = imageLoad(image, coords);
    return (1.0 - pixel.r) * pixel.g;
}

void main()
{
    ivec2 pixelCoords = ivec2(gl_GlobalInvocationID.xy);
    
    vec4 color = imageLoad(image, pixelCoords);
    float light = color.g;

    light = max(light, getLight(pixelCoords + ivec2( 1,  0)) - LIGHT_DECREASE);
    light = max(light, getLight(pixelCoords + ivec2(-1,  0)) - LIGHT_DECREASE);
    light = max(light, getLight(pixelCoords + ivec2( 0,  1)) - LIGHT_DECREASE);
    light = max(light, getLight(pixelCoords + ivec2( 0, -1)) - LIGHT_DECREASE);
    light = max(0.0, light);

    light = (1.0 - color.r) * light;
    color.g = light;

    imageStore(
        image, pixelCoords,
        color
    );
}";

        private const string CONVERTING_SOURCE =
@"#version 430

layout(local_size_x = 1, local_size_y = 1) in;
layout(rgba8, binding = 0) uniform image2D flatLighting;
layout(rgba8, binding = 1) uniform image2D fancyLighting;

#define REGION_WIDTH 16
#define REGION_HEIGHT 16

void main()
{
    ivec2 pixelCoords = ivec2(gl_GlobalInvocationID.xy);

    float accumulator = 0.0;
    float probesCount = 0.0;

    for (int dx = -1; dx <= 0; dx++)
    {
        for (int dy = -1; dy <= 0; dy++)
        {
            ivec2 probeCoords = pixelCoords + ivec2(dx, dy);
            vec4 info = imageLoad(flatLighting, probeCoords);
            accumulator += info.g;

            probesCount += step(0, probeCoords.x) * step(probeCoords.x, REGION_WIDTH - 1)
                         * step(0, probeCoords.y) * step(probeCoords.y, REGION_HEIGHT - 1)
                         * (1.0 - info.r);
       }
    }

    imageStore(
        fancyLighting, pixelCoords,
        vec4(vec3(accumulator / probesCount), 1.0)
    );
}
";

        private const int REGION_WIDTH = 16, REGION_HEIGHT = 16;

        private readonly Texture flatLighting, fancyLighting;
        private readonly uint lightingProgramId;
        private readonly uint convertingProgramId;

        public ComputeShader()
        {
            // Set up the textures

            flatLighting = new Texture(
                @"resources\textures\test_map.png",
                REGION_WIDTH, REGION_HEIGHT
            );

            fancyLighting = new Texture(
                "",
                REGION_WIDTH + 1, REGION_HEIGHT + 1
            );

            // Set up the shaders

            lightingProgramId = BuildProgram(LIGHTING_SOURCE);
            convertingProgramId = BuildProgram(CONVERTING_SOURCE);
        }

        private uint CompileShader(string source)
        {
            uint shaderId = Gl.CreateShader(ShaderType.ComputeShader);
            Gl.ShaderSource(shaderId, new string[] { source });
            Gl.CompileShader(shaderId);

            Gl.GetShader(shaderId, ShaderParameterName.CompileStatus, out int сompileStatus);
            if (сompileStatus == 0)
            {
                Gl.GetShader(shaderId, ShaderParameterName.InfoLogLength, out int infoLogLength);

                StringBuilder errorString = new StringBuilder(infoLogLength);
                Gl.GetShaderInfoLog(shaderId, infoLogLength, out _, errorString);

                throw new InvalidOperationException($"Shader compiling error: {errorString}");
            };

            return shaderId;
        }

        private uint BuildProgram(string shaderSource)
        {
            uint compiledShader = CompileShader(shaderSource);

            uint programId = Gl.CreateProgram();
            Gl.AttachShader(programId, compiledShader);
            Gl.LinkProgram(programId);

            Gl.GetProgram(programId, ProgramProperty.LinkStatus, out int linkStatus);
            if (linkStatus == 0)
            {
                Gl.GetProgram(programId, ProgramProperty.InfoLogLength, out int infoLogLength);

                StringBuilder errorString = new StringBuilder(infoLogLength);
                Gl.GetProgramInfoLog(programId, infoLogLength, out _, errorString);

                throw new InvalidOperationException($"Program linking error: {errorString}");
            };

            Gl.DeleteShader(compiledShader);
            return programId;
        }

        public void BindTexture()
        {
            fancyLighting.Bind();
        }

        public void Run()
        {
            Gl.BindImageTexture(
                0, flatLighting.glId, 0,
                false, 0,
                BufferAccess.ReadWrite,
                InternalFormat.Rgba8
            );

            Gl.BindImageTexture(
                1, fancyLighting.glId, 0,
                false, 0,
                BufferAccess.WriteOnly,
                InternalFormat.Rgba8
            );

            Gl.UseProgram(lightingProgramId);
            for (int i = 0; i < 16; i++)
            {
                Gl.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);
                Gl.DispatchCompute(REGION_WIDTH, REGION_HEIGHT, 1);
            }

            Gl.UseProgram(convertingProgramId);
            Gl.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);
            Gl.DispatchCompute(REGION_WIDTH + 1, REGION_HEIGHT + 1, 1);
        }

        public void Dispose()
        {
            flatLighting.Dispose();
            fancyLighting.Dispose();
            Gl.DeleteProgram(lightingProgramId);
            Gl.DeleteProgram(convertingProgramId);
        }
    }
}
