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

        public bool needsLightRecalculation;
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

            lightMap = new LightMap(this);
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

            List<Quad> cachedQuads = new List<Quad>(quads);
            float[] array = new float[cachedQuads.Count * QUAD_SIZE];

            for (int i = 0; i < cachedQuads.Count; i++)
            {
                Quad quad = cachedQuads[i];
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

        public Task UpdateLightAsync()
        {
            if (!needsLightRecalculation)
                return Task.CompletedTask;

            needsLightRecalculation = false;
            return Task.Run(lightMap.BuildDataMap);
        }

        public Task UpdateMeshAsync()
        {
            if (!needsUpdate)
                return Task.CompletedTask;

            needsUpdate = false;
            needsLightRecalculation = true;

            return Task.Run(() =>
            {
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
                List<Quad> cachedWaterQuads = new List<Quad>(waterQuads);

                Vertex3f offset = new Vertex3f(xOffset, 0f, zOffset) - world.ObservingCamera.Position;
                cachedWaterQuads.Sort(
                    (Quad a, Quad b) => (b.Center + offset).ModuleSquared()
                             .CompareTo((a.Center + offset).ModuleSquared())
                );

                waterVertices = ToFloatArray(cachedWaterQuads);
            });
        }

        public bool RefreshVertexData()
        {
            bool refreshed = false;

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

            if (lightMap.UpdateIfNeeded())
                refreshed = true;

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
