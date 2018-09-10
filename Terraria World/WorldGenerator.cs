using Microsoft.Xna.Framework;
using System;

namespace Terraria_World
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
            int surfaceY = random.Next(minSurfaceY, (maxSurfaceY + 1));
            int surfaceDepth = 8;
            int undergroundY = (maxSurfaceY + 32);
            for (int tileX = 0; tileX < tilesWidth; tileX++)
            {
                if (tileX == (tilesWidth / 2))
                    world.Spawn = new Point(tileX, (surfaceY - 1));
                for (int tileY = surfaceY; tileY < tilesHeight; tileY++)
                {
                    int chunkX = (tileX >> Chunk.Bits);
                    int chunkY = (tileY >> Chunk.Bits);
                    int chunkTileX = (tileX & Chunk.Modulo);
                    int chunkTileY = (tileY & Chunk.Modulo);
                    if (tileY < (surfaceY + surfaceDepth))
                    {
                        if (tileY == surfaceY)
                            world.Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Fore = Tile.Fores.Grass;
                        else if (world.Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Fore != Tile.Fores.Grass)
                        {
                            world.Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Fore = Tile.Fores.Dirt;
                            world.Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Back = Tile.Backs.Dirt;
                        }
                    }
                    else// if (tileY <= undergroundY)
                    {
                        world.Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Fore = Tile.Fores.Stone;
                        world.Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Back = Tile.Backs.Stone;
                    }
                }
                if (random.Next(3) == 1)
                {
                    int newSurfaceY = Math.Min(maxSurfaceY, Math.Max(minSurfaceY, (surfaceY + random.Next(-1, 2))));
                    if (newSurfaceY < surfaceY)
                    {
                        if (tileX < (tilesWidth - 1))
                        {
                            int chunkX = ((tileX + 1) >> Chunk.Bits);
                            int chunkY = (surfaceY >> Chunk.Bits);
                            int chunkTileX = ((tileX + 1) & Chunk.Modulo);
                            int chunkTileY = (surfaceY & Chunk.Modulo);
                            world.Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Fore = Tile.Fores.Grass;
                            world.Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Back = Tile.Backs.None;
                        }
                    }
                    else if (newSurfaceY > surfaceY)
                    {
                        int chunkX = (tileX >> Chunk.Bits);
                        int chunkY = ((surfaceY + 1) >> Chunk.Bits);
                        int chunkTileX = (tileX & Chunk.Modulo);
                        int chunkTileY = ((surfaceY + 1) & Chunk.Modulo);
                        world.Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Fore = Tile.Fores.Grass;
                        world.Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Back = Tile.Backs.None;
                    }
                    surfaceY = newSurfaceY;
                }
            }
            for (int tileX = 0; tileX < tilesWidth; tileX++)
                for (int tileY = 0; tileY < tilesHeight; tileY++)
                {
                    world.UpdateForeUV(tileX, tileY, true, false, false);
                    world.UpdateBackUV(tileX, tileY, true, false, false);
                }
            return world;
        }
    }
}