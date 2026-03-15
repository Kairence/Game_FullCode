using System;

namespace Server.Items
{
    [Flipable(0x2645, 0x2646)]
    public class RedDragonHelm : BaseArmor
    {
        [Constructable]
        public RedDragonHelm()
            : base(0x2645)
        {
            Weight = 12.0;
			PrefixOption[50] = 8;    //세트 옵션 번호
        }

        public RedDragonHelm(Serial serial)
            : base(serial)
        {
        }

 		public override int LabelNumber { get { return 1029797; } }
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
                return 2500;
            }
        }
        public override int AosDexReq
        {
            get
            {
                return 2500;
            }
        }
        public override int AosIntReq
        {
            get
            {
                return 2500;
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
                return CraftResource.RedScales;
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