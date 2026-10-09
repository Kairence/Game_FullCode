using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Items;
using Server.Accounting;
using Server.Misc;
namespace Server.Gumps
{
    public class SkillPointGump : Gump
    {
        private PlayerMobile m_pm;
        private FirstSkillCheck fsc = null;

        private string CenterColor(string text, string color) { return $"<CENTER><BASEFONT COLOR=#{color}>{text}</BASEFONT></CENTER>"; }
        private string LeftColor(string text, string color) { return $"<BASEFONT COLOR=#{color}>{text}</BASEFONT>"; }
        private string RightColor(string text, string color) { return $"<DIV ALIGN=RIGHT><BASEFONT COLOR=#{color}>{text}</BASEFONT></DIV>"; }

        public SkillPointGump(PlayerMobile pm) : base(50, 50)
        {
            m_pm = pm;
            pm.CloseGump(typeof(SkillPointGump));

            foreach (Item item in World.Items.Values)
            {
                if (item is FirstSkillCheck) { fsc = item as FirstSkillCheck; break; }
            }

            AddPage(0);
            
            // 🌟 리마스터 디자인: 투명 블랙 레이아웃
            AddBackground(0, 0, 780, 650, 9270); 
            AddAlphaRegion(10, 10, 760, 630);

            // 헤더 정보 (골드 톤)
            AddHtml(0, 30, 780, 25, CenterColor("<b>캐릭터 스킬 마스터리 시스템</b>", "FFD700"), false, false);
            
            string totalStr = String.Format("{0:F1} / {1:F1}", (double)m_pm.SkillsTotal * 0.1, (double)m_pm.SkillsCap * 0.1);
            AddHtml(530, 30, 200, 20, RightColor($"스킬 총합: <BASEFONT COLOR=#00FA9A>{totalStr}</BASEFONT>", "FFFFFF"), false, false);

            AddImageTiled(30, 60, 720, 1, 96); 

            int y = 75;
            // 칼럼 헤더 (민트)
            AddHtml(45, y, 120, 20, LeftColor("스킬 명칭", "00FA9A"), false, false);
            AddHtml(165, y, 160, 20, CenterColor("현재 수치 / MAX", "00FA9A"), false, false);
            AddHtml(335, y, 200, 20, CenterColor("경험치 진행도", "00FA9A"), false, false);
            AddHtml(545, y, 180, 20, RightColor("가문 최고 기록", "00FA9A"), false, false);

            y += 30;

            int page = m_pm.SkillGumpPage;
            Account acc = pm.Account as Account;
            int startIndex = page * 22; // 가독성을 위해 한 페이지당 22개로 조정
            int totalSkills = m_pm.Skills.Length;
            int endIndex = Math.Min(startIndex + 22, totalSkills); 

            for (int i = startIndex; i < endIndex; i++)
            {
                double accountBest = 0.0;
                if (acc != null)
                {
                    for (int j = 0; j < acc.Length; ++j)
                    {
                        Mobile m = acc[j];
                        if (m != null && accountBest < m.Skills[i].Base) accountBest = m.Skills[i].Base;
                    }
                }

                // 줄무늬 배경 효과
                if (i % 2 == 0) AddAlphaRegion(30, y, 720, 20);

                // 1. 스킬 이름
                string name = (fsc != null && fsc.SkillName.Length > i) ? fsc.SkillName[i] : ((SkillName)i).ToString();
                AddHtml(45, y, 120, 20, LeftColor(name, "FFFFFF"), false, false);

                // 2. 현재 스킬 수치
                double skillBase = m_pm.Skills[i].Base;
                string skillColor = skillBase >= 200.0 ? "00FFFF" : "FFD700";
                AddHtml(165, y, 160, 20, CenterColor($"{skillBase:F1} / 200.0", skillColor), false, false);

                // 3. 프로그레스 바
                double expNow = m_pm.SkillList[i];
                double expMax = Math.Max(1, Misc.Util.SkillExp_Calc(m_pm, i));
                double pct = Math.Min(1.0, expNow / expMax);
                
                // 바 프레임
                AddImageTiled(335, y + 6, 200, 8, 2624); 
                int barWidth = (int)(pct * 200);
                if (barWidth > 0)
                {
                    // 진행도에 따른 색상 변화 (완료 시 하늘색)
                    AddImageTiled(335, y + 6, barWidth, 8, pct >= 1.0 ? 9204 : 9201); 
                }
                AddHtml(335, y - 2, 200, 20, CenterColor($"{pct * 100:F1}%", "FFFFFF"), false, false);

                // 4. 가문 최고 기록
                AddHtml(545, y, 180, 20, RightColor($"{accountBest:F1}", "A9A9A9"), false, false);

                y += 24;
            }

            // 하단 네비게이션
            AddImageTiled(30, 605, 720, 1, 96);
            if (page > 0)
                AddButton(40, 615, 4014, 4016, 98, GumpButtonType.Reply, 0);
            
            if (endIndex < totalSkills)
                AddButton(100, 615, 4005, 4007, 99, GumpButtonType.Reply, 0);

            // 돌아가기 버튼 (CityPointGump 연동)
            AddButton(350, 615, 4017, 4019, 0, GumpButtonType.Reply, 0);
            AddHtml(385, 615, 100, 20, LeftColor("돌아가기", "FFFFFF"), false, false);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_pm == null || !m_pm.CheckAlive()) return;

            switch (info.ButtonID)
            {
                case 0: 
                    m_pm.SendGump(new CityPointGump(m_pm)); 
                    break;
                case 98: 
                    m_pm.SkillGumpPage = Math.Max(0, m_pm.SkillGumpPage - 1); 
                    m_pm.SendGump(new SkillPointGump(m_pm)); 
                    break;
                case 99: 
                    m_pm.SkillGumpPage++; 
                    m_pm.SendGump(new SkillPointGump(m_pm)); 
                    break;
            }
        }
    }
}