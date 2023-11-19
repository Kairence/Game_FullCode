using System;

namespace Server.Items
{
    public class TotalStrengthPotion : BaseStrengthPotion
    {
        [Constructable]
        public TotalStrengthPotion()
            : base(PotionEffect.StrengthGreater)
        {
			Name = "최상급 파워 물약";
        }

        public TotalStrengthPotion(Serial serial)
            : base(serial)
        {
        }

        public override int StrOffset
        {
            get
            {
                return 200;
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