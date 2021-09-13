using OpenGL;

namespace Mycraft.Physics
{
    public class AABB
    {
        public Vertex3f Size { get; set; }
        public Vertex3f Position
        {
            get => position;
            set
            {
                lastPosition = value;
                position = value;
                delta = new Vertex3f();
            }
        }

        private Vertex3f position, lastPosition;
        private Vertex3f delta;

        public AABB(Vertex3f position, Vertex3f size)
        {
            Position = position;
            Size = size;

            lastPosition = position;
            delta = new Vertex3f();
        }

        public void Move(Vertex3f d)
        {
            position += d;
            delta += d;
        }

        public void Collide(AABB other)
        {
            bool alignedInX = other.position.x - Size.x < position.x && position.x < other.position.x + other.Size.x;
            bool alignedInY = other.position.y - Size.y < position.y && position.y < other.position.y + other.Size.y;
            bool alignedInZ = other.position.z - Size.z < position.z && position.z < other.position.z + other.Size.z;


             if (delta.x != 0 && alignedInY && alignedInZ)
                if (delta.x > 0
                    && lastPosition.x + Size.x <= other.position.x
                    && other.position.x < position.x
                   )
                    position.x = other.position.x - Size.x;
                else if (
                    position.x < other.position.x + other.Size.x
                    && other.position.x + other.Size.x <= lastPosition.x
                   )
                    position.x = other.position.x + other.Size.x;

            if (delta.y != 0 && alignedInX && alignedInZ)
                if (delta.y > 0
                    && lastPosition.y + Size.y <= other.position.y
                    && other.position.y < position.y
                   )
                    position.y = other.position.y - Size.y;
                else if (
                    position.y < other.position.y + other.Size.y
                    && other.position.y + other.Size.y <= lastPosition.y
                   )
                    position.y = other.position.y + other.Size.y;

            if (delta.z != 0 && alignedInX && alignedInY)
                if (delta.z > 0
                    && lastPosition.z + Size.z <= other.position.z
                    && other.position.z < position.z
                   )
                    position.z = other.position.z - Size.z;
                else if (
                    position.z < other.position.z + other.Size.z
                    && other.position.z + other.Size.z <= lastPosition.z
                   )
                    position.z = other.position.z + other.Size.z;
        }

        public void End()
        {
            lastPosition = position;
            delta = new Vertex3f();
        }
    }
}
