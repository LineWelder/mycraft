using System;
using OpenGL;

namespace Mycraft.Graphics
{
    public class LightMap : IDisposable
    {
        public readonly uint glId;

        /// <reamarks>
        /// The coords in the given array should be shuffled: (z, y, x)
        /// </remarks>
        public unsafe float[,,] Data
        {
            set
            {
                fixed (float* valuePtr = &value[0, 0, 0])
                {
                    Gl.TexImage3D(
                        TextureTarget.Texture3d, 0,
                        InternalFormat.Luminance8,
                        value.GetLength(2), value.GetLength(1), value.GetLength(0), 0,
                        PixelFormat.Luminance, PixelType.Float,
                        new IntPtr(valuePtr)
                    );
                }
            }
        }

        public LightMap()
        {
            glId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture3d, glId);
            
            Gl.TexParameteri(TextureTarget.Texture3d, TextureParameterName.TextureWrapS, TextureWrapMode.ClampToEdge);
            Gl.TexParameteri(TextureTarget.Texture3d, TextureParameterName.TextureWrapT, TextureWrapMode.ClampToEdge);
            Gl.TexParameteri(TextureTarget.Texture3d, TextureParameterName.TextureWrapR, TextureWrapMode.ClampToEdge);
            Gl.TexParameteri(TextureTarget.Texture3d, TextureParameterName.TextureMinFilter, TextureMinFilter.Linear);
            Gl.TexParameteri(TextureTarget.Texture3d, TextureParameterName.TextureMagFilter, TextureMagFilter.Linear);
        }

        public void Bind() => Gl.BindTexture(TextureTarget.Texture3d, glId);
        public void Dispose() => Gl.DeleteTextures(glId);
    }
}
