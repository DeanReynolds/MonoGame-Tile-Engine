using Microsoft.Xna.Framework;
using System;

namespace Terraria_World
{
#if WINDOWS || LINUX
    public static class Program
    {
#if WINDOWS
        internal static Game Game { get; private set; }
#endif

        [STAThread]
        static void Main()
        {
            using (var game = new Game1())
            {
#if WINDOWS
                Game = game;
#endif
                game.Run();
            }
        }
    }
#endif
}