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

    public class SpeciesState
    {
        public Type AnimalType { get; set; }
        public int MaxPopulation { get; set; }
        public int Vitality { get; set; }
        public List<BaseCreature> ActiveAnimals { get; set; } = new List<BaseCreature>();

        public SpeciesState(Type type, int maxPop, int vitality) { AnimalType = type; MaxPopulation = maxPop; Vitality = vitality; }

        public SpeciesState(GenericReader reader)
        {
            int version = reader.ReadInt();
            string typeName = reader.ReadString();
            if (!string.IsNullOrEmpty(typeName)) AnimalType = ScriptCompiler.FindTypeByFullName(typeName);
            MaxPopulation = reader.ReadInt(); Vitality = reader.ReadInt();
            int count = reader.ReadInt();
            for (int i = 0; i < count; i++) { BaseCreature m = reader.ReadMobile() as BaseCreature; if (m != null && !m.Deleted) ActiveAnimals.Add(m); }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(1); writer.Write(AnimalType?.FullName ?? string.Empty);
            writer.Write(MaxPopulation); writer.Write(Vitality);
            writer.Write(ActiveAnimals.Count); foreach (var m in ActiveAnimals) writer.Write(m);
        }
    }

    public class EcoZone
    {
        public string ZoneId { get; set; }
        public Map Facet { get; set; }
        public Dictionary<Type, SpeciesState> SpeciesInfo { get; set; } = new Dictionary<Type, SpeciesState>();
        public List<DungeonNode> Nodes { get; set; } = new List<DungeonNode>();
        public EcoZone(string zoneId, Map map) { ZoneId = zoneId; Facet = map; }
        public void AddSpecies(Type type, int maxPop) { if (type != null) SpeciesInfo[type] = new SpeciesState(type, maxPop, 10000); }

        public void CacheNodes()
        {
            if (Facet == null || Facet == Map.Internal) Facet = DungeonManager.ResolveMapByName(ZoneId);
            Nodes.Clear();
            string myClean = DungeonManager.CleanString(ZoneId);
            foreach (Item item in World.Items.Values)
            {
                if (item is DungeonNode node && (node.Map == Facet || Facet == null))
                {
                    string nodeClean = DungeonManager.CleanString(node.ZoneId);
                    if (nodeClean.Contains(myClean) || myClean.Contains(nodeClean)) Nodes.Add(node);
                }
            }
        }

        public void ProcessDeath(BaseCreature bc) { if (bc != null && SpeciesInfo.TryGetValue(bc.GetType(), out SpeciesState state)) { state.ActiveAnimals.Remove(bc); state.Vitality = Math.Max(0, state.Vitality - (10000 / Math.Max(1, state.MaxPopulation))); } }

        public void PerformEcoTick()
        {
            if (Nodes.Count == 0) CacheNodes(); if (Nodes.Count == 0) return;
            foreach (SpeciesState state in SpeciesInfo.Values)
            {
                state.ActiveAnimals.RemoveAll(a => a == null || a.Deleted || !a.Alive || a.Controlled);
                if (state.Vitality < 10000) state.Vitality = Math.Min(10000, state.Vitality + 100);
                if (state.Vitality < 2000 || state.ActiveAnimals.Count >= state.MaxPopulation) continue;
                double spawnChance = (state.ActiveAnimals.Count == 0) ? 0.5 : (0.02 + (0.98 * ((double)state.ActiveAnimals.Count / state.MaxPopulation))) * (state.Vitality / 10000.0);
                if (Utility.RandomDouble() <= spawnChance)
                {
                    DungeonNode node = Nodes[Utility.Random(Nodes.Count)]; Point3D? loc = node.GetValidSpawnLocation();
                    if (loc.HasValue && state.AnimalType != null) { try { BaseCreature animal = (BaseCreature)Activator.CreateInstance(state.AnimalType); animal.Home = loc.Value; animal.RangeHome = node.HomeRange; animal.MoveToWorld(loc.Value, Facet); state.ActiveAnimals.Add(animal); } catch { } }
                }
            }
        }
    }

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
                // [조정] 면적 대비 인구 밀도 추가 하향 (150 -> 600)
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

                // [조정] 한 번에 리스폰되는 최대 개수를 5마리로 제한
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

        public void ClearAllSpawns() { foreach (var list in ActiveMonsters.Values) { foreach (var m in list) m.Delete(); list.Clear(); } foreach (var list in ActiveItems.Values) { foreach (var i in list) i.Delete(); list.Clear(); } }

        private void SpawnBoss() { if (BossType == null || MaxPopulation == 0) return; Phase = DungeonPhase.BossSpawned; ClearAllSpawns(); var bn = Nodes.FirstOrDefault(n => n.Depth == DungeonDepth.BossRoom) ?? Nodes.FirstOrDefault(); if (bn != null) { try { BaseCreature b = (BaseCreature)Activator.CreateInstance(BossType); b.MoveToWorld(bn.Location, Facet); ActiveMonsters[DungeonDepth.BossRoom].Add(b); } catch { } } }

        public void PerformRecovery() { if (Phase == DungeonPhase.Cooldown && DateTime.Now >= CooldownEndTime) { CurrentDifficulty = Math.Min(MaxDifficulty, CurrentDifficulty + (MaxDifficulty / 10)); if (CurrentDifficulty >= MaxDifficulty) Phase = DungeonPhase.Active; } }
        public int GetTotalActiveCount() => ActiveMonsters.Values.Sum(l => l.Count) + ActiveItems.Values.Sum(l => l.Count);
    }

    public static class DungeonManager
    {
        public static Dictionary<string, DungeonZone> Zones { get; private set; } = new Dictionary<string, DungeonZone>();
        public static Dictionary<string, EcoZone> EcoZones { get; private set; } = new Dictionary<string, EcoZone>();

        public static void Configure() { TrammelEcology.Setup(); TrammelDungeon.Setup(); EventSink.WorldSave += OnSave; EventSink.WorldLoad += OnLoad; EventSink.ServerStarted += OnServerStarted; }

        public static void Initialize() { foreach (var z in Zones.Values) if (z.BossType != null && z.BossThreshold == 0) z.BossThreshold = 1000; }

        private static void OnServerStarted() { foreach (var d in Zones.Values) d.CacheNodes(); foreach (var e in EcoZones.Values) e.CacheNodes(); Timer.DelayCall(TimeSpan.FromSeconds(10.0), TimeSpan.FromMinutes(1.0), OnTick); }

        public static void OnCreatureKilled(BaseCreature bc)
        {
            if (bc == null || bc.Controlled || bc.Summoned) return;
            foreach (var dz in Zones.Values) if (dz.Facet == bc.Map && dz.ActiveMonsters.Values.Any(l => l.Contains(bc))) { dz.ProcessDeath(bc); return; }
            foreach (var ez in EcoZones.Values) if (ez.Facet == bc.Map && ez.SpeciesInfo.ContainsKey(bc.GetType()) && ez.SpeciesInfo[bc.GetType()].ActiveAnimals.Contains(bc)) { ez.ProcessDeath(bc); return; }
        }

        private static void OnTick() { foreach (var d in Zones.Values) { d.CheckRespawn(); d.PerformRecovery(); } foreach (var e in EcoZones.Values) e.PerformEcoTick(); }

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
            string pathE = Path.Combine(Core.BaseDirectory, "Saves", "RespawnSystem", "EcoZones.bin"); string pathD = Path.Combine(Core.BaseDirectory, "Saves", "RespawnSystem", "DungeonZones.bin"); Directory.CreateDirectory(Path.GetDirectoryName(pathE));
            using (FileStream fs = new FileStream(pathE, FileMode.Create)) { BinaryFileWriter w = new BinaryFileWriter(fs, true); w.Write(0); w.Write(EcoZones.Count); foreach (var z in EcoZones.Values) { w.Write(z.ZoneId); w.Write(z.SpeciesInfo.Count); foreach (var sp in z.SpeciesInfo.Values) sp.Serialize(w); } w.Close(); }
            using (FileStream fs = new FileStream(pathD, FileMode.Create)) { BinaryFileWriter w = new BinaryFileWriter(fs, true); w.Write(0); w.Write(Zones.Count); foreach (var d in Zones.Values) d.Serialize(w); w.Close(); }
        }

        private static void OnLoad()
        {
            string pathE = Path.Combine(Core.BaseDirectory, "Saves", "RespawnSystem", "EcoZones.bin"); string pathD = Path.Combine(Core.BaseDirectory, "Saves", "RespawnSystem", "DungeonZones.bin");
            if (File.Exists(pathE)) { using (FileStream fs = new FileStream(pathE, FileMode.Open)) { BinaryFileReader r = new BinaryFileReader(new BinaryReader(fs)); r.ReadInt(); int c = r.ReadInt(); for (int i = 0; i < c; i++) { string zid = r.ReadString(); int sc = r.ReadInt(); if (EcoZones.TryGetValue(zid, out var z)) for (int j = 0; j < sc; j++) { var st = new SpeciesState(r); if (st.AnimalType != null && z.SpeciesInfo.TryGetValue(st.AnimalType, out var ex)) { ex.Vitality = st.Vitality; ex.ActiveAnimals = st.ActiveAnimals; } } else for (int j = 0; j < sc; j++) new SpeciesState(r); } r.Close(); } }
            if (File.Exists(pathD)) { using (FileStream fs = new FileStream(pathD, FileMode.Open)) { BinaryFileReader r = new BinaryFileReader(new BinaryReader(fs)); r.ReadInt(); int c = r.ReadInt(); for (int i = 0; i < c; i++) { DungeonZone ld = new DungeonZone(r); if (Zones.TryGetValue(ld.ZoneId, out var ex)) { ex.CurrentDifficulty = ld.CurrentDifficulty; ex.Phase = ld.Phase; ex.CooldownEndTime = ld.CooldownEndTime; ex.ActiveMonsters = ld.ActiveMonsters; ex.ActiveItems = ld.ActiveItems; if (ld.ManualMaxPopulation != -2) ex.SetPopulation(ld.ManualMaxPopulation); } } r.Close(); } }
        }
    }

    public static class EcosystemManager { public static Dictionary<string, EcoZone> Zones => DungeonManager.EcoZones; }
    public static class DungeonEcology { public static void SetupAllEcologies() { FeluccaEcology.Setup(); TrammelEcology.Setup(); IlshenarEcology.Setup(); MalasEcology.Setup(); TokunoEcology.Setup(); TerMurEcology.Setup(); } }
}