using System;
using Server.Targeting;
using Server.Mobiles;
using System.Collections.Generic;

namespace Server.Spells.Fifth
{
    public class MindBlastSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Mind Blast", "Por Corp Wis",
            218,
            Core.AOS ? 9002 : 9032,
            Reagent.BlackPearl,
            Reagent.MandrakeRoot,
            Reagent.Nightshade,
            Reagent.SulfurousAsh);
        public MindBlastSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
            if (Core.AOS)
                m_Info.LeftHandEffect = m_Info.RightHandEffect = 9002;
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Fifth;
            }
        }
        public override bool DelayedDamage
        {
            get
            {
                return !Core.AOS;
            }
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
            else if (Core.AOS)
            {
                if (Caster.CanBeHarmful(m) && CheckHSequence(m))
                {
                    Mobile from = Caster, target = m;

                    SpellHelper.Turn(from, target);
                    //SpellHelper.CheckReflect((int)Circle, ref from, ref target);

					int min = 70;
					int max = 100;
					double duration = 1.0;
					double damage = GetNewAosDamage(0, min, max, m);

                    Timer.DelayCall(TimeSpan.FromSeconds(duration),
					new TimerStateCallback(AosDelay_Callback),
					new object[] { Caster, target, m, (int)damage });
                }
            }
            FinishSequence();
        }

        private void AosDelay_Callback(object state)
        {
            object[] states = (object[])state;
            Mobile caster = (Mobile)states[0];
            Mobile target = (Mobile)states[1];
            Mobile defender = (Mobile)states[2];
            int damage = (int)states[3];
			int range = 3;
			int count = 0;

            if (caster.HarmfulCheck(defender))
            {
				caster.FixedParticles(0x374A, 10, 15, 5038, EffectLayer.Head);
				
                target.FixedParticles(0x374A, 10, 15, 5038, 1181, 2, EffectLayer.Head);
                target.PlaySound(0x213);

                SpellHelper.Damage(this, target, damage, 0, 0, 100, 0, 0);
            }
        }

        private class InternalTarget : Target
        {
            private readonly MindBlastSpell m_Owner;
            public InternalTarget(MindBlastSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                    m_Owner.Target((Mobile)o);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }
}