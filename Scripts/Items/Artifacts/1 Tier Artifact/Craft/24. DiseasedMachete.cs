using System;

namespace Server.Items
{
    public class DiseasedMachete : Scythe
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public DiseasedMachete()
        {
			//영혼 대화 5, 물리 치명 피해 4.5%
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 44; //옵션 종류
			SuffixOption[11] = 500; //옵션 값
			PrefixOption[12] = 82; //옵션 종류
			SuffixOption[12] = 450; //옵션 값

        }

        public DiseasedMachete(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073536;
            }
        }// Diseased Machete
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