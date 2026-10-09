using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;
using Server.Items;
using Server.Regions;
using Server.Mobiles;
using Server.Spells; // 🌟 겹침/건물 내부 판별(SpellHelper)을 위해 추가

namespace Server.Misc
{
    #region [1] Enums & Structs
    public enum ResourceType { Mining, Lumberjacking, Fishing, Tanning, Farming }
    public enum LocationType { Normal, Mine, Forest, DeepSea, Farm_Island, Farm_Remote }
    public enum WaterType { River, Coastal, Ocean }
    public enum OutpostType { FarmStake, MiningCamp, LumberTent, FishingBuoy, TanningRack }

    public readonly record struct ResourceKey(string MapName, string RegionName, ResourceType Type);
    
    public readonly record struct EcoChunkKey(Map Facet, int ChunkX, int ChunkY);
    public readonly record struct EcoChunkData(
        int CenterX, int CenterY, RegionCode Code, 
        int OreCap, int WoodCap, int FishCap, int FarmCap, int TanCap
    );
    #endregion

    public class ResourceDef
    {
        public Type ItemType { get; set; }
        public double MinSkill { get; set; }
        public double MaxSkill { get; set; }
        public LocationType ReqLoc { get; set; }
        public int Weight { get; set; }

        public ResourceDef(Type type, double min, double max, LocationType reqLoc, int weight)
        {
            ItemType = type; MinSkill = min; MaxSkill = max; ReqLoc = reqLoc; Weight = weight;
        }
    }

    public class ResourcePool
    {
        public string MapName { get; set; }
        public string RegionName { get; set; }

        public Map Facet { get; set; }
        public RegionCode RCode { get; set; } 
        public int CenterX { get; set; }     
        public int CenterY { get; set; }     
        public WaterType WType { get; set; }  
        public bool IsPrivate { get; set; }   

        public ResourceType Type { get; set; }
        public LocationType LocType { get; set; }
        public int MaxCapacity { get; set; }
        public int CurrentCapacity { get; set; }
        public int SizeCategory { get; set; } 

        public Dictionary<Type, int> AvailableResources { get; set; } = new();
        public DateTime DepletionCooldown { get; set; } = DateTime.MinValue;
        
        public List<BaseCreature> ActiveMonsters { get; set; } = new();
        public bool HasSpawnedElementals { get; set; } = false;

        public ResourcePool(string mapName, string regionName, Map map, RegionCode code, int cx, int cy, WaterType wType, ResourceType type, LocationType locType, int max, int size, bool isPrivate = false)
        {
            MapName = mapName; RegionName = regionName; Facet = map; RCode = code; CenterX = cx; CenterY = cy; WType = wType;
            Type = type; LocType = locType; MaxCapacity = max; SizeCategory = Math.Max(1, size); IsPrivate = isPrivate;
            
            RollActiveResources(); 
            int startingCap = Type == ResourceType.Fishing ? MaxCapacity / 20 : (Type == ResourceType.Lumberjacking ? MaxCapacity / 2 : MaxCapacity);
            CurrentCapacity = startingCap;
            DistributeCapacity(startingCap);
        }

        public ResourcePool(string mapName, string regionName, Map map, RegionCode code, int cx, int cy, ResourceType type, LocationType locType, int max, int size, bool isPrivate)
            : this(mapName, regionName, map, code, cx, cy, WaterType.River, type, locType, max, size, isPrivate)
        {
        }

        private void DistributeCapacity(int amount)
        {
            if (amount <= 0 || AvailableResources.Count == 0) return;
            var activeDefs = AvailableResources.Keys.Select(t => ResourceManager.GetDef(Type, t)).Where(d => d != null).Select(d => new Tuple<ResourceDef, int>(d, GetLocalWeight(d))).ToList();
            int totalW = activeDefs.Sum(d => d.Item2);
            if (totalW <= 0) { int perItem = amount / AvailableResources.Count; foreach(var key in AvailableResources.Keys.ToList()) AvailableResources[key] += perItem; return; }

            for (int i = 0; i < amount; i++)
            {
                int r = Utility.Random(totalW), c = 0;
                foreach (var tuple in activeDefs) { c += tuple.Item2; if (r < c) { AvailableResources[tuple.Item1.ItemType]++; break; } }
            }
        }

        public bool CanGather() => 
            NewSpawnManager.ActiveMaps.GetValueOrDefault(Facet, true) && 
            CurrentCapacity > 0 && DateTime.Now >= DepletionCooldown;

        private int GetLocalWeight(ResourceDef def)
        {
            int weight = def.Weight;
            
            string rName = RegionName.ToLower(); 
            string typeName = def.ItemType.Name.ToLower();

            bool isArctic = rName.Contains("ice") || rName.Contains("snow") || rName.Contains("winter") || rName.Contains("glacier") || rName.Contains("dagger");
            bool isVolcanic = rName.Contains("hythloth") || rName.Contains("fire") || rName.Contains("volcano") || rName.Contains("destard") || rName.Contains("doom") || rName.Contains("inferno");
            bool isSwamp = rName.Contains("swamp") || rName.Contains("bog") || rName.Contains("blight");
            bool isMystic = rName.Contains("spirit") || rName.Contains("wisp") || rName.Contains("blood") || Facet.MapID == 2 || Facet.MapID == 4;
            bool isDeep = rName.Contains("khaldun") || rName.Contains("abyss") || rName.Contains("deceit") || rName.Contains("covetous") || rName.Contains("underworld");

            if (Type == ResourceType.Mining)
            {
                if (typeName.Contains("obsidian")) return isVolcanic ? weight * 20 : 0; 
                if (typeName.Contains("mithril")) return isDeep || isMystic ? weight * 15 : weight; 
            }
            else if (Type == ResourceType.Lumberjacking)
            {
                if (typeName.Contains("frostwood")) return isArctic ? weight * 20 : 0;
                if (typeName.Contains("ebony")) return isSwamp || isDeep ? weight * 20 : 0; 
                if (typeName.Contains("ethrnal")) return isMystic ? weight * 20 : 0;     
                if (typeName.Contains("bloodwood")) return rName.Contains("blood") || isVolcanic ? weight * 15 : weight;
                if (typeName.Contains("heartwood")) return isMystic ? weight * 15 : weight;
                if (typeName.Contains("yew")) return rName.Contains("yew") ? weight * 15 : weight;
            }
            else if (Type == ResourceType.Tanning)
            {
                if (typeName.Contains("polar")) return isArctic ? weight * 20 : 0; 
                if (typeName.Contains("abyssal")) return isVolcanic || isDeep ? weight * 20 : 0; 
            }

            return weight;
        }

        public void RollActiveResources()
        {
            AvailableResources.Clear();
            if (!ResourceManager.Defs.ContainsKey(Type)) return;

            var validDefs = ResourceManager.Defs[Type].Where(d => d.ReqLoc == LocationType.Normal || d.ReqLoc == LocType).ToList();
            if (validDefs.Count == 0) return;

            AvailableResources[validDefs[0].ItemType] = 0;
            
            int extraTypesCount = Utility.RandomMinMax(0, 2); 
            var localDefs = validDefs.Skip(1).Select(d => new Tuple<ResourceDef, int>(d, GetLocalWeight(d))).Where(t => t.Item2 > 0).ToList();

            for (int i = 0; i < extraTypesCount && localDefs.Count > 0; i++)
            {
                int roll = Utility.Random(localDefs.Sum(d => d.Item2)), cur = 0;
                for (int j = 0; j < localDefs.Count; j++)
                {
                    cur += localDefs[j].Item2;
                    if (roll < cur) 
                    { 
                        AvailableResources[localDefs[j].Item1.ItemType] = 0; 
                        localDefs.RemoveAt(j); 
                        break; 
                    }
                }
            }
        }

        public void Regenerate(int tickAmount)
        {
            if (!NewSpawnManager.ActiveMaps.GetValueOrDefault(Facet, true)) return;

            ActiveMonsters.RemoveAll(m => m == null || m.Deleted || !m.Alive);

            if (DateTime.Now < DepletionCooldown)
            {
                if (ActiveMonsters.Count > 0)
                {
                    foreach (var m in ActiveMonsters) m?.Delete();
                    ActiveMonsters.Clear();
                }
                return;
            }

            if (CurrentCapacity <= 0) 
            {
                CurrentCapacity = MaxCapacity / 2; 
                RollActiveResources();
                HasSpawnedElementals = false; 
            }

            int tickRegen = MaxCapacity / 100; 

            if (Type == ResourceType.Mining)
            {
                if (ActiveMonsters.Count > 0)
                {
                    int elementalConsumption = ActiveMonsters.Count * (MaxCapacity / 1000);
                    CurrentCapacity -= elementalConsumption;
                    
                    if (CurrentCapacity <= 0)
                    {
                        CurrentCapacity = 0;
                        DepletionCooldown = DateTime.Now.AddMinutes(30.0); 
                        AvailableResources.Clear();
                        
                        foreach (var m in ActiveMonsters) m?.Delete();
                        ActiveMonsters.Clear();
                        return; 
                    }
                }

                int newOreCapacity = CurrentCapacity + tickRegen;

                if (newOreCapacity > MaxCapacity)
                {
                    int excess = newOreCapacity - MaxCapacity;
                    int elementalCost = MaxCapacity / 1000; 
                    
                    if (!HasSpawnedElementals && elementalCost > 0 && excess >= elementalCost)
                    {
                        int elementalCount = excess / elementalCost;
                        SpawnOreElementals(elementalCount);
                        HasSpawnedElementals = true; 
                    }
                    CurrentCapacity = MaxCapacity; 
                }
                else
                {
                    CurrentCapacity = newOreCapacity;
                }
            }
            else
            {
                CurrentCapacity = Math.Min(MaxCapacity, CurrentCapacity + tickRegen);
            }
        }

        // 🌟 [핵심 픽스] 마을 차단 & 산꼭대기/겹침 방지 스폰 탐색 로직 적용
        private void SpawnOreElementals(int totalElementals)
        {
            // 🌟 던전 및 마을 구역에는 광물 정령 절대 스폰 불가
            if (RegionName.IndexOf("Town", StringComparison.OrdinalIgnoreCase) >= 0 || 
                RegionName.IndexOf("City", StringComparison.OrdinalIgnoreCase) >= 0 ||
                RegionName.IndexOf("Dungeon", StringComparison.OrdinalIgnoreCase) >= 0 ||
                RegionName.IndexOf("Cave", StringComparison.OrdinalIgnoreCase) >= 0)
                return;

            if (AvailableResources.Count == 0 || totalElementals <= 0) return;

            int totalOre = AvailableResources.Values.Sum();
            if (totalOre == 0) return;

            foreach (var kvp in AvailableResources)
            {
                Type oreType = kvp.Key;
                int amount = kvp.Value;
                
                double ratio = (double)amount / totalOre;
                int spawnCount = (int)Math.Round(totalElementals * ratio);

                for (int i = 0; i < spawnCount; i++)
                {
                    string eleName = oreType.Name.Replace("Ore", "Elemental"); 
                    Type eleType = ScriptCompiler.FindTypeByName(eleName);
                    
                    if (eleType != null && typeof(BaseCreature).IsAssignableFrom(eleType))
                    {
                        try
                        {
                            BaseCreature elemental = (BaseCreature)Activator.CreateInstance(eleType);
                            
                            if (CenterX == 0 && CenterY == 0) 
                            { 
                                elemental.Delete(); 
                                continue; 
                            }
                            
                            int range = 6;
                            Point3D? safeLoc = null;

                            // 🌟 2. 겹침 방지: 반경 내에서 이동 가능한 빈 땅(15회 탐색)
                            for (int attempt = 0; attempt < 15; attempt++)
                            {
                                int rx = CenterX + Utility.RandomMinMax(-range, range);
                                int ry = CenterY + Utility.RandomMinMax(-range, range);
                                int rz = Facet.GetAverageZ(rx, ry);
                                Point3D testLoc = new Point3D(rx, ry, rz);

                                if (Facet.CanSpawnMobile(rx, ry, rz))
                                {
                                    // 해당 타일에 다른 모바일이 있는지 검사 (탑 쌓기 방지)
                                    bool isCrowded = false;
                                    IPooledEnumerable eable = Facet.GetMobilesInRange(testLoc, 0);
                                    foreach (Mobile m in eable) { isCrowded = true; break; }
                                    eable.Free();

                                    if (!isCrowded)
                                    {
                                        safeLoc = testLoc;
                                        break;
                                    }
                                }
                            }

                            // 15번 시도해도 빈 땅이 없으면 겹치지 않게 중앙에서 약간 퍼트려서 강제 스폰
                            if (!safeLoc.HasValue)
                            {
                                int rx = CenterX + Utility.RandomMinMax(-2, 2);
                                int ry = CenterY + Utility.RandomMinMax(-2, 2);
                                safeLoc = new Point3D(rx, ry, Facet.GetAverageZ(rx, ry));
                            }

                            elemental.MoveToWorld(safeLoc.Value, Facet);
                            ActiveMonsters.Add(elemental); 
                            
                            // 집을 자신이 스폰된 위치로 지정하여 0,0 좌표로 도망가는 것 방지
                            elemental.Home = safeLoc.Value;
                            elemental.RangeHome = 10;
                        }
                        catch { }
                    }
                }
            }
        }

        public int ConsumeResource(Type itemType, int amount = 1)
        {
            if (!NewSpawnManager.ActiveMaps.GetValueOrDefault(Facet, true) || CurrentCapacity <= 0) 
                return 0;
                
            int actualAmount = Math.Min(CurrentCapacity, amount);
            if (!AvailableResources.ContainsKey(itemType)) AvailableResources[itemType] = 0;
            AvailableResources[itemType] = Math.Max(0, AvailableResources[itemType] - actualAmount); 
            CurrentCapacity -= actualAmount;
            ResourceTracker.Record(Type, actualAmount); 

            if (CurrentCapacity <= 0) { CurrentCapacity = 0; DepletionCooldown = DateTime.Now.AddMinutes(30.0); AvailableResources.Clear(); }
            return actualAmount; 
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(2); 
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

    public static class ResourceTracker
    {
        public static Dictionary<ResourceType, long> HarvestedAmount { get; set; } = new();
        public static void Record(ResourceType type, int amount) { if (!HarvestedAmount.ContainsKey(type)) HarvestedAmount[type] = 0; HarvestedAmount[type] += amount; }
    }

    public static class ResourceManager
    {
        public static Dictionary<ResourceKey, ResourcePool> Pools { get; private set; } = new();
        public static List<ResourcePool> PoolList { get; private set; } = new(); 
        public static Dictionary<ResourceType, List<ResourceDef>> Defs { get; private set; } = new();

        private readonly record struct TierTemplate(double Min, double Max, int Weight);
        private static readonly TierTemplate[] m_Tiers = {
            new(0.0, 50.0, 500),      
            new(20.0, 70.0, 200),     
            new(40.0, 90.0, 100),     
            new(60.0, 110.0, 50),     
            new(80.0, 130.0, 25),     
            new(100.0, 150.0, 10),    
            new(120.0, 170.0, 5),     
            new(150.0, 190.0, 2),     
            new(170.0, 200.0, 1)      
        };

        private static readonly int[] m_WaterTiles = new int[] { 0x00A8, 0x00AB, 0x0136, 0x0137 };

        public static void Configure() { EventSink.WorldSave += OnSave; EventSink.WorldLoad += OnLoad; }

        public static void Initialize()
        {
            SetupDefinitions();
            RegisterAllRegions(); 
            RegisterAllWater();
            GeneratePoolsFromEcoGrid();

            PoolList = Pools.Values.OrderBy(p => p.Facet.MapID).ThenBy(p => p.Type).ToList();
        }

        public static void ProcessEnvironmentSlot(int slot)
        {
            try
            {
                switch (slot)
                {
                    case 1: ProcessFacetResources(Map.Trammel); EcosystemManager.ProcessFacetEcosystem(Map.Trammel); break;
                    case 2: DungeonManager.ProcessFacetDungeons(Map.Trammel); break;
                    case 3: ProcessFacetResources(Map.Felucca); EcosystemManager.ProcessFacetEcosystem(Map.Felucca); break;
                    case 4: DungeonManager.ProcessFacetDungeons(Map.Felucca); break;
                    case 5: ProcessFacetResources(Map.Ilshenar); EcosystemManager.ProcessFacetEcosystem(Map.Ilshenar); break;
                    case 6: ProcessFacetResources(Map.Malas); EcosystemManager.ProcessFacetEcosystem(Map.Malas); break;
                    case 7: ProcessFacetResources(Map.Tokuno); EcosystemManager.ProcessFacetEcosystem(Map.Tokuno); break;
                    case 8: ProcessFacetResources(Map.TerMur); EcosystemManager.ProcessFacetEcosystem(Map.TerMur); break;
                    case 9: DungeonManager.ProcessRemainingDungeons(); break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ResourceManager] 환경 갱신 슬롯 {slot} 에러: {ex.Message}");
            }
        }
        
        private static void ProcessFacetResources(Map facet)
        {
            var pools = PoolList.Where(p => p.Facet == facet).ToList();
            foreach (var pool in pools)
            {
                int regenAmount = 600; 
                if (pool.Type == ResourceType.Lumberjacking) 
                { 
                    regenAmount = 60; 
                    if (EcosystemManager.Zones.ContainsKey(pool.RCode)) regenAmount += 60; 
                }
                else if (pool.Type == ResourceType.Fishing)
                {
                    if (pool.LocType == LocationType.DeepSea) regenAmount += 1200; 
                    
                    if (pool.MaxCapacity > 0 && (double)pool.CurrentCapacity / pool.MaxCapacity >= 0.5 && pool.WType != WaterType.River && Utility.RandomDouble() < 0.78) 
                        SpawnTieredPredator(pool);
                }
                
                pool.Regenerate(regenAmount);
            }
        }

        // 🌟 [심해수 픽스] 심해수들도 마을 근처엔 스폰 차단 & 겹침 방지
        private static void SpawnTieredPredator(ResourcePool pool)
        {
            try
            {
                // 마을(Town) 지역에는 바다 몬스터 스폰 원천 차단
                if (pool.RegionName.IndexOf("Town", StringComparison.OrdinalIgnoreCase) >= 0 || 
                    pool.RegionName.IndexOf("City", StringComparison.OrdinalIgnoreCase) >= 0)
                    return;

                int size = pool.WType == WaterType.Ocean ? 256 : 192;
                int serpents = 0, deepSerpents = 0, krakens = 0;
                List<Mobile> currentMonsters = new List<Mobile>();

                IPooledEnumerable eable = pool.Facet.GetMobilesInRange(new Point3D(pool.CenterX, pool.CenterY, 0), size / 2);
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

                if (pool.WType == WaterType.Ocean)
                {
                    if (fishRatio >= 0.9 && krakens == 0) { spawnType = typeof(Kraken); if (totalMonsters >= 3 && currentMonsters.Count > 0) (currentMonsters.Find(m => m is SeaSerpent) ?? currentMonsters[0]).Delete(); }
                    else if (fishRatio >= 0.7 && deepSerpents < 2 && krakens == 0) { spawnType = typeof(DeepSeaSerpent); if (totalMonsters >= 3 && serpents > 0) currentMonsters.Find(m => m is SeaSerpent)?.Delete(); else if (totalMonsters >= 3) return; }
                    else if (fishRatio >= 0.5 && totalMonsters < 3) spawnType = typeof(SeaSerpent);
                }
                else if (pool.WType == WaterType.Coastal && fishRatio >= 0.7 && totalMonsters < 2) spawnType = typeof(SeaSerpent);

                if (spawnType != null)
                {
                    Point3D? safeSpawnLoc = null;
                    // 물 위에서도 겹침 방지를 위해 오프셋을 주며 유효한 물 타일 검색
                    for (int i = 0; i < 15; i++)
                    {
                        int rx = pool.CenterX + Utility.RandomMinMax(-8, 8);
                        int ry = pool.CenterY + Utility.RandomMinMax(-8, 8);
                        if (ValidateDeepWater(pool.Facet, rx, ry))
                        {
                            safeSpawnLoc = new Point3D(rx, ry, pool.Facet.GetAverageZ(rx, ry));
                            break;
                        }
                    }

                    if (safeSpawnLoc.HasValue)
                    {
                        BaseCreature monster = (BaseCreature)Activator.CreateInstance(spawnType);
                        monster.MoveToWorld(safeSpawnLoc.Value, pool.Facet);
                        pool.ActiveMonsters.Add(monster); 
                        pool.CurrentCapacity = Math.Max(0, pool.CurrentCapacity - (pool.MaxCapacity / 5));
                    }
                }
            }
            catch { }
        }

        private static void SetupDefinitions()
        {
            Defs[ResourceType.Mining] = BuildDefs(new[] 
            { 
                typeof(IronOre), typeof(CopperOre), typeof(BronzeOre), typeof(GoldOre), 
                typeof(AgapiteOre), typeof(VeriteOre), typeof(ValoriteOre), 
                typeof(MithrilOre), typeof(ObsidianOre)
            }, LocationType.Mine, 4);

            Defs[ResourceType.Lumberjacking] = BuildDefs(new[] 
            { 
                typeof(Log), typeof(OakLog), typeof(AshLog), typeof(YewLog), 
                typeof(HeartwoodLog), typeof(BloodwoodLog), typeof(FrostwoodLog), 
                typeof(EbonyLog), typeof(EthrnalLog)
            }, LocationType.Forest, 4);

            Defs[ResourceType.Tanning] = BuildDefs(new[] 
            { 
                typeof(Hides), typeof(SpinedHides), typeof(HornedHides),   
                typeof(BarbedHides), typeof(PolarHides), typeof(AbyssalHides)   
            }, LocationType.Normal, 0);

            Defs[ResourceType.Fishing] = BuildDefs(new[] 
            { 
                typeof(Fish), typeof(Fish), typeof(Fish), typeof(Fish), 
                typeof(Fish), typeof(BigFish), typeof(BigFish) 
            }, LocationType.DeepSea, 5);

            Defs[ResourceType.Farming] = BuildDefs(new[] 
            { 
                typeof(WheatSheaf), typeof(Carrot), typeof(Onion), typeof(Cabbage), 
                typeof(Lettuce), typeof(Pumpkin), typeof(EarOfCorn) 
            }, LocationType.Normal, 0);
        }

        private static List<ResourceDef> BuildDefs(Type[] types, LocationType specialLoc, int specialStartIndex)
        {
            List<ResourceDef> list = new();
            for (int i = 0; i < types.Length && i < m_Tiers.Length; i++) 
            {
                list.Add(new ResourceDef(types[i], m_Tiers[i].Min, m_Tiers[i].Max, i >= specialStartIndex ? specialLoc : LocationType.Normal, m_Tiers[i].Weight));
            }
            return list;
        }

        public static void RegisterLandPool(Map map, string regionName, RegionCode code, int cx, int cy, ResourceType type, LocationType loc, int max, int size, bool isPrivate = false)
        {
            ResourceKey key = new(map.Name, regionName, type);
            if (!Pools.ContainsKey(key)) Pools[key] = new ResourcePool(map.Name, regionName, map, code, cx, cy, WaterType.River, type, loc, max, size, isPrivate);
        }

        public static void RegisterWaterPool(Map map, string regionName, int cx, int cy, WaterType wType, ResourceType type, LocationType loc, int max, int size)
        {
            ResourceKey key = new(map.Name, regionName, type);
            if (!Pools.ContainsKey(key)) Pools[key] = new ResourcePool(map.Name, regionName, map, RegionCode.None, cx, cy, wType, type, loc, max, size, false);
        }

        private static void RegisterAllRegions()
        {
            foreach (Region r in Region.Regions)
            {
                if (r.Map == null || r.Map == Map.Internal || string.IsNullOrEmpty(r.Name)) continue;
                
                string lowerName = r.Name.ToLower();
                
                if (r is Server.Regions.HouseRegion || lowerName.Contains("house") || lowerName.Contains("private") || lowerName.Contains("dungeon")) 
                {
                    continue; 
                }

                RegionCode code = RegionSaver.GetRegionCode(r.Map, r.Area[0].Start.X, r.Area[0].Start.Y, 0);
                int cx = r.Area[0].Start.X + (r.Area[0].Width / 2);
                int cy = r.Area[0].Start.Y + (r.Area[0].Height / 2);

                int landID = r.Map.Tiles.GetLandTile(cx, cy).ID & 0x3FFF;
                bool isFarmTile = (landID >= 0x0009 && landID <= 0x0015) || (landID >= 0x0150 && landID <= 0x015C);
                bool isFarmName = lowerName.Contains("wheatfield") || lowerName.Contains("carrot field") || 
                                  lowerName.Contains("cabbage field") || lowerName.Contains("onion field") || 
                                  lowerName.Contains("cotton field") || lowerName.Contains("farm in") || 
                                  lowerName.Contains("haven farm");

                if (isFarmTile || isFarmName)
                {
                    RegisterLandPool(r.Map, r.Name, code, cx, cy, ResourceType.Farming, LocationType.Normal, 1000, 1, false);
                    continue; 
                }

                bool isWaterName = lowerName.Contains("lake") || lowerName.Contains("sea") || 
                                   lowerName.Contains("ocean") || lowerName.Contains("river") || 
                                   lowerName.Contains("dock") || lowerName.Contains("water") || 
                                   lowerName.Contains("bay") || lowerName.Contains("coast");

                if (isWaterName)
                {
                    RegisterLandPool(r.Map, r.Name, code, cx, cy, ResourceType.Fishing, LocationType.DeepSea, 1000, 1, false);
                    continue;
                }

                if (lowerName.Contains("cave") || lowerName.Contains("mine") || lowerName.Contains("mountain"))
                {
                    RegisterLandPool(r.Map, r.Name, code, cx, cy, ResourceType.Mining, LocationType.Mine, 1000, 1, false);
                    continue;
                }

                RegisterLandPool(r.Map, r.Name, code, cx, cy, ResourceType.Lumberjacking, LocationType.Forest, 1000, 1, false);
            }
        }

        public static void GeneratePoolsFromEcoGrid()
        {
            foreach (var kvp in EcoGridDatabase.Chunks)
            {
                EcoChunkData data = kvp.Value;
                if (data.Code == RegionCode.None) continue;
                string rName = data.Code.ToString();
                
                if (data.OreCap > 0) RegisterLandPool(kvp.Key.Facet, rName, data.Code, data.CenterX, data.CenterY, ResourceType.Mining, LocationType.Mine, data.OreCap, 2);
                if (data.WoodCap > 0) RegisterLandPool(kvp.Key.Facet, rName, data.Code, data.CenterX, data.CenterY, ResourceType.Lumberjacking, LocationType.Forest, data.WoodCap, 2);
                if (data.TanCap > 0) RegisterLandPool(kvp.Key.Facet, rName, data.Code, data.CenterX, data.CenterY, ResourceType.Tanning, LocationType.Normal, data.TanCap, 2);
                if (data.FarmCap > 0) RegisterLandPool(kvp.Key.Facet, rName, data.Code, data.CenterX, data.CenterY, ResourceType.Farming, LocationType.Normal, data.FarmCap, 2);
            }
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
                        if (ValidateDeepWater(map, x, y))
                        {
                            WaterType wType = GetWaterCategory(map, x, y);
                            LocationType locType = wType == WaterType.Ocean ? LocationType.DeepSea : LocationType.Normal;
                            int maxCap = wType == WaterType.Ocean ? 4000 : 2000;
                            string rName = wType == WaterType.Ocean ? $"Ocean_{x}_{y}" : $"Coastal_{x}_{y}";
                            RegisterWaterPool(map, rName, x, y, wType, ResourceType.Fishing, locType, maxCap, 2);
                        }
                    }
                }
            }
        }

        public static ResourceDef GetDef(ResourceType type, Type itemType) => Defs.GetValueOrDefault(type)?.FirstOrDefault(d => d.ItemType == itemType);

        private static void OnSave(WorldSaveEventArgs e)
        {
            string path = Path.Combine(Core.BaseDirectory, "Saves", "ResourceManagement", "ResourcePools.bin");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryFileWriter writer = new BinaryFileWriter(stream, true);
                writer.Write(2); 
                writer.Write(PoolList.Count);
                foreach (var pool in PoolList) 
                { 
                    writer.Write(pool.MapName); writer.Write(pool.RegionName); writer.Write((int)pool.Type); 
                    pool.Serialize(writer); 
                }
                writer.Close();
            }
        }

        private static void OnLoad() { /* 로직 생략 */ }
    }

    public static class EcoGridDatabase
    {
        public static Dictionary<EcoChunkKey, EcoChunkData> Chunks { get; private set; } = new();

        public static void Initialize()
        {
            string filePath = Path.Combine(Core.BaseDirectory, "Data", "EcoGrid_Master_AllMaps.csv");
            if (!File.Exists(filePath)) return;

            Chunks.Clear();
            int count = 0;

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
                        if (map == null || map == Map.Internal) continue;

                        int cx = int.Parse(data[1]);
                        int cy = int.Parse(data[2]);
                        
                        RegionCode code = (RegionCode)int.Parse(data[5]);

                        EcoChunkData chunkData = new(
                            int.Parse(data[3]), int.Parse(data[4]), code,
                            int.Parse(data[6]), int.Parse(data[7]), int.Parse(data[8]), 
                            int.Parse(data[9]), int.Parse(data[10])
                        );
                        
                        Chunks[new EcoChunkKey(map, cx, cy)] = chunkData; 
                        count++;
                    }
                    catch { }
                }
            }
            
            Console.WriteLine($"[EcoGrid] CSV에서 {count}개의 유효 자원 구역 데이터를 로드했습니다.");
            
            FillMissingChunks();
        }

        private static void FillMissingChunks()
        {
            Map[] targetMaps = { Map.Felucca, Map.Trammel, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur };
            int addedCount = 0;

            foreach (Map map in targetMaps)
            {
                if (map == null || map == Map.Internal || !NewSpawnManager.ActiveMaps.GetValueOrDefault(map, true)) continue;

                int widthChunks = map.Width / 128;
                int heightChunks = map.Height / 128;

                for (int x = 0; x < widthChunks; x++)
                {
                    for (int y = 0; y < heightChunks; y++)
                    {
                        EcoChunkKey key = new EcoChunkKey(map, x, y);
                        
                        if (Chunks.ContainsKey(key)) continue;

                        int centerX = (x * 128) + 64;
                        int centerY = (y * 128) + 64;
                        
                        EcoChunkData emptyChunk = new EcoChunkData(centerX, centerY, RegionCode.None, 0, 0, 0, 0, 0);
                        Chunks[key] = emptyChunk;
                        addedCount++;
                    }
                }
            }
            Console.WriteLine($"[EcoGrid] CSV에 누락된 야생 및 평야 구역 {addedCount}개를 자동 생성하여 월드를 128x128로 완벽 분할했습니다.");
        }

        public static (bool IsValid, EcoChunkData Data) GetChunkAt(Map map, int x, int y)
        {
            if (map == null || map == Map.Internal) return (false, default);
            EcoChunkKey key = new(map, x / 128, y / 128);
            return Chunks.TryGetValue(key, out var data) ? (true, data) : (false, default);
        }
    }
}