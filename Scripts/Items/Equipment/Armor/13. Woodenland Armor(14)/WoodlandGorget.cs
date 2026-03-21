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
			PrefixOption[50] = 18;	 //세트 옵션 번호

            this.Weight = 30.0;

        }

        public WoodlandGorget(Serial serial)
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