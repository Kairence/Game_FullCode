using System;
using System.Collections.Generic;
using System.Linq;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using Server.Spells.Sixth;

namespace Server.Spells.Seventh
{
    public class MassDispelSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Mass Dispel", "Vas An Ort",
            263, 9002,
            Reagent.Garlic, Reagent.MandrakeRoot, Reagent.BlackPearl, Reagent.SulfurousAsh);

        public MassDispelSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Seventh;

        public override void OnCast()
        {
            this.Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if (!this.Caster.CanSee(p))
            {
                this.Caster.SendLocalizedMessage(500237);
            }
            else if (this.CheckSequence())
            {
                SpellHelper.Turn(this.Caster, p);
                SpellHelper.GetSurfaceTop(ref p);

                Point3D loc = (p is Item) ? ((Item)p).GetWorldLocation() : new Point3D(p);

                // --- 기획: 4타일 범위 내 모든 적군 타겟팅 ---
                List<Mobile> targets = AcquireIndirectTargets(loc, 4).OfType<Mobile>().ToList();

                foreach (Mobile m in targets)
                {
                    // 6서클 디스펠에 이미 구현된 [정령/소환수 판별 및 화염 타격] 로직 호출
                    // DispelSpell 클래스에 Target(Mobile) 메서드가 public으로 선언되어 있어야 합니다.
                    new DispelSpell(Caster, null).Target(m);
                }

                ColUtility.Free(targets);
            }

            this.FinishSequence();
        }

        public class InternalTarget : Target
        {
            private readonly MassDispelSpell m_Owner;
            public InternalTarget(MassDispelSpell owner) : base(12, true, TargetFlags.None) { m_Owner = owner; }

            protected override void OnTarget(Mobile from, object o)
            {
                IPoint3D p = o as IPoint3D;
                if (p != null) m_Owner.Target(p);
            }

            protected override void OnTargetFinish(Mobile from) { m_Owner.FinishSequence(); }
        }
    }
}