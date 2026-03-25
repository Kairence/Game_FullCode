using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(MediumPlateShield))]
    public class MetalShield : BaseShield
    {
        [Constructable]
        public MetalShield()
            : base(0x1B7B)
        {
            this.Weight = 40.0;
			ShieldMinDamage = 3;
			ShieldMaxDamage = 5;

        }

        public MetalShield(Serial serial)
            : base(serial)
        {
        }

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
                return 3000;
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
                return 100;
            }
        }
		public override double ArmorRating
		{
			get
			{
				return 14.0; // 원하는 감소 수치를 입력하세요.
			}
		}
        public override int ArmorBase
        {
            get
            {
                return 11;
            }
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
