using System;

namespace Server.Items
{
    [FlipableAttribute(0x2647, 0x2648)]
    public class WhiteDragonLegs : BaseArmor
    {
        [Constructable]
        public WhiteDragonLegs()
            : base(0x2647)
        {
			PrefixOption[50] = 12;
			PrefixOption[61] = 12;
			SuffixOption[61] = 100000;
			PrefixOption[62] = 32;
			SuffixOption[62] = 200000;
            this.Weight = 20.0;
        }

        public WhiteDragonLegs(Serial serial)
            : base(serial)
        {
        }

 		public override int LabelNumber { get { return 1029799; } }
		public override int AosStrReq { get { return 3500; } }
        public override int AosDexReq { get { return 3500; } }
        public override int AosIntReq { get { return 3500; } }
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
                return CraftResource.WhiteScales;
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