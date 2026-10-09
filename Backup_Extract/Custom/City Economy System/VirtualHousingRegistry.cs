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
        public List<Rectangle2D> MasterLots { get; set; } = []; // ?ŒŸ ìµœë? ë©´ì  ê±°ë? ?‰ì???
        public List<OccupiedLot> OccupiedLots { get; set; } = []; // ?ŒŸ ?¤ì œ ?…ì£¼??ì§‘ë“¤

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
        
        // 7?ë¦¬ ZoneID(?? 1101002)ë¥??¤ë¡œ ?˜ì—¬ ê±°ë????‰ì?(Master Lots)ë¥?ë³´ê??˜ëŠ” ?€
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
            
            // ?Œì¼???†ë‹¤ë©?ìµœì‹  ?Œì¼ ?´ë¦„ ?¨í„´?¼ë¡œ ?¬ê????œë„
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
                Console.WriteLine("[HousingSystem] ERROR: Housing_Master_Data.csv ?Œì¼???†ìŠµ?ˆë‹¤!");
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
                Console.WriteLine($"[HousingSystem] ë§ˆìŠ¤???¥ë? ë¡œë“œ ?„ë£Œ! (ê°€??ê³µí„° ë¬¶ìŒ {loadedLots}ê°??•ë³´)");
            }
            catch (Exception ex) { Console.WriteLine($"[HousingSystem] ë¡œë”© ?¤íŒ¨: {ex.Message}"); }
        }

        // ==============================================================================
        // ?ŒŸ [ê¸°ì¡´ ê°€???™ê¸°??ë¡œì§ ë³µêµ¬] - FreeSpaces ?˜ì¡´???„ë²½ ?œê±°
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
                Console.WriteLine($"[HousingSystem] ?œë²„ ???Œë ˆ?´ì–´ ê°€??{totalFound}ì±?ì¤?{protectedCount}ì±??ìœ  ë³´í˜¸ ì²˜ë¦¬ ?„ë£Œ.");
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
                        // OccupiedLots???±ë¡ë§??˜ë©´ ??(RecalculateFreeSpaces ?? œ)
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
                Console.WriteLine($"[HousingSystem] ?œë²„ ?¬ì‹œ???™ê¸°?? AI ê°€??{restoredCount}ì±??ìœ  ë³´í˜¸ ë³µêµ¬ ?„ë£Œ.");
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

            EcoGridChunk lastConflictChunk = null;
            Rectangle2D lastConflictCandidate = default;
            int[] zoneTypes = [2, 3];

            foreach (int zType in zoneTypes)
            {
                var matchingLots = m_MasterLotPool
                    .Where(kvp => ((kvp.Key / 10) / 100) * 100 == baseRegion && kvp.Key % 10 == zType)
                    .SelectMany(kvp => kvp.Value)
                    .ToList();

                if (matchingLots.Count == 0) continue;

                // ?ŒŸ [?˜ì • 1] 15% ?•ë¥ (?Œë°•ê¸? ?œê±°, ?¬ê¸°???•ë ¬ë¡??ì—°?¤ëŸ½ê²?ë¹??…ì„ ì°¾ì•„ ?¬ë¼ê°€?„ë¡ ? ë„
                var sortedLots = (rank <= NobilityRank.Commoner) 
                    ? matchingLots.OrderBy(l => l.Width * l.Height).ToList() // ?‰ë?: ?‘ì? ë¹ˆë?ê°€ë¶€??ì±„ìš°ê³? ê½?ì°¨ë©´ ?€??ë¶€ì§€ë¡?ì§„ì¶œ
                    : matchingLots.OrderByDescending(l => l.Width * l.Height).ToList(); // ê·€ì¡? ?€??ë¶€ì§€ ? ì 

                foreach (var masterLot in sortedLots)
                {
                    bool isSmallLot = masterLot.Width < 14 || masterLot.Height < 14;

                    // ê·€ì¡±ì? ?ˆë¬´ ì¢ì? ë¹ˆë?ê°€(14ë¯¸ë§Œ)?ëŠ” ?ˆë? ì§€ì§€ ?ŠìŒ
                    if (rank >= NobilityRank.Knight && isSmallLot) continue; 
                    if (masterLot.Width < reqW || masterLot.Height < reqH) continue; 

                    // ?ŒŸ ?ŒíŠ¸ë¦¬ìŠ¤??ë¹ˆìë¦?ì°¾ê¸°
                    for (int x = masterLot.X; x <= masterLot.X + masterLot.Width - reqW; x += 1)
                    {
                        for (int y = masterLot.Y; y <= masterLot.Y + masterLot.Height - reqH; y += 1)
                        {
                            Rectangle2D candidate = new Rectangle2D(x, y, reqW, reqH);

                            int cx = (x / 128) * 128;
                            int cy = (y / 128) * 128;
                            var chunk = Chunks.FirstOrDefault(c => c.Facet == facet && c.Bounds.X == cx && c.Bounds.Y == cy);
                            
                            // ?ŒŸ [?˜ì • 2] ì²?¬ ê²½ê³„??ë²„ê·¸ ?´ê²°: ì²?¬ê°€ ?†ìœ¼ë©??¤í‚µ?˜ì? ë§ê³  ê·??ë¦¬???ˆë¡œ ?ì„±
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
                                    isConflict = true; lastConflictChunk = chunk; lastConflictCandidate = candidate; break;
                                }

                                // ?ŒŸ [?˜ì • 3] ?ë’¤ ?¬ìœ  ê³µê°„ 5ì¹?-> 2ì¹¸ìœ¼ë¡?ì¶•ì†Œ (ë¶€ì§€ ??¹„ ê·¹ë³µ)
                                if (candidate.X < lot.Footprint.X + lot.Footprint.Width && candidate.X + candidate.Width > lot.Footprint.X)
                                {
                                    if (lot.Footprint.Y < candidate.Y)
                                    {
                                        int dist = candidate.Y - (lot.Footprint.Y + lot.Footprint.Height);
                                        if (dist >= 0 && dist < 2) { isConflict = true; lastConflictChunk = chunk; lastConflictCandidate = candidate; break; }
                                    }
                                    else
                                    {
                                        int dist = lot.Footprint.Y - (candidate.Y + candidate.Height);
                                        if (dist >= 0 && dist < 2) { isConflict = true; lastConflictChunk = chunk; lastConflictCandidate = candidate; break; }
                                    }
                                }
                            }

                            if (!isConflict)
                            {
                                // ?ë¦¬ë¥?ì°¾ì•˜?¼ë?ë¡??êµ¬ ?ìœ (Lock) ì²˜ë¦¬
                                chunk.OccupiedLots.Add(new OccupiedLot { HouseName = houseId, Footprint = candidate, IsSafeZone = true });
                                return (true, chunk, candidate);
                            }
                        }
                    }
                }
            }
            
            return (false, lastConflictChunk, lastConflictCandidate); 
        }
    }
}
