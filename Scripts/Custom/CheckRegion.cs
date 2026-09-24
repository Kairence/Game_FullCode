using System;
using Server;
namespace Server.Misc {
    public class CheckRegion {
        public static void Run() {
            Point3D cave = new Point3D(5400, 700, 0); // Inside Despise 2
            Point3D voidLoc = new Point3D(5500, 650, 0); // Outside Despise 2
            Console.WriteLine("Cave Region: " + Region.Find(cave, Map.Trammel).Name);
            Console.WriteLine("Void Region: " + Region.Find(voidLoc, Map.Trammel).Name);
        }
    }
}
