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

        public virtual bool IsSelectable => true;

        private readonly int textureId;

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

            // Bottom
            if (GetChunkBlock(chunk, x, y - 1, z).IsTransparent)
                mesh.Add(QuadGenerator.Bottom(
                    new Vertex3f(x + 1f, y, z + 1f),
                    new Vertex2f(1f, 1f),
                    GetTexture(BlockSide.Bottom),
                    .7f
                ));

            // Top
            if (GetChunkBlock(chunk, x, y + 1, z).IsTransparent)
                mesh.Add(QuadGenerator.Top(
                    new Vertex3f(x + 1f, y + 1f, z + 1f),
                    new Vertex2f(1f, 1f),
                    GetTexture(BlockSide.Top),
                    1f
                ));

            // Left
            if (GetChunkBlock(chunk, x - 1, y, z).IsTransparent)
                mesh.Add(QuadGenerator.Left(
                    new Vertex3f(x, y, z + 1f),
                    new Vertex2f(1f, 1f),
                    GetTexture(BlockSide.Left),
                    .8f
                ));

            // Right
            if (GetChunkBlock(chunk, x + 1, y, z).IsTransparent)
                mesh.Add(QuadGenerator.Right(
                    new Vertex3f(x + 1f, y, z + 1f),
                    new Vertex2f(1f, 1f),
                    GetTexture(BlockSide.Right),
                    .8f
                ));

            // Back
            if (GetChunkBlock(chunk, x, y, z - 1).IsTransparent)
                mesh.Add(QuadGenerator.Back(
                    new Vertex3f(x + 1f, y, z),
                    new Vertex2f(1f, 1f),
                    GetTexture(BlockSide.Back),
                    .7f
                ));

            // Front
            if (GetChunkBlock(chunk, x, y, z + 1).IsTransparent)
                mesh.Add(QuadGenerator.Front(
                     new Vertex3f(x, y, z + 1f),
                     new Vertex2f(1f, 1f),
                     GetTexture(BlockSide.Front),
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
    }
}
