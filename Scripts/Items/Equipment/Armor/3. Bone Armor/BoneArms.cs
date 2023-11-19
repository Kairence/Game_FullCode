using System;

namespace Server.Items
{
    [FlipableAttribute(0x144e, 0x1453)]
    public class BoneArms : BaseArmor
    {
        public override int InitMinHits { get { return 25; } }
        public override int InitMaxHits { get { return 30; } }
        public override int AosStrReq { get { return 35; } }
        public override int OldStrReq { get { return 40; } }
		public override int OldDexBonus { get { return -4; } }
        public override int ArmorBase { get { return 3; } }
        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Bone; } }
		public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }
		
        [Constructable]
        public BoneArms()
            : base(0x144E)
        {
            Weight = 2.0;
  			AbsorptionAttributes.ResonancePoison += 120;
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