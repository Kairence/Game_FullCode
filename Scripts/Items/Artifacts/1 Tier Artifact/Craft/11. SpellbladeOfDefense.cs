using System;

namespace Server.Items
{
    public class SpellbladeOfDefense : BoneHarvester
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public SpellbladeOfDefense()
        {
			//물리 저항력 50%, 체력 500 
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 12; //옵션 종류
			SuffixOption[11] = 500000; //옵션 값
			PrefixOption[12] = 4; //옵션 종류
			SuffixOption[12] = 5000000; //옵션 값


        }

        public SpellbladeOfDefense(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073516;
            }
        }// spellblade of defense
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