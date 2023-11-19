using System;

namespace Server.Items
{
    public class WoundingAssassinSpike : WarFork
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public WoundingAssassinSpike()
        {
			//기력 회복 2, 기력 200
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 20; //옵션 종류
			SuffixOption[11] = 200; //옵션 값
			PrefixOption[12] = 5; //옵션 종류
			SuffixOption[12] = 20000; //옵션 값


        }

        public WoundingAssassinSpike(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073520;
            }
        }// wounding assassin spike
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