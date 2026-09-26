using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Multis;

namespace Server.Misc
{
    public class OwnerWorkshopPricingGump : Gump
    {
        private Mobile m_From;
        private BaseHouse m_House;
        
        private static readonly SkillName[] TargetSkills = new SkillName[]
        {
            SkillName.Blacksmith, SkillName.Tailoring, SkillName.Carpentry,
            SkillName.Tinkering, SkillName.Alchemy, SkillName.Cooking,
            SkillName.Inscribe, SkillName.Fletching
        };

        private List<SkillName> m_ActiveSkills = new List<SkillName>();

        public OwnerWorkshopPricingGump(Mobile from, BaseHouse house) : base(50, 50)
        {
            m_From = from;
            m_House = house;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            // 뒷배경 (예전 구인공고 디자인 재활용)
            AddPage(0);
            AddBackground(0, 0, 450, 400, 9270);
            
            // 타이틀
            AddHtml(0, 15, 450, 20, "<CENTER><BASEFONT SIZE='6' COLOR=#FFFFFF>명인 공방 [이용권 가격] 관리</BASEFONT></CENTER>", false, false);
            AddImageTiled(20, 45, 410, 3, 9151);

            int y = 60;
            
            foreach (var skill in TargetSkills)
            {
                double synergy = HouseTeamManager.GetCraftingSynergy(house, skill);
                if (synergy > 0)
                {
                    m_ActiveSkills.Add(skill);
                    
                    int basePrice = HouseTeamManager.GetBasePriceFor100Charges(house, skill);
                    int minPrice = (int)(basePrice * 0.5);
                    int maxPrice = (int)(basePrice * 5.0);
                    int currentPrice = HouseTeamManager.GetActualPriceFor100Charges(house, skill);

                    // 스킬 이름 및 한도 안내
                    AddHtml(20, y, 410, 20, $"<BASEFONT COLOR=#FDB913>{skill} 버프 (+{(int)(synergy*10)}%)</BASEFONT>", false, false);
                    AddHtml(20, y + 20, 410, 20, $"<BASEFONT COLOR=#CCCCCC>100회 이용료 [권장: {basePrice}G] ({minPrice}G ~ {maxPrice}G)</BASEFONT>", false, false);
                    
                    // 가격 입력창 (배경 + TextEntry)
                    AddImageTiled(300, y + 17, 100, 22, 2624);
                    AddTextEntry(305, y + 18, 90, 20, 0x480, (int)skill, currentPrice.ToString());
                    
                    y += 50;
                }
            }

            if (m_ActiveSkills.Count == 0)
            {
                AddHtml(20, y, 410, 20, "<BASEFONT COLOR=#CCCCCC>현재 이 집에 고용된 생산/접객 알바생이 없습니다.</BASEFONT>", false, false);
                y += 50;
            }

            AddImageTiled(20, y, 410, 3, 9151);
            y += 15;

            // [OKAY] 가격 설정 저장 버튼
            if (m_ActiveSkills.Count > 0)
            {
                AddButton(20, y, 4005, 4007, 1, GumpButtonType.Reply, 0);
                AddHtml(55, y + 2, 300, 20, "<BASEFONT COLOR=#FFFFFF>가격 설정 저장 (간판에 즉시 반영)</BASEFONT>", false, false);
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1 && m_House != null && !m_House.Deleted)
            {
                bool updated = false;

                foreach (var skill in m_ActiveSkills)
                {
                    TextRelay tr = info.GetTextEntry((int)skill);
                    if (tr != null)
                    {
                        if (int.TryParse(tr.Text.Trim(), out int newPrice))
                        {
                            int basePrice = HouseTeamManager.GetBasePriceFor100Charges(m_House, skill);
                            int minPrice = (int)(basePrice * 0.5);
                            int maxPrice = (int)(basePrice * 5.0);

                            // 상하한선 캡(Cap) 씌우기
                            if (newPrice < minPrice) newPrice = minPrice;
                            if (newPrice > maxPrice) newPrice = maxPrice;

                            HouseTeamManager.SetCustomPrice(m_House, skill, newPrice);
                            updated = true;
                        }
                    }
                }

                if (updated)
                {
                    m_From.SendMessage(0x35, "공방 이용권 가격 설정이 완료되었습니다.");
                }
                
                // 설정 후 다시 보여줌 (바뀐 가격 확인용)
                m_From.SendGump(new OwnerWorkshopPricingGump(m_From, m_House));
            }
        }
    }
}

