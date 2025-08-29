using System;

namespace Server.Items
{
    public class RubyMace : Longsword
    {
        [Constructable]
        public RubyMace()
        {
			//체력 2000, 힘 250
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 4; //옵션 종류
			SuffixOption[11] = 20000000; //옵션 값
			PrefixOption[12] = 0; //옵션 종류
			SuffixOption[12] = 250; //옵션 값
		}
		
        public RubyMace(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073529;
            }
        }// ruby mace
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