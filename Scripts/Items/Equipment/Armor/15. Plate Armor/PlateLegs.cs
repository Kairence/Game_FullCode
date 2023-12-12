using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(GargishPlateLegs))]
    [FlipableAttribute(0x1411, 0x141a)]
    public class PlateLegs : BaseArmor
    {
        [Constructable]
        public PlateLegs()
            : base(0x1411)
        {
			PrefixOption[50] = 17;
			PrefixOption[61] = 114;
			SuffixOption[61] = 800;
			PrefixOption[62] = 4;
			SuffixOption[62] = 25000;			
			PrefixOption[63] = 5;
			SuffixOption[63] = 25000;			
            Weight = 17.0;
        }

        public PlateLegs(Serial serial)
            : base(serial)
        {
        }

		public override int AosStrReq { get { return 4200; } }
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