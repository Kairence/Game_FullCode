using System;
using Server.Targeting;
using Server.Network;

namespace Server.Spells.Third
{
    public class TelekinesisSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Telekinesis", "Ort Por Ylem",
            203,
            9031,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot);

        public TelekinesisSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Third;

        public override void OnCast()
        {
            this.Caster.Target = new InternalTarget(this);
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

                // --- 1. 에너지 데미지 계산 (DPS 60 기반: 30 ~ 150) ---
                int min = 30;
                int max = 150;
                int damage = GetNewAosDamage(0, min, max, m);

                if (damage > 0)
                {
                    // 에너지 속성 100% 데미지
                    SpellHelper.Damage(this, m, damage, 0, 0, 0, 0, 100);
                }

                // --- 2. 마비 확률 판정 (5% + 보너스 * 0.001%) ---
                // 예: 보너스 2000일 때 5% + 2% = 7% 확률로 5초 마비
                double bonus = SpellHelper.GetMagicValue(Caster, 0.001);
                double chance = 0.05 + (bonus * 0.01); 

                if (Utility.RandomDouble() < chance)
                {
                    m.Paralyze(TimeSpan.FromSeconds(5.0)); // 성공 시 5초
                }

                // 에너지 공격 연출 (번개 혹은 염동력 이펙트)
                Effects.SendTargetParticles(m, 0x3779, 1, 32, 0x13BA, EffectLayer.Head);
                m.PlaySound(0x1F5);

                HarmfulSpell(m);
            }

            this.FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly TelekinesisSpell m_Owner;
            public InternalTarget(TelekinesisSpell owner) : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful) { m_Owner = owner; }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
                else if (o is ITelekinesisable)
                {
                    // 기존의 아이템 조작 기능도 유지 (스위치 등 작동용)
                    if (m_Owner.CheckSequence())
                    {
                        ((ITelekinesisable)o).OnTelekinesis(from);
                    }
                }
            }

            protected override void OnTargetFinish(Mobile from) { m_Owner.FinishSequence(); }
        }
    }
}

namespace Server
{
    public interface ITelekinesisable : IPoint3D
    {
        void OnTelekinesis(Mobile from);
    }
}

