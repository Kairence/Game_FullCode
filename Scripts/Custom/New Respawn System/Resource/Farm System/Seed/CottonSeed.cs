using System;

namespace Server.Items
{
    public class CottonSeed : BaseSeed
    {
        public override Type CropType => typeof(Cotton);
        public override double MinSkill => 80.0;
        public override double MaxSkill => 130.0;

        [Constructable]
        public CottonSeed() : base(0xDCF)
        {
            Hue = 1153;
            Name = "¸ñÈ­ ¾¾¾Ñ";
        }

        public CottonSeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
}

