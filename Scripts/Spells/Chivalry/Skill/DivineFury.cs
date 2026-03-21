using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;

namespace Server.Spells.Chivalry
{
    public class DivineFurySpell : PaladinSpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Divine Fury", "Divinum Furis",
            -1,
            9002);

        // 외부에서 참조할 수 있도록 static 테이블 유지
        private static readonly Dictionary<Mobile, Timer> m_Table = new Dictionary<Mobile, Timer>();

        public DivineFurySpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(1.0);
        public override double RequiredSkill => 200;

        // [기획] 마나 1000 소모
        public override int RequiredMana => 1000;
        public override int RequiredTithing => 0;
        public override int MantraNumber => 1060722;

        public static bool UnderEffect(Mobile m) => m_Table.ContainsKey(m);

        public override void OnCast()
        {
            if (CheckSequence())
            {
                // [기획] 기본 60초 * 카르마 효율 (최대 150초)
                double durationSeconds = GetKarmaScaler(60.0, true);
                TimeSpan duration = TimeSpan.FromSeconds(durationSeconds);

                Caster.PlaySound(0x20F);
                Caster.PlaySound(Caster.Female ? 0x338 : 0x44A);
                Caster.FixedParticles(0x376A, 1, 31, 9961, 1160, 0, EffectLayer.Waist);
                Caster.FixedParticles(0x37C4, 1, 31, 9502, 43, 2, EffectLayer.Waist);

                // 기존 타이머 제거
                if (m_Table.ContainsKey(Caster))
                {
                    Timer t = m_Table[Caster];
                    if (t != null) t.Stop();
                }

                // 타이머 시작 및 테이블 등록
                m_Table[Caster] = Timer.DelayCall(duration, new TimerStateCallback(Expire_Callback), Caster);

                // [버프창 출력] 
                // 기사도 특공 15%, 신성피해 35% 문구를 인자로 전달 (args 순서는 클라이언트 스크립트에 따라 다를 수 있음)
                string args = String.Format("15\t35"); 
                BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.DivineFury, 1060589, 1150218, duration, Caster, args));
            }

            FinishSequence();
        }

        public static void RemoveEffects(Mobile m)
        {
            if (m_Table.ContainsKey(m))
            {
                m_Table[m].Stop();
                m_Table.Remove(m);
                BuffInfo.RemoveBuff(m, BuffIcon.DivineFury);
            }
        }

        private static void Expire_Callback(object state)
        {
            Mobile m = (Mobile)state;
            if (m_Table.ContainsKey(m))
                m_Table.Remove(m);

            m.PlaySound(0xF8);
        }
    }
}