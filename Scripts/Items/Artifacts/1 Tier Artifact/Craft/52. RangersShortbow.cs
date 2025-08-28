using System;

namespace Server.Items
{
    public class RangersShortbow : Axe
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public RangersShortbow()
        {
			//곤충슬 20%, 무피 30%, 공속 60%
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 59; //옵션 종류
			SuffixOption[11] = 200000; //옵션 값
			PrefixOption[12] = 7; //옵션 종류
			SuffixOption[12] = 300000; //옵션 값
			PrefixOption[13] = 40; //옵션 종류
			SuffixOption[13] = 600000; //옵션 값
        }

        public RangersShortbow(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073509;
            }
        }// ranger's shortbow
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