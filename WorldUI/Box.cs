using Mycraft.Graphics;
using Mycraft.Utils;
using OpenGL;

namespace Mycraft.WorldUI
{
    public class Box : VertexArray
    {
        public Vertex3f Color { get; set; }

        public Box(Vertex3f start, Vertex3f end, Vertex3f color)
            : base(PrimitiveType.Lines, new int[] { 3 })
        {
            Color = color;
            Data = new float[]
            {
                // X aligned
                start.x, start.y, start.z,
                end.x,   start.y, start.z,
                start.x, start.y, end.z,
                end.x,   start.y, end.z,
                start.x, end.y,   start.z,
                end.x,   end.y,   start.z,
                start.x, end.y,   end.z,
                end.x,   end.y,   end.z,

                // Y aligned
                start.x, start.y, start.z,
                start.x, end.y,   start.z,
                start.x, start.y, end.z,
                start.x, end.y,   end.z,
                end.x,   start.y, start.z,
                end.x,   end.y,   start.z,
                end.x,   start.y, end.z,
                end.x,   end.y,   end.z,

                // Z aligned
                start.x, start.y, start.z,
                start.x, start.y, end.z,
                start.x, end.y,   start.z,
                start.x, end.y,   end.z,
                end.x,   start.y, start.z,
                end.x,   start.y, end.z,
                end.x,   end.y,   start.z,
                end.x,   end.y,   end.z
            };
        }

        public new void Draw()
        {
            Resources.WorldUIShader.Color = Color;
            base.Draw();
        }
    }
}
