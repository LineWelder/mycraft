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
            Gl.BindTexture(TextureTarget.Texture2dArray, glId);

            Gl.TexParameteri(TextureTarget.Texture2dArray, TextureParameterName.TextureWrapS, TextureWrapMode.ClampToEdge);
            Gl.TexParameteri(TextureTarget.Texture2dArray, TextureParameterName.TextureWrapT, TextureWrapMode.ClampToEdge);
            Gl.TexParameteri(TextureTarget.Texture2dArray, TextureParameterName.TextureMinFilter, TextureMinFilter.Nearest);
            Gl.TexParameteri(TextureTarget.Texture2dArray, TextureParameterName.TextureMagFilter, TextureMagFilter.Nearest);

            int textureWidth = image.Width / numTexturesX;
            int textureHeight = image.Height / numTexturesY;

            unsafe
            {
                float[,,] black = new float[numTexturesX * numTexturesY, textureHeight, textureWidth];

                fixed (float* blackPtr = black)
                {
                    Gl.TexImage3D(
                        TextureTarget.Texture2dArray, 0,
                        InternalFormat.Rgba,
                        textureWidth, textureHeight, numTexturesX * numTexturesY, 0,
                        OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte,
                        new IntPtr(blackPtr)
                    );
                }
            }

            for (int x = 0; x < numTexturesX; x++)
                for (int y = 0; y < numTexturesX; y++)
                {
                    BitmapData texture = image.LockBits(
                        new Rectangle(
                            textureWidth * x,
                            textureHeight * y,
                            textureWidth,
                            textureHeight
                        ),
                        ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppRgb
                    );

                    Gl.TexSubImage3D(
                        TextureTarget.Texture2dArray, 0,
                        0, 0, x + y * numTexturesX,
                        textureWidth, textureHeight, 1,
                        OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte,
                        texture.Scan0
                    );

                    image.UnlockBits(texture);
                }
        }

        public TextureArray(string path, int numTexturesX, int numTexturesY)
            : this(path, numTexturesX, numTexturesY, 2, 2) { }

        public void Bind() => Gl.BindTexture(TextureTarget.Texture2dArray, glId);
        public void Dispose() => Gl.DeleteTextures(glId);
    }
}
