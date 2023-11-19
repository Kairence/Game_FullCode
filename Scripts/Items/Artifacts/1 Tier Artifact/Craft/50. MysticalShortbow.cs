using System;

namespace Server.Items
{
    public class MysticalShortbow : Scepter
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public MysticalShortbow()
        {
			//치유량 100%, 체 500, 마 200
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 46; //옵션 종류
			SuffixOption[11] = 500; //옵션 값
			PrefixOption[12] = 4; //옵션 종류
			SuffixOption[12] = 50000; //옵션 값
			PrefixOption[13] = 6; //옵션 종류
			SuffixOption[13] = 20000; //옵션 값
        }

        public MysticalShortbow(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073511;
            }
        }// mystical shortbow
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