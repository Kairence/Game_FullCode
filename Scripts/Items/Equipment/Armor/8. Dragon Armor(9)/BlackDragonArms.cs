using System;

namespace Server.Items
{
    [FlipableAttribute(0x2657, 0x2658)]
    public class BlackDragonArms : BaseArmor
    {
        [Constructable]
        public BlackDragonArms()
            : base(0x2657)
        {
			PrefixOption[50] = 13;   //세트 옵션 번호
			PrefixOption[61] = 27;   //혼돈 피해 증가
			SuffixOption[61] = 100000; //10
			PrefixOption[62] = 120;  //모든 피격 피해 감소%
			SuffixOption[62] = 50000; //5%
            Weight = 15.0;
        }

        public BlackDragonArms(Serial serial)
            : base(serial)
        {
        }

 		public override int LabelNumber { get { return 1029815; } }
		public override int AosStrReq { get { return 3000; } }
        public override int AosDexReq { get { return 3000; } }
        public override int AosIntReq { get { return 3000; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase
        {
            get
            {
                return 7;
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
                return CraftResource.BlackScales;
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