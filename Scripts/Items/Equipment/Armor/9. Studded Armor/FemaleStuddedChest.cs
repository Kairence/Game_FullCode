using System;

namespace Server.Items
{
    [FlipableAttribute(0x1c02, 0x1c03)]
    public class FemaleStuddedChest : BaseArmor
    {
        [Constructable]
        public FemaleStuddedChest()
            : base(0x1C02)
        {
			ArmorAttributes.PierceResist += 15;
            Weight = 6.0;
        }

        public FemaleStuddedChest(Serial serial)
            : base(serial)
        {
        }


        public override int InitMinHits
        {
            get
            {
                return 35;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 45;
            }
        }
        public override int AosStrReq
        {
            get
            {
                return 85;
            }
        }
        public override int OldStrReq
        {
            get
            {
                return 35;
            }
        }
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
                return ArmorMaterialType.Studded;
            }
        }
        public override CraftResource DefaultResource
        {
            get
            {
                return CraftResource.RegularLeather;
            }
        }
        public override ArmorMeditationAllowance DefMedAllowance
        {
            get
            {
                return ArmorMeditationAllowance.Half;
            }
        }
        public override bool AllowMaleWearer
        {
            get
            {
                return false;
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