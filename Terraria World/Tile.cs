namespace Terraria_World
{
    public struct Tile
    {
        public const int Size = 16;
        public const int BackTextureSize = 34;
        public const int BackTextureSizeOver2 = (BackTextureSize / 2);

        public Fores Fore;
        public byte ForeUV;
        public byte ForeVariation;
        public Backs Back;
        public byte BackUV;
        public byte BackVariation;

        public enum Fores : ushort { None, Dirt, Stone, Grass }
        public enum Backs : ushort { None, Dirt, Stone }
    }
}