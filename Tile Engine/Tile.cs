using System;

namespace Tile_Engine
{
    public struct Tile
    {
        public const int Bits = 4;

        public static readonly int Size = (int)Math.Pow(Bits, 2);
        public static readonly int Modulo = (Size - 1);

        public Fores Fore;
        public Backs Back;

        public enum Fores : ushort { None, Dirt, Stone }
        public enum Backs : ushort { None, Dirt, Stone }
    }
}