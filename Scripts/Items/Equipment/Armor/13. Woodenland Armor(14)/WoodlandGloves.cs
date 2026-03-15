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

            this.Weight = 25.0;
        }

        public WoodlandGloves(Serial serial)
            : base(serial)
        {
        }
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
                return 10;
            }
        }
		public override double ArmorRating
		{
			get
			{
				return 7.0; // 원하는 감소 수치를 입력하세요.
			}
		}
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Wood;
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