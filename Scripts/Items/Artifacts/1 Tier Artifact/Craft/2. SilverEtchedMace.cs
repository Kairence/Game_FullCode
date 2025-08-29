using System;

namespace Server.Items
{
    public class SilverEtchedMace : Mace
    {
        [Constructable]
        public SilverEtchedMace()
        {
            //언데드 피해 증가 30%, 공격 속도 증가 20%, 둔기 특수기 2
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 40; //옵션 종류
			SuffixOption[11] = 2000; //옵션 값
			PrefixOption[12] = 57; //옵션 종류
			SuffixOption[12] = 3000; //옵션 값
			PrefixOption[13] = 136; //옵션 종류
			SuffixOption[13] = 200; //옵션 값
        }

        public SilverEtchedMace(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073532;
            }
        }// silver-etched mace
        public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 1;
            }
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