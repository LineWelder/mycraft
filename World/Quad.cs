using OpenGL;

namespace Mycraft.World
{
    public struct Vertex
    {
        public Vertex3f position;
        public Vertex2f texture;
        public float textureId;
        public float light;
    }

    public struct Quad
    {
        public Vertex a, b, c, d;
        
        public Vertex3f Center => (a.position + b.position + c.position + d.position) / 4f;

        public Quad(
            Vertex3f pa, Vertex3f pb,
            Vertex3f pc, Vertex3f pd,
            int textureId, float light
        )
        {
            a = new Vertex
            {
                position = pa,
                texture = new Vertex2f(1f, 1f),
                textureId = textureId,
                light = light
            };

            b = new Vertex
            {
                position = pb,
                texture = new Vertex2f(1f, 0f),
                textureId = textureId,
                light = light
            };

            c = new Vertex
            {
                position = pc,
                texture = new Vertex2f(0f, 0f),
                textureId = textureId,
                light = light
            };

            d = new Vertex
            {
                position = pd,
                texture = new Vertex2f(0f, 1f),
                textureId = textureId,
                light = light
            };
        }
    }
}
