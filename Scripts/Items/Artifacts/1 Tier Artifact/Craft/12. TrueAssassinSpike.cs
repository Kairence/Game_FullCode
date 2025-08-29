using System;

namespace Server.Items
{
    public class TrueAssassinSpike : AssassinSpike
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public TrueAssassinSpike()
        {
			//명중률 50%, 물리 피해 증가 50, 공격 속도 30%
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 17; //옵션 종류
			SuffixOption[11] = 500000; //옵션 값
			PrefixOption[12] = 32; //옵션 종류
			SuffixOption[12] = 500000; //옵션 값
			PrefixOption[13] = 40; //옵션 종류
			SuffixOption[13] = 300000; //옵션 값

        }

        public TrueAssassinSpike(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073517;
            }
        }// true assassin spike
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