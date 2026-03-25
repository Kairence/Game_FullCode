using System;
using Server.Items;
using Server.Targeting;
using Server.SkillHandlers; // 추가

namespace Server.Spells.Second
{
    public class RemoveTrapSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Remove Trap", "An Jux",
            212, 9001, Reagent.Bloodmoss, Reagent.SulfurousAsh);

        public RemoveTrapSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Second;

        public override void OnCast()
        {
            this.Caster.Target = new InternalTarget(this);
        }

        public void Target(object targeted)
        {
            if (targeted is Item && !this.Caster.CanSee((Item)targeted))
            {
                this.Caster.SendLocalizedMessage(500237);
                return;
            }

            // 기획 반영: 20 + 보너스 * 0.004
            double bonus = SpellHelper.GetMagicValue(this.Caster, 0.004);
            double power = 20.0 + bonus;

            if (this.CheckSequence())
            {
                // 시각 효과 연출
                Point3D loc = (targeted is IPoint3D) ? new Point3D((IPoint3D)targeted) : Caster.Location;
                Effects.SendLocationParticles(EffectItem.Create(loc, Caster.Map, EffectItem.DefaultDuration), 0x376A, 9, 32, 5015);
                Effects.PlaySound(loc, Caster.Map, 0x1F0);

                // [설계 변경] 스킬 핸들러의 공용 로직 호출
                RemoveTrap.OnRemove(Caster, targeted, true, power);
            }

            this.FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly RemoveTrapSpell m_Owner;
            public InternalTarget(RemoveTrapSpell owner) : base(12, false, TargetFlags.None) { m_Owner = owner; }
            protected override void OnTarget(Mobile from, object o) { m_Owner.Target(o); }
            protected override void OnTargetFinish(Mobile from) { this.m_Owner.FinishSequence(); }
        }
    }
}
