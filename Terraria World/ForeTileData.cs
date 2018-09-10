using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria_World
{
    public class ForeTileData
    {
        public static Rectangle[][][] UV = new Rectangle[][][] {
            new[] {
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
            },
            new[] {
                new[] { Source(9, 3), Source(10, 3), Source(11, 3) },                       // 0: no tile
                new[] { Source(6, 3), Source(7, 3), Source(8, 3) },                         // 1: tile up
                new[] { Source(9, 0), Source(9, 1), Source(9, 2) },                         // 2: tile right
                new[] { Source(0, 4), Source(2, 4), Source(4, 4) },                         // 3: tile up & right
                new[] { Source(6, 0), Source(7, 0), Source(8, 0) },                         // 4: tile down
                new[] { Source(5, 0), Source(5, 1), Source(5, 2) },                         // 5: tile up & down
                new[] { Source(0, 3), Source(2, 3), Source(4, 3) },                         // 6: tile right & down
                new[] { Source(0, 0), Source(0, 1), Source(0, 2) },                         // 7: tile up & right & down
                new[] { Source(12, 0), Source(12, 1), Source(12, 2) },                      // 8: tile left
                new[] { Source(1, 4), Source(3, 4), Source(5, 4) },                         // 9: tile up & left
                new[] { Source(6, 4), Source(7, 4), Source(8, 4) },                         // 10: tile right & left
                new[] { Source(1, 2), Source(2, 2), Source(3, 2) },                         // 11: tile up & right & left
                new[] { Source(1, 3), Source(3, 3), Source(5, 3) },                         // 12: tile down & left
                new[] { Source(4, 0), Source(4, 1), Source(4, 2) },                         // 13: tile up & down & left
                new[] { Source(1, 0), Source(2, 0), Source(3, 0) },                         // 14: tile right & down & left
                new[] { Source(1, 1), Source(2, 1), Source(3, 1) },                         // 15: tile up & right & down & left

                new[] { Source(7, 14), Source(10, 14), Source(13, 14) },                    // 3-16: 
                new[] { Source(0, 20), Source(1, 20), Source(1, 20) },                      // 7-17: 
                new[] { Source(0, 19), Source(1, 19), Source(1, 19) },                      // 11-18: 
                new[] { Source(2, 6), Source(2, 8), Source(2, 10) },                        // 15-19: 
                
                new[] { Source(7, 12), Source(10, 12), Source(13, 12) },                    // 6-20: 
                new[] { Source(3, 20), Source(4, 20), Source(5, 20) },                      // 7-21: 
                new[] { Source(0, 18), Source(1, 18), Source(1, 18) },                      // 14-22: 
                new[] { Source(2, 5), Source(2, 7), Source(2, 9) },                         // 15-23: 
                new[] { Source(7, 13), Source(10, 13), Source(13, 13) },                    // 17-24: 
                new[] { Source(11, 0), Source(11, 1), Source(11, 2) },                      // 19-25: 
                
                new[] { Source(9, 12), Source(12, 12), Source(15, 12) },                    // 12-26: 
                new[] { Source(0, 21), Source(1, 21), Source(2, 21) },                      // 13-27: 
                new[] { Source(3, 18), Source(4, 18), Source(5, 18) },                      // 14-28: 
                new[] { Source(3, 5), Source(3, 7), Source(3, 9) },                         // 15-29: 
                new[] { Source(5, 17), Source(6, 17), Source(7, 17) },                      // 19-30: 
                new[] { Source(8, 12), Source(11, 12), Source(14, 12) },                    // 22-31: 
                new[] { Source(6, 2), Source(7, 2), Source(8, 2) },                         // 23-32: 
                new[] { Source(6, 21), Source(7, 21), Source(8, 21) },                      // 25-33: 
                
                new[] { Source(9, 14), Source(12, 14), Source(15, 14) },                    // 9-34: 
                new[] { Source(3, 19), Source(4, 19), Source(4, 19) },                      // 11-35: 
                new[] { Source(3, 21), Source(4, 21), Source(5, 21) },                      // 13-36: 
                new[] { Source(3, 6), Source(3, 8), Source(3, 10) },                        // 15-37: 
                new[] { Source(8, 14), Source(11, 14), Source(14, 14) },                    // 18-38: 
                new[] { Source(6, 1), Source(7, 1), Source(8, 1) },                         // 19-39: 
                new[] { Source(2, 17), Source(3, 17), Source(4, 17) },                      // 23-40: 
                new[] { Source(6, 20), Source(7, 20), Source(8, 20) },                      // 25-41: 
                new[] { Source(9, 13), Source(12, 13), Source(15, 13) },                    // 27-42: 
                new[] { Source(10, 0), Source(10, 1), Source(10, 2) },                      // 29-43: 
                new[] { Source(7, 18), Source(8, 18), Source(9, 18) },                      // 30-44: 
                new[] { Source(6, 19), Source(7, 19), Source(8, 19) },                      // 32-45: 
                new[] { Source(8, 13), Source(11, 13), Source(14, 13) },                    // 33-46: 
            }
        };

        public static Rectangle Source(int x, int y) { return new Rectangle((x * 18), (y * 18), 16, 16); }

        public Texture2D Texture;
        public UVTypes UVType;
        public Tile.Fores[] AttachedTiles;

        public enum UVTypes : byte { tile1, tile2 }

        public ForeTileData(Texture2D texture, UVTypes uvType, Tile.Fores[] attachedTiles)
        {
            Texture = texture;
            UVType = uvType;
            AttachedTiles = attachedTiles;
        }
    }
}