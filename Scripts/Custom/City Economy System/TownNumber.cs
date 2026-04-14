using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server;
using Server.Regions;

namespace Server.Misc
{
    public enum ChunkType
    {
        Ocean,
        Plains,
        Mountain,
        City,
        ChokePoint,
        MagicNode
    }

    public class ChunkData(int x, int y, ChunkType type, string name = "")
    {
        public int X { get; } = x;
        public int Y { get; } = y;
        public ChunkType Type { get; set; } = type;
        public string Name { get; set; } = name;
        public string OwnerGuild { get; set; } = string.Empty;
        public int TollCost { get; set; } = 0;
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Gump 시각화를 위한 아이콘 매핑
        public string GetIcon() => Type switch
        {
            ChunkType.City => "🏛️",
            ChunkType.ChokePoint => "🛑",
            ChunkType.MagicNode => "🌀",
            ChunkType.Mountain => "🟫",
            ChunkType.Plains => "🟩",
            ChunkType.Ocean => "🌊",
            _ => "⬛"
        };
    }

    public static class TownNumber
    {
        // Grade 필드 및 가상 그리드(W, H)가 추가된 마을 정의 레코드
        private record TownDef(int ID, int[] Maps, int X1, int X2, int Y1, int Y2, string Name, string Grade, int GridW, int GridH);

        // 1. 대도시 사각형 박스 정의 (RegionSaver의 정밀 좌표로 100% 동기화 완료)
        private static readonly List<TownDef> m_Towns =
        [
            // [S 등급] 트라멜 브리튼 (RegionSaver: 1093~1740, 1408~1907)
            new(1,  [0],    1093, 1740, 1408, 1907, "Britain", "S", 50, 50),

            // [A 등급] 주요 대도시
            new(1,  [1],    1093, 1740, 1408, 1907, "Britain", "A", 50, 50),
            new(7,  [0, 1], 2411, 2628, 366,  690,  "Minoc", "A", 50, 20),
            new(8,  [0, 1], 4278, 4726, 844,  1509, "Moonglow", "A", 30, 40),
            new(13, [0, 1], 1796, 2117, 2636, 2954, "Trinsic", "A", 50, 30),
            new(14, [0, 1], 2728, 3065, 598,  1054, "Vesper", "A", 40, 40),
            new(1,  [3],    919,  1036, 490,  652,  "Luna", "A", 40, 40),
            new(1,  [4],    650,  816,  1192, 1400, "Zento", "A", 40, 40),
            new(1,  [5],    624,  927,  3296, 3583, "Royal City", "A", 40, 40),

            // [B 등급] 일반 마을
            new(2,  [0, 1], 2592, 2887, 2057, 2303, "Buccaneer's Den", "B", 55, 11),
            new(5,  [0, 1], 1224, 1533, 3592, 4065, "Jhelom", "B", 40, 20), // 메인 아일랜드 기준
            new(6,  [0, 1], 3624, 3812, 2032, 2303, "Magincia", "B", 25, 25),
            new(9,  [0, 1], 3475, 3835, 1000, 1435, "Nujel'm", "B", 30, 25),
            new(10, [0, 1], 3314, 3814, 2345, 3095, "Haven", "B", 30, 30),
            new(11, [0, 1], 2868, 3073, 3324, 3519, "Serpent's Hold", "B", 30, 20),
            new(12, [0, 1], 538,  688,  2107, 2297, "Skara Brae", "B", 40, 25),
            new(15, [0, 1], 5132, 5366, 3,    204,  "Wind", "B", 35, 20),
            new(16, [0, 1], 92,   756,  656,  1261, "Yew", "B", 35, 35),
            new(17, [0, 1], 5123, 5315, 3930, 4084, "Delucia", "B", 30, 32),
            new(18, [0, 1], 5639, 5851, 3095, 3318, "Papua", "B", 35, 25),
            new(1,  [2],    1448, 1632, 496,  640,  "Ancient Citadel", "B", 30, 30),

            // [C 등급] 은둔 마을
            new(3,  [0, 1], 2200, 2286, 1110, 1246, "Cove", "C", 25, 20)
        ];

        // 2. 위험/특수 구역 블랙리스트 (유저님 리스트 100% 동일)
        private static readonly string[] m_Blacklist = 
        [
            "Doom", "Solen", "Terathan", "Cave", "Dungeon", "Covetous", 
            "Wrong", "Despise", "Prism", "Bedlam", "Underworld", "Abyss", "Tartarus",
            "Khaldun", "Blighted", "Gumshoe", "Mercutio", "Paroxysmus",
            "Sanctuary", "Labyrinth", "Painted", "Tomb", "Heartwood"
        ];

        private static readonly Dictionary<int, string> m_OutpostNames = [];

        // 128x128 전략 청크 맵 시스템 데이터
        private static readonly Dictionary<(int X, int Y), ChunkData> m_StrategicChunks = [];
        public const int MaxChunkX = 56;
        public const int MaxChunkY = 32;

        static TownNumber()
        {
            InitializeStrategicMap();
        }

        private static void InitializeStrategicMap()
        {
            // 1. 맵 전체를 기본(Plains)으로 초기화
            for (int y = 0; y < MaxChunkY; y++)
            {
                for (int x = 0; x < MaxChunkX; x++)
                {
                    m_StrategicChunks[(x, y)] = new ChunkData(x, y, ChunkType.Plains);
                }
            }

            // 2. 주요 도시 (City Core)
            SetChunk(11, 12, ChunkType.City, "Britain");
            SetChunk(19, 4, ChunkType.City, "Minoc");
            SetChunk(23, 6, ChunkType.City, "Vesper");
            SetChunk(14, 21, ChunkType.City, "Trinsic");
            SetChunk(34, 8, ChunkType.City, "Moonglow");
            SetChunk(40, 25, ChunkType.City, "Delucia");
            SetChunk(44, 25, ChunkType.City, "Papua");
            SetChunk(4, 7, ChunkType.City, "Yew");
            SetChunk(17, 9, ChunkType.City, "Cove");
            SetChunk(4, 16, ChunkType.City, "Skara Brae");
            SetChunk(28, 17, ChunkType.City, "Magincia");
            SetChunk(10, 28, ChunkType.City, "Jhelom");

            // 3. 1-Chunk 절대 병목 구역 (Choke Points)
            SetChunk(21, 5, ChunkType.ChokePoint, "Vesper Bridge");
            SetChunk(12, 26, ChunkType.ChokePoint, "Delucia Passage");
            SetChunk(5, 16, ChunkType.ChokePoint, "Skara Brae Ferry");
            SetChunk(16, 9, ChunkType.ChokePoint, "Cove Pass");
            SetChunk(13, 24, ChunkType.ChokePoint, "Trinsic South Path");
            SetChunk(10, 6, ChunkType.ChokePoint, "Wind Entrance");

            // 4. 마법 및 특수 전송 제단 (Magic Nodes)
            SetChunk(35, 9, ChunkType.MagicNode, "Moonglow Recsu");
            SetChunk(45, 25, ChunkType.MagicNode, "Papua Resdu");
        }

        private static void SetChunk(int x, int y, ChunkType type, string name = "")
        {
            if (m_StrategicChunks.TryGetValue((x, y), out var chunk))
            {
                chunk.Type = type;
                chunk.Name = name;
                chunk.LastUpdated = DateTime.Now;
            }
        }

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

            if (b >= 50 && m_OutpostNames.TryGetValue(townID, out string outName))
            {
                return (m == 1 ? $"{outName} (F)" : outName, "C");
            }

            var def = m_Towns.FirstOrDefault(t => t.ID == b && t.Maps.Contains(m));
            if (def == null) return ("Unknown Outpost", "C");

            // 유저님의 특수 지명(Haven/Ocllo) 분리 판정 로직 유지
            string name = (def.ID == 10 && m == 0) ? "Haven" : (def.ID == 10 && m == 1) ? "Ocllo" : def.Name;
            
            return (m == 1 ? $"{name} (F)" : name, def.Grade);
        }

        // [추가] 가상 그리드 정보 반환 (Outpost는 0 반환)
        public static (int W, int H, int Total) GetGridInfo(int townID)
        {
            int m = townID / 100, b = townID % 100;
            if (b >= 50) return (0, 0, 0); // Outpost는 영토 없음

            var def = m_Towns.FirstOrDefault(t => t.ID == b && t.Maps.Contains(m));
            return def == null ? (0, 0, 0) : (def.GridW, def.GridH, def.GridW * def.GridH);
        }

        // [추가] 좌표를 가상 그리드 인덱스로 변환
        public static int GetMapIndex(int townID, Point3D loc)
        {
            int m = townID / 100, b = townID % 100;
            var def = m_Towns.FirstOrDefault(t => t.ID == b && t.Maps.Contains(m));
            if (def == null || b >= 50) return -1;

            double px = (double)(loc.X - def.X1) / (def.X2 - def.X1);
            double py = (double)(loc.Y - def.Y1) / (def.Y2 - def.Y1);

            int vx = (int)Math.Clamp(px * (def.GridW - 1), 0, def.GridW - 1);
            int vy = (int)Math.Clamp(py * (def.GridH - 1), 0, def.GridH - 1);

            return (vy * def.GridW) + vx;
        }

        public static int GetTotalTiles(int townID)
        {
            int m = townID / 100, b = townID % 100;
            if (b >= 50) return 0;

            var def = m_Towns.FirstOrDefault(t => t.ID == b && t.Maps.Contains(m));
            return def == null ? 0 : (def.X2 - def.X1 + 1) * (def.Y2 - def.Y1 + 1);
        }

        // [신규] 128x128 전략 청크 좌표 반환 메서드 (Point3D 기반)
        public static (bool IsValid, ChunkData? Chunk) GetStrategicChunk(Point3D loc)
        {
            int chunkX = loc.X / 128;
            int chunkY = loc.Y / 128;
            
            return GetStrategicChunk(chunkX, chunkY);
        }

        // [신규] 128x128 전략 청크 좌표 반환 메서드 (청크 X/Y 인덱스 기반)
        public static (bool IsValid, ChunkData? Chunk) GetStrategicChunk(int chunkX, int chunkY)
        {
            if (m_StrategicChunks.TryGetValue((chunkX, chunkY), out var chunk))
            {
                return (true, chunk);
            }
            return (false, null);
        }

        // [신규] Gump 출력을 위한 HTML 미니맵 렌더링
        public static string GenerateGumpHtmlMap(int startX, int startY, int width, int height)
        {
            StringBuilder sb = new();
            
            sb.AppendLine("<font face=\"courier\">");

            for (int y = startY; y < startY + height; y++)
            {
                for (int x = startX; x < startX + width; x++)
                {
                    if (m_StrategicChunks.TryGetValue((x, y), out var chunk))
                    {
                        sb.Append(chunk.GetIcon());
                    }
                    else
                    {
                        sb.Append("⬛");
                    }
                }
                sb.AppendLine("<br>");
            }
            
            sb.AppendLine("</font>");
            return sb.ToString();
        }
		// [추가] 가상 그리드 인덱스를 실제 맵 좌표(Point3D)로 역산 변환
        public static Point3D GetLocationFromIndex(int townID, int index, int defaultZ = 0)
        {
            int m = townID / 100, b = townID % 100;
            var def = m_Towns.FirstOrDefault(t => t.ID == b && t.Maps.Contains(m));
            
            // Outpost거나 유효하지 않은 타운이면 0,0,0 반환
            if (def == null || b >= 50 || def.GridW <= 0 || def.GridH <= 0) return Point3D.Zero;

            // 1D 인덱스를 2D 그리드(vx, vy) 좌표로 변환
            int vx = index % def.GridW;
            int vy = index / def.GridW;

            // 0.0 ~ 1.0 사이의 비율(px, py) 계산 (0으로 나누기 방지)
            double px = def.GridW > 1 ? (double)vx / (def.GridW - 1) : 0;
            double py = def.GridH > 1 ? (double)vy / (def.GridH - 1) : 0;

            // 실제 맵 좌표로 치환
            int x = def.X1 + (int)Math.Round(px * (def.X2 - def.X1));
            int y = def.Y1 + (int)Math.Round(py * (def.Y2 - def.Y1));

            return new Point3D(x, y, defaultZ);
        }
		// TownNumber.cs 파일 안에 아래 메서드를 추가해 주세요.
		public static Point3D GetCenter(int townID)
		{
			int m = townID / 100, b = townID % 100;
			// 정의된 리스트에서 해당 마을을 찾습니다.
			var def = m_Towns.FirstOrDefault(t => t.ID == b && t.Maps.Contains(m));
			if (def != null)
			{
				// (X1+X2)/2, (Y1+Y2)/2 공식을 사용하여 중앙점을 계산합니다.
				return new Point3D((def.X1 + def.X2) / 2, (def.Y1 + def.Y2) / 2, 0);
			}
			return Point3D.Zero;
		}
    }
}