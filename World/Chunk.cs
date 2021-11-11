using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenGL;

using Mycraft.Blocks;
using Mycraft.Graphics;
using Mycraft.Utils;

namespace Mycraft.World
{
    public class Chunk : IDisposable
    {
        private class WorldGeometry : VertexArray
        {
            public new float[] Data { set => base.Data = value; }

            public WorldGeometry()
                : base(PrimitiveType.Quads, Resources.GameWorldShader) { }
        }

        public const int SIZE = 16;
        public const int HEIGHT = 256;

        public bool isLoaded;
        public bool needsUpdate;
        public bool needsTransparentGeometrySort;
        public readonly Block[,,] blocks;
        public readonly int[,] groundLevel;

        public readonly GameWorld world;
        public readonly int xOffset, zOffset;

        private readonly List<Quad> waterQuads;
        private float[] solidVertices, doubleSidedVertices, waterVertices;
        private readonly WorldGeometry solidMesh, doubleSidedMesh, waterMesh;

        public bool lightMapNeedsUpdate;
        public float[,,] flatLightMapData;
        private float[,,] lightMapData;
        private readonly LightMap lightMap;

        public Chunk(GameWorld world, int x, int z)
        {
            blocks = new Block[SIZE, HEIGHT, SIZE];
            groundLevel = new int[SIZE, SIZE];

            this.world = world;
            xOffset = x * SIZE;
            zOffset = z * SIZE;

            waterQuads = new List<Quad>();
            solidMesh = new WorldGeometry();
            doubleSidedMesh = new WorldGeometry();
            waterMesh = new WorldGeometry();

            lightMap = new LightMap(SIZE + 1, HEIGHT + 1, SIZE + 1);
            flatLightMapData = new float[SIZE, HEIGHT, SIZE];
            lightMapData = new float[SIZE + 1, HEIGHT + 1, SIZE + 1];
        }

        public void Draw()
        {
            Gl.ActiveTexture(TextureUnit.Texture1);
            lightMap.Bind();

            Gl.ActiveTexture(TextureUnit.Texture0);
            Resources.BlocksTexture.Bind();

            Gl.Enable(EnableCap.CullFace);
            solidMesh.Draw();

            Gl.Disable(EnableCap.CullFace);
            doubleSidedMesh.Draw();

            Gl.Enable(EnableCap.Blend);
            waterMesh.Draw();
            Gl.Disable(EnableCap.Blend);
        }

        private float[] ToFloatArray(List<Quad> quads)
        {
            const int VERTEX_SIZE = 7;
            const int QUAD_SIZE = VERTEX_SIZE * 4;

            float[] array = new float[quads.Count * QUAD_SIZE];

            for (int i = 0; i < quads.Count; i++)
            {
                Quad quad = quads[i];
                void SaveVertex(int index, Vertex vertex)
                {
                    if (index >= array.Length)
                        return;

                    array[index]     = vertex.position.x;
                    array[index + 1] = vertex.position.y;
                    array[index + 2] = vertex.position.z;
                    array[index + 3] = vertex.texture.x;
                    array[index + 4] = vertex.texture.y;
                    array[index + 5] = quad.textureId;
                    array[index + 6] = vertex.light;
                }

                SaveVertex(i * QUAD_SIZE,                   quad.a);
                SaveVertex(i * QUAD_SIZE + VERTEX_SIZE,     quad.b);
                SaveVertex(i * QUAD_SIZE + VERTEX_SIZE * 2, quad.c);
                SaveVertex(i * QUAD_SIZE + VERTEX_SIZE * 3, quad.d);
            }

            return array;
        }

        private void RecalculateLight()
        {
            // Caching the neighbouring chunks

            Chunk frontChunk = world.GetChunk(xOffset / SIZE, zOffset / SIZE + 1);
            Chunk backChunk = world.GetChunk(xOffset / SIZE, zOffset / SIZE - 1);
            Chunk rightChunk = world.GetChunk(xOffset / SIZE + 1, zOffset / SIZE);
            Chunk leftChunk = world.GetChunk(xOffset / SIZE - 1, zOffset / SIZE);
            Chunk frontRightChunk = world.GetChunk(xOffset / SIZE + 1, zOffset / SIZE + 1);
            Chunk backRightChunk = world.GetChunk(xOffset / SIZE + 1, zOffset / SIZE - 1);
            Chunk frontLeftChunk = world.GetChunk(xOffset / SIZE - 1, zOffset / SIZE + 1);
            Chunk backLeftChunk = world.GetChunk(xOffset / SIZE - 1, zOffset / SIZE - 1);

            // Reset the light map

            for (int x = 0; x < SIZE; x++)
                for (int y = 0; y < HEIGHT; y++)
                    for (int z = 0; z < SIZE; z++)
                        flatLightMapData[x, y, z] = 1f;

            // Apply the shades

            void MakeShade(int x, int y, int z)
            {
                for (int y_ = y - 1; y_ >= 0; y_--)
                    flatLightMapData[x, y_, z] = 0f;
            }

            for (int y = HEIGHT - 1; y > 0; y--)
                for (int x = 0; x < SIZE; x++)
                    for (int z = 0; z < SIZE; z++)
                        if (!blocks[x, y, z].IsTransparent)
                            MakeShade(x, y, z);

            // Blend the shades

            Chunk GetChunkFromCoords(int x, int z, out int newX, out int newZ)
            {
                if (x >= SIZE)
                {
                    newX = x - SIZE;

                    if (z >= SIZE)
                    {
                        newZ = z - SIZE;
                        return frontRightChunk;
                    }
                    else if (z < 0)
                    {
                        newZ = z + SIZE;
                        return backRightChunk;
                    }
                    else
                    {
                        newZ = z;
                        return rightChunk;
                    }
                }
                else if (x < 0)
                {
                    newX = x + SIZE;

                    if (z >= SIZE)
                    {
                        newZ = z - SIZE;
                        return frontLeftChunk;
                    }
                    else if (z < 0)
                    {
                        newZ = z + SIZE;
                        return backLeftChunk;
                    }
                    else
                    {
                        newZ = z;
                        return leftChunk;
                    }
                }
                else
                {
                    newX = x;

                    if (z >= SIZE)
                    {
                        newZ = z - SIZE;
                        return frontChunk;
                    }
                    else if (z < 0)
                    {
                        newZ = z + SIZE;
                        return backChunk;
                    }
                    else
                    {
                        newZ = z;
                        return this;
                    }
                }
            }

            bool ProbeLight(int x, int y, int z, out float level)
            {
                if (y >= HEIGHT)
                {
                    level = 1f;
                    return false;
                }
                else if (y < 0)
                {
                    level = 0f;
                    return false;
                }

                Chunk chunk = GetChunkFromCoords(x, z, out int newX, out int newZ);
                if (chunk is null || !chunk.blocks[newX, y, newZ].IsTransparent)
                {
                    level = 0f;
                    return false;
                }

                level = chunk.flatLightMapData[newX, y, newZ];
                return true;
            }

            bool IsNeighbouring(int x, int y, int z)
                => x == 0 && y == 0 && z != 0
                || x == 0 && y != 0 && z == 0
                || x != 0 && y == 0 && z == 0;

            void BlendShades(int x, int y, int z)
            {
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                        for (int dz = -1; dz <= 1; dz++)
                            if (IsNeighbouring(dx, dy, dz))
                            {
                                if (ProbeLight(x + dx, y + dy, z + dz, out float level))
                                {
                                    float blendValue = level - 1f / 16f;
                                    if (flatLightMapData[x, y, z] < blendValue)
                                        flatLightMapData[x, y, z] = blendValue;
                                }
                            }
            }

            for (int i = 0; i < 16; i++)
            {
                for (int y = HEIGHT - 1; y > 0; y--)
                    for (int x = 0; x < SIZE; x++)
                        for (int z = 0; z < SIZE; z++)
                            BlendShades(x, y, z);
            }

            // Use the flat light map to generate the actual LightMap data

            for (int x = 0; x <= SIZE; x++)
                for (int y = 0; y <= HEIGHT; y++)
                    for (int z = 0; z <= SIZE; z++)
                    {
                        float lightAccum = 0f;
                        int probesCount = 0;
                        for (int x_ = -1; x_ <= 0; x_++)
                            for (int y_ = -1; y_ <= 0; y_++)
                                for (int z_ = -1; z_ <= 0; z_++)
                                    if (ProbeLight(x + x_, y + y_, z + z_, out float level))
                                    {
                                        lightAccum += level;
                                        probesCount++;
                                    }

                        lightMapData[z, y, x] = lightAccum / probesCount;
                    }

            lightMapNeedsUpdate = true;
        }

        public Task UpdateMeshAsync()
        {
            if (!needsUpdate)
                return Task.CompletedTask;

            needsUpdate = false;

            return Task.Run(() =>
            {
                RecalculateLight();

                List<Quad> solidQuads = new List<Quad>();
                List<Quad> doubleSidedQuads = new List<Quad>();
                waterQuads.Clear();

                for (int cx = 0; cx < SIZE; cx++)
                    for (int cz = 0; cz < SIZE; cz++)
                        for (int cy = 0; cy < HEIGHT; cy++)
                        {
                            Block block = blocks[cx, cy, cz];

                            if (block is LiquidBlock)
                                block.EmitMesh(waterQuads, this, cx, cy, cz);
                            else if (block is PlantBlock)
                                block.EmitMesh(doubleSidedQuads, this, cx, cy, cz);
                            else
                                block.EmitMesh(solidQuads, this, cx, cy, cz);
                        }

                solidVertices = ToFloatArray(solidQuads);
                doubleSidedVertices = ToFloatArray(doubleSidedQuads);

                needsTransparentGeometrySort = true;
            });
        }

        public Task EnsureTransparentGeometrySortedAsync()
        {
            if (!needsTransparentGeometrySort)
                return Task.CompletedTask;

            needsTransparentGeometrySort = false;
            return Task.Run(() =>
            {
                Vertex3f offset = new Vertex3f(xOffset, 0f, zOffset) - world.ObservingCamera.Position;
                waterQuads.Sort(
                    (Quad a, Quad b) => (b.Center + offset).ModuleSquared()
                             .CompareTo((a.Center + offset).ModuleSquared())
                );

                waterVertices = ToFloatArray(waterQuads);
            });
        }

        public bool RefreshVertexData()
        {
            bool refreshed = false;

            if (lightMapNeedsUpdate)
            {
                lightMap.Data = lightMapData;
                lightMapNeedsUpdate = false;

                refreshed = true;
            }

            if (!(solidVertices is null))
            {
                solidMesh.Data = solidVertices;
                solidVertices = null;

                refreshed = true;
            }

            if (!(doubleSidedVertices is null))
            {
                doubleSidedMesh.Data = doubleSidedVertices;
                doubleSidedVertices = null;

                refreshed = true;
            }

            if (!(waterVertices is null))
            {
                waterMesh.Data = waterVertices;
                waterVertices = null;

                refreshed = true;
            }

            return refreshed;
        }

        public void Dispose()
        {
            solidMesh.Dispose();
            doubleSidedMesh.Dispose();
            waterMesh.Dispose();
            lightMap.Dispose();
        }
    }
}
