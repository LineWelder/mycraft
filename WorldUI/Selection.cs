using Mycraft.Graphics;
using Mycraft.Utils;
using Mycraft.World;
using OpenGL;

namespace Mycraft.WorldUI
{
    public class Selection : VertexArray
    {
        private static readonly float[] vertices =
        {
            // X aligned
            0f, 0f, 0f,
            1f, 0f, 0f,
            0f, 0f, 1f,
            1f, 0f, 1f,
            0f, 1f, 0f,
            1f, 1f, 0f,
            0f, 1f, 1f,
            1f, 1f, 1f,

            // Y aligned
            0f, 0f, 0f,
            0f, 1f, 0f,
            0f, 0f, 1f,
            0f, 1f, 1f,
            1f, 0f, 0f,
            1f, 1f, 0f,
            1f, 0f, 1f,
            1f, 1f, 1f,

            // Z aligned
            0f, 0f, 0f,
            0f, 0f, 1f,
            0f, 1f, 0f,
            0f, 1f, 1f,
            1f, 0f, 0f,
            1f, 0f, 1f,
            1f, 1f, 0f,
            1f, 1f, 1f
        };

        public bool IsSelected { get; private set; }

        public BlockSide Side { get; private set; }
        public Vertex3i Position { get; private set; }

        private Matrix4x4f modelMatrix;

        public Selection()
            : base(PrimitiveType.Lines, new int[] { 3 }, vertices) { }

        public void Select(Vertex3i position, BlockSide side)
        {
            IsSelected = true;
            Position = position;
            Side = side;
            modelMatrix = Matrix4x4f.Translated(position.x + .5f, position.y + .5f, position.z + .5f)
                        * Matrix4x4f.Scaled(1.01f, 1.01f, 1.01f)
                        * Matrix4x4f.Translated(-.5f, -.5f, -.5f);
        }

        public void Deselect()
        {
            IsSelected = false;
        }

        public new void Draw()
        {
            if (IsSelected)
            {
                Resources.WorldUIShader.Model = modelMatrix;
                Resources.WorldUIShader.Color = new Vertex3f(.1f, .1f, .1f);
                base.Draw();
            }
        }
    }
}
