using Terraria_World.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Terraria_World
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public const long ActiveFrameRate = (TimeSpan.TicksPerSecond / 60);
        public const long InactiveFrameRate = (TimeSpan.TicksPerSecond / 30);

        public static Random Random;
        public static Dictionary<Tile.Fores, ForeTileData> ForeTileData;
        public static Dictionary<Tile.Backs, BackTileData> BackTileData;
        public static int VirtualWidth;
        public static int VirtualHeight;

#if DEBUG
        public static long WorldBakeDrawCount;
        public static long WorldBakeTextureCount;
        public static long WorldBakeSpriteCount;
        public static long WorldBakePrimitiveCount;
        public static long WorldBakeTargetCount;
#endif

        public static Texture2D Pixel { get; private set; }

        public static int ViewportX { get { return Program.Game.GraphicsDevice.Viewport.X; } }
        public static int ViewportY { get { return Program.Game.GraphicsDevice.Viewport.Y; } }
        public static int ViewportWidth { get { return Program.Game.GraphicsDevice.Viewport.Width; } }
        public static int ViewportHeight { get { return Program.Game.GraphicsDevice.Viewport.Height; } }
        public static int WindowWidth { get { return Program.Game.Window.ClientBounds.Width; } }
        public static int WindowHeight { get { return Program.Game.Window.ClientBounds.Height; } }
        public static int PreferredBackBufferWidth { get { return Program.Game.Services.GetService<GraphicsDeviceManager>().PreferredBackBufferWidth; } }
        public static int PreferredBackBufferHeight { get { return Program.Game.Services.GetService<GraphicsDeviceManager>().PreferredBackBufferHeight; } }

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Scene _scene;
        private RenderTarget2D _output;
        private SpriteFont _font;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                GraphicsProfile = GraphicsProfile.HiDef,
                SynchronizeWithVerticalRetrace = false
            };
            VirtualWidth = _graphics.PreferredBackBufferWidth;
            VirtualHeight = _graphics.PreferredBackBufferHeight;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
            Services.AddService(_graphics);
            Services.AddService(_spriteBatch);
            if (IsActive)
                OnActivated(this, EventArgs.Empty);
            else
                OnDeactivated(this, EventArgs.Empty);
            IsMouseVisible = true;
            //IsFixedTimeStep = false;
            _graphics.PreferredBackBufferHeight = 1920;
            _graphics.PreferredBackBufferWidth = 1080;
            _graphics.HardwareModeSwitch = false;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
            VirtualWidth = 1920;
            VirtualHeight = 1080;
            Random = new Random();
            ForeTileData = new Dictionary<Tile.Fores, ForeTileData>()
            {
                { Tile.Fores.Dirt, new ForeTileData(Content.Load<Texture2D>("Textures\\Tiles_0"), Terraria_World.ForeTileData.UVTypes.tile1, new[] { Tile.Fores.Dirt, Tile.Fores.Grass }) },
                { Tile.Fores.Stone, new ForeTileData(Content.Load<Texture2D>("Textures\\Tiles_1"), Terraria_World.ForeTileData.UVTypes.tile1, new[] { Tile.Fores.Stone }) },
                { Tile.Fores.Grass, new ForeTileData(Content.Load<Texture2D>("Textures\\Tiles_2"), Terraria_World.ForeTileData.UVTypes.tile2, new[] { Tile.Fores.Grass, Tile.Fores.Dirt }) }
            };
            BackTileData = new Dictionary<Tile.Backs, BackTileData>()
            {
                { Tile.Backs.Stone, new BackTileData(Content.Load<Texture2D>("Textures\\Wall_1")) },
                { Tile.Backs.Dirt, new BackTileData(Content.Load<Texture2D>("Textures\\Wall_2")) },
            };
            _scene = new Scenes.Game(256);
            _output = new RenderTarget2D(GraphicsDevice, VirtualWidth, VirtualHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Pixel = new Texture2D(GraphicsDevice, 1, 1);
            Pixel.SetData(new[] { Color.White });
            _font = Content.Load<SpriteFont>("Fonts\\VCR OSD Mono");
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            Profiler.Start("Game Update");
            _scene?.Update(gameTime);
            Profiler.Stop("Game Update");
            Profiler.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Profiler.Start("Game Draw");
            GraphicsDevice.SetRenderTarget(_output);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _scene?.Draw(_spriteBatch, gameTime);
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
            _spriteBatch.Draw(_output, new Rectangle(0, 0, ViewportWidth, ViewportHeight), Color.White);
            _spriteBatch.End();
            Profiler.Stop("Game Draw");
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
            string text = string.Format("FPS: {0}", Math.Floor(1 / gameTime.ElapsedGameTime.TotalSeconds));
            Vector2 textSize = _font.MeasureString(text);
            _spriteBatch.DrawString(_font, text, new Vector2((ViewportWidth - textSize.X - 3), 5), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            _spriteBatch.DrawString(_font, text, new Vector2((ViewportWidth - textSize.X - 4), 4), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
#if DEBUG
            text = string.Format("Game\n  Draw Count: {0}\n  Texture Count: {1}\n  Sprite Count: {2}\n  Primitive Count: {3}\n  Target Count: {4}\n\nWorld Bake\n  Draw Count: {5}\n  Texture Count: {6}\n  Sprite Count: {7}\n  Primitive Count: {8}\n  Target Count: {9}", GraphicsDevice.Metrics.DrawCount, GraphicsDevice.Metrics.TextureCount, GraphicsDevice.Metrics.SpriteCount, GraphicsDevice.Metrics.PrimitiveCount, GraphicsDevice.Metrics.TargetCount, WorldBakeDrawCount, WorldBakeTextureCount, WorldBakeSpriteCount, WorldBakePrimitiveCount, WorldBakeTargetCount);
            _spriteBatch.DrawString(_font, text, new Vector2(5, 5), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            _spriteBatch.DrawString(_font, text, new Vector2(4, 4), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
#endif
            _spriteBatch.End();
            Profiler.Draw(_spriteBatch, _font, ViewportWidth, ViewportHeight);
            base.Draw(gameTime);
        }

        protected override void OnActivated(object sender, EventArgs args)
        {
            TargetElapsedTime = new TimeSpan(ActiveFrameRate);
            base.OnActivated(sender, args);
        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {
            TargetElapsedTime = new TimeSpan(InactiveFrameRate);
            base.OnDeactivated(sender, args);
        }
    }
}