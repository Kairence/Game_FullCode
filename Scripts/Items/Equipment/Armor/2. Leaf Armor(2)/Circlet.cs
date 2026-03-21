using System;

namespace Server.Items
{
    [FlipableAttribute(0x2B6E, 0x3165)]
    public class Circlet : BaseArmor
    {
        [Constructable]
        public Circlet()
            : base(0x2B6E)
        {
            Weight = 4.0;
			PrefixOption[50] = 1;    //세트 옵션 번호
        }

        public Circlet(Serial serial)
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
				return 1.0; // 원하는 감소 수치를 입력하세요.
			}
        }
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Cloth;
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

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}