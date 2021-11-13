using System;
using System.Text;
using OpenGL;

using Mycraft.Blocks;
using Mycraft.Graphics;
using Mycraft.World;

namespace Mycraft.Utils
{
    public class LightComputeShader
    {
        private const string LIGHTING_SOURCE =
@"#version 430

layout(local_size_x = 1, local_size_y = 1) in;
layout(rg8, binding = 0) uniform image2D dataMap;

#define LIGHT_DECREASE 1.0 / 16.0

float getLight(ivec2 coords)
{
    vec4 pixel = imageLoad(dataMap, coords);
    return (1.0 - pixel.r) * pixel.g;
}

void main()
{
    ivec2 pixelCoords = ivec2(gl_GlobalInvocationID.xy);
    
    vec4 info = imageLoad(dataMap, pixelCoords);
    float light = info.g;

    light = max(light, getLight(pixelCoords + ivec2( 1,  0)) - LIGHT_DECREASE);
    light = max(light, getLight(pixelCoords + ivec2(-1,  0)) - LIGHT_DECREASE);
    light = max(light, getLight(pixelCoords + ivec2( 0,  1)) - LIGHT_DECREASE);
    light = max(light, getLight(pixelCoords + ivec2( 0, -1)) - LIGHT_DECREASE);
    light = max(0.0, light);

    info.g = (1.0 - info.r) * light;

    imageStore(
        dataMap, pixelCoords,
        info
    );
}";

        private const string CONVERTING_SOURCE =
@"#version 430

layout(local_size_x = 1, local_size_y = 1) in;
layout(rg8, binding = 0) uniform image2D dataMap;
layout(r8, binding = 1) uniform image2D lightingMap;

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
            vec4 info = imageLoad(dataMap, probeCoords);
            accumulator += info.g;

            probesCount += step(0, probeCoords.x) * step(probeCoords.x, REGION_WIDTH - 1)
                         * step(0, probeCoords.y) * step(probeCoords.y, REGION_HEIGHT - 1)
                         * (1.0 - info.r);
       }
    }

    imageStore(
        lightingMap, pixelCoords,
        vec4(vec3(accumulator / probesCount), 1.0)
    );
}
";

        private const int REGION_HEIGHT = 16;

        private readonly uint dataMapId;
        private readonly uint lightMapId;

        private readonly uint lightingProgramId;
        private readonly uint convertingProgramId;

        public unsafe LightComputeShader()
        {
            // Set up the textures

            dataMapId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2d, dataMapId);
            Gl.TexStorage2D(
                TextureTarget.Texture2d, 1,
                InternalFormat.Rg8,
                Chunk.SIZE, REGION_HEIGHT
            );

            lightMapId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2d, lightMapId);
            Gl.TexStorage2D(
                TextureTarget.Texture2d, 1,
                InternalFormat.R8,
                Chunk.SIZE, REGION_HEIGHT
            );

            // Set up the shaders

            lightingProgramId = BuildProgram(LIGHTING_SOURCE);
            convertingProgramId = BuildProgram(CONVERTING_SOURCE);
        }

        public unsafe void BuildDataMap(Chunk chunk, int z, int y)
        {
            float[,,] data = new float[REGION_HEIGHT, Chunk.SIZE, 2];
            for (int x_ = 0; x_ < Chunk.SIZE; x_++)
            {
                bool drawSunLight = true;
                for (int y_ = 0; y_ < REGION_HEIGHT; y_++)
                {
                    bool blockTransparent = chunk.blocks[x_, y + REGION_HEIGHT - y_, z].IsTransparent;
                    if (!blockTransparent)
                        drawSunLight = false;

                    data[y_, x_, 0] = blockTransparent ? 0f : 1f;
                    data[y_, x_, 1] = drawSunLight ? 1f : 0f;
                }
            }

            fixed (float* dataPtr = data)
            {
                Gl.BindTexture(TextureTarget.Texture2d, dataMapId);
                Gl.TexSubImage2D(
                    TextureTarget.Texture2d, 0,
                    0, 0,
                    Chunk.SIZE, REGION_HEIGHT,
                    PixelFormat.Rg, PixelType.Float,
                    new IntPtr(dataPtr)
                );
            }
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
            Gl.BindTexture(TextureTarget.Texture2d, lightMapId);
        }

        public void Run()
        {
            Gl.BindImageTexture(
                0, dataMapId, 0,
                false, 0,
                BufferAccess.ReadWrite,
                InternalFormat.Rg8
            );

            Gl.BindImageTexture(
                1, lightMapId, 0,
                false, 0,
                BufferAccess.WriteOnly,
                InternalFormat.R8
            );

            Gl.UseProgram(lightingProgramId);
            for (int i = 0; i < 16; i++)
            {
                Gl.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);
                Gl.DispatchCompute(Chunk.SIZE, REGION_HEIGHT, 1);
            }

            Gl.UseProgram(convertingProgramId);
            Gl.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);
            Gl.DispatchCompute(Chunk.SIZE + 1, REGION_HEIGHT + 1, 1);
        }

        public void Dispose()
        {
            Gl.DeleteTextures(dataMapId, lightMapId);
            Gl.DeleteProgram(lightingProgramId);
            Gl.DeleteProgram(convertingProgramId);
        }
    }
}
