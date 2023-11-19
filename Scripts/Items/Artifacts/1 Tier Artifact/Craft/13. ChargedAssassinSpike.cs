using System;

namespace Server.Items
{
    public class ChargedAssassinSpike : MagicalShortbow
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public ChargedAssassinSpike()
        {
			//에너지 피해 100, 에너지 저항 70%
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 36; //옵션 종류
			SuffixOption[11] = 10000; //옵션 값
			PrefixOption[12] = 16; //옵션 종류
			SuffixOption[12] = 7000; //옵션 값


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