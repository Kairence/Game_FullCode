using System;

namespace Server.Items
{
    public class CorruptedRuneBlade : BaseWand
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public CorruptedRuneBlade()
        {
			//시전 100%, 공속 -20%
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 41; //옵션 종류
			SuffixOption[11] = 10000; //옵션 값
			PrefixOption[12] = 40; //옵션 종류
			SuffixOption[12] = -2000; //옵션 값
        }

        public CorruptedRuneBlade(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073540;
            }
        }// Corrupted Rune Blade
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