using System;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Spells.Fourth
{
    public class GreaterHealSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Greater Heal", "In Vas Mani",
            204,
            9061,
            Reagent.Garlic,
            Reagent.Ginseng,
            Reagent.MandrakeRoot,
            Reagent.SpidersSilk);

        public GreaterHealSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Fourth;

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
            else if (m is BaseCreature && ((BaseCreature)m).IsAnimatedDead)
            {
                this.Caster.SendLocalizedMessage(1061654); // You cannot heal that which is not alive.
            }
            else if (m.IsDeadBondedPet)
            {
                this.Caster.SendLocalizedMessage(1060177); // You cannot heal a creature that is already dead!
            }
            else if (m is IRepairableMobile)
            {
                this.Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500951); // You cannot heal that.
            }
            else if (this.CheckBSequence(m))
            {
                SpellHelper.Turn(this.Caster, m);

                // --- 1. 회복량 계산 (150 ~ 250 + 보너스 * 0.04) ---
                double bonus = SpellHelper.GetMagicValue(Caster, 0.04);
                int toHeal = Utility.RandomMinMax(150, 250) + (int)bonus;

                // --- 2. 최종 회복 적용 (SpellHelper 내부에서 어그로 등 모든 로직 처리) ---
                SpellHelper.Heal(toHeal, m, this.Caster);

                // 연출 및 사운드
                m.FixedParticles(0x376A, 9, 32, 5030, EffectLayer.Waist);
                m.PlaySound(0x202);
            }

            this.FinishSequence();
        }

        public class InternalTarget : Target
        {
            private readonly GreaterHealSpell m_Owner;
            public InternalTarget(GreaterHealSpell owner)
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