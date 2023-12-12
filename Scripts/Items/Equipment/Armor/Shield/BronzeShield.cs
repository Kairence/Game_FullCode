using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(SmallPlateShield))]
    public class BronzeShield : BaseShield
    {
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
                return 300;
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
                return 500;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 0;
            }
        }
		
		
        [Constructable]
        public BronzeShield()
            : base(0x1B72)
        {
            Weight = 6.0;
			PrefixOption[61] = 109;
			SuffixOption[61] = 100;
			PrefixOption[62] = 110;
			SuffixOption[62] = 1000;
			PrefixOption[63] = 110;
			SuffixOption[63] = 200;
			PrefixOption[64] = 116;
			SuffixOption[64] = 3000;

		}

        public BronzeShield(Serial serial)
            : base(serial)
        {
        }
              
        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);//version
        }
    }
}