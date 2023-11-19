using System;

namespace Server.Items
{
    public class GreaterStrengthPotion : BaseStrengthPotion
    {
        [Constructable]
        public GreaterStrengthPotion()
            : base(PotionEffect.StrengthGreater)
        {
 			Name = "상급 파워 물약";
       }

        public GreaterStrengthPotion(Serial serial)
            : base(serial)
        {
        }

        public override int StrOffset
        {
            get
            {
                return 175;
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