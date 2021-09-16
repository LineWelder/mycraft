using Mycraft.Blocks;
using Mycraft.Graphics;
using Mycraft.World;
using OpenGL;
using System;
using System.Collections.Generic;

namespace Mycraft.Physics
{
    public class ParticleSystem : VertexArray
    {
        private class Particle : FallingBox
        {
            public Block block;

            public Particle(GameWorld world, Block block, Vertex3f position, Vertex3f velocity)
                : base(world, position, new Vertex3f())
            {
                this.block = block;
                Velocity = velocity;
            }
        }

        private readonly List<Particle> particles;
        private readonly GameWorld world;
        private readonly float size;

        public float GetRangedRandom(Random rand, float start, float end)
            => (float)(rand.NextDouble() * (end - start) + start);

        public ParticleSystem(GameWorld world, float size)
            : base(PrimitiveType.Quads, new int[] { 3, 2, 2 })
        {
            particles = new List<Particle>();
            this.world = world;
            this.size = size;
            UpdateVertices();
        }

        public void Spawn(Vertex3f spawnAreaStart, Vertex3f spawnAreaEnd, int count, Block block)
        {
            Particle[] newParticles = new Particle[count];

            Random rand = new Random();
            for (int i = 0; i < count; i++)
                newParticles[i] = new Particle(
                    world, block,
                    new Vertex3f(
                        GetRangedRandom(rand, spawnAreaStart.x, spawnAreaEnd.x),
                        GetRangedRandom(rand, spawnAreaStart.y, spawnAreaEnd.y),
                        GetRangedRandom(rand, spawnAreaStart.z, spawnAreaEnd.z)
                    ),
                    new Vertex3f()
                );

            particles.AddRange(newParticles);
            UpdateVertices();
        }

        public void Update(double deltaTime)
        {
            foreach (Particle particle in particles)
                particle.Update(deltaTime);
            UpdateVertices();
        }
          
        private void UpdateVertices()
        {
            List<float> mesh = new List<float>();

            foreach (Particle particle in particles)
            {
                Vertex4f texCoords = Block.GetTextureCoords(particle.block.GetTexture(BlockSide.Top));
                Vertex3f pos = particle.Position;
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
