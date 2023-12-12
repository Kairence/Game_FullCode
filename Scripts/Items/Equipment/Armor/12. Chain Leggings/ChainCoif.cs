using System;

namespace Server.Items
{
    [FlipableAttribute(0x13BB, 0x13C0)]
    public class ChainCoif : BaseArmor
    {
        [Constructable]
        public ChainCoif()
            : base(0x13BB)
        {
			PrefixOption[50] = 16;
			PrefixOption[61] = 117;
			SuffixOption[61] = 700;
			PrefixOption[62] = 42;
			SuffixOption[62] = 500;
			PrefixOption[63] = 44;
			SuffixOption[63] = 2500;

            this.Weight = 9.0;
        }

        public ChainCoif(Serial serial)
            : base(serial)
        {
        }
		public override int AosStrReq { get { return 1800; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase
        {
            get
            {
                return 12;
            }
        }
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Chainmail;
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