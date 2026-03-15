using System;

namespace Server.Items
{
    [FlipableAttribute(0x2643, 0x2644)]
    public class GreenDragonGloves : BaseArmor
    {
        [Constructable]
        public GreenDragonGloves()
            : base(0x2643)
        {
			PrefixOption[50] = 10;   //세트 옵션 번호
			PrefixOption[61] = 15;   //독 저항
			SuffixOption[61] = 250000; //25%
			PrefixOption[62] = 25;   //독 피해 증가
			SuffixOption[62] = 200000; //20
            Weight = 10.0;
        }

        public GreenDragonGloves(Serial serial)
            : base(serial)
        {
        }

 		public override int LabelNumber { get { return 1029795; } }
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
                return 2000;
            }
        }
        public override int AosDexReq
        {
            get
            {
                return 2000;
            }
        }
        public override int AosIntReq
        {
            get
            {
                return 2000;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 7;
            }
        }
		public override double ArmorRating
		{
			get
			{
				return 10.0; // 원하는 감소 수치를 입력하세요.
			}
		}
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Dragon;
            }
        }
        public override CraftResource DefaultResource
        {
            get
            {
                return CraftResource.GreenScales;
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