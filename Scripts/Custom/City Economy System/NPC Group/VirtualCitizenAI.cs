using System;
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
                            TownSocietyEngine.ProcessMidnightLifeCycleTick(town);
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

            // [수정됨] 이중 감가상각 방지: Hunger와 Thirst를 여기서 강제로 깎지 않습니다. (VirtualCitizen.OnTick에서 이미 처리함)

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
                    break;
                case 18: 
                    HandleWork(agent, town, groupID, profile);
                    ProcessLuxury(agent, town, profile);
                    break;
                case 24: ProcessNightRest(agent, town, groupID); break;
            }
        }

        private static void HandleWork(VirtualCitizen agent, TownEconomy town, int groupID, 
            (SkillName Skill, NobilityRank MinRank, NobilityRank MaxRank, Type[] Necessities, Type[] JobMaterials, Type[] Luxuries, Type[] Produces, int BaseQty) profile)
        {
            // [2번 기획] 직업 이원화: 100번대(채집가)는 야생 추출, 그 외는 벤더 연동 가공
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
                    // [안전장치] 시장에 물이 없을 경우 마을 우물에서 직접 마심
                    agent.Thirst = Math.Min(100000, agent.Thirst + 15000);
                    agent.Stress = Math.Min(100, agent.Stress + 5); 
                }
            }

            if (agent.Hunger < 20000 || agent.IsStarving)
            {
                if (TryPurchaseFromList(agent, town, profile.Necessities).Success)
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
			// 1. 생산 가능 상태 체크
			if (!agent.IsProductive || agent.Stress >= 90) return;

			// 2. [유지] 작업 재료 소모 로직 (재료가 없으면 생산 불가)
			if (profile.JobMaterials != null && profile.JobMaterials.Length > 0)
			{
				if (!TryPurchaseFromList(agent, town, profile.JobMaterials).Success)
				{
					agent.Stress = Math.Min(100, agent.Stress + 10);
					return;
				}
			}

			// 3. [교정] 생산 성공 확률 계산 (스킬 0=20%, 200=100%)
			double successChance = 0.2 + (0.8 * (agent.PrimarySkill / 200.0));
				
			if (Utility.RandomDouble() < successChance && profile.Produces != null && profile.Produces.Length > 0)
			{
				Type targetProduce = profile.Produces[Utility.Random(profile.Produces.Length)];
				
				// 4. [보정] 생산량 계산: (BaseQty * 0.2)를 기본으로 잠재력/신분/연령 가중치 적용
				double rankMult = 1.0 + ((int)agent.RankLevel * 0.1); 
				double ageFactor = agent.IsElder ? 0.5 : 1.0;
				
				// 최종 생산량 = 유저 효율의 20% * 시민 잠재력 * 신분 보너스 * 노화 페널티
				int finalQty = (int)Math.Max(1, Math.Ceiling(profile.BaseQty * 0.2 * agent.Potential * rankMult * ageFactor));
				
				int basePrice = Math.Max(1, town.GetPrice(targetProduce));

				// 5. 판매 및 사후 처리
				if (VirtualTradeAI.ExecuteSell(agent, town, targetProduce, basePrice, finalQty).Success)
				{
					agent.CheckSkillGain(); // 기존의 단순 합산 대신 세제곱 성장 곡선 로직 호출
					agent.Stress = Math.Min(100, agent.Stress + 5);
					agent.Fame += 1;
				}
			}
			else 
			{
				agent.Stress = Math.Min(100, agent.Stress + 8);
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

            var shuffled = itemList.OrderBy(x => Utility.RandomDouble()).ToArray();
            foreach (var itemType in shuffled)
            {
                int basePrice = Math.Max(1, town.GetPrice(itemType)); 
                var result = VirtualTradeAI.ExecutePurchase(agent, town, itemType, basePrice);
                if (result.Success) return result; 
            }
            return (false, 0); 
        }
    }
}