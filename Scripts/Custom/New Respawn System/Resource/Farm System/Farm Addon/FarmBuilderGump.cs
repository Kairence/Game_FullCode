using System;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Targeting;
using Server.Mobiles;

namespace Server.Misc
{
    public class FarmBuilderGump : Gump
    {
        private PrivateFarmAddon m_Addon;
        private int m_SelectedType;

        public FarmBuilderGump(PrivateFarmAddon addon, int selectedType) : base(50, 50)
        {
            m_Addon = addon;
            m_SelectedType = selectedType;

            AddPage(0);
            int size = m_Addon.FarmSize;
            int spacing = 40; 
            
            // 왼쪽 그리드 영역
            int gridWidth = (size * spacing) + 30;
            // 전체 배경 크기 (아이콘이 빠졌으므로 폭을 줄여 슬림하게 변경)
            AddBackground(0, 0, gridWidth + 180, Math.Max(size * spacing + 100, 300), 9270);
            AddAlphaRegion(10, 10, gridWidth + 160, Math.Max(size * spacing + 80, 280));

            int px = gridWidth + 20;

            AddLabel(px, 30, 1152, "건설 메뉴");

            // [심플 리스트] 이미지 없이 버튼과 텍스트만 배치
            DrawSimpleRow(px, 65, 1, "지우기", 0);
            DrawSimpleRow(px, 105, 3, "양봉통 (50)", 2);
            DrawSimpleRow(px, 145, 2, "밭 타일 (100)", 1);
            DrawSimpleRow(px, 185, 4, "과일 나무 (150)", 3);

            // 가축 관리
            AddImageTiled(px, 225, 130, 2, 0x2626); // 구분선
            AddButton(px, 240, 4005, 4007, 5, GumpButtonType.Reply, 0);
            AddLabel(px + 35, 242, 200, "가축 등록하기");

            // 하단 상태 표시
            int count = m_Addon.GetLivestockCount();
            AddLabel(30, Math.Max(size * spacing + 55, 255), 68, $"현재 가축: {count}마리");

            // 타일 그리드
            for (int i = 0; i < m_Addon.TileData.Length; i++)
            {
                int x = i % size;
                int y = i / size;
                int current = m_Addon.TileData[i];

                // 현재 선택된 타일 종류를 직관적으로 알 수 있게 Hue를 활용할 수도 있습니다.
                int btnID = (current == 0) ? 2151 : (current == 1) ? 2103 : (current == 2) ? 2118 : 2117;
                AddButton(30 + (x * spacing), 45 + (y * spacing), btnID, btnID + 1, 100 + i, GumpButtonType.Reply, 0);
            }
        }

        private void DrawSimpleRow(int x, int y, int btnID, string name, int type)
        {
            bool isSelected = (m_SelectedType == type);
            
            // 선택된 항목은 체크 표시(209)나 다른 버튼 이미지(9724)로 강조
            AddButton(x, y, isSelected ? 9724 : 9721, 9724, btnID, GumpButtonType.Reply, 0);
            AddLabel(x + 35, y + 2, isSelected ? 68 : 906, name);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
            if (info.ButtonID == 0) return;

            // 팔레트 선택 버튼 (1:지우기, 2:밭, 3:양봉, 4:과수원)
            if (info.ButtonID <= 4) 
            {
                int typeToSelect = 0;
                if (info.ButtonID == 1) typeToSelect = 0;
                else if (info.ButtonID == 2) typeToSelect = 1;
                else if (info.ButtonID == 3) typeToSelect = 2;
                else if (info.ButtonID == 4) typeToSelect = 3;

                from.SendGump(new FarmBuilderGump(m_Addon, typeToSelect));
            }
            // 가축 넣기 버튼
            else if (info.ButtonID == 5)
            {
                from.SendMessage(68, "농장에 배치할 동물을 타겟팅하세요. (ESC 취소)");
                from.Target = new AssignAnimalTarget(m_Addon, m_SelectedType);
            }
            // 그리드 클릭 (타일 설치)
            else if (info.ButtonID >= 100)
            {
                int index = info.ButtonID - 100;
                double skill = from.Skills[SkillName.Herding].Base; // 농사 스킬(Herding)

                // 🌟 [기획 로직] 스킬 제한 체크
                if (m_SelectedType == 2 && skill < 50.0) // 양봉
                    from.SendMessage(33, "양봉통을 설치하려면 목동(Herding) 스킬이 50 이상 필요합니다.");
                else if (m_SelectedType == 1 && skill < 100.0) // 밭
                    from.SendMessage(33, "밭을 일구려면 목동(Herding) 스킬이 100 이상 필요합니다.");
                else if (m_SelectedType == 3 && skill < 150.0) // 과수원
                    from.SendMessage(33, "과수원 나무를 심으려면 목동(Herding) 스킬이 150 이상 필요합니다.");
                else
                {
                    // 스킬 조건을 만족하면 타일 데이터 변경 및 에드온 업데이트
                    if (index < m_Addon.TileData.Length)
                    {
                        m_Addon.TileData[index] = m_SelectedType;
                        m_Addon.UpdateLayout();
                    }
                }
                from.SendGump(new FarmBuilderGump(m_Addon, m_SelectedType));
            }
        }
    }
}