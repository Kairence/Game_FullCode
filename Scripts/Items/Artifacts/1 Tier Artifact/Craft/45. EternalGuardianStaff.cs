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
			//체 200, 방어율 100%, 모저 20%
			SuffixOption[0] = 7; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 4; //옵션 종류
			SuffixOption[11] = 20000; //옵션 값
			PrefixOption[12] = 18; //옵션 종류
			SuffixOption[12] = 10000; //옵션 값
			PrefixOption[13] = 12; //옵션 종류
			SuffixOption[13] = 2000; //옵션 값
			PrefixOption[14] = 13; //옵션 종류
			SuffixOption[14] = 2000; //옵션 값
			PrefixOption[15] = 14; //옵션 종류
			SuffixOption[15] = 2000; //옵션 값
			PrefixOption[16] = 15; //옵션 종류
			SuffixOption[16] = 2000; //옵션 값	
			PrefixOption[17] = 16; //옵션 종류
			SuffixOption[17] = 2000; //옵션 값	
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