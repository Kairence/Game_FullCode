using System;
using Server.Items;

namespace Server.Items
{
    public class DragonTurtleHideBustier : BaseArmor
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

        // We like to cross dress here!
        public override bool AllowMaleWearer { get { return true; } }

        public override int LabelNumber { get { return 1109635; } } // Dragon Turtle Hide Bustier

        [Constructable]
        public DragonTurtleHideBustier()
            : base(0x782B)
        {
            Weight = 25.0;
            PrefixOption[50] = 3;    //세트 옵션 번호
        }

        public DragonTurtleHideBustier(Serial serial)
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
