using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(SmallPlateShield))]
    public class Buckler : BaseShield
    {
        public override int InitMinHits
        {
            get
            {
                return 100;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 100;
            }
        }
        public override int AosStrReq
        {
            get
            {
                return 2000;
            }
        }
        public override int AosDexReq
        {
            get
            {
                return 1000;
            }
        }
        public override int AosIntReq
        {
            get
            {
                return 1000;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 25;
            }
        }
		
        [Constructable]
        public Buckler()
            : base(0x1B73)
        {
            this.Weight = 15.0;
			PrefixOption[61] = 41; //시전 속도
			SuffixOption[61] = -500000;
			PrefixOption[62] = 109; //방패 방어 확률
			SuffixOption[62] = 500000;
			PrefixOption[63] = 110; //모든 피격 데미지 감소
			SuffixOption[63] = 100000;

		}

        public Buckler(Serial serial)
            : base(serial)
        {
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);//version
        }
    }
}