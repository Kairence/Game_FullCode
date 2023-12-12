using System;

namespace Server.Items
{
    [FlipableAttribute(0x2641, 0x2642)]
    public class RedDragonChest : BaseArmor
    {
        [Constructable]
        public RedDragonChest()
            : base(0x2641)
        {
			PrefixOption[50] = 8;
			PrefixOption[61] = 13;
			SuffixOption[61] = 1000;
			PrefixOption[62] = 33;
			SuffixOption[62] = 2000;
            Weight = 10.0;
        }

        public RedDragonChest(Serial serial)
            : base(serial)
        {
        }

 		public override int LabelNumber { get { return 1109669; } }
		public override int AosStrReq { get { return 800; } }
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
                return CraftResource.RedScales;
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