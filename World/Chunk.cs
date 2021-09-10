using Mycraft.Graphics;
using OpenGL;
using System.Collections.Generic;

namespace Mycraft.World
{
    public class Chunk : VertexArray
    {
        public const int CHUNK_SIZE = 16;

        private readonly Texture testTexture;

        public Chunk()
            : base(PrimitiveType.Quads, new int[] { 3, 2 })
        {
            testTexture = new Texture(@"resources\textures\test_texture.png");
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

        public void RegenerateMesh()
        {
            List<float> mesh = new List<float>();

            for (float x = 0; x < CHUNK_SIZE; x++)
                for (float z = 0; z < CHUNK_SIZE; z++)
                    for (float y = 0; y < 3; y++)
                        AddBlock(mesh, x, y, z);

            Data = mesh.ToArray();
        }

        private void AddBlock(List<float> mesh, float x, float y, float z)
            => mesh.AddRange(new float[] {
                // Back
                x,      y,      z,         1f, 1f,
                x,      y + 1f, z,         1f, 0f,
                x + 1f, y + 1f, z,         0f, 0f,
                x + 1f, y,      z,         0f, 1f,

                // Front     
                x + 1f, y,      z + 1f,    1f, 1f,
                x + 1f, y + 1f, z + 1f,    1f, 0f,
                x,      y + 1f, z + 1f,    0f, 0f,
                x,      y,      z + 1f,    0f, 1f,

                // Right     
                x + 1f, y,      z,         1f, 1f,
                x + 1f, y + 1f, z,         1f, 0f,
                x + 1f, y + 1f, z + 1f,    0f, 0f,
                x + 1f, y,      z + 1f,    0f, 1f,

                // Left      
                x,      y,      z + 1f,    1f, 1f,
                x,      y + 1f, z + 1f,    1f, 0f,
                x,      y + 1f, z,         0f, 0f,
                x,      y,      z,         0f, 1f,

                // Top       
                x + 1f, y + 1f, z + 1f,    1f, 1f,
                x + 1f, y + 1f, z,         1f, 0f,
                x,      y + 1f, z,         0f, 0f,
                x,      y + 1f, z + 1f,    0f, 1f,

                // Bottom    
                x,      y,      z + 1f,    1f, 1f,
                x,      y,      z,         1f, 0f,
                x + 1f, y,      z,         0f, 0f,
                x + 1f, y,      z + 1f,    0f, 1f
            });
    }
}
