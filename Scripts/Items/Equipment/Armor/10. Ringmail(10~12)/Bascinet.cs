using System;

namespace Server.Items
{
    public class Bascinet : BaseArmor
    {
		public override int AosStrReq { get { return 2000; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase { get { return 11; } }
        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Ringmail; } }
		
        [Constructable]
        public Bascinet()
            : base(0x140C)
        {
			PrefixOption[50] = 15;
			PrefixOption[61] = 114;
			SuffixOption[61] = 100000;
			PrefixOption[62] = 4;
			SuffixOption[62] = 5000000;
			PrefixOption[63] = 6;
			SuffixOption[63] = 5000000;
            Weight = 18.0;
        }

        public Bascinet(Serial serial)
            : base(serial)
        {
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