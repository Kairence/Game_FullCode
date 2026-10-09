using System;
using System.Collections.Generic;

namespace Server.Spells.Second
{
    public class ProtectionSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Protection", "Uus Sanct",
            236,
            9011,
            Reagent.Garlic,
            Reagent.Ginseng,
            Reagent.SulfurousAsh);

        public ProtectionSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        // 단일/광역 통합 관리 테이블
        private static Dictionary<Mobile, Timer> m_Table = new Dictionary<Mobile, Timer>();

        public override SpellCircle Circle => SpellCircle.Second;

        public override void OnCast()
        {
            if (CheckSequence())
            {
                Mobile caster = Caster;

                double bonus = SpellHelper.GetMagicValue(caster, 0.012);
                TimeSpan length = TimeSpan.FromSeconds(60.0 + bonus);

                // 효과 적용 (공용 메서드 호출)
                ApplyEffect(caster, length, false);
            }

            FinishSequence();
        }

        // Protection과 ArchProtection 모두가 사용하는 핵심 로직
        public static void ApplyEffect(Mobile m, TimeSpan length, bool isArch)
        {
            if (m == null) return;

            // 1. 기존에 걸린 모든 프로텍션 계열 효과 제거 (중첩 방지)
            StopTimer(m);

            // 2. 방어력 추가
            m.MeleeDamageAbsorb += 1;
            m.MagicDamageAbsorb += 1;

            // 3. 연출
            m.PlaySound(0x1E9);
            m.FixedParticles(0x375A, 9, 20, 5016, EffectLayer.Waist);

            // 4. 버프 아이콘 (광역 여부에 따라 아이콘 구분)
            BuffIcon icon = isArch ? BuffIcon.ArchProtection : BuffIcon.Protection;
            int nameNum = isArch ? 1075816 : 1075814;
            BuffInfo.AddBuff(m, new BuffInfo(icon, nameNum, length, m, "1"));

            // 5. 타이머 등록
            m_Table[m] = Timer.DelayCall(length, () => EndProtection(m));
        }

        public static void EndProtection(Mobile m)
        {
            if (m == null) return;

            if (m_Table.ContainsKey(m))
            {
                m_Table[m].Stop();
                m_Table.Remove(m);

                // 수치 차감
                m.MeleeDamageAbsorb -= 1;
                m.MagicDamageAbsorb -= 1;

                if (m.MeleeDamageAbsorb < 0) m.MeleeDamageAbsorb = 0;
                if (m.MagicDamageAbsorb < 0) m.MagicDamageAbsorb = 0;
            }

            BuffInfo.RemoveBuff(m, BuffIcon.Protection);
            BuffInfo.RemoveBuff(m, BuffIcon.ArchProtection);
        }

        public static void StopTimer(Mobile m)
        {
            // 테이블에 존재한다면 이미 효과가 적용 중인 상태이므로 수치를 먼저 원복시킴
            if (m_Table.ContainsKey(m))
            {
                m_Table[m].Stop();
                m_Table.Remove(m);

                m.MeleeDamageAbsorb -= 1;
                m.MagicDamageAbsorb -= 1;
            }
        }
    }
}
