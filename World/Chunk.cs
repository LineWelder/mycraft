﻿using Mycraft.Blocks;
using Mycraft.Graphics;
using Mycraft.Utils;
using OpenGL;
using System;
using System.Collections.Generic;

namespace Mycraft.World
{
    public class Chunk : VertexArray
    {
        public const int SIZE = 16;
        public const int HEIGHT = 256;

        public bool needsUpdate;
        public readonly Block[,,] blocks;

        private readonly GameWorld world;
        private readonly int chunkX, chunkZ;

        public Chunk(GameWorld world, int x, int z)
            : base(PrimitiveType.Quads, new int[] { 3, 2, 1 })
        {
            blocks = new Block[SIZE, HEIGHT, SIZE];
            this.world = world;
            chunkX = x;
            chunkZ = z;
        }

        public new void Draw()
        {
            Resources.BlocksTexture.Bind();
            base.Draw();
        }

        public void Generate()
        {
            FastNoiseLite noise = new FastNoiseLite();
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

            int offsetX = chunkX * SIZE;
            int offsetZ = chunkZ * SIZE;

            for (int x = 0; x < SIZE; x++)
                for (int z = 0; z < SIZE; z++)
                {
                    float NoiseLayer(float scale, float amplitude)
                            => noise.GetNoise((x + offsetX) * scale, (z + offsetZ) * scale) * amplitude;

                    int height = (int)Math.Round(
                        19f + NoiseLayer(1f, 8f) + NoiseLayer(4f, 2f)
                    );

                    for (int y = 0; y < HEIGHT; y++)
                        if (y < height - 3)
                            blocks[x, y, z] = BlockRegistry.Stone;
                        else if (height - 3 <= y && y < height)
                            blocks[x, y, z] = BlockRegistry.Dirt;
                        else if (y == height)
                            blocks[x, y, z] = BlockRegistry.Grass;
                        else
                            blocks[x, y, z] = BlockRegistry.Air;
                }
        }

        private Block GetBlockExtended(int x, int y, int z)
        {
            if (y < 0 || y >= HEIGHT)
                return BlockRegistry.Void;

            if (x >= 0 && x < SIZE
             && z >= 0 && z < SIZE)
                return blocks[x, y, z];

            return world.GetBlock(chunkX * SIZE + x, y, chunkZ * SIZE + z);
        }

        public void UpToDateMesh()
        {
            if (!needsUpdate) return;
            needsUpdate = false;

            List<float> mesh = new List<float>();

            float chunkX = this.chunkX * SIZE;
            float chunkZ = this.chunkZ * SIZE;

            for (int cx = 0; cx < SIZE; cx++)
                for (int cz = 0; cz < SIZE; cz++)
                    for (int cy = 0; cy < HEIGHT; cy++)
                    {
                        Block block = blocks[cx, cy, cz];

                        if (!block.IsVisible)
                            continue;

                        float wx = chunkX + cx;
                        float wz = chunkZ + cz;
                        float wy = cy;

                        // Bottom
                        if (GetBlockExtended(cx, cy - 1, cz).IsTransparent)
                        {
                            Vertex4f texCoords = Block.GetTextureCoords(block.GetTexture(BlockSide.Bottom));
                            mesh.AddRange(new float[] {
                                wx,      wy,      wz + 1f,    texCoords.z, texCoords.w, .7f,
                                wx,      wy,      wz,         texCoords.z, texCoords.y, .7f,
                                wx + 1f, wy,      wz,         texCoords.x, texCoords.y, .7f,
                                wx + 1f, wy,      wz + 1f,    texCoords.x, texCoords.w, .7f
                            });
                        }

                        // Top
                        if (GetBlockExtended(cx, cy + 1, cz).IsTransparent)
                        {
                            Vertex4f texCoords = Block.GetTextureCoords(block.GetTexture(BlockSide.Top));
                            mesh.AddRange(new float[] {
                                wx + 1f, wy + 1f, wz + 1f,    texCoords.z, texCoords.w, 1f,
                                wx + 1f, wy + 1f, wz,         texCoords.z, texCoords.y, 1f,
                                wx,      wy + 1f, wz,         texCoords.x, texCoords.y, 1f,
                                wx,      wy + 1f, wz + 1f,    texCoords.x, texCoords.w, 1f
                            });
                        }

                        // Left
                        if (GetBlockExtended(cx - 1, cy, cz).IsTransparent)
                        {
                            Vertex4f texCoords = Block.GetTextureCoords(block.GetTexture(BlockSide.Left));
                            mesh.AddRange(new float[] {
                                wx,      wy,      wz + 1f,    texCoords.z, texCoords.w, .8f,
                                wx,      wy + 1f, wz + 1f,    texCoords.z, texCoords.y, .8f,
                                wx,      wy + 1f, wz,         texCoords.x, texCoords.y, .8f,
                                wx,      wy,      wz,         texCoords.x, texCoords.w, .8f
                            });
                        }

                        // Right
                        if (GetBlockExtended(cx + 1, cy, cz).IsTransparent)
                        {
                            Vertex4f texCoords = Block.GetTextureCoords(block.GetTexture(BlockSide.Right));
                            mesh.AddRange(new float[] {
                                wx + 1f, wy,      wz,         texCoords.z, texCoords.w, .8f,
                                wx + 1f, wy + 1f, wz,         texCoords.z, texCoords.y, .8f,
                                wx + 1f, wy + 1f, wz + 1f,    texCoords.x, texCoords.y, .8f,
                                wx + 1f, wy,      wz + 1f,    texCoords.x, texCoords.w, .8f
                            });
                        }

                        // Back
                        if (GetBlockExtended(cx, cy, cz - 1).IsTransparent)
                        {
                            Vertex4f texCoords = Block.GetTextureCoords(block.GetTexture(BlockSide.Back));
                            mesh.AddRange(new float[] {
                                wx,      wy,      wz,         texCoords.z, texCoords.w, .7f,
                                wx,      wy + 1f, wz,         texCoords.z, texCoords.y, .7f,
                                wx + 1f, wy + 1f, wz,         texCoords.x, texCoords.y, .7f,
                                wx + 1f, wy,      wz,         texCoords.x, texCoords.w, .7f
                            });
                        }

                        // Front
                        if (GetBlockExtended(cx, cy, cz + 1).IsTransparent)
                        {
                            Vertex4f texCoords = Block.GetTextureCoords(block.GetTexture(BlockSide.Front));
                            mesh.AddRange(new float[] {
                                wx + 1f, wy,      wz + 1f,    texCoords.z, texCoords.w, .9f,
                                wx + 1f, wy + 1f, wz + 1f,    texCoords.z, texCoords.y, .9f,
                                wx,      wy + 1f, wz + 1f,    texCoords.x, texCoords.y, .9f,
                                wx,      wy,      wz + 1f,    texCoords.x, texCoords.w, .9f
                            });
                        }
                    }

            Data = mesh.ToArray();
        }
    }
}
