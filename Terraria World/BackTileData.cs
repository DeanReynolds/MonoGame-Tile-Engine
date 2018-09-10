using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria_World
{
    public class BackTileData
    {
        public static Rectangle[][] UV = new Rectangle[][] {
            new[] { Source(9, 3), Source(10, 3), Source(11, 3) },    // 0: no tile
            new[] { Source(6, 3), Source(7, 3), Source(8, 3) },      // 1: tile up
            new[] { Source(9, 0), Source(9, 1), Source(9, 2) },      // 2: tile right
            new[] { Source(0, 4), Source(2, 4), Source(4, 4) },      // 3: tile up & right
            new[] { Source(6, 0), Source(7, 0), Source(8, 0) },      // 4: tile down
            new[] { Source(5, 0), Source(5, 1), Source(5, 2) },      // 5: tile up & down
            new[] { Source(0, 3), Source(2, 3), Source(4, 3) },      // 6: tile right & down
            new[] { Source(0, 0), Source(0, 1), Source(0, 2) },      // 7: tile up & right & down
            new[] { Source(12, 0), Source(12, 1), Source(12, 2) },   // 8: tile left
            new[] { Source(1, 4), Source(3, 4), Source(5, 4) },      // 9: tile up & left
            new[] { Source(6, 4), Source(7, 4), Source(8, 4) },      // 10: tile right & left
            new[] { Source(1, 2), Source(2, 2), Source(3, 2) },      // 11: tile up & right & left
            new[] { Source(1, 3), Source(3, 3), Source(5, 3) },      // 12: tile down & left
            new[] { Source(4, 0), Source(4, 1), Source(4, 2) },      // 13: tile up & down & left
            new[] { Source(1, 0), Source(2, 0), Source(3, 0) },      // 14: tile right & down & left
            new[] { Source(1, 1), Source(2, 1), Source(3, 1) },      // 15: tile up & right & down & left
        };

        public static Rectangle Source(int x, int y)
        {
            return new Rectangle((x * 36), (y * 36), 34, 34);
        }

        public Texture2D Texture;
        
        public BackTileData(Texture2D texture)
        {
            Texture = texture;
        }
    }
}