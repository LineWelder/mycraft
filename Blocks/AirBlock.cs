namespace Mycraft.Blocks
{
    public class AirBlock : Block
    {
        public override bool IsTransparent => true;
        public override bool HasCollider => false;
        public override bool IsVisible => false;

        public AirBlock()
            : base(0) { }
    }
}
