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
			SuffixOption[61] = 800;
			PrefixOption[62] = 4;
			SuffixOption[62] = 25000;
			PrefixOption[63] = 17;
			SuffixOption[63] = 500;
            Weight = 5.0;
        }

        public Helmet(Serial serial)
            : base(serial)
        {
        }

		public override int AosStrReq { get { return 1100; } }
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