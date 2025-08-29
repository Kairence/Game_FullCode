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
			PrefixOption[61] = 114;
			SuffixOption[61] = 30000;
			PrefixOption[62] = 42;
			SuffixOption[62] = 50000;
			PrefixOption[63] = 44;
			SuffixOption[63] = 50000;
			Weight = 15.0;
        }

        public CloseHelm(Serial serial)
            : base(serial)
        {
        }
		public override int AosStrReq { get { return 2500; } }
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