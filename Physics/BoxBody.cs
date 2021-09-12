using Mycraft.World;
using OpenGL;
using System;

namespace Mycraft.Physics
{
    public class BoxBody
    {
        public Vertex3f Position { get => position; set => position = value; }
        public Vertex3f Size { get; set; }
        public Vertex3f Velocity { get => velocity; set => velocity = value; }

        private GameWorld world;
        private Vertex3f position;
        private Vertex3f velocity;

        public BoxBody(GameWorld world, Vertex3f position, Vertex3f size)
        {
            this.world = world;
            this.position = position;
            Size = size;
        }

        public void Update()
        {
            position += velocity;
            velocity.y -= .002f;

            if (world.GetBlock((int)Math.Floor(position.x), (int)Math.Floor(position.y), (int)Math.Floor(position.z)) > Block.Void)
            {
                position.y = (float)Math.Ceiling(Position.y);
                velocity = new Vertex3f();
            }
        }
    }
}
