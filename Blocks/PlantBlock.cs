using System.Collections.Generic;
using OpenGL;
using Mycraft.World;

namespace Mycraft.Blocks
{
    public class PlantBlock : Block
    {
        public override bool IsTransparent => true;
        public override bool HasCollider => false;
        public override bool IsVisible => true;

        public PlantBlock(int textureId)
            : base(textureId) { }

        public override void EmitMesh(List<Quad> mesh, Chunk chunk, int x, int y, int z)
        {
            mesh.Add(
                new Quad(
                    new Vertex3f(x + 1f, y,      z + 1f),
                    new Vertex3f(x + 1f, y + 1f, z + 1f),
                    new Vertex3f(x,      y + 1f, z),
                    new Vertex3f(x,      y,      z),
                    GetTexture(BlockSide.Top),
                    1f
                )
            );

            mesh.Add(
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
