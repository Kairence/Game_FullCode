using System;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Fifth
{
    public class BladeSpiritsSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Blade Spirits", "In Jux Hur Ylem",
            266,
            9040,
            false,
            Reagent.BlackPearl,
            Reagent.MandrakeRoot,
            Reagent.Nightshade);

        public BladeSpiritsSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Fifth;

        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            // 추종자 슬롯 체크 (1 슬롯 사용)
            if ((Caster.Followers + 1) > Caster.FollowersMax)
            {
                Caster.SendLocalizedMessage(1049612); // You have too many followers to summon that creature.
                return false;
            }

            return true;
        }

        public override void OnCast()
        {
            this.Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            Map map = this.Caster.Map;

            SpellHelper.GetSurfaceTop(ref p);

            if (map == null || !map.CanSpawnMobile(p.X, p.Y, p.Z))
            {
                this.Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if (SpellHelper.CheckTown(p, this.Caster) && this.CheckSequence())
            {
                // --- 1. 지속 시간 계산 (20초 + 보너스 * 0.008) ---
                double bonus = SpellHelper.GetMagicValue(Caster, 0.004);
                TimeSpan duration = TimeSpan.FromSeconds(20.0 + bonus);

                // --- 2. 소환 로직 ---
                BladeSpirits summoned = new BladeSpirits();

                // 시전자를 따라다니도록 설정 (Summon 메서드 내부에서 제어)
                // 기본 BladeSpirits AI는 모두를 공격하지만, Summon 시 파라미터로 제어 가능
                BaseCreature.Summon(summoned, true, this.Caster, new Point3D(p), 0x212, duration);
                
                // 추종자 슬롯 강제 설정 (엔진에 따라 Summon에서 자동 처리되기도 함)
                summoned.ControlSlots = 1;
            }

            this.FinishSequence();
        }

        public class InternalTarget : Target
        {
            private readonly BladeSpiritsSpell m_Owner;
            public InternalTarget(BladeSpiritsSpell owner)
                : base(12, true, TargetFlags.None)
            {
                this.m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is IPoint3D)
                    this.m_Owner.Target((IPoint3D)o);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                this.m_Owner.FinishSequence();
            }
        }
    }
}