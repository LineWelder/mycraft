using System;
using Mycraft.Blocks;
using Mycraft.Utils;

namespace Mycraft.World.Generation
{
    public class SimpleWorldGenerator : IWorldGenerator
    {
        private const int WATER_LEVEL = 18;

        private readonly FastNoiseLite noise;

        public SimpleWorldGenerator(int seed)
        {
            noise = new FastNoiseLite(seed);
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        }

        private float GetHeight(Chunk chunk, int x, int z)
        {
            float NoiseLayer(float scale, float amplitude)
                => noise.GetNoise(
                    (x + chunk.xOffset) * scale,
                    (z + chunk.zOffset) * scale
                ) * amplitude;

            return 19f + NoiseLayer(1f, 8f) + NoiseLayer(4f, 2f);
        }

        private void GenerateTree(Chunk chunk, int x, int y, int z)
        {
            void SetBlock(int bx, int by, int bz, Block block)
            {
                if (by < 0 || by >= Chunk.HEIGHT
                 || bx < 0 || bx >= Chunk.SIZE
                 || bz < 0 || bz >= Chunk.SIZE)
                    return;

                chunk.blocks[bx, by, bz] = block;
            }

            SetBlock(x, y, z, BlockRegistry.Dirt);

            for (int dy = 1; dy <= 5; dy++)
                SetBlock(x, y + dy, z, BlockRegistry.Log);

            for (int dx = -2; dx <= 2; dx++)
                for (int dz = -2; dz <= 2; dz++)
                    for (int dy = 4; dy <= 5; dy++)
                        if (dx != 0 || dz != 0)
                            SetBlock(x + dx, y + dy, z + dz, BlockRegistry.Leaves);

            for (int dx = -1; dx <= 1; dx++)
                for (int dz = -1; dz <= 1; dz++)
                    for (int dy = 6; dy <= 7; dy++)
                        SetBlock(x + dx, y + dy, z + dz, BlockRegistry.Leaves);
        }

        public void GenerateChunk(Chunk chunk)
        {
            for (int x = 0; x < Chunk.SIZE; x++)
                for (int z = 0; z < Chunk.SIZE; z++)
                {
                    float realHeight = GetHeight(chunk, x, z);
                    int height = (int)Math.Round(realHeight);

                    for (int y = 0; y < Chunk.HEIGHT; y++)
                        if (y < height - 3)
                            chunk.blocks[x, y, z] = BlockRegistry.Stone;
                        else if (height - 3 <= y && y < height)
                            chunk.blocks[x, y, z] = BlockRegistry.Dirt;
                        else if (y == height)
                            if (realHeight < WATER_LEVEL + .2f)
                                chunk.blocks[x, y, z] = BlockRegistry.Sand;
                            else
                                chunk.blocks[x, y, z] = BlockRegistry.Grass;
                        else if (y <= WATER_LEVEL)
                            chunk.blocks[x, y, z] = BlockRegistry.Water;
                        else
                            chunk.blocks[x, y, z] = BlockRegistry.Air;

                    chunk.groundLevel[x, z] = height;
                }

            for (int x = -2; x < Chunk.SIZE + 2; x++)
                for (int z = -2; z < Chunk.SIZE + 2; z++)
                {
                    float height = GetHeight(chunk, x, z);
                    int y = (int)Math.Round(height);
                    if (height < WATER_LEVEL + .2f)
                        continue;

                    int bx = chunk.xOffset + x;
                    int bz = chunk.zOffset + z;

                    if (bx % 8 == 0 && bz % 8 == 0)
                    {
                        GenerateTree(chunk, x, y, z);
                    }
                    else if (x >= 0 && x < Chunk.SIZE
                          && z >= 0 && z < Chunk.SIZE)
                    {
                        bx += 1;
                        bz += 1;

                        if (bx % 4 == 0 && bz % 4 == 0
                         && (bx / 4 + bz / 4) % 2 == 0)
                            chunk.blocks[x, y + 1, z] = BlockRegistry.RedFlower;
                        else if (bx % 4 == 0 && bz % 4 == 0)
                            chunk.blocks[x, y + 1, z] = BlockRegistry.YellowFlower;
                    }
                }
        }
    }
}
