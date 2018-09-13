using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Terraria_World
{
    public static class Profiler
    {
        public static bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                if (!value)
                    foreach (var p in _profiles.Values)
                        p.Stopwatch.Reset();
            }
        }

        public static Texture2D LowestIcon { get; internal set; }
        public static Texture2D AverageIcon { get; internal set; }
        public static Texture2D HighestIcon { get; internal set; }

        public static double TotalTime { get; private set; }

        private static Vector2 _textScale;
        private static Vector2 _iconScale;
        private static Dictionary<string, Profile> _profiles = new Dictionary<string, Profile>();
        private static bool _enabled = true;

        static double _profileResetTimer;

        static Profiler()
        {
            _textScale = new Vector2(.8f);
            _iconScale = (_textScale * .5f);
            LowestIcon = Texture2D.FromStream(Program.Game.GraphicsDevice, new MemoryStream(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABoAAAAaCAYAAACpSkzOAAAAlUlEQVRIS+1UwQ2AMAik6QJ1kybsPwIzOIJuoGkjPgwKWn7Cp0lD7trjuAROVWstOecZAIoAuSYnHjiIljs8N6JGgIhbO4noxOW7IHoc6bB0mqOIaHKZkeYoHv7wj7TXBtFnR5mlQ8QWHWJGWRz1hqhHh1QWEEtPjyC2ZRBdF1Rd2JCOTRPS/XyPLOkh9XAEWUJ1qGcHBkFPegdkD7UAAAAASUVORK5CYII=")));
            AverageIcon = Texture2D.FromStream(Program.Game.GraphicsDevice, new MemoryStream(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABoAAAAaCAYAAACpSkzOAAAAkElEQVRIS2NkoBIwMDAQYGZmvs/AwCCAxcgPjFSyhwFq0Xtc5lHNIpAFxsbG/0H02bNn4ebCxEYtwhulVAk6Y2NjUERjTVFnz54VpFocwVyLzUuwyKeWj8ApatQikvMRVeKIlBRFURyR4tpRi1ASw2jQoScIooug0aAbDTpQCIAbJ9RODNiKKZhFxFTTFKkBACPnD3qJIMGkAAAAAElFTkSuQmCC")));
            HighestIcon = Texture2D.FromStream(Program.Game.GraphicsDevice, new MemoryStream(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABoAAAAaCAYAAACpSkzOAAAAdElEQVRIS2NkoCIwNjZ+z8DAIIDFyA+MVLSHwdjY+D8u80YtwhvSQyvo8KWos2fPCoK8ShUf4TPk7Nmz4EQ1ahHWlDW0go7SFEV0YqA0WEYtwpraiAkWYtQQzNHEGEKMmlGLyI7H0aAbOkGHs+GHVE1TpAYAIO+3a8NOImoAAAAASUVORK5CYII=")));
        }

        public static void Start(string name)
        {
            if (!_enabled)
                return;
            if (!_profiles.ContainsKey(name))
                _profiles.Add(name, new Profile());
            _profiles[name].Stopwatch.Start();
        }

        public static void Stop(string name)
        {
            if (!_enabled)
                return;
            double totalMs = _profiles[name].Stopwatch.Elapsed.TotalMilliseconds;
            _profiles[name]._records[_profiles[name]._index++] = totalMs;
            if ((_profiles[name].Highest == null) || (totalMs > _profiles[name].Highest))
                _profiles[name].Highest = totalMs;
            if ((_profiles[name].Lowest == null) || (totalMs < _profiles[name].Lowest))
                _profiles[name].Lowest = totalMs;
            if (_profiles[name]._index >= _profiles[name]._records.Length)
                _profiles[name]._index = 0;
            if (_profiles[name]._recorded < _profiles[name]._records.Length)
                _profiles[name]._recorded++;
            _profiles[name].Stopwatch.Reset();
            TotalTime = Math.Max(0, (TotalTime - _profiles[name].Average));
            _profiles[name].Average = 0;
            for (var i = 0; i < _profiles[name]._recorded; i++)
                _profiles[name].Average += _profiles[name]._records[i];
            _profiles[name].Average /= _profiles[name]._recorded;
            _profiles[name].Average = Math.Round(_profiles[name].Average, 3);
            TotalTime += _profiles[name].Average;
        }

        public static void Update(GameTime time)
        {
            if (!_enabled)
                return;
            _profileResetTimer -= time.ElapsedGameTime.TotalSeconds;
            if (_profileResetTimer <= 0)
            {
                foreach (Profile p in _profiles.Values)
                    p.Lowest = p.Highest = null;
                _profileResetTimer += 5;
            }
        }

        public static void Draw(SpriteBatch spriteBatch, SpriteFont font, int screenWidth, int screenHeight)
        {
            const int iconWidth = 32;
            const int maxTimeTextWidth = 100;
            const int timeTextsWidth = ((iconWidth + maxTimeTextWidth) * 3);
            lock (_profiles)
            {
                float screenWidthOver20 = (screenWidth * .05f);
                spriteBatch.Begin();
                Vector2 textPosition = new Vector2(0, (screenHeight - (screenHeight / 20f)));
                foreach (string name in _profiles.Keys)
                {
                    Profile profile = _profiles[name];
                    textPosition.Y += 2;
                    string text = name;
                    Vector2 textSize = (font.MeasureString(text) * _textScale);
                    textPosition.X = screenWidthOver20;
                    spriteBatch.DrawString(font, text, new Vector2((textPosition.X + 1), (textPosition.Y + 1)), Color.Black, 0, new Vector2(0, (textSize.Y / 2)), _textScale, SpriteEffects.None, .000001f);
                    spriteBatch.DrawString(font, text, textPosition, Color.White, 0, new Vector2(0, (textSize.Y / 2)), _textScale, SpriteEffects.None, 0);
                    textPosition.X += (screenWidth - timeTextsWidth - screenWidthOver20);
                    spriteBatch.Draw(LowestIcon, new Vector2(textPosition.X, textPosition.Y), null, Color.White, 0, new Vector2(0, 12), _iconScale, SpriteEffects.None, 0);
                    text = string.Format("{0} ms", Math.Round((profile.Lowest ?? 0), 3));
                    textSize = (font.MeasureString(text) * _textScale);
                    textPosition.X += (iconWidth * _iconScale.X);
                    spriteBatch.DrawString(font, text, new Vector2((textPosition.X + 1), (textPosition.Y + 1)), Color.Black, 0, new Vector2(0, (textSize.Y / 2)), _textScale, SpriteEffects.None, .000001f);
                    spriteBatch.DrawString(font, text, new Vector2(textPosition.X, textPosition.Y), Color.White, 0, new Vector2(0, (textSize.Y / 2)), _textScale, SpriteEffects.None, 0);
                    textPosition.X += (maxTimeTextWidth * _textScale.X);
                    spriteBatch.Draw(AverageIcon, new Vector2(textPosition.X, textPosition.Y), null, Color.White, 0, new Vector2(0, 12), _iconScale, SpriteEffects.None, 0);
                    text = string.Format("{0} ms", profile.Average);
                    textSize = (font.MeasureString(text) * _textScale);
                    textPosition.X += (iconWidth * _iconScale.X);
                    spriteBatch.DrawString(font, text, new Vector2((textPosition.X + 1), (textPosition.Y + 1)), Color.Black, 0, new Vector2(0, (textSize.Y / 2)), _textScale, SpriteEffects.None, .000001f);
                    spriteBatch.DrawString(font, text, new Vector2(textPosition.X, textPosition.Y), Color.White, 0, new Vector2(0, (textSize.Y / 2)), _textScale, SpriteEffects.None, 0);
                    textPosition.X += (maxTimeTextWidth * _textScale.X);
                    spriteBatch.Draw(HighestIcon, new Vector2(textPosition.X, textPosition.Y), null, Color.White, 0, new Vector2(0, 12), _iconScale, SpriteEffects.None, 0);
                    text = string.Format("{0} ms", Math.Round((profile.Highest ?? 0), 3));
                    textSize = (font.MeasureString(text) * _textScale);
                    textPosition.X += (iconWidth * _iconScale.X);
                    spriteBatch.DrawString(font, text, new Vector2((textPosition.X + 1), (textPosition.Y + 1)), Color.Black, 0, new Vector2(0, (textSize.Y / 2)), _textScale, SpriteEffects.None, .000001f);
                    spriteBatch.DrawString(font, text, new Vector2(textPosition.X, textPosition.Y), Color.White, 0, new Vector2(0, (textSize.Y / 2)), _textScale, SpriteEffects.None, 0);
                    textPosition.Y -= (18 * _textScale.Y);
                }
                spriteBatch.End();
            }
        }

        internal class Profile
        {
            public Stopwatch Stopwatch;

            public double? Lowest { get; internal set; }
            public double Average { get; internal set; }
            public double? Highest { get; internal set; }

            internal byte _index;
            internal double[] _records;
            internal byte _recorded;

            public Profile()
            {
                _records = new double[50];
                Stopwatch = new Stopwatch();
            }
        }
    }
}