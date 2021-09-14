namespace Mycraft.Blocks
{
    public class MultiTexturedBlock : Block
    {
        private int topTexture;
        private int sideTexture;
        private int bottomTexture;

        public MultiTexturedBlock(int topTexture, int sideTexture, int bottomTexture)
            : base(sideTexture)
        {
            this.topTexture = topTexture;
            this.sideTexture = sideTexture;
            this.bottomTexture = bottomTexture;
        }

        public override int GetTexture(BlockSide side)
        {
            switch (side)
            {
                case BlockSide.Top:
                    return topTexture;
                case BlockSide.Bottom:
                    return bottomTexture;
                default:
                    return sideTexture;
            }
        }
    }
}