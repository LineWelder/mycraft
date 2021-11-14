using System.Collections.Generic;
using OpenGL;
using Mycraft.World;

namespace Mycraft.Blocks
{
    public class LiquidBlock : Block
    {
        public override bool IsTransparent => true;
        public override bool HasCollider => false;
        public override bool IsSelectable => false;

        public LiquidBlock(int textureId)
            : base(textureId) { }

        public override void EmitMesh(MeshBuildingContext context, int x, int y, int z)
        {
            Block topNeighbour = GetChunkBlock(context.chunk, x, y + 1, z);
            float height = topNeighbour is LiquidBlock ? 1f : 15f / 16f;

            // Top
            if (!(topNeighbour is LiquidBlock))
            {
                context.AddTransparentQuad(QuadGenerator.Top(
                    new Vertex3f(x + 1f, y + height, z + 1f),
                    new Vertex2f(1f, 1f),
                    GetTexture(BlockSide.Top),
                    1f
                ));
            }

            // Bottom
            if ( HasFace(GetChunkBlock(context.chunk, x, y - 1, z)) )
                context.AddTransparentQuad(QuadGenerator.Bottom(
                    new Vertex3f(x + 1f, y, z + 1f),
                    new Vertex2f(1f, 1f),
                    GetTexture(BlockSide.Bottom),
                    .7f
                ));

            // Left
            if ( HasFace(GetChunkBlock(context.chunk, x - 1, y, z)) )
                context.AddTransparentQuad(QuadGenerator.Left(
                    new Vertex3f(x, y, z + 1f),
                    new Vertex2f(1f, height),
                    GetTexture(BlockSide.Left),
                    .8f
                ));

            // Right
            if ( HasFace(GetChunkBlock(context.chunk, x + 1, y, z)) )
                context.AddTransparentQuad(QuadGenerator.Right(
                    new Vertex3f(x + 1f, y, z + 1f),
                    new Vertex2f(1f, height),
                    GetTexture(BlockSide.Right),
                    .8f
                ));

            // Back
            if ( HasFace(GetChunkBlock(context.chunk, x, y, z - 1)) )
                context.AddTransparentQuad(QuadGenerator.Back(
                    new Vertex3f(x + 1f, y, z),
                    new Vertex2f(1f, height),
                    GetTexture(BlockSide.Back),
                    .7f
                ));

            // Front
            if ( HasFace(GetChunkBlock(context.chunk, x, y, z + 1)) )
                context.AddTransparentQuad(QuadGenerator.Front(
                     new Vertex3f(x, y, z + 1f),
                     new Vertex2f(1f, height),
                     GetTexture(BlockSide.Front),
                     .9f
                 ));
        }

        private bool HasFace(Block neighbour)
            => neighbour.IsTransparent && !(neighbour is LiquidBlock);
    }
}
