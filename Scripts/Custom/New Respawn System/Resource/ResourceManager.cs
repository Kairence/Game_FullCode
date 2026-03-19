using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;
using Server.Items;

namespace Server.Misc
{
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

        public static void Configure()
        {
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
        }

        public static void Initialize()
        {
            SetupDefinitions();
            RegisterAllPools();
            m_Timer = new ResourceTimer();
            m_Timer.Start();
        }

        private static void SetupDefinitions()
        {
            Defs[ResourceType.Mining] = BuildDefs(new[] { typeof(IronOre), typeof(CopperOre), typeof(BronzeOre), typeof(GoldOre), typeof(AgapiteOre), typeof(VeriteOre), typeof(ValoriteOre) }, LocationType.Mine, 4);
            Defs[ResourceType.Lumberjacking] = BuildDefs(new[] { typeof(Log), typeof(OakLog), typeof(AshLog), typeof(YewLog), typeof(HeartwoodLog), typeof(BloodwoodLog), typeof(FrostwoodLog) }, LocationType.Forest, 4);
            Defs[ResourceType.Fishing] = BuildDefs(new[] { typeof(Trout), typeof(Bass), typeof(Shiner), typeof(CrucianCarp), typeof(CatFish), typeof(CodFish), typeof(PerchFish) }, LocationType.DeepSea, 5);
            Defs[ResourceType.Tanning] = BuildDefs(new[] { typeof(Hides), typeof(DernedHides), typeof(RatnedHides), typeof(SernedHides), typeof(SpinedHides), typeof(HornedHides), typeof(BarbedHides) }, LocationType.Normal, 99);
            
            Defs[ResourceType.Farming] = new List<ResourceDef> {
                new(typeof(Turnip), 0, 50, LocationType.Normal, 500), new(typeof(Cabbage), 20, 70, LocationType.Normal, 200),
                new(typeof(Carrot), 40, 90, LocationType.Normal, 100), new(typeof(Onion), 60, 110, LocationType.Normal, 50),
                new(typeof(WheatSheaf), 80, 130, LocationType.Normal, 25), new(typeof(Cotton), 100, 150, LocationType.Farm_Island, 10),
                new(typeof(Flax), 120, 170, LocationType.Farm_Remote, 5)
            };
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
            ResourceKey key = new(map, region);
            if (!Pools.ContainsKey(key)) Pools[key] = new ResourcePool(map, region, type, loc, max, size);
        }

        private static void RegisterAllPools()
        {
            string[] maps = { "Felucca", "Trammel" };
            foreach (var m in maps) {
                RegisterPool(m, "Minoc Mine", ResourceType.Mining, LocationType.Mine, 2000, 3);
                RegisterPool(m, "Yew", ResourceType.Lumberjacking, LocationType.Forest, 3000, 3);
                RegisterPool(m, "South Britannian Sea", ResourceType.Fishing, LocationType.DeepSea, 5000, 3);
                RegisterPool(m, "A Wheatfield in Britain 1", ResourceType.Farming, LocationType.Normal, 800, 1);
            }
        }

        public static Type TryGather(Mobile from, string mapName, string regionName, ResourceType type, double skill)
        {
            if (!Pools.TryGetValue(new ResourceKey(mapName, regionName), out ResourcePool pool) || pool.Type != type) return null;
            if (!pool.CanGather()) { if (from != null) from.SendMessage("자원이 고갈되었습니다."); return null; }

            var possible = pool.AvailableResources.Where(kvp => kvp.Value > 0 && skill >= GetDef(type, kvp.Key)?.MinSkill).ToList();
            if (possible.Count == 0) { if (from != null) from.SendMessage("얻을 수 있는 자원이 없습니다."); return null; }

            int total = possible.Sum(x => x.Value);
            int roll = Utility.Random(total);
            int cur = 0;
            foreach (var kvp in possible) { cur += kvp.Value; if (roll < cur) { pool.AvailableResources[kvp.Key]--; pool.CurrentCapacity--; return kvp.Key; } }
            return null;
        }

        public static ResourceDef GetDef(ResourceType type, Type itemType) => Defs.GetValueOrDefault(type)?.FirstOrDefault(d => d.ItemType == itemType);

        private static void OnSave(WorldSaveEventArgs e)
        {
            string path = Path.Combine(Core.BaseDirectory, "Saves", "ResourceManagement", "ResourcePools.bin");
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryFileWriter writer = new BinaryFileWriter(stream, true); // CS1674 해결: using 제거
                writer.Write(0); // version
                writer.Write(Pools.Count);
                foreach (var kvp in Pools) { writer.Write(kvp.Key.MapName); writer.Write(kvp.Key.RegionName); kvp.Value.Serialize(writer); }
                writer.Close(); // 직접 닫기
            }
        }

        private static void OnLoad()
        {
            string path = Path.Combine(Core.BaseDirectory, "Saves", "ResourceManagement", "ResourcePools.bin");
            if (!File.Exists(path)) return;

            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                BinaryFileReader reader = new BinaryFileReader(new BinaryReader(stream)); // CS1674 해결
                int version = reader.ReadInt();
                int count = reader.ReadInt();
                for (int i = 0; i < count; i++) {
                    ResourceKey key = new(reader.ReadString(), reader.ReadString());
                    if (Pools.TryGetValue(key, out var pool)) pool.Deserialize(reader);
                    else new ResourcePool(key.MapName, key.RegionName, ResourceType.Mining, LocationType.Normal, 1, 1).Deserialize(reader);
                }
                reader.Close(); // 직접 닫기
            }
        }

        private class ResourceTimer : Timer {
            public ResourceTimer() : base(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0)) { Priority = TimerPriority.OneMinute; }
            protected override void OnTick() { foreach (var p in Pools.Values) p.Regenerate(20); }
        }
    }
}