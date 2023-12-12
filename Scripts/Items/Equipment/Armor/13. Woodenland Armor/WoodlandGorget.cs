using System;

namespace Server.Items
{
    [FlipableAttribute(0x2B69, 0x3160)]
    public class WoodlandGorget : BaseArmor
    {
        [Constructable]
        public WoodlandGorget()
            : base(0x2B69)
        {
			PrefixOption[50] = 18;
			PrefixOption[61] = 114;
			SuffixOption[61] = 200;
			PrefixOption[62] = 100;
			SuffixOption[62] = 1000;			
			PrefixOption[63] = 106;
			SuffixOption[63] = 50;
			
        }

        public WoodlandGorget(Serial serial)
            : base(serial)
        {
        }
		public override int AosStrReq { get { return 1400; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 1400; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase
        {
            get
            {
                return 13;
            }
        }
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Wood;
            }
        }
        public override Race RequiredRace
        {
            get
            {
                return Race.Elf;
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            if (version == 0)
                this.Weight = -1;
        }
    }
}