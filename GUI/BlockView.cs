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
            : base(PrimitiveType.Quads, Resources.GUIShader)
        {
            this.block = block;
            Resize(position, size);
        }

        public void Resize(Vertex2i position, Vertex2i size)
        {
            Vertex4f topTexture = Block.GetTextureCoords(block.GetTexture(BlockSide.Top));
            Vertex4f frontTexture = Block.GetTextureCoords(block.GetTexture(BlockSide.Front));
            Vertex4f rightTexture = Block.GetTextureCoords(block.GetTexture(BlockSide.Right));

            float tilt = size.y / 4f;

            Data = new float[] {
                // Top
                position.x + size.x,       position.y + tilt,            topTexture.z,   topTexture.y,
                position.x + size.x * .5f, position.y,                   topTexture.x,   topTexture.y,
                position.x,                position.y + tilt,            topTexture.x,   topTexture.w,
                position.x + size.x * .5f, position.y + tilt * 2f,       topTexture.z,   topTexture.w,

                // Front
                position.x,                position.y + tilt,            frontTexture.x, frontTexture.y,
                position.x,                position.y + size.y - tilt,   frontTexture.x, frontTexture.w,
                position.x + size.x * .5f, position.y + size.y,          frontTexture.z, frontTexture.w,
                position.x + size.x * .5f, position.y + tilt * 2f,       frontTexture.z, frontTexture.y,

                // Right
                position.x + size.x * .5f, position.y + tilt * 2f,       rightTexture.x, rightTexture.y,
                position.x + size.x * .5f, position.y + size.y,          rightTexture.x, rightTexture.w,
                position.x + size.x,       position.y + size.y - tilt,   rightTexture.z, rightTexture.w,
                position.x + size.x,       position.y + tilt,            rightTexture.z, rightTexture.y
            };
        }
    }
}
