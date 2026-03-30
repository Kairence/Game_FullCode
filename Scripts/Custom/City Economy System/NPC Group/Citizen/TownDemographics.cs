using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
    public enum TownType { Agricultural, Industrial, Academic, Metropolis }

    public class TownDemographics
    {
        public const long PlatinumUnit = 100000000;

 // ====================================================================
        // ??? 1. 메인 파이프라인 (50/50 직업 구성 및 도시/외곽 차별화)
        // ====================================================================
        public static void InitializeTown(TownEconomy town, int targetPop)
        {
            if (town == null) return;

            town.Citizens.Clear();
			town.Houses.Clear();

            // [기획 반영] 잠재 총 인구 계산 (S: +500, A: +200 등)
            int multiplier = town.TownIndex switch { "S" => 20, "A" => 15, "B" => 10, _ => 5 };
            int extraPop = town.TownIndex switch { "S" => 500, "A" => 200, "B" => 100, _ => 50 };
            int totalPotential = (town.VendorCount * multiplier) + extraPop;
            
            // 모든 마을은 잠재 인구의 50%를 '경제 핵심군'으로 가짐
            int coreCount = totalPotential / 2;
            double townMultiplier = GetTownMultiplier(town.TownIndex);
            bool isCity = (town.TownID % 100) < 50;

            // 1-A. [경제 핵심군] 창고와 구매/판매가 모두 연동된 직업군 추출
            var warehouseJobs = Enum.GetValues<NpcJobClass>()
                .Cast<NpcJobClass>()
                .Where(j => {
                    var p = VirtualJobCore.GetDeepJobProfile(j);
                    // 튜플 방어 코드 및 구매/판매 연동 체크
                    bool buys = (p.Necessities != null && p.Necessities.Length > 0) || 
                                (p.JobMaterials != null && p.JobMaterials.Length > 0) || 
                                (p.Luxuries != null && p.Luxuries.Length > 0);
                    bool sells = p.Produces != null && p.Produces.Length > 0;
                    return buys && sells;
                }).ToList();

            if (warehouseJobs.Count == 0) warehouseJobs = Enum.GetValues<NpcJobClass>().Cast<NpcJobClass>().ToList();

            // 핵심 직업군 생성 (모든 마을 공통 50% 파이)
            for (int i = 0; i < coreCount; i++)
            {
                NpcJobClass job = warehouseJobs[Utility.Random(warehouseJobs.Count)];
                NobilityRank rank = DetermineNobility(job);
                AddCitizen(town, GetSpecificJobForRankAndGroup(rank, job), GetRandomRank(), rank, townMultiplier);
            }

            // 1-B. [유동 랜덤군] 도시에만 추가되는 나머지 50% 파이
            if (isCity)
            {
                var allJobs = Enum.GetValues<NpcJobClass>().Cast<NpcJobClass>().ToList();
                int randomCount = targetPop - coreCount; // 도시인 경우 나머지 절반을 채움

                for (int i = 0; i < randomCount; i++)
                {
                    NpcJobClass job = allJobs[Utility.Random(allJobs.Count)];
                    NobilityRank rank = DetermineNobility(job);
                    AddCitizen(town, GetSpecificJobForRankAndGroup(rank, job), GetRandomRank(), rank, townMultiplier);
                }
            }

            // 2. [핵심 추가] 마을별 특색(Lore)에 맞는 보너스 인구(+a) 투입!
            ApplyTownSpecialties(town, townMultiplier); 

            Console.WriteLine($"[{town.TownName}] 인구 배치 완료: {town.Citizens.Count}명 (도시여부: {isCity})");
        }


        // ====================================================================
        // ?? 2. 신분 및 직업 결정 엔진 (복리 대응)
        // ====================================================================
        private static NpcJobClass GetSpecificJobForRankAndGroup(NobilityRank rank, NpcJobClass groupHeader)
        {
            int startRange = (int)groupHeader;
            int endRange = startRange + 100;

            var validJobs = Enum.GetValues(typeof(NpcJobClass))
                                .Cast<NpcJobClass>()
                                .Where(j => (int)j >= startRange && (int)j < endRange)
                                .Where(j => 
                                {
                                    var profile = VirtualJobCore.GetDeepJobProfile(j);
                                    return rank >= profile.MinRank && rank <= profile.MaxRank;
                                }).ToList();

            if (validJobs.Count == 0)
            {
                validJobs = Enum.GetValues(typeof(NpcJobClass))
                                .Cast<NpcJobClass>()
                                .Where(j => (int)j >= startRange && (int)j < endRange).ToList();
            }

            return validJobs.Count > 0 ? validJobs[Utility.Random(validJobs.Count)] : groupHeader;
        }

        private static NobilityRank DetermineNobility(NpcJobClass jobGroup)
        {
            double roll = Utility.RandomDouble();
            int groupID = ((int)jobGroup / 100) * 100;

            if (groupID == 500) return roll < 0.25 ? NobilityRank.Baron : NobilityRank.Knight;
            if ((groupID == 600 || groupID == 1000) && roll < 0.12) return NobilityRank.Knight;

            return roll < 0.03 ? NobilityRank.Knight : NobilityRank.Commoner;
        }

        // ====================================================================
        // ?? 3. 복리(Multiplicative) 가중치 최적화
        // ====================================================================
        private static Dictionary<NpcJobClass, double> OptimizeDemographics(TownEconomy town)
        {
            var ratios = GetBaseRatios(town.Type);

            double wealthFactor = 1.0 + (Math.Min(10.0, (double)town.Wealth / (PlatinumUnit * 5)) * 0.1); 
            
            if (ratios.ContainsKey((NpcJobClass)500)) ratios[(NpcJobClass)500] *= wealthFactor;
            if (ratios.ContainsKey((NpcJobClass)600)) ratios[(NpcJobClass)600] *= (wealthFactor * 0.9);

            int totalFood = town.Warehouse.Values.Sum(v => v.Stock);
            if (totalFood < 500)
            {
                if (ratios.ContainsKey((NpcJobClass)100)) ratios[(NpcJobClass)100] *= 2.0;
            }

            int totalMat = town.Warehouse.Count;
            if (totalMat < 20)
            {
                if (ratios.ContainsKey((NpcJobClass)200)) ratios[(NpcJobClass)200] *= 1.5;
            }

            double sum = ratios.Values.Sum();
            if (sum > 0)
            {
                var keys = ratios.Keys.ToList();
                foreach (var key in keys) ratios[key] /= sum;
            }

            return ratios;
        }

        private static Dictionary<NpcJobClass, double> GetBaseRatios(TownType type)
        {
            return new Dictionary<NpcJobClass, double>
            {
                { (NpcJobClass)100, 0.30 }, { (NpcJobClass)200, 0.20 }, { (NpcJobClass)300, 0.10 },
                { (NpcJobClass)400, 0.05 }, { (NpcJobClass)500, 0.02 }, { (NpcJobClass)600, 0.10 },
                { (NpcJobClass)700, 0.05 }, { (NpcJobClass)800, 0.08 }, { (NpcJobClass)900, 0.05 },
                { (NpcJobClass)1000, 0.02 }, { (NpcJobClass)1100, 0.03 }
            };
        }

        // ====================================================================
        // ?? 4. 시민 속성 및 자본 생성
        // ====================================================================
        public static void RespawnFacet(Map facet, Mobile from)
        {
            if (facet == null || facet == Map.Internal) return;
            var towns = TownEconomyManager.Towns.Values.Where(t => t.Facet == facet).ToList();
            foreach (var town in towns)
            {
                town.VendorCount = World.Mobiles.Values.OfType<BaseVendor>()
                    .Count(v => v.Map == facet && TownNumber.GetID(v.Location, v.Map) == town.TownID && !(v is Banker));
                InitializeTown(town, CalculatePopulationCap(town));
            }
            from.SendMessage(66, $"{facet.Name} 대륙의 인구 배치를 갱신했습니다.");
        }

		// ====================================================================
        // ?? [신규] 18개 마을별 고유 특색(Lore) 부여 엔진
        // ====================================================================
        private static void ApplyTownSpecialties(TownEconomy town, double townMultiplier)
        {
            string name = town.TownName.ToLower();
            List<(NpcJobClass Job, int Count)> specialties = new();

            // 1. 브리튼 (수도: 귀족, 근위대, 바드)
            if (name.Contains("britain")) {
                specialties.Add((NpcJobClass.TownGuard, 15));
                specialties.Add((NpcJobClass.Aristocrat, 10));
                specialties.Add((NpcJobClass.Bard, 5));
            }
            // 2. 미녹 (광산: 광부, 제련공, 땜장이)
            else if (name.Contains("minoc")) {
                specialties.Add((NpcJobClass.SurfaceMiner, 15));
                specialties.Add((NpcJobClass.StoneQuarryman, 10));
                specialties.Add((NpcJobClass.Smelter, 10));
                specialties.Add((NpcJobClass.PigIronWorker, 5));
            }
            // 3. 문글로우 (마법: 마법사, 연금술사, 천문학자)
            else if (name.Contains("moonglow")) {
                specialties.Add((NpcJobClass.Wizard, 15));
                specialties.Add((NpcJobClass.Alchemist, 10));
                specialties.Add((NpcJobClass.Astronomer_Scholar, 5));
            }
            // 4. 버커니어스 덴 (무법지대: 도둑, 밀수꾼, 해적)
            else if (name.Contains("buccaneer")) {
                specialties.Add((NpcJobClass.Smuggler, 15));
                specialties.Add((NpcJobClass.Cutpurse, 10));
                specialties.Add((NpcJobClass.ShipCaptain, 5));
            }
            // 5. 젤롬 (용병: 검투사, 무장병)
            else if (name.Contains("jhelom")) {
                specialties.Add((NpcJobClass.Duelist, 15));
                specialties.Add((NpcJobClass.Swashbuckler, 10));
                specialties.Add((NpcJobClass.Militia_Warrior, 10));
            }
            // 6. 마진시아 (오만함: 대상인, 귀족, 사교계 명사)
            else if (name.Contains("magincia")) {
                specialties.Add((NpcJobClass.DeedBroker_Merchant, 10));
                specialties.Add((NpcJobClass.Aristocrat, 10));
                specialties.Add((NpcJobClass.Socialite, 5));
            }
            // 7. 스카라 브라에 (자연/영성: 레인저, 치료사)
            else if (name.Contains("skara")) {
                specialties.Add((NpcJobClass.Trapper, 15));
                specialties.Add((NpcJobClass.BirdHunter, 10));
                specialties.Add((NpcJobClass.Healer_Master, 5));
            }
            // 8. 트린식 (명예: 팔라딘, 무구 제작자)
            else if (name.Contains("trinsic")) {
                specialties.Add((NpcJobClass.Paladin, 15));
                specialties.Add((NpcJobClass.Crusader, 10));
                specialties.Add((NpcJobClass.ArmamentMajor, 5));
            }
            // 9. 베스퍼 (운하/상업: 심해어부, 해상 상인)
            else if (name.Contains("vesper")) {
                specialties.Add((NpcJobClass.DeepSeaFisher, 15));
                specialties.Add((NpcJobClass.MaritimeTrader, 10));
                specialties.Add((NpcJobClass.Shipwright_Master, 5));
            }
            // 10. 유 (법과 깊은 숲: 벌목꾼, 판사, 법무관)
            else if (name.Contains("yew")) {
                specialties.Add((NpcJobClass.Woodcutter, 20));
                specialties.Add((NpcJobClass.Magistrate, 5));
                specialties.Add((NpcJobClass.LegalAdvocate, 5));
            }
            // 11. 코브 (은둔/연인: 마을 경비대, 버섯 채집가)
            else if (name.Contains("cove")) {
                specialties.Add((NpcJobClass.TownGuard, 10));
                specialties.Add((NpcJobClass.MushroomGatherer, 10));
            }
            // 12. 누젤름 (휴양/사막: 무희, 귀족, 캐러밴 마스터)
            else if (name.Contains("nujel'm") || name.Contains("nujelm")) {
                specialties.Add((NpcJobClass.Dancer, 15));
                specialties.Add((NpcJobClass.Aristocrat, 10));
                specialties.Add((NpcJobClass.CaravanMaster, 5));
            }
            // 13. 서펀츠 홀드 (전초기지: 기사, 창병)
            else if (name.Contains("serpent")) {
                specialties.Add((NpcJobClass.Knight, 15));
                specialties.Add((NpcJobClass.Halberdier, 15));
            }
            // 14. 윈드 (비밀 마법 도시: 대마법사, 네크로맨서)
            else if (name.Contains("wind")) {
                specialties.Add((NpcJobClass.Archmage, 15));
                specialties.Add((NpcJobClass.Necromancer, 10));
                specialties.Add((NpcJobClass.Evoker, 5));
            }
            // 15. 헤이븐 (초보자: 교관, 학생, 신병)
            else if (name.Contains("haven")) {
                specialties.Add((NpcJobClass.Professor_Scholar, 5));
                specialties.Add((NpcJobClass.Student_Scholar, 15));
                specialties.Add((NpcJobClass.Recruit, 10));
            }
            // 16. 하트우드 (엘프/자연: 나무 가공, 드루이드)
            else if (name.Contains("heartwood")) {
                specialties.Add((NpcJobClass.Woodcutter, 10));
                specialties.Add((NpcJobClass.Sawyer, 10));
                specialties.Add((NpcJobClass.Druid, 10));
            }
            // 17. 델루시아 (농경/목축: 소몰이꾼, 양치기)
            else if (name.Contains("delucia")) {
                specialties.Add((NpcJobClass.CattleDrover, 15));
                specialties.Add((NpcJobClass.Shepherd, 10));
                specialties.Add((NpcJobClass.StableHand, 5));
            }
            // 18. 파푸아 (정글/주술: 마녀, 독술사, 약초꾼)
            else if (name.Contains("papua")) {
                specialties.Add((NpcJobClass.Witch, 10));
                specialties.Add((NpcJobClass.Venomist, 10));
                specialties.Add((NpcJobClass.Herbalist, 10));
            }

            // 추가된 특수 직업들을 마을에 스폰
            foreach (var spec in specialties)
            {
                for (int i = 0; i < spec.Count; i++)
                {
                    NobilityRank rank = DetermineSpecialtyRank(spec.Job);
                    NpcRank skill = GetRandomRank();
                    //AddCitizen(town, spec.Job, skill, rank, townMultiplier);
                }
            }
        }
		
		// [헬퍼] 특수 스폰 NPC가 터무니없는 신분을 갖지 않도록 보정 (예: 마법사는 최소 기사급, 근위대는 평민 등)
        private static NobilityRank DetermineSpecialtyRank(NpcJobClass job)
        {
            int group = ((int)job / 100) * 100;
            if (group == 500) return NobilityRank.Baron; // 귀족 계급은 무조건 남작 이상
            if (group == 300) return NobilityRank.Knight; // 전사/경비대는 기사급 대우
            if (group == 400 || group == 1000) return Utility.RandomBool() ? NobilityRank.Knight : NobilityRank.Commoner; // 학자/법사는 복불복
            return NobilityRank.Commoner; // 나머지 생산직/범죄자는 평민
        }

        public static int CalculatePopulationCap(TownEconomy town)
        {
            if (town == null) return 0;

            int multiplier = town.TownIndex switch { "S" => 20, "A" => 15, "B" => 10, _ => 5 };
            int extraPop = town.TownIndex switch { "S" => 500, "A" => 200, "B" => 100, _ => 50 };

            // 잠재 총 인구 (S등급 기준 상인계산 + 500)
            int totalPotential = (town.VendorCount * multiplier) + extraPop;

            // [기획 핵심] 도시는 잠재 인구 100% 사용, 외곽은 절반(50%)만 사용
            bool isCity = (town.TownID % 100) < 50;
            
            return isCity ? totalPotential : (totalPotential / 2);
        }

		// ====================================================================
		// ?? 4. 시민 속성 및 자본 생성 (가문명 버그 수정 버전)
		// ====================================================================
		public static void AddCitizen(TownEconomy town, NpcJobClass job, NpcRank skill, NobilityRank rank, double townM)
		{
			int satisfaction = Utility.RandomMinMax(60, 90);
			
			VirtualCitizen citizen = new VirtualCitizen(job, rank, satisfaction) 
			{
				RankLevel = rank,
				Potential = 1.0 + (Utility.RandomDouble() * 1.5),
				BirthTime = DateTime.Now // 규칙 준수: Now 사용
			};
			
			// [버그 수정] 가문명을 짓기 전에 시민의 성별에 맞는 랜덤 이름을 먼저 부여합니다.
			citizen.Name = NameList.RandomName(citizen.Gender == Gender.Female ? "female" : "male");

			int adultMinAge = (int)(citizen.MaxLifespan.TotalMinutes * 0.15);
			int adultMaxAge = (int)(citizen.MaxLifespan.TotalMinutes * 0.80);
			citizen.BirthTime = DateTime.Now - TimeSpan.FromMinutes(Utility.RandomMinMax(adultMinAge, adultMaxAge));
			
			// 이제 citizen.Name이 "Citizen"이 아닌 실제 이름(예: "John")이므로 "John House"로 생성됩니다.
			string houseName = $"{citizen.Name} House";
			var newHouse = new VirtualHouse(houseName, rank) 
			{ 
				IsActive = true,
				Prestige = 10,
				TotalWealth = CalculateCompoundedGold(job, skill, rank, townM)
			};

			VirtualCitizen father = citizen.Gender == Gender.Male ? citizen : null;
			VirtualCitizen mother = citizen.Gender == Gender.Female ? citizen : null;
			
			var newFamily = new FamilyUnit(father, mother) 
			{ 
				IsActive = true 
			};

			// 상호 참조 연결 및 시스템 등록
			newHouse.Families.Add(newFamily);
			citizen.House = newHouse;
			citizen.Family = newFamily;

			if (!town.Houses.Contains(newHouse)) town.Houses.Add(newHouse);
			if (!town.Citizens.Contains(citizen)) town.Citizens.Add(citizen);
		}
		private static int CalculateCompoundedGold(NpcJobClass job, NpcRank skill, NobilityRank rank, double townM)
        {
            int group = ((int)job / 100) * 100;
            // [수정] 경제 규모 인플레이션 반영 (최소 5천 ~ 최대 15만 베이스)
            double baseG = group switch { 
                100 => 5000, 200 => 8000, 300 => 10000, 400 => 15000, 
                500 => 150000, 600 => 80000, 700 => 10000, 800 => 8000, 
                900 => 12000, 1000 => 15000, 1100 => 5000, _ => 5000 
            };
            
            // 스킬(1.5배수)과 작위(1.5배수)가 겹치면 최대 수십만 골드로 뻥튀기 됨
            return (int)(baseG * Math.Pow(1.5, (int)skill) * Math.Pow(1.5, (int)rank) * townM);
        }

        private static TimeSpan GenerateLifespan(NobilityRank rank) => 
            TimeSpan.FromDays(14 * Math.Pow(1.1, (int)rank)) + TimeSpan.FromHours(Utility.RandomMinMax(-24, 24));

        private static NpcRank GetRandomRank()
        {
            int roll = Utility.Random(100);
            return roll < 40 ? NpcRank.Novice : roll < 70 ? NpcRank.Journeyman : roll < 90 ? NpcRank.Expert : NpcRank.Master;
        }

        private static NpcJobClass GetRandomJobFromWeights(Dictionary<NpcJobClass, double> weights)
        {
            double roll = Utility.RandomDouble();
            double cumulative = 0.0;
            foreach (var kvp in weights) { cumulative += kvp.Value; if (roll <= cumulative) return kvp.Key; }
            return (NpcJobClass)100;
        }

        private static double GetTownMultiplier(string index) => index switch { "S" => 2.5, "A" => 1.8, "B" => 1.2, _ => 0.8 };
    }
}