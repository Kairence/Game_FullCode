using System;
using System.Collections.Generic;
using Server.Targeting;

namespace Server.Spells.Fourth
{
    public class ManaDrainSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Mana Drain", "Ort Rel",
            215,
            9031,
            Reagent.BlackPearl,
            Reagent.MandrakeRoot,
            Reagent.SpidersSilk);

        public ManaDrainSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Fourth;

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
            else if (this.CheckHSequence(m))
            {
                SpellHelper.Turn(this.Caster, m);
                SpellHelper.CheckReflect((int)this.Circle, this.Caster, ref m);

                if (m.Spell != null)
                    m.Spell.OnCasterHurt();

                m.Paralyzed = false;

                // --- 1. 마나 하한선 계산 (최대 마나의 50%) ---
                int manaThreshold = (int)(m.ManaMax * 0.5);

                if (m.Mana <= manaThreshold)
                {
                    // 이미 마나가 50% 이하인 경우 효과 없음
                    m.PlaySound(0x1DF); 
                }
                else
                {
                    // --- 2. 마나 제거량 계산 (500 + 보너스 * 0.1) ---
                    double bonus = SpellHelper.GetMagicValue(Caster, 0.1);
                    int toDrain = 500 + (int)bonus;

                    // --- 3. 최종 마나 적용 (하한선 밑으로는 내려가지 않도록 차단) ---
                    int newMana = m.Mana - toDrain;

                    if (newMana < manaThreshold)
                    {
                        newMana = manaThreshold; // 50% 미만으로 내려가면 50%로 고정
                    }

                    m.Mana = newMana;

                    // 연출 및 사운드
                    m.FixedParticles(0x3789, 10, 25, 5032, EffectLayer.Head);
                    m.PlaySound(0x1F8);
                }

                this.HarmfulSpell(m);
            }

            this.FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly ManaDrainSpell m_Owner;
            public InternalTarget(ManaDrainSpell owner)
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