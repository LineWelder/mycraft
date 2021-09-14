using Mycraft.Blocks;
using Mycraft.World;
using OpenGL;
using System;

namespace Mycraft.Utils
{
    public struct Hit
    {
        public readonly Vertex3f point;
        public readonly BlockSide side;
        public readonly Vertex3i blockCoords;
        public readonly Block block;

        public Hit(Vertex3f point, BlockSide side, Vertex3i blockCoords, Block block)
        {
            this.point = point;
            this.side = side;
            this.blockCoords = blockCoords;
            this.block = block;
        }
    }

    public static class RayCasting
    {
        public static bool Raycast(GameWorld world, Vertex3f origin, Vertex3f direction, out Hit ray, float maxDistance = 6f)
        {
            // If the ray is parallel to a plane, it will never go through the faces parallel to it
            bool doCheckXY = direction.z != 0;
            bool doCheckXZ = direction.y != 0;
            bool doCheckYZ = direction.x != 0;

            if (!(doCheckXY || doCheckXZ || doCheckYZ))
                throw new InvalidOperationException("Direction vector cannot be zero");

            // Setup the face intersection checkers and move them to the first face on their way
            Vertex3f checkXYStep = new Vertex3f();
            Vertex3f checkXZStep = new Vertex3f();
            Vertex3f checkYZStep = new Vertex3f();
            Vertex3f checkXYCurrent = origin;
            Vertex3f checkXZCurrent = origin;
            Vertex3f checkYZCurrent = origin;

            if (doCheckXY)
            {
                float z = direction.z > 0
                    ? (float)Math.Ceiling(origin.z)
                    : (float)Math.Floor(origin.z);
                float dz = Math.Abs(z - origin.z);

                checkXYStep = new Vertex3f(
                    direction.x / direction.z,
                    direction.y / direction.z,
                    1f
                ) * Math.Sign(direction.z);

                checkXYCurrent = new Vertex3f(
                    origin.x + dz * checkXYStep.x,
                    origin.y + dz * checkXYStep.y,
                    z
                );
            }

            if (doCheckXZ)
            {
                float y = direction.y > 0
                    ? (float)Math.Ceiling(origin.y)
                    : (float)Math.Floor(origin.y);
                float dy = Math.Abs(y - origin.y);

                checkXZStep = new Vertex3f(
                    direction.x / direction.y,
                    1f,
                    direction.z / direction.y
                ) * Math.Sign(direction.y);

                checkXZCurrent = new Vertex3f(
                    origin.x + dy * checkXZStep.x,
                    y,
                    origin.z + dy * checkXZStep.z
                );
            }

            if (doCheckYZ)
            {
                float x = direction.x > 0
                    ? (float)Math.Ceiling(origin.x)
                    : (float)Math.Floor(origin.x);
                float dx = Math.Abs(x - origin.x);

                checkYZStep = new Vertex3f(
                    1f,
                    direction.y / direction.x,
                    direction.z / direction.x
                ) * Math.Sign(direction.x);

                checkYZCurrent = new Vertex3f(
                    x,
                    origin.y + dx * checkYZStep.y,
                    origin.z + dx * checkYZStep.z
                );
            }

            // Since we are using squared distances
            float maxDistanceSquared = maxDistance * maxDistance;

            while (true)
            {
                float distanceToXY = (checkXYCurrent - origin).ModuleSquared();
                float distanceToXZ = (checkXZCurrent - origin).ModuleSquared();
                float distanceToYZ = (checkYZCurrent - origin).ModuleSquared();

                Vertex3f currentPoint;
                BlockSide hitSide;
                Vertex3i hitCoords;

                // The next face is in the XY plane
                if (doCheckXY
                    && (!doCheckXZ || distanceToXY < distanceToXZ)
                    && (!doCheckYZ || distanceToXY < distanceToYZ))
                {
                    if (distanceToXY > maxDistanceSquared)
                        break;

                    currentPoint = checkXYCurrent;
                    hitCoords = new Vertex3i(
                        (int)Math.Floor(checkXYCurrent.x),
                        (int)Math.Floor(checkXYCurrent.y),
                        (int)checkXYCurrent.z - (checkXYStep.z < 0 ? 1 : 0)
                    );
                    hitSide = checkXZStep.z < 0 ? BlockSide.Front : BlockSide.Back;

                    checkXYCurrent += checkXYStep;
                }

                // The next face is in the XZ plane
                else if (doCheckXZ
                    && (!doCheckXY || distanceToXZ < distanceToXY)
                    && (!doCheckYZ || distanceToXZ < distanceToYZ))
                {
                    if (distanceToXZ > maxDistanceSquared)
                        break;

                    currentPoint = checkXZCurrent;
                    hitCoords = new Vertex3i(
                        (int)Math.Floor(checkXZCurrent.x),
                        (int)checkXZCurrent.y - (checkXZStep.y < 0 ? 1 : 0),
                        (int)Math.Floor(checkXZCurrent.z)
                    );
                    hitSide = checkXZStep.y < 0 ? BlockSide.Top : BlockSide.Bottom;

                    checkXZCurrent += checkXZStep;
                }

                // The next face is in the YZ plane
                else
                {
                    if (distanceToYZ > maxDistanceSquared)
                        break;

                    currentPoint = checkYZCurrent;
                    hitCoords = new Vertex3i(
                        (int)checkYZCurrent.x - (checkXYStep.x < 0 ? 1 : 0),
                        (int)Math.Floor(checkYZCurrent.y),
                        (int)Math.Floor(checkYZCurrent.z)
                    );
                    hitSide = checkYZStep.x < 0 ? BlockSide.Right : BlockSide.Left;

                    checkYZCurrent += checkYZStep;
                }

                Block hitBlock = world.GetBlock(hitCoords.x, hitCoords.y, hitCoords.z);
                if (hitBlock.HasCollider)
                {
                    ray = new Hit(currentPoint, hitSide, hitCoords, hitBlock);
                    return true;
                }
            }

            Vertex3f endPoint = origin + direction * maxDistance;
            Vertex3i endCoords = new Vertex3i(
                (int)Math.Floor(endPoint.x),
                (int)Math.Floor(endPoint.y),
                (int)Math.Floor(endPoint.z)
            );
            Block endBlock = world.GetBlock(endCoords.x, endCoords.y, endCoords.z);

            ray = new Hit(endPoint, BlockSide.Front, endCoords, endBlock);
            return false;
        }
    }
}
