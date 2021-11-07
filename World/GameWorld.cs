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
        public const int LOAD_DISTANCE = 16;
        public const int UNLOAD_DISTANCE = LOAD_DISTANCE + 2;

        public Camera ObservingCamera { get; set; }

        private readonly Dictionary<(int x, int z), Chunk> chunks;
        private readonly List<(int distance, Chunk chunk)> renderQueue;

        private readonly IWorldGenerator generator;
        
        private int lastCameraChunkX, lastCameraChunkZ;
        private int lastCameraX, lastCameraY, lastCameraZ;
        private bool renderQueueNeedsUpdate;

        public GameWorld(IWorldGenerator generator)
        {
            chunks = new Dictionary<(int x, int z), Chunk>();
            renderQueue = new List<(int distance, Chunk chunk)>();

            this.generator = generator;
        }

        public static (int chunk, int block) ToChunkCoord(int v)
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
            renderQueueNeedsUpdate = true;
        }

        private readonly Profiler profiler = new Profiler();

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void ThrottleUpdates(Func<Chunk, bool> updateFunc, int updates)
        {
            for (int i = renderQueue.Count - 1; i >= 0; i--)
            {
                if (updateFunc(renderQueue[i].chunk))
                    updates--;

                if (updates <= 0)
                    return;
            }
        }

        public void Update()
        {
            int cameraX = (int)Math.Floor(ObservingCamera.Position.x);
            int cameraY = (int)Math.Floor(ObservingCamera.Position.y);
            int cameraZ = (int)Math.Floor(ObservingCamera.Position.z);

            int cameraChunkX = ToChunkCoord(cameraX).chunk;
            int cameraChunkZ = ToChunkCoord(cameraZ).chunk;
            
            // Chunk loading

            if (lastCameraChunkX != cameraChunkX || lastCameraChunkZ != cameraChunkZ)
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
            bool printInfo = renderQueueNeedsUpdate;

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
                    pair.Value.needsTransparentGeometrySort = true;
                }

                renderQueue.Sort(
                    ((int distance, Chunk chunk) a, (int distance, Chunk chunk) b)
                        => b.distance.CompareTo(a.distance)
                );
            }

            // If the player changes the block they are in

            if (cameraX != lastCameraX || cameraY != lastCameraY || cameraZ != lastCameraZ)
            {
                lastCameraX = cameraX;
                lastCameraY = cameraY;
                lastCameraZ = cameraZ;

                chunks[(cameraChunkX, cameraChunkZ)].needsTransparentGeometrySort = true;
            }

            // Update chunk meshes

            profiler.NewFrame();

            ThrottleUpdates(chunk => !chunk.UpdateMeshAsync().IsCompleted, 3);

            profiler.EndFragment("Mesh updates");

            ThrottleUpdates(chunk => !chunk.EnsureTransparentGeometrySortedAsync().IsCompleted, 3);

            profiler.EndFragment("Transparent geometry sorting");

            ThrottleUpdates(chunk => chunk.RefreshVertexData(), 3);

            profiler.EndFragment("Vertex data updates");
            profiler.EndFrame();

            if (profiler.FrameTime > 2)
                profiler.PrintInfo();
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
