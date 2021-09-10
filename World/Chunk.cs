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

        public void RegenerateMesh()
        {
            List<float> mesh = new List<float>();

            AddBlock(mesh);

            Data = mesh.ToArray();
        }

        private void AddBlock(List<float> mesh)
            => mesh.AddRange(new float[] {
                // Back
                0f, 0f, 0f,  1f, 1f,
                0f, 1f, 0f,  1f, 0f,
                1f, 1f, 0f,  0f, 0f,
                1f, 0f, 0f,  0f, 1f,

                // Front     
                1f, 0f, 1f,  1f, 1f,
                1f, 1f, 1f,  1f, 0f,
                0f, 1f, 1f,  0f, 0f,
                0f, 0f, 1f,  0f, 1f,

                // Right     
                1f, 0f, 0f,  1f, 1f,
                1f, 1f, 0f,  1f, 0f,
                1f, 1f, 1f,  0f, 0f,
                1f, 0f, 1f,  0f, 1f,

                // Left      
                0f, 0f, 1f,  1f, 1f,
                0f, 1f, 1f,  1f, 0f,
                0f, 1f, 0f,  0f, 0f,
                0f, 0f, 0f,  0f, 1f,

                // Top       
                1f, 1f, 1f,  1f, 1f,
                1f, 1f, 0f,  1f, 0f,
                0f, 1f, 0f,  0f, 0f,
                0f, 1f, 1f,  0f, 1f,

                // Bottom    
                0f, 0f, 1f,  1f, 1f,
                0f, 0f, 0f,  1f, 0f,
                1f, 0f, 0f,  0f, 0f,
                1f, 0f, 1f,  0f, 1f
            });
    }
}
