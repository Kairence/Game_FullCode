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
		
        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!this.Validate(attacker))
                return;
			if( attacker is PlayerMobile )
			{
				if( attacker.Stam < 15 )
					return;
				attacker.Stam -= 15;
			}
			if( Utility.RandomDouble() < 0.2 )
			{
				double duration = 10.0;
				if( defender is BaseCreature )
				{
					BaseCreature bc = defender as BaseCreature;
					duration *= MonsterTier(bc);
				}
				defender.Paralyze(TimeSpan.FromSeconds(duration));
			}
			defender.FixedParticles(0x3789, 10, 25, 5032, EffectLayer.Head);
			defender.PlaySound(0x1F8);
			defender.SendLocalizedMessage(1074384); // Your mind is attacked by psychic force!
			AOS.Damage(defender, attacker, damage, false, 0, 0, 0, 0, 100, 0, 0, false, false, false);
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