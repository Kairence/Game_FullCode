using System;

namespace Server.Items
{
    public class ButchersWarCleaver : BattleAxe
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public ButchersWarCleaver()
            : base()
        {
			//영장류 피해 증가 300%, 공격 속도 증가 20%
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 56; //옵션 종류
			SuffixOption[11] = 20000; //옵션 값
			PrefixOption[12] = 40; //옵션 종류
			SuffixOption[12] = 5000; //옵션 값
        }

        public ButchersWarCleaver(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073526;
            }
        }// butcher's war cleaver
        public override void AppendChildNameProperties(ObjectPropertyList list)
        {
            base.AppendChildNameProperties(list);
			
            list.Add(1072512); // Bovine Slayer
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