using System;
using System.Threading.Tasks;
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

        private bool needsUpdate;
        private byte[,,] data;

        public LightMap(Chunk chunk)
        {
            this.chunk = chunk;
            needsUpdate = false;

            dataMapId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture3d, dataMapId);
            Gl.TexStorage3D(
                TextureTarget.Texture3d, 1,
                InternalFormat.R8ui,
                Chunk.SIZE * 3, Chunk.HEIGHT, Chunk.SIZE * 3
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

        public void BuildDataMap()
        {
            byte[,,] data = new byte[Chunk.SIZE * 3, Chunk.HEIGHT, Chunk.SIZE * 3];

            int startChunkX = chunk.xOffset / Chunk.SIZE - 1;
            int startChunkZ = chunk.zOffset / Chunk.SIZE - 1;

            for (int chunkX = 0; chunkX < 3; chunkX++)
            {
                for (int chunkZ = 0; chunkZ < 3; chunkZ++)
                {
                    Chunk currentChunk = chunk.world.GetChunk(
                        startChunkX + chunkX,
                        startChunkZ + chunkZ
                    );

                    if (currentChunk == null)
                        continue;

                    for (int x = 0; x < Chunk.SIZE; x++)
                    {
                        for (int z = 0; z < Chunk.SIZE; z++)
                        {
                            byte sunLight = 8;
                            for (int y = Chunk.HEIGHT - 1; y >= 0; y--)
                            {
                                Block block = currentChunk.blocks[x, y, z];
                                bool blockTransparent = block.IsTransparent;
                                if (!blockTransparent)
                                    sunLight = 0;

                                data[z + chunkZ * Chunk.SIZE, y, x + chunkX * Chunk.SIZE] = (byte)(
                                    (blockTransparent ? 0x10 : 0x00)
                                  | Math.Max(sunLight, block.LightLevel) & 0x0F
                                );
                            }
                        }
                    }
                }
            }

            this.data = data;
            needsUpdate = true;
        }

        public unsafe bool UpdateIfNeeded()
        {
            if (!needsUpdate)
                return false;
            needsUpdate = false;

            fixed (void* dataPtr = data)
            {
                Gl.BindTexture(TextureTarget.Texture3d, dataMapId);
                Gl.TexSubImage3D(
                    TextureTarget.Texture3d, 0,
                    0, 0, 0,
                    Chunk.SIZE * 3, Chunk.HEIGHT, Chunk.SIZE * 3,
                    PixelFormat.RedInteger, PixelType.UnsignedByte,
                    new IntPtr(dataPtr)
                );
            }

            Gl.BindImageTexture(
                0, dataMapId, 0,
                false, 0,
                BufferAccess.ReadWrite,
                InternalFormat.R8ui
            );

            Gl.BindImageTexture(
                1, lightMapId, 0,
                false, 0,
                BufferAccess.WriteOnly,
                InternalFormat.R8
            );

            Resources.LightComputeShader.Run();
            data = null;
            return true;
        }

        public void Bind()
        {
            Gl.BindTexture(TextureTarget.Texture3d, lightMapId);
            Gl.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);
        }

        public void Dispose() => Gl.DeleteTextures(dataMapId, lightMapId);
    }
}
