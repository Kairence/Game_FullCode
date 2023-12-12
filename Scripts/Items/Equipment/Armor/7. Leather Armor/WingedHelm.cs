using System;

namespace Server.Items
{
    [FlipableAttribute(0x2B73, 0x316A)]
    public class WingedHelm : BaseArmor
    {
        public override int InitMinHits { get { return 100; } }
        public override int InitMaxHits { get { return 100; } }

        public override int AosStrReq { get { return 400; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase
        {
            get
            {
                return 7;
            }
        }

        [Constructable]
        public WingedHelm()
            : base(0x2B73)
        {
			PrefixOption[50] = 4;
			PrefixOption[61] = 114;
			SuffixOption[61] = 200;
			PrefixOption[62] = 41;
			SuffixOption[62] = 250;
			PrefixOption[63] = 8;
			SuffixOption[63] = 1000;
			PrefixOption[64] = 6;
			SuffixOption[64] = 25000;
       }

        public WingedHelm(Serial serial)
            : base(serial)
        {
        }
		
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Leather;
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}