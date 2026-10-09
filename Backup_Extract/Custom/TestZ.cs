using System;
using Server;
namespace Server.Misc {
    public class TestZ {
        public static void Run() {
            int x = 5400; int y = 700;
            Console.WriteLine("Terrain Z: " + Map.Trammel.GetAverageZ(x, y));
            var statics = Map.Trammel.Tiles.GetStaticTiles(x, y);
            Console.WriteLine("Statics Count: " + statics.Length);
            foreach(var t in statics) {
                Console.WriteLine("Static Z: " + t.Z + " ID: " + t.ID);
            }
        }
    }
}
