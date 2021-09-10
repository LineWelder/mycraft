using OpenGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Mycraft.Graphics
{
    public class Texture : IDisposable
    {
        public readonly uint glId;

        public Texture(string path)
        {
            Bitmap image = new Bitmap(path);
            BitmapData data = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );

            glId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2d, glId);
            
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, TextureWrapMode.Repeat);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, TextureWrapMode.Repeat);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, TextureMinFilter.Linear);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, TextureMagFilter.Nearest);

            Gl.TexImage2D(
                TextureTarget.Texture2d, 0,
                InternalFormat.Rgba,
                image.Width, image.Height, 0,
                OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte,
                data.Scan0
            );
        }

        public void Bind()
        {
            Gl.BindTexture(TextureTarget.Texture2d, glId);
        }

        public void Dispose()
        {
            Gl.DeleteTextures(glId);
        }
    }
}
