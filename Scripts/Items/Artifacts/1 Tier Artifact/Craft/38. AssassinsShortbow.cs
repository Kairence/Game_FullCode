using System;

namespace Server.Items
{
    public class AssassinsShortbow : ShortSpear
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public AssassinsShortbow()
        {
			//무기 치명 확률 30%, 무기 피해 25%, 명중 10%
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 42; //옵션 종류
			SuffixOption[11] = 300000; //옵션 값
			PrefixOption[12] = 7; //옵션 종류
			SuffixOption[12] = 250000; //옵션 값
			PrefixOption[13] = 17; //옵션 종류
			SuffixOption[13] = 100000; //옵션 값
        }

        public AssassinsShortbow(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073512;
            }
        }// assassin's shortbow
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