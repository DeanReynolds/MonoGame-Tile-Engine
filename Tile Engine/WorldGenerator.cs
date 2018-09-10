using System;

namespace Tile_Engine
{
    public class WorldGenerator
    {
        public static World Generate(int tilesWidth, int tilesHeight)
        {
            World world = new World(tilesWidth, tilesHeight);
            tilesWidth = world.TilesWidth;
            tilesHeight = world.TilesHeight;
            Random random = new Random();
            int minSurfaceY = (tilesHeight / 5);
            int maxSurfaceY = (minSurfaceY + minSurfaceY);
            for (int tileX = 0; tileX < tilesWidth; tileX++)
                for (int tileY = 0; tileY < tilesHeight; tileY++)
                {
                    int chunkX = (tileX >> Chunk.Bits);
                    int chunkY = (tileY >> Chunk.Bits);
                    int chunkTileX = (tileX & Chunk.Modulo);
                    int chunkTileY = (tileY & Chunk.Modulo);
                    world.Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Type = (Tile.Types)random.Next(Enum.GetValues(typeof(Tile.Types)).Length);
                }
            return world;
        }
    }
}