using Mycraft.Blocks;
using Mycraft.Graphics;
using Mycraft.Utils;
using Mycraft.World;
using OpenGL;

namespace Mycraft.WorldUI
{
    public class Selection : Box
    {
        public bool IsSelected { get; private set; }

        public BlockSide Side { get; private set; }
        public Vertex3i Position { get; private set; }

        private Matrix4x4f modelMatrix;

        public Selection()
            : base(
                  new Vertex3f(-.01f, -.01f, -.01f),
                  new Vertex3f(1.01f, 1.01f, 1.01f),
                  new Vertex3f(.1f, .1f, .1f)
              ) { }

        public void Select(Vertex3i position, BlockSide side)
        {
            IsSelected = true;
            Position = position;
            Side = side;
            modelMatrix = Matrix4x4f.Translated(position.x, position.y, position.z);
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
                base.Draw();
            }
        }
    }
}
