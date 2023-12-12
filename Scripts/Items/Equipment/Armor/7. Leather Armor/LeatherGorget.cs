using System;

namespace Server.Items
{
    public class LeatherGorget : BaseArmor
    {
        [Constructable]
        public LeatherGorget()
            : base(0x13C7)
        {
            this.Weight = 1.0;
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

        public LeatherGorget(Serial serial)
            : base(serial)
        {
        }

        public override int InitMinHits { get { return 100; } }
        public override int InitMaxHits { get { return 100; } }

        public override int AosStrReq { get { return 400; } }
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