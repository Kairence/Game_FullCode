using System;

namespace Server.Items
{
    public class GreaterAgilityPotion : BaseAgilityPotion
    {
        [Constructable]
        public GreaterAgilityPotion()
            : base(PotionEffect.AgilityGreater)
        {
 			Name = "상급 마나 회복 포션";
       }

        public GreaterAgilityPotion(Serial serial)
            : base(serial)
        {
        }

        public override int Refresh
        {
            get
            {
                return 320;
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