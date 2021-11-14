using System;
using System.Threading.Tasks;
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
            public bool needsUpdate;

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
        private bool needsSolidVertexRefresh, needsTransparentVertexRefresh;

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
            Gl.ActiveTexture(TextureUnit.Texture1);
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

        public Task UpdateLightAsync()
        {
            if (!needsLightRecalculation)
                return Task.CompletedTask;

            needsLightRecalculation = false;
            return Task.Run(lightMap.BuildDataMap);
        }

        public Task UpdateMeshAsync()
        {
            if (!needsUpdate)
                return Task.CompletedTask;

            needsUpdate = false;

            return Task.Run(async () =>
            {
                needsLightRecalculation = true;
                Task lightUpdating = UpdateLightAsync();

                MeshBuildingContext context = new MeshBuildingContext(this);

                for (int cx = 0; cx < SIZE; cx++)
                    for (int cz = 0; cz < SIZE; cz++)
                        for (int cy = 0; cy < HEIGHT; cy++)
                            blocks[cx, cy, cz].EmitMesh(context, cx, cy, cz);

                solidMesh.quads       = context.solidQuads.ToArray();
                doubleSidedMesh.quads = context.doubleSidedQuads.ToArray();
                transparentMesh.quads = context.transparentQuads.ToArray();

                needsSolidVertexRefresh = true;
                needsTransparentGeometrySort = true;

                await lightUpdating;
            });
        }

        public Task EnsureTransparentGeometrySortedAsync()
        {
            if (!needsTransparentGeometrySort || transparentMesh.quads is null)
                return Task.CompletedTask;

            needsTransparentGeometrySort = false;
            return Task.Run(() =>
            {
                Vertex3f offset = new Vertex3f(xOffset, 0f, zOffset) - world.ObservingCamera.Position;
                Array.Sort(
                    transparentMesh.quads,
                    (Quad a, Quad b) => (b.Center + offset).ModuleSquared()
                             .CompareTo((a.Center + offset).ModuleSquared())
                );

                needsTransparentVertexRefresh = true;
            });
        }

        public bool RefreshVertexData()
        {
            bool refreshed = false;

            if (needsSolidVertexRefresh)
            {
                 needsSolidVertexRefresh = false;
                 
                 solidMesh.UpdateVertexData();
                 doubleSidedMesh.UpdateVertexData();
                 transparentMesh.UpdateVertexData();
                 
                 refreshed = true;
            }

            if (needsTransparentVertexRefresh)
            {
                needsTransparentVertexRefresh = false;
                transparentMesh.UpdateVertexData();

                refreshed = true;
            }

            if (lightMap.UpdateIfNeeded())
                refreshed = true;

            return refreshed;
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
