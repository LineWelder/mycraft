using OpenGL;
using Mycraft.Blocks;
using Mycraft.Utils;

namespace Mycraft.GUI
{
    public class Hotbar
    {
        public const int CAPACITY = 10;

        public int Selected
        {
            get => selected;
            set
            {
                selected = value;
                hotbarSelector = new GUIRectangle(
                    position + new Vertex2i((18 * value - 1) * scale, -scale),
                    new Vertex2i(22 * scale, 22 * scale)
                );
            }
        }

        public Block SelectedBlock => blocks[selected];

        public readonly Block[] blocks;

        private GUIRectangle hotbar, hotbarSelector;
        private BlockView[] blockViews;

        private Vertex2i position;
        private int scale;
        public int selected;

        public Hotbar(Vertex2i position, int scale, int selected, Block[] blocks)
        {
            this.position = position;
            this.scale = scale;
            Selected = selected;

            hotbar = new GUIRectangle(
                position,
                new Vertex2i(182 * scale, 20 * scale)
            );

            this.blocks = blocks;
            blockViews = new BlockView[10];
            for (int i = 0; i < CAPACITY; i++)
                if (!(blocks[i] is null))
                    blockViews[i] = new BlockView(
                        position + new Vertex2i((int)((3.5 + 18 * i) * scale), 3 * scale),
                        new Vertex2i(13 * scale, 14 * scale),
                        blocks[i]
                    );
        }

        public void Draw()
        {
            Resources.HotbarTexture.Bind();
            hotbar.Draw();

            Resources.HotbarSelectorTexture.Bind();
            hotbarSelector.Draw();

            Resources.BlocksTexture.Bind();
            foreach (BlockView block in blockViews)
                if (!(block is null))
                    block.Draw();
        }

        public void Dispose()
        {
            hotbar.Dispose();
            hotbarSelector.Dispose();
            foreach (BlockView block in blockViews)
                if (!(block is null))
                    block.Dispose();
        }
    }
}
