using System;
using Server;
using Server.Items;
using Server.Gumps;
using Server.Network;

namespace Server.Misc
{
    public class DungeonNode : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public DungeonDepth Depth { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SpawnRange { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HomeRange { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public RegionCode RCode { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ZoneId => NewSpawnManager.GetDisplayName(RCode);

        [Constructable]
        public DungeonNode() : base(0x1F1C)
        {
            Movable = false;
            Visible = false;
            Name = "Dungeon Node";
            
            Depth = DungeonDepth.Middle;
            SpawnRange = 5;
            HomeRange = 10;
        }

        public DungeonNode(Serial serial) : base(serial) { }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
            {
                from.CloseGump(typeof(DungeonNodeGump));
                from.SendGump(new DungeonNodeGump(this));
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(2); 

            writer.Write((int)Depth);
            writer.Write(SpawnRange);
            writer.Write(HomeRange);
            writer.Write((int)RCode); 
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            Depth = (DungeonDepth)reader.ReadInt();
            SpawnRange = reader.ReadInt();
            HomeRange = reader.ReadInt();

            if (version >= 2)
            {
                RCode = (RegionCode)reader.ReadInt();
            }
            else
            {
                reader.ReadString(); 
                RCode = RegionCode.None;
            }
        }

        public Point3D? GetValidSpawnLocation()
        {
            for (int i = 0; i < 10; i++)
            {
                int x = X + Utility.RandomMinMax(-SpawnRange, SpawnRange);
                int y = Y + Utility.RandomMinMax(-SpawnRange, SpawnRange);
                int z = Map.GetAverageZ(x, y);

                if (Map.CanSpawnMobile(x, y, z))
                    return new Point3D(x, y, z);
            }
            return null;
        }
    }

    public class DungeonNodeGump : Gump
    {
        private DungeonNode m_Node;

        public DungeonNodeGump(DungeonNode node) : base(100, 100)
        {
            m_Node = node;

            AddPage(0);
            AddBackground(0, 0, 350, 300, 9200);
            AddHtml(0, 15, 350, 20, "<CENTER><BASEFONT COLOR=#FFFFFF>던전 노드 설정 (Dungeon Node)</BASEFONT></CENTER>", false, false);

            AddLabel(20, 50, 0x480, "RCode (구역 코드):");
            AddLabel(160, 50, 0x35, m_Node.RCode.ToString());
            AddButton(20, 75, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddLabel(55, 75, 0x480, "현재 위치로 RCode 자동 갱신");

            AddLabel(20, 110, 0x480, "Depth (깊이):");
            AddLabel(160, 110, 0x35, m_Node.Depth.ToString());
            AddButton(20, 130, 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddLabel(55, 130, 0x480, "깊이 변경 (순차 변경)");

            AddLabel(20, 170, 0x480, "Spawn Range:");
            AddImageTiled(160, 170, 60, 20, 0xBBC);
            AddTextEntry(160, 170, 60, 20, 0, 0, m_Node.SpawnRange.ToString());

            AddLabel(20, 200, 0x480, "Home Range:");
            AddImageTiled(160, 200, 60, 20, 0xBBC);
            AddTextEntry(160, 200, 60, 20, 0, 1, m_Node.HomeRange.ToString());

            AddButton(130, 250, 247, 248, 3, GumpButtonType.Reply, 0); 
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
            if (m_Node == null || m_Node.Deleted || from.AccessLevel < AccessLevel.GameMaster)
                return;

            switch (info.ButtonID)
            {
                case 1:
                    RegionCode newCode = RegionSaver.GetRegionCode(m_Node.Map, m_Node.X, m_Node.Y, m_Node.Z);
                    if (newCode != RegionCode.None)
                    {
                        m_Node.RCode = newCode;
                        from.SendMessage(68, string.Format("RCode가 {0}(으)로 갱신되었습니다.", newCode));
                    }
                    else
                    {
                        from.SendMessage(33, "현재 위치에서 유효한 RCode를 찾을 수 없습니다.");
                    }
                    from.SendGump(new DungeonNodeGump(m_Node));
                    break;
                case 2:
                    int d = (int)m_Node.Depth + 1;
                    if (d > 4) d = 1;
                    m_Node.Depth = (DungeonDepth)d;
                    from.SendGump(new DungeonNodeGump(m_Node));
                    break;
                case 3:
                    try
                    {
                        TextRelay trSpawn = info.GetTextEntry(0);
                        TextRelay trHome = info.GetTextEntry(1);

                        if (trSpawn != null) m_Node.SpawnRange = Math.Max(1, int.Parse(trSpawn.Text));
                        if (trHome != null) m_Node.HomeRange = Math.Max(1, int.Parse(trHome.Text));

                        from.SendMessage(68, "던전 노드 설정이 저장되었습니다.");
                    }
                    catch
                    {
                        from.SendMessage(33, "숫자 형식이 잘못되었습니다.");
                    }
                    from.SendGump(new DungeonNodeGump(m_Node));
                    break;
            }
        }
    }
}