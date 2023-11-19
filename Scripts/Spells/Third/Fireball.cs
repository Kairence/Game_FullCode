using System;
using Server.Targeting;
using Server.Mobiles;
using System.Collections.Generic;

namespace Server.Spells.Third
{
    public class FireballSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Fireball", "Vas Flam",
            203,
            9041,
            Reagent.BlackPearl);
        public FireballSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Third;
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
			int range = 10;
			if ( Caster is FireElemental )
				range = 20;

            Caster.Target = new InternalTarget(this, range);
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
                if (SpellHelper.CheckReflect((int)Circle, ref source, ref target))
                {
                    Timer.DelayCall(TimeSpan.FromSeconds(.5), () =>
                        {
                            source.MovingParticles(target, 0x36D4, 7, 0, false, true, 9502, 4019, 0x160);
                            source.PlaySound(Core.AOS ? 0x15E : 0x44B);
                        });
                }
				/*
                if (SpellHelper.CheckReflect((int)Circle, ref source, ref target))
                {
                    Timer.DelayCall(TimeSpan.FromSeconds(.5), () =>
                        {
                            source.MovingParticles(target, 0x36D4, 7, 0, false, true, 9502, 4019, 0x160);
                            source.PlaySound(Core.AOS ? 0x15E : 0x44B);
                        });
                }
				*/
                double damage = 0;


                if (Core.AOS)
                {
					int min = 30;
					int max = 70;
					
					if( Caster is SummonedFireElemental )
					{
						min = 40;
						max = 60;
					}

                    damage = GetNewAosDamage(0, min, max, m);
               }

                if (damage > 0)
                {
                    Caster.MovingParticles(m, 0x36D4, 7, 0, false, true, 9502, 4019, 0x160);
                    Caster.PlaySound(Core.AOS ? 0x15E : 0x44B);

                    SpellHelper.Damage(this, target, damage, 0, 100, 0, 0, 0);
				}
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly FireballSpell m_Owner;
            public InternalTarget(FireballSpell owner, int range)
                : base(range, false, TargetFlags.Harmful)
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