using System;
using Server.Targeting;
using System.Collections.Generic;

namespace Server.Spells.Third
{
    public class BlessSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Bless", "Rel Sanct",
            203, 9061,
            Reagent.Garlic, Reagent.MandrakeRoot);

        public BlessSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Third;

        // --- [Line 1215 에러 해결: IsBlessed 메서드 추가] ---
        // 대상이 이미 블레스 효과(스탯 보너스)를 받고 있는지 확인합니다.
        public static bool IsBlessed(Mobile m)
        {
            if (m == null) return false;
            
            // SpellHelper의 보너스 액션 점유 여부로 판단하거나 
            // 블레스 전용 버프 아이콘 유무로 판단합니다.
            return !m.CanBeginAction(typeof(BlessSpell));
        }

        public override void OnCast()
        {
            this.Caster.Target = new InternalTarget(this);
        }

        public void Target(Mobile m)
        {
            if (!this.Caster.CanSee(m))
            {
                this.Caster.SendLocalizedMessage(500237);
            }
            else if (this.CheckBSequence(m))
            {
                SpellHelper.Turn(this.Caster, m);

                // 1. 유저님 기획 보너스 계산 (500 + 보너스 * 0.1)
                int totalBonus = 500 + (int)SpellHelper.GetMagicValue(this.Caster, 0.1);

                // 2. 지속 시간 계산 (1분 고정 + 지능 보너스 반영)
                double timeBonus = SpellHelper.GetMagicValue(this.Caster, 0.012);
                TimeSpan length = TimeSpan.FromSeconds(60.0 + timeBonus);

                // 중복 시전 방지 액션 시작
                if (m.BeginAction(typeof(BlessSpell)))
                {
                    // 3. 3대 능력치 절대치 적용
                    SpellHelper.AddStatBonus(this.Caster, m, StatType.Str, totalBonus, length);
                    SpellHelper.AddStatBonus(this.Caster, m, StatType.Dex, totalBonus, length);
                    SpellHelper.AddStatBonus(this.Caster, m, StatType.Int, totalBonus, length);

                    // 4. 연출 및 버프 아이콘
                    m.FixedParticles(0x373A, 10, 15, 5018, EffectLayer.Waist);
                    m.PlaySound(0x1EA);

                    string args = String.Format("{0}\t{1}\t{2}", totalBonus, totalBonus, totalBonus);
                    BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Bless, 1075847, 1075848, length, m, args));
                    
                    // 타이머 종료 시 액션을 해제하기 위한 딜레이 콜
                    Timer.DelayCall(length, new TimerStateCallback(RemoveEffect), m);
                }
            }

            this.FinishSequence();
        }

        // 효과 만료 시 호출되는 메서드
        public static void RemoveEffect(object state)
        {
            Mobile m = (Mobile)state;
            m.EndAction(typeof(BlessSpell));
            BuffInfo.RemoveBuff(m, BuffIcon.Bless);
        }

        private class InternalTarget : Target
        {
            private readonly BlessSpell m_Owner;
            public InternalTarget(BlessSpell owner) : base(12, false, TargetFlags.Beneficial) { m_Owner = owner; }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile) m_Owner.Target((Mobile)o);
            }

            protected override void OnTargetFinish(Mobile from) { m_Owner.FinishSequence(); }
        }
    }
}
