using System;
using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Accounting;
using Server.Items;
using Server.Misc;

namespace Server.Gumps
{
    public class FamilyGump : Gump
    {
        private PlayerMobile m_pm;
        private Account m_acc;
        private int m_SelectedTheme;

        public FamilyGump(PlayerMobile pm, int theme) : base(50, 50)
        {
            m_pm = pm;
            m_acc = pm.Account as Account;
            m_SelectedTheme = theme;

            pm.CloseGump(typeof(FamilyGump));

            AddPage(0);
            AddBackground(0, 0, 420, 600, 5054);
            AddImageTiled(10, 10, 400, 580, 2624);

            int y = 20;
            AddHtml(20, y, 300, 20, LeftColor(String.Format("가문 포인트: {0:#,0} Pt", m_acc.Point[0]), "FFD700"), false, false);
            y += 25;
            AddImageTiled(20, y, 380, 2, 96);
            y += 10;

            // 랭킹 테마 탭
            string[] themes = { "부호", "자원", "토벌", "경제" };
            for (int i = 0; i < 4; i++)
            {
                int color = (i == m_SelectedTheme) ? 0x00FF7F : 0xA9A9A9;
                AddButton(20 + (i * 95), y, 4005, 4007, 100 + i, GumpButtonType.Reply, 0);
                AddHtml(55 + (i * 95), y + 2, 60, 20, LeftColor(themes[i], color.ToString("X6")), false, false);
            }
            y += 30;

            RenderRankingList(ref y);
            y += 10;
            AddImageTiled(20, y, 380, 2, 96);
            y += 10;

            RenderActionButtons(ref y);
        }

        private void RenderRankingList(ref int y)
        {
            DonationCheck dc = Server.Event.dc;
            if (dc == null) return;

            AddHtml(20, y, 380, 20, CenterColor(String.Format("[{0}] 랭킹 상위 10위", GetThemeName(m_SelectedTheme)), "00FA9A"), false, false);
            y += 25;

            for (int i = 0; i < 10; i++)
            {
                string name = dc.RankingNames[m_SelectedTheme][i];
                int score = dc.RankingScores[m_SelectedTheme][i];
                bool isNpc = dc.IsNpc[m_SelectedTheme][i];

                if (string.IsNullOrEmpty(name)) continue;

                string displayName = "익명의 가문";
                string color = "A9A9A9";

                if (isNpc) { displayName = name; color = "87CEFA"; }
                else if (name == m_acc.Username) { displayName = "나의 가문"; color = "FFD700"; }

                AddHtml(20, y, 30, 20, LeftColor(String.Format("{0}.", i + 1), "FFFFFF"), false, false);
                AddHtml(55, y, 200, 20, LeftColor(displayName, color), false, false);
                AddHtml(260, y, 140, 20, RightColor(String.Format("{0:#,0}", score), "FFFFFF"), false, false);
                y += 20;
            }
        }

        private void RenderActionButtons(ref int y)
        {
            AddHtml(20, y, 200, 20, LeftColor("가문 활동", "00FA9A"), false, false);
            y += 25;

            string[] actions = { "10만 골드 기부", "100만 골드 기부", "즉석 복권 (1,000 Pt)", "캐릭터 슬롯 확장" };
            for (int i = 0; i < actions.Length; i++)
            {
                AddButton(20, y, 4005, 4007, 10 + i, GumpButtonType.Reply, 0);
                AddHtml(55, y + 2, 300, 20, LeftColor(actions[i], "A9A9A9"), false, false);
                y += 22;
            }
        }

        private string GetThemeName(int t) => t switch { 0 => "부호", 1 => "자원", 2 => "수호", _ => "경제" };
        private string LeftColor(string t, string c) => String.Format("<basefont color=#{0}><DIV ALIGN=LEFT>{1}</DIV></basefont>", c, t);
        private string CenterColor(string t, string c) => String.Format("<basefont color=#{0}><DIV ALIGN=CENTER>{1}</DIV></basefont>", c, t);
        private string RightColor(string t, string c) => String.Format("<basefont color=#{0}><DIV ALIGN=RIGHT>{1}</DIV></basefont>", c, t);

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_pm == null || info.ButtonID == 0) return;

            int bid = info.ButtonID;

            if (bid >= 100 && bid <= 103) m_pm.SendGump(new FamilyGump(m_pm, bid - 100));
            else if (bid == 10) FamilySystem.ProcessDonation(m_pm, 1);
            else if (bid == 11) FamilySystem.ProcessDonation(m_pm, 2);
            else if (bid == 12 && m_acc.Point[0] >= 1000) { m_acc.Point[0] -= 1000; FamilySystem.RollScratcher(m_pm); }
            else if (bid == 13) { /* 슬롯 확장 로직 */ }

            m_pm.SendGump(new FamilyGump(m_pm, m_SelectedTheme));
        }
    }
}