using System;

namespace Server.Items
{
    [Flipable(0x2645, 0x2646)]
    public class BlueDragonHelm : BaseArmor
    {
        [Constructable]
        public BlueDragonHelm()
            : base(0x2645)
        {
			PrefixOption[50] = 9;
			PrefixOption[61] = 14;
			SuffixOption[61] = 100000;
			PrefixOption[62] = 34;
			SuffixOption[62] = 200000;
            Weight = 12.0;
        }

        public BlueDragonHelm(Serial serial)
            : base(serial)
        {
        }

 		public override int LabelNumber { get { return 1029797; } }
		public override int AosStrReq { get { return 2500; } }
        public override int AosDexReq { get { return 2500; } }
        public override int AosIntReq { get { return 2500; } }
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
                return CraftResource.BlueScales;
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