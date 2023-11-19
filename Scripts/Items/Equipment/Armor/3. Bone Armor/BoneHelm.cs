using System;

namespace Server.Items
{
    [FlipableAttribute(0x1451, 0x1456)]
    public class BoneHelm : BaseArmor
    {
        public override int InitMinHits { get { return 25; } }
        public override int InitMaxHits { get { return 30; } }
        public override int AosStrReq { get { return 25; } }
        public override int OldStrReq { get { return 40; } }
		public override int OldDexBonus { get { return -4; } }
        public override int ArmorBase { get { return 3; } }
        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Bone; } }
		public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }
		
        [Constructable]
        public BoneHelm()
            : base(0x1451)
        {
  			AbsorptionAttributes.EaterPoison += 7;
            Weight = 3.0;
        }

        public BoneHelm(Serial serial)
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