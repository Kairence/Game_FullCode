using System;

namespace Server.Items
{
    public class FierySpellblade : PaladinSword
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public FierySpellblade()
        {
			//화염 피해 60, 공격 속도 60%
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 33; //옵션 종류
			SuffixOption[11] = 6000; //옵션 값
			PrefixOption[12] = 40; //옵션 종류
			SuffixOption[12] = 6000; //옵션 값

        }

        public FierySpellblade(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073515;
            }
        }// fiery spellblade
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