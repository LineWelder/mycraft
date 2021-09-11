using Mycraft.Graphics;
using Mycraft.Utils;
using OpenGL;
using System.Collections.Generic;

namespace Mycraft.World
{
    public enum Block
    {
        Air, Test
    }

    public class Chunk : VertexArray
    {
        public const int CHUNK_SIZE = 16;
        public const int CHUNK_HEIGHT = 256;

        private readonly GameWorld world;
        private readonly int x, z;

        private readonly Block[,,] blocks;

        public Chunk(GameWorld world, int x, int z)
            : base(PrimitiveType.Quads, new int[] { 3, 2 })
        {
            blocks = new Block[CHUNK_SIZE, CHUNK_HEIGHT, CHUNK_SIZE];
            this.x = x;
            this.z = z;
        }

        public new void Draw()
        {
            Resources.TestTexture.Bind();
            base.Draw();
        }

        public void Generate()
        {
            for (int x = 0; x < CHUNK_SIZE; x++)
                for (int z = 0; z < CHUNK_SIZE; z++)
                    for (int y = 0; y < 3; y++)
                        blocks[x, y, z] = Block.Test;
        }

        public void RegenerateMesh()
        {
            List<float> mesh = new List<float>();

            float chunkX = x * CHUNK_SIZE;
            float chunkZ = z * CHUNK_SIZE;

            for (int cx = 0; cx < CHUNK_SIZE; cx++)
                for (int cz = 0; cz < CHUNK_SIZE; cz++)
                    for (int cy = 0; cy < CHUNK_HEIGHT; cy++)
                    {
                        if (blocks[cx, cy, cz] == Block.Air)
                            continue;

                        float wx = chunkX + cx;
                        float wz = chunkZ + cz;
                        float wy = cy;

                        // Bottom
                        if (cy == 0 || blocks[cx, cy - 1, cz] == Block.Air)
                            mesh.AddRange(new float[] {
                                wx,      wy,      wz + 1f,    1f, 1f,
                                wx,      wy,      wz,         1f, 0f,
                                wx + 1f, wy,      wz,         0f, 0f,
                                wx + 1f, wy,      wz + 1f,    0f, 1f
                            });

                        // Top
                        if (cy == CHUNK_HEIGHT - 1 || blocks[cx, cy + 1, cz] == Block.Air)
                            mesh.AddRange(new float[] {
                                wx + 1f, wy + 1f, wz + 1f,    1f, 1f,
                                wx + 1f, wy + 1f, wz,         1f, 0f,
                                wx,      wy + 1f, wz,         0f, 0f,
                                wx,      wy + 1f, wz + 1f,    0f, 1f
                            });

                        // Left
                        if (cx == 0 || blocks[cx - 1, cy, cz] == Block.Air)
                            mesh.AddRange(new float[] {
                                wx,      wy,      wz + 1f,    1f, 1f,
                                wx,      wy + 1f, wz + 1f,    1f, 0f,
                                wx,      wy + 1f, wz,         0f, 0f,
                                wx,      wy,      wz,         0f, 1f
                            });

                        // Right
                        if (cx == CHUNK_SIZE - 1 || blocks[cx + 1, cy, cz] == Block.Air)
                            mesh.AddRange(new float[] {
                                // Top       
                                wx + 1f, wy,      wz,         1f, 1f,
                                wx + 1f, wy + 1f, wz,         1f, 0f,
                                wx + 1f, wy + 1f, wz + 1f,    0f, 0f,
                                wx + 1f, wy,      wz + 1f,    0f, 1f
                            });

                        // Back
                        if (cz == 0 || blocks[cx, cy, cz - 1] == Block.Air)
                            mesh.AddRange(new float[] {
                                wx,      wy,      wz,         1f, 1f,
                                wx,      wy + 1f, wz,         1f, 0f,
                                wx + 1f, wy + 1f, wz,         0f, 0f,
                                wx + 1f, wy,      wz,         0f, 1f
                            });

                        // Front
                        if (cz == CHUNK_SIZE - 1 || blocks[cx, cy, cz + 1] == Block.Air)
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
