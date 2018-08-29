﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Tile_Engine
{
    public class World
    {
        public readonly Chunk[,] Chunks;
        public readonly int ChunksWidth;
        public readonly int ChunksHeight;
        public readonly int ChunksLastIndexX;
        public readonly int ChunksLastIndexY;
        public readonly int TilesWidth;
        public readonly int TilesHeight;

        public RenderTarget2D Texture { get; private set; }
        public int BakedChunksWidth { get; private set; }
        public int BakedChunksHeight { get; private set; }
        public int RawChunksMinX { get; private set; }
        public int RawChunksMaxX { get; private set; }
        public int RawChunksMinY { get; private set; }
        public int RawChunksMaxY { get; private set; }
        public int ChunksMinX { get; private set; }
        public int ChunksMaxX { get; private set; }
        public int ChunksMinY { get; private set; }
        public int ChunksMaxY { get; private set; }
        public int OldChunksMinX { get; private set; }
        public int OldChunksMaxX { get; private set; }
        public int OldChunksMinY { get; private set; }
        public int OldChunksMaxY { get; private set; }
        public float CameraX { get; private set; }
        public float CameraY { get; private set; }
        public float DrawOffsetX { get; private set; }
        public float DrawOffsetY { get; private set; }
        public Point Spawn { get; private set; }

        public World(int tilesWidth, int tilesHeight)
        {
            if ((tilesWidth <= 0) || (tilesHeight <= 0) || (tilesWidth > ushort.MaxValue) || (tilesHeight > ushort.MaxValue))
                throw new ArgumentOutOfRangeException(string.Format("World width and height must be between 0 and {0}", (ushort.MaxValue + 1)));
            int chunksWidth;
            if ((tilesWidth >= Chunk.Size) && ((tilesWidth % Chunk.Size) == 0))
                chunksWidth = (tilesWidth >> Chunk.Bits);
            else
                chunksWidth = ((tilesWidth >> Chunk.Bits) + 1);
            TilesWidth = (chunksWidth * Chunk.Size);
            int chunksHeight;
            if ((tilesHeight >= Chunk.Size) && ((tilesHeight % Chunk.Size) == 0))
                chunksHeight = (tilesHeight >> Chunk.Bits);
            else
                chunksHeight = ((tilesHeight >> Chunk.Bits) + 1);
            TilesHeight = (chunksHeight * Chunk.Size);
            Chunks = new Chunk[(ChunksWidth = chunksWidth), (ChunksHeight = chunksHeight)];
            ChunksLastIndexX = (ChunksWidth - 1);
            ChunksLastIndexY = (ChunksHeight - 1);
            for (int x = 0; x < ChunksWidth; x++)
                for (int y = 0; y < ChunksHeight; y++)
                    Chunks[x, y] = new Chunk() { Tiles = new Tile[Tile.Size, Tile.Size] };
            BakedChunksWidth = ((int)Math.Ceiling(Screen.VirtualWidth / (float)Chunk.Size) + Chunk.TwoBufferX);
            BakedChunksHeight = ((int)Math.Ceiling(Screen.VirtualHeight / (float)Chunk.Size) + Chunk.TwoBufferY);
            int textureWidth = (BakedChunksWidth * Chunk.Size * Tile.Size);
            int textureHeight = (BakedChunksHeight * Chunk.Size * Tile.Size);
            Texture = new RenderTarget2D(Program.Game.GraphicsDevice, textureWidth, textureHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            //Console.WriteLine(string.Format("World size (in tiles): {0}x{1}", tilesWidth, tilesHeight));
            //Console.WriteLine(string.Format("World size (in chunks): {0}x{1}", chunksWidth, chunksHeight));
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(Texture, new Vector2(DrawOffsetX, DrawOffsetY), Color.White);
        }

        public void Bake(float cameraX, float cameraY, float screenWidthOver2, float screenHeightOver2)
        {
            float cameraOffsetX = (cameraX - screenWidthOver2);
            int minChunkX = (int)((cameraOffsetX / Tile.Size) / Chunk.Size);
            CameraX = cameraX;
            DrawOffsetX = (((minChunkX * Chunk.TextureSize) - cameraOffsetX) - Chunk.BufferXTextureSize);
            RawChunksMinX = (minChunkX - Chunk.BufferX);
            RawChunksMaxX = (((int)((cameraX + screenWidthOver2) / Tile.Size) >> Chunk.Bits) + 1 + Chunk.BufferX);
            ChunksMinX = Math.Max(0, RawChunksMinX);
            ChunksMaxX = Math.Min(ChunksLastIndexX, RawChunksMaxX);
            float cameraOffsetY = (cameraY - screenHeightOver2);
            int minChunkY = (int)((cameraOffsetY / Tile.Size) / Chunk.Size);
            CameraY = cameraY;
            DrawOffsetY = (((minChunkY * Chunk.TextureSize) - cameraOffsetY) - Chunk.BufferYTextureSize);
            RawChunksMinY = (minChunkY - Chunk.BufferY);
            RawChunksMaxY = (((int)((cameraY + screenHeightOver2) / Tile.Size) >> Chunk.Bits) + 1 + Chunk.BufferY);
            ChunksMinY = Math.Max(0, RawChunksMinY);
            ChunksMaxY = Math.Min(ChunksLastIndexY, RawChunksMaxY);
            //Console.WriteLine(string.Format("Chunks X: {0}-{1}, Y: {2}-{3} | Offsets: {4}, {5}", RawChunksMinX, ChunksMinX, ChunksMinY, ChunksMaxY, DrawOffsetX, DrawOffsetY));
            if ((RawChunksMinX != OldChunksMinX) || (RawChunksMaxX != OldChunksMaxX) || (RawChunksMinY != OldChunksMinY) || (RawChunksMaxY != OldChunksMaxY))
            {
                Profiler.Start("Chunks/World Bake");
                HashSet<Point> chunksToUnload = new HashSet<Point>();
                for (int x = OldChunksMinX; x < OldChunksMaxX; x++)
                    if ((x >= 0) && (x < ChunksWidth))
                        for (int y = OldChunksMinY; y < OldChunksMaxY; y++)
                            if ((y >= 0) && (y < ChunksHeight))
                                if (Chunks[x, y].Texture != null)
                                    chunksToUnload.Add(new Point(x, y));
                for (int x = RawChunksMinX; x < RawChunksMaxX; x++)
                    if ((x >= 0) && (x < ChunksWidth))
                        for (int y = RawChunksMinY; y < RawChunksMaxY; y++)
                            if ((y >= 0) && (y < ChunksHeight))
                            {
                                chunksToUnload.Remove(new Point(x, y));
                                if (Chunks[x, y].Texture == null)
                                    Chunks[x, y].Bake();
                            }
                foreach (Point point in chunksToUnload)
                    Chunks[point.X, point.Y].Dispose();
                SpriteBatch spriteBatch = Program.Game.Services.GetService<SpriteBatch>();
                Program.Game.GraphicsDevice.SetRenderTarget(Texture);
                Program.Game.GraphicsDevice.Clear(Color.TransparentBlack);
                spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
                int kReset = ((ChunksMinY - RawChunksMinY) * Chunk.TextureSize);
                for (int x = RawChunksMinX, j = ((ChunksMinX - RawChunksMinX) * Chunk.TextureSize), k = kReset; x < RawChunksMaxX; x++, k = kReset)
                    if ((x >= 0) && (x < ChunksWidth))
                    {
                        for (int y = RawChunksMinY; y < RawChunksMaxY; y++)
                            if ((y >= 0) && (y < ChunksHeight))
                            {
                                spriteBatch.Draw(Chunks[x, y].Texture, new Rectangle(j, k, Chunk.TextureSize, Chunk.TextureSize), Color.White);
                                k += Chunk.TextureSize;
                            }
                        j += Chunk.TextureSize;
                    }
                spriteBatch.End();
                Profiler.Stop("Chunks/World Bake");
                OldChunksMinX = RawChunksMinX;
                OldChunksMaxX = RawChunksMaxX;
                OldChunksMinY = RawChunksMinY;
                OldChunksMaxY = RawChunksMaxY;
                Program.Game.GraphicsDevice.SetRenderTarget(null);
            }
        }

        public Tile GetTile(int tileX, int tileY)
        {
            int chunkX = (tileX >> Chunk.Bits);
            int chunkY = (tileY >> Chunk.Bits);
            int chunkTileX = (tileX & Chunk.Modulo);
            int chunkTileY = (tileY & Chunk.Modulo);
            return Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY];
        }

        public void SetTileFore(int tileX, int tileY, Tile.Fores fore)
        {
            int chunkX = (tileX >> Chunk.Bits);
            int chunkY = (tileY >> Chunk.Bits);
            int chunkTileX = (tileX & Chunk.Modulo);
            int chunkTileY = (tileY & Chunk.Modulo);
            Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Fore = fore;
        }

        public void SetTileBack(int tileX, int tileY, Tile.Backs back)
        {
            int chunkX = (tileX >> Chunk.Bits);
            int chunkY = (tileY >> Chunk.Bits);
            int chunkTileX = (tileX & Chunk.Modulo);
            int chunkTileY = (tileY & Chunk.Modulo);
            Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Back = back;
        }

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
                        {
                            world.Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Fore = Tile.Fores.Dirt;
                            world.Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Back = Tile.Backs.Dirt;
                        }
                        else
                        {
                            world.Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Fore = Tile.Fores.Dirt;
                            world.Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Back = Tile.Backs.Dirt;
                        }
                    }
                    else if (tileY <= undergroundY)
                    {
                        world.Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Fore = Tile.Fores.Stone;
                        world.Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Back = Tile.Backs.Stone;
                    }
                }
                if (random.Next(3) == 1)
                    surfaceY = Math.Min(maxSurfaceY, Math.Max(minSurfaceY, (surfaceY + random.Next(-1, 2))));
            }
            return world;
        }
    }
}