namespace Mycraft.Blocks
{
    public static class BlockRegistry
    {
        public static readonly Block Void = new AirBlock();
        public static readonly Block Air = new AirBlock();
        public static readonly Block Stone = new Block(0);
        public static readonly Block Dirt = new Block(3);
        public static readonly Block Grass = new MultiTexturedBlock(1, 2, 3);
    }
}
