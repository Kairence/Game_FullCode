using System;

namespace Server.Items
{
    public class RuneBladeOfKnowledge : HeavyCrossbow
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public RuneBladeOfKnowledge()
        {
			//운 1, 명중률 40%
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 3; //옵션 종류
			SuffixOption[11] = 100; //옵션 값
			PrefixOption[12] = 17; //옵션 종류
			SuffixOption[12] = 4000; //옵션 값

        }

        public RuneBladeOfKnowledge(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073539;
            }
        }// rune blade of knowledge
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