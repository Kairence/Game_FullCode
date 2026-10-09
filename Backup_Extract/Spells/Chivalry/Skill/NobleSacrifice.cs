using System;
using Server.Targeting;
using Server.Mobiles;

namespace Server.Spells.Chivalry
{
    public class NobleSacrificeSpell : PaladinSpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo("Noble Sacrifice", "Dium Prostra", -1, 9002);

        public NobleSacrificeSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        // CS0534 에러 해결: 필수 추상 멤버 구현
        public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(1.5);
        public override int MantraNumber => 1060725; // Dium Prostra

        public override int RequiredMana => 500; // 기획: 마나 500 소모
        public override double RequiredSkill => 200.0;

        // [기획] 십일조 값 0으로 명시
        public override int RequiredTithing => 0;

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(Mobile m)
        {
            if (!Caster.CanSee(m) || !Caster.CanBeBeneficial(m, false, true))
            {
                // 유효하지 않은 대상 처리
            }
            else if (CheckSequence())
            {
                // 시전자 효과음 및 이펙트
                Caster.PlaySound(0x244);
                Caster.FixedParticles(0x3709, 1, 30, 9965, 5, 7, EffectLayer.Waist);

                // [기획] 카르마 효율 계산 (기본 400 * 카르마 보정 배율 = 최대 1000)
                int toHeal = (int)GetKarmaScaler(400.0, true);

                // SpellHelper 힐 체크 및 적용
                SpellHelper.Heal(toHeal, m, Caster);

                // 대상 이펙트
                m.FixedParticles(0x375A, 1, 15, 5005, 5, 3, EffectLayer.Head);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private NobleSacrificeSpell m_Owner;

            public InternalTarget(NobleSacrificeSpell owner) : base(12, false, TargetFlags.Beneficial)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Mobile)
                    m_Owner.Target((Mobile)targeted);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }
}
