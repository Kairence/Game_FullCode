using System;

namespace Server.Items
{
    [FlipableAttribute(0x2B67, 0x315E)]
    public class WoodlandChest : BaseArmor
    {
        [Constructable]
        public WoodlandChest()
            : base(0x2B67)
        {
			PrefixOption[50] = 18;
			PrefixOption[61] = 114;
			SuffixOption[61] = 20000;
			PrefixOption[62] = 4;
			SuffixOption[62] = 20000;			
			PrefixOption[63] = 5;
			SuffixOption[63] = 20000;
			PrefixOption[64] = 6;
			SuffixOption[64] = 20000;
            this.Weight = 40.0;

        }

        public WoodlandChest(Serial serial)
            : base(serial)
        {
        }

		public override int AosStrReq { get { return 4500; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase
        {
            get
            {
                return 14;
            }
        }
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Wood;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}