using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(GargishKiteShield))]
    public class MetalKiteShield : BaseShield, IDyable
    {
        [Constructable]
        public MetalKiteShield()
            : base(0x1B74)
        {
            this.Weight = 45.0;
			ShieldMinDamage = 4;
			ShieldMaxDamage = 7;
        }

        public MetalKiteShield(Serial serial)
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
                return 4000;
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
				return 18.0; // 원하는 감소 수치를 입력하세요.
			}
		}
        public override int ArmorBase
        {
            get
            {
                return 11;
            }
        }		
        public bool Dye(Mobile from, DyeTub sender)
        {
            if (this.Deleted)
                return false;

            this.Hue = sender.DyedHue;

            return true;
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