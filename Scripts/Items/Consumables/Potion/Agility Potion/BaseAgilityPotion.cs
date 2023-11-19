using System;

namespace Server.Items
{
    public abstract class BaseAgilityPotion : BasePotion
    {
        public BaseAgilityPotion(PotionEffect effect)
            : base(0xF08, effect)
        {
        }

        public BaseAgilityPotion(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }
        public abstract int Refresh { get; }
        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override void Drink(Mobile from)
        {
            if (from.Mana < from.ManaMax)
            {
				if (from.BeginAction(typeof(BaseAgilityPotion)))
				{
					int mana = Scale(from, this.Refresh);
					from.Mana += mana;

					PlayDrinkEffect(from);
					Consume();
					Timer.DelayCall(TimeSpan.FromSeconds(0.0), new TimerStateCallback(ReleaseAgilityLock), from);
				}
				else
				{
					from.SendMessage("동일한 포션은 30초 이내에 마실 수 없습니다!"); // You must wait 10 seconds before using another healing potion.
				}
            }
            else
            {
                from.SendMessage("You decide against drinking this potion, as you are already at full mana.");
            }
        }
        private static void ReleaseAgilityLock(object state)
        {
            ((Mobile)state).EndAction(typeof(BaseAgilityPotion));
        }
	}
}
