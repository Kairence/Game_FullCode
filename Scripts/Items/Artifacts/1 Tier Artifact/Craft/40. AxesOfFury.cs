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
		//민첩 500, 공격 속도 100%, 무기 피해 25%
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 1; //옵션 종류
			SuffixOption[11] = 500; //옵션 값
			PrefixOption[12] = 40; //옵션 종류
			SuffixOption[12] = 1000000; //옵션 값
			PrefixOption[13] = 7; //옵션 종류
			SuffixOption[13] = 250000; //옵션 값
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
