using System;
using Server.Targeting;
using Server.Mobiles;
using System.Collections.Generic;

namespace Server.Spells.Second
{
    public class HarmSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Harm", "An Mani",
            212,
            Core.AOS ? 9001 : 9041,
            Reagent.Nightshade,
            Reagent.SpidersSilk);
        public HarmSpell(Mobile caster, Item scroll)
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
        public override bool DelayedDamage
        {
            get
            {
                return false;
            }
        }
        public override void OnCast()
        {
			int range = 10;
			if ( Caster is WaterElemental )
				range = 20;
			else if ( Caster is Reaper )
				range = 15;

            this.Caster.Target = new InternalTarget(this, range);
        }
		
        public void Target(IDamageable m)
        {
            Mobile mob = m as Mobile;

            if (!this.Caster.CanSee(m))
            {
                this.Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (this.CheckHSequence(m))
            {
                SpellHelper.Turn(this.Caster, m);
                Mobile source = this.Caster;

                SpellHelper.CheckReflect((int)this.Circle, ref source, ref m);

                double damage = 0;
                if (Core.AOS)
                {
					int min = 40;
					int max = 50;
					if( Caster is Reaper )
					{
						min = 120;
						max = 160;
					}
                    damage = GetNewAosDamage(0, min, max, m);
					if( Caster is SummonedWaterElemental )
					{
						min = 40;
						max = 60;
					}
   
                    if (mob != null)
                    {
                        mob.FixedParticles(0x374A, 10, 30, 5013, 1153, 2, EffectLayer.Waist);
                        mob.PlaySound(0x0FC);
                    }
                    else
                    {
                        Effects.SendLocationParticles(m, 0x374A, 10, 30, 1153, 2, 5013, 0);
                        Effects.PlaySound(m.Location, m.Map, 0x0FC);
                    }
                }

                if (damage > 0)
                {
                    SpellHelper.Damage(this, m, damage, 0, 0, 100, 0, 0);
					Caster.MovingParticles(m, 0x36E4, 5, 0, false, false, 3006, 0, 0);
				}
            }

            this.FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly HarmSpell m_Owner;
            public InternalTarget(HarmSpell owner, int range)
                : base(range, false, TargetFlags.Harmful)
            {
                this.m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is IDamageable)
                {
                    this.m_Owner.Target((IDamageable)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                this.m_Owner.FinishSequence();
            }
        }
    }
}