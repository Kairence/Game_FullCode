using System;

namespace Server.Items
{
    [FlipableAttribute(0x2B6C, 0x3163)]
    public class WoodlandArms : BaseArmor
    {
        [Constructable]
        public WoodlandArms()
            : base(0x2B6C)
        {
			PrefixOption[50] = 18;
			PrefixOption[61] = 114;
			SuffixOption[61] = 200;
			PrefixOption[62] = 100;
			SuffixOption[62] = 1000;			
			PrefixOption[63] = 106;
			SuffixOption[63] = 50;
            this.Weight = 5.0;
        }

        public WoodlandArms(Serial serial)
            : base(serial)
        {
        }
		public override int AosStrReq { get { return 1700; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 1700; } }
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

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}