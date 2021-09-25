using Mycraft.Blocks;
using Mycraft.Graphics;
using Mycraft.Utils;
using OpenGL;
using System;
using System.Collections.Generic;

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

        private readonly WorldGeometry solidMesh;
        private readonly WorldGeometry waterMesh;

        public Chunk(GameWorld world, int x, int z)
        {
            blocks = new Block[SIZE, HEIGHT, SIZE];
            groundLevel = new int[SIZE, SIZE];

            this.world = world;
            xOffset = x * SIZE;
            zOffset = z * SIZE;

            solidMesh = new WorldGeometry();
            waterMesh = new WorldGeometry();
        }

        public void Draw()
        {
            Resources.BlocksTexture.Bind();
            solidMesh.Draw();

            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            waterMesh.Draw();
            Gl.Disable(EnableCap.Blend);
        }

        public void UpToDateMesh(int cameraX, int cameraY, int cameraZ)
        {
            if (!needsUpdate) return;
            needsUpdate = false;

            List<float> solidVertices = new List<float>();
            List<float> liquidVertices = new List<float>();

            for (int cx = 0; cx < SIZE; cx++)
                for (int cz = 0; cz < SIZE; cz++)
                    for (int cy = 0; cy < HEIGHT; cy++)
                    {
                        Block block = blocks[cx, cy, cz];

                        if (block is LiquidBlock)
                            block.EmitVertices(liquidVertices, this, cx, cy, cz);
                        else
                            block.EmitVertices(solidVertices, this, cx, cy, cz);
                    }

            solidMesh.Data = solidVertices.ToArray();
            waterMesh.Data = liquidVertices.ToArray();
        }

        public void Dispose()
        {
            solidMesh.Dispose();
            waterMesh.Dispose();
        }
    }
}
