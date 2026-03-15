using System;
using Server.Targeting;
using Server.Mobiles;
using Server.SkillHandlers;

namespace Server.Spells.Sixth
{
    public class RevealSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Reveal", "Wis Quas",
            206, 9002,
            Reagent.Bloodmoss, Reagent.SulfurousAsh);

        public RevealSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Sixth;

        public override bool CheckCast()
        {
            // 기획: 은신찾기 스킬 쿨타임 체크
            if (!Caster.CanBeginAction(typeof(DetectHidden)))
                return false;

            return base.CheckCast();
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237);
            }
            else if (CheckSequence())
            {
                SpellHelper.Turn(Caster, p);
                Point3D loc = new Point3D(p);

                // --- 기획 범위 적용 (30 + 보너스 * 0.006) ---
                double bonus = SpellHelper.GetMagicValue(Caster, 0.006);
                int range = (int)(30.0 + bonus);

                // DetectHidden의 공용 메서드를 사용하여 광역 탐지
                if (DetectHidden.OnDetect(Caster, loc, range))
                {
                    // 마법 성공 연출
                    Effects.PlaySound(loc, Caster.Map, 0x1FD);
                }
            }
            FinishSequence();
        }

        public class InternalTarget : Target
        {
            private readonly RevealSpell m_Owner;
            public InternalTarget(RevealSpell owner) : base(12, true, TargetFlags.None) { m_Owner = owner; }
            protected override void OnTarget(Mobile from, object o) { if (o is IPoint3D) m_Owner.Target((IPoint3D)o); }
            protected override void OnTargetFinish(Mobile from) { m_Owner.FinishSequence(); }
        }
    }
}