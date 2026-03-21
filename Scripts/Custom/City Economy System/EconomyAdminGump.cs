using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Commands;
using Server.Gumps;
using Server.Network;

namespace Server.Misc
{
    public class EconomyAdminGump : Gump
    {
        public static void Initialize()
        {
            CommandSystem.Register("EconomyAdmin", AccessLevel.GameMaster, e => e.Mobile.SendGump(new EconomyAdminGump(e.Mobile, null, 0, 0)));
        }

        private readonly Mobile m_From;
        private readonly string m_SelectedTown;
        private int m_ItemPage; 
        private int m_TownPage; 

        public EconomyAdminGump(Mobile from, string selectedTown, int townPage, int itemPage) : base(50, 50)
        {
            m_From = from;
            m_SelectedTown = selectedTown;
            m_TownPage = townPage;
            m_ItemPage = itemPage;

            from.CloseGump(typeof(EconomyAdminGump));
            from.CloseGump(typeof(EconomyItemEditGump));

            AddPage(0);
            AddBackground(0, 0, 700, 550, 9270);
            AddHtml(0, 15, 700, 20, "<CENTER><BASEFONT COLOR=#FFFFFF>마을 경제 및 창고 관리 시스템 (GM)</BASEFONT></CENTER>", false, false);

            // [★ 추가] Gump 상단 중앙에 관리자 전용 퀵 버튼 3종 세트 배치
            AddButton(190, 35, 4005, 4007, 600, GumpButtonType.Reply, 0);
            AddLabel(225, 37, 1152, "자동 스폰&등록");

            AddButton(350, 35, 4005, 4007, 601, GumpButtonType.Reply, 0);
            AddLabel(385, 37, 1152, "마을 수동구축");

            AddButton(510, 35, 4005, 4007, 602, GumpButtonType.Reply, 0);
            AddLabel(545, 37, 1152, "재고 500개 누적");

            DrawTownList();
            DrawWarehouseItems();
        }

        private void DrawTownList()
        {
            AddBackground(20, 50, 150, 470, 9300);
            AddHtml(20, 60, 150, 20, "<CENTER><BASEFONT COLOR=#FFFFFF>마을 목록</BASEFONT></CENTER>", false, false);

            var towns = TownEconomyManager.Towns.Values
                            .Where(t => t.Warehouse.Count > 0)
                            .Select(t => t.TownName)
                            .OrderBy(k => k).ToList();

            int townsPerPage = 15; 
            int totalTownPages = Math.Max(1, (towns.Count + townsPerPage - 1) / townsPerPage);

            if (m_TownPage >= totalTownPages) m_TownPage = totalTownPages - 1;

            int start = m_TownPage * townsPerPage;
            int end = Math.Min(start + townsPerPage, towns.Count);

            int y = 90;
            for (int i = start; i < end; i++)
            {
                string townName = towns[i];
                bool isSelected = (m_SelectedTown == townName);
                
                AddButton(25, y, isSelected ? 4006 : 4005, 4007, i + 1, GumpButtonType.Reply, 0);
                AddLabel(60, y, isSelected ? 68 : 1152, townName);

                y += 25;
            }

            if (m_TownPage > 0)
                AddButton(25, 485, 4014, 4016, 200, GumpButtonType.Reply, 0); 
            
            AddLabel(75, 487, 1152, $"{m_TownPage + 1}/{totalTownPages}");

            if (m_TownPage < totalTownPages - 1)
                AddButton(130, 485, 4005, 4007, 201, GumpButtonType.Reply, 0); 
        }

        private void DrawWarehouseItems()
        {
            AddBackground(180, 60, 500, 460, 9300); // 버튼 공간을 위해 Y축 약간 조절

            if (string.IsNullOrEmpty(m_SelectedTown) || !TownEconomyManager.Towns.TryGetValue(m_SelectedTown, out var town))
            {
                AddHtml(180, 250, 500, 20, "<CENTER>좌측에서 마을을 선택해주세요.</CENTER>", false, false);
                return;
            }

            AddHtml(190, 70, 300, 20, $"<BASEFONT COLOR=#FDB913>[{town.TownName}] 자산: {town.Wealth:#,0} gp</BASEFONT>", false, false);
            
            AddButton(530, 68, 4005, 4007, 500, GumpButtonType.Reply, 0); 
            AddLabel(565, 70, 1152, "XML 동기화");

            AddLabel(190, 100, 1152, "아이템 종류");
            AddLabel(380, 100, 1152, "재고");
            AddLabel(470, 100, 1152, "기준가");
            AddLabel(540, 100, 1152, "적용가");

            var items = town.Warehouse.Values.OrderBy(i => i.ItemType.Name).ToList();
            int itemsPerPage = 13; // 버튼 공간 때문에 리스트 하나 줄임
            int totalItemPages = Math.Max(1, (items.Count + itemsPerPage - 1) / itemsPerPage);

            if (m_ItemPage >= totalItemPages) m_ItemPage = totalItemPages - 1;

            int start = m_ItemPage * itemsPerPage;
            int end = Math.Min(start + itemsPerPage, items.Count);

            int y = 125;
            for (int i = start; i < end; i++)
            {
                var item = items[i];
                int currentPrice = town.GetPrice(item.ItemType, town.PriceMultiplier);

                AddLabel(190, y, 0, item.ItemType.Name);
                AddLabel(380, y, item.Stock < 500 ? 33 : 68, $"{item.Stock:#,0}");
                AddLabel(470, y, 0, $"{item.BasePrice:#,0}");
                AddLabel(540, y, 88, $"{currentPrice:#,0}");
                AddButton(615, y, 4011, 4012, 1000 + i, GumpButtonType.Reply, 0);
                y += 28;
            }

            if (m_ItemPage > 0)
                AddButton(190, 485, 4014, 4016, 100, GumpButtonType.Reply, 0); 
            if (m_ItemPage < totalItemPages - 1)
                AddButton(640, 485, 4005, 4007, 101, GumpButtonType.Reply, 0); 
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0) return;
            int btn = info.ButtonID;

            // [★ 추가] 관리 편의성 명령어 원클릭 연동
            if (btn == 600)
            {
                CommandSystem.Handle(m_From, $"{CommandSystem.Prefix}AutoVendorSpawn");
                m_From.SendGump(new EconomyAdminGump(m_From, m_SelectedTown, m_TownPage, m_ItemPage));
                return;
            }
            if (btn == 601)
            {
                CommandSystem.Handle(m_From, $"{CommandSystem.Prefix}InitTowns");
                m_From.SendGump(new EconomyAdminGump(m_From, m_SelectedTown, m_TownPage, m_ItemPage));
                return;
            }
            if (btn == 602)
            {
                CommandSystem.Handle(m_From, $"{CommandSystem.Prefix}SyncTownStock");
                m_From.SendGump(new EconomyAdminGump(m_From, m_SelectedTown, m_TownPage, m_ItemPage));
                return;
            }

            if (btn == 100) { m_From.SendGump(new EconomyAdminGump(m_From, m_SelectedTown, m_TownPage, m_ItemPage - 1)); return; }
            if (btn == 101) { m_From.SendGump(new EconomyAdminGump(m_From, m_SelectedTown, m_TownPage, m_ItemPage + 1)); return; }

            if (btn == 200) { m_From.SendGump(new EconomyAdminGump(m_From, m_SelectedTown, m_TownPage - 1, 0)); return; }
            if (btn == 201) { m_From.SendGump(new EconomyAdminGump(m_From, m_SelectedTown, m_TownPage + 1, 0)); return; }

            if (btn == 500 && !string.IsNullOrEmpty(m_SelectedTown))
            {
                TownInventoryData.LoadFromXml();
                if (TownEconomyManager.Towns.TryGetValue(m_SelectedTown, out var town))
                {
                    var xmlData = TownInventoryData.GetSetupData(m_SelectedTown);
                    foreach (var entry in xmlData)
                    {
                        if (town.Warehouse.TryGetValue(entry.ItemType, out var item))
                            item.BasePrice = entry.BasePrice;
                    }
                    m_From.SendMessage(68, "XML 가격 동기화 완료.");
                }
                m_From.SendGump(new EconomyAdminGump(m_From, m_SelectedTown, m_TownPage, m_ItemPage));
                return;
            }

            if (btn >= 1 && btn < 200)
            {
                var towns = TownEconomyManager.Towns.Values
                                .Where(t => t.Warehouse.Count > 0)
                                .Select(t => t.TownName)
                                .OrderBy(k => k).ToList();
                int idx = btn - 1;
                if (idx >= 0 && idx < towns.Count)
                    m_From.SendGump(new EconomyAdminGump(m_From, towns[idx], m_TownPage, 0));
                return;
            }

            if (btn >= 1000 && !string.IsNullOrEmpty(m_SelectedTown))
            {
                int itemIdx = btn - 1000;
                if (TownEconomyManager.Towns.TryGetValue(m_SelectedTown, out var town))
                {
                    var items = town.Warehouse.Values.OrderBy(i => i.ItemType.Name).ToList();
                    if (itemIdx >= 0 && itemIdx < items.Count)
                        m_From.SendGump(new EconomyItemEditGump(m_From, town, items[itemIdx], m_TownPage, m_ItemPage));
                }
            }
        }
    }

    public class EconomyItemEditGump : Gump
    {
        private readonly Mobile m_From;
        private readonly TownEconomy m_Town;
        private readonly WarehouseItem m_Item;
        private readonly int m_TownPage;
        private readonly int m_ItemPage;

        public EconomyItemEditGump(Mobile from, TownEconomy town, WarehouseItem item, int townPage, int itemPage) : base(200, 200)
        {
            m_From = from; m_Town = town; m_Item = item; m_TownPage = townPage; m_ItemPage = itemPage;
            AddPage(0);
            AddBackground(0, 0, 300, 220, 9270);
            AddHtml(0, 15, 300, 20, $"<CENTER><BASEFONT COLOR=#FDB913>{item.ItemType.Name}</BASEFONT></CENTER>", false, false);
            AddLabel(30, 60, 0, "현재 재고:");
            AddBackground(120, 55, 100, 25, 9300);
            AddTextEntry(125, 58, 90, 20, 0, 1, item.Stock.ToString());
            AddLabel(30, 100, 0, "기준 가격:");
            AddBackground(120, 95, 100, 25, 9300);
            AddTextEntry(125, 98, 90, 20, 0, 2, item.BasePrice.ToString());
            AddButton(60, 160, 2128, 2129, 1, GumpButtonType.Reply, 0); 
            AddButton(180, 160, 2119, 2120, 0, GumpButtonType.Reply, 0); 
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1) 
            {
                if (int.TryParse(info.GetTextEntry(1)?.Text, out int nStock) && int.TryParse(info.GetTextEntry(2)?.Text, out int nPrice))
                {
                    m_Item.Stock = Math.Max(0, nStock);
                    m_Item.BasePrice = Math.Max(1, nPrice);
                    m_From.SendMessage(68, "수정 완료.");
                }
            }
            m_From.SendGump(new EconomyAdminGump(m_From, m_Town.TownName, m_TownPage, m_ItemPage));
        }
    }
}