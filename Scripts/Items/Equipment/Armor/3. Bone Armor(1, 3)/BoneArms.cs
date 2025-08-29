using System;

namespace Server.Items
{
    [FlipableAttribute(0x144e, 0x1453)]
    public class BoneArms : BaseArmor
    {
		public override int AosStrReq { get { return 750; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase { get { return 1; } }
        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Bone; } }
		public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }
		
        [Constructable]
        public BoneArms()
            : base(0x144E)
        {
            Weight = 4.0;

			PrefixOption[50] = 7;
			PrefixOption[61] = 117;
			SuffixOption[61] = 100000;
			PrefixOption[62] = 118;
			SuffixOption[62] = 50000;
			PrefixOption[63] = 12;
			SuffixOption[63] = 10000;
        }

        public BoneArms(Serial serial)
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