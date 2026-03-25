using System;
using System.Collections.Generic;
using System.Linq;
using Server.Network;
using Server.Mobiles;

namespace Server.Items
{
    public class GasTrap : BaseTrap
    {
        public enum GasTrapType { NorthWall, WestWall, Floor }

        private Poison m_Poison;
        private GasTrapType m_Type;

        [CommandProperty(AccessLevel.GameMaster)]
        public Poison Poison { get => m_Poison; set => m_Poison = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public GasTrapType Type { get => m_Type; set => m_Type = value; }

        [Constructable]
        public GasTrap() : this(GasTrapType.Floor, Poison.Lesser, 50.0, 1) { }

        [Constructable]
        public GasTrap(GasTrapType type, Poison poison, double difficulty, int range) 
            : base(0x35B6) 
        {
            m_Type = type;
            m_Poison = poison;
            Difficulty = difficulty;
            Range = range; 
        }

        public GasTrap(Serial serial) : base(serial) { }

        public override void OnTrigger(Mobile from)
        {
            // 부모의 OnTrigger에서 Detected = true 및 5초 후 자동 숨김 타이머가 작동합니다.
            base.OnTrigger(from);

            if (m_Poison == null || !from.Alive || from.AccessLevel > AccessLevel.Player)
                return;

            // 1. 연출: 밟은 타일 위치에서 가스 분출
            Effects.SendLocationEffect(from.Location, from.Map, 0x11A6, 16, 3);
            Effects.PlaySound(from.Location, from.Map, 0x231);

            // 2. 광역 중독 판정 (함정의 설정된 Range 사용)
            var targets = Map.GetMobilesInRange(Location, Range);
            foreach (var m in targets)
            {
                if (m is PlayerMobile { Alive: true, AccessLevel: AccessLevel.Player } pm)
                {
                    pm.ApplyPoison(from, m_Poison);
                    pm.LocalOverheadMessage(MessageType.Regular, 0x22, 500855);
                }
            }
            targets.Free();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // GasTrap 전용 버전

            writer.Write((int)m_Type);
            Poison.Serialize(m_Poison, writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            m_Type = (GasTrapType)reader.ReadInt();
            m_Poison = Poison.Deserialize(reader);
        }
    }
}
