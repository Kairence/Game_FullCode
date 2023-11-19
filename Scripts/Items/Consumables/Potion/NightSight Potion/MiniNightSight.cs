using System;
using Server.Mobiles;

namespace Server.Items
{
    public class MiniNightSightPotion : BasePotion
    {
        [Constructable]
        public MiniNightSightPotion()
            : base(0xF06, PotionEffect.NightsightMini)
        {
  			Name = "최하급 방어 물약";
        }

        public MiniNightSightPotion(Serial serial)
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

        public override void Drink(Mobile from)
        {
			if( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				pm.TimerList[69] = 3000;
				from.SendMessage("5분 동안 몬스터에게 피해를 10% 감소시킵니다.");
				pm.PotionDefense = 100;

				from.FixedParticles(0x376A, 9, 32, 5007, EffectLayer.Waist);
				from.PlaySound(0x1E3);

				PlayDrinkEffect(from);
				Consume();
			}
        }
    }
}
