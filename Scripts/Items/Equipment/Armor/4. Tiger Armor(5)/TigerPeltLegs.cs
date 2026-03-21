using System;
using Server.Items;

namespace Server.Items
{
    public class TigerPeltLegs : BaseArmor
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
                return 2500;
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
                return 5;
            }
        }
		public override double ArmorRating
		{
			get
			{
				return 3.0; // 원하는 감소 수치를 입력하세요.
			}
		}

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Leather; } }
        public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

        public override ArmorMeditationAllowance DefMedAllowance { get { return ArmorMeditationAllowance.All; } }

        public override bool AllowMaleWearer { get { return true; } }

        public override int LabelNumber { get { return 1109628; } } // Tiger Pelt Leggins

        [Constructable]
        public TigerPeltLegs()
            : base(0x7824)
        {
            Weight = 13.0;
            PrefixOption[50] = 2;    //세트 옵션 번호
       }

        public TigerPeltLegs(Serial serial)
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