using OpenGL;
using System.Runtime.CompilerServices;

namespace Mycraft.World
{
    public static class QuadGenerator
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quad Bottom(Vertex3f start, Vertex2f size, int textureId, float light)
            => new Quad(
                new Vertex3f(start.x - size.x, start.y, start.z),
                new Vertex3f(start.x - size.x, start.y, start.z - size.y),
                new Vertex3f(start.x,          start.y, start.z - size.y),
                new Vertex3f(start.x,          start.y, start.z),
                textureId,
                light
            );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quad Top(Vertex3f start, Vertex2f size, int textureId, float light)
            => new Quad(
                new Vertex3f(start.x,          start.y, start.z),
                new Vertex3f(start.x,          start.y, start.z - size.y),
                new Vertex3f(start.x - size.x, start.y, start.z - size.y),
                new Vertex3f(start.x - size.x, start.y, start.z),
                textureId,
                light
            );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quad Front(Vertex3f start, Vertex2f size, int textureId, float light)
            => new Quad(
                new Vertex3f(start.x + size.x, start.y,          start.z),
                new Vertex3f(start.x + size.x, start.y + size.y, start.z),
                new Vertex3f(start.x,          start.y + size.y, start.z),
                new Vertex3f(start.x,          start.y,          start.z),
                textureId,
                light
            );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quad Back(Vertex3f start, Vertex2f size, int textureId, float light)
            => new Quad(
                new Vertex3f(start.x - size.x,          start.y,          start.z),
                new Vertex3f(start.x - size.x,          start.y + size.y, start.z),
                new Vertex3f(start.x, start.y + size.y, start.z),
                new Vertex3f(start.x, start.y,          start.z),
                textureId,
                light
            );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quad Right(Vertex3f start, Vertex2f size, int textureId, float light)
            => new Quad(
                new Vertex3f(start.x, start.y,          start.z - size.x),
                new Vertex3f(start.x, start.y + size.y, start.z - size.x),
                new Vertex3f(start.x, start.y + size.y, start.z),
                new Vertex3f(start.x, start.y,          start.z),
                textureId,
                light
            );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quad Left(Vertex3f start, Vertex2f size, int textureId, float light)
            => new Quad(
                new Vertex3f(start.x, start.y,          start.z),
                new Vertex3f(start.x, start.y + size.y, start.z),
                new Vertex3f(start.x, start.y + size.y, start.z - size.x),
                new Vertex3f(start.x, start.y,          start.z - size.x),
                textureId,
                light
            );
    }
}
