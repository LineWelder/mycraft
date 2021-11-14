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
layout(rg8, binding = 0) uniform image3D dataMap;

#define LIGHT_DECREASE 1.0 / 16.0

float getLight(ivec3 coords)
{
    vec4 pixel = imageLoad(dataMap, coords);
    return (1.0 - pixel.r) * pixel.g;
}

void main()
{
    ivec3 pixelCoords = ivec3(gl_GlobalInvocationID.xyz);
    
    vec4 info = imageLoad(dataMap, pixelCoords);
    float light = info.g;

    light = max(light, getLight(pixelCoords + ivec3( 1,  0,  0)) - LIGHT_DECREASE);
    light = max(light, getLight(pixelCoords + ivec3(-1,  0,  0)) - LIGHT_DECREASE);
    light = max(light, getLight(pixelCoords + ivec3( 0,  1,  0)) - LIGHT_DECREASE);
    light = max(light, getLight(pixelCoords + ivec3( 0, -1,  0)) - LIGHT_DECREASE);
    light = max(light, getLight(pixelCoords + ivec3( 0,  0,  1)) - LIGHT_DECREASE);
    light = max(light, getLight(pixelCoords + ivec3( 0,  0, -1)) - LIGHT_DECREASE);
    light = max(0.0, light);

    info.g = (1.0 - info.r) * light;

    imageStore(
        dataMap, pixelCoords,
        info
    );
}";

        private const string CONVERTING_SOURCE =
@"#version 430

layout(local_size_x = 1, local_size_y = 1, local_size_z = 1) in;
layout(rg8, binding = 0) uniform image3D dataMap;
layout(r8, binding = 1) uniform image3D lightMap;

#define REGION_SIZE 16
#define REGION_HEIGHT 256

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
                ivec3 probeCoords = pixelCoords + ivec3(dx, dy, dz);
                vec4 info = imageLoad(dataMap, probeCoords);

                accumulator += (1.0 - info.r) * info.g;
                probesCount += step(0, probeCoords.x) * step(probeCoords.x, REGION_SIZE - 1)
                             * step(0, probeCoords.y) * step(probeCoords.y, REGION_HEIGHT - 1)
                             * step(0, probeCoords.z) * step(probeCoords.z, REGION_SIZE - 1)
                             * (1.0 - info.r);
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
                Gl.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);
                Gl.DispatchCompute(Chunk.SIZE, Chunk.HEIGHT, Chunk.SIZE);
            }

            Gl.UseProgram(convertingProgramId);
            Gl.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);
            Gl.DispatchCompute(Chunk.SIZE + 1, Chunk.HEIGHT + 1, Chunk.SIZE + 1);
        }

        public void Dispose()
        {
            Gl.DeleteProgram(lightingProgramId);
            Gl.DeleteProgram(convertingProgramId);
        }
    }
}
