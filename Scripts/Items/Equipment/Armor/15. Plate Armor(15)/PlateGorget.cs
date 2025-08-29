using System;

namespace Server.Items
{
    public class PlateGorget : BaseArmor
    {
        [Constructable]
        public PlateGorget()
            : base(0x1413)
        {
			PrefixOption[50] = 17;
			PrefixOption[61] = 114;
			SuffixOption[61] = 80000;
			PrefixOption[62] = 110;
			SuffixOption[62] = 50000;			
			PrefixOption[63] = 106;
			SuffixOption[63] = 5000;				
            Weight = 30.0;

        }

        public PlateGorget(Serial serial)
            : base(serial)
        {
        }
		public override int AosStrReq { get { return 3750; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }
        public override int ArmorBase
        {
            get
            {
                return 15;
            }
        }
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Plate;
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