using Mycraft.Graphics;
using System;

namespace Mycraft.Utils
{
    public static class Resources
    {
        public static WorldUIShader WorldUIShader { get; private set; }
        public static GUIShader GUIShader { get; private set; }
        public static GameWorldShader GameWorldShader { get; private set; }
        public static ParticleShader ParticleShader { get; private set; }

        public static Texture BlocksTexture { get; private set; }
        public static Texture CrossTexture { get; private set; }
        public static Texture HotbarTexture { get; private set; }
        public static Texture HotbarSelectorTexture { get; private set; }

        public static void LoadAll()
        {
            WorldUIShader = new WorldUIShader();
            GUIShader = new GUIShader();
            GameWorldShader = new GameWorldShader();
            ParticleShader = new ParticleShader();

            BlocksTexture = new Texture(@"resources\textures\blocks.png");
            CrossTexture = new Texture(@"resources\textures\cross.png");
            HotbarTexture = new Texture(@"resources\textures\hotbar.png");
            HotbarSelectorTexture = new Texture(@"resources\textures\hotbar_selector.png");

            GC.Collect();
        }

        public static void DisposeAll()
        {
            WorldUIShader.Dispose();
            GUIShader.Dispose();
            GameWorldShader.Dispose();
            ParticleShader.Dispose();

            BlocksTexture.Dispose();
            CrossTexture.Dispose();
            HotbarTexture.Dispose();
            HotbarSelectorTexture.Dispose();
        }
    }
}
