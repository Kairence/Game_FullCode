using System;

namespace Server.Items
{
    public class IcyScimitar : BladedStaff
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public IcyScimitar()
        {
			//마법 치명 확률 8%
			SuffixOption[0] = 1; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 43; //옵션 종류
			SuffixOption[11] = 800; //옵션 값
        }

        public IcyScimitar(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073543;
            }
        }// icy scimitar
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