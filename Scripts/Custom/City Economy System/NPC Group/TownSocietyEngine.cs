using System;
using System.Collections.Generic;
using System.Linq;
using Server;

namespace Server.Misc
{
    public static class TownSocietyEngine
    {
        // ====================================================================
        // 🌆 1. 저녁(18:00) 틱: 사회 지표 및 결혼 처리
        // ====================================================================
        public static void ProcessEveningSocialTick(TownEconomy town)
        {
            if (town == null || town.Citizens == null) return;

            // 1. 사회 지표 업데이트 (명성 감소, 소문 발생, 작위 승강급)
            UpdateSocialStatus(town);

            // 2. 미혼 성인 남녀 매칭 및 결혼
            ProcessMarriages(town);
        }

        // ====================================================================
        // 🌃 2. 심야(24:00) 틱: 생애 주기 및 세대교체 처리
        // ====================================================================
        public static void ProcessMidnightLifeCycleTick(TownEconomy town)
        {
            if (town == null || town.Citizens == null) return;

            // 1. 가족 단위 출산 및 공동 자산 취합
            UpdateFamilies(town);

            // 2. 사망자 처리 및 상속 (리스트 순회 중 수정을 피하기 위해 ToList 사용)
            var expiredCitizens = town.Citizens.Where(c => c.IsExpired || c.IsStarving || c.IsDehydrated).ToList();
            foreach (var deceased in expiredCitizens)
            {
                PerformSuccession(deceased, town);
            }
        }

        // ====================================================================
        // 🗣️ 3. 사회 및 명성 (Social & Rank) 로직
        // ====================================================================
        private static void UpdateSocialStatus(TownEconomy town)
        {
            if (town.Houses == null) return;

            foreach (var house in town.Houses)
            {
                if (house.Families == null) continue;

                foreach (var family in house.Families)
                {
                    if (family == null) continue;

                    ProcessAgentStatus(family.Father);
                    ProcessAgentStatus(family.Mother);
                    
                    if (family.Children != null)
                    {
                        foreach (var child in family.Children.Where(c => c != null && !c.IsChild))
                            ProcessAgentStatus(child);
                    }
                }
                // 가문 단위 소문 발생
                ApplyGossip(house);
            }
        }

        private static void ProcessAgentStatus(VirtualCitizen agent)
        {
            if (agent == null) return;

            // 명성 자연 감소 (30,000 스케일)
            if (agent.Fame > 2000)
            {
                int decay = agent.Fame >= 25000 ? Utility.RandomMinMax(50, 100) : 
                            agent.Fame >= 10000 ? Utility.RandomMinMax(20, 40) : 10;
                agent.Fame -= decay;
            }

            // 작위 승강급 체크
            CheckRankTransition(agent);
        }

        private static void ApplyGossip(VirtualHouse house)
        {
            if (house.Families == null || house.Families.Count == 0) return;

            int fameChange = Utility.RandomMinMax(-50, 50);

            // 긍정적 소문 보정
            if (house.Families[0].Father != null && house.Families[0].Father.Karma > 5000 && Utility.RandomDouble() > 0.4)
                fameChange = Math.Abs(fameChange);

            foreach (var family in house.Families)
            {
                if (family == null) continue;
                if (family.Father != null) { family.Father.Fame += (fameChange / 2); CheckRankTransition(family.Father); }
                if (family.Mother != null) { family.Mother.Fame += (fameChange / 2); CheckRankTransition(family.Mother); }
            }
        }

        private static void CheckRankTransition(VirtualCitizen agent)
        {
            int fame = agent.Fame;
            NobilityRank currentRank = agent.RankLevel;

            if (currentRank < NobilityRank.Marquis && fame >= GetRequiredFame(currentRank + 1))
            {
                agent.RankLevel = currentRank + 1;
                agent.Satisfaction = Math.Min(100, agent.Satisfaction + 30);
                if (agent.House != null) agent.House.Prestige += 20;
                Console.WriteLine($"[Rank Up] {agent.Name}: {agent.RankLevel} 작위 수여! (Fame: {fame})");
            }
            else if (currentRank > NobilityRank.Commoner && fame < GetRequiredFame(currentRank) - 1000)
            {
                agent.RankLevel = currentRank - 1;
                agent.Satisfaction = Math.Max(0, agent.Satisfaction - 40);
                if (agent.House != null) agent.House.Prestige = Math.Max(0, agent.House.Prestige - 15);
                Console.WriteLine($"[Rank Down] {agent.Name}: {agent.RankLevel}(으)로 강등... (Fame: {fame})");
            }
        }

        private static int GetRequiredFame(NobilityRank rank) => rank switch {
            NobilityRank.Knight => 3000, NobilityRank.SubBaronet => 6500, NobilityRank.Baronet => 10000,
            NobilityRank.SubBaron => 14000, NobilityRank.Baron => 18500, NobilityRank.Viscount => 23000,
            NobilityRank.Count => 27500, NobilityRank.Marquis => 29500, _ => 0
        };

        // ====================================================================
        // 💍 4. 결혼 로직 (Marriage)
        // ====================================================================
        private static void ProcessMarriages(TownEconomy town)
        {
            var bachelors = town.Citizens.Where(c => c.Gender == Gender.Male && c.Family == null && IsEligibleForMarriage(c)).ToList();
            var bachelorettes = town.Citizens.Where(c => c.Gender == Gender.Female && c.Family == null && IsEligibleForMarriage(c)).ToList();

            foreach (var male in bachelors)
            {
                var bride = bachelorettes.FirstOrDefault(f => f.Family == null && male.House != f.House && CheckAgeGap(male, f));
                if (bride != null)
                {
                    FormFamily(male, bride, town);
                    bachelorettes.Remove(bride);
                }
            }
        }

        private static bool IsEligibleForMarriage(VirtualCitizen c) { double p = (double)c.Age / c.MaxLifespan.TotalMinutes; return p >= 0.2 && p <= 0.7; }
        private static bool CheckAgeGap(VirtualCitizen p1, VirtualCitizen p2) => Math.Abs(p1.Age - p2.Age) <= (p1.MaxLifespan.TotalMinutes * 0.2);

        private static void FormFamily(VirtualCitizen male, VirtualCitizen female, TownEconomy town)
        {
            var newFamily = new FamilyUnit(male, female) { SharedWealth = male.Gold + female.Gold };
            male.Family = female.Family = newFamily;
            
            if (male.House != null) { male.House.Prestige += 10; if (!male.House.Families.Contains(newFamily)) male.House.Families.Add(newFamily); female.House = male.House; }
            if (female.House != null) female.House.Prestige += 10;
            
            Console.WriteLine($"[Marriage] {town.TownName}: {male.Name} & {female.Name} 성혼 완료.");
        }

        // ====================================================================
        // 👶 5. 출산 및 가족 자산 로직 (Family)
        // ====================================================================
        private static void UpdateFamilies(TownEconomy town)
        {
            if (town.Houses == null) return;
            foreach (var house in town.Houses)
            {
                foreach (var family in house.Families.ToList())
                {
                    if (family == null) continue;
                    
                    // 출산 체크
                    if (family.Children.Count < 3 && family.Father != null && family.Mother != null && IsEligibleForMarriage(family.Father) && IsEligibleForMarriage(family.Mother))
                    {
                        if (Utility.RandomDouble() < (0.05 * (1 + (house.Prestige * 0.001))))
                            CreateChild(family, house, town);
                    }

                    // 자산 취합
                    TransferToSharedWealth(family.Father, family);
                    TransferToSharedWealth(family.Mother, family);
                }
            }
        }

        private static void TransferToSharedWealth(VirtualCitizen c, FamilyUnit f) { if (c != null && c.Gold > 100) { f.SharedWealth += (c.Gold - 100); c.Gold = 100; } }

        private static void CreateChild(FamilyUnit family, VirtualHouse house, TownEconomy town)
        {
            NpcJobClass job = Utility.RandomBool() ? family.Father.JobClass : family.Mother.JobClass;
            var child = new VirtualCitizen(job, NobilityRank.Commoner, 100) { House = house, Family = family };
            
            ApplyGenetics(child, family.Father, family.Mother);
            family.Children.Add(child);
            town.Citizens.Add(child);
            Console.WriteLine($"[Birth] {town.TownName}: {house.HouseName} 가문 자녀 탄생!");
        }

        // ====================================================================
        // ⚰️ 6. 사망 및 상속 로직 (Inheritance)
        // ====================================================================
        private static void PerformSuccession(VirtualCitizen deceased, TownEconomy town)
        {
            var candidates = town.Citizens.Where(c => c != deceased).ToList();
            var p1 = candidates.Count > 0 ? candidates[Utility.Random(candidates.Count)] : null;
            var p2 = candidates.Count > 1 ? candidates[Utility.Random(candidates.Count)] : null;

            int tax = (int)(deceased.Gold * 0.3);
            int legacy = deceased.Gold - tax;
            town.Wealth += tax;

            var child = new VirtualCitizen(deceased.JobClass, NobilityRank.Commoner, 70);
            
            double townM = town.TownIndex switch { "S" => 2.5, "A" => 1.8, "B" => 1.2, _ => 0.8 };
            child.Gold = (int)(5000 * townM) + legacy;
            
            ApplyGenetics(child, p1, p2);
            child.Potential = (p1 != null) ? Math.Max(1.0, p1.Potential * Utility.RandomMinMax(80, 120) / 100.0) : 1.0;
            child.MaxLifespan = TimeSpan.FromHours(Utility.RandomMinMax(168, 336));
            if (deceased.House != null) child.House = deceased.House;

            town.Citizens.Remove(deceased);
            town.Citizens.Add(child);
            Console.WriteLine($"[{town.TownName}] 세대교체: {deceased.Name} 사망 -> 신규 탄생 (유산 {legacy:#,0}gp)");
        }

        // ====================================================================
        // 🧬 7. 공통 유전 헬퍼 (Genetics)
        // ====================================================================
        private static void ApplyGenetics(VirtualCitizen child, VirtualCitizen p1, VirtualCitizen p2)
        {
            if (p1 == null || child.Skills == null) return;
            foreach (SkillName sk in Enum.GetValues(typeof(SkillName)))
            {
                double v1 = p1.Skills.TryGetValue(sk, out double val1) ? val1 : 0.0;
                double v2 = (p2 != null && p2.Skills.TryGetValue(sk, out double val2)) ? val2 : 0.0;
                child.Skills[sk] = ((v1 + v2) / 2.0) * Utility.RandomMinMax(30, 50) / 100.0;
            }
        }
    }
}