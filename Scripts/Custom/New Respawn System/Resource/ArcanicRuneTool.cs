using System;
using Server.Engines.Craft;

namespace Server.Items
{
    public class ArcanicRuneTool : BaseTool
    {
        public override CraftSystem CraftSystem => DefImbuing.CraftSystem;

        [Constructable]
        public ArcanicRuneTool()
            : this(50)
        {
        }

        [Constructable]
        public ArcanicRuneTool(int uses)
            : base(uses, 0x573C)
        {
            Weight = 1.0;
        }

        public ArcanicRuneTool(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1113352; // arcanic rune stone

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