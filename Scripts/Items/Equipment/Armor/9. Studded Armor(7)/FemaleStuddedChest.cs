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
            Weight = 26.0;
			PrefixOption[50] = 6;
			PrefixOption[61] = 12;
			SuffixOption[61] = 40000;
			PrefixOption[62] = 3;
			SuffixOption[62] = 500000;
			PrefixOption[63] = 5;
			SuffixOption[63] = 2500000;
        }

        public FemaleStuddedChest(Serial serial)
            : base(serial)
        {
        }

        public override int InitMinHits { get { return 100; } }
        public override int InitMaxHits { get { return 100; } }

        public override int AosStrReq { get { return 3250; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase
        {
            get
            {
                return 7;
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