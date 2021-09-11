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
