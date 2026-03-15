using System;
using Server.Mobiles;
using Server.Targeting;

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

        public override SpellCircle Circle => SpellCircle.Fifth;

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
            // --- 1. 중복 마비 방지: 이미 마비되거나 굳은 적에게는 적용 불가 (메시지 출력 안함) ---
            else if (m.Paralyzed || m.Frozen)
            {
                // 아무 처리 없이 넘어감으로써 마나/시퀀스 소모를 방지
            }
            else if (this.CheckHSequence(m))
            {
                SpellHelper.Turn(this.Caster, m);
                SpellHelper.CheckReflect((int)this.Circle, this.Caster, ref m);

                if (m.Spell != null)
                    m.Spell.OnCasterHurt();

                // 대상의 마비를 풀기 위해 기존 마비 상태 해제 후 재적용하는 엔진 대응 (필요 시)
                m.Paralyzed = false;

                // --- 2. 지속 시간 계산 (10초 + 보너스 * 0.002) ---
                double bonus = SpellHelper.GetMagicValue(this.Caster, 0.002);
                double duration = 10.0 + bonus;

                // 특수 몬스터 예외 처리
                if (m is PlagueBeastLord)
                {
                    ((PlagueBeastLord)m).OnParalyzed(this.Caster);
                    duration = 120.0;
                }

                // 마비 적용
                m.Paralyze(TimeSpan.FromSeconds(duration));

                // 연출 및 사운드
                m.PlaySound(0x204);
                m.FixedEffect(0x376A, 6, 1);

                this.HarmfulSpell(m);
            }

            this.FinishSequence();
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