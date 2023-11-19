using System;

namespace Server.Items
{
    public class LesserAgilityPotion : BaseAgilityPotion
    {
        [Constructable]
        public LesserAgilityPotion()
            : base(PotionEffect.AgilityLesser)
        {
 			Name = "하급 마나 회복 포션";
       }

        public LesserAgilityPotion(Serial serial)
            : base(serial)
        {
        }

        public override int Refresh
        {
            get
            {
                return 80;
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