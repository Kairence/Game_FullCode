using System;

namespace Server.Items
{
    public class WoodenKiteShield : BaseShield
    {
        [Constructable]
        public WoodenKiteShield()
            : base(0x1B78)
        {
            Weight = 20.0;
			ShieldMinDamage = 2;
			ShieldMaxDamage = 4;
			PrefixOption[61] = 41; //시전 속도
			SuffixOption[61] = 100000;
        }

        public WoodenKiteShield(Serial serial)
            : base(serial)
        {
        }

        public override int InitMinHits
        {
            get
            {
                return 100;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 100;
            }
        }
        public override int AosStrReq
        {
            get
            {
                return 2000;
            }
        }
        public override int AosDexReq
        {
            get
            {
                return 1000;
            }
        }
        public override int AosIntReq
        {
            get
            {
                return 1000;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 3;
            }
        }
		
		public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);//version
        }
        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }       
    }
}