using System;
using System.Collections.Generic;

namespace Server.Spells.First
{
    public class ReactiveArmorSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Reactive Armor", "Flam Sanct",
            236,
            9011,
            Reagent.Garlic,
            Reagent.SpidersSilk,
            Reagent.SulfurousAsh);

        public ReactiveArmorSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        // 지속 시간 종료 시 원복을 위한 타이머 관리 테이블
        private static Dictionary<Mobile, Timer> m_Table = new Dictionary<Mobile, Timer>();

        public override SpellCircle Circle => SpellCircle.First;

        public override void OnCast()
        {
            if (CheckSequence())
            {
                Mobile caster = Caster;

                // 1. 지속 시간 계산: 60초 + 보너스 * 0.012
                double bonus = SpellHelper.GetMagicValue(caster, 0.012);
                TimeSpan length = TimeSpan.FromSeconds(60.0 + bonus);

                // 2. 효과 적용 (이미 켜져 있다면 갱신)
                StopTimer(caster);

                // [핵심] MeleeDamageAbsorb를 1로 설정 (방어력 +1 효과)
                caster.MeleeDamageAbsorb += 1;

                // 3. 연출 및 버프 아이콘
                caster.PlaySound(0x1E9);
                caster.FixedParticles(0x376A, 9, 32, 5008, EffectLayer.Waist);

                // 버프 정보창에 "방어력 1 증가" 표시
                BuffInfo.AddBuff(caster, new BuffInfo(BuffIcon.ReactiveArmor, 1075812, length, caster, "1"));
                caster.SendMessage("리액티브 아머로 인해 물리 방어력이 1 증가했습니다.");

                // 4. 타이머 설정 (종료 시 0으로 원복)
                m_Table[caster] = Timer.DelayCall(length, () =>
                {
                    EndArmor(caster);
                });
            }

            FinishSequence();
        }

        public static void EndArmor(Mobile m)
        {
            if (m == null) return;

            StopTimer(m);
            
            // 방어력 원복
            m.MeleeDamageAbsorb -= 1;
            BuffInfo.RemoveBuff(m, BuffIcon.ReactiveArmor);
        }

        private static void StopTimer(Mobile m)
        {
            if (m_Table.ContainsKey(m))
            {
                Timer t = m_Table[m];
                if (t != null) t.Stop();
                m_Table.Remove(m);
            }
        }

        // 외부(예: 데미지 핸들러)에서 체크할 수 있는 메서드
        public static bool IsUnderEffects(Mobile m)
        {
            return m_Table.ContainsKey(m);
        }
    }
}