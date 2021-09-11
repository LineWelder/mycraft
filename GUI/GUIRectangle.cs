using Mycraft.Graphics;
using OpenGL;

namespace Mycraft.GUI
{
    public class GUIRectangle : VertexArray
    {
        public GUIRectangle(Vertex2i position, Vertex2i size)
            : base(PrimitiveType.Quads, new int[] { 2, 2 })
        {
            Data = new float[]
            {
                position.x + size.x, position.y + size.y,  1f, 1f,
                position.x + size.x, position.y,           1f, 0f,
                position.x,          position.y,           0f, 0f,
                position.x,          position.y + size.y,  0f, 1f
            };
        }
    }
}
