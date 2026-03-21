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
        private int m_SelectedType; // 0:지우기, 1:밭

        // 우측 팔레트에서 유저에게 보여줄 실제 타일 미리보기 이미지
        private readonly int GrassTileID = 0x17C0; // 잔디 (지우기)
        private readonly int FarmTileID = 0x0914;  // 밭 타일

        public FarmBuilderGump(PrivateFarmAddon addon, int selectedType) : base(50, 50)
        {
            m_Addon = addon;
            m_SelectedType = selectedType;

            AddPage(0);
            int size = m_Addon.FarmSize;
            int spacing = 40; // 원래의 깔끔한 40 간격으로 복구

            // 배경 사이즈 계산
            int bgWidth = (size * spacing) + 180;
            int bgHeight = (size * spacing) + 120;

            AddBackground(0, 0, bgWidth, bgHeight, 9270);
            AddAlphaRegion(10, 10, bgWidth - 20, bgHeight - 20);

            // --- 우측 팔레트 (기능 선택) ---
            int px = (size * spacing) + 40;
            AddLabel(px, 50, 1152, "기능");
            
            // 1. 지우기
            AddButton(px, 80, m_SelectedType == 0 ? 9724 : 9721, 9724, 1, GumpButtonType.Reply, 0);
            AddLabel(px + 35, 85, 906, "지우기");
            AddItem(px + 90, 75, GrassTileID); // [유지] 유저가 지울 때 어떤 바닥이 되는지 미리보기

            // 2. 밭 타일
            AddButton(px, 125, m_SelectedType == 1 ? 9724 : 9721, 9724, 2, GumpButtonType.Reply, 0);
            AddLabel(px + 35, 130, 1358, "밭 타일");
            AddItem(px + 90, 120, FarmTileID); // [유지] 유저가 심을 밭의 모양 미리보기

            // 3. 가축 넣기
            AddButton(px, 170, 9721, 9724, 3, GumpButtonType.Reply, 0);
            AddLabel(px + 35, 175, 200, "가축 넣기");

            // 하단 가축 현황
            int livestockCount = m_Addon.GetLivestockCount();
            AddLabel(30, (size * spacing) + 70, 68, $"현재 등록된 가축: {livestockCount}마리");

            // --- 좌측 맵 그리드 (도화지) ---
            for (int i = 0; i < m_Addon.TileData.Length; i++)
            {
                int x = i % size;
                int y = i / size;
                int current = m_Addon.TileData[i];

                int gumpX = 30 + (x * spacing);
                int gumpY = 60 + (y * spacing);

                // [★ 복구] 이상한 마름모 이미지들 다 치우고, 유저님의 깔끔했던 보석 버튼으로 렌더링
                // 2152(큰 파란 보석 = 빈 땅), 2103(작은 보석 = 밭)
                int btnID = (current == 0) ? 2152 : 2103; 
                
                AddButton(gumpX, gumpY, btnID, btnID + 1, 100 + i, GumpButtonType.Reply, 0);
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0) return;

            if (info.ButtonID <= 2)
            {
                sender.Mobile.SendGump(new FarmBuilderGump(m_Addon, info.ButtonID - 1));
            }
            else if (info.ButtonID == 3)
            {
                sender.Mobile.SendMessage(68, "농장에 배치할 동물을 타겟팅하세요. (ESC 취소)");
                sender.Mobile.Target = new AssignAnimalTarget(m_Addon, m_SelectedType);
            }
            else if (info.ButtonID >= 100)
            {
                int index = info.ButtonID - 100;
                if (index < m_Addon.TileData.Length)
                {
                    m_Addon.TileData[index] = m_SelectedType;
                    m_Addon.UpdateLayout();
                }
                sender.Mobile.SendGump(new FarmBuilderGump(m_Addon, m_SelectedType));
            }
        }
    }

    public class AssignAnimalTarget : Target
    {
        private PrivateFarmAddon m_Addon;
        private int m_LastSelectedType;

        public AssignAnimalTarget(PrivateFarmAddon addon, int selectedType) : base(10, false, TargetFlags.None)
        {
            m_Addon = addon;
            m_LastSelectedType = selectedType;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            if (targeted is BaseCreature animal)
            {
                m_Addon.AssignAnimal(from, animal);
            }
            else
            {
                from.SendMessage(33, "살아있는 동물만 지정할 수 있습니다.");
            }
            
            from.SendGump(new FarmBuilderGump(m_Addon, m_LastSelectedType));
        }
    }
}