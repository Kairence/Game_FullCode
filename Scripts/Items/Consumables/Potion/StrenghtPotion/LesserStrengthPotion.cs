using System;

namespace Server.Items
{
    public class LesserStrengthPotion : BaseStrengthPotion
    {
        [Constructable]
        public LesserStrengthPotion()
            : base(PotionEffect.StrengthGreater)
        {
 			Name = "하급 파워 물약";
       }

        public LesserStrengthPotion(Serial serial)
            : base(serial)
        {
        }

        public override int StrOffset
        {
            get
            {
                return 125;
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