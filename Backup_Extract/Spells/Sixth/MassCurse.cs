using System;
using System.Collections.Generic;
using System.Linq;
using Server.Targeting;
using Server.Spells.Fourth; // CurseSpell 참조를 위해 추가
using Server.Mobiles;

namespace Server.Spells.Sixth
{
    public class MassCurseSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Mass Curse", "Vas Des Sanct",
            218, 9031, false,
            Reagent.Garlic, Reagent.Nightshade, Reagent.MandrakeRoot, Reagent.SulfurousAsh);

        public MassCurseSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Sixth;

        public override void OnCast()
        {
            this.Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if (!this.Caster.CanSee(p))
            {
                this.Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (SpellHelper.CheckTown(p, this.Caster) && this.CheckSequence())
            {
                SpellHelper.Turn(this.Caster, p);
                SpellHelper.GetSurfaceTop(ref p);

                // --- 1. 3타일 범위 내 모든 적군 포착 (AcquireIndirectTargets) ---
                // AcquireIndirectTargets(지점, 범위)
                List<Mobile> targets = AcquireIndirectTargets(p, 3).OfType<Mobile>().ToList();

                foreach (Mobile m in targets)
                {
                    // 2. 이미 CurseSpell에 설계된 로직을 그대로 사용
                    MagerySpell.CastDirect<CurseSpell>(Caster, m);
                }
            }

            this.FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly MassCurseSpell m_Owner;
            public InternalTarget(MassCurseSpell owner)
                : base(12, true, TargetFlags.None)
            {
                this.m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                IPoint3D p = o as IPoint3D;
                if (p != null)
                    this.m_Owner.Target(p);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                this.m_Owner.FinishSequence();
            }
        }
    }
}
