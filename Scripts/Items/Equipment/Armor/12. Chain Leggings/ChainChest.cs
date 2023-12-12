using System;

namespace Server.Items
{
    [FlipableAttribute(0x13bf, 0x13c4)]
    public class ChainChest : BaseArmor
    {
		public override int AosStrReq { get { return 2000; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase { get { return 12; } }
        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Chainmail; } }
		
        [Constructable]
        public ChainChest()
            : base(0x13BF)
        {
			PrefixOption[50] = 16;
			PrefixOption[61] = 117;
			SuffixOption[61] = 700;
			PrefixOption[62] = 42;
			SuffixOption[62] = 500;
			PrefixOption[63] = 44;
			SuffixOption[63] = 2500;
            Weight = 7.0;
        }

        public ChainChest(Serial serial)
            : base(serial)
        {
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