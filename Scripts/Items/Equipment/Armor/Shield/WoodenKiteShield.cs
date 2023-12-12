using System;

namespace Server.Items
{
    public class WoodenKiteShield : BaseShield
    {
        [Constructable]
        public WoodenKiteShield()
            : base(0x1B78)
        {
            Weight = 5.0;
			PrefixOption[61] = 109;
			SuffixOption[61] = 100;
			PrefixOption[62] = 110;
			SuffixOption[62] = 2000;
			PrefixOption[63] = 110;
			SuffixOption[63] = 200;
			PrefixOption[64] = 116;
			SuffixOption[64] = 5000;			
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
                return 1500;
            }
        }
        public override int AosDexReq
        {
            get
            {
                return 100;
            }
        }
        public override int AosIntReq
        {
            get
            {
                return 2000;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 0;
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