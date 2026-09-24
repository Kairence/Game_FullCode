using System;
using Server;
namespace Server.Misc {
    public class CheckTiles2 {
        public static void Run() {
            int x = 5510; int y = 1805; // Inside Covetous Torture Chambers
            var statics = Map.Trammel.Tiles.GetStaticTiles(x, y);
            Console.WriteLine("Statics Count: " + statics.Length);
            foreach(var t in statics) {
                int id = t.ID & 0x3FFF;
                bool surf = TileData.ItemTable[id].Surface;
                bool imp = TileData.ItemTable[id].Impassable;
                Console.WriteLine("Static Z: " + t.Z + " ID: " + id.ToString("X") + " Surf: " + surf + " Imp: " + imp);
            }
            Console.WriteLine("Average Z: " + Map.Trammel.GetAverageZ(x, y));
        }
    }
}
