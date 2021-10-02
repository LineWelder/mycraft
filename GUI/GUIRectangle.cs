using OpenGL;
using Mycraft.Graphics;
using Mycraft.Utils;

namespace Mycraft.GUI
{
    public class GUIRectangle : VertexArray
    {
        public GUIRectangle(Vertex2i position, Vertex2i size)
            : base(PrimitiveType.Quads, Resources.GUIShader)
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
