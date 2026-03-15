using System;
using System.Collections.Generic;
using System.Linq;
using Server.Targeting;
using Server.Mobiles;
using Server.Items; // CS0103 해결: EffectItem 참조를 위해 추가

namespace Server.Spells.Sixth
{
    public class ParalyzeFieldSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Paralyze Field", "In Ex Grav",
            230, 9012, false,
            Reagent.BlackPearl, Reagent.Ginseng, Reagent.SpidersSilk);

        public ParalyzeFieldSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Sixth;

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
            else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
            {
                SpellHelper.Turn(Caster, p);

                if (p is Item)
                    p = ((Item)p).GetWorldLocation();

                Point3D loc = new Point3D(p);

                // 1. 타겟 지점 시각 효과
                Effects.PlaySound(loc, Caster.Map, 0x20B);
                
                // CS0103 해결: Server.Items 네임스페이스 추가 후 사용
                Effects.SendLocationParticles(EffectItem.Create(loc, Caster.Map, EffectItem.DefaultDuration), 0x376A, 9, 20, 5048);

                // 2. 4타일 반경 내의 모든 적군 포착
                List<Mobile> targets = AcquireIndirectTargets(loc, 4).OfType<Mobile>().ToList();

                foreach (Mobile m in targets)
                {
                    if (Caster.CanBeHarmful(m, false))
                    {
                        Caster.DoHarmful(m);

                        // --- [CS1503 해결] ---
                        // m.Paralyze(Caster) 대신 지속 시간을 직접 계산하여 전달합니다.
                        // 공식: (시전자 스킬 / 10) 초 (예: 100이면 10초)
                        double durationValue = Caster.Skills[SkillName.Magery].Value / 10.0;
                        TimeSpan duration = TimeSpan.FromSeconds(durationValue);

                        m.Paralyze(duration); 

                        // 개별 피격 효과
                        m.PlaySound(0x204);
                        m.FixedEffect(0x376A, 10, 16);

                        if (m is BaseCreature)
                            ((BaseCreature)m).OnHarmfulSpell(Caster);
                    }
                }

                ColUtility.Free(targets);
            }

            FinishSequence();
        }

        public class InternalTarget : Target
        {
            private readonly ParalyzeFieldSpell m_Owner;
            public InternalTarget(ParalyzeFieldSpell owner) : base(12, true, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is IPoint3D) m_Owner.Target((IPoint3D)o);
            }

            protected override void OnTargetFinish(Mobile from) { m_Owner.FinishSequence(); }
        }
    }
}