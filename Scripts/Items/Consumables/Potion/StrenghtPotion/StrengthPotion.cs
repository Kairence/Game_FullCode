using System;

namespace Server.Items
{
    public class StrengthPotion : BaseStrengthPotion
    {
        [Constructable]
        public StrengthPotion()
            : base(PotionEffect.Strength)
        {
			Name = "파워 물약";
        }

        public StrengthPotion(Serial serial)
            : base(serial)
        {
        }

        public override int StrOffset
        {
            get
            {
                return 150;
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