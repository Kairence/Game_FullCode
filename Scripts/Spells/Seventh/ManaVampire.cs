using System;
using Server.Targeting;
using Server.Mobiles;

namespace Server.Spells.Seventh
{
    public class ManaVampireSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Mana Vampire", "Ort Sanct",
            221,
            9032,
            Reagent.BlackPearl,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot,
            Reagent.SpidersSilk);

        public ManaVampireSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Seventh;

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

                // --- 기획: 흡혈량 계산 (500 + 보너스 * 0.1) ---
                double bonus = SpellHelper.GetMagicValue(Caster, 0.1);
                int toDrain = (int)(500 + bonus);

                // --- 기획: 상대 마나의 50%까지만 흡혈 가능 ---
                int maxDrainable = m.Mana / 2;

                if (toDrain > maxDrainable)
                    toDrain = maxDrainable;

                if (toDrain < 0)
                    toDrain = 0;

                // 내 마나 최대치까지만 회복
                int casterRecovery = toDrain;
                if (casterRecovery > (Caster.ManaMax - Caster.Mana))
                    casterRecovery = Caster.ManaMax - Caster.Mana;

                m.Mana -= toDrain;
                Caster.Mana += casterRecovery;

                // 시각 및 사운드 효과
                m.FixedParticles(0x374A, 1, 15, 5054, 23, 7, EffectLayer.Head);
                m.PlaySound(0x1F9);
                Caster.FixedParticles(0x0000, 10, 5, 2054, EffectLayer.Head);

                HarmfulSpell(m);
            }

            FinishSequence();
        }

        public override double GetResistPercent(Mobile target)
        {
            return 98.0;
        }

        private class InternalTarget : Target
        {
            private readonly ManaVampireSpell m_Owner;
            public InternalTarget(ManaVampireSpell owner)
                : base(12, false, TargetFlags.Harmful)
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