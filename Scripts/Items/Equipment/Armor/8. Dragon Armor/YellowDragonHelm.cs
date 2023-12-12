using System;

namespace Server.Items
{
    [Flipable(0x2645, 0x2646)]
    public class YellowDragonHelm : BaseArmor
    {
        [Constructable]
        public YellowDragonHelm()
            : base(0x2645)
        {
			PrefixOption[50] = 11;
			PrefixOption[61] = 16;
			SuffixOption[61] = 1000;
			PrefixOption[62] = 36;
			SuffixOption[62] = 2000;
            Weight = 5.0;
        }

        public YellowDragonHelm(Serial serial)
            : base(serial)
        {
        }

 		public override int LabelNumber { get { return 1109681; } }
		public override int AosStrReq { get { return 500; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase
        {
            get
            {
                return 8;
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
                return CraftResource.YellowScales;
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