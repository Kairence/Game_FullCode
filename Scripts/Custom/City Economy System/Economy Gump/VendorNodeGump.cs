using System;
using Server;
using Server.Gumps;
using Server.Network;

namespace Server.Misc
{
    public class VendorNodeGump : Gump
    {
        private readonly VendorNode m_Node;

        public VendorNodeGump(VendorNode node) : base(100, 100)
        {
            m_Node = node;

            AddPage(0);
            AddBackground(0, 0, 420, 390, 9270);
            AddAlphaRegion(10, 10, 400, 370);

            AddHtml(0, 20, 420, 20, "<CENTER><BASEFONT COLOR=#FDB913>VENDOR NODE EDITOR</BASEFONT></CENTER>", false, false);

            int y = 60;
            int spacing = 35;

            // 1. 마을 ID (TownID)
            AddLabel(30, y, 1152, "마을 ID (TownID):");
            AddBackground(160, y - 5, 70, 25, 9300);
            AddTextEntry(165, y - 3, 60, 20, 0, 1, m_Node.TownID.ToString());
            string townName = TownNumber.GetName(m_Node.TownID) ?? "Unknown";
            AddLabel(240, y, 68, $"({townName})");
            y += spacing;

            // 2. 상인 이름 (VendorName) - 새로 추가됨
            AddLabel(30, y, 1152, "상인 이름 (Name):");
            AddBackground(160, y - 5, 200, 25, 9300);
            AddTextEntry(165, y - 3, 190, 20, 0, 5, m_Node.VendorName ?? "");
            y += spacing;

            // 3. 최대 스폰 (MaxCount)
            AddLabel(30, y, 1152, "최대 스폰 (MaxCount):");
            AddBackground(160, y - 5, 70, 25, 9300);
            AddTextEntry(165, y - 3, 60, 20, 0, 2, m_Node.MaxCount.ToString());
            y += spacing;

            // 4. 활동 반경 (HomeRange)
            AddLabel(30, y, 1152, "활동 반경 (HomeRange):");
            AddBackground(160, y - 5, 70, 25, 9300);
            AddTextEntry(165, y - 3, 60, 20, 0, 3, m_Node.HomeRange.ToString());
            y += spacing;

            // 5. 스폰 리스트 (SpawnList)
            AddLabel(30, y, 1152, "스폰 리스트 (SpawnList):");
            y += 25;
            AddBackground(30, y, 360, 45, 9300);
            AddTextEntry(35, y + 2, 350, 40, 0, 4, m_Node.SpawnList ?? "");
            y += 60;

            // 6. 버튼 영역
            AddButton(60, y, 4023, 4025, 1, GumpButtonType.Reply, 0);
            AddLabel(95, y, 68, "설정 저장 및 상인 즉시 소환");

            y += 35;
            AddButton(60, y, 4017, 4019, 2, GumpButtonType.Reply, 0);
            AddLabel(95, y, 33, "이 노드 완전히 삭제");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Node == null || m_Node.Deleted || info.ButtonID == 0) return;

            Mobile from = sender.Mobile;

            switch (info.ButtonID)
            {
                case 1: // 저장 및 적용
                    {
                        m_Node.TownID = Utility.ToInt32(info.GetTextEntry(1)?.Text ?? "0");
                        m_Node.MaxCount = Utility.ToInt32(info.GetTextEntry(2)?.Text ?? "1");
                        m_Node.HomeRange = Utility.ToInt32(info.GetTextEntry(3)?.Text ?? "5");
                        m_Node.SpawnList = info.GetTextEntry(4)?.Text ?? "";
                        m_Node.VendorName = info.GetTextEntry(5)?.Text ?? ""; // VendorName 적용

                        // 상인 소환 및 정리
                        // (VendorNode에 Respawn() 이 아닌 DoSpawn() 으로 되어있다면 DoSpawn()으로 변경하세요)
                        m_Node.Respawn(); 

                        from.SendMessage(68, "노드 설정이 저장되고 상인이 재배치되었습니다.");
                        
                        // 변경된 정보를 바탕으로 검프 새로고침
                        from.SendGump(new VendorNodeGump(m_Node));
                        break;
                    }
                case 2: // 노드 삭제
                    {
                        from.SendMessage(33, "상인 노드를 제거했습니다.");
                        m_Node.Delete();
                        break;
                    }
            }
        }
    }
}
