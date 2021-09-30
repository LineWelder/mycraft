using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenGL;

namespace Mycraft.Graphics
{
    public class Texture : IDisposable
    {
        public readonly uint glId;

        public Texture(string path, int errorTextureWidth, int errorTextureHeight)
        {
            Bitmap image;

            if (File.Exists(path))
                image = new Bitmap(path);
            else
            {
                image = new Bitmap(
                    errorTextureWidth,
                    errorTextureHeight
                );
                for (int x = 0; x < errorTextureWidth; x++)
                    for (int y = 0; y < errorTextureHeight; y++)
                        image.SetPixel(
                            x, y,
                            (x + y) % 2 == 0
                                ? Color.Magenta
                                : Color.Black
                        );
            }

            BitmapData data = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );

            glId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2d, glId);
            
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, TextureWrapMode.ClampToEdge);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, TextureWrapMode.ClampToEdge);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, TextureMinFilter.Nearest);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, TextureMagFilter.Nearest);

            Gl.TexImage2D(
                TextureTarget.Texture2d, 0,
                InternalFormat.Rgba,
                image.Width, image.Height, 0,
                OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte,
                data.Scan0
            );
        }

        public Texture(string path)
            : this(path, 2, 2) { }

        public void Bind() => Gl.BindTexture(TextureTarget.Texture2d, glId);
        public void Dispose() => Gl.DeleteTextures(glId);
    }
}
