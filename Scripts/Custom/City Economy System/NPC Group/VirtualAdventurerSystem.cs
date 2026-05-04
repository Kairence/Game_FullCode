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
    // [기초 Enum 및 구조체 - 모험가 전용]
    // ==============================================================================
    public enum LawChaos { Lawful, Neutral, Chaotic }
    public enum GoodEvil { Good, Neutral, Evil }
    public enum AdventurerState { Resting, Traveling, Exploring }
    public enum WorldNodeType { Town, Dungeon, Ruin }
    public enum AdventurerRole { Tank, MeleeDPS, RangedDPS, MagicDPS, Healer, Support }
    public enum LootDistributionRule { Equal, Contribution }
    
    public enum AdventurerAiType { Melee, Archer, Mage, Paladin, Necro }

    public record CombatProfile(AdventurerRole Role, double HpWeight, double MpWeight, double SpWeight, int[] PreferredOptions, params Layer[] RequiredLayers);

    // 🌟 [유저 기획] 퇴각 루트 정보 구조체 (도착 마을, 섬 여부, 기준 거리)
    public record RetreatRoute(RegionCode TownCode, bool IsIsland, int BaseDistance);

    public static class DungeonRetreatManager
    {
        public static readonly Dictionary<RegionCode, RetreatRoute> Map = new()
        {
            // --- Trammel Dungeons ---
            { RegionCode.Trammel_Dungeon_Covetous, new(RegionCode.Trammel_Town_Minoc, false, 800) },       
            { RegionCode.Trammel_Dungeon_Deceit, new(RegionCode.Trammel_Town_Moonglow, true, 1200) },      
            { RegionCode.Trammel_Dungeon_Despise, new(RegionCode.Trammel_Town_Britain, false, 600) },      
            { RegionCode.Trammel_Dungeon_Destard, new(RegionCode.Trammel_Town_SkaraBrae, false, 900) },    
            { RegionCode.Trammel_Dungeon_Hythloth, new(RegionCode.Trammel_Town_Magincia, true, 1500) },    
            { RegionCode.Trammel_Dungeon_Shame, new(RegionCode.Trammel_Town_Yew, false, 1100) },           
            { RegionCode.Trammel_Dungeon_Wrong, new(RegionCode.Trammel_Town_Minoc, false, 1000) },         
            { RegionCode.Trammel_Dungeon_Fire, new(RegionCode.Trammel_Town_SerpentsHold, true, 500) },     
            { RegionCode.Trammel_Dungeon_Ice, new(RegionCode.Trammel_Town_Vesper, false, 1300) },          
            { RegionCode.Trammel_Dungeon_OrcCave, new(RegionCode.Trammel_Town_Cove, false, 400) },         
            { RegionCode.Trammel_Dungeon_PaintedCaves, new(RegionCode.Trammel_Town_Trinsic, false, 700) },
            { RegionCode.Trammel_Dungeon_PalaceOfParoxysmus, new(RegionCode.Trammel_Town_Papua, false, 800) },
            { RegionCode.Trammel_Dungeon_PrismOfLight, new(RegionCode.Trammel_Town_Nujelm, true, 1000) },
            { RegionCode.Trammel_Dungeon_Sanctuary, new(RegionCode.Trammel_Town_Yew, false, 600) },
            { RegionCode.Trammel_Dungeon_SolenHives, new(RegionCode.Trammel_Town_Minoc, false, 700) },
            
            // --- Felucca Dungeons ---
            { RegionCode.Felucca_Dungeon_Covetous, new(RegionCode.Felucca_Town_Minoc, false, 800) },
            { RegionCode.Felucca_Dungeon_Deceit, new(RegionCode.Felucca_Town_Moonglow, true, 1200) },
            { RegionCode.Felucca_Dungeon_Despise, new(RegionCode.Felucca_Town_Britain, false, 600) },
            { RegionCode.Felucca_Dungeon_Destard, new(RegionCode.Felucca_Town_SkaraBrae, false, 900) },
            { RegionCode.Felucca_Dungeon_Hythloth, new(RegionCode.Felucca_Town_Magincia, true, 1500) },
            { RegionCode.Felucca_Dungeon_Shame, new(RegionCode.Felucca_Town_Yew, false, 1100) },
            { RegionCode.Felucca_Dungeon_Wrong, new(RegionCode.Felucca_Town_Minoc, false, 1000) },
            { RegionCode.Felucca_Dungeon_Fire, new(RegionCode.Felucca_Town_SerpentsHold, true, 500) },
            { RegionCode.Felucca_Dungeon_Ice, new(RegionCode.Felucca_Town_Vesper, false, 1300) },
            { RegionCode.Felucca_Dungeon_OrcCave, new(RegionCode.Felucca_Town_Yew, false, 400) },
            { RegionCode.Felucca_Dungeon_Khaldun, new(RegionCode.Felucca_Town_Minoc, false, 1500) }
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
    // [가상 모험가 매니저]
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
                        Console.WriteLine("[Adventurer] 파티가 전멸하여 해산되었습니다.");
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
                        Console.WriteLine($"[Adventurer] {town.Name} 마을이 {spawnCost}gp로 용병 {spawnAmount}명을 고용했습니다. (마을 TO: {advInTown + spawnAmount}/{townAdvCap})");
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
                    Console.WriteLine(" [주의] 모험가 세이브 파일 손상 감지! 꼬인 데이터를 파기하고 초기화합니다.");
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

            Console.WriteLine("[Adventurer] 가상 모험가 시스템 데이터 로드 완료.");
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
    // [월드 노드 (이동 거점)]
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
    // [가상 모험가 파티 시스템]
    // ==============================================================================
    public class AdventurerParty
    {
        public string Name => Members.Count > 0 ? $"{Members[0].Name} 파티" : "무명의 파티";
        public List<VirtualAdventurer> Members { get; set; } = [];
        public AdventurerState State { get; set; }
        public WorldNode CurrentNode { get; set; }
        public WorldNode TargetNode { get; set; }
        public Point3D CurrentLocation { get; set; }
        public Map CurrentMap { get; set; }
        
        public int PartyWealth { get; set; }
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

                Console.WriteLine($"[Quest] {Members[0].Name} 파티가 의뢰를 완수하고 {AcceptedJobReward}gp를 획득했습니다.");
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
                    Console.WriteLine($"[Adventurer] 셰르파 {EmployedSherpa.Name}이(가) 던전 길잡이 및 물자 관리 수당으로 {sherpaCut}gp를 받았습니다.");
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
            var town = TownEconomyManager.Towns.Values.OrderBy(t => Utility.GetDistanceToSqrt(CurrentLocation, t.Center)).FirstOrDefault();
            
            if (town != null)
            {
                SettleTownReturn(town); 

                int innFee = Members.Count * 20;
                if (PartyWealth >= innFee) 
                { 
                    PartyWealth -= innFee; 
                    VirtualAdventurerManager.PayToCitizenOrTown(town, innFee, NpcJobClass.InnKeeper, NpcJobClass.Barmaid); 
                }

                if (PartyWealth > 2000) TryHireSherpa(town); 

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
                    Console.WriteLine($"[Adventurer] {Members[0].Name} 파티가 든든하게 전원 승마를 마쳤습니다.");
                }

                int targetBandages = 100 + (Members.Count * 25);
                int targetPotions = 40 + (Members.Count * 10);

                if (EmployedSherpa != null)
                {
                    targetBandages = 300; 
                    targetPotions = 150;

                    if (PartyWealth > 5000 && PackAnimals < 3)
                    {
                        int animalCost = 1000;
                        PartyWealth -= animalCost;
                        town.Wealth += animalCost;
                        PackAnimals++;
                        Console.WriteLine($"[Adventurer] {Members[0].Name} 파티가 원정을 위해 짐말을 추가 구매했습니다. (현재: {PackAnimals}마리)");
                    }

                    targetBandages += (PackAnimals * 300);
                    targetPotions += (PackAnimals * 150);
                }

                if (Bandages < targetBandages && PartyWealth > 100) 
                { 
                    int needed = targetBandages - Bandages;
                    int buyQty = Math.Min(needed, PartyWealth / 10); 
                    PartyWealth -= (buyQty * 10); 
                    town.Wealth += (buyQty * 10);
                    Bandages += buyQty; 
                }
                
                if (Potions < targetPotions && PartyWealth > 500) 
                { 
                    int needed = targetPotions - Potions;
                    int buyQty = Math.Min(needed, PartyWealth / 50); 
                    PartyWealth -= (buyQty * 50); 
                    town.Wealth += (buyQty * 50);
                    Potions += buyQty; 
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
                    
                    var questDz = DungeonManager.ZoneList.FirstOrDefault(z => z.RCode.ToString() == acceptedJob.RegionName);
                    if (questDz != null && questDz.Nodes.Count > 0)
                    {
                        Point3D dest = questDz.Nodes[0].Location;
                        TargetNode = new WorldNode(questDz.ZoneId, questDz.RCode, WorldNodeType.Dungeon, questDz.Facet, dest, dest, questDz.CurrentDifficulty);
                        this.AcceptedJobReward = acceptedJob.RewardGold; 
                        
                        Console.WriteLine($"[Quest] {Members[0].Name} 파티가 방치된 '{acceptedJob.Title}' 의뢰를 수락하고 출발 준비를 마쳤습니다.");
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
                        
                        Console.WriteLine($"[Adventurer] 초보인 {Members[0].Name} 파티가 호위를 위해 이웃 마을인 {TargetNode.Name}(으)로 향하기로 했습니다.");
                    }
                }
                else
                {
                    var validDungeons = DungeonManager.ZoneList.Where(z => z.Nodes.Count > 0).ToList();
                    if (validDungeons.Count > 0)
                    {
                        int targetDiff = (int)(AverageLevel * 200);
                        var suitableDungeons = validDungeons
                            .OrderBy(z => Math.Abs(z.CurrentDifficulty - targetDiff))
                            .Take(3) 
                            .ToList();

                        if (suitableDungeons.Count > 0)
                        {
                            var targetDz = suitableDungeons[Utility.Random(suitableDungeons.Count)];
                            Point3D dest = targetDz.Nodes[0].Location;
                            TargetNode = new WorldNode(targetDz.ZoneId, targetDz.RCode, WorldNodeType.Dungeon, targetDz.Facet, dest, dest, targetDz.CurrentDifficulty);
                            
                            Console.WriteLine($"[Adventurer] {Members[0].Name} 파티가 자율 사냥을 위해 {TargetNode.Name}(으)로 출발 준비를 마쳤습니다.");
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
                    
                    Console.WriteLine($"[Travel] {Members[0].Name} 파티가 {CurrentNode.Name}에서 {TargetNode.Name}(으)로 이동을 시작합니다. (비용: {plan.TotalCost}gp / 소요: {plan.TotalTicks}틱)");
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
                        Console.WriteLine($"[Travel] {Members[0].Name} 파티가 비정규 지역인 {TargetNode.Name}(으)로 탐험을 떠납니다. ({ticks}틱 소요)");
                    }
                    else
                    {
                        Console.WriteLine($"[Travel] {Members[0].Name} 파티가 자금 부족으로 발이 묶였습니다.");
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
                        Console.WriteLine($"[Adventurer] {Members[0].Name} 파티가 {CurrentNode.Name}에 무사히 도착하여 호위 수당 {patrolReward}gp를 받았습니다.");
                    }
                }

                State = CurrentNode.Type == WorldNodeType.Dungeon ? AdventurerState.Exploring : AdventurerState.Resting;
                
                if (CurrentNode.Type == WorldNodeType.Dungeon)
                    Console.WriteLine($"[Adventurer] {Members[0].Name} 파티가 던전 {CurrentNode.Name}에 진입했습니다.");
            }
        }

        private void ProcessExploring()
        {
            double efficiency = 1.0 - (PackAnimals * 0.15); 
            
            if (EmployedSherpa != null) efficiency -= 0.20;

            int bandageConsume = (int)Math.Max(1, Members.Count * efficiency * 1.5);
            int potionConsume = (int)Math.Max(1, (Members.Count / 2) * efficiency * 0.5);

            Bandages = Math.Max(0, Bandages - bandageConsume);
            Potions = Math.Max(0, Potions - potionConsume);

            bool needsRetreat = false;
            if (Bandages < 10 || Potions < 5) needsRetreat = true;
            else
            {
                for (int i = 0; i < Members.Count; i++)
                {
                    if (Members[i].HP < Members[i].MaxHP * 0.3 || Members[i].Stress > 90)
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

            if (isPhysicalActive)
            {
                return; 
            }

            BatchCombatTick();
        }

        // ====================================================================
        // 🌟 [유저 기획 완벽 반영] 섬과 내륙을 완벽히 분리한 생존 퇴각 로직
        // ====================================================================
        private void RetreatToTown()
        {
            RegionCode majorDungeonCode = RegionSaver.GetMajorCode(CurrentNode.RCode);
            
            if (!DungeonRetreatManager.Map.TryGetValue(majorDungeonCode, out RetreatRoute route))
            {
                TargetNode = GetFallbackNearestTown();
                if (TargetNode == CurrentNode || TargetNode == null)
                {
                    Console.WriteLine($"[Adventurer] {Members[0].Name} 파티가 오지에서 조난당했습니다!");
                    this.State = AdventurerState.Resting;
                    return;
                }
                int fallbackDist = (int)Utility.GetDistanceToSqrt(CurrentLocation, TargetNode.Location);
                route = new RetreatRoute(TargetNode.RCode, false, fallbackDist);
            }
            else
            {
                var targetTown = TownEconomyManager.Towns.Values.FirstOrDefault(t => 
                    RegionSaver.GetRegionCodes(t.Facet, t.Center.X, t.Center.Y, t.Center.Z).Major == route.TownCode);

                if (targetTown != null)
                {
                    string safeName = targetTown.TownName ?? targetTown.Name ?? "지정 대피소";
                    TargetNode = new WorldNode(safeName, route.TownCode, WorldNodeType.Town, targetTown.Facet, targetTown.Center, targetTown.Center, 1);
                }
                else
                {
                    Console.WriteLine($"[Adventurer] {Members[0].Name} 파티가 대피소를 찾지 못해 조난당했습니다.");
                    this.State = AdventurerState.Resting;
                    return;
                }
            }

            int ferryCost = route.IsIsland ? 500 * Members.Count : 0;
            bool allMounted = true;
            for (int i = 0; i < Members.Count; i++)
            {
                if (!Members[i].HasMount) { allMounted = false; break; }
            }

            var townToPay = TownEconomyManager.Towns.Values.FirstOrDefault(t => 
                RegionSaver.GetRegionCodes(t.Facet, t.Center.X, t.Center.Y, t.Center.Z).Major == route.TownCode);

            if (route.IsIsland)
            {
                if (PartyWealth >= ferryCost)
                {
                    PartyWealth -= ferryCost;
                    if (townToPay != null) townToPay.Wealth += ferryCost;

                    this.TravelHoursRemaining = Math.Max(1, route.BaseDistance / 300); 
                    this.State = AdventurerState.Traveling;
                    Console.WriteLine($"[Adventurer] {Members[0].Name} 파티가 배삯 {ferryCost}gp를 내고 {TargetNode.Name}(으)로 해상 퇴각합니다. ({TravelHoursRemaining}틱 소요)");
                }
                else
                {
                    Console.WriteLine($"[Adventurer] {Members[0].Name} 파티가 {TargetNode.Name}행 배삯({ferryCost}gp)이 없어 던전에 배수진을 칩니다! (사망 위기)");
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
                    Console.WriteLine($"[Adventurer] {Members[0].Name} 파티가 여비를 지불하고 {TargetNode.Name}(으)로 퇴각합니다. ({TravelHoursRemaining}틱 소요)");
                }
                else
                {
                    int ticks = Math.Max(2, (route.BaseDistance / 300) * 3); 
                    
                    this.TravelHoursRemaining = ticks;
                    this.State = AdventurerState.Traveling;
                    Console.WriteLine($"[Adventurer] {Members[0].Name} 파티가 무일푼으로 {TargetNode.Name}까지 맨몸 행군을 강행합니다! ({TravelHoursRemaining}틱 소요 - 아사 위험)");
                }
            }
        }

        private WorldNode GetFallbackNearestTown()
        {
            var town = TownEconomyManager.Towns.Values
                .Where(t => t.Facet == CurrentMap && t.VendorCount > 0)
                .OrderBy(t => Utility.GetDistanceToSqrt(CurrentLocation, t.Center))
                .FirstOrDefault();

            if (town != null)
            {
                var tCode = RegionSaver.GetRegionCodes(town.Facet, town.Center.X, town.Center.Y, town.Center.Z).Major;
                return new WorldNode(town.TownName ?? town.Name ?? "인근 마을", tCode, WorldNodeType.Town, town.Facet, town.Center, town.Center, 1);
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
            Bandages = Math.Max(0, Bandages - extraSupply);

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
                        if (town != null) VirtualAdventurerManager.PayToCitizenOrTown(town, 1000, NpcJobClass.Healer_Master, NpcJobClass.Priest, NpcJobClass.Surgeon_Relig);
                        
                        Console.WriteLine($"[Adventurer] {m.Name}이(가) 치명상을 입었으나 파티 자금으로 구조했습니다.");
                    }
                    else
                    {
                        m.SpawnAdventurerChest(CurrentMap, CurrentLocation);
                        Members.RemoveAt(i);
                        Console.WriteLine($"[Adventurer] {m.Name}이 전사했습니다.");
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
    // [가상 모험가 클래스 본체]
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
            
            var requirements = new List<(Type Type, int Target, int DefaultPrice)> {
                (typeof(BreadLoaf), 5, 10), 
                (typeof(Pitcher), 5, 15)
            };

            if (profile.Role == AdventurerRole.MagicDPS || profile.Role == AdventurerRole.Healer)
                requirements.Add((typeof(BlackPearl), 50, 10)); 
            if (profile.Role == AdventurerRole.RangedDPS)
                requirements.Add((typeof(Arrow), 200, 3)); 
            if (profile.Role == AdventurerRole.Tank || profile.Role == AdventurerRole.MeleeDPS)
                requirements.Add((typeof(Bandage), 100, 5)); 

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
                if (item.Stackable) 
                {
                    item.Amount = qty;
                    this.Backpack.DropItem(item);
                } 
                else 
                {
                    this.Backpack.DropItem(item);
                    for (int i = 1; i < qty; i++) 
                        this.Backpack.DropItem((Item)Activator.CreateInstance(type));
                }
            } 
            catch { Console.WriteLine($"[Error] {type.Name} 아이템 생성 실패 (모험가 보급)"); }
        }

        private void TrySmartPurchase(TownEconomy town, Type itemType, int qty, int defaultPrice)
        {
            int maxAcceptablePrice = (int)(defaultPrice * 1.5); 
            
            int townNpcPrice = town.GetPrice(itemType);
            if (townNpcPrice <= 0) townNpcPrice = defaultPrice; 

            if (townNpcPrice <= maxAcceptablePrice)
            {
                int totalCost = qty * townNpcPrice;
                if (this.Gold >= totalCost)
                {
                    this.Gold -= totalCost;
                    town.Wealth += totalCost; 
                    SafeCreateAndDrop(itemType, qty);
                    return;
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
            int budget = this.Gold / 2; 
            bool foundMagicGear = false;

            if (PlayerVendor.PlayerVendors != null)
            {
                foreach (var vendor in PlayerVendor.PlayerVendors.Where(v => v.Map == town.Facet))
                {
                    if (vendor.Backpack == null) continue;

                    foreach (var item in vendor.Backpack.Items)
                    {
                        if (!(item is BaseWeapon || item is BaseArmor || item is BaseJewel)) continue;

                        var vi = vendor.GetVendorItem(item);
                        if (vi == null || vi.Price > budget) continue;

                        double itemValueScore = EvaluateItemScore(item, profile);
                        
                        if (itemValueScore > (this.CombatPower * 0.1)) 
                        {
                            CompletePurchase(vendor, item, 1, vi.Price);
                            Console.WriteLine($"[Adventurer] {this.Name} 상위 특수 장비 구매");
                            foundMagicGear = true;
                            return; 
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
                            int npcPrice = town.GetPrice(fallbackItem);
                            if (npcPrice <= 0) npcPrice = 200; 

                            if (this.Gold >= npcPrice && budget >= npcPrice)
                            {
                                this.Gold -= npcPrice;
                                town.Wealth += npcPrice; 
                                VirtualEquipments[requiredLayer] = fallbackItem;
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
        private int GetRarityLevel(Item item) { return 1; }

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
                    Console.WriteLine($"[생애주기] 모험가 {this.Name}({Math.Floor(currentAge)}세)가 은퇴하여 시민이 됩니다.");
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

        public void UpdateSurvivalTick()
        {
            this.Hunger += 60; 
            if (this.Hunger >= 100)
            {
                if (FoodRations > 0)
                {
                    FoodRations--;
                    this.Hunger = 0;
                    this.HP = Math.Min(MaxHP, HP + (MaxHP / 10)); 
                }
                else
                {
                    this.HP -= 5;        
                    this.Stress += 5;    
                }
            }

            if (Party != null && Party.State == AdventurerState.Resting)
            {
                this.HP = Math.Min(MaxHP, HP + 20);
                this.Stress = Math.Max(0, Stress - 10);
            }

            GainExp(25); 

            this.HP = Math.Clamp(HP, 0, MaxHP);
            this.Stress = Math.Clamp(this.Stress, 0, 100);

            if (this.HP <= 0 && Party == null) Die(); 
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

            int nearbyChests = 0;
            IPooledEnumerable eable = map.GetItemsInRange(loc, 10);
            foreach (Item item in eable)
                if (item is MetalGoldenChest && item.Name == "Adventurer's Grave") nearbyChests++;
            eable.Free();

            if (nearbyChests >= 5) return;

            RegionCode code = RegionSaver.GetRegionCode(map, loc.X, loc.Y, loc.Z);
            if (DungeonManager.Zones.TryGetValue(code, out var zone))
            {
                int maxAllowed = Math.Max(2, zone.Nodes.Count * 2); 
                int currentCount = World.Items.Values.OfType<MetalGoldenChest>().Count(c => c.Map == map && RegionSaver.GetRegionCode(map, c.X, c.Y, c.Z) == code);
                if (currentCount >= maxAllowed) return;
            }

            MetalGoldenChest chest = new MetalGoldenChest { 
                Name = "Adventurer's Grave", 
                Hue = 0x482,
                Locked = true 
            };
            
            chest.LockLevel = chest.RequiredSkill = Math.Clamp(this.Level + 20, 30, 120);

            Pouch supplyBag = new Pouch { Name = "Supplies" };
            supplyBag.DropItem(new Gold(this.Gold / 2));
            var resources = this.Backpack.Items.Where(i => i is BasePotion || i is Bandage || i is BaseReagent).ToList();
            foreach (var res in resources.Take(10)) supplyBag.DropItem(res);
            
            chest.DropItem(supplyBag);

            Timer.DelayCall(TimeSpan.FromHours(6.0), () => {
                if (!chest.Deleted) {
                    Effects.SendLocationEffect(chest.Location, chest.Map, 0x376A, 10, 1);
                    chest.Delete();
                }
            });

            chest.MoveToWorld(loc, map);
        }
    }
}