using Mycraft.Graphics;
using System;

namespace Mycraft.Utils
{
    public static class Resources
    {
        public static bool AreLoaded { get; private set; }

        public static WorldUIShader WorldUIShader { get; private set; }
        public static GUIShader GUIShader { get; private set; }
        public static GameWorldShader GameWorldShader { get; private set; }

        public static Texture TestTexture { get; private set; }
        public static Texture CrossTexture { get; private set; }

        public static void LoadAll()
        {
            if (AreLoaded) return;
            AreLoaded = true;

            WorldUIShader = new WorldUIShader();
            GUIShader = new GUIShader();
            GameWorldShader = new GameWorldShader();

            TestTexture = new Texture(@"resources\textures\test.png");
            CrossTexture = new Texture(@"resources\textures\cross.png");

            GC.Collect();
        }

        public static void DisposeAll()
        {
            WorldUIShader.Dispose();
            GUIShader.Dispose();
            GameWorldShader.Dispose();
            TestTexture.Dispose();
        }
    }
}
