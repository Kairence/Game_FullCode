using System;
using Server;
using Server.Mobiles;

namespace Server.Misc
{
    public static class TownDonationManager
    {
        /// <summary>
        /// 플레이어가 마을 자본금 또는 특정 가문에 기여할 때 호출됩니다.
        /// </summary>
        public static void Donate(Mobile from, TownEconomy town, VirtualHouse targetHouse, int amount)
        {
            if (from == null || amount <= 0 || town == null) return;

            // [규칙] 기부 처리
            if (targetHouse != null)
            {
                // 1. 가문 기부: 가문 금고에 골드 추가 및 위신(Prestige) 상승
                targetHouse.TotalWealth += amount;
                
                // 10,000gp당 명성 1 상승 (30,000 스케일에 맞춰 조정)
                int prestigeGain = amount / 10000;
                if (prestigeGain > 0)
                {
                    targetHouse.Prestige += prestigeGain;
                }
            }
            else
            {
                // 2. 마을 기부: 마을 전체 재정(Wealth) 상승
                town.Wealth += (long)amount;

                /* [에러 해결] PriceMultiplier는 읽기 전용이므로 직접 수정할 수 없습니다.
                 * 마을 재정이 늘어나면 TownEconomy 내의 PriceMultiplier 수식에 의해 
                 * 자동으로 마을 물가 지수에 영향을 주게 됩니다. 
                 */
            }

            // 3. 플레이어 보상 및 피드백
            string targetName = targetHouse != null ? targetHouse.HouseName : town.TownName;
            from.SendMessage(68, $"{targetName}에 {amount:#,0}gp를 기부하여 부흥에 기여하셨습니다.");
            
            // Platinum 단위 체크 (1억 gp 이상 기부 시 특별 메시지)
            if (amount >= 100000000)
            {
                from.SendMessage(1150, "대규모 기부를 통해 마을의 수호자로 칭송받습니다! (1 Platinum 기부 달성)");
                // 여기서 추가적인 Merit(공훈) 시스템 연동 가능
            }

            // [연출] 기부 로그 출력
            Console.WriteLine($"[Donation] {from.Name} -> {targetName}: {amount}gp (Current Wealth: {town.TotalWealthString})");
        }
    }
}