using System;
using Server.Engines.Harvest;

namespace Server.Items
{
    public class JadeWarAxe : WarAxe
	{
		public override bool IsArtifact { get { return true; } }
		public override int LabelNumber { get { return 1115445; } } // Jade War Axe
		
        [Constructable]
        public JadeWarAxe()
        {	
			//힘 1000, 체 1000, 무기피해 30%
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 0; //옵션 종류
			SuffixOption[11] = 1000; //옵션 값
			PrefixOption[12] = 4; //옵션 종류
			SuffixOption[12] = 10000000; //옵션 값
			PrefixOption[13] = 7; //옵션 종류
			SuffixOption[13] = 300000; //옵션 값
        }

        public JadeWarAxe(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}