using System;
using Server;
using Server.Items;
using Server.Multis;
using Server.Targeting;

namespace Server.Misc
{
    public class PrivateFarmDeed : BaseAddonDeed
    {
        private int m_Size;
        private System.Collections.Generic.List<FarmStorageData> m_StorageData = new System.Collections.Generic.List<FarmStorageData>();
        public System.Collections.Generic.List<FarmStorageData> StorageData { get { return m_StorageData; } set { m_StorageData = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Size { get => m_Size; set => m_Size = value; }

        // 설치할 에드온 지정
        public override BaseAddon Addon => new PrivateFarmAddon(m_Placer, m_Size, m_StorageData);

        [Constructable]
        public PrivateFarmDeed() : this(6) { }

        [Constructable]
        public PrivateFarmDeed(int size)
        {
            if (size <= 6) m_Size = 6;
            else if (size <= 12) m_Size = 12;
            else m_Size = 18;

            Name = $"농장 설계도 ({m_Size}x{m_Size})";
            LootType = LootType.Blessed;
        }

        private Mobile m_Placer;

        public override void OnDoubleClick(Mobile from)
        {
            m_Placer = from;

            // 설치 전 위치 확인을 위해 타겟팅 시작
            if (IsChildOf(from.Backpack))
                base.OnDoubleClick(from);
            else
                from.SendLocalizedMessage(1042664); // 유물은 가방 안에 있어야 합니다.
        }

        public PrivateFarmDeed(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1); // version
            writer.Write(m_Size);
            writer.Write(m_StorageData == null ? 0 : m_StorageData.Count);
            if (m_StorageData != null) { foreach (var item in m_StorageData) { item.Serialize(writer); } }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            if (version >= 0) m_Size = reader.ReadInt();
            if (version >= 1)
            {
                int count = reader.ReadInt();
                m_StorageData = new System.Collections.Generic.List<FarmStorageData>();
                for (int i=0; i<count; i++) m_StorageData.Add(new FarmStorageData(reader));
            }
        }
    }

    public class SmallFarmDeed : PrivateFarmDeed
    {
        [Constructable]
        public SmallFarmDeed() : base(6) { }
        public SmallFarmDeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class MediumFarmDeed : PrivateFarmDeed
    {
        [Constructable]
        public MediumFarmDeed() : base(12) { }
        public MediumFarmDeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }

    public class LargeFarmDeed : PrivateFarmDeed
    {
        [Constructable]
        public LargeFarmDeed() : base(18) { }
        public LargeFarmDeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
}
