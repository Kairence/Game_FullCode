using System;

namespace Server.Items
{
    public class AgilityPotion : BaseAgilityPotion
    {
        [Constructable]
        public AgilityPotion()
            : base(PotionEffect.Agility)
        {
			Name = "마나 회복 포션";
        }

        public AgilityPotion(Serial serial)
            : base(serial)
        {
        }

        public override int Refresh
        {
            get
            {
                return 180;
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