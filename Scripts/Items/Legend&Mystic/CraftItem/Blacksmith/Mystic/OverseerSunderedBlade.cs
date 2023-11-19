using System;

namespace Server.Items
{
    public class OverseerSunderedBlade : Broadsword
	{
        [Constructable]
        public OverseerSunderedBlade()
        {
			PrefixOption[80] = 1;
			PrefixOption[81] = 40;
			PrefixOption[82] = 7;
			PrefixOption[83] = 17;
        }

        public OverseerSunderedBlade(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1072920;
            }
        }// Overseer Sundered Blade

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