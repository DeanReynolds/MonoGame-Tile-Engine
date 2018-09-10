using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Terraria_World.Scenes
{
    public class Game : Scene
    {
        private readonly World _world;
        private readonly Camera _camera;

        public Game(int maxPlayers)
        {
            _world = World.Generate(2100, 300);
            _camera = new Camera(new Vector2((_world.Spawn.X * Tile.Size), (_world.Spawn.Y * Tile.Size)));
        }

        public override void Update(GameTime gameTime)
        {
            float virtualWidthOver2 = (Screen.VirtualWidth / 2f);
            float virtualHeightOver2 = (Screen.VirtualHeight / 2f);
            if (Program.Game.IsActive)
            {
                float camSpeed = (float)(500 * gameTime.ElapsedGameTime.TotalSeconds);
                if (Keyboard.GetState().IsKeyDown(Keys.W))
                    _camera.Y -= camSpeed;
                if (Keyboard.GetState().IsKeyDown(Keys.S))
                    _camera.Y += camSpeed;
                if (Keyboard.GetState().IsKeyDown(Keys.A))
                    _camera.X -= camSpeed;
                if (Keyboard.GetState().IsKeyDown(Keys.D))
                    _camera.X += camSpeed;
                //_camera.X = MathHelper.Clamp(_camera.X, virtualWidthOver2, ((_world.TilesWidth * Tile.Size) - virtualWidthOver2));
                //_camera.Y = MathHelper.Clamp(_camera.X, virtualHeightOver2, ((_world.TilesWidth * Tile.Size) - virtualHeightOver2));
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
                    _world.SetTileFore(mouseTileX, mouseTileY, Tile.Fores.Grass);
                }
                if (mouseState.RightButton == ButtonState.Pressed)
                {
                    if (mouseTileX == -1)
                        mouseTileX = (int)(_camera.MousePosition.X / Tile.Size);
                    if (mouseTileY == -1)
                        mouseTileY = (int)(_camera.MousePosition.Y / Tile.Size);
                    _world.SetTileBack(mouseTileX, mouseTileY, Tile.Backs.Dirt);
                }
            }
            _world.Bake(_camera.X, _camera.Y, virtualWidthOver2, virtualHeightOver2);
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.BackToFront, null, SamplerState.PointClamp, null, null, null, null);
            _world.Draw(spriteBatch, gameTime);
            spriteBatch.End();
            base.Draw(spriteBatch, gameTime);
        }
    }
}