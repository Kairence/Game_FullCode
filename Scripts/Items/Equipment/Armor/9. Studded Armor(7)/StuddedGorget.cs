using System;

namespace Server.Items
{
    public class StuddedGorget : BaseArmor
    {
        [Constructable]
        public StuddedGorget()
            : base(0x13D6)
        {
            Weight = 15.0;
			PrefixOption[50] = 6;    //세트 옵션 번호
			PrefixOption[61] = 118;  //모든 속도%
			SuffixOption[61] = 50000; //5%
			PrefixOption[62] = 117;  //모든 피해%
			SuffixOption[62] = 50000; //5%

        }

        public StuddedGorget(Serial serial)
            : base(serial)
        {
        }

        public override int InitMinHits { get { return 100; } }
        public override int InitMaxHits { get { return 100; } }

        public override int AosStrReq { get { return 1150; } }
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