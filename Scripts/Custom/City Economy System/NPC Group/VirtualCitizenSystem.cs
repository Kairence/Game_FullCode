using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Multis;

namespace Server.Misc
{
    // ==============================================================================
    // 🌟 [신규] 기존의 복잡한 Tuple을 대체하는 C# 12 Record 객체
    // ==============================================================================
    public record DeepJobProfile(
        SkillName Skill, 
        NobilityRank MinRank, 
        NobilityRank MaxRank, 
        Type[] Necessities, 
        Type[] JobMaterials, 
        Type[] Luxuries, 
        Type[] Produces, 
        Type[] Addons,      // 🌟 [추가] 집에 설치할 공방 에드온 리스트
        int BaseQty
    );

    // ==============================================================================
    // 1. [VirtualCitizen] 개별 시민 객체 (기존 동일)
    // ==============================================================================
    public enum Gender { Male, Female }

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
            
            Hunger = 100;
            Thirst = 100;
            Skills = [];
            foreach (SkillName sk in VirtualJobCore.AllSkills) Skills[sk] = 0.0;
        }
        public bool HasCheckedAdventurer { get; set; } = false;
        public void OnHourTick()
        {
            if (IsExpired) return;

            // 🌟 [기획 2번] 17세 성인식: 5% 확률로 전투 자질 개화 -> 모험가 전직
            if (this.Age >= 17.0 && this.Age < 17.1 && !this.HasCheckedAdventurer)
            {
                this.HasCheckedAdventurer = true; // this.Bio 삭제
                
                if (Utility.RandomDouble() < 0.05)
                {
                    Console.WriteLine($"[생애주기] {this.Name}(17세)가 마을을 떠나 모험가의 길을 걷습니다!");
                    
                    // 전투 직업 무작위 부여 후 대기열 합류
                    NpcJobClass[] combatJobs = { NpcJobClass.Knight, NpcJobClass.Archer_Expert, NpcJobClass.Wizard, NpcJobClass.Halberdier, NpcJobClass.Healer_Master };
                    var adv = new VirtualAdventurer(combatJobs[Utility.Random(combatJobs.Length)], this.RankLevel) { Gold = this.Gold };
                    VirtualAdventurerManager.IdleAdventurers.Add(adv);
                    
                    this.IsKilled = true; // 시민 명부에서 삭제
                    return; // 전직했으므로 아래 생존 스탯 감소는 패스
                }
            }

            // 기존 생존 소모 로직
            double decayFactor = 1.5 / Potential; 
            int totalDecay = (int)Math.Max(1, Math.Ceiling(decayFactor)); 

            this.Hunger = Math.Max(0, this.Hunger - totalDecay);
            this.Thirst = Math.Max(0, this.Thirst - totalDecay);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(7); 
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
        }

        public VirtualCitizen(GenericReader reader) : base(reader)
        {
            int version = reader.ReadInt();
            if (version >= 7) IsKilled = reader.ReadBool();
            if (version >= 6) { TargetRegionName = reader.ReadString(); if (reader.ReadBool()) Bio = new BioStats(reader); else Bio = new BioStats(); } else Bio = new BioStats();
            Generation = reader.ReadInt(); Name = reader.ReadString(); Fame = reader.ReadInt(); Karma = reader.ReadInt(); Gender = (Gender)reader.ReadInt(); Potential = reader.ReadDouble(); BirthTime = reader.ReadDateTime(); MaxLifespan = reader.ReadTimeSpan(); Satisfaction = reader.ReadInt(); RankLevel = (NobilityRank)reader.ReadInt(); Thirst = reader.ReadInt(); if (version >= 4) LastProcessedHour = reader.ReadInt();
            Skills = []; int skillCount = reader.ReadInt(); for (int i = 0; i < skillCount; i++) Skills[(SkillName)reader.ReadInt()] = reader.ReadDouble();
        }
    }

    // ==============================================================================
    // 2. [VirtualCitizenAI] 두뇌 및 파이프라인 수신 루틴 (마스터 틱 기반)
    // ==============================================================================
    public static class VirtualCitizenAI
    {
        public static void Initialize()
        {
            // 🌟 개별 타이머 완전 삭제
        }

        // [Step 1] 틱 0 처리 (일괄 동기화)
        public static void ExecuteFinalBatchProcess(int gameHour)
        {
            var towns = TownEconomyManager.Towns.Values.ToList();
            foreach (var town in towns)
            {
                if (town.Citizens != null)
                {
                    foreach (var c in town.Citizens) c.OnHourTick();
                }

                if (gameHour == 18) 
                    TownSocietyEngine.ProcessEveningSocialTick(town);
                else if (gameHour == 0) 
                    TownSocietyEngine.ProcessDeepNightLifeCycleTick(town);
            }
            Console.WriteLine($"[MasterTick] 30분 사이클 시민 경제/생존 정산 완료. (게임시간: {gameHour}시)");
        }

        // [Step 2] 틱 1 ~ 40 처리 (시민 분할)
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
						// 아침에 위시리스트 갱신하고
						VirtualTradeSystem.UpdateHouseWishlist(agent.House);
						// 어제 못 산 물건들을 게시판 퀘스트로 변환!
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

                    // 🌟 퇴근길 짐말을 이끌고 유저 상점과 마을 벤더에서 싹쓸이 쇼핑을 합니다!
                    VirtualTradeSystem.ProcessHoardingShopping(agent, town);

                    ProcessLuxury(agent, town, profile);
                    if (agent.Age >= 7.0 && agent.Age <= 16.0) VirtualEducation.ProcessSchool(agent, town); 
                    break;
                case 24: ProcessNightRest(agent, town, groupID); break;
            }
        }

        private static void TryAcceptTownJob(VirtualCitizen agent, TownEconomy town, int groupID)
        {
            if (agent.Stress >= 80 || !agent.IsProductive) return;

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
                
                if ((DateTime.Now - req.CreationTime).TotalMinutes < 25.0) continue;

                req.CurrentParticipants++;
                req.IsAIAssigned = true;
                
                agent.Satisfaction = Math.Min(100, agent.Satisfaction + 5);
                agent.Stress = Math.Max(0, agent.Stress - 2);
                
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

        // 🌟 [변경] 튜플 대신 DeepJobProfile 적용
        private static void HandleWork(VirtualCitizen agent, TownEconomy town, int groupID, DeepJobProfile profile)
        {
            if (groupID == 100) // 채집, 광부, 벌목꾼
            {
                ResourceType targetType = ResourceType.Farming;
                if (agent.JobClass == NpcJobClass.SurfaceMiner || agent.JobClass == NpcJobClass.StoneQuarryman) targetType = ResourceType.Mining;
                else if (agent.JobClass == NpcJobClass.Woodcutter) targetType = ResourceType.Lumberjacking;
                else if (agent.JobClass == NpcJobClass.DeepSeaFisher) targetType = ResourceType.Fishing;

                var nearestPool = ResourceManager.PoolList
                    .Where(p => p.Facet == town.Facet && p.Type == targetType && p.CanGather())
                    .OrderBy(p => Utility.GetDistanceToSqrt(new Point3D(p.CenterX, p.CenterY, 0), town.Center))
                    .FirstOrDefault();

                if (nearestPool != null)
                {
                    Type targetProduce = profile.Produces[Utility.Random(profile.Produces.Length)];
                    int harvested = nearestPool.ConsumeResource(targetProduce, profile.BaseQty);

                    if (harvested > 0)
                    {
                        int basePrice = Math.Max(1, town.GetPrice(targetProduce));
                        VirtualTradeSystem.ExecuteSell(agent, town, targetProduce, basePrice, harvested);
                        agent.Satisfaction = Math.Min(100, agent.Satisfaction + 2);
                    }
                    else
                    {
                        agent.Stress = Math.Min(100, agent.Stress + 10);
                    }
                }
                else
                {
                    agent.Stress = Math.Min(100, agent.Stress + 15);
                }
            }
            else 
            {
                ProcessProductionTick(agent, town, profile); // 일반 제작직
            }
        }

        // 🌟 [변경] 튜플 대신 DeepJobProfile 적용
        private static void ProcessNeeds(VirtualCitizen agent, TownEconomy town, DeepJobProfile profile)
        {
            if (agent.Thirst < 20000 || agent.IsDehydrated)
            {
                Type[] drinks = [typeof(Pitcher), typeof(BeverageBottle)]; 
                if (TryPurchaseFromList(agent, town, drinks).Success)
                {
                    agent.Thirst = Math.Min(100000, agent.Thirst + 40000);
                    agent.Satisfaction = Math.Min(100, agent.Satisfaction + 2);
                }
                else 
                {
                    agent.Thirst = Math.Min(100000, agent.Thirst + 15000);
                    agent.Stress = Math.Min(100, agent.Stress + 5); 
                }
            }

            if (agent.Hunger < 20000 || agent.IsStarving)
            {
                Type[] extendedFoods = [.. profile.Necessities ?? [], typeof(TroutFishSteak), typeof(TroutRawFishSteak), typeof(FishSteak)];
                if (TryPurchaseFromList(agent, town, extendedFoods).Success)
                {
                    agent.Hunger = Math.Min(100000, agent.Hunger + 35000);
                    agent.Satisfaction = Math.Min(100, agent.Satisfaction + 3);
                }
                else agent.Stress = Math.Min(100, agent.Stress + 15);
            }
        }

        // 🌟 [변경] 튜플 대신 DeepJobProfile 적용
        private static void ProcessLuxury(VirtualCitizen agent, TownEconomy town, DeepJobProfile profile)
        {
            if (agent.Stress > 40 && profile.Luxuries != null && profile.Luxuries.Length > 0)
            {
                var (success, spent) = TryPurchaseFromList(agent, town, profile.Luxuries);
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

        // 🌟 [변경] 튜플 대신 DeepJobProfile 적용
        private static void ProcessProductionTick(VirtualCitizen agent, TownEconomy town, DeepJobProfile profile)
        {
            if (!agent.IsProductive || agent.Stress >= 90) return;

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

            double successChance = 0.2 + (0.8 * (agent.PrimarySkill / 200.0)) + (0.2 * focus);
            if (agent.House != null && agent.House.HasWorkshop) successChance = Math.Min(1.0, successChance * 1.2);
                
            if (Utility.RandomDouble() < successChance && profile.Produces != null && profile.Produces.Length > 0)
            {
                Type targetProduce = profile.Produces[Utility.Random(profile.Produces.Length)];
                
                double rankMult = 1.0 + ((int)agent.RankLevel * 0.1); 
                double ageFactor = agent.IsElder ? 0.5 : 1.0;
                int workshopBonus = (agent.House != null && agent.House.HasWorkshop) ? 1 : 0;

                double adaptMult = 1.0 + (0.3 * adaptability);
                int finalQty = (int)Math.Max(1, Math.Ceiling(profile.BaseQty * 0.2 * agent.Potential * rankMult * ageFactor * adaptMult)) + workshopBonus;
                
                int basePrice = Math.Max(1, town.GetPrice(targetProduce));

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

        private static (bool Success, int Spent) TryPurchaseFromList(VirtualCitizen agent, TownEconomy town, Type[] itemList)
        {
            if (itemList == null || itemList.Length == 0) return (true, 0);

            var searchList = itemList.ToList();
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

            foreach (var itemType in searchList)
            {
                int basePrice = Math.Max(1, town.GetPrice(itemType)); 
                var result = VirtualTradeSystem.ExecutePurchase(agent, town, itemType, basePrice);
                if (result.Success) return result; 
            }
            return (false, 0); 
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
    // 4. [VirtualJobCore] 직업 프로필 매핑 (🌟 DeepJobProfile Record 적용)
    // ==============================================================================
    public static class VirtualJobCore
    {
        public static readonly NpcJobClass[] AllJobs = Enum.GetValues<NpcJobClass>();
        public static readonly SkillName[] AllSkills = Enum.GetValues<SkillName>();

        public static DeepJobProfile GetDeepJobProfile(NpcJobClass job)
        {
            var profile = job switch
            {
                NpcJobClass.Pauper => new DeepJobProfile(SkillName.Begging, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], null, null, null, null, 0),
                NpcJobClass.Beggar => new DeepJobProfile(SkillName.Begging, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins), typeof(Carrot)], null, null, null, null, 0),
                NpcJobClass.Laborer => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(BreadLoaf)], [typeof(Shoes)], [typeof(Candle)], null, null, 0),
                NpcJobClass.StreetSweeper => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Boots)], [typeof(Muffins)], [typeof(Bone)], null, 2),
                NpcJobClass.WaterCarrier => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Turnip)], [typeof(Pouch)], [typeof(Sandals)], null, null, 20),
                NpcJobClass.NightSoilMan => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Cabbage)], [typeof(Shovel), typeof(Torch)], [typeof(Candle)], [typeof(FertileDirt)], null, 5),
                NpcJobClass.GongFarmer => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Onion)], [typeof(Shovel), typeof(Boots)], [typeof(Torch)], [typeof(FertileDirt)], null, 5),
                NpcJobClass.RatCatcher => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Dagger)], [typeof(Bandage)], [typeof(RawRibs)], null, 3),
                NpcJobClass.ChimneySweep => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Pear)], [typeof(Candle), typeof(Bandage)], [typeof(Shoes)], [typeof(GraveDust)], null, 4),
                NpcJobClass.Lamplighter => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(Torch), typeof(Lantern), typeof(OilFlask)], [typeof(Boots)], null, null, 0),
                NpcJobClass.LinkBoy => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Torch)], [typeof(Shoes)], null, null, 0),
                NpcJobClass.GraveDigger_Basic => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(BreadLoaf)], [typeof(Shovel), typeof(Torch)], [typeof(BeverageBottle)], [typeof(GraveDust), typeof(Bone)], null, 5),
                
                NpcJobClass.Scullion => new DeepJobProfile(SkillName.Cooking, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(TroutRawFishSteak), typeof(HalfApron), typeof(Skillet)], [typeof(Candle)], [typeof(TroutFishSteak)], [typeof(StoneOvenEastDeed)], 20),
                NpcJobClass.GrainFarmer => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Pitchfork)], [typeof(StrawHat)], [typeof(EarOfCorn)], null, 20),
                NpcJobClass.VegetableFarmer => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Pear)], [typeof(Pitchfork)], [typeof(Shoes)], [typeof(Carrot), typeof(Onion), typeof(Cabbage), typeof(Lettuce), typeof(Turnip)], null, 18),
                NpcJobClass.GourdFarmer => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Shovel)], [typeof(Boots)], [typeof(Pumpkin), typeof(Squash), typeof(GreenGourd), typeof(YellowGourd)], null, 15),
                NpcJobClass.Orchardist => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(Basket), typeof(Bag)], [typeof(FloppyHat)], [typeof(Apple), typeof(Pear), typeof(Peach)], null, 25),
                NpcJobClass.CitrusGrower => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(Basket)], [typeof(StrawHat)], [typeof(Lemon), typeof(Lime)], null, 15),
                NpcJobClass.VineyardWorker => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(BreadLoaf)], [typeof(Scissors)], [typeof(Bandana)], [typeof(Grapes)], null, 20),
                NpcJobClass.BerryPicker => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Pouch)], [typeof(Sandals)], [typeof(ParasiticPlant)], null, 10),
                NpcJobClass.Herbalist => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Pear)], [typeof(Dagger), typeof(Bag)], [typeof(Cap)], [typeof(Garlic), typeof(Ginseng)], null, 15),
                NpcJobClass.MushroomGatherer => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(Candle), typeof(Pouch)], [typeof(Shoes)], [typeof(MandrakeRoot), typeof(Nightshade), typeof(SpidersSilk)], null, 8),
                NpcJobClass.Beekeeper => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Torch)], [typeof(HalfApron)], [typeof(JarHoney)], null, 10),
                NpcJobClass.CoastalFisher => new DeepJobProfile(SkillName.Fishing, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(FishingPole)], [typeof(ThighBoots)], [typeof(Trout)], null, 15),
                NpcJobClass.DeepSeaFisher_Basic => new DeepJobProfile(SkillName.Fishing, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(BreadLoaf)], [typeof(SpecialFishingNet)], [typeof(TricorneHat)], [typeof(Trout)], null, 25),
                NpcJobClass.OysterDiver_Basic => new DeepJobProfile(SkillName.Fishing, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(SkinningKnife)], [typeof(Bandana)], [typeof(BlackPearl)], null, 5),
                NpcJobClass.SeaweedCollector => new DeepJobProfile(SkillName.Fishing, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Pear)], [typeof(Bag), typeof(Sandals)], [typeof(Candle)], [typeof(FertileDirt)], null, 8),
                NpcJobClass.BeachComber => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(Spyglass), typeof(Torch)], [typeof(Boots)], [typeof(Bone)], null, 3),
                NpcJobClass.SaltGatherer => new DeepJobProfile(SkillName.Mining, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Shovel)], [typeof(Shoes)], [typeof(Sand)], null, 10), 
                NpcJobClass.Shepherd => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(BreadLoaf)], [typeof(ShepherdsCrook)], [typeof(StrawHat)], [typeof(Wool)], null, 15),
                NpcJobClass.Swineherd => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Carrot)], [typeof(Cleaver)], [typeof(ShortPants)], [typeof(RawRibs), typeof(Bacon)], null, 12),
                NpcJobClass.PoultryFarmer => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(EarOfCorn)], [typeof(Basket)], [typeof(HalfApron)], [typeof(Eggs), typeof(Feather), typeof(RawBird), typeof(RawChickenLeg)], null, 18),
                NpcJobClass.CattleDrover => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(RawRibs)], [typeof(Whip)], [typeof(Boots)], [typeof(Hides), typeof(RawRibs)], null, 10),
                NpcJobClass.StableHand => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(EarOfCorn)], [typeof(Pitchfork)], [typeof(Shirt)], null, null, 0),
                NpcJobClass.GooseHerd => new DeepJobProfile(SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(QuarterStaff)], [typeof(Sandals)], [typeof(Feather), typeof(Eggs)], null, 12),
                
                NpcJobClass.Woodcutter => new DeepJobProfile(SkillName.Lumberjacking, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(BreadLoaf)], [typeof(Axe), typeof(TwoHandedAxe), typeof(Hatchet)], [typeof(Doublet)], [typeof(Log)], null, 25),
                NpcJobClass.BarkCollector => new DeepJobProfile(SkillName.Lumberjacking, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(SkinningKnife)], [typeof(Cap)], [typeof(BarkFragment)], null, 15),
                NpcJobClass.SurfaceMiner => new DeepJobProfile(SkillName.Mining, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(Pickaxe)], [typeof(Boots)], [typeof(IronOre)], null, 20),
                NpcJobClass.SandDigger => new DeepJobProfile(SkillName.Mining, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Shovel), typeof(Bag)], [typeof(Sandals)], [typeof(Sand)], null, 25),
                NpcJobClass.StoneQuarryman => new DeepJobProfile(SkillName.Mining, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(BreadLoaf)], [typeof(Pickaxe)], [typeof(LeatherGloves)], [typeof(IronOre)], null, 15),
                NpcJobClass.FlintKnapper => new DeepJobProfile(SkillName.Mining, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Hammer)], [typeof(Bandana)], [typeof(IronOre)], null, 5),
                NpcJobClass.Trapper => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Sausage)], [typeof(SkinningKnife)], [typeof(LeatherCap)], [typeof(Hides), typeof(RawRibs)], null, 8),
                NpcJobClass.BirdHunter => new DeepJobProfile(SkillName.Archery, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Apple)], [typeof(Bow), typeof(Arrow)], [typeof(TallStrawHat)], [typeof(Feather), typeof(RawBird)], null, 15),
                NpcJobClass.BigGameHunter => new DeepJobProfile(SkillName.Tactics, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Bacon)], [typeof(Spear), typeof(Bandage)], [typeof(ThighBoots)], [typeof(TigerPelt), typeof(RawRibs), typeof(Bone)], null, 5),
                NpcJobClass.FeatherPlucker => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, [typeof(Muffins)], [typeof(HalfApron)], [typeof(Candle)], [typeof(Feather)], null, 20),

                NpcJobClass.Smelter => new DeepJobProfile(SkillName.Blacksmith, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(FrenchBread)], [typeof(IronOre), typeof(Tongs)], [typeof(SilverRing)], [typeof(IronIngot)], [typeof(SmallForgeDeed)], 15),
                NpcJobClass.PigIronWorker => new DeepJobProfile(SkillName.Blacksmith, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(CheesePizza)], [typeof(IronIngot), typeof(SmithHammer)], [typeof(BeverageBottle)], [typeof(PigIron), typeof(Pickaxe), typeof(Shovel)], [typeof(LargeForgeEastDeed), typeof(AnvilEastDeed)], 15),
                NpcJobClass.NailMaker => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(BreadLoaf)], [typeof(IronIngot), typeof(Hammer)], [typeof(Shirt)], [typeof(Nails)], [typeof(TinkerBenchDeed)], 25),
                NpcJobClass.AxleMaker => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Sausage)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(Boots)], [typeof(Axle)], [typeof(TinkerBenchDeed)], 10),
                NpcJobClass.GearCutter => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Ham)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(SilverBracelet)], [typeof(Gears), typeof(AxleGears)], [typeof(TinkerBenchDeed)], 10),
                NpcJobClass.SpringMaker => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(ApplePie)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(FancyShirt)], [typeof(Springs)], [typeof(TinkerBenchDeed)], 15),
                NpcJobClass.HingeMaker => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Cookies)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(PlainDress)], [typeof(Hinge)], [typeof(TinkerBenchDeed)], 20),
                NpcJobClass.ClockPartMaker => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Cake)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(SilverNecklace)], [typeof(ClockParts)], [typeof(TinkerBenchDeed)], 5),
                NpcJobClass.SextantPartMaker => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(JarHoney)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(SilverEarrings)], [typeof(SextantParts)], [typeof(TinkerBenchDeed)], 5),
                NpcJobClass.Weaver => new DeepJobProfile(SkillName.Tailoring, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(FrenchBread)], [typeof(Wool), typeof(Flax), typeof(Cotton)], [typeof(FancyDress)], [typeof(BoltOfCloth), typeof(UncutCloth)], [typeof(LoomEastDeed)], 15),
                NpcJobClass.Spinner => new DeepJobProfile(SkillName.Tailoring, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Muffins)], [typeof(Wool), typeof(Cotton)], [typeof(PlainDress)], [typeof(SpoolOfThread), typeof(DarkYarn), typeof(LightYarn)], [typeof(SpinningwheelEastDeed)], 20),
                NpcJobClass.LeatherTanner => new DeepJobProfile(SkillName.Tailoring, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(RawRibs)], [typeof(Hides), typeof(Scissors), typeof(SewingKit)], [typeof(StuddedGloves)], [typeof(LeatherChest), typeof(LeatherLegs), typeof(LeatherCap), typeof(LeatherGloves)], null, 8),
                NpcJobClass.Dyer_Producer => new DeepJobProfile(SkillName.Tailoring, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(ApplePie)], [typeof(Dyes), typeof(DyeTub)], [typeof(GoldRing)], [typeof(Shirt), typeof(ShortPants), typeof(PlainDress)], null, 10),
                NpcJobClass.Sawyer => new DeepJobProfile(SkillName.Carpentry, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(BreadLoaf)], [typeof(Log), typeof(Saw)], [typeof(SilverRing)], [typeof(Board)], [typeof(WoodworkersBenchDeed)], 25),
                NpcJobClass.ShaftMaker => new DeepJobProfile(SkillName.Carpentry, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Muffins)], [typeof(Board), typeof(DrawKnife)], [typeof(Bandana)], [typeof(Shaft)], [typeof(WoodworkersBenchDeed)], 30),
                NpcJobClass.BarrelMaker_Base => new DeepJobProfile(SkillName.Carpentry, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Ham)], [typeof(Board), typeof(IronIngot), typeof(DovetailSaw)], [typeof(Boots)], [typeof(Barrel)], [typeof(WoodworkersBenchDeed)], 5),
                NpcJobClass.BoxMaker_Base => new DeepJobProfile(SkillName.Carpentry, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Bacon)], [typeof(Board), typeof(Nails), typeof(JointingPlane)], [typeof(Shoes)], [typeof(WoodenBox), typeof(SmallCrate), typeof(MediumCrate), typeof(LargeCrate), typeof(WoodenChest)], [typeof(WoodworkersBenchDeed)], 6),
                NpcJobClass.Miller => new DeepJobProfile(SkillName.Cooking, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Apple)], [typeof(EarOfCorn), typeof(FlourSifter)], [typeof(SilverBracelet)], [typeof(SackFlour), typeof(BowlFlour)], [typeof(FlourMillEastDeed)], 15),
                NpcJobClass.Butcher_Expert => new DeepJobProfile(SkillName.Cooking, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(RawRibs)], [typeof(RawLambLeg), typeof(RawRibs), typeof(Cleaver), typeof(ButcherKnife)], [typeof(BeverageBottle)], [typeof(Bacon), typeof(Ham), typeof(Sausage)], [typeof(BBQSmokerDeed)], 20),
                NpcJobClass.PoultryProcessor => new DeepJobProfile(SkillName.Cooking, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(RawBird)], [typeof(RawBird), typeof(ButcherKnife)], [typeof(HalfApron)], [typeof(RawChickenLeg)], null, 25),
                NpcJobClass.PizzaChef_Producer => new DeepJobProfile(SkillName.Cooking, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(SackFlour)], [typeof(SackFlour), typeof(RollingPin)], [typeof(GoldRing)], [typeof(CheesePizza), typeof(BreadLoaf), typeof(FrenchBread)], [typeof(StoneOvenEastDeed)], 10),
                NpcJobClass.GlassBlower => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.Baronet, [typeof(Cake)], [typeof(Sand), typeof(TinkersTools)], [typeof(SilverNecklace)], [typeof(Bottle), typeof(SolventFlask)], [typeof(TinkerBenchDeed)], 40),
                NpcJobClass.AshProcessor => new DeepJobProfile(SkillName.Alchemy, NobilityRank.Commoner, NobilityRank.Baronet, [typeof(Muffins)], [typeof(Log), typeof(Torch)], [typeof(Boots)], [typeof(SulfurousAsh)], [typeof(HeatingStand)], 20),
                NpcJobClass.BoneGrinder => new DeepJobProfile(SkillName.Alchemy, NobilityRank.Commoner, NobilityRank.Baronet, [typeof(BreadLoaf)], [typeof(Bone), typeof(MortarPestle)], [typeof(Shoes)], [typeof(GraveDust)], [typeof(AlchemyStationDeed)], 15),
                NpcJobClass.CandleDipper => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.Baronet, [typeof(ApplePie)], [typeof(JarHoney), typeof(SpoolOfThread)], [typeof(PlainDress)], [typeof(Candle)], [typeof(TinkerBenchDeed)], 25),
                NpcJobClass.JewelryBaseMaker => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.Baronet, [typeof(FrenchBread)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(SilverRing)], [typeof(GoldRing), typeof(GoldNecklace), typeof(SilverRing), typeof(SilverNecklace)], [typeof(TinkerBenchDeed)], 5),
                NpcJobClass.BeadMaker => new DeepJobProfile(SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.Baronet, [typeof(Cookies)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(SilverBracelet)], [typeof(Beads)], [typeof(TinkerBenchDeed)], 15),

                NpcJobClass.Knight => new DeepJobProfile(SkillName.Swords, NobilityRank.Knight, NobilityRank.Baron, [typeof(FrenchBread), typeof(Ham)], [typeof(PlateChest), typeof(PlateLegs), typeof(MetalKiteShield), typeof(Longsword), typeof(Bandage)], [typeof(GoldRing), typeof(Cloak)], [typeof(Gold), typeof(DragonBlood), typeof(DaemonBone)], null, 30),
                NpcJobClass.Halberdier => new DeepJobProfile(SkillName.Tactics, NobilityRank.Knight, NobilityRank.Baron, [typeof(Sausage), typeof(CheesePizza)], [typeof(PlateChest), typeof(Halberd), typeof(Bandage)], [typeof(SilverNecklace)], [typeof(Gold), typeof(DaemonBlood)], null, 25),
                NpcJobClass.TownGuard => new DeepJobProfile(SkillName.Swords, NobilityRank.Commoner, NobilityRank.SubBaronet, [typeof(Bacon), typeof(BreadLoaf)], [typeof(ChainChest), typeof(Broadsword), typeof(Bandage)], [typeof(BeverageBottle), typeof(Shoes)], [typeof(Gold), typeof(Hides)], null, 15),
                NpcJobClass.Duelist => new DeepJobProfile(SkillName.Fencing, NobilityRank.Knight, NobilityRank.Baronet, [typeof(CookedBird)], [typeof(StuddedChest), typeof(Kryss), typeof(Bandage)], [typeof(GoldEarrings), typeof(FancyShirt)], [typeof(Gold)], null, 20),
                NpcJobClass.Archer_Expert => new DeepJobProfile(SkillName.Archery, NobilityRank.Knight, NobilityRank.Baronet, [typeof(Ham), typeof(ApplePie)], [typeof(Bow), typeof(Arrow), typeof(LeatherChest)], [typeof(TricorneHat)], [typeof(Gold), typeof(Feather), typeof(TigerPelt)], null, 20),
                NpcJobClass.Crossbowman => new DeepJobProfile(SkillName.Archery, NobilityRank.Knight, NobilityRank.Baronet, [typeof(Sausage)], [typeof(Crossbow), typeof(HeavyCrossbow), typeof(Bolt), typeof(StuddedChest)], [typeof(Boots)], [typeof(Gold), typeof(Bone)], null, 18),
                NpcJobClass.UndeadHunter => new DeepJobProfile(SkillName.Macing, NobilityRank.Knight, NobilityRank.SubBaron, [typeof(FrenchBread)], [typeof(Mace), typeof(WarMace), typeof(ChainChest), typeof(Bandage)], [typeof(SilverRing)], [typeof(Gold), typeof(GraveDust), typeof(DaemonBone), typeof(Bone)], null, 25),
                NpcJobClass.DragonTracker => new DeepJobProfile(SkillName.Tactics, NobilityRank.Knight, NobilityRank.Baron, [typeof(Ham)], [typeof(Spear), typeof(PlateChest), typeof(GreaterHealPotion)], [typeof(GoldBracelet)], [typeof(Gold), typeof(DragonBlood), typeof(DragonTurtleScute)], null, 15),

                NpcJobClass.Wizard => new DeepJobProfile(SkillName.Magery, NobilityRank.SubBaronet, NobilityRank.Viscount, [typeof(Cake), typeof(FrenchBread)], [typeof(BlackPearl), typeof(Bloodmoss), typeof(BlankScroll), typeof(Spellbook)], [typeof(Sapphire), typeof(Robe)], [typeof(RecallScroll), typeof(FireballScroll), typeof(LightningScroll)], null, 8),
                NpcJobClass.Archmage => new DeepJobProfile(SkillName.Magery, NobilityRank.Baronet, NobilityRank.Count, [typeof(Cake), typeof(JarHoney)], [typeof(MandrakeRoot), typeof(SpidersSilk), typeof(SulfurousAsh), typeof(BlankScroll)], [typeof(StarSapphire), typeof(MagicWizardsHat)], [typeof(GateTravelScroll), typeof(EnergyBoltScroll), typeof(ExplosionScroll), typeof(MeteorSwarmScroll)], null, 5),
                NpcJobClass.Alchemist => new DeepJobProfile(SkillName.Alchemy, NobilityRank.SubBaronet, NobilityRank.Baron, [typeof(Cookies), typeof(Muffins)], [typeof(Ginseng), typeof(Garlic), typeof(Bottle), typeof(MortarPestle)], [typeof(SilverNecklace)], [typeof(LesserHealPotion), typeof(HealPotion), typeof(GreaterHealPotion), typeof(LesserCurePotion), typeof(CurePotion), typeof(GreaterCurePotion)], [typeof(AlchemyStationDeed)], 15),
                NpcJobClass.PotionMaker => new DeepJobProfile(SkillName.Alchemy, NobilityRank.SubBaronet, NobilityRank.Baron, [typeof(ApplePie)], [typeof(Nightshade), typeof(SulfurousAsh), typeof(Bottle), typeof(MortarPestle)], [typeof(SilverRing)], [typeof(LesserPoisonPotion), typeof(PoisonPotion), typeof(GreaterPoisonPotion), typeof(LesserExplosionPotion), typeof(ExplosionPotion), typeof(GreaterExplosionPotion)], [typeof(AlchemyStationDeed)], 15),
                NpcJobClass.Scribe_Mage => new DeepJobProfile(SkillName.Inscribe, NobilityRank.SubBaronet, NobilityRank.Viscount, [typeof(CheesePizza)], [typeof(BlankScroll), typeof(ScribesPen), typeof(BlackPearl)], [typeof(Amethyst)], [typeof(Spellbook), typeof(Magerybook), typeof(RecallScroll)], [typeof(WritingDeskDeed)], 5),
                NpcJobClass.Necromancer => new DeepJobProfile(SkillName.Magery, NobilityRank.SubBaronet, NobilityRank.Viscount, [typeof(RawRibs)], [typeof(GraveDust), typeof(BatWing), typeof(DaemonBlood), typeof(BlankScroll)], [typeof(Robe), typeof(SkullCap)], [typeof(NecromancerSpellbook), typeof(PoisonFieldScroll)], null, 4),

                NpcJobClass.Mayor => new DeepJobProfile(SkillName.ItemID, NobilityRank.Baron, NobilityRank.Marquis, [typeof(Cake), typeof(CookedBird)], [typeof(BlankScroll), typeof(ScribesPen)], [typeof(StarSapphire), typeof(Diamond), typeof(GoldBracelet), typeof(Throne)], [typeof(CommissionContractOfEmployment)], null, 1),
                NpcJobClass.TaxCollector_Noble => new DeepJobProfile(SkillName.ItemID, NobilityRank.Baron, NobilityRank.Marquis, [typeof(JarHoney)], [typeof(BlankScroll), typeof(ScribesPen)], [typeof(GoldNecklace), typeof(Ruby)], [typeof(VendorRentalContract)], null, 2),
                NpcJobClass.Aristocrat => new DeepJobProfile(SkillName.ItemID, NobilityRank.Baron, NobilityRank.Marquis, [typeof(Cake)], [typeof(BlankScroll)], [typeof(Emerald), typeof(Tourmaline), typeof(HairDye), typeof(OrnateElvenChair)], [typeof(ContractOfEmployment)], null, 1),

                NpcJobClass.CaravanMaster => new DeepJobProfile(SkillName.Camping, NobilityRank.Knight, NobilityRank.Count, [typeof(FrenchBread), typeof(Ham)], [typeof(PackHorse), typeof(PackLlama), typeof(IronIngot), typeof(Log)], [typeof(GoldRing), typeof(Cloak)], [typeof(CommodityDeed)], null, 8),
                NpcJobClass.ClothWholesaler => new DeepJobProfile(SkillName.Camping, NobilityRank.Knight, NobilityRank.Count, [typeof(Sausage)], [typeof(PackHorse), typeof(BoltOfCloth), typeof(SpoolOfThread)], [typeof(TricorneHat)], [typeof(CommodityDeed)], null, 10),
                NpcJobClass.ArmamentMajor => new DeepJobProfile(SkillName.Camping, NobilityRank.Knight, NobilityRank.Count, [typeof(Bacon)], [typeof(PackHorse), typeof(Broadsword), typeof(ChainChest)], [typeof(Boots)], [typeof(CommodityDeed)], null, 5),

                NpcJobClass.Priest => new DeepJobProfile(SkillName.Healing, NobilityRank.Commoner, NobilityRank.Viscount, [typeof(BreadLoaf)], [typeof(Bandage), typeof(Candle), typeof(Garlic)], [typeof(PlainDress)], [typeof(GraveDust)], null, 6),
                NpcJobClass.Healer_Master => new DeepJobProfile(SkillName.Healing, NobilityRank.Knight, NobilityRank.Viscount, [typeof(Apple)], [typeof(Bandage), typeof(Ginseng), typeof(GreaterHealPotion)], [typeof(SilverRing)], [typeof(Bone)], null, 8),
                NpcJobClass.Gravedigger_Relig => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Knight, [typeof(BreadLoaf)], [typeof(Shovel), typeof(Torch)], [typeof(Shoes)], [typeof(GraveDust), typeof(Bone)], null, 10),

                NpcJobClass.Bard => new DeepJobProfile(SkillName.Musicianship, NobilityRank.Commoner, NobilityRank.Baronet, [typeof(CheesePizza)], [typeof(Lute), typeof(LapHarp)], [typeof(FancyShirt), typeof(FeatheredHat)], null, null, 0),
                NpcJobClass.Drummer => new DeepJobProfile(SkillName.Musicianship, NobilityRank.Commoner, NobilityRank.Baronet, [typeof(Cookies)], [typeof(Drums), typeof(Tambourine)], [typeof(JesterSuit), typeof(JesterHat)], null, null, 0),
                NpcJobClass.InnKeeper => new DeepJobProfile(SkillName.Cooking, NobilityRank.Commoner, NobilityRank.Baronet, [typeof(ApplePie)], [typeof(RawRibs), typeof(SackFlour), typeof(Pitcher)], [typeof(GoldRing)], [typeof(BeverageBottle), typeof(BeverageBottle), typeof(FrenchBread), typeof(Cake)], [typeof(StoneOvenEastDeed)], 30),

                NpcJobClass.Navigator => new DeepJobProfile(SkillName.Cartography, NobilityRank.Knight, NobilityRank.Baron, [typeof(Trout), typeof(Bacon)], [typeof(Sextant), typeof(BlankMap)], [typeof(Spyglass), typeof(TricorneHat)], [typeof(Trout)], null, 10),
                NpcJobClass.Shipwright_Master => new DeepJobProfile(SkillName.Carpentry, NobilityRank.Knight, NobilityRank.Baron, [typeof(FrenchBread)], [typeof(Log), typeof(Board), typeof(Nails)], [typeof(GoldRing)], [typeof(RowBoatDeed)], [typeof(WoodworkersBenchDeed)], 1),
                NpcJobClass.DeepSeaFisher => new DeepJobProfile(SkillName.Fishing, NobilityRank.Commoner, NobilityRank.Knight, [typeof(Trout)], [typeof(SpecialFishingNet)], [typeof(ThighBoots)], [typeof(Trout), typeof(BlackPearl)], null, 30),

                NpcJobClass.Librarian => new DeepJobProfile(SkillName.Inscribe, NobilityRank.Baronet, NobilityRank.Count, [typeof(Muffins), typeof(Pear)], [typeof(BlankScroll), typeof(ScribesPen)], [typeof(SilverNecklace), typeof(ElvenReadingChair)], [typeof(RedBook), typeof(BlueBook), typeof(TanBook)], null, 5),
                NpcJobClass.Cartographer_Scholar => new DeepJobProfile(SkillName.Cartography, NobilityRank.Baronet, NobilityRank.Count, [typeof(CheesePizza)], [typeof(BlankScroll), typeof(MapmakersPen)], [typeof(SilverRing)], [typeof(BlankMap)], null, 8),

                NpcJobClass.Thief => new DeepJobProfile(SkillName.Stealing, NobilityRank.Commoner, NobilityRank.Knight, [typeof(RawLambLeg), typeof(BeverageBottle)], [typeof(Dagger)], [typeof(Bandana), typeof(SkullCap)], [typeof(Gold), typeof(GoldRing), typeof(SilverRing)], null, 10),
                NpcJobClass.Assassin => new DeepJobProfile(SkillName.Poisoning, NobilityRank.Commoner, NobilityRank.Knight, [typeof(Sausage)], [typeof(Dagger)], [typeof(Cloak)], [typeof(Bloodmoss), typeof(Nightshade), typeof(Gold)], null, 10),

                _ => new DeepJobProfile(SkillName.Camping, NobilityRank.Commoner, NobilityRank.Marquis, [typeof(BreadLoaf)], null, null, null, null, 0)
            };

            // 🌟 [명예 보정] 
            profile = InjectFameTableware(profile);

            return profile;
        }

        private static DeepJobProfile InjectFameTableware(DeepJobProfile profile)
        {
            List<Type> injectedLuxuries = profile.Luxuries?.ToList() ?? new List<Type>();

            if (!injectedLuxuries.Contains(typeof(Candle))) injectedLuxuries.Add(typeof(Candle));
            if (!injectedLuxuries.Contains(typeof(Lantern))) injectedLuxuries.Add(typeof(Lantern));

            if (profile.MinRank >= NobilityRank.Baron)
            {
                injectedLuxuries.AddRange([typeof(Goblet), typeof(Plate), typeof(ForkLeft), typeof(KnifeRight)]);
            }
            else if (profile.MinRank >= NobilityRank.Knight)
            {
                injectedLuxuries.AddRange([typeof(PewterMug), typeof(Plate), typeof(SpoonRight)]);
            }
            else
            {
                injectedLuxuries.AddRange([typeof(WoodenBowl), typeof(SpoonLeft)]); 
            }

            return profile with { Luxuries = injectedLuxuries.ToArray() };
        }
    }
}