using System;

namespace Server.Items
{
    public class AssassinsShortbow : ShortSpear
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public AssassinsShortbow()
        {
			//물치확률 5%, 무피 50%, 명중 4%
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 42; //옵션 종류
			SuffixOption[11] = 500; //옵션 값
			PrefixOption[12] = 7; //옵션 종류
			SuffixOption[12] = 5000; //옵션 값
			PrefixOption[13] = 17; //옵션 종류
			SuffixOption[13] = 400; //옵션 값
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