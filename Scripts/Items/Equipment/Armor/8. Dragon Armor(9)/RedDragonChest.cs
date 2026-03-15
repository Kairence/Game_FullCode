using System;

namespace Server.Items
{
    [FlipableAttribute(0x2641, 0x2642)]
    public class RedDragonChest : BaseArmor
    {
        [Constructable]
        public RedDragonChest()
            : base(0x2641)
        {
            Weight = 25.0;
			PrefixOption[50] = 8;    //세트 옵션 번호
        }

        public RedDragonChest(Serial serial)
            : base(serial)
        {
        }

 		public override int LabelNumber { get { return 1029793; } }
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
                return 4000;
            }
        }
        public override int AosDexReq
        {
            get
            {
                return 4000;
            }
        }
        public override int AosIntReq
        {
            get
            {
                return 4000;
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