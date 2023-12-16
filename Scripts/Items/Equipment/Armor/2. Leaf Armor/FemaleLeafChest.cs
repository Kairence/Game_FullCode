using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefTailoring), typeof(FemaleGargishLeatherChest))]
    [FlipableAttribute(0x2FCB, 0x3181)]
    public class FemaleLeafChest : BaseArmor
    {
        [Constructable]
        public FemaleLeafChest()
            : base(0x2FCB)
        {
            this.Weight = 2.0;
			PrefixOption[50] = 1;
			PrefixOption[61] = 12;
			SuffixOption[61] = 200;
			PrefixOption[62] = 14;
			SuffixOption[62] = 200;
			PrefixOption[63] = 6;
			SuffixOption[63] = 25000;
        }

        public FemaleLeafChest(Serial serial)
            : base(serial)
        {
        }
        public override int InitMinHits
        {
            get
            {
                return 100;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 100;
            }
        }
        public override int AosStrReq
        {
            get
            {
                return 100;
            }
        }
        public override int AosDexReq
        {
            get
            {
                return 100;
            }
        }
        public override int AosIntReq
        {
            get
            {
                return 100;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 2;
            }
        }
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Cloth;
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
                return ArmorMeditationAllowance.All;
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

            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}