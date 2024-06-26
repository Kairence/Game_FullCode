using System;

namespace Server.Items
{
    public class KnightsWarCleaver : Lance
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public KnightsWarCleaver()
        {
			//힘 1, 체력 200
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 0; //옵션 종류
			SuffixOption[11] = 100; //옵션 값
			PrefixOption[12] = 4; //옵션 종류
			SuffixOption[12] = 20000; //옵션 값


        }

        public KnightsWarCleaver(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073525;
            }
        }// knight's war cleaver
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