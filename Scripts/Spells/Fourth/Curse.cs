using System;
using System.Collections.Generic;
using Server.Targeting;
using Server.Mobiles;

namespace Server.Spells.Fourth
{
    public class CurseSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Curse", "Des Sanct",
            227, 9031,
            Reagent.Nightshade, Reagent.Garlic, Reagent.SulfurousAsh);

        public CurseSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Fourth;

        // --- [Line 156 에러 해결: UnderEffect 추가] ---
        // Enchanted Apple 등이 저주 상태인지 확인할 때 사용합니다.
        public static bool UnderEffect(Mobile m)
        {
            return !m.CanBeginAction(typeof(CurseSpell));
        }

        // --- [Line 3208 에러 해결: AddEffect 추가] ---
        // 무기 효과 등으로 인해 강제로 저주를 걸 때 사용합니다.
        public static void AddEffect(Mobile caster, Mobile target)
        {
            // 직접 주문 객체를 생성하여 타겟팅 로직을 실행합니다.
            new CurseSpell(caster, null).Target(target);
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(Mobile m)
        {
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237);
            }
            else if (CheckHSequence(m))
            {
                SpellHelper.Turn(Caster, m);
                SpellHelper.CheckReflect((int)Circle, Caster, ref m);

                // 1. 절대치 저주 수치 계산
                int totalPenalty = 500 + (int)SpellHelper.GetMagicValue(Caster, 0.1);

                // 2. 지속 시간 계산
                double timeBonus = SpellHelper.GetMagicValue(Caster, 0.012);
                TimeSpan length = TimeSpan.FromSeconds(60.0 + timeBonus);

                if (Mysticism.StoneFormSpell.CheckImmunity(m))
                {
                    Caster.SendLocalizedMessage(1080192);
                }
                else if (m.BeginAction(typeof(CurseSpell))) // 중복 방지 액션 시작
                {
                    // 3. 3대 능력치 절대치 감소 적용
                    SpellHelper.AddStatCurse(Caster, m, StatType.Str, totalPenalty, length);
                    SpellHelper.AddStatCurse(Caster, m, StatType.Dex, totalPenalty, length);
                    SpellHelper.AddStatCurse(Caster, m, StatType.Int, totalPenalty, length);

                    // 4. 연출 및 버프 아이콘
                    m.FixedParticles(0x374A, 10, 15, 5028, EffectLayer.Waist);
                    m.PlaySound(0x1E1);

                    string args = String.Format("{0}\t{1}\t{2}", totalPenalty, totalPenalty, totalPenalty);
                    BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Curse, 1075835, 1075836, length, m, args));

                    if (m.Spell != null)
                        m.Spell.OnCasterHurt();

                    m.Paralyzed = false;
                    HarmfulSpell(m);

                    // 지속시간 종료 시 해제 예약
                    Timer.DelayCall(length, () => RemoveEffect(m));
                }
            }
            FinishSequence();
        }

        public static void RemoveEffect(Mobile m)
        {
            if (m.CanBeginAction(typeof(CurseSpell))) return;

            m.EndAction(typeof(CurseSpell));
            
            m.RemoveStatMod("[Magic] Str Curse");
            m.RemoveStatMod("[Magic] Dex Curse");
            m.RemoveStatMod("[Magic] Int Curse");
            
            BuffInfo.RemoveBuff(m, BuffIcon.Curse);
        }

        private class InternalTarget : Target
        {
            private readonly CurseSpell m_Owner;
            public InternalTarget(CurseSpell owner) : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful) { m_Owner = owner; }
            protected override void OnTarget(Mobile from, object o) { if (o is Mobile) m_Owner.Target((Mobile)o); }
            protected override void OnTargetFinish(Mobile from) { m_Owner.FinishSequence(); }
        }
    }
}