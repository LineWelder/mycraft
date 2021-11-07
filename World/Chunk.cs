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
        public float[,,] lightMapData;
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

            lightMap = new LightMap();
            lightMapData = new float[SIZE + 2, HEIGHT, SIZE + 2];
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
            // Reset the light map

            for (int x = 0; x < SIZE; x++)
                for (int y = 0; y < HEIGHT; y++)
                    for (int z = 0; z < SIZE; z++)
                        lightMapData[z + 1, y, x + 1] = 1f;

            // Apply the shades

            for (int y = HEIGHT - 1; y > 0; y--)
                for (int x = 0; x < SIZE; x++)
                    for (int z = 0; z < SIZE; z++)
                        if (!blocks[x, y, z].IsTransparent)
                            for (int y_ = y - 1; y_ >= 0; y_--)
                                lightMapData[z + 1, y_, x + 1] = .7f;

            // Update the neighbouring chunks

            Chunk frontChunk = world.GetChunk(xOffset / SIZE, zOffset / SIZE + 1);
            if (!(frontChunk is null))
            {
                for (int y = 0; y < HEIGHT; y++)
                    for (int x = 0; x < SIZE; x++)
                    {
                        frontChunk.lightMapData[0, y, x + 1]
                            = lightMapData[16, y, x + 1];

                        lightMapData[17, y, x + 1]
                            = frontChunk.lightMapData[1, y, x + 1];

                        frontChunk.lightMapNeedsUpdate = true;
                    }
            }

            Chunk backChunk = world.GetChunk(xOffset / SIZE, zOffset / SIZE - 1);
            if (!(backChunk is null))
            {
                for (int y = 0; y < HEIGHT; y++)
                    for (int x = 0; x < SIZE; x++)
                    {
                        backChunk.lightMapData[17, y, x + 1]
                            = lightMapData[1, y, x + 1];

                        lightMapData[0, y, x + 1]
                            = backChunk.lightMapData[16, y, x + 1];

                        backChunk.lightMapNeedsUpdate = true;
                    }
            }

            Chunk rightChunk = world.GetChunk(xOffset / SIZE + 1, zOffset / SIZE);
            if (!(rightChunk is null))
            {
                for (int y = 0; y < HEIGHT; y++)
                    for (int z = 0; z < SIZE; z++)
                    {
                        rightChunk.lightMapData[z + 1, y, 0]
                            = lightMapData[z + 1, y, 16];

                        lightMapData[z + 1, y, 17]
                            = rightChunk.lightMapData[z + 1, y, 1];

                        rightChunk.lightMapNeedsUpdate = true;
                    }
            }

            Chunk leftChunk = world.GetChunk(xOffset / SIZE - 1, zOffset / SIZE);
            if (!(leftChunk is null))
            {
                for (int y = 0; y < HEIGHT; y++)
                    for (int z = 0; z < SIZE; z++)
                    {
                        leftChunk.lightMapData[z + 1, y, 17]
                            = lightMapData[z + 1, y, 1];

                        lightMapData[z + 1, y, 0]
                            = leftChunk.lightMapData[z + 1, y, 16];

                        leftChunk.lightMapNeedsUpdate = true;
                    }
            }

            Chunk frontRightChunk = world.GetChunk(xOffset / SIZE + 1, zOffset / SIZE + 1);
            if (!(frontRightChunk is null))
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    frontRightChunk.lightMapData[0, y, 0]
                        = lightMapData[16, y, 16];

                    lightMapData[17, y, 17]
                        = frontRightChunk.lightMapData[1, y, 1];

                    frontRightChunk.lightMapNeedsUpdate = true;
                }
            }

            Chunk backRightChunk = world.GetChunk(xOffset / SIZE + 1, zOffset / SIZE - 1);
            if (!(backRightChunk is null))
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    backRightChunk.lightMapData[17, y, 0]
                        = lightMapData[1, y, 16];

                    lightMapData[0, y, 17]
                        = backRightChunk.lightMapData[16, y, 1];

                    backRightChunk.lightMapNeedsUpdate = true;
                }
            }

            Chunk frontLeftChunk = world.GetChunk(xOffset / SIZE - 1, zOffset / SIZE + 1);
            if (!(frontLeftChunk is null))
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    frontLeftChunk.lightMapData[0, y, 17]
                        = lightMapData[16, y, 1];

                    lightMapData[17, y, 0]
                        = frontLeftChunk.lightMapData[1, y, 16];

                    frontLeftChunk.lightMapNeedsUpdate = true;
                }
            }

            Chunk backLeftChunk = world.GetChunk(xOffset / SIZE - 1, zOffset / SIZE - 1);
            if (!(backLeftChunk is null))
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    backLeftChunk.lightMapData[17, y, 17]
                        = lightMapData[1, y, 1];

                    lightMapData[0, y, 0]
                        = backLeftChunk.lightMapData[16, y, 16];

                    backLeftChunk.lightMapNeedsUpdate = true;
                }
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
