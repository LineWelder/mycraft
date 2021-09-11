using Mycraft.Graphics;
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

        private readonly Texture testTexture;
        private readonly Block[,,] blocks;

        public Chunk()
            : base(PrimitiveType.Quads, new int[] { 3, 2 })
        {
            testTexture = new Texture(@"resources\textures\test_texture.png");
            blocks = new Block[CHUNK_SIZE, CHUNK_HEIGHT, CHUNK_SIZE];
        }

        public new void Draw()
        {
            testTexture.Bind();
            base.Draw();
        }

        public new void Dispose()
        {
            testTexture.Dispose();
            base.Dispose();
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

            for (int x = 0; x < CHUNK_SIZE; x++)
                for (int z = 0; z < CHUNK_SIZE; z++)
                    for (int y = 0; y < CHUNK_HEIGHT; y++)
                    {
                        if (blocks[x, y, z] == Block.Air)
                            continue;

                        // Bottom
                        if (y == 0 || blocks[x, y - 1, z] == Block.Air)
                            mesh.AddRange(new float[] {
                                x,      y,      z + 1f,    1f, 1f,
                                x,      y,      z,         1f, 0f,
                                x + 1f, y,      z,         0f, 0f,
                                x + 1f, y,      z + 1f,    0f, 1f
                            });

                        // Top
                        if (y == CHUNK_HEIGHT - 1 || blocks[x, y + 1, z] == Block.Air)
                            mesh.AddRange(new float[] {
                                x + 1f, y + 1f, z + 1f,    1f, 1f,
                                x + 1f, y + 1f, z,         1f, 0f,
                                x,      y + 1f, z,         0f, 0f,
                                x,      y + 1f, z + 1f,    0f, 1f
                            });

                        // Left
                        if (x == 0 || blocks[x - 1, y, z] == Block.Air)
                            mesh.AddRange(new float[] {
                                x,      y,      z + 1f,    1f, 1f,
                                x,      y + 1f, z + 1f,    1f, 0f,
                                x,      y + 1f, z,         0f, 0f,
                                x,      y,      z,         0f, 1f
                            });

                        // Right
                        if (x == CHUNK_SIZE - 1 || blocks[x + 1, y, z] == Block.Air)
                            mesh.AddRange(new float[] {
                                // Top       
                                x + 1f, y,      z,         1f, 1f,
                                x + 1f, y + 1f, z,         1f, 0f,
                                x + 1f, y + 1f, z + 1f,    0f, 0f,
                                x + 1f, y,      z + 1f,    0f, 1f
                            });

                        // Back
                        if (z == 0 || blocks[x, y, z - 1] == Block.Air)
                            mesh.AddRange(new float[] {
                                x,      y,      z,         1f, 1f,
                                x,      y + 1f, z,         1f, 0f,
                                x + 1f, y + 1f, z,         0f, 0f,
                                x + 1f, y,      z,         0f, 1f
                            });

                        // Front
                        if (z == CHUNK_SIZE - 1 || blocks[x, y, z + 1] == Block.Air)
                            mesh.AddRange(new float[] {     
                                x + 1f, y,      z + 1f,    1f, 1f,
                                x + 1f, y + 1f, z + 1f,    1f, 0f,
                                x,      y + 1f, z + 1f,    0f, 0f,
                                x,      y,      z + 1f,    0f, 1f
                            });
                    }

            Data = mesh.ToArray();
        }
    }
}
