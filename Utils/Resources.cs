using Mycraft.Graphics;
using System;

namespace Mycraft.Utils
{
    public static class Resources
    {
        public static WorldUIShader WorldUIShader { get; private set; }
        public static GUIShader GUIShader { get; private set; }
        public static GameWorldShader GameWorldShader { get; private set; }

        public static Texture BlocksTexture { get; private set; }
        public static Texture CrossTexture { get; private set; }

        public static void LoadAll()
        {
            WorldUIShader = new WorldUIShader();
            GUIShader = new GUIShader();
            GameWorldShader = new GameWorldShader();

            BlocksTexture = new Texture(@"resources\textures\blocks.png");
            CrossTexture = new Texture(@"resources\textures\cross.png");

            GC.Collect();
        }

        public static void DisposeAll()
        {
            WorldUIShader.Dispose();
            GUIShader.Dispose();
            GameWorldShader.Dispose();
            BlocksTexture.Dispose();
        }
    }
}
