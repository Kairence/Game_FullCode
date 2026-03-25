using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Fourth
{
    public class ArchCureSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Arch Cure", "Vas An Nox",
            215,
            9061,
            Reagent.Garlic,
            Reagent.Ginseng,
            Reagent.MandrakeRoot);

        public ArchCureSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Fourth;

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
            else if (this.CheckSequence())
            {
                SpellHelper.Turn(this.Caster, p);
                SpellHelper.GetSurfaceTop(ref p);

                List<Mobile> targets = new List<Mobile>();
                Map map = this.Caster.Map;

                if (map != null)
                {
                    bool feluccaRules = (map.Rules == MapRules.FeluccaRules);

                    // 기획: 3타일 내의 모든 대상 (중심점 포함)
                    IPooledEnumerable eable = map.GetMobilesInRange(new Point3D(p), 3);

                    foreach (Mobile m in eable)
                    {
                        if (this.AreaCanTarget(m, feluccaRules))
                            targets.Add(m);
                    }

                    eable.Free();
                }

                Effects.PlaySound(p, this.Caster.Map, 0x299);

                // --- 보너스 수치 계산 (보너스 * 0.02) ---
                double bonus = SpellHelper.GetMagicValue(this.Caster, 0.02);

                for (int i = 0; i < targets.Count; ++i)
                {
                    Mobile m = targets[i];

                    this.Caster.DoBeneficial(m);

                    // 1. 독 레벨 1단계 감소 로직
                    Poison poison = m.Poison;
                    if (poison != null)
                    {
                        int currentLevel = poison.RealLevel;

                        if (currentLevel <= 0)
                        {
                            m.CurePoison(this.Caster);
                        }
                        else
                        {
                            int nextLevel = Math.Max(0, currentLevel - 1);
                            m.ApplyPoison(this.Caster, Poison.GetPoison(nextLevel));
                        }
                    }

                    // 2. 체력 회복 로직 (80 ~ 120 + 보너스)
                    int healAmount = Utility.RandomMinMax(80, 120) + (int)bonus;
                    SpellHelper.Heal(healAmount, m, this.Caster);

                    m.FixedParticles(0x373A, 10, 15, 5012, EffectLayer.Waist);
                    m.PlaySound(0x1E0);
                }
            }

            this.FinishSequence();
        }

        private static bool IsInnocentTo(Mobile from, Mobile to)
        {
            return (Notoriety.Compute(from, (Mobile)to) == Notoriety.Innocent);
        }

        private static bool IsAllyTo(Mobile from, Mobile to)
        {
            return (Notoriety.Compute(from, (Mobile)to) == Notoriety.Ally);
        }

        private bool AreaCanTarget(Mobile target, bool feluccaRules)
        {
            if (!this.Caster.CanBeBeneficial(target, false))
                return false;

            if (Core.AOS && target != this.Caster)
            {
                if (this.IsAggressor(target) || this.IsAggressed(target))
                    return false;

                if ((!IsInnocentTo(this.Caster, target) || !IsInnocentTo(target, this.Caster)) && !IsAllyTo(this.Caster, target))
                    return false;

                if (feluccaRules && !(target is PlayerMobile))
                    return false;
            }

            return true;
        }

        private bool IsAggressor(Mobile m)
        {
            foreach (AggressorInfo info in this.Caster.Aggressors)
            {
                if (m == info.Attacker && !info.Expired)
                    return true;
            }

            return false;
        }

        private bool IsAggressed(Mobile m)
        {
            foreach (AggressorInfo info in this.Caster.Aggressed)
            {
                if (m == info.Defender && !info.Expired)
                    return true;
            }

            return false;
        }

        public class InternalTarget : Target
        {
            private readonly ArchCureSpell m_Owner;
            public InternalTarget(ArchCureSpell owner)
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
