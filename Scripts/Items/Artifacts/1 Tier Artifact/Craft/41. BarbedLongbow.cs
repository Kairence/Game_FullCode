using System;

namespace Server.Items
{
    public class BarbedLongbow : DoubleBladedStaff
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public BarbedLongbow()
        {
			//포이즈닝 +5, 독피해 40, 독저 50%
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 80; //옵션 종류
			SuffixOption[11] = 500; //옵션 값
			PrefixOption[12] = 35; //옵션 종류
			SuffixOption[12] = 4000; //옵션 값
			PrefixOption[13] = 15; //옵션 종류
			SuffixOption[13] = 5000; //옵션 값
        }

        public BarbedLongbow(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073505;
            }
        }// barbed longbow
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