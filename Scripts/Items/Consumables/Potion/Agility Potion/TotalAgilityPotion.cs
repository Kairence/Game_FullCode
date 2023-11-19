using System;

namespace Server.Items
{
    public class TotalAgilityPotion : BaseAgilityPotion
    {
        [Constructable]
        public TotalAgilityPotion()
            : base(PotionEffect.AgilityGreater)
        {
 			Name = "최상급 마나 회복 포션";
       }

        public TotalAgilityPotion(Serial serial)
            : base(serial)
        {
        }

        public override int Refresh
        {
            get
            {
                return 500;
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