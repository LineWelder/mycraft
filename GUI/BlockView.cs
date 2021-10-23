﻿using OpenGL;
using Mycraft.Blocks;
using Mycraft.Graphics;
using Mycraft.Utils;

namespace Mycraft.GUI
{
    public class BlockView : VertexArray
    {
        private Block block;

        public BlockView(Vertex2i position, Vertex2i size, Block block)
            : base(PrimitiveType.Quads, Resources.GUIShader)
        {
            this.block = block;
            Resize(position, size);
        }

        public void Resize(Vertex2i position, Vertex2i size)
        {
            float topTexture   = block.GetTexture(BlockSide.Top);
            float frontTexture = block.GetTexture(BlockSide.Front);
            float rightTexture = block.GetTexture(BlockSide.Right);

            float tilt = size.y / 4f;

            Data = new float[] {
                // Top
                position.x + size.x,       position.y + tilt,            1f, 0f,
                position.x + size.x * .5f, position.y,                   0f, 0f,
                position.x,                position.y + tilt,            0f, 1f,
                position.x + size.x * .5f, position.y + tilt * 2f,       1f, 1f,

                // Front
                position.x,                position.y + tilt,            0f, 0f,
                position.x,                position.y + size.y - tilt,   0f, 1f,
                position.x + size.x * .5f, position.y + size.y,          1f, 1f,
                position.x + size.x * .5f, position.y + tilt * 2f,       1f, 0f,

                // Right
                position.x + size.x * .5f, position.y + tilt * 2f,       0f, 0f,
                position.x + size.x * .5f, position.y + size.y,          0f, 1f,
                position.x + size.x,       position.y + size.y - tilt,   1f, 1f,
                position.x + size.x,       position.y + tilt,            1f, 0f
            };
        }
    }
}
