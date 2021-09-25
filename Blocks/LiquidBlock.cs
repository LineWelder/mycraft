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

        public override void EmitMesh(List<Quad> mesh, Chunk chunk, int x, int y, int z)
        {
            const float HEIGHT = 15f / 16f;

            Block topNeighbour = GetChunkBlock(chunk, x, y + 1, z);
            if (topNeighbour is LiquidBlock)
            {
                base.EmitMesh(mesh, chunk, x, y, z);
                return;
            }

            float wx = chunk.xOffset + x;
            float wz = chunk.zOffset + z;
            float wy = y;

            // Top
            {
                mesh.Add(new Quad(
                    new Vertex3f(wx + 1f, wy + HEIGHT, wz + 1f),
                    new Vertex3f(wx + 1f, wy + HEIGHT, wz),
                    new Vertex3f(wx,      wy + HEIGHT, wz),
                    new Vertex3f(wx,      wy + HEIGHT, wz + 1f),
                    GetTextureCoords(GetTexture(BlockSide.Top)),
                    1f
                ));
            }

            // Bottom
            if (HasFace(GetChunkBlock(chunk, x, y - 1, z)))
                mesh.Add(new Quad(
                    new Vertex3f(wx,      wy,          wz + 1f),
                    new Vertex3f(wx,      wy,          wz),
                    new Vertex3f(wx + 1f, wy,          wz),
                    new Vertex3f(wx + 1f, wy,          wz + 1f),
                    GetTextureCoords(GetTexture(BlockSide.Bottom)),
                    .7f
                ));

            // Left
            if (HasFace(GetChunkBlock(chunk, x - 1, y, z)))
                mesh.Add(new Quad(
                    new Vertex3f(wx,      wy,          wz + 1f),
                    new Vertex3f(wx,      wy + HEIGHT, wz + 1f),
                    new Vertex3f(wx,      wy + HEIGHT, wz),
                    new Vertex3f(wx,      wy,          wz),
                    GetTextureCoords(GetTexture(BlockSide.Left)),
                    .8f
                ));

            // Right
            if (HasFace(GetChunkBlock(chunk, x + 1, y, z)))
                mesh.Add(new Quad(
                    new Vertex3f(wx + 1f, wy,          wz),
                    new Vertex3f(wx + 1f, wy + HEIGHT, wz),
                    new Vertex3f(wx + 1f, wy + HEIGHT, wz + 1f),
                    new Vertex3f(wx + 1f, wy,          wz + 1f),
                    GetTextureCoords(GetTexture(BlockSide.Right)),
                    .8f
                ));

            // Back
            if (HasFace(GetChunkBlock(chunk, x, y, z - 1)))
                mesh.Add(new Quad(
                    new Vertex3f(wx,      wy,          wz),
                    new Vertex3f(wx,      wy + HEIGHT, wz),
                    new Vertex3f(wx + 1f, wy + HEIGHT, wz),
                    new Vertex3f(wx + 1f, wy,          wz),
                    GetTextureCoords(GetTexture(BlockSide.Back)),
                    .7f
                ));

            // Front
            if (HasFace(GetChunkBlock(chunk, x, y, z + 1)))
                mesh.Add(new Quad(
                    new Vertex3f(wx + 1f, wy,          wz + 1f),
                    new Vertex3f(wx + 1f, wy + HEIGHT, wz + 1f),
                    new Vertex3f(wx,      wy + HEIGHT, wz + 1f),
                    new Vertex3f(wx,      wy,          wz + 1f),
                    GetTextureCoords(GetTexture(BlockSide.Front)),
                    .9f
                ));
        }
    }
}
