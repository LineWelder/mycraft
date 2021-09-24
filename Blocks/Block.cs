using Mycraft.World;
using OpenGL;
using System.Collections.Generic;

namespace Mycraft.Blocks
{
    public enum BlockSide
    {
        Front, Back,
        Right, Left,
        Top, Bottom
    }

    public class Block
    {
        /// <summary>
        /// If is false, the neighbouring blocks' faces touching this block will not render
        /// </summary>
        public virtual bool IsTransparent => false;

        public virtual bool HasCollider => true;

        /// <summary>
        /// If is false, the block will not render
        /// </summary>
        public virtual bool IsVisible => true;

        private int textureId;

        public Block(int textureId)
        {
            this.textureId = textureId;
        }

        public virtual int GetTexture(BlockSide side)
            => textureId;

        public virtual void EmitVertices(List<float> mesh, Chunk chunk, int x, int y, int z)
        {
            if (!IsVisible)
                return;

            float wx = chunk.xOffset + x;
            float wz = chunk.zOffset + z;
            float wy = y;

            // Bottom
            if (HasFace(GetChunkBlock(chunk, x, y - 1, z)))
            {
                Vertex4f texCoords = GetTextureCoords(GetTexture(BlockSide.Bottom));
                mesh.AddRange(new float[] {
                    wx,      wy,      wz + 1f,    texCoords.z, texCoords.w, .7f,
                    wx,      wy,      wz,         texCoords.z, texCoords.y, .7f,
                    wx + 1f, wy,      wz,         texCoords.x, texCoords.y, .7f,
                    wx + 1f, wy,      wz + 1f,    texCoords.x, texCoords.w, .7f
                });
            }

            // Top
            if (HasFace(GetChunkBlock(chunk, x, y + 1, z)))
            {
                Vertex4f texCoords = GetTextureCoords(GetTexture(BlockSide.Top));
                mesh.AddRange(new float[] {
                    wx + 1f, wy + 1f, wz + 1f,    texCoords.z, texCoords.w, 1f,
                    wx + 1f, wy + 1f, wz,         texCoords.z, texCoords.y, 1f,
                    wx,      wy + 1f, wz,         texCoords.x, texCoords.y, 1f,
                    wx,      wy + 1f, wz + 1f,    texCoords.x, texCoords.w, 1f
                });
            }

            // Left
            if (HasFace(GetChunkBlock(chunk, x - 1, y, z)))
            {
                Vertex4f texCoords = GetTextureCoords(GetTexture(BlockSide.Left));
                mesh.AddRange(new float[] {
                    wx,      wy,      wz + 1f,    texCoords.z, texCoords.w, .8f,
                    wx,      wy + 1f, wz + 1f,    texCoords.z, texCoords.y, .8f,
                    wx,      wy + 1f, wz,         texCoords.x, texCoords.y, .8f,
                    wx,      wy,      wz,         texCoords.x, texCoords.w, .8f
                });
            }

            // Right
            if (HasFace(GetChunkBlock(chunk, x + 1, y, z)))
            {
                Vertex4f texCoords = GetTextureCoords(GetTexture(BlockSide.Right));
                mesh.AddRange(new float[] {
                    wx + 1f, wy,      wz,         texCoords.z, texCoords.w, .8f,
                    wx + 1f, wy + 1f, wz,         texCoords.z, texCoords.y, .8f,
                    wx + 1f, wy + 1f, wz + 1f,    texCoords.x, texCoords.y, .8f,
                    wx + 1f, wy,      wz + 1f,    texCoords.x, texCoords.w, .8f
                });
            }

            // Back
            if (HasFace(GetChunkBlock(chunk, x, y, z - 1)))
            {
                Vertex4f texCoords = GetTextureCoords(GetTexture(BlockSide.Back));
                mesh.AddRange(new float[] {
                    wx,      wy,      wz,         texCoords.z, texCoords.w, .7f,
                    wx,      wy + 1f, wz,         texCoords.z, texCoords.y, .7f,
                    wx + 1f, wy + 1f, wz,         texCoords.x, texCoords.y, .7f,
                    wx + 1f, wy,      wz,         texCoords.x, texCoords.w, .7f
                });
            }

            // Front
            if (HasFace(GetChunkBlock(chunk, x, y, z + 1)))
            {
                Vertex4f texCoords = GetTextureCoords(GetTexture(BlockSide.Front));
                mesh.AddRange(new float[] {
                    wx + 1f, wy,      wz + 1f,    texCoords.z, texCoords.w, .9f,
                    wx + 1f, wy + 1f, wz + 1f,    texCoords.z, texCoords.y, .9f,
                    wx,      wy + 1f, wz + 1f,    texCoords.x, texCoords.y, .9f,
                    wx,      wy,      wz + 1f,    texCoords.x, texCoords.w, .9f
                });
            }
        }

        protected Block GetChunkBlock(Chunk chunk, int x, int y, int z)
        {
            if (y < 0 || y >= Chunk.HEIGHT)
                return BlockRegistry.Void;

            if (x >= 0 && x < Chunk.SIZE
             && z >= 0 && z < Chunk.SIZE)
                return chunk.blocks[x, y, z];

            return chunk.world.GetBlock(chunk.xOffset + x, y, chunk.zOffset + z);
        }

        protected bool HasFace(Block neighbour)
            => !IsTransparent && neighbour.IsTransparent
            || IsTransparent && !neighbour.IsVisible;

        public static Vertex3i GetNeighbour(Vertex3i coords, BlockSide side)
        {
            switch (side)
            {
                case BlockSide.Front:
                    return coords + new Vertex3i(0, 0, 1);
                case BlockSide.Back:
                    return coords + new Vertex3i(0, 0, -1);
                case BlockSide.Right:
                    return coords + new Vertex3i(1, 0, 0);
                case BlockSide.Left:
                    return coords + new Vertex3i(-1, 0, 0);
                case BlockSide.Top:
                    return coords + new Vertex3i(0, 1, 0);
                case BlockSide.Bottom:
                    return coords + new Vertex3i(0, -1, 0);
                default:
                    return coords;
            }
        }

        public static Vertex4f GetTextureCoords(int textureId)
            => new Vertex4f(
                (textureId % 4) * .25f,
                (textureId / 4) * .25f,
                (textureId % 4 + 1f) * .25f,
                (textureId / 4 + 1f) * .25f
            );
    }
}
