using System;
using System.Collections.Generic;
using System.Linq;
using Server;

namespace Server.Misc
{
    public static class TownNumber
    {
        private record TownDef(int ID, int[] Maps, int X1, int X2, int Y1, int Y2, string Name);

        private static readonly List<TownDef> m_Towns = new()
        {
            // [정의 변경] 이제 0은 Trammel, 1은 Felucca입니다.
            new(1,  [0, 1], 1200, 1750, 1400, 1750, "Britain"),
            new(2,  [0, 1], 2600, 2850, 2000, 2250, "Buccaneer's Den"),
            new(3,  [0, 1], 2100, 2500, 1100, 1400, "Cove"),
            new(4,  [0],    6900, 7150, 200,  450,  "Heartwood"), // 트라멜 전용
            new(5,  [0, 1], 1100, 1600, 3500, 4100, "Jhelom"),
            new(6,  [0, 1], 3600, 3900, 2000, 2350, "Magincia"),
            new(7,  [0, 1], 2400, 2700, 350,  750,  "Minoc"),
            new(8,  [0, 1], 4350, 4750, 1000, 1450, "Moonglow"),
            new(9,  [0, 1], 3500, 3900, 1050, 1450, "Nujel'm"),
            new(10, [0, 1], 3450, 3800, 2400, 2750, "Haven"), // 0번(Trammel)일 때 Haven
            new(11, [0, 1], 2850, 3150, 3350, 3650, "Serpent's Hold"),
            new(12, [0, 1], 550,  1000, 2100, 2450, "Skara Brae"),
            new(13, [0, 1], 1750, 2200, 2600, 3050, "Trinsic"),
            new(14, [0, 1], 2700, 3100, 550,  1050, "Vesper"),
            new(15, [0, 1], 5100, 5450, 0,    300,  "Wind"),
            new(16, [0, 1], 250,  850,  700,  1600, "Yew"),
            new(17, [0, 1], 5150, 5400, 3500, 4050, "Delucia"),
            new(18, [0, 1], 5650, 5950, 3100, 3350, "Papua"),

            // 타 대륙 (기존 유지)
            new(1,  [2],    1450, 1650, 500,  650,  "Ancient Citadel"),
            new(1,  [3],    900,  1150, 450,  700,  "Luna"),
            new(1,  [4],    700,  900,  1150, 1350, "Zento"),
            new(1,  [5],    750,  1000, 3350, 3650, "Royal City")
        };

        public static int GetID(Point3D loc, Map map)
        {
            if (map == null || map == Map.Internal) return 0;

            // [핵심 매핑] 엔진 MapID 1(Tram) -> 우리 0 / 엔진 MapID 0(Fel) -> 우리 1
            int logicID = (map.MapID == 1) ? 0 : (map.MapID == 0 ? 1 : map.MapID);

            var def = m_Towns.FirstOrDefault(t => t.Maps.Contains(logicID) && 
                      loc.X >= t.X1 && loc.X <= t.X2 && loc.Y >= t.Y1 && loc.Y <= t.Y2);

            return def != null ? def.ID + (logicID * 100) : 0;
        }

        public static string GetName(int townID)
        {
            int m = townID / 100, b = townID % 100;
            var def = m_Towns.FirstOrDefault(t => t.ID == b && t.Maps.Contains(m));
            
            if (def == null) return "Wilderness";

            // 0번(트라멜)이면 Haven, 1번(펠루카)이면 Ocllo
            string name = (def.ID == 10 && m == 0) ? "Haven" : def.Name;
            if (def.ID == 10 && m == 1) name = "Ocllo";

            // 1번(펠루카)일 때만 이름 뒤에 (F) 추가
            return m == 1 ? $"{name} (F)" : name;
        }

        public static int GetTotalTiles(int townID)
        {
            int m = townID / 100, b = townID % 100;
            var def = m_Towns.FirstOrDefault(t => t.ID == b && t.Maps.Contains(m));
            return def == null ? 0 : (def.X2 - def.X1 + 1) * (def.Y2 - def.Y1 + 1);
        }
    }
}