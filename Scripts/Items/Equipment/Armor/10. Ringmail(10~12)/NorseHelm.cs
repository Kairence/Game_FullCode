using System;

namespace Server.Items
{
    public class NorseHelm : BaseArmor
    {
        [Constructable]
        public NorseHelm()
            : base(0x140E)
        {
			PrefixOption[50] = 15;
			PrefixOption[61] = 114;
			SuffixOption[61] = 30000;
			PrefixOption[62] = 7;
			SuffixOption[62] = 100000;
			PrefixOption[63] = 40;
			SuffixOption[63] = 50000;
            Weight = 15.0;
        }

        public NorseHelm(Serial serial)
            : base(serial)
        {
        }

		public override int AosStrReq { get { return 2250; } }
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