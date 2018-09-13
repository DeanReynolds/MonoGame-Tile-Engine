using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Tile_Engine
{
    public class World
    {
        public static readonly Point[] dir8 = new[]
        {
            new Point(-1, -1), new Point(0, -1), new Point(1, -1),
            new Point(-1, 0), new Point(1, 0),
            new Point(-1, 1), new Point(0, 1), new Point(1, 1)
        };

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
        public int OldRawChunksMinX { get; private set; }
        public int OldRawChunksMaxX { get; private set; }
        public int OldRawChunksMinY { get; private set; }
        public int OldRawChunksMaxY { get; private set; }
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
            BakedChunksWidth = ((int)Math.Ceiling((Game1.VirtualWidth / (float)Tile.Size) / Chunk.Size) + 1 + Chunk.TwoBufferX);
            BakedChunksHeight = ((int)Math.Ceiling((Game1.VirtualHeight / (float)Tile.Size) / Chunk.Size) + 1 + Chunk.TwoBufferY);
            int textureWidth = (BakedChunksWidth * Chunk.TextureSize);
            int textureHeight = (BakedChunksHeight * Chunk.TextureSize);
            Texture = new RenderTarget2D(Program.Game.GraphicsDevice, textureWidth, textureHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            Texture.ContentLost += Texture_ContentLost;
        }

        private void Texture_ContentLost(object sender, EventArgs e)
        {
            Bake();
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(Texture, new Vector2(DrawOffsetX, DrawOffsetY), Color.White);
        }

        public bool InChunkBounds(int chunkX, int chunkY) { return ((chunkX >= 0) && (chunkY >= 0) && (chunkX < ChunksWidth) && (chunkY < ChunksHeight)); }

        public bool InTileBounds(int tileX, int tileY) { return ((tileX >= 0) && (tileY >= 0) && (tileX < TilesWidth) && (tileY < TilesHeight)); }

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
            if ((RawChunksMinX != OldRawChunksMinX) || (RawChunksMaxX != OldRawChunksMaxX) || (RawChunksMinY != OldRawChunksMinY) || (RawChunksMaxY != OldRawChunksMaxY))
            {
                Profiler.Start("Chunks/World Bake");
                HashSet<Point> chunksToUnload = new HashSet<Point>();
                for (int x = OldRawChunksMinX; x < OldRawChunksMaxX; x++)
                    if ((x >= 0) && (x < ChunksWidth))
                        for (int y = OldRawChunksMinY; y < OldRawChunksMaxY; y++)
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
                Bake();
                Profiler.Stop("Chunks/World Bake");
                OldRawChunksMinX = RawChunksMinX;
                OldRawChunksMaxX = RawChunksMaxX;
                OldRawChunksMinY = RawChunksMinY;
                OldRawChunksMaxY = RawChunksMaxY;
            }
        }

        internal void Bake()
        {
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
#if DEBUG
            Game1.WorldBakeDrawCount += (Program.Game.GraphicsDevice.Metrics.DrawCount - Game1.WorldBakeDrawCount);
            Game1.WorldBakeTextureCount += (Program.Game.GraphicsDevice.Metrics.TextureCount - Game1.WorldBakeTextureCount);
            Game1.WorldBakeSpriteCount += (Program.Game.GraphicsDevice.Metrics.SpriteCount - Game1.WorldBakeSpriteCount);
            Game1.WorldBakePrimitiveCount += (Program.Game.GraphicsDevice.Metrics.PrimitiveCount - Game1.WorldBakePrimitiveCount);
            Game1.WorldBakeTargetCount += (Program.Game.GraphicsDevice.Metrics.TargetCount - Game1.WorldBakeTargetCount);
#endif
        }

        public Tile? GetTile(int tileX, int tileY)
        {
            if (!InTileBounds(tileX, tileY))
                return null;
            int chunkX = (tileX >> Chunk.Bits);
            int chunkY = (tileY >> Chunk.Bits);
            int chunkTileX = (tileX & Chunk.Modulo);
            int chunkTileY = (tileY & Chunk.Modulo);
            return Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY];
        }

        public bool SetTile(int tileX, int tileY, Tile.Types type)
        {
            if (!InTileBounds(tileX, tileY))
                return false;
            int chunkX = (tileX >> Chunk.Bits);
            int chunkY = (tileY >> Chunk.Bits);
            int chunkTileX = (tileX & Chunk.Modulo);
            int chunkTileY = (tileY & Chunk.Modulo);
            return SetTile(chunkX, chunkY, chunkTileX, chunkTileY, type);
        }

        internal virtual bool SetTile(int chunkX, int chunkY, int chunkTileX, int chunkTileY, Tile.Types type)
        {
            if (Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Type != type)
            {
                Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Type = type;
                if (Chunks[chunkX, chunkY].Texture != null)
                {
                    Chunks[chunkX, chunkY].Bake();
                    Bake();
                }
                return true;
            }
            return false;
        }
    }
}