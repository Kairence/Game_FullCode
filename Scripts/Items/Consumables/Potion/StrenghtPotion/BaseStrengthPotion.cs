using System;
using Server.Mobiles;

namespace Server.Items
{
    public abstract class BaseStrengthPotion : BasePotion
    {
        public BaseStrengthPotion(PotionEffect effect)
            : base(0xF09, effect)
        {
        }

        public BaseStrengthPotion(Serial serial)
            : base(serial)
        {
        }

        public abstract int StrOffset { get; }
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

        public override void Drink(Mobile from)
        {
			if( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				if( pm.TimerList[70] == 0 )
				{
					pm.TimerList[70] = 3000;
					from.SendMessage("5분 동안 몬스터에게 피해를 {0}% 증가시킵니다.", StrOffset * 0.1);
					pm.PotionPower = StrOffset;

					from.FixedEffect(0x375A, 10, 15);
					from.PlaySound(0x1E7);

					PlayDrinkEffect(from);
					Consume();
				}
            }
			else
				from.SendLocalizedMessage(502173); // You are already under a similar effect.
		}
    }
}
