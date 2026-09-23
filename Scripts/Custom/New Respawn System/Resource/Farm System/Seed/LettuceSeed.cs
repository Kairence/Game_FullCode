using System;

namespace Server.Items
{
    public class LettuceSeed : BaseSeed
    {
        public override Type CropType => typeof(Lettuce);
        public override double MinSkill => 75.0;
        public override double MaxSkill => 125.0;

        [Constructable]
        public LettuceSeed() : base(0xDCF)
        {
            Hue = 0x1D8;
            Name = "»óÃß ¾¾¾Ñ";
        }

        public LettuceSeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
}

