using Mycraft.Blocks;
using Mycraft.Graphics;
using OpenGL;
using System;
using System.Collections.Generic;

namespace Mycraft.Physics
{
    public class ParticleSystem : VertexArray
    {
        private struct Particle
        {
            public Vertex3f position;
            public Vertex3f velocity;
        }

        private readonly Particle[] particles;
        private readonly float size;
        private readonly Block block;

        public float GetRangedRandom(Random rand, float start, float end)
            => (float)(rand.NextDouble() * (end - start) + start);

        public ParticleSystem(Vertex3f spawnAreaStart, Vertex3f spawnAreaEnd, int count, float size, Block block)
            : base(PrimitiveType.Quads, new int[] { 3, 2, 2 })
        {
            particles = new Particle[count];
            this.size = size;
            this.block = block;

            Random rand = new Random();
            for (int i = 0; i < count; i++)
                particles[i] = new Particle
                {
                    position = new Vertex3f(
                        GetRangedRandom(rand, spawnAreaStart.x, spawnAreaEnd.x),
                        GetRangedRandom(rand, spawnAreaStart.y, spawnAreaEnd.y),
                        GetRangedRandom(rand, spawnAreaStart.z, spawnAreaEnd.z)
                    ),
                    velocity = new Vertex3f()
                };
        }

        public void UpdateVertices()
        {
            List<float> mesh = new List<float>();

            foreach (Particle particle in particles)
            {
                Vertex4f texCoords = Block.GetTextureCoords(block.GetTexture(BlockSide.Top));
                Vertex3f pos = particle.position;
                float offset = size / 2f;

                mesh.AddRange(new float[] {
                    pos.x, pos.y, pos.z,   offset, -offset,  texCoords.z, texCoords.w,
                    pos.x, pos.y, pos.z,   offset,  offset,  texCoords.z, texCoords.y,
                    pos.x, pos.y, pos.z,  -offset,  offset,  texCoords.x, texCoords.y,
                    pos.x, pos.y, pos.z,  -offset, -offset,  texCoords.x, texCoords.w
                });
            }

            Data = mesh.ToArray();
        }
    }
}
