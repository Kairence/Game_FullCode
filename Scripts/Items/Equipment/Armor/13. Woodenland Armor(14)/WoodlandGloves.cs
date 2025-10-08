using System;

namespace Server.Items
{
    [FlipableAttribute(0x2B6A, 0x3161)]
    public class WoodlandGloves : BaseArmor
    {
        [Constructable]
        public WoodlandGloves()
            : base(0x2B6A)
        {
			PrefixOption[50] = 18;	 //세트 옵션 번호
			PrefixOption[61] = 4;	 //체력
			SuffixOption[61] = 2000000; //200
			PrefixOption[62] = 5;	 //기력
			SuffixOption[62] = 2000000; //200
			PrefixOption[63] = 6;	 //마나
			SuffixOption[63] = 2000000; //200
            this.Weight = 15.0;
        }

        public WoodlandGloves(Serial serial)
            : base(serial)
        {
        }
		public override int AosStrReq { get { return 2000; } }
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