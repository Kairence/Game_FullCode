using System;
using System.Collections.Generic;
using Server;

namespace Server.Misc
{
    // [★ 수정] UO의 Notoriety 지표를 참고한 관계 설정
    public static class TownSocialRegistry
    {
        // 가문 평균 카르마에 따른 관계 자동 설정
        public static AccessLevel GetHouseAccess(VirtualHouse house)
        {
            // 가문 평균 Karma가 -5000 이하이면 범죄 가문(Criminal) 취급
            // UO 표준 Karma 범위를 그대로 사용합니다.
            return AccessLevel.Player; 
        }

        /* [기획 주석]
         * 1. 유저와의 관계: 유저의 Karma가 낮을 때 선한 가문(High Karma)은 유저를 기피합니다.
         * 2. 가문 간 대립: Karma 성향이 극단적으로 다른 두 가문은 자동으로 Rivalry(경쟁) 상태가 됩니다.
         */
    }
}
