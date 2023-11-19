using System;

namespace Server.Items
{
    public class AxesOfFury : LargeBattleAxe
	{
		public override bool IsArtifact { get { return true; } }
		public override int LabelNumber { get { return 1113517; } } // Axes Of Fury
		
        [Constructable]
        public AxesOfFury() 
        {	
		////민첩 1, 공속 30%, 무피 50%, 모저 -20%
			SuffixOption[0] = 8; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 1; //옵션 종류
			SuffixOption[11] = 100; //옵션 값
			PrefixOption[12] = 40; //옵션 종류
			SuffixOption[12] = 3000; //옵션 값
			PrefixOption[13] = 7; //옵션 종류
			SuffixOption[13] = 5000; //옵션 값
			PrefixOption[14] = 12; //옵션 종류
			SuffixOption[14] = -2000; //옵션 값
			PrefixOption[15] = 13; //옵션 종류
			SuffixOption[15] = -2000; //옵션 값
			PrefixOption[16] = 14; //옵션 종류
			SuffixOption[16] = -2000; //옵션 값	
			PrefixOption[17] = 15; //옵션 종류
			SuffixOption[17] = -2000; //옵션 값	
			PrefixOption[18] = 16; //옵션 종류
			SuffixOption[18] = -2000; //옵션 값	
        }

        public AxesOfFury(Serial serial)
            : base(serial)
        {
        }

        public override int InitMinHits
        {
            get
            {
                return 255;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 255;
            }
        }
       
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
