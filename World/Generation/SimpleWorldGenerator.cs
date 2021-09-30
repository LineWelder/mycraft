using System;
using Mycraft.Blocks;
using Mycraft.Utils;

namespace Mycraft.World.Generation
{
    public class SimpleWorldGenerator : IWorldGenerator
    {
        private const int WATER_LEVEL = 18;

        private Chunk chunk;

        private void SetBlockExtended(int x, int y, int z, Block block)
        {
            if (y < 0 || y >= Chunk.HEIGHT)
                return;

            if (x >= 0 && x < Chunk.SIZE
             && z >= 0 && z < Chunk.SIZE)
                chunk.blocks[x, y, z] = block;
            else
                chunk.world.SetBlock(chunk.xOffset + x, y, chunk.zOffset + z, block);
        }

        private void GenerateTree(int x, int z)
        {
            int y = chunk.groundLevel[x, z];

            chunk.blocks[x, y, z] = BlockRegistry.Dirt;

            for (int dy = 1; dy <= 5; dy++)
                chunk.blocks[x, y + dy, z] = BlockRegistry.Log;

            for (int dx = -2; dx <= 2; dx++)
                for (int dz = -2; dz <= 2; dz++)
                    for (int dy = 4; dy <= 5; dy++)
                        if (dx != 0 || dz != 0)
                            SetBlockExtended(x + dx, y + dy, z + dz, BlockRegistry.Leaves);

            for (int dx = -1; dx <= 1; dx++)
                for (int dz = -1; dz <= 1; dz++)
                    for (int dy = 6; dy <= 7; dy++)
                        SetBlockExtended(x + dx, y + dy, z + dz, BlockRegistry.Leaves);
        }

        public void GenerateChunk(Chunk chunk)
        {
            this.chunk = chunk;

            FastNoiseLite noise = new FastNoiseLite();
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

            for (int x = 0; x < Chunk.SIZE; x++)
                for (int z = 0; z < Chunk.SIZE; z++)
                {
                    float NoiseLayer(float scale, float amplitude)
                        => noise.GetNoise(
                            (x + chunk.xOffset) * scale,
                            (z + chunk.zOffset) * scale
                        ) * amplitude;

                    int height = (int)Math.Round(
                        19f + NoiseLayer(1f, 8f) + NoiseLayer(4f, 2f)
                    );

                    for (int y = 0; y < Chunk.HEIGHT; y++)
                        if (y < height - 3)
                            chunk.blocks[x, y, z] = BlockRegistry.Stone;
                        else if (height - 3 <= y && y < height)
                            chunk.blocks[x, y, z] = BlockRegistry.Dirt;
                        else if (y == height)
                            if (height < WATER_LEVEL)
                                chunk.blocks[x, y, z] = BlockRegistry.Dirt;
                            else
                                chunk.blocks[x, y, z] = BlockRegistry.Grass;
                        else if (y <= WATER_LEVEL)
                            chunk.blocks[x, y, z] = BlockRegistry.Water;
                        else
                            chunk.blocks[x, y, z] = BlockRegistry.Air;

                    chunk.groundLevel[x, z] = height;
                }

            Random random = new Random(((short)chunk.zOffset << 16) + chunk.xOffset);
            for (int x = 0; x < Chunk.SIZE; x++)
                for (int z = 0; z < Chunk.SIZE; z++)
                    if (chunk.groundLevel[x, z] >= WATER_LEVEL && random.Next(100) < 1)
                        GenerateTree(x, z);
        }
    }
}
