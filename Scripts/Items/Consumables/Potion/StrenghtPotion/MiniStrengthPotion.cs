using System;

namespace Server.Items
{
    public class MiniStrengthPotion : BaseStrengthPotion
    {
        [Constructable]
        public MiniStrengthPotion()
            : base(PotionEffect.StrengthGreater)
        {
 			Name = "최하급 파워 물약";
       }

        public MiniStrengthPotion(Serial serial)
            : base(serial)
        {
        }

        public override int StrOffset
        {
            get
            {
                return 100;
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