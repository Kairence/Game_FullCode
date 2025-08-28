using System;

namespace Server.Items
{
    public class SingingAxe : Magerybook
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public SingingAxe()
        {
			//마법 치명 확률 50%, 체력 -500
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 43; //옵션 종류
			SuffixOption[11] = 500000; //옵션 값
			PrefixOption[12] = 4; //옵션 종류
			SuffixOption[12] = -5000000; //옵션 값
        }

        public SingingAxe(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073546;
            }
        }// singing axe
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