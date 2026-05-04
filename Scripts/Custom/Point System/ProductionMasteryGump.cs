using System;
using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Misc;
using Server.Engines.Craft;
using System.Collections.Generic;

namespace Server.Gumps
{
    public class CraftMasteryGump : Gump
    {
        private PlayerMobile m_pm;
        private CraftType m_Category;
        private int m_Page;
        private const int ItemsPerPage = 12;

        // UI 출력을 위한 내부 구조체
        private struct DisplayEntry
        {
            public CraftType Type;
            public int NameNumber;
            public string NameString;
            public bool IsTotal;
        }

        public CraftMasteryGump(PlayerMobile pm, int page) 
            : this(pm, CraftType.TotalBlacksmithy, page) 
        {
        }

        public CraftMasteryGump(PlayerMobile pm, CraftType category, int page) : base(50, 50)
        {
            m_pm = pm;
            m_Page = page;
            
            if (category == CraftType.None && CraftMastery.Categories.Length > 0)
                m_Category = CraftMastery.Categories[0];
            else
                m_Category = category;

            pm.CloseGump(typeof(CraftMasteryGump));

            AddPage(0);

            AddBackground(0, 0, 850, 430, 5054);
            AddImageTiled(10, 10, 830, 410, 2624);
            AddHtml(0, 20, 850, 20, CenterColor("제작 숙련도 (Production Mastery)", "A9A9A9"), false, false);

            // 1. 좌측 탭 구성
            int tabY = 45;
            for (int i = 0; i < CraftMastery.Categories.Length; i++)
            {
                CraftType t = CraftMastery.Categories[i];
                bool isSelected = (t == m_Category);
                int btnID = 100 + i;

                if (isSelected)
                {
                    AddImageTiled(15, tabY, 150, 25, 2624);
                    AddHtml(20, tabY + 2, 140, 20, ColorText(CraftMastery.GetCategoryName(t), "FFD700"), false, false);
                }
                else
                {
                    AddButton(15, tabY + 2, 4005, 4007, btnID, GumpButtonType.Reply, 0);
                    AddHtml(50, tabY + 2, 110, 20, ColorText(CraftMastery.GetCategoryName(t), "FFFFFF"), false, false);
                }
                tabY += 28;
            }

            AddImageTiled(170, 40, 2, 360, 96);

            AddHtml(180, 55, 150, 20, LeftColor("제작 항목", "00FA9A"), false, false);
            AddHtml(360, 55, 60, 20, LeftColor("레벨", "00FA9A"), false, false);
            AddHtml(420, 55, 150, 20, LeftColor("진행도 (현재 / 다음)", "00FA9A"), false, false);

            // 2. CraftSystem 연동을 통한 동적 데이터 구성
            List<DisplayEntry> displayList = new List<DisplayEntry>();
            displayList.Add(new DisplayEntry { Type = m_Category, IsTotal = true });

            CraftSystem sys = CraftMastery.GetCraftSystem(m_Category);
            if (sys != null)
            {
                foreach (CraftItem cItem in sys.CraftItems)
                {
                    CraftType cType = CraftMastery.ParseCraftType(cItem.ItemType, m_Category);
                    if (cType != CraftType.None && !displayList.Exists(x => x.Type == cType))
                    {
                        displayList.Add(new DisplayEntry {
                            Type = cType,
                            NameNumber = cItem.NameNumber,
                            NameString = cItem.NameString,
                            IsTotal = false
                        });
                    }
                }
            }

            int maxPage = (displayList.Count - 1) / ItemsPerPage;
            if (m_Page > maxPage) m_Page = maxPage;
            if (m_Page < 0) m_Page = 0;

            // 투명 화살표 4005 교체 -> 선명한 표준 화살표 0x15E1(Right), 0x15E3(Left) 적용
            if (m_Page > 0)
                AddButton(530, 20, 0x15E3, 0x15E7, 1, GumpButtonType.Reply, 0); 
            
            if (m_Page < maxPage)
                AddButton(560, 20, 0x15E1, 0x15E5, 2, GumpButtonType.Reply, 0); 

            int startIndex = m_Page * ItemsPerPage;
            int endIndex = Math.Min(startIndex + ItemsPerPage, displayList.Count);
            int listY = 85;

            // 3. 리스트 출력
            for (int i = startIndex; i < endIndex; i++)
            {
                DisplayEntry entry = displayList[i];
                int exp = pm.CraftPoint[(int)entry.Type];
                int level = pm.CraftPoint[(int)entry.Type + CraftMastery.LevelOffset];
                int nextExp = CraftMastery.GetNextExp(level);

                string levelColor = entry.IsTotal ? "FFD700" : "00FA9A";
                string levelStr = level >= CraftMastery.MaxLevel ? "MASTER" : $"Lv.{level}";
                string expStr = level >= CraftMastery.MaxLevel ? "최대 수치 도달" : $"{exp:#,0} / {nextExp:#,0}";

                // 사용자 지침에 따른 Cliloc 공식 한글명칭 연동
                string nameText = "";
                if (entry.IsTotal)
                {
                    nameText = "제작 총합";
                }
                else
                {
                    if (entry.NameNumber > 0)
                        nameText = ClilocData.GetString(entry.NameNumber);
                    else if (!string.IsNullOrEmpty(entry.NameString))
                        nameText = entry.NameString;
                    else
                        nameText = entry.Type.ToString();
                }

                string nameColor = entry.IsTotal ? "FFD700" : "FFFFFF";

                AddHtml(180, listY, 170, 20, ColorText(nameText, nameColor), false, false);
                AddHtml(360, listY, 55, 20, ColorText(levelStr, levelColor), false, false);
                AddHtml(420, listY, 150, 20, ColorText(expStr, "FFFFFF"), false, false);

                listY += 25;
            }

            // 4. 보너스 패널
            int panelX = 580;
            AddImageTiled(panelX, 40, 250, 360, 2624);
            AddAlphaRegion(panelX, 40, 250, 360);
            
            AddHtml(panelX, 45, 250, 20, CenterColor("제작 보너스 (Production Bonus)", "FFD700"), false, false);
            AddImageTiled(panelX + 20, 65, 210, 1, 96);

            int infoY = 70;
            AddHtml(panelX + 15, infoY, 220, 20, LeftColor("[개별 제작 숙련도]", "87CEEB"), false, false);
            infoY += 22;
            AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.1", "제작 성공 확률 증가 보너스"), false, false);
            infoY += 32;
            AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.10", "익셉셔널(Exceptional) 제작 확률 증가"), false, false);
            infoY += 32;
            AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.25", "제작 소요 시간(Delay) 단축 (최대 4회 중첩)"), false, false);
            infoY += 32;
            AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.100", "5% 확률로 재료 소모 없이 무료 제작 발동"), false, false);

            infoY += 38;
            AddImageTiled(panelX + 20, infoY - 5, 210, 1, 96);
            AddHtml(panelX + 15, infoY, 220, 20, LeftColor("[직업 총합 숙련도]", "FFD700"), false, false);
            infoY += 22;
            AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.1", "제작 도구 내구도 소모 확률 감소"), false, false);
            infoY += 32;
            AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.10", "해당 직업의 모든 제작/익셉 성공률 5% 보정"), false, false);
            infoY += 32;
            AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.25", "작업 피로도(기력 등) 감소 (최대 4회 중첩)"), false, false);
            infoY += 32;
            AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.100", "5% 확률로 재료 추가 소모 없이 2개 생성"), false, false);

            AddButton(425, 390, 4017, 4019, 0, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
        {
            if (!m_pm.CheckAlive()) return;

            int bid = info.ButtonID;

            if (bid == 1)
                m_pm.SendGump(new CraftMasteryGump(m_pm, m_Category, m_Page - 1));
            else if (bid == 2)
                m_pm.SendGump(new CraftMasteryGump(m_pm, m_Category, m_Page + 1));
            else if (bid >= 100 && bid <= 111)
            {
                int index = bid - 100;
                if (index >= 0 && index < CraftMastery.Categories.Length)
                    m_pm.SendGump(new CraftMasteryGump(m_pm, CraftMastery.Categories[index], 0));
            }
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