using Server;
using System;

namespace Server.Items
{
    public class RunedDriftwoodBow : Magerybook
	{
		public override bool IsArtifact { get { return true; } }
        public override int LabelNumber { get { return 1149961; } }

        [Constructable]
        public RunedDriftwoodBow()
        {
			//마법 치명 피해 50%, 마나 회복 0.5, 마법 치명 확률 3%
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 45; //옵션 종류
			SuffixOption[11] = 5000; //옵션 값
			PrefixOption[12] = 21; //옵션 종류
			SuffixOption[12] = 50; //옵션 값
			PrefixOption[13] = 43; //옵션 종류
			SuffixOption[13] = 300; //옵션 값
        }

        public RunedDriftwoodBow(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}