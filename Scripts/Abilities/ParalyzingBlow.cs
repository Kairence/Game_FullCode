using System;
using System.Collections;
using Server.Mobiles;

namespace Server.Items
{
    /// <summary>
    /// A successful Paralyzing Blow will leave the target stunned, unable to move, attack, or cast spells, for a few seconds.
    /// </summary>
    public class ParalyzingBlow : WeaponAbility
    {
        public static readonly TimeSpan PlayerFreezeDuration = TimeSpan.FromSeconds(3.0);
        public static readonly TimeSpan NPCFreezeDuration = TimeSpan.FromSeconds(6.0);
        public static readonly TimeSpan FreezeDelayDuration = TimeSpan.FromSeconds(8.0);
        // No longer active in pub21:
        private static readonly Hashtable m_Table = new Hashtable();
        public ParalyzingBlow()
        {
        }
        public static bool IsImmune(Mobile m)
        {
            return m_Table.Contains(m);
        }

        public static void BeginImmunity(Mobile m, TimeSpan duration)
        {
            Timer t = (Timer)m_Table[m];

            if (t != null)
                t.Stop();

            t = new InternalTimer(m, duration);
            m_Table[m] = t;

            t.Start();
        }

        public static void EndImmunity(Mobile m)
        {
            Timer t = (Timer)m_Table[m];

            if (t != null)
                t.Stop();

            m_Table.Remove(m);
        }

        public override bool RequiresSecondarySkill(Mobile from)
        {
            BaseWeapon weapon = from.Weapon as BaseWeapon;

            if (weapon == null)
                return true;

            return weapon.Skill != SkillName.Wrestling;
        }

        public override bool OnBeforeSwing(Mobile attacker, Mobile defender)
        {
            if(defender == null)
                return false;
                
            if (defender.Paralyzed)
            {
                if (attacker != null)
                {
                    attacker.SendLocalizedMessage(1061923); // The target is already frozen.
                }

                return false;
            }

            return true;
        }
        public override int BaseMana
        {
            get
            {
                return 10;
            }
        }
        public override void BeforeAttack(Mobile attacker, Mobile defender, int damage)
        {
			BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            if ( !this.Validate(attacker)|| (!attacker.InRange(defender, weapon.MaxRange)) )
                return;
			
			if( attacker is PlayerMobile )
			{
				if( attacker.Stam < 20 )
					return;
				attacker.Stam -= 20;
			}

            ClearCurrentAbility(attacker);

            if (IsImmune(defender))	//Intentionally going after Mana consumption
            {
                attacker.SendLocalizedMessage(1070804); // Your target resists paralysis.
                defender.SendLocalizedMessage(1070813); // You resist paralysis.
                return;
            }

            defender.FixedEffect(0x376A, 9, 32);
            defender.PlaySound(0x204);

            attacker.SendLocalizedMessage(1060163); // You deliver a paralyzing blow!
            defender.SendLocalizedMessage(1060164); // The attack has temporarily paralyzed you!

			if( attacker is Beholder || Utility.RandomDouble() < 0.2 )
			{
				double duration = 10.0;
				if( defender.Paralyzed )
				{
					duration = 0;
					damage *= 2;
				}
				if( attacker is Beholder )
				{
					damage *= 5;
				}

				if( defender is BaseCreature )
				{
					BaseCreature bc = defender as BaseCreature;
					duration *= MonsterTier(bc);
				}
				defender.Paralyze(TimeSpan.FromSeconds(duration));
			}
			AOS.Damage(defender, attacker, damage, true, 100, 0, 0, 0, 0, 0, 0, false, false, false);
            // Treat it as paralyze not as freeze, effect must be removed when damaged.

            //BeginImmunity(defender, duration + FreezeDelayDuration);
        }

        private class InternalTimer : Timer
        {
            private readonly Mobile m_Mobile;
            public InternalTimer(Mobile m, TimeSpan duration)
                : base(duration)
            {
                this.m_Mobile = m;
                this.Priority = TimerPriority.TwoFiftyMS;
            }

            protected override void OnTick()
            {
                EndImmunity(this.m_Mobile);
            }
        }
    }
}
