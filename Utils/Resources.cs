using System;
using System.Linq;

using Mycraft.Graphics;
using Mycraft.Shaders;

namespace Mycraft.Utils
{
    public static class Resources
    {
        public static bool AreLoaded { get; private set; }

        public static WorldUIShader WorldUIShader { get; private set; }
        public static GUIShader GUIShader { get; private set; }
        public static BlockViewShader BlockViewShader { get; private set; }
        public static GameWorldShader GameWorldShader { get; private set; }
        public static ParticleShader ParticleShader { get; private set; }
        public static OverlayShader OverlayShader { get; private set; }
        public static SkyShader SkyShader { get; private set; }

        public static TextureArray BlocksTexture { get; private set; }
        public static Texture CrossTexture { get; private set; }
        public static Texture HotbarTexture { get; private set; }
        public static Texture HotbarSelectorTexture { get; private set; }

        public static void LoadAll()
        {
            if (AreLoaded) return;
            AreLoaded = true;

            WorldUIShader   = new WorldUIShader();
            GUIShader       = new GUIShader();
            BlockViewShader = new BlockViewShader();
            GameWorldShader = new GameWorldShader();
            ParticleShader  = new ParticleShader();
            OverlayShader   = new OverlayShader();
            SkyShader       = new SkyShader();

            BlocksTexture         = new TextureArray(@"resources\textures\blocks.png", 4, 4);
            CrossTexture          = new Texture(@"resources\textures\cross.png");
            HotbarTexture         = new Texture(@"resources\textures\hotbar.png", 20, 2);
            HotbarSelectorTexture = new Texture(@"resources\textures\hotbar_selector.png");

            GC.Collect();
        }

        public static void DisposeAll()
        {
            AreLoaded = false;

            var disposableResources =
                from property in typeof(Resources).GetProperties()
                where property.PropertyType.IsSubclassOf(typeof(IDisposable))
                select (IDisposable)property.GetValue(null);

            foreach (IDisposable resource in disposableResources)
                resource.Dispose();
        }
    }
}
