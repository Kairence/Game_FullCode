using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Multis;

namespace Server.Misc
{
    public record DeepJobProfile(
        SkillName Skill, 
        NobilityRank MinRank, 
        NobilityRank MaxRank, 
        EconomyItemKey[] Necessities, 
        EconomyItemKey[] JobMaterials, 
        EconomyItemKey[] Luxuries, 
        EconomyItemKey[] Produces, 
        EconomyItemKey[] Addons,
        int BaseQty
    );

    // ==============================================================================
    // 1. [VirtualCitizen] 개별 시민 객체
    // ==============================================================================
    public enum Gender { Male, Female }
    
    public enum FoodDesireLevel { Low, Normal, High } 

    public class VirtualCitizen : VirtualAgent 
    {
        public const double GameYearMinutes = 3600.0;

        public int Fame { get; set; }          
        public int Karma { get; set; }
        public Dictionary<SkillName, double> Skills { get; set; }

        public Gender Gender { get; set; }
        public double Potential { get; set; }  
        public DateTime BirthTime { get; set; } 
        public TimeSpan MaxLifespan { get; set; } 
        
        public double Age => (DateTime.Now - BirthTime).TotalMinutes / GameYearMinutes;

        public int Satisfaction { get; set; }  
        public NobilityRank RankLevel { get; set; } 
        public int Thirst { get; set; } 
        public string TargetRegionName { get; set; }

        public bool IsStarving => Hunger <= 0; 
        public bool IsDehydrated => Thirst <= 0; 

        public bool IsChild => Age < 18.0; 
        public bool IsProductive => Age is >= 18.0 and < 60.0;
        public bool IsElder => Age >= 60.0;
        
        public bool IsKilled { get; set; } = false;

        public bool IsExpired => IsKilled || Age >= (MaxLifespan.TotalMinutes / GameYearMinutes);

        public FamilyUnit Family { get; set; } 
        public VirtualHouse House { get; set; } 
        
        public int LastProcessedHour { get; set; } = -1;
        public DateTime LastSurvivalTick { get; set; } = DateTime.Now;
        public BioStats Bio { get; set; } = new BioStats();
        public int Generation { get; set; } = 1;
        public Point3D Location { get; set; }
        public Map Map { get; set; }

        public FoodDesireLevel FoodDesire { get; set; }
        public Type FavoriteFood { get; set; }
        public Type DislikedFood { get; set; }

        public void InitializeTaste()
        {
            Type[] foodPool = [typeof(Ribs), typeof(Bacon), typeof(Sausage), typeof(ChickenLeg), typeof(CookedBird), typeof(FishSteak), typeof(Carrot), typeof(Cabbage), typeof(Apple), typeof(Peach), typeof(BreadLoaf), typeof(Muffins), typeof(CheesePizza), typeof(Turnip)];
            FavoriteFood = foodPool[Utility.Random(foodPool.Length)];
            do { DislikedFood = foodPool[Utility.Random(foodPool.Length)]; } while (DislikedFood == FavoriteFood);
            
            double dRoll = Utility.RandomDouble();
            FoodDesire = dRoll > 0.8 ? FoodDesireLevel.High : (dRoll > 0.3 ? FoodDesireLevel.Normal : FoodDesireLevel.Low);
        }

        public VirtualCitizen(NpcJobClass job, NobilityRank rank, int satisfaction, int gen = 1) : base(job, NpcRank.Novice)
        {
            Generation = gen;
            RankLevel = rank;
            Satisfaction = satisfaction;
            Gender = Utility.RandomBool() ? Gender.Female : Gender.Male;
            string genderString = Gender == Gender.Female ? "female" : "male";
            Name = NameList.RandomName(genderString);

            int gameMaxAge = Utility.RandomMinMax(60, 90);
            MaxLifespan = TimeSpan.FromMinutes(gameMaxAge * GameYearMinutes);

            int startingAge = Utility.RandomMinMax(20, 25);
            BirthTime = DateTime.Now - TimeSpan.FromMinutes(startingAge * GameYearMinutes);

            double roll = Utility.RandomDouble();
            Potential = roll > 0.97 ? 3.0 : (roll > 0.90 ? 1.5 : 1.0);
            
            Hunger = 100000;
            Thirst = 100000;
            Skills = [];
            foreach (SkillName sk in VirtualJobCore.AllSkills) Skills[sk] = 0.0;

            InitializeTaste(); 
        }

        public bool HasCheckedAdventurer { get; set; } = false;

        public void OnHourTick()
        {
            if (IsExpired) return;

            if (this.Age >= 17.0 && this.Age < 17.1 && !this.HasCheckedAdventurer)
            {
                this.HasCheckedAdventurer = true; 
                
                if (Utility.RandomDouble() < 0.05)
                {
                    Console.WriteLine($"[생애주기] {this.Name}(17세)가 마을을 떠나 모험가의 길을 걷습니다!");
                    
                    NpcJobClass[] combatJobs = { NpcJobClass.Knight, NpcJobClass.Archer_Expert, NpcJobClass.Wizard, NpcJobClass.Halberdier, NpcJobClass.Healer_Master };
                    var adv = new VirtualAdventurer(combatJobs[Utility.Random(combatJobs.Length)], this.RankLevel) { Gold = this.Gold };
                    VirtualAdventurerManager.IdleAdventurers.Add(adv);
                    
                    this.IsKilled = true; 
                    return; 
                }
            }

            double decayFactor = 1.5 / Potential;
            int baseMetabolism = (int)(2000.0 * decayFactor); // 노동을 안 할 때의 1틱당 기초 대사량 (2000)

            this.Hunger = Math.Max(0, this.Hunger - baseMetabolism);
            this.Thirst = Math.Max(0, this.Thirst - baseMetabolism);
        }

        public void UpdateClothingMentalHealth()
        {
            if (this.Family == null || this.IsChild || this.House == null) return;

            int stressModifier = ClothingEconomy.CalculateStressChange(this.Family);
            this.Stress = Math.Clamp(this.Stress + stressModifier, 0, 100);

            if (stressModifier > 0)
            {
                this.Satisfaction = Math.Max(0, this.Satisfaction - 5);
                RegisterMissingClothesToNeeds();
            }
        }

        private void RegisterMissingClothesToNeeds()
        {
            if (this.House == null || this.House.HouseWarehouse == null) return;

            var warehouse = this.House.HouseWarehouse;
            
            foreach (ClothSlot slot in Enum.GetValues<ClothSlot>())
            {
                int currentCount = 0;
                foreach (var key in warehouse.Keys)
                {
                    if (ClothingEconomy.ClothCategoryMap.TryGetValue(key.ItemType, out ClothSlot s) && s == slot)
                        currentCount += warehouse[key];
                }

                if (currentCount < 3)
                {
                    int deficit = 3 - currentCount;
                    
                    if (!WardrobeEconomy.CanStoreMoreClothes(this.House, deficit))
                    {
                        Type armoireType = typeof(CherryArmoire); 
                        if (this.RankLevel >= NobilityRank.Baron) armoireType = typeof(FancyElvenArmoire); 
                        
                        if (!this.House.UnfulfilledNeeds.ContainsKey(armoireType))
                            this.House.UnfulfilledNeeds[armoireType] = 0;
                        
                        this.House.UnfulfilledNeeds[armoireType] += 1;
                        Console.WriteLine($"[Wardrobe] '{this.House.HouseName}' 가문의 옷장이 가득 찼습니다! 새 옷장({armoireType.Name})을 의뢰합니다.");
                        return; 
                    }

                    Type targetType = GetRepresentativeItemForSlot(slot);
                    if (!this.House.UnfulfilledNeeds.ContainsKey(targetType))
                        this.House.UnfulfilledNeeds[targetType] = 0;
                    
                    this.House.UnfulfilledNeeds[targetType] += deficit;
                }
            }
        }

        private Type GetRepresentativeItemForSlot(ClothSlot slot) => slot switch
        {
            ClothSlot.Head => typeof(Kasa),
            ClothSlot.Shirt => typeof(FancyShirt),
            ClothSlot.Pants => typeof(FancyKilt),
            ClothSlot.Outer => typeof(Robe),
            ClothSlot.Footwear => typeof(ElvenBoots),
            _ => typeof(BodySash)
        };

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(8); // 버전 8 
            writer.Write(IsKilled); 
            writer.Write(TargetRegionName);
            if (Bio != null) { writer.Write(true); Bio.Serialize(writer); } else { writer.Write(false); }
            writer.Write((int)Generation);
            writer.Write(Name);
            writer.Write(Fame);
            writer.Write(Karma);
            writer.Write((int)Gender);
            writer.Write(Potential);
            writer.Write(BirthTime); 
            writer.Write(MaxLifespan);
            writer.Write(Satisfaction);
            writer.Write((int)RankLevel);
            writer.Write(Thirst);
            writer.Write(LastProcessedHour);
            writer.Write(Skills.Count);
            foreach (var (skill, val) in Skills) { writer.Write((int)skill); writer.Write(val); }

            writer.Write((int)FoodDesire);
            writer.Write(FavoriteFood?.FullName ?? "");
            writer.Write(DislikedFood?.FullName ?? "");
        }

        public VirtualCitizen(GenericReader reader) : base(reader)
        {
            int version = reader.ReadInt();
            if (version >= 7) IsKilled = reader.ReadBool();
            if (version >= 6) { TargetRegionName = reader.ReadString(); if (reader.ReadBool()) Bio = new BioStats(reader); else Bio = new BioStats(); } else Bio = new BioStats();
            Generation = reader.ReadInt(); Name = reader.ReadString(); Fame = reader.ReadInt(); Karma = reader.ReadInt(); Gender = (Gender)reader.ReadInt(); Potential = reader.ReadDouble(); BirthTime = reader.ReadDateTime(); MaxLifespan = reader.ReadTimeSpan(); Satisfaction = reader.ReadInt(); RankLevel = (NobilityRank)reader.ReadInt(); Thirst = reader.ReadInt(); if (version >= 4) LastProcessedHour = reader.ReadInt();
            Skills = []; int skillCount = reader.ReadInt(); for (int i = 0; i < skillCount; i++) Skills[(SkillName)reader.ReadInt()] = reader.ReadDouble();

            if (version >= 8)
            {
                FoodDesire = (FoodDesireLevel)reader.ReadInt();
                string fStr = reader.ReadString(); if (!string.IsNullOrEmpty(fStr)) FavoriteFood = ScriptCompiler.FindTypeByFullName(fStr);
                string dStr = reader.ReadString(); if (!string.IsNullOrEmpty(dStr)) DislikedFood = ScriptCompiler.FindTypeByFullName(dStr);
            }
            else
            {
                InitializeTaste(); 
            }
        }
    }

    // ==============================================================================
    // 2. [VirtualCitizenAI] 두뇌 및 파이프라인
    // ==============================================================================
    public static class VirtualCitizenAI
    {
        public static void Initialize() { }

        public static void ExecuteFinalBatchProcess(int gameHour)
        {
			ExecuteDungeonSecurityImpact();
            var towns = TownEconomyManager.Towns.Values.ToList();
            foreach (var town in towns)
            {
                if (town.Citizens != null)
                {
                    // [안전 패치] ToList()를 사용하여 순회 중 삭제(Remove)로 인한 컬렉션 에러 방지
                    var currentCitizens = town.Citizens.ToList();
                    foreach (var c in currentCitizens) 
                    {
                        c.OnHourTick();

                        // [패치 3번] 유령 시민 장례식 및 호적 정리 로직
                        if (c.IsExpired)
                        {
                            // 1. 가족 관계도(FamilyUnit)에서 본인 이름 지우기 (NullReference 방지)
                            if (c.Family != null)
                            {
                                if (c.Family.Father == c) c.Family.Father = null;
                                if (c.Family.Mother == c) c.Family.Mother = null;
                                if (c.Family.Children != null && c.Family.Children.Contains(c))
                                    c.Family.Children.Remove(c);
                            }
                            // 2. 마을 명부에서 최종 제적
                            town.Citizens.Remove(c);
                        }
                    }
                }

                if (gameHour == 18) 
                    TownSocietyEngine.ProcessEveningSocialTick(town);
                else if (gameHour == 0) 
                    TownSocietyEngine.ProcessDeepNightLifeCycleTick(town);
            }

            // [패치 2번] 알바 게시판 쓰레기 데이터 청소 로직
            if (PartTimeManager.ActiveRequests != null)
            {
                // AI가 수락했거나, 정원이 다 찼거나, 등록된 지 24시간이 지난 악성 재고 의뢰 일괄 삭제
                int removed = PartTimeManager.ActiveRequests.RemoveAll(r => 
                    r.IsAIAssigned || 
                    r.IsFullyBooked || 
                    (DateTime.Now - r.CreationTime).TotalHours >= 24.0);
                
                if (removed > 0)
                {
                    Console.WriteLine($"[JobBoard] 마감되거나 기한이 지난 알바 의뢰 {removed}건이 게시판에서 정리되었습니다.");
                }
            }

            Console.WriteLine($"[MasterTick] 30분 사이클 시민 경제/생존 정산 완료. (게임시간: {gameHour}시)");
        }

		// ========================================================================
        // 🏛️ [검증 및 교정 완료] ExecuteDungeonSecurityImpact 
        // ========================================================================
       // ========================================================================
        // 🏛️ [검증 완료] ExecuteDungeonSecurityImpact
        // ========================================================================
        public static void ExecuteDungeonSecurityImpact()
        {
            if (DungeonManager.Zones == null || DungeonManager.Zones.Count == 0)
                return;

            if (TownEconomyManager.Towns == null || TownEconomyManager.Towns.Count == 0)
                return;

            string[] cityNames = new string[] { "Britain", "Minoc", "Moonglow", "Trinsic", "Vesper", "Luna", "Zento", "Royal City", "Buccaneer's Den", "Jhelom", "Magincia", "Nujel'm", "Haven", "Serpent's Hold", "Skara Brae", "Wind", "Yew", "Delucia", "Papua", "Cove" };
            int[] cityIds = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };

            for (int zIdx = 0; zIdx < DungeonManager.ZoneList.Count; zIdx++)
            {
                DungeonZone zone = DungeonManager.ZoneList[zIdx];
                if (zone == null || !zone.IsActive || zone.CurrentHeat <= 0)
                    continue;

                if (zone.CitySecurityImpact == null || zone.CitySecurityImpact.Count == 0)
                    continue;

                double heatRatio = 0.0;
                if (zone.TargetHeat > 0)
                {
                    heatRatio = (double)zone.CurrentHeat / zone.TargetHeat;
                }
                if (heatRatio > 1.0) heatRatio = 1.0;

                for (int i = 0; i < cityNames.Length; i++)
                {
                    string targetCityName = cityNames[i];

                    if (zone.CitySecurityImpact.TryGetValue(targetCityName, out double impactWeight))
                    {
                        if (impactWeight <= 0.0)
                            continue;

                        int targetCityId = cityIds[i];
                        int fullTownId = 100 + targetCityId; 

                        if (TownEconomyManager.Towns.TryGetValue(fullTownId, out TownEconomy town) && town != null)
                        {
                            int basePenalty = 15;
                            int finalSecurityPenalty = (int)(basePenalty * heatRatio * impactWeight);

                            if (finalSecurityPenalty > 0)
                            {
                                int currentSecurity = town.Security; 
                                int nextSecurity = currentSecurity - finalSecurityPenalty;
                                
                                if (nextSecurity < 0) 
                                    nextSecurity = 0;

                                town.Security = nextSecurity;

                                if (nextSecurity < 30)
                                    town.CrimeIndex += (int)(finalSecurityPenalty * 0.5);
                            }
                        }
                    }
                }
            }
        }

        public static void ProcessCitizenSegment(int tickIdx, int gameHour)
        {
            var towns = TownEconomyManager.Towns.Values.ToList();
            var allCitizens = towns.SelectMany(t => t.Citizens).ToList();
            if (allCitizens.Count == 0) return;

            int startIdx = (allCitizens.Count * (tickIdx - 1)) / 40;
            int endIdx = (allCitizens.Count * tickIdx) / 40;

            for (int i = startIdx; i < endIdx; i++)
            {
                var agent = allCitizens[i];
                if (agent != null && !agent.IsExpired)
                {
                    if (agent.House == null) continue; // 집이 없으면 에러 방지를 위해 스킵

                    int checkHour = (gameHour / 6) * 6;
                    if (agent.LastProcessedHour != checkHour)
                    {
                        agent.LastProcessedHour = checkHour;
                        var town = towns.FirstOrDefault(t => t.Citizens.Contains(agent));
                        if (town != null) ExecuteDeepRoutine(agent, town, checkHour);
                    }
                }
            }
        }

        public static void ProcessQuarterlyRoutine(VirtualCitizen agent, TownEconomy town, int currentHour)
        {
            ExecuteDeepRoutine(agent, town, currentHour);
        }

        public static void ExecuteDeepRoutine(VirtualCitizen agent, TownEconomy town, int currentHour)
        {
            if (agent == null || town == null || agent.IsExpired) return;
            if (agent.House == null) return;

            var profile = VirtualJobCore.GetDeepJobProfile(agent.JobClass);
            int groupID = ((int)agent.JobClass / 100) * 100;
            
            bool isNightShift = (groupID == 800 || groupID == 1100);
            bool isOverworked = agent.IsProductive && (agent.Gold <= 10 || (agent.Family != null && agent.Family.SharedWealth <= 50));

            int logicalHour = isNightShift ? (currentHour + 12) % 24 : currentHour;
            if (logicalHour == 0) logicalHour = 24;

            if (isOverworked && (logicalHour == 18 || logicalHour == 24))
            {
                HandleWork(agent, town, groupID, profile);
                agent.Satisfaction = Math.Max(0, agent.Satisfaction - 10);
                return;
            }

            switch (logicalHour)
            {
                case 6: 
                    if (agent.Family != null && agent.Family.Father == agent && agent.House != null)
                    {
                        CheckAndOrderStorageFurniture(agent.House);
                        VirtualTradeSystem.UpdateHouseWishlist(agent.House);
                        VirtualTradeSystem.GenerateAIJobRequests(agent.House, town);
                    }
                    ProcessNeeds(agent, town, profile); 
                    break;
                case 12: 
                    TryAcceptTownJob(agent, town, groupID); 
                    HandleWork(agent, town, groupID, profile);
                    ProcessNeeds(agent, town, profile);
                    if (agent.Age >= 7.0 && agent.Age <= 16.0) VirtualEducation.ProcessSchool(agent, town); 
                    break;
                case 18: 
                    TryAcceptTownJob(agent, town, groupID); 
                    HandleWork(agent, town, groupID, profile);
                    VirtualTradeSystem.ProcessHoardingShopping(agent, town);
                    ProcessNeeds(agent, town, profile); 
                    ProcessLuxury(agent, town, profile);
                    if (agent.Age >= 7.0 && agent.Age <= 16.0) VirtualEducation.ProcessSchool(agent, town); 
                    break;
                case 24: 
                    agent.UpdateClothingMentalHealth(); 
                    ProcessNeeds(agent, town, profile); 
                    ProcessNightRest(agent, town, groupID); 
                    break;
            }
        }

        private static void CheckAndOrderStorageFurniture(VirtualHouse house)
        {
            if (house == null || house.HouseWarehouse == null || house.MultiID == 0) return; 

            var (maxTypes, maxQuantity) = StorageEconomy.GetStorageLimits(house.HouseWarehouse);
            
            int currentTypes = house.HouseWarehouse.Count;
            int currentQuantity = 0;
            foreach (var amount in house.HouseWarehouse.Values) currentQuantity += amount;

            if (currentQuantity >= maxQuantity * 0.9 || currentTypes >= maxTypes)
            {
                Type targetFurniture = typeof(MediumCrate); 
                
                if (house.PrimaryRank >= NobilityRank.Baron) 
                    targetFurniture = typeof(MetalChest);   
                else if (house.PrimaryRank >= NobilityRank.Knight) 
                    targetFurniture = typeof(WoodenChest);  

                if (!house.UnfulfilledNeeds.ContainsKey(targetFurniture))
                {
                    house.UnfulfilledNeeds[targetFurniture] = 0;
                    house.UnfulfilledNeeds[targetFurniture] += 1;
                    Console.WriteLine($"[Storage] '{house.HouseName}' 가문의 창고가 포화 상태입니다. {targetFurniture.Name} 제작을 의뢰합니다.");
                }
            }
        }

        private static void TryAcceptTownJob(VirtualCitizen agent, TownEconomy town, int groupID)
        {
            if (agent.Stress >= 80 || !agent.IsProductive) return;

            if (PartTimeManager.ActiveRequests == null) return;

            JobCategory[] targetCategories = GetCategoriesForGroup(groupID, agent);
            if (targetCategories.Length == 0) return;

            JobTier maxTier = GetMaxTierForCitizen(agent);

            for (int i = 0; i < PartTimeManager.ActiveRequests.Count; i++)
            {
                var req = PartTimeManager.ActiveRequests[i];
                if (req.TownName != town.TownName) continue;
                if (req.IsFullyBooked) continue;
                if (req.IsAIAssigned) continue; 

                if (!targetCategories.Contains(req.Category)) continue;
                if (req.Tier > maxTier) continue;
                
                bool isDesperate = agent.Gold <= 50 || (agent.Family != null && agent.Family.SharedWealth <= 100);
                double waitTime = isDesperate ? 5.0 : 25.0; 

                if ((DateTime.Now - req.CreationTime).TotalMinutes < waitTime) continue;

                req.CurrentParticipants++;
                req.IsAIAssigned = true;
                
                agent.Gold += req.RewardGold; 
                
                agent.Satisfaction = Math.Min(100, agent.Satisfaction + 15);
                agent.Stress = Math.Max(0, agent.Stress - 5);
                break; 
            }
        }

        private static JobCategory[] GetCategoriesForGroup(int groupID, VirtualCitizen agent)
        {
            string jobName = agent.JobClass.ToString();
            List<JobCategory> cats = [];

            if (groupID == 100) 
            {
                cats.Add(JobCategory.Menial);
                cats.Add(JobCategory.Gathering);
                if (jobName.Contains("Hunter") || jobName.Contains("Fisher") || jobName.Contains("Trapper") || jobName.Contains("Comber"))
                    cats.Add(JobCategory.EcoHunting);
            }
            else if (groupID == 200) cats.Add(JobCategory.Crafting);
            else if (groupID == 300 || groupID == 400) cats.Add(JobCategory.Delivery);
            else if (groupID == 600 || groupID == 700) 
            {
                cats.Add(JobCategory.DungeonHunting);
                cats.Add(JobCategory.EcoHunting);
            }
            else if (groupID == 1100 || jobName.Contains("Thief") || jobName.Contains("Assassin")) 
                cats.Add(JobCategory.BlackMarket);
            
            return cats.ToArray();
        }

        private static JobTier GetMaxTierForCitizen(VirtualCitizen agent)
        {
            if (agent.RankLevel >= NobilityRank.Baron) return JobTier.Special;
            if (agent.RankLevel >= NobilityRank.Baronet) return JobTier.Advanced;
            if (agent.Potential >= 2.0 || agent.RankLevel >= NobilityRank.Knight) return JobTier.Intermediate;
            return JobTier.Beginner;
        }

        private static void HandleWork(VirtualCitizen agent, TownEconomy town, int groupID, DeepJobProfile profile)
        {
            if (agent.Hunger < 20000 || agent.Thirst < 20000)
            {
                agent.Stress = Math.Min(100, agent.Stress + 10);
                agent.Satisfaction = Math.Max(0, agent.Satisfaction - 5);
                
                if (town.Wealth >= 1000)
                {
                    town.Wealth -= 100;
                    agent.Gold += 100;
                }
                return; // 파업 (출근 포기)
            }

            int energyCost = (groupID == 100) ? 15000 : 10000;
            agent.Hunger = Math.Max(0, agent.Hunger - energyCost);
            agent.Thirst = Math.Max(0, agent.Thirst - energyCost);

            if (groupID == 100) 
            {
                if (profile.Produces == null || profile.Produces.Length == 0)
                {
                    agent.Satisfaction = Math.Min(100, agent.Satisfaction + 2);
                    return;
                }

                ResourceType targetType = ResourceType.Farming;
                if (agent.JobClass == NpcJobClass.SurfaceMiner || agent.JobClass == NpcJobClass.StoneQuarryman) targetType = ResourceType.Mining;
                else if (agent.JobClass == NpcJobClass.Woodcutter) targetType = ResourceType.Lumberjacking;
                else if (agent.JobClass == NpcJobClass.DeepSeaFisher) targetType = ResourceType.Fishing;

                string townName = TownNumber.GetName(town.TownID).ToLower();

                var validPools = ResourceManager.PoolList
                    .Where(p => p.Facet == town.Facet && p.Type == targetType && p.CanGather() && !p.IsPrivate);

                if (DungeonManager.Zones != null)
                    validPools = validPools.Where(p => !DungeonManager.Zones.ContainsKey(p.RCode));
                else
                    validPools = validPools.Where(p => p.RegionName != null && !p.RegionName.ToLower().Contains("dungeon"));

                if (targetType != ResourceType.Fishing && agent.Potential < 2.5)
                {
                    var townPools = validPools.Where(p => p.RegionName != null && p.RegionName.ToLower().Contains(townName));
                    if (townPools.Any()) validPools = townPools;
                }

                var nearestPool = validPools
                    .OrderBy(p => Utility.GetDistanceToSqrt(new Point3D(p.CenterX, p.CenterY, 0), town.Center))
                    .FirstOrDefault();

                if (nearestPool != null)
                {
                    EconomyItemKey targetProduce = profile.Produces[Utility.Random(profile.Produces.Length)];
                    int gatherAmount = (int)Math.Max(1, Math.Ceiling(profile.BaseQty * agent.Potential * 1.0)); 
                    int harvested = nearestPool.ConsumeResource(targetProduce.ItemType, gatherAmount);

                    if (harvested > 0)
                    {
                        int basePrice = Math.Max(1, town.GetPrice(targetProduce));
                        VirtualTradeSystem.ExecuteSell(agent, town, targetProduce, basePrice, harvested);
                        agent.Satisfaction = Math.Min(100, agent.Satisfaction + 2);
                    }
                    else agent.Stress = Math.Min(100, agent.Stress + 10);
                }
                else agent.Stress = Math.Min(100, agent.Stress + 15);
            }
            else ProcessProductionTick(agent, town, profile); 
        }

        private static void ApplyFoodMentalEffect(VirtualCitizen agent, EconomyItemKey food)
        {
            if (food.ItemType == null) return;
            
            int stressChange = 0;
            int satisfactionChange = 0;

            bool isFavorite = (food.ItemType == agent.FavoriteFood);
            bool isDisliked = (food.ItemType == agent.DislikedFood);

            int foodTier = GetFoodTier(food.ItemType);
            int expectedTier = agent.RankLevel >= NobilityRank.Baron ? 3 : (agent.RankLevel >= NobilityRank.Knight ? 2 : 1);

            if (agent.FoodDesire == FoodDesireLevel.High)
            {
                if (isFavorite) { stressChange = -20; satisfactionChange = 15; }
                else if (isDisliked) { stressChange = 25; satisfactionChange = -20; }
                
                if (foodTier < expectedTier) { stressChange += 15; satisfactionChange -= 10; }
                else if (foodTier > expectedTier) { stressChange -= 15; satisfactionChange += 10; }
            }
            else if (agent.FoodDesire == FoodDesireLevel.Normal)
            {
                if (isFavorite) { stressChange = -10; satisfactionChange = 5; }
                else if (isDisliked) { stressChange = 10; satisfactionChange = -5; }
                
                if (foodTier < expectedTier) { stressChange += 5; }
            }
            else 
            {
                if (isFavorite) { stressChange = -2; satisfactionChange = 2; }
                if (isDisliked) { stressChange = 2; satisfactionChange = -2; }
            }

            agent.Stress = Math.Clamp(agent.Stress + stressChange, 0, 100);
            agent.Satisfaction = Math.Clamp(agent.Satisfaction + satisfactionChange, 0, 100);
        }

        private static int GetFoodTier(Type t)
        {
            if (t == typeof(ThreeTieredCake) || t == typeof(CookedBird) || t == typeof(FishSteak) || t == typeof(CheesePizza) || t == typeof(Cake)) return 3;
            if (t == typeof(Ribs) || t == typeof(Bacon) || t == typeof(Sausage) || t == typeof(ApplePie) || t == typeof(Ham)) return 2;
            return 1;
        }

        private static void ProcessNeeds(VirtualCitizen agent, TownEconomy town, DeepJobProfile profile)
        {
            int targetThirst = agent.FoodDesire == FoodDesireLevel.High ? 100000 : (agent.FoodDesire == FoodDesireLevel.Normal ? 80000 : 60000);
            int loopCount = 0;
            
            while (agent.Thirst < targetThirst && loopCount < 50)
            {
                loopCount++;
                
                // 🌟 [수정] var 대신 명시적으로 튜플 이름을 지정하여 컴파일 에러(이름 증발) 방지
                (bool Success, EconomyItemKey ConsumedKey) drinkResult = agent.House != null ? agent.House.ConsumeFoodOrDrink(false, 1) : (false, default);
                
                bool drank = false;

                if (drinkResult.Success) drank = true;
                else
                {
                    List<EconomyItemKey> drinks = new List<EconomyItemKey>();
                    if (agent.IsChild && agent.LastProcessedHour == 6) drinks.Add(new EconomyItemKey(typeof(Pitcher), CraftResource.None, (int)BeverageType.Milk));
                    if (agent.LastProcessedHour >= 18 && (agent.Stress > 40 || (int)agent.JobClass < 300))
                    {
                        drinks.Add(new EconomyItemKey(typeof(BeverageBottle), CraftResource.None, (int)BeverageType.Ale));
                        drinks.Add(new EconomyItemKey(typeof(BeverageBottle), CraftResource.None, (int)BeverageType.Wine));
                    }
                    drinks.Add(new EconomyItemKey(typeof(Pitcher), CraftResource.None, (int)BeverageType.Water));

                    var buyResult = TryPurchaseFromList(agent, town, drinks.ToArray(), 1, false); 
                    if (buyResult.Success) drank = true;
                }

                if (drank)
                {
                    agent.Thirst = Math.Min(100000, agent.Thirst + 1500);
                    agent.Satisfaction = Math.Min(100, agent.Satisfaction + 1);
                }
                else
                {
                    agent.Stress = Math.Min(100, agent.Stress + 2); 
                    break; 
                }
            }

            int targetHunger = agent.FoodDesire == FoodDesireLevel.High ? 100000 : (agent.FoodDesire == FoodDesireLevel.Normal ? 80000 : 60000);
            loopCount = 0;
            
            while (agent.Hunger < targetHunger && loopCount < 50)
            {
                loopCount++;
                
                // 🌟 [수정] 여기도 var 대신 명시적으로 튜플 이름을 지정
                (bool Success, EconomyItemKey ConsumedKey) foodResult = agent.House != null ? agent.House.ConsumeFoodOrDrink(true, 1) : (false, default);
                
                EconomyItemKey consumedFood = default;
                bool ate = false;

                if (foodResult.Success)
                {
                    consumedFood = foodResult.ConsumedKey;
                    ate = true;
                }
                else
                {
                    List<EconomyItemKey> searchFoods = new List<EconomyItemKey>();
                    if (agent.FavoriteFood != null) searchFoods.Add(agent.FavoriteFood);
                    if (profile.Necessities != null) searchFoods.AddRange(profile.Necessities);
                    searchFoods.Add(typeof(TroutFishSteak));
                    searchFoods.Add(typeof(BreadLoaf));

                    if (agent.DislikedFood != null) searchFoods.RemoveAll(k => k.ItemType == agent.DislikedFood);

                    var buyResult = TryPurchaseFromList(agent, town, searchFoods.ToArray(), 1, false); 
                    if (buyResult.Success)
                    {
                        consumedFood = buyResult.BoughtItem;
                        ate = true;
                    }
                }

                if (ate)
                {
                    int fillAmount = 1500; 
                    if (consumedFood.ItemType == agent.FavoriteFood) fillAmount = 2500;
                    else if (consumedFood.ItemType == agent.DislikedFood) fillAmount = 1000;

                    if (GetFoodTier(consumedFood.ItemType) == 3) fillAmount += 500; 

                    agent.Hunger += fillAmount;

                    if (agent.FoodDesire == FoodDesireLevel.High && consumedFood.ItemType == agent.FavoriteFood)
                        agent.Hunger = Math.Min(120000, agent.Hunger); 
                    else
                        agent.Hunger = Math.Min(100000, agent.Hunger);

                    ApplyFoodMentalEffect(agent, consumedFood);
                }
                else
                {
                    agent.Stress = Math.Min(100, agent.Stress + 5);
                    break;
                }
            }
        }

        private static void ProcessLuxury(VirtualCitizen agent, TownEconomy town, DeepJobProfile profile)
        {
            if (agent.Stress > 40 && profile.Luxuries != null && profile.Luxuries.Length > 0)
            {
                var (success, _, spent) = TryPurchaseFromList(agent, town, profile.Luxuries);
                if (success)
                {
                    int relief = 30 + (spent / 100);
                    agent.Stress = Math.Max(0, agent.Stress - relief);
                    agent.Satisfaction = Math.Min(100, agent.Satisfaction + 20);
                    agent.Fame += 2;
                }
                else agent.Stress = Math.Min(100, agent.Stress + 5);
            }
        }

        private static (bool IsProfitable, int Cost) CheckProfitability(TownEconomy town, Type produce)
        {
            Type[] ingredients = GetRecipeIngredients(produce);
            if (ingredients == null || ingredients.Length == 0) return (true, 0); 

            int totalCost = 0;
            foreach (var ing in ingredients)
            {
                totalCost += town.GetPrice(new EconomyItemKey(ing));
            }

            int sellPrice = town.GetPrice(new EconomyItemKey(produce));
            return (totalCost < sellPrice * 0.9, totalCost);
        }

        private static Type[] GetRecipeIngredients(Type produce)
        {
            if (produce == typeof(Arrow) || produce == typeof(Bolt)) return new[] { typeof(Shaft), typeof(Feather) };
            if (produce == typeof(LesserHealPotion) || produce == typeof(LesserCurePotion)) return new[] { typeof(Bottle), typeof(Ginseng) };
            if (produce == typeof(HealPotion) || produce == typeof(CurePotion)) return new[] { typeof(Bottle), typeof(Ginseng), typeof(Ginseng) };
            if (produce == typeof(RecallScroll) || produce == typeof(FireballScroll)) return new[] { typeof(BlankScroll), typeof(BlackPearl), typeof(Bloodmoss), typeof(MandrakeRoot) };
            if (produce == typeof(Spellbook)) return new[] { typeof(BlankScroll), typeof(Leather) };
            if (produce == typeof(SackFlour)) return new[] { typeof(WheatSheaf) };
            if (produce == typeof(BreadLoaf)) return new[] { typeof(SackFlour) };
            if (produce == typeof(CheesePizza)) return new[] { typeof(SackFlour), typeof(CheeseWheel) };
            if (produce == typeof(Bottle)) return new[] { typeof(Sand) };
            if (produce == typeof(Board)) return new[] { typeof(Log) };
            if (produce == typeof(IronIngot)) return new[] { typeof(IronOre) };
            return null;
        }

        private static void ProcessProductionTick(VirtualCitizen agent, TownEconomy town, DeepJobProfile profile)
        {
            if (!agent.IsProductive || agent.Stress >= 90) return;

            if (profile.Addons != null && profile.Addons.Length > 0 && agent.House != null && agent.House.MultiID > 0)
            {
                bool hasWorkshopAddon = false;
                foreach (var addonType in profile.Addons)
                {
                    if (agent.House.HouseWarehouse != null && agent.House.HouseWarehouse.Keys.Any(k => k.ItemType == addonType.ItemType))
                    {
                        hasWorkshopAddon = true;
                        break;
                    }
                }

                if (!hasWorkshopAddon)
                {
                    EconomyItemKey neededAddon = profile.Addons[Utility.Random(profile.Addons.Length)];
                    if (!agent.House.UnfulfilledNeeds.ContainsKey(neededAddon))
                        agent.House.UnfulfilledNeeds[neededAddon] = 0;
                    
                    agent.House.UnfulfilledNeeds[neededAddon] += 1;
                    agent.Stress = Math.Min(100, agent.Stress + 5);
                    return; 
                }
            }

            double workshopSuccessBonus = 0.0;
            double workshopExcBonus = 0.0;
            double workshopSaveBonus = 0.0;

            if (agent.House != null && agent.House.HouseWarehouse != null)
            {
                workshopSuccessBonus = WorkshopEconomy.GetFinalBonus(agent.House.HouseWarehouse, profile.Skill, WorkshopBonusType.SuccessRate);
                workshopExcBonus = WorkshopEconomy.GetFinalBonus(agent.House.HouseWarehouse, profile.Skill, WorkshopBonusType.ExceptionalChance);
                workshopSaveBonus = WorkshopEconomy.GetFinalBonus(agent.House.HouseWarehouse, profile.Skill, WorkshopBonusType.ResourceSave);
            }

            double focus = agent.Bio != null ? Math.Max(0, agent.Bio.Focus / 1000000.0) : 0;
            double adaptability = agent.Bio != null ? Math.Max(0, agent.Bio.Adaptability / 1000000.0) : 0;
            double metabolism = agent.Bio != null ? Math.Max(0, agent.Bio.Metabolism / 1000000.0) : 0;

            if (profile.JobMaterials != null && profile.JobMaterials.Length > 0)
            {
                if (!TryPurchaseFromList(agent, town, profile.JobMaterials).Success)
                {
                    agent.Stress = Math.Min(100, agent.Stress + 10);
                    return;
                }
            }

            double successChance = 0.2 + (0.8 * (agent.PrimarySkill / 200.0)) + (0.2 * focus) + workshopSuccessBonus;
            successChance = Math.Min(1.0, successChance);
                
            if (Utility.RandomDouble() < successChance && profile.Produces != null && profile.Produces.Length > 0)
            {
                EconomyItemKey targetProduce = profile.Produces[Utility.Random(profile.Produces.Length)];
                
                var profitCheck = CheckProfitability(town, targetProduce.ItemType);
                if (!profitCheck.IsProfitable)
                {
                    agent.Stress = Math.Min(100, agent.Stress + 5);
                    TryPurchaseFromList(agent, town, new EconomyItemKey[] { targetProduce }, 1);
                    return; 
                }

                bool isExceptional = Utility.RandomDouble() < (0.05 + workshopExcBonus);

                double rankMult = 1.0 + ((int)agent.RankLevel * 0.1); 
                double ageFactor = agent.IsElder ? 0.5 : 1.0;
                int baseWorkshopBonus = (workshopSuccessBonus > 0) ? 1 : 0;
                double adaptMult = 1.0 + (0.3 * adaptability);
                
                int finalQty = (int)Math.Max(1, Math.Ceiling(profile.BaseQty * 1.0 * agent.Potential * rankMult * ageFactor * adaptMult)) + baseWorkshopBonus;
                
                if (Utility.RandomDouble() < workshopSaveBonus)
                {
                    finalQty = (int)(finalQty * 1.5);
                }

                int basePrice = Math.Max(1, town.GetPrice(targetProduce));
                if (isExceptional) basePrice = (int)(basePrice * 1.5); 
                if (isExceptional) targetProduce.IsExceptional = true;

                if (VirtualTradeSystem.ExecuteSell(agent, town, targetProduce, basePrice, finalQty).Success)
                {
                    agent.CheckSkillGain(); 
                    int stressGain = Math.Max(1, 5 - (int)(2 * metabolism));
                    agent.Stress = Math.Min(100, agent.Stress + stressGain);
                    agent.Fame += 1;
                }
            }
            else 
            {
                int failStress = Math.Max(2, 8 - (int)(4 * focus));
                agent.Stress = Math.Min(100, agent.Stress + failStress);
            }
        }

        private static void ProcessNightRest(VirtualCitizen agent, TownEconomy town, int groupID)
        {
            agent.Stress = Math.Max(0, agent.Stress - Utility.RandomMinMax(10, 20)); 
            agent.Satisfaction = Math.Min(100, agent.Satisfaction + 5);

            if (groupID == 500 || groupID == 200)
            {
                if (!TryPurchaseFromList(agent, town, [typeof(Candle)]).Success)
                {
                    agent.Satisfaction = Math.Max(0, agent.Satisfaction - 10);
                }
            }
        }

        private static (bool Success, EconomyItemKey BoughtItem, int Spent) TryPurchaseFromList(VirtualCitizen agent, TownEconomy town, EconomyItemKey[] itemList, int amount = 1, bool shuffle = true)
        {
            if (itemList == null || itemList.Length == 0) return (true, default, 0);

            var searchList = itemList.ToList();
            
            if (shuffle)
            {
                if (searchList.Contains(typeof(Pickaxe)) && !searchList.Contains(typeof(Shovel)))
                    searchList.Add(typeof(Shovel));

                for (int i = searchList.Count - 1; i > 0; i--)
                {
                    int swapIndex = Utility.Random(i + 1);
                    (searchList[i], searchList[swapIndex]) = (searchList[swapIndex], searchList[i]);
                }

                if (searchList.Contains(typeof(Shovel)))
                {
                    searchList.Remove(typeof(Shovel));
                    searchList.Insert(0, typeof(Shovel)); 
                }
            }

            foreach (var itemKey in searchList)
            {
                int basePrice = Math.Max(1, town.GetPrice(itemKey));
                var result = VirtualTradeSystem.ExecutePurchase(agent, town, itemKey, basePrice, amount, true);
                
                if (result.Success) return (true, itemKey, result.Spent); 
            }
            return (false, default, 0);
        }
    }

    // ==============================================================================
    // 3. [VirtualEducation] 학교 및 교육 로직
    // ==============================================================================
    public static class VirtualEducation
    {
        public static void ProcessSchool(VirtualCitizen agent, TownEconomy town)
        {
            if (agent == null || town == null || agent.Family == null) return;

            var track = DetermineTrack(agent);
            var teacher = SelectTeacher(town, agent, track);
            if (teacher == null) return;

            var (success, fee) = ChargeTuition(agent, town, track, teacher);

            if (success) ApplyEducationEffects(agent, teacher, track, fee);
            else
            {
                agent.Stress = Math.Min(100, agent.Stress + 5);
                agent.Satisfaction = Math.Max(0, agent.Satisfaction - 2);
            }
        }

        private static string DetermineTrack(VirtualCitizen agent)
        {
            if (agent.RankLevel >= NobilityRank.Baron && agent.Fame >= 5000) return "Elite";
            if (agent.RankLevel >= NobilityRank.Knight) return (Utility.RandomDouble() < 0.01) ? "Workshop" : "Academy"; 
            return (agent.Family.SharedWealth >= 1000) ? "Academy" : "Workshop";
        }

        private static VirtualCitizen SelectTeacher(TownEconomy town, VirtualCitizen student, string track)
        {
            int[] targetGroups = track switch
            {
                "Elite" or "Academy" => [300, 400, 500, 700, 1000], 
                _ => [100, 200] 
            };

            VirtualCitizen bestTeacher = null;
            double maxAge = -1;

            for (int i = 0; i < town.Citizens.Count; i++)
            {
                var c = town.Citizens[i];
                if (!c.IsChild && targetGroups.Contains(((int)c.JobClass / 100) * 100))
                {
                    if (c.Age > maxAge)
                    {
                        maxAge = c.Age;
                        bestTeacher = c;
                    }
                }
            }
            return bestTeacher;
        }

        public static (bool Success, int Amount) ChargeTuition(VirtualCitizen agent, TownEconomy town, string track, VirtualCitizen teacher)
        {
            int tuition = track switch { "Elite" => 50000, "Academy" => 5000, _ => 500 };

            if (agent.Family.SharedWealth >= tuition)
            {
                agent.Family.SharedWealth -= tuition;
                int teacherPay = (int)(tuition * 0.3);
                teacher.Gold += teacherPay;
                town.Wealth += (tuition - teacherPay);
                return (true, tuition);
            }
            return (false, 0);
        }

        private static void ApplyEducationEffects(VirtualCitizen agent, VirtualCitizen teacher, string track, int fee)
        {
            agent.Satisfaction = Math.Min(100, agent.Satisfaction + 5);
            agent.Karma = (int)(agent.Karma * 0.9 + teacher.Karma * 0.1);

            double potentialGain = track switch { "Elite" => 0.2, "Academy" => 0.15, _ => 0.05 };
            int fameWeight = track switch { "Elite" => 20, "Academy" => 10, _ => 2 };

            if (agent.IsChild && Utility.RandomDouble() < 0.15) agent.Potential = Math.Min(3.0, agent.Potential + potentialGain);
            agent.Fame += (int)((fee / (double)fameWeight) * agent.Potential);

            SkillName[] targetPool = (track == "Workshop") 
                ? [SkillName.Blacksmith, SkillName.Tailoring, SkillName.Carpentry, SkillName.Mining]
                : [SkillName.EvalInt, SkillName.Magery, SkillName.Tactics, SkillName.Inscribe];

            SkillName targetSkill = targetPool[Utility.Random(targetPool.Length)];
            double currentVal = agent.Skills.TryGetValue(targetSkill, out var v) ? v : 0.0;
            agent.Skills[targetSkill] = Math.Min(100.0, currentVal + (track == "Elite" ? 1.0 : 0.5));
        }
    }

   // ==============================================================================
    // 4. [VirtualJobCore] 직업 프로필 매핑
    // ==============================================================================
    public static class VirtualJobCore
    {
        public static readonly NpcJobClass[] AllJobs = Enum.GetValues<NpcJobClass>();
        public static readonly SkillName[] AllSkills = Enum.GetValues<SkillName>();

        public static DeepJobProfile GetDeepJobProfile(NpcJobClass job)
        {
            var profile = job switch
            {
                NpcJobClass.Pauper => new DeepJobProfile(SkillName.Begging, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [], [], [], [], 0),
                NpcJobClass.Beggar => new DeepJobProfile(SkillName.Begging, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins), typeof(Carrot)], [], [], [], [], 0),
                NpcJobClass.Laborer => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(BreadLoaf)], [typeof(Shoes)], [typeof(Candle)], [], [], 0),
                NpcJobClass.StreetSweeper => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Boots)], [typeof(Muffins)], [typeof(Bone)], [], 2),
                NpcJobClass.WaterCarrier => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Turnip)], [typeof(Pouch)], [typeof(Sandals)], [], [], 20),
                NpcJobClass.NightSoilMan => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Cabbage)], [typeof(Shovel), typeof(Torch)], [typeof(Candle)], [typeof(FertileDirt)], [], 5),
                NpcJobClass.GongFarmer => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Onion)], [typeof(Shovel), typeof(Boots)], [typeof(Torch)], [typeof(FertileDirt)], [], 5),
                NpcJobClass.RatCatcher => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Dagger)], [typeof(Bandage)], [typeof(RawRibs)], [], 3),
                NpcJobClass.ChimneySweep => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Pear)], [typeof(Candle), typeof(Bandage)], [typeof(Shoes)], [typeof(GraveDust)], [], 4),
                NpcJobClass.Lamplighter => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(Torch), typeof(Lantern), typeof(OilFlask)], [typeof(Boots)], [], [], 0),
                NpcJobClass.LinkBoy => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Torch)], [typeof(Shoes)], [], [], 0),
                
                NpcJobClass.GraveDigger_Basic => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(BreadLoaf)], [typeof(Shovel), typeof(Torch)], [new EconomyItemKey(typeof(BeverageBottle), CraftResource.None, (int)BeverageType.Ale)], [typeof(GraveDust), typeof(Bone)], [], 5),
                
                NpcJobClass.Scullion => new DeepJobProfile(SkillName.Cooking, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(TroutRawFishSteak), typeof(HalfApron), typeof(Skillet)], [typeof(Candle)], [typeof(TroutFishSteak)], [typeof(StoneOvenEastDeed)], 20),
                NpcJobClass.GrainFarmer => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Pitchfork)], [typeof(StrawHat)], [typeof(EarOfCorn)], [], 20),
                NpcJobClass.VegetableFarmer => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Pear)], [typeof(Pitchfork)], [typeof(Shoes)], [typeof(Carrot), typeof(Onion), typeof(Cabbage), typeof(Lettuce), typeof(Turnip)], [], 18),
                NpcJobClass.GourdFarmer => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Shovel)], [typeof(Boots)], [typeof(Pumpkin), typeof(Squash), typeof(GreenGourd), typeof(YellowGourd)], [], 15),
                NpcJobClass.Orchardist => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(Basket), typeof(Bag)], [typeof(FloppyHat)], [typeof(Apple), typeof(Pear), typeof(Peach)], [], 25),
                NpcJobClass.CitrusGrower => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(Basket)], [typeof(StrawHat)], [typeof(Lemon), typeof(Lime)], [], 15),
                NpcJobClass.VineyardWorker => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(BreadLoaf)], [typeof(Scissors)], [typeof(Bandana)], [typeof(Grapes)], [], 20),
                NpcJobClass.BerryPicker => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Pouch)], [typeof(Sandals)], [typeof(ParasiticPlant)], [], 10),
                NpcJobClass.Herbalist => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Pear)], [typeof(Dagger), typeof(Bag)], [typeof(Cap)], [typeof(Garlic), typeof(Ginseng)], [], 15),
                NpcJobClass.MushroomGatherer => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(Candle), typeof(Pouch)], [typeof(Shoes)], [typeof(MandrakeRoot), typeof(Nightshade), typeof(SpidersSilk)], [], 8),
                NpcJobClass.Beekeeper => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Torch)], [typeof(HalfApron)], [typeof(JarHoney)], [], 10),
                NpcJobClass.CoastalFisher => new DeepJobProfile(SkillName.Fishing, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(FishingPole)], [typeof(ThighBoots)], [typeof(Trout)], [], 15),
                NpcJobClass.DeepSeaFisher_Basic => new DeepJobProfile(SkillName.Fishing, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(BreadLoaf)], [typeof(SpecialFishingNet)], [typeof(TricorneHat)], [typeof(Trout)], [], 25),
                NpcJobClass.OysterDiver_Basic => new DeepJobProfile(SkillName.Fishing, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(SkinningKnife)], [typeof(Bandana)], [typeof(BlackPearl)], [], 5),
                NpcJobClass.SeaweedCollector => new DeepJobProfile(SkillName.Fishing, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Pear)], [typeof(Bag), typeof(Sandals)], [typeof(Candle)], [typeof(FertileDirt)], [], 8),
                NpcJobClass.BeachComber => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(Spyglass), typeof(Torch)], [typeof(Boots)], [typeof(Bone)], [], 3),
                NpcJobClass.SaltGatherer => new DeepJobProfile(SkillName.Mining, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Shovel)], [typeof(Shoes)], [typeof(Sand)], [], 10), 
                NpcJobClass.Shepherd => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(BreadLoaf)], [typeof(ShepherdsCrook)], [typeof(StrawHat)], [typeof(Wool)], [], 15),
                NpcJobClass.Swineherd => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Carrot)], [typeof(Cleaver)], [typeof(ShortPants)], [typeof(RawRibs), typeof(Bacon)], [], 12),
                NpcJobClass.PoultryFarmer => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(EarOfCorn)], [typeof(Basket)], [typeof(HalfApron)], [typeof(Eggs), typeof(Feather), typeof(RawBird), typeof(RawChickenLeg)], [], 18),
                NpcJobClass.CattleDrover => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(RawRibs)], [typeof(Whip)], [typeof(Boots)], [typeof(Hides), typeof(RawRibs)], [], 10),
                NpcJobClass.StableHand => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(EarOfCorn)], [typeof(Pitchfork)], [typeof(Shirt)], [], [], 0),
                NpcJobClass.GooseHerd => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(QuarterStaff)], [typeof(Sandals)], [typeof(Feather), typeof(Eggs)], [], 12),
                
                NpcJobClass.Woodcutter => new DeepJobProfile(SkillName.Lumberjacking, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(BreadLoaf)], [typeof(Axe), typeof(TwoHandedAxe), typeof(Hatchet)], [typeof(Doublet)], [typeof(Log)], [], 25),
                NpcJobClass.BarkCollector => new DeepJobProfile(SkillName.Lumberjacking, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(SkinningKnife)], [typeof(Cap)], [typeof(BarkFragment)], [], 15),
                NpcJobClass.SurfaceMiner => new DeepJobProfile(SkillName.Mining, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(Pickaxe)], [typeof(Boots)], [typeof(IronOre)], [], 20),
                NpcJobClass.SandDigger => new DeepJobProfile(SkillName.Mining, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Shovel), typeof(Bag)], [typeof(Sandals)], [typeof(Sand)], [], 25),
                NpcJobClass.StoneQuarryman => new DeepJobProfile(SkillName.Mining, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(BreadLoaf)], [typeof(Pickaxe)], [typeof(LeatherGloves)], [typeof(IronOre)], [], 15),
                NpcJobClass.FlintKnapper => new DeepJobProfile(SkillName.Mining, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Hammer)], [typeof(Bandana)], [typeof(IronOre)], [], 5),
                NpcJobClass.Trapper => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Sausage)], [typeof(SkinningKnife)], [typeof(LeatherCap)], [typeof(Hides), typeof(RawRibs)], [], 8),
                NpcJobClass.BirdHunter => new DeepJobProfile(SkillName.Archery, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Bow), typeof(Arrow)], [typeof(TallStrawHat)], [typeof(Feather), typeof(RawBird)], [], 15),
                NpcJobClass.BigGameHunter => new DeepJobProfile(SkillName.Tactics, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Bacon)], [typeof(Spear), typeof(Bandage)], [typeof(ThighBoots)], [typeof(TigerPelt), typeof(RawRibs), typeof(Bone)], [], 5),
                NpcJobClass.FeatherPlucker => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(HalfApron)], [typeof(Candle)], [typeof(Feather)], [], 20),

                NpcJobClass.Smelter => new DeepJobProfile(SkillName.Blacksmith, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(FrenchBread)], [typeof(IronOre), typeof(Tongs)], [typeof(SilverRing)], [typeof(IronIngot)], [typeof(SmallForgeDeed)], 15),
                NpcJobClass.PigIronWorker => new DeepJobProfile(SkillName.Blacksmith, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(CheesePizza)], [typeof(IronIngot), typeof(SmithHammer)], [new EconomyItemKey(typeof(BeverageBottle), CraftResource.None, (int)BeverageType.Ale)], [typeof(PigIron), typeof(Pickaxe), typeof(Shovel)], [typeof(LargeForgeEastDeed), typeof(AnvilEastDeed)], 15),
                NpcJobClass.NailMaker => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(BreadLoaf)], [typeof(IronIngot), typeof(Hammer)], [typeof(Shirt)], [typeof(Nails)], [typeof(TinkerBenchDeed)], 25),
                NpcJobClass.AxleMaker => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Sausage)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(Boots)], [typeof(Axle)], [typeof(TinkerBenchDeed)], 10),
                NpcJobClass.GearCutter => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Ham)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(SilverBracelet)], [typeof(Gears), typeof(AxleGears)], [typeof(TinkerBenchDeed)], 10),
                NpcJobClass.SpringMaker => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(ApplePie)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(FancyShirt)], [typeof(Springs)], [typeof(TinkerBenchDeed)], 15),
                NpcJobClass.HingeMaker => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Cookies)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(PlainDress)], [typeof(Hinge)], [typeof(TinkerBenchDeed)], 20),
                NpcJobClass.ClockPartMaker => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Cake)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(SilverNecklace)], [typeof(ClockParts)], [typeof(TinkerBenchDeed)], 5),
                NpcJobClass.SextantPartMaker => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(JarHoney)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(SilverEarrings)], [typeof(SextantParts)], [typeof(TinkerBenchDeed)], 5),
                NpcJobClass.Weaver => new DeepJobProfile(SkillName.Tailoring, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(FrenchBread)], [typeof(Wool), typeof(Flax), typeof(Cotton)], [typeof(FancyDress)], [typeof(BoltOfCloth), typeof(UncutCloth)], [typeof(LoomEastDeed)], 15),
                NpcJobClass.Spinner => new DeepJobProfile(SkillName.Tailoring, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Muffins)], [typeof(Wool), typeof(Cotton)], [typeof(PlainDress)], [typeof(SpoolOfThread), typeof(DarkYarn), typeof(LightYarn)], [typeof(SpinningwheelEastDeed)], 20),
                NpcJobClass.LeatherTanner => new DeepJobProfile(SkillName.Tailoring, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(RawRibs)], [typeof(Hides), typeof(Scissors), typeof(SewingKit)], [typeof(StuddedGloves)], [typeof(LeatherChest), typeof(LeatherLegs), typeof(LeatherCap), typeof(LeatherGloves)], [], 8),
                NpcJobClass.Dyer_Producer => new DeepJobProfile(SkillName.Tailoring, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(ApplePie)], [typeof(Dyes), typeof(DyeTub)], [typeof(GoldRing)], [typeof(Shirt), typeof(ShortPants), typeof(PlainDress)], [], 10),
                NpcJobClass.Sawyer => new DeepJobProfile(SkillName.Carpentry, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(BreadLoaf)], [typeof(Log), typeof(Saw)], [typeof(SilverRing)], [typeof(Board)], [typeof(WoodworkersBenchDeed)], 25),
                NpcJobClass.ShaftMaker => new DeepJobProfile(SkillName.Carpentry, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Muffins)], [typeof(Board), typeof(DrawKnife)], [typeof(Bandana)], [typeof(Shaft)], [typeof(WoodworkersBenchDeed)], 30),
                NpcJobClass.BarrelMaker_Base => new DeepJobProfile(SkillName.Carpentry, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Ham)], [typeof(Board), typeof(IronIngot), typeof(DovetailSaw)], [typeof(Boots)], [typeof(Barrel)], [typeof(WoodworkersBenchDeed)], 5),
                NpcJobClass.BoxMaker_Base => new DeepJobProfile(SkillName.Carpentry, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Bacon)], [typeof(Board), typeof(Nails), typeof(JointingPlane)], [typeof(Shoes)], [typeof(WoodenBox), typeof(SmallCrate), typeof(MediumCrate), typeof(LargeCrate), typeof(WoodenChest)], [typeof(WoodworkersBenchDeed)], 6),
                NpcJobClass.Miller => new DeepJobProfile(SkillName.Cooking, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Apple)], [typeof(EarOfCorn), typeof(FlourSifter)], [typeof(SilverBracelet)], [typeof(SackFlour), typeof(BowlFlour)], [typeof(FlourMillEastDeed)], 15),
                
                NpcJobClass.Butcher_Expert => new DeepJobProfile(SkillName.Cooking, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(RawRibs)], [typeof(RawLambLeg), typeof(RawRibs), typeof(Cleaver), typeof(ButcherKnife)], [new EconomyItemKey(typeof(BeverageBottle), CraftResource.None, (int)BeverageType.Ale)], [typeof(Bacon), typeof(Ham), typeof(Sausage)], [typeof(BBQSmokerDeed)], 20),
                NpcJobClass.PoultryProcessor => new DeepJobProfile(SkillName.Cooking, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(RawBird)], [typeof(RawBird), typeof(ButcherKnife)], [typeof(HalfApron)], [typeof(RawChickenLeg)], [], 25),
                NpcJobClass.PizzaChef_Producer => new DeepJobProfile(SkillName.Cooking, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(SackFlour)], [typeof(SackFlour), typeof(RollingPin)], [typeof(GoldRing)], [typeof(CheesePizza), typeof(BreadLoaf), typeof(FrenchBread)], [typeof(StoneOvenEastDeed)], 10),
                NpcJobClass.GlassBlower => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.Baronet, [typeof(Cake)], [typeof(Sand), typeof(TinkersTools)], [typeof(SilverNecklace)], [typeof(Bottle), typeof(SolventFlask)], [typeof(TinkerBenchDeed)], 40),
                NpcJobClass.AshProcessor => new DeepJobProfile(SkillName.Alchemy, NobilityRank.Commoner, NobilityRank.Baronet, [typeof(Muffins)], [typeof(Log), typeof(Torch)], [typeof(Boots)], [typeof(SulfurousAsh)], [typeof(HeatingStand)], 20),
                NpcJobClass.BoneGrinder => new DeepJobProfile(SkillName.Alchemy, NobilityRank.Commoner, NobilityRank.Baronet, [typeof(BreadLoaf)], [typeof(Bone), typeof(MortarPestle)], [typeof(Shoes)], [typeof(GraveDust)], [typeof(AlchemyStationDeed)], 15),
                NpcJobClass.CandleDipper => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.Baronet, [typeof(ApplePie)], [typeof(JarHoney), typeof(SpoolOfThread)], [typeof(PlainDress)], [typeof(Candle)], [typeof(TinkerBenchDeed)], 25),
                NpcJobClass.JewelryBaseMaker => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.Baronet, [typeof(FrenchBread)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(SilverRing)], [typeof(GoldRing), typeof(GoldNecklace), typeof(SilverRing), typeof(SilverNecklace)], [typeof(TinkerBenchDeed)], 5),
                NpcJobClass.BeadMaker => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.Baronet, [typeof(Cookies)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(SilverBracelet)], [typeof(Beads)], [typeof(TinkerBenchDeed)], 15),

                NpcJobClass.Bowyer => new DeepJobProfile(SkillName.Fletching, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Apple)], [typeof(Board), typeof(Shaft), typeof(Feather), typeof(FletcherTools)], [typeof(Bandana)], [typeof(Arrow), typeof(Bolt), typeof(Bow), typeof(Crossbow)], [typeof(FletchingStationDeed)], 30),

                NpcJobClass.Knight => new DeepJobProfile(SkillName.Swords, NobilityRank.Knight, NobilityRank.Baron, [typeof(FrenchBread), typeof(Ham)], [typeof(PlateChest), typeof(PlateLegs), typeof(MetalKiteShield), typeof(Longsword), typeof(Bandage)], [typeof(GoldRing), typeof(Cloak)], [typeof(Gold), typeof(DragonBlood), typeof(DaemonBone)], [], 30),
                NpcJobClass.Halberdier => new DeepJobProfile(SkillName.Tactics, NobilityRank.Knight, NobilityRank.Baron, [typeof(Sausage), typeof(CheesePizza)], [typeof(PlateChest), typeof(Halberd), typeof(Bandage)], [typeof(SilverNecklace)], [typeof(Gold), typeof(DaemonBlood)], [], 25),
                
                NpcJobClass.TownGuard => new DeepJobProfile(SkillName.Swords, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Bacon), typeof(BreadLoaf)], [typeof(ChainChest), typeof(Broadsword), typeof(Bandage)], [new EconomyItemKey(typeof(BeverageBottle), CraftResource.None, (int)BeverageType.Ale), typeof(Shoes)], [typeof(Gold), typeof(Hides)], [], 15),
                
                NpcJobClass.Duelist => new DeepJobProfile(SkillName.Fencing, NobilityRank.Knight, NobilityRank.Baronet, [typeof(CookedBird)], [typeof(StuddedChest), typeof(Kryss), typeof(Bandage)], [typeof(GoldEarrings), typeof(FancyShirt)], [typeof(Gold)], [], 20),
                NpcJobClass.Archer_Expert => new DeepJobProfile(SkillName.Archery, NobilityRank.Knight, NobilityRank.Baronet, [typeof(Ham), typeof(ApplePie)], [typeof(Bow), typeof(Arrow), typeof(LeatherChest)], [typeof(TricorneHat)], [typeof(Gold), typeof(Feather), typeof(TigerPelt)], [], 20),
                NpcJobClass.Crossbowman => new DeepJobProfile(SkillName.Archery, NobilityRank.Knight, NobilityRank.Baronet, [typeof(Sausage)], [typeof(Crossbow), typeof(HeavyCrossbow), typeof(Bolt), typeof(StuddedChest)], [typeof(Boots)], [typeof(Gold), typeof(Bone)], [], 18),
                NpcJobClass.UndeadHunter => new DeepJobProfile(SkillName.Macing, NobilityRank.Knight, NobilityRank.SubBaron, [typeof(FrenchBread)], [typeof(Mace), typeof(WarMace), typeof(ChainChest), typeof(Bandage)], [typeof(SilverRing)], [typeof(Gold), typeof(GraveDust), typeof(DaemonBone), typeof(Bone)], [], 25),
                NpcJobClass.DragonTracker => new DeepJobProfile(SkillName.Tactics, NobilityRank.Knight, NobilityRank.Baron, [typeof(Ham)], [typeof(Spear), typeof(PlateChest), typeof(GreaterHealPotion)], [typeof(GoldBracelet)], [typeof(Gold), typeof(DragonBlood), typeof(DragonTurtleScute)], [], 15),

                NpcJobClass.Wizard => new DeepJobProfile(SkillName.Magery, NobilityRank.SubBaronet, NobilityRank.Viscount, [typeof(Cake), typeof(FrenchBread)], [typeof(BlackPearl), typeof(Bloodmoss), typeof(BlankScroll), typeof(Spellbook)], [typeof(Sapphire), typeof(Robe)], [typeof(RecallScroll), typeof(FireballScroll), typeof(LightningScroll)], [], 8),
                NpcJobClass.Archmage => new DeepJobProfile(SkillName.Magery, NobilityRank.Baronet, NobilityRank.Count, [typeof(Cake), typeof(JarHoney)], [typeof(MandrakeRoot), typeof(SpidersSilk), typeof(SulfurousAsh), typeof(BlankScroll)], [typeof(StarSapphire), typeof(MagicWizardsHat)], [typeof(GateTravelScroll), typeof(EnergyBoltScroll), typeof(ExplosionScroll), typeof(MeteorSwarmScroll)], [], 5),
                NpcJobClass.Alchemist => new DeepJobProfile(SkillName.Alchemy, NobilityRank.SubBaronet, NobilityRank.Baron, [typeof(Cookies), typeof(Muffins)], [typeof(Ginseng), typeof(Garlic), typeof(Bottle), typeof(MortarPestle)], [typeof(SilverNecklace)], [typeof(LesserHealPotion), typeof(HealPotion), typeof(GreaterHealPotion), typeof(LesserCurePotion), typeof(CurePotion), typeof(GreaterCurePotion)], [typeof(AlchemyStationDeed)], 15),
                NpcJobClass.PotionMaker => new DeepJobProfile(SkillName.Alchemy, NobilityRank.SubBaronet, NobilityRank.Baron, [typeof(ApplePie)], [typeof(Nightshade), typeof(SulfurousAsh), typeof(Bottle), typeof(MortarPestle)], [typeof(SilverRing)], [typeof(LesserPoisonPotion), typeof(PoisonPotion), typeof(GreaterPoisonPotion), typeof(LesserExplosionPotion), typeof(ExplosionPotion), typeof(GreaterExplosionPotion)], [typeof(AlchemyStationDeed)], 15),
                NpcJobClass.Scribe_Mage => new DeepJobProfile(SkillName.Inscribe, NobilityRank.SubBaronet, NobilityRank.Viscount, [typeof(CheesePizza)], [typeof(BlankScroll), typeof(ScribesPen), typeof(BlackPearl)], [typeof(Amethyst)], [typeof(Spellbook), typeof(Magerybook), typeof(RecallScroll)], [typeof(WritingDeskDeed)], 5),
                NpcJobClass.Necromancer => new DeepJobProfile(SkillName.Magery, NobilityRank.SubBaronet, NobilityRank.Viscount, [typeof(RawRibs)], [typeof(GraveDust), typeof(BatWing), typeof(DaemonBlood), typeof(BlankScroll)], [typeof(Robe), typeof(SkullCap)], [typeof(NecromancerSpellbook), typeof(PoisonFieldScroll)], [], 4),

                NpcJobClass.Mayor => new DeepJobProfile(SkillName.ItemID, NobilityRank.Baron, NobilityRank.Marquis, [typeof(Cake), typeof(CookedBird)], [typeof(BlankScroll), typeof(ScribesPen)], [typeof(StarSapphire), typeof(Diamond), typeof(GoldBracelet), typeof(Throne)], [typeof(CommissionContractOfEmployment)], [], 1),
                NpcJobClass.TaxCollector_Noble => new DeepJobProfile(SkillName.ItemID, NobilityRank.Baron, NobilityRank.Marquis, [typeof(JarHoney)], [typeof(BlankScroll), typeof(ScribesPen)], [typeof(GoldNecklace), typeof(Ruby)], [typeof(VendorRentalContract)], [], 2),
                NpcJobClass.Aristocrat => new DeepJobProfile(SkillName.ItemID, NobilityRank.Baron, NobilityRank.Marquis, [typeof(Cake)], [typeof(BlankScroll)], [typeof(Emerald), typeof(Tourmaline), typeof(HairDye), typeof(OrnateElvenChair)], [typeof(ContractOfEmployment)], [], 1),

                NpcJobClass.CaravanMaster => new DeepJobProfile(SkillName.Camping, NobilityRank.Knight, NobilityRank.Count, [typeof(FrenchBread), typeof(Ham)], [typeof(PackHorse), typeof(PackLlama), typeof(IronIngot), typeof(Log)], [typeof(GoldRing), typeof(Cloak)], [typeof(CommodityDeed)], [], 8),
                NpcJobClass.ClothWholesaler => new DeepJobProfile(SkillName.Camping, NobilityRank.Knight, NobilityRank.Count, [typeof(Sausage)], [typeof(PackHorse), typeof(BoltOfCloth), typeof(SpoolOfThread)], [typeof(TricorneHat)], [typeof(CommodityDeed)], [], 10),
                NpcJobClass.ArmamentMajor => new DeepJobProfile(SkillName.Camping, NobilityRank.Knight, NobilityRank.Count, [typeof(Bacon)], [typeof(PackHorse), typeof(Broadsword), typeof(ChainChest)], [typeof(Boots)], [typeof(CommodityDeed)], [], 5),

                NpcJobClass.Priest => new DeepJobProfile(SkillName.Healing, NobilityRank.Commoner, NobilityRank.Viscount, [typeof(BreadLoaf)], [typeof(Bandage), typeof(Candle), typeof(Garlic)], [typeof(PlainDress)], [typeof(GraveDust)], [], 6),
                NpcJobClass.Healer_Master => new DeepJobProfile(SkillName.Healing, NobilityRank.Knight, NobilityRank.Viscount, [typeof(Apple)], [typeof(Bandage), typeof(Ginseng), typeof(GreaterHealPotion)], [typeof(SilverRing)], [typeof(Bone)], [], 8),
                NpcJobClass.Gravedigger_Relig => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Knight, [typeof(BreadLoaf)], [typeof(Shovel), typeof(Torch)], [typeof(Shoes)], [typeof(GraveDust), typeof(Bone)], [], 10),

                NpcJobClass.Bard => new DeepJobProfile(SkillName.Musicianship, NobilityRank.Commoner, NobilityRank.Baronet, [typeof(CheesePizza)], [typeof(Lute), typeof(LapHarp)], [typeof(FancyShirt), typeof(FeatheredHat)], [], [], 0),
                NpcJobClass.Drummer => new DeepJobProfile(SkillName.Musicianship, NobilityRank.Commoner, NobilityRank.Baronet, [typeof(Cookies)], [typeof(Drums), typeof(Tambourine)], [typeof(JesterSuit), typeof(JesterHat)], [], [], 0),
                NpcJobClass.InnKeeper => new DeepJobProfile(SkillName.Cooking, NobilityRank.Commoner, NobilityRank.Baronet, [typeof(ApplePie)], [typeof(RawRibs), typeof(SackFlour), typeof(Pitcher)], [typeof(GoldRing)], [new EconomyItemKey(typeof(BeverageBottle), CraftResource.None, (int)BeverageType.Ale), new EconomyItemKey(typeof(BeverageBottle), CraftResource.None, (int)BeverageType.Wine), typeof(FrenchBread), typeof(Cake)], [typeof(StoneOvenEastDeed)], 30),

                NpcJobClass.Navigator => new DeepJobProfile(SkillName.Cartography, NobilityRank.Knight, NobilityRank.Baron, [typeof(Trout), typeof(Bacon)], [typeof(Sextant), typeof(BlankMap)], [typeof(Spyglass), typeof(TricorneHat)], [typeof(Trout)], [], 10),
                NpcJobClass.Shipwright_Master => new DeepJobProfile(SkillName.Carpentry, NobilityRank.Knight, NobilityRank.Baron, [typeof(FrenchBread)], [typeof(Log), typeof(Board), typeof(Nails)], [typeof(GoldRing)], [typeof(RowBoatDeed)], [typeof(WoodworkersBenchDeed)], 1),
                NpcJobClass.DeepSeaFisher => new DeepJobProfile(SkillName.Fishing, NobilityRank.Commoner, NobilityRank.Knight, [typeof(Trout)], [typeof(SpecialFishingNet)], [typeof(ThighBoots)], [typeof(Trout), typeof(BlackPearl)], [], 30),

                NpcJobClass.Librarian => new DeepJobProfile(SkillName.Inscribe, NobilityRank.Baronet, NobilityRank.Count, [typeof(Muffins), typeof(Pear)], [typeof(BlankScroll), typeof(ScribesPen)], [typeof(SilverNecklace), typeof(ElvenReadingChair)], [typeof(RedBook), typeof(BlueBook), typeof(TanBook)], [], 5),
                NpcJobClass.Cartographer_Scholar => new DeepJobProfile(SkillName.Cartography, NobilityRank.Baronet, NobilityRank.Count, [typeof(CheesePizza)], [typeof(BlankScroll), typeof(MapmakersPen)], [typeof(SilverRing)], [typeof(BlankMap)], [], 8),

                NpcJobClass.Thief => new DeepJobProfile(SkillName.Stealing, NobilityRank.Commoner, NobilityRank.Knight, [typeof(RawLambLeg), new EconomyItemKey(typeof(BeverageBottle), CraftResource.None, (int)BeverageType.Ale)], [typeof(Dagger)], [typeof(Bandana), typeof(SkullCap)], [typeof(Gold), typeof(GoldRing), typeof(SilverRing)], [], 10),
                NpcJobClass.Assassin => new DeepJobProfile(SkillName.Poisoning, NobilityRank.Commoner, NobilityRank.Knight, [typeof(Sausage)], [typeof(Dagger)], [typeof(Cloak)], [typeof(Bloodmoss), typeof(Nightshade), typeof(Gold)], [], 10),

                _ => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Marquis, [typeof(BreadLoaf)], [], [], [], [], 0)
            };

            profile = InjectFameTableware(profile);
            return profile;
        }

        private static DeepJobProfile InjectFameTableware(DeepJobProfile profile)
        {
            if (profile == null) return null;

            List<EconomyItemKey> injectedLuxuries = profile.Luxuries?.ToList() ?? new List<EconomyItemKey>();

            if (!injectedLuxuries.Any(k => k.ItemType == typeof(Candle))) injectedLuxuries.Add(typeof(Candle));
            if (!injectedLuxuries.Any(k => k.ItemType == typeof(Lantern))) injectedLuxuries.Add(typeof(Lantern));

            if (profile.MinRank >= NobilityRank.Baron)
            {
                injectedLuxuries.Add(typeof(Goblet));
                injectedLuxuries.Add(typeof(Plate));
                injectedLuxuries.Add(typeof(ForkLeft));
                injectedLuxuries.Add(typeof(KnifeRight));
            }
            else if (profile.MinRank >= NobilityRank.Knight)
            {
                injectedLuxuries.Add(typeof(PewterMug));
                injectedLuxuries.Add(typeof(Plate));
                injectedLuxuries.Add(typeof(SpoonRight));
            }
            else
            {
                injectedLuxuries.Add(typeof(WoodenBowl));
                injectedLuxuries.Add(typeof(SpoonLeft)); 
            }

            return profile with { Luxuries = injectedLuxuries.ToArray() };
        }
    }
}