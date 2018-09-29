using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria_World
{
    public class World
    {
        public static readonly Point[] dir8 = new[]
        {
            new Point(-1, -1), new Point(0, -1), new Point(1, -1),
            new Point(-1, 0), new Point(1, 0),
            new Point(-1, 1), new Point(0, 1), new Point(1, 1)
        };

        public Point Spawn { get; internal set; }

        public readonly Chunk[,] Chunks;
        public readonly int ChunksWidth;
        public readonly int ChunksHeight;
        public readonly int ChunksLastIndexX;
        public readonly int ChunksLastIndexY;
        public readonly int TilesWidth;
        public readonly int TilesHeight;

        public RenderTarget2D ForeTexture { get; private set; }
        public RenderTarget2D BackTexture { get; private set; }
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

        private Vector2 _foreDrawOffset;
        private Vector2 _backDrawOffset;

        public World(int tilesWidth, int tilesHeight)
        {
            if ((tilesWidth <= 0) || (tilesHeight <= 0) || (tilesWidth > ushort.MaxValue) || (tilesHeight > ushort.MaxValue))
                throw new ArgumentOutOfRangeException(string.Format("World width and height must be between 0 and {0}", (ushort.MaxValue + 1)));
            int chunksWidth;
            if ((tilesWidth >= Chunk.Size) && ((tilesWidth % Chunk.Size) == 0))
                chunksWidth = (tilesWidth >> Chunk.Bits);
            else
                chunksWidth = ((tilesWidth >> Chunk.Bits) + 1);
            tilesWidth = TilesWidth = (chunksWidth << Chunk.Bits);
            int chunksHeight;
            if ((tilesHeight >= Chunk.Size) && ((tilesHeight % Chunk.Size) == 0))
                chunksHeight = (tilesHeight >> Chunk.Bits);
            else
                chunksHeight = ((tilesHeight >> Chunk.Bits) + 1);
            tilesHeight = TilesHeight = (chunksHeight << Chunk.Bits);
            Chunks = new Chunk[(ChunksWidth = chunksWidth), (ChunksHeight = chunksHeight)];
            ChunksLastIndexX = (ChunksWidth - 1);
            ChunksLastIndexY = (ChunksHeight - 1);
            for (int x = 0; x < ChunksWidth; x++)
                for (int y = 0; y < ChunksHeight; y++)
                    Chunks[x, y] = new Chunk()
                    {
                        Tiles = new Tile[Chunk.Size, Chunk.Size]
                    };
            BakedChunksWidth = ((int)Math.Ceiling((Game1.VirtualWidth / (float)Tile.Size) / Chunk.Size) + 1 + Chunk.TwoBufferX);
            BakedChunksHeight = ((int)Math.Ceiling((Game1.VirtualHeight / (float)Tile.Size) / Chunk.Size) + 1 + Chunk.TwoBufferY);
            int textureWidth = (BakedChunksWidth * Chunk.ForeTextureSize);
            int textureHeight = (BakedChunksHeight * Chunk.ForeTextureSize);
            ForeTexture = new RenderTarget2D(Program.Game.GraphicsDevice, textureWidth, textureHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            ForeTexture.ContentLost += ForeTexture_ContentLost;
            BackTexture = new RenderTarget2D(Program.Game.GraphicsDevice, (textureWidth + Tile.BackTextureSize), (textureHeight + Tile.BackTextureSize), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            BackTexture.ContentLost += BackTexture_ContentLost;
            OldChunksMinX = -1;
            OldChunksMinY = -1;
            OldChunksMaxX = -1;
            OldChunksMaxY = -1;
        }

        private void ForeTexture_ContentLost(object sender, EventArgs e)
        {
            BakeFore();
        }

        private void BackTexture_ContentLost(object sender, EventArgs e)
        {
            BakeBack();
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(BackTexture, _backDrawOffset, null, Color.White, 0, Vector2.Zero, Game1.VirtualScale, SpriteEffects.None, 1);
            spriteBatch.Draw(ForeTexture, _foreDrawOffset, null, Color.White, 0, Vector2.Zero, Game1.VirtualScale, SpriteEffects.None, 0);
        }

        public bool InChunkBounds(int chunkX, int chunkY) { return ((chunkX >= 0) && (chunkY >= 0) && (chunkX < ChunksWidth) && (chunkY < ChunksHeight)); }

        public bool InTileBounds(int tileX, int tileY) { return ((tileX >= 0) && (tileY >= 0) && (tileX < TilesWidth) && (tileY < TilesHeight)); }

        public void Bake(float cameraX, float cameraY, float screenWidthOver2, float screenHeightOver2)
        {
            float cameraOffsetX = (cameraX - screenWidthOver2);
            int minChunkX = (int)((cameraOffsetX / Tile.Size) / Chunk.Size);
            CameraX = cameraX;
            _foreDrawOffset.X = ((((minChunkX * Chunk.ForeTextureSize) - cameraOffsetX) - Chunk.BufferXForeTextureSize) * Game1.VirtualScale);
            _backDrawOffset.X = (_foreDrawOffset.X - Tile.Size);
            RawChunksMinX = (minChunkX - Chunk.BufferX);
            RawChunksMaxX = (((int)((cameraX + screenWidthOver2) / Tile.Size) >> Chunk.Bits) + 1 + Chunk.BufferX);
            ChunksMinX = Math.Max(0, RawChunksMinX);
            ChunksMaxX = Math.Min(ChunksLastIndexX, RawChunksMaxX);
            float cameraOffsetY = (cameraY - screenHeightOver2);
            int minChunkY = (int)((cameraOffsetY / Tile.Size) / Chunk.Size);
            CameraY = cameraY;
            _foreDrawOffset.Y = ((((minChunkY * Chunk.ForeTextureSize) - cameraOffsetY) - Chunk.BufferYForeTextureSize) * Game1.VirtualScale);
            _backDrawOffset.Y = (_foreDrawOffset.Y - Tile.Size);
            RawChunksMinY = (minChunkY - Chunk.BufferY);
            RawChunksMaxY = (((int)((cameraY + screenHeightOver2) / Tile.Size) >> Chunk.Bits) + 1 + Chunk.BufferY);
            ChunksMinY = Math.Max(0, RawChunksMinY);
            ChunksMaxY = Math.Min(ChunksLastIndexY, RawChunksMaxY);
            if ((RawChunksMinX != OldChunksMinX) || (RawChunksMaxX != OldChunksMaxX) || (RawChunksMinY != OldChunksMinY) || (RawChunksMaxY != OldChunksMaxY))
            {
                Profiler.Start("Chunks/World Bake");
                HashSet<Point> chunksToUnload = new HashSet<Point>();
                for (int x = OldChunksMinX; x < OldChunksMaxX; x++)
                    if ((x >= 0) && (x < ChunksWidth))
                        for (int y = OldChunksMinY; y < OldChunksMaxY; y++)
                            if ((y >= 0) && (y < ChunksHeight))
                                if ((Chunks[x, y].ForeTexture != null) || (Chunks[x, y].BackTexture != null))
                                    chunksToUnload.Add(new Point(x, y));
#if DEBUG
                Game1.WorldBakeDrawCount = 0;
                Game1.WorldBakeTextureCount = 0;
#endif
                for (int x = RawChunksMinX; x < RawChunksMaxX; x++)
                    if ((x >= 0) && (x < ChunksWidth))
                        for (int y = RawChunksMinY; y < RawChunksMaxY; y++)
                            if ((y >= 0) && (y < ChunksHeight))
                            {
                                chunksToUnload.Remove(new Point(x, y));
                                if (Chunks[x, y].ForeTexture == null)
                                    Chunks[x, y].BakeFore();
                                if (Chunks[x, y].BackTexture == null)
                                    Chunks[x, y].BakeBack();
                            }
                foreach (Point point in chunksToUnload)
                    Chunks[point.X, point.Y].Dispose();
                BakeFore();
                BakeBack();
                Profiler.Stop("Chunks/World Bake");
                OldChunksMinX = RawChunksMinX;
                OldChunksMaxX = RawChunksMaxX;
                OldChunksMinY = RawChunksMinY;
                OldChunksMaxY = RawChunksMaxY;
            }
        }

        internal void BakeFore()
        {
            SpriteBatch spriteBatch = Program.Game.Services.GetService<SpriteBatch>();
            Program.Game.GraphicsDevice.SetRenderTarget(ForeTexture);
            Program.Game.GraphicsDevice.Clear(Color.TransparentBlack);
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
            int kReset = ((ChunksMinY - RawChunksMinY) * Chunk.ForeTextureSize);
            for (int x = RawChunksMinX, j = ((ChunksMinX - RawChunksMinX) * Chunk.ForeTextureSize), k = kReset; x < RawChunksMaxX; x++, k = kReset)
                if ((x >= 0) && (x < ChunksWidth))
                {
                    for (int y = RawChunksMinY; y < RawChunksMaxY; y++)
                        if ((y >= 0) && (y < ChunksHeight))
                        {
                            spriteBatch.Draw(Chunks[x, y].ForeTexture, new Rectangle(j, k, Chunk.ForeTextureSize, Chunk.ForeTextureSize), Color.White);
                            k += Chunk.ForeTextureSize;
                        }
                    j += Chunk.ForeTextureSize;
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

        internal void BakeBack()
        {
            SpriteBatch spriteBatch = Program.Game.Services.GetService<SpriteBatch>();
            Program.Game.GraphicsDevice.SetRenderTarget(BackTexture);
            Program.Game.GraphicsDevice.Clear(Color.TransparentBlack);
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
            int kReset = ((ChunksMinY - RawChunksMinY) * Chunk.ForeTextureSize);
            for (int x = RawChunksMinX, j = ((ChunksMinX - RawChunksMinX) * Chunk.ForeTextureSize), k = kReset; x < RawChunksMaxX; x++, k = kReset)
                if ((x >= 0) && (x < ChunksWidth))
                {
                    for (int y = RawChunksMinY; y < RawChunksMaxY; y++)
                        if ((y >= 0) && (y < ChunksHeight))
                        {
                            spriteBatch.Draw(Chunks[x, y].BackTexture, new Rectangle(j, k, Chunk.BackTextureSize, Chunk.BackTextureSize), Color.White);
                            k += Chunk.ForeTextureSize;
                        }
                    j += Chunk.ForeTextureSize;
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

        public bool SetTile(int tileX, int tileY, Tile.Fores fore, Tile.Backs back)
        {
            if (!InTileBounds(tileX, tileY))
                return false;
            int chunkX = (tileX >> Chunk.Bits);
            int chunkY = (tileY >> Chunk.Bits);
            int chunkTileX = (tileX & Chunk.Modulo);
            int chunkTileY = (tileY & Chunk.Modulo);
            if (Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Fore != fore)
            {
                Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Fore = fore;
                UpdateForeUV(tileX, tileY, true, false, true);
                if (Chunks[chunkX, chunkY].ForeTexture != null)
                {
                    Chunks[chunkX, chunkY].BakeFore();
                    BakeFore();
                }
                return true;
            }
            if (Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Back != back)
            {
                Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Back = back;
                UpdateBackUV(tileX, tileY, true, false, true);
                if (Chunks[chunkX, chunkY].BackTexture != null)
                {
                    Chunks[chunkX, chunkY].BakeBack();
                    BakeBack();
                }
                return true;
            }
            return false;
        }

        public bool SetTileFore(int tileX, int tileY, Tile.Fores fore)
        {
            if (!InTileBounds(tileX, tileY))
                return false;
            int chunkX = (tileX >> Chunk.Bits);
            int chunkY = (tileY >> Chunk.Bits);
            int chunkTileX = (tileX & Chunk.Modulo);
            int chunkTileY = (tileY & Chunk.Modulo);
            if (Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Fore != fore)
            {
                Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Fore = fore;
                UpdateForeUV(tileX, tileY, true, false, true);
                if (Chunks[chunkX, chunkY].ForeTexture != null)
                {
                    Chunks[chunkX, chunkY].BakeFore();
                    BakeFore();
                }
                return true;
            }
            return false;
        }

        public bool UpdateForeUV(int tileX, int tileY, bool updateVariation, bool rebakeChunk, bool updateAdjacentUVs)
        {
            if (!InTileBounds(tileX, tileY))
                return false;
            int chunkX = (tileX >> Chunk.Bits);
            int chunkY = (tileY >> Chunk.Bits);
            int chunkTileX = (tileX & Chunk.Modulo);
            int chunkTileY = (tileY & Chunk.Modulo);
            Tile tile = Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY];
            if (tile.Fore == Tile.Fores.None)
            {
                if (UpdateAdjacentForeUVs(tileX, tileY, chunkX, chunkY))
                    BakeFore();
                return false;
            }
            ForeTileData tileData = Game1.ForeTileData[tile.Fore];
            if (updateVariation)
                RandomizeForeVariation(chunkX, chunkY, chunkTileX, chunkTileY, tile, tileData);
            if (UpdateForeUV(tileX, tileY, chunkX, chunkY, chunkTileX, chunkTileY, tile, tileData))
            {
                if (updateAdjacentUVs && UpdateAdjacentForeUVs(tileX, tileY, chunkX, chunkY))
                    BakeFore();
                if (rebakeChunk)
                    if (Chunks[chunkX, chunkY].ForeTexture != null)
                    {
                        Chunks[chunkX, chunkY].BakeFore();
                        BakeFore();
                    }
                return true;
            }
            return false;
        }

        internal bool UpdateForeUV(int tileX, int tileY, int chunkX, int chunkY, int chunkTileX, int chunkTileY, Tile tile, ForeTileData tileData)
        {
            byte uv = 0;
            if (tileData.UVType == ForeTileData.UVTypes.tile1)
            {
                Tile? up = GetTile(tileX, (tileY - 1)),
                    right = GetTile((tileX + 1), tileY),
                    down = GetTile(tileX, (tileY + 1)),
                    left = GetTile((tileX - 1), tileY);
                if (up.HasValue && (Array.IndexOf(tileData.AttachedTiles, up.Value.Fore) != -1))
                    uv++;
                if (!right.HasValue || (Array.IndexOf(tileData.AttachedTiles, right.Value.Fore) != -1))
                    uv += 2;
                if (!down.HasValue || (Array.IndexOf(tileData.AttachedTiles, down.Value.Fore) != -1))
                    uv += 4;
                if (!left.HasValue || (Array.IndexOf(tileData.AttachedTiles, left.Value.Fore) != -1))
                    uv += 8;
            }
            else if (tileData.UVType == ForeTileData.UVTypes.tile2)
            {
                Tile? up = GetTile(tileX, (tileY - 1)),
                    right = GetTile((tileX + 1), tileY),
                    down = GetTile(tileX, (tileY + 1)),
                    left = GetTile((tileX - 1), tileY),
                    upRight = GetTile((tileX + 1), (tileY - 1)),
                    rightDown = GetTile((tileX + 1), (tileY + 1)),
                    downLeft = GetTile((tileX - 1), (tileY + 1)),
                    leftUp = GetTile((tileX - 1), (tileY - 1));
                if (up.HasValue && (Array.IndexOf(tileData.AttachedTiles, up.Value.Fore) != -1))
                    uv++;
                if (!right.HasValue || (Array.IndexOf(tileData.AttachedTiles, right.Value.Fore) != -1))
                    uv += 2;
                if (!down.HasValue || (Array.IndexOf(tileData.AttachedTiles, down.Value.Fore) != -1))
                    uv += 4;
                if (!left.HasValue || (Array.IndexOf(tileData.AttachedTiles, left.Value.Fore) != -1))
                    uv += 8;
                if (upRight.HasValue && (Array.IndexOf(tileData.AttachedTiles, upRight.Value.Fore) == -1))
                {
                    if (uv == 3)
                        uv = 16;
                    else if (uv == 7)
                        uv = 17;
                    else if (uv == 11)
                        uv = 18;
                    else if (uv == 15)
                        uv = 19;
                }
                if (!rightDown.HasValue || (Array.IndexOf(tileData.AttachedTiles, rightDown.Value.Fore) == -1))
                {
                    if (uv == 6)
                        uv = 20;
                    else if (uv == 7)
                        uv = 21;
                    else if (uv == 14)
                        uv = 22;
                    else if (uv == 15)
                        uv = 23;
                    else if (uv == 17)
                        uv = 24;
                    else if (uv == 19)
                        uv = 25;
                }
                if (!downLeft.HasValue || (Array.IndexOf(tileData.AttachedTiles, downLeft.Value.Fore) == -1))
                {
                    if (uv == 12)
                        uv = 26;
                    else if (uv == 13)
                        uv = 27;
                    else if (uv == 14)
                        uv = 28;
                    else if (uv == 15)
                        uv = 29;
                    else if (uv == 19)
                        uv = 30;
                    else if (uv == 22)
                        uv = 31;
                    else if (uv == 23)
                        uv = 32;
                    else if (uv == 25)
                        uv = 33;
                }
                if (leftUp.HasValue && (Array.IndexOf(tileData.AttachedTiles, leftUp.Value.Fore) == -1))
                {
                    if (uv == 9)
                        uv = 34;
                    else if (uv == 11)
                        uv = 35;
                    else if (uv == 13)
                        uv = 36;
                    else if (uv == 15)
                        uv = 37;
                    else if (uv == 18)
                        uv = 38;
                    else if (uv == 19)
                        uv = 39;
                    else if (uv == 23)
                        uv = 40;
                    else if (uv == 25)
                        uv = 41;
                    else if (uv == 27)
                        uv = 42;
                    else if (uv == 29)
                        uv = 43;
                    else if (uv == 30)
                        uv = 44;
                    else if (uv == 32)
                        uv = 45;
                    else if (uv == 33)
                        uv = 46;
                }
            }
            if (Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].ForeUV == uv)
                return false;
            Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].ForeUV = uv;
            return true;
        }

        internal bool UpdateAdjacentForeUVs(int tileX, int tileY, int chunkX, int chunkY)
        {
            HashSet<Point> chunksToRebake = new HashSet<Point>();
            foreach (Point dir in dir8)
            {
                int adjacentTileX = (tileX + dir.X);
                int adjacentTileY = (tileY + dir.Y);
                if (!InTileBounds(adjacentTileX, adjacentTileY))
                    continue;
                int adjacentTileChunkX = (adjacentTileX >> Chunk.Bits);
                int adjacentTileChunkY = (adjacentTileY >> Chunk.Bits);
                int adjacentTileChunkTileX = (adjacentTileX & Chunk.Modulo);
                int adjacentTileChunkTileY = (adjacentTileY & Chunk.Modulo);
                Tile adjacentTile = Chunks[adjacentTileChunkX, adjacentTileChunkY].Tiles[adjacentTileChunkTileX, adjacentTileChunkTileY];
                if (adjacentTile.Fore == Tile.Fores.None)
                    continue;
                if (UpdateForeUV(adjacentTileX, adjacentTileY, adjacentTileChunkX, adjacentTileChunkY, adjacentTileChunkTileX, adjacentTileChunkTileY, adjacentTile, Game1.ForeTileData[adjacentTile.Fore]))
                    chunksToRebake.Add(new Point(adjacentTileChunkX, adjacentTileChunkY));
            }
            chunksToRebake.Remove(new Point(chunkX, chunkY));
            if (chunksToRebake.Count == 0)
                return false;
            foreach (Point chunk in chunksToRebake)
                if (Chunks[chunk.X, chunk.Y].ForeTexture != null)
                    Chunks[chunk.X, chunk.Y].BakeFore();
            return true;
        }

        public bool RenewForeVariation(int tileX, int tileY)
        {
            if (!InTileBounds(tileX, tileY))
                return false;
            int chunkX = (tileX >> Chunk.Bits);
            int chunkY = (tileY >> Chunk.Bits);
            int chunkTileX = (tileX & Chunk.Modulo);
            int chunkTileY = (tileY & Chunk.Modulo);
            Tile tile = Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY];
            if (tile.Fore == Tile.Fores.None)
                return false;
            ForeTileData tileData = Game1.ForeTileData[tile.Fore];
            RenewForeVariation(chunkX, chunkY, chunkTileX, chunkTileY, tile, tileData);
            return true;
        }

        internal void RenewForeVariation(int chunkX, int chunkY, int chunkTileX, int chunkTileY, Tile tile, ForeTileData tileData)
        {
            int maxVariations = ForeTileData.UV[(int)tileData.UVType][tile.ForeUV].Length;
            Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].ForeVariation = (byte)Enumerable.Range(0, maxVariations).Where(i => (i != tile.ForeVariation)).ElementAt(Game1.Random.Next(--maxVariations));
        }

        public bool RandomizeForeVariation(int tileX, int tileY)
        {
            if (!InTileBounds(tileX, tileY))
                return false;
            int chunkX = (tileX >> Chunk.Bits);
            int chunkY = (tileY >> Chunk.Bits);
            int chunkTileX = (tileX & Chunk.Modulo);
            int chunkTileY = (tileY & Chunk.Modulo);
            Tile tile = Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY];
            if (tile.Fore == Tile.Fores.None)
                return false;
            ForeTileData tileData = Game1.ForeTileData[tile.Fore];
            RandomizeForeVariation(chunkX, chunkY, chunkTileX, chunkTileY, tile, tileData);
            return true;
        }

        internal void RandomizeForeVariation(int chunkX, int chunkY, int chunkTileX, int chunkTileY, Tile tile, ForeTileData tileData)
        {
            int maxVariations = ForeTileData.UV[(int)tileData.UVType][tile.ForeUV].Length;
            Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].ForeVariation = (byte)Enumerable.Range(0, maxVariations).ElementAt(Game1.Random.Next(maxVariations));
        }

        public bool SetTileBack(int tileX, int tileY, Tile.Backs back)
        {
            if (!InTileBounds(tileX, tileY))
                return false;
            int chunkX = (tileX >> Chunk.Bits);
            int chunkY = (tileY >> Chunk.Bits);
            int chunkTileX = (tileX & Chunk.Modulo);
            int chunkTileY = (tileY & Chunk.Modulo);
            if (Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Back != back)
            {
                Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].Back = back;
                UpdateBackUV(tileX, tileY, true, false, true);
                if (Chunks[chunkX, chunkY].BackTexture != null)
                {
                    Chunks[chunkX, chunkY].BakeBack();
                    BakeBack();
                }
                return true;
            }
            return false;
        }

        public bool UpdateBackUV(int tileX, int tileY, bool updateVariation, bool rebakeChunk, bool updateAdjacentUVs)
        {
            if (!InTileBounds(tileX, tileY))
                return false;
            int chunkX = (tileX >> Chunk.Bits);
            int chunkY = (tileY >> Chunk.Bits);
            int chunkTileX = (tileX & Chunk.Modulo);
            int chunkTileY = (tileY & Chunk.Modulo);
            Tile tile = Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY];
            if (tile.Back == Tile.Backs.None)
            {
                if (UpdateAdjacentBackUVs(tileX, tileY, chunkX, chunkY))
                    BakeBack();
                return false;
            }
            BackTileData tileData = Game1.BackTileData[tile.Back];
            if (updateVariation)
                RandomizeBackVariation(chunkX, chunkY, chunkTileX, chunkTileY, tile, tileData);
            if (UpdateBackUV(tileX, tileY, chunkX, chunkY, chunkTileX, chunkTileY, tile))
            {
                if (updateAdjacentUVs && UpdateAdjacentBackUVs(tileX, tileY, chunkX, chunkY))
                    BakeBack();
                if (rebakeChunk)
                    if (Chunks[chunkX, chunkY].BackTexture != null)
                    {
                        Chunks[chunkX, chunkY].BakeBack();
                        BakeBack();
                    }
                return true;
            }
            return false;
        }

        internal bool UpdateBackUV(int tileX, int tileY, int chunkX, int chunkY, int chunkTileX, int chunkTileY, Tile tile)
        {
            byte uv = 0;
            Tile? up = GetTile(tileX, (tileY - 1)),
                right = GetTile((tileX + 1), tileY),
                down = GetTile(tileX, (tileY + 1)),
                left = GetTile((tileX - 1), tileY);
            if (up.HasValue && (up.Value.Back != Tile.Backs.None))
                uv++;
            if (!right.HasValue || (right.Value.Back != Tile.Backs.None))
                uv += 2;
            if (!down.HasValue || (down.Value.Back != Tile.Backs.None))
                uv += 4;
            if (!left.HasValue || (left.Value.Back != Tile.Backs.None))
                uv += 8;
            if (Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].BackUV == uv)
                return false;
            Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].BackUV = uv;
            return true;
        }

        internal bool UpdateAdjacentBackUVs(int tileX, int tileY, int chunkX, int chunkY)
        {
            HashSet<Point> chunksToRebake = new HashSet<Point>();
            foreach (Point dir in dir8)
            {
                int adjacentTileX = (tileX + dir.X);
                int adjacentTileY = (tileY + dir.Y);
                if (!InTileBounds(adjacentTileX, adjacentTileY))
                    continue;
                int adjacentTileChunkX = (adjacentTileX >> Chunk.Bits);
                int adjacentTileChunkY = (adjacentTileY >> Chunk.Bits);
                int adjacentTileChunkTileX = (adjacentTileX & Chunk.Modulo);
                int adjacentTileChunkTileY = (adjacentTileY & Chunk.Modulo);
                Tile adjacentTile = Chunks[adjacentTileChunkX, adjacentTileChunkY].Tiles[adjacentTileChunkTileX, adjacentTileChunkTileY];
                if (adjacentTile.Back == Tile.Backs.None)
                    continue;
                if (UpdateBackUV(adjacentTileX, adjacentTileY, adjacentTileChunkX, adjacentTileChunkY, adjacentTileChunkTileX, adjacentTileChunkTileY, adjacentTile))
                    chunksToRebake.Add(new Point(adjacentTileChunkX, adjacentTileChunkY));
            }
            chunksToRebake.Remove(new Point(chunkX, chunkY));
            if (chunksToRebake.Count == 0)
                return false;
            foreach (Point chunk in chunksToRebake)
                if (Chunks[chunk.X, chunk.Y].BackTexture != null)
                    Chunks[chunk.X, chunk.Y].BakeBack();
            return true;
        }

        public bool RenewBackVariation(int tileX, int tileY)
        {
            if (!InTileBounds(tileX, tileY))
                return false;
            int chunkX = (tileX >> Chunk.Bits);
            int chunkY = (tileY >> Chunk.Bits);
            int chunkTileX = (tileX & Chunk.Modulo);
            int chunkTileY = (tileY & Chunk.Modulo);
            Tile tile = Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY];
            if (tile.Back == Tile.Backs.None)
                return false;
            BackTileData tileData = Game1.BackTileData[tile.Back];
            RenewBackVariation(chunkX, chunkY, chunkTileX, chunkTileY, tile, tileData);
            return true;
        }

        internal void RenewBackVariation(int chunkX, int chunkY, int chunkTileX, int chunkTileY, Tile tile, BackTileData tileData)
        {
            int maxVariations = BackTileData.UV[tile.BackUV].Length;
            Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].BackVariation = (byte)Enumerable.Range(0, maxVariations).Where(i => (i != tile.BackVariation)).ElementAt(Game1.Random.Next(--maxVariations));
        }

        public bool RandomizeBackVariation(int tileX, int tileY)
        {
            if (!InTileBounds(tileX, tileY))
                return false;
            int chunkX = (tileX >> Chunk.Bits);
            int chunkY = (tileY >> Chunk.Bits);
            int chunkTileX = (tileX & Chunk.Modulo);
            int chunkTileY = (tileY & Chunk.Modulo);
            Tile tile = Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY];
            if (tile.Back == Tile.Backs.None)
                return false;
            BackTileData tileData = Game1.BackTileData[tile.Back];
            RandomizeBackVariation(chunkX, chunkY, chunkTileX, chunkTileY, tile, tileData);
            return true;
        }

        internal void RandomizeBackVariation(int chunkX, int chunkY, int chunkTileX, int chunkTileY, Tile tile, BackTileData tileData)
        {
            int maxVariations = BackTileData.UV[tile.BackUV].Length;
            Chunks[chunkX, chunkY].Tiles[chunkTileX, chunkTileY].BackVariation = (byte)Enumerable.Range(0, maxVariations).ElementAt(Game1.Random.Next(maxVariations));
        }
    }
}