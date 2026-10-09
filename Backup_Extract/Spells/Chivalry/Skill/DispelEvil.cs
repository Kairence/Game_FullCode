using System;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Chivalry
{
    public class DispelEvilSpell : PaladinSpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo("Dispel Evil", "Dispiro Malas", -1, 9002);

        public DispelEvilSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(0.25);
        public override double RequiredSkill => 150.0; // 150. 이 값은 수정하지 말 것
        public override int RequiredMana => 300;
        public override int RequiredTithing => 0;
        public override int MantraNumber => 1060721;

        public override void OnCast()
        {
            // 메저리 Dispel처럼 타겟 커서 생성
            Caster.Target = new InternalTarget(this);
        }

        public void Target(Mobile m)
        {
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else
            {
                // 1. 대상 판정 (슬레이어 그룹 활용)
                SlayerEntry undeadSlayer = SlayerGroup.GetEntryByName(SlayerName.Silver); // 언데드 슬레이어
                SlayerEntry daemonSlayer = SlayerGroup.GetEntryByName(SlayerName.Exorcism); // 엑소시즘(데몬) 슬레이어

                bool isUndead = (undeadSlayer != null && undeadSlayer.Slays(m));
                bool isDaemon = (daemonSlayer != null && daemonSlayer.Slays(m));

                // 언데드도 아니고 데몬도 아니면 무효
                if (!isUndead && !isDaemon)
                {
                    Caster.SendMessage("대상이 언데드나 데몬이 아닙니다.");
                    return;
                }
                else if (CheckHSequence(m))
                {
                    SpellHelper.Turn(Caster, m);

                    // 시전자 이펙트
                    Caster.PlaySound(0xF5);
                    Caster.FixedParticles(0x37C4, 1, 25, 9922, 14, 3, EffectLayer.Head);

                    // [기획] 데미지 계산: 600 * 카르마 효율 (최대 1500)
                    int damage = (int)GetKarmaScaler(600.0, true);

                    // 대상 이펙트 및 사운드
                    m.FixedParticles(0x3709, 10, 30, 5052, EffectLayer.LeftFoot);
                    m.PlaySound(0x201);
                    m.PlaySound(0x299);

                    // 피해 입힘 (에너지 100% 또는 신성 속성 적용)
                    // SpellHelper.Damage를 사용하여 마법 데미지 판정 처리
                    SpellHelper.Damage(this, m, damage, 0, 0, 0, 0, 100);

                    this.HarmfulSpell(m);
                }
            }

            this.FinishSequence();
        }

        public class InternalTarget : Target
        {
            private readonly DispelEvilSpell m_Owner;

            public InternalTarget(DispelEvilSpell owner) : base(12, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                    m_Owner.Target((Mobile)o);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }
}
