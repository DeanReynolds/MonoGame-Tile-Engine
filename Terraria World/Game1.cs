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

        public static Texture2D Pixel { get; private set; }
        public static Random Random;
        public static Dictionary<Tile.Fores, ForeTileData> ForeTileData;
        public static Dictionary<Tile.Backs, BackTileData> BackTileData;

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
            Screen.Mode = Screen.Modes.WindowedBorderless;
            Screen.VirtualWidth = 1920;
            Screen.VirtualHeight = 1080;
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
            _output = new RenderTarget2D(GraphicsDevice, Screen.VirtualWidth, Screen.VirtualHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            for (int i = 0; i < 33; i++)
            {
                Console.WriteLine(string.Format("{0} << {1} = {2}", i, Chunk.Bits, (i << 1)));
            }
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
            _spriteBatch.Draw(_output, new Rectangle(0, 0, Screen.ViewportWidth, Screen.ViewportHeight), Color.White);
            _spriteBatch.End();
            Profiler.Stop("Game Draw");
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
            string fps = string.Format("FPS: {0}", Math.Floor(1 / gameTime.ElapsedGameTime.TotalSeconds));
            Vector2 fpsSize = _font.MeasureString(fps);
            _spriteBatch.DrawString(_font, fps, new Vector2((Screen.ViewportWidth - fpsSize.X - 3), 5), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            _spriteBatch.DrawString(_font, fps, new Vector2((Screen.ViewportWidth - fpsSize.X - 4), 4), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            _spriteBatch.End();
            Profiler.Draw(_spriteBatch, _font, Screen.ViewportWidth, Screen.ViewportHeight);
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