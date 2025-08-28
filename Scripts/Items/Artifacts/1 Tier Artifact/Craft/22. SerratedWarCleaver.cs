using System;

namespace Server.Items
{
    public class SerratedWarCleaver : Bardiche
    {
        [Constructable]
        public SerratedWarCleaver()
        {
			//물리 피해 100, 무기 피해 50%, 명중률 20%
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 32; //옵션 종류
			SuffixOption[11] = 1000000; //옵션 값
			PrefixOption[12] = 7; //옵션 종류
			SuffixOption[12] = 500000; //옵션 값
			PrefixOption[13] = 17; //옵션 종류
			SuffixOption[13] = 200000; //옵션 값

        }

        public SerratedWarCleaver(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1073527;
            }
        }// serrated war cleaver
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