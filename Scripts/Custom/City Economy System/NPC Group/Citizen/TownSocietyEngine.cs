using System;
using System.Collections.Generic;
using System.Linq;
using Server;

namespace Server.Misc
{
    public static class TownSocietyEngine
    {
        // ====================================================================
        // 🌆 1. 저녁(18:00) 틱: 사회 지표 및 성인 남녀 짝맺기
        // ====================================================================
        public static void ProcessEveningSocialTick(TownEconomy town)
        {
            if (town == null || town.Citizens == null) return;

            UpdateSocialStatus(town);
            ProcessMatching(town);
        }

        // ====================================================================
        // 🌃 2. 심야(24:00) 틱: 생애 주기 및 세대교체 처리
        // ====================================================================
        public static void ProcessDeepNightLifeCycleTick(TownEconomy town)
        {
            if (town == null || town.Citizens == null) return;

            // 1. 성인 자녀 독립 및 자금 분할
            ProcessIndependence(town);

            // 2. 가족 단위 출산 및 공동 자산 취합
            UpdateFamilies(town);

            // 3. [신규] 가문별 영토 확장, 세금, 매매, 전쟁 및 부속 건물 건설
            ProcessTerritoryAndTaxes(town);

            // 4. 사망자 처리 및 상속 (리스트 순회 중 수정을 피하기 위해 ToList 사용)
            var expiredCitizens = town.Citizens.Where(c => c.IsExpired || c.IsStarving || c.IsDehydrated).ToList();
            foreach (var deceased in expiredCitizens)
            {
                PerformInheritance(deceased, town);
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
                    if (family == null || !family.IsActive) continue;

                    ProcessAgentStatus(family.Father);
                    ProcessAgentStatus(family.Mother);
                    
                    if (family.Children != null)
                    {
                        foreach (var child in family.Children.Where(c => c != null && !c.IsChild))
                            ProcessAgentStatus(child);
                    }
                }
                ApplyGossip(house);
            }
        }

        private static void ProcessAgentStatus(VirtualCitizen agent)
        {
            if (agent == null) return;

            if (agent.Fame > 2000)
            {
                int decay = agent.Fame >= 25000 ? Utility.RandomMinMax(50, 100) : 
                            agent.Fame >= 10000 ? Utility.RandomMinMax(20, 40) : 10;
                agent.Fame -= decay;
            }

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
                if (family == null || !family.IsActive) continue;
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
                //Console.WriteLine($"[Rank Down] {agent.Name}: {agent.RankLevel}(으)로 강등... (Fame: {fame})");
            }
        } 

        private static int GetRequiredFame(NobilityRank rank) => rank switch {
            NobilityRank.Knight => 3000, NobilityRank.SubBaronet => 6500, NobilityRank.Baronet => 10000,
            NobilityRank.SubBaron => 14000, NobilityRank.Baron => 18500, NobilityRank.Viscount => 23000,
            NobilityRank.Count => 27500, NobilityRank.Marquis => 29500, _ => 0
        };

        // ====================================================================
        // 🕊️ 4. 성인 독립 로직 (Independence)
        // ====================================================================
        private static void ProcessIndependence(TownEconomy town)
        {
            if (town.Houses == null) return;
            foreach (var house in town.Houses)
            {
                var newFamilies = new List<FamilyUnit>();
                foreach (var family in house.Families.ToList())
                {
                    if (!family.IsActive) continue;

                    // 20세 이상 성인 자녀 추출
                    var adults = family.Children.Where(c => c.Age >= 20.0).ToList();
                    foreach (var adult in adults)
                    {
                        family.Children.Remove(adult);
                        
                        // 부모 공동 자산의 15%를 독립 자금으로 분할
                        long fund = (long)(family.SharedWealth * 0.15); 
                        family.SharedWealth -= fund;
                        
                        // 30%는 세금으로 마을에 귀속
                        long tax = (long)(fund * 0.3); 
                        town.Wealth += tax;
                        adult.Gold += (int)(fund - tax);

                        // 새 1인 가구 형성
                        var singleFamily = new FamilyUnit(adult, null) { ParentFamily = family };
                        adult.Family = singleFamily;
                        newFamilies.Add(singleFamily);
                        
                        //Console.WriteLine($"[Independence] {town.TownName}: {adult.Name} 성인 독립 (초기 자금: {adult.Gold}gp)");
                    }
                }
                house.Families.AddRange(newFamilies);
            }
        }

        // ====================================================================
        // 💍 5. 혼인 및 가약 로직 (Matching)
        // ====================================================================
        private static void ProcessMatching(TownEconomy town)
        {
            var bachelors = town.Citizens.Where(c => c.Gender == Gender.Male && IsSingle(c) && IsEligibleForMatching(c)).ToList();
            var bachelorettes = town.Citizens.Where(c => c.Gender == Gender.Female && IsSingle(c) && IsEligibleForMatching(c)).ToList();

            foreach (var male in bachelors)
            {
                var bride = bachelorettes.FirstOrDefault(f => male.House != f.House && CheckAgeGap(male, f));
                if (bride != null)
                {
                    FormFamily(male, bride, town);
                    bachelorettes.Remove(bride);
                }
            }
        }

        private static bool IsSingle(VirtualCitizen c)
        {
            return c.Family != null && ((c.Gender == Gender.Male && c.Family.Mother == null) || (c.Gender == Gender.Female && c.Family.Father == null));
        }

        private static bool IsEligibleForMatching(VirtualCitizen c) 
        { 
            double p = c.Age / (c.MaxLifespan.TotalMinutes / VirtualCitizen.GameYearMinutes); 
            return p >= 0.2 && p <= 0.7; 
        }
        
        private static bool CheckAgeGap(VirtualCitizen p1, VirtualCitizen p2) 
        {
            return Math.Abs(p1.Age - p2.Age) <= (p1.MaxLifespan.TotalMinutes / VirtualCitizen.GameYearMinutes * 0.2);
        }

        private static void FormFamily(VirtualCitizen male, VirtualCitizen female, TownEconomy town)
        {
            var maleFam = male.Family;
            var femaleFam = female.Family;

            if (maleFam == null || femaleFam == null) return;

            // 남성의 기존 1인 가구에 여성을 병합하고 여성의 1인 가구는 비활성화
            maleFam.Mother = female;
            maleFam.SharedWealth += femaleFam.SharedWealth; 
            femaleFam.IsActive = false; 
            
            female.Family = maleFam;
            
            TransferToSharedWealth(male, maleFam);
            TransferToSharedWealth(female, maleFam);

            if (male.House != null) 
            { 
                male.House.Prestige += 10; 
                female.House = male.House; 
            }
            
            //Console.WriteLine($"[Match] {town.TownName}: {male.Name} & {female.Name} 가약 성사.");
        }

        // ====================================================================
        // 👶 6. 출산 및 가족 자산 로직 (Family)
        // ====================================================================
        private static void UpdateFamilies(TownEconomy town)
        {
            if (town.Houses == null) return;
            foreach (var house in town.Houses)
            {
                foreach (var family in house.Families.ToList())
                {
                    if (family == null || !family.IsActive) continue;
                    
                    // 출산 체크
                    if (family.Children.Count < 3 && family.Father != null && family.Mother != null)
                    {
                        double ageRatio = Math.Max(
                            family.Father.Age / (family.Father.MaxLifespan.TotalMinutes / VirtualCitizen.GameYearMinutes),
                            family.Mother.Age / (family.Mother.MaxLifespan.TotalMinutes / VirtualCitizen.GameYearMinutes)
                        );

                        // 70% 이상 노화 시 출산 불가
                        if (ageRatio <= 0.7)
                        {
                            double agePenalty = ageRatio > 0.5 ? 0.2 : 1.0; // 50% 넘어가면 확률 급감
                            double wealthBonus = Math.Min(5.0, 1.0 + ((double)family.SharedWealth / 20000.0)); // 자산이 많을수록 보너스
                            
                            double chance = 0.05 * (1 + (house.Prestige * 0.001)) * agePenalty * wealthBonus;

                            if (Utility.RandomDouble() < chance)
                                CreateChild(family, house, town);
                        }
                    }

                    // 자산 취합
                    TransferToSharedWealth(family.Father, family);
                    TransferToSharedWealth(family.Mother, family);
                }
            }
        }

        private static void TransferToSharedWealth(VirtualCitizen c, FamilyUnit f) 
        { 
            if (c != null && c.Gold > 100) 
            { 
                f.SharedWealth += (c.Gold - 100); 
                c.Gold = 100; 
            } 
        }

        private static void CreateChild(FamilyUnit family, VirtualHouse house, TownEconomy town)
        {
            NpcJobClass job = Utility.RandomBool() ? family.Father.JobClass : family.Mother.JobClass;
            var child = new VirtualCitizen(job, NobilityRank.Commoner, 100) { House = house, Family = family };
            
            ApplyGenetics(child, family.Father, family.Mother);
            family.Children.Add(child);
            town.Citizens.Add(child);
            //Console.WriteLine($"[Birth] {town.TownName}: {house.HouseName} 가문 자녀 탄생!");
        }

        // ====================================================================
        // ⚰️ 7. 사망 및 상속 로직 (Inheritance)
        // ====================================================================
        private static void PerformInheritance(VirtualCitizen deceased, TownEconomy town)
        {
            // 1. 선조 기록 보존
            if (deceased.House != null)
            {
                string cause = deceased.IsStarving ? "아사" : deceased.IsDehydrated ? "탈수" : "노환";
                deceased.House.AncestorRecords.Add(new AncestorRecord(deceased.Name, deceased.JobClass, deceased.RankLevel, (int)deceased.Age, cause));
            }

            int totalAsset = deceased.Gold;
            int tax = (int)(totalAsset * 0.3);
            int legacy = totalAsset - tax;
            
            town.Wealth += tax; 

            // 2. 신규 이민자(상속자) 생성
            var child = new VirtualCitizen(deceased.JobClass, NobilityRank.Commoner, 70);
            
            double townMultiplier = town.TownIndex switch { "S" => 2.5, "A" => 1.8, "B" => 1.2, _ => 0.8 };
            child.Gold = (int)(5000 * townMultiplier) + legacy;

            int gameMaxAge = Utility.RandomMinMax(60, 90);
            child.MaxLifespan = TimeSpan.FromMinutes(gameMaxAge * VirtualCitizen.GameYearMinutes);
            child.BirthTime = DateTime.Now; 

            // 3. 신규 1인 가구로 가문 편입
            if (deceased.House != null) 
            {
                child.House = deceased.House;
                var singleFam = new FamilyUnit(child, null);
                child.Family = singleFam;
                deceased.House.Families.Add(singleFam);
            }

            town.Citizens.Remove(deceased);
            town.Citizens.Add(child);
        }

        // ====================================================================
        // 🧬 8. 공통 유전 헬퍼 (Genetics)
        // ====================================================================
        private static void ApplyGenetics(VirtualCitizen child, VirtualCitizen p1, VirtualCitizen p2)
        {
            if (p1 == null || child.Skills == null) return;
            foreach (SkillName sk in Enum.GetValues(typeof(SkillName)))
            {
                double v1 = p1.Skills.ContainsKey(sk) ? p1.Skills[sk] : 0.0;
                double v2 = (p2 != null && p2.Skills.ContainsKey(sk)) ? p2.Skills[sk] : 0.0;
                child.Skills[sk] = ((v1 + v2) / 2.0) * Utility.RandomMinMax(30, 50) / 100.0;
            }
        }

        // ====================================================================
        // 🗺️ 9. NPC 가문 작위별 영토 한도 (신규)
        // ====================================================================
        public static int GetHouseMaxTiles(NobilityRank rank) => rank switch
        {
            NobilityRank.Commoner => 10,
            NobilityRank.Knight => 50,
            NobilityRank.SubBaronet => 100,
            NobilityRank.Baronet => 150,
            NobilityRank.SubBaron => 200,
            NobilityRank.Baron => 300,
            NobilityRank.Viscount => 500,
            NobilityRank.Count => 700,
            NobilityRank.Marquis => 1000,
            _ => 10
        };

		// ====================================================================
        // 🏛️ 10. 가문 영토 확장 및 지분 기반 토지세 징수 (+ 매매 및 건물 건설)
        // ====================================================================
        private static void ProcessTerritoryAndTaxes(TownEconomy town)
        {
            // TerritoryMap이 null인 경우의 방어 코드 추가
            if (town.Houses == null || town.TotalTiles <= 0 || town.TerritoryMap == null) return;

            int tilePrice = town.CurrentTilePrice;
            long totalLandValue = (long)tilePrice * town.TotalTiles;
            int gridWidth = (int)Math.Max(1, Math.Sqrt(town.TotalTiles)); // 2차원 행렬 계산용
            
            // 배열 길이 캐싱
            int mapLength = town.TerritoryMap.Length;

            foreach (var house in town.Houses.ToList())
            {
                if (!house.IsActive) continue;

                // ----------------------------------------------------------------
                // A. 토지세 징수 (Land Tax)
                // ----------------------------------------------------------------
                if (house.OwnedTileIndices.Count > 0)
                {
                    double ownershipShare = (double)house.OwnedTileIndices.Count / town.TotalTiles;
                    long taxAmount = (long)(totalLandValue * ownershipShare * 0.01);
                    house.LandTaxLiability = taxAmount;

                    if (house.TotalWealth >= taxAmount)
                    {
                        house.TotalWealth -= taxAmount;
                        town.Wealth += taxAmount; 
                    }
                    else
                    {
                        house.Prestige = Math.Max(0, house.Prestige - 15);
                        int lostTile = house.OwnedTileIndices[^1]; 
                        
                        house.OwnedTileIndices.RemoveAt(house.OwnedTileIndices.Count - 1);
                        
                        // 인덱스 범위 체크 추가
                        if (lostTile >= 0 && lostTile < mapLength)
                            town.TerritoryMap[lostTile] = null; 
                        
                        Console.WriteLine($"[Tax Default] {town.TownName}: '{house.HouseName}' 가문 세금 체납으로 영토(타일 {lostTile}) 압류.");
                        continue; 
                    }
                }

                // ----------------------------------------------------------------
                // B. 영토 점유, 협상 및 전쟁 (Expansion & Conflict)
                // ----------------------------------------------------------------
                int maxAllowedTiles = GetHouseMaxTiles(house.PrimaryRank) / 100;
                
                if (house.OwnedTileIndices.Count < maxAllowedTiles && house.TotalWealth >= tilePrice * 2)
                {
                    int targetTile = -1;
                    var adjacentTiles = new HashSet<int>();

                    if (house.OwnedTileIndices.Count == 0)
                    {
                        // TotalTiles 대신 Math.Min으로 실제 배열 범위를 벗어나지 않게 수정
                        var emptyTiles = Enumerable.Range(0, Math.Min(town.TotalTiles, mapLength))
                            .Where(i => string.IsNullOrEmpty(town.TerritoryMap[i])).ToList();
                            
                        if (emptyTiles.Count > 0) targetTile = emptyTiles[Random.Shared.Next(emptyTiles.Count)];
                    }
                    else
                    {
                        foreach (int idx in house.OwnedTileIndices)
                        {
                            int x = idx % gridWidth;
                            int y = idx / gridWidth;

                            if (x > 0) adjacentTiles.Add(idx - 1);                  
                            if (x < gridWidth - 1) adjacentTiles.Add(idx + 1);      
                            if (y > 0) adjacentTiles.Add(idx - gridWidth);          
                            if (y < (town.TotalTiles / gridWidth) - 1) adjacentTiles.Add(idx + gridWidth); 
                        }

                        // mapLength 범위 제한 추가
                        var validExpansions = adjacentTiles
                            .Where(idx => idx >= 0 && idx < mapLength && string.IsNullOrEmpty(town.TerritoryMap[idx])).ToList();
                            
                        if (validExpansions.Count > 0) targetTile = validExpansions[Random.Shared.Next(validExpansions.Count)];
                    }

                    // 1. 빈 땅이 있으면 정상 매입
                    if (targetTile != -1)
                    {
                        house.TotalWealth -= tilePrice;
                        town.Wealth += tilePrice;
                        house.OwnedTileIndices.Add(targetTile);
                        
                        // 인덱스 범위 체크 추가
                        if (targetTile >= 0 && targetTile < mapLength)
                            town.TerritoryMap[targetTile] = house.HouseName; 
                        
                        house.Prestige += 1; 
                    }
                    // 2. 빈 땅이 없고 남의 땅으로 막혀 있다면? -> 협상 또는 전쟁 발동!
                    else if (adjacentTiles.Count > 0 && house.OwnedTileIndices.Count > 0)
                    {
                        ProcessTradeOrWar(town, house, adjacentTiles, tilePrice);
                    }
                }

                // ----------------------------------------------------------------
                // C. 부속 건물 자동 건설 (Estate Sub-systems)
                // ----------------------------------------------------------------
                int tileCount = house.OwnedTileIndices.Count;

                // 1. 가문 텃밭 (10칸 이상, 50,000gp)
                if (!house.HasGarden && tileCount >= 10 && house.TotalWealth >= 50000)
                {
                    house.TotalWealth -= 50000;
                    town.Wealth += 50000;
                    house.HasGarden = true;
                    Console.WriteLine($"[Build] {town.TownName}: '{house.HouseName}' 가문이 영토에 [텃밭]을 조성했습니다.");
                }

                // 2. 사유지 공방 (50칸 이상, 500,000gp)
                if (!house.HasWorkshop && tileCount >= 50 && house.TotalWealth >= 500000)
                {
                    house.TotalWealth -= 500000;
                    town.Wealth += 500000;
                    house.HasWorkshop = true;
                    Console.WriteLine($"[Build] {town.TownName}: '{house.HouseName}' 가문이 영토에 [공방]을 건설했습니다.");
                }

                // 3. 병영 (200칸 이상, 2,000,000gp)
                if (!house.HasBarracks && tileCount >= 200 && house.TotalWealth >= 2000000)
                {
                    house.TotalWealth -= 2000000;
                    town.Wealth += 2000000;
                    house.HasBarracks = true;
                    Console.WriteLine($"[Build] {town.TownName}: '{house.HouseName}' 가문이 영토에 [병영]을 주둔시켰습니다.");
                }

                // ----------------------------------------------------------------
                // D. 부속 건물 효과 발동 (심야 틱 처리)
                // ----------------------------------------------------------------
                if (house.HasGarden)
                {
                    // 텃밭 효과: 소속된 모든 가문원의 허기와 스트레스를 소폭 자동 회복 (자급자족)
                    foreach (var family in house.Families.Where(f => f.IsActive))
                    {
                        if (family.Father != null) { family.Father.Hunger += 5000; family.Father.Stress = Math.Max(0, family.Father.Stress - 2); }
                        if (family.Mother != null) { family.Mother.Hunger += 5000; family.Mother.Stress = Math.Max(0, family.Mother.Stress - 2); }
                        foreach (var child in family.Children) { child.Hunger += 5000; child.Stress = Math.Max(0, child.Stress - 2); }
                    }
                }
            }
        }
    

        // ====================================================================
        // 🤝⚔️ 11. 매매 협상 및 가문 전쟁 처리기 (+ 병영 효과)
        // ====================================================================
        private static void ProcessTradeOrWar(TownEconomy town, VirtualHouse attacker, HashSet<int> adjacentTiles, int baseTilePrice)
        {
            var foreignTiles = adjacentTiles.Where(idx => idx >= 0 && idx < town.TotalTiles && town.TerritoryMap[idx] != attacker.HouseName).ToList();
            if (foreignTiles.Count == 0) return;

            int targetTile = foreignTiles[Random.Shared.Next(foreignTiles.Count)];
            string defenderName = town.TerritoryMap[targetTile];
            VirtualHouse defender = town.Houses.FirstOrDefault(h => h.HouseName == defenderName && h.IsActive);

            if (defender == null) return;

            // 1. 평화적 매매 협상 (시세의 150% 프리미엄)
            int premiumPrice = (int)(baseTilePrice * 1.5);
            if (attacker.TotalWealth >= premiumPrice)
            {
                bool acceptTrade = defender.Families.Any(f => f.IsWillingToSell) || defender.TotalWealth < defender.LandTaxLiability * 2;

                if (acceptTrade)
                {
                    attacker.TotalWealth -= premiumPrice;
                    defender.TotalWealth += premiumPrice;
                    
                    defender.OwnedTileIndices.Remove(targetTile);
                    attacker.OwnedTileIndices.Add(targetTile);
                    town.TerritoryMap[targetTile] = attacker.HouseName;

                    attacker.Prestige += 2;
                    Console.WriteLine($"[Trade] {town.TownName}: '{attacker.HouseName}' 가문이 웃돈을 주고 '{defender.HouseName}' 가문의 영토(타일 {targetTile})를 {premiumPrice}gp에 매수했습니다.");
                    return; 
                }
            }

            // 2. 가문 전쟁 발동 (자본 30% 이상 장악 & 명성이 더 높을 때)
            if (attacker.TotalWealth > town.Wealth * 0.3 && attacker.Prestige > defender.Prestige && attacker.PrimaryRank >= NobilityRank.Baron)
            {
                long warCost = attacker.TotalWealth / 10;
                attacker.TotalWealth -= warCost;
                town.Wealth += warCost; 

                Console.WriteLine($"[War Declared] {town.TownName}: '{attacker.HouseName}' 가문이 '{defender.HouseName}' 가문을 침공합니다!");

                // [수정됨] 병영(Barracks) 보유 시 전투력 1.5배 증가!
                double attackerPower = (attacker.TotalWealth * 0.01) + attacker.Prestige;
                if (attacker.HasBarracks) attackerPower *= 1.5; 

                double defenderPower = (defender.TotalWealth * 0.01) + defender.Prestige;
                if (defender.HasBarracks) defenderPower *= 1.5;
                
                bool attackerWins = (Random.Shared.NextDouble() * (attackerPower + defenderPower)) < attackerPower;

                if (attackerWins)
                {
                    ExecuteWarAftermath(town, attacker, defender);
                }
                else
                {
                    attacker.Prestige = Math.Max(0, attacker.Prestige - 50);
                    Console.WriteLine($"[War Failed] {town.TownName}: '{attacker.HouseName}' 가문의 공격이 격퇴되었습니다.");
                }
            }
        }

        // ====================================================================
        // 💥 12. 전쟁 패배 가문 파멸 처리
        // ====================================================================
        private static void ExecuteWarAftermath(TownEconomy town, VirtualHouse winner, VirtualHouse loser)
        {
            Console.WriteLine($"[War Result] {town.TownName}: '{winner.HouseName}' 가문 승리! '{loser.HouseName}' 가문은 파멸했습니다.");

            loser.IsActive = false;
            var loserTiles = loser.OwnedTileIndices.ToList();
            loser.OwnedTileIndices.Clear();

            int absorbCount = loserTiles.Count / 2;
            var shuffledTiles = loserTiles.OrderBy(x => Random.Shared.Next()).ToList();
            
            for (int i = 0; i < shuffledTiles.Count; i++)
            {
                int tile = shuffledTiles[i];
                if (i < absorbCount)
                {
                    winner.OwnedTileIndices.Add(tile);
                    town.TerritoryMap[tile] = winner.HouseName;
                }
                else
                {
                    town.TerritoryMap[tile] = null;
                }
            }

            winner.Prestige += 100;
            winner.TotalWealth += loser.TotalWealth / 2; 
            loser.TotalWealth = 0;
            
            foreach (var family in loser.Families)
            {
                if (family.Father != null) KillCitizenInWar(town, family.Father);
                if (family.Mother != null) KillCitizenInWar(town, family.Mother);

                foreach (var child in family.Children.ToList())
                {
                    child.Gold = 0;
                    child.JobClass = NpcJobClass.Pauper;
                    child.RankLevel = NobilityRank.Commoner;
                    child.House = null;
                    
                    var newPauperFamily = new FamilyUnit(child, null);
                    child.Family = newPauperFamily;
                }
            }
        }

        private static void KillCitizenInWar(TownEconomy town, VirtualCitizen c)
        {
            if (c.House != null)
                c.House.AncestorRecords.Add(new AncestorRecord(c.Name, c.JobClass, c.RankLevel, (int)c.Age, "가문 전쟁으로 전사"));
            town.Citizens.Remove(c);
        }

        // ====================================================================
        // 🏘️ 13. 시민 리스폰 및 직업 자동화 (Populate)
        // ====================================================================
        public static NpcJobClass GetJobForItem(Type targetItem)
        {
            var allJobs = Enum.GetValues<NpcJobClass>();
            
            var validJobs = allJobs.Where(job =>
            {
                var profile = VirtualJobCore.GetDeepJobProfile(job);
                return profile.Produces != null && profile.Produces.Contains(targetItem);
            }).ToList();

            return validJobs.Count > 0 
                ? validJobs[Random.Shared.Next(validJobs.Count)] 
                : NpcJobClass.Laborer; 
        }

        public static void PopulateTownCitizens(TownEconomy town)
        {
            if (town == null) return;
            town.Citizens ??= []; 

            int basePop = town.VendorCount * 10; 
            
            bool isOutpost = (town.TownID % 100) >= 50 || town.TownIndex == "C";

            int targetPop = isOutpost ? basePop / 2 : basePop;

            if (town.Citizens.Count >= targetPop) return;

            var warehouseItems = town.Warehouse.Keys.ToList();
            var allJobs = Enum.GetValues<NpcJobClass>();

            for (int i = town.Citizens.Count; i < targetPop; i++)
            {
                NpcJobClass selectedJob;

                bool spawnByItem = isOutpost || Random.Shared.NextDouble() < 0.5;

                if (spawnByItem && warehouseItems.Count > 0)
                {
                    Type randomItem = warehouseItems[Random.Shared.Next(warehouseItems.Count)];
                    selectedJob = GetJobForItem(randomItem);
                }
                else
                {
                    selectedJob = allJobs[Random.Shared.Next(allJobs.Length)];
                }

                var newCitizen = new VirtualCitizen(selectedJob, NobilityRank.Commoner, 70);
                town.Citizens.Add(newCitizen);
            }
        }
    }
}