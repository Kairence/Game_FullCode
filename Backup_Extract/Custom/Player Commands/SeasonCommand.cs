using System;
using Server;
using Server.Commands;
using Server.Gumps;

namespace Server.Misc
{
    public class SeasonCommand
    {
        public static void Initialize()
        {
            // [시즌 및 [season 명령어 등록 (일반 유저 권한)
            //CommandSystem.Register("시즌", AccessLevel.Player, new CommandEventHandler(OnSeasonCommand));
            CommandSystem.Register("season", AccessLevel.Player, new CommandEventHandler(OnSeasonCommand));
        }

        [Usage("시즌")]
        [Description("시즌 업적 메뉴를 엽니다.")]
        private static void OnSeasonCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            if (from != null && !from.Deleted)
            {
                // 메인 허브 검프를 호출합니다.
                from.SendGump(new SeasonMainGump(from));
                //from.PlaySound(0x1F2); // 메뉴 열 때 효과음 (선택 사항)
            }
        }
    }
}
