using System;
using Server;
namespace Server.Misc {
    public class CheckTiles {
        public static void Run() {
            for (int i=0; i<10; i++) {
                int x = 5400 + i; int y = 700 + i;
                Console.WriteLine("Land ID: " + (Map.Trammel.Tiles.GetLandTile(x, y).ID & 0x3FFF).ToString("X"));
            }
        }
    }
}
