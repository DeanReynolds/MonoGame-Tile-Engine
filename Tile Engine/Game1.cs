using Tile_Engine.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Tile_Engine
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public const long ActiveFrameRate = (TimeSpan.TicksPerSecond / 60);
        public const long InactiveFrameRate = (TimeSpan.TicksPerSecond / 30);

        public static Texture2D Pixel { get; private set; }

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
            _scene = new Scenes.Game(256);
            _output = new RenderTarget2D(GraphicsDevice, Screen.VirtualWidth, Screen.VirtualHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
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
            GraphicsDevice.Clear(Color.Black);
            _scene?.Draw(_spriteBatch, gameTime);
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
            string fps = string.Format("FPS: {0}", Math.Floor(1 / gameTime.ElapsedGameTime.TotalSeconds));
            Vector2 fpsSize = _font.MeasureString(fps);
            _spriteBatch.DrawString(_font, fps, new Vector2((Screen.VirtualWidth - fpsSize.X - 3), 5), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            _spriteBatch.DrawString(_font, fps, new Vector2((Screen.VirtualWidth - fpsSize.X - 4), 4), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            _spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
            _spriteBatch.Draw(_output, new Rectangle(0, 0, Screen.ViewportWidth, Screen.ViewportHeight), Color.White);
            _spriteBatch.End();
            Profiler.Stop("Game Draw");
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