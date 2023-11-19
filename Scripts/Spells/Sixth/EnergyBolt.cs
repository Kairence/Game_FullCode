using System;
using Server.Targeting;
using Server.Mobiles;
using System.Collections.Generic;

namespace Server.Spells.Sixth
{
    public class EnergyBoltSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Energy Bolt", "Corp Por",
            230,
            9022,
            Reagent.BlackPearl,
            Reagent.Nightshade);
        public EnergyBoltSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

		public int Range = 10;
		
        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Sixth;
            }
        }
        public override bool DelayedDamage
        {
            get
            {
                return true;
            }
        }
        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IDamageable m)
        {
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(m))
            {
                IDamageable source = Caster;
                IDamageable target = m;

                SpellHelper.Turn(Caster, m);

                double damage = 0;

                if (Core.AOS)
                {
                    Mobile mob = m as Mobile;
					int min = 30;
					int max = 180;

                    damage = GetNewAosDamage(0, min, max, mob);
                }
                else if (m is Mobile)
                {
                    Mobile mob = m as Mobile;
                    damage = Utility.Random(24, 18);

					/*
                    if (CheckResisted(mob))
                    {
                        damage *= 0.75;

                        mob.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                    }
					*/
                    // Scale damage based on evalint and resist
                    damage *= GetDamageScalar(mob);
                }

                // Do the effects
                Caster.MovingParticles(m, 0x379F, 7, 0, false, true, 3043, 4043, 0x211);
                Caster.PlaySound(0x20A);

                if (damage > 0)
                {
                    // Deal the damage
                    SpellHelper.Damage(this, target, damage, 0, 0, 0, 0, 100);
				}
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly EnergyBoltSpell m_Owner;
            public InternalTarget(EnergyBoltSpell owner)
                : base(owner.Range, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is IDamageable)
                    m_Owner.Target((IDamageable)o);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }
}