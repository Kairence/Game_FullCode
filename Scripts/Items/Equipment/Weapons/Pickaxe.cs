using System;
using Server.Engines.Harvest;

namespace Server.Items
{
    [FlipableAttribute(0xE86, 0xE85)]
    public class Pickaxe : BaseHarvestTool
    {
		public override HarvestSystem HarvestSystem { get { return Mining.System; } }

        [Constructable]
        public Pickaxe()
            : this(75)
        {
        }

        [Constructable]
        public Pickaxe(int uses)
            : base(uses, 0xE86)
        {
            Weight = 11.0;
        }

        public Pickaxe(Serial serial)
            : base(serial)
        {
        }

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