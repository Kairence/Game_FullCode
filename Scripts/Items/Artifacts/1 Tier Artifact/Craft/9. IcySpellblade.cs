using System;

namespace Server.Items
{
    public class IcySpellblade : ElvenSpellblade
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public IcySpellblade()
        {
			//냉기 속성 25%, 냉기 피해 140, 냉기 저항 18%
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 34; //옵션 종류
			SuffixOption[11] = 1400000; //옵션 값
			PrefixOption[12] = 14; //옵션 종류
			SuffixOption[12] = 180000; //옵션 값
			PrefixOption[13] = 154; //옵션 종류
			SuffixOption[13] = 250000; //옵션 값
        }

        public IcySpellblade(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073514;
            }
        }// icy spellblade
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