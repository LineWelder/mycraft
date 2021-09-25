using Mycraft.Blocks;
using Mycraft.Graphics;
using Mycraft.Utils;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mycraft.World
{
    public class Chunk : IDisposable
    {
        private class WorldGeometry : VertexArray
        {
            public new float[] Data { set => base.Data = value; }

            public WorldGeometry()
                : base(PrimitiveType.Quads, new int[] { 3, 2, 1 }) { }
        }

        public const int SIZE = 16;
        public const int HEIGHT = 256;

        public bool needsUpdate;
        public readonly Block[,,] blocks;
        public readonly int[,] groundLevel;

        public readonly GameWorld world;
        public readonly int xOffset, zOffset;

        private readonly List<Quad> liquidQuads;
        private readonly WorldGeometry solidMesh, waterMesh;

        public Chunk(GameWorld world, int x, int z)
        {
            blocks = new Block[SIZE, HEIGHT, SIZE];
            groundLevel = new int[SIZE, SIZE];

            this.world = world;
            xOffset = x * SIZE;
            zOffset = z * SIZE;

            liquidQuads = new List<Quad>();
            solidMesh = new WorldGeometry();
            waterMesh = new WorldGeometry();
        }

        public void Draw()
        {
            Resources.BlocksTexture.Bind();

            Gl.Enable(EnableCap.CullFace);
            solidMesh.Draw();
            Gl.Disable(EnableCap.CullFace);

            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
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

        public void UpToDateMesh(Vertex3f cameraPosition)
        {
            if (needsUpdate)
            {
                needsUpdate = false;

                List<Quad> solidQuads = new List<Quad>();
                liquidQuads.Clear();

                for (int cx = 0; cx < SIZE; cx++)
                    for (int cz = 0; cz < SIZE; cz++)
                        for (int cy = 0; cy < HEIGHT; cy++)
                        {
                            Block block = blocks[cx, cy, cz];

                            if (block is LiquidBlock)
                                block.EmitMesh(liquidQuads, this, cx, cy, cz);
                            else
                                block.EmitMesh(solidQuads, this, cx, cy, cz);
                        }

                solidMesh.Data = ToFloatArray(solidQuads);
            }

            liquidQuads.Sort(
                (Quad a, Quad b) => (b.Center - cameraPosition).ModuleSquared()
                         .CompareTo((a.Center - cameraPosition).ModuleSquared())
            );

            waterMesh.Data = ToFloatArray(liquidQuads);
        }

        public void Dispose()
        {
            solidMesh.Dispose();
            waterMesh.Dispose();
        }
    }
}
