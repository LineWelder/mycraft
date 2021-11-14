using Mycraft.World;
using System.Collections.Generic;

namespace Mycraft.Blocks
{
    public class AirBlock : Block
    {
        public override bool IsTransparent => true;
        public override bool HasCollider => false;
        public override bool IsSelectable => false;

        public AirBlock()
            : base(0) { }

        public override void EmitMesh(MeshBuildingContext context, int x, int y, int z) { }
    }
}
