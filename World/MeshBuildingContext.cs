using System.Collections.Generic;

namespace Mycraft.World
{
    public class MeshBuildingContext
    {
        public readonly Chunk chunk;
        public readonly List<Quad> solidQuads, doubleSidedQuads, transparentQuads;

        public MeshBuildingContext(Chunk chunk)
        {
            this.chunk       = chunk;
            solidQuads       = new List<Quad>();
            doubleSidedQuads = new List<Quad>();
            transparentQuads = new List<Quad>();
        }

        public void AddSolidQuad(Quad quad)
        {
            solidQuads.Add(quad);
        }

        public void AddDoubleSidedQuad(Quad quad)
        {
            doubleSidedQuads.Add(quad);
        }

        public void AddTransparentQuad(Quad quad)
        {
            transparentQuads.Add(quad);
        }

        
    }
}
