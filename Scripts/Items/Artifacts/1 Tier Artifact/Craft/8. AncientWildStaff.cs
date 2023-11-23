using System;

namespace Server.Items
{
    public class AncientWildStaff : Bow
	{
        [Constructable]
        public AncientWildStaff()
        {
			//파충류 피해 20%, 무기 피해 30%, 공속 20%
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 60; //옵션 종류
			SuffixOption[11] = 2000; //옵션 값
			PrefixOption[12] = 7; //옵션 종류
			SuffixOption[12] = 3000; //옵션 값
			PrefixOption[13] = 40; //옵션 종류
			SuffixOption[13] = 2000; //옵션 값

		}

        public AncientWildStaff(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073550;
            }
        }// ancient wild staff
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