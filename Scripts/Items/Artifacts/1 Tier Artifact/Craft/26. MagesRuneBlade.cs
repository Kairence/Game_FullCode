using System;

namespace Server.Items
{
    public class MagesRuneBlade : ElvenSpellblade
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public MagesRuneBlade()
        {
			//마쟁 2 마나 200
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 21; //옵션 종류
			SuffixOption[11] = 200; //옵션 값
			PrefixOption[12] = 6; //옵션 종류
			SuffixOption[12] = 20000; //옵션 값

        }

        public MagesRuneBlade(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073538;
            }
        }// mage's rune blade
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