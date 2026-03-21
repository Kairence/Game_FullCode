using System;
using Server;

namespace Server.Items
{
    public class DragonTurtleHideHelm : BaseArmor
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
                return 8;
            }
        }
		public override double ArmorRating
		{
			get
			{
				return 7.0; // 원하는 감소 수치를 입력하세요.
			}
		}

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Leather; } }
        public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

        public override ArmorMeditationAllowance DefMedAllowance { get { return ArmorMeditationAllowance.All; } }

        public override int LabelNumber { get { return 1109637; } } // Dragon Turtle Hide Helm

        [Constructable]
        public DragonTurtleHideHelm()
            : base(0x782D)
        {
            Weight = 15.0;
            PrefixOption[50] = 3;    //세트 옵션 번호
            PrefixOption[61] = 18;   //방어율 증가
            SuffixOption[61] = 50000; //5%
            PrefixOption[62] = 100;  //무기 공격 반사%
            SuffixOption[62] = 250000; //25%
        }

        public DragonTurtleHideHelm(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}