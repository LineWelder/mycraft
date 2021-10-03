using System;
using System.Collections.Generic;
using OpenGL;

using Mycraft.Blocks;
using Mycraft.World;

namespace Mycraft.Physics
{
    public class FallingBox : AABB
    {
        private const float GRAVITY = 15f;
        private const float MAX_FALL_SPEED = 50f;
        private const float WATER_GRAVITY = 1.5f;

        // If the box's speed is heigher than the max one
        private const float WATER_MAX_SPEED = 1.5f;
        private const float WATER_VELOCITY_FIX = 60f;

        public Vertex3f Velocity { get => velocity; set => velocity = value; }

        public bool IsGrounded { get; private set; }
        public bool IsInWater { get; private set; }

        protected GameWorld world;
        private Vertex3f velocity;

        public FallingBox(GameWorld world, Vertex3f position, Vertex3f size)
            : base(position, size)
        {
            this.world = world;
        }

        private Vertex3i ToBlockCoords(Vertex3f coords)
            => new Vertex3i(
                (int)Math.Floor(coords.x),
                (int)Math.Floor(coords.y),
                (int)Math.Floor(coords.z)
            );

        public void Update(double deltaTime)
        {
            Move(velocity * deltaTime);

            if (IsInWater)
            {
                float velocityModule = velocity.Module();
                Vertex3f velocityDirection = velocity.Normalized;
                float velocityFix = (float)(WATER_VELOCITY_FIX * deltaTime);

                if (velocityModule >= WATER_MAX_SPEED + velocityFix)
                    velocity -= velocityDirection * velocityFix;
                else if (velocityModule > WATER_MAX_SPEED)
                    velocity = velocityDirection * WATER_MAX_SPEED;
                else
                    velocity.y -= (float)(WATER_GRAVITY * deltaTime);
            }
            else
            {
                float velocityDelta = (float)(GRAVITY * deltaTime);
                if (velocity.y > -MAX_FALL_SPEED + velocityDelta)
                    velocity.y -= (float)(GRAVITY * deltaTime);
                else if (velocity.y > -MAX_FALL_SPEED)
                    velocity.y = -MAX_FALL_SPEED;

            }

            IsGrounded = false;
            IsInWater = false;

            Vertex3f boxEnd_ = Position + Size;
            Vertex3i boxStart = ToBlockCoords(Position);
            Vertex3i boxEnd = ToBlockCoords(boxEnd_);

            List<AABB> aabbs = new List<AABB>();
            for (int x = boxStart.x; x <= boxEnd.x; x++)
                for (int y = boxStart.y; y <= boxEnd.y; y++)
                    for (int z = boxStart.z; z <= boxEnd.z; z++)
                    {
                        Block block = world.GetBlock(x, y, z);
                        if (block.HasCollider)
                            aabbs.Add(new AABB(new Vertex3f(x, y, z), new Vertex3f(1f, 1f, 1f)));
                        else if (block is LiquidBlock)
                            IsInWater = true;
                    }

            Start();
            if (CollideX(aabbs)) velocity.x = 0f;
            if (CollideY(aabbs))
            {
                IsGrounded = velocity.y < 0;
                velocity = new Vertex3f();
            }
            if (CollideZ(aabbs)) velocity.z = 0f;
            End();
        }
    }
}
