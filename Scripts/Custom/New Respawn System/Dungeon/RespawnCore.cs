using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
    public enum DungeonPhase { Active, BossSpawned, Cooldown }

    // ========================================================================
    // 🌟 [통합/개편됨] EcoZone은 이제 UI(모니터링 Gump)용 그룹핑 껍데기 역할만 합니다.
    // 실제 몬스터 스폰과 마릿수 관리는 개별 EcoNode가 100% 독립적으로 수행하므로
    // 무거운 SpeciesState나 Vitality 추적 로직은 완전히 삭제되었습니다.
    // ========================================================================
    public class EcoZone
    {
        public string ZoneId { get; set; }
        public Map Facet { get; set; }
        public List<EcoNode> Nodes { get; set; } = new List<EcoNode>();
        
        private int m_GoIndex = -1;

        public EcoZone(string zoneId, Map map) { ZoneId = zoneId; Facet = map; }

        public void GoToNextNode(Mobile m)
        {
            if (Nodes == null || Nodes.Count == 0)
            {
                m.SendMessage(33, "이 구역에는 등록된 노드가 없습니다.");
                return;
            }
            
            m_GoIndex++;
            if (m_GoIndex >= Nodes.Count) m_GoIndex = 0; 
            
            EcoNode target = Nodes[m_GoIndex];
            if (target != null && !target.Deleted && target.Map != null && target.Map != Map.Internal)
            {
                m.MoveToWorld(target.Location, target.Map);
                m.SendMessage(66, $"[{ZoneId}] {m_GoIndex + 1} / {Nodes.Count} 번째 생태계 노드로 이동했습니다.");
            }
        }	

        public void KeepCurrentNodeOnly(Mobile m)
        {
            if (Nodes == null || Nodes.Count <= 1)
            {
                m.SendMessage(33, "삭제할 중복 노드가 없습니다.");
                return;
            }

            int keepIndex = (m_GoIndex >= 0 && m_GoIndex < Nodes.Count) ? m_GoIndex : 0;
            var nodeToKeep = Nodes[keepIndex];
            int deletedCount = 0;

            foreach (var node in Nodes.ToList())
            {
                if (node != nodeToKeep && node != null && !node.Deleted)
                {
                    node.Delete();
                    deletedCount++;
                }
            }
            
            m_GoIndex = 0;
            CacheNodes(); 
            m.SendMessage(66, $"[{ZoneId}] 현재 위치한 노드를 대표로 지정하고 {deletedCount}개의 중복 노드를 삭제했습니다.");
        }

        public void CacheNodes()
        {
            if (Facet == null || Facet == Map.Internal) Facet = DungeonManager.ResolveMapByName(ZoneId);
            Nodes.Clear();
            string myClean = DungeonManager.CleanString(ZoneId);
            foreach (Item item in World.Items.Values)
            {
                if (item is EcoNode node && (node.Map == Facet || Facet == null))
                {
                    string nodeClean = DungeonManager.CleanString(node.ZoneId);
                    if (nodeClean.Contains(myClean) || myClean.Contains(nodeClean)) Nodes.Add(node);
                }
            }
        }

        // 🌟 생태계 몬스터 강제 리셋 시 사용 (리플렉션으로 EcoNode 내부의 m_Spawned를 직접 지움)
        public void ClearAllSpawns()
        {
            foreach (var node in Nodes)
            {
                var field = node.GetType().GetField("m_Spawned", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null && field.GetValue(node) is List<Mobile> list)
                {
                    foreach(var m in list.ToList()) m?.Delete();
                    list.Clear();
                }
            }
        }
    }

    // ========================================================================
    // ⚔️ DungeonZone (던전 로직은 기존 시스템의 핵심이므로 완벽히 유지됩니다)
    // ========================================================================
    public class DungeonZone
    {
        public string ZoneId { get; set; }
        public Map Facet { get; set; }
        public int MaxDifficulty { get; set; }
        public int CurrentDifficulty { get; set; }
        public int BossThreshold { get; set; }
        public Type BossType { get; set; }
        public TimeSpan CooldownDuration { get; set; }
        public DateTime CooldownEndTime { get; set; }
        public DungeonPhase Phase { get; set; }
        public int MaxPopulation { get; private set; }
        public int ManualMaxPopulation { get; private set; } = -1;
        public Dictionary<DungeonDepth, int> Quotas { get; private set; } = new Dictionary<DungeonDepth, int>();
        public Dictionary<DungeonDepth, List<BaseCreature>> ActiveMonsters { get; set; } = new Dictionary<DungeonDepth, List<BaseCreature>>();
        public Dictionary<DungeonDepth, List<Item>> ActiveItems { get; set; } = new Dictionary<DungeonDepth, List<Item>>();
        public List<DungeonNode> Nodes { get; set; } = new List<DungeonNode>();
        public Dictionary<DungeonDepth, Type[]> SpawnProfiles { get; set; } = new Dictionary<DungeonDepth, Type[]>();
        private DateTime m_NextRespawnTime;

        private int m_GoIndex = -1;

        public void KeepCurrentNodeOnly(Mobile m)
        {
            if (Nodes == null || Nodes.Count <= 1) return;
            int keepIndex = (m_GoIndex >= 0 && m_GoIndex < Nodes.Count) ? m_GoIndex : 0;
            var nodeToKeep = Nodes[keepIndex];
            int deletedCount = 0;

            foreach (var node in Nodes.ToList())
            {
                if (node != nodeToKeep && node != null && !node.Deleted) { node.Delete(); deletedCount++; }
            }
            
            m_GoIndex = 0; CacheNodes(); 
            m.SendMessage(66, $"[{ZoneId}] 중복 노드 {deletedCount}개 삭제 완료.");
        }

        public void GoToNextNode(Mobile m)
        {
            if (Nodes == null || Nodes.Count == 0) return;
            m_GoIndex++; if (m_GoIndex >= Nodes.Count) m_GoIndex = 0; 
            DungeonNode target = Nodes[m_GoIndex];
            if (target != null && !target.Deleted && target.Map != null && target.Map != Map.Internal)
                m.MoveToWorld(target.Location, target.Map);
        }

        public DungeonZone(string zoneId, Map map, int maxDiff, Type bossType, TimeSpan cooldown)
        {
            ZoneId = zoneId; Facet = map; MaxDifficulty = maxDiff; CurrentDifficulty = maxDiff; BossType = bossType; CooldownDuration = cooldown; Phase = DungeonPhase.Active; m_NextRespawnTime = DateTime.MinValue;
            foreach (DungeonDepth depth in Enum.GetValues(typeof(DungeonDepth))) { ActiveMonsters[depth] = new List<BaseCreature>(); ActiveItems[depth] = new List<Item>(); Quotas[depth] = 0; }
        }

        public void SetPopulation(int maxPop) => ManualMaxPopulation = maxPop;

        public DungeonZone(GenericReader reader)
        {
            int version = reader.ReadInt(); ZoneId = reader.ReadString(); CurrentDifficulty = reader.ReadInt(); Phase = (DungeonPhase)reader.ReadInt(); CooldownEndTime = reader.ReadDateTime();
            ManualMaxPopulation = (version >= 1) ? reader.ReadInt() : -2;
            foreach (DungeonDepth depth in Enum.GetValues(typeof(DungeonDepth))) { ActiveMonsters[depth] = new List<BaseCreature>(); ActiveItems[depth] = new List<Item>(); Quotas[depth] = 0; }
            if (version >= 2) { foreach (DungeonDepth depth in Enum.GetValues(typeof(DungeonDepth))) { reader.ReadInt(); int mCount = reader.ReadInt(); for (int i = 0; i < mCount; i++) { BaseCreature m = reader.ReadMobile() as BaseCreature; if (m != null && !m.Deleted) ActiveMonsters[depth].Add(m); } int iCount = reader.ReadInt(); for (int i = 0; i < iCount; i++) { Item it = reader.ReadItem(); if (it != null && !it.Deleted) ActiveItems[depth].Add(it); } } }
            m_NextRespawnTime = DateTime.MinValue;
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(2); writer.Write(ZoneId); writer.Write(CurrentDifficulty); writer.Write((int)Phase); writer.Write(CooldownEndTime); writer.Write(ManualMaxPopulation);
            foreach (DungeonDepth depth in Enum.GetValues(typeof(DungeonDepth))) { writer.Write((int)depth); writer.Write(ActiveMonsters[depth].Count); foreach (var m in ActiveMonsters[depth]) writer.Write(m); writer.Write(ActiveItems[depth].Count); foreach (var i in ActiveItems[depth]) writer.Write(i); }
        }

        public void SetSpawnProfile(DungeonDepth d, Type[] t) => SpawnProfiles[d] = t.Where(x => !x.Name.ToLower().Contains("summon")).ToArray();

        public void CacheNodes()
        {
            if (Facet == null || Facet == Map.Internal) Facet = DungeonManager.ResolveMapByName(ZoneId);
            Nodes.Clear();
            HashSet<Region> regions = new HashSet<Region>();
            string myClean = DungeonManager.CleanString(ZoneId);

            foreach (Item item in World.Items.Values)
            {
                if (item is DungeonNode node && (node.Map == Facet || Facet == null))
                {
                    string nodeClean = DungeonManager.CleanString(node.ZoneId);
                    if (nodeClean.Contains(myClean) || myClean.Contains(nodeClean))
                    {
                        Nodes.Add(node);
                        Region reg = Region.Find(node.Location, node.Map);
                        if (reg != null) regions.Add(reg);
                    }
                }
            }

            if (ManualMaxPopulation >= 0) MaxPopulation = ManualMaxPopulation;
            else
            {
                int totalArea = 0;
                foreach (Region r in regions) { if (r.Area != null) foreach (var rect in r.Area) totalArea += (Math.Abs(rect.End.X - rect.Start.X) * Math.Abs(rect.End.Y - rect.Start.Y)); }
                MaxPopulation = totalArea > 0 ? Math.Min(totalArea / 600, 200) : 30;
            }

            Quotas[DungeonDepth.Entrance] = (int)(MaxPopulation * 0.10);
            Quotas[DungeonDepth.Middle] = (int)(MaxPopulation * 0.30);
            Quotas[DungeonDepth.Deep] = (int)(MaxPopulation * 0.60);
            Quotas[DungeonDepth.BossRoom] = (MaxPopulation == 0) ? 0 : 1;
        }

        public void CheckRespawn()
        {
            if (Phase != DungeonPhase.Active || DateTime.Now < m_NextRespawnTime) return;
            if (Nodes.Count == 0) CacheNodes();
            if (Nodes.Count == 0) return;

            bool spawned = false;
            foreach (DungeonDepth depth in Enum.GetValues(typeof(DungeonDepth)))
            {
                if (depth == DungeonDepth.BossRoom) continue;
                ActiveMonsters[depth].RemoveAll(m => m == null || m.Deleted || !m.Alive);
                ActiveItems[depth].RemoveAll(i => i == null || i.Deleted || i.Map == null || i.Map == Map.Internal);

                int missing = Quotas[depth] - (ActiveMonsters[depth].Count + ActiveItems[depth].Count);
                if (missing <= 0) continue;

                List<DungeonNode> vNodes = Nodes.Where(n => n.Depth == depth).ToList();
                if (vNodes.Count == 0) vNodes = Nodes.ToList();
                vNodes = vNodes.OrderBy(x => Utility.RandomDouble()).ToList();

                Type[] av = SpawnProfiles.ContainsKey(depth) ? SpawnProfiles[depth] : SpawnProfiles.Values.FirstOrDefault(p => p != null && p.Length > 0);
                if (av == null) continue;

                int count = Math.Min(missing, 5);
                for (int i = 0; i < count; i++)
                {
                    DungeonNode n = vNodes[i % vNodes.Count]; Point3D? loc = n.GetValidSpawnLocation();
                    if (loc.HasValue) { try { object obj = Activator.CreateInstance(av[Utility.Random(av.Length)]); if (obj is BaseCreature m) { m.MoveToWorld(loc.Value, Facet); ActiveMonsters[depth].Add(m); spawned = true; } else if (obj is Item it) { it.MoveToWorld(loc.Value, Facet); ActiveItems[depth].Add(it); spawned = true; } } catch { } }
                }
            }
            if (spawned) m_NextRespawnTime = DateTime.Now + TimeSpan.FromMinutes(1 + (int)(4 * (1.0 - (double)CurrentDifficulty / Math.Max(1, MaxDifficulty))));
        }

        public void ProcessDeath(BaseCreature bc)
        {
            if (Phase == DungeonPhase.Cooldown) return;
            if (Phase == DungeonPhase.BossSpawned && bc.GetType() == BossType) { Phase = DungeonPhase.Cooldown; CooldownEndTime = DateTime.Now + CooldownDuration; ClearAllSpawns(); return; }
            int mult = (bc.Grade == 1) ? 1 : (bc.Grade <= 5 ? 2 : (bc.Grade == 6 ? 3 : 4));
            CurrentDifficulty = Math.Max(0, CurrentDifficulty - (bc.Fame / 100 * mult));
            if (CurrentDifficulty <= BossThreshold) SpawnBoss();
        }

        public void ClearAllSpawns()
        {
            foreach (var list in ActiveMonsters.Values) { foreach (var m in list.ToList()) { if (m != null && !m.Deleted) m.Delete(); } list.Clear(); }
            foreach (var list in ActiveItems.Values) { foreach (var i in list.ToList()) { if (i != null && !i.Deleted) i.Delete(); } list.Clear(); }
        }

        private void SpawnBoss() { if (BossType == null || MaxPopulation == 0) return; Phase = DungeonPhase.BossSpawned; ClearAllSpawns(); var bn = Nodes.FirstOrDefault(n => n.Depth == DungeonDepth.BossRoom) ?? Nodes.FirstOrDefault(); if (bn != null) { try { BaseCreature b = (BaseCreature)Activator.CreateInstance(BossType); b.MoveToWorld(bn.Location, Facet); ActiveMonsters[DungeonDepth.BossRoom].Add(b); } catch { } } }

        public void PerformRecovery() { if (Phase == DungeonPhase.Cooldown && DateTime.Now >= CooldownEndTime) { CurrentDifficulty = Math.Min(MaxDifficulty, CurrentDifficulty + (MaxDifficulty / 10)); if (CurrentDifficulty >= MaxDifficulty) Phase = DungeonPhase.Active; } }
        public int GetTotalActiveCount() => ActiveMonsters.Values.Sum(l => l.Count) + ActiveItems.Values.Sum(l => l.Count);
    }

    // ========================================================================
    // 🌍 생태계 매니저 초경량화 (Gump 연동용 UI 캐시 역할만 수행)
    // ========================================================================
    public static class EcosystemManager 
    { 
        public static Dictionary<string, EcoZone> Zones { get; private set; } = new Dictionary<string, EcoZone>(); 

        // 월드 로드 직후 모든 EcoNode를 검색하여 Zones 딕셔너리를 자동 그룹핑합니다.
        public static void RebuildZones()
        {
            Zones.Clear();
            foreach(Item item in World.Items.Values)
            {
                if(item is EcoNode node && !string.IsNullOrEmpty(node.ZoneId) && node.ZoneId != "Unknown")
                {
                    if(!Zones.TryGetValue(node.ZoneId, out var zone))
                    {
                        zone = new EcoZone(node.ZoneId, node.Map);
                        Zones[node.ZoneId] = zone;
                    }
                    zone.Nodes.Add(node);
                }
            }
        }
		/// <summary>
        /// 특정 좌표의 환경 보너스 배율을 튜플로 반환합니다. (극지, 옵시디언, 영목 보너스)
        /// 기본값은 1.0(1배)이며, 환경에 따라 5.0(500%) 등으로 폭발적으로 증가합니다.
        /// </summary>
        public static (double polarBonus, double obsidianBonus, double soulBonus) GetEnvironmentBonus(Point3D loc, Map map)
        {
            // 1. 가장 가까운 EcoNode 찾기 (울티마 온라인 고유의 거리 계산 방식 적용)
            var nearestNode = Zones.Values
                .SelectMany(z => z.Nodes)
                .Where(n => n.Map == map && Math.Max(Math.Abs(n.Location.X - loc.X), Math.Abs(n.Location.Y - loc.Y)) <= n.SpawnRange)
                .OrderBy(n => Math.Max(Math.Abs(n.Location.X - loc.X), Math.Abs(n.Location.Y - loc.Y)))
                .FirstOrDefault();

            if (nearestNode == null) return (1.0, 1.0, 1.0); // 야생이 아니면 기본 배율

            double polar = 1.0;
            double obsidian = 1.0;
            double soul = 1.0;

            // 2. 기후(Climate)에 따른 가중치 폭발적 증가
            if (nearestNode.ClimateType == EcoClimateType.Arctic)
            {
                polar += 5.0; // 북극지방에서 극지 가죽(Polar) 500% 확률 증가
                soul += 1.5;  // 눈 덮인 영목 확률 소폭 증가
            }
            else if (nearestNode.ClimateType == EcoClimateType.Volcanic || nearestNode.ZoneId.ToLower().Contains("hythloth"))
            {
                obsidian += 5.0; // 화산/히스로스 등지에서 옵시디언(Obsidian) 500% 증가
                soul -= 0.5;     // 척박한 땅에서 영목은 거의 안 나옴
            }
            else if (nearestNode.ClimateType == EcoClimateType.Void) // 일쉐나 영성 등 특수 지대
            {
                soul += 5.0; // 에테리얼/영목(Soul Log) 극대화
            }

            return (Math.Max(0.1, polar), Math.Max(0.1, obsidian), Math.Max(0.1, soul));
        }
    }

    // ========================================================================
    // 🌍 던전 매니저 (EcoZones 종속성 완전 분리)
    // ========================================================================
    public static class DungeonManager
    {
        public static Dictionary<string, DungeonZone> Zones { get; private set; } = new Dictionary<string, DungeonZone>();

        public static void Configure() 
        { 
            // 🌟 낡은 Ecology 파일 호출 삭제, 새로운 통합 데이터베이스 초기화!
            EcoSpawnDatabase.Initialize(); 
            
            // 던전 프로필 셋업은 유지
            // TrammelDungeon.Setup(); 

            EventSink.WorldSave += OnSave; 
            EventSink.WorldLoad += OnLoad; 
            EventSink.ServerStarted += OnServerStarted; 
        }

        public static void Initialize() 
        { 
            foreach (var z in Zones.Values) if (z.BossType != null && z.BossThreshold == 0) z.BossThreshold = 1000; 
        }

        private static void OnServerStarted() 
        { 
            foreach (var d in Zones.Values) d.CacheNodes(); 
            EcosystemManager.RebuildZones(); // Gump 띄우기 전 EcoZone 그룹핑
            Timer.DelayCall(TimeSpan.FromSeconds(10.0), TimeSpan.FromMinutes(1.0), OnTick); 
        }

        public static void OnCreatureKilled(BaseCreature bc)
        {
            if (bc == null || bc.Controlled || bc.Summoned) return;
            
            // 🌟 [최적화] 생태계는 EcoNode 자체 타이머가 죽은 몹을 정리하므로
            // 여기서 더이상 OnCreatureKilled 이벤트를 가로채서 연산할 필요가 없습니다! (서버 성능 대폭 상승)
            foreach (var dz in Zones.Values) 
            {
                if (dz.Facet == bc.Map && dz.ActiveMonsters.Values.Any(l => l.Contains(bc))) 
                { 
                    dz.ProcessDeath(bc); 
                    return; 
                }
            }
        }

        private static void OnTick() 
        { 
            // 🌟 [최적화] 생태계 틱(PerformEcoTick) 제거. 이제 EcoNode가 각자 호흡합니다.
            foreach (var d in Zones.Values) 
            { 
                d.CheckRespawn(); 
                d.PerformRecovery(); 
            } 
        }

        public static string CleanString(string input) => new string((input ?? "").Where(char.IsLetterOrDigit).ToArray()).ToLower();

        public static Map ResolveMapByName(string name)
        {
            if (name.Contains("Felucca")) return Map.Felucca;
            if (name.Contains("Trammel")) return Map.Trammel;
            if (name.Contains("Ilshenar")) return Map.Ilshenar;
            if (name.Contains("Malas")) return Map.Malas;
            if (name.Contains("Tokuno")) return Map.Tokuno;
            if (name.Contains("TerMur")) return Map.TerMur;
            return Map.Trammel;
        }

        private static void OnSave(WorldSaveEventArgs e)
        {
            // 🌟 [최적화] EcoZones.bin 생성 및 저장 로직 완전 삭제! (EcoNode가 알아서 저장함)
            string pathD = Path.Combine(Core.BaseDirectory, "Saves", "RespawnSystem", "DungeonZones.bin"); 
            Directory.CreateDirectory(Path.GetDirectoryName(pathD));
            
            using (FileStream fs = new FileStream(pathD, FileMode.Create)) 
            { 
                BinaryFileWriter w = new BinaryFileWriter(fs, true); 
                w.Write(0); 
                w.Write(Zones.Count); 
                foreach (var d in Zones.Values) d.Serialize(w); 
                w.Close(); 
            }
        }

        private static void OnLoad()
        {
            // 🌟 [최적화] EcoZones.bin 로드 로직 완전 삭제!
            string pathD = Path.Combine(Core.BaseDirectory, "Saves", "RespawnSystem", "DungeonZones.bin");
            if (File.Exists(pathD)) 
            { 
                using (FileStream fs = new FileStream(pathD, FileMode.Open)) 
                { 
                    BinaryFileReader r = new BinaryFileReader(new BinaryReader(fs)); 
                    r.ReadInt(); 
                    int c = r.ReadInt(); 
                    for (int i = 0; i < c; i++) 
                    { 
                        DungeonZone ld = new DungeonZone(r); 
                        if (Zones.TryGetValue(ld.ZoneId, out var ex)) 
                        { 
                            ex.CurrentDifficulty = ld.CurrentDifficulty; 
                            ex.Phase = ld.Phase; 
                            ex.CooldownEndTime = ld.CooldownEndTime; 
                            ex.ActiveMonsters = ld.ActiveMonsters; 
                            ex.ActiveItems = ld.ActiveItems; 
                            if (ld.ManualMaxPopulation != -2) ex.SetPopulation(ld.ManualMaxPopulation); 
                        } 
                    } 
                    r.Close(); 
                } 
            }
        }
    }
}