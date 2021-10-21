using System;
using System.Collections.Generic;
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

        public bool needsUpdate;
        public readonly Block[,,] blocks;
        public readonly int[,] groundLevel;

        public readonly GameWorld world;
        public readonly int xOffset, zOffset;

        private readonly List<Quad> waterQuads;
        private float[] solidVertices, waterVertices;
        private readonly WorldGeometry solidMesh, waterMesh;

        private float[,,] lightMapData;
        private LightMap lightMap;

        public Chunk(GameWorld world, int x, int z)
        {
            blocks = new Block[SIZE, HEIGHT, SIZE];
            groundLevel = new int[SIZE, SIZE];

            this.world = world;
            xOffset = x * SIZE;
            zOffset = z * SIZE;

            waterQuads = new List<Quad>();
            solidMesh = new WorldGeometry();
            waterMesh = new WorldGeometry();

            lightMap = new LightMap();
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

            Gl.Enable(EnableCap.Blend);
            waterMesh.Draw();
            Gl.Disable(EnableCap.Blend);
        }

        private float[] ToFloatArray(List<Quad> quads)
        {
            const int QUAD_SIZE = 6 * 4;

            float[] array = new float[quads.Count * QUAD_SIZE];
            void SaveVertex(int i, Vertex vertex)
            {
                array[i]     = vertex.position.x;
                array[i + 1] = vertex.position.y;
                array[i + 2] = vertex.position.z;
                array[i + 3] = vertex.texture.x;
                array[i + 4] = vertex.texture.y;
                array[i + 5] = vertex.light;
            }

            for (int i = 0; i < quads.Count; i++)
            {
                Quad quad = quads[i];

                SaveVertex(i * QUAD_SIZE,      quad.a);
                SaveVertex(i * QUAD_SIZE + 6,  quad.b);
                SaveVertex(i * QUAD_SIZE + 12, quad.c);
                SaveVertex(i * QUAD_SIZE + 18, quad.d);
            }

            return array;
        }

        private void RecalculateLight()
        {
            lightMapData = new float[SIZE, HEIGHT, SIZE];

            for (int x = 0; x < SIZE; x++)
                for (int y = 0; y < HEIGHT; y++)
                    for (int z = 0; z < SIZE; z++)
                        lightMapData[z, y, x] = 1f;

            for (int y = HEIGHT - 1; y > 0; y--)
                for (int x = 0; x < SIZE; x++)
                    for (int z = 0; z < SIZE; z++)
                        if (!blocks[x, y, z].IsTransparent)
                            for (int y_ = y - 1; y_ >= 0; y_--)
                                lightMapData[z, y_, x] = .5f;
        }

        public void GenerateMesh(Vertex3f cameraPosition)
        {
            if (needsUpdate)
            {
                needsUpdate = false;

                RecalculateLight();

                List<Quad> solidQuads = new List<Quad>();
                waterQuads.Clear();

                for (int cx = 0; cx < SIZE; cx++)
                    for (int cz = 0; cz < SIZE; cz++)
                        for (int cy = 0; cy < HEIGHT; cy++)
                        {
                            Block block = blocks[cx, cy, cz];

                            if (block is LiquidBlock)
                                block.EmitMesh(waterQuads, this, cx, cy, cz);
                            else
                                block.EmitMesh(solidQuads, this, cx, cy, cz);
                        }

                solidVertices = ToFloatArray(solidQuads);
            }

            waterQuads.Sort(
                (Quad a, Quad b) => (b.Center - cameraPosition).ModuleSquared()
                         .CompareTo((a.Center - cameraPosition).ModuleSquared())
            );

            waterVertices = ToFloatArray(waterQuads);
        }

        public void RefreshVertexData()
        {
            if (!(solidVertices is null))
            {
                solidMesh.Data = solidVertices;
                solidVertices = null;

                lightMap.Data = lightMapData;
                lightMapData = null;
            }

            if (!(waterVertices is null))
            {
                waterMesh.Data = waterVertices;
                waterVertices = null;
            }
        }

        public void Dispose()
        {
            solidMesh.Dispose();
            waterMesh.Dispose();
            lightMap.Dispose();
        }
    }
}
