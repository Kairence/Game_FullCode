using System;

namespace Server.Items
{
    public class ChargedAssassinSpike : MagicalShortbow
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public ChargedAssassinSpike()
        {
			//에너지 속성 25%, 에너지 피해 140, 에너지 저항 10%
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 36; //옵션 종류
			SuffixOption[11] = 1400000; //옵션 값
			PrefixOption[12] = 16; //옵션 종류
			SuffixOption[12] = 180000; //옵션 값
			PrefixOption[13] = 156; //옵션 종류
			SuffixOption[13] = 250000; //옵션 값


        }

        public ChargedAssassinSpike(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073518;
            }
        }// charged assassin spike
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