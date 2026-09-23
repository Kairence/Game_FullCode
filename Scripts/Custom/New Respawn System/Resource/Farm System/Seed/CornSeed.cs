using System;

namespace Server.Items
{
    public class CornSeed : BaseSeed
    {
        public override Type CropType => typeof(Corn);
        public override double MinSkill => 30.0;
        public override double MaxSkill => 80.0;

        [Constructable]
        public CornSeed() : base(0xDCF)
        {
            Hue = 0x160;
            Name = "¿Á¼ö¼ö ¾¾¾Ñ";
        }

        public CornSeed(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
}

