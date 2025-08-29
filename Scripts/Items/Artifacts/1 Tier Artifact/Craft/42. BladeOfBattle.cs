using System;

namespace Server.Items
{
    public class BladeOfBattle : WarMace
	{
		public override bool IsArtifact { get { return true; } }
		public override int LabelNumber { get { return 1113525; } } // Blade of Battle
		
        [Constructable]
        public BladeOfBattle() 
        {
			//치유량 60, 체 1000, 마 1000
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 47; //옵션 종류
			SuffixOption[11] = 600000; //옵션 값
			PrefixOption[12] = 4; //옵션 종류
			SuffixOption[12] = 10000000; //옵션 값
			PrefixOption[13] = 6; //옵션 종류
			SuffixOption[13] = 10000000; //옵션 값
        }

        public BladeOfBattle(Serial serial)
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
