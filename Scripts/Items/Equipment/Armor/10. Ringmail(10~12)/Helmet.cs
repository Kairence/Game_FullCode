using System;

namespace Server.Items
{
    public class Helmet : BaseArmor
    {
        [Constructable]
        public Helmet()
            : base(0x140A)
        {
			PrefixOption[50] = 15;	 //세트 옵션 번호
			PrefixOption[61] = 5;	 //기력
			SuffixOption[61] = 2500000; //250
			PrefixOption[62] = 99;	 //명중율% (임의 코드 99)
			SuffixOption[62] = 100000; //10%
            Weight = 20.0;
        }

        public Helmet(Serial serial)
            : base(serial)
        {
        }

		public override int AosStrReq { get { return 1750; } }
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
                return ArmorMaterialType.Ringmail;
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