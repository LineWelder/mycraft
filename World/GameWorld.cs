using OpenGL;
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

        public static Vertex3i GetNeighbour(Vertex3i coords, BlockSide side)
        {
            switch (side)
            {
                case BlockSide.Front:
                    return coords + new Vertex3i(0, 0, 1);
                case BlockSide.Back:
                    return coords + new Vertex3i(0, 0, -1);
                case BlockSide.Right:
                    return coords + new Vertex3i(1, 0, 0);
                case BlockSide.Left:
                    return coords + new Vertex3i(-1, 0, 0);
                case BlockSide.Top:
                    return coords + new Vertex3i(0, 1, 0);
                case BlockSide.Bottom:
                    return coords + new Vertex3i(0, -1, 0);
                default:
                    return coords;
            }
        }

        public Block GetBlock(int x, int y, int z)
        {
            var (chunkX, blockX) = ToChunkCoord(x);
            var (chunkZ, blockZ) = ToChunkCoord(z);

            if (y >= Chunk.HEIGHT || y < 0
             || !chunks.TryGetValue((chunkX, chunkZ), out Chunk chunk))
                return Block.Void;

            return chunk.blocks[blockX, y, blockZ];
        }

        public void SetBlock(int x, int y, int z, Block block)
        {
            var (chunkX, blockX) = ToChunkCoord(x);
            var (chunkZ, blockZ) = ToChunkCoord(z);

            if (y < Chunk.HEIGHT && y >= 0
             && chunks.TryGetValue((chunkX, chunkZ), out Chunk chunk))
                chunk.blocks[blockX, y, blockZ] = block;
        }

        public void GenerateSpawnArea()
        {
            for (int x = -SPAWN_AREA_RADIUS; x <= SPAWN_AREA_RADIUS; x++)
                for (int z = -SPAWN_AREA_RADIUS; z <= SPAWN_AREA_RADIUS; z++)
                    LoadChunk(x, z);

            RegenerateMesh();
        }

        public void RegenerateMesh()
        {
            foreach (var chunk in chunks.Values)
                chunk.RegenerateMesh();
        }

        public void LoadChunk(int x, int z)
        {
            Chunk newChunk = new Chunk(this, x, z);
            newChunk.Generate();
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
