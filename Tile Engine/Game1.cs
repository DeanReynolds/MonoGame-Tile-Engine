
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Tile_Engine.Scenes;

namespace Tile_Engine
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public const long ActiveFrameRate = (TimeSpan.TicksPerSecond / 60);
        public const long InactiveFrameRate = (TimeSpan.TicksPerSecond / 30);

#if DEBUG
        public static long WorldBakeDrawCount;
        public static long WorldBakeTextureCount;
        public static long WorldBakeSpriteCount;
        public static long WorldBakePrimitiveCount;
        public static long WorldBakeTargetCount;
#endif

        public static int VirtualWidth { get; private set; }
        public static int VirtualHeight { get; private set; }
        public static float VirtualScale { get; private set; }
        public static Viewport Viewport { get; private set; }
        public static Texture2D Pixel { get; private set; }
        public static Vector2 PixelOrigin { get; private set; }

        public static int WindowWidth { get { return Program.Game.Window.ClientBounds.Width; } }
        public static int WindowHeight { get { return Program.Game.Window.ClientBounds.Height; } }
        public static int PreferredBackBufferWidth { get { return Program.Game.Services.GetService<GraphicsDeviceManager>().PreferredBackBufferWidth; } }
        public static int PreferredBackBufferHeight { get { return Program.Game.Services.GetService<GraphicsDeviceManager>().PreferredBackBufferHeight; } }

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Scene _scene;
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
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.HardwareModeSwitch = false;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
            SetVirtualResolution(1920, 1080);
            PixelOrigin = new Vector2(.5f);
            _scene = new Scenes.Game(256);
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
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Viewport = Viewport;
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _scene?.Draw(_spriteBatch, gameTime);
            Profiler.Stop("Game Draw");
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
            string text = string.Format("FPS: {0}", Math.Floor(1 / gameTime.ElapsedGameTime.TotalSeconds));
            Vector2 textSize = _font.MeasureString(text);
            _spriteBatch.DrawString(_font, text, new Vector2((Viewport.Width - textSize.X - 3), 5), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            _spriteBatch.DrawString(_font, text, new Vector2((Viewport.Width - textSize.X - 4), 4), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
#if DEBUG
            text = string.Format("Game\n  Draw Count: {0}\n  Texture Count: {1}\n  Sprite Count: {2}\n  Primitive Count: {3}\n  Target Count: {4}\n\nWorld Bake\n  Draw Count: {5}\n  Texture Count: {6}\n  Sprite Count: {7}\n  Primitive Count: {8}\n  Target Count: {9}", GraphicsDevice.Metrics.DrawCount, GraphicsDevice.Metrics.TextureCount, GraphicsDevice.Metrics.SpriteCount, GraphicsDevice.Metrics.PrimitiveCount, GraphicsDevice.Metrics.TargetCount, WorldBakeDrawCount, WorldBakeTextureCount, WorldBakeSpriteCount, WorldBakePrimitiveCount, WorldBakeTargetCount);
            _spriteBatch.DrawString(_font, text, new Vector2(5, 5), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            _spriteBatch.DrawString(_font, text, new Vector2(4, 4), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
#endif
            _spriteBatch.End();
            Profiler.Draw(_spriteBatch, _font, Viewport.Width, Viewport.Height);
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

        public static void SetVirtualResolution(int width, int height)
        {
            VirtualWidth = width;
            VirtualHeight = height;
            GraphicsDeviceManager graphicsDeviceManager = Program.Game.Services.GetService<GraphicsDeviceManager>();
            var targetAspectRatio = (width / (float)height);
            var width2 = graphicsDeviceManager.PreferredBackBufferWidth;
            var height2 = (int)(width2 / targetAspectRatio + .5f);
            if (height2 > graphicsDeviceManager.PreferredBackBufferHeight)
            {
                height2 = graphicsDeviceManager.PreferredBackBufferHeight;
                width2 = (int)(height2 * targetAspectRatio + .5f);
            }
            Viewport = new Viewport()
            {
                X = ((graphicsDeviceManager.PreferredBackBufferWidth / 2) - (width2 / 2)),
                Y = ((graphicsDeviceManager.PreferredBackBufferHeight / 2) - (height2 / 2)),
                Width = width2,
                Height = height2
            };
            VirtualScale = MathHelper.Min((graphicsDeviceManager.PreferredBackBufferWidth / (float)width), (graphicsDeviceManager.PreferredBackBufferHeight / (float)height));
        }
    }
}