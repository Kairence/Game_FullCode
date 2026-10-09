using System;

namespace Server.Items
{
    public class WheatSeed : BaseSeed
    {
        public override Type CropType => typeof(Wheat);
        public override double MinSkill => 100.0;
        public override double MaxSkill => 150.0;

        [Constructable]
        public WheatSeed() : base(0xDCF)
        {
            Hue = 45;
            Name = "¹Ð ¾¾¾Ñ";
        }

        public WheatSeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
}

