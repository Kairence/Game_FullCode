using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Gumps;
using Server.Network;

namespace Server.Misc
{
    // ==========================================
    // [통합] 모험가 대시보드 & 상세 상태창
    // ==========================================
    public class EconomyAdventurerMainGump : Gump
    {
        private Mobile m_From;
        private int m_MapIndex;
        private int m_TPage;
        private int m_Tab;  // 0: 활동 파티, 1: 대기 모험가, 2: 개별 상세창
        private int m_Page;
        private VirtualAdventurer m_SelectedAdv; // 선택된 모험가

        private static Map[] Facets = [Map.Trammel, Map.Felucca, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur];

        public EconomyAdventurerMainGump(Mobile from, int mapIdx, int tPage, int tab, int page, VirtualAdventurer selectedAdv = null) : base(50, 50)
        {
            m_From = from; m_MapIndex = mapIdx; m_TPage = tPage; m_Tab = tab; m_Page = page; m_SelectedAdv = selectedAdv;
            from.CloseGump(typeof(EconomyAdventurerMainGump));
            
            AddPage(0);
            AddBackground(0, 0, 900, 640, 9270);
            AddAlphaRegion(10, 10, 880, 620);

            Map targetMap = Facets[m_MapIndex];

            // --- 상단 헤더 및 탭 메뉴 ---
            AddHtml(0, 15, 900, 25, $"<CENTER><BASEFONT SIZE='6' COLOR='#FDB913'>[{targetMap.Name}] 가상 모험가 시스템</BASEFONT></CENTER>", false, false);
            
            if (m_Tab == 2) // 상세창 모드일 때
            {
                AddButton(20, 50, 4014, 4016, 11, GumpButtonType.Reply, 0); 
                AddLabel(55, 52, 1152, "◀ 목록으로 돌아가기");
            }
            else // 목록 모드일 때
            {
                AddButton(20, 15, 4014, 4016, 999, GumpButtonType.Reply, 0); 
                AddLabel(55, 15, 1152, "마을 관리로 귀환");

                string[] labels = ["활동 중인 파티", "대기 중인 모험가"];
                int startX = (900 - 300) / 2;
                for (int i = 0; i < 2; i++)
                {
                    AddButton(startX + (i * 150), 50, m_Tab == i ? 4006 : 4005, 4007, 10 + i, GumpButtonType.Reply, 0);
                    AddLabel(startX + 35 + (i * 150), 52, m_Tab == i ? 1152 : 898, labels[i]);
                }
            }
            
            AddImageTiled(20, 80, 860, 2, 9651);

            // 탭에 따른 화면 분기
            if (m_Tab == 0) DrawActiveParties(targetMap);
            else if (m_Tab == 1) DrawIdleAdventurers(targetMap);
            else if (m_Tab == 2) DrawAdventurerDetail();
        }

        private void DrawActiveParties(Map targetMap)
        {
            var parties = VirtualAdventurerManager.ActiveParties.Where(p => p.CurrentNode.NodeMap == targetMap).ToList();
            
            AddLabel(30, 90, 53, "현재 상태");
            AddLabel(130, 90, 53, "위치 (출발/도착)");
            AddLabel(380, 90, 53, "전투력");
            AddLabel(480, 90, 53, "인원수 (셰르파)");
            AddLabel(630, 90, 53, "유대감");
            AddLabel(730, 90, 53, "남은 시간/상세");

            int start = m_Page * 15;
            int end = Math.Min(start + 15, parties.Count);
            int y = 120;

            for (int i = start; i < end; i++)
            {
                var p = parties[i];
                string stateStr = p.State switch {
                    AdventurerState.Resting => "<BASEFONT COLOR='#68FF68'>휴식/정비</BASEFONT>",
                    AdventurerState.Traveling => "<BASEFONT COLOR='#FDB913'>이동 중</BASEFONT>",
                    AdventurerState.Exploring => "<BASEFONT COLOR='#FF5555'>던전 탐험</BASEFONT>",
                    _ => ""
                };

                AddHtml(30, y, 100, 20, stateStr, false, false);
                AddLabel(130, y, 1152, $"{p.CurrentNode?.Name ?? "알수없음"} -> {p.TargetNode?.Name ?? "대기"}");
                AddLabel(380, y, 1161, $"{p.GetTotalPower():N0}");
                
                string sherpa = p.EmployedSherpa != null ? " (셰르파 O)" : "";
                AddLabel(480, y, 1152, $"{p.Members.Count}명{sherpa}");
                AddLabel(630, y, p.CalculatePartyUnity() > 70 ? 68 : 33, $"{p.CalculatePartyUnity()}/100");
                AddLabel(730, y, 1152, p.State == AdventurerState.Traveling ? $"{p.TravelHoursRemaining} 시간" : "-");

                y += 25;
            }
            DrawPaging(parties.Count);
        }

        private void DrawIdleAdventurers(Map targetMap)
        {
            var idles = VirtualAdventurerManager.IdleAdventurers.ToList();

            AddLabel(30, 90, 53, "이름");
            AddLabel(180, 90, 53, "직업 (신분)");
            AddLabel(380, 90, 53, "레벨 (경험치)");
            AddLabel(500, 90, 53, "보유 골드");
            AddLabel(650, 90, 53, "HP / 체력");
            AddLabel(800, 90, 53, "상세/수정");

            int start = m_Page * 15;
            int end = Math.Min(start + 15, idles.Count);
            int y = 120;

            for (int i = start; i < end; i++)
            {
                var a = idles[i];
                AddLabel(30, y, 1152, a.Name);
                AddLabel(180, y, 1152, $"{a.JobClass} ({a.RankLevel})");
                AddLabel(380, y, 68, $"Lv.{a.Level} ({a.Exp}/{a.GetRequiredExp()})");
                AddLabel(500, y, 53, $"{a.Gold:N0} GP");
                AddLabel(650, y, a.HP < a.MaxHP * 0.3 ? 33 : 1152, $"{a.HP} / {a.MaxHP}");

                // 상세 보기 버튼 (아이디: 1000 + 인덱스)
                int realIndex = VirtualAdventurerManager.IdleAdventurers.IndexOf(a);
                AddButton(810, y + 2, 2117, 2118, 1000 + realIndex, GumpButtonType.Reply, 0);

                y += 25;
            }
            DrawPaging(idles.Count);
        }

        private void DrawPaging(int totalCount)
        {
            int maxPage = Math.Max(0, (totalCount - 1) / 15);
            if (m_Page > 0) AddButton(400, 590, 4014, 4016, 1, GumpButtonType.Reply, 0);
            if (m_Page < maxPage) AddButton(490, 590, 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddLabel(440, 590, 1152, $"{m_Page + 1} / {maxPage + 1}");
        }

        // ==========================================
        // 🌟 탭 3: 모험가 디테일 화면 (캐릭터 상태창)
        // ==========================================
        private void DrawAdventurerDetail()
        {
            if (m_SelectedAdv == null) return;
            var a = m_SelectedAdv;

            AddHtml(0, 90, 900, 20, $"<CENTER><BASEFONT SIZE='6' COLOR=#00FF00>◆ [Lv.{a.Level}] {a.Name} 상세 스펙 ◆</BASEFONT></CENTER>", false, false);
            
            // 데이터 수정을 위한 버튼 (우측 상단)
            AddButton(780, 90, 2117, 2118, 888, GumpButtonType.Reply, 0);
            AddLabel(810, 90, 68, "수정 모드");

            // --- 1. 좌측 영역: 초상화 및 기본 정보 ---
            AddImageTiled(30, 130, 200, 200, 9304); // 페이퍼돌/초상화 대체 배경
            AddLabel(80, 140, 1152, "캐릭터 모델"); // 임시 텍스트
            
            AddLabel(40, 350, 53, $"직업: {a.JobClass} ({a.Role})");
            AddLabel(40, 375, 53, $"신분: {a.RankLevel}");
            AddLabel(40, 400, 53, $"잠재력: {a.Potential:F1} | 준비율: {a.PrepMultiplier:F1}");
            AddLabel(40, 425, 53, $"골드: {a.Gold:N0} gp");

            // --- 2. 중앙 영역: 스펙 & 게이지 (생존 / 인연 / 성향) ---
            int midX = 260;
            AddLabel(midX, 130, 68, "◆ 전투 & 생존 스펙 ◆");
            
            AddLabel(midX, 160, 1152, "HP:"); AddImageTiled(midX+40, 163, 180, 12, 9303);
            int hpW = a.MaxHP > 0 ? (int)(180.0 * a.HP / a.MaxHP) : 0;
            if (hpW > 0) AddImageTiled(midX+40, 163, Math.Min(180, hpW), 12, 11411);
            AddLabel(midX+230, 160, 1152, $"{a.HP} / {a.MaxHP}");

            AddLabel(midX, 185, 1152, "스트레스:"); AddImageTiled(midX+60, 188, 160, 12, 9303);
            int stressW = (int)(160.0 * a.Stress / 100);
            if (stressW > 0) AddImageTiled(midX+60, 188, Math.Min(160, stressW), 12, 11400);
            AddLabel(midX+230, 185, a.Stress > 70 ? 33 : 1152, $"{a.Stress} / 100");

            AddLabel(midX, 220, 1152, $"전투 스킬: {a.CombatSkill}");
            AddLabel(midX + 120, 220, 1152, $"캠핑 스킬: {a.CampingSkill}");
            AddLabel(midX, 245, 1152, $"Fame: {a.Fame:N0}   |   Karma: {a.Karma:N0}");

            AddLabel(midX, 285, 68, "◆ 성향 및 파티 정보 ◆");
            AddLabel(midX, 315, GetAlignColor(a.GoodEvilAlignment), $"성향 (善/惡): {a.GoodEvilAlignment}");
            AddLabel(midX, 340, GetAlignColor(a.LawChaosAlignment), $"성향 (질서/혼돈): {a.LawChaosAlignment}");

            AddLabel(midX, 375, 1152, "파티 유대감 (Affinity):"); AddImageTiled(midX, 398, 200, 10, 9303);
            int affW = (int)(200.0 * a.Affinity / 150);
            if (affW > 0) AddImageTiled(midX, 398, Math.Min(200, affW), 10, 11414);
            AddLabel(midX + 210, 395, 1161, $"{a.Affinity}");

            string petStr = a.Party != null ? (a.Party.EmployedSherpa != null ? "고용된 셰르파 동행" : "없음") : "없음 (대기 중)";
            AddLabel(midX, 430, 1152, $"보조/펫: {petStr}");

            // --- 3. 우측 영역: 장비 현황 (9슬롯) ---
            int eqX = 580;
            AddLabel(eqX + 70, 130, 68, "◆ 착용 장비 현황 ◆");

            DrawEquipSlot(eqX + 60, 160, Layer.Helm, "머리");
            DrawEquipSlot(eqX + 130, 160, Layer.Neck, "목걸이");
            
            DrawEquipSlot(eqX + 60, 220, Layer.InnerTorso, "갑옷");
            DrawEquipSlot(eqX + 130, 220, Layer.Arms, "팔");
            
            DrawEquipSlot(eqX + 60, 280, Layer.Gloves, "장갑");
            DrawEquipSlot(eqX + 130, 280, Layer.Pants, "다리");
            
            DrawEquipSlot(eqX + 60, 340, Layer.Shoes, "신발");
            
            DrawEquipSlot(eqX - 10, 220, Layer.OneHanded, "주무기");
            DrawEquipSlot(eqX + 200, 220, Layer.TwoHanded, "방패/보조");
        }

        private void DrawEquipSlot(int x, int y, Layer layer, string slotName)
        {
            // [수정] 박스 크기를 너비 55, 높이 45로 살짝 넓혀서 글씨가 잘 들어가게 조정
            AddImageTiled(x, y, 55, 45, 9270); 
            AddAlphaRegion(x + 2, y + 2, 51, 41);

            // [수정] 박스 상단에 금색으로 부위 이름(머리, 목 등) 표시
            AddLabel(x + 5, y + 2, 53, slotName);

            if (m_SelectedAdv.VirtualEquipments != null && m_SelectedAdv.VirtualEquipments.TryGetValue(layer, out Type itemType))
            {
                // [수정] CANCEL 버튼(2121) 그래픽 제거
                // 장비가 있으면 박스 하단에 녹색으로 장비명 표시
                string shortName = itemType.Name.Length <= 6 ? itemType.Name : itemType.Name.Substring(0, 6);
                AddLabel(x + 5, y + 20, 68, shortName);
            }
            else
            {
                // 장비가 없으면 박스 하단에 회색으로 표시
                AddLabel(x + 5, y + 20, 999, "비어있음");
            }
        }

        private int GetAlignColor(LawChaos a) => a == LawChaos.Lawful ? 68 : (a == LawChaos.Chaotic ? 33 : 1152);
        private int GetAlignColor(GoodEvil a) => a == GoodEvil.Good ? 68 : (a == GoodEvil.Evil ? 33 : 1152);

        // EconomyAdventurerMainGump 클래스 내부의 OnResponse 메서드 부분입니다.
        public override void OnResponse(NetState sender, RelayInfo info)
        {
            int id = info.ButtonID;
            if (id == 0 || id == 999) 
            {
                m_From.SendGump(new EconomyAdminGump(m_From, m_MapIndex, 0, m_TPage, 0));
                return;
            }

            if (id == 1) m_Page--;
            else if (id == 2) m_Page++;
            else if (id == 10) { m_Tab = 0; m_Page = 0; } // 파티 탭
            else if (id == 11) { m_Tab = 1; m_Page = 0; } // 대기 목록 탭
            else if (id == 888 && m_SelectedAdv != null) // 상세창 내 수정 버튼
            {
                m_From.SendGump(new EconomyAdventurerEditGump(m_From, m_SelectedAdv, m_MapIndex, m_TPage, m_Tab, m_Page));
                return;
            }
            else if (id >= 1000) // 상세 보기 클릭 시
            {
                int targetIdx = id - 1000; // 변수명이 targetIdx 입니다. target 오타 조심!
                if (targetIdx >= 0 && targetIdx < VirtualAdventurerManager.IdleAdventurers.Count)
                {
                    m_Tab = 2; // 상세창 탭으로 변경
                    m_SelectedAdv = VirtualAdventurerManager.IdleAdventurers[targetIdx];
                }
            }

            m_From.SendGump(new EconomyAdventurerMainGump(m_From, m_MapIndex, m_TPage, m_Tab, m_Page, m_SelectedAdv));
        }
    } // <--- 🚨 가장 중요: EconomyAdventurerMainGump 클래스를 닫는 중괄호입니다! 이게 빠지면 아래가 다 에러납니다.

    // ==========================================
    // 데이터 수정용 미니 Gump (수정 기능 독립)
    // ==========================================
    public class EconomyAdventurerEditGump : Gump
    {
        private Mobile m_From; 
        private VirtualAdventurer m_Adv;
        private int m_MapIndex, m_TPage, m_Tab, m_Page;

        public EconomyAdventurerEditGump(Mobile f, VirtualAdventurer a, int mapIdx, int tp, int tab, int page) : base(300, 300)
        {
            m_From = f; m_Adv = a; m_MapIndex = mapIdx; m_TPage = tp; m_Tab = tab; m_Page = page;
            
            AddBackground(0, 0, 300, 300, 9270);
            AddHtml(0, 15, 300, 20, $"<CENTER><BASEFONT COLOR=#FDB913>모험가 정보 수정</BASEFONT></CENTER>", false, false);
            
            AddLabel(30, 60, 1152, "보유 Gold:"); AddBackground(120, 55, 100, 25, 9300);
            AddTextEntry(125, 58, 90, 20, 0, 1, a.Gold.ToString());
            
            AddLabel(30, 100, 1152, "레벨(Level):"); AddBackground(120, 95, 100, 25, 9300);
            AddTextEntry(125, 98, 90, 20, 0, 2, a.Level.ToString());

            AddLabel(30, 140, 1152, "현재 HP:"); AddBackground(120, 135, 100, 25, 9300);
            AddTextEntry(125, 138, 90, 20, 0, 3, a.HP.ToString());

            AddLabel(30, 180, 1152, "스트레스:"); AddBackground(120, 175, 100, 25, 9300);
            AddTextEntry(125, 178, 90, 20, 0, 4, a.Stress.ToString());

            AddButton(60, 240, 2128, 2129, 1, GumpButtonType.Reply, 0); 
            AddButton(170, 240, 2119, 2120, 0, GumpButtonType.Reply, 0); 
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1)
            {
                m_Adv.Gold = Math.Max(0, Utility.ToInt32(info.GetTextEntry(1).Text));
                m_Adv.Level = Math.Clamp(Utility.ToInt32(info.GetTextEntry(2).Text), 1, 100);
                m_Adv.HP = Math.Clamp(Utility.ToInt32(info.GetTextEntry(3).Text), 0, m_Adv.MaxHP);
                m_Adv.Stress = Math.Clamp(Utility.ToInt32(info.GetTextEntry(4).Text), 0, 100);
                m_From.SendMessage(68, $"{m_Adv.Name} 수정 완료.");
            }
            m_From.SendGump(new EconomyAdventurerMainGump(m_From, m_MapIndex, m_TPage, m_Tab, m_Page, m_Adv));
        }
    }
} // <--- 네임스페이스를 닫는 마지막 중괄호