using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Server.Gumps;
using Server.Network;
using Server.Commands;
using Server.Mobiles;

namespace Server.Misc
{
    public class MonsterDropHandlerGump : Gump
    {
        public static void Initialize()
        {
            CommandSystem.Register("mlt", AccessLevel.GameMaster, new CommandEventHandler(OnCommand));
        }

        [Usage("mlt")]
        [Description("몬스터 업적 표를 확인합니다.")]
        public static void OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendGump(new MonsterDropHandlerGump(e.Mobile, 0));
        }

        private Mobile m_From;
        private int m_Page;
        private List<string> m_List;

        public MonsterDropHandlerGump(Mobile from, int page) : base(50, 50)
        {
            m_From = from;
            m_Page = page;
            m_List = MonsterDropHandler.GetRegisteredList(); 

            Closable = true; Disposable = true; Dragable = true;

            AddPage(0);
            
            // 1. 배경 설정: 랭킹 버튼 공간을 위해 가로를 1150으로 소폭 확장
            AddBackground(0, 0, 1150, 720, 9270);
            
            AddHtml(0, 20, 1150, 30, "<BASEFONT SIZE=6 COLOR=#ffff00><CENTER>몬스터 시즌 토벌 기록</CENTER></BASEFONT>", false, false);
            AddImageTiled(20, 55, 1110, 2, 2621);
            
            int itemsPerPage = 50; 
            int totalCount = m_List.Count;
            int totalPages = (int)Math.Ceiling(totalCount / (double)itemsPerPage);
            int startIndex = page * itemsPerPage;
            
            for (int i = 0; i < itemsPerPage; i++)
            {
                int index = startIndex + i;
                if (index >= totalCount) break;

                int column = i / 25; 
                int row = i % 25;
                
                int x = 40 + (column * 550); // 열 간격 조정
                int y = 75 + (row * 23);

                string className = m_List[index];
                string displayName = SplitCamelCase(className);
                int killCount = GetKillCount(m_From, className);

                // 슬롯 배경
                AddImageTiled(x, y, 520, 21, 9354);
                
                // 번호 및 이름
                AddLabel(x + 5, y + 1, 1152, $"{index + 1}. {displayName}");
                
                // 킬수
                AddLabel(x + 380, y + 1, 88, $"{killCount:N0} 킬");

                // --- 랭킹 확인 버튼 (ID: 100 + 인덱스) ---
                // 몬스터 종류가 많으므로 100번부터 순차적으로 부여
                AddButton(x + 490, y + 2, 0x15E3, 0x15E7, 100 + index, GumpButtonType.Reply, 0);
            }

            // 하단 네비게이션
            AddImageTiled(20, 660, 1110, 2, 2621);
            
            string pageText = $"<BASEFONT SIZE=5 COLOR=#ffff00><CENTER>페이지 {page + 1} / {totalPages}</CENTER></BASEFONT>";
            AddHtml(0, 675, 1150, 25, pageText, false, false);

            if (page > 0)
                AddButton(40, 675, 4014, 4016, 1, GumpButtonType.Reply, 0); 

            if (startIndex + itemsPerPage < totalCount)
                AddButton(1080, 675, 4005, 4007, 2, GumpButtonType.Reply, 0); 
        }

        // TODO: 실제 킬수를 DB나 계정 속성에서 가져오는 로직 구현 필요
        private int GetKillCount(Mobile from, string className) { return 0; }

        private string SplitCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile from = sender.Mobile as PlayerMobile;
            if (from == null) return;

            int bid = info.ButtonID;

            if (bid == 1) // 이전 페이지
            {
                from.SendGump(new MonsterDropHandlerGump(from, m_Page - 1));
            }
            else if (bid == 2) // 다음 페이지
            {
                from.SendGump(new MonsterDropHandlerGump(from, m_Page + 1));
            }
            else if (bid >= 100) // 랭킹 버튼 (100 + index)
            {
                int monsterIdx = bid - 100;
                // SeasonRankingGump 호출 (타입: Monster)
                from.SendGump(new SeasonRankingGump(from, RankingType.Monster, monsterIdx));
            }
            else // 닫기 또는 메인
            {
                from.SendGump(new SeasonMainGump(from));
            }
        }
    }
}