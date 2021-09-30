using Mycraft.Graphics;
using Mycraft.Shaders;
using System;

namespace Mycraft.Utils
{
    public static class Resources
    {
        public static bool AreLoaded { get; private set; }

        public static WorldUIShader WorldUIShader { get; private set; }
        public static GUIShader GUIShader { get; private set; }
        public static GameWorldShader GameWorldShader { get; private set; }
        public static ParticleShader ParticleShader { get; private set; }
        public static VignetteShader VignetteShader { get; private set; }

        public static Texture BlocksTexture { get; private set; }
        public static Texture CrossTexture { get; private set; }
        public static Texture HotbarTexture { get; private set; }
        public static Texture HotbarSelectorTexture { get; private set; }

        public static void LoadAll()
        {
            if (AreLoaded) return;
            AreLoaded = true;

            WorldUIShader = new WorldUIShader();
            GUIShader = new GUIShader();
            GameWorldShader = new GameWorldShader();
            ParticleShader = new ParticleShader();
            VignetteShader = new VignetteShader();

            BlocksTexture = new Texture(@"resources\textures\blocks.png", 8, 8);
            CrossTexture = new Texture(@"resources\textures\cross.png");
            HotbarTexture = new Texture(@"resources\textures\hotbar.png", 20, 2);
            HotbarSelectorTexture = new Texture(@"resources\textures\hotbar_selector.png");

            GC.Collect();
        }

        public static void DisposeAll()
        {
            WorldUIShader.Dispose();
            GUIShader.Dispose();
            GameWorldShader.Dispose();
            ParticleShader.Dispose();
            VignetteShader.Dispose();

            BlocksTexture.Dispose();
            CrossTexture.Dispose();
            HotbarTexture.Dispose();
            HotbarSelectorTexture.Dispose();
        }
    }
}
