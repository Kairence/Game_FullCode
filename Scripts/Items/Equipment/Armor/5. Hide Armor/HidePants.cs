using System;

namespace Server.Items
{
    [FlipableAttribute(0x2B78, 0x316F)]
    public class HidePants : BaseArmor
    {
        [Constructable]
        public HidePants()
            : base(0x2B78)
        {
            this.Weight = 5.0;
			PrefixOption[50] = 5;
			PrefixOption[61] = 114;
			SuffixOption[61] = 100;
			PrefixOption[62] = 116;
			SuffixOption[62] = 500;
			PrefixOption[63] = 41;
			SuffixOption[63] = 250;
       }

        public HidePants(Serial serial)
            : base(serial)
        {
        }


        public override int InitMinHits { get { return 100; } }
        public override int InitMaxHits { get { return 100; } }

        public override int AosStrReq { get { return 500; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase
        {
            get
            {
                return 5;
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
                return ArmorMeditationAllowance.Half;
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