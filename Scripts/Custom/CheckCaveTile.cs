using System;
using Server;
namespace Server.Misc {
    public class CheckCaveTile {
        public static void Run() {
            var lt1 = Map.Trammel.Tiles.GetLandTile(5400, 680); // Cave
            var lt2 = Map.Trammel.Tiles.GetLandTile(5380, 660); // Void
            Console.WriteLine("Cave Tile ID: " + (lt1.ID & 0x3FFF).ToString("X"));
            Console.WriteLine("Void Tile ID: " + (lt2.ID & 0x3FFF).ToString("X"));
        }
    }
}
