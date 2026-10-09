using System;
using Server;
using Server.Commands;
using Server.Gumps;
using Server.Network;

namespace Server.Misc
{
    public class ResourceActivityMonitorGump : Gump
    {
        public static void Initialize()
        {
            CommandSystem.Register("ResMonitor", AccessLevel.Administrator, new CommandEventHandler(OnCommand));
        }

        [Usage("ResMonitor")]
        [Description("NPC 전용 자원 채집량 모니터링 창을 엽니다.")]
        private static void OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendGump(new ResourceActivityMonitorGump());
        }

        public ResourceActivityMonitorGump() : base(100, 100)
        {
            AddBackground(0, 0, 500, 350, 9270);
            AddAlphaRegion(10, 10, 480, 330);

            AddImageTiled(20, 20, 460, 2, 9651);
            AddLabel(150, 25, 53, "◆ NPC 자원 채집량 라이브 트래커 ◆");
            AddImageTiled(20, 50, 460, 2, 9651);

            int y = 80;
            AddLabel(50, y, 53, "자원 종류 (Type)");
            AddLabel(250, y, 68, "NPC 누적 채집량 (Consumed)");
            y += 30;

            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                long npcCount = ResourceTracker.HarvestedAmount.ContainsKey(type) ? ResourceTracker.HarvestedAmount[type] : 0;

                AddImageTiled(20, y - 5, 460, 2, 2624);
                AddLabel(50, y, 1152, type.ToString());
                AddLabel(250, y, npcCount > 0 ? 68 : 1152, $"{npcCount:N0} 개");
                
                y += 25;
            }

            y += 20;
            AddLabel(30, y, 89, "※ 이 수치는 서버 오픈 후 NPC가 시스템에서 차감한 순수 총량입니다.");
            AddLabel(30, y + 20, 89, "※ 어부 파업 문제 해결 여부를 낚시(Fishing) 수치 증가로 확인하세요.");

            AddButton(210, 300, 4011, 4013, 1, GumpButtonType.Reply, 0); // 새로고침
            AddLabel(245, 302, 1152, "새로고침");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1) 
            {
                sender.Mobile.SendGump(new ResourceActivityMonitorGump());
            }
        }
    }
}