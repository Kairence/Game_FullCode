using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;
using Server.Multis;
using Server.Items;

namespace Server.Misc
{
    public class OccupiedLot
    {
        public string HouseName { get; set; }
        public Rectangle2D Footprint { get; set; }
        public bool IsSafeZone { get; set; } 
    }

    public class EcoGridChunk
    {
        public Map Facet { get; set; }
        public Point3D Location { get; set; }
        public Rectangle2D Bounds { get; set; } 
        public int ZoneID { get; set; } 
        public List<Rectangle2D> MasterLots { get; set; } = []; // 🌟 최대 면적 거대 평지들
        public List<OccupiedLot> OccupiedLots { get; set; } = []; // 🌟 실제 입주된 집들

        public EcoGridChunk(Map map, int cx, int cy, int zoneId)
        {
            Facet = map;
            Location = new Point3D(cx, cy, 0);
            ZoneID = zoneId;
            Bounds = new Rectangle2D(cx, cy, 128, 128);
        }

        public static bool CheckIntersection(Rectangle2D r1, Rectangle2D r2)
        {
            return r1.X < r2.X + r2.Width && r1.X + r1.Width > r2.X &&
                   r1.Y < r2.Y + r2.Height && r1.Y + r1.Height > r2.Y;
        }
    }

    public static class VirtualHousingRegistry
    {
        public static List<EcoGridChunk> Chunks { get; private set; } = [];
        
        // 7자리 ZoneID(예: 1101002)를 키로 하여 거대한 평지(Master Lots)를 보관하는 풀
        private static Dictionary<int, List<Rectangle2D>> m_MasterLotPool = new();

        public static void Configure()
        {
            EventSink.WorldLoad += OnWorldLoad;
            
            EventSink.ItemCreated += e => 
            { 
                if (e.Item is BaseHouse house) 
                    Timer.DelayCall(TimeSpan.FromSeconds(1.0), () => RegisterPlayerHouse(house)); 
            };

            EventSink.ItemDeleted += e => 
            { 
                if (e.Item is BaseHouse house) 
                    UnregisterPlayerHouse(house); 
            };
        }

        private static void OnWorldLoad()
        {
            LoadFromHousingMasterCsv();
            SyncExistingPlayerHouses(); 
            Timer.DelayCall(TimeSpan.FromSeconds(20.0), SyncExistingAIHouses);
        }

        public static void LoadFromHousingMasterCsv()
        {
            string path = Path.Combine(Core.BaseDirectory, "Data", "Housing_Master_Data.csv");
            
            // 파일이 없다면 최신 파일 이름 패턴으로 재검색 시도
            if (!File.Exists(path))
            {
                var dir = new DirectoryInfo(Path.Combine(Core.BaseDirectory, "Data"));
                if (dir.Exists)
                {
                    var latest = dir.GetFiles("Housing_Master_Data*.csv").OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
                    if (latest != null) path = latest.FullName;
                }
            }

            if (!File.Exists(path))
            {
                Console.WriteLine("[HousingSystem] ERROR: Housing_Master_Data.csv 파일이 없습니다!");
                return;
            }

            try 
            {
                Chunks.Clear();
                m_MasterLotPool.Clear();
                string[] lines = File.ReadAllLines(path);
                int loadedLots = 0;

                for (int i = 1; i < lines.Length; i++) 
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    string[] parts = lines[i].Split(',');
                    if (parts.Length < 7) continue;

                    string mapName = parts[0];
                    int lx = int.Parse(parts[1]), ly = int.Parse(parts[2]), lz = int.Parse(parts[3]); 
                    int w = int.Parse(parts[4]), h = int.Parse(parts[5]);
                    int zoneId = int.Parse(parts[6]);

                    Map facet = mapName switch { "Felucca" => Map.Felucca, "Trammel" => Map.Trammel, "Malas" => Map.Malas, "Tokuno" => Map.Tokuno, "TerMur" => Map.TerMur, _ => null };
                    if (facet == null) continue;

                    Rectangle2D lot = new Rectangle2D(lx, ly, w, h);

                    if (!m_MasterLotPool.ContainsKey(zoneId)) m_MasterLotPool[zoneId] = [];
                    m_MasterLotPool[zoneId].Add(lot);

                    int cx = (lx / 128) * 128;
                    int cy = (ly / 128) * 128;
                    var chunk = Chunks.FirstOrDefault(c => c.Facet == facet && c.Bounds.X == cx && c.Bounds.Y == cy);
                    if (chunk == null) { chunk = new EcoGridChunk(facet, cx, cy, zoneId); Chunks.Add(chunk); }
                    
                    chunk.MasterLots.Add(lot);
                    loadedLots++;
                }
                Console.WriteLine($"[HousingSystem] 마스터 장부 로드 완료! (가용 공터 묶음 {loadedLots}개 확보)");
            }
            catch (Exception ex) { Console.WriteLine($"[HousingSystem] 로딩 실패: {ex.Message}"); }
        }

        // ==============================================================================
        // 🌟 [기존 가옥 동기화 로직 복구] - FreeSpaces 의존성 완벽 제거
        // ==============================================================================
        private static void SyncExistingPlayerHouses()
        {
            int totalFound = 0;
            int protectedCount = 0;

            foreach (var house in BaseHouse.AllHouses)
            {
                totalFound++; 
                if (RegisterPlayerHouse(house)) protectedCount++; 
            }

            if (protectedCount > 0)
                Console.WriteLine($"[HousingSystem] 서버 내 플레이어 가옥 {totalFound}채 중 {protectedCount}채 점유 보호 처리 완료.");
        }

        private static bool RegisterPlayerHouse(BaseHouse house)
        {
            if (house == null || house.Deleted || house.Map == null || house.Region == null || house.Region.Area.Length == 0) 
                return false;

            string houseID = $"PlayerHouse_{house.Serial}";
            bool registered = false;

            foreach (Rectangle3D area in house.Region.Area)
            {
                Rectangle2D houseRect = new Rectangle2D(area.Start.X, area.Start.Y, area.Width, area.Height);
                var targetChunks = Chunks.Where(c => c.Facet == house.Map && EcoGridChunk.CheckIntersection(c.Bounds, houseRect)).ToList();

                if (targetChunks.Count == 0)
                {
                    int cx = (houseRect.X / 128) * 128;
                    int cy = (houseRect.Y / 128) * 128;
                    var newChunk = new EcoGridChunk(house.Map, cx, cy, 0); 
                    Chunks.Add(newChunk);
                    targetChunks.Add(newChunk);
                }

                foreach (var chunk in targetChunks)
                {
                    if (!chunk.OccupiedLots.Any(lot => lot.HouseName == houseID && lot.Footprint.X == houseRect.X && lot.Footprint.Y == houseRect.Y))
                    {
                        // OccupiedLots에 등록만 하면 끝 (RecalculateFreeSpaces 삭제)
                        chunk.OccupiedLots.Add(new OccupiedLot { HouseName = houseID, Footprint = houseRect, IsSafeZone = true });
                        registered = true;
                    }
                }
            }
            return registered;
        }

        private static void UnregisterPlayerHouse(BaseHouse house)
        {
            string houseID = $"PlayerHouse_{house.Serial}";
            foreach (var chunk in Chunks)
            {
                chunk.OccupiedLots.RemoveAll(lot => lot.HouseName == houseID);
            }
        }

        private static void SyncExistingAIHouses()
        {
            int restoredCount = 0;
            foreach (Item item in World.Items.Values)
            {
                if (item is VirtualEstateSign sign && sign.HouseData != null)
                {
                    int multiID = sign.HouseData.MultiID;
                    if (multiID == 0) continue;

                    var mcl = MultiData.GetComponents(multiID);
                    if (mcl == null || mcl.List.Length == 0) continue;

                    int padX = 1, padY = 5; 
                    int reqW = mcl.Width + (padX * 2);
                    int reqH = mcl.Height + (padY * 2);

                    int startX = sign.X - padX + mcl.Min.X;
                    int startY = sign.Y - padY + mcl.Min.Y;

                    Rectangle2D occBounds = new Rectangle2D(startX, startY, reqW, reqH);
                    var targetChunks = Chunks.Where(c => c.Facet == sign.Map && EcoGridChunk.CheckIntersection(c.Bounds, occBounds)).ToList();

                    foreach (var chunk in targetChunks)
                    {
                        if (!chunk.OccupiedLots.Any(lot => lot.HouseName == sign.HouseName))
                        {
                            chunk.OccupiedLots.Add(new OccupiedLot { HouseName = sign.HouseName, Footprint = occBounds });
                            restoredCount++;
                        }
                    }
                }
            }
            if (restoredCount > 0)
                Console.WriteLine($"[HousingSystem] 서버 재시작 동기화: AI 가옥 {restoredCount}채 점유 보호 복구 완료.");
        }

        public static (bool Success, EcoGridChunk Chunk, Rectangle2D Space) GetAndLockBestFreeSpace(Map facet, int reqW, int reqH, string houseId, int townID, NobilityRank rank)
        {
            var (townName, _) = TownNumber.GetInfo(townID);
            string cleanName = townName.Replace(" (F)", "").Replace("'", "").Replace(" ", "");
            string enumStr = $"{facet.Name}_Town_{cleanName}";
            
            int baseRegion = 0;
            if (Enum.TryParse(typeof(RegionCode), enumStr, true, out object parsedCode))
            {
                baseRegion = (int)parsedCode;
            }
            else
            {
                Point3D center = TownNumber.GetCenter(townID);
                var (majorCode, _) = RegionSaver.GetRegionCodes(facet, center.X, center.Y, 0);
                baseRegion = (int)majorCode;
            }

            if (baseRegion == 0) return (false, null, default);

            int[] zoneTypes = [2, 3];

            foreach (int zType in zoneTypes)
            {
                var matchingLots = m_MasterLotPool
                    .Where(kvp => ((kvp.Key / 10) / 100) * 100 == baseRegion && kvp.Key % 10 == zType)
                    .SelectMany(kvp => kvp.Value)
                    .ToList();

                if (matchingLots.Count == 0) continue;

                // 🌟 [수정 1] 15% 확률(알박기) 제거, 크기순 정렬로 자연스럽게 빈 땅을 찾아 올라가도록 유도
                var sortedLots = (rank <= NobilityRank.Commoner) 
                    ? matchingLots.OrderBy(l => l.Width * l.Height).ToList() // 평민: 작은 빈민가부터 채우고, 꽉 차면 대형 부지로 진출
                    : matchingLots.OrderByDescending(l => l.Width * l.Height).ToList(); // 귀족: 대형 부지 선점

                foreach (var masterLot in sortedLots)
                {
                    bool isSmallLot = masterLot.Width < 14 || masterLot.Height < 14;

                    // 귀족은 너무 좁은 빈민가(14미만)에는 절대 지지 않음
                    if (rank >= NobilityRank.Knight && isSmallLot) continue; 
                    if (masterLot.Width < reqW || masterLot.Height < reqH) continue; 

                    // 🌟 테트리스식 빈자리 찾기
                    for (int x = masterLot.X; x <= masterLot.X + masterLot.Width - reqW; x += 1)
                    {
                        for (int y = masterLot.Y; y <= masterLot.Y + masterLot.Height - reqH; y += 1)
                        {
                            Rectangle2D candidate = new Rectangle2D(x, y, reqW, reqH);

                            int cx = (x / 128) * 128;
                            int cy = (y / 128) * 128;
                            var chunk = Chunks.FirstOrDefault(c => c.Facet == facet && c.Bounds.X == cx && c.Bounds.Y == cy);
                            
                            // 🌟 [수정 2] 청크 경계선 버그 해결: 청크가 없으면 스킵하지 말고 그 자리에 새로 생성
                            if (chunk == null)
                            {
                                chunk = new EcoGridChunk(facet, cx, cy, (baseRegion * 10) + zType); 
                                Chunks.Add(chunk);
                            }

                            bool isConflict = false;
                            foreach (var lot in chunk.OccupiedLots)
                            {
                                if (EcoGridChunk.CheckIntersection(lot.Footprint, candidate))
                                {
                                    isConflict = true; break;
                                }

                                // 🌟 [수정 3] 앞뒤 여유 공간 5칸 -> 2칸으로 축소 (부지 낭비 극복)
                                if (candidate.X < lot.Footprint.X + lot.Footprint.Width && candidate.X + candidate.Width > lot.Footprint.X)
                                {
                                    if (lot.Footprint.Y < candidate.Y)
                                    {
                                        int dist = candidate.Y - (lot.Footprint.Y + lot.Footprint.Height);
                                        if (dist >= 0 && dist < 2) { isConflict = true; break; }
                                    }
                                    else
                                    {
                                        int dist = lot.Footprint.Y - (candidate.Y + candidate.Height);
                                        if (dist >= 0 && dist < 2) { isConflict = true; break; }
                                    }
                                }
                            }

                            if (!isConflict)
                            {
                                // 자리를 찾았으므로 영구 점유(Lock) 처리
                                chunk.OccupiedLots.Add(new OccupiedLot { HouseName = houseId, Footprint = candidate, IsSafeZone = true });
                                return (true, chunk, candidate);
                            }
                        }
                    }
                }
            }
            
            return (false, null, default); 
        }
    }
}