using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Tile_Engine
{
    public struct Chunk : IDisposable
    {
        public const int Bits = 4;
        public const int BufferX = 0;
        public const int TwoBufferX = (BufferX * 2);
        public const int BufferY = 0;
        public const int TwoBufferY = (BufferY * 2);

        public static readonly int Size = (int)Math.Pow(Bits, 2);
        public static readonly int Modulo = (Size - 1);
        public static readonly int TextureSize = (Tile.Size * Size);
        public static readonly int BufferXTextureSize = (BufferX * TextureSize);
        public static readonly int BufferYTextureSize = (BufferY * TextureSize);

        //static Chunk()
        //{
        //    Console.WriteLine(string.Format("Bits: {0}, Size: {1}x, Texture Size: {2}x", Bits, Size, TextureSize));
        //}

        public Tile[,] Tiles { get; internal set; }

        public RenderTarget2D Texture { get; private set; }

        public void Bake()
        {
            if (Texture == null)
            {
                Texture = new RenderTarget2D(Program.Game.GraphicsDevice, TextureSize, TextureSize, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                Texture.ContentLost += Texture_ContentLost;
            }
            SpriteBatch spriteBatch = Program.Game.Services.GetService<SpriteBatch>();
            Program.Game.GraphicsDevice.SetRenderTarget(Texture);
            Program.Game.GraphicsDevice.Clear(Color.TransparentBlack);
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                {
                    if (Tiles[x, y].Fore == Tile.Fores.Dirt)
                        spriteBatch.Draw(Game1.Pixel, new Rectangle((x * Tile.Size), (y * Tile.Size), Tile.Size, Tile.Size), Color.Brown);
                    else if (Tiles[x, y].Fore == Tile.Fores.Stone)
                        spriteBatch.Draw(Game1.Pixel, new Rectangle((x * Tile.Size), (y * Tile.Size), Tile.Size, Tile.Size), Color.Silver);
                }
            spriteBatch.End();
        }

        private void Texture_ContentLost(object sender, EventArgs e) { Bake(); }

        public void Dispose()
        {
            if (Texture != null)
            {
                Texture.ContentLost -= Texture_ContentLost;
                Texture.Dispose();
                Texture = null;
            }
        }
    }
}