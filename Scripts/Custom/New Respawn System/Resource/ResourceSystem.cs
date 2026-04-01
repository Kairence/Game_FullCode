using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;
using Server.Items;
using Server.Regions;
using Server.Mobiles;

namespace Server.Misc
{
    #region [1] Enums & Structs (열거형 및 구조체/레코드)
    
    // 자원 종류 및 환경
    public enum ResourceType { Mining, Lumberjacking, Fishing, Tanning, Farming }
    public enum LocationType { Normal, Mine, Forest, DeepSea, Farm_Island, Farm_Remote }
    public enum WaterType { River, Coastal, Ocean }
    public enum OutpostType { FarmStake, MiningCamp, LumberTent, FishingBuoy, TanningRack }

    // 고유 식별 키 및 데이터 레코드
    public readonly record struct ResourceKey(string MapName, string RegionName, ResourceType Type);
    public readonly record struct EcoChunkKey(Map Facet, int ChunkX, int ChunkY);
    public readonly record struct EcoChunkData(
        int CenterX, int CenterY, RegionCode Code, 
        int OreCap, int WoodCap, int FishCap, int FarmCap, int TanCap
    );

    // 자원 상세 정의
    public class ResourceDef
    {
        public Type ItemType { get; set; }
        public double MinSkill { get; set; }
        public double MaxSkill { get; set; }
        public LocationType ReqLoc { get; set; }
        public int Weight { get; set; }

        public ResourceDef(Type type, double min, double max, LocationType reqLoc, int weight)
        {
            ItemType = type; 
            MinSkill = min; 
            MaxSkill = max; 
            ReqLoc = reqLoc; 
            Weight = weight;
        }
    }

    #endregion

    #region [2] Core Entities (핵심 데이터 객체)

    // 🌟 1. ResourcePool (각 지역별 자원 웅덩이)
    public class ResourcePool
    {
        public string MapName { get; set; }
        public string RegionName { get; set; }
        public ResourceType Type { get; set; }
        public LocationType LocType { get; set; }
        public int MaxCapacity { get; set; }
        public int CurrentCapacity { get; set; }
        public int SizeCategory { get; set; } 

        public Dictionary<Type, int> AvailableResources { get; set; } = new();
        public DateTime DepletionCooldown { get; set; } = DateTime.MinValue;

        public ResourcePool(string mapName, string regionName, ResourceType type, LocationType locType, int max, int size)
        {
            MapName = mapName; RegionName = regionName; Type = type; LocType = locType;
            MaxCapacity = max; CurrentCapacity = 0; SizeCategory = Math.Max(1, size);
            
            RollActiveResources(); 

            // 초기 용량 세팅
            int startingCap = MaxCapacity;
            if (Type == ResourceType.Fishing) startingCap = MaxCapacity / 20;
            else if (Type == ResourceType.Lumberjacking) startingCap = MaxCapacity / 2;

            CurrentCapacity = startingCap;
            DistributeCapacity(startingCap);
        }

        private void DistributeCapacity(int amount)
        {
            if (amount <= 0 || AvailableResources.Count == 0) return;

            var activeDefs = new List<Tuple<ResourceDef, int>>();
            foreach (Type t in AvailableResources.Keys.ToList())
            {
                var def = ResourceManager.GetDef(Type, t);
                if (def != null) activeDefs.Add(new Tuple<ResourceDef, int>(def, GetLocalWeight(def)));
            }

            int totalW = activeDefs.Sum(d => d.Item2);
            if (totalW <= 0)
            {
                int perItem = amount / AvailableResources.Count;
                foreach(var key in AvailableResources.Keys.ToList()) AvailableResources[key] += perItem;
                return;
            }

            for (int i = 0; i < amount; i++)
            {
                int r = Utility.Random(totalW);
                int c = 0;
                foreach (var tuple in activeDefs)
                {
                    c += tuple.Item2;
                    if (r < c) { AvailableResources[tuple.Item1.ItemType]++; break; }
                }
            }
        }

        public bool CanGather() => CurrentCapacity > 0 && DateTime.Now >= DepletionCooldown;

        private int GetLocalWeight(ResourceDef def)
        {
            int weight = def.Weight;
            string rName = RegionName.ToLower();
            string typeName = def.ItemType.Name.ToLower();

            if (Type == ResourceType.Lumberjacking)
            {
                bool isArctic = rName.Contains("ice") || rName.Contains("snow") || rName.Contains("glacier") || rName.Contains("dagger") || rName.Contains("winter");
                if (isArctic) { if (typeName.Contains("frostwood")) return weight * 20; if (typeName == "log") return weight; return 0; }
                else if (typeName.Contains("frostwood")) return 0; 

                if (rName.Contains("yew") && typeName.Contains("yew")) return weight * 15;
                if ((rName.Contains("swamp") || rName.Contains("bog") || rName.Contains("blood") || rName.Contains("dark")) && typeName.Contains("bloodwood")) return weight * 15;
                if ((rName.Contains("spirit") || rName.Contains("elf") || rName.Contains("wisp") || MapName == "Ilshenar") && typeName.Contains("heartwood")) return weight * 15;
                if ((rName.Contains("fire") || rName.Contains("ash") || rName.Contains("desert")) && typeName.Contains("ash")) return weight * 15;
            }
            return weight;
        }

        public void RollActiveResources()
        {
            AvailableResources.Clear();
            if (!ResourceManager.Defs.ContainsKey(Type)) return;

            List<ResourceDef> validDefs = ResourceManager.Defs[Type].Where(d => d.ReqLoc == LocationType.Normal || d.ReqLoc == LocType).ToList();
            if (validDefs.Count == 0) return;

            ResourceDef baseDef = validDefs[0]; 
            AvailableResources[baseDef.ItemType] = 0;

            int extraTypesCount = Utility.RandomMinMax(0, 2); 
            List<Tuple<ResourceDef, int>> localDefs = new List<Tuple<ResourceDef, int>>();
            foreach (var def in validDefs.Skip(1)) { int w = GetLocalWeight(def); if (w > 0) localDefs.Add(new Tuple<ResourceDef, int>(def, w)); }

            for (int i = 0; i < extraTypesCount && localDefs.Count > 0; i++)
            {
                int totalWeight = localDefs.Sum(d => d.Item2);
                int roll = Utility.Random(totalWeight);
                int cur = 0;
                for (int j = 0; j < localDefs.Count; j++)
                {
                    cur += localDefs[j].Item2;
                    if (roll < cur) { AvailableResources[localDefs[j].Item1.ItemType] = 0; localDefs.RemoveAt(j); break; }
                }
            }
        }

        public void Regenerate(int tickAmount)
        {
            if (DateTime.Now < DepletionCooldown) return;
            if (CurrentCapacity <= 0) RollActiveResources();
            if (CurrentCapacity >= MaxCapacity) return;

            int restoredAmount = Math.Min(MaxCapacity - CurrentCapacity, tickAmount / SizeCategory);
            if (restoredAmount <= 0) return;

            CurrentCapacity += restoredAmount;
            DistributeCapacity(restoredAmount); 
        }

        public int ConsumeResource(Type itemType, int amount = 1)
        {
            if (CurrentCapacity <= 0) return 0; 

            int actualAmount = Math.Min(CurrentCapacity, amount);

            if (!AvailableResources.ContainsKey(itemType)) 
                AvailableResources[itemType] = 0;

            AvailableResources[itemType] -= actualAmount;
            if (AvailableResources[itemType] < 0) AvailableResources[itemType] = 0; 

            CurrentCapacity -= actualAmount;

            ResourceTracker.Record(Type, actualAmount); 

            if (CurrentCapacity <= 0)
            {
                CurrentCapacity = 0;
                DepletionCooldown = DateTime.Now.AddMinutes(30.0); 
                AvailableResources.Clear(); 
            }
            
            return actualAmount; 
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(1); 
            writer.Write(DepletionCooldown); 
            writer.Write(CurrentCapacity);
            writer.Write(AvailableResources.Count);
            foreach (var kvp in AvailableResources) { writer.Write(kvp.Key.FullName); writer.Write(kvp.Value); }
        }

        public void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();
            if (version >= 1) DepletionCooldown = reader.ReadDateTime(); 
            CurrentCapacity = reader.ReadInt();
            int count = reader.ReadInt();
            AvailableResources.Clear();
            for (int i = 0; i < count; i++)
            {
                Type type = ScriptCompiler.FindTypeByFullName(reader.ReadString());
                int amount = reader.ReadInt();
                if (type != null) AvailableResources[type] = amount;
            }
            if (CurrentCapacity > MaxCapacity) CurrentCapacity = MaxCapacity;
        }
    }

    // 🌟 2. OutpostInfo (전초기지 데이터)
    public class OutpostInfo
    {
        public Mobile Owner { get; set; }
        public OutpostType Type { get; set; }
        public Point3D Location { get; set; }
        public Map Facet { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastRefreshed { get; set; }

        public bool IsDecayed => DateTime.Now - LastRefreshed > TimeSpan.FromDays(7.0);

        public OutpostInfo(Mobile owner, OutpostType type, Point3D loc, Map map)
        {
            Owner = owner; Type = type; Location = loc; Facet = map;
            CreatedOn = DateTime.Now; LastRefreshed = DateTime.Now;
        }

        public void Refresh() => LastRefreshed = DateTime.Now;

        public void Serialize(GenericWriter writer)
        {
            writer.Write(0); writer.Write(Owner); writer.Write((int)Type);
            writer.Write(Location); writer.Write(Facet); writer.Write(CreatedOn); writer.Write(LastRefreshed);
        }

        public OutpostInfo(GenericReader reader)
        {
            int version = reader.ReadInt(); Owner = reader.ReadMobile(); Type = (OutpostType)reader.ReadInt();
            Location = reader.ReadPoint3D(); Facet = reader.ReadMap(); CreatedOn = reader.ReadDateTime(); LastRefreshed = reader.ReadDateTime();
        }
    }

    #endregion

    #region [3] Trackers & Databases (추적기 및 DB)

    public static class ResourceTracker
    {
        public static Dictionary<ResourceType, long> HarvestedAmount { get; set; } = new();

        public static void Record(ResourceType type, int amount)
        {
            if (!HarvestedAmount.ContainsKey(type)) HarvestedAmount[type] = 0;
            HarvestedAmount[type] += amount;
        }
    }

    public static class EcoGridDatabase
    {
        public static Dictionary<EcoChunkKey, EcoChunkData> Chunks { get; private set; } = new();

        public static void Initialize()
        {
            string filePath = Path.Combine(Core.BaseDirectory, "Data", "EcoGrid_Master_AllMaps.csv");
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[EcoGrid] ⚠️ 마스터 CSV 파일을 찾을 수 없습니다: {filePath}");
                return;
            }

            Chunks.Clear();
            using (StreamReader reader = new(filePath))
            {
                string line;
                bool isFirstLine = true;
                while ((line = reader.ReadLine()) != null)
                {
                    if (isFirstLine || string.IsNullOrWhiteSpace(line)) { isFirstLine = false; continue; }
                    string[] data = line.Split(',');
                    if (data.Length < 11) continue;

                    try
                    {
                        Map map = Map.Parse(data[0]);
                        int cx = int.Parse(data[1]);
                        int cy = int.Parse(data[2]);
                        RegionCode code = (RegionCode)Enum.Parse(typeof(RegionCode), data[5]);
                        
                        EcoChunkData chunkData = new(
                            int.Parse(data[3]), int.Parse(data[4]), code,
                            int.Parse(data[6]), int.Parse(data[7]), int.Parse(data[8]), 
                            int.Parse(data[9]), int.Parse(data[10])
                        );
                        Chunks[new EcoChunkKey(map, cx, cy)] = chunkData;
                    }
                    catch { /* 무시 */ }
                }
            }
            Console.WriteLine($"[EcoGrid] 🌍 전 대륙 생태계 마스터 로드 완료! (청크: {Chunks.Count}개)");
        }

        public static string GetGridRegionName(Map map, int chunkX, int chunkY, RegionCode code) => $"Chunk_{chunkX}_{chunkY}_{code}";

        public static (bool IsValid, EcoChunkData Data) GetChunkAt(Map map, int x, int y)
        {
            if (map == null || map == Map.Internal) return (false, default);
            EcoChunkKey key = new(map, x / 128, y / 128);
            if (Chunks.ContainsKey(key)) return (true, Chunks[key]);
            return (false, default);
        }
    }

    #endregion

    #region [4] Managers (매니저 코어)

    public static class OutpostManager
    {
        public static List<OutpostInfo> Outposts { get; private set; } = new();

        public static void Configure()
        {
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
        }

        public static void RegisterOutpost(Mobile owner, OutpostType type, Point3D loc, Map map) => Outposts.Add(new OutpostInfo(owner, type, loc, map));
        public static void RemoveOutpost(OutpostInfo info) => Outposts.Remove(info);

        private static void OnSave(WorldSaveEventArgs e)
        {
            string folder = Path.Combine(Core.BaseDirectory, "Saves", "ResourceManagement");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            using (FileStream stream = new FileStream(Path.Combine(folder, "Outposts.bin"), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryFileWriter writer = new BinaryFileWriter(stream, true); 
                writer.Write(0); 
                Outposts.RemoveAll(o => o.IsDecayed || o.Owner == null || o.Owner.Deleted);
                writer.Write(Outposts.Count);
                foreach (var info in Outposts) info.Serialize(writer);
                writer.Close();
            }
        }

        private static void OnLoad()
        {
            string filePath = Path.Combine(Core.BaseDirectory, "Saves", "ResourceManagement", "Outposts.bin");
            if (!File.Exists(filePath)) return;

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                BinaryFileReader reader = new BinaryFileReader(new BinaryReader(stream)); 
                int version = reader.ReadInt();
                int count = reader.ReadInt();
                for (int i = 0; i < count; i++) Outposts.Add(new OutpostInfo(reader));
                reader.Close(); 
            }
        }
    }

    public static class ResourceManager
    {
        public static Dictionary<ResourceKey, ResourcePool> Pools { get; private set; } = new();
        public static Dictionary<ResourceType, List<ResourceDef>> Defs { get; private set; } = new();
        private static ResourceTimer m_Timer;

        private readonly record struct TierTemplate(double Min, double Max, int Weight);
        private static readonly TierTemplate[] m_Tiers = {
            new(0.0, 50.0, 500), new(20.0, 70.0, 200), new(40.0, 90.0, 100),
            new(60.0, 110.0, 50), new(80.0, 130.0, 25), new(100.0, 150.0, 10), new(120.0, 170.0, 5)
        };

        private static readonly int[] m_WaterTiles = new int[] { 0x00A8, 0x00AB, 0x0136, 0x0137 };

        public static void Configure()
        {
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
        }

        public static void Initialize()
        {
            SetupDefinitions();
            RegisterAllRegions(); 
            RegisterAllWater();
            RegisterFarmingRegions(); 

            EcoGridDatabase.Initialize();
            GeneratePoolsFromEcoGrid();

            m_Timer = new ResourceTimer();
            m_Timer.Start();
        }

        private static void SetupDefinitions()
        {
            Defs[ResourceType.Mining] = BuildDefs(new[] { typeof(IronOre), typeof(CopperOre), typeof(BronzeOre), typeof(GoldOre), typeof(AgapiteOre), typeof(VeriteOre), typeof(ValoriteOre) }, LocationType.Mine, 4);
            Defs[ResourceType.Lumberjacking] = BuildDefs(new[] { typeof(Log), typeof(OakLog), typeof(AshLog), typeof(YewLog), typeof(HeartwoodLog), typeof(BloodwoodLog), typeof(FrostwoodLog) }, LocationType.Forest, 4);
            Defs[ResourceType.Fishing] = BuildDefs(new[] { typeof(Fish), typeof(Fish), typeof(Fish), typeof(Fish), typeof(Fish), typeof(BigFish), typeof(BigFish) }, LocationType.DeepSea, 5);
        }

        private static List<ResourceDef> BuildDefs(Type[] types, LocationType specialLoc, int specialStartIndex)
        {
            List<ResourceDef> list = new();
            for (int i = 0; i < types.Length && i < m_Tiers.Length; i++)
                list.Add(new ResourceDef(types[i], m_Tiers[i].Min, m_Tiers[i].Max, i >= specialStartIndex ? specialLoc : LocationType.Normal, m_Tiers[i].Weight));
            return list;
        }

        public static void RegisterPool(string map, string region, ResourceType type, LocationType loc, int max, int size)
        {
            ResourceKey key = new(map, region, type);
            if (!Pools.ContainsKey(key)) Pools[key] = new ResourcePool(map, region, type, loc, max, size);
        }

        private static void RegisterAllRegions()
        {
            foreach (Region r in Region.Regions)
            {
                if (r.Map == null || r.Map == Map.Internal || string.IsNullOrEmpty(r.Name)) continue;
                string lowerName = r.Name.ToLower();

                if (lowerName.Contains("farm") || lowerName.Contains("field") || lowerName.Contains("wheat") || lowerName.Contains("garden")) continue;

                bool isDungeon = r.IsPartOf(typeof(DungeonRegion)) || lowerName.Contains("dungeon") || 
                                 (DungeonManager.Zones != null && DungeonManager.Zones.Keys.Any(k => k.Contains(r.Name) || r.Name.Contains(k)));

                if (isDungeon) continue;

                LocationType locType = LocationType.Normal;
                bool isEcosystem = EcosystemManager.Zones != null && EcosystemManager.Zones.Keys.Any(k => k.Contains(r.Name) || r.Name.Contains(k));

                if (lowerName.Contains("cave") || lowerName.Contains("mine")) locType = LocationType.Mine;
                else if (lowerName.Contains("forest") || lowerName.Contains("woods") || lowerName.Contains("jungle") || isEcosystem) locType = LocationType.Forest;
                
                int baseCapacity = 1000;
                int sizeCategory = 1;
                
                if (r.Area != null && r.Area.Length > 0)
                {
                    int areaSize = r.Area[0].Width * r.Area[0].Height;
                    if (areaSize > 100000) { baseCapacity = 4000; sizeCategory = 3; } 
                    else if (areaSize > 20000) { baseCapacity = 2000; sizeCategory = 2; } 
                }

                if (locType == LocationType.Mine) RegisterPool(r.Map.Name, r.Name, ResourceType.Mining, locType, baseCapacity, sizeCategory);
                else if (locType == LocationType.Forest) RegisterPool(r.Map.Name, r.Name, ResourceType.Lumberjacking, locType, baseCapacity, sizeCategory);
            }
        }

        public static void RegisterFarmingRegions() { /* 호환성 유지 */ }

        public static void GeneratePoolsFromEcoGrid()
        {
            int newPoolsCreated = 0;
            foreach (var kvp in EcoGridDatabase.Chunks)
            {
                EcoChunkKey gridKey = kvp.Key;
                EcoChunkData data = kvp.Value;
                string gridRegionName = EcoGridDatabase.GetGridRegionName(gridKey.Facet, gridKey.ChunkX, gridKey.ChunkY, data.Code);
                string mapName = gridKey.Facet.Name;

                if (data.OreCap > 0) { RegisterPool(mapName, gridRegionName, ResourceType.Mining, LocationType.Mine, data.OreCap, data.OreCap > 1000 ? 3 : (data.OreCap > 500 ? 2 : 1)); newPoolsCreated++; }
                if (data.WoodCap > 0) { RegisterPool(mapName, gridRegionName, ResourceType.Lumberjacking, LocationType.Forest, data.WoodCap, data.WoodCap > 800 ? 3 : (data.WoodCap > 400 ? 2 : 1)); newPoolsCreated++; }
                if (data.TanCap > 0) { RegisterPool(mapName, gridRegionName, ResourceType.Tanning, LocationType.Normal, data.TanCap, data.TanCap > 800 ? 3 : (data.TanCap > 400 ? 2 : 1)); newPoolsCreated++; }
            }
            Console.WriteLine($"[ResourceManager] EcoGrid 마스터 기반 {newPoolsCreated}개의 야생 자원 풀 자동 생성 완료!");
        }

        public static bool ValidateDeepWater(Map map, int x, int y)
        {
            if (x < 0 || x >= map.Width || y < 0 || y >= map.Height) return false;
            int tileID = map.Tiles.GetLandTile(x, y).ID;
            bool water = false;
            for (int i = 0; !water && i < m_WaterTiles.Length; i += 2) water = (tileID >= m_WaterTiles[i] && tileID <= m_WaterTiles[i + 1]);
            return water;
        }

        public static WaterType GetWaterCategory(Map map, int x, int y)
        {
            if (ValidateDeepWater(map, x, y)) return WaterType.Ocean;
            int scanRange = 12; 
            for (int dx = -scanRange; dx <= scanRange; dx++)
                for (int dy = -scanRange; dy <= scanRange; dy++)
                    if (ValidateDeepWater(map, x + dx, y + dy)) return WaterType.Coastal;
            return WaterType.River;
        }

        public static string GetFishingChunk(Point3D loc, WaterType waterType)
        {
            if (waterType == WaterType.Ocean) return $"Ocean_{loc.X / 256}_{loc.Y / 256}";
            if (waterType == WaterType.Coastal) return $"Coastal_{loc.X / 192}_{loc.Y / 192}";
            return $"River_{loc.X / 128}_{loc.Y / 128}";
        }

        private static void RegisterAllWater()
        {
            Map[] maps = { Map.Felucca, Map.Trammel, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur };
            foreach (Map map in maps)
            {
                if (map == null || map == Map.Internal) continue;
                for (int x = 0; x < map.Width; x += 64)
                {
                    for (int y = 0; y < map.Height; y += 64)
                    {
                        int tileID = map.Tiles.GetLandTile(x, y).ID;
                        if (ValidateDeepWater(map, x, y) || (tileID >= 0x1797 && tileID <= 0x179C))
                        {
                            Point3D loc = new Point3D(x, y, 0);
                            Region r = Region.Find(loc, map);
                            if (r != null && r.IsPartOf(typeof(DungeonRegion))) continue;

                            WaterType wType = GetWaterCategory(map, x, y);
                            string chunkName = GetFishingChunk(loc, wType);
                            ResourceKey key = new ResourceKey(map.Name, chunkName, ResourceType.Fishing);

                            if (!Pools.ContainsKey(key))
                            {
                                LocationType locType = wType == WaterType.Ocean ? LocationType.DeepSea : LocationType.Normal;
                                int maxCap = wType == WaterType.Ocean ? 4000 : wType == WaterType.Coastal ? 2000 : 1000;
                                int sizeCat = wType == WaterType.Ocean ? 3 : wType == WaterType.Coastal ? 2 : 1;
                                Pools[key] = new ResourcePool(map.Name, chunkName, ResourceType.Fishing, locType, maxCap, sizeCat);
                            }
                        }
                    }
                }
            }
            Console.WriteLine($"[ResourceManager] 전 세계 바다 및 강 낚시터(Chunk) 스캔 완료!");
        }

        public static Type TryGatherFishing(Mobile from, Map map, Point3D loc, double skill)
        {
            if (map == null || map == Map.Internal) return null;

            Region r = Region.Find(loc, map);
            if (r != null && (r.IsPartOf(typeof(DungeonRegion)) || (r.Name != null && r.Name.ToLower().Contains("dungeon"))))
            {
                from?.SendMessage(33, "이곳의 물은 너무 탁하고 오염되어 물고기가 살 수 없습니다.");
                return null; 
            }

            WaterType wType = GetWaterCategory(map, loc.X, loc.Y);
            ResourceKey key = new ResourceKey(map.Name, GetFishingChunk(loc, wType), ResourceType.Fishing);

            if (!Pools.TryGetValue(key, out ResourcePool pool)) { from?.SendMessage(33, "이곳은 물고기가 살 수 없는 환경입니다."); return null; }
            if (!pool.CanGather()) { from?.SendMessage("이 구역의 물고기가 일시적으로 씨가 말랐습니다."); return null; }

            var possible = pool.AvailableResources.Where(kvp => kvp.Value > 0 && skill >= GetDef(ResourceType.Fishing, kvp.Key)?.MinSkill).ToList();
            if (possible.Count == 0) { from?.SendMessage("당신의 낚시 실력으로 잡을 수 있는 물고기가 없습니다."); return null; }

            int roll = Utility.Random(possible.Sum(x => x.Value)), cur = 0;
            foreach (var kvp in possible) { cur += kvp.Value; if (roll < cur) { pool.ConsumeResource(kvp.Key); return kvp.Key; } }
            return null;
        }

        public static ResourceDef GetDef(ResourceType type, Type itemType) => Defs.GetValueOrDefault(type)?.FirstOrDefault(d => d.ItemType == itemType);

        private static void OnSave(WorldSaveEventArgs e)
        {
            string path = Path.Combine(Core.BaseDirectory, "Saves", "ResourceManagement", "ResourcePools.bin");
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryFileWriter writer = new BinaryFileWriter(stream, true);
                writer.Write(1); 
                writer.Write(Pools.Count);
                foreach (var kvp in Pools) { writer.Write(kvp.Key.MapName); writer.Write(kvp.Key.RegionName); writer.Write((int)kvp.Key.Type); kvp.Value.Serialize(writer); }
                writer.Close();
            }
        }

        private static void OnLoad()
        {
            string path = Path.Combine(Core.BaseDirectory, "Saves", "ResourceManagement", "ResourcePools.bin");
            if (!File.Exists(path)) return;

            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                BinaryFileReader reader = new BinaryFileReader(new BinaryReader(stream));
                int version = reader.ReadInt(), count = reader.ReadInt();
                for (int i = 0; i < count; i++) 
                {
                    string mName = reader.ReadString(), rName = reader.ReadString();
                    ResourceType type = version >= 1 ? (ResourceType)reader.ReadInt() : ResourceType.Mining;
                    ResourceKey key = new(mName, rName, type);
                    
                    if (Pools.TryGetValue(key, out var pool)) pool.Deserialize(reader);
                    else 
                    {
                        LocationType locType = LocationType.Normal; int maxCap = 1000; int sizeCat = 1;
                        if (type == ResourceType.Fishing) { if (rName.StartsWith("Ocean")) { locType = LocationType.DeepSea; maxCap = 4000; sizeCat = 3; } else if (rName.StartsWith("Coastal")) { maxCap = 2000; sizeCat = 2; } }
                        Pools[key] = new ResourcePool(key.MapName, key.RegionName, type, locType, maxCap, sizeCat);
                        Pools[key].Deserialize(reader);
                    }
                }
                reader.Close();
            }
        }

        private class ResourceTimer : Timer 
        {
            private int m_TotalTicks = 0; 
            public ResourceTimer() : base(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0)) { Priority = TimerPriority.OneMinute; }
            
            protected override void OnTick() 
            { 
                m_TotalTicks++; 
                foreach (var pool in Pools.Values) 
                {
                    int regenAmount = 20; 
                    if (pool.Type == ResourceType.Lumberjacking)
                    {
                        regenAmount = 2; 
                        if (EcosystemManager.Zones != null && EcosystemManager.Zones.Values.Any(z => z.ZoneId.Contains(pool.RegionName) || pool.RegionName.Contains(z.ZoneId))) regenAmount += 2; 
                    }
                    else if (pool.Type == ResourceType.Fishing)
                    {
                        if (pool.LocType == LocationType.DeepSea) regenAmount += 40; 
                        if (pool.MaxCapacity > 0 && (double)pool.CurrentCapacity / pool.MaxCapacity >= 0.5 && !pool.RegionName.StartsWith("River") && Utility.RandomDouble() < 0.05) SpawnTieredPredator(pool);
                    }
                    else if (pool.Type == ResourceType.Farming) { continue; /* 농사는 다른 시스템이 제어 */ }
                    pool.Regenerate(regenAmount);
                }
            }

            private void SpawnTieredPredator(ResourcePool pool)
            {
                if (!pool.RegionName.Contains("_") || pool.Type != ResourceType.Fishing) return;
                try
                {
                    string[] parts = pool.RegionName.Split('_');
                    if (parts.Length < 3) return;

                    string waterType = parts[0];
                    int size = waterType == "Ocean" ? 256 : waterType == "Coastal" ? 192 : 128;
                    int cx = int.Parse(parts[1]) * size + (size / 2), cy = int.Parse(parts[2]) * size + (size / 2);
                    Map map = Map.Parse(pool.MapName);

                    if (map == null || map == Map.Internal) return;

                    int serpents = 0, deepSerpents = 0, krakens = 0;
                    List<Mobile> currentMonsters = new List<Mobile>();

                    IPooledEnumerable eable = map.GetMobilesInRange(new Point3D(cx, cy, 0), size / 2);
                    foreach (Mobile m in eable)
                    {
                        if (!m.Alive) continue;
                        if (m is SeaSerpent) { serpents++; currentMonsters.Add(m); }
                        else if (m is DeepSeaSerpent) { deepSerpents++; currentMonsters.Add(m); }
                        else if (m is Kraken) { krakens++; currentMonsters.Add(m); }
                    }
                    eable.Free();

                    int totalMonsters = serpents + deepSerpents + krakens;
                    Type spawnType = null;
                    double fishRatio = (double)pool.CurrentCapacity / pool.MaxCapacity;

                    if (waterType == "Ocean")
                    {
                        if (fishRatio >= 0.9 && krakens == 0) { spawnType = typeof(Kraken); if (totalMonsters >= 3 && currentMonsters.Count > 0) (currentMonsters.Find(m => m is SeaSerpent) ?? currentMonsters[0]).Delete(); }
                        else if (fishRatio >= 0.7 && deepSerpents < 2 && krakens == 0) { spawnType = typeof(DeepSeaSerpent); if (totalMonsters >= 3 && serpents > 0) currentMonsters.Find(m => m is SeaSerpent)?.Delete(); else if (totalMonsters >= 3) return; }
                        else if (fishRatio >= 0.5 && totalMonsters < 3) spawnType = typeof(SeaSerpent);
                    }
                    else if (waterType == "Coastal" && fishRatio >= 0.7 && totalMonsters < 2) spawnType = typeof(SeaSerpent);

                    if (spawnType != null)
                    {
                        BaseCreature monster = (BaseCreature)Activator.CreateInstance(spawnType);
                        monster.MoveToWorld(new Point3D(cx, cy, map.GetAverageZ(cx, cy)), map);

                        int eatenAmount = pool.MaxCapacity / 5;
                        pool.CurrentCapacity = Math.Max(0, pool.CurrentCapacity - eatenAmount);
                    }
                }
                catch { }
            }
        }
    }
    #endregion
}