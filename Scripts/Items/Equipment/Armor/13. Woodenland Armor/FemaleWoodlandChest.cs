using System;

namespace Server.Items
{
    //Is this a filler-type item? the clilocs don't match up and at a glacnce I can't find direct reference of it
    [FlipableAttribute(0x2B6D, 0x3164)]
    public class FemaleElvenPlateChest : BaseArmor
    {
        [Constructable]
        public FemaleElvenPlateChest()
            : base(0x2B6D)
        {
			PrefixOption[50] = 18;
			PrefixOption[61] = 114;
			SuffixOption[61] = 200;
			PrefixOption[62] = 100;
			SuffixOption[62] = 1000;			
			PrefixOption[63] = 106;
			SuffixOption[63] = 50;
            this.Weight = 8.0;
        }

        public FemaleElvenPlateChest(Serial serial)
            : base(serial)
        {
        }

		public override int AosStrReq { get { return 2100; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 2100; } }
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