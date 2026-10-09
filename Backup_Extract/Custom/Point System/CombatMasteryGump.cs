using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Misc;

namespace Server.Gumps
{
    public class CombatMasteryGump : Gump
    {
        public enum CombatTab
        {
            Slayer = 0,
            Bestiary = 1,
            Grade = 2
        }

        private PlayerMobile m_Owner;
        private CombatTab m_Tab;
        private int m_Page;
        private const int ItemsPerPage = 11;

        // AllPointGump(CityPointGump) 연동용 기본 생성자
        public CombatMasteryGump(PlayerMobile pm) : this(pm, CombatTab.Slayer, 0)
        {
        }

        public CombatMasteryGump(PlayerMobile pm, CombatTab tab, int page) : base(50, 50)
        {
            m_Owner = pm;
            m_Tab = tab;
            m_Page = page;

            pm.CloseGump(typeof(CombatMasteryGump));

            AddPage(0);
            
            // 🌟 디자인 리마스터: 블랙 투명 배경
            AddBackground(0, 0, 750, 480, 9270); 
            AddAlphaRegion(10, 10, 730, 460);

            // 타이틀 (골드)
            AddHtml(0, 20, 480, 25, CenterColor("<b>전투 숙련도 (Combat Mastery)</b>", "FFD700"), false, false);

            // 상단 탭 버튼
            AddButton(20, 55, m_Tab == CombatTab.Slayer ? 4006 : 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddHtml(55, 55, 100, 20, ColorText("슈퍼 슬레이어", m_Tab == CombatTab.Slayer ? "FFD700" : "FFFFFF"), false, false);

            AddButton(160, 55, m_Tab == CombatTab.Bestiary ? 4006 : 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddHtml(195, 55, 100, 20, ColorText("도감 (개별)", m_Tab == CombatTab.Bestiary ? "FFD700" : "FFFFFF"), false, false);

            AddButton(300, 55, m_Tab == CombatTab.Grade ? 4006 : 4005, 4007, 3, GumpButtonType.Reply, 0);
            AddHtml(335, 55, 100, 20, ColorText("등급 숙련도", m_Tab == CombatTab.Grade ? "FFD700" : "FFFFFF"), false, false);

            AddImageTiled(20, 85, 440, 1, 96); 

            RenderRightPanel();

            List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();

            // 데이터 로드부 (기존 로직 유지)
            if (m_Tab == CombatTab.Slayer)
            {
                string[] slayerNames = { "Repond", "Undead", "Elemental", "Demon", "Arachnid", "Reptilian", "Fey", "Others" };
                for (int i = 0; i < 8; i++)
                {
                    int points = (m_Owner.SlayerData != null && m_Owner.SlayerData.Length > i) ? m_Owner.SlayerData[i] : 0;
                    list.Add(new KeyValuePair<string, int>(slayerNames[i], points));
                }
            }
            else if (m_Tab == CombatTab.Bestiary)
            {
                if (m_Owner.MonsterKills != null)
                {
                    foreach (KeyValuePair<string, int> kvp in m_Owner.MonsterKills)
                        list.Add(kvp);
                    list.Sort(new KvpComparer());
                }
            }
            else if (m_Tab == CombatTab.Grade)
            {
                string[] gradeNames = { "일반 (Normal)", "희귀 (Rare)", "엘리트 (Elite)", "치프 (Chief)", "보스 (Boss)", "네임드 (Named)" };
                for (int i = 0; i < 6; i++)
                {
                    int points = (m_Owner.GradeData != null && m_Owner.GradeData.Length > i) ? m_Owner.GradeData[i] : 0;
                    list.Add(new KeyValuePair<string, int>(gradeNames[i], points));
                }
            }

            RenderPagedList(list);

            // 🌟 닫기 버튼 추가: CityPointGump로 돌아감
            AddButton(350, 435, 4017, 4019, 0, GumpButtonType.Reply, 0);
            AddHtml(385, 435, 100, 20, "<BASEFONT COLOR=#FFFFFF>돌아가기</BASEFONT>", false, false);
        }

        private void RenderRightPanel()
        {
            int panelX = 480;
            // 패널 디자인 리마스터
            AddImageTiled(panelX, 40, 250, 410, 2624);
            AddAlphaRegion(panelX, 40, 250, 410);
            
            AddHtml(panelX, 50, 250, 20, CenterColor("숙련도 보너스 (Bonus)", "FFD700"), false, false);
            AddImageTiled(panelX + 20, 75, 210, 1, 96);

            int infoY = 85;

            // 보너스 설명 텍스트 (색상 가독성 개선)
            if (m_Tab == CombatTab.Slayer)
            {
                AddHtml(panelX + 15, infoY, 220, 20, LeftColor("[슈퍼 슬레이어]", "87CEEB"), false, false);
                infoY += 30;
                AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.1", "모든 피해 0.1% 증가"), false, false);
                infoY += 35;
                AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.10", "최종 피해 2 증가"), false, false);
                infoY += 35;
                AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.25", "받는 피해 5% 감소"), false, false);
                infoY += 45;
                AddImageTiled(panelX + 20, infoY - 10, 210, 1, 96);
                AddHtml(panelX + 15, infoY, 220, 20, LeftColor("[마스터 보너스]", "FFD700"), false, false);
                infoY += 30;
                AddHtml(panelX + 15, infoY, 220, 50, GetBonusShort("Lv.100", "20% 확률로 데미지 주사위<br>최대치 고정 적용"), false, false);
            }
            else if (m_Tab == CombatTab.Bestiary)
            {
                AddHtml(panelX + 15, infoY, 220, 20, LeftColor("[개별 몬스터 도감]", "87CEEB"), false, false);
                infoY += 30;
                AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.1", "모든 피해 0.1% 증가"), false, false);
                infoY += 35;
                AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.10", "최종 피해 1 증가"), false, false);
                infoY += 35;
                AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.25", "치명 추가 피해 25 증가"), false, false);
                infoY += 45;
                AddImageTiled(panelX + 20, infoY - 10, 210, 1, 96);
                AddHtml(panelX + 15, infoY, 220, 20, LeftColor("[마스터 보너스]", "FFD700"), false, false);
                infoY += 30;
                AddHtml(panelX + 15, infoY, 220, 40, GetBonusShort("Lv.100", "5% 확률로 상대 저항 무시"), false, false);
            }
            else if (m_Tab == CombatTab.Grade)
            {
                AddHtml(panelX + 15, infoY, 220, 20, LeftColor("[등급 숙련도 요약]", "87CEEB"), false, false);
                infoY += 30;
                AddHtml(panelX + 15, infoY, 220, 300, "<basefont color=#A9A9A9>1. 희귀: 스텟, 자원, 행운<br><br>2. 엘리트: 속도, 치명, 최종피해<br><br>3. 치프: 최종피해, 스텟, 방어<br><br>4. 보스: 행운, 골드, 드랍수</basefont>", false, false);
            }
        }

        private void RenderPagedList(List<KeyValuePair<string, int>> list)
        {
            if (list.Count == 0)
            {
                AddHtml(20, 150, 440, 20, CenterColor("기록된 데이터가 없습니다.", "FFFFFF"), false, false);
                return;
            }

            int totalPages = (list.Count + ItemsPerPage - 1) / ItemsPerPage;
            if (m_Page >= totalPages) m_Page = totalPages - 1;
            if (m_Page < 0) m_Page = 0;

            int start = m_Page * ItemsPerPage;
            int end = Math.Min(start + ItemsPerPage, list.Count);

            // 헤더 스타일 (민트)
            AddHtml(35, 100, 150, 20, LeftColor("항목 이름", "00FA9A"), false, false);
            AddHtml(195, 100, 60, 20, CenterColor("레벨", "00FA9A"), false, false);
            AddHtml(265, 100, 200, 20, CenterColor("진행도 (현재 / 다음)", "00FA9A"), false, false);

            int y = 130;
            for (int i = start; i < end; i++)
            {
                // 줄무늬 효과
                if (i % 2 == 0) AddAlphaRegion(30, y, 440, 20);

                string mobName = list[i].Key;
                int exp = list[i].Value;
                int level = CombatMastery.GetLevel(exp);
                int nextExp = CombatMastery.GetNextExp(level);

                AddHtml(35, y, 150, 20, ColorText(mobName, level >= 100 ? "FFD700" : "FFFFFF"), false, false);
                AddHtml(195, y, 60, 20, CenterColor(level >= 100 ? "MASTER" : $"Lv.{level}", level >= 100 ? "FFD700" : "00FA9A"), false, false);
                AddHtml(265, y, 200, 20, CenterColor(level >= 100 ? "MAX" : $"{exp:#,0} / {nextExp:#,0}", "FFFFFF"), false, false);

                y += 25;
            }

            // 페이징 버튼
            if (m_Page > 0)
                AddButton(30, 435, 4014, 4016, 4, GumpButtonType.Reply, 0);

            if (end < list.Count)
                AddButton(420, 435, 4005, 4007, 5, GumpButtonType.Reply, 0);
            
            AddHtml(200, 435, 100, 20, CenterColor($"{m_Page + 1} / {totalPages}", "FFFFFF"), false, false);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Owner == null || m_Owner.Deleted) return;

            switch (info.ButtonID)
            {
                case 0: // 🌟 닫기 시 메인 허브로 복귀
                    m_Owner.SendGump(new CityPointGump(m_Owner));
                    break;
                case 1: m_Owner.SendGump(new CombatMasteryGump(m_Owner, CombatTab.Slayer, 0)); break;
                case 2: m_Owner.SendGump(new CombatMasteryGump(m_Owner, CombatTab.Bestiary, 0)); break;
                case 3: m_Owner.SendGump(new CombatMasteryGump(m_Owner, CombatTab.Grade, 0)); break;
                case 4: m_Owner.SendGump(new CombatMasteryGump(m_Owner, m_Tab, m_Page - 1)); break;
                case 5: m_Owner.SendGump(new CombatMasteryGump(m_Owner, m_Tab, m_Page + 1)); break;
            }
        }

        // 텍스트 스타일 헬퍼 (디자인 통일)
        private string GetBonusShort(string level, string desc) => $"<basefont color=#FFD700>{level}:</basefont> <basefont color=#A9A9A9>{desc}</basefont>";
        private string ColorText(string text, string color) => $"<basefont color=#{color}>{text}</basefont>";
        private string CenterColor(string text, string color) => $"<basefont color=#{color}><DIV ALIGN=CENTER>{text}</DIV></basefont>";
        private string LeftColor(string text, string color) => $"<basefont color=#{color}><DIV ALIGN=LEFT>{text}</DIV></basefont>";

        private class KvpComparer : IComparer<KeyValuePair<string, int>>
        {
            public int Compare(KeyValuePair<string, int> x, KeyValuePair<string, int> y) => string.Compare(x.Key, y.Key, StringComparison.Ordinal);
        }
    }
}