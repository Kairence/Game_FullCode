using System;

namespace Server.Items
{
    public class LuminousRuneBlade : VikingSword
	{
        [Constructable]
        public LuminousRuneBlade()
        {
			PrefixOption[80] = 1;
			PrefixOption[81] = 40;
			PrefixOption[82] = 7;
			PrefixOption[83] = 55;
			PrefixOption[84] = 5;
        }

        public LuminousRuneBlade(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1072922;
            }
        }// Luminous Rune Blade

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