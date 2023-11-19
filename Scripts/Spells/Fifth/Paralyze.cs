using System;
using Server.Mobiles;
using Server.Spells.Chivalry;
using Server.Targeting;
using System.Collections.Generic;

namespace Server.Spells.Fifth
{
    public class ParalyzeSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Paralyze", "An Ex Por",
            218,
            9012,
            Reagent.Garlic,
            Reagent.MandrakeRoot,
            Reagent.SpidersSilk);
        public ParalyzeSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Fifth;
            }
        }
        public override void OnCast()
        {
            this.Caster.Target = new InternalTarget(this);
        }

        private static readonly Dictionary<Mobile, Timer> m_Table = new Dictionary<Mobile, Timer>();
        public void Target(Mobile m)
        {
            if (!this.Caster.CanSee(m))
            {
                this.Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (m_Table.ContainsKey(m))
            {
                this.Caster.SendMessage("1분 안에 같은 타겟을 마비시킬 수 없습니다."); // Target can not be seen.
            }
			/*
            else if (Core.AOS && (m.Frozen || m.Paralyzed || (m.Spell != null && m.Spell.IsCasting && !(m.Spell is PaladinSpell))))
            {
                this.Caster.SendLocalizedMessage(1061923); // The target is already frozen.
            }
			*/
            else if (this.CheckHSequence(m))
            {
                SpellHelper.Turn(this.Caster, m);

                //SpellHelper.CheckReflect((int)this.Circle, this.Caster, ref m);

                double duration;
				
                if (Core.AOS)
                {
                    duration = 5;
                }
                else
                {
                    // Algorithm: ((20% of magery) + 7) seconds [- 50% if resisted]
                    duration = 10.0 + (this.Caster.Skills[SkillName.Magery].Value * 0.01);
                }

                if (m is PlagueBeastLord)
                {
                    ((PlagueBeastLord)m).OnParalyzed(this.Caster);
                    duration = 120;
                }

                m.Paralyze(TimeSpan.FromSeconds(duration));
                m_Table[m] = Timer.DelayCall(TimeSpan.FromSeconds(60.0), new TimerStateCallback(AosDelay_Callback), new object[] { m });

                m.PlaySound(0x204);
                m.FixedEffect(0x376A, 6, 1);

                this.HarmfulSpell(m);
            }

            this.FinishSequence();
        }
        private void AosDelay_Callback(object state)
        {
            object[] states = (object[])state;

            Mobile m = (Mobile)states[0];
            m_Table.Remove(m);
        }

        public class InternalTarget : Target
        {
            private readonly ParalyzeSpell m_Owner;
            public InternalTarget(ParalyzeSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
            {
                this.m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                    this.m_Owner.Target((Mobile)o);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                this.m_Owner.FinishSequence();
            }
        }
    }
}