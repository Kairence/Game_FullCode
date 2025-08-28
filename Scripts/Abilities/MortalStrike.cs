using System;
using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Items
{
    /// <summary>
    /// The assassin's friend.
    /// A successful Mortal Strike will render its victim unable to heal any damage for several seconds. 
    /// Use a gruesome follow-up to finish off your foe.
    /// </summary>
    public class MortalStrike : WeaponAbility
    {
        public static readonly TimeSpan PlayerDuration = TimeSpan.FromSeconds(6.0);
        public static readonly TimeSpan NPCDuration = TimeSpan.FromSeconds(12.0);

        private static readonly Dictionary<Mobile, Timer> m_Table = new Dictionary<Mobile, Timer>();
        private static readonly List<Mobile> m_EffectReduction = new List<Mobile>();
        public MortalStrike()
        {
        }
        public static bool IsWounded(Mobile m)
        {
            return m_Table.ContainsKey(m);
        }
        public override int BaseMana
        {
            get
            {
                return 10;
            }
        }
        public static void BeginWound(Mobile m, TimeSpan duration)
        {
            Timer t;

            if (m_Table.ContainsKey(m))
            {
                EndWound(m, true);
            }

            if (Core.HS && m_EffectReduction.Contains(m))
            {
                double d = duration.TotalSeconds;
                duration = TimeSpan.FromSeconds(d / 2);
            }

            t = new InternalTimer(m, duration);
            m_Table[m] = t;

            t.Start();

            m.YellowHealthbar = true;
            BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.MortalStrike, 1075810, 1075811, duration, m));
        }

        public static void EndWound(Mobile m, bool natural = false)
        {
            if (!IsWounded(m))
                return;

            Timer t = m_Table[m];

            if (t != null)
                t.Stop();

            m_Table.Remove(m);

            BuffInfo.RemoveBuff(m, BuffIcon.MortalStrike);

            m.YellowHealthbar = false;
            m.SendLocalizedMessage(1060208); // You are no longer mortally wounded.

            if (Core.HS && natural && !m_EffectReduction.Contains(m))
            {
                m_EffectReduction.Add(m);

                Timer.DelayCall(TimeSpan.FromSeconds(8), () =>
                    {
                        if (m_EffectReduction.Contains(m))
                            m_EffectReduction.Remove(m);
                    });
            }
        }
        public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double tactics )
        {
            if (!this.Validate(attacker) )
                return;
			
			if ( defender == null )
				return;
			
			bool bonus = attacker.Skills.Tactics.Value >= 100 ? true : false;
			//double levelDeathBonus = level >= 5 ? 0.66 : 0;
			//bool levelSneakBonus = level >= 5 ? true : false;
			
			if ( !this.CalculateStam(attacker, Misc.Util.SPMStam[8,0], Misc.Util.SPMStam[8,1], level, bonus ) )
				return;

			bool overkill = false;

            ClearCurrentAbility(attacker);

			/*
			if( defender is BaseCreature )
			{
				BaseCreature bc = defender as BaseCreature;
				double deathChance = 2.0 + level * 0.11 + tactics * 0.005 + levelDeathBonus - bc.Fame * 0.0001 - Misc.Util.MonsterTierCalc(bc);
				if( deathChance > 0 && Utility.RandomDouble() < deathChance )
				{
					overkill = true;

					attacker.SendLocalizedMessage(1063129); // You catch your opponent off guard with your Surprise Attack!
					defender.SendLocalizedMessage(1063130); // Your defenses are lowered as your opponent surprises you!

					defender.FixedParticles(0x37B9, 1, 5, 0x26DA, 0, 3, EffectLayer.Head);

					AOS.Damage(defender, attacker, defender.HitsMax + 10000, true, 100, 0, 0, 0, 0, 0, 0, false, false, false);
					return;
				}
			}
			*/
			damage = (int)( damage * ( 7.66 + level * 0.0666 ) );

			if( Utility.RandomDouble() < tactics * 0.005 )
			{
				int specialDamage = (int)( damage * 1 + Misc.Util.SneakCalc(attacker, defender, damage, 1) );
				damage += specialDamage;
			}
			
			attacker.FixedParticles(0x37BE, 1, 5, 0x26BD, 0x0, 0x1, EffectLayer.Waist);
			attacker.PlaySound(0x510);

			attacker.SendLocalizedMessage(1063100); // Your quick flight to your target causes extra damage as you strike!
			defender.FixedParticles(0x37BE, 1, 5, 0x26BD, 0, 0x1, EffectLayer.Waist);
			AOS.Damage(defender, attacker, damage, false, 100, 0, 0, 0, 0, 0, 0, false, false, false);
        }

        private class InternalTimer : Timer
        {
            private readonly Mobile m_Mobile;
            public InternalTimer(Mobile m, TimeSpan duration)
                : base(duration)
            {
                m_Mobile = m;
                Priority = TimerPriority.TwoFiftyMS;
            }

            protected override void OnTick()
            {
                EndWound(m_Mobile, true);
            }
        }
    }
}