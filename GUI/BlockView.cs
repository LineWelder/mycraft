using Mycraft.Blocks;
using Mycraft.Graphics;
using OpenGL;

namespace Mycraft.GUI
{
    public class BlockView : VertexArray
    {
        public BlockView(Vertex2i position, Vertex2i size, Block block)
            : base(PrimitiveType.Quads, new int[] { 2, 2 })
        {
            Vertex4f topTexture = Block.GetTextureCoords(block.GetTexture(BlockSide.Top));
            Vertex4f frontTexture = Block.GetTextureCoords(block.GetTexture(BlockSide.Front));
            Vertex4f rightTexture = Block.GetTextureCoords(block.GetTexture(BlockSide.Right));

            float tilt = size.y / 4f;

            Data = new float[] {
                // Top
                position.x + size.x,       position.y + tilt,            topTexture.x,   topTexture.y,
                position.x + size.x * .5f, position.y,                   topTexture.z,   topTexture.y,
                position.x,                position.y + tilt,            topTexture.z,   topTexture.w,
                position.x + size.x * .5f, position.y + tilt * 2f,       topTexture.x,   topTexture.w,

                // Front
                position.x,                position.y + tilt,            frontTexture.z, frontTexture.y,
                position.x,                position.y + size.y - tilt,   frontTexture.z, frontTexture.w,
                position.x + size.x * .5f, position.y + size.y,          frontTexture.x, frontTexture.w,
                position.x + size.x * .5f, position.y + tilt * 2f,       frontTexture.x, frontTexture.y,

                // Right
                position.x + size.x * .5f, position.y + tilt * 2f,       rightTexture.z, rightTexture.y,
                position.x + size.x * .5f, position.y + size.y,          rightTexture.z, rightTexture.w,
                position.x + size.x,       position.y + size.y - tilt,   rightTexture.x, rightTexture.w,
                position.x + size.x,       position.y + tilt,            rightTexture.x, rightTexture.y
            };
        }
    }
}
