using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefTailoring), typeof(GargishLeatherLegs))]
    [FlipableAttribute(0x2FC9, 0x317F)]
    public class LeafLegs : BaseArmor
    {
        [Constructable]
        public LeafLegs()
            : base(0x2FC9)
        {
            this.Weight = 4.0;
            PrefixOption[50] = 1;    //세트 옵션 번호
        }

        public LeafLegs(Serial serial)
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
        public override CraftResource DefaultResource
        {
            get
            {
                return CraftResource.RegularLeather;
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

            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}