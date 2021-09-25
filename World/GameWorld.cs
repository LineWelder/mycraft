﻿using Mycraft.Blocks;
using Mycraft.World.Generation;
using System;
using System.Collections.Generic;

namespace Mycraft.World
{
    public class GameWorld : IDisposable
    {
        private struct BlockToBeSet
        {
            public int x, y, z;
            public Block block;
        }

        public const int LOAD_DISTANCE = 7;
        public const int UNLOAD_DISTANCE = 9;

        private readonly Dictionary<(int x, int z), Chunk> chunks;
        private readonly List<(int distance, Chunk chunk)> renderQueue;

        private readonly Dictionary<(int chunkX, int chunkZ), List<BlockToBeSet>> toBeSet;
        private readonly IWorldGenerator generator;

        private int lastPlayerChunkX, lastPlayerChunkZ;

        public GameWorld(IWorldGenerator generator)
        {
            chunks = new Dictionary<(int x, int z), Chunk>();
            renderQueue = new List<(int distance, Chunk chunk)>();

            toBeSet = new Dictionary<(int chunkX, int chunkZ), List<BlockToBeSet>>();
            this.generator = generator;
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

            if (y >= Chunk.HEIGHT && y < 0)
                return;

            if (chunks.TryGetValue((chunkX, chunkZ), out Chunk chunk))
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
            else
            {
                if (!toBeSet.ContainsKey( (chunkX, chunkZ) ))
                    toBeSet[(chunkX, chunkZ)] = new List<BlockToBeSet>();

                toBeSet[(chunkX, chunkZ)].Add(new BlockToBeSet
                {
                    x = blockX,
                    y = y,
                    z = blockZ,
                    block = block
                });
            }
        }

        public int GetGroundLevel(int x, int z)
        {
            var (chunkX, blockX) = ToChunkCoord(x);
            var (chunkZ, blockZ) = ToChunkCoord(z);

            if (!chunks.TryGetValue((chunkX, chunkZ), out Chunk chunk))
                return -1;

            return chunk.groundLevel[blockX, blockZ];
        }

        public void GenerateSpawnArea()
        {
            for (int x = -LOAD_DISTANCE; x <= LOAD_DISTANCE; x++)
                for (int z = -LOAD_DISTANCE; z <= LOAD_DISTANCE; z++)
                    LoadChunk(x, z);
        }

        public void Update(int playerX, int playerZ, bool firstUpdate = false)
        {
            int playerChunkX = ToChunkCoord(playerX).chunk;
            int playerChunkZ = ToChunkCoord(playerZ).chunk;

            if (firstUpdate || lastPlayerChunkX != playerChunkX || lastPlayerChunkZ != playerChunkZ)
            {
                lastPlayerChunkX = playerChunkX;
                lastPlayerChunkZ = playerChunkZ;

                for (int x = playerChunkX - LOAD_DISTANCE; x <= playerChunkX + LOAD_DISTANCE; x++)
                    for (int z = playerChunkZ - LOAD_DISTANCE; z <= playerChunkZ + LOAD_DISTANCE; z++)
                        LoadChunk(x, z);

                List<(int x, int z)> chunksToUnload = new List<(int x, int z)>();
                foreach (var coords in chunks.Keys)
                    if (Math.Abs(coords.x - playerChunkX) > UNLOAD_DISTANCE
                     || Math.Abs(coords.z - playerChunkZ) > UNLOAD_DISTANCE)
                        chunksToUnload.Add(coords);

                foreach (var coords in chunksToUnload)
                    UnloadChunk(coords.x, coords.z);

                renderQueue.Clear();
                foreach (var pair in chunks)
                {
                    int dx = pair.Key.x - playerChunkX;
                    int dz = pair.Key.z - playerChunkZ;
                    int distance = dx * dx + dz * dz;

                    renderQueue.Add((distance, pair.Value));
                }

                renderQueue.Sort(
                    ((int distance, Chunk chunk) a, (int distance, Chunk chunk) b)
                        => b.distance.CompareTo(a.distance)
                );
            }

            foreach (Chunk chunk in chunks.Values)
                chunk.UpToDateMesh();
        }

        private void OnChunkUpdate(int x, int z)
        {
            Chunk chunk;
            if (chunks.TryGetValue((x - 1, z), out chunk)) chunk.needsUpdate = true;
            if (chunks.TryGetValue((x + 1, z), out chunk)) chunk.needsUpdate = true;
            if (chunks.TryGetValue((x, z - 1), out chunk)) chunk.needsUpdate = true;
            if (chunks.TryGetValue((x, z + 1), out chunk)) chunk.needsUpdate = true;
        }

        private void LoadChunk(int x, int z)
        {
            if (chunks.ContainsKey((x, z)))
                return;

            Chunk newChunk = new Chunk(this, x, z);
            chunks.Add((x, z), newChunk);

            generator.GenerateChunk(newChunk);
            if (toBeSet.TryGetValue((x, z), out List<BlockToBeSet> blocks))
                foreach (BlockToBeSet block in blocks)
                    newChunk.blocks[block.x, block.y, block.z] = block.block;

            newChunk.needsUpdate = true;
            OnChunkUpdate(x, z);
        }

        private void UnloadChunk(int x, int z)
        {
            chunks.Remove((x, z));
            OnChunkUpdate(x, z);
        }

        public void Draw()
        {
            foreach (var pair in renderQueue)
                pair.chunk.Draw();
        }

        public void Dispose()
        {
            foreach (Chunk chunk in chunks.Values)
                chunk.Dispose();
        }
    }
}
