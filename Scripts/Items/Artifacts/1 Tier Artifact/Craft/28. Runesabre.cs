using System;

namespace Server.Items
{
    public class Runesabre : HammerPick
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public Runesabre()
        {
			//거미슬 30%, 공속 20%
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 59; //옵션 종류
			SuffixOption[11] = 3000; //옵션 값
			PrefixOption[12] = 40; //옵션 종류
			SuffixOption[12] = 2000; //옵션 값

        }

        public Runesabre(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073537;
            }
        }// runesabre
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