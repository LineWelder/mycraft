using Mycraft.Graphics;
using OpenGL;
using System;
using System.Text;

namespace Mycraft.Utils
{
    public class ComputeShader
    {
        private const string SHADER_SOURCE =
@"#version 430

layout(local_size_x = 1, local_size_y = 1) in;
layout(rgba8, binding = 0) uniform image2D imageIn;
layout(rgba8, binding = 1) uniform image2D imageOut;

#define LIGHT_PROPOGATION 16

float getLight(ivec2 coords)
{
    vec4 pixel = imageLoad(imageIn, coords);
    return 1.0 - step((pixel.r + pixel.g + pixel.b) * pixel.a, 0.0);
}

float propogateLight(ivec2 coords)
{
    return getLight(coords) - 0.5;
}

void main()
{
    ivec2 pixel_coords = ivec2(gl_GlobalInvocationID.xy);
 
    float light = getLight(pixel_coords);
    for (int dx = -LIGHT_PROPOGATION; dx <= LIGHT_PROPOGATION; dx++)
    {
        for (int dy = -LIGHT_PROPOGATION; dy <= LIGHT_PROPOGATION; dy++)
        {
            float lightDecrease = (abs(dx) + abs(dy))  / float(LIGHT_PROPOGATION + 1);
            float currentLight = getLight(pixel_coords + ivec2(dx, dy));
            float borowedLight = max(0.0, currentLight - lightDecrease);
            light = max(light, borowedLight);
        }
    }

    imageStore(
        imageOut, pixel_coords,
        vec4(vec3(light), 1.0)
    );
}";

        private const int TEXTURE_WIDTH = 24, TEXTURE_HEIGHT = 24;

        private readonly Texture textureIn, textureOut;
        private readonly uint programId;

        public ComputeShader()
        {
            // Set up the texture

            textureIn = new Texture(@"resources\textures\cross.png", 24, 24);
            textureOut = new Texture("", 24, 24);

            // Set up the shader

            uint shaderId = Gl.CreateShader(ShaderType.ComputeShader);
            Gl.ShaderSource(shaderId, new string[] { SHADER_SOURCE });
            Gl.CompileShader(shaderId);

            Gl.GetShader(shaderId, ShaderParameterName.CompileStatus, out int сompileStatus);
            if (сompileStatus == 0)
            {
                Gl.GetShader(shaderId, ShaderParameterName.InfoLogLength, out int infoLogLength);

                StringBuilder errorString = new StringBuilder(infoLogLength);
                Gl.GetShaderInfoLog(shaderId, infoLogLength, out _, errorString);

                throw new InvalidOperationException($"Shader compiling error: {errorString}");
            };

            programId = Gl.CreateProgram();
            Gl.AttachShader(programId, shaderId);
            Gl.LinkProgram(programId);

            Gl.GetProgram(programId, ProgramProperty.LinkStatus, out int linkStatus);
            if (linkStatus == 0)
            {
                Gl.GetProgram(programId, ProgramProperty.InfoLogLength, out int infoLogLength);

                StringBuilder errorString = new StringBuilder(infoLogLength);
                Gl.GetProgramInfoLog(programId, infoLogLength, out _, errorString);

                throw new InvalidOperationException($"Program linking error: {errorString}");
            };

            Gl.DeleteShader(shaderId);
        }

        public void BindTexture()
        {
            textureOut.Bind();
        }

        public void Run()
        {
            Gl.BindImageTexture(
                0, textureIn.glId, 0,
                false, 0,
                BufferAccess.ReadOnly,
                InternalFormat.Rgba8
            );

            Gl.BindImageTexture(
                1, textureOut.glId, 0,
                false, 0,
                BufferAccess.WriteOnly,
                InternalFormat.Rgba8
            );

            Gl.UseProgram(programId);
            Gl.DispatchCompute(TEXTURE_WIDTH, TEXTURE_HEIGHT, 1);
        }

        public void Dispose()
        {
            textureIn.Dispose();
            textureOut.Dispose();
            Gl.DeleteProgram(programId);
        }
    }
}
