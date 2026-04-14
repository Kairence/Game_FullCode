using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Mobiles;

namespace Server.Misc
{
    // [★ 통합] UO의 Notoriety/Karma 지표를 활용한 가문 성향 및 범죄(원한) 관리 레지스트리
    public static class TownSocialRegistry
    {
        // ====================================================================
        // 1. 가문 성향 (Karma) 분석 로직
        // ====================================================================
        
        /// <summary>
        /// 가문원들의 평균 카르마를 계산하여 가문의 도덕적 성향을 반환합니다.
        /// </summary>
        public static int GetHouseAverageKarma(VirtualHouse house)
        {
            if (house.Families == null || house.Families.Count == 0) return 0;
            
            int totalKarma = 0;
            int count = 0;
            foreach (var fam in house.Families.Where(f => f.IsActive))
            {
                if (fam.Father != null && !fam.Father.IsExpired) { totalKarma += fam.Father.Karma; count++; }
                if (fam.Mother != null && !fam.Mother.IsExpired) { totalKarma += fam.Mother.Karma; count++; }
            }
            return count > 0 ? totalKarma / count : 0;
        }

        // 가문 평균 카르마에 따른 접근 권한 설정 (기존 코드 유지 및 보완)
        public static AccessLevel GetHouseAccess(VirtualHouse house)
        {
            int avgKarma = GetHouseAverageKarma(house);
            
            // 가문 평균 Karma가 -5000 이하이면 범죄 가문(Criminal) 취급
            // (차후 NPC 집에 들어갈 때 문(Door)의 접근 권한 등에 사용 가능)
            if (avgKarma <= -5000) return AccessLevel.Player; 
            
            return AccessLevel.Player; 
        }


        // ====================================================================
        // 2. 범죄 및 원한(Grudge) 처리 로직 (구 VirtualCrimeManager)
        // ====================================================================

        /// <summary>
        /// 유저가 가상 시민을 살해했을 때 발생하는 사회적 파장과 멸문지화를 처리합니다.
        /// </summary>
        public static void ProcessMurder(VirtualCitizen victim, PlayerMobile killer)
        {
            if (victim == null || killer == null) return;

            // 변경 전 로직을 지우고 이 코드로 교체합니다.
			TownEconomy town = TownEconomyManager.Towns.Values.FirstOrDefault(t => t.TownName == victim.TargetRegionName);
            
            if (town != null)
            {
                // 치안 악화로 인한 마을 전체의 경제적 타격 (총 자산의 1% 증발)
                town.Wealth = (long)(town.Wealth * 0.99); 
            }

            // 🌟 가문의 살생부에 유저 계정 등록
            if (victim.House != null)
            {
                // 부캐릭으로 와도 알아볼 수 있게 계정명(Account) 기반으로 추적
                string accName = killer.Account != null ? killer.Account.Username : killer.Name;

                if (!victim.House.PlayerGrudges.ContainsKey(accName))
                    victim.House.PlayerGrudges[accName] = 0;

                // 살인은 극악한 범죄이므로 원한 수치 100 적립
                victim.House.PlayerGrudges[accName] += 100; 

                // 피해자가 '선한 가문(Karma > 5000)' 출신일 경우, 명분 있는 분노로 인해 원한이 더 깊어짐
                if (GetHouseAverageKarma(victim.House) > 5000) 
                    victim.House.PlayerGrudges[accName] += 50;

                killer.SendMessage(38, $"{victim.House.HouseName} 가문이 당신의 끔찍한 만행을 영원히 기억할 것입니다.");
            }

            // 🌟 멸문지화(대 끊김) 및 상속/부동산 철거 체크 호출
            if (town != null)
            {
                TownSocietyEngine.PerformInheritance(victim, town);
            }
        }


        // ====================================================================
        // 3. 사회적 상호작용 및 평판(Reputation) 계산
        // ====================================================================

        /// <summary>
        /// 특정 가문이 특정 유저를 어떻게 생각하는지 호감도(0 이하면 적대적)를 반환합니다.
        /// </summary>
        public static int GetPlayerStanding(VirtualHouse house, PlayerMobile player)
        {
            string accName = player.Account != null ? player.Account.Username : player.Name;
            
            // 1. 직접적인 살인/범죄로 쌓인 원한 (가장 치명적)
            int grudge = house.PlayerGrudges.ContainsKey(accName) ? house.PlayerGrudges[accName] : 0;

            // 2. 가치관(Karma) 차이에 따른 배척 (기획 주석 1번 반영)
            int houseKarma = GetHouseAverageKarma(house);
            int playerKarma = player.Karma;
            int karmaPenalty = 0;

            // 선한 가문은 악인(머더러)을 기피하고, 범죄 가문은 선한 자를 비웃음
            if ((houseKarma > 5000 && playerKarma < -2000) || (houseKarma < -5000 && playerKarma > 2000))
            {
                karmaPenalty = 30; // 가치관 충돌 페널티
            }

            // 호감도 계산: 기본 0에서 원한과 카르마 페널티를 뺌 (음수일수록 적대적)
            return -(grudge + karmaPenalty);
        }

        /// <summary>
        /// (매일 밤 호출) 극단적인 카르마 성향을 가진 가문들끼리 자동으로 경쟁(Rivalry) 상태를 만듭니다.
        /// 기획 주석 2번 반영.
        /// </summary>
        public static void UpdateIdeologicalRivalries(TownEconomy town)
        {
            if (town.Houses == null) return;
            
            var activeHouses = town.Houses.Where(h => h.IsActive).ToList();
            foreach (var house in activeHouses)
            {
                int myKarma = GetHouseAverageKarma(house);

                foreach (var other in activeHouses)
                {
                    if (house == other) continue;

                    int otherKarma = GetHouseAverageKarma(other);

                    // 한쪽은 아주 선하고(>5000), 다른 쪽은 아주 악하면(<-5000) 이념적 원한 자동 생성
                    if ((myKarma > 5000 && otherKarma < -5000) || (myKarma < -5000 && otherKarma > 5000))
                    {
                        string rivalName = other.HouseName;
                        if (!house.Grudges.ContainsKey(rivalName)) house.Grudges[rivalName] = 0;
                        
                        // 이념 차이로 인한 원한은 천천히 오르지만(매 틱당 +5), 꾸준히 누적됨
                        house.Grudges[rivalName] = Math.Min(100, house.Grudges[rivalName] + 5);
                    }
                }
            }
        }
    }
}