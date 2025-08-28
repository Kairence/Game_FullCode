using System;

namespace Server.Items
{
    public class BarbedLongbow : DoubleBladedStaff
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public BarbedLongbow()
        {
			//독 속성 25%, 독 피해 140, 독저 18%
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 35; //옵션 종류
			SuffixOption[11] = 1400000; //옵션 값
			PrefixOption[12] = 15; //옵션 종류
			SuffixOption[12] = 180000; //옵션 값
			PrefixOption[13] = 155; //옵션 종류
			SuffixOption[13] = 250000; //옵션 값
        }

        public BarbedLongbow(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073505;
            }
        }// barbed longbow
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