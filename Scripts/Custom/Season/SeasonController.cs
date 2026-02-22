using System;
using Server.Mobiles;
using Server.Accounting;
using Server.Network;

namespace Server.Misc
{
    public class SeasonController
    {
        // 서버 시작 시 자동으로 호출되는 Initialize (타이머 시작)
        public static void Initialize()
        {
            // 1분마다 접속자 전원을 체크하여 26일이 되는 순간 이동시킴
            Timer.DelayCall(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0), CheckAllOnlinePlayers);
        }

        private static void CheckAllOnlinePlayers()
        {
            // 26일 이후인데 아직 Young인 유저가 있는지 확인
            if (IsSeasonActive()) return;

            foreach (NetState state in NetState.Instances)
            {
                PlayerMobile pm = state.Mobile as PlayerMobile;
                if (pm != null && pm.Young)
                {
                    // 26일이 되었으므로 즉시 이동 처리
                    CheckSeasonEnd(pm);
                }
            }
        }

        public static bool IsSeasonActive()
        {
            return DateTime.Now.Day >= 1 && DateTime.Now.Day <= 25;
        }

        // 로그인 시점 체크 (OnLogin에서 간접 호출되거나 직접 호출 가능)
        public static void CheckSeasonEnd(PlayerMobile pm)
        {
            if (!IsSeasonActive() && pm.Young)
            {
                pm.Young = false; // 시즌 상태 해제
                pm.PlayerMove(false); // 트라멜 SaveTown으로 이동 (작성하신 PlayerMove 활용)
                pm.SendMessage(0x22, "시즌이 종료되어 트라멜 마을로 이송되었습니다.");
            }
        }

        public static bool SetSeasonStatus(PlayerMobile pm, IAccount a)
        {
            if (IsSeasonActive() && !HasAccountYoungChar(a, pm))
            {
                pm.Young = true;
                pm.SendMessage(0x481, "시즌 캐릭터로 등록되어 펠루카에서 시작합니다!");
                return true;
            }
            pm.Young = false;
            return false;
        }

        private static bool HasAccountYoungChar(IAccount a, Mobile newChar)
        {
            if (a == null) return false;
            for (int i = 0; i < a.Length; ++i)
            {
                if (a[i] is PlayerMobile pm && pm != newChar && pm.Young)
                    return true;
            }
            return false;
        }

        public static bool CanMoveToOtherMap(Mobile m, Map targetMap)
        {
            if (m is PlayerMobile pm && pm.Young && IsSeasonActive())
            {
                if (targetMap != Map.Felucca)
                {
                    m.SendMessage(0x22, "시즌 캐릭터는 시즌 중 펠루카를 떠날 수 없습니다.");
                    return false;
                }
            }
            return true;
        }
    }
}