using System;

namespace Server.Items
{
    [FlipableAttribute(0x13ee, 0x13ef)]
    public class RingmailArms : BaseArmor
    {
        [Constructable]
        public RingmailArms()
            : base(0x13EE)
        {
			PrefixOption[50] = 15;
			PrefixOption[61] = 114;
			SuffixOption[61] = 300;
			PrefixOption[62] = 7;
			SuffixOption[62] = 1000;
			PrefixOption[63] = 40;
			SuffixOption[63] = 500;
            Weight = 15.0;
        }

        public RingmailArms(Serial serial)
            : base(serial)
        {
        }

		public override int AosStrReq { get { return 1400; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase
        {
            get
            {
                return 10;
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