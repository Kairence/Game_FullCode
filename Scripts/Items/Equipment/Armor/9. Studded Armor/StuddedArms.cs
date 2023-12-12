using System;

namespace Server.Items
{
    [FlipableAttribute(0x13dc, 0x13d4)]
    public class StuddedArms : BaseArmor
    {
        [Constructable]
        public StuddedArms()
            : base(0x13DC)
        {
            Weight = 4.0;
			PrefixOption[50] = 6;
			PrefixOption[61] = 12;
			SuffixOption[61] = 300;
			PrefixOption[62] = 111;
			SuffixOption[62] = 200;
			PrefixOption[63] = 3;
			SuffixOption[63] = 100;

       }

        public StuddedArms(Serial serial)
            : base(serial)
        {
        }

        public override int InitMinHits { get { return 100; } }
        public override int InitMaxHits { get { return 100; } }

        public override int AosStrReq { get { return 1000; } }
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