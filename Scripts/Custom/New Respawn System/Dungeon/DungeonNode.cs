using System;
using Server;
using Server.Items;

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

        // 🌟 핵심 인덱스
        [CommandProperty(AccessLevel.GameMaster)]
        public RegionCode RCode { get; set; }

        // 기존 외부 호환용 디스플레이 프로퍼티 (읽기 전용)
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

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(2); // version up

            writer.Write((int)Depth);
            writer.Write(SpawnRange);
            writer.Write(HomeRange);
            writer.Write((int)RCode); // 문자열 대신 Enum 정수값 저장
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
                reader.ReadString(); // 구버전 m_ZoneId 문자열 쓰레기통
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
}