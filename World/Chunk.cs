using Mycraft.Blocks;
using Mycraft.Graphics;
using Mycraft.Utils;
using OpenGL;
using System.Collections.Generic;

namespace Mycraft.World
{
    public class Chunk : VertexArray
    {
        public const int SIZE = 16;
        public const int HEIGHT = 256;

        public bool needsUpdate;
        public readonly Block[,,] blocks;
        public readonly int[,] groundLevel;

        public readonly GameWorld world;
        public readonly int xOffset, zOffset;

        public Chunk(GameWorld world, int x, int z)
            : base(PrimitiveType.Quads, new int[] { 3, 2, 1 })
        {
            blocks = new Block[SIZE, HEIGHT, SIZE];
            groundLevel = new int[SIZE, SIZE];

            this.world = world;
            xOffset = x * SIZE;
            zOffset = z * SIZE;
        }

        public new void Draw()
        {
            Resources.BlocksTexture.Bind();
            base.Draw();
        }

        public void UpToDateMesh()
        {
            if (!needsUpdate) return;
            needsUpdate = false;

            List<float> mesh = new List<float>();

            for (int cx = 0; cx < SIZE; cx++)
                for (int cz = 0; cz < SIZE; cz++)
                    for (int cy = 0; cy < HEIGHT; cy++)
                    {
                        Block block = blocks[cx, cy, cz];
                        block.EmitVertices(mesh, this, cx, cy, cz);
                    }

            Data = mesh.ToArray();
        }
    }
}
