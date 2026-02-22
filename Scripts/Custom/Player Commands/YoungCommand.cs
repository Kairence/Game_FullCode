using System;
using Server;
using Server.Mobiles;
using Server.Network;
using Server.Misc;
using Server.Targeting;

namespace Server.Commands
{
    public class YoungInfoCommand
    {
        public static void Initialize()
        {
            // 명령어를 누구나 사용할 수 있도록 Player 권한으로 등록하되, 내부에서 로직을 분리합니다.
            CommandSystem.Register("Young", AccessLevel.Player, new CommandEventHandler(YoungInfo_OnCommand));
        }

        [Usage("Young")]
        [Description("시즌 캐릭터 상태를 관리하거나 포기합니다.")]
        public static void YoungInfo_OnCommand(CommandEventArgs e)
        {
            PlayerMobile from = e.Mobile as PlayerMobile;
            if (from == null) return;

            // 1. 관리자(GameMaster 이상)가 사용한 경우 -> 타겟팅 모드
            if (from.AccessLevel >= AccessLevel.GameMaster)
            {
                from.SendMessage("시즌(Young) 상태를 변경할 캐릭터를 선택하세요.");
                from.Target = new YoungTarget();
            }
            // 2. 일반 플레이어가 사용한 경우 -> 본인 시즌 포기 모드
			else
			{
				if (!from.Young)
				{
					from.SendMessage(0x22, "당신은 시즌 캐릭터 상태가 아닙니다.");
					return;
				}

				from.Young = false;
				from.SendMessage(0x481, "시즌 캐릭터 상태를 포기하셨습니다.");

				if (SeasonController.IsSeasonActive())
				{
					// 뉴헤븐 좌표 대신 pm.PlayerMove를 호출하여 
					// 본인이 설정한 SaveTown 위치의 트라멜 맵으로 이동시킵니다.
					from.PlayerMove(false);
					from.SendMessage(0x22, "시즌 포기로 인해 본인의 고향 마을(트라멜)로 이송되었습니다.");
				}
			}
        }

        // 관리자 전용 타겟 클래스
        private class YoungTarget : Target
        {
            public YoungTarget() : base(12, false, TargetFlags.None) { }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is PlayerMobile pm)
                {
                    pm.Young = !pm.Young; // 상태 반전 (ON/OFF)
                    
                    from.SendMessage(0x481, "{0} 캐릭터의 시즌(Young) 상태를 {1}로 변경했습니다.", 
                        pm.Name, pm.Young ? "ON" : "OFF");
                    
                    pm.SendMessage(0x481, "관리자에 의해 시즌 캐릭터 상태가 {0} 되었습니다.", 
                        pm.Young ? "활성화" : "해제");
                }
                else
                {
                    from.SendMessage("플레이어 캐릭터만 선택 가능합니다.");
                }
            }
        }
    }
}