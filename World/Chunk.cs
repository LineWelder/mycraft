using System;
using OpenGL;

using Mycraft.Blocks;
using Mycraft.Graphics;
using Mycraft.Utils;

namespace Mycraft.World
{
    public class Chunk : IDisposable
    {
        private class WorldGeometry : VertexArray
        {
            public Quad[] quads;

            public WorldGeometry()
                : base(PrimitiveType.Quads, Resources.GameWorldShader) { }

            public unsafe void UpdateVertexData()
            {
                fixed (Quad* ptr = quads)
                {
                    LoadData(new IntPtr(ptr), quads.Length * sizeof(Quad) / sizeof(float));
                }
            }
        }

        public const int SIZE = 16;
        public const int HEIGHT = 256;

        public bool isLoaded;
        public bool needsUpdate;
        
        public readonly Block[,,] blocks;
        public readonly int[,] groundLevel;

        public readonly GameWorld world;
        public readonly int xOffset, zOffset;

        public bool needsTransparentGeometrySort;
        private readonly WorldGeometry solidMesh, doubleSidedMesh, transparentMesh;

        public bool needsLightRecalculation;
        private readonly LightMap lightMap;

        public Chunk(GameWorld world, int x, int z)
        {
            blocks = new Block[SIZE, HEIGHT, SIZE];
            groundLevel = new int[SIZE, SIZE];

            this.world = world;
            xOffset = x * SIZE;
            zOffset = z * SIZE;

            solidMesh = new WorldGeometry();
            doubleSidedMesh = new WorldGeometry();
            transparentMesh = new WorldGeometry();

            lightMap = new LightMap(this);
        }

        public void Draw()
        {
            lightMap.Bind();

            Gl.ActiveTexture(TextureUnit.Texture0);
            Resources.BlocksTexture.Bind();

            Gl.Enable(EnableCap.CullFace);
            solidMesh.Draw();

            Gl.Disable(EnableCap.CullFace);
            doubleSidedMesh.Draw();

            Gl.Enable(EnableCap.Blend);
            transparentMesh.Draw();
            Gl.Disable(EnableCap.Blend);
        }

        public bool UpdateLight()
        {
            if (!needsLightRecalculation)
                return false;

            needsLightRecalculation = false;
            lightMap.BuildDataMap();
            lightMap.UpdateIfNeeded();

            return true;
        }

        public bool UpdateMesh()
        {
            if (!needsUpdate)
                return false;

            needsUpdate = false;
            needsLightRecalculation = true;
            UpdateLight();

            MeshBuildingContext context = new MeshBuildingContext(this);

            for (int cx = 0; cx < SIZE; cx++)
                for (int cz = 0; cz < SIZE; cz++)
                    for (int cy = 0; cy < HEIGHT; cy++)
                        blocks[cx, cy, cz].EmitMesh(context, cx, cy, cz);

            solidMesh.quads       = context.solidQuads.ToArray();
            doubleSidedMesh.quads = context.doubleSidedQuads.ToArray();
            transparentMesh.quads = context.transparentQuads.ToArray();

            needsTransparentGeometrySort = true;
            EnsureTransparentGeometrySorted();

            solidMesh.UpdateVertexData();
            doubleSidedMesh.UpdateVertexData();

            return true;
        }

        public bool EnsureTransparentGeometrySorted()
        {
            if (!needsTransparentGeometrySort
             || transparentMesh.quads is null)
                return false;

            needsTransparentGeometrySort = false;
            Vertex3f offset = new Vertex3f(xOffset, 0f, zOffset) - world.ObservingCamera.Position;
            Array.Sort(
                transparentMesh.quads,
                (Quad a, Quad b) => (b.Center + offset).ModuleSquared()
                         .CompareTo((a.Center + offset).ModuleSquared())
            );

            transparentMesh.UpdateVertexData();
            return true;
        }

        public void Dispose()
        {
            solidMesh.Dispose();
            doubleSidedMesh.Dispose();
            transparentMesh.Dispose();
            lightMap.Dispose();
        }
    }
}
