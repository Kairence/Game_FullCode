using System;

namespace Server.Items
{
    public class EternalGuardianStaff : QuarterStaff
	{
		public override bool IsArtifact { get { return true; } }
		public override int LabelNumber { get { return 1112443; } } // Eternal Guardian Staff
		
        [Constructable]
        public EternalGuardianStaff()
        {		
			//체 500, 방어율 50%, 모저 5%
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 4; //옵션 종류
			SuffixOption[11] = 5000000; //옵션 값
			PrefixOption[12] = 18; //옵션 종류
			SuffixOption[12] = 500000; //옵션 값
			PrefixOption[13] = 114; //옵션 종류
			SuffixOption[13] = 50000; //옵션 값

        }

        public EternalGuardianStaff(Serial serial)
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