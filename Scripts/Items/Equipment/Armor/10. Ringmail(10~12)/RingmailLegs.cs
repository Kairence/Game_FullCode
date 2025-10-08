using System;

namespace Server.Items
{
    [FlipableAttribute(0x13f0, 0x13f1)]
    public class RingmailLegs : BaseArmor
    {
        [Constructable]
        public RingmailLegs()
            : base(0x13F0)
        {
			PrefixOption[50] = 15;	 //세트 옵션 번호
			PrefixOption[61] = 7;	 //무기 피해%
			SuffixOption[61] = 100000; //10%
			PrefixOption[62] = 40;	 //공격 속도%
			SuffixOption[62] = 50000; //5%

            Weight = 25.0;
        }

        public RingmailLegs(Serial serial)
            : base(serial)
        {
        }

		public override int AosStrReq { get { return 3000; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase
        {
            get
            {
                return 8;
            }
        }

        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Ringmail;
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