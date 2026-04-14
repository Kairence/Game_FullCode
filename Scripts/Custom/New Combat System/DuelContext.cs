using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Mobiles;
using Server.Network;
using Server.Gumps;
using Server.Regions;

namespace Server.Misc
{
    // 1. 결투의 상태와 로직을 관리하는 컨텍스트
    public class DuelContext
    {
        public PlayerMobile Challenger { get; set; }
        public PlayerMobile Target { get; set; }
        public bool IsStarted { get; set; }
        public Point3D Center { get; set; }
        public DateTime StartTime { get; set; }
        public Timer RangeTimer { get; set; }
        
        private int m_OutOfRangeCount;

        public DuelContext(PlayerMobile challenger, PlayerMobile target)
        {
            Challenger = challenger;
            Target = target;
            Center = challenger.Location;
            m_OutOfRangeCount = 0;
        }

        public void Start()
        {
            IsStarted = true;
            StartTime = DateTime.Now;
            
            DuelSystem.ActiveDuels.Add(this);

            Challenger.SendMessage(0x35, "결투가 시작되었습니다!");
            Target.SendMessage(0x35, "결투가 시작되었습니다!");

            // 20x20 범위 체크 타이머 (1초 주기)
            RangeTimer = Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0), CheckRange);
        }

        private void CheckRange()
        {
            if (!IsStarted || Challenger.Deleted || Target.Deleted)
            {
                Stop(null);
                return;
            }

            bool challengerOut = !Challenger.InRange(Center, 20);
            bool targetOut = !Target.InRange(Center, 20);

            if (challengerOut || targetOut)
            {
                m_OutOfRangeCount++;
                int remaining = 10 - m_OutOfRangeCount;

                if (remaining > 0)
                {
                    string msg = $"범위를 벗어났습니다! {remaining}초 내로 복귀하지 않으면 패배합니다.";
                    if (challengerOut) Challenger.SendMessage(0x22, msg);
                    if (targetOut) Target.SendMessage(0x22, msg);
                }
                else
                {
                    Stop(challengerOut ? Target : Challenger);
                }
            }
            else
            {
                m_OutOfRangeCount = 0;
            }
        }

        public void Stop(PlayerMobile winner)
        {
            if (RangeTimer != null) RangeTimer.Stop();
            DuelSystem.ActiveDuels.Remove(this);
            IsStarted = false;

            if (winner != null)
            {
                PlayerMobile loser = (winner == Challenger) ? Target : Challenger;
                winner.SendMessage(0x3F, "결투에서 승리했습니다!");
                loser.SendMessage(0x22, "결투에서 패배했습니다.");
                winner.PublicOverheadMessage(MessageType.Regular, 0x3F, false, $"[{winner.Name}]이 결투에서 승리했습니다!");
            }

            Challenger.Combatant = null;
            Challenger.Warmode = false;
            Target.Combatant = null;
            Target.Warmode = false;
        }
    }

    // 2. 수락/거절을 위한 Gump UI
    public class DuelConfirmGump : Gump
    {
        private PlayerMobile m_Challenger;
        private PlayerMobile m_Target;

        public DuelConfirmGump(PlayerMobile challenger, PlayerMobile target) : base(150, 200)
        {
            m_Challenger = challenger;
            m_Target = target;

            AddPage(0);
            AddBackground(0, 0, 300, 150, 9270);
            AddAlphaRegion(10, 10, 280, 130);

            AddHtml(10, 20, 280, 20, $"<Center><BASEFONT COLOR=#FFFFFF>{m_Challenger.Name}님의 결투 신청</Center>", false, false);
            AddHtml(10, 50, 280, 40, "<Center>결투를 수락하시겠습니까?</Center>", false, false);

            AddButton(50, 100, 247, 248, 1, GumpButtonType.Reply, 0); // 수락
            AddButton(170, 100, 241, 242, 0, GumpButtonType.Reply, 0); // 거절
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1) // 수락
            {
                DuelSystem.AcceptDuel(m_Target, m_Challenger);
            }
            else // 거절
            {
                m_Challenger.SendMessage($"{m_Target.Name}님이 결투 신청을 거절했습니다.");
                m_Target.SendMessage("결투 신청을 거절했습니다.");
            }
        }
    }

    // 3. 전역 결투 관리 시스템
    public static class DuelSystem
    {
        public static List<DuelContext> ActiveDuels = new List<DuelContext>();

        public static void SendRequest(PlayerMobile challenger, PlayerMobile target)
        {
            if (challenger == null || target == null || challenger == target) return;

            // [조건 1] 던전 체크
            if (challenger.Region.IsPartOf<DungeonRegion>() || target.Region.IsPartOf<DungeonRegion>())
            {
                challenger.SendMessage("던전 내에서는 결투를 할 수 없습니다.");
                return;
            }

            // [조건 2] 전투 상태 체크 (TimerList[65] 활용)
            if (challenger.TimerList[65] > 0 || target.TimerList[65] > 0)
            {
                challenger.SendMessage("본인 또는 대상이 현재 전투 중이어서 결투를 신청할 수 없습니다.");
                return;
            }

            if (AreDueling(challenger, target)) return;

            challenger.SendMessage($"{target.Name}님에게 결투를 신청했습니다. 응답을 기다립니다.");
            
            // 상대방에게 Gump 전송
            target.CloseGump(typeof(DuelConfirmGump));
            target.SendGump(new DuelConfirmGump(challenger, target));
        }

        public static void AcceptDuel(PlayerMobile target, PlayerMobile challenger)
        {
            if (challenger.Deleted || !challenger.Alive || !challenger.InRange(target, 12))
            {
                target.SendMessage("상대방이 없거나 너무 멉니다.");
                return;
            }

            InitiateDuel(challenger, target);
        }

        public static void InitiateDuel(PlayerMobile challenger, PlayerMobile target)
        {
            if (AreDueling(challenger, target)) return;
            
            DuelContext context = new DuelContext(challenger, target);
            context.Start();
        }

        public static bool AreDueling(Mobile m1, Mobile m2)
        {
            if (!(m1 is PlayerMobile) || !(m2 is PlayerMobile)) return false;

            return ActiveDuels.Any(d => d.IsStarted && 
                ((d.Challenger == m1 && d.Target == m2) || (d.Challenger == m2 && d.Target == m1)));
        }

        public static void OnDuelWin(PlayerMobile winner, PlayerMobile loser)
        {
            var context = ActiveDuels.FirstOrDefault(d => 
                (d.Challenger == winner && d.Target == loser) || (d.Challenger == loser && d.Target == winner));

            if (context != null)
            {
                context.Stop(winner);
            }
        }
    }
}