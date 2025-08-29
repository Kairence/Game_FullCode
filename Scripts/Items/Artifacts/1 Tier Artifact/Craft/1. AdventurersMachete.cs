using System;

namespace Server.Items
{
    public class AdventurersMachete : Katana
	{
        [Constructable]
        public AdventurersMachete()
        {
			//금화 획득 20%, 체력 100, 기력 100, 피해 증가 50%
			SuffixOption[0] = 4; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 51; //옵션 종류
			SuffixOption[11] = 200000; //옵션 값
			PrefixOption[12] = 4; //옵션 종류
			SuffixOption[12] = 2500000; //옵션 값
			PrefixOption[13] = 5; //옵션 종류
			SuffixOption[13] = 2500000; //옵션 값
			PrefixOption[14] = 7; //옵션 종류
			SuffixOption[14] = 5000; //옵션 값
			
			//PrefixOption[80] = 1;
			//PrefixOption[81] = 51;
            //Attributes.Luck = 20; 3, 4, 51
        }

        public AdventurersMachete(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073533;
            }
        }// adventurer's machete
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