using System.Collections.Generic;
using OpenGL;
using Mycraft.World;

namespace Mycraft.Blocks
{
    public class PlantBlock : Block
    {
        public override bool IsTransparent => true;
        public override bool HasCollider => false;

        public PlantBlock(int textureId)
            : base(textureId) { }

        public override void EmitMesh(MeshBuildingContext context, int x, int y, int z)
        {
            context.AddDoubleSidedQuad(
                new Quad(
                    new Vertex3f(x + 1f, y,      z + 1f),
                    new Vertex3f(x + 1f, y + 1f, z + 1f),
                    new Vertex3f(x,      y + 1f, z),
                    new Vertex3f(x,      y,      z),
                    GetTexture(BlockSide.Top),
                    1f
                )
            );

            context.AddDoubleSidedQuad(
                new Quad(
                    new Vertex3f(x,      y,      z + 1f),
                    new Vertex3f(x,      y + 1f, z + 1f),
                    new Vertex3f(x + 1f, y + 1f, z),
                    new Vertex3f(x + 1f, y,      z),
                    GetTexture(BlockSide.Top),
                    .9f
                )
            );
        }
    }
}
