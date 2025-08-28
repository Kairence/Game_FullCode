using System;
using System.Collections.Generic;
using System.Linq;
using Server.Spells;
using Server.Mobiles;

namespace Server.Items
{
    public class PsychicAttack : WeaponAbility
    {
        public PsychicAttack()
        {
        }
        public override int BaseMana
        {
            get
            {
                return 25;
            }
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage, int level, double tactics )
        {
            if (!this.Validate(attacker) )
                return;
			
			if ( defender == null )
				return;
			
			bool bonus = attacker.Skills.Tactics.Value >= 100 ? true : false;
			bool levelCastSlowBonus = level >= 5 ? true : false;
			
			if ( !this.CalculateStam(attacker, Misc.Util.SPMStam[13,0], Misc.Util.SPMStam[13,1], level, bonus ) )
				return;

			double bonusDamage = 1.0 + level * 0.005;
			double time = 20;
			double damageDown = 0.2 + tactics * 0.0015;
			
			//계산
			damage = (int)( damage * ( 1 + bonusDamage ) );
			
			defender.FixedParticles(0x3789, 10, 25, 5032, EffectLayer.Head);
			defender.PlaySound(0x1F8);
			defender.SendLocalizedMessage(1074384); // Your mind is attacked by psychic force!
			AOS.Damage(defender, attacker, damage, false, 0, 0, 0, 0, 100, 0, 0, false, false, false);

			if( defender is PlayerMobile )
			{
				PlayerMobile pm = defender as PlayerMobile;
				pm.psychicTime = DateTime.Now + TimeSpan.FromSeconds(time);
				pm.psychicDamageDown = damageDown;
				pm.psychicSlow = levelCastSlowBonus;
			}
			else if( defender is BaseCreature )
			{
				BaseCreature bc = defender as BaseCreature;
				bc.psychicTime = DateTime.Now + TimeSpan.FromSeconds(time);
				bc.psychicDamageDown = damageDown;
				bc.psychicSlow = levelCastSlowBonus;
			}
            ClearCurrentAbility(attacker);
        }

        private static Dictionary<Mobile, PsychicAttackTimer> m_Registry = new Dictionary<Mobile, PsychicAttackTimer>();
        public static Dictionary<Mobile, PsychicAttackTimer> Registry { get { return m_Registry; } }

        public static void RemoveEffects(Mobile defender)
        {
            if (defender == null)
                return;

            BuffInfo.RemoveBuff(defender, BuffIcon.PsychicAttack);

            if (m_Registry.ContainsKey(defender))
                m_Registry.Remove(defender);

            defender.SendLocalizedMessage(1150292); // You recover from the effects of the psychic attack.
        }

        public class PsychicAttackTimer : Timer
        {
            private Mobile m_Defender;
            private int m_SpellDamageMalus;
            private int m_ManaCostMalus;
            private bool m_DoneIncrease;

            public int SpellDamageMalus { get { return m_SpellDamageMalus; } set { m_SpellDamageMalus = value; m_DoneIncrease = true; } }
            public int ManaCostMalus { get { return m_ManaCostMalus; } set { m_ManaCostMalus = value; m_DoneIncrease = true; } }
            public bool DoneIncrease { get { return m_DoneIncrease; } }

            public PsychicAttackTimer(Mobile defender)
                : base(TimeSpan.FromSeconds(10))
            {
                m_Defender = defender;
                m_SpellDamageMalus = 15;
                m_ManaCostMalus = 15;
                m_DoneIncrease = false;
                Start();
            }

            protected override void OnTick()
            {
                RemoveEffects(m_Defender);
                Stop();
            }
        }
    }
}