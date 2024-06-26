using System;
using Server.Targeting;
using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Spells.Seventh
{
    public class FlameStrikeSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Flame Strike", "Kal Vas Flam",
            245,
            9042,
            Reagent.SpidersSilk,
            Reagent.SulfurousAsh);
        public FlameStrikeSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Seventh;
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
            this.Caster.Target = new InternalTarget(this);
        }

        public void Target(IDamageable m)
        {
            if (!this.Caster.CanSee(m))
            {
                this.Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (this.CheckHSequence(m))
            {
                SpellHelper.Turn(this.Caster, m);

                Mobile source = this.Caster;

                //SpellHelper.CheckReflect((int)this.Circle, ref source, ref m);

                double damage = 0;

				int min = 75;
				int max = 165;
				
                if (Core.AOS)
                {
                    damage = GetNewAosDamage(0, min, max, m);
                }

                if (m != null)
                {
                    m.FixedParticles(0x3709, 10, 30, 5052, EffectLayer.LeftFoot);
                    m.PlaySound(0x208);
                }

                if (damage > 0)
                {
					SpellHelper.Damage(this, m, damage, 0, 100, 0, 0, 0);
                }
            }

            this.FinishSequence();
        }
        private void AosDelay_Callback(object state)
        {
            object[] states = (object[])state;
            Mobile caster = (Mobile)states[0];
            Mobile target = (Mobile)states[1];
            int damage = (int)states[2];

            if (caster.HarmfulCheck(target))
            {
				target.FixedParticles(0x3709, 10, 30, 5052, EffectLayer.LeftFoot);
				target.PlaySound(0x208);

                SpellHelper.Damage(this, target, damage, 0, 100, 0, 0, 0);
            }
        }

        private class InternalTarget : Target
        {
            private readonly FlameStrikeSpell m_Owner;
            public InternalTarget(FlameStrikeSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
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