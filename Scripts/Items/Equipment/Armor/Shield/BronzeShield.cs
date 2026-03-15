using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(SmallPlateShield))]
    public class BronzeShield : BaseShield
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
                return 1000;
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
                return 2000;
            }
        }
		public override double ArmorRating
		{
			get
			{
				return 13.0; // 원하는 감소 수치를 입력하세요.
			}
		}	
        public override int ArmorBase
        {
            get
            {
                return 11;
            }
        }		
		
        [Constructable]
        public BronzeShield()
            : base(0x1B72)
        {
            Weight = 15.0;
			ShieldMinDamage = 1;
			ShieldMaxDamage = 3;			
		}

        public BronzeShield(Serial serial)
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