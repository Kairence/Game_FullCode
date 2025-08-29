using System;

namespace Server.Items
{
    public class LongbowOfMight : Maul
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public LongbowOfMight()
        {
			//명중률 50%, 무기 피해 25%, 공속 30%, 체력 150, 기력 150
			SuffixOption[0] = 5; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 17; //옵션 종류
			SuffixOption[11] = 500000; //옵션 값
			PrefixOption[12] = 7; //옵션 종류
			SuffixOption[12] = 250000; //옵션 값
			PrefixOption[13] = 40; //옵션 종류
			SuffixOption[13] = 300000; //옵션 값
			PrefixOption[14] = 4; //옵션 종류
			SuffixOption[14] = 1500000; //옵션 값
			PrefixOption[15] = 5; //옵션 종류
			SuffixOption[15] = 1500000; //옵션 값

        }

        public LongbowOfMight(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073508;
            }
        }// longbow of might
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