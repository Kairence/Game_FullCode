using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server;
using Server.Gumps;
using Server.Network;

namespace Server.Misc
{
    public class TownSocietyGump : Gump
    {
        private Mobile m_From;
        private TownEconomy m_Town;
        private int m_Tab;  // 0: 랭킹, 1: 족보/재정(상세), 2: 영토맵, 3: 공용 창고
        private int m_Page;
        private VirtualHouse m_SelectedHouse;
        private int m_ReturnMapIdx;
        private int m_ReturnTPage;

        public TownSocietyGump(Mobile from, TownEconomy town, int tab = 0, int page = 0, VirtualHouse selectedHouse = null, int returnMapIdx = -1, int returnTPage = 0) : base(50, 50)
        {
            m_From = from;
            m_Town = town;
            m_Tab = tab;
            m_Page = page;
            m_SelectedHouse = selectedHouse;
            m_ReturnMapIdx = returnMapIdx;
            m_ReturnTPage = returnTPage;

            from.CloseGump(typeof(TownSocietyGump));
            AddPage(0);

            // 전체 Gump를 900x640의 쾌적한 와이드 사이즈로 통일
            AddBackground(0, 0, 900, 640, 9270);
            AddAlphaRegion(10, 10, 880, 620);

            DrawCommonElements(900);

            switch (m_Tab)
            {
                case 0: DrawRankingPage(); break;
                case 1: DrawFamilyTreeAndFinancePage(); break; 
                case 2: DrawTerritoryMapPage(); break;
                case 3: DrawWarehousePage(); break;
            }
        }

        private void DrawCommonElements(int width)
        {
            AddHtml(0, 15, width, 25, $"<CENTER><BASEFONT SIZE='6' COLOR='#FDB913'>[{m_Town.TownName}] 가문 및 영토 현황판</BASEFONT></CENTER>", false, false);

            if (m_Tab != 1)
            {
                // [개선] 족보 탭을 없애고 마을 창고를 메인 탭으로 승격
                string[] labels = { "가문 랭킹", "영토 지도", "마을 창고" };
                int[] tabValues = new int[] { 0, 2, 3 };
                int tabBlockWidth = 3 * 150;
                int startX = (width - tabBlockWidth) / 2;

                for (int i = 0; i < 3; i++)
                {
                    int t = tabValues[i];
                    AddButton(startX + (i * 150), 50, m_Tab == t ? 4006 : 4005, 4007, 10 + t, GumpButtonType.Reply, 0);
                    AddLabel(startX + 35 + (i * 150), 52, m_Tab == t ? 1152 : 898, labels[i]);
                }
            }
            else
            {
                // [개선] 상세 보기(족보) 모드일 때는 돌아가기 버튼 제공
                AddButton(20, 50, 4014, 4016, 10, GumpButtonType.Reply, 0); 
                AddLabel(55, 52, 1152, "◀ 랭킹으로 돌아가기");
            }
            AddImageTiled(20, 80, width - 40, 2, 9651);
        }

        private void DrawRankingPage()
        {
            var activeHouses = m_Town.Houses.Where(h => h.IsActive).OrderByDescending(h => h.Prestige).ToList();
            int totalHouses = activeHouses.Count;
            int perPage = 15; // 세로로 길어졌으니 한 페이지에 15개씩
            int maxPage = (int)Math.Ceiling((double)totalHouses / perPage) - 1;
            if (m_Page > maxPage) m_Page = Math.Max(0, maxPage);

            // 가로가 900이므로 널찍하게 간격 배치
            AddLabel(30, 90, 53, "순위");
            AddLabel(100, 90, 53, "가문명");
            AddLabel(300, 90, 53, "최고 작위");
            AddLabel(450, 90, 53, "명성");
            AddLabel(550, 90, 53, "보유 자산");
            AddLabel(700, 90, 53, "소유 영토");
            AddLabel(800, 90, 53, "상세 (족보)");

            int y = 120;
            for (int i = m_Page * perPage; i < (m_Page + 1) * perPage && i < totalHouses; i++)
            {
                var house = activeHouses[i];
                int rankColor = i == 0 ? 1161 : i == 1 ? 2213 : i == 2 ? 2206 : 1152;

                AddLabel(30, y, rankColor, $"{i + 1}위");
                AddLabel(100, y, rankColor, house.HouseName);
                AddLabel(300, y, 1152, house.PrimaryRank.ToString());
                AddLabel(450, y, 1152, $"{house.Prestige:N0}");
                AddLabel(550, y, 65, $"{house.TotalWealth:N0} gp"); 
                AddLabel(700, y, 1359, $"{house.OwnedTileIndices.Count:N0} 칸");
                
                AddButton(810, y + 3, 2117, 2118, 1000 + m_Town.Houses.IndexOf(house), GumpButtonType.Reply, 0);
                
                y += 25;
            }

            if (m_Page > 0) AddButton(30, 590, 4014, 4016, 1, GumpButtonType.Reply, 0);
            if (m_Page < maxPage) AddButton(800, 590, 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddLabel(420, 590, 1152, $"Page {m_Page + 1} / {maxPage + 1}");
        }

		// =========================================================
        // [수정] 족보 트리 세대 계산 오류 수정 및 텍스트 색상 변경
        // =========================================================
        private void DrawFamilyTreeAndFinancePage()
        {
            if (m_SelectedHouse == null)
            {
                AddHtml(20, 200, 860, 40, "<CENTER><BASEFONT COLOR='#FFFFFF'>가문 데이터를 불러올 수 없습니다.</BASEFONT></CENTER>", false, false);
                return;
            }

            // 제목 색상 및 내용 유지
            AddHtml(20, 90, 860, 25, $"<CENTER><BASEFONT SIZE='5' COLOR='#00FF00'>◆ {m_SelectedHouse.HouseName} 가문 상세 현황 ◆</BASEFONT></CENTER>", false, false);

            string bldGarden = m_SelectedHouse.HasGarden ? "<BASEFONT COLOR='#00FF00'>[텃밭]</BASEFONT>" : "<BASEFONT COLOR='#555555'>[텃밭]</BASEFONT>";
            string bldWorkshop = m_SelectedHouse.HasWorkshop ? "<BASEFONT COLOR='#00FFFF'>[공방]</BASEFONT>" : "<BASEFONT COLOR='#555555'>[공방]</BASEFONT>";
            string bldBarracks = m_SelectedHouse.HasBarracks ? "<BASEFONT COLOR='#FF5555'>[병영]</BASEFONT>" : "<BASEFONT COLOR='#555555'>[병영]</BASEFONT>";
            AddHtml(20, 115, 860, 25, $"<CENTER><BASEFONT COLOR='#FFFFFF'>총 자산: {m_SelectedHouse.TotalWealth:N0} gp | 보유 인프라: {bldGarden} {bldWorkshop} {bldBarracks}</BASEFONT></CENTER>", false, false);

            // ---------------- [좌측: 가문 족보 트리] ----------------
            AddLabel(20, 145, 53, "가문 족보 (Family Tree)"); // 53: 금색
            AddImageTiled(20, 165, 590, 2, 9651);
            
            var allFams = m_SelectedHouse.Families.Where(f => f.IsActive).ToList();
            var rootFams = new List<FamilyUnit>();
            var visited = new HashSet<FamilyUnit>();

            // 1. 최상위(뿌리) 가구 판별 로직 강화
            foreach (var fam in allFams)
            {
                // 부모 가구가 활성화되어 있다면 뿌리가 아님
                if (fam.ParentFamily != null && fam.ParentFamily.IsActive) continue;

                bool isChildElsewhere = false;
                foreach (var other in allFams)
                {
                    if (other == fam) continue;
                    // 다른 가구의 자녀 목록에 현재 가구의 가주(Father)나 배우자(Mother)가 포함되어 있는지 확인
                    if ((fam.Father != null && other.Children.Contains(fam.Father)) || 
                        (fam.Mother != null && other.Children.Contains(fam.Mother)))
                    {
                        isChildElsewhere = true;
                        break;
                    }
                }

                if (!isChildElsewhere)
                {
                    if (fam.Father == null && fam.Mother == null && fam.Children.Count == 0) continue;
                    rootFams.Add(fam);
                }
            }

            StringBuilder treeSb = new StringBuilder();
            foreach (var root in rootFams)
            {
                // [수정] 무조건 1을 주는 대신, 가주가 가진 실제 세대(Generation) 값을 시작점으로 사용
                int startGen = root.Father?.Generation ?? 1;
                BuildNodeHtml(treeSb, root, startGen, "", allFams, visited);
            }
            
            AddHtml(20, 170, 590, 440, treeSb.ToString(), true, true);

            // ---------------- [우측: 선조 기록] ----------------
            AddLabel(630, 145, 53, "선조 기록 (Ancestor Records)"); // 53: 금색
            AddImageTiled(630, 165, 240, 2, 9651);

            StringBuilder ancSb = new StringBuilder();
            var ancestors = m_SelectedHouse.AncestorRecords.AsEnumerable().Reverse().ToList();
            
            if (ancestors.Count == 0) 
                ancSb.Append("<BASEFONT COLOR='#FFFFFF'><BR>기록된 선조가 없습니다.</BASEFONT>"); // 검은색 방지 위해 흰색 지정
            else
            {
                foreach (var anc in ancestors)
                {
                    ancSb.Append($"<BASEFONT COLOR='#FFD700'>{anc.Name}</BASEFONT> <BASEFONT COLOR='#FFFFFF'>({anc.HighestRank})</BASEFONT><BR>");
                    ancSb.Append($"<BASEFONT COLOR='#FFFFFF'>사인: {anc.CauseOfDeath}</BASEFONT><BR>");
                    ancSb.Append($"<BASEFONT COLOR='#BBBBBB'>향년: {anc.DeathAge}세</BASEFONT><BR><BR>");
                }
            }
            
            AddHtml(630, 170, 240, 440, ancSb.ToString(), true, true);
        }

        // [함께 수정] BuildNodeHtml 메서드에서도 세대 증가 로직 확인
        private void BuildNodeHtml(StringBuilder sb, FamilyUnit fam, int gen, string prefix, List<FamilyUnit> allFams, HashSet<FamilyUnit> visited)
        {
            if (!visited.Add(fam)) return;

            string headName = fam.Father?.Name ?? fam.Mother?.Name ?? "알 수 없음";
            string spouseName = fam.Father != null ? (fam.Mother?.Name ?? "없음") : "없음";

            // HTML 태그 내의 색상도 금색(#FFD700)과 흰색(#FFFFFF)으로 지정
            sb.Append($"{prefix}<BASEFONT COLOR='#FFD700'>▣ [{gen}대] {headName}</BASEFONT>");
            if (spouseName != "없음") sb.Append($" <BASEFONT COLOR='#FFFFFF'>/ 배우자: {spouseName}</BASEFONT>");
            sb.Append($" <BASEFONT COLOR='#00FF00'>[{fam.SharedWealth:N0} gp]</BASEFONT><BR>");

            string childPrefix = prefix + "　　"; 

            foreach (var child in fam.Children)
            {
                if (child == null) continue;

                var indFam = allFams.FirstOrDefault(f => f.Father == child || f.Mother == child);
                
                if (indFam != null)
                {
                    // 독립 가구가 있다면 세대를 하나 올려서(gen + 1) 재귀 호출
                    BuildNodeHtml(sb, indFam, gen + 1, childPrefix + "└─ ", allFams, visited);
                }
                else
                {
                    // 미혼/미성년 자녀 표시 (흰색 적용)
                    string statusColor = child.IsExpired ? "#FF5555" : "#FFFFFF";
                    sb.Append($"{childPrefix}└─ <BASEFONT COLOR='{statusColor}'>◈ [{gen+1}대] {child.Name}</BASEFONT> <BR>");
                }
            }
        }

        private void DrawTerritoryMapPage()
        {
            var grid = TownNumber.GetGridInfo(m_Town.TownID);
            int side = grid.W; 
            int currentMapSize = m_Town.TerritoryMap?.Length ?? 0;

            if (currentMapSize <= 0 || grid.Total <= 0) return;

            int currentPrice = m_Town.CurrentTilePrice;

            AddImageTiled(20, 85, 860, 30, 9354);
            AddHtml(30, 90, 840, 20, $"<CENTER><BASEFONT COLOR='#FFFFFF'>가상 영토: <BASEFONT COLOR='#00FF00'>{currentMapSize:N0}</BASEFONT> 칸 | 지가: <BASEFONT COLOR='#FFFF00'>{currentPrice:N0}</BASEFONT> GP</BASEFONT></CENTER>", false, false);

            AddImageTiled(25, 120, 850, 460, 9274); 
            AddAlphaRegion(25, 120, 850, 460);

            StringBuilder sb = new();
            sb.Append("<CENTER><BASEFONT SIZE='7' FACE='Courier'>"); 

            string lastColor = string.Empty;
            for (int i = 0; i < currentMapSize; i++)
            {
                if (i > 0 && i % side == 0) sb.Append("<BR>");
                string owner = m_Town.TerritoryMap[i];
                string currentColor = string.IsNullOrEmpty(owner) ? "#555555" : GetHouseColor(owner);
                string symbol = string.IsNullOrEmpty(owner) ? "□" : "■";

                if (currentColor != lastColor)
                {
                    if (!string.IsNullOrEmpty(lastColor)) sb.Append("</BASEFONT>");
                    sb.Append($"<BASEFONT COLOR='{currentColor}'>");
                    lastColor = currentColor;
                }
                sb.Append(symbol);
            }
            if (!string.IsNullOrEmpty(lastColor)) sb.Append("</BASEFONT>");
            sb.Append("</BASEFONT></CENTER>");

            AddHtml(30, 130, 840, 440, sb.ToString(), true, true);
            AddLabel(320, 590, 1152, "□ : 국유지 (Empty)  |  ■ : 가문 점유지 (Occupied)");
        }

        private string GetHouseColor(string houseName)
        {
            if (string.IsNullOrEmpty(houseName)) return "#555555";
            int hash = houseName.GetHashCode();
            int r = 130 + (Math.Abs(hash % 125));
            int g = 130 + (Math.Abs((hash / 100) % 125));
            int b = 130 + (Math.Abs((hash / 10000) % 125));
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        private void DrawWarehousePage()
        {
            AddHtml(20, 90, 860, 25, $"<CENTER><BASEFONT SIZE='5' COLOR='#00FFFF'>◆ {m_Town.TownName} 마을 공용 창고 현황 ◆</BASEFONT></CENTER>", false, false);
            
            AddLabel(50, 130, 1152, $"마을 자본금: {m_Town.Wealth:N0} gp");
            AddLabel(350, 130, 1152, $"세금 보관함: {m_Town.TaxFund:N0} gp");
            AddLabel(700, 130, 1152, $"[{DateTime.Now:HH:mm}]");

            AddLabel(50, 170, 53, "물품 종류");
            AddLabel(300, 170, 53, "현재 재고량");
            AddLabel(500, 170, 53, "목표 수요량");
            AddLabel(700, 170, 53, "현재 기준가");
            AddImageTiled(20, 190, 860, 2, 9651);

            int y = 200;
            var sortedWarehouse = m_Town.Warehouse.OrderByDescending(x => x.Value.Stock).ToList();

            foreach (var kvp in sortedWarehouse)
            {
                var data = kvp.Value;
                int stockColor = data.Stock < (data.TargetStock * 0.3) ? 33 : 63; 

                AddLabel(50, y, 1152, kvp.Key.Name);
                AddLabel(300, y, stockColor, $"{data.Stock:N0}");
                AddLabel(500, y, 1152, $"{data.TargetStock:N0}");
                AddLabel(700, y, 65, $"{data.BasePrice:N0} gp");

                y += 25;
                if (y > 580) break;
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile m = sender.Mobile;
            int buttonID = info.ButtonID;

            if (buttonID == 0)
            {
                if (m_ReturnMapIdx != -1)
                    m_From.SendGump(new EconomyCitizenMainGump(m_From, m_Town, m_ReturnMapIdx, m_ReturnTPage));
                return;
            }
            
            if (buttonID == 1) m_Page = Math.Max(0, m_Page - 1);
            else if (buttonID == 2) m_Page++;
            
            // 탭 버튼 처리
            else if (buttonID == 10) m_Tab = 0; // 가문 랭킹 (또는 족보에서 돌아가기)
            else if (buttonID == 12) m_Tab = 2; // 영토 지도
            else if (buttonID == 13) m_Tab = 3; // 마을 창고
            
            // 족보 상세 보기 진입 처리
            else if (buttonID >= 1000 && buttonID < 2000)
            {
                int idx = buttonID - 1000;
                if (idx >= 0 && idx < m_Town.Houses.Count)
                {
                    m_SelectedHouse = m_Town.Houses[idx];
                    m_Tab = 1; // 1번 탭(족보)으로 강제 전환
                }
            }

            m_From.SendGump(new TownSocietyGump(m_From, m_Town, m_Tab, m_Page, m_SelectedHouse, m_ReturnMapIdx, m_ReturnTPage));
        }
    }
}