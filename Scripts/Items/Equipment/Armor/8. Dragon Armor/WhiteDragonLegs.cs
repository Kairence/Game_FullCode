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
			PrefixOption[61] = 41;
			SuffixOption[61] = 800;
			PrefixOption[62] = 116;
			SuffixOption[62] = 2000;
            this.Weight = 6.0;
        }

        public WhiteDragonLegs(Serial serial)
            : base(serial)
        {
        }

 		public override int LabelNumber { get { return 1109688; } }
		public override int AosStrReq { get { return 700; } }
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