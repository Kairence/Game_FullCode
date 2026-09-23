using System;

namespace Server.Items
{
    public class PumpkinSeed : BaseSeed
    {
        public override Type CropType => typeof(Pumpkin);
        public override double MinSkill => 90.0;
        public override double MaxSkill => 140.0;

        [Constructable]
        public PumpkinSeed() : base(0xDCF)
        {
            Hue = 0x30;
            Name = "È£¹Ú ¾¾¾Ñ";
        }

        public PumpkinSeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
}

