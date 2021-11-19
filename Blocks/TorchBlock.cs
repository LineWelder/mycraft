using System.Collections.Generic;
using OpenGL;

using Mycraft.World;

namespace Mycraft.Blocks
{
    public class TorchBlock : Block
    {
        public override bool IsTransparent => true;
        public override bool HasCollider => false;
        public override byte LightLevel => 16 * 7;

        public TorchBlock(int textureId)
            : base(textureId) { }

        public override void EmitMesh(MeshBuildingContext context, int x, int y, int z)
        {
            const float offset = 7f / 16f;

            // Left
            context.AddSolidQuad(QuadGenerator.Left(
                new Vertex3f(x + offset, y, z + 1f),
                new Vertex2f(1f, 1f),
                GetTexture(BlockSide.Left),
                .8f
            ));

            // Right
            context.AddSolidQuad(QuadGenerator.Right(
                new Vertex3f(x + 1f - offset, y, z + 1f),
                new Vertex2f(1f, 1f),
                GetTexture(BlockSide.Right),
                .8f
            ));

            // Back
            context.AddSolidQuad(QuadGenerator.Back(
                new Vertex3f(x + 1f, y, z + offset),
                new Vertex2f(1f, 1f),
                GetTexture(BlockSide.Back),
                .7f
            ));

            // Front
            context.AddSolidQuad(QuadGenerator.Front(
                new Vertex3f(x, y, z + 1f - offset),
                new Vertex2f(1f, 1f),
                GetTexture(BlockSide.Front),
                .9f
            ));

            // Top
            context.AddSolidQuad(QuadGenerator.Top(
                new Vertex3f(x + 1f - offset, y + 1f - offset, z + 1f - offset),
                new Vertex2f(1f - 2f * offset),
                8,
                1f
            ));
        }
    }
}
