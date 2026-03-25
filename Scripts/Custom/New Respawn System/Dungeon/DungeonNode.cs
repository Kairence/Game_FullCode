using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Regions;

namespace Server.Misc
{
    public enum DungeonDepth
    {
        Entrance = 1,
        Middle = 2,
        Deep = 3,
        BossRoom = 4
    }

    public class DungeonNode : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public string ZoneId { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DungeonDepth Depth { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SpawnRange { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HomeRange { get; set; }

        [Constructable]
        public DungeonNode() : base(0x1F1C) // 보라 크리스탈
        {
            Movable = false;
            Visible = false;
            Name = "Dungeon Spawn Node";
            ZoneId = "None";
            Depth = DungeonDepth.Entrance;
            SpawnRange = 5;
            HomeRange = 10;
        }

        public DungeonNode(Serial serial) : base(serial) { }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
                from.SendGump(new DungeonNodeGump(from, this));
        }

        // 지형 감지 알고리즘 (원본 보존)
        public Point3D? GetValidSpawnLocation()
        {
            if (Map == null || Map == Map.Internal) return null;
            Region nodeRegion = Region.Find(Location, Map);

            for (int i = 0; i < 10; i++)
            {
                int rx = X + Utility.RandomMinMax(-SpawnRange, SpawnRange);
                int ry = Y + Utility.RandomMinMax(-SpawnRange, SpawnRange);
                int rz = Map.GetAverageZ(rx, ry);

                if (Map.CanSpawnMobile(rx, ry, rz))
                {
                    Region spawnRegion = Region.Find(new Point3D(rx, ry, rz), Map);
                    if (nodeRegion == spawnRegion) return new Point3D(rx, ry, rz);
                }
            }
            return null;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(ZoneId ?? string.Empty);
            writer.Write((int)Depth);
            writer.Write(SpawnRange);
            writer.Write(HomeRange);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            ZoneId = reader.ReadString();
            Depth = (DungeonDepth)reader.ReadInt();
            SpawnRange = reader.ReadInt();
            HomeRange = reader.ReadInt();
        }
    }

    // ========================================================================
    // 메인 세팅 Gump (원본 레이아웃 유지 + 실시간 반영 추가)
    // ========================================================================
    public class DungeonNodeGump : Gump
    {
        private readonly DungeonNode m_Node;

        public DungeonNodeGump(Mobile from, DungeonNode node) : base(100, 100)
        {
            m_Node = node;
            from.CloseGump(typeof(DungeonNodeGump));
            from.CloseGump(typeof(NodeTreeGump));

            AddPage(0);
            AddBackground(0, 0, 400, 350, 9270);
            AddHtml(10, 10, 380, 20, "<CENTER>던전 노드 세팅 매니저</CENTER>", false, false);

            AddHtml(20, 50, 100, 20, "현재 구역:", false, false);
            AddLabel(120, 50, 68, node.ZoneId);
            AddButton(20, 75, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddHtml(55, 75, 200, 20, "지역 트리에서 선택 (GoGump)", false, false);

            AddHtml(20, 110, 100, 20, "등장 몬스터 깊이:", false, false);
            AddRadio(120, 110, 208, 209, node.Depth == DungeonDepth.Entrance, 10); AddLabel(145, 110, 0, "입구");
            AddRadio(200, 110, 208, 209, node.Depth == DungeonDepth.Middle, 11); AddLabel(225, 110, 0, "중간");
            AddRadio(280, 110, 208, 209, node.Depth == DungeonDepth.Deep, 12); AddLabel(305, 110, 0, "심층");
            AddRadio(120, 135, 208, 209, node.Depth == DungeonDepth.BossRoom, 13); AddLabel(145, 135, 0, "보스룸");

            AddHtml(20, 180, 150, 20, "스폰 탐색 반경:", false, false);
            AddBackground(150, 180, 50, 20, 9300);
            AddTextEntry(150, 180, 50, 20, 0, 20, node.SpawnRange.ToString());

            AddHtml(20, 210, 150, 20, "몬스터 배회 반경:", false, false);
            AddBackground(150, 210, 50, 20, 9300);
            AddTextEntry(150, 210, 50, 20, 0, 21, node.HomeRange.ToString());

            AddButton(150, 280, 2128, 2129, 2, GumpButtonType.Reply, 0); // OK
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Node == null || m_Node.Deleted) return;

            if (info.ButtonID == 1) // 트리 오픈
            {
                LocationTree tree = GetTree(m_Node.Map);
                if (tree != null) sender.Mobile.SendGump(new NodeTreeGump(sender.Mobile, m_Node, tree, tree.Root, 0, ""));
                return;
            }

            if (info.ButtonID == 2) // OK 버튼 (저장 및 동기화)
            {
                if (int.TryParse(info.GetTextEntry(20)?.Text, out int sRange)) m_Node.SpawnRange = sRange;
                if (int.TryParse(info.GetTextEntry(21)?.Text, out int hRange)) m_Node.HomeRange = hRange;

                foreach (int switchId in info.Switches)
                {
                    if (switchId == 10) m_Node.Depth = DungeonDepth.Entrance;
                    else if (switchId == 11) m_Node.Depth = DungeonDepth.Middle;
                    else if (switchId == 12) m_Node.Depth = DungeonDepth.Deep;
                    else if (switchId == 13) m_Node.Depth = DungeonDepth.BossRoom;
                }

                // [추가] 매니저에게 노드 변경 알림 (실시간 반영)
                foreach (var z in DungeonManager.Zones.Values) z.CacheNodes();
                foreach (var z in EcosystemManager.Zones.Values) z.CacheNodes();
                
                sender.Mobile.SendMessage(68, "설정이 저장되었으며 시스템에 즉시 반영되었습니다.");
            }
        }

        private static LocationTree GetTree(Map map)
        {
            if (map == Map.Felucca) return GoGump.Felucca;
            if (map == Map.Trammel) return GoGump.Trammel;
            if (map == Map.Ilshenar) return GoGump.Ilshenar;
            if (map == Map.Malas) return GoGump.Malas;
            if (map == Map.Tokuno) return GoGump.Tokuno;
            if (map == Map.TerMur) return GoGump.TerMur;
            return null;
        }
    }

    // ========================================================================
    // 위치 트리 탐색 Gump (원본 페이징 로직 보존 + 이름 스냅 기능 추가)
    // ========================================================================
    public class NodeTreeGump : Gump
    {
        private readonly DungeonNode m_Node;
        private readonly LocationTree m_Tree;
        private readonly ParentNode m_CurrentNode;
        private readonly int m_Page;
        private readonly string m_PathPrefix;

        public NodeTreeGump(Mobile from, DungeonNode node, LocationTree tree, ParentNode pNode, int page, string pathPrefix) : base(150, 100)
        {
            m_Node = node; m_Tree = tree; m_CurrentNode = pNode; m_Page = page; m_PathPrefix = pathPrefix;
            from.CloseGump(typeof(NodeTreeGump));

            AddPage(0);
            AddBackground(0, 0, 350, 420, 9270);
            AddHtml(10, 10, 330, 20, $"<CENTER>{pNode.Name} 탐색 중</CENTER>", false, false);

            int x = 20, y = 40;
            if (pNode.Parent != null)
            {
                AddButton(x, y, 4014, 4016, 1, GumpButtonType.Reply, 0);
                AddLabel(x + 35, y, 0, "상위 폴더로 이동");
                y += 25;
            }

            int entriesPerPage = 12;
            int start = page * entriesPerPage;
            int end = Math.Min(start + entriesPerPage, pNode.Children.Length);

            for (int i = start; i < end; i++)
            {
                object child = pNode.Children[i];
                int buttonId = i + 10;

                if (child is ParentNode cp)
                {
                    AddButton(x, y, 4005, 4007, buttonId, GumpButtonType.Reply, 0);
                    AddLabel(x + 35, y, 0, $"[폴더] {cp.Name}");
                }
                else if (child is ChildNode cn)
                {
                    AddButton(x, y, 4011, 4012, buttonId, GumpButtonType.Reply, 0);
                    AddLabel(x + 35, y, 68, cn.Name);
                }
                y += 25;
            }

            if (page > 0) AddButton(20, 380, 4014, 4016, 2, GumpButtonType.Reply, 0); // 이전
            if (end < pNode.Children.Length) AddButton(300, 380, 4005, 4007, 3, GumpButtonType.Reply, 0); // 다음
            
            AddButton(150, 380, 2128, 2129, 4, GumpButtonType.Reply, 0);
            AddLabel(180, 380, 33, "취소 / 뒤로");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Node == null || m_Node.Deleted) return;

            if (info.ButtonID == 0 || info.ButtonID == 4) 
            { 
                sender.Mobile.SendGump(new DungeonNodeGump(sender.Mobile, m_Node)); return; 
            }
            
            if (info.ButtonID == 1) // 상위로
            {
                if (m_CurrentNode.Parent != null)
                    sender.Mobile.SendGump(new NodeTreeGump(sender.Mobile, m_Node, m_Tree, m_CurrentNode.Parent, 0, ""));
                return;
            }

            if (info.ButtonID == 2) // 이전 페이지
            {
                sender.Mobile.SendGump(new NodeTreeGump(sender.Mobile, m_Node, m_Tree, m_CurrentNode, m_Page - 1, m_PathPrefix));
                return;
            }

            if (info.ButtonID == 3) // 다음 페이지
            {
                sender.Mobile.SendGump(new NodeTreeGump(sender.Mobile, m_Node, m_Tree, m_CurrentNode, m_Page + 1, m_PathPrefix));
                return;
            }

            int index = info.ButtonID - 10;
            if (index >= 0 && index < m_CurrentNode.Children.Length)
            {
                object child = m_CurrentNode.Children[index];
                string nextPrefix = string.IsNullOrEmpty(m_PathPrefix) ? $"{m_Node.Map.Name} {m_CurrentNode.Name}" : $"{m_PathPrefix} {m_CurrentNode.Name}";

                if (child is ParentNode childParent)
                {
                    sender.Mobile.SendGump(new NodeTreeGump(sender.Mobile, m_Node, m_Tree, childParent, 0, nextPrefix));
                }
                else if (child is ChildNode childNode)
                {
                    string rawName = $"{nextPrefix} {childNode.Name}".Replace("Locations ", "").Trim();
                    
                    // [핵심] 수동 선택 시 로직(Logic)에 등록된 정확한 이름으로 자동 스냅
                    m_Node.ZoneId = NewSpawnManager.FindBestLogicKey(rawName) ?? rawName;

                    sender.Mobile.SendGump(new DungeonNodeGump(sender.Mobile, m_Node));
                }
            }
        }
    }
}
