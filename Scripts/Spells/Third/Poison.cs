using System;
using Server.Targeting;

namespace Server.Spells.Third
{
    public class PoisonSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Poison", "In Nox",
            203,
            9051,
            Reagent.Nightshade);

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Third;
            }
        }

        public PoisonSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(Mobile m)
        {
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                SpellHelper.CheckReflect((int)Circle, Caster, ref m);

                if (m.Spell != null)
                    m.Spell.OnCasterHurt();

                m.Paralyzed = false;

                if (CheckResisted(m) || Server.Spells.Mysticism.StoneFormSpell.CheckImmunity(m))
                {
                    m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                }
                else
                {
                    int level = 0;

                    if (Core.AOS)
                    {
                        level = Caster.Skills.Magery.Fixed / 500;
					}

                    m.ApplyPoison(Caster, Poison.GetPoison(level));
                }

                m.FixedParticles(0x374A, 10, 15, 5021, EffectLayer.Waist);
                m.PlaySound(0x205);

                HarmfulSpell(m);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly PoisonSpell m_Owner;

            public InternalTarget(PoisonSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }
}