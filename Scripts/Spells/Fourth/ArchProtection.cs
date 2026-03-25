using System;
using System.Collections.Generic;
using Server.Targeting;
using Server.Mobiles;
using Server.Spells.Second; // CS0103 해결: ProtectionSpell 클래스 참조를 위해 추가

namespace Server.Spells.Fourth
{
    public class ArchProtectionSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Arch Protection", "Vas Uus Sanct",
            215, 9011, Reagent.Garlic, Reagent.Ginseng, Reagent.MandrakeRoot, Reagent.SulfurousAsh);

        public ArchProtectionSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Fourth;

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

                double bonus = SpellHelper.GetMagicValue(Caster, 0.06);
                TimeSpan length = TimeSpan.FromSeconds(300.0 + bonus);

                Map map = this.Caster.Map;
                if (map != null)
                {
                    IPooledEnumerable eable = map.GetMobilesInRange(new Point3D(p), 3);
                    foreach (Mobile m in eable)
                    {
                        if (this.Caster.CanBeBeneficial(m, false))
                        {
                            // ProtectionSpell의 공용 메서드를 통해 효과 적용 (isArch: true)
                            // 이제 상단 using문 덕분에 정상적으로 인식됩니다.
                            ProtectionSpell.ApplyEffect(m, length, true);
                        }
                    }
                    eable.Free();
                }
            }
            this.FinishSequence();
        }

        // 기존에 수동으로 작성되었던 ApplyEffect 메서드는 
        // ProtectionSpell.ApplyEffect에서 모든 관리를 하므로 제거하거나 
        // 위와 같이 호출식으로만 남겨두는 것이 관리상 좋습니다.

        public class InternalTarget : Target
        {
            private readonly ArchProtectionSpell m_Owner;
            public InternalTarget(ArchProtectionSpell owner)
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
