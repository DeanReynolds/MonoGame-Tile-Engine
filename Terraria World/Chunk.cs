using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Terraria_World
{
    public struct Chunk : IDisposable
    {
        public const int Bits = 3;
        public const int BufferX = 0;
        public const int TwoBufferX = (BufferX * 2);
        public const int BufferY = 0;
        public const int TwoBufferY = (BufferY * 2);

        public static readonly int Size = (int)Math.Pow(2, Bits);
        public static readonly int Modulo = (Size - 1);
        public static readonly int ForeTextureSize = (Tile.Size * Size);
        public static readonly int BackTextureSize = (ForeTextureSize + Tile.BackTextureSize);
        public static readonly int BufferXTextureSize = (BufferX * ForeTextureSize);
        public static readonly int BufferYTextureSize = (BufferY * ForeTextureSize);

        public Tile[,] Tiles { get; internal set; }

        public RenderTarget2D ForeTexture { get; private set; }
        public RenderTarget2D BackTexture { get; private set; }

        public void BakeFore()
        {
            if (ForeTexture == null)
            {
                ForeTexture = new RenderTarget2D(Program.Game.GraphicsDevice, ForeTextureSize, ForeTextureSize, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                ForeTexture.ContentLost += Fore_ContentLost;
            }
            SpriteBatch spriteBatch = Program.Game.Services.GetService<SpriteBatch>();
            Program.Game.GraphicsDevice.SetRenderTarget(ForeTexture);
            Program.Game.GraphicsDevice.Clear(Color.TransparentBlack);
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                    if (Tiles[x, y].Fore != Tile.Fores.None)
                    {
                        ForeTileData tileData = Game1.ForeTileData[Tiles[x, y].Fore];
                        spriteBatch.Draw(tileData.Texture, new Rectangle((x * Tile.Size), (y * Tile.Size), Tile.Size, Tile.Size), ForeTileData.UV[(int)tileData.UVType][Tiles[x, y].ForeUV][Tiles[x, y].ForeVariation], Color.White);
                    }
            spriteBatch.End();
        }

        private void Fore_ContentLost(object sender, EventArgs e)
        {
            BakeFore();
        }

        public void BakeBack()
        {
            if (BackTexture == null)
            {
                BackTexture = new RenderTarget2D(Program.Game.GraphicsDevice, BackTextureSize, BackTextureSize, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                BackTexture.ContentLost += Back_ContentLost;
            }
            SpriteBatch spriteBatch = Program.Game.Services.GetService<SpriteBatch>();
            Program.Game.GraphicsDevice.SetRenderTarget(BackTexture);
            Program.Game.GraphicsDevice.Clear(Color.TransparentBlack);
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                    if (Tiles[x, y].Back != Tile.Backs.None)
                    {
                        BackTileData tileData = Game1.BackTileData[Tiles[x, y].Back];
                        spriteBatch.Draw(tileData.Texture, new Rectangle((x * Tile.Size), (y * Tile.Size), Tile.BackTextureSize, Tile.BackTextureSize), BackTileData.UV[Tiles[x, y].BackUV][Tiles[x, y].BackVariation], Color.White);
                    }
            spriteBatch.End();
        }

        private void Back_ContentLost(object sender, EventArgs e)
        {
            BakeBack();
        }

        public void Dispose()
        {
            if (ForeTexture != null)
            {
                ForeTexture.ContentLost -= Fore_ContentLost;
                ForeTexture.Dispose();
                ForeTexture = null;
            }
            if (BackTexture != null)
            {
                BackTexture.ContentLost -= Back_ContentLost;
                BackTexture.Dispose();
                BackTexture = null;
            }
        }
    }
}