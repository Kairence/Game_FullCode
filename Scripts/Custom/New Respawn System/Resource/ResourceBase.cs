using System;
using System.Collections.Generic;
using Server;
using Server.Items;

namespace Server.Misc
{
    public enum ResourceType { Mining, Lumberjacking, Fishing, Tanning, Farming }

    public enum LocationType
    {
        Normal,
        Mine,        // 광산
        Forest,      // 숲
        DeepSea,     // 심해
        Farm_Island, // 섬 지역
        Farm_Remote  // 오지 및 특수 지역
    }

    public readonly record struct ResourceKey(string MapName, string RegionName);

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
        public ResourceType Type { get; set; }
        public LocationType LocType { get; set; }
        public int MaxCapacity { get; set; }
        public int CurrentCapacity { get; set; }
        public int SizeCategory { get; set; } // 1: 소형, 2: 중형, 3: 대형
        public Dictionary<Type, int> AvailableResources { get; set; } = new();

        public ResourcePool(string mapName, string regionName, ResourceType type, LocationType locType, int max, int size)
        {
            MapName = mapName; RegionName = regionName; Type = type; LocType = locType;
            MaxCapacity = max; CurrentCapacity = max; SizeCategory = Math.Max(1, size);
            GenerateResources();
        }

        public bool CanGather() => CurrentCapacity >= (MaxCapacity / 2);

        public void GenerateResources()
        {
            AvailableResources.Clear();
            if (!ResourceManager.Defs.ContainsKey(Type)) return;

            List<ResourceDef> validDefs = new();
            int totalWeight = 0;

            foreach (ResourceDef def in ResourceManager.Defs[Type])
            {
                if (def.ReqLoc == LocationType.Normal || def.ReqLoc == LocType)
                {
                    validDefs.Add(def);
                    totalWeight += def.Weight;
                }
            }

            int remaining = MaxCapacity;
            foreach (ResourceDef def in validDefs)
            {
                int count = (int)(MaxCapacity * ((double)def.Weight / totalWeight));
                AvailableResources[def.ItemType] = count;
                remaining -= count;
            }
            if (remaining > 0 && validDefs.Count > 0) AvailableResources[validDefs[0].ItemType] += remaining;
        }

        public void Regenerate(int tickAmount)
        {
            if (CurrentCapacity < MaxCapacity)
            {
                CurrentCapacity += (tickAmount / SizeCategory);
                if (CurrentCapacity > MaxCapacity) { CurrentCapacity = MaxCapacity; GenerateResources(); }
            }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(0); // version
            writer.Write(CurrentCapacity);
            writer.Write(AvailableResources.Count);
            foreach (var kvp in AvailableResources)
            {
                writer.Write(kvp.Key.FullName);
                writer.Write(kvp.Value);
            }
        }

        public void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();
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
}