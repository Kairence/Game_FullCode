using System;

namespace Server.Items
{
    public class TrueLeafblade : Crossbow
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public TrueLeafblade()
        {
			//영장류 피해 200%, 무기 피해 60%, 공격속도 20%
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 56; //옵션 종류
			SuffixOption[11] = 20000; //옵션 값
			PrefixOption[12] = 7; //옵션 종류
			SuffixOption[12] = 6000; //옵션 값
			PrefixOption[13] = 40; //옵션 종류
			SuffixOption[13] = 2000; //옵션 값

        }

        public TrueLeafblade(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073521;
            }
        }// true leafblade
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