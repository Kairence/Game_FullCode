using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Misc
{
    // ==============================================================================
    // [Í∏∞Ï? Enum Î∞è Íµ¨Ï°∞Ï≤?- Î™®ÌóòÍ∞Ä ?Ñ??
    // ==============================================================================
    public enum LawChaos { Lawful, Neutral, Chaotic }
    public enum GoodEvil { Good, Neutral, Evil }
    public enum AdventurerState { Resting, Traveling, Exploring }
    public enum WorldNodeType { Town, Dungeon, Ruin }
    public enum AdventurerRole { Tank, MeleeDPS, RangedDPS, MagicDPS, Healer, Support }
    public enum LootDistributionRule { Equal, Contribution }
    
    public enum AdventurerAiType { Melee, Archer, Mage, Paladin, Necro }

    public record CombatProfile(AdventurerRole Role, double HpWeight, double MpWeight, double SpWeight, int[] PreferredOptions, params Layer[] RequiredLayers);

    public record RetreatRoute(RegionCode TownCode, bool IsIsland, int BaseDistance);

    public static class DungeonRetreatManager
    {
        public static readonly Dictionary<RegionCode, RetreatRoute> Map = new()
        {
            // --- Trammel Dungeons ---
            { RegionCode.Trammel_Dungeon_Covetous, new(RegionCode.Trammel_Town_Minoc, false, 800) },       
            { RegionCode.Trammel_Dungeon_Deceit, new(RegionCode.Trammel_Town_Moonglow, true, 1200) },      
            { RegionCode.Trammel_Dungeon_Despise, new(RegionCode.Trammel_Town_Britain, false, 600) },      
            { RegionCode.Trammel_Dungeon_Destard, new(RegionCode.Trammel_Town_Trinsic, false, 900) },    
            { RegionCode.Trammel_Dungeon_Hythloth, new(RegionCode.Trammel_Town_Haven, true, 1500) },    
            { RegionCode.Trammel_Dungeon_Shame, new(RegionCode.Trammel_Town_SkaraBrae, false, 1100) },           
            { RegionCode.Trammel_Dungeon_Wrong, new(RegionCode.Trammel_Town_Yew, false, 1000) },         
            { RegionCode.Trammel_Dungeon_Fire, new(RegionCode.Trammel_Town_SerpentsHold, true, 500) },     
            { RegionCode.Trammel_Dungeon_Ice, new(RegionCode.Trammel_Town_Vesper, false, 1300) },          
            { RegionCode.Trammel_Dungeon_OrcCave, new(RegionCode.Trammel_Town_Cove, false, 400) },         
            { RegionCode.Trammel_Dungeon_PaintedCaves, new(RegionCode.Trammel_Town_Trinsic, false, 700) },
            { RegionCode.Trammel_Dungeon_PalaceOfParoxysmus, new(RegionCode.Trammel_Town_Jhelom, true, 1200) },
            { RegionCode.Trammel_Dungeon_PrismOfLight, new(RegionCode.Trammel_Town_Nujelm, true, 1000) },
            { RegionCode.Trammel_Dungeon_Sanctuary, new(RegionCode.Trammel_Town_Yew, false, 600) },
            { RegionCode.Trammel_Dungeon_SolenHives, new(RegionCode.Trammel_Town_Wind, false, 700) },
            
            // --- Felucca Dungeons ---
            { RegionCode.Felucca_Dungeon_Covetous, new(RegionCode.Felucca_Town_Minoc, false, 800) },
            { RegionCode.Felucca_Dungeon_Deceit, new(RegionCode.Felucca_Town_Moonglow, true, 1200) },
            { RegionCode.Felucca_Dungeon_Despise, new(RegionCode.Felucca_Town_Britain, false, 600) },
            { RegionCode.Felucca_Dungeon_Destard, new(RegionCode.Felucca_Town_Trinsic, false, 900) },
            { RegionCode.Felucca_Dungeon_Hythloth, new(RegionCode.Felucca_Town_Ocllo, true, 1500) },
            { RegionCode.Felucca_Dungeon_Shame, new(RegionCode.Felucca_Town_SkaraBrae, false, 1100) },
            { RegionCode.Felucca_Dungeon_Wrong, new(RegionCode.Felucca_Town_Yew, false, 1000) },
            { RegionCode.Felucca_Dungeon_Fire, new(RegionCode.Felucca_Town_SerpentsHold, true, 500) },
            { RegionCode.Felucca_Dungeon_Ice, new(RegionCode.Felucca_Town_Vesper, false, 1300) },
            { RegionCode.Felucca_Dungeon_OrcCave, new(RegionCode.Felucca_Town_Yew, false, 400) },
            { RegionCode.Felucca_Dungeon_Khaldun, new(RegionCode.Felucca_Town_Vesper, false, 1500) }
        };
    }

    public static class AdventurerProfileManager
    {
        private static readonly Layer[] MeleeLayers = [Layer.Helm, Layer.Neck, Layer.InnerTorso, Layer.Arms, Layer.Gloves, Layer.Pants, Layer.Shoes, Layer.Ring, Layer.Bracelet, Layer.OneHanded, Layer.TwoHanded];
        private static readonly Layer[] RangedLayers = [Layer.Helm, Layer.Neck, Layer.InnerTorso, Layer.Arms, Layer.Gloves, Layer.Pants, Layer.Shoes, Layer.Ring, Layer.Bracelet, Layer.TwoHanded]; 
        private static readonly Layer[] MageLayers = [Layer.Helm, Layer.Neck, Layer.InnerTorso, Layer.OuterTorso, Layer.Gloves, Layer.Pants, Layer.Shoes, Layer.Ring, Layer.Bracelet, Layer.OneHanded, Layer.TwoHanded];

        public static CombatProfile GetProfile(NpcJobClass job)
        {
            return job switch
            {
                NpcJobClass.Knight or NpcJobClass.Paladin 
                    => new CombatProfile(AdventurerRole.Tank, 1.5, 0.2, 1.0, [CustomOption.Hits, CustomOption.DefChance, CustomOption.Str, CustomOption.AllRes], MeleeLayers),
                
                NpcJobClass.Halberdier or NpcJobClass.Assassin 
                    => new CombatProfile(AdventurerRole.MeleeDPS, 1.0, 0.0, 1.5, [CustomOption.WeaponDamage, CustomOption.HitChance, CustomOption.SwingSpeed, CustomOption.Str], MeleeLayers),
                
                NpcJobClass.Archer_Expert or NpcJobClass.Crossbowman
                    => new CombatProfile(AdventurerRole.RangedDPS, 0.8, 0.0, 1.5, [CustomOption.WeaponDamage, CustomOption.HitChance, CustomOption.SwingSpeed, CustomOption.Dex], RangedLayers),
                
                NpcJobClass.Healer_Master or NpcJobClass.Priest 
                    => new CombatProfile(AdventurerRole.Healer, 0.8, 1.5, 0.2, [CustomOption.Mana, CustomOption.LowerManaCost, CustomOption.SpellSpeed, CustomOption.Int], MageLayers),
                
                NpcJobClass.Wizard or NpcJobClass.Necromancer 
                    => new CombatProfile(AdventurerRole.MagicDPS, 0.5, 2.0, 0.1, [CustomOption.SpellDamage, CustomOption.SpellSpeed, CustomOption.LowerManaCost, CustomOption.Int], MageLayers),
                
                NpcJobClass.Bard or NpcJobClass.Lutanist 
                    => new CombatProfile(AdventurerRole.Support, 0.8, 1.0, 0.5, [CustomOption.Hits, CustomOption.Mana, CustomOption.AllSpeed], MeleeLayers),
                
                _ => new CombatProfile(AdventurerRole.MeleeDPS, 1.0, 0.0, 1.0, [CustomOption.WeaponDamage, CustomOption.Str], MeleeLayers)
            };
        }
    }

    // ==============================================================================
    // [Í∞Ä?Å Î™®ÌóòÍ∞Ä Îß§Îãà?Ä]
    // ==============================================================================
    public static class VirtualAdventurerManager
    {
        public static List<VirtualAdventurer> IdleAdventurers { get; set; } = [];
        public static List<AdventurerParty> ActiveParties { get; set; } = [];
        
        public static DateTime LastTickTime { get; set; }

        private static string SavePath => Path.Combine(Core.BaseDirectory, "Saves", "EconomySystem", "VirtualAdventurers.bin");

        public static void Configure()
        {
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
        }

        private static int m_NextPartyTeamID = 10001;

        public static int GetNextTeamID()
        {
            int id = m_NextPartyTeamID++;
            if (m_NextPartyTeamID > 999999) m_NextPartyTeamID = 10001;
            return id;
        }

        public static void PayToCitizenOrTown(TownEconomy town, int amount, params NpcJobClass[] targetJobs)
        {
            if (town == null) return;
            if (targetJobs != null && targetJobs.Length > 0 && town.Citizens != null)
            {
                var candidates = town.Citizens.Where(c => targetJobs.Contains(c.JobClass) && !c.IsExpired).ToList();
                if (candidates.Count > 0)
                {
                    var receiver = candidates[Utility.Random(candidates.Count)];
                    receiver.Gold += amount;
                    
                    int tax = (int)(amount * 0.1);
                    receiver.Gold -= tax;
                    town.Wealth += tax;
                    return;
                }
            }
            town.Wealth += amount; 
        }

        public static void ProcessAdventurerSegment(int advTickIdx)
        {
            if (advTickIdx == 1)
            {
                LastTickTime = DateTime.Now;
                CheckAndSpawnReinforcements();

                for (int i = ActiveParties.Count - 1; i >= 0; i--)
                {
                    if (ActiveParties[i].Members.Count == 0)
                    {
                        ActiveParties.RemoveAt(i);
                        Console.WriteLine("[Adventurer] ?å?∞Í? ?ÑÎ©∏Ìïò???¥ÏÇ∞Îêò?à?µÎãà??");
                    }
                }

                if (IdleAdventurers.Count >= 3 && ActiveParties.Count < 100) 
                {
                    var towns = TownEconomyManager.Towns.Values.ToList();
                    if (towns.Count > 0)
                    {
                        var startTown = towns[Utility.Random(towns.Count)]; 
                        var rCode = RegionSaver.GetRegionCodes(startTown.Facet, startTown.Center.X, startTown.Center.Y, startTown.Center.Z).Major;
                        WorldNode townNode = new WorldNode(startTown.Name, rCode, WorldNodeType.Town, startTown.Facet, startTown.Center, startTown.Center, 1);
                        
                        var newParty = AdventurerParty.TryFormBalancedParty(IdleAdventurers, townNode);
                        if (newParty != null) ActiveParties.Add(newParty);
                    }
                }
            }

            if (ActiveParties.Count == 0) return;

            int startIdx = (ActiveParties.Count * (advTickIdx - 1)) / 10;
            int endIdx = (ActiveParties.Count * advTickIdx) / 10;

            for (int i = startIdx; i < endIdx; i++)
            {
                ActiveParties[i].OnTick();
            }
        }

        private static void CheckAndSpawnReinforcements()
        {
            int totalAdventurers = IdleAdventurers.Count + ActiveParties.Sum(p => p.Members.Count);
            int totalCitizens = TownEconomyManager.Towns.Values.Sum(t => t.Citizens != null ? t.Citizens.Count : 0);
            int maxAdventurers = Math.Max(20, (int)(totalCitizens * 0.1)); 
            
            if (totalAdventurers >= maxAdventurers) return;

            int totalHeat = DungeonManager.ZoneList.Sum(z => z.CurrentDifficulty);
            int targetAdventurers = Math.Min(maxAdventurers, Math.Max(20, totalHeat / 2000));
            
            if (totalAdventurers < targetAdventurers)
            {
                if (Utility.RandomDouble() > 0.05) return; 

                var capableTowns = TownEconomyManager.Towns.Values
                    .Where(t => t.Wealth >= 15000 && t.Citizens != null && (t.Citizens.Count * 0.1) >= 1)
                    .ToList();

                if (capableTowns.Count > 0)
                {
                    var town = capableTowns.OrderByDescending(t => t.Wealth).First();
                    int townAdvCap = (int)(town.Citizens.Count * 0.1);
                    int advInTown = ActiveParties.Where(p => p.State == AdventurerState.Resting && p.CurrentNode != null && p.CurrentNode.Name == town.Name).Sum(p => p.Members.Count);
                    
                    if (advInTown >= townAdvCap) return; 

                    int spawnAmount = Math.Min(5, townAdvCap - advInTown);
                    int spawnCost = spawnAmount * 1000; 
                    
                    if (town.Wealth >= spawnCost && spawnAmount > 0)
                    {
                        town.Wealth -= spawnCost; 
                        SpawnInitialAdventurers(town, spawnAmount); 
                        Console.WriteLine($"[Adventurer] {town.Name} Îßà?Ñ??{spawnCost}gpÎ°ú ?©Î? {spawnAmount}Î™Ö?Ñ Í≥†?©Ìñà?µÎãà?? (Îßà?Ñ TO: {advInTown + spawnAmount}/{townAdvCap})");
                    }
                }
            }
        }

        private static void OnSave(WorldSaveEventArgs e)
        {
            if (!Directory.Exists(Path.GetDirectoryName(SavePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath));

            using (FileStream bin = new FileStream(SavePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                GenericWriter writer = new BinaryFileWriter(bin, true);
                writer.Write(0); 
                
                writer.Write(IdleAdventurers.Count);
                foreach (var adv in IdleAdventurers) adv.Serialize(writer);

                writer.Write(ActiveParties.Count);
                foreach (var party in ActiveParties) party.Serialize(writer);
                
                writer.Close(); 
            }
        }

        private static void OnLoad()
        {
            if (File.Exists(SavePath))
            {
                try
                {
                    using FileStream bin = new FileStream(SavePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    GenericReader reader = new BinaryFileReader(new BinaryReader(bin));
                    int version = reader.ReadInt();

                    int idleCount = reader.ReadInt();
                    for (int i = 0; i < idleCount; i++) IdleAdventurers.Add(new VirtualAdventurer(reader));

                    int partyCount = reader.ReadInt();
                    for (int i = 0; i < partyCount; i++) ActiveParties.Add(new AdventurerParty(reader));
                }
                catch
                {
                    Console.WriteLine("======================================================");
                    Console.WriteLine(" [Ï£ºÏùò] Î™®ÌóòÍ∞Ä ?∏Ïù¥Î? ?å???ê?Å Í∞êÏßÄ! Íº¨Ïù??∞Ïù¥ÌÑ∞Î? ?åÍ∏∞ÌïòÍ≥† Ï¥àÍ∏∞Ìôî?©Îãà??");
                    Console.WriteLine("======================================================");
                    IdleAdventurers.Clear();
                    ActiveParties.Clear();
                }
            }

            if (IdleAdventurers.Count == 0 && ActiveParties.Count == 0)
            {
                foreach (var town in TownEconomyManager.Towns.Values)
                {
                    SpawnInitialAdventurers(town, 15);
                }
            }

            Console.WriteLine("[Adventurer] Í∞Ä?Å Î™®ÌóòÍ∞Ä ?ú?§ÌÖú ?∞Ïù¥ÌÑ?Î°ú?ú ?ÑÎ£å.");
        }

        public static void SpawnInitialAdventurers(TownEconomy town, int amount)
        {
            NpcJobClass[] advJobs = [ NpcJobClass.Knight, NpcJobClass.Paladin, NpcJobClass.Halberdier, NpcJobClass.Assassin, NpcJobClass.Healer_Master, NpcJobClass.Priest, NpcJobClass.Wizard, NpcJobClass.Necromancer, NpcJobClass.Bard, NpcJobClass.Lutanist ];

            for (int i = 0; i < amount; i++)
            {
                var job = advJobs[Utility.Random(advJobs.Length)];
                var rank = (NobilityRank)Utility.RandomMinMax((int)NobilityRank.Commoner, (int)NobilityRank.Knight);
                var adv = new VirtualAdventurer(job, rank) { Gold = Utility.RandomMinMax(2000, 5000) };
                adv.EquipMissingLayers(town);
                IdleAdventurers.Add(adv);
            }
        }
    }

    // ==============================================================================
    // [?î?ú ?∏Îìú (?¥Îèô Í±∞Ï†ê)]
    // ==============================================================================
    public class WorldNode
    {
        public string Name { get; set; }
        public RegionCode RCode { get; set; } 
        public WorldNodeType Type { get; set; }
        public Map NodeMap { get; set; }
        public Point3D EntranceLoc { get; set; } 
        public Point3D TargetLoc { get; set; }   
        public int Difficulty { get; set; }      
        public Point3D Location { get => EntranceLoc; set => EntranceLoc = value; }

        public WorldNode(string name, RegionCode rCode, WorldNodeType type, Map map, Point3D ext, Point3D ins, int diff)
        {
            Name = name; RCode = rCode; Type = type; NodeMap = map; EntranceLoc = ext; TargetLoc = ins; Difficulty = diff;
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(1); 
            writer.Write(Name);
            writer.Write((int)RCode);
            writer.Write((int)Type);
            writer.Write(NodeMap);
            writer.Write(EntranceLoc);
            writer.Write(TargetLoc);
            writer.Write(Difficulty);
        }

        public WorldNode(GenericReader reader)
        {
            int version = reader.ReadInt();
            Name = reader.ReadString();
            if (version >= 1) RCode = (RegionCode)reader.ReadInt();
            Type = (WorldNodeType)reader.ReadInt();
            NodeMap = reader.ReadMap();
            EntranceLoc = reader.ReadPoint3D();
            TargetLoc = reader.ReadPoint3D();
            Difficulty = reader.ReadInt();
        }
    }

    // ==============================================================================
    // [Í∞Ä?Å Î™®ÌóòÍ∞Ä ?å???ú?§ÌÖú]
    // ==============================================================================
    public class AdventurerParty
    {
        public string Name => Members.Count > 0 ? $"{Members[0].Name}¿« ∆ƒ∆º" : "π´∏Ì ∆ƒ∆º";
        public List<VirtualAdventurer> Members { get; set; } = [];
        public AdventurerState State { get; set; }
        public WorldNode CurrentNode { get; set; }
        public WorldNode TargetNode { get; set; }
        public Point3D CurrentLocation { get; set; }
        public Map CurrentMap { get; set; }
        
        public int PartyWealth { get; set; }
        
        // ?ò?Ñ ?∏Ìôò?±ÏùÑ ?Ñ??Î≥Ä?òÎßå ?®Í≤®?êÍ≥† ?ú?§ÌÖú ?∞ÏÇ∞Ïóê?ú?î ?¨Ïö©ÌïòÏßÄ ?ä?å (?¥Ï? Íµ¨Îß§ Î≤ÑÍ∑??ê??
        public int Bandages { get; set; }
        public int Potions { get; set; }      
        
        public int TeamID { get; set; } 
        public int TravelHoursRemaining { get; set; }
        public int PackAnimals { get; set; }
        public int AcceptedJobReward { get; set; } = 0;

        public double AverageLevel => Members.Count > 0 ? Members.Average(m => m.Level) : 1.0;
        public VirtualCitizen EmployedSherpa { get; set; } 
        public LootDistributionRule LootRule { get; set; } = LootDistributionRule.Equal;

        public AdventurerParty() { }

        public AdventurerParty(WorldNode startNode)
        {
            this.TeamID = VirtualAdventurerManager.GetNextTeamID();
            CurrentNode = startNode;
            State = AdventurerState.Resting;
            if (startNode != null)
            {
                CurrentLocation = startNode.Location;
                CurrentMap = startNode.NodeMap;
            }
        }
        
        public void Dissolve()
        {
            DematerializeParty(); 
            for (int i = 0; i < Members.Count; i++)
            {
                if (Members[i].PhysicalObject != null)
                    Members[i].PhysicalObject.Team = 0; 
            }
            Members.Clear();
        }

        public int GetTotalPower()
        {
            if (Members.Count == 0) return 0;
            double synergy = 1.0 + (Members.Count * 0.1);
            return (int)(Members.Sum(m => m.CombatPower) * synergy);
        }

        public int CalculatePartyUnity()
        {
            if (Members.Count < 2) return 100;
            int totalDistance = 0;
            int pairs = 0;
            for (int i = 0; i < Members.Count; i++)
            {
                for (int j = i + 1; j < Members.Count; j++)
                {
                    totalDistance += Members[i].GetAffinityDistance(Members[j]);
                    pairs++;
                }
            }
            int avgDistance = pairs > 0 ? totalDistance / pairs : 0;
            int unity = 100 - (int)((avgDistance / 75.0) * 100);
            return Math.Max(0, unity);
        }

        public void OnTick()
        {
            if (Members.Count == 0) return;

            for (int i = Members.Count - 1; i >= 0; i--)
            {
                Members[i].UpdateSurvivalTick();
            }

            switch (State)
            {
                case AdventurerState.Resting: ProcessResting(); break;
                case AdventurerState.Traveling: ProcessTraveling(); break;
                case AdventurerState.Exploring: ProcessExploring(); break;
            }

            if (CurrentMap != null && CurrentMap != Map.Internal && CurrentLocation != Point3D.Zero)
            {
                var sector = CurrentMap.GetSector(CurrentLocation);
                if (State != AdventurerState.Traveling && sector.Active) 
                {
                    MaterializeParty(); 
                }
                else 
                {
                    DematerializeParty(); 
                }
            }
        }

        private void MaterializeParty()
        {
            bool hasMissingPhysical = false;
            for (int i = 0; i < Members.Count; i++)
            {
                if (Members[i].PhysicalObject == null || Members[i].PhysicalObject.Deleted)
                {
                    hasMissingPhysical = true;
                    break;
                }
            }

            if (!hasMissingPhysical) return;

            for (int i = 0; i < Members.Count; i++)
            {
                var m = Members[i];
                if (m.PhysicalObject == null || m.PhysicalObject.Deleted)
                {
                    var physical = new PhysicalAdventurer(m);
                    physical.MoveToWorld(CurrentLocation, CurrentMap);
                    m.PhysicalObject = physical; 
                }
            }
        }

        private void DematerializeParty()
        {
            for (int i = 0; i < Members.Count; i++)
            {
                var m = Members[i];
                if (m.PhysicalObject != null)
                {
                    m.PhysicalObject.Delete(); 
                    m.PhysicalObject = null;
                }
            }
        }

        public void SettleTownReturn(TownEconomy town)
        {
            if (Members.Count == 0) return;

            if (AcceptedJobReward > 0)
            {
                Members[0].Gold += AcceptedJobReward / 2;
                PartyWealth += AcceptedJobReward / 2;
                town.Wealth += (int)(AcceptedJobReward * 0.1); 

                Console.WriteLine($"[Quest] {Members[0].Name} ?å?∞Í? ?òÎ¢∞Î? ?Ñ?ò?òÍ≥† {AcceptedJobReward}gpÎ•??ç?ù?à?µÎãà??");
                int huntScore = this.AcceptedJobReward / 10;
                if (huntScore > 0)
                {
                    Server.Misc.FamilySystem.Contribute(this.Name, huntScore, Server.Items.FamilyCompType.Hunting, true);
                }
                AcceptedJobReward = 0; 
            }

            if (EmployedSherpa == null) return;

            if (EmployedSherpa.Backpack != null)
            {
                var itemsToSell = EmployedSherpa.Backpack.Items.ToArray();
                int totalEarned = 0;

                for (int i = 0; i < itemsToSell.Length; i++)
                {
                    var item = itemsToSell[i];
                    int itemValue = town.GetPrice(item.GetType()) / 2;
                    totalEarned += itemValue;
                    
                    if (!town.Warehouse.ContainsKey(item.GetType())) town.Warehouse[item.GetType()] = new WarehouseItem(item.GetType(), 0, itemValue * 2);
                    town.Warehouse[item.GetType()].Stock++; 
                    item.Delete();
                }

                town.Wealth -= totalEarned;

                int sherpaCut = (int)(totalEarned * 0.1);
                EmployedSherpa.Gold += sherpaCut;
                Members[0].Gold += (totalEarned - sherpaCut); 
                
                if (sherpaCut > 0)
                {
                    Console.WriteLine($"[Adventurer] ?∞Î•¥?å {EmployedSherpa.Name}??Í∞Ä) ?ò?Ñ Í∏∏Ïû°Ïù?Î∞è Î¨ºÏûê Í¥ÄÎ¶??ò?πÏúºÎ? {sherpaCut}gpÎ•?Î∞õ?ò?µÎãà??");
                }
            }

            if (PartyWealth < 2000)
            {
                EmployedSherpa.Stress = 0; 
                EmployedSherpa = null;
                
                if (PackAnimals > 0)
                {
                    int refund = PackAnimals * 500; 
                    PartyWealth += refund;
                    town.Wealth -= refund;
                    PackAnimals = 0;
                }
            }
        }

        public bool TryHireSherpa(TownEconomy town)
        {
            if (EmployedSherpa != null) return true;

            var laborer = town.Citizens.FirstOrDefault(c => c.JobClass == NpcJobClass.Laborer && c.Gold < 1000);
            if (laborer != null)
            {
                int hireCost = 150; 
                int totalGold = Members.Sum(m => m.Gold);

                if (totalGold >= hireCost)
                {
                    Members[0].Gold -= hireCost; 
                    laborer.Gold += hireCost; 
                    EmployedSherpa = laborer;
                    return true;
                }
            }
            return false;
        }

        private void ProcessResting()
        {
            // ?åü [?µÏã?Î∞©Ïñ¥Î?] Îßà?Ñ???Ñ?å Í≥??ò?Ñ ???ê?ú ?¥Ïãù ?Å?ú?ê Îπ†Ï°å?§Î©¥ Ï¶â?ú ÎπÑ?Å ?àÏ∂ú!
            if (CurrentNode != null && CurrentNode.Type != WorldNodeType.Town)
            {
                Console.WriteLine($"[Adventurer] {Members[0].Name} ?å?∞Í? {CurrentNode.Name}?ê?ú Ï°∞ÎÇú ?Å?úÎ•??∏Ï??òÍ≥† ÎπÑ?Å Í∑Ä?ò?Ñ ?ú?Ñ?©Îãà??");
                RetreatToTown();
                return;
            }

            var town = TownEconomyManager.Towns.Values.OrderBy(t => Utility.GetDistanceToSqrt(CurrentLocation, t.Center)).FirstOrDefault();
            
            if (town != null)
            {
                SettleTownReturn(town); 

                // ?åü ?ÅÍ∂å???Ü?î ?Ñ?ú(?à: ÎßàÏßÑ?ú?Ñ)?ê?ú?î ?∏ÏúÑ Î≥¥ÏÉÅ ?ï?∞Î? ?òÍ≥† ?•Î? ?òÎ¶¨ÎÇò ?Ñ?¥ÌÖú Íµ¨Îß§ ???ïÎπÑÎ•??ù?µÌï©Îãà??
                if (town.VendorCount > 0)
                {
                    int innFee = Members.Count * 20;
                    if (PartyWealth >= innFee) 
                    { 
                        PartyWealth -= innFee; 
                        VirtualAdventurerManager.PayToCitizenOrTown(town, innFee, NpcJobClass.InnKeeper); 
                    }

                    if (PartyWealth > 2000) TryHireSherpa(town); 
                    int reqB = Math.Max(0, 100 - this.Bandages); if (reqB > 0 && PartyWealth >= reqB * 2) { PartyWealth -= reqB * 2; town.Wealth += reqB * 2; this.Bandages += reqB; }
                    int reqP = Math.Max(0, 20 - this.Potions); if (reqP > 0 && PartyWealth >= reqP * 15) { PartyWealth -= reqP * 15; town.Wealth += reqP * 15; this.Potions += reqP; } 

                    int mountCost = 500;
                    bool needMounts = false;
                    for (int i = 0; i < Members.Count; i++)
                    {
                        if (!Members[i].HasMount) { needMounts = true; break; }
                    }

                    if (needMounts && PartyWealth > (Members.Count * mountCost))
                    {
                        for (int i = 0; i < Members.Count; i++) 
                        {
                            if (!Members[i].HasMount && PartyWealth >= mountCost) 
                            {
                                PartyWealth -= mountCost;
                                town.Wealth += mountCost;
                                Members[i].HasMount = true;
                            }
                        }
                        Console.WriteLine($"[Adventurer] {Members[0].Name} ?å?∞Í? ?†?†?òÍ≤å ?Ñ?ê ?πÎ?Î•?ÎßàÏ≥§ÏäµÎãà??");
                    }

                    if (EmployedSherpa != null)
                    {
                        if (PartyWealth > 5000 && PackAnimals < 3)
                        {
                            int animalCost = 1000;
                            PartyWealth -= animalCost;
                            town.Wealth += animalCost;
                            PackAnimals++;
                            Console.WriteLine($"[Adventurer] {Members[0].Name} ?å?∞Í? ?ê?ï?Ñ ?Ñ??ÏßêÎßê?Ñ Ï∂îÍ∞Ä Íµ¨Îß§?à?µÎãà?? (?Ñ?? {PackAnimals}ÎßàÎ¶?");
                        }
                    }

                    for (int i = 0; i < Members.Count; i++)
                    {
                        Members[i].ProcessSmarterShopping(town);
                    }

                    for (int i = Members.Count - 1; i >= 0; i--)
                    {
                        var m = Members[i];
                        var retirement = m.CheckRetirement();
                        if (retirement.IsRetiring)
                        {
                            m.RetireToCitizen(town, retirement.NewRank);
                            continue;
                        }
                        if (Utility.RandomDouble() < 0.4) m.TryRepairEquipment(town);
                    }

                    if (PartyWealth > 10000)
                    {
                        PartyWealth -= 5000; 
                        town.Wealth += 5000;
                        for (int i = 0; i < Members.Count; i++)
                        {
                            Members[i].EquipmentTier++; 
                        }
                    }
                }
            }

            for (int i = 0; i < Members.Count; i++)
            {
                Members[i].HP = Members[i].MaxHP;
                Members[i].Stress = Math.Max(0, Members[i].Stress - 30);
            }

            if (town != null)
            {
                var availableJobs = PartTimeManager.ActiveRequests
                    .Where(r => r.TownName == town.TownName 
                             && (r.Category == JobCategory.DungeonHunting || r.Category == JobCategory.EcoHunting)
                             && !r.IsFullyBooked
                             && !r.IsAIAssigned
                             && (DateTime.Now - r.CreationTime).TotalMinutes >= 25.0)
                    .ToList();

                if (availableJobs.Count > 0)
                {
                    var acceptedJob = availableJobs[Utility.Random(availableJobs.Count)];
                    acceptedJob.CurrentParticipants++;
                    acceptedJob.IsAIAssigned = true; 
                    
                    if (acceptedJob.Category == JobCategory.EcoHunting)
                    {
                        var ecoZone = Server.Misc.EcosystemManager.ZoneList.FirstOrDefault(z => z.ZoneId == acceptedJob.RegionName);
                        if (ecoZone != null && ecoZone.Nodes != null && ecoZone.Nodes.Count > 0 && ecoZone.Nodes[0] != null)
                        {
                            Point3D dest = ecoZone.Nodes[0].Location;
                            TargetNode = new WorldNode(ecoZone.ZoneId, ecoZone.RCode, WorldNodeType.Dungeon, ecoZone.Facet, dest, dest, 100);
                            this.AcceptedJobReward = acceptedJob.RewardGold; 
                            Console.WriteLine(String.Format("[Quest] {0} ?å?∞Í? ?ûÎßà???ù?úÍ≥Ñ ?¨ÎÉ?'{1}' ?òÎ¢∞Î? ?ò?ΩÌïòÍ≥† Ï≤≠?¨Î? ?•Ìï©Îãà??", Members[0].Name, acceptedJob.Title));
                        }
                    }
                    else
                    {
                        var questDz = DungeonManager.ZoneList.FirstOrDefault(z => z.RCode.ToString() == acceptedJob.RegionName && z.IsActive);
                        if (questDz != null)
                        {
                            Point3D dest = questDz.GetCenterLocation();
                            if (dest == Point3D.Zero) dest = RegionSaver.GetRegionCenter(questDz.RCode, questDz.Facet);

                            if (dest != Point3D.Zero)
                            {
                                TargetNode = new WorldNode(questDz.ZoneId, questDz.RCode, WorldNodeType.Dungeon, questDz.Facet, dest, dest, questDz.CurrentDifficulty);
                                this.AcceptedJobReward = acceptedJob.RewardGold; 
                                Console.WriteLine($"[Quest] {Members[0].Name} ?å?∞Í? Î∞©Ï??ú '{acceptedJob.Title}' ?òÎ¢∞Î? ?ò?ΩÌïòÍ≥† Ï∂úÎ∞ú Ï§ÄÎπÑÎ•?ÎßàÏ≥§ÏäµÎãà??");
                            }
                        }
                    }
                }
            }

            if (TargetNode == null && town != null)
            {
                if (AverageLevel < 10 && Utility.RandomBool())
                {
                    var sortedTowns = TownEconomyManager.Towns.Values
                        .Where(t => t.Facet == CurrentMap && t.TownName != town.TownName)
                        .OrderBy(t => Utility.GetDistanceToSqrt(CurrentLocation, t.Center))
                        .ToList();

                    if (sortedTowns.Count > 0)
                    {
                        int pool = Math.Min(3, sortedTowns.Count);
                        var targetTown = sortedTowns[Utility.Random(pool)];
                        var tCode = RegionSaver.GetRegionCodes(targetTown.Facet, targetTown.Center.X, targetTown.Center.Y, targetTown.Center.Z).Major;
                        TargetNode = new WorldNode(targetTown.TownName ?? targetTown.Name, tCode, WorldNodeType.Town, targetTown.Facet, targetTown.Center, targetTown.Center, 1);
                        
                        Console.WriteLine($"[Adventurer] Ï¥àÎ≥¥Ïù?{Members[0].Name} ?å?∞Í? ?∏ÏúÑÎ•??Ñ???¥ÏõÉ Îßà?Ñ??{TargetNode.Name}(??Î°ú ?•ÌïòÍ∏∞Î? ?à?µÎãà??");
                    }
                }
                else
                {
                    // ?åü [?µÏã??®Ï?] ÎπÑ?ú?±Ìôî(IsActive == false) ?ò?Ñ?Ä ?ê???¨ÎÉ?Î™©Ï†ÅÏßÄ?ê?ú ?ú??
                    var validDungeons = DungeonManager.ZoneList.Where(z => z.IsActive && z.MaxPopulation > 0 && z.Phase == DungeonPhase.Active).ToList();
                    
                    if (validDungeons.Count > 0)
                    {
                        // ?î?[Î≤ÑÍ∑??ò?ï] CurrentDifficulty(?Ñ???¥Í∏∞)?î ÎπÑ?¥Ïûà?ºÎ©¥ ?ÑÎ∂Ä 0?¥Îù?Íµ¨Î? Î∂àÍ∞Ä.
                        // MaxDifficulty(Ïµú?Ä ?¥Í∏∞ = ?§Í? ?ú?¥ÎèÑ)Î•?Í∏∞Ï??ºÎ? Îß§Ïπ≠?¥Ïï?Ï¥àÎ≥¥Ï°¥(Despise/Covetous L1)?ºÎ? ?¨Î?Î•¥Í? Î∞∞Ï†ï??
                        int targetDiff = (int)(AverageLevel * 100000);
                        var suitableDungeons = validDungeons
                            .OrderBy(z => Math.Abs(z.MaxDifficulty - targetDiff))
                            .Take(3) 
                            .ToList();

                        if (suitableDungeons.Count > 0)
                        {
                            var targetDz = suitableDungeons[Utility.Random(suitableDungeons.Count)];
                            // ?à ?ò?Ñ ?ú?§ÌÖú?ò Íµ¨Ïó≠ Ï§ë?ô Ï¢å?ú API ?∏Ï?
                            Point3D dest = targetDz.GetCenterLocation();
                            if (dest == Point3D.Zero) dest = RegionSaver.GetRegionCenter(targetDz.RCode, targetDz.Facet);
                            
                            if (dest != Point3D.Zero)
                            {
                                TargetNode = new WorldNode(targetDz.ZoneId, targetDz.RCode, WorldNodeType.Dungeon, targetDz.Facet, dest, dest, targetDz.CurrentDifficulty);
                                Console.WriteLine($"[Adventurer] {Members[0].Name} ?å?∞Í? ?ê???¨ÎÉ•ÏùÑ ?Ñ??{TargetNode.Name}(??Î°ú Ï∂úÎ∞ú Ï§ÄÎπÑÎ•?ÎßàÏ≥§ÏäµÎãà??");
                            }
                        }
                    }
                }
            }

            if (TargetNode != null && CurrentNode != null)
            {
                bool allMounted = true;
                for (int i = 0; i < Members.Count; i++)
                {
                    if (!Members[i].HasMount) { allMounted = false; break; }
                }

                var plan = VirtualTravelNetwork.CalculateBestRoute(CurrentNode.RCode, TargetNode.RCode, PartyWealth, allMounted);

                if (plan.IsPossible)
                {
                    PartyWealth -= plan.TotalCost;
                    if (town != null) town.Wealth += plan.TotalCost; 
                    
                    TravelHoursRemaining = plan.TotalTicks;
                    State = AdventurerState.Traveling;
                    
                    Console.WriteLine($"[Travel] {Members[0].Name} ?å?∞Í? {CurrentNode.Name}?ê?ú {TargetNode.Name}(??Î°ú ?¥Îèô?Ñ ?ú?ë?©Îãà?? (ÎπÑ?? {plan.TotalCost}gp / ?å?î: {plan.TotalTicks}??");
                }
                else
                {
                    bool isUnregistered = !VirtualTravelNetwork.IsNodeRegistered(CurrentNode.RCode) || !VirtualTravelNetwork.IsNodeRegistered(TargetNode.RCode);

                    if (isUnregistered)
                    {
                        int distance = (int)Utility.GetDistanceToSqrt(CurrentLocation, TargetNode.Location);
                        int ticks = Math.Max(1, distance / 300); 
                        if (allMounted) ticks = Math.Max(1, ticks / 2);

                        ticks = Math.Min(10, ticks); 

                        TravelHoursRemaining = ticks;
                        State = AdventurerState.Traveling;
                        Console.WriteLine($"[Travel] {Members[0].Name} ?å?∞Í? ÎπÑ?ïÍ∑ú ÏßÄ?≠??{TargetNode.Name}(??Î°ú ?ê?ò?Ñ ?†?©Îãà?? ({ticks}???å?î)");
                    }
                    else
                    {
                        Console.WriteLine($"[Travel] {Members[0].Name} ?å?∞Í? ?êÍ∏à Î∂ÄÏ°±ÏúºÎ? Î∞ú??Î¨∂ÏòÄ?µÎãà??");
                        TargetNode = null; 
                    }
                }
            }
        }

        private void ProcessTraveling()
        {
            if (TargetNode == null) { State = AdventurerState.Resting; return; }

            if (TravelHoursRemaining > 0)
            {
                TravelHoursRemaining--;
            }
            else
            {
                CurrentLocation = TargetNode.Location;
                CurrentMap = TargetNode.NodeMap;
                CurrentNode = TargetNode;
                TargetNode = null;
                
                if (CurrentNode.Type == WorldNodeType.Town && State == AdventurerState.Traveling && AverageLevel < 10)
                {
                    int patrolReward = Members.Count * 150;
                    var town = TownEconomyManager.Towns.Values.FirstOrDefault(t => t.TownName == CurrentNode.Name);
                    if (town != null && town.Wealth >= patrolReward)
                    {
                        town.Wealth -= patrolReward;
                        PartyWealth += patrolReward;
                        Console.WriteLine($"[Adventurer] {Members[0].Name} ?å?∞Í? {CurrentNode.Name}?ê Î¨¥ÏÇ¨Ìûà ?ÑÏ∞©Ìïò???∏ÏúÑ ?ò??{patrolReward}gpÎ•?Î∞õ?ò?µÎãà??");
                    }
                }

                State = CurrentNode.Type == WorldNodeType.Dungeon ? AdventurerState.Exploring : AdventurerState.Resting;
                
                if (CurrentNode.Type == WorldNodeType.Dungeon)
                    Console.WriteLine($"[Adventurer] {Members[0].Name} ?å?∞Í? ?ò?Ñ {CurrentNode.Name}?ê ÏßÑ?Ö?à?µÎãà??");
            }
        }

        // ?åü [?µÏã??ò?ï] Í∞ÄÏßú Î≥Ä?ò(Party.Bandages) ?Ä?† ?å?∞Ïõê Í∞ÄÎ∞??ç?ò 'ÏßÑÏßú ?§Î¨º ?Ñ?¥ÌÖú'?Ñ ?åÎ™?
        private void ConsumePartyPhysicalItems(Type type, int amount)
        {
            int remaining = amount;
            foreach (var m in Members)
            {
                if (remaining <= 0) break;
                if (m.Backpack == null) continue;
                
                Item[] items = m.Backpack.FindItemsByType(type);
                foreach (var item in items)
                {
                    if (item.Amount > remaining)
                    {
                        item.Amount -= remaining;
                        return;
                    }
                    else
                    {
                        remaining -= item.Amount;
                        item.Delete();
                        if (remaining <= 0) return;
                    }
                }
            }
        }

        private void ProcessExploring()
        {
            if (CurrentNode == null)
            {
                TargetNode = GetFallbackNearestTown();
                if (TargetNode == null)
                    TargetNode = new WorldNode("Britain", RegionCode.Trammel_Town_Britain, WorldNodeType.Town, Map.Trammel, new Point3D(1426, 1695, 0), new Point3D(1426, 1695, 0), 1);
                
                Console.WriteLine($"[Adventurer] {Members[0].Name} ?å?∞Í? ?ÑÏπò ?ïÎ≥¥Î? ?Å?§Ìïò??{TargetNode.Name}(??Î°ú Í∏¥Í? ?àÏ∂ú?©Îãà??");
                this.TravelHoursRemaining = 5; 
                this.State = AdventurerState.Traveling; 
                return;
            }

            double efficiency = 1.0 - (PackAnimals * 0.15); 
            if (EmployedSherpa != null) efficiency -= 0.20;

            int bandageConsume = (int)Math.Max(1, Members.Count * efficiency * 1.5);
            int potionConsume = (int)Math.Max(1, (Members.Count / 2) * efficiency * 0.5);

            // ?åü [?µÏã??ò?ï] ?å?∞Ïõê?§Ïùò Í∞ÄÎ∞©Ïóê?ú ÏßÑÏßú ?Ñ?¥ÌÖú ?åÎ™?
            ConsumePartyPhysicalItems(typeof(Bandage), bandageConsume);
            ConsumePartyPhysicalItems(typeof(GreaterHealPotion), potionConsume);

            this.Bandages -= bandageConsume;
            this.Potions -= potionConsume;

            bool needsRetreat = false;
            
            if (this.Bandages < 10 || this.Potions < 5) needsRetreat = true;
            else
            {
                for (int i = 0; i < Members.Count; i++)
                {
                    // ?åü Î∞∞Í??î, Î™©Î?Î¶Ñ??15000 ?¥ÌïòÎ°ú ?®Ïñ¥Ï?Î©??ùÏ°??Ñ?ë?ºÎ? Í∞ÑÏ£ºÌïòÍ≥† Ï¶âÍ∞Å ?¥Í?
                    if (Members[i].HP < Members[i].MaxHP * 0.3 || Members[i].Stress > 90 || Members[i].Hunger < 15000 || Members[i].Thirst < 15000)
                    {
                        needsRetreat = true; break;
                    }
                }
            }
            
            if (needsRetreat)
            {
                RetreatToTown();
                return;
            }

            bool isPhysicalActive = false;
            for (int i = 0; i < Members.Count; i++)
            {
                if (Members[i].PhysicalObject != null && !Members[i].PhysicalObject.Deleted)
                {
                    isPhysicalActive = true;
                    break;
                }
            }

            if (isPhysicalActive) return; 

            BatchCombatTick();
        }

        // ========================================================================
        // ?îÆ [Íµê?ï ?ÑÎ£å] RetreatToTown: ÎßàÎ≤ï???Å?ê ?Ñ?ú ?§Î???Í∑Ä?ò ?îÏßÑ ?Å??
        // ========================================================================
        private void RetreatToTown()
        {
            if (CurrentNode == null)
            {
                TargetNode = GetFallbackNearestTown();
                if (TargetNode == null) TargetNode = new WorldNode("Britain", RegionCode.Trammel_Town_Britain, WorldNodeType.Town, Map.Trammel, new Point3D(1426, 1695, 0), new Point3D(1426, 1695, 0), 1);
                this.TravelHoursRemaining = 5; 
                this.State = AdventurerState.Traveling; 
                return;
            }

            RegionCode majorDungeonCode = RegionSaver.GetMajorCode(CurrentNode.RCode);
            
            // ÎßàÎ≤ï???àÏ∂ú (Í≤å?¥Ìä?Î¶¨Ï? Í∞Ä?ô Ï°∞Í±¥) Ï≤¥ÌÅ?
            bool hasMage = false;
            for (int i = 0; i < Members.Count; i++)
            {
                if (Members[i].JobClass == NpcJobClass.Wizard || 
                    Members[i].JobClass == NpcJobClass.Necromancer || 
                    Members[i].Role == AdventurerRole.MagicDPS || 
                    Members[i].Role == AdventurerRole.Healer)
                {
                    hasMage = true;
                    break;
                }
            }

            // ?åü [Í∏∞Ìöç Î≥¥Ï†ï] ÎßàÎ≤ï?¨Í? Ï°¥Ïû¨Ìï† Í≤ΩÏö? Í≥†Î¶ΩÎêú ?¥Í?Î°ú?ò ??ÏßÄ?ï?Ñ Î¨¥Ïãú?òÍ≥†,
            // ?Å?ê ?ÅÍ∂å(VendorCount > 0)???¥ÏïÑ?à?î ?ÄÎ•ô ??Í∞Ä??Í∞ÄÍπå???Å?Ö Ï§ë?¨Ï?Î°ú Ï∞®Ïõê ?¥Îèô?©Îãà??
            if (hasMage)
            {
                TownEconomy bestShopTown = null;
                double minShopDistance = double.MaxValue;

                // ?åü [Î≤ÑÍ∑??ò?ï] ?ò?Ñ ?¥Î? Î¨ºÎ¶¨ Ï¢å?ú(CurrentLocation)Î•?Í∏∞Ï??ºÎ? Í≥Ñ?∞ÌïòÎ©?
                // Î™®Îì† ?ò?Ñ??Î™®Ïó¨Ïûà?î ?ôÏ™??ù Î¨ºÎ¶¨?Å ?ÑÏπò ?åÎ¨∏Ïóê ?≠?Å ÎßàÏßÑ?ú?Ñ/?å?∏ÏïÑÎßå ?†?ù?ò?î ?§Î?(Magincia Bug) Î∞ú?ù.
                // ?∞ÎùºÏÑú ?ò?ÑÍ≥??∞Í≤∞?ú '?ºÎ¶¨?Å ?¥Í? Îßà?Ñ'?ò Ï§ë??Ï¢å?úÎ•?Í∏∞Ï??ê?ºÎ? ?ºÏïÑ ?Å?ê ?Ñ?úÎ•?Í≤Ä?â?©Îãà??
                Point3D logicalReferencePoint = CurrentLocation;
                if (DungeonRetreatManager.Map.TryGetValue(majorDungeonCode, out RetreatRoute mRoute))
                {
                    var logicalTown = TownEconomyManager.Towns.Values.FirstOrDefault(t => 
                        t.Facet == CurrentMap && 
                        (t.TownName != null || t.Name != null) &&
                        mRoute.TownCode.ToString().Contains((t.TownName ?? t.Name).Replace(" ", "").Replace("'", "")));
                    
                    if (logicalTown != null) 
                        logicalReferencePoint = logicalTown.Center;
                }

                var allTowns = TownEconomyManager.Towns.Values;
                foreach (TownEconomy town in allTowns)
                {
                    if (town != null && town.Facet == CurrentMap && town.VendorCount > 0)
                    {
                        double dist = Utility.GetDistanceToSqrt(logicalReferencePoint, town.Center);
                        if (dist < minShopDistance)
                        {
                            minShopDistance = dist;
                            bestShopTown = town;
                        }
                    }
                }

                // Îßå???ÄÎ•ô ?¥Ïóê ?ú?±Ìôî?ú ?Å?ê ?Ñ?úÍ∞Ä ?Ü?§Î©¥, ?ÄÎ•ô ???Ñ?ò?ò Í∞Ä??Í∞ÄÍπå??Îßà?Ñ?Ñ ?†?ù
                if (bestShopTown == null)
                {
                    foreach (TownEconomy town in allTowns)
                    {
                        if (town != null && town.Facet == CurrentMap)
                        {
                            double dist = Utility.GetDistanceToSqrt(logicalReferencePoint, town.Center);
                            if (dist < minShopDistance)
                            {
                                minShopDistance = dist;
                                bestShopTown = town;
                            }
                        }
                    }
                }

                if (bestShopTown != null)
                {
                    var tCode = RegionSaver.GetRegionCodes(bestShopTown.Facet, bestShopTown.Center.X, bestShopTown.Center.Y, bestShopTown.Center.Z).Major;
                    string safeName = bestShopTown.TownName ?? bestShopTown.Name ?? "?Å?ê ?Ñ?ú";
                    TargetNode = new WorldNode(safeName, tCode, WorldNodeType.Town, bestShopTown.Facet, bestShopTown.Center, bestShopTown.Center, 1);

                    this.TravelHoursRemaining = 1;
                    this.State = AdventurerState.Traveling;
                    Console.WriteLine($"[Adventurer] {Members[0].Name} ?å?∞Í? ÎßàÎ≤ï?¨Ïùò Í≤å?¥Ìä?ÎßàÎ≤ï?Ñ ?µÌï??Å?ê???ÑÎπÑ?ú {TargetNode.Name}(??Î°ú ?à?Ñ?òÍ≤å Í≥µÍ??¥Îèô ?¥Í??à?µÎãà??");
                    return;
                }
            }

            // --------------------------------------------------------------------
            // ?ö?ÎπÑÎßàÎ≤ï???å?∞Ï†Ñ???ºÎ? ?ÑÎ≥??†Î∞ï ?¥Í? Î£®Ìä?(Í∏∞Ï°¥ ?à?Ñ ÏΩî?ú Î≥¥Ï°¥)
            // --------------------------------------------------------------------
            if (!DungeonRetreatManager.Map.TryGetValue(majorDungeonCode, out RetreatRoute route))
            {
                TargetNode = GetFallbackNearestTown();
                if (TargetNode == CurrentNode || TargetNode == null)
                    TargetNode = new WorldNode("Britain", RegionCode.Trammel_Town_Britain, WorldNodeType.Town, Map.Trammel, new Point3D(1426, 1695, 0), new Point3D(1426, 1695, 0), 1);
                
                Console.WriteLine($"[Adventurer] {Members[0].Name} ?å?∞Í? ?àÏ∂úÎ°úÎ•?Ï∞æÏ? Î™ªÌï?{TargetNode.Name}(??Î°ú ?ò?ú?ú ÎπÑ?Å ?âÍµ∞ÏùÑ ?ú?ë?©Îãà??");
                this.TravelHoursRemaining = 5; 
                this.State = AdventurerState.Traveling; 
                return;
            }
            else
            {
                TownEconomy targetTown = System.Linq.Enumerable.FirstOrDefault(TownEconomyManager.Towns.Values, t => 
                    t.Facet == CurrentMap && 
                    (t.TownName != null || t.Name != null) &&
                    route.TownCode.ToString().Contains((t.TownName ?? t.Name).Replace(" ", "").Replace("'", "")));

                if (targetTown != null)
                {
                    string safeName = targetTown.TownName ?? targetTown.Name ?? "ÏßÄ?ï ?Ä?ºÏÜå";
                    TargetNode = new WorldNode(safeName, route.TownCode, WorldNodeType.Town, targetTown.Facet, targetTown.Center, targetTown.Center, 1);
                }
                else
                {
                    TargetNode = GetFallbackNearestTown();
                    if (TargetNode == CurrentNode || TargetNode == null) 
                        TargetNode = new WorldNode("Britain", RegionCode.Trammel_Town_Britain, WorldNodeType.Town, Map.Trammel, new Point3D(1426, 1695, 0), new Point3D(1426, 1695, 0), 1);
                    
                    Console.WriteLine($"[Adventurer] {Members[0].Name} ?å?∞Í? ÏßÄ?ï ?Ä?ºÏÜåÍ∞Ä ?ê?Ñ?ò??{TargetNode.Name}(??Î°ú ÎπÑ?Å ?âÍµ∞Ìï©Îãà??");
                    this.TravelHoursRemaining = 5;
                    this.State = AdventurerState.Traveling;
                    return;
                }
            }

            int ferryCost = route.IsIsland ? 500 * Members.Count : 0;
            bool allMounted = true;
            for (int i = 0; i < Members.Count; i++)
            {
                if (!Members[i].HasMount) 
                { 
                    allMounted = false; 
                    break; 
                }
            }

            TownEconomy townToPay = null;
            foreach (TownEconomy t in TownEconomyManager.Towns.Values)
            {
                if (RegionSaver.GetRegionCodes(t.Facet, t.Center.X, t.Center.Y, t.Center.Z).Major == route.TownCode)
                {
                    townToPay = t;
                    break;
                }
            }

            if (route.IsIsland)
            {
                if (PartyWealth >= ferryCost)
                {
                    PartyWealth -= ferryCost;
                    if (townToPay != null) townToPay.Wealth += ferryCost;

                    this.TravelHoursRemaining = Math.Max(1, route.BaseDistance / 300); 
                    this.State = AdventurerState.Traveling;
                    Console.WriteLine($"[Adventurer] {Members[0].Name} ?å?∞Í? Î∞∞ÏÇØ {ferryCost}gpÎ•??¥Í? {TargetNode.Name}(??Î°ú ?¥ÏÉÅ ?¥Í??©Îãà??");
                }
                else
                {
                    Console.WriteLine($"[Adventurer] {Members[0].Name} ?å?∞Í? Î∞∞ÏÇØ({ferryCost}gp)???ÜÍ≥† ÎßàÎ≤ï?¨ÎèÑ ?Ü???¨Ïóê Í≥†Î¶ΩÎêò?à?µÎãà?? (?ÑÎ©??ÑÍ∏?");
                    this.State = AdventurerState.Exploring; 
                    this.TargetNode = null; 
                }
            }
            else
            {
                int baseCost = 50 * Members.Count; 
                if (PartyWealth >= baseCost)
                {
                    PartyWealth -= baseCost;
                    if (townToPay != null) townToPay.Wealth += baseCost;

                    int ticks = Math.Max(1, route.BaseDistance / 300);
                    if (allMounted) ticks = Math.Max(1, ticks / 2);
                    
                    this.TravelHoursRemaining = ticks;
                    this.State = AdventurerState.Traveling;
                    Console.WriteLine($"[Adventurer] {Members[0].Name} ?å?∞Í? ?¨Î?Î•?ÏßÄÎ∂à?òÍ≥† {TargetNode.Name}(??Î°ú ?¥Í??©Îãà??");
                }
                else
                {
                    int ticks = Math.Max(2, (route.BaseDistance / 300) * 3); 
                    this.TravelHoursRemaining = ticks;
                    this.State = AdventurerState.Traveling;
                    Console.WriteLine($"[Adventurer] {Members[0].Name} ?å?∞Í? Î¨¥ÏùºÌëºÏúºÎ? {TargetNode.Name}ÍπåÏßÄ Îß®Î™∏ ?âÍµ∞ÏùÑ Í∞ï?â?©Îãà?? (?Ñ???Ñ?ò)");
                }
            }
        }

        private WorldNode GetFallbackNearestTown()
        {
            Point3D referencePoint = CurrentLocation;
            if (CurrentNode != null)
            {
                RegionCode majorCode = RegionSaver.GetMajorCode(CurrentNode.RCode);
                if (DungeonRetreatManager.Map.TryGetValue(majorCode, out RetreatRoute mRoute))
                {
                    var logicalTown = System.Linq.Enumerable.FirstOrDefault(TownEconomyManager.Towns.Values, t => 
                        t.Facet == CurrentMap && 
                        (t.TownName != null || t.Name != null) &&
                        mRoute.TownCode.ToString().Contains((t.TownName ?? t.Name).Replace(" ", "").Replace("'", "")));
                    if (logicalTown != null) referencePoint = logicalTown.Center;
                }
            }

            var town = System.Linq.Enumerable.FirstOrDefault(
                System.Linq.Enumerable.OrderBy(
                    System.Linq.Enumerable.Where(TownEconomyManager.Towns.Values, t => t.Facet == CurrentMap),
                    t => Utility.GetDistanceToSqrt(referencePoint, t.Center)
                )
            );

            if (town != null)
            {
                var tCode = RegionSaver.GetRegionCodes(town.Facet, town.Center.X, town.Center.Y, town.Center.Z).Major;
                return new WorldNode(town.TownName ?? town.Name ?? "¿Œ±Ÿ ∏∂¿ª", tCode, WorldNodeType.Town, town.Facet, town.Center, town.Center, 1);
            }
            return CurrentNode;
        }

        public void BatchCombatTick()
        {
            if (Members.Count == 0 || CurrentNode == null) return;

            double totalPartyDamage = Members.Sum(m => m.CombatPower * 12.0 * 10); 
            double totalPartyHealing = Members.Where(m => m.Role == AdventurerRole.Healer).Sum(m => m.CombatPower * 8.0 * 10);

            var monsters = FindAllMonstersInDungeon(CurrentNode.NodeMap, CurrentNode.TargetLoc, 30);
            if (monsters.Count == 0) return;

            double remainingDamage = totalPartyDamage;
            int killCount = 0;

            foreach (var monster in monsters.OrderBy(m => m.Hits))
            {
                if (remainingDamage <= 0) break;

                int oldHits = monster.Hits;
                if (remainingDamage >= oldHits)
                {
                    remainingDamage -= oldHits;
                    DistributeLoot(monster); 
                    monster.Kill();
                    killCount++;
                }
                else
                {
                    monster.Hits -= (int)remainingDamage;
                    remainingDamage = 0;
                }
            }

            double incomingDamage = monsters.Take(8).Sum(m => m.DamageMax * 2.5 * 10);
            double mitigatedDamage = Math.Max(incomingDamage * 0.05, incomingDamage - totalPartyHealing);
            
            int extraSupply = Math.Max(1, monsters.Count / 3);
            ConsumePartyPhysicalItems(typeof(Bandage), extraSupply);

            // ?åü Í∂Å?ò?§ÏùÄ ?§Ï†ú ?î??Î≥ºÌä∏Î? ?åÎ™?(?ú??Í≤ΩÏ†ú ?∞Îèô)
            foreach (var m in Members)
            {
                if (m.Role == AdventurerRole.RangedDPS && m.Backpack != null)
                {
                    Item arrow = m.Backpack.FindItemByType(typeof(Arrow)) ?? m.Backpack.FindItemByType(typeof(Bolt));
                    if (arrow != null)
                    {
                        if (arrow.Amount > 10) arrow.Amount -= 10;
                        else arrow.Delete();
                    }
                }
            }

            for (int i = Members.Count - 1; i >= 0; i--)
            {
                var m = Members[i];
                double damageFactor = m.Role == AdventurerRole.Tank ? 0.4 : (m.Role == AdventurerRole.Healer ? 0.1 : 0.15);
                int taken = (int)(mitigatedDamage * damageFactor);
                
                m.HP -= taken;
                m.Stress += (1 + (monsters.Count / 5)); 

                if (m.HP <= 0)
                {
                    if (PartyWealth >= 1000)
                    {
                        PartyWealth -= 1000;
                        m.HP = m.MaxHP / 3; 
                        m.Stress = 80;      
                        
                        var town = TownEconomyManager.Towns.Values.OrderBy(t => Utility.GetDistanceToSqrt(CurrentLocation, t.Center)).FirstOrDefault();
                        if (town != null) VirtualAdventurerManager.PayToCitizenOrTown(town, 1000, NpcJobClass.Healer_Master, NpcJobClass.Priest);
                        
                        Console.WriteLine($"[Adventurer] {m.Name}??Í∞Ä) ÏπòÎ™Ö?Å?Ñ ?Ö?à?ºÎÇò ?å???êÍ∏à?ºÎ? Íµ¨Ï°∞?à?µÎãà??");
                    }
                    else
                    {
                        m.SpawnAdventurerChest(CurrentMap, CurrentLocation);
                        Members.RemoveAt(i);
                        Console.WriteLine($"[Adventurer] {m.Name}???Ñ?¨Ìñà?µÎãà??");
                    }
                }
            }
        }

        private List<BaseCreature> FindAllMonstersInDungeon(Map map, Point3D loc, int radius)
        {
            List<BaseCreature> list = [];
            if (map == null || map == Map.Internal) return list;

            IPooledEnumerable eable = map.GetMobilesInRange(loc, radius);
            foreach (Mobile mob in eable)
            {
                if (mob is BaseCreature bc && !bc.Controlled && !bc.Summoned && bc.Alive && !bc.IsInvulnerable) 
                {
                    list.Add(bc);
                    if (list.Count > 20) break; 
                }
            }
            eable.Free();
            return list;
        }

        private void DistributeLoot(BaseCreature monster)
        {
            if (Members.Count == 0 || monster == null || monster.Deleted) return;

            int totalGold = (monster.HitsMax + monster.Fame) / 5; 
            PartyWealth += totalGold;

            List<Item> droppedItems = new List<Item>();
            int powerLevel = monster.HitsMax + monster.DamageMax;
            
            if (powerLevel > 500 && Utility.RandomDouble() < 0.15) droppedItems.Add(new DragonBlood(Utility.RandomMinMax(1, 3)));
            if (powerLevel > 300 && Utility.RandomDouble() < 0.25) droppedItems.Add(new DaemonBone(Utility.RandomMinMax(1, 5)));
            if (Utility.RandomDouble() < 0.05) droppedItems.Add(new Ruby());

            if (droppedItems.Count > 0)
            {
                if (EmployedSherpa != null)
                {
                    if (EmployedSherpa.Backpack == null) EmployedSherpa.EquipItem(new Backpack());
                    
                    foreach (var item in droppedItems)
                    {
                        EmployedSherpa.Backpack.DropItem(item);
                        double stressChance = 1.0 - (PackAnimals * 0.3);
                        if (Utility.RandomDouble() < stressChance)
                            EmployedSherpa.Stress = Math.Min(100, EmployedSherpa.Stress + 1); 
                    }
                }
                else
                {
                    foreach (var item in droppedItems)
                    {
                        var randomMember = Members[Utility.Random(Members.Count)];
                        if (randomMember.Backpack == null) randomMember.EquipItem(new Backpack());
                        randomMember.Backpack.DropItem(item);
                        randomMember.Stress = Math.Min(100, randomMember.Stress + 2);
                    }
                }
            }
        }

        public static AdventurerParty TryFormBalancedParty(List<VirtualAdventurer> idleList, WorldNode startNode)
        {
            var tanks = idleList.Where(a => a.Role == AdventurerRole.Tank).ToList();
            var healers = idleList.Where(a => a.Role == AdventurerRole.Healer).ToList();
            var dpsList = idleList.Where(a => a.Role == AdventurerRole.MeleeDPS || a.Role == AdventurerRole.RangedDPS || a.Role == AdventurerRole.MagicDPS).ToList();

            if (tanks.Count == 0 || healers.Count == 0 || dpsList.Count == 0) return null; 

            var party = new AdventurerParty(startNode);

            var tank = tanks[0]; party.Members.Add(tank); idleList.Remove(tank);
            var healer = healers[0]; party.Members.Add(healer); idleList.Remove(healer);
            var dps = dpsList[0]; party.Members.Add(dps); idleList.Remove(dps);

            int extraCount = Utility.RandomMinMax(0, 2); 
            for (int i = 0; i < extraCount; i++)
            {
                if (idleList.Count > 0)
                {
                    var extra = idleList.OrderByDescending(a => a.Level).First();
                    party.Members.Add(extra);
                    idleList.Remove(extra);
                }
            }

            for (int i = 0; i < party.Members.Count; i++) 
            {
                var m = party.Members[i];
                m.Party = party;
                if (m.Gold >= 500) { m.Gold -= 500; party.PartyWealth += 500; }
            }

            return party;
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(6); 
            writer.Write((int)State);
            writer.Write(TravelHoursRemaining);
            writer.Write((int)LootRule);
            writer.Write(PackAnimals); 
            writer.Write(PartyWealth);
            writer.Write(Bandages);
            writer.Write(Potions);
            writer.Write(TeamID);
            writer.Write(AcceptedJobReward);

            writer.Write(CurrentNode != null);
            if (CurrentNode != null) CurrentNode.Serialize(writer);

            writer.Write(TargetNode != null);
            if (TargetNode != null) TargetNode.Serialize(writer);

            writer.Write(Members.Count);
            foreach (var m in Members) m.Serialize(writer);
        }

        public AdventurerParty(GenericReader reader)
        {
            int version = reader.ReadInt();
            State = (AdventurerState)reader.ReadInt();
            
            if (version >= 2)
            {
                TravelHoursRemaining = reader.ReadInt();
                LootRule = (LootDistributionRule)reader.ReadInt();
            }
            if (version >= 3) PackAnimals = reader.ReadInt();
            if (version >= 4)
            {
                PartyWealth = reader.ReadInt();
                Bandages = reader.ReadInt();
                Potions = reader.ReadInt();
            }
            if (version >= 5) TeamID = reader.ReadInt();
            if (version >= 6) AcceptedJobReward = reader.ReadInt();

            if (reader.ReadBool()) CurrentNode = new WorldNode(reader);
            if (reader.ReadBool()) TargetNode = new WorldNode(reader);

            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                var adv = new VirtualAdventurer(reader) { Party = this };
                Members.Add(adv);
            }

            if (CurrentNode != null)
            {
                CurrentLocation = CurrentNode.Location;
                CurrentMap = CurrentNode.NodeMap;
            }
        }
    }

    // ==============================================================================
    // [Í∞Ä?Å Î™®ÌóòÍ∞Ä ?¥Îûò??Î≥∏Ï≤¥]
    // ==============================================================================
    public class VirtualAdventurer : VirtualAgent
    {
        public AdventurerRole Role => AdventurerProfileManager.GetProfile(this.JobClass).Role;
        public Dictionary<Layer, Type> VirtualEquipments { get; set; } = [];
        public double Potential { get; set; } = 1.0;

        public LawChaos LawChaosAlignment { get; set; }
        public GoodEvil GoodEvilAlignment { get; set; }
        public int Karma { get; set; }
        public int Fame { get; set; }
        public int Affinity { get; set; }

        public int CombatSkill { get; set; }      
        public int EquipmentTier { get; set; }    
        public int HP { get; set; }                
        public int MaxHP { get; set; }             

        public int CampingSkill { get; set; }      
        public double Experience { get; set; }    
        public double PrepMultiplier { get; set; } 

        public int FoodRations { get; set; }      
        public int HealingPotions { get; set; }   
        public int Bandages { get; set; }         
        
        public int Arrows { get; set; }
        public int Bolts { get; set; }

        public bool HasBedroll { get; set; }      
        public bool IsRestingAtInn { get; set; }  
        
        public bool HasMount { get; set; } 
		
		public int Hunger { get; set; }
        public int Thirst { get; set; }

        public AdventurerParty Party { get; set; } 
        public bool IsFemale { get; set; }

        public int Level { get; set; } = 1;
        public int Exp { get; set; } = 0;
        public NobilityRank RankLevel { get; set; }
        
        public int CombatPower => GetCombatPower();

        public VirtualAdventurer(NpcJobClass job, NobilityRank rank) : base(job, NpcRank.Novice)
        {
            this.IsFemale = Utility.RandomBool();
            string genderString = this.IsFemale ? "female" : "male";
            this.Name = NameList.RandomName(genderString);
            
            this.RankLevel = rank;
            int rankValue = (int)rank;

            MaxHP = 100 + (rankValue * 50);
            HP = MaxHP;

            CombatSkill = (rankValue + 1) * Utility.RandomMinMax(50, 100);
            CampingSkill = Utility.RandomMinMax(10, 50); 
            EquipmentTier = 1;
            Experience = 0.0;
            PrepMultiplier = 1.0; 
            
            Potential = 1.0 + (Utility.RandomDouble() * 0.5);

            FoodRations = 5;
            HealingPotions = 3;
            Bandages = 10;
            Arrows = 0;
            Bolts = 0;
            HasBedroll = true;   
            IsRestingAtInn = false;
            HasMount = false;

            // ?åü ?úÎØ??ú?§ÌÖúÍ≥??ô?ºÌïú 10Îßå ?§Ï??ºÏùò ÎßåÎ≥µÎèÑÎ°ú ?∏ÌåÖ
            Hunger = 100000;
            Thirst = 100000;

            Affinity = Utility.RandomMinMax(1, 150);
            LawChaosAlignment = (LawChaos)Utility.Random(3);
            GoodEvilAlignment = (GoodEvil)Utility.Random(3);
        }

        public VirtualAdventurer(GenericReader reader) : base(reader)
        {
            int version = reader.ReadInt();

            RankLevel = (NobilityRank)reader.ReadInt();
            IsFemale = reader.ReadBool();
            Name = reader.ReadString();
            Potential = reader.ReadDouble();
            LawChaosAlignment = (LawChaos)reader.ReadInt();
            GoodEvilAlignment = (GoodEvil)reader.ReadInt();
            Karma = reader.ReadInt();
            Fame = reader.ReadInt();
            Affinity = reader.ReadInt();
            CombatSkill = reader.ReadInt();
            EquipmentTier = reader.ReadInt();
            HP = reader.ReadInt();
            MaxHP = reader.ReadInt();
            CampingSkill = reader.ReadInt();
            Experience = reader.ReadDouble();
            PrepMultiplier = reader.ReadDouble();
            FoodRations = reader.ReadInt();
            HealingPotions = reader.ReadInt();
            Bandages = reader.ReadInt();
            HasBedroll = reader.ReadBool();
            IsRestingAtInn = reader.ReadBool();
            Level = reader.ReadInt();
            Exp = reader.ReadInt();
            
            if (version >= 1)
            {
                Arrows = reader.ReadInt();
                Bolts = reader.ReadInt();
            }

            if (version >= 2)
            {
                int equipCount = reader.ReadInt();
                for (int i = 0; i < equipCount; i++)
                {
                    Layer layer = (Layer)reader.ReadInt();
                    string typeName = reader.ReadString();
                    Type type = ScriptCompiler.FindTypeByFullName(typeName);
                    if (type != null) VirtualEquipments[layer] = type;
                }
            }
            else
            {
                int equipCount = reader.ReadInt();
                for (int i = 0; i < equipCount; i++)
                {
                    reader.ReadInt();
                    reader.ReadString();
                }
            }

            if (version >= 3) HasMount = reader.ReadBool();
            
            if (Hunger <= 100) Hunger = 100000;
            if (Thirst <= 100) Thirst = 100000;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(3); 

            writer.Write((int)RankLevel);
            writer.Write(IsFemale);
            writer.Write(Name);
            writer.Write(Potential);
            writer.Write((int)LawChaosAlignment);
            writer.Write((int)GoodEvilAlignment);
            writer.Write(Karma);
            writer.Write(Fame);
            writer.Write(Affinity);
            writer.Write(CombatSkill);
            writer.Write(EquipmentTier);
            writer.Write(HP);
            writer.Write(MaxHP);
            writer.Write(CampingSkill);
            writer.Write(Experience);
            writer.Write(PrepMultiplier);
            writer.Write(FoodRations); 
            writer.Write(HealingPotions);
            writer.Write(Bandages);
            writer.Write(HasBedroll);
            writer.Write(IsRestingAtInn);
            writer.Write(Level);
            writer.Write(Exp);
            
            writer.Write(Arrows);
            writer.Write(Bolts);

            writer.Write(VirtualEquipments.Count);
            foreach (var kvp in VirtualEquipments)
            {
                writer.Write((int)kvp.Key);
                writer.Write(kvp.Value.FullName);
            }

            writer.Write(HasMount);
        }

        public PhysicalAdventurer PhysicalObject { get; set; }

        public void ProcessSmarterShopping(TownEconomy town)
        {
            if (this.Backpack == null || this.Gold < 100) return;

            var profile = AdventurerProfileManager.GetProfile(this.JobClass);
            
            // ?åü ÎπµÏùÄ 30Í∞ú???§ÌÉù, ?òÏßÄÎßå Í≤πÏ?ÏßÄÏßÄ ?ä?î ?åÎ£å(Pitcher)?î Í∞ÄÎ∞??ú?ÑÎ•??Ñ??5Í∞úÎ°ú ?ò??Ï°∞Ï†ï
            var requirements = new List<(Type Type, int Target, int DefaultPrice)> {
                (typeof(BreadLoaf), 30, 10), 
                (typeof(Pitcher), 5, 15)
            };

            if (profile.Role == AdventurerRole.MagicDPS || profile.Role == AdventurerRole.Healer)
            {
                requirements.Add((typeof(BlackPearl), 100, 5)); 
                requirements.Add((typeof(Bloodmoss), 100, 5)); 
                requirements.Add((typeof(MandrakeRoot), 100, 5)); 
                requirements.Add((typeof(SpidersSilk), 100, 5)); 
            }
            if (profile.Role == AdventurerRole.RangedDPS)
            {
                requirements.Add((typeof(Arrow), 500, 3)); 
                requirements.Add((typeof(Bolt), 300, 3)); 
            }
            if (profile.Role == AdventurerRole.Tank || profile.Role == AdventurerRole.MeleeDPS)
            {
                requirements.Add((typeof(Bandage), 300, 5)); 
                requirements.Add((typeof(GreaterHealPotion), 20, 50)); 
                requirements.Add((typeof(GreaterCurePotion), 20, 50)); 
                requirements.Add((typeof(TotalRefreshPotion), 20, 50)); 
            }

            for (int i = 0; i < requirements.Count; i++)
            {
                var req = requirements[i];
                int currentCount = this.Backpack.GetAmount(req.Type);
                if (currentCount < req.Target)
                {
                    int buyQty = req.Target - currentCount;
                    TrySmartPurchase(town, req.Type, buyQty, req.DefaultPrice);
                }
            }

            ScanVendorsForUpgrades(town, profile);
        }

        private void SafeCreateAndDrop(Type type, int qty)
        {
            try 
            {
                Item item = (Item)Activator.CreateInstance(type);
                
                // ?åü [?®Ï?] ?åÎ£å?òÎ©??ºÎã?Î¨ºÏùÑ ÍΩâ Ï±Ñ?¥Îã?
                if (item is BaseBeverage firstBev)
                {
                    firstBev.Content = BeverageType.Water;
                    firstBev.Quantity = firstBev.MaxQuantity;
                }

                if (item.Stackable) 
                {
                    item.Amount = qty;
                    this.Backpack.DropItem(item);
                } 
                else 
                {
                    this.Backpack.DropItem(item);
                    for (int i = 1; i < qty; i++) 
                    {
                        Item extraItem = (Item)Activator.CreateInstance(type);
                        
                        // ?åü [?®Ï?] Ï™ºÍ?ÏßÑ ?òÎ®∏Ï? Î≥ë?§Ïóê?Ñ Î¨ºÏùÑ ÍΩâÍΩâ Ï±Ñ?¥Îã?
                        if (extraItem is BaseBeverage extraBev)
                        {
                            extraBev.Content = BeverageType.Water;
                            extraBev.Quantity = extraBev.MaxQuantity;
                        }
                        
                        this.Backpack.DropItem(extraItem);
                    }
                }
            } 
            catch { Console.WriteLine($"[Error] {type.Name} ?Ñ?¥ÌÖú ?ù???§Ìå?(Î™®ÌóòÍ∞Ä Î≥¥Í?)"); }
        }

        private void TrySmartPurchase(TownEconomy town, Type itemType, int qty, int defaultPrice)
        {
            int maxAcceptablePrice = (int)(defaultPrice * 1.5); 
            EconomyItemKey searchKey = new EconomyItemKey(itemType, CraftResource.None, 0, false);
            
            if (town.Warehouse.TryGetValue(searchKey, out var wItem) && wItem.Stock > 0)
            {
                int townNpcPrice = Math.Max(1, town.GetPrice(searchKey));
                if (townNpcPrice <= maxAcceptablePrice)
                {
                    int availableQty = Math.Min(qty, wItem.Stock);
                    int totalCost = availableQty * townNpcPrice;
                    
                    if (this.Gold >= totalCost)
                    {
                        this.Gold -= totalCost;
                        town.Wealth += totalCost; 
                        wItem.Stock -= availableQty; 
                        
                        SafeCreateAndDrop(itemType, availableQty);
                        
                        qty -= availableQty; 
                        if (qty <= 0) return; 
                    }
                }
            }

            var userVendorResult = SearchUserVendorsForConsumables(town.Facet, itemType, qty, maxAcceptablePrice);
            if (userVendorResult.Found)
            {
                CompletePurchase(userVendorResult.Vendor, userVendorResult.Item, qty, userVendorResult.TotalPrice);
            }
        }

        private void CompletePurchase(Mobile vendor, Item item, int qty, int price)
        {
            if (this.Gold < price) return;
            this.Gold -= price;
            if (vendor is PlayerVendor pv) pv.HoldGold += price;
            
            Type typeToCreate = item.GetType(); 
            
            if (item.Stackable && item.Amount > qty) item.Amount -= qty; 
            else item.Delete();

            SafeCreateAndDrop(typeToCreate, qty);
        }

        private void ScanVendorsForUpgrades(TownEconomy town, CombatProfile profile)
        {
            int budget = (int)(this.Gold * 0.8); 
            bool foundMagicGear = false;

            if (PlayerVendor.PlayerVendors != null)
            {
                foreach (var vendor in PlayerVendor.PlayerVendors.Where(v => v.Map == town.Facet))
                {
                    if (vendor.Backpack == null) continue;

                    foreach (var item in vendor.Backpack.Items.ToList()) 
                    {
                        if (!(item is BaseWeapon || item is BaseArmor || item is BaseJewel)) continue;

                        var vi = vendor.GetVendorItem(item);
                        if (vi == null || vi.Price <= 0 || vi.Price > budget) continue;

                        double itemValueScore = EvaluateItemScore(item, profile);
                        
                        if (itemValueScore > (this.CombatPower * 0.15) || itemValueScore >= 50.0) 
                        {
                            Layer targetLayer = item.Layer;
                            if (targetLayer == Layer.Invalid && item is BaseWeapon bw) 
                                targetLayer = bw.Layer; 

                            if (profile.RequiredLayers.Contains(targetLayer))
                            {
                                int price = vi.Price;
                                CompletePurchase(vendor, item, 1, price);
                                
                                VirtualEquipments[targetLayer] = item.GetType();
                                this.CombatSkill += 1; 
                                this.EquipmentTier = Math.Max(this.EquipmentTier, GetRarityLevel(item));
                                budget -= price;

                                Console.WriteLine($"[Spec-Up] Î™®ÌóòÍ∞Ä {this.Name}??Í∞Ä) ?Å?ê?ê?ú {item.GetType().Name} Íµ¨Îß§! (ÏßÄÏ∂ú: {price}gp)");
                                foundMagicGear = true;
                                
                                if (budget < 1000) return; 
                            }
                        }
                    }
                }
            }

            if (!foundMagicGear)
            {
                for (int i = 0; i < profile.RequiredLayers.Length; i++)
                {
                    Layer requiredLayer = profile.RequiredLayers[i];
                    if (!VirtualEquipments.ContainsKey(requiredLayer))
                    {
                        Type fallbackItem = GetFallbackItemForLayer(requiredLayer, profile.Role);
                        if (fallbackItem != null) 
                        {
                            EconomyItemKey searchKey = new EconomyItemKey(fallbackItem, CraftResource.None, 0, false);
                            
                            if (this.Gold > 2000)
                            {
                                EconomyItemKey excKey = new EconomyItemKey(fallbackItem, CraftResource.None, 0, true);
                                if (town.Warehouse.ContainsKey(excKey) && town.Warehouse[excKey].Stock > 0)
                                    searchKey = excKey;
                            }

                            int npcPrice = Math.Max(200, town.GetPrice(searchKey)); 

                            if (this.Gold >= npcPrice && budget >= npcPrice)
                            {
                                this.Gold -= npcPrice;
                                town.Wealth += npcPrice; 
                                
                                if (town.Warehouse.ContainsKey(searchKey))
                                {
                                    town.Warehouse[searchKey].Stock--;
                                    if (town.Warehouse[searchKey].Stock <= 0) town.Warehouse.Remove(searchKey);
                                }

                                VirtualEquipments[requiredLayer] = searchKey.ItemType;
                                break; 
                            }
                        }
                    }
                }
            }
        }

        private double EvaluateItemScore(Item item, CombatProfile profile)
        {
            double score = 0;
            for (int i = 0; i < profile.PreferredOptions.Length; i++)
            {
                int val = GetOptionValue(item, profile.PreferredOptions[i]); 
                if (val > 0) score += val * 2.0; 
            }
            
            int rarity = GetRarityLevel(item); 
            score *= (1.0 + (rarity * 0.25)); 

            return score;
        }

        private (bool Found, Item Item, Mobile Vendor, int TotalPrice) SearchUserVendorsForConsumables(Map map, Type type, int qty, int maxPrice)
        {
            foreach (var v in PlayerVendor.PlayerVendors.Where(v => v.Map == map && v.Backpack != null))
            {
                var item = v.Backpack.FindItemByType(type);
                if (item != null) 
                { 
                    var vi = v.GetVendorItem(item); 
                    if (vi != null && vi.Price <= maxPrice) 
                        return (true, item, v, vi.Price * qty); 
                }
            }
            return (false, null, null, 0);
        }

        private int GetOptionValue(Item item, int optionID) { return 5; }
        private int GetRarityLevel(Item item) 
        { 
            var (res, _, isExc) = VirtualTradeSystem.GetResourceAndQuality(item);
            int tier = CraftResources.GetIndex(res) + 1;
            if (tier <= 0) tier = 1;
            if (isExc) tier++;
            return tier; 
        }

        private int GetCombatPower()
        {
            var profile = AdventurerProfileManager.GetProfile(this.JobClass);
            double optionMultiplier = 1.0 + (profile.PreferredOptions.Length * 0.1); 
            
            double basePower = (CombatSkill * 1.5) + (EquipmentTier * 60);
            return (int)(basePower * Potential * optionMultiplier);
        }

        public void EquipMissingLayers(TownEconomy town)
        {
            var profile = AdventurerProfileManager.GetProfile(this.JobClass);
            for (int i = 0; i < profile.RequiredLayers.Length; i++)
            {
                Layer requiredLayer = profile.RequiredLayers[i];
                if (VirtualEquipments.ContainsKey(requiredLayer)) continue;

                Type fallbackItem = GetFallbackItemForLayer(requiredLayer, profile.Role);
                if (fallbackItem != null) 
                {
                    VirtualEquipments[requiredLayer] = fallbackItem;
                }
            }
        }

        private Type GetFallbackItemForLayer(Layer layer, AdventurerRole role)
        {
            bool isMage = role == AdventurerRole.Healer || role == AdventurerRole.MagicDPS;
            bool isRanged = role == AdventurerRole.RangedDPS;
            bool isTank = role == AdventurerRole.Tank;

            int armorSetID = isTank ? 17 : (isMage ? 7 : (isRanged ? 4 : 1)); 
            int jewelrySetID = isMage ? 20 : 19;

            Type itemType = GetSetItemForLayer(armorSetID, layer) ?? GetSetItemForLayer(jewelrySetID, layer);

            return itemType ?? layer switch
            {
                Layer.OuterTorso => isMage ? typeof(Robe) : null,
                Layer.Shoes => typeof(Boots),
                Layer.OneHanded => isMage ? typeof(Spellbook) : (isRanged ? null : (isTank ? typeof(Broadsword) : typeof(Longsword))),
                Layer.TwoHanded => isRanged ? (Utility.RandomBool() ? typeof(Bow) : typeof(Crossbow)) : (isTank ? typeof(MetalKiteShield) : (isMage ? typeof(GnarledStaff) : typeof(Halberd))),
                _ => null
            };
        }

        private Type GetSetItemForLayer(int setID, Layer layer) => setID switch
        {
            1 => layer switch { Layer.Helm => typeof(Circlet), Layer.Neck => typeof(LeafGorget), Layer.InnerTorso => typeof(LeafChest), Layer.Arms => typeof(LeafArms), Layer.Gloves => typeof(LeafGloves), Layer.Pants => typeof(LeafLegs), _ => null },
            4 => layer switch { Layer.Helm => typeof(LeatherCap), Layer.Neck => typeof(LeatherGorget), Layer.InnerTorso => typeof(LeatherChest), Layer.Arms => typeof(LeatherArms), Layer.Gloves => typeof(LeatherGloves), Layer.Pants => typeof(LeatherLegs), _ => null },
            7 => layer switch { Layer.Helm => typeof(BoneHelm), Layer.InnerTorso => typeof(BoneChest), Layer.Arms => typeof(BoneArms), Layer.Gloves => typeof(BoneGloves), Layer.Pants => typeof(BoneLegs), _ => null },
            17 => layer switch { Layer.Helm => typeof(PlateHelm), Layer.Neck => typeof(PlateGorget), Layer.InnerTorso => typeof(PlateChest), Layer.Arms => typeof(PlateArms), Layer.Gloves => typeof(PlateGloves), Layer.Pants => typeof(PlateLegs), _ => null },
            19 => layer switch { Layer.Ring => typeof(GoldRing), Layer.Bracelet => typeof(GoldBracelet), Layer.Neck => typeof(GoldNecklace), Layer.Earrings => typeof(GoldEarrings), _ => null },
            20 => layer switch { Layer.Ring => typeof(SilverRing), Layer.Bracelet => typeof(SilverBracelet), Layer.Neck => typeof(SilverNecklace), Layer.Earrings => typeof(SilverEarrings), _ => null },
            _ => null
        };

        public int GetAffinityDistance(VirtualAdventurer other)
        {
            int diff = Math.Abs(this.Affinity - other.Affinity);
            return diff > 75 ? 150 - diff : diff;
        }

        public DateTime BirthTime { get; set; } = DateTime.Now - TimeSpan.FromMinutes(20 * VirtualCitizen.GameYearMinutes);
        public double Age => (DateTime.Now - BirthTime).TotalMinutes / VirtualCitizen.GameYearMinutes;

        public (bool IsRetiring, NobilityRank NewRank) CheckRetirement()
        {
            double currentAge = (DateTime.Now - BirthTime).TotalMinutes / VirtualCitizen.GameYearMinutes;

            if (currentAge >= 60.0)
            {
                if (Utility.RandomDouble() < 0.20)
                {
                    Console.WriteLine($"[?ù?†Ï£ºÍ∏∞] Î™®ÌóòÍ∞Ä {this.Name}({Math.Floor(currentAge)}??Í∞Ä ?Ä?¥Ìïò???úÎØºÏù??©Îãà??");
                    NobilityRank rank = Fame > 10000 ? NobilityRank.Knight : NobilityRank.Commoner; 
                    return (true, rank);
                }
            }

            if (this.Karma > 5000 && this.Gold > 100000)
            {
                double retireChance = (this.Karma / 10000.0) + (this.Potential > 2.0 ? 0.2 : 0);
                if (Utility.RandomDouble() < retireChance) return (true, NobilityRank.Knight);
            }
            return (false, NobilityRank.Commoner);
        }

        public void RetireToCitizen(TownEconomy town, NobilityRank newRank)
        {
            if (this.Party != null) this.Party.Members.Remove(this);
            VirtualCitizen citizen = new VirtualCitizen(this.JobClass, newRank, 100);
            citizen.Gold = this.Gold;
            town.Citizens.Add(citizen);
        }

        public (bool Success, int RepairCost) TryRepairEquipment(TownEconomy town)
        {
            int repairCost = 100; 
            var deedResult = SearchForRepairDeed(town);
            
            if (deedResult.Found && this.Gold >= deedResult.Cost)
            {
                this.Gold -= deedResult.Cost;
                return (true, deedResult.Cost);
            }
            
            if (this.Gold >= repairCost)
            {
                this.Gold -= repairCost;
                town.Wealth += repairCost; 
                return (true, repairCost);
            }
            return (false, 0);
        }

        private (bool Found, int Cost) SearchForRepairDeed(TownEconomy town)
        {
            var map = town.Facet;
            if (map == null || map == Map.Internal) return (false, 0);

            int maxAcceptablePrice = EquipmentTier * 5000;

            if (PlayerVendor.PlayerVendors != null)
            {
                foreach (var vendor in PlayerVendor.PlayerVendors.Where(v => v != null && v.Map == map && !v.Deleted && v.Backpack != null))
                {
                    foreach (var item in vendor.Backpack.Items)
                    {
                        if (item.GetType().Name.Contains("RepairDeed"))
                        {
                            var vi = vendor.GetVendorItem(item);
                            if (vi != null && vi.Price > 0 && vi.Price <= maxAcceptablePrice && this.Gold >= vi.Price)
                            {
                                int cost = vi.Price;
                                vendor.HoldGold += cost;
                                item.Delete();
                                return (true, cost);
                            }
                        }
                    }
                }
            }
            return (false, 0); 
        }

        // ?åü [?µÏã??ò?ï 1] ???ç?Ñ(30Î∂Ñ?ê 1Î≤à ?∞ÏÇ??ê ÎßûÏ∂?ÎßåÎ≥µÎèÑ Í∞ê?å ?§Ï??ºÏùÑ ?Ä?≠ ?å?ë (?åÎπÑ ?≠Î∞ú ?†?Ñ)
        public void UpdateSurvivalTick()
        {
            int metabolism = (Party != null && Party.State == AdventurerState.Exploring) ? 10000 : 2000;
            
            this.Hunger = Math.Max(0, this.Hunger - metabolism);
            this.Thirst = Math.Max(0, this.Thirst - metabolism);

            if (this.Hunger < 40000)
            {
                if (ConsumeItemFromBackpack(typeof(BreadLoaf)))
                {
                    this.Hunger = Math.Min(100000, this.Hunger + 30000);
                    this.HP = Math.Min(MaxHP, HP + (MaxHP / 10)); 
                    this.Stress = Math.Max(0, this.Stress - 2);
                }
                else if (this.Hunger == 0)
                {
                    this.HP -= 2;        
                    this.Stress += 5;    
                }
            }

            if (this.Thirst < 40000)
            {
                if (ConsumeItemFromBackpack(typeof(Pitcher)) || ConsumeItemFromBackpack(typeof(BeverageBottle)))
                {
                    this.Thirst = Math.Min(100000, this.Thirst + 30000);
                }
                else if (this.Thirst == 0)
                {
                    this.HP -= 2;
                    this.Stress += 5;
                }
            }

            if (Party != null && Party.State == AdventurerState.Resting)
            {
                this.HP = Math.Min(MaxHP, HP + 20);
                this.Stress = Math.Max(0, Stress - 5);
            }

            GainExp(25); 

            this.HP = Math.Clamp(HP, 0, MaxHP);
            this.Stress = Math.Clamp(this.Stress, 0, 100);

            if (this.HP <= 0 && Party == null) Die(); 
        }

        // ?åü [Ï∂îÍ∞Ä] Í∞ÄÎ∞??à?ò ?Ñ?¥ÌÖú?Ñ ?§Ï†úÎ°ú Ï∞æÏïÑ ?åÎ™®Ïãú?§Îäî Î°úÏßÅ
        private bool ConsumeItemFromBackpack(Type itemType)
        {
            if (this.Backpack == null) return false;
            
            // 1. Îßà??Í≤É(?åÎ£åÎ•ò)??Í≤ΩÏö??§ÌÉù(Amount)???Ñ?à???¥Ïö©Î¨º(Quantity)?Ñ ?åÎ™?
            if (typeof(BaseBeverage).IsAssignableFrom(itemType))
            {
                // ?¥Ïö©Î¨º??1 ?¥ÏÉÅ ?®ÏïÑ?à?î ?åÎ£åÎßå Í∞ÄÎ∞©Ïóê?ú Ï∞æÏùå
                var bev = this.Backpack.Items.OfType<BaseBeverage>().FirstOrDefault(b => b.GetType() == itemType && b.Quantity > 0);
                if (bev != null)
                {
                    bev.Quantity--; // ?¥Ïö©Î¨º ?ú Î™®Í?(1?î) Îßà??
                    
                    // ??Îßà?î?ú Îπà Î≥ë/Ï£ºÏ†Ñ?êÍ∞Ä ?òÎ©?Í∞ÄÎ∞?Ïπ??≠ÎπÑÎ•?ÎßâÍ∏??Ñ???åÍ¥?
                    if (bev.Quantity <= 0) bev.Delete(); 
                    return true;
                }
                return false;
            }

            // 2. ?ºÎ? ?Ñ?¥ÌÖú (Îπ? Î∂ï?Ä, ?¨ÏÖò ??
            Item item = this.Backpack.FindItemByType(itemType);
            if (item != null)
            {
                if (item.Amount > 1) item.Amount--;
                else item.Delete();
                return true;
            }
            return false;
        }

        public int GetRequiredExp() => (Level * Level * 50) + (Level * 100);

        public void GainExp(int amount)
        {
            if (Level >= 100) return; 

            int finalExp = (int)(amount * Potential); 
            this.Exp += finalExp;

            while (this.Exp >= GetRequiredExp() && Level < 100)
            {
                this.Exp -= GetRequiredExp();
                LevelUp();
            }
        }

        private void LevelUp()
        {
            Level++;
            MaxHP += Utility.RandomMinMax(5, 10) + (int)RankLevel;
            HP = MaxHP; 
            CombatSkill += Utility.RandomMinMax(1, 3);
            Stress = Math.Max(0, Stress - 50); 
            
            Potential += 0.01; 
        }

        public void CheckSkillGain(string skill, double chance)
        {
            if (Utility.RandomDouble() < chance)
            {
                if (skill == "Combat" && CombatSkill < 100) CombatSkill++;
                else if (skill == "Camping" && CampingSkill < 100) CampingSkill++;
                Experience += 0.5; 
            }
        }

        public void Die()
        {
            if (Party != null && Party.CurrentMap != null)
                SpawnAdventurerChest(Party.CurrentMap, Party.CurrentLocation);
            
            if (this.PhysicalObject != null && !this.PhysicalObject.Deleted)
            {
                this.PhysicalObject.Delete();
                this.PhysicalObject = null;
            }

            if (Party != null) Party.Members.Remove(this);
        }

        public void SpawnAdventurerChest(Map map, Point3D loc)
        {
            if (map == null || map == Map.Internal) return;

            // ?åü [?∏Îìú ?àÏßÄ?§Ìä∏Î¶¨ V2 ?∞Îèô] 
            // Í∏∞Ï°¥?ò ?®Ïàú ?Å?ê ?ù??Î∞è ?≠?ú ÏΩî?úÎ•?Î™®Îëê ?†Î¶¨Í?, 
            // ?ò?Ñ ?∏Îìú Í∏∞Î??ò '?Å?ê Î∞ÄÏßë?Ñ Ï≤¥ÌÅ?Î∞è ?®Ï†ï/?±Í? ?ÖÍ∑∏Î†à?¥Îìú' Î°úÏßÅ?ºÎ? ?ú?¥Í??Ñ ?òÍπÅ?à??
            
            int carryGold = this.Gold / 2; // Î™®ÌóòÍ∞ÄÍ∞Ä ?§Í? ?à?ò Í≥®Îìú?ò ?àÎ∞ò?Ñ ?†?à?ºÎ? ?§Ï†ï

            Server.Misc.AdventurerChestManager.ProcessAdventurerDeath(this.PhysicalObject, loc, map, carryGold);
        }
    }
}
