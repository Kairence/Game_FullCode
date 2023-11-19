using System;

namespace Server.Items
{
    [FlipableAttribute(0x1450, 0x1455)]
    public class BoneGloves : BaseArmor
    {
        public override int InitMinHits { get { return 25; } }
        public override int InitMaxHits { get { return 30; } }
        public override int AosStrReq { get { return 15; } }
        public override int OldStrReq { get { return 40; } }
		public override int OldDexBonus { get { return -4; } }
        public override int ArmorBase { get { return 3; } }
        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Bone; } }
		public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }
		
        [Constructable]
        public BoneGloves()
            : base(0x1450)
        {
  			AbsorptionAttributes.EaterPoison += 7;
            Weight = 2.0;
        }

        public BoneGloves(Serial serial)
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