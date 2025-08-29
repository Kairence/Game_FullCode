using System;

namespace Server.Items
{
    [FlipableAttribute(0xF47, 0xF48)]
    public class AxeOfAbandon : TwoHandedAxe
	{
		public override bool IsArtifact { get { return true; } }
		public override int LabelNumber { get { return 1113863; } } // Axe of Abandon
		
        [Constructable]
        public AxeOfAbandon() 
        {		
		    //방어율 50%, 기력 1000
			SuffixOption[0] = 6; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 18; //옵션 종류
			SuffixOption[11] = 500000; //옵션 값
			PrefixOption[12] = 5; //옵션 종류
			SuffixOption[12] = 1000000; //옵션 값
        }

        public AxeOfAbandon(Serial serial)
            : base(serial)
        {
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
