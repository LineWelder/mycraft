using Mycraft.World;
using OpenGL;
using System.Collections.Generic;

namespace Mycraft.Blocks
{
    public class LiquidBlock : Block
    {
        public override bool IsTransparent => true;
        public override bool HasCollider => false;
        public override bool IsVisible => true;

        public LiquidBlock(int textureId)
            : base(textureId) { }

        public override void EmitVertices(List<float> mesh, Chunk chunk, int x, int y, int z)
        {
            const float HEIGHT = 15f / 16f;

            Block topNeighbour = GetChunkBlock(chunk, x, y + 1, z);
            if (topNeighbour is LiquidBlock)
            {
                base.EmitVertices(mesh, chunk, x, y, z);
                return;
            }

            float wx = chunk.xOffset + x;
            float wz = chunk.zOffset + z;
            float wy = y;

            // Top
            {
                Vertex4f texCoords = GetTextureCoords(GetTexture(BlockSide.Top));
                mesh.AddRange(new float[] {
                    wx + 1f, wy + HEIGHT, wz + 1f,    texCoords.z, texCoords.w, 1f,
                    wx + 1f, wy + HEIGHT, wz,         texCoords.z, texCoords.y, 1f,
                    wx,      wy + HEIGHT, wz,         texCoords.x, texCoords.y, 1f,
                    wx,      wy + HEIGHT, wz + 1f,    texCoords.x, texCoords.w, 1f
                });
            }

            // Bottom
            if (HasFace(GetChunkBlock(chunk, x, y - 1, z)))
            {
                Vertex4f texCoords = GetTextureCoords(GetTexture(BlockSide.Bottom));
                mesh.AddRange(new float[] {
                    wx,      wy,          wz + 1f,    texCoords.z, texCoords.w, .7f,
                    wx,      wy,          wz,         texCoords.z, texCoords.y, .7f,
                    wx + 1f, wy,          wz,         texCoords.x, texCoords.y, .7f,
                    wx + 1f, wy,          wz + 1f,    texCoords.x, texCoords.w, .7f
                });
            }

            // Left
            if (HasFace(GetChunkBlock(chunk, x - 1, y, z)))
            {
                Vertex4f texCoords = GetTextureCoords(GetTexture(BlockSide.Left));
                mesh.AddRange(new float[] {
                    wx,      wy,          wz + 1f,    texCoords.z, texCoords.w, .8f,
                    wx,      wy + HEIGHT, wz + 1f,    texCoords.z, texCoords.y, .8f,
                    wx,      wy + HEIGHT, wz,         texCoords.x, texCoords.y, .8f,
                    wx,      wy,          wz,         texCoords.x, texCoords.w, .8f
                });
            }

            // Right
            if (HasFace(GetChunkBlock(chunk, x + 1, y, z)))
            {
                Vertex4f texCoords = GetTextureCoords(GetTexture(BlockSide.Right));
                mesh.AddRange(new float[] {
                    wx + 1f, wy,          wz,         texCoords.z, texCoords.w, .8f,
                    wx + 1f, wy + HEIGHT, wz,         texCoords.z, texCoords.y, .8f,
                    wx + 1f, wy + HEIGHT, wz + 1f,    texCoords.x, texCoords.y, .8f,
                    wx + 1f, wy,          wz + 1f,    texCoords.x, texCoords.w, .8f
                });
            }

            // Back
            if (HasFace(GetChunkBlock(chunk, x, y, z - 1)))
            {
                Vertex4f texCoords = GetTextureCoords(GetTexture(BlockSide.Back));
                mesh.AddRange(new float[] {
                    wx,      wy,          wz,         texCoords.z, texCoords.w, .7f,
                    wx,      wy + HEIGHT, wz,         texCoords.z, texCoords.y, .7f,
                    wx + 1f, wy + HEIGHT, wz,         texCoords.x, texCoords.y, .7f,
                    wx + 1f, wy,          wz,         texCoords.x, texCoords.w, .7f
                });
            }

            // Front
            if (HasFace(GetChunkBlock(chunk, x, y, z + 1)))
            {
                Vertex4f texCoords = GetTextureCoords(GetTexture(BlockSide.Front));
                mesh.AddRange(new float[] {
                    wx + 1f, wy,          wz + 1f,    texCoords.z, texCoords.w, .9f,
                    wx + 1f, wy + HEIGHT, wz + 1f,    texCoords.z, texCoords.y, .9f,
                    wx,      wy + HEIGHT, wz + 1f,    texCoords.x, texCoords.y, .9f,
                    wx,      wy,          wz + 1f,    texCoords.x, texCoords.w, .9f
                });
            }
        }
    }
}
