using System;

namespace Server.Items
{
    public class SlayerLongbow : Mace
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public SlayerLongbow()
        {
			//체력 750, 공격 속도 20%, 무기 피해 20%, 명중률 40%
			SuffixOption[0] = 4; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 4; //옵션 종류
			SuffixOption[11] = 7500000; //옵션 값
			PrefixOption[12] = 40; //옵션 종류
			SuffixOption[12] = 200000; //옵션 값
			PrefixOption[13] = 7; //옵션 종류
			SuffixOption[13] = 200000; //옵션 값
			PrefixOption[14] = 17; //옵션 종류
			SuffixOption[14] = 400000; //옵션 값
        }

        public SlayerLongbow(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073506;
            }
        }// slayer longbow
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