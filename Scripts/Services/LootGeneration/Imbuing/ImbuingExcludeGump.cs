using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.SkillHandlers;
using Server.Misc;

namespace Server.Gumps
{
    public class ImbuingExcludeGump : Gump
    {
        private readonly PlayerMobile m_User;
        private readonly int m_SelectedGem;
        private readonly int[] m_CurrentOptions;

        public ImbuingExcludeGump(PlayerMobile user, int selectedGem) : base(50, 50)
        {
            m_User = user;
            m_SelectedGem = selectedGem;
            
            if (selectedGem >= 0 && selectedGem < 9)
                m_CurrentOptions = ItemOptionCreator.GemRefineOptions[selectedGem];

            AddPage(0);
            
            // 유저님의 CityPointGump와 동일한 배경으로 통일감 부여
            AddBackground(0, 0, 540, 460, 5054);
            AddImageTiled(10, 10, 520, 440, 2624);

            // [해결] SIZE 태그 삭제. 유저님 코드처럼 순수 AddHtml + COLOR만 사용합니다.
            AddHtml(10, 20, 520, 20, "<CENTER><BASEFONT COLOR=#FFFFFF>--- 유물 재련 필터 (우선순위 세팅) ---</BASEFONT></CENTER>", false, false);
            AddHtml(10, 45, 520, 20, "<CENTER><BASEFONT COLOR=#FFFF00>최대 4개 선택 (보석 등급에 따라 1순위부터 자동 적용)</BASEFONT></CENTER>", false, false);

            for (int i = 0; i < 9; i++)
            {
                int y = 90 + (i * 35);
                bool isSelected = (m_SelectedGem == i);

                // 유저님 UI 스타일에 맞춘 버튼 (4005, 4007)
                AddButton(25, y + 2, isSelected ? 4006 : 4005, isSelected ? 4007 : 4007, 100 + i, GumpButtonType.Reply, 0);

                int gemCliloc = Imbuing.GetGemCliloc(i);
                int color = isSelected ? 0x03E0 : 0x7FFF; 
                
                AddHtmlLocalized(55, y + 2, 160, 20, gemCliloc, color, false, false);
            }

            if (m_CurrentOptions != null)
            {
                var profile = RefineFilterSystem.GetProfile(user);
                if (!profile.ContainsKey(m_SelectedGem)) profile[m_SelectedGem] = new List<int>();
                
                List<int> banned = profile[m_SelectedGem];

                for (int i = 0; i < m_CurrentOptions.Length; i++)
                {
                    int optID = m_CurrentOptions[i];
                    int banRank = banned.IndexOf(optID);
                    bool isBanned = banRank != -1;
                    int y = 90 + (i * 35);

                    // 체크박스
                    AddButton(260, y + 2, isBanned ? 0xD3 : 0xD2, isBanned ? 0xD2 : 0xD3, 200 + i, GumpButtonType.Reply, 0);

                    int optCliloc = ItemOptionCreator.GetCliloc(optID);
                    int color = isBanned ? 0x7C00 : 0x7FFF;
                    
                    AddHtmlLocalized(290, y + 2, 180, 20, optCliloc, " ", color, false, false);

                    // [해결] SIZE 태그 없이 순수 AddHtml로 한글 전송 (정상 출력됨)
                    if (isBanned)
                    {
                        AddHtml(460, y + 2, 80, 20, $"<BASEFONT COLOR=#FF0000>[ {banRank + 1}순위 ]</BASEFONT>", false, false);
                    }
                }
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            int btn = info.ButtonID;
            if (btn == 0) return;

            if (btn >= 100 && btn < 110)
            {
                m_User.SendGump(new ImbuingExcludeGump(m_User, btn - 100));
            }
            else if (btn >= 200 && btn < 200 + m_CurrentOptions.Length)
            {
                int optID = m_CurrentOptions[btn - 200];
                var profile = RefineFilterSystem.GetProfile(m_User);
                List<int> banned = profile[m_SelectedGem];

                if (banned.Contains(optID))
                {
                    banned.Remove(optID);
                }
                else if (banned.Count < 4) 
                {
                    banned.Add(optID);
                }
                else
                {
                    m_User.SendMessage(0x22, "최대 4개까지만 우선순위를 지정할 수 있습니다.");
                }

                m_User.SendGump(new ImbuingExcludeGump(m_User, m_SelectedGem));
            }
        }
    }
}