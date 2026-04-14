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
                        WorldNode townNode = new WorldNode(startTown.Name, WorldNodeType.Town, startTown.Facet, startTown.Center, startTown.Center, 1);
                        
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
        public WorldNodeType Type { get; set; }
        public Map NodeMap { get; set; }
        public Point3D EntranceLoc { get; set; } 
        public Point3D TargetLoc { get; set; }   
        public int Difficulty { get; set; }      
        public Point3D Location { get => EntranceLoc; set => EntranceLoc = value; }

        public WorldNode(string name, WorldNodeType type, Map map, Point3D ext, Point3D ins, int diff)
        {
            Name = name; Type = type; NodeMap = map; EntranceLoc = ext; TargetLoc = ins; Difficulty = diff;
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(0); 
            writer.Write(Name);
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
            foreach(var m in Members)
            {
                if (m.PhysicalObject != null)
                    m.PhysicalObject.Team = 0; 
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
            if (Members.Any(m => m.PhysicalObject != null && !m.PhysicalObject.Deleted)) return;

            foreach (var m in Members)
            {
                var physical = new PhysicalAdventurer(m);
                physical.MoveToWorld(CurrentLocation, CurrentMap);
                m.PhysicalObject = physical; 
            }
        }

        private void DematerializeParty()
        {
            foreach (var m in Members)
            {
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

                Console.WriteLine($"[Quest] {Members[0].Name} 파티가 의뢰를 완수하고 {AcceptedJobReward}gp를 획득했습니다!");
                AcceptedJobReward = 0; 
            }

            if (EmployedSherpa == null) return;

            if (EmployedSherpa.Backpack != null)
            {
                var itemsToSell = EmployedSherpa.Backpack.Items.ToArray();
                int totalEarned = 0;

                foreach (var item in itemsToSell)
                {
                    int itemValue = town.GetPrice(item.GetType()) / 2;
                    totalEarned += itemValue;
                    
                    if (!town.Warehouse.ContainsKey(item.GetType())) town.Warehouse[item.GetType()] = new WarehouseItem(item.GetType(), 0, itemValue * 2);
                    town.Warehouse[item.GetType()].Stock++; 
                    item.Delete();
                }

                town.Wealth -= totalEarned;
                Members[0].Gold += totalEarned; 
            }

            int bonus = 50;
            if (Members[0].Gold >= bonus)
            {
                Members[0].Gold -= bonus;
                EmployedSherpa.Gold += bonus;
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
                if (PartyWealth >= innFee) { PartyWealth -= innFee; town.Wealth += innFee; }

                if (PartyWealth > 2000) TryHireSherpa(town); 

                int targetBandages = 50 + (Members.Count * 10);
                int targetPotions = 20 + (Members.Count * 5);

                if (EmployedSherpa != null)
                {
                    targetBandages = 200; 
                    targetPotions = 100;

                    if (PartyWealth > 5000 && PackAnimals < 3)
                    {
                        int animalCost = 1000;
                        PartyWealth -= animalCost;
                        town.Wealth += animalCost;
                        PackAnimals++;
                        Console.WriteLine($"[Adventurer] {Members[0].Name} 파티가 원정을 위해 짐말을 추가 구매했습니다! (현재: {PackAnimals}마리)");
                    }

                    targetBandages += (PackAnimals * 300);
                    targetPotions += (PackAnimals * 150);
                }

                if (Bandages < targetBandages && PartyWealth > 100) 
                { 
                    int needed = targetBandages - Bandages;
                    int buyQty = Math.Min(needed, PartyWealth / 10); 
                    PartyWealth -= (buyQty * 10); 
                    Bandages += buyQty; 
                    town.Wealth += (buyQty * 10); 
                }
                
                if (Potions < targetPotions && PartyWealth > 500) 
                { 
                    int needed = targetPotions - Potions;
                    int buyQty = Math.Min(needed, PartyWealth / 50); 
                    PartyWealth -= (buyQty * 50); 
                    Potions += buyQty; 
                    town.Wealth += (buyQty * 50); 
                }

                foreach (var m in Members) m.ProcessSmarterShopping(town);

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
                    foreach (var m in Members) m.EquipmentTier++; 
                }
            }

            foreach (var m in Members) { m.HP = m.MaxHP; m.Stress = Math.Max(0, m.Stress - 30); }

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
                        TargetNode = new WorldNode(questDz.ZoneId, WorldNodeType.Dungeon, questDz.Facet, dest, dest, questDz.CurrentDifficulty);
                        
                        TravelHoursRemaining = 4; 
                        State = AdventurerState.Traveling;
                        this.AcceptedJobReward = acceptedJob.RewardGold; 
                        
                        Console.WriteLine($"[Quest] {Members[0].Name} 파티가 방치된 '{acceptedJob.Title}' 의뢰를 수락하고 출발합니다!");
                    }
                }
            }

            if (TargetNode == null)
            {
                var validDungeons = DungeonManager.ZoneList.Where(z => z.Nodes.Count > 0).ToList();
                if (validDungeons.Count > 0)
                {
                    var targetDz = validDungeons.OrderBy(z => Math.Abs(z.CurrentDifficulty - (AverageLevel * 200))).FirstOrDefault();
                    if (targetDz != null)
                    {
                        Point3D dest = targetDz.Nodes[0].Location;
                        TargetNode = new WorldNode(targetDz.ZoneId, WorldNodeType.Dungeon, targetDz.Facet, dest, dest, targetDz.CurrentDifficulty);
                        
                        TravelHoursRemaining = 4; 
                        State = AdventurerState.Traveling;
                        
                        Console.WriteLine($"[Adventurer] {Members[0].Name} 파티가 자율 사냥을 위해 {TargetNode.Name}(으)로 출발합니다.");
                    }
                }
            }
        }

        private void ProcessTraveling()
        {
            if (TargetNode == null) { State = AdventurerState.Resting; return; }

            if (TravelHoursRemaining > 0)
            {
                double progress = 1.0 / (TravelHoursRemaining + 1);
                
                int nextX = CurrentLocation.X + (int)((TargetNode.Location.X - CurrentLocation.X) * progress);
                int nextY = CurrentLocation.Y + (int)((TargetNode.Location.Y - CurrentLocation.Y) * progress);
                
                CurrentLocation = new Point3D(nextX, nextY, CurrentLocation.Z);
                TravelHoursRemaining--;
            }
            else
            {
                CurrentLocation = TargetNode.Location;
                CurrentMap = TargetNode.NodeMap;
                CurrentNode = TargetNode;
                TargetNode = null;
                
                State = CurrentNode.Type == WorldNodeType.Dungeon ? AdventurerState.Exploring : AdventurerState.Resting;
                
                Console.WriteLine($"[Adventurer] {Members[0].Name} 파티가 {CurrentNode.Name}에 도착했습니다.");
            }
        }

        private void ProcessExploring()
        {
            double efficiency = 1.0 - (PackAnimals * 0.15); 
            
            int bandageConsume = (int)Math.Max(1, Members.Count * efficiency * 10);
            int potionConsume = (int)Math.Max(1, (Members.Count / 2) * efficiency * 10);

            Bandages = Math.Max(0, Bandages - bandageConsume);
            Potions = Math.Max(0, Potions - potionConsume);

            bool needsRetreat = Bandages < 10 || Potions < 5 || Members.Any(m => m.HP < m.MaxHP * 0.3) || Members.Any(m => m.Stress > 90);
            
            if (needsRetreat)
            {
                RetreatToTown();
                return;
            }

            BatchCombatTick();
        }

        private void RetreatToTown()
        {
            TargetNode = GetNearestTown();
            TravelHoursRemaining = 2; 
            State = AdventurerState.Traveling;
            Console.WriteLine($"[Adventurer] 물자 부족으로 인해 {TargetNode.Name}으로 퇴각 중...");
        }

        // 🌟 [핵심 기획 반영] 유령마을 필터링 및 거리 기반 벤더 탐색 로직
        private WorldNode GetNearestTown()
        {
            // 1. 현재 대륙에 있는 마을들을 모험가 파티와 가까운 거리 순으로 정렬합니다.
            var sortedTowns = TownEconomyManager.Towns.Values
                .Where(t => t.Facet == CurrentMap)
                .OrderBy(t => Utility.GetDistanceToSqrt(CurrentLocation, t.Center))
                .ToList();

            // 2 & 3. 가까운 마을부터 순서대로 루프를 돌며 상인이 있는지 검사합니다.
            foreach (var town in sortedTowns)
            {
                // 마을 내 NPC 상인 수 체크
                bool hasNpcVendor = town.VendorCount > 0;
                
                // 마을 근처(반경 150타일 내외)에 유저 상인이 한 명이라도 있는지 체크
                bool hasPlayerVendor = PlayerVendor.PlayerVendors != null && 
                                       PlayerVendor.PlayerVendors.Any(v => v.Map == town.Facet && Utility.GetDistanceToSqrt(v.Location, town.Center) < 150);

                // NPC 벤더나 유저 벤더가 단 1명이라도 존재하면 이 마을로 대피 결정! (마진시아 등 유령마을 자연스럽게 배제)
                if (hasNpcVendor || hasPlayerVendor)
                {
                    return new WorldNode(town.TownName ?? town.Name, WorldNodeType.Town, town.Facet, town.Center, town.Center, 1);
                }
            }

            // 4. 만약 대륙 내의 모든 마을에 상인이 0명이라면 (서버 멸망 수준), 거리가 가장 가까운 아무 마을이나 리턴
            var fallbackTown = sortedTowns.FirstOrDefault();
            if (fallbackTown != null)
            {
                return new WorldNode(fallbackTown.TownName ?? fallbackTown.Name, WorldNodeType.Town, fallbackTown.Facet, fallbackTown.Center, fallbackTown.Center, 1);
            }

            // 갈 곳이 없으면 일단 제자리 대기
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
                        if (town != null) town.Wealth += 1000; 
                        
                        Console.WriteLine($"[Adventurer] {m.Name}이(가) 치명상을 입었으나 동료들이 파티 자금으로 구조했습니다.");
                    }
                    else
                    {
                        m.SpawnAdventurerChest(CurrentMap, CurrentLocation);
                        Members.RemoveAt(i);
                        Console.WriteLine($"[Adventurer] {m.Name}이 던전에서 전사했습니다.");
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

            foreach (var m in party.Members) 
            {
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
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(2); 

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

            foreach (var req in requirements)
            {
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

        private void TrySmartPurchase(TownEconomy town, Type itemType, int qty, int npcPrice)
        {
            var userVendorResult = SearchUserVendorsForConsumables(town.Facet, itemType, qty, npcPrice);
            
            if (userVendorResult.Found)
            {
                CompletePurchase(userVendorResult.Vendor, userVendorResult.Item, qty, userVendorResult.TotalPrice);
            }
            else
            {
                int total = qty * npcPrice;
                if (this.Gold >= total)
                {
                    this.Gold -= total;
                    town.Wealth += total;
                    SafeCreateAndDrop(itemType, qty);
                }
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
                        Console.WriteLine($"[Adventurer Shopping] {this.Name}이 유저 벤더에서 상위 등급 장비 {item.Name}를 구매!");
                        return; 
                    }
                }
            }
        }

        private double EvaluateItemScore(Item item, CombatProfile profile)
        {
            double score = 0;
            foreach (int optID in profile.PreferredOptions)
            {
                int val = GetOptionValue(item, optID); 
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
            foreach (Layer requiredLayer in profile.RequiredLayers)
            {
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
                    Console.WriteLine($"[생애주기] 위대한 모험가 {this.Name}({Math.Floor(currentAge)}세)가 노환으로 은퇴하여 시민이 됩니다.");
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