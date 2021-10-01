using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenGL;
using Mycraft.Utils;

namespace Mycraft.Physics
{
    public class AABB
    {
        // There was a bug caused by float perisision that the player could sometimes pass through the block above
        private const float PERSISION_FIX = .001f;

        public Vertex3f Size { get; set; }
        public Vertex3f Position => position;

        private Vertex3f position, lastPosition;
        private Vertex3f delta;

        public AABB(Vertex3f position, Vertex3f size)
        {
            this.position = position;
            Size = size;

            lastPosition = position;
            delta = new Vertex3f();
        }

        /// <summary>
        /// Teleport the box to the coords.
        /// </summary>
        /// <param name="pos">The coords</param>
        public void ForceMoveTo(Vertex3f pos)
        {
            lastPosition = pos;
            position = pos;
            delta = new Vertex3f();
        }

        /// <summary>
        /// Move the box ignoring collisions.
        /// </summary>
        /// <param name="d">The movement amount</param>
        public void ForceMove(Vertex3f d)
            => ForceMoveTo(position + d);

        /// <summary>
        /// Move the box.
        /// </summary>
        /// <param name="d">The movement amount</param>
        public void Move(Vertex3f d)
        {
            position += d;
            delta += d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool OverlapsX(AABB other)
            => FuncUtils.IsBetween(other.position.x - Size.x, position.x, other.position.x + other.Size.x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool OverlapsY(AABB other)
            => FuncUtils.IsBetween(other.position.y - Size.y, position.y, other.position.y + other.Size.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool OverlapsZ(AABB other)
            => FuncUtils.IsBetween(other.position.z - Size.z, position.z, other.position.z + other.Size.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HandleCollision(
            float delta,
            ref float thisPosition, float thisSize,
            float otherPosition, float otherSize,
            float lastPosition
        )
        {
            if (delta > 0
             && lastPosition + thisSize <= otherPosition + PERSISION_FIX
             && otherPosition - PERSISION_FIX < thisPosition + thisSize
               )
            {
                thisPosition = otherPosition - thisSize;
                return true;
            }
            else if (
                thisPosition < otherPosition + otherSize + PERSISION_FIX
                && otherPosition + otherSize - PERSISION_FIX <= lastPosition
               )
            {
                thisPosition = otherPosition + otherSize;
                return true;
            }

            return false;
        }

        protected bool CollideX(IEnumerable<AABB> others)
        {
            if (delta.x == 0)
                return false;

            position.x += delta.x;
            return others.Where((other) =>
                OverlapsY(other) && OverlapsZ(other)
             && HandleCollision(
                    delta.x,
                    ref position.x, Size.x,
                    other.position.x, other.Size.x,
                    lastPosition.x
                )).Any();
        }

        protected bool CollideY(IEnumerable<AABB> others)
        {
            if (delta.y == 0)
                return false;

            position.y += delta.y;
            return others.Where((other) =>
                OverlapsX(other) && OverlapsZ(other)
             && HandleCollision(
                    delta.y,
                    ref position.y, Size.y,
                    other.position.y, other.Size.y,
                    lastPosition.y
                )).Any();
        }

        protected bool CollideZ(IEnumerable<AABB> others)
        {
            if (delta.z == 0)
                return false;

            position.z += delta.z;
            return others.Where((other) =>
                OverlapsX(other) && OverlapsY(other)
             && HandleCollision(
                    delta.z,
                    ref position.z, Size.z,
                    other.position.z, other.Size.z,
                    lastPosition.z
                )).Any();
        }

        protected void Start()
        {
            position = lastPosition;
        }

        protected void End()
        {
            lastPosition = position;
            delta = new Vertex3f();
        }

        public void Collide(IEnumerable<AABB> others)
        {
            Start();

            CollideX(others);
            CollideY(others);
            CollideZ(others);

            End();
        }

        public bool Intersects(AABB other)
            => OverlapsX(other) && OverlapsY(other) && OverlapsZ(other);
    }
}
