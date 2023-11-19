using System;
using Server.Engines.Harvest;

namespace Server.Items
{
    public class SturdyPickaxe : BaseHarvestTool
    {
		public override HarvestSystem HarvestSystem { get { return Mining.System; } }

        [Constructable]
        public SturdyPickaxe()
            : this(250)
        {
        }

        [Constructable]
        public SturdyPickaxe(int uses)
            : base(0xE86)
        {
            Weight = 11.0;
            Hue = 0x973;

        }

        public SturdyPickaxe(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1045126;
            }
        }// sturdy pickaxe

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}
