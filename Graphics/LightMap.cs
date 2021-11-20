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

        private readonly uint sunDataMapId, blockDataMapId;
        private readonly uint sunLightMapId, blockLightMapId;

        private bool needsUpdate;
        private byte[,,] sunData, blockData;

        public LightMap(Chunk chunk)
        {
            this.chunk = chunk;
            needsUpdate = false;

            sunDataMapId = CreateDataMap();
            blockDataMapId = CreateDataMap();

            sunLightMapId = CreateLightMap();
            blockLightMapId = CreateLightMap();
        }

        private uint CreateDataMap()
        {
            uint glId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture3d, glId);
            Gl.TexStorage3D(
                TextureTarget.Texture3d, 1,
                InternalFormat.R8ui,
                Chunk.SIZE * 3, Chunk.HEIGHT, Chunk.SIZE * 3
            );

            return glId;
        }

        private uint CreateLightMap()
        {
            uint glId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture3d, glId);

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

            return glId;
        }

        public void BuildDataMap()
        {
            byte[,,] sunData = new byte[Chunk.SIZE * 3, Chunk.HEIGHT, Chunk.SIZE * 3];
            byte[,,] blockData = new byte[Chunk.SIZE * 3, Chunk.HEIGHT, Chunk.SIZE * 3];

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
                            byte sunLight = 15;
                            for (int y = Chunk.HEIGHT - 1; y >= 0; y--)
                            {
                                Block block = currentChunk.blocks[x, y, z];
                                bool blockTransparent = block.IsTransparent;
                                if (!blockTransparent)
                                    sunLight = 0;

                                sunData[z + chunkZ * Chunk.SIZE, y, x + chunkX * Chunk.SIZE] = (byte)(
                                    (blockTransparent ? 0x10 : 0x00)
                                  | sunLight & 0x0F
                                );

                                blockData[z + chunkZ * Chunk.SIZE, y, x + chunkX * Chunk.SIZE] = (byte)(
                                    (blockTransparent ? 0x10 : 0x00)
                                  | block.LightLevel & 0x0F
                                );
                            }
                        }
                    }
                }
            }

            this.sunData = sunData;
            this.blockData = blockData;
            needsUpdate = true;
        }

        public unsafe bool UpdateIfNeeded()
        {
            if (!needsUpdate)
                return false;
            needsUpdate = false;

            UpdateMap(sunData, sunDataMapId, sunLightMapId);
            UpdateMap(blockData, blockDataMapId, blockLightMapId);
            sunData = null;
            blockData = null;
            return true;
        }

        private unsafe void UpdateMap(byte[,,] data, uint dataMapId, uint lightMapId)
        {
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
        }

        public void Bind()
        {
            Gl.ActiveTexture(TextureUnit.Texture1);
            Gl.BindTexture(TextureTarget.Texture3d, sunLightMapId);

            Gl.ActiveTexture(TextureUnit.Texture2);
            Gl.BindTexture(TextureTarget.Texture3d, blockLightMapId);

            Gl.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);
        }

        public void Dispose()
            => Gl.DeleteTextures(
                sunDataMapId, blockDataMapId,
                sunLightMapId, blockLightMapId
            );
    }
}
