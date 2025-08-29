using System;

namespace Server.Items
{
    public class TrueWarCleaver : WarAxe
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public TrueWarCleaver()
        {
			//체력 1000, 모든 저항력 10%
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 4; //옵션 종류
			SuffixOption[11] = 10000000; //옵션 값
			PrefixOption[12] = 114; //옵션 종류
			SuffixOption[12] = 100000; //옵션 값

        }

        public TrueWarCleaver(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073528;
            }
        }// true war cleaver
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