namespace Mycraft.World.Generation
{
    public interface IWorldGenerator
    {
        void GenerateChunk(GameWorld world, Chunk chunk);
    }
}
