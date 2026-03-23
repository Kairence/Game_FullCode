using System;
using System.Collections.Generic;
using System.Linq;
using Server.Network;
using Server.Gumps;
using Server.Misc;
using Server.Commands;

namespace Server.Misc
{
    // ==========================================
    // 1. 테스트 명령어 (기존 유지)
    // ==========================================
    public class EconomyTestCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("EcoTick", AccessLevel.GameMaster, new CommandEventHandler(EcoTick_OnCommand));
        }

        [Usage("EcoTick [시간]")]
        private static void EcoTick_OnCommand(CommandEventArgs e)
        {
            int targetHour = e.Length >= 1 ? e.GetInt32(0) : 18; 
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
            AddBackground(0, 0, 500, 550, 9270);
            AddAlphaRegion(10, 10, 480, 530);

            // 통계 화면 그리기
            AddHtml(10, 20, 480, 25, $"<CENTER><BASEFONT SIZE='6' COLOR='#68FF68'>[{m_Town.Name}] 인구 통계</BASEFONT></CENTER>", false, false);
            AddButton(20, 20, 4014, 4016, 999, GumpButtonType.Reply, 0); // 인벤토리로 복귀

            var citizens = m_Town.Citizens ??= new List<VirtualCitizen>();
            int totalPop = citizens.Count;
            
            // 상단 요약 정보
            AddImageTiled(20, 70, 460, 60, 9354);
            AddLabel(40, 80, 1152, $"총 인구 수: {totalPop:N0} 명");
            AddLabel(40, 105, 68, $"마을 자산: {m_Town.TotalWealthString}");

            // 직업군별 버튼 리스트
            int y = 150;
            AddLabel(25, y, 53, "직업군 (Job Group)");
            AddLabel(300, y, 53, "인구");
            AddLabel(400, y, 53, "목록");

            int[] groups = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100 };
            foreach (int groupID in groups)
            {
                y += 28;
                int count = citizens.Count(c => ((int)c.JobClass / 100) * 100 == groupID);
                AddImageTiled(20, y, 460, 24, 9304);
                AddLabel(35, y + 2, 1152, $"[{groupID}] {GetGroupName(groupID)}");
                AddLabel(300, y + 2, count > 0 ? 68 : 0x384, $"{count} 명");

                if (count > 0) // 인구가 있는 그룹만 상세 리스트 버튼 활성화
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
            if (id == 999) m_From.SendGump(new EconomyAdminGump(m_From, m_MapIndex, m_Town.TownID, m_TPage, 0));
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
            AddBackground(0, 0, 800, 600, 9270);
            AddAlphaRegion(10, 10, 780, 580);
            DrawCitizenList();
        }

        private void DrawCitizenList()
        {
            // [결합 핵심] 전체 리스트 대신 선택된 그룹만 필터링하여 출력
            var list = m_Town.Citizens.Where(c => ((int)c.JobClass / 100) * 100 == m_FilterGroup).ToList();

            AddHtml(10, 15, 780, 25, $"<CENTER><BASEFONT COLOR='#68FF68' SIZE='6'>[{m_Town.Name}] {m_FilterGroup}그룹 상세 리스트</BASEFONT></CENTER>", false, false);
            AddButton(20, 15, 4014, 4016, 999, GumpButtonType.Reply, 0); 
            AddLabel(55, 15, 1152, "통계 대시보드로");

            int y = 80;
            AddLabel(35, y, 53, "이름 (작위)");
            AddLabel(180, y, 53, "직업 (계급)");
            AddLabel(340, y, 53, "나이");
            AddLabel(420, y, 53, "자산(Gold)");
            AddLabel(530, y, 53, "스트레스/만족");
            AddLabel(680, y, 53, "관리");

            int start = m_CPage * 15;
            int end = Math.Min(start + 15, list.Count);

            for (int i = start; i < end; i++)
            {
                y += 26;
                var c = list[i];
                AddImageTiled(20, y, 760, 24, 9354);
                
                AddLabel(35, y + 2, 1152, $"{c.Name} ({c.RankLevel})");
                AddLabel(180, y + 2, 1152, $"{c.JobClass} ({c.Rank})");
                AddLabel(340, y + 2, c.Age > c.MaxLifespan.TotalMinutes ? 33 : 1152, $"{c.Age}분");
                AddLabel(420, y + 2, 68, $"{c.Gold:N0}");
                AddLabel(530, y + 2, c.Stress > 80 ? 33 : 1152, $"스트: {c.Stress} / 만족: {c.Satisfaction}");

                // [중요] 수정/삭제 시 원본 리스트(Citizens)에서의 실제 인덱스를 전달
                int realIndex = m_Town.Citizens.IndexOf(c);
                AddButton(680, y + 2, 2117, 2118, 20000 + realIndex, GumpButtonType.Reply, 0);
                AddButton(710, y + 2, 4002, 4004, 30000 + realIndex, GumpButtonType.Reply, 0);
            }

            // 페이징 로직 (필터링된 리스트 기준)
            if (m_CPage > 0) AddButton(350, 555, 4014, 4016, 997, GumpButtonType.Reply, 0);
            AddLabel(390, 549, 1152, $"{m_CPage + 1} / {Math.Max(1, (list.Count - 1) / 15 + 1)}");
            if (end < list.Count) AddButton(440, 555, 4005, 4007, 998, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            int id = info.ButtonID;
            if (id == 999) m_From.SendGump(new EconomyCitizenMainGump(m_From, m_Town, m_MapIndex, m_TPage));
            else if (id == 997) { m_CPage--; Refresh(); }
            else if (id == 998) { m_CPage++; Refresh(); }
            else if (id >= 30000) // 삭제
            {
                int idx = id - 30000;
                if (idx >= 0 && idx < m_Town.Citizens.Count) m_Town.Citizens.RemoveAt(idx);
                Refresh();
            }
            else if (id >= 20000) // 수정 창 열기 (필터 그룹 정보 추가 전달)
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

        // 생성자에 m_FilterGroup을 추가하여 돌아갈 리스트를 기억하게 함
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
            // 수정 후 원래 보고 있던 그룹의 상세 리스트로 복귀
            m_From.SendGump(new EconomyCitizenListGump(m_From, m_Town, m_MapIndex, m_TPage, m_FilterGroup, m_CPage));
        }
    }
}