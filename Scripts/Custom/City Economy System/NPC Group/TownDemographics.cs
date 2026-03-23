using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Multis;

namespace Server.Misc
{
    // 마을의 특성을 정의하는 Enum (기존 TownDemographicAI에서 이동)
    public enum TownType { Agricultural, Industrial, Academic, Metropolis }

    public class TownDemographics
    {
        public const long PlatinumUnit = 100000000;

        // ====================================================================
        // 🏙️ 1. 메인 파이프라인: 마을 초기화 및 인구 리스폰
        // ====================================================================
		public static void InitializeTown(TownEconomy town, int targetPop)
		{
			if (town == null) return;

			// 1. 기존 가상 시민 데이터 초기화
			town.Citizens.Clear();

			// 2. 새로운 인구 할당 (기본적으로 노동자 계층으로 시작)
			for (int i = 0; i < targetPop; i++)
			{
				// 최적화된 산업 비중에 따라 생성하는 것이 좋으나, 우선 테스트를 위해 기초 직업으로 생성합니다.
				AddCitizen(town, NpcJobClass.Pauper, NpcRank.Novice);
			}
            Console.WriteLine($"[{town.TownName}] 인구 배치 완료: 총 {town.Citizens.Count}명");
		}
		/*
        public static void InitializeTown(TownEconomy town)
        {
            if (town == null || town.Warehouse == null) return;

            // [Step 1] 마을 자산(Wealth) 동기화
            long totalAssetsValue = 0;
            foreach (var kvp in town.Warehouse)
            {
                totalAssetsValue += (long)kvp.Value.Stock * town.GetPrice(kvp.Key, 1.0);
            }
            town.Wealth = totalAssetsValue; 

            Console.WriteLine($"[{town.TownName}] 자산 동기화: {totalAssetsValue / PlatinumUnit}P");

            // [Step 2] 인구 상한선(Cap) 계산 (기존 Controller 로직)
            int targetPopulation = CalculatePopulationCap(town);

            // [Step 3] AI 직업 가중치 파이프라인 실행
            var jobWeights = OptimizeDemographics(town);

            // [Step 4] 시민 배치
            if (town.Citizens == null) town.Citizens = new List<VirtualCitizen>();
            town.Citizens.Clear();

            for (int i = 0; i < targetPopulation; i++)
            {
                NpcJobClass jobHeader = GetRandomJobFromWeights(jobWeights);
                NpcRank skillRank = GetRandomRank();
                
                AddCitizen(town, jobHeader, skillRank);
            }

            Console.WriteLine($"[{town.TownName}] 인구 배치 완료: 총 {town.Citizens.Count}명");
        }
		*/
        public static void RespawnFacet(Map facet, Mobile from)
		{
			if (facet == null || facet == Map.Internal) return;

			// 1. 현재 선택한 대륙(예: 트라멜)에 속한 마을만 필터링
			var targetTowns = TownEconomyManager.Towns.Values
				.Where(t => t.Facet == facet)
				.ToList();

			if (targetTowns.Count == 0)
			{
				from.SendMessage(33, $"{facet.Name} 대륙에 등록된 마을이 없습니다.");
				return;
			}

			from.SendMessage(68, $"{facet.Name} 대륙 NPC 생성을 시작합니다...");

			foreach (TownEconomy town in targetTowns)
			{
				// 2. 해당 마을의 상인 수를 '실시간'으로 다시 체크 (대륙 격리 확인)
				// Banker를 제외한 해당 대륙/해당 마을 구역 내의 BaseVendor 카운트
				town.VendorCount = World.Mobiles.Values.OfType<BaseVendor>()
					.Count(v => v.Map == facet && TownNumber.GetID(v.Location, v.Map) == town.TownID && !(v is Banker));

				// 3. 상인 수에 비례한 인구 상한선 계산
				int targetPop = CalculatePopulationCap(town); 
				
				// 4. 해당 마을 시민 생성 (기존 인구 삭제 후 재배치)
				InitializeTown(town, targetPop);
			}

			from.SendMessage(68, $"{facet.Name} 대륙 리스폰 완료. (활성 마을: {targetTowns.Count}개)");
		}

        // ====================================================================
        // 📊 2. AI 가중치 파이프라인 (기존 AI + Controller 병합)
        // ====================================================================
        private static Dictionary<NpcJobClass, double> OptimizeDemographics(TownEconomy town)
        {
            // 1. 기본 비율 로드
            var ratios = GetBaseRatios(town.Type);

            // 2. 젠트리피케이션 (부유할수록 귀족 증가)
            ApplyGentrification(town, ratios);

            // 3. 자원 결핍 대응 (식량/자재 부족 시 노동자 증가)
            ApplyResourceUrgency(town, ratios);

            // 4. 최소 유지 비율(Hard Floor) 적용 및 정규화
            FinalizeJobRatios(ratios);

            return ratios;
        }

        private static Dictionary<NpcJobClass, double> GetBaseRatios(TownType type) => type switch
        {
            TownType.Industrial => new() { { NpcJobClass.Pauper, 0.2 }, { NpcJobClass.Smelter, 0.5 }, { NpcJobClass.Knight, 0.1 }, { NpcJobClass.CaravanMaster, 0.15 }, { NpcJobClass.Thief, 0.05 } },
            TownType.Academic => new() { { NpcJobClass.Pauper, 0.1 }, { NpcJobClass.Wizard, 0.35 }, { NpcJobClass.Librarian, 0.35 }, { NpcJobClass.Priest, 0.15 }, { NpcJobClass.Mayor, 0.05 } },
            TownType.Metropolis => new() { { NpcJobClass.Pauper, 0.05 }, { NpcJobClass.Smelter, 0.1 }, { NpcJobClass.Knight, 0.2 }, { NpcJobClass.Mayor, 0.25 }, { NpcJobClass.CaravanMaster, 0.2 }, { NpcJobClass.Bard, 0.2 } },
            _ => new() { { NpcJobClass.Pauper, 0.6 }, { NpcJobClass.Smelter, 0.15 }, { NpcJobClass.Knight, 0.1 }, { NpcJobClass.Priest, 0.1 }, { NpcJobClass.Thief, 0.05 } }
        };

        private static void ApplyGentrification(TownEconomy town, Dictionary<NpcJobClass, double> ratios)
        {
            double wealthFactor = Math.Min(1.0, (double)town.TotalWealth / (PlatinumUnit * 10)); 
            if (ratios.ContainsKey(NpcJobClass.Pauper)) ratios[NpcJobClass.Pauper] -= (0.2 * wealthFactor);
            if (ratios.ContainsKey(NpcJobClass.Mayor)) ratios[NpcJobClass.Mayor] += (0.1 * wealthFactor);
            if (ratios.ContainsKey(NpcJobClass.Knight)) ratios[NpcJobClass.Knight] += (0.1 * wealthFactor);
            if (ratios.ContainsKey(NpcJobClass.CaravanMaster)) ratios[NpcJobClass.CaravanMaster] += (0.05 * wealthFactor);
        }

        private static void ApplyResourceUrgency(TownEconomy town, Dictionary<NpcJobClass, double> ratios)
        {
            int totalFood = town.Warehouse.Where(k => typeof(Food).IsAssignableFrom(k.Key) || k.Key.Name.Contains("Raw")).Sum(k => k.Value.Stock);
            int totalMat = town.Warehouse.Where(k => typeof(BaseIngot).IsAssignableFrom(k.Key) || typeof(BaseLeather).IsAssignableFrom(k.Key) || typeof(BaseLog).IsAssignableFrom(k.Key)).Sum(k => k.Value.Stock);

            if (totalFood < 500 && ratios.ContainsKey(NpcJobClass.Pauper)) ratios[NpcJobClass.Pauper] += 0.4;
            if (totalMat < 300 && ratios.ContainsKey(NpcJobClass.Smelter)) ratios[NpcJobClass.Smelter] += 0.3;
        }

        private static void FinalizeJobRatios(Dictionary<NpcJobClass, double> ratios)
        {
            var floors = new Dictionary<NpcJobClass, double> {
                { NpcJobClass.Pauper, 0.05 }, { NpcJobClass.Smelter, 0.15 }, { NpcJobClass.Knight, 0.10 }, { NpcJobClass.Mayor, 0.02 }
            };

            foreach (var floor in floors)
            {
                if (!ratios.ContainsKey(floor.Key) || ratios[floor.Key] < floor.Value)
                    ratios[floor.Key] = floor.Value;
            }

            // 음수 보정 후 1.0 정규화
            foreach (var key in ratios.Keys.ToList()) if (ratios[key] < 0.01) ratios[key] = 0.01;
            double total = ratios.Values.Sum();
            if (total > 0) foreach (var key in ratios.Keys.ToList()) ratios[key] /= total;
        }

        // ====================================================================
        // 🧑‍🤝‍🧑 3. 시민 생성 및 보조 연산 (기존 Helper + Controller 병합)
        // ====================================================================
        public static void AddCitizen(TownEconomy town, NpcJobClass jobHeader, NpcRank skillRank)
        {
            // 헤더(예: 100)를 기반으로 실제 직업(예: 102 Farmer) 추출
            NpcJobClass specificJob = GetRandomSpecificJob(jobHeader);
            NobilityRank socialRank = DetermineNobility(specificJob);

            VirtualCitizen newCitizen = new VirtualCitizen(specificJob, socialRank, 100)
            {
                Rank = skillRank,
                // Helper 통합: 초기 자본금과 가변 수명 세팅
                Gold = CalculateStartingGold(specificJob, skillRank, socialRank, GetTownMultiplier(town.TownIndex)),
                MaxLifespan = GenerateLifespan(socialRank)
            };

            town.Citizens.Add(newCitizen);
        }

        // 상인 수 비례 인구 계산기
		public static int CalculatePopulationCap(TownEconomy town)
		{
			// 등급별 배율: S(20배), A(15배), B(10배), 기타(5배)
			int multiplier = town.TownIndex switch { "S" => 20, "A" => 15, "B" => 10, _ => 5 };
			int basePop = town.TownIndex switch { "S" => 500, "A" => 200, "B" => 100, _ => 50 };

			// 공식: (상인 수 * 배율) + 마을 기본 인구
			return (town.VendorCount * multiplier) + basePop;
		}

        private static int CalculateStartingGold(NpcJobClass job, NpcRank skill, NobilityRank rank, double townIndex)
        {
            int groupID = ((int)job / 100) * 100;
            int jobBase = groupID switch { 100 => 100, 200 => 300, 300 => 500, 400 => 800, 500 => 2000, 600 => 1500, 1100 => 400, _ => 100 };
            int skillMult = skill switch { NpcRank.Novice => 1, NpcRank.Journeyman => 2, NpcRank.Expert => 5, NpcRank.Master => 10, _ => 1 };
            
            var (bonus, _) = GetNobilityData(rank);
            return (int)(((jobBase * skillMult) + bonus) * townIndex);
        }

        private static TimeSpan GenerateLifespan(NobilityRank rank)
        {
            return TimeSpan.FromDays(7 + (int)rank) + TimeSpan.FromHours(Utility.RandomMinMax(-48, 48));
        }

        private static NobilityRank DetermineNobility(NpcJobClass job)
        {
            double roll = Utility.RandomDouble();
            int groupID = ((int)job / 100) * 100;

            if (groupID == 500) return roll < 0.20 ? NobilityRank.Baron : NobilityRank.Knight;
            if (roll < 0.02) return NobilityRank.Knight;
            return NobilityRank.Commoner;
        }

        // --- 유틸리티 헬퍼 ---
        private static (int Bonus, double Rate) GetNobilityData(NobilityRank rank) => rank switch
        {
            NobilityRank.Knight => (7500, 1.2), NobilityRank.SubBaronet => (20000, 1.4), NobilityRank.Baronet => (45000, 1.6),
            NobilityRank.SubBaron => (90000, 1.8), NobilityRank.Baron => (200000, 2.0), NobilityRank.Viscount => (450000, 2.2),
            NobilityRank.Count => (1000000, 2.5), NobilityRank.Marquis => (3000000, 3.0), _ => (0, 1.0)
        };

        private static double GetTownMultiplier(string index) => index switch { "S" => 2.5, "A" => 1.8, "B" => 1.2, _ => 0.8 };

        private static NpcRank GetRandomRank()
        {
            int chance = Utility.Random(100);
            return chance < 50 ? NpcRank.Novice : chance < 80 ? NpcRank.Journeyman : chance < 95 ? NpcRank.Expert : NpcRank.Master;
        }

        private static NpcJobClass GetRandomJobFromWeights(Dictionary<NpcJobClass, double> weights)
        {
            double roll = Utility.RandomDouble();
            double cumulative = 0.0;
            foreach (var kvp in weights)
            {
                cumulative += kvp.Value;
                if (roll <= cumulative) return kvp.Key;
            }
            return NpcJobClass.Pauper;
        }

        private static NpcJobClass GetRandomSpecificJob(NpcJobClass groupHeader)
        {
            int headerVal = (int)groupHeader;
            var jobsInGroup = Enum.GetValues(typeof(NpcJobClass)).Cast<NpcJobClass>()
                                  .Where(j => (int)j >= headerVal && (int)j < headerVal + 100).ToList();
            return jobsInGroup.Count > 0 ? jobsInGroup[Utility.Random(jobsInGroup.Count)] : NpcJobClass.Laborer;
        }
    }
}