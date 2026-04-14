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
        // 🌟 1. 메인 파이프라인 (1/10 인구 및 마을 특색(Lore) 기반 생성)
        // ====================================================================
        public static void InitializeTown(TownEconomy town, int targetPop)
        {
            if (town == null) return;

            // 기존 인구 및 가문 데이터 초기화
            town.Citizens.Clear();
            town.Houses.Clear();

            double townMultiplier = GetTownMultiplier(town.TownIndex);

            // [기획 반영] 인구는 무조건 평민(Commoner)부터 시작하며, 마을 특색에 맞는 직업을 가짐
            for (int i = 0; i < targetPop; i++)
            {
                NobilityRank startingRank = NobilityRank.Commoner;
                NpcJobClass assignedJob = AssignJobByTown(town.TownName);
                NpcRank skillLevel = GetRandomRank();

                AddCitizen(town, assignedJob, skillLevel, startingRank, townMultiplier);
            }

            Console.WriteLine($"[{town.TownName}] 소수 정예 맞춤형 인구 배치 완료: {town.Citizens.Count}명");
        }

        // ====================================================================
        // 🌟 2. 18개 마을별 고유 특색(Lore)에 맞춘 직업 분배 로직
        // ====================================================================
        private static NpcJobClass AssignJobByTown(string townName)
        {
            string tName = townName.ToLower();

            // 1. 씨 마켓 (Sea Market): 100% 어부/잠수부
            if (tName.Contains("sea market"))
            {
                return Utility.RandomBool() ? NpcJobClass.DeepSeaFisher_Basic : NpcJobClass.CoastalFisher;
            }

            // 2. 파푸아 & 델루시아 (오지): 1차 산업(광/농/벌목)에 100% 종사
            if (tName.Contains("papua") || tName.Contains("delucia"))
            {
                int roll = Utility.Random(100);
                if (roll < 30) return (NpcJobClass)Utility.RandomMinMax(113, 122); // 농부/과수원
                if (roll < 50) return (NpcJobClass)Utility.RandomMinMax(136, 138); // 광부
                if (roll < 70) return (NpcJobClass)Utility.RandomMinMax(133, 135); // 벌목꾼
                if (roll < 90) return (NpcJobClass)Utility.RandomMinMax(128, 132); // 목동
                return NpcJobClass.Laborer;
            }

            // 3. 마진시아 (Magincia): 폐허 컨셉에 맞춰 채집, 청소부
            if (tName.Contains("magincia"))
            {
                int roll = Utility.Random(100);
                if (roll < 40) return (NpcJobClass)Utility.RandomMinMax(113, 116); // 채집가
                if (roll < 70) return (NpcJobClass)Utility.RandomMinMax(124, 126); // 해변/미역 수집가
                return NpcJobClass.StreetSweeper; 
            }

            // 4. 부케니어스 덴 (Buccaneer's Den): 음지 경제 및 도둑들
            if (tName.Contains("buccaneer"))
            {
                int roll = Utility.Random(100);
                if (roll < 40) return NpcJobClass.Thief;
                if (roll < 70) return NpcJobClass.Assassin;
                if (roll < 85) return NpcJobClass.InnKeeper;
                return NpcJobClass.Trapper;
            }

            // 5. 일반 대도시 (Britain, Trinsic, Minoc 등): 밸런스 있는 도시형 직업군
            int baseGroup = Utility.RandomMinMax(1, 10) * 100; 
            
            // 도시 내 모험가(전사/마법사) 계층 비율 조절 (평민 중 15% 정도만)
            if ((baseGroup == 600 || baseGroup == 700 || baseGroup == 800) && Utility.RandomDouble() > 0.15)
            {
                baseGroup = Utility.RandomMinMax(1, 5) * 100; // 기초 생산/제작직으로 강등
            }

            return SelectValidJobFromGroup(baseGroup, NobilityRank.Commoner);
        }

        // 지정된 직업 그룹(100, 200 등) 내에서 현재 신분(Commoner)이 가질 수 있는 직업 필터링
        private static NpcJobClass SelectValidJobFromGroup(int groupID, NobilityRank rank)
        {
            var candidates = new List<NpcJobClass>();
            
            foreach (NpcJobClass job in VirtualJobCore.AllJobs)
            {
                if (((int)job / 100) * 100 == groupID)
                {
                    var profile = VirtualJobCore.GetDeepJobProfile(job);
                    if (profile.MinRank <= rank)
                    {
                        candidates.Add(job);
                    }
                }
            }

            if (candidates.Count == 0) return NpcJobClass.Laborer;

            return candidates[Utility.Random(candidates.Count)];
        }


        // ====================================================================
        // 🌟 3. 시민 속성, 가문(House), 자본 생성 엔진
        // ====================================================================
        public static void AddCitizen(TownEconomy town, NpcJobClass job, NpcRank skill, NobilityRank rank, double townM)
        {
            int satisfaction = Utility.RandomMinMax(60, 90);
            
            VirtualCitizen citizen = new VirtualCitizen(job, rank, satisfaction) 
            {
                RankLevel = rank,
                Potential = 1.0 + (Utility.RandomDouble() * 1.5),
                BirthTime = DateTime.Now,
                TargetRegionName = town.TownName // [추가] 소속 마을 명시
            };
            
            // 가문명을 짓기 전에 시민의 성별에 맞는 랜덤 이름을 먼저 부여
            citizen.Name = NameList.RandomName(citizen.Gender == Gender.Female ? "female" : "male");

            int adultMinAge = (int)(citizen.MaxLifespan.TotalMinutes * 0.15);
            int adultMaxAge = (int)(citizen.MaxLifespan.TotalMinutes * 0.80);
            citizen.BirthTime = DateTime.Now - TimeSpan.FromMinutes(Utility.RandomMinMax(adultMinAge, adultMaxAge));
            
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
            // 10배수 경제 인플레이션 반영 (최소 5천 ~ 최대 15만 베이스)
            double baseG = group switch { 
                100 => 5000, 200 => 8000, 300 => 10000, 400 => 15000, 
                500 => 150000, 600 => 80000, 700 => 10000, 800 => 8000, 
                900 => 12000, 1000 => 15000, 1100 => 5000, _ => 5000 
            };
            
            return (int)(baseG * Math.Pow(1.5, (int)skill) * Math.Pow(1.5, (int)rank) * townM);
        }

        private static NpcRank GetRandomRank()
        {
            int roll = Utility.Random(100);
            return roll < 40 ? NpcRank.Novice : roll < 70 ? NpcRank.Journeyman : roll < 90 ? NpcRank.Expert : NpcRank.Master;
        }

        private static double GetTownMultiplier(string index) => index switch { "S" => 2.5, "A" => 1.8, "B" => 1.2, _ => 0.8 };

        // ====================================================================
        // 🌟 4. 대륙별(Facet) 자동 스폰 및 인구 상한선 1/10 적용
        // ====================================================================
        public static void RespawnFacet(Map facet, Mobile from)
        {
            if (facet == null || facet == Map.Internal) return;
            var towns = TownEconomyManager.Towns.Values.Where(t => t.Facet == facet).ToList();
            foreach (var town in towns)
            {
                // 상인 수 실시간 카운트
                town.VendorCount = World.Mobiles.Values.OfType<BaseVendor>()
                    .Count(v => v.Map == facet && TownNumber.GetID(v.Location, v.Map) == town.TownID && !(v is Banker));
                
                // 새로운 1/10 캡 연산식 호출
                int newCap = CalculatePopulationCap(town);
                InitializeTown(town, newCap);
            }
            from.SendMessage(66, $"{facet.Name} 대륙의 인구 배치를 소수 정예로 갱신했습니다.");
        }

        public static int CalculatePopulationCap(TownEconomy town)
        {
            if (town == null) return 0;

            // [기획 반영] 과거 수천 명의 캡을 없애고, VendorCount 그대로(1/10) 사용합니다.
            int basePop = town.VendorCount;
            
            // 최소 인구 보장 (아무리 상인이 없어도 5가문은 살도록)
            if (basePop < 5) basePop = 5;

            // 도시는 100% 온전히 유지, 시골 외곽은 절반 수준으로 억제
            bool isCity = (town.TownID % 100) < 50;
            return isCity ? basePop : (basePop / 2);
        }
    }
}