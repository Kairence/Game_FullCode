using System;

namespace Server.Items
{
    public class RuneCarvingKnife : AssassinSpike
	{
        [Constructable]
        public RuneCarvingKnife()
        {
            Hue = 0x48D;
			PrefixOption[80] = 1;
			PrefixOption[81] = 20;
			PrefixOption[82] = 7;
			PrefixOption[83] = 39;
			PrefixOption[84] = 40;
        }

        public RuneCarvingKnife(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1072915;
            }
        }// Rune Carving Knife
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