using System;
using Server.Targeting;

namespace Server.Spells.First
{
    public class NightSightSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Night Sight", "In Lor",
            236, 9031,
            Reagent.SulfurousAsh, Reagent.SpidersSilk);

        public NightSightSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.First;

        public override void OnCast()
        {
            if (CheckSequence())
            {
                Mobile caster = Caster;

                // 1. 지속 시간 계산
                double bonus = SpellHelper.GetMagicValue(caster, 0.012);
                TimeSpan length = TimeSpan.FromSeconds(60.0 + bonus);

                // 2. 효과 적용 (중복 방지 체크)
                if (caster.BeginAction(typeof(NightSightSpell)))
                {
                    // 연출 및 시야
                    caster.FixedParticles(0x376A, 9, 32, 5007, EffectLayer.Waist);
                    caster.PlaySound(0x1E3);
                    caster.LightLevel = 100;

                    // 버프 아이콘 추가 (수치는 5%로 표시)
                    BuffInfo.AddBuff(caster, new BuffInfo(BuffIcon.NightSight, 1075643, length, caster, "5"));

                    // 3. 종료 타이머 (수치 원복은 AosAttributes가 자동으로 처리함)
                    Timer.DelayCall(length, () => 
                    {
                        caster.EndAction(typeof(NightSightSpell));
                        caster.LightLevel = 0;
                        BuffInfo.RemoveBuff(caster, BuffIcon.NightSight);
                    });
                }
            }
            FinishSequence();
        }
    }
}