using System;

namespace Server.Items
{
    public class LeafbladeOfEase : CrescentBlade
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public LeafbladeOfEase()
        {
			//번개 공격 30%, 체력 회복 1.5
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 55; //옵션 종류
			SuffixOption[11] = 3000; //옵션 값
			PrefixOption[12] = 19; //옵션 종류
			SuffixOption[12] = 150; //옵션 값
        }

        public LeafbladeOfEase(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073524;
            }
        }// leafblade of ease
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