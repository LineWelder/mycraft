using Mycraft.Blocks;
using System;
using System.Collections.Generic;

namespace Mycraft.World
{
    public class GameWorld : IDisposable
    {
        private const int SPAWN_AREA_RADIUS = 1;

        private readonly Dictionary<(int, int), Chunk> chunks;

        public GameWorld()
        {
            chunks = new Dictionary<(int, int), Chunk>();
        }

        private (int chunk, int block) ToChunkCoord(int v)
            => v >= 0
            ? (v / Chunk.SIZE, v % Chunk.SIZE)
            : ((v + 1) / Chunk.SIZE - 1, (v + 1) % Chunk.SIZE + Chunk.SIZE - 1);

        public Block GetBlock(int x, int y, int z)
        {
            var (chunkX, blockX) = ToChunkCoord(x);
            var (chunkZ, blockZ) = ToChunkCoord(z);

            if (y >= Chunk.HEIGHT || y < 0
             || !chunks.TryGetValue((chunkX, chunkZ), out Chunk chunk))
                return BlockRegistry.Void;

            return chunk.blocks[blockX, y, blockZ];
        }

        public void SetBlock(int x, int y, int z, Block block)
        {
            var (chunkX, blockX) = ToChunkCoord(x);
            var (chunkZ, blockZ) = ToChunkCoord(z);

            if (y < Chunk.HEIGHT && y >= 0
             && chunks.TryGetValue((chunkX, chunkZ), out Chunk chunk))
            {
                chunk.blocks[blockX, y, blockZ] = block;
                chunk.needsUpdate = true;

                Chunk neighbour;
                if (blockX == 0 && chunks.TryGetValue((chunkX - 1, chunkZ), out neighbour))
                    neighbour.needsUpdate = true;
                else if (blockX == Chunk.SIZE - 1 && chunks.TryGetValue((chunkX + 1, chunkZ), out neighbour))
                    neighbour.needsUpdate = true;

                if (blockZ == 0 && chunks.TryGetValue((chunkX, chunkZ - 1), out neighbour))
                    neighbour.needsUpdate = true;
                else if (blockZ == Chunk.SIZE - 1 && chunks.TryGetValue((chunkX, chunkZ + 1), out neighbour))
                    neighbour.needsUpdate = true;
            }
        }

        public void GenerateSpawnArea()
        {
            for (int x = -SPAWN_AREA_RADIUS; x <= SPAWN_AREA_RADIUS; x++)
                for (int z = -SPAWN_AREA_RADIUS; z <= SPAWN_AREA_RADIUS; z++)
                    LoadChunk(x, z);
        }

        public void Update()
        {
            foreach (var chunk in chunks.Values)
                chunk.UpToDateMesh();
        }

        public void LoadChunk(int x, int z)
        {
            Chunk newChunk = new Chunk(this, x, z);
            newChunk.Generate();
            newChunk.needsUpdate = true;

            Chunk chunk;
            if (chunks.TryGetValue((x - 1, z), out chunk)) chunk.needsUpdate = true;
            if (chunks.TryGetValue((x + 1, z), out chunk)) chunk.needsUpdate = true;
            if (chunks.TryGetValue((x, z - 1), out chunk)) chunk.needsUpdate = true;
            if (chunks.TryGetValue((x, z + 1), out chunk)) chunk.needsUpdate = true;

            chunks.Add((x, z), newChunk);
        }

        public void Draw()
        {
            foreach (var chunk in chunks.Values)
                chunk.Draw();
        }

        public void Dispose()
        {
            foreach (var chunk in chunks.Values)
                chunk.Dispose();
        }
    }
}
