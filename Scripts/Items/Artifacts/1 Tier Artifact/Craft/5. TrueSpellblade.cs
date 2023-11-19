using System;

namespace Server.Items
{
    public class TrueSpellblade : Magerybook
	{
       [Constructable]
        public TrueSpellblade()
        {
			//마나 200, 주문 피해 50%, 마나 회복 0.5, 시전 속도 20%, 마법 치피 20%
			SuffixOption[0] = 5; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 6; //옵션 종류
			SuffixOption[11] = 20000; //옵션 값
			PrefixOption[12] = 8; //옵션 종류
			SuffixOption[12] = 5000; //옵션 값
			PrefixOption[13] = 21; //옵션 종류
			SuffixOption[13] = 50; //옵션 값
			PrefixOption[14] = 41; //옵션 종류
			SuffixOption[14] = 2000; //옵션 값
			PrefixOption[15] = 45; //옵션 종류
			SuffixOption[15] = 2000; //옵션 값

		}

        public TrueSpellblade(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073513;
            }
        }// true spellblade
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