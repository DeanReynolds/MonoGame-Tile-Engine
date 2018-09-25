using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Tile_Engine.Scenes
{
    public class Game : Scene
    {
        private readonly World _world;
        private readonly Camera _camera;

        public Game(int maxPlayers)
        {
            _world = WorldGenerator.Generate(500, 500);
            _camera = new Camera(new Vector2((_world.Spawn.X * Tile.Size), (_world.Spawn.Y * Tile.Size)));
        }

        public override void Update(GameTime gameTime)
        {
            float virtualWidthOver2 = (Game1.VirtualWidth / 2f);
            float virtualHeightOver2 = (Game1.VirtualHeight / 2f);
            if (Program.Game.IsActive)
            {
                float camSpeed = (float)(500 * gameTime.ElapsedGameTime.TotalSeconds);
                bool cameraNeedsUpdate = false;
                if (Keyboard.GetState().IsKeyDown(Keys.W))
                {
                    _camera.Y -= camSpeed;
                    cameraNeedsUpdate = true;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.S))
                {
                    _camera.Y += camSpeed;
                    cameraNeedsUpdate = true;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.A))
                {
                    _camera.X -= camSpeed;
                    cameraNeedsUpdate = true;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.D))
                {
                    _camera.X += camSpeed;
                    cameraNeedsUpdate = true;
                }
                //_camera.X = MathHelper.Clamp(_camera.X, virtualWidthOver2, ((_world.TilesWidth * Tile.Size) - virtualWidthOver2));
                //_camera.Y = MathHelper.Clamp(_camera.X, virtualHeightOver2, ((_world.TilesWidth * Tile.Size) - virtualHeightOver2));
                if (cameraNeedsUpdate)
                    _camera.UpdateTransform();
                MouseState mouseState = Mouse.GetState();
                _camera.UpdateMousePosition(mouseState);
                int mouseTileX = -1;
                int mouseTileY = -1;
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (mouseTileX == -1)
                        mouseTileX = (int)(_camera.MousePosition.X / Tile.Size);
                    if (mouseTileY == -1)
                        mouseTileY = (int)(_camera.MousePosition.Y / Tile.Size);
                    _world.SetTile(mouseTileX, mouseTileY, Tile.Types.Tile1);
                }
                if (mouseState.RightButton == ButtonState.Pressed)
                {
                    if (mouseTileX == -1)
                        mouseTileX = (int)(_camera.MousePosition.X / Tile.Size);
                    if (mouseTileY == -1)
                        mouseTileY = (int)(_camera.MousePosition.Y / Tile.Size);
                    _world.SetTile(mouseTileX, mouseTileY, Tile.Types.None);
                }
            }
            _world.Bake(_camera.X, _camera.Y, virtualWidthOver2, virtualHeightOver2);
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
            _world.Draw(spriteBatch, gameTime);
            spriteBatch.End();
            base.Draw(spriteBatch, gameTime);
        }
    }
}