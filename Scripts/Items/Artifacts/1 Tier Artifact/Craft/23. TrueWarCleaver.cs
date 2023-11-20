using System;

namespace Server.Items
{
    public class TrueWarCleaver : WarAxe
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public TrueWarCleaver()
        {
			//체력 1000, 모든 저항력 40%
			SuffixOption[0] = 6; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 4; //옵션 종류
			SuffixOption[11] = 100000; //옵션 값
			PrefixOption[12] = 12; //옵션 종류
			SuffixOption[12] = 3500; //옵션 값
			PrefixOption[13] = 13; //옵션 종류
			SuffixOption[13] = 6000; //옵션 값
			PrefixOption[14] = 14; //옵션 종류
			SuffixOption[14] = 3500; //옵션 값
			PrefixOption[15] = 15; //옵션 종류
			SuffixOption[15] = 3500; //옵션 값
			PrefixOption[16] = 16; //옵션 종류
			SuffixOption[16] = 3500; //옵션 값

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