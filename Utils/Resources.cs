using Mycraft.Graphics;
using System;

namespace Mycraft.Utils
{
    public static class Resources
    {
        public static ColoredShader ColoredShader { get; private set; }
        public static GameWorldShader TexturedShader { get; private set; }

        public static Texture TestTexture { get; private set; }

        public static void LoadAll()
        {
            ColoredShader = new ColoredShader();
            TexturedShader = new GameWorldShader();

            TestTexture = new Texture(@"resources\textures\test_texture.png");

            GC.Collect();
        }

        public static void DisposeAll()
        {
            ColoredShader.Dispose();
            TexturedShader.Dispose();
            TestTexture.Dispose();
        }
    }
}
