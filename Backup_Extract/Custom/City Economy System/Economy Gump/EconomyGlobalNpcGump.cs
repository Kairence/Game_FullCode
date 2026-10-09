using System;
using System.Linq;
using Server.Gumps;
using Server.Network;

namespace Server.Misc
{
    public class EconomyGlobalNpcGump : Gump
    {
        private Mobile m_From;
        private int m_MapIndex;
        private static Map[] Facets = { Map.Trammel, Map.Felucca, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur };

        public EconomyGlobalNpcGump(Mobile from, int mapIndex) : base(150, 150)
        {
            m_From = from;
            m_MapIndex = mapIndex;

            from.CloseGump(typeof(EconomyGlobalNpcGump));

            AddPage(0);
            // [수정] 배경 창의 높이를 360 -> 480으로 넉넉하게 늘려줍니다.
            AddBackground(0, 0, 420, 480, 9270); 
            AddAlphaRegion(10, 10, 400, 460);    

            DrawInterface();
        }

        private void DrawInterface()
        {
            Map targetMap = Facets[m_MapIndex];
            var towns = TownEconomyManager.Towns.Values.Where(t => t.Facet == targetMap).ToList();

            AddHtml(10, 20, 400, 25, $"<CENTER><BASEFONT SIZE='6' COLOR='#FDB913'>{targetMap.Name} 대륙 경제 & NPC 마스터</BASEFONT></CENTER>", false, false);

            int totalCitizens = towns.Sum(t => t.Citizens?.Count ?? 0);
            
            AddImageTiled(20, 60, 380, 50, 9354);
            AddLabel(40, 75, 1152, $"대륙 내 활성 마을: {towns.Count}개 / 총 인구: {totalCitizens:N0}명");

            int y = 130;
            
            AddButton(30, y, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddLabel(65, y + 2, 1152, "대륙 전체 NPC 리스폰 (상인 비례)");

            y += 40;
            AddButton(30, y, 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddLabel(65, y + 2, 33, "대륙 전체 NPC 데이터 삭제 (Clear)");

            y += 40;
            AddButton(30, y, 4005, 4007, 3, GumpButtonType.Reply, 0);
            AddLabel(65, y + 2, 1152, "대륙 경제 지표 강제 동기화 (Wealth)");

            // [신규 추천] 강제 시간 진행 (테스트용)
            y += 40;
            AddButton(30, y, 4005, 4007, 4, GumpButtonType.Reply, 0);
            AddLabel(65, y + 2, 65, "[테스트] 가상 경제 1 사이클 강제 진행");

            // [신규 추천] 일괄 상태 회복 (구제용)
            y += 40;
            AddButton(30, y, 4005, 4007, 5, GumpButtonType.Reply, 0);
            AddLabel(65, y + 2, 2100, "[구제] 전체 시민 상태(허기/스트레스) 회복");

            // [모험가] 추가 버튼들
            y += 40;
            AddButton(30, y, 4005, 4007, 6, GumpButtonType.Reply, 0);
            AddLabel(65, y + 2, 1161, "[모험가] 대륙 내 초기 스폰 (마을당 15명)");

            y += 40;
            AddButton(30, y, 4005, 4007, 7, GumpButtonType.Reply, 0);
            AddLabel(65, y + 2, 33, "[모험가] 대륙 내 모험가/파티 전체 데이터 삭제");

            // [수정] OKAY 버튼의 Y좌표를 315에서 420으로 시원하게 내려줍니다!
            AddButton(180, 420, 247, 248, 0, GumpButtonType.Reply, 0);
			
			AddButton(600, 20, 0x15E1, 0x15E5, 999, GumpButtonType.Reply, 0); 
			AddLabel(625, 19, 53, "거시 경제 대시보드 ▶");


        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Map targetMap = Facets[m_MapIndex];
            var towns = TownEconomyManager.Towns.Values.Where(t => t.Facet == targetMap).ToList();

            if (info.ButtonID == 0)
            {
                m_From.SendGump(new EconomyAdminGump(m_From, m_MapIndex, 0, 0, 0));
                return;
            }

            switch (info.ButtonID)
            {
                case 1: // 리스폰 (가상 시민만 대상)
                    // 물리적 노드는 건드리지 않고, TownDemographics를 통해 가상 시민 데이터만 재생성합니다.
                    TownDemographics.RespawnFacet(targetMap, m_From);
                    
                    // 마을 수와 현재 인구수를 요약해서 메시지로 보냅니다.
                    int totalNewPop = towns.Sum(t => t.Citizens?.Count ?? 0);
                    m_From.SendMessage(66, $"{targetMap.Name} 대륙의 가상 시민 {totalNewPop:N0}명이 새롭게 배치되었습니다.");
                    break;

                case 2: // 전체 삭제 (가상 시민 데이터 및 물리적 하우징 철거)
                    int demolishedCount = 0;

                    foreach (var town in towns) 
                    {
                        if (town.Houses != null)
                        {
                            // 🌟 [핵심 패치] 장부를 날리기 전에 물리적인 집부터 싹 다 철거합니다.
                            foreach (var house in town.Houses.ToList())
                            {
                                TownSocietyEngine.DemolishEstateArea(house, town);
                                demolishedCount++;
                            }
                            town.Houses.Clear(); // 물리적 철거가 끝났으니 장부도 초기화
                        }
                        
                        town.Citizens?.Clear();
                    }

                    m_From.SendMessage(33, $"{targetMap.Name} 대륙의 가상 시민 데이터 초기화 및 {demolishedCount}채의 집이 완벽하게 철거되었습니다.");
                    break;

                case 3: // 지표 동기화
                    foreach (var town in towns) { 
                        town.UpdateBaseWealth();
                        town.Wealth = town.BaseWealth;
                    }
                    m_From.SendMessage(68, $"{targetMap.Name}의 경제 데이터 동기화 완료.");
                    break;

                case 4: // 강제 시간 진행
                    foreach (var town in towns) 
                    {
                        foreach (var citizen in town.Citizens.ToList())
                        {
                            // 🌟 [수정 완료] 구형 OnTick 대신 마스터 틱 파이프라인 호출
                            citizen.OnHourTick(); 
                            // 강제로 정오(12시)를 기준으로 1사이클 일과를 진행시킵니다.
                            VirtualCitizenAI.ExecuteDeepRoutine(citizen, town, 12); 
                        }
                    }
                    m_From.SendMessage(65, $"{targetMap.Name} 대륙의 모든 시민들이 1사이클 경제 활동을 진행했습니다.");
                    break;

                case 5: // 일괄 구제
                    int healCount = 0;
                    foreach (var town in towns) 
                    {
                        foreach (var citizen in town.Citizens) 
                        {
                            citizen.Hunger = 100;
                            citizen.Thirst = 100;
                            citizen.Stress = 0;
                            healCount++;
                        }
                    }
				m_From.SendMessage(2100, $"{targetMap.Name} 대륙의 시민 {healCount}명의 상태가 완전히 회복되었습니다.");
                    break;
				case 6: // 모험가 스폰
                    foreach (var town in towns) 
                        VirtualAdventurerManager.SpawnInitialAdventurers(town, 15);
                    m_From.SendMessage(66, $"{targetMap.Name} 대륙에 모험가들이 새롭게 배치되었습니다.");
                    break;

                case 7: // 모험가 데이터 클리어
                    VirtualAdventurerManager.IdleAdventurers.RemoveAll(a => towns.Any(t => t.Facet == targetMap));
                    VirtualAdventurerManager.ActiveParties.RemoveAll(p => p.CurrentNode.NodeMap == targetMap);
                    m_From.SendMessage(33, $"{targetMap.Name} 대륙의 모험가 및 파티 데이터가 삭제되었습니다.");
                    break;
            }

            m_From.SendGump(new EconomyGlobalNpcGump(m_From, m_MapIndex));
        }
    }
}