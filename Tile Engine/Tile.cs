using System;

namespace Tile_Engine
{
    public struct Tile
    {
        public static readonly int Size = 16;

        public Types Type;

        public enum Types : ushort { None, Tile1, Tile2, Tile3, Tile4 }
    }
}