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
    public class DungeonPointGump : Gump
    {
        private PlayerMobile m_Owner;
        private int m_ContinentPage;

        public DungeonPointGump(PlayerMobile pm, int continent = 1) : base(20, 20)
        {
            m_Owner = pm;
            m_ContinentPage = continent;

            AddPage(0);
            
            AddBackground(0, 0, 1150, 880, 9270);
            AddAlphaRegion(10, 10, 1130, 860);

            string[] contNames = { "", "TRAMMEL", "FELUCCA", "ILSHENAR", "MALAS", "TOKUNO", "TER MUR" };
            AddHtml(0, 25, 1150, 35, $"<CENTER><BASEFONT COLOR=#FFD700 SIZE=7><b>{contNames[m_ContinentPage]} 마스터리 보드</b></BASEFONT></CENTER>", false, false);
            
            for (int i = 1; i <= 6; i++)
            {
                int x = 40 + ((i - 1) * 95);
                AddButton(x, 65, m_ContinentPage == i ? 4006 : 4005, 4007, 10 + i, GumpButtonType.Reply, 0);
                AddHtml(x + 35, 67, 80, 20, $"<BASEFONT COLOR=#FFFFFF>{contNames[i]}</BASEFONT>", false, false);
            }

            AddImageTiled(30, 95, 1090, 1, 96);

            RenderPointStatus(); 
            RenderMasteryGrid(); 

            AddButton(530, 830, 4017, 4019, 0, GumpButtonType.Reply, 0);
            AddHtml(565, 830, 100, 20, "<BASEFONT COLOR=#FFFFFF>보드 닫기</BASEFONT>", false, false);
        }

        private void RenderPointStatus()
        {
            AddHtml(40, 110, 300, 20, "<BASEFONT COLOR=#00FA9A SIZE=4>지역별 보유 정수</BASEFONT>", false, false);
            
            int startIdx = m_ContinentPage switch { 1 => 1, 2 => 26, 3 => 51, 4 => 66, 5 => 81, _ => 91 };
            int endIdx = m_ContinentPage switch { 1 => 24, 2 => 50, 3 => 65, 4 => 80, 5 => 90, _ => 100 };

            var regions = DungeonPointSystem.ActiveRegions.Values
                .Where(r => r.SilverPointIndex >= startIdx && r.SilverPointIndex <= endIdx)
                .OrderBy(r => r.SilverPointIndex).ToList();

            // 🌟 세로 간격을 줄여서 펠루카(25항목)도 버튼에 겹치지 않게 처리
            int y = 140; 
            foreach (var info in regions)
            {
                int val = (info.SilverPointIndex >= 0 && info.SilverPointIndex < m_Owner.SilverPoint.Length) ? m_Owner.SilverPoint[info.SilverPointIndex] : 0;
                
                AddAlphaRegion(40, y, 280, 24); // 슬롯 높이 압축
                
                string color = (info.SilverPointIndex == 1) ? "87CEEB" : "FFFFFF";
                AddHtml(45, y + 3, 180, 20, $"<BASEFONT COLOR=#{color}>#{info.SilverPointIndex} {info.Name}</BASEFONT>", false, false);
                AddHtml(205, y + 3, 110, 20, $"<DIV ALIGN=RIGHT><BASEFONT COLOR=#FFD700>{val:#,0} Pt</BASEFONT></DIV>", false, false);
                
                y += 27; // 🌟 리스트 간격 최적화
            }
        }

        private void RenderMasteryGrid()
        {
            if (m_ContinentPage == 1)
            {
                AddHtml(340, 110, 500, 20, "<BASEFONT COLOR=#00FA9A SIZE=4>능력치 강화 (최대 Lv.1000)</BASEFONT>", false, false);

                int xStart = 340;
                int yStart = 140;

                for (int i = 1; i <= 48; i++)
                {
                    int column = (i - 1) / 24; 
                    int row = (i - 1) % 24;

                    int x = xStart + (column * 395); 
                    int y = yStart + (row * 29);     

                    int silverIdx = DungeonPointSystem.GetRequiredSilverIdx(i);
                    int perLevel = DungeonPointSystem.GetIncrementValue(i);
                    int curVal = (i >= 0 && i < m_Owner.GoldPoint.Length) ? m_Owner.GoldPoint[i] : 0;
                    
                    int curLevel = curVal / perLevel;
                    int cost = (curLevel * 100) + 1000;
                    double actualStat = curVal / 10000.0;

                    AddAlphaRegion(x, y, 385, 26);
                    
                    AddHtml(x + 8, y + 4, 135, 20, $"<BASEFONT COLOR=#FFFFFF>{DungeonPointSystem.GetStatName(i)} <BASEFONT COLOR=#A9A9A9>[#{silverIdx}]</BASEFONT></BASEFONT>", false, false);
                    AddHtml(x + 145, y + 4, 55, 20, $"<DIV ALIGN=RIGHT><BASEFONT COLOR=#00BFFF>+{actualStat:0.##}</BASEFONT></DIV>", false, false);
                    AddHtml(x + 205, y + 4, 90, 20, $"<DIV ALIGN=RIGHT><BASEFONT COLOR=#FFD700>비용: {cost:#,0}</BASEFONT></DIV>", false, false);
                    AddHtml(x + 300, y + 4, 45, 20, $"<DIV ALIGN=RIGHT><BASEFONT COLOR=#00FA9A>Lv.{curLevel}</BASEFONT></DIV>", false, false);
                    AddButton(x + 350, y + 3, 4005, 4007, 100 + i, GumpButtonType.Reply, 0);
                }

                AddHtml(340, 840, 750, 40, "<BASEFONT COLOR=#A9A9A9>* 1,000 레벨 달성 시 해당 능력치의 위력이 전설적인 무기 최대치의 2배까지 증폭됩니다.</BASEFONT>", false, false);
            }
            else
            {
                AddHtml(340, 110, 500, 20, "<BASEFONT COLOR=#00FA9A SIZE=4>마스터리 능력 각성 보드</BASEFONT>", false, false);
                AddAlphaRegion(340, 140, 780, 696);
                AddHtml(340, 450, 780, 40, "<CENTER><BASEFONT COLOR=#A9A9A9 SIZE=5>현재 능력치 강화는 트라멜 대륙의 정수로만 가능합니다.<br>타 대륙의 전용 마스터리 시스템은 준비 중입니다.</BASEFONT></CENTER>", false, false);
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Owner == null || m_Owner.Deleted) return;

            if (info.ButtonID == 0) return;

            if (info.ButtonID >= 11 && info.ButtonID <= 16)
            {
                m_Owner.SendGump(new DungeonPointGump(m_Owner, info.ButtonID - 10));
                return;
            }

            if (info.ButtonID >= 101 && info.ButtonID <= 148)
            {
                int goldIdx = info.ButtonID - 100;
                DungeonPointSystem.DoTrain(m_Owner, goldIdx);
                m_Owner.SendGump(new DungeonPointGump(m_Owner, m_ContinentPage));
            }
        }
    }
}