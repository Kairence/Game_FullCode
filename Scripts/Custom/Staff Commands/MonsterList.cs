using System;
using Server;
using Server.Commands;
using Server.Misc; // MonsterDropHandler와 Gump가 있는 네임스페이스

namespace Server.Commands
{
    public class MonsterDropHandlerCommand
    {
        public static void Initialize()
        {
            // [mlt 명령어 등록: 게임마스터(GameMaster) 권한 필요
            CommandSystem.Register("mlt", AccessLevel.GameMaster, new CommandEventHandler(MLT_OnCommand));
        }

        [Usage("mlt")]
        [Description("MonsterDropHandler에 등록된 드랍 테이블 몬스터 목록을 검프로 확인합니다.")]
        private static void MLT_OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            if (from != null && !from.Deleted)
            {
                // 첫 번째 페이지(0)부터 검프를 보냅니다.
                from.SendGump(new MonsterDropHandlerGump(from, 0));
                from.SendMessage(0x482, "몬스터 드랍 핸들러 리스트를 불러왔습니다.");
            }
        }
    }
}