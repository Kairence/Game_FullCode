using System;

namespace Server.Items
{
    public class MacheteOfDefense : VikingSword
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public MacheteOfDefense()
        {
			//기력 1000, 공속 10%, 무피 5%
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 5; //옵션 종류
			SuffixOption[11] = 100000; //옵션 값
			PrefixOption[12] = 40; //옵션 종류
			SuffixOption[12] = 1000; //옵션 값
			PrefixOption[13] = 7; //옵션 종류
			SuffixOption[13] = 500; //옵션 값
        }

        public MacheteOfDefense(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073535;
            }
        }// machete of defense
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