using System;
using System.Text;
using OpenGL;

namespace Mycraft.Utils
{
    public class ComputeShader
    {
        private const string SHADER_SOURCE =
@"#version 430

layout(local_size_x = 1, local_size_y = 1) in;
layout(rgba8, binding = 0) uniform image2D img_output;

void main()
{
    vec4 pixel = vec4(1.0, 1.0, 1.0, 1.0);
    ivec2 pixel_coords = ivec2(gl_GlobalInvocationID.xy);
  
    imageStore(img_output, pixel_coords, pixel);
}";

        private const int TEXTURE_WIDTH = 16, TEXTURE_HEIGHT = 16;

        private uint textureId;
        private uint programId;

        public ComputeShader()
        {
            // Set up the texture

            textureId = Gl.GenTexture();
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2d, textureId);

            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, TextureWrapMode.ClampToEdge);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, TextureWrapMode.ClampToEdge);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, TextureMagFilter.Linear);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, TextureMinFilter.Linear);

            Gl.TexImage2D(
                TextureTarget.Texture2d, 0,
                InternalFormat.Rgba8,
                TEXTURE_WIDTH, TEXTURE_HEIGHT, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte,
                IntPtr.Zero
            );

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
            Gl.BindTexture(TextureTarget.Texture2d, textureId);
        }

        public void Run()
        {
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindImageTexture(
                0, textureId, 0,
                false, 0,
                BufferAccess.WriteOnly,
                InternalFormat.Rgba8
            );

            Gl.UseProgram(programId);
            Gl.DispatchCompute(TEXTURE_WIDTH, TEXTURE_HEIGHT, 1);

            ErrorCode code = Gl.GetError();
            if (code != ErrorCode.NoError)
                Console.WriteLine($"Error! {code}");
        }

        public void Dispose()
        {
            Gl.DeleteTextures(textureId);
            Gl.DeleteProgram(programId);
        }
    }
}
