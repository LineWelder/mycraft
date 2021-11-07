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
                fixed (float* valuePtr = value)
                {
                    Gl.BindTexture(TextureTarget.Texture3d, glId);
                    Gl.TexSubImage3D(
                        TextureTarget.Texture3d, 0,
                        0, 0, 0,
                        width, height, depth,
                        PixelFormat.Luminance, PixelType.Float,
                        new IntPtr(valuePtr)
                    );
                }
            }
        }

        private readonly int width, height, depth;

        public LightMap(int width, int height, int depth)
        {
            this.width  = width;
            this.height = height;
            this.depth  = depth;

            glId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture3d, glId);
            
            Gl.TexParameteri(TextureTarget.Texture3d, TextureParameterName.TextureWrapS, TextureWrapMode.ClampToEdge);
            Gl.TexParameteri(TextureTarget.Texture3d, TextureParameterName.TextureWrapT, TextureWrapMode.ClampToEdge);
            Gl.TexParameteri(TextureTarget.Texture3d, TextureParameterName.TextureWrapR, TextureWrapMode.ClampToEdge);
            Gl.TexParameteri(TextureTarget.Texture3d, TextureParameterName.TextureMinFilter, TextureMinFilter.Linear);
            Gl.TexParameteri(TextureTarget.Texture3d, TextureParameterName.TextureMagFilter, TextureMagFilter.Linear);

            Gl.TexStorage3D(
                TextureTarget.Texture3d, 1,
                InternalFormat.Luminance8,
                width, height, depth
            );
        }

        public void Bind() => Gl.BindTexture(TextureTarget.Texture3d, glId);
        public void Dispose() => Gl.DeleteTextures(glId);
    }
}
