using System;
using System.Text;
using OpenGL;

using Mycraft.Blocks;
using Mycraft.Graphics;
using Mycraft.World;

namespace Mycraft.Graphics
{
    public class LightComputeShader
    {
        private const string LIGHTING_SOURCE =
@"#version 430

layout(local_size_x = 1, local_size_y = 1, local_size_z = 1) in;
layout(rg8ui, binding = 0) uniform uimage3D dataMap;

int getLight(ivec3 coords)
{
    uvec4 pixel = imageLoad(dataMap, coords);
    return int(pixel.r * pixel.g);
}

void main()
{
    ivec3 pixelCoords = ivec3(gl_GlobalInvocationID.xyz);
    
    uvec4 info = imageLoad(dataMap, pixelCoords);
    int light = int(info.g);

    light = max(light, getLight(pixelCoords + ivec3( 1,  0,  0)) - 1);
    light = max(light, getLight(pixelCoords + ivec3(-1,  0,  0)) - 1);
    light = max(light, getLight(pixelCoords + ivec3( 0,  1,  0)) - 1);
    light = max(light, getLight(pixelCoords + ivec3( 0, -1,  0)) - 1);
    light = max(light, getLight(pixelCoords + ivec3( 0,  0,  1)) - 1);
    light = max(light, getLight(pixelCoords + ivec3( 0,  0, -1)) - 1);
    light = max(0, light);

    info.g = info.r * uint(light);

    imageStore(
        dataMap, pixelCoords,
        info
    );
}";

        private const string CONVERTING_SOURCE =
@"#version 430

layout(local_size_x = 1, local_size_y = 1, local_size_z = 1) in;
layout(rg8ui, binding = 0) uniform uimage3D dataMap;
layout(r8, binding = 1) uniform image3D lightMap;

#define CHUNK_SIZE 16
#define CHUNK_HEIGHT 256

void main()
{
    ivec3 pixelCoords = ivec3(gl_GlobalInvocationID.xyz);

    float accumulator = 0.0;
    float probesCount = 0.0;

    for (int dx = -1; dx <= 0; dx++)
    {
        for (int dy = -1; dy <= 0; dy++)
        {
            for (int dz = -1; dz <= 0; dz++)
            {
                ivec3 probeCoords = pixelCoords + ivec3(dx, dy, dz) + ivec3(CHUNK_SIZE, 0, CHUNK_SIZE);
                uvec4 info = imageLoad(dataMap, probeCoords);
                float block = info.r;
                float light = float(info.g) / 15.0;

                accumulator += block * light;
                probesCount += step(0, probeCoords.y) * step(probeCoords.y, CHUNK_HEIGHT - 1)
                             * block;
            }
        }
    }

    imageStore(
        lightMap, pixelCoords,
        vec4(vec3(accumulator / probesCount), 1.0)
    );
}";

        private readonly uint lightingProgramId;
        private readonly uint convertingProgramId;

        public LightComputeShader()
        {
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

        public void Run()
        {
            Gl.UseProgram(lightingProgramId);
            for (int i = 0; i < 16; i++)
            {
                Gl.DispatchCompute(Chunk.SIZE * 3, Chunk.HEIGHT, Chunk.SIZE * 3);
                Gl.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);
            }

            Gl.UseProgram(convertingProgramId);
            Gl.DispatchCompute(Chunk.SIZE + 1, Chunk.HEIGHT + 1, Chunk.SIZE + 1);
        }

        public void Dispose()
        {
            Gl.DeleteProgram(lightingProgramId);
            Gl.DeleteProgram(convertingProgramId);
        }
    }
}
