using System;

namespace Server.Items
{
    [FlipableAttribute(0x1450, 0x1455)]
    public class DaemonGloves : BaseArmor
    {
        [Constructable]
        public DaemonGloves()
            : base(0x1450)
        {
            this.Weight = 5.0;
            this.Hue = 0x648;
			PrefixOption[50] = 14;   //세트 옵션 번호
        }

        public DaemonGloves(Serial serial)
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
                return 2222;
            }
        }
        public override int AosDexReq
        {
            get
            {
                return 2222;
            }
        }
        public override int AosIntReq
        {
            get
            {
                return 2222;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 6;
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
                return ArmorMaterialType.Bone;
            }
        }
        public override CraftResource DefaultResource
        {
            get
            {
                return CraftResource.RegularLeather;
            }
        }
        public override int LabelNumber
        {
            get
            {
                return 1041373;
            }
        }// daemon bone gloves
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (this.Weight == 1.0)
                this.Weight = 2.0;

            if (this.ArmorAttributes.SelfRepair == 0)
                this.ArmorAttributes.SelfRepair = 1;
        }
    }
}