using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Gumps;
using Server.Network;

namespace Server.Misc
{
    public class EconomyTradeLedgerGump : Gump
    {
        private Mobile m_From;
        private TownEconomy m_Town;
        private int m_Page;
        private int m_MapIndex;
        private int m_TPage;
        private int m_IPage;

        public EconomyTradeLedgerGump(Mobile from, TownEconomy town, int page, int mapIndex, int tPage, int iPage) : base(50, 50)
        {
            m_From = from;
            m_Town = town;
            m_Page = page;
            m_MapIndex = mapIndex;
            m_TPage = tPage;
            m_IPage = iPage;

            from.CloseGump(typeof(EconomyTradeLedgerGump));

            AddPage(0);
            AddBackground(0, 0, 800, 600, 9270);
            AddAlphaRegion(10, 10, 780, 580);

            AddHtml(10, 15, 780, 25, $"<CENTER><BASEFONT COLOR='#68FF68' SIZE='6'>[{town.TownName}] TRADE LEDGER</BASEFONT></CENTER>", false, false);
            
            AddButton(20, 15, 4014, 4016, 1, GumpButtonType.Reply, 0); 
            AddLabel(55, 15, 1152, "창고로 돌아가기");

            AddButton(680, 15, 4005, 4007, 2, GumpButtonType.Reply, 0); 
            AddLabel(715, 17, 1152, "새로고침");

            int y = 50;
            AddLabel(30, y, 53, "시간");
            AddLabel(150, y, 53, "주체");
            AddLabel(210, y, 53, "이름");
            AddLabel(330, y, 53, "종류");
            AddLabel(400, y, 53, "품목 (Item)");
            AddLabel(550, y, 53, "수량");
            AddLabel(610, y, 53, "단가");
            AddLabel(680, y, 53, "총액");

            if (!VirtualTradeSystem.MasterLedger.ContainsKey(town.TownID) || VirtualTradeSystem.MasterLedger[town.TownID].Count == 0)
            {
                AddLabel(300, 300, 33, "기록된 거래 내역이 없습니다.");
                return;
            }

            // 최신 거래 내역이 가장 위로 올라오도록 역순(Reverse) 정렬
            var ledger = VirtualTradeSystem.MasterLedger[town.TownID].Reverse().ToList(); 
            int start = m_Page * 18;
            int end = Math.Min(start + 18, ledger.Count);

            for (int i = start; i < end; i++)
            {
                y += 26;
                var record = ledger[i];

                AddImageTiled(20, y, 760, 24, 9354);
                
                AddLabel(30, y + 2, 1152, record.Timestamp.ToString("MM-dd HH:mm:ss"));
                
                // 시스템은 빨강, 플레이어는 녹색, NPC는 기본색으로 주체 구분
                int actorColor = record.ActorType == TraderType.Player ? 68 : (record.ActorType == TraderType.System ? 33 : 1152);
                AddLabel(150, y + 2, actorColor, record.ActorType.ToString());
                
                string tName = record.TraderName ?? "Unknown";
                AddLabel(210, y + 2, 1152, tName.Length > 12 ? tName.Substring(0, 12) : tName);
                
                // 구매/수입은 녹색, 판매/수출은 빨강
                int actionColor = (record.Action == TradeType.Buy || record.Action == TradeType.Import) ? 68 : 33;
                AddLabel(330, y + 2, actionColor, record.Action.ToString());
                
                string itemName = record.ItemKey.Name ?? "Unknown";
                AddLabel(400, y + 2, 1152, itemName.Length > 15 ? itemName.Substring(0, 15) : itemName);
                
                AddLabel(550, y + 2, 1152, record.Amount.ToString("N0"));
                AddLabel(610, y + 2, 1152, record.UnitPrice.ToString("N0"));
                AddLabel(680, y + 2, 53, record.TotalCost.ToString("N0"));
            }

            if (m_Page > 0) AddButton(350, 555, 4014, 4016, 997, GumpButtonType.Reply, 0);
            AddLabel(390, 549, 1152, $"{m_Page + 1} / {Math.Max(1, (ledger.Count - 1) / 18 + 1)}");
            if (end < ledger.Count) AddButton(440, 555, 4005, 4007, 998, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0) return;
            
            if (info.ButtonID == 1) // 창고로 돌아가기
            {
                m_From.SendGump(new EconomyAdminGump(m_From, m_MapIndex, m_Town.TownID, m_TPage, m_IPage));
                return;
            }
            else if (info.ButtonID == 2) // 새로고침
            {
                m_From.SendGump(new EconomyTradeLedgerGump(m_From, m_Town, m_Page, m_MapIndex, m_TPage, m_IPage));
                return;
            }
            else if (info.ButtonID == 997) // 이전 페이지
            {
                m_From.SendGump(new EconomyTradeLedgerGump(m_From, m_Town, m_Page - 1, m_MapIndex, m_TPage, m_IPage));
                return;
            }
            else if (info.ButtonID == 998) // 다음 페이지
            {
                m_From.SendGump(new EconomyTradeLedgerGump(m_From, m_Town, m_Page + 1, m_MapIndex, m_TPage, m_IPage));
                return;
            }
        }
    }
}