using Mycraft.Blocks;

namespace Mycraft.World.Generation
{
    public class FlatWorldGenerator : IWorldGenerator
    {
        public void GenerateChunk(Chunk chunk)
        {
            for (int x = 0; x < Chunk.SIZE; x++)
                for (int z = 0; z < Chunk.SIZE; z++)
                {
                    for (int y = 0; y < Chunk.HEIGHT; y++)
                        if (y < 16)
                            chunk.blocks[x, y, z] = BlockRegistry.Stone;
                        else if (16 <= y && y < 19)
                            chunk.blocks[x, y, z] = BlockRegistry.Dirt;
                        else if (y == 19)
                            chunk.blocks[x, y, z] = BlockRegistry.Grass;
                        else
                            chunk.blocks[x, y, z] = BlockRegistry.Air;

                    chunk.groundLevel[x, z] = 19;
                }
        }
    }
}
