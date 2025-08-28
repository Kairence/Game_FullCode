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
			PrefixOption[61] = 110;
			SuffixOption[61] = 50000;
			PrefixOption[62] = 42;
			SuffixOption[62] = 50000;
			PrefixOption[63] = 44;
			SuffixOption[63] = 50000;

            this.Weight = 13.0;
        }

        public ChainCoif(Serial serial)
            : base(serial)
        {
        }
		public override int AosStrReq { get { return 3000; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase
        {
            get
            {
                return 13;
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