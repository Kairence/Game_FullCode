using System;

namespace Server.Items
{
    [FlipableAttribute(0x2657, 0x2658)]
    public class GreenDragonArms : BaseArmor
    {
        [Constructable]
        public GreenDragonArms()
            : base(0x2657)
        {
			PrefixOption[50] = 10;
			PrefixOption[61] = 15;
			SuffixOption[61] = 100000;
			PrefixOption[62] = 35;
			SuffixOption[62] = 200000;
            Weight = 15.0;
        }

        public GreenDragonArms(Serial serial)
            : base(serial)
        {
        }

 		public override int LabelNumber { get { return 1029815; } }
		public override int AosStrReq { get { return 3000; } }
        public override int AosDexReq { get { return 3000; } }
        public override int AosIntReq { get { return 3000; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase
        {
            get
            {
                return 9;
            }
        }
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Dragon;
            }
        }
        public override CraftResource DefaultResource
        {
            get
            {
                return CraftResource.GreenScales;
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