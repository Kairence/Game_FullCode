using System;

namespace Server.Items
{
    public class OrcChieftainHelm : OrcHelm
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
        public OrcChieftainHelm()
        {
            Hue = 0x2a3;
			PrefixOption[80] = 1;
			PrefixOption[81] = 3;
			PrefixOption[82] = 4;
			PrefixOption[83] = 19;
        }

        public OrcChieftainHelm(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1094924;
            }
        }// Orc Chieftain Helm [Replica]

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version < 1 && this.Hue == 0x3f) /* Pigmented? */
            {
                this.Hue = 0x2a3;
            }
        }
    }
}