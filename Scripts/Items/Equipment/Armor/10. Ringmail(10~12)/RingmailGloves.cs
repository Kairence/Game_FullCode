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
			PrefixOption[50] = 15;
			PrefixOption[61] = 114;
			SuffixOption[61] = 30000;
			PrefixOption[62] = 7;
			SuffixOption[62] = 100000;
			PrefixOption[63] = 40;
			SuffixOption[63] = 50000;
            Weight = 10.0;
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
                return 12;
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