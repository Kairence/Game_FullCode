using System;
using System.Collections.Generic;
using System.Linq;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Eighth
{
    public class EarthquakeSpell : MagerySpell
    {
        public override DamageType SpellDamageType => DamageType.SpellAOE;

        private static readonly SpellInfo m_Info = new SpellInfo(
            "Earthquake", "In Vas Por",
            233, 9012, false,
            Reagent.Bloodmoss, Reagent.Ginseng, Reagent.MandrakeRoot, Reagent.SulfurousAsh);

        public EarthquakeSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Eighth;

        // [체크] 스킬 수치와 상관없이 엔진 차단을 방지하기 위해 true 반환
        public override bool CheckCast()
        {
            return true;
        }

        public override void OnCast()
        {
            // 체인 라이트닝처럼 수동으로 체크 로직을 수행합니다.
            if (SpellHelper.CheckTown(Caster, Caster))
            {
                // 마나 부족 체크
                if (Caster.Mana < GetMana())
                {
                    Caster.SendLocalizedMessage(500613);
                    return;
                }

                // 시약 소모 체크
                if (!ConsumeReagents())
                {
                    Caster.SendLocalizedMessage(500612);
                    return;
                }

                // 자원 소모 및 연출
                Caster.Mana -= GetMana();
                Caster.PlaySound(Caster.Female ? 0x338 : 0x44B);

                // [핵심] 범용 채널링 시스템 호출
                // 간격 2초, 횟수 30번 (총 60초), 움직이면 취소(true)
                StartChanneling(TimeSpan.FromSeconds(2.0), 30, true, (tick) =>
                {
                    DoEffect();
                });
            }

            FinishSequence();
        }

        public void DoEffect()
        {
            // 10타일 범위 내 모든 대상 확보
            List<Mobile> targets = AcquireIndirectTargets(Caster.Location, 10).OfType<Mobile>().ToList();

            // 마비 확률 계산
            double bonusChance = SpellHelper.GetMagicValue(Caster, 0.00001);
            double stunChance = 0.05 + bonusChance;

            foreach (Mobile m in targets)
            {
                if (Caster.CanBeHarmful(m, false))
                {
                    Caster.DoHarmful(m);

                    // 데미지 설정 (2초마다 실행되므로 DPS 50 기준)
                    int min = 25;
                    int max = 75;

                    // GetNewAosDamage 호출 (스펠위빙 연쇄 발동 지원)
                    double damage = GetNewAosDamage(0, min, max, m);

                    if (damage > 0)
                    {
                        // 물리 100% 데미지 적용
                        SpellHelper.Damage(this, m, damage, 100, 0, 0, 0, 0);
                    }

                    // 시각 및 사운드 효과
                    m.FixedParticles(0x3779, 1, 30, 0x26EC, 0x3, 0x3, EffectLayer.Waist);
                    m.PlaySound(0x220);

                    // 확률적 2초 마비
                    if (stunChance > Utility.RandomDouble())
                    {
                        m.Paralyze(TimeSpan.FromSeconds(2.0));
                    }
                }
            }
        }

        // [중요] Spell.cs의 연쇄 발동(Extra Cast)에서 호출할 메서드
        // override 없이 리플렉션으로 호출되도록 설계
        public void OnExtraCast(Mobile target)
        {
            if (target != null && target.Alive)
            {
                double damage = GetNewAosDamage(0, 25, 75, target);

                if (damage > 0)
                {
                    SpellHelper.Damage(this, target, damage, 100, 0, 0, 0, 0);
                }

                target.FixedParticles(0x3779, 1, 30, 0x26EC, 0x3, 0x3, EffectLayer.Waist);
                target.PlaySound(0x220);
            }
        }
    }
}
