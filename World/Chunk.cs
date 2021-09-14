using Mycraft.Graphics;
using Mycraft.Utils;
using OpenGL;
using System.Collections.Generic;

namespace Mycraft.World
{
    public enum BlockSide
    {
        Front, Back,
        Right, Left,
        Top, Bottom
    }

    public enum Block
    {
        Air, Void, Test
    }

    public class Chunk : VertexArray
    {
        public const int SIZE = 16;
        public const int HEIGHT = 256;

        public bool needsUpdate;
        public readonly Block[,,] blocks;

        private readonly GameWorld world;
        private readonly int chunkX, chunkZ;

        public Chunk(GameWorld world, int x, int z)
            : base(PrimitiveType.Quads, new int[] { 3, 2 })
        {
            blocks = new Block[SIZE, HEIGHT, SIZE];
            this.world = world;
            chunkX = x;
            chunkZ = z;
        }

        public new void Draw()
        {
            Resources.TestTexture.Bind();
            base.Draw();
        }

        public void Generate()
        {
            for (int x = 0; x < SIZE; x++)
                for (int z = 0; z < SIZE; z++)
                    for (int y = 0; y < 3; y++)
                        blocks[x, y, z] = Block.Test;
        }

        private Block GetBlockExtended(int x, int y, int z)
        {
            if (y < 0 || y >= HEIGHT)
                return Block.Void;

            if (x >= 0 && x < SIZE
             && z >= 0 && z < SIZE)
                return blocks[x, y, z];

            return world.GetBlock(chunkX * SIZE + x, y, chunkZ * SIZE + z);
        }

        public void UpToDateMesh()
        {
            if (!needsUpdate) return;
            needsUpdate = false;

            List<float> mesh = new List<float>();

            float chunkX = this.chunkX * SIZE;
            float chunkZ = this.chunkZ * SIZE;

            for (int cx = 0; cx < SIZE; cx++)
                for (int cz = 0; cz < SIZE; cz++)
                    for (int cy = 0; cy < HEIGHT; cy++)
                    {
                        if (blocks[cx, cy, cz] == Block.Air)
                            continue;

                        float wx = chunkX + cx;
                        float wz = chunkZ + cz;
                        float wy = cy;

                        // Bottom
                        if (GetBlockExtended(cx, cy - 1, cz) <= Block.Void)
                            mesh.AddRange(new float[] {
                                wx,      wy,      wz + 1f,    1f, 1f,
                                wx,      wy,      wz,         1f, 0f,
                                wx + 1f, wy,      wz,         0f, 0f,
                                wx + 1f, wy,      wz + 1f,    0f, 1f
                            });

                        // Top
                        if (GetBlockExtended(cx, cy + 1, cz) <= Block.Void)
                            mesh.AddRange(new float[] {
                                wx + 1f, wy + 1f, wz + 1f,    1f, 1f,
                                wx + 1f, wy + 1f, wz,         1f, 0f,
                                wx,      wy + 1f, wz,         0f, 0f,
                                wx,      wy + 1f, wz + 1f,    0f, 1f
                            });

                        // Left
                        if (GetBlockExtended(cx - 1, cy, cz) <= Block.Void)
                            mesh.AddRange(new float[] {
                                wx,      wy,      wz + 1f,    1f, 1f,
                                wx,      wy + 1f, wz + 1f,    1f, 0f,
                                wx,      wy + 1f, wz,         0f, 0f,
                                wx,      wy,      wz,         0f, 1f
                            });

                        // Right
                        if (GetBlockExtended(cx + 1, cy, cz) <= Block.Void)
                            mesh.AddRange(new float[] {
                                // Top       
                                wx + 1f, wy,      wz,         1f, 1f,
                                wx + 1f, wy + 1f, wz,         1f, 0f,
                                wx + 1f, wy + 1f, wz + 1f,    0f, 0f,
                                wx + 1f, wy,      wz + 1f,    0f, 1f
                            });

                        // Back
                        if (GetBlockExtended(cx, cy, cz - 1) <= Block.Void)
                            mesh.AddRange(new float[] {
                                wx,      wy,      wz,         1f, 1f,
                                wx,      wy + 1f, wz,         1f, 0f,
                                wx + 1f, wy + 1f, wz,         0f, 0f,
                                wx + 1f, wy,      wz,         0f, 1f
                            });

                        // Front
                        if (GetBlockExtended(cx, cy, cz + 1) <= Block.Void)
                            mesh.AddRange(new float[] {     
                                wx + 1f, wy,      wz + 1f,    1f, 1f,
                                wx + 1f, wy + 1f, wz + 1f,    1f, 0f,
                                wx,      wy + 1f, wz + 1f,    0f, 0f,
                                wx,      wy,      wz + 1f,    0f, 1f
                            });
                    }

            Data = mesh.ToArray();
        }
    }
}
