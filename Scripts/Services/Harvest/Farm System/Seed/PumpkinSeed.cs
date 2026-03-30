using System;

namespace Server.Items
{
    public class PumpkinSeed : BaseSeed
    {
        public override Type CropType => typeof(Pumpkin);

        [Constructable]
        public PumpkinSeed() : base(0xDCF)
        {
            Hue = 0x30;
            Name = "호박 씨앗";
        }

        public PumpkinSeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
}
