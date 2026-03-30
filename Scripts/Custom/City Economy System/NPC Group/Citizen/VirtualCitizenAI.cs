using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Items;

namespace Server.Misc
{
    public static class VirtualCitizenAI
    {
        private static int _LastProcessedHour = -1; 
        private static bool m_ProcessGroupA = true;

        public static void Initialize()
        {
            // [1번 기획] 30초마다 체크 (부하 50% 분산 엔진)
            Timer.DelayCall(TimeSpan.FromSeconds(30.0), TimeSpan.FromSeconds(30.0), () => 
            {
                double totalMinutes = DateTime.Now.TimeOfDay.TotalMinutes;
                int gameHour = ((int)(totalMinutes * 6) % 24); 

                foreach (var town in TownEconomyManager.Towns.Values)
                {
                    if (town.Citizens == null || town.Citizens.Count == 0) continue;

                    var targetGroup = town.Citizens.Where(c => {
                        int groupSeed = c.House?.GetHashCode() ?? c.GetHashCode();
                        return (Math.Abs(groupSeed) % 2 == 0) == m_ProcessGroupA;
                    }).ToList();

                    foreach (var agent in targetGroup)
                    {
                        agent.OnTick(town);
                    }

                    if (m_ProcessGroupA)
                    {
                        if (gameHour == 18) 
                            TownSocietyEngine.ProcessEveningSocialTick(town);
                        else if (gameHour == 0) 
                            TownSocietyEngine.ProcessDeepNightLifeCycleTick(town);
                    }
                }
                m_ProcessGroupA = !m_ProcessGroupA;
            });
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
                case 6: ProcessNeeds(agent, town, profile); break;
                case 12: 
                    HandleWork(agent, town, groupID, profile);
                    ProcessNeeds(agent, town, profile);
                    if (agent.Age >= 7.0 && agent.Age <= 16.0) VirtualEducation.ProcessSchool(agent, town); 
                    break;
                case 18: 
                    HandleWork(agent, town, groupID, profile);
                    ProcessLuxury(agent, town, profile);
                    if (agent.Age >= 7.0 && agent.Age <= 16.0) VirtualEducation.ProcessSchool(agent, town); 
                    break;
                case 24: ProcessNightRest(agent, town, groupID); break;
            }
        }

        private static void HandleWork(VirtualCitizen agent, TownEconomy town, int groupID, 
            (SkillName Skill, NobilityRank MinRank, NobilityRank MaxRank, Type[] Necessities, Type[] JobMaterials, Type[] Luxuries, Type[] Produces, int BaseQty) profile)
        {
            if (groupID == 100) 
                VirtualTradeAI.ExecuteHarvestAndSell(agent, town, profile.BaseQty);
            else 
                ProcessProductionTick(agent, town, profile);
        }

        private static void ProcessNeeds(VirtualCitizen agent, TownEconomy town, (SkillName Skill, NobilityRank MinRank, NobilityRank MaxRank, Type[] Necessities, Type[] JobMaterials, Type[] Luxuries, Type[] Produces, int BaseQty) profile)
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
                // [수정] 시민 기본 식단에 등급별 생선(TroutFishSteak, TroutRawFishSteak)을 병합합니다.
                Type[] extendedFoods = [.. profile.Necessities, typeof(TroutFishSteak), typeof(TroutRawFishSteak), typeof(FishSteak)];

                if (TryPurchaseFromList(agent, town, extendedFoods).Success)
                {
                    agent.Hunger = Math.Min(100000, agent.Hunger + 35000);
                    agent.Satisfaction = Math.Min(100, agent.Satisfaction + 3);
                }
                else agent.Stress = Math.Min(100, agent.Stress + 15);
            }
        }

        private static void ProcessLuxury(VirtualCitizen agent, TownEconomy town, (SkillName Skill, NobilityRank MinRank, NobilityRank MaxRank, Type[] Necessities, Type[] JobMaterials, Type[] Luxuries, Type[] Produces, int BaseQty) profile)
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

        private static void ProcessProductionTick(VirtualCitizen agent, TownEconomy town, (SkillName Skill, NobilityRank MinRank, NobilityRank MaxRank, Type[] Necessities, Type[] JobMaterials, Type[] Luxuries, Type[] Produces, int BaseQty) profile)
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

			// 1. [집중(Focus) 반영] 성공 확률 + 집중력 보정 (최대 +20%)
			double successChance = 0.2 + (0.8 * (agent.PrimarySkill / 200.0)) + (0.2 * focus);
			
			if (agent.House != null && agent.House.HasWorkshop)
			{
				successChance = Math.Min(1.0, successChance * 1.2);
			}
				
			if (Utility.RandomDouble() < successChance && profile.Produces != null && profile.Produces.Length > 0)
			{
				Type targetProduce = profile.Produces[Utility.Random(profile.Produces.Length)];
				
				double rankMult = 1.0 + ((int)agent.RankLevel * 0.1); 
				double ageFactor = agent.IsElder ? 0.5 : 1.0;
				int workshopBonus = (agent.House != null && agent.House.HasWorkshop) ? 1 : 0;

				// 2. [적응(Adaptability) 반영] 생산 수량 증폭 (최대 +30% 증가)
				double adaptMult = 1.0 + (0.3 * adaptability);
				int finalQty = (int)Math.Max(1, Math.Ceiling(profile.BaseQty * 0.2 * agent.Potential * rankMult * ageFactor * adaptMult)) + workshopBonus;
				
				int basePrice = Math.Max(1, town.GetPrice(targetProduce));

				if (VirtualTradeAI.ExecuteSell(agent, town, targetProduce, basePrice, finalQty).Success)
				{
					agent.CheckSkillGain(); 
					// 3. [대사(Metabolism) 반영] 노동 후 스트레스 증가량을 대사량이 높을수록 완화
					int stressGain = Math.Max(1, 5 - (int)(2 * metabolism));
					agent.Stress = Math.Min(100, agent.Stress + stressGain);
					agent.Fame += 1;
				}
			}
			else 
			{
				// 4. [집중(Focus) 반영] 집중력이 높으면 실패 시 스트레스 페널티 방어
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

            // [수정] 유저 선호도 반영: 곡괭이(Pickaxe)보다 삽(Shovel)을 우선 찾도록 리스트 재구성
            var searchList = itemList.ToList();
            if (searchList.Contains(typeof(Pickaxe)) && !searchList.Contains(typeof(Shovel)))
            {
                searchList.Add(typeof(Shovel));
            }

            var prioritizedList = new List<Type>();
            // 삽이 목록에 있다면 최우선 탐색 순위(0번 인덱스)로 강제 배치
            if (searchList.Contains(typeof(Shovel)))
            {
                prioritizedList.Add(typeof(Shovel));
                searchList.Remove(typeof(Shovel));
            }
            
            // 나머지는 기존처럼 랜덤 셔플
            prioritizedList.AddRange(searchList.OrderBy(x => Utility.RandomDouble()));

            foreach (var itemType in prioritizedList)
            {
                int basePrice = Math.Max(1, town.GetPrice(itemType)); 
                // 해당 메서드 내에서 마을 창고 -> 유저 벤더(SearchPlayerVendors) 순으로 자동으로 탐색됩니다.
                var result = VirtualTradeAI.ExecutePurchase(agent, town, itemType, basePrice);
                if (result.Success) return result; 
            }
            return (false, 0); 
        }
    }
}