using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(GargishPlateArms))]
    [FlipableAttribute(0x1410, 0x1417)]
    public class PlateArms : BaseArmor
    {
        [Constructable]
        public PlateArms()
            : base(0x1410)
        {
			PrefixOption[50] = 17;
			PrefixOption[61] = 114;
			SuffixOption[61] = 800;
			PrefixOption[62] = 4;
			SuffixOption[62] = 25000;			
			PrefixOption[63] = 5;
			SuffixOption[63] = 25000;				
            Weight = 5.0;
        }

        public PlateArms(Serial serial)
            : base(serial)
        {
        }

		public override int AosStrReq { get { return 3800; } }
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