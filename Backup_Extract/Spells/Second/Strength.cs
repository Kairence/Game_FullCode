using System;
using Server.Targeting;

namespace Server.Spells.Second
{
    public class StrengthSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Strength", "Uus Mani",
            212,
            9061,
            Reagent.MandrakeRoot,
            Reagent.Nightshade);
        public StrengthSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Second;
            }
        }
        
        public override void OnCast()
        {
            this.Caster.Target = new InternalTarget(this);
        }

        public void Target(Mobile m)
        {
            if (!this.Caster.CanSee(m))
            {
                this.Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (this.CheckBSequence(m))
            {
                SpellHelper.Turn(this.Caster, m);

                int totalBonus = 500 + (int)SpellHelper.GetMagicValue(this.Caster, 0.01);

                TimeSpan length = TimeSpan.FromSeconds(60.0); // 기본 1분

                SpellHelper.AddStatBonus(this.Caster, m, StatType.Str, totalBonus, length);

                // 4. 연출 및 버프 알림
                m.FixedParticles(0x375A, 10, 15, 5010, EffectLayer.Waist);
                m.PlaySound(0x1e7);

                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Agility, 1075841, length, m, totalBonus.ToString()));
            }

            this.FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly StrengthSpell m_Owner;
            public InternalTarget(StrengthSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Beneficial)
            {
                this.m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    this.m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                this.m_Owner.FinishSequence();
            }
        }
    }
}
