using System;

namespace Server.Items
{
    [FlipableAttribute(0x2647, 0x2648)]
    public class BlackDragonLegs : BaseArmor
    {
        [Constructable]
        public BlackDragonLegs()
            : base(0x2647)
        {
			PrefixOption[50] = 13;
			PrefixOption[61] = 107;
			SuffixOption[61] = 100000;
			PrefixOption[62] = 110;
			SuffixOption[62] = 50000;
            this.Weight = 20.0;
        }

        public BlackDragonLegs(Serial serial)
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
                return CraftResource.BlackScales;
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