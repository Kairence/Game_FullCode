using System;
using Server.Spells;

namespace Server.Items
{
    public class CureLevelInfo
    {
        private readonly Poison m_Poison;
        private readonly double m_Chance;
        public CureLevelInfo(Poison poison, double chance)
        {
            this.m_Poison = poison;
            this.m_Chance = chance;
        }

        public Poison Poison
        {
            get
            {
                return this.m_Poison;
            }
        }
        public double Chance
        {
            get
            {
                return this.m_Chance;
            }
        }
    }

    public abstract class BaseCurePotion : BasePotion
    {
        public BaseCurePotion(PotionEffect effect)
            : base(0xF07, effect)
        {
        }

        public BaseCurePotion(Serial serial)
            : base(serial)
        {
        }

        public abstract CureLevelInfo[] LevelInfo { get; }
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

        public void DoCure(Mobile from)
        {
            bool cure = false;

            CureLevelInfo[] info = this.LevelInfo;

            for (int i = 0; i < info.Length; ++i)
            {
                CureLevelInfo li = info[i];

                if (li.Poison.RealLevel == from.Poison.RealLevel &&
					Scale(from, li.Chance) > Utility.RandomDouble())
                {
                    cure = true;
                    break;
                }
            }

            if (cure && from.CurePoison(from))
            {
                from.SendLocalizedMessage(500231); // You feel cured of poison!

                from.FixedEffect(0x373A, 10, 15);
                from.PlaySound(0x1E0);
            }
            else if (!cure)
            {
                from.SendLocalizedMessage(500232); // That potion was not strong enough to cure your ailment!
            }
        }

        public override void Drink(Mobile from)
        {
            if (TransformationSpellHelper.UnderTransformation(from, typeof(Spells.Necromancy.VampiricEmbraceSpell)))
            {
                from.SendLocalizedMessage(1061652); // The garlic in the potion would surely kill you.
            }
            else if (from.Poisoned)
            {
				if (from.BeginAction(typeof(BaseCurePotion)))
				{
					DoCure(from);
					PlayDrinkEffect(from);
					from.FixedParticles(0x373A, 10, 15, 5012, EffectLayer.Waist);
					from.PlaySound(0x1E0);
					Consume();

					Timer.DelayCall(TimeSpan.FromSeconds(30.0), new TimerStateCallback(ReleaseCureLock), from);
				}
				else
				{
					from.SendLocalizedMessage(1049547); // You decide against drinking this potion, as you are already at full health.
				}
			}
            else
            {
                from.SendLocalizedMessage(1042000); // You are not poisoned.
            }
        }
        private static void ReleaseCureLock(object state)
        {
            ((Mobile)state).EndAction(typeof(BaseCurePotion));
        }
	}
}
