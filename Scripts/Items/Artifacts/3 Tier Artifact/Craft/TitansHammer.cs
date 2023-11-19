using System;

namespace Server.Items
{
    public class TitansHammer : WarHammer
	{
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 3;
            }
        }
        [Constructable]
        public TitansHammer()
        {
            Hue = 0x482;
			PrefixOption[80] = 1;
			PrefixOption[81] = 0;
			PrefixOption[82] = 7;
			PrefixOption[83] = 17;
        }

        public TitansHammer(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1060024;
            }
        }// Titan's Hammer
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}