using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(GargishPlateKilt))]
    [FlipableAttribute(0x13eb, 0x13f2)]
    public class RingmailGloves : BaseArmor
    {
        [Constructable]
        public RingmailGloves()
            : base(0x13EB)
        {
			PrefixOption[50] = 15;	 //세트 옵션 번호
			PrefixOption[61] = 7;	 //무기 피해%
			SuffixOption[61] = 100000; //10%
			PrefixOption[62] = 40;	 //공격 속도%
			SuffixOption[62] = 50000; //5%
            Weight = 20.0;
        }

        public RingmailGloves(Serial serial)
            : base(serial)
        {
        }
		public override int AosStrReq { get { return 1500; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
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