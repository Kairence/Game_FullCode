using System;
using Server;
namespace Server.Misc {
    public class CheckVoidRegion {
        public static void Run() {
            int x1 = 5376; int y1 = 650; // Inside AreaBounds but might be void
            int x2 = 5420; int y2 = 680; // Might be cave
            Region r1 = Region.Find(new Point3D(x1, y1, 0), Map.Trammel);
            Region r2 = Region.Find(new Point3D(x2, y2, 0), Map.Trammel);
            Console.WriteLine("R1: " + r1.Name);
            Console.WriteLine("R2: " + r2.Name);
        }
    }
}
