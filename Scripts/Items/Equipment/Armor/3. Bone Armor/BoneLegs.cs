using System;

namespace Server.Items
{
    [FlipableAttribute(0x1452, 0x1457)]
    public class BoneLegs : BaseArmor
    {
		public override int AosStrReq { get { return 400; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase { get { return 3; } }
        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Bone; } }
		public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }
		
        [Constructable]
        public BoneLegs()
            : base(0x1452)
        {
            Weight = 3.0;
			PrefixOption[50] = 7;
			PrefixOption[61] = 117;
			SuffixOption[61] = 500;
			PrefixOption[62] = 35;
			SuffixOption[62] = 1000;
        }

        public BoneLegs(Serial serial)
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