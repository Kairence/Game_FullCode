using System;
using System.Collections.Generic;
using System.Collections.Frozen;
using System.IO;
using System.Linq;
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
            foreach (var node in Nodes.ToList()) if (node != nodeToKeep && !node.Deleted) node.Delete();
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
            foreach (var node in Nodes)
            {
                var field = node.GetType().GetField("m_Spawned", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null && field.GetValue(node) is List<Mobile> list)
                {
                    foreach (var m in list.ToList()) m?.Delete();
                    list.Clear();
                }
            }
        }
    }

    // ========================================================================
    // ⚔️ DungeonZone: 열기(Heat) 에스컬레이션 및 중앙 통제 엔진
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

        public int TargetHeat { get; set; }      
        public int CurrentHeat { get; set; }    
        public int MaxDifficulty { get => TargetHeat; set => TargetHeat = value; }
        public int CurrentDifficulty { get => CurrentHeat; set => CurrentHeat = value; }

        public Type BossType { get; set; }
        public TimeSpan CooldownDuration { get; set; }
        public DateTime CooldownEndTime { get; set; }
        public DungeonPhase Phase { get; set; }
        public int MaxPopulation { get; private set; }
        public int ManualMaxPopulation { get; private set; } = -1;
        
        public Dictionary<DungeonDepth, int> Quotas { get; private set; } = new();
        public Dictionary<DungeonDepth, List<BaseCreature>> ActiveMonsters { get; set; } = new();
        public Dictionary<DungeonDepth, List<Item>> ActiveItems { get; set; } = new();
        public List<DungeonNode> Nodes { get; set; } = new();
        public Dictionary<DungeonDepth, Type[]> SpawnProfiles { get; set; } = new();
        public List<Type> UniqueTypes { get; set; } = new(); 

        private DateTime m_NextRespawnTime;
        private int m_GoIndex = -1;

        public DungeonZone(RegionCode code, Map map, int targetHeat, Type bossType, TimeSpan cooldown)
        {
            RCode = code;
            Facet = map; 
            TargetHeat = targetHeat; 
            BossType = bossType; 
            CooldownDuration = cooldown; 
            Phase = DungeonPhase.Active;
            CurrentHeat = bossType != null ? (int)(targetHeat * 0.2) : (int)(targetHeat * 0.5);

            foreach (DungeonDepth d in Enum.GetValues(typeof(DungeonDepth)))
            { ActiveMonsters[d] = new(); ActiveItems[d] = new(); Quotas[d] = 0; }
        }

        public void AddUnique(Type t) { if (!UniqueTypes.Contains(t)) UniqueTypes.Add(t); }

        public void SetPopulation(int maxPop) 
        { 
            ManualMaxPopulation = maxPop; 
            CacheNodes(); 
        }

        public void SetSpawnProfile(DungeonDepth depth, Type[] types) => SpawnProfiles[depth] = types.Where(x => x != null && !x.Name.ToLower().Contains("summon")).ToArray();
        public void SetSpawnProfile(int tier, Type[] types) => SetSpawnProfile((DungeonDepth)tier, types);

        public void KeepCurrentNodeOnly(Mobile m)
        {
            if (Nodes.Count <= 1) return;
            var nodeToKeep = Nodes[m_GoIndex >= 0 ? m_GoIndex : 0];
            foreach (var node in Nodes.ToList()) if (node != nodeToKeep && !node.Deleted) node.Delete();
            m_GoIndex = 0; CacheNodes(); 
        }

        public void GoToNextNode(Mobile m)
        {
            if (Nodes.Count == 0) return;
            if (++m_GoIndex >= Nodes.Count) m_GoIndex = 0;
            if (Nodes[m_GoIndex] is DungeonNode target && !target.Deleted) m.MoveToWorld(target.Location, target.Map);
        }

        public void CacheNodes()
        {
            Nodes.Clear();
            foreach (Item item in World.Items.Values)
            {
                if (item is DungeonNode node && node.RCode == this.RCode)
                {
                    if (this.Facet == null) this.Facet = node.Map; 
                    if (node.Map == this.Facet) Nodes.Add(node);
                }
            }

            int totalArea = RegionSaver.GetRealArea(this.RCode);
            MaxPopulation = ManualMaxPopulation >= 0 ? ManualMaxPopulation : (totalArea > 0 ? Math.Max(Nodes.Count * 10, Math.Min(totalArea / 600, 250)) : Nodes.Count * 15);
            Quotas[DungeonDepth.Entrance] = (int)(MaxPopulation * 0.25); Quotas[DungeonDepth.Middle] = (int)(MaxPopulation * 0.55); Quotas[DungeonDepth.Deep] = (int)(MaxPopulation * 0.20);
        }

        public void CheckRespawn()
        {
            // 이제 30분마다 호출되므로 짧은 시간 차단 로직(m_NextRespawnTime)은 사실상 항상 패스합니다.
            if (Phase != DungeonPhase.Active || DateTime.Now < m_NextRespawnTime) return;
            if (Nodes.Count == 0) { CacheNodes(); if (Nodes.Count == 0) return; }
            if (!NewSpawnManager.ActiveMaps.GetValueOrDefault(Facet, true)) return;

            foreach (var list in ActiveMonsters.Values) list.RemoveAll(m => m == null || m.Deleted || !m.Alive);
            foreach (var list in ActiveItems.Values) list.RemoveAll(i => i == null || i.Deleted || i.Map == null);
            if (ActiveMonsters.Values.Sum(l => l.Count) + ActiveItems.Values.Sum(l => l.Count) >= MaxPopulation) return;

            double heatRatio = TargetHeat > 0 ? (double)CurrentHeat / TargetHeat : 0;
            DungeonDepth spawnTier = heatRatio >= 0.85 ? DungeonDepth.Deep : (heatRatio >= 0.45 ? DungeonDepth.Middle : DungeonDepth.Entrance);
            bool spawned = false;

            foreach (DungeonDepth depth in Enum.GetValues(typeof(DungeonDepth)))
            {
                if (depth == DungeonDepth.BossRoom) continue;
                int missing = Quotas.GetValueOrDefault(depth) - (ActiveMonsters[depth].Count + ActiveItems[depth].Count);
                if (missing <= 0) continue;

                var vNodes = Nodes.Where(n => n.Depth == depth).ToList();
                if (vNodes.Count == 0) vNodes = Nodes.ToList();
                
                Type[] av = SpawnProfiles.GetValueOrDefault(spawnTier) ?? SpawnProfiles.Values.FirstOrDefault(p => p != null && p.Length > 0);
                if (av == null) continue;

                // 🌟 [최적화 패치] 30분치 스폰이므로 Math.Min(missing, 5)를 제거하고 missing만큼 풀로 스폰합니다.
                for (int i = 0; i < missing; i++)
                {
                    Point3D? loc = vNodes[Utility.Random(vNodes.Count)].GetValidSpawnLocation();
                    if (loc.HasValue)
                    {
                        try
                        {
                            Type selected = av[Utility.Random(av.Length)];
                            if (UniqueTypes.Contains(selected) && ActiveMonsters.Values.Any(l => l.Any(m => m.GetType() == selected))) continue;

                            object obj = Activator.CreateInstance(selected);
                            if (obj is BaseCreature bc) 
                            { 
                                bool isWaterMob = bc is Kraken || bc is SeaSerpent || bc is DeepSeaSerpent || bc is WaterElemental;
                                int tileID = Facet.Tiles.GetLandTile(loc.Value.X, loc.Value.Y).ID;
                                bool isWaterTile = (tileID >= 0x00A8 && tileID <= 0x00AB) || (tileID >= 0x0136 && tileID <= 0x0137);

                                if (isWaterMob && !isWaterTile)
                                {
                                    bc.Delete(); 
                                    continue;
                                }

                                bc.MoveToWorld(loc.Value, Facet); 
                                if (heatRatio > 0.5 && Utility.RandomDouble() < (heatRatio - 0.4)) bc.Grade = Utility.RandomDouble() < 0.2 ? 3 : 2; 
                                ActiveMonsters[depth].Add(bc); 
                                spawned = true; 
                            }
                            else if (obj is Item it) 
                            { 
                                it.MoveToWorld(loc.Value, Facet); ActiveItems[depth].Add(it); spawned = true; 
                            }
                        }
                        catch { }
                    }
                }
            }
            if (spawned) m_NextRespawnTime = DateTime.Now + TimeSpan.FromSeconds(Utility.RandomMinMax(10, 20));
        }

        public void ProcessDeath(BaseCreature bc)
        {
            if (Phase == DungeonPhase.Cooldown) return;
            if (Phase == DungeonPhase.BossSpawned && bc.GetType() == BossType)
            {
                Phase = DungeonPhase.Cooldown; CooldownEndTime = DateTime.Now + CooldownDuration; ClearAllSpawns(); return;
            }

            CurrentHeat += Math.Max(1, (bc.Fame / 500)) * (bc.Grade == 1 ? 1 : bc.Grade == 2 ? 2 : bc.Grade == 3 ? 3 : 4);
            if (CurrentHeat > TargetHeat) CurrentHeat = TargetHeat;
            if (CurrentHeat >= TargetHeat && BossType != null && Phase == DungeonPhase.Active)
            {
                Phase = DungeonPhase.BossSpawned; ClearAllSpawns(); 
                var deepNodes = Nodes.Where(n => n.Depth == DungeonDepth.Deep).ToList();
                if (deepNodes.Count == 0) deepNodes = Nodes.ToList();
                if (deepNodes.Count > 0) try { BaseCreature b = (BaseCreature)Activator.CreateInstance(BossType); b.MoveToWorld(deepNodes[Utility.Random(deepNodes.Count)].Location, Facet); ActiveMonsters[DungeonDepth.Deep].Add(b); } catch { } 
            }
        }

        public void ClearAllSpawns()
        {
            foreach (var list in ActiveMonsters.Values) { foreach (var m in list.ToList()) m?.Delete(); list.Clear(); }
            foreach (var list in ActiveItems.Values) { foreach (var i in list.ToList()) i?.Delete(); list.Clear(); }
        }

        public void PerformRecovery() 
        { 
            int minHeat = BossType != null ? (int)(TargetHeat * 0.2) : (int)(TargetHeat * 0.5);
            // 🌟 [최적화 패치] 30분에 한 번 갱신되므로 120으로 나누던 것을 4로 나누어 30배 속도로 보정
            if (CurrentHeat > minHeat) CurrentHeat = Math.Max(minHeat, CurrentHeat - (TargetHeat / 4)); 
            if (Phase == DungeonPhase.Cooldown && DateTime.Now >= CooldownEndTime) { CurrentHeat = minHeat; Phase = DungeonPhase.Active; }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(4); 
            writer.Write((int)RCode); 
            writer.Write(CurrentHeat); 
            writer.Write((int)Phase);
            writer.Write(CooldownEndTime); 
            writer.Write(ManualMaxPopulation);
            foreach (DungeonDepth depth in Enum.GetValues(typeof(DungeonDepth)))
            {
                writer.Write((int)depth); 
                writer.Write(ActiveMonsters[depth].Count);
                foreach (var m in ActiveMonsters[depth]) writer.Write(m);
                writer.Write(ActiveItems[depth].Count); 
                foreach (var i in ActiveItems[depth]) writer.Write(i);
            }
        }

        public DungeonZone(GenericReader reader)
        {
            foreach (DungeonDepth depth in Enum.GetValues(typeof(DungeonDepth))) 
            { ActiveMonsters[depth] = new(); ActiveItems[depth] = new(); Quotas[depth] = 0; }

            int version = reader.ReadInt(); 
            
            if (version >= 3) 
                RCode = (RegionCode)reader.ReadInt(); 
            else 
            { 
                reader.ReadString(); 
                RCode = RegionCode.None; 
            }
            
            CurrentHeat = reader.ReadInt(); 
            Phase = (DungeonPhase)reader.ReadInt(); 
            CooldownEndTime = reader.ReadDateTime(); 
            ManualMaxPopulation = (version >= 1) ? reader.ReadInt() : -2;
            
            if (version >= 2)
            {
                foreach (DungeonDepth depth in Enum.GetValues(typeof(DungeonDepth))) 
                { 
                    reader.ReadInt(); 
                    int mCount = reader.ReadInt(); 
                    for (int i = 0; i < mCount; i++) 
                    {
                        Mobile m = reader.ReadMobile();
                        if (m is BaseCreature bc && !bc.Deleted) ActiveMonsters[depth].Add(bc);
                    }
                    
                    int iCount = reader.ReadInt(); 
                    for (int i = 0; i < iCount; i++) 
                    {
                        Item it = reader.ReadItem();
                        if (it != null && !it.Deleted) ActiveItems[depth].Add(it);
                    }
                }
            }
        }
        public int GetTotalActiveCount() => ActiveMonsters.Values.Sum(l => l.Count) + ActiveItems.Values.Sum(l => l.Count);
    }

    // ========================================================================
    // 🌍 EcosystemManager: 생태계 구역 캐싱 및 환경 보너스 계산
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

        public static void FreezeData() { Zones = m_TempZones.ToFrozenDictionary(); ZoneList = Zones.Values.OrderByDescending(z => z.Facet == Map.Trammel).ThenBy(z => z.Facet?.MapID ?? 99).ThenBy(z => (int)z.RCode).ToList(); }
        public static void RemoveZone(RegionCode code) { if (m_TempZones.ContainsKey(code)) m_TempZones.Remove(code); }

        public static (double polarBonus, double obsidianBonus, double soulBonus) GetEnvironmentBonus(Point3D loc, Map map)
        {
            var nearestNode = ZoneList.Where(z => z.Facet == map).SelectMany(z => z.Nodes).Where(n => Math.Max(Math.Abs(n.X - loc.X), Math.Abs(n.Y - loc.Y)) <= n.SpawnRange).OrderBy(n => Math.Max(Math.Abs(n.X - loc.X), Math.Abs(n.Y - loc.Y))).FirstOrDefault();
            if (nearestNode == null) return (1.0, 1.0, 1.0); 
            double polar = 1.0, obsidian = 1.0, soul = 1.0;
            if (nearestNode.ClimateType == EcoClimateType.Arctic) { polar += 5.0; soul += 1.5; } else if (nearestNode.ClimateType == EcoClimateType.Volcanic || nearestNode.RCode.ToString().Contains("Hythloth")) { obsidian += 5.0; soul -= 0.5; } else if (nearestNode.ClimateType == EcoClimateType.Void) { soul += 5.0; }
            return (Math.Max(0.1, polar), Math.Max(0.1, obsidian), Math.Max(0.1, soul));
        }
		// ==============================================================================
        // 🌟 [MasterTick 파이프라인] 틱 51~59에 호출되는 대륙별 야생 생태계 갱신 로직
        // ==============================================================================
        public static void ProcessFacetEcosystem(Map facet)
        {
            var zones = ZoneList.Where(z => z.Facet == facet).ToList();
            foreach (var zone in zones)
            {
                foreach (var node in zone.Nodes)
                {
                    if (node != null && !node.Deleted)
                    {
                        // 30분에 한 번씩 대륙 단위로 스폰/정산 로직 실행
                        node.DoTick();
                    }
                }
            }
        }
    }

    // ========================================================================
    // ⚔️ DungeonManager: 전체 데이터 저장/로드 및 예외 처리
    // ========================================================================
    public static class DungeonManager
    {
        public static FrozenDictionary<RegionCode, DungeonZone> Zones { get; private set; }
        public static List<DungeonZone> ZoneList { get; private set; } = new();
        private static Dictionary<RegionCode, DungeonZone> m_TempZones = new();

        public static void RegisterZone(DungeonZone zone) { if (zone != null && zone.RCode != RegionCode.None) m_TempZones[zone.RCode] = zone; }
        public static void FreezeData() { Zones = m_TempZones.ToFrozenDictionary(); ZoneList = Zones.Values.OrderBy(z => z.Facet?.MapID ?? 99).ThenBy(z => (int)z.RCode).ToList(); }

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
            EventSink.WorldSave += OnSave; EventSink.WorldLoad += OnLoad; EventSink.ServerStarted += OnServerStarted; 
        }

        public static void NukeDungeon(Mobile from)
        {
            RegionCode currentCode = RegionSaver.GetRegionCode(from.Map, from.X, from.Y, from.Z);
            if (currentCode == RegionCode.None) { from.SendMessage(33, "던전 구역 안에서 명령어를 실행해 주세요."); return; }

            int targetBase = ((int)currentCode / 100) * 100;
            List<Mobile> targets = World.Mobiles.Values.Where(m => m is BaseCreature bc && !bc.Controlled && !bc.Summoned && bc.Map == from.Map && ((int)RegionSaver.GetRegionCode(bc.Map, bc.X, bc.Y, bc.Z) / 100) * 100 == targetBase).ToList();
            foreach (Mobile m in targets) m.Delete();
            foreach (var dz in ZoneList) if (((int)dz.RCode / 100) * 100 == targetBase || dz.RCode == currentCode) dz.ClearAllSpawns();
            from.SendMessage(66, $"{targetBase} 던전의 미아 몬스터 {targets.Count}마리를 모두 소거했습니다.");
        }

        public static void Initialize() { }

        private static void OnServerStarted() 
        { 
            foreach (var d in ZoneList) d.CacheNodes(); 
            EcosystemManager.RebuildZones(); 
            // 🌟 [수정] 기존 1분짜리 타이머 삭제 (MasterTickEngine으로 이관)
        }

        // ==============================================================================
        // 🌟 [MasterTick 파이프라인] 틱 51~59에 ResourceManager를 거쳐 호출됨
        // ==============================================================================
        public static void ProcessFacetDungeons(Map facet)
        {
            var zones = ZoneList.Where(z => z.Facet == facet).ToList();
            foreach (var zone in zones)
            {
                zone.CheckRespawn();
                zone.PerformRecovery();
            }
        }

        public static void ProcessRemainingDungeons()
        {
            var zones = ZoneList.Where(z => z.Facet != Map.Trammel && z.Facet != Map.Felucca).ToList();
            foreach (var zone in zones)
            {
                zone.CheckRespawn();
                zone.PerformRecovery();
            }
        }

        public static void OnCreatureKilled(BaseCreature bc) { if (bc == null || bc.Controlled || bc.Summoned) return; RegionCode locCode = RegionSaver.GetRegionCode(bc.Map, bc.X, bc.Y, bc.Z); if (locCode != RegionCode.None && Zones.TryGetValue(locCode, out DungeonZone zone) && zone.Facet == bc.Map && zone.ActiveMonsters.Values.Any(l => l.Contains(bc))) zone.ProcessDeath(bc); }
        
        // 🌟 [수정] OnTick 메서드 삭제 (MasterTickEngine으로 대체됨)

        public static string CleanString(string input) => new string((input ?? "").Where(char.IsLetterOrDigit).ToArray()).ToLower();

        public static Map ResolveMapByName(string name) { if (name.Contains("Felucca")) return Map.Felucca; if (name.Contains("Trammel")) return Map.Trammel; if (name.Contains("Ilshenar")) return Map.Ilshenar; if (name.Contains("Malas")) return Map.Malas; if (name.Contains("Tokuno")) return Map.Tokuno; if (name.Contains("TerMur")) return Map.TerMur; return Map.Trammel; }

        private static void OnSave(WorldSaveEventArgs e)
        {
            string path = Path.Combine(Core.BaseDirectory, "Saves", "RespawnSystem", "DungeonZones.bin"); 
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)) 
            { 
                BinaryFileWriter w = new BinaryFileWriter(fs, true); 
                w.Write(ZoneList.Count); 
                foreach (var d in ZoneList) d.Serialize(w); 
                
                w.Close();
            }
        }

        private static void OnLoad()
        {
            string path = Path.Combine(Core.BaseDirectory, "Saves", "RespawnSystem", "DungeonZones.bin");
            if (!File.Exists(path)) return;
            
            try
            {
                using (FileStream fs = new(path, FileMode.Open)) 
                { 
                    BinaryFileReader r = new(new BinaryReader(fs)); 
                    int c = r.ReadInt(); 
                    for (int i = 0; i < c; i++) 
                    { 
                        DungeonZone ld = new(r); 
                        if (ld.RCode != RegionCode.None && Zones.TryGetValue(ld.RCode, out var ex)) 
                        { 
                            ex.CurrentHeat = ld.CurrentHeat; 
                            ex.Phase = ld.Phase; 
                            ex.CooldownEndTime = ld.CooldownEndTime; 
                            ex.ActiveMonsters = ld.ActiveMonsters; 
                            ex.ActiveItems = ld.ActiveItems; 
                            if (ld.ManualMaxPopulation != -2) ex.SetPopulation(ld.ManualMaxPopulation); 
                        } 
                    } 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[DungeonManager] 경고: 던전 세이브 데이터를 불러올 수 없습니다. (사유: {ex.Message})");
                Console.WriteLine("[DungeonManager] 새로운 데이터 구조로 덮어쓰기되며 열기(Heat)가 초기화됩니다.\n");
            }
        }
    }
}