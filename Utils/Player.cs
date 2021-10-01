using OpenGL;
using Mycraft.Physics;
using Mycraft.World;
using Mycraft.WorldUI;
using System;

namespace Mycraft.Utils
{
    public class Player : FallingBox, IDisposable
    {
        private const float EYE_HEIGHT = 1.5f;
        private static readonly Vertex3f SIZE = new Vertex3f(.75f, 1.7f, .75f);
        private static readonly Vertex3f BOX_OFFSET = new Vertex3f(-SIZE.x / 2f, 0f, -SIZE.z / 2f);

        public new Vertex3f Position => base.Position - BOX_OFFSET;
        public Selection Selection { get; set; }

        public readonly Camera camera;

        public Player(GameWorld world, Vertex3f position)
            : base(
                  world,
                  position + BOX_OFFSET,
                  SIZE
              )
        {
            Selection = new Selection();
            camera = new Camera(new Vertex3f(position.x, EYE_HEIGHT, position.y), new Vertex2f(0f, 0f));
        }

        public void RotateCamera(float dyaw, float dpitch)
        {
            const float HALF_PI = .5f * (float)Math.PI;

            Vertex2f rotation = camera.Rotation;
            rotation.x = FuncUtils.FixRotation(rotation.x + dyaw);
            rotation.y = FuncUtils.Clamp(-HALF_PI, rotation.y + dpitch, HALF_PI);

            camera.Rotation = rotation;
        }

        public void MoveRelativeToYaw(float forward, float right)
        {
            Move(camera.RelativeToYaw(
                forward,
                right
            ));
        }

        public new void Update(double deltaTime)
        {
            base.Update(deltaTime);

            camera.Position = base.Position + new Vertex3f(-BOX_OFFSET.x, EYE_HEIGHT, -BOX_OFFSET.z);
            camera.UpdateTransformMatrix();

            if (RayCasting.Raycast(world, camera.Position, camera.Forward, out Hit hit))
                Selection.Select(hit.blockCoords, hit.side);
            else
                Selection.Deselect();
        }

        public void Dispose()
        {
            Selection.Dispose();
        }
    }
}
