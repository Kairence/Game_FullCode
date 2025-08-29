using System;

namespace Server.Items
{
    public class Luckblade : Dagger
	{
        [Constructable]
        public Luckblade()
        {
			//운 200, 물리 치명 확률 30%, 금화 획득 10%
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 3; //옵션 종류
			SuffixOption[11] = 20000; //옵션 값
			PrefixOption[12] = 42; //옵션 종류
			SuffixOption[12] = 300000; //옵션 값
			PrefixOption[13] = 51; //옵션 종류
			SuffixOption[13] = 100000; //옵션 값
        }

        public Luckblade(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073522;
            }
        }// luckblade
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