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

        [CommandProperty(AccessLevel.GameMaster)]
        public int Size { get => m_Size; set => m_Size = value; }

        // 설치할 에드온 지정
        public override BaseAddon Addon => new PrivateFarmAddon(m_Placer, m_Size);

        [Constructable]
        public PrivateFarmDeed() : this(4) { }

        [Constructable]
        public PrivateFarmDeed(int size)
        {
            m_Size = size;
            Name = $"농장 설계도 ({size}x{size})";
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
            writer.Write(0); // version
            writer.Write(m_Size);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
            m_Size = reader.ReadInt();
        }
    }
}