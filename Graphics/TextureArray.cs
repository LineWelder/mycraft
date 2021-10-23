using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenGL;

namespace Mycraft.Graphics
{
    public class TextureArray : IDisposable
    {
        public readonly uint glId;

        public TextureArray(string path, int numTexturesX, int numTexturesY, int errorTextureWidth, int errorTextureHeight)
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

            glId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2d, glId);

            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, TextureWrapMode.ClampToEdge);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, TextureWrapMode.ClampToEdge);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, TextureMinFilter.Nearest);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, TextureMagFilter.Nearest);

            int textureWidth = image.Width / numTexturesX;
            int textureHeight = image.Height / numTexturesY;

            unsafe
            {
                float[,] black = new float[textureHeight, textureWidth];

                fixed (float* blackPtr = black)
                {
                    Gl.TexImage2D(
                        TextureTarget.Texture2d, 0,
                        InternalFormat.Rgba,
                        textureWidth, textureHeight, 0,
                        OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte,
                        new IntPtr(blackPtr)
                    );
                }
            }

            BitmapData grass = image.LockBits(
                new Rectangle(textureWidth, 0, textureWidth, textureHeight),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );

            Gl.TexSubImage2D(
                TextureTarget.Texture2d, 0,
                0, 0,
                textureWidth, textureHeight,
                OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte,
                grass.Scan0
            );

            image.UnlockBits(grass);
        }

        public TextureArray(string path, int numTexturesX, int numTexturesY)
            : this(path, numTexturesX, numTexturesY, 2, 2) { }

        public void Bind() => Gl.BindTexture(TextureTarget.Texture2d, glId);
        public void Dispose() => Gl.DeleteTextures(glId);
    }
}
