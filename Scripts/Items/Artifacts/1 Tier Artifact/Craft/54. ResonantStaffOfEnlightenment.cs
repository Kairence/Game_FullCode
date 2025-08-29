using System;

namespace Server.Items
{
    public class ResonantStaffofEnlightenment : Magerybook
	{
		public override bool IsArtifact { get { return true; } }
		public override int LabelNumber { get { return 1113757; } } // Resonant Staff of Enlightenment
		
        [Constructable]
        public ResonantStaffofEnlightenment()
        {
			//매저리 스킬 30.0, 마나 450
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 75; //옵션 종류
			SuffixOption[11] = 3000; //옵션 값
			PrefixOption[12] = 6; //옵션 종류
			SuffixOption[12] = 4500000; //옵션 값		
        }

        public ResonantStaffofEnlightenment(Serial serial)
            : base(serial)
        {
        }

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