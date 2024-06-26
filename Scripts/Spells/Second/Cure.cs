using System;
using Server.Targeting;

namespace Server.Spells.Second
{
    public class CureSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Cure", "An Nox",
            212,
            9061,
            Reagent.Garlic,
            Reagent.Ginseng);
        public CureSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Second;
            }
        }
        
        public override void OnCast()
        {
            this.Caster.Target = new InternalTarget(this);
        }

        public void Target(Mobile m)
        {
            if (!this.Caster.CanSee(m))
            {
                this.Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (this.CheckBSequence(m))
            {
                SpellHelper.Turn(this.Caster, m);

                Poison p = m.Poison;

                if (p != null)
                {
					int CureCheck = (int)(this.Caster.Skills[SkillName.Magery].Value / 75 );
					
					if( p.RealLevel <= CureCheck )
					{
                        if (m.CurePoison(this.Caster))
                        {
                            if (this.Caster != m)
                                this.Caster.SendLocalizedMessage(1010058); // You have cured the target of all poisons!
							

                            m.SendLocalizedMessage(1010059); // You have been cured of all poisons.
                        }
                    }
                    else
                    {
                        m.SendLocalizedMessage(1010060); // You have failed to cure your target!
                    }
                }

                m.FixedParticles(0x373A, 10, 15, 5012, EffectLayer.Waist);
                m.PlaySound(0x1E0);
            }

            this.FinishSequence();
        }

        public class InternalTarget : Target
        {
            private readonly CureSpell m_Owner;
            public InternalTarget(CureSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Beneficial)
            {
                this.m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    this.m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                this.m_Owner.FinishSequence();
            }
        }
    }
}
