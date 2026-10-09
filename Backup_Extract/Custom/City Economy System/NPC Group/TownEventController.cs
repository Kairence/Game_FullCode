using System;
using Server;

namespace Server.Misc
{
    public static class TownEventController
    {
        public static void OnTick(TownEconomy town)
        {
            // 주기적으로 이벤트 발생 조건 체크
            CheckFestivalTrigger(town);
            CheckEconomicCrisis(town);
        }

        private static void CheckFestivalTrigger(TownEconomy town)
        {
            /* [기획 주석]
             * - 가문 명성의 평균치가 매우 높고 만족도가 높을 때 '가문 연합 축제' 발생.
             * - 축제 중에는 플레이어 벤더의 Luxury 품목 구매 확률이 200% 증가.
             */
        }

        private static void CheckEconomicCrisis(TownEconomy town)
        {
            /* [기획 주석]
             * - 특정 가문의 자산이 바닥나면 '파산 구제' 퀘스트를 플레이어에게 자동 발송.
             * - 플레이어가 구제에 성공하면 해당 가문은 플레이어의 '우호 가문'으로 등록되어 영구 혜택 제공.
             */
        }
    }
}
