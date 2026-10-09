using System;
using System.Linq;
using Server.Network;
using Server.Gumps;
using Server.Misc;
using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Misc
{
    public class EconomyAdminGump : Gump
    {
        private Mobile m_From;
        private int m_MapIndex; // 0:Trammel, 1:Felucca ...
        private int m_TownID;   // 0이면 목록, >0이면 상세
        private int m_TPage;    // 마을 목록 페이지
        private int m_IPage;    // 인벤토리 페이지

        public EconomyAdminGump(Mobile from, int mapIdx = 0, int townID = 0, int tPage = 0, int iPage = 0) 
            : base(50, 50)
        {
            m_From = from;
            m_MapIndex = mapIdx;
            m_TownID = townID;
            m_TPage = tPage;
            m_IPage = iPage;

            from.CloseGump(typeof(EconomyAdminGump));

            AddPage(0);
            AddBackground(0, 0, 800, 600, 9270);
            AddAlphaRegion(10, 10, 780, 580);

            if (m_TownID == 0) DrawTownList();
            else DrawTownInventory();
        }

        private void DrawTownList()
        {
            AddHtml(10, 15, 780, 25, "<CENTER><BASEFONT SIZE='6' COLOR='#FFFFFF'>TOWN ECONOMY & VENDOR ADMIN</BASEFONT></CENTER>", false, false);
            
            // 우측 상단: 관리 버튼들
            AddButton(620, 15, 4005, 4007, 20, GumpButtonType.Reply, 0); 
            AddLabel(655, 17, 0x481, "NPC 전역 관리");
            
            AddButton(460, 15, 4005, 4007, 30, GumpButtonType.Reply, 0); 
            AddLabel(495, 17, 1152, "모험가 전역 현황");
            
            AddButton(300, 15, 4005, 4007, 50, GumpButtonType.Reply, 0); 
            AddLabel(335, 17, 68, "유저 자산 순위");

            // [추가] 마크다운 리포트 통합 출력 버튼 (ID: 60)
            AddButton(140, 15, 4005, 4007, 60, GumpButtonType.Reply, 0); 
            AddLabel(175, 17, 1152, "리포트 출력");
            
            // --- 1. 상단 대륙 탭 ---
            string[] mapNames = { "Trammel", "Felucca", "Ilshenar", "Malas", "Tokuno", "TerMur" };
            for (int i = 0; i < mapNames.Length; i++)
            {
                int x = 20 + (i * 130);
                AddButton(x, 50, m_MapIndex == i ? 4006 : 4005, 4007, i + 1, GumpButtonType.Reply, 0);
                AddLabel(x + 35, 52, m_MapIndex == i ? 68 : 1152, mapNames[i]);
            }

            // [통합된 기능] 글로벌 경제 대시보드 (Y: 80 ~ 130)
            AddImageTiled(20, 75, 760, 55, 2624); 

            long adjustedTown = Math.Max(1L, GlobalEconomyMonitor.TotalTownWealth + GlobalEconomyMonitor.TownWealthOffset);
            
            AddLabel(30, 80, 2100, "유저 골드:");
            AddLabel(100, 80, 1152, $"{GlobalEconomyMonitor.TotalUserWealth:N0}");
            
            AddLabel(260, 80, 2100, "마을 골드:");
            AddLabel(330, 80, 1152, $"{GlobalEconomyMonitor.TotalTownWealth:N0}");
            
            AddLabel(490, 80, 2100, "보정 후:");
            AddLabel(545, 80, 68, $"{adjustedTown:N0}");
            
            AddLabel(680, 80, 2100, "GII:");
            AddLabel(710, 80, 1258, $"{GlobalEconomyMonitor.GII:F4}");

            bool engineOn = GlobalEconomyMonitor.EnableEconomyEngine;
            AddLabel(30, 105, 2100, "엔진 개입:");
            AddButton(100, 105, 4005, 4007, 40, GumpButtonType.Reply, 0); 
            AddLabel(135, 107, engineOn ? 68 : 33, engineOn ? "ON" : "OFF");

            AddLabel(260, 105, 2100, "보정값(+/-):");
            AddImageTiled(345, 105, 120, 20, 3004); 
            AddTextEntry(347, 105, 116, 20, 0, 0, GlobalEconomyMonitor.TownWealthOffset.ToString()); 
            AddButton(470, 105, 4011, 4012, 41, GumpButtonType.Reply, 0); 

            // --- 2. 마을 리스트 헤더 ---
            int y = 140; 
            AddImageTiled(20, y + 25, 760, 2, 9277); 
            AddLabel(25, y, 1152, "ID");
            AddLabel(75, y, 1152, "마을 이름");
            AddLabel(235, y, 1152, "상인 수");
            AddLabel(345, y, 1152, "창고 가치 (Platinum/Gold)");
            AddLabel(695, y, 1152, "세부 관리");

            // --- 3. 마을 리스트 출력 ---
            Map targetMap = Facets[m_MapIndex];
            int logicID = m_MapIndex;
            var townList = TownEconomyManager.Towns.Values
                .Where(t => (t.TownID / 100) == logicID) 
                .OrderBy(t => t.TownID).ToList();

            int start = m_TPage * 13; 
            int end = Math.Min(start + 13, townList.Count);

            for (int i = start; i < end; i++)
            {
                y += 28;
                var town = townList[i];
                
                int vCount = World.Mobiles.Values.OfType<BaseVendor>()
            .Count(v => v.Map == targetMap && TownNumber.GetID(v.Location, v.Map) == town.TownID && !(v is Banker));

                AddLabel(25, y, 0x481, town.TownID.ToString());
                AddLabel(75, y, 0x481, town.Name);
                AddLabel(235, y, vCount == 0 ? 33 : 68, $"{vCount} 명");
                AddLabel(345, y, 0x481, town.TotalWealthString);
                
                AddButton(705, y, 4005, 4007, 100 + town.TownID, GumpButtonType.Reply, 0);
            }

            // --- 4. 페이징 버튼 ---
            if (m_TPage > 0) AddButton(350, 535, 4014, 4016, 98, GumpButtonType.Reply, 0);
            AddLabel(390, 535, 1152, $"{m_TPage + 1} / {Math.Max(1, (townList.Count - 1) / 13 + 1)}");
            if (end < townList.Count) AddButton(440, 535, 4005, 4007, 99, GumpButtonType.Reply, 0);

            // --- 5. 하단 공통 컨트롤 ---
            int btnY = 565; 
            AddButton(80, btnY, 4005, 4007, 10, GumpButtonType.Reply, 0); AddLabel(115, btnY + 2, 68, "대륙 리스폰 ON");
            AddButton(320, btnY, 4005, 4007, 11, GumpButtonType.Reply, 0); AddLabel(355, btnY + 2, 33, "대륙 리스폰 OFF");
            AddButton(560, btnY, 4005, 4007, 14, GumpButtonType.Reply, 0); AddLabel(595, btnY + 2, 1152, "경제 지표 동기화");
        }

        private void DrawTownInventory()
        {
            if (!TownEconomyManager.Towns.ContainsKey(m_TownID)) return;
            var town = TownEconomyManager.Towns[m_TownID];

            town.UpdateBaseWealth();

            AddHtml(10, 15, 780, 25, $"<CENTER><BASEFONT COLOR='#68FF68' SIZE='6'>[{town.Name}] WAREHOUSE</BASEFONT></CENTER>", false, false);
            AddButton(20, 15, 4014, 4016, 999, GumpButtonType.Reply, 0); AddLabel(55, 15, 1152, "목록으로");

            AddButton(150, 15, 4005, 4007, 1000, GumpButtonType.Reply, 0); 
            AddLabel(185, 15, 68, "시민(NPC) 관리");
			
			// [추가] 상세 거래 내역(장부) 열람 버튼 (ID: 1001)
            AddButton(320, 15, 4005, 4007, 1001, GumpButtonType.Reply, 0); 
            AddLabel(355, 15, 53, "상세 거래 내역(장부)");

            string ecoInfo = $"현금: {town.Wealth:N0}g  |  기준: {town.BaseWealth:N0}g  |  물가: {town.PriceMultiplier:F2}x";
            AddLabel(35, 50, 0x481, ecoInfo);

            int y = 80;
            AddLabel(35, y, 53, "아이템 이름 (Type)");
            AddLabel(280, y, 53, "현재 재고 (추이)"); // 🌟 UI 라벨 위치 조정
            AddLabel(450, y, 53, "현재가 (원가)");
            AddLabel(635, y, 53, "수정");

            var items = town.Warehouse.Values.OrderBy(w => w.ItemKey.Name).ToList();
            int start = m_IPage * 15;
            int end = Math.Min(start + 15, items.Count);

            for (int i = start; i < end; i++)
            {
                y += 26;
                var wItem = items[i];
                
                int currentPrice = town.GetPrice(wItem.ItemType);

                // 🌟 [추가] 변동 추이 계산 및 시각화 화살표
                int trend = wItem.Stock - wItem.LastStock;
                string trendStr = trend == 0 ? "-" : (trend > 0 ? $"▲{trend}" : $"▼{Math.Abs(trend)}");
                int trendColor = trend == 0 ? 1152 : (trend > 0 ? 68 : 33); // 68: 녹색(재고 증가), 33: 빨강(재고 감소)

                AddImageTiled(20, y, 760, 24, 9354);
                AddLabel(35, y + 2, 1152, wItem.ItemKey.Name);
                
                // 🌟 재고량 수치 옆에 변동 추이 출력
                AddLabel(280, y + 2, wItem.Stock <= 500 ? 33 : 1152, wItem.Stock.ToString("N0"));
                AddLabel(340, y + 2, trendColor, $"({trendStr})");
                
                AddLabel(450, y + 2, 1152, $"{currentPrice:N0} ({wItem.BasePrice:N0}) gp");
                AddButton(635, y + 2, 4005, 4007, 2000 + i, GumpButtonType.Reply, 0);
            }

            if (m_IPage > 0) AddButton(350, 555, 4014, 4016, 997, GumpButtonType.Reply, 0);
            AddLabel(390, 549, 1152, $"{m_IPage + 1} / {Math.Max(1, (items.Count - 1) / 15 + 1)}");
            if (end < items.Count) AddButton(440, 555, 4005, 4007, 998, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            int id = info.ButtonID;
            if (id == 0) return;

            Map targetMap = Facets[m_MapIndex];
            int logicID = m_MapIndex;

            var targets = TownEconomyManager.Towns.Values
                .Where(t => (t.TownID / 100) == logicID).ToList();

            if (id >= 1 && id <= 6)
            {
                m_From.SendGump(new EconomyAdminGump(m_From, id - 1, 0, 0, 0));
                return;
            }

            if (m_TownID == 0)
            {
                switch (id)
                {
                    case 50: 
                        m_From.SendGump(new EconomyUserWealthGump(m_From, m_MapIndex, m_TPage, 0));
                        return;

                    // [추가] 리포트 2종 동시 추출 로직 연동
                    case 60:
                        TownEconomyExporter.ManualExport();
                        GlobalEconomyReport.GenerateMasterReport(TownEconomyManager.Towns.Values.ToList());
                        m_From.SendMessage(68, "경제 시스템 분석 보고서 2종이 성공적으로 생성되었습니다.");
                        break;

                    case 40: 
                        GlobalEconomyMonitor.EnableEconomyEngine = !GlobalEconomyMonitor.EnableEconomyEngine;
                        m_From.SendMessage(GlobalEconomyMonitor.EnableEconomyEngine ? 68 : 33, $"Economy Engine is now {(GlobalEconomyMonitor.EnableEconomyEngine ? "ON" : "OFF")}.");
                        break;
                    
                    case 41:
                        if (info.GetTextEntry(0) is TextRelay text)
                        {
                            if (long.TryParse(text.Text, out long newOffset))
                            {
                                GlobalEconomyMonitor.TownWealthOffset = newOffset;
                                GlobalEconomyMonitor.GenerateUserReport(); 
                                m_From.SendMessage(68, "마을 자산 보정값이 적용되어 GII가 갱신되었습니다.");
                            }
                            else m_From.SendMessage(33, "잘못된 숫자 형식입니다.");
                        }
                        break;

                    case 20: 
                        m_From.SendGump(new EconomyGlobalNpcGump(m_From, m_MapIndex));
                        return; 
                    case 30: 
                        m_From.SendGump(new EconomyAdventurerMainGump(m_From, m_MapIndex, m_TPage, 0, 0));
                        return;             
                    case 10: 
                        ToggleVendorNodes(targetMap, true);
                        m_From.SendMessage(68, "상인 리스폰이 완료되었습니다. 정보를 보려면 [경제 지표 동기화]를 눌러주세요.");
                        break;
                    case 11: 
                        foreach (var t in targets) { t.Warehouse.Clear(); t.Wealth = 0; }
                        ToggleVendorNodes(targetMap, false);
                        break;
                    case 14: 
                        foreach (var m in World.Mobiles.Values)
                        {
                            if (m is BaseVendor v && v is not Banker && v.Map == targetMap)
                            {
                                int tID = TownNumber.GetID(v.Location, v.Map);
                                if (tID > 0 && !TownEconomyManager.Towns.ContainsKey(tID))
                                {
                                    var newTown = new TownEconomy(tID, 0);
                                    if ((tID % 100) >= 50) newTown.TownIndex = "C"; 
                                    TownEconomyManager.Towns[tID] = newTown;
                                }
                            }
                        }

                        var syncTargets = TownEconomyManager.Towns.Values.Where(t => (t.TownID / 100) == logicID).ToList();
                        foreach (var t in syncTargets)
                        {
                            t.VendorCount = World.Mobiles.Values.OfType<BaseVendor>()
                                .Count(v => v.Map == targetMap && TownNumber.GetID(v.Location, v.Map) == t.TownID && v is not Banker);
                            
                            t.UpdateBaseWealth();
                            t.Wealth = t.BaseWealth; 
                        }
                        
                        GlobalEconomyMonitor.GenerateUserReport(); 
                        m_From.SendMessage(68, "도시 및 전초기지를 스캔하여 경제 지표를 동기화했습니다.");
                        break;
                    case 98: m_TPage--; break;
                    case 99: m_TPage++; break;
                    default:
                        if (id >= 100 && id < 1000)
                        {
                            m_From.SendGump(new EconomyAdminGump(m_From, m_MapIndex, id - 100, m_TPage, 0));
                            return;
                        }
                        break;
                }
            }
            else 
            {
                if (id == 999) m_TownID = 0;
                else if (id == 997) m_IPage--;
                else if (id == 998) m_IPage++;
                else if (id == 1000) 
                {
                    m_From.SendGump(new EconomyCitizenMainGump(m_From, TownEconomyManager.Towns[m_TownID], m_MapIndex, m_TPage));
                    return;
                }
                // [추가] 1001번 버튼 클릭 시 신규 장부 UI 호출
                else if (id == 1001) 
                {
                    m_From.SendGump(new EconomyTradeLedgerGump(m_From, TownEconomyManager.Towns[m_TownID], 0, m_MapIndex, m_TPage, m_IPage));
                    return;
                }
                else if (id >= 2000)
                {
                    var items = TownEconomyManager.Towns[m_TownID].Warehouse.Values.OrderBy(w => w.ItemKey.Name).ToList();
                    int idx = id - 2000;
                    if (idx >= 0 && idx < items.Count)
                    {
                        m_From.SendGump(new EconomyItemEditGump(m_From, TownEconomyManager.Towns[m_TownID], items[idx], m_TPage, m_IPage, m_MapIndex));
                        return;
                    }
                }
            }

            m_From.SendGump(new EconomyAdminGump(m_From, m_MapIndex, m_TownID, m_TPage, m_IPage));
        }

        private void ToggleVendorNodes(Map map, bool isActive)
        {
            var nodes = World.Items.Values.OfType<VendorNode>().Where(n => n.Map == map).ToList();
            foreach (var node in nodes)
            {
                node.IsActive = isActive;
                if (!isActive) node.ClearSpawned();
                else node.Respawn();
            }
        }

        private static Map[] Facets = { Map.Trammel, Map.Felucca, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur };
    }

    public class EconomyItemEditGump : Gump
    {
        // 기존과 동일하여 생략 (그대로 두시면 됩니다)
        private Mobile m_From; private TownEconomy m_Town; private WarehouseItem m_Item;
        private int m_TPage, m_IPage, m_MapIndex;

        public EconomyItemEditGump(Mobile f, TownEconomy t, WarehouseItem i, int tp, int ip, int mapIndex) : base(300, 300)
        {
            m_From = f; m_Town = t; m_Item = i; m_TPage = tp; m_IPage = ip; m_MapIndex = mapIndex;
            AddBackground(0, 0, 300, 220, 9270);
            AddHtml(0, 15, 300, 20, $"<CENTER><BASEFONT COLOR=#FDB913>{i.ItemKey.Name}</BASEFONT></CENTER>", false, false);
            AddLabel(30, 60, 1152, "현재 재고:"); AddBackground(120, 55, 100, 25, 9300);
            AddTextEntry(125, 58, 90, 20, 0, 1, i.Stock.ToString());
            AddLabel(30, 100, 1152, "기준 가격:"); AddBackground(120, 95, 100, 25, 9300);
            AddTextEntry(125, 98, 90, 20, 0, 2, i.BasePrice.ToString());
            AddButton(60, 160, 2128, 2129, 1, GumpButtonType.Reply, 0);
            AddButton(170, 160, 2119, 2120, 0, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1)
            {
                m_Item.Stock = Math.Max(0, Utility.ToInt32(info.GetTextEntry(1).Text));
                m_Item.BasePrice = Math.Max(1, Utility.ToInt32(info.GetTextEntry(2).Text));
                m_From.SendMessage(68, $"{m_Item.ItemKey.Name} 수정 완료.");
            }
            m_From.SendGump(new EconomyAdminGump(m_From, m_MapIndex, m_Town.TownID, m_TPage, m_IPage));
        }
    }
}