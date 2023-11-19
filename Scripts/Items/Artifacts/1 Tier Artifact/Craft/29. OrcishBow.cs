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
                return 1;
            }
        }

        [Constructable]
        public OrcishBow()
        {
			//요정슬 300%, 공속 20%
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 62; //옵션 종류
			SuffixOption[11] = 100000; //옵션 값
			PrefixOption[12] = 40; //옵션 종류
			SuffixOption[12] = 2000; //옵션 값
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