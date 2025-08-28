using System;

namespace Server.Items
{
    public class Helmet : BaseArmor
    {
        [Constructable]
        public Helmet()
            : base(0x140A)
        {
			PrefixOption[50] = 15;
			PrefixOption[61] = 114;
			SuffixOption[61] = 20000;
			PrefixOption[62] = 5;
			SuffixOption[62] = 5000000;
			PrefixOption[63] = 17;
			SuffixOption[63] = 100000;
            Weight = 13.0;
        }

        public Helmet(Serial serial)
            : base(serial)
        {
        }

		public override int AosStrReq { get { return 1750; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase
        {
            get
            {
                return 10;
            }
        }
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Ringmail;
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}