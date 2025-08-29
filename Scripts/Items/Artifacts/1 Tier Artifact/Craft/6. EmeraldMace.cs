using System;

namespace Server.Items
{
    public class EmeraldMace : Pike
    {
        [Constructable]
        public EmeraldMace()
        {
			//민첩 1000, 펜싱 5%, 기력 회복 100
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 1; //옵션 종류
			SuffixOption[11] = 1000; //옵션 값
			PrefixOption[12] = 89; //옵션 종류
			SuffixOption[12] = 500; //옵션 값
			PrefixOption[13] = 20; //옵션 종류
			SuffixOption[13] = 1000000; //옵션 값

        }

        public EmeraldMace(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073530;
            }
        }// emerald mace
        public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 1;
            }
        }
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