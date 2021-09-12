using Mycraft.World;
using OpenGL;
using System;

namespace Mycraft.Physics
{
    public class BoxBody
    {
        private const float GRAVITY = .005f;

        public Vertex3f Position { get => position; set => position = value; }
        public Vertex3f Size { get; set; }
        public Vertex3f Velocity { get => velocity; set => velocity = value; }

        public bool IsGrounded { get; private set; }

        private GameWorld world;
        private Vertex3f position;
        private Vertex3f velocity;

        public BoxBody(GameWorld world, Vertex3f position, Vertex3f size)
        {
            this.world = world;
            this.position = position;
            Size = size;
        }

        private Vertex3i ToBlockCoords(Vertex3f coords)
            => new Vertex3i(
                (int)Math.Floor(coords.x),
                (int)Math.Floor(coords.y),
                (int)Math.Floor(coords.z)
            );

        public void Update()
        {
            position += velocity;
            velocity.y -= GRAVITY;

            Vertex3i startPos = ToBlockCoords(position);
            Vertex3i endPos = ToBlockCoords(position + new Vertex3f(Size.x, 0f, Size.z));

            IsGrounded = false;
            for (int x = startPos.x; x <= endPos.x; x++)
                for (int z = startPos.z; z <= endPos.z; z++)
                    if (world.GetBlock(x, startPos.y, z) > Block.Void)
                    {
                        position.y = (float)Math.Ceiling(Position.y);
                        velocity = new Vertex3f();
                        IsGrounded = true;
                        break;
                    }
        }
    }
}
