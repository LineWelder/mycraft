using System;
using System.Collections.Generic;
using OpenGL;

using Mycraft.Blocks;
using Mycraft.Graphics;
using Mycraft.World;
using Mycraft.Utils;

namespace Mycraft.Physics
{
    public class ParticleSystem : VertexArray
    {
        private class Particle : FallingBox
        {
            public Block block;
            public double lifeLeft;

            public Particle(GameWorld world, Block block, double lifeSpan, Vertex3f position, Vertex3f velocity)
                : base(world, position, new Vertex3f())
            {
                this.block = block;
                lifeLeft = lifeSpan;
                Velocity = velocity;
            }
        }

        private readonly List<Particle> particles;
        private readonly GameWorld world;
        private readonly float size;
        private readonly double lifeSpan;

        public float GetRangedRandom(Random rand, float start, float end)
            => (float)(rand.NextDouble() * (end - start) + start);

        public ParticleSystem(GameWorld world, float size, double lifeSpan)
            : base(PrimitiveType.Quads, Resources.ParticleShader)
        {
            particles = new List<Particle>();
            this.world = world;
            this.size = size;
            this.lifeSpan = lifeSpan;
            UpdateVertices();
        }

        public void Spawn(Vertex3f spawnAreaStart, Vertex3f spawnAreaEnd, int count, Block block)
        {
            Particle[] newParticles = new Particle[count];
            Vertex3f center = (spawnAreaStart + spawnAreaEnd) / 2f;

            Random rand = new Random();
            for (int i = 0; i < count; i++)
            {
                Vertex3f position = new Vertex3f(
                    GetRangedRandom(rand, spawnAreaStart.x, spawnAreaEnd.x),
                    GetRangedRandom(rand, spawnAreaStart.y, spawnAreaEnd.y),
                    GetRangedRandom(rand, spawnAreaStart.z, spawnAreaEnd.z)
                );

                newParticles[i] = new Particle(
                    world, block,
                    lifeSpan,
                    position,
                    (position - center) * 4f
                );
            }

            particles.AddRange(newParticles);
            UpdateVertices();
        }

        public void Update(double deltaTime)
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                Particle particle = particles[i];

                particle.lifeLeft -= deltaTime;
                if (particle.lifeLeft <= 0)
                    particles.RemoveAt(i);

                particle.Update(deltaTime);
            }

            UpdateVertices();
        }
          
        private void UpdateVertices()
        {
            List<float> mesh = new List<float>();

            foreach (Particle particle in particles)
            {
                int textureId = particle.block.GetTexture(BlockSide.Top);
                Vertex3f pos = particle.Position;
                float offset = size / 2f;

                mesh.AddRange(new float[] {
                    pos.x, pos.y, pos.z,   offset, -offset,  1f, 1f,
                    pos.x, pos.y, pos.z,   offset,  offset,  1f, 0f,
                    pos.x, pos.y, pos.z,  -offset,  offset,  0f, 0f,
                    pos.x, pos.y, pos.z,  -offset, -offset,  0f, 1f
                });
            }

            Data = mesh.ToArray();
        }
    }
}
