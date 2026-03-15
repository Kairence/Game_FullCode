using System;

namespace Server.Items
{
    [FlipableAttribute(0x13be, 0x13c3)]
    public class ChainLegs : BaseArmor
    {
        [Constructable]
        public ChainLegs()
            : base(0x13BE)
        {
			PrefixOption[50] = 16;	 //세트 옵션 번호

            this.Weight = 30.0;
        }

        public ChainLegs(Serial serial)
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
                return 3500;
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
                return 11;
            }
        }
		public override double ArmorRating
		{
			get
			{
				return 5.0; // 원하는 감소 수치를 입력하세요.
			}
		}
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Chainmail;
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