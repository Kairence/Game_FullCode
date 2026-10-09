using System;

namespace Server.Items
{
    public class OnionSeed : BaseSeed
    {
        public override Type CropType => typeof(Onion);
        public override double MinSkill => 45.0;
        public override double MaxSkill => 95.0;

        [Constructable]
        public OnionSeed() : base(0xDCF)
        {
            Hue = 0x1BF;
            Name = "¾çÆÄ ¾¾¾Ñ";
        }

        public OnionSeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
}

