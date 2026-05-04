using System;
using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Misc;
using System.Collections.Generic;

namespace Server.Gumps
{
    public class HarvestMasteryGump : Gump
    {
        private PlayerMobile m_pm;
        private int m_Page;
        private const int ItemsPerPage = 12;

        public HarvestMasteryGump(PlayerMobile pm, int page) : base(50, 50)
        {
            m_pm = pm;
            m_Page = page;
            pm.CloseGump(typeof(HarvestMasteryGump));

            AddPage(0);
            
            // 전체 배경
            AddBackground(0, 0, 750, 430, 5054);
            AddImageTiled(10, 10, 730, 410, 2624);

            AddHtml(0, 20, 480, 20, CenterColor("채집 숙련도 (Harvest Mastery)", "A9A9A9"), false, false);

            AddHtml(30, 55, 150, 20, LeftColor("채집 항목", "00FA9A"), false, false);
            AddHtml(200, 55, 60, 20, LeftColor("레벨", "00FA9A"), false, false);
            AddHtml(270, 55, 200, 20, LeftColor("진행도 (현재 / 다음)", "00FA9A"), false, false);

            // =========================================================
            // 1. 우측 채집 보너스 안내 패널 (개별/총합 분리)
            // =========================================================
            int panelX = 480;
            AddImageTiled(panelX, 40, 250, 360, 2624);
            AddAlphaRegion(panelX, 40, 250, 360);
            
            AddHtml(panelX, 45, 250, 20, CenterColor("채집 보너스 (Harvest Bonus)", "FFD700"), false, false);
            AddImageTiled(panelX + 20, 65, 210, 1, 96);

            // --- 개별 채집 보너스 (자원 전용) ---
            int infoY = 70;
            AddHtml(panelX + 15, infoY, 220, 20, LeftColor("[개별 자원 숙련도]", "87CEEB"), false, false);
            infoY += 22;
            AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.1", "2배 획득 확률 증가 (0.2%/Lv)"), false, false);
            infoY += 32;
            AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.10", "도구 내구도 소모 방지 (0.3%/Lv)"), false, false);
            infoY += 32;
            AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.25", "해당 자원 채집 동작 횟수 1회 감소"), false, false);
            infoY += 32;
            AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.100", "자원 수확량이 항상 최대치로 고정"), false, false);

            // --- 총합 채집 보너스 (직업 공통) ---
            infoY += 38;
            AddImageTiled(panelX + 20, infoY - 5, 210, 1, 96);
            AddHtml(panelX + 15, infoY, 220, 20, LeftColor("[카테고리 총합 숙련도]", "FFD700"), false, false);
            infoY += 22;
            AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.1", "상위 등급 자원 출현 확률 5% 증가"), false, false);
            infoY += 32;
            AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.10", "희귀 부산물 발견 확률 5% 증가"), false, false);
            infoY += 32;
            AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.25", "기력 소모 감소 (25레벨당 -1)"), false, false);
            infoY += 32;
            AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.100", "5% 확률로 즉시 채집 완료"), false, false);

            // =========================================================
            // 2. 좌측 숙련도 리스트 출력
            // =========================================================
            Array enumValues = Enum.GetValues(typeof(HarvestType));
            List<HarvestType> displayList = new List<HarvestType>();
            foreach (HarvestType type in enumValues)
            {
                if (type != HarvestType.None)
                    displayList.Add(type);
            }

            int maxPage = (displayList.Count - 1) / ItemsPerPage;
            if (m_Page > maxPage) m_Page = maxPage;
            if (m_Page < 0) m_Page = 0;

            if (m_Page > 0)
                AddButton(410, 20, 4014, 4016, 1, GumpButtonType.Reply, 0); 
            
            if (m_Page < maxPage)
                AddButton(440, 20, 4005, 4007, 2, GumpButtonType.Reply, 0); 

            int startIndex = m_Page * ItemsPerPage;
            int endIndex = Math.Min(startIndex + ItemsPerPage, displayList.Count);
            int listY = 85;

            for (int i = startIndex; i < endIndex; i++)
            {
                HarvestType type = displayList[i];
                int exp = pm.HarvestPoint[(int)type];
                int level = pm.HarvestPoint[(int)type + HarvestMastery.LevelOffset];
                int nextExp = HarvestMastery.GetNextExp(level);

                bool isTotal = (type == HarvestMastery.GetCategoryTotal(type));
                string nameColor = isTotal ? "FFD700" : "FFFFFF"; 
                string levelColor = isTotal ? "FFD700" : "00FA9A";

                string name = HarvestMastery.GetHarvestName(type);
                string levelStr = level >= HarvestMastery.MaxLevel ? "MASTER" : $"Lv.{level}";
                string expStr = level >= HarvestMastery.MaxLevel ? "최대 수치 도달" : $"{exp:#,0} / {nextExp:#,0}";

                AddHtml(30, listY, 160, 20, ColorText(name, nameColor), false, false);
                AddHtml(200, listY, 65, 20, ColorText(levelStr, levelColor), false, false);
                AddHtml(270, listY, 200, 20, ColorText(expStr, "FFFFFF"), false, false);

                listY += 25;
            }

            AddButton(335, 390, 4017, 4019, 0, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
        {
            if (!m_pm.CheckAlive()) return;

            if (info.ButtonID == 1)
                m_pm.SendGump(new HarvestMasteryGump(m_pm, m_Page - 1));
            else if (info.ButtonID == 2)
                m_pm.SendGump(new HarvestMasteryGump(m_pm, m_Page + 1));
            else if (info.ButtonID == 0)
                m_pm.SendGump(new CityPointGump(m_pm));
        }

        private string GetBonusShort(string level, string desc)
        {
            return $"<basefont color=#B0C4DE>{level}:</basefont> <basefont color=#A9A9A9>{desc}</basefont>";
        }

        private string ColorText(string text, string color)
        {
            return $"<basefont color=#{color}>{text}</basefont>";
        }

        private string CenterColor(string text, string color)
        {
            return $"<basefont color=#{color}><DIV ALIGN=CENTER>{text}</DIV></basefont>";
        }

        private string LeftColor(string text, string color)
        {
            return $"<basefont color=#{color}><DIV ALIGN=LEFT>{text}</DIV></basefont>";
        }
    }
}