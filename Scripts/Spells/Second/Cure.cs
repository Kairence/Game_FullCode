using System;
using Server.Targeting;
using Server.Network;

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

        public override SpellCircle Circle => SpellCircle.First; // 2서클이지만 1서클 기반 처리 시 확인 필요

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

                // --- 1. 독 레벨 1단계 감소 로직 ---
                Poison p = m.Poison;

                if (p != null)
                {
                    int currentLevel = p.RealLevel;

                    if (currentLevel <= 0)
                    {
                        m.CurePoison(this.Caster);
                    }
                    else
                    {
                        // 독 레벨을 1단계 감소시켜 적용
                        int nextLevel = Math.Max(0, currentLevel - 1);
                        m.ApplyPoison(this.Caster, Poison.GetPoison(nextLevel));
                    }
                }

                // --- 2. 체력 회복 로직 (40~60 + 보너스 * 0.01) ---
                // 보너스 수치 계산 (GetMagicValue 사용)
                // 예: 지능 2000, 지평 100, scale 0.01 -> 보너스 20.0
                double bonus = SpellHelper.GetMagicValue(this.Caster, 0.01);
                
                int healAmount = Utility.RandomMinMax(40, 60) + (int)bonus;

                // [수정] 엔진 표준 SpellHelper.Heal 호출
                SpellHelper.Heal(healAmount, m, this.Caster);

                // 연출 및 사운드
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