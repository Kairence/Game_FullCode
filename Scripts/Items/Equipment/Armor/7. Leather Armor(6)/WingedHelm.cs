using System;

namespace Server.Items
{
    [FlipableAttribute(0x2B73, 0x316A)]
    public class WingedHelm : BaseArmor
    {
        public override int InitMinHits
        {
            get
            {
                return 100;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 100;
            }
        }
        public override int AosStrReq
        {
            get
            {
                return 1000;
            }
        }
        public override int AosDexReq
        {
            get
            {
                return 1000;
            }
        }
        public override int AosIntReq
        {
            get
            {
                return 1000;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 2;
            }
        }
		public override double ArmorRating
		{
			get
			{
				return 2.0; // 원하는 감소 수치를 입력하세요.
			}
		}

        [Constructable]
        public WingedHelm()
            : base(0x2B73)
        {
            Weight = 13.0;
			PrefixOption[50] = 4;    //세트 옵션 번호
			PrefixOption[61] = 6;   //마나 회복
			SuffixOption[61] = 5000000; //5
       }

        public WingedHelm(Serial serial)
            : base(serial)
        {
        }
		
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Leather;
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
