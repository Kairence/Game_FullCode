using System;

namespace Server.Items
{
    public class PhantomStaff : Magerybook
	{
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public PhantomStaff()
        {
			//지능 0.5, 시전 속도 30%, 주피 15%, 물저 50%, 화저 -30%
			SuffixOption[0] = 5; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 2; //옵션 종류
			SuffixOption[11] = 50; //옵션 값
			PrefixOption[12] = 41; //옵션 종류
			SuffixOption[12] = 3000; //옵션 값
			PrefixOption[13] = 8; //옵션 종류
			SuffixOption[13] = 1500; //옵션 값
			PrefixOption[14] = 12; //옵션 종류
			SuffixOption[14] = 5000; //옵션 값
			PrefixOption[15] = 13; //옵션 종류
			SuffixOption[15] = -3000; //옵션 값

        }

        public PhantomStaff(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1072919;
            }
        }// Phantom Staff
		/*
        public override void GetDamageTypes(Mobile wielder, out int phys, out int fire, out int cold, out int pois, out int nrgy, out int chaos, out int direct)
        {
            phys = fire = nrgy = chaos = direct = 0;
            cold = pois = 50;
        }
		*/
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