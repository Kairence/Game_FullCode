using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Regions;

namespace Server.Misc
{
    public static class TownNumber
    {
        // Grade 필드가 추가된 마을 정의 레코드
        private record TownDef(int ID, int[] Maps, int X1, int X2, int Y1, int Y2, string Name, string Grade);

        // 1. 대도시 사각형 박스 정의 (유저님의 21개 마을 데이터 완벽 복구)
        private static readonly List<TownDef> m_Towns =
        [
            // [S 등급] 트라멜 브리튼 전용
            new(1,  [0],    1200, 1750, 1400, 1750, "Britain", "S"),

            // [A 등급] 주요 대도시 (펠루카 브리튼 포함)
            new(1,  [1],    1200, 1750, 1400, 1750, "Britain", "A"),
            new(7,  [0, 1], 2400, 2700, 350,  750,  "Minoc", "A"),
            new(8,  [0, 1], 4350, 4750, 1000, 1450, "Moonglow", "A"),
            new(13, [0, 1], 1750, 2200, 2600, 3050, "Trinsic", "A"),
            new(14, [0, 1], 2700, 3100, 550,  1050, "Vesper", "A"),
            new(1,  [3],    900,  1150, 450,  700,  "Luna", "A"),
            new(1,  [4],    700,  900,  1150, 1350, "Zento", "A"),
            new(1,  [5],    750,  1000, 3350, 3650, "Royal City", "A"),

            // [B 등급] 일반 마을
            new(2,  [0, 1], 2600, 2850, 2000, 2250, "Buccaneer's Den", "B"),
            new(5,  [0, 1], 1100, 1600, 3500, 4100, "Jhelom", "B"),
            new(6,  [0, 1], 3600, 3900, 2000, 2350, "Magincia", "B"),
            new(9,  [0, 1], 3500, 3900, 1050, 1450, "Nujel'm", "B"),
            new(10, [0, 1], 3450, 3800, 2400, 2750, "Haven", "B"),
            new(11, [0, 1], 2850, 3150, 3350, 3650, "Serpent's Hold", "B"),
            new(12, [0, 1], 550,  1000, 2100, 2450, "Skara Brae", "B"),
            new(15, [0, 1], 5100, 5450, 0,    300,  "Wind", "B"),
            new(16, [0, 1], 250,  850,  700,  1600, "Yew", "B"),
            new(17, [0, 1], 5150, 5400, 3500, 4050, "Delucia", "B"),
            new(18, [0, 1], 5650, 5950, 3100, 3350, "Papua", "B"),
            new(1,  [2],    1450, 1650, 500,  650,  "Ancient Citadel", "B"),

            // [C 등급] 은둔 마을
            new(3,  [0, 1], 2100, 2500, 1100, 1400, "Cove", "C")
        ];

        // 2. 위험/특수 구역 블랙리스트 (유저님 리스트 100% 동일)
        private static readonly string[] m_Blacklist = 
        [
            "Doom", "Solen", "Terathan", "Cave", "Dungeon", "Covetous", 
            "Wrong", "Despise", "Prism", "Bedlam", "Underworld", "Abyss", "Tartarus",
            "Khaldun", "Blighted", "Gumshoe", "Mercutio", "Paroxysmus",
            "Sanctuary", "Labyrinth", "Painted", "Tomb", "Heartwood"
        ];

        private static readonly Dictionary<int, string> m_OutpostNames = new();

        public static int GetID(Point3D loc, Map map)
        {
            if (map == null || map == Map.Internal) return 0;
            int logicID = (map.MapID == 1) ? 0 : (map.MapID == 0 ? 1 : map.MapID);

            // [1순위] 대도시 사각형 영역 검색
            var def = m_Towns.FirstOrDefault(t => t.Maps.Contains(logicID) && 
                      loc.X >= t.X1 && loc.X <= t.X2 && loc.Y >= t.Y1 && loc.Y <= t.Y2);

            if (def != null) return def.ID + (logicID * 100);

            Region reg = Region.Find(loc, map);
            string rName = reg != null && !string.IsNullOrEmpty(reg.Name) ? reg.Name : "Wilderness";

            // [2순위] 블랙리스트 검열
            if (m_Blacklist.Any(b => rName.Contains(b, StringComparison.OrdinalIgnoreCase))) return 0;

            // [3순위] 본성 키워드 기반 자동 편입
            foreach (var t in m_Towns)
            {
                if (rName.Contains(t.Name, StringComparison.OrdinalIgnoreCase) && t.Maps.Contains(logicID))
                    return t.ID + (logicID * 100);
            }

            if (rName.Contains("Ocllo", StringComparison.OrdinalIgnoreCase) && logicID == 1) return 110;

            // [4순위] 전초기지(Outpost) 번호 부여 (유저님 방식의 해시 루프 유지)
            int stableHash = 0;
            foreach (char c in rName)
            {
                stableHash = (stableHash * 31) + c;
            }
            
            int outpostBaseID = 50 + (Math.Abs(stableHash) % 45); 
            int finalOutpostID = outpostBaseID + (logicID * 100);

            m_OutpostNames[finalOutpostID] = rName;
            return finalOutpostID;
        }

		public static string GetName(int townID)
		{
			return GetInfo(townID).Name;
		}

		// 등급만 따로 필요한 경우를 위한 헬퍼
		public static string GetGrade(int townID)
		{
			return GetInfo(townID).Grade;
		}

        // 유저님의 GetName 로직을 튜플 형태로 확장 (이름 + 등급 반환)
        public static (string Name, string Grade) GetInfo(int townID)
        {
            int m = townID / 100, b = townID % 100;

            if (b >= 50 && m_OutpostNames.ContainsKey(townID))
            {
                string outName = m_OutpostNames[townID];
                return (m == 1 ? $"{outName} (F)" : outName, "C");
            }

            var def = m_Towns.FirstOrDefault(t => t.ID == b && t.Maps.Contains(m));
            if (def == null) return ("Unknown Outpost", "C");

            // 유저님의 특수 지명(Haven/Ocllo) 분리 판정 로직 유지
            string name = (def.ID == 10 && m == 0) ? "Haven" : (def.ID == 10 && m == 1) ? "Ocllo" : def.Name;
            
            return (m == 1 ? $"{name} (F)" : name, def.Grade);
        }

        public static int GetTotalTiles(int townID)
        {
            int m = townID / 100, b = townID % 100;
            if (b >= 50) return 0;

            var def = m_Towns.FirstOrDefault(t => t.ID == b && t.Maps.Contains(m));
            return def == null ? 0 : (def.X2 - def.X1 + 1) * (def.Y2 - def.Y1 + 1);
        }
    }
}