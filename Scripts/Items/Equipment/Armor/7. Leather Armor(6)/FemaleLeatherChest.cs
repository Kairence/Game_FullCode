using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefTailoring), typeof(FemaleGargishLeatherChest))]
    [FlipableAttribute(0x1c06, 0x1c07)]
    public class FemaleLeatherChest : BaseArmor
    {
        [Constructable]
        public FemaleLeatherChest()
            : base(0x1C06)
        {
            Weight = 17.0;
			PrefixOption[50] = 4;
			PrefixOption[61] = 114;
			SuffixOption[61] = 20000;
			PrefixOption[62] = 19;
			SuffixOption[62] = 300000;
			PrefixOption[63] = 20;
			SuffixOption[63] = 300000;
			PrefixOption[64] = 21;
			SuffixOption[64] = 300000;
        }

        public FemaleLeatherChest(Serial serial)
            : base(serial)
        {
        }
        public override int InitMinHits { get { return 100; } }
        public override int InitMaxHits { get { return 100; } }

        public override int AosStrReq { get { return 2100; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase
        {
            get
            {
                return 6;
            }
        }
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Leather;
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
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}