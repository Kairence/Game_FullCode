using System;

namespace Server.Items
{
    public class CloseHelm : BaseArmor
    {
        [Constructable]
        public CloseHelm()
            : base(0x1408)
        {
			PrefixOption[50] = 15;
			PrefixOption[61] = 7;
			SuffixOption[61] = 2000;
			PrefixOption[62] = 40;
			SuffixOption[62] = 1000;
			Weight = 5.0;
        }

        public CloseHelm(Serial serial)
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