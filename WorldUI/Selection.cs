using Mycraft.Graphics;
using Mycraft.Utils;
using OpenGL;

namespace Mycraft.WorldUI
{
    public class Selection : VertexArray
    {
        private static readonly float[] vertices =
        {
            // X aligned
            0f, 0f, 0f,  0f, 0f, 0f,
            1f, 0f, 0f,  0f, 0f, 0f,
            0f, 0f, 1f,  0f, 0f, 0f,
            1f, 0f, 1f,  0f, 0f, 0f,
            0f, 1f, 0f,  0f, 0f, 0f,
            1f, 1f, 0f,  0f, 0f, 0f,
            0f, 1f, 1f,  0f, 0f, 0f,
            1f, 1f, 1f,  0f, 0f, 0f,

            // Y aligned
            0f, 0f, 0f,  0f, 0f, 0f,
            0f, 1f, 0f,  0f, 0f, 0f,
            0f, 0f, 1f,  0f, 0f, 0f,
            0f, 1f, 1f,  0f, 0f, 0f,
            1f, 0f, 0f,  0f, 0f, 0f,
            1f, 1f, 0f,  0f, 0f, 0f,
            1f, 0f, 1f,  0f, 0f, 0f,
            1f, 1f, 1f,  0f, 0f, 0f,

            // Z aligned
            0f, 0f, 0f,  0f, 0f, 0f,
            0f, 0f, 1f,  0f, 0f, 0f,
            0f, 1f, 0f,  0f, 0f, 0f,
            0f, 1f, 1f,  0f, 0f, 0f,
            1f, 0f, 0f,  0f, 0f, 0f,
            1f, 0f, 1f,  0f, 0f, 0f,
            1f, 1f, 0f,  0f, 0f, 0f,
            1f, 1f, 1f,  0f, 0f, 0f
        };

        public bool IsSelected { get; set; }
        public Vertex3i Selected
        {
            get => position;
            set
            {
                IsSelected = true;
                position = value;
                modelMatrix = Matrix4x4f.Translated(value.x + .5f, value.y + .5f, value.z + .5f)
                            * Matrix4x4f.Scaled(1.01f, 1.01f, 1.01f)
                            * Matrix4x4f.Translated(-.5f, -.5f, -.5f);
            }
        }

        private Vertex3i position;
        private Matrix4x4f modelMatrix;

        public Selection()
            : base(PrimitiveType.Lines, new int[] { 3, 3 }, vertices) { }

        public new void Draw()
        {
            if (IsSelected)
            {
                Resources.WorldUIShader.Model = modelMatrix;
                base.Draw();
            }
        }
    }
}
