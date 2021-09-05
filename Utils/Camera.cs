using OpenGL;
using System;

namespace Mycraft.Utils
{
    public class Camera
    {
        private const float RADIANS_TO_DEGREES = (float)(180d / Math.PI);

        public Matrix4x4f TransformMatrix { get; private set; }

        public Vertex3f Position;
        public Vertex2f Rotation;

        public Camera(Vertex3f position, Vertex2f rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public void Translate(float dx, float dy, float dz)
        {
            Position.x += dx;
            Position.y += dy;
            Position.z += dz;
        }

        public void Rotate(float yawDelta, float pitchDelta)
        {
            Rotation.x += yawDelta;
            Rotation.y += pitchDelta;
        }

        public void MoveRelativeToYaw(float forward, float right)
        {
            Position.z -= (float)Math.Cos(Rotation.x) * forward
                        - (float)Math.Sin(Rotation.x) * right;
            Position.x += (float)Math.Cos(Rotation.x) * right
                        + (float)Math.Sin(Rotation.x) * forward;
        }

        public void Update()
        {
            TransformMatrix = Matrix4x4f.RotatedX(-Rotation.y * RADIANS_TO_DEGREES)
                            * Matrix4x4f.RotatedY(Rotation.x * RADIANS_TO_DEGREES)
                            * Matrix4x4f.Translated(-Position.x, -Position.y, -Position.z);
        }
    }
}
