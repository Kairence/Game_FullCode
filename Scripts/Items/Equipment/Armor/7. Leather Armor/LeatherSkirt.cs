using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefTailoring), typeof(FemaleGargishLeatherLegs))]
    [FlipableAttribute(0x1c08, 0x1c09)]
    public class LeatherSkirt : BaseArmor
    {
        [Constructable]
        public LeatherSkirt()
            : base(0x1C08)
        {
            Weight = 1.0;
			PrefixOption[50] = 4;
			PrefixOption[61] = 114;
			SuffixOption[61] = 200;
			PrefixOption[62] = 19;
			SuffixOption[62] = 25;
			PrefixOption[63] = 20;
			SuffixOption[63] = 25;
			PrefixOption[64] = 21;
			SuffixOption[64] = 25;
        }

        public LeatherSkirt(Serial serial)
            : base(serial)
        {
        }

        public override int InitMinHits { get { return 100; } }
        public override int InitMaxHits { get { return 100; } }

        public override int AosStrReq { get { return 600; } }
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