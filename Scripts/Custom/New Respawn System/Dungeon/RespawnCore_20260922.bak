using System;
using System.Collections.Generic;
using System.Collections.Frozen;
using System.IO;
using System.Xml;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
    public enum DungeonPhase { Active, BossSpawned, Cooldown }
    public enum DungeonDepth { Entrance = 1, Middle = 2, Deep = 3, BossRoom = 4 } 

    // ========================================================================
    // 🌍 EcoZone: UI(모니터링 Gump)용 그룹핑 및 노드 관리 로직
    // ========================================================================
    public class EcoZone
    {
        public RegionCode RCode { get; set; } 
        public string ZoneId => NewSpawnManager.GetDisplayName(RCode); 
        public Map Facet { get; set; }
        public List<EcoNode> Nodes { get; set; } = new();
        
        private int m_GoIndex = -1;

        public EcoZone(RegionCode code, Map map)
        {
            RCode = code;
            Facet = map;
        }

        public void GoToNextNode(Mobile m)
        {
            if (Nodes.Count == 0) { m.SendMessage(33, "이 구역에는 등록된 노드가 없습니다."); return; }
            if (++m_GoIndex >= Nodes.Count) m_GoIndex = 0;
            if (Nodes[m_GoIndex] != null && !Nodes[m_GoIndex].Deleted && Nodes[m_GoIndex].Map != null)
                m.MoveToWorld(Nodes[m_GoIndex].Location, Nodes[m_GoIndex].Map);
        }

        public void KeepCurrentNodeOnly(Mobile m)
        {
            if (Nodes.Count <= 1) return;
            var nodeToKeep = Nodes[m_GoIndex >= 0 && m_GoIndex < Nodes.Count ? m_GoIndex : 0];
            for (int i = Nodes.Count - 1; i >= 0; i--)
            {
                if (Nodes[i] != nodeToKeep && !Nodes[i].Deleted) Nodes[i].Delete();
            }
            m_GoIndex = 0; CacheNodes(); 
        }

        public void CacheNodes()
        {
            Nodes.Clear();
            foreach (Item item in World.Items.Values)
                if (item is EcoNode node && node.Map == Facet && node.RCode == this.RCode) Nodes.Add(node);
        }

        public void ClearAllSpawns()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                var field = Nodes[i].GetType().GetField("m_Spawned", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null && field.GetValue(Nodes[i]) is List<Mobile> list)
                {
                    for (int j = list.Count - 1; j >= 0; j--)
                    {
                        if (list[j] != null) list[j].Delete();
                    }
                    list.Clear();
                }
            }
        }
    }

	// ========================================================================
    // 특수 스폰 및 기믹 규칙 데이터 클래스
    // ========================================================================
    public class SpecialSpawnRule
    {
        public Type MonsterType { get; set; }
        public int MaxCount { get; set; }
        public double SpawnChance { get; set; }
    }

    public class BossGimmickRule
    {
        public Type BossType { get; set; }
        public bool RequiresAltar { get; set; }
        public Dictionary<Type, int> RequiredItems { get; set; } = new Dictionary<Type, int>();
    }

    // ========================================================================
    // ⚔️ DungeonZone: 중앙 통제형 던전 엔진 (30분 사이클 비율제 스폰 및 열기 연산 적용)
    // ========================================================================
    public class DungeonZone
    {
        public RegionCode RCode { get; set; } 
        public string ZoneId => NewSpawnManager.GetDisplayName(RCode); 
        
        private Map m_Facet;
        public Map Facet 
        { 
            get 
            { 
                if (m_Facet == null) m_Facet = DungeonManager.ResolveMapByName(RCode.ToString()); 
                return m_Facet; 
            }
            set => m_Facet = value; 
        }

        public bool IsActive { get; set; }
        public List<Rectangle2D> AreaBounds { get; set; } = new List<Rectangle2D>();
        
        public int TargetHeat { get; set; }      
        public int CurrentHeat { get; set; }    
        public int MaxDifficulty { get => TargetHeat; set => TargetHeat = value; }
        public int CurrentDifficulty { get => CurrentHeat; set => CurrentHeat = value; }

        public Type BossType { get; set; }
        public TimeSpan RestDuration { get; set; }
        public DateTime CooldownEndTime { get; set; }
        public DateTime LastKillTime { get; set; }
        public DungeonPhase Phase { get; set; }

        public int MaxPopulation { get; set; }
        public int ManualMaxPopulation { get; set; } = -1;
        
        // 🌟 [변경] 초 단위 타이머 삭제 -> 틱당 보충률(%)과 열기 냉각 가중치로 변경
        public double ReplenishRate { get; set; }
        public int HeatDecayWeight { get; set; }

        public bool EnableRareDrops { get; set; }
        public int RareDropHeatThreshold { get; set; }
        public double RareDropChance { get; set; }
        public bool IsStealable { get; set; }
        public List<Type> RareItemTypes { get; set; } = new List<Type>();

        public List<BaseCreature> ActiveMonsters { get; set; } = new List<BaseCreature>();
        public List<Item> ActiveItems { get; set; } = new List<Item>();
        public Dictionary<int, Type[]> SpawnProfiles { get; set; } = new Dictionary<int, Type[]>();
        public List<Type> UniqueTypes { get; set; } = new List<Type>(); 
		// [기획 1 & 4] 그룹 및 세부 구역 관리
        public string GroupName { get; set; } = "Uncategorized";
        public string SubZoneName { get; set; } = "Main";

        // [기획 2] 열기(Heat)에 따른 몬스터 리스폰 목록 관리 (문자열 기반)
        public Dictionary<int, List<string>> SpawnProfileStrings { get; set; } = new Dictionary<int, List<string>>();

        // [기획 3] 특수 던전 기믹 및 보스 설정
        public List<SpecialSpawnRule> SpecialSpawns { get; set; } = new List<SpecialSpawnRule>();
        public BossGimmickRule BossGimmick { get; set; } = new BossGimmickRule();

        // [기획 5] 던전 열기에 따른 도시 치안 영향도 (Key: 도시명, Value: 영향 가중치)
        public Dictionary<string, double> CitySecurityImpact { get; set; } = new Dictionary<string, double>();

        public DungeonZone(RegionCode code, Map map, int targetHeat, Type bossType, TimeSpan cooldown)
        {
            RCode = code;
            Facet = map; 
            TargetHeat = targetHeat; 
            BossType = bossType; 
            RestDuration = cooldown; 
            Phase = DungeonPhase.Active;
            CurrentHeat = bossType != null ? (int)(targetHeat * 0.2) : (int)(targetHeat * 0.5);
            LastKillTime = DateTime.Now;

            // 기본값: 빈자리의 40% 보충, 잔존 몹 1마리당 열기 -5 감소
            ReplenishRate = 0.40;
            HeatDecayWeight = 5;
            MaxPopulation = 20;

            if (map == Map.Trammel) IsActive = true;
            else IsActive = false;
        }

        public void AddUnique(Type t) { if (!UniqueTypes.Contains(t)) UniqueTypes.Add(t); }

        public void SetPopulation(int maxPop) { ManualMaxPopulation = maxPop; MaxPopulation = maxPop; }

        public void SetSpawnProfile(DungeonDepth depth, Type[] types) => SetSpawnProfile((int)depth, types);
        public void SetSpawnProfile(int tier, Type[] types)
        {
            List<Type> cleanTypes = new List<Type>();
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] != null && !types[i].Name.ToLower().Contains("summon"))
                    cleanTypes.Add(types[i]);
            }
            SpawnProfiles[tier] = cleanTypes.ToArray();
        }

        public Point3D GetCenterLocation()
        {
            if (AreaBounds == null || AreaBounds.Count == 0) return Point3D.Zero;
            Rectangle2D rect = AreaBounds[0];
            int cx = rect.X + (rect.Width / 2);
            int cy = rect.Y + (rect.Height / 2);
            return new Point3D(cx, cy, Facet != null ? Facet.GetAverageZ(cx, cy) : 0);
        }

        private Point3D? GetValidSpawnLocation()
        {
            if (AreaBounds == null || AreaBounds.Count == 0 || AreaBounds[0].Width <= 0) return null;
            
            Rectangle2D bounds = AreaBounds[Utility.Random(AreaBounds.Count)];
            
            for (int i = 0; i < 50; i++) 
            {
                int x = Utility.RandomMinMax(bounds.X, bounds.X + bounds.Width);
                int y = Utility.RandomMinMax(bounds.Y, bounds.Y + bounds.Height);
                
                int z = Facet.GetAverageZ(x, y); 
                if (Facet.CanSpawnMobile(x, y, z)) 
                    return new Point3D(x, y, z);

                var statics = Facet.Tiles.GetStaticTiles(x, y);
                foreach (var tile in statics)
                {
                    int staticZ = tile.Z + tile.Height; 
                    if (Facet.CanSpawnMobile(x, y, staticZ))
                        return new Point3D(x, y, staticZ);
                }
            }
            return null; 
        }

        public void KeepCurrentNodeOnly(Mobile m) { }
        
        public void GoToNextNode(Mobile m) 
        {
            Point3D center = GetCenterLocation();
            if (center != Point3D.Zero) m.MoveToWorld(center, Facet);
        }
        
        public void CacheNodes() { } 

        public int GetTotalActiveCount()
        {
            int count = 0;
            for (int i = ActiveMonsters.Count - 1; i >= 0; i--)
            {
                if (ActiveMonsters[i] == null || ActiveMonsters[i].Deleted || !ActiveMonsters[i].Alive)
                    ActiveMonsters.RemoveAt(i);
                else count++;
            }
            for (int i = ActiveItems.Count - 1; i >= 0; i--)
            {
                if (ActiveItems[i] == null || ActiveItems[i].Deleted || ActiveItems[i].Map == null)
                    ActiveItems.RemoveAt(i);
                else count++;
            }
            return count;
        }

        public void CalculateDynamicPopulation()
        {
            if (ManualMaxPopulation >= 0)
            {
                MaxPopulation = ManualMaxPopulation;
                return;
            }

            if (AreaBounds == null || AreaBounds.Count == 0)
            {
                MaxPopulation = 0;
                return;
            }

            long totalArea = 0;
            for (int i = 0; i < AreaBounds.Count; i++)
            {
                totalArea += (AreaBounds[i].Width * AreaBounds[i].Height);
            }

            double basePop = totalArea / 64.0;
            double heatBonus = TargetHeat > 0 ? (TargetHeat / 15000.0) : 0;
            int calcPop = (int)(basePop + heatBonus);

            if (calcPop < 5) calcPop = 5;
            if (calcPop > 150) calcPop = 150;

            MaxPopulation = calcPop;
        }

        // ========================================================================
        // 🧬 [교정 완료] CheckRespawn 및 명성(Fame) 기반 스폰 연산
        // ========================================================================
        public void CheckRespawn()
        {
            if (!IsActive || Phase == DungeonPhase.Cooldown || AreaBounds.Count == 0)
                return;

            int activeCount = 0;
            for (int i = ActiveMonsters.Count - 1; i >= 0; i--)
            {
                BaseCreature bc = ActiveMonsters[i];
                if (bc == null || bc.Deleted || !bc.Alive)
                    ActiveMonsters.RemoveAt(i);
                else
                    activeCount++;
            }

            int maxPop = ManualMaxPopulation;
            if (maxPop == -1)
            {
                CalculateDynamicPopulation();
                maxPop = MaxPopulation;
            }

            if (activeCount >= maxPop)
                return;

            int missingPop = maxPop - activeCount;
            int toSpawn = (int)(missingPop * ReplenishRate);
            
            int minSpawn = Math.Min(3, missingPop);
            if (toSpawn < minSpawn) 
                toSpawn = minSpawn;

            if (toSpawn <= 0)
                return;

            double heatRatio = 0.0;
            if (TargetHeat > 0)
                heatRatio = (double)CurrentHeat / TargetHeat;
            if (heatRatio > 1.0) heatRatio = 1.0;

            List<DungeonNode> validNodes = new List<DungeonNode>();
            Rectangle2D bounds = AreaBounds[0];

            foreach (Item item in World.Items.Values)
            {
                if (item is DungeonNode node && node.RCode == this.RCode)
                {
                    if (bounds.Contains(node.Location))
                        validNodes.Add(node);
                }
            }

            if (validNodes.Count == 0)
                return;

            for (int s = 0; s < toSpawn; s++)
            {
                DungeonNode targetNode = validNodes[Utility.Random(validNodes.Count)];

                int selectedTier = 1;
                double roll = Utility.RandomDouble();

                if (heatRatio >= 0.8)
                {
                    selectedTier = (roll < 0.70) ? 3 : 2;
                }
                else if (heatRatio >= 0.4)
                {
                    if (roll < 0.60) selectedTier = 2;
                    else if (roll < 0.85) selectedTier = 1;
                    else selectedTier = 3;
                }
                else
                {
                    selectedTier = (roll < 0.85) ? 1 : 2;
                }

                Type spawnType = GetWeightedSpawnTypeByFame(selectedTier, heatRatio);

                if (spawnType == null && selectedTier > 1) spawnType = GetWeightedSpawnTypeByFame(selectedTier - 1, heatRatio);
                if (spawnType == null) spawnType = GetWeightedSpawnTypeByFame(1, heatRatio);

                if (spawnType == null)
                    continue;

                try
                {
                    object obj = Activator.CreateInstance(spawnType);
                    if (obj is BaseCreature bc)
                    {
                        int rx = targetNode.X + Utility.RandomMinMax(-targetNode.SpawnRange, targetNode.SpawnRange);
                        int ry = targetNode.Y + Utility.RandomMinMax(-targetNode.SpawnRange, targetNode.SpawnRange);
                        int rz = targetNode.Map.GetAverageZ(rx, ry);

                        bc.Home = targetNode.Location;
                        bc.RangeHome = targetNode.HomeRange; // 고유 변수명 일치 완료
                        bc.Grade = 1; 

                        bc.MoveToWorld(new Point3D(rx, ry, rz), targetNode.Map);
                        ActiveMonsters.Add(bc);

                        if (selectedTier == 3) CurrentHeat += 10;
                        else if (selectedTier == 2) CurrentHeat += 3;
                    }
                }
                catch
                {
                }
            }
        }

        private Type GetWeightedSpawnTypeByFame(int tier, double heatRatio)
        {
            if (SpawnProfileStrings == null)
                return null;

            if (SpawnProfileStrings.TryGetValue(tier, out List<string> list) && list != null && list.Count > 0)
            {
                int count = list.Count;
                if (count == 1)
                {
                    Type singleType = ScriptCompiler.FindTypeByName(list[0].Trim());
                    if (singleType != null && singleType.IsSubclassOf(typeof(BaseCreature))) return singleType;
                    return null;
                }

                List<Type> validTypes = new List<Type>();
                List<int> fameTable = new List<int>();

                for (int i = 0; i < count; i++)
                {
                    Type t = ScriptCompiler.FindTypeByName(list[i].Trim());
                    if (t != null && t.IsSubclassOf(typeof(BaseCreature)))
                    {
                        validTypes.Add(t);
                        int baseFame = 0;
                        try
                        {
                            if (Activator.CreateInstance(t) is BaseCreature dummy)
                            {
                                baseFame = dummy.Fame;
                                dummy.Delete();
                            }
                        }
                        catch { }
                        fameTable.Add(Math.Max(100, baseFame));
                    }
                }

                int validCount = validTypes.Count;
                if (validCount == 0) return null;

                double[] weights = new double[validCount];
                double totalWeight = 0.0;

                for (int i = 0; i < validCount; i++)
                {
                    double fameFactor = fameTable[i] / 1000.0;
                    double calculatedWeight = Math.Pow(fameFactor, 1.0 + (heatRatio * 3.0));
                    
                    if (heatRatio < 0.3)
                        calculatedWeight = 1.0 / calculatedWeight;

                    weights[i] = calculatedWeight;
                    totalWeight += calculatedWeight;
                }

                double choiceRoll = Utility.RandomDouble() * totalWeight;
                double cumulative = 0.0;

                for (int i = 0; i < validCount; i++)
                {
                    cumulative += weights[i];
                    if (choiceRoll <= cumulative)
                        return validTypes[i];
                }
                return validTypes[0];
            }
            return null;
        }

        public void ProcessDeath(BaseCreature bc)
        {
            if (!IsActive || Phase == DungeonPhase.Cooldown) return;

            LastKillTime = DateTime.Now;

            if (Phase == DungeonPhase.BossSpawned && bc.GetType() == BossType)
            {
                Phase = DungeonPhase.Cooldown; CooldownEndTime = DateTime.Now + RestDuration; ClearAllSpawns(); return;
            }

            // 플레이어가 몹을 처치할 때마다 열기 대폭 증가
            CurrentHeat += Math.Max(1, (bc.Fame / 500)) * (bc.Grade == 1 ? 1 : bc.Grade == 2 ? 2 : bc.Grade == 3 ? 3 : 4);
            if (CurrentHeat > TargetHeat) CurrentHeat = TargetHeat;
            
            if (CurrentHeat >= TargetHeat && Phase == DungeonPhase.Active)
            {
                if (BossType != null)
                {
                    Phase = DungeonPhase.BossSpawned; 
                    ClearAllSpawns(); 
                    try 
                    { 
                        BaseCreature b = (BaseCreature)Activator.CreateInstance(BossType); 
                        Point3D? spawnLoc = GetValidSpawnLocation();
                        if (spawnLoc.HasValue)
                        {
                            b.Home = spawnLoc.Value;
                            b.RangeHome = 40;
                            b.MoveToWorld(spawnLoc.Value, Facet); 
                            ActiveMonsters.Add(b); 
                        }
                        else { b.Delete(); }
                    } 
                    catch { } 
                }
                else
                {
                    CurrentHeat = TargetHeat;
                }
            }
        }

        public void ClearAllSpawns()
        {
            for (int i = ActiveMonsters.Count - 1; i >= 0; i--) { if (ActiveMonsters[i] != null) ActiveMonsters[i].Delete(); }
            ActiveMonsters.Clear();
            for (int i = ActiveItems.Count - 1; i >= 0; i--) { if (ActiveItems[i] != null) ActiveItems[i].Delete(); }
            ActiveItems.Clear();

            if (Facet != null && AreaBounds != null && AreaBounds.Count > 0)
            {
                List<Mobile> orphans = new List<Mobile>();
                foreach (Mobile m in World.Mobiles.Values)
                {
                    if (m is BaseCreature bc && !bc.Controlled && !bc.Summoned && bc.Map == Facet)
                    {
                        if (bc is BaseVendor || bc is BaseHealer || bc is BaseGuard || bc.IsInvulnerable) continue;

                        for (int b = 0; b < AreaBounds.Count; b++)
                        {
                            if (AreaBounds[b].Contains(new Point2D(bc.X, bc.Y)))
                            {
                                orphans.Add(bc);
                                break;
                            }
                        }
                    }
                }
                for (int i = 0; i < orphans.Count; i++) orphans[i].Delete();
            }
        }

        public void PerformRecovery() 
        { 
            if (!IsActive) return;

            int minHeat = BossType != null ? (int)(TargetHeat * 0.2) : (int)(TargetHeat * 0.5);
            if (Phase == DungeonPhase.Cooldown && DateTime.Now >= CooldownEndTime) 
            { 
                CurrentHeat = minHeat; 
                Phase = DungeonPhase.Active; 
                LastKillTime = DateTime.Now;
            }
        }
    }

    // ========================================================================
    // 🌍 EcosystemManager: 생태계 구역 캐싱 및 환경 보너스 계산 (기존 유지)
    // ========================================================================
    public static class EcosystemManager 
    { 
        public static FrozenDictionary<RegionCode, EcoZone> Zones { get; private set; }
        public static List<EcoZone> ZoneList { get; private set; } = new();
        private static Dictionary<RegionCode, EcoZone> m_TempZones = new();

        public static void RebuildZones()
        {
            m_TempZones.Clear();
            foreach (Item item in World.Items.Values)
            {
                if (item is EcoNode node)
                {
                    if (node.RCode == RegionCode.None)
                    {
                        int mapId = node.Map?.MapID ?? 0;
                        int cx = node.X / 128;
                        int cy = node.Y / 128;
                        int pseudoCode = ((mapId + 1) * 1000000) + (cx * 1000) + cy;
                        node.RCode = (RegionCode)pseudoCode;
                    }

                    if (!m_TempZones.TryGetValue(node.RCode, out var zone)) { zone = new EcoZone(node.RCode, node.Map); m_TempZones[node.RCode] = zone; }
                    zone.Nodes.Add(node);
                }
            }
            FreezeData();
        }

        public static int ClearMapSpawns(Map map)
        {
            int count = 0;
            for (int i = 0; i < ZoneList.Count; i++)
            {
                if (ZoneList[i].Facet == map)
                {
                    ZoneList[i].ClearAllSpawns(); 
                    count++; 
                }
            }
            return count; 
        }

        public static void FreezeData() 
        { 
            Zones = m_TempZones.ToFrozenDictionary(); 
            ZoneList = new List<EcoZone>(Zones.Values);
            ZoneList.Sort((a, b) => 
            {
                if (a.Facet == Map.Trammel && b.Facet != Map.Trammel) return -1;
                if (a.Facet != Map.Trammel && b.Facet == Map.Trammel) return 1;
                int mapCmp = (a.Facet?.MapID ?? 99).CompareTo(b.Facet?.MapID ?? 99);
                if (mapCmp != 0) return mapCmp;
                return ((int)a.RCode).CompareTo((int)b.RCode);
            });
        }
        public static void RemoveZone(RegionCode code) { if (m_TempZones.ContainsKey(code)) m_TempZones.Remove(code); }

        public static (double polarBonus, double obsidianBonus, double soulBonus) GetEnvironmentBonus(Point3D loc, Map map)
        {
            EcoNode nearestNode = null;
            int minDst = int.MaxValue;
            for (int i = 0; i < ZoneList.Count; i++)
            {
                if (ZoneList[i].Facet != map) continue;
                for (int j = 0; j < ZoneList[i].Nodes.Count; j++)
                {
                    EcoNode n = ZoneList[i].Nodes[j];
                    int dst = Math.Max(Math.Abs(n.X - loc.X), Math.Abs(n.Y - loc.Y));
                    if (dst <= n.SpawnRange && dst < minDst) { nearestNode = n; minDst = dst; }
                }
            }
            
            if (nearestNode == null) return (1.0, 1.0, 1.0); 
            double polar = 1.0, obsidian = 1.0, soul = 1.0;
            if (nearestNode.ClimateType == EcoClimateType.Arctic) { polar += 5.0; soul += 1.5; } else if (nearestNode.ClimateType == EcoClimateType.Volcanic || nearestNode.RCode.ToString().Contains("Hythloth")) { obsidian += 5.0; soul -= 0.5; } else if (nearestNode.ClimateType == EcoClimateType.Void) { soul += 5.0; }
            return (Math.Max(0.1, polar), Math.Max(0.1, obsidian), Math.Max(0.1, soul));
        }

        public static void ProcessFacetEcosystem(Map facet)
        {
            if (!NewSpawnManager.IsMapActive(facet)) return;
            for (int i = 0; i < ZoneList.Count; i++)
            {
                if (ZoneList[i].Facet == facet)
                {
                    for (int j = 0; j < ZoneList[i].Nodes.Count; j++)
                    {
                        if (ZoneList[i].Nodes[j] != null && !ZoneList[i].Nodes[j].Deleted)
                            ZoneList[i].Nodes[j].DoTick();
                    }
                }
            }
        }
    }

    // ========================================================================
    // ⚔️ DungeonManager: XML(설정) & BIN(라이브 상태) 이중 관리 시스템
    // ========================================================================
    public static class DungeonManager
    {
        public static FrozenDictionary<RegionCode, DungeonZone> Zones { get; private set; }
        public static List<DungeonZone> ZoneList { get; private set; } = new();
        private static Dictionary<RegionCode, DungeonZone> m_TempZones = new();

        public static void RegisterZone(DungeonZone zone) { if (zone != null && zone.RCode != RegionCode.None) m_TempZones[zone.RCode] = zone; }
        public static void FreezeData() 
        { 
            Zones = m_TempZones.ToFrozenDictionary(); 
            ZoneList = new List<DungeonZone>(Zones.Values);
            ZoneList.Sort((a, b) => 
            {
                int mapCmp = (a.Facet?.MapID ?? 99).CompareTo(b.Facet?.MapID ?? 99);
                if (mapCmp != 0) return mapCmp;
                return ((int)a.RCode).CompareTo((int)b.RCode);
            });
        }

        public static void Configure() 
        { 
            EcoSpawnDatabase.Initialize(); 
            
            TrammelDungeon.Setup(); 
            FeluccaDungeon.Setup(); 
            TokunoDungeon.Setup();  
            IlshenarDungeon.Setup(); 
            MalasDungeon.Setup();    
            TerMurDungeon.Setup();   
            FreezeData();
            
            EventSink.WorldSave += OnSave; 
            EventSink.WorldLoad += OnLoad; 
            EventSink.ServerStarted += OnServerStarted; 
        }

        public static void NukeDungeon(Mobile from)
        {
            RegionCode currentCode = RegionSaver.GetRegionCode(from.Map, from.X, from.Y, from.Z);
            if (currentCode == RegionCode.None) { from.SendMessage(33, "던전 구역 안에서 명령어를 실행해 주세요."); return; }

            int targetBase = ((int)currentCode / 100) * 100;
            int deletedCount = 0;
            
            List<Mobile> toDelete = new List<Mobile>();
            foreach (Mobile m in World.Mobiles.Values)
            {
                if (m is BaseCreature bc && !bc.Controlled && !bc.Summoned && bc.Map == from.Map)
                {
                    if (((int)RegionSaver.GetRegionCode(bc.Map, bc.X, bc.Y, bc.Z) / 100) * 100 == targetBase)
                        toDelete.Add(m);
                }
            }
            
            for (int i = 0; i < toDelete.Count; i++) { toDelete[i].Delete(); deletedCount++; }

            for (int i = 0; i < ZoneList.Count; i++)
            {
                if (((int)ZoneList[i].RCode / 100) * 100 == targetBase || ZoneList[i].RCode == currentCode)
                    ZoneList[i].ClearAllSpawns();
            }
            from.SendMessage(66, $"{targetBase} 던전의 미아 몬스터 {deletedCount}마리를 모두 소거했습니다.");
        }

        public static int ClearMapSpawns(Map map)
        {
            int count = 0;
            for (int i = 0; i < ZoneList.Count; i++)
            {
                if (ZoneList[i].Facet == map)
                {
                    count += ZoneList[i].ActiveMonsters.Count;
                    ZoneList[i].ClearAllSpawns();
                }
            }
            return count;
        }

        public static void Initialize() { }

        private static void OnServerStarted() 
        { 
            EcosystemManager.RebuildZones(); 
        }

        public static void ProcessFacetDungeons(Map facet)
        {
            for (int i = 0; i < ZoneList.Count; i++)
            {
                if (ZoneList[i].Facet == facet)
                {
                    ZoneList[i].CheckRespawn();
                    ZoneList[i].PerformRecovery();
                }
            }
        }

        public static void ProcessRemainingDungeons()
        {
            for (int i = 0; i < ZoneList.Count; i++)
            {
                if (ZoneList[i].Facet != Map.Trammel && ZoneList[i].Facet != Map.Felucca)
                {
                    ZoneList[i].CheckRespawn();
                    ZoneList[i].PerformRecovery();
                }
            }
        }

        public static void OnCreatureKilled(BaseCreature bc) 
        { 
            if (bc == null || bc.Controlled || bc.Summoned) return; 
            RegionCode locCode = RegionSaver.GetRegionCode(bc.Map, bc.X, bc.Y, bc.Z); 
            if (locCode != RegionCode.None && Zones.TryGetValue(locCode, out DungeonZone zone) && zone.Facet == bc.Map) 
            {
                bool found = false;
                for (int i = 0; i < zone.ActiveMonsters.Count; i++)
                {
                    if (zone.ActiveMonsters[i] == bc) { found = true; break; }
                }
                if (found) zone.ProcessDeath(bc);
            }
        }

        public static string CleanString(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsLetterOrDigit(input[i])) sb.Append(input[i]);
            }
            return sb.ToString().ToLower();
        }

        public static Map ResolveMapByName(string name) { if (name.Contains("Felucca")) return Map.Felucca; if (name.Contains("Trammel")) return Map.Trammel; if (name.Contains("Ilshenar")) return Map.Ilshenar; if (name.Contains("Malas")) return Map.Malas; if (name.Contains("Tokuno")) return Map.Tokuno; if (name.Contains("TerMur")) return Map.TerMur; return Map.Trammel; }

        private static void OnSave(WorldSaveEventArgs e)
        {
            SaveConfigurationXML();
            SaveLiveStateBIN();
        }

        private static void OnLoad()
        {
            LoadConfigurationXML();
            LoadLiveStateBIN();
        }

        private static void SaveConfigurationXML()
        {
            string path = Path.Combine(Core.BaseDirectory, "Data", "DungeonSystem", "DungeonConfigs.xml");
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("Dungeons");
            doc.AppendChild(root);

            for (int i = 0; i < ZoneList.Count; i++)
            {
                DungeonZone z = ZoneList[i];
                XmlElement node = doc.CreateElement("Zone");
                node.SetAttribute("RCode", ((int)z.RCode).ToString());
                node.SetAttribute("IsActive", z.IsActive.ToString());
                node.SetAttribute("TargetHeat", z.TargetHeat.ToString());
                node.SetAttribute("MaxPop", z.MaxPopulation.ToString());
                
                node.SetAttribute("ReplenishRate", z.ReplenishRate.ToString());
                node.SetAttribute("HeatDecayWeight", z.HeatDecayWeight.ToString());
                node.SetAttribute("RestMin", z.RestDuration.TotalMinutes.ToString());
                if (z.BossType != null) node.SetAttribute("BossType", z.BossType.FullName);

                node.SetAttribute("EnableRareDrops", z.EnableRareDrops.ToString());
                node.SetAttribute("RareDropHeatThreshold", z.RareDropHeatThreshold.ToString());
                node.SetAttribute("RareDropChance", z.RareDropChance.ToString());
                node.SetAttribute("IsStealable", z.IsStealable.ToString());

                // [신규 기획 데이터 저장]
                node.SetAttribute("GroupName", z.GroupName);
                node.SetAttribute("SubZoneName", z.SubZoneName);

                XmlElement profilesNode = doc.CreateElement("SpawnProfiles");
                foreach (var kvp in z.SpawnProfileStrings)
                {
                    XmlElement pNode = doc.CreateElement("Tier");
                    pNode.SetAttribute("Level", kvp.Key.ToString());
                    pNode.SetAttribute("Mobs", string.Join(",", kvp.Value));
                    profilesNode.AppendChild(pNode);
                }
                node.AppendChild(profilesNode);

                XmlElement citiesNode = doc.CreateElement("CitySecurity");
                foreach (var kvp in z.CitySecurityImpact)
                {
                    XmlElement cNode = doc.CreateElement("City");
                    cNode.SetAttribute("Name", kvp.Key);
                    cNode.SetAttribute("Impact", kvp.Value.ToString());
                    citiesNode.AppendChild(cNode);
                }
                node.AppendChild(citiesNode);

                XmlElement specialsNode = doc.CreateElement("SpecialSpawns");
                foreach (var sp in z.SpecialSpawns)
                {
                    if (sp.MonsterType == null) continue;
                    XmlElement sNode = doc.CreateElement("Rule");
                    sNode.SetAttribute("Type", sp.MonsterType.FullName);
                    sNode.SetAttribute("Max", sp.MaxCount.ToString());
                    sNode.SetAttribute("Chance", sp.SpawnChance.ToString());
                    specialsNode.AppendChild(sNode);
                }
                node.AppendChild(specialsNode);

                XmlElement gimmickNode = doc.CreateElement("BossGimmick");
                if (z.BossGimmick.BossType != null)
                {
                    gimmickNode.SetAttribute("Type", z.BossGimmick.BossType.FullName);
                    gimmickNode.SetAttribute("RequiresAltar", z.BossGimmick.RequiresAltar.ToString());
                    foreach (var req in z.BossGimmick.RequiredItems)
                    {
                        if (req.Key == null) continue;
                        XmlElement reqNode = doc.CreateElement("ReqItem");
                        reqNode.SetAttribute("Type", req.Key.FullName);
                        reqNode.SetAttribute("Amount", req.Value.ToString());
                        gimmickNode.AppendChild(reqNode);
                    }
                }
                node.AppendChild(gimmickNode);

                for (int b = 0; b < z.AreaBounds.Count; b++)
                {
                    Rectangle2D rect = z.AreaBounds[b];
                    XmlElement rectNode = doc.CreateElement("Bounds");
                    rectNode.SetAttribute("X", rect.X.ToString());
                    rectNode.SetAttribute("Y", rect.Y.ToString());
                    rectNode.SetAttribute("W", rect.Width.ToString());
                    rectNode.SetAttribute("H", rect.Height.ToString());
                    node.AppendChild(rectNode);
                }
                root.AppendChild(node);
            }
            doc.Save(path);
        }

        private static void LoadConfigurationXML()
        {
            string path = Path.Combine(Core.BaseDirectory, "Data", "DungeonSystem", "DungeonConfigs.xml");
            if (!File.Exists(path)) return;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                XmlNodeList list = doc.SelectNodes("//Zone");
                for (int i = 0; i < list.Count; i++)
                {
                    XmlNode node = list[i];
                    if (int.TryParse(node.Attributes["RCode"]?.Value, out int codeVal))
                    {
                        RegionCode code = (RegionCode)codeVal;
                        if (Zones.TryGetValue(code, out DungeonZone z))
                        {
                            if (bool.TryParse(node.Attributes["IsActive"]?.Value, out bool active)) z.IsActive = active;
                            if (int.TryParse(node.Attributes["TargetHeat"]?.Value, out int heat)) z.TargetHeat = heat;
                            if (int.TryParse(node.Attributes["MaxPop"]?.Value, out int pop)) z.SetPopulation(pop);
                            
                            if (double.TryParse(node.Attributes["ReplenishRate"]?.Value, out double repRate)) z.ReplenishRate = repRate;
                            if (int.TryParse(node.Attributes["HeatDecayWeight"]?.Value, out int hdWeight)) z.HeatDecayWeight = hdWeight;
                            if (double.TryParse(node.Attributes["RestMin"]?.Value, out double rmin)) z.RestDuration = TimeSpan.FromMinutes(rmin);
                            
                            string bossStr = node.Attributes["BossType"]?.Value;
                            if (!string.IsNullOrEmpty(bossStr)) z.BossType = ScriptCompiler.FindTypeByFullName(bossStr);

                            if (bool.TryParse(node.Attributes["EnableRareDrops"]?.Value, out bool rEnable)) z.EnableRareDrops = rEnable;
                            if (int.TryParse(node.Attributes["RareDropHeatThreshold"]?.Value, out int rHeat)) z.RareDropHeatThreshold = rHeat;
                            if (double.TryParse(node.Attributes["RareDropChance"]?.Value, out double rChance)) z.RareDropChance = rChance;
                            if (bool.TryParse(node.Attributes["IsStealable"]?.Value, out bool rSteal)) z.IsStealable = rSteal;

                            // [신규 기획 데이터 로드]
                            if (node.Attributes["GroupName"] != null) z.GroupName = node.Attributes["GroupName"].Value;
                            if (node.Attributes["SubZoneName"] != null) z.SubZoneName = node.Attributes["SubZoneName"].Value;

                            z.SpawnProfileStrings.Clear();
                            XmlNode profilesNode = node.SelectSingleNode("SpawnProfiles");
                            if (profilesNode != null)
                            {
                                foreach (XmlNode pNode in profilesNode.ChildNodes)
                                {
                                    if (pNode.Name == "Tier" && int.TryParse(pNode.Attributes["Level"]?.Value, out int level))
                                    {
                                        string mobs = pNode.Attributes["Mobs"]?.Value ?? "";
                                        z.SpawnProfileStrings[level] = new List<string>(mobs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                                    }
                                }
                            }

                            z.CitySecurityImpact.Clear();
                            XmlNode citiesNode = node.SelectSingleNode("CitySecurity");
                            if (citiesNode != null)
                            {
                                foreach (XmlNode cNode in citiesNode.ChildNodes)
                                {
                                    if (cNode.Name == "City")
                                    {
                                        string cName = cNode.Attributes["Name"]?.Value;
                                        if (!string.IsNullOrEmpty(cName) && double.TryParse(cNode.Attributes["Impact"]?.Value, out double impact))
                                        {
                                            z.CitySecurityImpact[cName] = impact;
                                        }
                                    }
                                }
                            }

                            z.SpecialSpawns.Clear();
                            XmlNode specialsNode = node.SelectSingleNode("SpecialSpawns");
                            if (specialsNode != null)
                            {
                                foreach (XmlNode sNode in specialsNode.ChildNodes)
                                {
                                    if (sNode.Name == "Rule")
                                    {
                                        Type t = ScriptCompiler.FindTypeByFullName(sNode.Attributes["Type"]?.Value ?? "");
                                        if (t != null && int.TryParse(sNode.Attributes["Max"]?.Value, out int max) && double.TryParse(sNode.Attributes["Chance"]?.Value, out double chance))
                                        {
                                            z.SpecialSpawns.Add(new SpecialSpawnRule { MonsterType = t, MaxCount = max, SpawnChance = chance });
                                        }
                                    }
                                }
                            }

                            z.BossGimmick = new BossGimmickRule();
                            XmlNode gimmickNode = node.SelectSingleNode("BossGimmick");
                            if (gimmickNode != null)
                            {
                                Type t = ScriptCompiler.FindTypeByFullName(gimmickNode.Attributes["Type"]?.Value ?? "");
                                if (t != null)
                                {
                                    z.BossGimmick.BossType = t;
                                    if (bool.TryParse(gimmickNode.Attributes["RequiresAltar"]?.Value, out bool reqAltar)) z.BossGimmick.RequiresAltar = reqAltar;
                                    
                                    foreach (XmlNode reqNode in gimmickNode.ChildNodes)
                                    {
                                        if (reqNode.Name == "ReqItem")
                                        {
                                            Type iType = ScriptCompiler.FindTypeByFullName(reqNode.Attributes["Type"]?.Value ?? "");
                                            if (iType != null && int.TryParse(reqNode.Attributes["Amount"]?.Value, out int amt))
                                            {
                                                z.BossGimmick.RequiredItems[iType] = amt;
                                            }
                                        }
                                    }
                                }
                            }

                            z.AreaBounds.Clear();
                            foreach (XmlNode child in node.ChildNodes)
                            {
                                if (child.Name == "Bounds" &&
                                    int.TryParse(child.Attributes["X"]?.Value, out int x) &&
                                    int.TryParse(child.Attributes["Y"]?.Value, out int y) &&
                                    int.TryParse(child.Attributes["W"]?.Value, out int w) &&
                                    int.TryParse(child.Attributes["H"]?.Value, out int h))
                                {
                                    z.AreaBounds.Add(new Rectangle2D(x, y, w, h));
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private static void SaveLiveStateBIN()
        {
            string path = Path.Combine(Core.BaseDirectory, "Saves", "DungeonSystem", "LiveState.bin");
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryFileWriter w = new BinaryFileWriter(fs, true);
                w.Write(0); 

                w.Write(ZoneList.Count);
                for (int i = 0; i < ZoneList.Count; i++)
                {
                    DungeonZone z = ZoneList[i];
                    w.Write((int)z.RCode);
                    w.Write(z.CurrentHeat);
                    w.Write((int)z.Phase);
                    w.Write(z.CooldownEndTime);
                    w.Write(z.LastKillTime);

                    int mobCount = 0;
                    for (int j = 0; j < z.ActiveMonsters.Count; j++) if (z.ActiveMonsters[j] != null && !z.ActiveMonsters[j].Deleted) mobCount++;
                    w.Write(mobCount);
                    for (int j = 0; j < z.ActiveMonsters.Count; j++) if (z.ActiveMonsters[j] != null && !z.ActiveMonsters[j].Deleted) w.Write(z.ActiveMonsters[j]);

                    int itemCount = 0;
                    for (int j = 0; j < z.ActiveItems.Count; j++) if (z.ActiveItems[j] != null && !z.ActiveItems[j].Deleted) itemCount++;
                    w.Write(itemCount);
                    for (int j = 0; j < z.ActiveItems.Count; j++) if (z.ActiveItems[j] != null && !z.ActiveItems[j].Deleted) w.Write(z.ActiveItems[j]);
                }
                w.Close();
            }
        }

        private static void LoadLiveStateBIN()
        {
            string path = Path.Combine(Core.BaseDirectory, "Saves", "DungeonSystem", "LiveState.bin");
            if (!File.Exists(path)) return;

            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    BinaryFileReader r = new BinaryFileReader(new BinaryReader(fs));
                    int version = r.ReadInt();

                    int count = r.ReadInt();
                    for (int i = 0; i < count; i++)
                    {
                        RegionCode code = (RegionCode)r.ReadInt();
                        int curHeat = r.ReadInt();
                        DungeonPhase phase = (DungeonPhase)r.ReadInt();
                        DateTime cdEnd = r.ReadDateTime();
                        DateTime lastKill = r.ReadDateTime();

                        List<BaseCreature> mobs = new List<BaseCreature>();
                        int mCount = r.ReadInt();
                        for (int j = 0; j < mCount; j++)
                        {
                            Mobile m = r.ReadMobile();
                            if (m is BaseCreature bc && !bc.Deleted) mobs.Add(bc);
                        }

                        List<Item> items = new List<Item>();
                        int iCount = r.ReadInt();
                        for (int j = 0; j < iCount; j++)
                        {
                            Item it = r.ReadItem();
                            if (it != null && !it.Deleted) items.Add(it);
                        }

                        if (Zones.TryGetValue(code, out DungeonZone z))
                        {
                            z.CurrentHeat = curHeat;
                            z.Phase = phase;
                            z.CooldownEndTime = cdEnd;
                            z.LastKillTime = lastKill;
                            z.ActiveMonsters = mobs;
                            z.ActiveItems = items;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[DungeonManager] 라이브 상태 로드 실패: {ex.Message}");
            }
        }
    }
}