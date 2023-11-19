using System;

namespace Server.Items
{
    public class ColdForgedBlade : Scimitar
	{
        [Constructable]
        public ColdForgedBlade()
        {
			PrefixOption[80] = 1;
			PrefixOption[81] = 24;
			PrefixOption[82] = 39;
			PrefixOption[83] = 53;
		}

        public ColdForgedBlade(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1072916;
            }
        }// Cold Forged Blade
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