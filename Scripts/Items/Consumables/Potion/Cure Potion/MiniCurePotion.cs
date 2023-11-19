using System;

namespace Server.Items
{
    public class MiniCurePotion : BaseCurePotion
    {
        private static readonly CureLevelInfo[] m_OldLevelInfo = new CureLevelInfo[]
        {
            new CureLevelInfo(Poison.Lesser, 1.00) // 75% chance to cure lesser poison
        };
        private static readonly CureLevelInfo[] m_AosLevelInfo = new CureLevelInfo[]
        {
            new CureLevelInfo(Poison.Lesser, 1.00)
        };
        [Constructable]
        public MiniCurePotion()
            : base(PotionEffect.CureLesser)
        {
			Name = "최하급 해독 뮬약";
        }

        public MiniCurePotion(Serial serial)
            : base(serial)
        {
        }

        public override CureLevelInfo[] LevelInfo
        {
            get
            {
                return Core.AOS ? m_AosLevelInfo : m_OldLevelInfo;
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}