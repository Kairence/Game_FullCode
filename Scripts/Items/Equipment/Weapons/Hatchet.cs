using System;
using Server.Engines.Harvest;

namespace Server.Items
{
    [FlipableAttribute(0xF43, 0xF44)]
    public class Hatchet : BaseHarvestTool
    {
        public override HarvestSystem HarvestSystem
        {
            get
            {
                return Lumberjacking.System;
            }
        }
        [Constructable]
        public Hatchet()
            : this(500)
        {
        }
		
        [Constructable]
        public Hatchet(int uses)
            : base(uses, 0xF43)
        {
            Layer = Layer.OneHanded;
            Weight = 8.0;
            this.Weight = 4.0;
        }

        public Hatchet(Serial serial)
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
