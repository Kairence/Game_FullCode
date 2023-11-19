using System;

namespace Server.Items
{
    public class MelisandesCorrodedHatchet : Hatchet
	{

        [Constructable]
        public MelisandesCorrodedHatchet() : this( 2500 )
        {
            Hue = 0x494;

        }

        public MelisandesCorrodedHatchet(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1072115;
            }
        }// Melisande's Corroded Hatchet
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}