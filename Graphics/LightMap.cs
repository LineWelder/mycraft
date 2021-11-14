using System;
using OpenGL;

using Mycraft.Blocks;
using Mycraft.Utils;
using Mycraft.World;

namespace Mycraft.Graphics
{
    public class LightMap : IDisposable
    {
        private readonly Chunk chunk;

        private readonly uint dataMapId;
        private readonly uint lightMapId;

        public LightMap(Chunk chunk)
        {
            this.chunk = chunk;

            dataMapId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture3d, dataMapId);
            Gl.TexStorage3D(
                TextureTarget.Texture3d, 1,
                InternalFormat.Rg8,
                Chunk.SIZE, Chunk.HEIGHT, Chunk.SIZE
            );

            lightMapId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture3d, lightMapId);

            Gl.TexParameteri(TextureTarget.Texture3d, TextureParameterName.TextureWrapS, TextureWrapMode.ClampToEdge);
            Gl.TexParameteri(TextureTarget.Texture3d, TextureParameterName.TextureWrapT, TextureWrapMode.ClampToEdge);
            Gl.TexParameteri(TextureTarget.Texture3d, TextureParameterName.TextureWrapR, TextureWrapMode.ClampToEdge);
            Gl.TexParameteri(TextureTarget.Texture3d, TextureParameterName.TextureMinFilter, TextureMinFilter.Linear);
            Gl.TexParameteri(TextureTarget.Texture3d, TextureParameterName.TextureMagFilter, TextureMagFilter.Linear);

            Gl.TexStorage3D(
                TextureTarget.Texture3d, 1,
                InternalFormat.R8,
                Chunk.SIZE + 1, Chunk.HEIGHT + 1, Chunk.SIZE + 1
            );
        }

        public void Update()
        {
            // Building the data map

            unsafe
            {
                float[,,,] data = new float[Chunk.SIZE, Chunk.HEIGHT, Chunk.SIZE, 2];
                for (int x = 0; x < Chunk.SIZE; x++)
                {
                    for (int z = 0; z < Chunk.SIZE; z++)
                    {
                        bool drawSunLight = true;
                        for (int y = Chunk.HEIGHT - 1; y >= 0; y--)
                        {
                            Block block = chunk.blocks[x, y, z];
                            bool blockTransparent = block.IsTransparent;
                            if (!blockTransparent)
                                drawSunLight = false;

                            data[z, y, x, 0] = blockTransparent ? 0f : 1f;
                            data[z, y, x, 1] = drawSunLight || block is TorchBlock ? 1f : 0f;
                        }
                    }
                }

                fixed (float* dataPtr = data)
                {
                    Gl.BindTexture(TextureTarget.Texture3d, dataMapId);
                    Gl.TexSubImage3D(
                        TextureTarget.Texture3d, 0,
                        0, 0, 0,
                        Chunk.SIZE, Chunk.HEIGHT, Chunk.SIZE,
                        PixelFormat.Rg, PixelType.Float,
                        new IntPtr(dataPtr)
                    );
                }
            }

            // Calling the compute shader

            Gl.BindImageTexture(
                0, dataMapId, 0,
                false, 0,
                BufferAccess.ReadWrite,
                InternalFormat.Rg8
            );

            Gl.BindImageTexture(
                1, lightMapId, 0,
                false, 0,
                BufferAccess.WriteOnly,
                InternalFormat.R8
            );

            Resources.LightComputeShader.Run();
        }

        public void Bind()
        {
            Gl.BindTexture(TextureTarget.Texture3d, lightMapId);
            Gl.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);
        }

        public void Dispose() => Gl.DeleteTextures(dataMapId, lightMapId);
    }
}
