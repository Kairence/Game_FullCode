using System;

namespace Server.Items
{
    public class IcySpellblade : ElvenSpellblade
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public IcySpellblade()
        {
			//냉기 피해 140, 냉기 저항 50%
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 34; //옵션 종류
			SuffixOption[11] = 14000; //옵션 값
			PrefixOption[12] = 14; //옵션 종류
			SuffixOption[12] = 5000; //옵션 값
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