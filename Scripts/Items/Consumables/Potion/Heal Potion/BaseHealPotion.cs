using System;
using Server.Network;

namespace Server.Items
{
    public abstract class BaseHealPotion : BasePotion
    {
        public BaseHealPotion(PotionEffect effect)
            : base(0xF0C, effect)
        {
        }

        public BaseHealPotion(Serial serial)
            : base(serial)
        {
        }

        public abstract int MinHeal { get; }
        public abstract int MaxHeal { get; }
        public abstract double Delay { get; }
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

        public void DoHeal(Mobile from)
        {
            int min = Scale(from, this.MinHeal);
            int max = Scale(from, this.MaxHeal);

            from.Heal(Utility.RandomMinMax(min, max));
        }

        public override void Drink(Mobile from)
        {
            if (from.Hits < from.HitsMax)
            {
				if (from.BeginAction(typeof(BaseHealPotion)))
				{
					DoHeal(from);
					PlayDrinkEffect(from);
					Consume();

					Timer.DelayCall(TimeSpan.FromSeconds(0.0), new TimerStateCallback(ReleaseHealLock), from);
				}
				else
				{
					from.SendMessage("동일한 포션은 30초 이내에 마실 수 없습니다!"); // You must wait 10 seconds before using another healing potion.
				}
            }
            else
            {
                from.SendLocalizedMessage(1049547); // You decide against drinking this potion, as you are already at full health.
            }
        }

        private static void ReleaseHealLock(object state)
        {
            ((Mobile)state).EndAction(typeof(BaseHealPotion));
        }
    }
}
