using System;

namespace Server.Items
{
    public class ArcanistsWildStaff : GnarledStaff
	{
        [Constructable]
        public ArcanistsWildStaff()
        {
			//마나 회복 2, 시전 속도 10%, 에너지 저항력 5%
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 21; //옵션 종류
			SuffixOption[11] = 200; //옵션 값
			PrefixOption[12] = 41; //옵션 종류
			SuffixOption[12] = 1000; //옵션 값
			PrefixOption[13] = 16; //옵션 종류
			SuffixOption[13] = 500; //옵션 값

        }

        public ArcanistsWildStaff(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073549;
            }
        }// arcanist's wild staff
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