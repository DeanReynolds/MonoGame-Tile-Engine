﻿using Tile_Engine.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Tile_Engine.Scenes
{
    public class Game : Scene
    {
        private readonly World _world;
        private readonly Camera _camera;
        private readonly Player[] _players;

        public Game(int maxPlayers)
        {
            _world = World.Generate(400, 400);
            _camera = new Camera(new Vector2((_world.Spawn.X * Tile.Size), (_world.Spawn.Y * Tile.Size)));
            _players = new Player[maxPlayers];
            _players[0] = new Player();
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < _players.Length; i++)
                if (_players[i] != null)
                    _players[i].Update(gameTime);
            float camSpeed = (float)(500 * gameTime.ElapsedGameTime.TotalSeconds);
            if (Keyboard.GetState().IsKeyDown(Keys.W))
                _camera.Y -= camSpeed;
            if (Keyboard.GetState().IsKeyDown(Keys.S))
                _camera.Y += camSpeed;
            if (Keyboard.GetState().IsKeyDown(Keys.A))
                _camera.X -= camSpeed;
            if (Keyboard.GetState().IsKeyDown(Keys.D))
                _camera.X += camSpeed;
            float virtualWidthOver2 = (Screen.VirtualWidth / 2f);
            //_camera.X = MathHelper.Clamp(_camera.X, virtualWidthOver2, ((_world.TilesWidth * Tile.Size) - virtualWidthOver2));
            float virtualHeightOver2 = (Screen.VirtualHeight / 2f);
            //_camera.Y = MathHelper.Clamp(_camera.X, virtualHeightOver2, ((_world.TilesWidth * Tile.Size) - virtualHeightOver2));
            _world.Bake(_camera.X, _camera.Y, virtualWidthOver2, virtualHeightOver2);
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
            _world.Draw(spriteBatch, gameTime);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, _camera.Transform);
            foreach (Player player in _players)
                player?.Draw(gameTime);
            spriteBatch.End();
            base.Draw(spriteBatch, gameTime);
        }
    }
}