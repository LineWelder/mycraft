using System;
using System.Collections.Generic;
using OpenGL;

using Mycraft.Graphics;
using Mycraft.Utils;
using Mycraft.World;

namespace Mycraft.WorldUI
{
    public class ChunkBorders : IDisposable
    {
        private Matrix4x4f modelMatrix;
        private readonly VertexArray currentChunkBorders;
        private readonly VertexArray otherChunkBorders;

        public ChunkBorders()
        {
            currentChunkBorders = new VertexArray(PrimitiveType.Lines, Resources.WorldUIShader);
            otherChunkBorders   = new VertexArray(PrimitiveType.Lines, Resources.WorldUIShader);
            modelMatrix = Matrix4x4f.Identity;

            List<float> currentChunkVertexData = new List<float>(4 * Chunk.SIZE + 2 * 6);
            List<float> otherChunkVertexData = new List<float>(12 * 6);

            void EmitVerticalLine(List<float> vertexData, int x, int z)
            {
                vertexData.Add(x);
                vertexData.Add(0f);
                vertexData.Add(z);

                vertexData.Add(x);
                vertexData.Add(Chunk.HEIGHT);
                vertexData.Add(z);
            }

            for (int x = 0; x < Chunk.SIZE + 1; x++)
                for (int z = 0; z < Chunk.SIZE + 1; z++)
                    if (x == 0 || x == Chunk.SIZE || z == 0 || z == Chunk.SIZE)
                        EmitVerticalLine(
                            currentChunkVertexData,
                            x, z
                        );

            for (int x = 0; x < 4; x++)
                for (int z = 0; z < 4; z++)
                    if (x == 0 || x == 3 || z == 0 || z == 3)
                        EmitVerticalLine(
                            otherChunkVertexData,
                            (x - 1) * Chunk.SIZE,
                            (z - 1) * Chunk.SIZE
                        );

            currentChunkBorders = new VertexArray(
                PrimitiveType.Lines,
                Resources.WorldUIShader,
                currentChunkVertexData.ToArray()
            );

            otherChunkBorders = new VertexArray(
                PrimitiveType.Lines,
                Resources.WorldUIShader,
                otherChunkVertexData.ToArray()
            );
        }

        public void Update(Vertex3f cameraPosition)
        {
            var (chunkX, _) = GameWorld.ToChunkCoord((int)Math.Floor(cameraPosition.x));
            var (chunkZ, _) = GameWorld.ToChunkCoord((int)Math.Floor(cameraPosition.z));

            modelMatrix = Matrix4x4f.Translated(chunkX * Chunk.SIZE, 0f, chunkZ * Chunk.SIZE);
        }

        public void Draw()
        {
            Resources.WorldUIShader.Model = modelMatrix;

            Resources.WorldUIShader.Color = new Vertex3f(.1f);
            currentChunkBorders.Draw();

            Resources.WorldUIShader.Color = new Vertex3f(.3f);
            otherChunkBorders.Draw();
        }

        public void Dispose()
        {
            currentChunkBorders.Dispose();
            otherChunkBorders.Dispose();
        }
    }
}
