using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Network;
using Server.Multis;
using Server.Mobiles;

namespace Server.Misc
{
    public class PublicWorkshopServiceGump : Gump
    {
        private Mobile m_From;
        private BaseHouse m_House;
        
        // 스킬별 판매 설정 (현재는 임시로 100회당 2,000골드 고정)
        private const int ChargesToSell = 100;
        private const int PricePerCharge = 20; // 100회 = 2,000G
        private const int TotalPrice = ChargesToSell * PricePerCharge;

        private static readonly SkillName[] TargetSkills = new SkillName[]
        {
            SkillName.Blacksmith, SkillName.Tailoring, SkillName.Carpentry,
            SkillName.Tinkering, SkillName.Alchemy, SkillName.Cooking,
            SkillName.Inscribe, SkillName.Fletching
        };

        public PublicWorkshopServiceGump(Mobile from, BaseHouse house) : base(100, 100)
        {
            m_From = from;
            m_House = house;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddBackground(0, 0, 450, 400, 9270);
            AddHtml(0, 15, 450, 20, $"<CENTER><BASEFONT SIZE='6' COLOR=#FFFFFF>[ {house?.Sign?.Name ?? "명인 공방"} ] 대여 서비스</BASEFONT></CENTER>", false, false);
            
            AddImageTiled(20, 45, 410, 3, 9151);
            AddHtml(20, 55, 400, 20, "<BASEFONT COLOR=#FDB913>이 공방에 고용된 조수들로부터 제작 지원(버프)을 대여합니다.</BASEFONT>", false, false);

            int y = 90;
            bool hasAnyBuff = false;

            foreach (var skill in TargetSkills)
            {
                double synergy = HouseTeamManager.GetCraftingSynergy(house, skill);
                if (synergy > 0)
                {
                    hasAnyBuff = true;
                    int buffPercent = (int)(synergy * 10);
                    int TotalPrice = HouseTeamManager.GetActualPriceFor100Charges(house, skill);
                    
                    AddHtml(20, y, 200, 20, $"<BASEFONT COLOR=#55FF55>▶ {skill} 버프 (+{buffPercent}%)</BASEFONT>", false, false);
                    
                    AddButton(250, y - 2, 4005, 4007, 100 + (int)skill, GumpButtonType.Reply, 0);
                    AddHtml(285, y, 150, 20, $"<BASEFONT COLOR=#FFFFFF>{ChargesToSell}회 구매 ({TotalPrice:N0}G)</BASEFONT>", false, false);
                    
                    y += 35;
                }
            }

            if (!hasAnyBuff)
            {
                AddHtml(20, y, 400, 20, "<BASEFONT COLOR=#CCCCCC>현재 이 공방에는 고용된 생산 조수가 없습니다.</BASEFONT>", false, false);
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0 || m_House == null || m_House.Deleted) return;

            if (info.ButtonID >= 100)
            {
                SkillName selectedSkill = (SkillName)(info.ButtonID - 100);
                int TotalPrice = HouseTeamManager.GetActualPriceFor100Charges(m_House, selectedSkill);
                
                // 다시 한번 시너지 체크 (어뷰징 방지)
                double synergy = HouseTeamManager.GetCraftingSynergy(m_House, selectedSkill);
                if (synergy <= 0)
                {
                    m_From.SendMessage("해당 조수들이 현재 공방에 없습니다.");
                    return;
                }

                if (!Banker.Withdraw(m_From, TotalPrice))
                {
                    m_From.SendMessage($"은행 잔고가 부족합니다. ({TotalPrice:N0} 골드 필요)");
                    return;
                }

                // 수익금을 집주인에게 지급
                bool deposited = false;
                if (m_House.Owner != null && Banker.Deposit(m_House.Owner, TotalPrice))
                    deposited = true;

                if (!deposited)
                {
                    // 최악의 경우 집주인이 돈을 받을 수 없는 상태면 유저에게 환불
                    Banker.Deposit(m_From, TotalPrice);
                    m_From.SendMessage("집주인의 수금함에 문제가 있어 결제가 취소되었습니다.");
                    return;
                }

                // 직업군 매핑 (스킬 -> NpcJobClass)
                NpcJobClass jobClass = MapSkillToJobClass(selectedSkill);

                // 이용권 발급
                WorkshopTicket ticket = new WorkshopTicket(m_House, jobClass, ChargesToSell, TotalPrice / ChargesToSell);
                m_From.AddToBackpack(ticket);
                
                m_From.SendMessage(0x35, $"{selectedSkill} 공방 이용권({ChargesToSell}회)을 구매했습니다! 가방을 확인하세요.");
                m_From.SendGump(new PublicWorkshopServiceGump(m_From, m_House)); // 리프레시
            }
        }

        private NpcJobClass MapSkillToJobClass(SkillName skill)
        {
            return skill switch
            {
                SkillName.Blacksmith => NpcJobClass.Blacksmith,
                SkillName.Tailoring => NpcJobClass.Tailor,
                SkillName.Carpentry => NpcJobClass.Carpenter_Producer,
                SkillName.Tinkering => NpcJobClass.Tinker,
                SkillName.Alchemy => NpcJobClass.Alchemist,
                SkillName.Cooking => NpcJobClass.Cook_Entertainer,
                SkillName.Inscribe => NpcJobClass.Scribe_Mage,
                SkillName.Fletching => NpcJobClass.Bowyer,
                _ => NpcJobClass.Laborer
            };
        }
    }
}




