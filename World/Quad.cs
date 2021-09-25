using OpenGL;

namespace Mycraft.World
{
    public struct Vertex
    {
        public Vertex3f position;
        public Vertex2f texture;
        public float light;
    }

    public struct Quad
    {
        public Vertex a, b, c, d;

        public Vertex3f Center => (a.position + b.position + c.position + d.position) / 4f;

        public Quad(
            Vertex3f pa, Vertex3f pb,
            Vertex3f pc, Vertex3f pd,
            Vertex4f textureCoords, float light
        )
        {
            a = new Vertex
            {
                position = pa,
                texture = new Vertex2f(textureCoords.z, textureCoords.w),
                light = light
            };

            b = new Vertex
            {
                position = pb,
                texture = new Vertex2f(textureCoords.z, textureCoords.y),
                light = light
            };

            c = new Vertex
            {
                position = pc,
                texture = new Vertex2f(textureCoords.x, textureCoords.y),
                light = light
            };

            d = new Vertex
            {
                position = pd,
                texture = new Vertex2f(textureCoords.x, textureCoords.w),
                light = light
            };
        }
    }
}
