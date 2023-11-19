using System;

namespace Server.Items
{
    public class MiniAgilityPotion : BaseAgilityPotion
    {
        [Constructable]
        public MiniAgilityPotion()
            : base(PotionEffect.AgilityMini)
        {
 			Name = "최하급 마나 회복 포션";
       }

        public MiniAgilityPotion(Serial serial)
            : base(serial)
        {
        }

        public override int Refresh
        {
            get
            {
                return 20;
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