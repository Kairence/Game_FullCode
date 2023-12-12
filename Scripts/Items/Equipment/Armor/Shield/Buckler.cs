using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(SmallPlateShield))]
    public class Buckler : BaseShield
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
                return 500;
            }
        }
        public override int AosDexReq
        {
            get
            {
                return 200;
            }
        }
        public override int AosIntReq
        {
            get
            {
                return 100;
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
        public Buckler()
            : base(0x1B73)
        {
            Weight = 5.0;
			PrefixOption[61] = 109;
			SuffixOption[61] = 50;
			PrefixOption[62] = 110;
			SuffixOption[62] = 3500;
			PrefixOption[63] = 110;
			SuffixOption[63] = 100;
		}

        public Buckler(Serial serial)
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