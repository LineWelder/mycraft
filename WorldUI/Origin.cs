using Mycraft.Graphics;
using Mycraft.Utils;
using OpenGL;

namespace Mycraft.WorldUI
{
    public class Origin : VertexArray
    {
        private static readonly float[] vertices =
        {
            // X axis
            0f, 0f, 0f,  1f, 0f, 0f,
            1f, 0f, 0f,  1f, 0f, 0f,

            // Y axis
            0f, 0f, 0f,  0f, 1f, 0f,
            0f, 1f, 0f,  0f, 1f, 0f,

            // Z axis
            0f, 0f, 0f,  0f, 0f, 1f,
            0f, 0f, 1f,  0f, 0f, 1f
        };

        public Origin()
            : base(PrimitiveType.Lines, new int[] { 3, 3 }, vertices) { }

        public new void Draw()
        {
            Resources.WorldUIShader.Model = Matrix4x4f.Identity;
            base.Draw();
        }
    }
}
