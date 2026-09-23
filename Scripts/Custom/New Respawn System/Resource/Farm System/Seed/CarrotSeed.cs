using System;

namespace Server.Items
{
    public class CarrotSeed : BaseSeed
    {
        public override Type CropType => typeof(Carrot);
        public override double MinSkill => 0.0;
        public override double MaxSkill => 50.0;

        [Constructable]
        public CarrotSeed() : base(0xDCF)
        {
            Hue = 0x5E2;
            Name = "´ç±Ù ¾¾¾Ñ";
        }

        public CarrotSeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
}

