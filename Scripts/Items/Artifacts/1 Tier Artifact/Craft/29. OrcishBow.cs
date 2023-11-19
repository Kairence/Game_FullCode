using System;

namespace Server.Items
{
    public class OrcishBow : CompositeBow
    {
        public override int LabelNumber { get { return 1153778; } } // an orcish bow
        public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 2;
            }
        }

        [Constructable]
        public OrcishBow()
        {
			//요정슬 300%, 공속 20%
            Hue = 1107;
			PrefixOption[80] = 1;
			PrefixOption[81] = 7;
			PrefixOption[82] = 73;
        }

        public OrcishBow(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}