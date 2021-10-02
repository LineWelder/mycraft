using System.Collections.Generic;
using OpenGL;
using Mycraft.World;

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

        public virtual void EmitMesh(List<Quad> mesh, Chunk chunk, int x, int y, int z)
        {
            if (!IsVisible)
                return;

            float wx = chunk.xOffset + x;
            float wz = chunk.zOffset + z;
            float wy = y;

            // Bottom
            if (HasFace(GetChunkBlock(chunk, x, y - 1, z)))
                mesh.Add(QuadGenerator.Bottom(
                    new Vertex3f(wx + 1f, wy, wz + 1f),
                    new Vertex2f(1f, 1f),
                    GetTextureCoords(GetTexture(BlockSide.Bottom)),
                    .7f
                ));

            // Top
            if (HasFace(GetChunkBlock(chunk, x, y + 1, z)))
                mesh.Add(QuadGenerator.Top(
                    new Vertex3f(wx + 1f, wy + 1f, wz + 1f),
                    new Vertex2f(1f, 1f),
                    GetTextureCoords(GetTexture(BlockSide.Top)),
                    1f
                ));

            // Left
            if (HasFace(GetChunkBlock(chunk, x - 1, y, z)))
                mesh.Add(QuadGenerator.Left(
                    new Vertex3f(wx, wy, wz + 1f),
                    new Vertex2f(1f, 1f),
                    GetTextureCoords(GetTexture(BlockSide.Left)),
                    .8f
                ));

            // Right
            if (HasFace(GetChunkBlock(chunk, x + 1, y, z)))
                mesh.Add(QuadGenerator.Right(
                    new Vertex3f(wx + 1f, wy, wz + 1f),
                    new Vertex2f(1f, 1f),
                    GetTextureCoords(GetTexture(BlockSide.Right)),
                    .8f
                ));

            // Back
            if (HasFace(GetChunkBlock(chunk, x, y, z - 1)))
                mesh.Add(QuadGenerator.Back(
                    new Vertex3f(wx + 1f, wy, wz),
                    new Vertex2f(1f, 1f),
                    GetTextureCoords(GetTexture(BlockSide.Back)),
                    .7f
                ));

            // Front
            if (HasFace(GetChunkBlock(chunk, x, y, z + 1)))
                mesh.Add(QuadGenerator.Front(
                     new Vertex3f(wx, wy, wz + 1f),
                     new Vertex2f(1f, 1f),
                     GetTextureCoords(GetTexture(BlockSide.Front)),
                     .9f
                 ));
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
