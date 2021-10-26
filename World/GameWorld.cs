using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenGL;

using Mycraft.Blocks;
using Mycraft.World.Generation;
using Mycraft.Utils;

namespace Mycraft.World
{
    public class GameWorld : IDisposable
    {
        public const int LOAD_DISTANCE = 7;
        public const int UNLOAD_DISTANCE = 9;

        private readonly Dictionary<(int x, int z), Chunk> chunks;
        private readonly List<(int distance, Chunk chunk)> renderQueue;

        private readonly IWorldGenerator generator;

        private int lastCameraChunkX, lastCameraChunkZ;
        private bool renderQueueNeedsUpdate;

        public GameWorld(IWorldGenerator generator)
        {
            chunks = new Dictionary<(int x, int z), Chunk>();
            renderQueue = new List<(int distance, Chunk chunk)>();

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
             || !chunks.TryGetValue((chunkX, chunkZ), out Chunk chunk)
             || !chunk.isLoaded)
                return BlockRegistry.Void;

            return chunk.blocks[blockX, y, blockZ];
        }

        public void SetBlock(int x, int y, int z, Block block)
        {
            var (chunkX, blockX) = ToChunkCoord(x);
            var (chunkZ, blockZ) = ToChunkCoord(z);

            if (y >= Chunk.HEIGHT || y < 0)
                return;

            if (chunks.TryGetValue((chunkX, chunkZ), out Chunk chunk)
             || !chunk.isLoaded)
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

        public int GetGroundLevel(int x, int z)
        {
            var (chunkX, blockX) = ToChunkCoord(x);
            var (chunkZ, blockZ) = ToChunkCoord(z);

            if (!chunks.TryGetValue((chunkX, chunkZ), out Chunk chunk)
             || !chunk.isLoaded)
                return -1;

            return chunk.groundLevel[blockX, blockZ];
        }

        public float GetLightLevel(int x, int y, int z)
        {
            var (chunkX, blockX) = ToChunkCoord(x);
            var (chunkZ, blockZ) = ToChunkCoord(z);

            if (!chunks.TryGetValue((chunkX, chunkZ), out Chunk chunk)
             || !chunk.isLoaded
             || chunk.lightMapData is null)
                return 0f;

            return chunk.lightMapData[blockX + 1, y, blockZ + 1];
        }

        public Chunk GetChunk(int x, int z)
        {
            if (chunks.TryGetValue((x, z), out Chunk chunk)
             && chunk.isLoaded)
                return chunk;
            else
                return null;
        }

        public void GenerateSpawnArea()
        {
            List<Task> chunkLoaders = new List<Task>();
            for (int x = -LOAD_DISTANCE; x <= LOAD_DISTANCE; x++)
                for (int z = -LOAD_DISTANCE; z <= LOAD_DISTANCE; z++)
                    chunkLoaders.Add(LoadChunkAsync((x, z)));

            while (!Task.WhenAll(chunkLoaders).IsCompleted) ;
        }

        public void Update(Vertex3f cameraPosition, bool firstUpdate = false)
        {
            int cameraChunkX = ToChunkCoord((int)Math.Floor(cameraPosition.x)).chunk;
            int cameraChunkZ = ToChunkCoord((int)Math.Floor(cameraPosition.z)).chunk;

            // Chunk loading

            if (firstUpdate || lastCameraChunkX != cameraChunkX || lastCameraChunkZ != cameraChunkZ)
            {
                lastCameraChunkX = cameraChunkX;
                lastCameraChunkZ = cameraChunkZ;

                // Ensure all chunks in the area are loaded

                for (int x = cameraChunkX - LOAD_DISTANCE; x <= cameraChunkX + LOAD_DISTANCE; x++)
                    for (int z = cameraChunkZ - LOAD_DISTANCE; z <= cameraChunkZ + LOAD_DISTANCE; z++)
                        if ( !chunks.ContainsKey((x, z)) )
                            LoadChunkAsync((x, z));

                // Unload all the chunks outside the area

                List<(int x, int z)> chunksToUnload = new List<(int x, int z)>();
                foreach (var coords in chunks.Keys)
                    if (Math.Abs(coords.x - cameraChunkX) > UNLOAD_DISTANCE
                     || Math.Abs(coords.z - cameraChunkZ) > UNLOAD_DISTANCE)
                        chunksToUnload.Add(coords);

                foreach (var coords in chunksToUnload)
                    UnloadChunk(coords.x, coords.z);

                renderQueueNeedsUpdate = true;
            }

            // Update the render queue if needed

            if (renderQueueNeedsUpdate)
            {
                renderQueueNeedsUpdate = false;

                renderQueue.Clear();
                foreach (var pair in chunks)
                {
                    if (!pair.Value.isLoaded)
                        continue;

                    int dx = pair.Key.x - cameraChunkX;
                    int dz = pair.Key.z - cameraChunkZ;
                    int distance = dx * dx + dz * dz;

                    renderQueue.Add((distance, pair.Value));
                }

                renderQueue.Sort(
                    ((int distance, Chunk chunk) a, (int distance, Chunk chunk) b)
                        => b.distance.CompareTo(a.distance)
                );
            }

            // Update chunk meshes

            Parallel.ForEach(renderQueue, (pair) =>
                pair.chunk.GenerateMesh(cameraPosition)
            );

            foreach (var (_, chunk) in renderQueue)
                chunk.RefreshVertexData();
        }

        private void OnChunkUpdate(int x, int z)
        {
            Chunk chunk;
            if (chunks.TryGetValue((x - 1, z), out chunk)) chunk.needsUpdate = true;
            if (chunks.TryGetValue((x + 1, z), out chunk)) chunk.needsUpdate = true;
            if (chunks.TryGetValue((x, z - 1), out chunk)) chunk.needsUpdate = true;
            if (chunks.TryGetValue((x, z + 1), out chunk)) chunk.needsUpdate = true;
        }

        private Task LoadChunkAsync((int x, int z) coords)
        {
            Chunk newChunk = new Chunk(this, coords.x, coords.z);
            chunks.Add(coords, newChunk);

            return Task.Run(() =>
            {
                generator.GenerateChunk(newChunk);

                newChunk.isLoaded = true;
                newChunk.needsUpdate = true;
                OnChunkUpdate(coords.x, coords.z);
                renderQueueNeedsUpdate = true;
            });
        }

        private void UnloadChunk(int x, int z)
        {
            chunks.Remove((x, z));
            OnChunkUpdate(x, z);
        }

        public void Draw()
        {
            foreach (var (_, chunk) in renderQueue)
            {
                Resources.GameWorldShader.ChunkStart = new Vertex3f(chunk.xOffset, 0f, chunk.zOffset);
                chunk.Draw();
            }
        }

        public void Dispose()
        {
            foreach (Chunk chunk in chunks.Values)
                chunk.Dispose();
        }
    }
}
