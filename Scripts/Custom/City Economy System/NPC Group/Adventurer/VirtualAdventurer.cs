using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Misc
{
    // ==============================================================================
    // [기초 Enum 및 구조체]
    // ==============================================================================
    public enum LawChaos { Lawful, Neutral, Chaotic }
    public enum GoodEvil { Good, Neutral, Evil }
    public enum AdventurerState { Resting, Traveling, Exploring }
    public enum WorldNodeType { Town, Dungeon, Ruin }
    public enum AdventurerRole { Tank, MeleeDPS, RangedDPS, MagicDPS, Healer, Support }
    public enum LootDistributionRule { Equal, Contribution }
    
    // ==============================================================================
    // [직업 프로필 및 장비/옵션 매니저]
    // ==============================================================================
    public record CombatProfile(AdventurerRole Role, double HpWeight, double MpWeight, double SpWeight, int[] PreferredOptions, params Layer[] RequiredLayers);

    public static class AdventurerProfileManager
    {
        // 18종 방어구 및 장신구 체계를 고려한 레이어 세분화
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

        public WorldNode(string name, WorldNodeType type, Map map, Point3D ext, Point3D ins, int diff)
        {
            Name = name; Type = type; NodeMap = map; EntranceLoc = ext; TargetLoc = ins; Difficulty = diff;
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(0); // Version
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
        public List<VirtualAdventurer> Members { get; set; }
        public AdventurerState State { get; set; }           
        public WorldNode CurrentNode { get; set; }           
        public WorldNode TargetNode { get; set; }            
        public int TravelHoursRemaining { get; set; }        

        public VirtualCitizen EmployedSherpa { get; set; } 
        public LootDistributionRule LootRule { get; set; } = LootDistributionRule.Equal;

        public AdventurerParty(WorldNode startNode)
        {
            Members = [];
            CurrentNode = startNode;
            State = AdventurerState.Resting;
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(0); // Version
            writer.Write((int)State);
            writer.Write(TravelHoursRemaining);
            writer.Write((int)LootRule);

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
            TravelHoursRemaining = reader.ReadInt();
            LootRule = (LootDistributionRule)reader.ReadInt();

            if (reader.ReadBool()) CurrentNode = new WorldNode(reader);
            if (reader.ReadBool()) TargetNode = new WorldNode(reader);

            Members = [];
            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                var adv = new VirtualAdventurer(reader) { Party = this };
                Members.Add(adv);
            }
        }

        public void SettleTownReturn(TownEconomy town)
        {
            if (Members.Count == 0) return;

            if (EmployedSherpa != null)
            {
                if (EmployedSherpa.Backpack != null)
                {
                    var itemsToSell = EmployedSherpa.Backpack.Items.ToArray();
                    int totalEarned = 0;

                    foreach (var item in itemsToSell)
                    {
                        int itemValue = town.GetPrice(item.GetType()) / 2;
                        totalEarned += itemValue;
                        
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
                
                EmployedSherpa.Stress = 0; 
                EmployedSherpa = null;
            }

            foreach (var member in Members) member.ConductTownErrands(town);
        }

        public void DistributeLoot(BaseCreature monster)
        {
            if (Members.Count == 0 || monster == null || monster.Deleted) return;

            int totalGold = (monster.HitsMax + monster.Fame) / 5; 
            int share = totalGold / Members.Count;
            int remainder = totalGold % Members.Count;

            List<Item> droppedItems = GenerateLoot(monster);

            if (EmployedSherpa != null && droppedItems.Count > 0)
            {
                if (EmployedSherpa.Backpack == null) EmployedSherpa.EquipItem(new Backpack());

                foreach (var item in droppedItems)
                {
                    EmployedSherpa.Backpack.DropItem(item);
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
                }
            }

            foreach (var member in Members)
            {
                int myGold = share;
                if (remainder > 0) { myGold++; remainder--; }
                member.Gold += myGold;

                if (LootRule == LootDistributionRule.Equal)
                {
                    if (member.LawChaosAlignment == LawChaos.Chaotic) member.Stress = Math.Min(100, member.Stress + 2);
                    else member.Stress = Math.Max(0, member.Stress - 1);
                }
                else 
                {
                    if (member.Role == AdventurerRole.Support || member.Role == AdventurerRole.Healer) member.Stress = Math.Min(100, member.Stress + 3);
                    if (member.GoodEvilAlignment == GoodEvil.Evil) member.Stress = Math.Max(0, member.Stress - 2);
                }
            }
        }

        private List<Item> GenerateLoot(BaseCreature monster)
        {
            List<Item> loot = [];
            int powerLevel = monster.HitsMax + monster.DamageMax;

            if (powerLevel > 500 && Utility.RandomDouble() < 0.1) loot.Add(new DragonBlood(Utility.RandomMinMax(1, 3)));
            if (powerLevel > 300 && Utility.RandomDouble() < 0.2) loot.Add(new DaemonBone(Utility.RandomMinMax(1, 5)));
            if (Utility.RandomDouble() < 0.05) loot.Add(new Ruby());
            if (Utility.RandomDouble() < 0.05) loot.Add(new Sapphire());
            
            return loot;
        }

        public static AdventurerParty TryFormBalancedParty(List<VirtualAdventurer> idleAdventurers, WorldNode startNode)
        {
            if (idleAdventurers.Count == 0) return null; 

            int targetSize = Utility.RandomMinMax(1, 5);
            AdventurerParty newParty = new AdventurerParty(startNode);

            var tanks = idleAdventurers.Where(a => a.Role == AdventurerRole.Tank).ToList();
            var healers = idleAdventurers.Where(a => a.Role == AdventurerRole.Healer).ToList();
            var others = idleAdventurers.Where(a => a.Role != AdventurerRole.Tank && a.Role != AdventurerRole.Healer).ToList();

            for (int i = 0; i < targetSize; i++)
            {
                VirtualAdventurer selected = null;

                if (tanks.Count > 0 && !newParty.Members.Any(m => m.Role == AdventurerRole.Tank))
                { selected = tanks[0]; tanks.RemoveAt(0); }
                else if (healers.Count > 0 && !newParty.Members.Any(m => m.Role == AdventurerRole.Healer))
                { selected = healers[0]; healers.RemoveAt(0); }
                else if (others.Count > 0)
                { selected = others[0]; others.RemoveAt(0); }
                else if (tanks.Count > 0) 
                { selected = tanks[0]; tanks.RemoveAt(0); }
                else if (healers.Count > 0) 
                { selected = healers[0]; healers.RemoveAt(0); }

                if (selected != null)
                {
                    newParty.Members.Add(selected);
                    idleAdventurers.Remove(selected);
                }
                else break; 
            }

            if (newParty.Members.Count == 0) return null;
            return newParty;
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

        public void BatchCombatTick()
        {
            if (Members.Count == 0 || CurrentNode == null) return;

            // 1. 파티 화력 측정 (로그용 데이터 준비)
            double totalPartyDamage = 0;
            double totalPartyHealing = 0;
            string leadName = Members[0].Name;

            foreach (var member in Members)
            {
                var profile = AdventurerProfileManager.GetProfile(member.JobClass);
                // 화력 배율 조정 (개발 모니터링을 위해 15.0으로 상향)
                double dps = member.CombatPower * profile.SpWeight * 15.0;
                totalPartyDamage += dps;
                
                if (profile.Role == AdventurerRole.Healer)
                    totalPartyHealing += member.CombatPower * profile.MpWeight * 10.0;
            }

            // 2. 사냥 대상 스캔
            var monsters = FindAllMonstersInDungeon(CurrentNode.NodeMap, CurrentNode.TargetLoc, 100);
            if (monsters.Count == 0) return;

            // --- [전투 로그 시작] ---
            Console.WriteLine($"\n[Combat] 파티 '{leadName}' ({Members.Count}명) -> {CurrentNode.Name} 진입");
            Console.WriteLine($" > 총 화력: {totalPartyDamage:N0} DP | 힐량: {totalPartyHealing:N0} HP");

            double remainingDamage = totalPartyDamage;
            int totalKills = 0;

            // 3. 데미지 적용 프로세스
            foreach (var monster in monsters.OrderBy(m => m.Hits))
            {
                if (remainingDamage <= 0) break;

                int oldHits = monster.Hits;

                if (remainingDamage >= oldHits)
                {
                    // 처치 성공
                    remainingDamage -= oldHits;
                    monster.Kill();
                    totalKills++;
                }
                else
                {
                    // 피 깎기 성공 (실제 인게임 몬스터 HP 반영)
                    int damageDealt = (int)remainingDamage;
                    monster.Hits -= damageDealt;
                    remainingDamage = 0;

                    Console.WriteLine($" > [피해] {monster.Name}: {oldHits:N0} -> {monster.Hits:N0} HP (잔여 데미지 소진)");
                }
            }

            // 4. 몬스터의 반격 및 파티 피해 (로그)
            double incomingDamage = monsters.Take(5).Sum(m => m.DamageMax * 2.0); // 상위 5마리만 반격
            double finalDamage = Math.Max(0, incomingDamage - totalPartyHealing);
            
            bool needRetreat = false;
            foreach (var m in Members)
            {
                // 탱커는 더 많이 맞고, 나머지는 덜 맞음
                int taken = (int)(m.Role == AdventurerRole.Tank ? finalDamage * 0.4 : finalDamage * 0.1);
                m.HP -= taken;

                if (taken > 0)
                    Console.WriteLine($" > [피격] {m.Name}({m.Role}): -{taken} HP (현재: {m.HP}/{m.MaxHP})");

                if (m.HP < m.MaxHP * 0.3 || m.Stress > 80) needRetreat = true;
            }

            // 5. 결과 요약
            if (totalKills > 0)
                Console.WriteLine($" > [결과] {totalKills}마리 처치! 사냥 화력 여분: {remainingDamage:N0}");

            if (needRetreat)
            {
                Console.WriteLine($" > [상태] 부상자 발생! '{leadName}' 파티가 긴급 퇴각을 결정했습니다.");
                this.State = AdventurerState.Traveling;
                this.TargetNode = GetNearestTown();
            }
            Console.WriteLine("--------------------------------------------------");
        }

        private void CheckPartyStatus(WorldNode node)
        {
            bool needRetreat = false;
            foreach (var member in Members)
            {
                // 간단한 피격 시뮬레이션 (몬스터가 살아있을 경우 반격)
                // 이 부분은 기존 로직을 유지하되 로그만 추가
                if (member.HP < member.MaxHP * 0.3)
                {
                    Console.WriteLine($"[Status] {member.Name} 부상 심각 (HP: {member.HP}/{member.MaxHP}) - 퇴각 결정!");
                    needRetreat = true;
                }
            }

            if (needRetreat)
            {
                Console.WriteLine($"[Action] 파티({CurrentNode.Name}) -> 안전지대로 도망가는 중...");
                this.State = AdventurerState.Traveling;
                this.TargetNode = GetNearestTown();
            }
        }

        private List<BaseCreature> FindAllMonstersInDungeon(Map map, Point3D loc, int radius)
        {
            List<BaseCreature> list = [];
            IPooledEnumerable eable = map.GetObjectsInRange(loc, radius);
            foreach (object obj in eable)
            {
                if (obj is BaseCreature bc && !bc.Controlled && bc.IsEnemy(null)) list.Add(bc);
            }
            eable.Free();
            return list;
        }

        private WorldNode GetNearestTown()
        {
            return new WorldNode("Nearest Town", WorldNodeType.Town, CurrentNode.NodeMap, CurrentNode.EntranceLoc, CurrentNode.EntranceLoc, 1);
        }

        public int GetTotalPower()
        {
            if (Members.Count == 0) return 0;
            double synergy = 1.0 + (Members.Count * 0.1);
            return (int)(Members.Sum(m => m.CombatPower) * synergy);
        }

        public void HourlyRoutine(TownEconomy town)
        {
            if (Members.Count == 0) return;

            foreach (var m in Members.ToList()) 
                m.HourlyRoutine(town, 0);

            switch (State)
            {
                case AdventurerState.Traveling:
                    TravelHoursRemaining--;
                    if (TravelHoursRemaining <= 0) ReachDestination();
                    break;
                case AdventurerState.Resting:
                    if (Members.TrueForAll(m => m.HP >= m.MaxHP * 0.9 && m.Stress < 20))
                    {
                        TryHireSherpa(town);
                        DecideNextDestination();
                    }
                    break;
                case AdventurerState.Exploring:
                    break;
            }
        }

        public void SetDestination(WorldNode target)
        {
            if (target == null) return;
            TargetNode = target;

            Point3D p1 = CurrentNode.EntranceLoc;
            Point3D p2 = TargetNode.EntranceLoc;

            int dist = (int)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
            const int TilesPerHour = 80;
            TravelHoursRemaining = Math.Max(1, dist / TilesPerHour);
            State = AdventurerState.Traveling;
        }

        private void ReachDestination()
        {
            CurrentNode = TargetNode;
            State = (CurrentNode.Type == WorldNodeType.Town) ? AdventurerState.Resting : AdventurerState.Exploring;
        }

        private void DecideNextDestination() { }
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
            
            // [수정됨] 기존 세이브 파일과의 호환성을 위해 int로 읽은 후 bool로 변환
            // 통상적으로 UO에서 0은 Male, 1은 Female입니다.
            int genderInt = reader.ReadInt();
            IsFemale = (genderInt == 1);
            
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

            VirtualEquipments = [];
            int equipCount = reader.ReadInt();
            for (int i = 0; i < equipCount; i++)
            {
                Layer layer = (Layer)reader.ReadInt();
                string typeName = reader.ReadString();
                Type type = ScriptCompiler.FindTypeByFullName(typeName);
                if (type != null) VirtualEquipments[layer] = type;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1); // version

            writer.Write((int)RankLevel);
            
            // [수정됨] 세이브 파일이 깨지지 않도록 다시 int로 변환하여 저장
            writer.Write(IsFemale ? 1 : 0);
            
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
        private int GetCombatPower()
        {
            var profile = AdventurerProfileManager.GetProfile(this.JobClass);
            double optionMultiplier = 1.0 + (profile.PreferredOptions.Length * 0.1); 
            return (int)(CombatSkill + (EquipmentTier * 50 * optionMultiplier));
        }

        public void EquipMissingLayers(TownEconomy town)
        {
            var profile = AdventurerProfileManager.GetProfile(this.JobClass);
            
            foreach (Layer requiredLayer in profile.RequiredLayers)
            {
                if (VirtualEquipments.ContainsKey(requiredLayer)) continue;

                Type fallbackItem = GetFallbackItemForLayer(requiredLayer, profile.Role);
                if (fallbackItem != null && this.Gold >= 500) 
                {
                    var (success, _, _) = TryBuyItem(town, fallbackItem, 1);
                    if (success) VirtualEquipments[requiredLayer] = fallbackItem;
                }
            }
        }

		private Type GetFallbackItemForLayer(Layer layer, AdventurerRole role)
        {
            bool isMage = role == AdventurerRole.Healer || role == AdventurerRole.MagicDPS;
            bool isRanged = role == AdventurerRole.RangedDPS;
            bool isTank = role == AdventurerRole.Tank;

            // 역할별 세트 번호 매핑 (Tank: Plate(17), Mage: Bone(7), Ranged: Leather(4), Melee: Leaf(1))
            int armorSetID = isTank ? 17 : (isMage ? 7 : (isRanged ? 4 : 1)); 
            
            // 장신구 매핑 (Mage: Silver(20), Others: Gold(19))
            int jewelrySetID = isMage ? 20 : 19;

            Type itemType = GetSetItemForLayer(armorSetID, layer) ?? GetSetItemForLayer(jewelrySetID, layer);

            // 방어구/장신구가 아닌 무기나 공통 의상 처리
            return itemType ?? layer switch
            {
                Layer.OuterTorso => isMage ? typeof(Robe) : null,
                Layer.Shoes => typeof(Boots),
                Layer.OneHanded => isMage ? typeof(Spellbook) : (isRanged ? null : (isTank ? typeof(Broadsword) : typeof(Longsword))),
                Layer.TwoHanded => isRanged ? (Utility.RandomBool() ? typeof(Bow) : typeof(Crossbow)) : (isTank ? typeof(MetalKiteShield) : (isMage ? typeof(GnarledStaff) : typeof(Halberd))),
                _ => null
            };
        }

        // 1~20번 세트 데이터를 기반으로 부위별 아이템 반환 (가고일 및 99번 제외)
        private Type GetSetItemForLayer(int setID, Layer layer) => setID switch
        {
            1 => layer switch { Layer.Helm => typeof(Circlet), Layer.Neck => typeof(LeafGorget), Layer.InnerTorso => typeof(LeafChest), Layer.Arms => typeof(LeafArms), Layer.Gloves => typeof(LeafGloves), Layer.Pants => typeof(LeafLegs), _ => null },
            2 => layer switch { Layer.Helm => typeof(TigerPeltHelm), Layer.Neck => typeof(TigerPeltCollar), Layer.InnerTorso => typeof(TigerPeltChest), Layer.Pants => typeof(TigerPeltLegs), _ => null },
            3 => layer switch { Layer.Helm => typeof(DragonTurtleHideHelm), Layer.InnerTorso => typeof(DragonTurtleHideChest), Layer.Arms => typeof(DragonTurtleHideArms), Layer.Pants => typeof(DragonTurtleHideLegs), _ => null },
            4 => layer switch { Layer.Helm => typeof(LeatherCap), Layer.Neck => typeof(LeatherGorget), Layer.InnerTorso => typeof(LeatherChest), Layer.Arms => typeof(LeatherArms), Layer.Gloves => typeof(LeatherGloves), Layer.Pants => typeof(LeatherLegs), _ => null },
            5 => layer switch { Layer.Helm => typeof(VultureHelm), Layer.Neck => typeof(HideGorget), Layer.InnerTorso => typeof(HideChest), Layer.Arms => typeof(HidePauldrons), Layer.Gloves => typeof(HideGloves), Layer.Pants => typeof(HidePants), _ => null },
            6 => layer switch { Layer.Neck => typeof(StuddedGorget), Layer.InnerTorso => typeof(StuddedChest), Layer.Arms => typeof(StuddedArms), Layer.Gloves => typeof(StuddedGloves), Layer.Pants => typeof(StuddedLegs), _ => null },
            7 => layer switch { Layer.Helm => typeof(BoneHelm), Layer.InnerTorso => typeof(BoneChest), Layer.Arms => typeof(BoneArms), Layer.Gloves => typeof(BoneGloves), Layer.Pants => typeof(BoneLegs), _ => null },
            8 => layer switch { Layer.Helm => typeof(RedDragonHelm), Layer.InnerTorso => typeof(RedDragonChest), Layer.Arms => typeof(RedDragonArms), Layer.Gloves => typeof(RedDragonGloves), Layer.Pants => typeof(RedDragonLegs), _ => null },
            9 => layer switch { Layer.Helm => typeof(BlueDragonHelm), Layer.InnerTorso => typeof(BlueDragonChest), Layer.Arms => typeof(BlueDragonArms), Layer.Gloves => typeof(BlueDragonGloves), Layer.Pants => typeof(BlueDragonLegs), _ => null },
            10 => layer switch { Layer.Helm => typeof(GreenDragonHelm), Layer.InnerTorso => typeof(GreenDragonChest), Layer.Arms => typeof(GreenDragonArms), Layer.Gloves => typeof(GreenDragonGloves), Layer.Pants => typeof(GreenDragonLegs), _ => null },
            11 => layer switch { Layer.Helm => typeof(YellowDragonHelm), Layer.InnerTorso => typeof(YellowDragonChest), Layer.Arms => typeof(YellowDragonArms), Layer.Gloves => typeof(YellowDragonGloves), Layer.Pants => typeof(YellowDragonLegs), _ => null },
            12 => layer switch { Layer.Helm => typeof(WhiteDragonHelm), Layer.InnerTorso => typeof(WhiteDragonChest), Layer.Arms => typeof(WhiteDragonArms), Layer.Gloves => typeof(WhiteDragonGloves), Layer.Pants => typeof(WhiteDragonLegs), _ => null },
            13 => layer switch { Layer.Helm => typeof(BlackDragonHelm), Layer.InnerTorso => typeof(BlackDragonChest), Layer.Arms => typeof(BlackDragonArms), Layer.Gloves => typeof(BlackDragonGloves), Layer.Pants => typeof(BlackDragonLegs), _ => null },
            14 => layer switch { Layer.Helm => typeof(DaemonHelm), Layer.InnerTorso => typeof(DaemonChest), Layer.Arms => typeof(DaemonArms), Layer.Gloves => typeof(DaemonGloves), Layer.Pants => typeof(DaemonLegs), _ => null },
            15 => layer switch { Layer.Helm => typeof(CloseHelm), Layer.InnerTorso => typeof(RingmailChest), Layer.Arms => typeof(RingmailArms), Layer.Gloves => typeof(RingmailGloves), Layer.Pants => typeof(RingmailLegs), _ => null },
            16 => layer switch { Layer.Helm => typeof(ChainCoif), Layer.InnerTorso => typeof(ChainChest), Layer.Pants => typeof(ChainLegs), _ => null },
            17 => layer switch { Layer.Helm => typeof(PlateHelm), Layer.Neck => typeof(PlateGorget), Layer.InnerTorso => typeof(PlateChest), Layer.Arms => typeof(PlateArms), Layer.Gloves => typeof(PlateGloves), Layer.Pants => typeof(PlateLegs), _ => null },
            18 => layer switch { Layer.Helm => typeof(RavenHelm), Layer.Neck => typeof(WoodlandGorget), Layer.InnerTorso => typeof(WoodlandChest), Layer.Arms => typeof(WoodlandArms), Layer.Gloves => typeof(WoodlandGloves), Layer.Pants => typeof(WoodlandLegs), _ => null },
            19 => layer switch { Layer.Ring => typeof(GoldRing), Layer.Bracelet => typeof(GoldBracelet), Layer.Neck => typeof(GoldNecklace), Layer.Earrings => typeof(GoldEarrings), _ => null },
            20 => layer switch { Layer.Ring => typeof(SilverRing), Layer.Bracelet => typeof(SilverBracelet), Layer.Neck => typeof(SilverNecklace), Layer.Earrings => typeof(SilverEarrings), _ => null },
            _ => null
        };

        public (bool Success, int AmountBought, int TotalCost) TryBuyItem(TownEconomy town, Type itemType, int requestedAmount)
        {
            if (itemType == null || requestedAmount <= 0) return (false, 0, 0);

            int unitPrice = GetEffectivePrice(town, itemType);
            int maxPricePerItem = unitPrice * 3; 
            int remainingAmount = requestedAmount;
            int totalSpent = 0;

            var map = town.Facet;
            if (map != null && map != Map.Internal)
            {
                if (RetailVendor.RetailVendors != null)
                {
                    foreach (var vendor in RetailVendor.RetailVendors.Where(v => v != null && v.Map == map && !v.Deleted))
                    {
                        var matchingItems = vendor.MarketItems.Where(m => m.RealItem != null && !m.RealItem.Deleted && m.RealItem.GetType() == itemType).ToList();
                        foreach (var mItem in matchingItems)
                        {
                            if (mItem.PricePerUnit > maxPricePerItem) continue;

                            int affordable = this.Gold / Math.Max(1, mItem.PricePerUnit);
                            int buyAmount = Math.Min(mItem.RealItem.Amount, Math.Min(affordable, remainingAmount));

                            if (buyAmount > 0)
                            {
                                int cost = mItem.PricePerUnit * buyAmount;
                                Item boughtItem = vendor.ExtractItemForAI(mItem, buyAmount);
                                
                                if (boughtItem != null)
                                {
                                    this.Gold -= cost;
                                    vendor.HoldGold += cost;
                                    totalSpent += cost;
                                    remainingAmount -= buyAmount;
                                    boughtItem.Delete(); 

                                    if (remainingAmount <= 0) return (true, requestedAmount, totalSpent);
                                }
                            }
                        }
                    }
                }

                if (PlayerVendor.PlayerVendors != null)
                {
                    foreach (var vendor in PlayerVendor.PlayerVendors.Where(v => v != null && v.Map == map && !v.Deleted && v.Backpack != null))
                    {
                        var itemsToCheck = new List<Item>();
                        var containersToSearch = new Queue<Container>();
                        containersToSearch.Enqueue(vendor.Backpack);

                        while (containersToSearch.Count > 0)
                        {
                            var currentContainer = containersToSearch.Dequeue();
                            foreach (var item in currentContainer.Items.ToArray())
                            {
                                if (item.GetType() == itemType) itemsToCheck.Add(item);
                                else if (item is Container sub) containersToSearch.Enqueue(sub);
                            }
                        }

                        foreach (var item in itemsToCheck)
                        {
                            var vi = vendor.GetVendorItem(item);
                            if (vi == null || vi.Price <= 0) continue;

                            int vendorUnitPrice = vi.Price / Math.Max(1, item.Amount);
                            if (vendorUnitPrice > maxPricePerItem) continue;

                            if (vi.Price <= this.Gold && item.Amount <= remainingAmount)
                            {
                                int cost = vi.Price;
                                this.Gold -= cost;
                                vendor.HoldGold += cost;
                                totalSpent += cost;
                                remainingAmount -= item.Amount;
                                item.Delete();

                                if (remainingAmount <= 0) return (true, requestedAmount, totalSpent);
                            }
                        }
                    }
                }
            }

            if (remainingAmount > 0)
            {
                int townPrice = town.GetPrice(itemType);
                int cost = townPrice * remainingAmount;
                
                if (this.Gold >= cost)
                {
                    this.Gold -= cost;
                    town.Wealth += cost;
                    totalSpent += cost;
                    return (true, requestedAmount, totalSpent);
                }
            }

            int bought = requestedAmount - remainingAmount;
            return (bought > 0, bought, totalSpent);
        }

        protected override void ProcessJob(TownEconomy town)
        {
            if (Party != null && Party.TargetNode != null)
            {
                Point3D p1 = Party.CurrentNode.EntranceLoc;
                Point3D p2 = Party.TargetNode.EntranceLoc;
                int dist = (int)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
                this.PrepMultiplier = Math.Min(2.0, 1.2 + (dist / 3000.0));
            }

            int foodT = (int)(10 * PrepMultiplier);
            if (FoodRations < foodT) 
            {
                var buyReq = TryBuyItem(town, typeof(BreadLoaf), foodT - FoodRations);
                if (buyReq.Success) FoodRations += buyReq.AmountBought;
            }

            int potT = (int)(5 * PrepMultiplier);
            if (HealingPotions < potT) 
            {
                var buyReq = TryBuyItem(town, typeof(GreaterHealPotion), potT - HealingPotions);
                if (buyReq.Success) HealingPotions += buyReq.AmountBought;
            }

            int bandT = (int)(20 * PrepMultiplier);
            if (Bandages < bandT) 
            {
                var buyReq = TryBuyItem(town, typeof(Bandage), bandT - Bandages);
                if (buyReq.Success) Bandages += buyReq.AmountBought;
            }

            if (this.VirtualEquipments.ContainsKey(Layer.TwoHanded))
            {
                Type weaponType = this.VirtualEquipments[Layer.TwoHanded];
                if (weaponType == typeof(Bow))
                {
                    int ammoNeeds = (int)(50 * PrepMultiplier);
                    if (Arrows < ammoNeeds)
                    {
                        var buyReq = TryBuyItem(town, typeof(Arrow), ammoNeeds - Arrows);
                        if (buyReq.Success) Arrows += buyReq.AmountBought;
                    }
                }
                else if (weaponType == typeof(Crossbow) || weaponType == typeof(HeavyCrossbow))
                {
                    int ammoNeeds = (int)(50 * PrepMultiplier);
                    if (Bolts < ammoNeeds)
                    {
                        var buyReq = TryBuyItem(town, typeof(Bolt), ammoNeeds - Bolts);
                        if (buyReq.Success) Bolts += buyReq.AmountBought;
                    }
                }
            }

            if (!HasBedroll) 
            {
                if (TryBuyItem(town, typeof(Bedroll), 1).Success) HasBedroll = true;
            }

            if (this.HP < MaxHP * 0.6 || this.Stress > 40)
            {
                if (this.Gold >= 50) 
                { 
                    this.Gold -= 50; 
                    town.Wealth += 50; 
                    IsRestingAtInn = true; 
                    this.Stress = Math.Max(0, this.Stress - 10); 
                }
            }
            else IsRestingAtInn = false;

            int upgradeCost = EquipmentTier * 2000;
            if (this.Gold >= upgradeCost)
            {
                this.Gold -= upgradeCost;
                town.Wealth += upgradeCost;
                EquipmentTier++;
            }
        }

        public void ConductTownErrands(TownEconomy town)
        {
            if (Utility.RandomDouble() < 0.4) TryRepairEquipment(town, new Broadsword());
            ProcessJob(town);

            var retirement = CheckRetirement();
            if (retirement.IsRetiring)
            {
                Console.WriteLine($"[Adventurer] {this.Name} 은퇴. 새로운 신분: {retirement.NewRank}");
                RetireToCitizen(town, retirement.NewRank);
            }
        }

        private void RetireToCitizen(TownEconomy town, NobilityRank newRank)
        {
            if (this.Party != null) this.Party.Members.Remove(this);
            VirtualCitizen citizen = new VirtualCitizen(this.JobClass, newRank, 100);
            town.Citizens.Add(citizen);
        }

        public int GetAffinityDistance(VirtualAdventurer other)
        {
            int diff = Math.Abs(this.Affinity - other.Affinity);
            return diff > 75 ? 150 - diff : diff;
        }

        public (bool IsRetiring, NobilityRank NewRank) CheckRetirement()
        {
            if (this.Karma > 5000 && this.Gold > 100000)
            {
                double retireChance = (this.Karma / 10000.0) + (this.Potential > 2.0 ? 0.2 : 0);
                if (Utility.RandomDouble() < retireChance)
                {
                    NobilityRank rank = Fame > 10000 ? NobilityRank.Knight : NobilityRank.Commoner; 
                    return (true, rank);
                }
            }
            return (false, NobilityRank.Commoner);
        }

        public (bool Success, int RepairCost) TryRepairEquipment(TownEconomy town, Item item)
        {
            if (item == null || item.Deleted) return (false, 0);

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

        public override void HourlyRoutine(TownEconomy town, int currentHour)
        {
            if (Party == null) return;

            UpdateSurvival(); 

            switch (Party.State)
            {
                case AdventurerState.Resting: HandleResting(town); break;
                case AdventurerState.Traveling: HandleTraveling(); break;
                case AdventurerState.Exploring: HandleExploring(town); break;
            }
        }

        private void UpdateSurvival()
        {
            this.Hunger += 2;
            if (this.Hunger >= 50)
            {
                if (FoodRations > 0)
                {
                    FoodRations--;
                    this.Hunger = 0;
                    this.HP = Math.Min(MaxHP, HP + 5); 
                }
                else
                {
                    this.HP -= 5;       
                    this.Stress += 5;    
                }
            }

            if (Party.State == AdventurerState.Resting)
            {
                if (Party.CurrentNode.Type == WorldNodeType.Town)
                {
                    int recoverHP = IsRestingAtInn ? 20 : 5;
                    int reduceStress = IsRestingAtInn ? 10 : 2;

                    this.HP += recoverHP;
                    this.Stress -= reduceStress;
                }
                else
                {
                    bool success = Utility.Random(100) < this.CampingSkill;
                    int bedrollBonus = HasBedroll ? 4 : 0;

                    if (success)
                    {
                        this.HP += 8 + bedrollBonus;
                        this.Stress -= 5 + (HasBedroll ? 2 : 0);
                        CheckSkillGain("Camping", 0.1); 
                    }
                    else
                    {
                        this.HP += 2 + bedrollBonus;
                        this.Stress += 1; 
                        CheckSkillGain("Camping", 0.05);
                    }
                }
            }
            else
            {
                this.HP -= 2;
                this.Stress += 1;
            }
            GainExp(25);
            
            this.HP = Math.Max(0, Math.Min(MaxHP, HP));
            this.Stress = Math.Max(0, Math.Min(100, this.Stress));

            if (this.HP <= 0) Die();
        }

        private void HandleTraveling()
        {
            CheckSkillGain("Combat", 0.05);
            CheckSkillGain("Camping", 0.03);
            GainExp(10);
        }

        private void HandleResting(TownEconomy town)
        {
            if (Party.CurrentNode.Type == WorldNodeType.Town)
            {
                ProcessJob(town);
            }
        }

        private void HandleExploring(TownEconomy town)
        {
            VirtualCombatTick();

            if (this.HP < MaxHP * 0.4 || HealingPotions == 0 || this.Stress > 85)
            {
                ReturnToSafety();
            }
        }

        public int GetRequiredExp()
        {
            return (Level * Level * 50) + (Level * 100);
        }

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
            int hpGrowth = Utility.RandomMinMax(5, 10) + (int)RankLevel;
            MaxHP += hpGrowth;
            HP = MaxHP; 

            CombatSkill += Utility.RandomMinMax(1, 3);
            Stress = Math.Max(0, Stress - 50);
        }

        private void VirtualCombatTick()
        {
            WorldNode node = Party.CurrentNode;
            if (node == null || Party == null) return; // 파티 자체가 없으면 리턴

            BaseCreature monster = FindMonster(node.NodeMap, node.TargetLoc, 100);
            if (monster == null) return;

            // [수정] 멤버가 있을 때만 0번 인덱스에 접근하도록 방어막 설치
            string leadName = (Party.Members != null && Party.Members.Count > 0) 
                              ? Party.Members[0].Name 
                              : this.Name; // 파티원이 없으면 자신의 이름이라도 출력

            string mName = monster.Name ?? monster.GetType().Name;
            
            // [로그 출력 부분] 이제 안전합니다.
            Console.WriteLine($"\n[현장 상황] {leadName} 파티 vs {mName}");

            // ... 나머지 로직 (기존과 동일)
            int oldHP = monster.Hits;
            int damage = (int)(Party.GetTotalPower() / 15); // 화력 계산
            monster.Hits -= damage;

            Console.WriteLine($" > [타격] {mName}: HP {oldHP} -> {monster.Hits}");

            if (monster.Hits <= 0)
            {
                Console.WriteLine($" > [처치] {mName}을(를) 물리쳤습니다!");
                ApplyCombatResult(monster, true, 5, 1, 1, 0, 100);
            }
            else
            {
                ApplyCombatResult(monster, false, 15, 5, 0, 1, 100);
            }
            Console.WriteLine("--------------------------------------------------");
        }

        private void ApplyCombatResult(BaseCreature monster, bool isKilled, int hpLoss, int stressAdd, int bandageUse, int potionUse, int mobStats)
        {
            if (potionUse > 0 && HealingPotions >= potionUse)
            {
                HealingPotions -= potionUse;
                hpLoss = Math.Max(0, hpLoss - 40); 
            }
            
            if (bandageUse > 0 && Bandages >= bandageUse)
            {
                Bandages -= bandageUse;
                hpLoss = Math.Max(0, hpLoss - (bandageUse * 5)); 
            }

            this.HP -= Math.Max(0, hpLoss);
            this.Stress += stressAdd;

            if (isKilled && monster != null && !monster.Deleted)
            {
                int goldLoot = (monster.HitsMax + monster.Fame) / 10; 
                this.Gold += goldLoot;
                
                monster.Kill();
                CheckSkillGain("Combat", 0.15);
                GainExp(mobStats / 2); 
            }

            if (this.HP <= 0) Die(); 
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
            SpawnAdventurerChest(Party.CurrentNode.NodeMap, Party.CurrentNode.TargetLoc);
            Party.Members.Remove(this);
        }

        private void SpawnAdventurerChest(Map map, Point3D loc)
        {
            MetalGoldenChest chest = new MetalGoldenChest();
            chest.Locked = true;
            chest.LockLevel = chest.RequiredSkill = (this.EquipmentTier * 20) + 10;
            
            chest.DropItem(new Gold(this.Gold / 2));
            if (Utility.RandomBool()) chest.DropItem(new StarSapphire(Utility.RandomMinMax(1, 3)));
            
            chest.MoveToWorld(loc, map);
        }

        private void ReturnToSafety()
        {
            Party.State = AdventurerState.Traveling;
        }

        private bool IsNormalPlayerNearby(Map map, Point3D loc, int radius)
        {
            IPooledEnumerable eable = map.GetClientsInRange(loc, radius);
            foreach (NetState state in eable)
            {
                if (state.Mobile != null && state.Mobile.AccessLevel == AccessLevel.Player)
                { eable.Free(); return true; }
            }
            eable.Free(); return false;
        }

        private BaseCreature FindMonster(Map map, Point3D loc, int radius)
        {
            IPooledEnumerable eable = map.GetObjectsInRange(loc, radius);
            foreach (object obj in eable)
            {
                if (obj is BaseCreature bc && !bc.Controlled && bc.IsEnemy(null))
                { eable.Free(); return bc; }
            }
            eable.Free(); return null;
        }
    }
}