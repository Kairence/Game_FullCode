using System;
using System.Collections.Generic;
using System.Linq;
using Server.Network;
using Server.Gumps;
using Server.Misc;
using Server.Commands;
using Server.Items;

namespace Server.Misc
{
public class EconomyTestCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("EcoTick", AccessLevel.GameMaster, new CommandEventHandler(EcoTick_OnCommand));
        }

        [Usage("EcoTick [시간] 또는 [EcoTick build]")]
        private static void EcoTick_OnCommand(CommandEventArgs e)
        {
            // 입력값이 없으면 18, 문자열이 들어오면 소문자로 변환
            string arg = e.Length >= 1 ? e.GetString(0).ToLower() : "18";

            // [신규] 타임랩스 촬영용 건축 강제 트리거
            if (arg == "build")
            {
                int townCount = 0;
                foreach (var town in TownEconomyManager.Towns.Values)
                {
                    // 🌟 자정에 실행되는 상속, 세금 정산 등은 전부 무시하고 
                    // 오직 '집 지을 돈과 야망이 있는지 검사하고 공사 시작'하는 함수만 단독 실행!
                    TownSocietyEngine.ProcessPhysicalHousingAndInvestment(town);
                    townCount++;
                }
                e.Mobile.SendMessage(0x42, $"[촬영용] {townCount}개 마을의 부동산 건축 심사를 강제로 실행했습니다! (조건 충족 시 즉시 공사 시작)");
                return;
            }

            //  [기존] 강제 시간 틱 루틴
            if (int.TryParse(arg, out int targetHour))
            {
                int count = 0;
                foreach (var town in TownEconomyManager.Towns.Values)
                {
                    if (town.Citizens != null)
                    {
                        foreach (var c in town.Citizens)
                        {
                            VirtualCitizenAI.ProcessQuarterlyRoutine(c, town, targetHour);
                            count++;
                        }
                    }
                }
                e.Mobile.SendMessage(68, $"강제 틱({targetHour}시 루틴) 실행 완료! 총 {count}명의 시민이 행동했습니다.");
            }
            else
            {
                e.Mobile.SendMessage(0x22, "잘못된 입력입니다. 숫자(시간) 또는 'build'를 입력하세요.");
            }
        }
    }

    // ==========================================
    // [신규] 글로벌 경제 & 생존 모니터링 명령어
    // ==========================================
    public class EconomyMonitorCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("EcoStatus", AccessLevel.GameMaster, EcoStatus_OnCommand);
        }

        private static void EcoStatus_OnCommand(CommandEventArgs e)
        {
            Mobile gm = e.Mobile;
            gm.SendMessage(88, $"=== [글로벌 경제 & 생존 리포트] ({DateTime.Now:HH:mm:ss}) ===");

            foreach (var town in TownEconomyManager.Towns.Values)
            {
                if (town.Citizens == null || town.Citizens.Count == 0) continue;
                PrintTownReport(gm, town);
            }
        }

        // UI 버튼에서도 호출할 수 있도록 분리된 튜플 기반 출력 메서드
        public static void PrintTownReport(Mobile gm, TownEconomy town)
        {
            var stats = AnalyzeTown(town);

            gm.SendMessage(1152, $"[{town.TownName}] 인구: {stats.Population:N0}명 | 자산: {town.Wealth:N0} GP");
            gm.SendMessage(2119, $"  ▶ 갈증: {stats.AvgThirst:F0} | 허기: {stats.AvgHunger:F0} (평균/10만)");
            gm.SendMessage(33,   $"  ▶ [위험] 탈수: {stats.Dehydrated}명 | 아사: {stats.Starving}명 | 과로: {stats.HighStress}명");
            gm.SendMessage(68,   $"  ▶ [재고] 식수(병/통): {stats.WaterCount}개 | 잉갓: {stats.IngotCount}개 | 원목: {stats.LogCount}개");
            gm.SendMessage(88, "-----------------------------------");
        }

        private static (
            int Population, double AvgThirst, double AvgHunger, 
            int Dehydrated, int Starving, int HighStress,
            int WaterCount, int IngotCount, int LogCount
        ) AnalyzeTown(TownEconomy town)
        {
            var citizens = town.Citizens;
            int pop = citizens.Count;

            double avgThirst = citizens.Average(x => x.Thirst);
            double avgHunger = citizens.Average(x => x.Hunger);

            int dehyd = citizens.Count(x => x.IsDehydrated || x.Thirst < 10000);
            int starve = citizens.Count(x => x.IsStarving || x.Hunger < 10000);
            int overstressed = citizens.Count(x => x.Stress >= 80);

            int water = GetStock(town, typeof(BeverageBottle)) + GetStock(town, typeof(Pitcher));
            int ingot = GetStock(town, typeof(IronIngot));
            int log = GetStock(town, typeof(Log));

            return (pop, avgThirst, avgHunger, dehyd, starve, overstressed, water, ingot, log);
        }

        private static int GetStock(TownEconomy town, Type itemType)
        {
            return town.Warehouse.ContainsKey(itemType) ? town.Warehouse[itemType].Stock : 0;
        }
    }

    // ==========================================
    // 2. [1단계] 시민 관리 메인 대시보드 (통계창)
    // ==========================================
    public class EconomyCitizenMainGump : Gump
    {
        private Mobile m_From;
        private TownEconomy m_Town;
        private int m_MapIndex;
        private int m_TPage;

        public EconomyCitizenMainGump(Mobile from, TownEconomy town, int mapIndex, int tPage) : base(50, 50)
        {
            m_From = from; m_Town = town; m_MapIndex = mapIndex; m_TPage = tPage;

            from.CloseGump(typeof(EconomyCitizenMainGump));
            from.CloseGump(typeof(EconomyCitizenListGump));

            AddPage(0);
            AddBackground(0, 0, 500, 580, 9270);
            AddAlphaRegion(10, 10, 480, 560);

            AddHtml(10, 20, 480, 25, $"<CENTER><BASEFONT SIZE='6' COLOR='#68FF68'>[{m_Town.Name}] 인구 및 노동 통계</BASEFONT></CENTER>", false, false);
            AddButton(20, 20, 4014, 4016, 999, GumpButtonType.Reply, 0); 

            var citizens = m_Town.Citizens ??= new List<VirtualCitizen>();
            int totalPop = citizens.Count;
            
            int childCount = citizens.Count(c => c.IsChild);        
            int productiveCount = citizens.Count(c => c.IsProductive); 
            int elderCount = citizens.Count(c => c.IsElder);        
            double avgAge = totalPop > 0 ? citizens.Average(c => c.Age) : 0; 
            
            AddImageTiled(20, 70, 460, 100, 9354);
            AddLabel(40, 80, 1152, $"총 인구 수: {totalPop:N0} 명 (평균 {avgAge:F1}세)");
            AddLabel(40, 105, 68, $"마을 자산: {m_Town.TotalWealthString}");
            
            AddLabel(40, 130, 53, $"인구 구성: ");
            AddLabel(110, 130, 1152, $"유년 {childCount}"); 
            AddLabel(180, 130, 68, $"생산 {productiveCount}"); 
            AddLabel(250, 130, 33, $"노년 {elderCount}");

            // [신규 연동] EcoStatus 즉시 출력 버튼 추가
            AddButton(350, 133, 4005, 4007, 888, GumpButtonType.Reply, 0);
            AddLabel(385, 134, 88, "EcoStatus 출력");
			
			// [수정] 버튼들을 한 줄 아래(y=155)로 이동하여 겹침 방지
			AddButton(40, 155, 4005, 4007, 777, GumpButtonType.Reply, 0);
			AddLabel(75, 156, 1152, "가문/영토 현황");

            int y = 180;
            AddLabel(25, y, 53, "직업군 (Job Group)");
            AddLabel(300, y, 53, "인구");
            AddLabel(400, y, 53, "상세");

            int[] groups = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100 };
            foreach (int groupID in groups)
            {
                y += 28;
                int count = citizens.Count(c => ((int)c.JobClass / 100) * 100 == groupID);
                AddImageTiled(20, y, 460, 24, 9304);
                AddLabel(35, y + 2, 1152, $"[{groupID}] {GetGroupName(groupID)}");
                
                AddLabel(300, y + 2, count > 0 ? 68 : 0x384, $"{count} 명");

                if (count > 0) 
                    AddButton(410, y + 2, 4005, 4007, 2000 + groupID, GumpButtonType.Reply, 0);
            }
        }

        private string GetGroupName(int id) => id switch {
            100 => "노동자", 200 => "생산자", 300 => "전사", 400 => "마법사",
            500 => "귀족", 600 => "상인", 700 => "종교인", 800 => "예능인",
            900 => "해양인", 1000 => "학자", 1100 => "범죄자", _ => "기타"
        };

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            int id = info.ButtonID;
            if (id == 999) 
                m_From.SendGump(new EconomyAdminGump(m_From, m_MapIndex, m_Town.TownID, m_TPage, 0));
            else if (id == 888) // [신규 연동] 리포트 출력 후 창 유지
            {
                EconomyMonitorCommand.PrintTownReport(m_From, m_Town);
                m_From.SendGump(new EconomyCitizenMainGump(m_From, m_Town, m_MapIndex, m_TPage));
            }
			else if (id == 777)
			{
				// 돌아올 때 필요한 정보(m_MapIndex, m_TPage)를 같이 넘겨주도록 파라미터를 추가할 예정입니다.
				m_From.SendGump(new TownSocietyGump(m_From, m_Town, 0, 0, null, m_MapIndex, m_TPage));
			}
            else if (id >= 2000)
            {
                int groupID = id - 2000;
                m_From.SendGump(new EconomyCitizenListGump(m_From, m_Town, m_MapIndex, m_TPage, groupID, 0));
            }
        }
    }

    // ==========================================
    // 3. [2단계] 상세 리스트 (필터링된 목록)
    // ==========================================
    public class EconomyCitizenListGump : Gump
    {
        private Mobile m_From;
        private TownEconomy m_Town;
        private int m_MapIndex, m_TPage, m_FilterGroup, m_CPage;

        public EconomyCitizenListGump(Mobile f, TownEconomy t, int mapIdx, int tp, int group, int cp) : base(50, 50)
        {
            m_From = f; m_Town = t; m_MapIndex = mapIdx; m_TPage = tp; m_FilterGroup = group; m_CPage = cp;
            
            f.CloseGump(typeof(EconomyCitizenListGump));
            AddPage(0);
            AddBackground(0, 0, 900, 600, 9270);
            AddAlphaRegion(10, 10, 880, 580);
            DrawCitizenList();
        }

        private void DrawCitizenList()
        {
            var list = m_Town.Citizens.Where(c => ((int)c.JobClass / 100) * 100 == m_FilterGroup).ToList();

            AddHtml(10, 15, 880, 25, $"<CENTER><BASEFONT COLOR='#68FF68' SIZE='6'>[{m_Town.Name}] {m_FilterGroup}그룹 상세 리스트</BASEFONT></CENTER>", false, false);
            AddButton(20, 15, 4014, 4016, 999, GumpButtonType.Reply, 0); 
            AddLabel(55, 15, 1152, "통계 대시보드로");

            int y = 80;
            AddLabel(35, y, 53, "이름");
            AddLabel(160, y, 53, "직업 (계급)");
            AddLabel(440, y, 53, "나이 (현재/수명)"); 
            AddLabel(580, y, 53, "자산 (GP)"); 
            AddLabel(700, y, 53, "스트레스/만족");
            AddLabel(820, y, 53, "관리");

            int start = m_CPage * 15;
            int end = Math.Min(start + 15, list.Count);

            for (int i = start; i < end; i++)
            {
                y += 26;
                var c = list[i];
                AddImageTiled(20, y, 860, 24, 9354);
                
                AddLabel(35, y + 2, 1152, $"{c.Name}"); 
                AddLabel(160, y + 2, 1152, $"{c.JobClass} ({c.Rank})");

                double gameAge = c.Age; 
                double maxGameAge = c.MaxLifespan.TotalMinutes / 1440.0; 

                int ageColor = (c.Age >= maxGameAge) ? 33 : (c.Age >= maxGameAge * 0.9 ? 53 : 1152);
                AddLabel(440, y + 2, ageColor, $"{gameAge:F1} / {maxGameAge:F1} 세");

                AddLabel(580, y + 2, 68, $"{c.Gold:N0}");
                AddLabel(700, y + 2, c.Stress > 80 ? 33 : 1152, $"{c.Stress} / {c.Satisfaction}");

                int realIndex = m_Town.Citizens.IndexOf(c);
                AddButton(820, y + 2, 2117, 2118, 20000 + realIndex, GumpButtonType.Reply, 0);
                AddButton(850, y + 2, 4002, 4004, 30000 + realIndex, GumpButtonType.Reply, 0);
            }

            if (m_CPage > 0) AddButton(400, 555, 4014, 4016, 997, GumpButtonType.Reply, 0);
            AddLabel(440, 549, 1152, $"{m_CPage + 1} / {Math.Max(1, (list.Count - 1) / 15 + 1)}");
            if (end < list.Count) AddButton(490, 555, 4005, 4007, 998, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            int id = info.ButtonID;
            if (id == 999) m_From.SendGump(new EconomyCitizenMainGump(m_From, m_Town, m_MapIndex, m_TPage));
            else if (id == 997) { m_CPage--; Refresh(); }
            else if (id == 998) { m_CPage++; Refresh(); }
            else if (id >= 30000) 
            {
                int idx = id - 30000;
                if (idx >= 0 && idx < m_Town.Citizens.Count) m_Town.Citizens.RemoveAt(idx);
                Refresh();
            }
            else if (id >= 20000) 
            {
                int idx = id - 20000;
                if (idx >= 0 && idx < m_Town.Citizens.Count)
                    m_From.SendGump(new EconomyCitizenEditGump(m_From, m_Town, m_Town.Citizens[idx], m_MapIndex, m_TPage, m_CPage, m_FilterGroup));
            }
        }
        private void Refresh() => m_From.SendGump(new EconomyCitizenListGump(m_From, m_Town, m_MapIndex, m_TPage, m_FilterGroup, m_CPage));
    }

    // ==========================================
    // 4. [수정] 개별 시민 데이터 수정 창
    // ==========================================
    public class EconomyCitizenEditGump : Gump
    {
        private Mobile m_From; private TownEconomy m_Town; private VirtualCitizen m_Citizen;
        private int m_MapIndex, m_TPage, m_CPage, m_FilterGroup;

        public EconomyCitizenEditGump(Mobile f, TownEconomy t, VirtualCitizen c, int mapIdx, int tp, int cp, int group) : base(300, 300)
        {
            m_From = f; m_Town = t; m_Citizen = c; m_MapIndex = mapIdx; m_TPage = tp; m_CPage = cp; m_FilterGroup = group;
            
            AddBackground(0, 0, 300, 260, 9270);
            AddHtml(0, 15, 300, 20, $"<CENTER><BASEFONT COLOR=#FDB913>시민 수정: {c.Name}</BASEFONT></CENTER>", false, false);
            
            AddLabel(30, 60, 1152, "보유 Gold:"); AddBackground(120, 55, 100, 25, 9300);
            AddTextEntry(125, 58, 90, 20, 0, 1, c.Gold.ToString());
            
            AddLabel(30, 100, 1152, "만족도:"); AddBackground(120, 95, 100, 25, 9300);
            AddTextEntry(125, 98, 90, 20, 0, 2, c.Satisfaction.ToString());

            AddLabel(30, 140, 1152, "스트레스:"); AddBackground(120, 135, 100, 25, 9300);
            AddTextEntry(125, 138, 90, 20, 0, 3, c.Stress.ToString());

            AddButton(60, 200, 2128, 2129, 1, GumpButtonType.Reply, 0); 
            AddButton(170, 200, 2119, 2120, 0, GumpButtonType.Reply, 0); 
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1)
            {
                m_Citizen.Gold = Math.Max(0, Utility.ToInt32(info.GetTextEntry(1).Text));
                m_Citizen.Satisfaction = Math.Clamp(Utility.ToInt32(info.GetTextEntry(2).Text), 0, 100);
                m_Citizen.Stress = Math.Clamp(Utility.ToInt32(info.GetTextEntry(3).Text), 0, 100);
                m_From.SendMessage(68, $"{m_Citizen.Name} 수정 완료.");
            }
            m_From.SendGump(new EconomyCitizenListGump(m_From, m_Town, m_MapIndex, m_TPage, m_FilterGroup, m_CPage));
        }
    }
}