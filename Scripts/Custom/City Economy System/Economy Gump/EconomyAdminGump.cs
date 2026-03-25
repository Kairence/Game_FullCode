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
			
			// [추가] 우측 상단: NPC 전역 관리 버튼 (ID: 20)
            AddButton(620, 15, 4005, 4007, 20, GumpButtonType.Reply, 0); 
            AddLabel(655, 17, 0x481, "NPC 전역 관리");
            // --- 1. 상단 대륙 탭 (ID: 1 ~ 6) ---
            string[] mapNames = { "Trammel", "Felucca", "Ilshenar", "Malas", "Tokuno", "TerMur" };
            for (int i = 0; i < mapNames.Length; i++)
            {
                int x = 20 + (i * 130);
                AddButton(x, 50, m_MapIndex == i ? 4006 : 4005, 4007, i + 1, GumpButtonType.Reply, 0);
                AddLabel(x + 35, 52, m_MapIndex == i ? 68 : 1152, mapNames[i]);
            }

            // --- 2. 헤더 라인 ---
            int y = 110;
            AddImageTiled(20, y + 25, 760, 2, 9277); // 헤더 구분선
            AddLabel(25, y, 1152, "ID");
            AddLabel(75, y, 1152, "마을 이름");
            AddLabel(235, y, 1152, "상인 수");
            AddLabel(345, y, 1152, "창고 가치 (Platinum/Gold)");
            AddLabel(695, y, 1152, "세부 관리");

            // --- 3. 마을 리스트 출력 루프 ---
            Map targetMap = Facets[m_MapIndex];
			int logicID = m_MapIndex;
            var townList = TownEconomyManager.Towns.Values
				.Where(t => (t.TownID / 100) == logicID) 
				.OrderBy(t => t.TownID).ToList();

            int start = m_TPage * 14; // 한 페이지에 14개씩
            int end = Math.Min(start + 14, townList.Count);

            for (int i = start; i < end; i++)
            {
                y += 28;
                var town = townList[i];
                
                // 실시간 상인 수 카운트 (Gump를 그릴 때 최신화)
                int vCount = World.Mobiles.Values.OfType<BaseVendor>()
            .Count(v => v.Map == targetMap && TownNumber.GetID(v.Location, v.Map) == town.TownID && !(v is Banker));

                AddLabel(25, y, 0x481, town.TownID.ToString());
                AddLabel(75, y, 0x481, town.Name);
                AddLabel(235, y, vCount == 0 ? 33 : 68, $"{vCount} 명");
                AddLabel(345, y, 0x481, town.TotalWealthString); // Wealth + 재고 가치 합산 문자열
                
                // 상세 관리 버튼 (ID: 100 + TownID)
                AddButton(705, y, 4005, 4007, 100 + town.TownID, GumpButtonType.Reply, 0);
            }

            // --- 4. 페이징 버튼 ---
            if (m_TPage > 0) AddButton(350, 510, 4014, 4016, 98, GumpButtonType.Reply, 0);
            AddLabel(390, 520, 1152, $"{m_TPage + 1} / {(townList.Count - 1) / 14 + 1}");
            if (end < townList.Count) AddButton(440, 510, 4005, 4007, 99, GumpButtonType.Reply, 0);

            // --- 5. 하단 공통 컨트롤 (3개 버튼) ---
            int btnY = 550;
            AddButton(80, btnY, 4005, 4007, 10, GumpButtonType.Reply, 0); AddLabel(115, btnY + 2, 68, "대륙 리스폰 ON");
            AddButton(320, btnY, 4005, 4007, 11, GumpButtonType.Reply, 0); AddLabel(355, btnY + 2, 33, "대륙 리스폰 OFF");
            AddButton(560, btnY, 4005, 4007, 14, GumpButtonType.Reply, 0); AddLabel(595, btnY + 2, 1152, "경제 지표 동기화");
        }

		private void DrawTownInventory()
        {
            // out 키워드 금지 규칙 적용
            if (!TownEconomyManager.Towns.ContainsKey(m_TownID)) return;
            var town = TownEconomyManager.Towns[m_TownID];

            // 진입 시 지표 동기화 (가격 계산용)
            town.UpdateBaseWealth();

            AddHtml(10, 15, 780, 25, $"<CENTER><BASEFONT COLOR='#68FF68' SIZE='6'>[{town.Name}] WAREHOUSE</BASEFONT></CENTER>", false, false);
            AddButton(20, 15, 4014, 4016, 999, GumpButtonType.Reply, 0); AddLabel(55, 15, 1152, "목록으로");

            // 시민 관리 창 이동 버튼
            AddButton(150, 15, 4005, 4007, 1000, GumpButtonType.Reply, 0); 
            AddLabel(185, 15, 68, "시민(NPC) 관리");

            string ecoInfo = $"현금: {town.Wealth:N0}g  |  기준: {town.BaseWealth:N0}g  |  물가: {town.PriceMultiplier:F2}x";
            AddLabel(35, 50, 0x481, ecoInfo);

            int y = 80;
            AddLabel(35, y, 53, "아이템 이름 (Type)");
            AddLabel(300, y, 53, "현재 재고");
            AddLabel(450, y, 53, "현재가 (원가)"); // 헤더 텍스트 변경
            AddLabel(635, y, 53, "수정");

            var items = town.Warehouse.Values.OrderBy(w => w.ItemType.Name).ToList();
            int start = m_IPage * 15;
            int end = Math.Min(start + 15, items.Count);

            for (int i = start; i < end; i++)
            {
                y += 26;
                var wItem = items[i];
                
                // [추가] 동적 물가가 반영된 실제 계산된 현재 가격
                int currentPrice = town.GetPrice(wItem.ItemType);

                AddImageTiled(20, y, 760, 24, 9354);
                AddLabel(35, y + 2, 1152, wItem.ItemType.Name);
                AddLabel(300, y + 2, wItem.Stock <= 500 ? 33 : 68, wItem.Stock.ToString("N0"));
                
                // [수정] 요청하신 8(10) gp 형식으로 출력
                AddLabel(450, y + 2, 1152, $"{currentPrice:N0} ({wItem.BasePrice:N0}) gp");
                
                AddButton(635, y + 2, 4005, 4007, 2000 + i, GumpButtonType.Reply, 0);
            }

            if (m_IPage > 0) AddButton(350, 555, 4014, 4016, 997, GumpButtonType.Reply, 0);
            AddLabel(390, 549, 1152, $"{m_IPage + 1} / {(items.Count - 1) / 15 + 1}");
            if (end < items.Count) AddButton(440, 555, 4005, 4007, 998, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            int id = info.ButtonID;
            if (id == 0) return;

            Map targetMap = Facets[m_MapIndex];
            int logicID = m_MapIndex; // 현재 보고 있는 대륙의 논리 번호

			// [수정] 하단 리스폰/동기화 대상 마을 필터링
			var targets = TownEconomyManager.Towns.Values
				.Where(t => (t.TownID / 100) == logicID).ToList();

            // 1. 대륙 탭 이동
            if (id >= 1 && id <= 6)
            {
                m_From.SendGump(new EconomyAdminGump(m_From, id - 1, 0, 0, 0));
                return;
            }

            if (m_TownID == 0)
            {
                switch (id)
                {
					case 20: // 전체 NPC 관리 창 (신설	)
                        m_From.SendGump(new EconomyGlobalNpcGump(m_From, m_MapIndex));
                        return;				
					case 10: // 리스폰 ON
                        // 순수하게 월드에 상인을 스폰시키는 작업만 수행합니다. (스캔 X, 명부 등록 X)
                        ToggleVendorNodes(targetMap, true);
                        m_From.SendMessage(68, "상인 리스폰이 완료되었습니다. 정보를 보려면 [경제 지표 동기화]를 눌러주세요.");
                        break;

                    case 11: // 리스폰 OFF
                        foreach (var t in targets) { t.Warehouse.Clear(); t.Wealth = 0; }
                        ToggleVendorNodes(targetMap, false);
                        break;
					case 14: // 지표 동기화 (스캔 + 명부 등록 + 연산)
						// 1. 월드를 싹 읽어서 명부에 없는 도시/전초기지 객체 생성
						foreach (var m in World.Mobiles.Values)
						{
							if (m is BaseVendor v && v is not Banker && v.Map == targetMap)
							{
								int tID = TownNumber.GetID(v.Location, v.Map);
								if (tID > 0 && !TownEconomyManager.Towns.ContainsKey(tID))
								{
									var newTown = new TownEconomy(tID, 0);
									
									// TownID 뒷자리가 50 이상이면 자동으로 C등급 배정
									if ((tID % 100) >= 50) 
									{
										newTown.TownIndex = "C"; 
									}
									TownEconomyManager.Towns[tID] = newTown;
								}
							}
						}

						// 2. 완성된 명부를 바탕으로 상인 수 세고 돈 계산
						var syncTargets = TownEconomyManager.Towns.Values.Where(t => (t.TownID / 100) == logicID).ToList();
						foreach (var t in syncTargets)
						{
							t.VendorCount = World.Mobiles.Values.OfType<BaseVendor>()
								.Count(v => v.Map == targetMap && TownNumber.GetID(v.Location, v.Map) == t.TownID && v is not Banker);
							
							t.UpdateBaseWealth();
							t.Wealth = t.BaseWealth; // 초기화/동기화 시 자본금 세팅
						}
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
            else // 인벤토리 뷰 응답
            {
                if (id == 999) m_TownID = 0;
                else if (id == 997) m_IPage--;
                else if (id == 998) m_IPage++;
				else if (id == 1000) // [새로 추가할 부분] 시민 관리 Gump 호출
				{
					m_From.SendGump(new EconomyCitizenMainGump(m_From, TownEconomyManager.Towns[m_TownID], m_MapIndex, m_TPage));
					return;
				}
				else if (id >= 2000) // 아이템 수정
                {
                    var items = TownEconomyManager.Towns[m_TownID].Warehouse.Values.OrderBy(w => w.ItemType.Name).ToList();
                    int idx = id - 2000;
                    if (idx >= 0 && idx < items.Count)
                    {
                        m_From.SendGump(new EconomyItemEditGump(m_From, TownEconomyManager.Towns[m_TownID], items[idx], m_TPage, m_IPage, m_MapIndex));
                        return;
                    }
                }
                // 아이템 수정(2000+) 등은 기존 로직 유지
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

    // ==========================================
    // 개별 아이템 수정 Gump (유저님 작성본 연동)
    // ==========================================
    public class EconomyItemEditGump : Gump
    {
        private Mobile m_From; private TownEconomy m_Town; private WarehouseItem m_Item;
        private int m_TPage, m_IPage, m_MapIndex;

        public EconomyItemEditGump(Mobile f, TownEconomy t, WarehouseItem i, int tp, int ip, int mapIndex) : base(300, 300)
        {
            m_From = f; m_Town = t; m_Item = i; m_TPage = tp; m_IPage = ip; m_MapIndex = mapIndex;
            AddBackground(0, 0, 300, 220, 9270);
            AddHtml(0, 15, 300, 20, $"<CENTER><BASEFONT COLOR=#FDB913>{i.ItemType.Name}</BASEFONT></CENTER>", false, false);
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
                m_From.SendMessage(68, $"{m_Item.ItemType.Name} 수정 완료.");
            }
            m_From.SendGump(new EconomyAdminGump(m_From, m_MapIndex, m_Town.TownID, m_TPage, m_IPage));
        }
    }
}
