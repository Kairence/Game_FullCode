using System;

namespace Server.Items
{
    public class TurnipSeed : BaseSeed
    {
        // Turnip 클래스가 존재한다고 가정합니다.
        public override Type CropType => typeof(Turnip);

        [Constructable]
        public TurnipSeed() : base(0xDCF)
        {
            Hue = 0x1F6;
            Name = "순무 씨앗";
        }

        public TurnipSeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
}