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
            AddBackground(0, 0, 400, 320, 9270);
            AddAlphaRegion(10, 10, 380, 300);

            DrawInterface();
        }

        private void DrawInterface()
        {
            Map targetMap = Facets[m_MapIndex];
            var towns = TownEconomyManager.Towns.Values.Where(t => t.Facet == targetMap).ToList();

            AddHtml(10, 20, 380, 25, $"<CENTER><BASEFONT SIZE='6' COLOR='#FDB913'>{targetMap.Name} NPC 마스터 관리</BASEFONT></CENTER>", false, false);

            // 1. 대륙 통계 요약
            int totalCitizens = towns.Sum(t => t.Citizens?.Count ?? 0);
            
            AddImageTiled(20, 60, 360, 50, 9354);
            AddLabel(40, 75, 1152, $"대륙 내 활성 마을: {towns.Count}개 / 총 인구: {totalCitizens:N0}명");

            // 2. 일괄 실행 명령
            int y = 130;
            
            // [전체 리스폰] 상인 수에 비례하여 모든 마을 NPC 생성
            AddButton(30, y, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddLabel(65, y + 2, 1152, "대륙 전체 NPC 리스폰 (상인 비례)");

            y += 40;
            // [전체 삭제] 현재 대륙의 모든 가상 NPC 데이터 즉시 삭제
            AddButton(30, y, 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddLabel(65, y + 2, 33, "대륙 전체 NPC 데이터 삭제 (Clear)");

            y += 40;
            // [자산 동기화] 모든 마을의 Wealth를 실시간 재계산 수치로 맞춤
            AddButton(30, y, 4005, 4007, 3, GumpButtonType.Reply, 0);
            AddLabel(65, y + 2, 1152, "대륙 경제 지표 강제 동기화");

            // 닫기 버튼
            AddButton(150, 270, 247, 248, 0, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Map targetMap = Facets[m_MapIndex];
            var towns = TownEconomyManager.Towns.Values.Where(t => t.Facet == targetMap).ToList();

            switch (info.ButtonID)
            {
                case 1: // 리스폰 (TownDemographics 연동)
                    TownDemographics.RespawnFacet(targetMap, m_From);
                    break;

                case 2: // 전체 삭제
                    foreach (var town in towns) town.Citizens?.Clear();
                    m_From.SendMessage(33, $"{targetMap.Name}의 가상 NPC 데이터가 모두 삭제되었습니다.");
                    break;

                case 3: // 지표 동기화
                    foreach (var town in towns) { 
                        town.UpdateBaseWealth();
                        town.Wealth = town.BaseWealth;
                    }
                    m_From.SendMessage(68, $"{targetMap.Name}의 경제 데이터 동기화 완료.");
                    break;
            }

            if (info.ButtonID > 0)
                m_From.SendGump(new EconomyGlobalNpcGump(m_From, m_MapIndex));
        }
    }
}
