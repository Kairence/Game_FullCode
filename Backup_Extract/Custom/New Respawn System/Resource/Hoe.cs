using System;
using Server;
using Server.Engines.Harvest;
using Server.Network;

namespace Server.Items
{
    // 🌟 BaseAxe 상속을 제거하고 순수 Item으로 변경 (장착 불가, 무기 아님)
    public class Hoe : Item, IUsesRemaining 
    {
        public override int LabelNumber => 1150482; // hoe

        private int m_UsesRemaining;
        private bool m_ShowUsesRemaining;

        [CommandProperty(AccessLevel.GameMaster)]
        public int UsesRemaining
        {
            get => m_UsesRemaining;
            set { m_UsesRemaining = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ShowUsesRemaining
        {
            get => m_ShowUsesRemaining;
            set { m_ShowUsesRemaining = value; InvalidateProperties(); }
        }

        [Constructable]
        public Hoe() : base(0xE86)
        {
            Hue = 2524;
            Weight = 11.0;
            UsesRemaining = 50;
            ShowUsesRemaining = true;
        }

        public override void OnDoubleClick(Mobile from)
        {
            // 장착이 불가능하므로 가방 안에 있는지(IsChildOf)만 검사합니다.
            if (IsChildOf(from.Backpack))
            {
                Farming.System.BeginHarvesting(from, this);
            }
            else
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
        }

        // 🌟 무기 클래스에서 해주던 내구도 툴팁 출력을 직접 구현
        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (m_ShowUsesRemaining)
            {
                list.Add(1060584, m_UsesRemaining.ToString()); // uses remaining: ~1_val~
            }
        }

        public Hoe(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1); // 버전을 1로 올려 새로운 데이터 구조 기록
            writer.Write(m_UsesRemaining);
            writer.Write(m_ShowUsesRemaining);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version >= 1)
            {
                m_UsesRemaining = reader.ReadInt();
                m_ShowUsesRemaining = reader.ReadBool();
            }
            else
            {
                m_UsesRemaining = 50;
                m_ShowUsesRemaining = true;
            }
        }
    }
}