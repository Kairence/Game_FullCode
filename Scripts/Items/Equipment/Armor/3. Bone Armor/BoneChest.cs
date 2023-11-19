using System;

namespace Server.Items
{
    [FlipableAttribute(0x144f, 0x1454)]
    public class BoneChest : BaseArmor
    {
        public override int InitMinHits { get { return 25; } }
        public override int InitMaxHits { get { return 30; } }
        public override int AosStrReq { get { return 55; } }
        public override int OldStrReq { get { return 40; } }
		public override int OldDexBonus { get { return -4; } }
        public override int ArmorBase { get { return 3; } }
        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Bone; } }
		public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }
		
        [Constructable]
        public BoneChest()
            : base(0x144F)
        {
            Weight = 6.0;
  			AbsorptionAttributes.ResonancePoison += 140;
        }

        public BoneChest(Serial serial)
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