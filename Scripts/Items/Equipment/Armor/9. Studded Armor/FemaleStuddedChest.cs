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
			PrefixOption[50] = 6;
			PrefixOption[61] = 12;
			SuffixOption[61] = 300;
			PrefixOption[62] = 111;
			SuffixOption[62] = 200;
			PrefixOption[63] = 3;
			SuffixOption[63] = 100;

            Weight = 6.0;
        }

        public FemaleStuddedChest(Serial serial)
            : base(serial)
        {
        }

        public override int InitMinHits { get { return 100; } }
        public override int InitMaxHits { get { return 100; } }

        public override int AosStrReq { get { return 1500; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
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