using OpenGL;
using Mycraft.Blocks;
using Mycraft.Graphics;
using Mycraft.Utils;

namespace Mycraft.GUI
{
    public class BlockView : VertexArray
    {
        private Block block;

        public BlockView(Vertex2i position, Vertex2i size, Block block)
            : base(PrimitiveType.Quads, Resources.BlockViewShader)
        {
            this.block = block;
            Resize(position, size);
        }

        public void Resize(Vertex2i position, Vertex2i size)
        {
            if (block is PlantBlock)
            {
                float texture = block.GetTexture(BlockSide.Top);

                Data = new float[] {
                    position.x + size.x, position.y,           1f, 0f, texture,  1f,
                    position.x,          position.y,           0f, 0f, texture,  1f,
                    position.x,          position.y + size.y,  0f, 1f, texture,  1f,
                    position.x + size.x, position.y + size.y,  1f, 1f, texture,  1f,
                };
            }
            else
            {
                float topTexture   = block.GetTexture(BlockSide.Top);
                float frontTexture = block.GetTexture(BlockSide.Front);
                float rightTexture = block.GetTexture(BlockSide.Right);

                float tilt = size.y / 4f;
                float padding = size.y / 32f;

                Data = new float[] {
                    // Top
                    position.x + size.x - padding, position.y + tilt,            1f, 0f, topTexture,    1f,
                    position.x + size.x * .5f,     position.y,                   0f, 0f, topTexture,    1f,
                    position.x + padding,          position.y + tilt,            0f, 1f, topTexture,    1f,
                    position.x + size.x * .5f,     position.y + tilt * 2f,       1f, 1f, topTexture,    1f,
                                                                                               
                    // Front                                                                       
                    position.x + padding,          position.y + tilt,            0f, 0f, frontTexture,  .9f,
                    position.x + padding,          position.y + size.y - tilt,   0f, 1f, frontTexture,  .9f,
                    position.x + size.x * .5f,     position.y + size.y,          1f, 1f, frontTexture,  .9f,
                    position.x + size.x * .5f,     position.y + tilt * 2f,       1f, 0f, frontTexture,  .9f,
                                                                                               
                    // Right                                                                       
                    position.x + size.x * .5f,     position.y + tilt * 2f,       0f, 0f, rightTexture,  .8f,
                    position.x + size.x * .5f,     position.y + size.y,          0f, 1f, rightTexture,  .8f,
                    position.x + size.x - padding, position.y + size.y - tilt,   1f, 1f, rightTexture,  .8f,
                    position.x + size.x - padding, position.y + tilt,            1f, 0f, rightTexture,  .8f
                };
            }
        }
    }
}
