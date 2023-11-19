using System;

namespace Server.Items
{
    public class MagekillerAssassinSpike : OrnateAxe
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public MagekillerAssassinSpike()
        {
			//정령 피해 300%, 공격 속도 20%
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 58; //옵션 종류
			SuffixOption[11] = 30000; //옵션 값
			PrefixOption[12] = 40; //옵션 종류
			SuffixOption[12] = 2000; //옵션 값

        }

        public MagekillerAssassinSpike(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073519;
            }
        }// magekiller assassin spike
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