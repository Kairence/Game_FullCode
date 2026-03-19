using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
    // [1] 열거형 정의
    public enum NpcJobClass { Peasant, Crafter, Warrior, Mage, Noble }
    public enum NpcRank { Novice, Journeyman, Expert, Master }
    public enum ItemCategory { None, Essential, Tool, Luxury }

    public class VirtualDungeon
    {
        public string Name { get; set; }
        public int ThreatLevel { get; set; }
        public long LootChestGold { get; set; }
        public VirtualDungeon(string name, int initialThreat) { Name = name; ThreatLevel = initialThreat; LootChestGold = 0; }
        public VirtualDungeon(GenericReader reader) { int v = reader.ReadInt(); Name = reader.ReadString(); ThreatLevel = reader.ReadInt(); LootChestGold = reader.ReadLong(); }
        public void Serialize(GenericWriter writer) { writer.Write(0); writer.Write(Name); writer.Write(ThreatLevel); writer.Write(LootChestGold); }
    }

    // [2] 가상 에이전트 베이스
    public abstract class VirtualAgent
    {
        public NpcJobClass JobClass { get; set; }
        public NpcRank Rank { get; set; }
        public int Gold { get; set; }

        public VirtualAgent(NpcJobClass job, NpcRank rank) { JobClass = job; Rank = rank; Gold = CalculateStartingGold(job, rank); }
        public VirtualAgent(GenericReader reader) { int v = reader.ReadInt(); JobClass = (NpcJobClass)reader.ReadInt(); Rank = (NpcRank)reader.ReadInt(); Gold = reader.ReadInt(); }
        public virtual void Serialize(GenericWriter writer) { writer.Write(0); writer.Write((int)JobClass); writer.Write((int)Rank); writer.Write(Gold); }

        public static int CalculateStartingGold(NpcJobClass job, NpcRank rank)
        {
            int baseGold = job switch { NpcJobClass.Peasant => 100, NpcJobClass.Crafter => 300, NpcJobClass.Warrior => 500, NpcJobClass.Mage => 800, NpcJobClass.Noble => 2000, _ => 100 };
            int rankMultiplier = rank switch { NpcRank.Novice => 1, NpcRank.Journeyman => 2, NpcRank.Expert => 5, NpcRank.Master => 10, _ => 1 };
            int total = baseGold * rankMultiplier; return total + Utility.RandomMinMax(-(int)(total * 0.2), (int)(total * 0.2));
        }

        public ItemCategory ClassifyItem(Item item)
        {
            if (item is Food || item is BaseBeverage || item is Backpack || item is Pouch || item is BaseClothing) return ItemCategory.Essential;
            if (item is BaseJewel || item is GoldRing || item is SilverRing) return ItemCategory.Luxury;
            switch (JobClass) {
                case NpcJobClass.Warrior: if (item is BaseWeapon || item is BaseArmor || item is BaseShield || item is Bandage) return ItemCategory.Tool; break;
                case NpcJobClass.Mage: if (item is BaseReagent || item is BaseWand || item is Spellbook || item is BasePotion) return ItemCategory.Tool; break;
                case NpcJobClass.Crafter: if (item is BaseTool || item is BaseIngot || item is Board || item is Log || item is Cloth) return ItemCategory.Tool; break;
            }
            return ItemCategory.None;
        }

        public bool WantsToBuy(Item item, int price, double priceMultiplier, double resistance)
        {
            if (priceMultiplier > resistance || price > Gold) return false;
            ItemCategory category = ClassifyItem(item);
            return category switch {
                ItemCategory.Essential => true,
                ItemCategory.Tool => Gold > CalculateStartingGold(JobClass, Rank) * 0.1,
                ItemCategory.Luxury => JobClass == NpcJobClass.Noble || Gold > CalculateStartingGold(JobClass, Rank) * 1.5, _ => false
            };
        }
    }

    public class VirtualCitizen : VirtualAgent
    {
        public int Satisfaction { get; set; }
        public VirtualCitizen(NpcJobClass job, NpcRank rank, int satisfaction) : base(job, rank) { Satisfaction = Math.Clamp(satisfaction, 0, 100); }
        public VirtualCitizen(GenericReader reader) : base(reader) { int v = reader.ReadInt(); Satisfaction = reader.ReadInt(); }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); writer.Write(Satisfaction); }
    }

    public class VirtualAdventurer : VirtualAgent
    {
        public double PriceResistance { get; set; }
        public VirtualAdventurer(NpcJobClass job, NpcRank rank, double resistance) : base(job, rank) { PriceResistance = resistance; }
        public VirtualAdventurer(GenericReader reader) : base(reader) { int v = reader.ReadInt(); PriceResistance = reader.ReadDouble(); }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); writer.Write(PriceResistance); }
    }

    public class VirtualGatherer : VirtualAgent
    {
        public string TargetRegion { get; set; }
        public ResourceType TargetResource { get; set; }
        public VirtualGatherer(NpcJobClass job, NpcRank rank, string targetRegion, ResourceType targetResource) : base(job, rank) { TargetRegion = targetRegion; TargetResource = targetResource; }
        public VirtualGatherer(GenericReader reader) : base(reader) { int v = reader.ReadInt(); TargetRegion = reader.ReadString(); TargetResource = (ResourceType)reader.ReadInt(); }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); writer.Write(TargetRegion); writer.Write((int)TargetResource); }
    }

    // [3] 도시 경제 구역
    public class TownEconomy
    {
        public string TownName { get; set; }
        public Point3D Center { get; set; }
        public Map Facet { get; set; }
        public long Wealth { get; set; }
        public long TaxFund { get; set; }
        public long BaseWealth { get; set; }
        public VirtualDungeon NearbyDungeon { get; set; }
        public List<VirtualCitizen> Citizens { get; set; } = new();
        public List<VirtualAdventurer> Adventurers { get; set; } = new();
        public List<VirtualGatherer> Gatherers { get; set; } = new();

        public double PriceMultiplier => Math.Clamp((double)Wealth / BaseWealth, 0.5, 1.5);
        public double SafetyMultiplier => NearbyDungeon != null ? Math.Clamp(1.0 - (NearbyDungeon.ThreatLevel / 10000.0), 0.1, 1.0) : 1.0;
        public int Prosperity => Citizens.Count == 0 ? 0 : (int)Citizens.Average(c => c.Satisfaction);

        public TownEconomy(string name, Point3D center, Map map, long baseWealth, string dungeonName)
        {
            TownName = name; Center = center; Facet = map; BaseWealth = baseWealth; Wealth = baseWealth; TaxFund = 0;
            if (!string.IsNullOrEmpty(dungeonName)) NearbyDungeon = new VirtualDungeon(dungeonName, 1000);
            for (int i = 0; i < 150; i++) Citizens.Add(new VirtualCitizen(Utility.RandomBool() ? NpcJobClass.Peasant : NpcJobClass.Crafter, (NpcRank)Utility.Random(3), 80));
            for (int i = 0; i < 30; i++) Adventurers.Add(new VirtualAdventurer(Utility.RandomBool() ? NpcJobClass.Warrior : NpcJobClass.Mage, (NpcRank)Utility.Random(4), 1.2));
            ResourceType resType = (name == "Minoc" || name == "Cove") ? ResourceType.Mining : ResourceType.Lumberjacking;
            for (int i = 0; i < 20; i++) Gatherers.Add(new VirtualGatherer(NpcJobClass.Peasant, (NpcRank)Utility.Random(3), name == "Minoc" ? "Minoc Mine" : "Yew Forest", resType));
        }

        public TownEconomy(GenericReader reader)
        {
            int v = reader.ReadInt(); TownName = reader.ReadString(); Center = reader.ReadPoint3D(); Facet = reader.ReadMap();
            Wealth = reader.ReadLong(); TaxFund = reader.ReadLong(); BaseWealth = reader.ReadLong();
            if (reader.ReadBool()) NearbyDungeon = new VirtualDungeon(reader);
            int citCount = reader.ReadInt(); for (int i = 0; i < citCount; i++) Citizens.Add(new VirtualCitizen(reader));
            int advCount = reader.ReadInt(); for (int i = 0; i < advCount; i++) Adventurers.Add(new VirtualAdventurer(reader));
            int gatCount = reader.ReadInt(); for (int i = 0; i < gatCount; i++) Gatherers.Add(new VirtualGatherer(reader));
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(0); writer.Write(TownName); writer.Write(Center); writer.Write(Facet);
            writer.Write(Wealth); writer.Write(TaxFund); writer.Write(BaseWealth);
            writer.Write(NearbyDungeon != null); if (NearbyDungeon != null) NearbyDungeon.Serialize(writer);
            writer.Write(Citizens.Count); foreach (var c in Citizens) c.Serialize(writer);
            writer.Write(Adventurers.Count); foreach (var a in Adventurers) a.Serialize(writer);
            writer.Write(Gatherers.Count); foreach (var g in Gatherers) g.Serialize(writer);
        }
    }

    // [4] 경제 시스템 메인 매니저
    public static class EconomyManager
    {
        public static Dictionary<string, TownEconomy> Towns { get; private set; } = new();
        public static bool IsEnabled { get; private set; } = false;
        private static Timer m_Timer;

        public static void Configure() { EventSink.WorldSave += OnSave; EventSink.WorldLoad += OnLoad; }

        public static void Initialize()
        {
            if (Towns.Count == 0)
            {
                Towns["Minoc"] = new TownEconomy("Minoc", new Point3D(2466, 544, 0), Map.Trammel, 4000000, "Covetous");
                Towns["Yew"] = new TownEconomy("Yew", new Point3D(546, 992, 0), Map.Trammel, 2500000, "Wrong");
                Towns["Britain"] = new TownEconomy("Britain", new Point3D(1495, 1629, 10), Map.Trammel, 15000000, "Despise");
            }
            m_Timer = Timer.DelayCall(TimeSpan.FromHours(1.0), TimeSpan.FromHours(1.0), SimulateAllTowns);
        }

        public static void Start(bool enable) { IsEnabled = enable; Console.WriteLine($"Economy System: {(IsEnabled ? "ENABLED" : "DISABLED")}"); }

        public static void SimulateAllTowns()
        {
            if (!IsEnabled) return;
            foreach (TownEconomy town in Towns.Values)
            {
                double priceM = town.PriceMultiplier;
                foreach (var gat in town.Gatherers) { int supply = (int)(30 * priceM); if (gat.Gold >= supply) { gat.Gold -= supply; town.Wealth += supply; } }
                foreach (var cit in town.Citizens) { cit.Gold += Utility.RandomMinMax(15, 35); int cost = (int)(20 * priceM); if (cit.Gold >= cost) { cit.Gold -= cost; cit.Satisfaction += 2; } else cit.Satisfaction -= 5; cit.Satisfaction = Math.Clamp(cit.Satisfaction, 0, 100); }
            }
        }

        private static void OnSave(WorldSaveEventArgs e)
        {
            string path = Path.Combine(Core.BaseDirectory, "Saves", "EconomySystem", "TownEconomy.bin");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryFileWriter writer = new BinaryFileWriter(stream, true);
                writer.Write(0); writer.Write(Towns.Count);
                foreach (var eco in Towns.Values) eco.Serialize(writer);
                writer.Close(); // CS1674 해결
            }
        }

        private static void OnLoad()
        {
            string path = Path.Combine(Core.BaseDirectory, "Saves", "EconomySystem", "TownEconomy.bin");
            if (!File.Exists(path)) return;
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                BinaryFileReader reader = new BinaryFileReader(new BinaryReader(stream));
                int v = reader.ReadInt(); int count = reader.ReadInt();
                for (int i = 0; i < count; i++) { var eco = new TownEconomy(reader); Towns[eco.TownName] = eco; }
                reader.Close(); // CS1674 해결
            }
        }
    }
}