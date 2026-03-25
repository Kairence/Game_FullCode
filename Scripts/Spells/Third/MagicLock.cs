using System;
using Server.Items;
using Server.Network;
using Server.Targeting;

namespace Server.Spells.Third
{
    public class MagicLockSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Magic Lock", "An Por",
            215,
            9001,
            Reagent.Garlic,
            Reagent.Bloodmoss,
            Reagent.SulfurousAsh);

        public MagicLockSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Third;

        public override void OnCast()
        {
            this.Caster.Target = new InternalTarget(this);
        }

        public void Target(LockableContainer targ)
        {
            if (Multis.BaseHouse.CheckLockedDownOrSecured(targ))
            {
                this.Caster.LocalOverheadMessage(MessageType.Regular, 0x22, 501761);
            }
            else if (targ.Locked || targ.LockLevel == 0 || targ is ParagonChest)
            {
                this.Caster.SendLocalizedMessage(501762);
            }
            else if (this.CheckSequence())
            {
                SpellHelper.Turn(this.Caster, targ);

                // [기획 반영] 영향력 = 30 + 보너스 * 0.006
                // 예: 보너스 2000일 때 30 + 12 = 42의 잠금 강도
                double bonus = SpellHelper.GetMagicValue(this.Caster, 0.006);
                int lockPower = 30 + (int)bonus;

                Point3D loc = targ.GetWorldLocation();
                Effects.SendLocationParticles(EffectItem.Create(loc, targ.Map, EffectItem.DefaultDuration), 0x376A, 9, 32, 5020);
                Effects.PlaySound(loc, targ.Map, 0x1FA);

                // 상자 잠금 설정
                targ.Locked = true;
                
                // 엔진에 따라 LockLevel 혹은 RequiredSkill을 위력만큼 설정하여 
                // 해당 수치보다 낮은 락픽 스킬로는 열지 못하게 합니다.
                targ.LockLevel = lockPower; 
                targ.RequiredSkill = lockPower;

                this.Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 501763); // The chest is now locked!
            }

            this.FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly MagicLockSpell m_Owner;
            public InternalTarget(MagicLockSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.None)
            {
                this.m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is LockableContainer)
                    this.m_Owner.Target((LockableContainer)o);
                else
                    from.SendLocalizedMessage(501762); // Target must be an unlocked chest.
            }

            protected override void OnTargetFinish(Mobile from)
            {
                this.m_Owner.FinishSequence();
            }
        }
    }
}
