using System;
using Server.Targeting;
using Server.Mobiles;

namespace Server.Spells.Third
{
    public class PoisonSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Poison", "In Nox",
            203,
            9051,
            Reagent.Nightshade);

        public PoisonSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Third;

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

                if (m.Spell != null)
                    m.Spell.OnCasterHurt();

                m.Paralyzed = false;

                // --- 1. 즉각적인 독 데미지 로직 ---
                // 기획: 30 ~ 90 데미지 기반, 엔진의 GetNewAosDamage를 통해 최종 데미지 산출
                int min = 30;
                int max = 90;
                int damage = GetNewAosDamage(0, min, max, m);

                if (damage > 0)
                {
                    // 독 속성 100% 데미지
                    SpellHelper.Damage(this, m, damage, 0, 0, 0, 100, 0);
                }

                // --- 2. 중독 확률 판정 (20% + 보너스 * 0.004%) ---
                double bonus = SpellHelper.GetMagicValue(Caster, 0.004);
                double applyChance = 0.20 + (bonus * 0.01);

                if (Utility.RandomDouble() < applyChance)
                {
                    // --- 3. 독 레벨 결정 (중독술 보너스 계산) ---
                    // 기획: 스킬 30당 1레벨 확정 + (50% + 1.5% * 남은스킬) 확률로 레벨+1
                    double poisoningSkill = Caster.Skills[SkillName.Poisoning].Value;
                    
                    int baseLevel = (int)(poisoningSkill / 30.0); // 60스킬이면 2레벨 확정
                    double remainder = poisoningSkill % 30.0;
                    
                    // 상승 확률: 50% + (남은 스킬 * 1.5%)
                    double upgradeChance = (50.0 + (remainder * 1.5)) * 0.01;
                    
                    int finalLevel = baseLevel;
                    if (Utility.RandomDouble() < upgradeChance)
                    {
                        finalLevel++;
                    }

                    if (finalLevel >= 0)
                    {
                        m.ApplyPoison(Caster, Poison.GetPoison(finalLevel));
                    }
                }

                m.FixedParticles(0x374A, 10, 15, 5021, EffectLayer.Waist);
                m.PlaySound(0x205);

                HarmfulSpell(m);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly PoisonSpell m_Owner;
            public InternalTarget(PoisonSpell owner) : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful) { m_Owner = owner; }
            protected override void OnTarget(Mobile from, object o) { if (o is Mobile) m_Owner.Target((Mobile)o); }
            protected override void OnTargetFinish(Mobile from) { m_Owner.FinishSequence(); }
        }
    }
}
