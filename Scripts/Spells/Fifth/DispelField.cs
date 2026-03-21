using System;
using System.Collections.Generic;
using Server.Items;
using Server.Misc;
using Server.Targeting;

namespace Server.Spells.Fifth
{
    public class DispelFieldSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Dispel Field", "An Grav",
            206,
            9002,
            Reagent.BlackPearl,
            Reagent.SpidersSilk,
            Reagent.SulfurousAsh,
            Reagent.Garlic);

        public DispelFieldSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Fifth;

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

                Map map = this.Caster.Map;

                if (map != null)
                {
                    // --- 1. 확률 계산 (20% + 보너스 * 0.008%) ---
                    double bonus = SpellHelper.GetMagicValue(Caster, 0.004);
                    double chance = 0.20 + (bonus * 0.01); // 0.01을 곱해 백분율 확률로 변환

                    Point3D center = new Point3D(p);

                    // --- 2. 5x5 범위 내 필드 아이템 제거 ---
                    IPooledEnumerable itemEable = map.GetItemsInRange(center, 2); // 중심 기준 거리 2 = 5x5
                    List<Item> toDelete = new List<Item>();

                    foreach (Item item in itemEable)
                    {
                        if (item is Moongate && !((Moongate)item).Dispellable)
                            continue;

                        // DispellableFieldAttribute가 정의된 아이템(필드 마법들) 체크
                        if (item.GetType().IsDefined(typeof(DispellableFieldAttribute), false))
                        {
                            if (Utility.RandomDouble() < chance)
                                toDelete.Add(item);
                        }
                    }
                    itemEable.Free();

                    for (int i = 0; i < toDelete.Count; ++i)
                    {
                        Item item = toDelete[i];
                        Effects.SendLocationParticles(EffectItem.Create(item.Location, item.Map, EffectItem.DefaultDuration), 0x376A, 9, 20, 5042);
                        item.Delete();
                    }

                    // --- 3. 5x5 범위 내 대상 마비 해제 ---
                    IPooledEnumerable mobileEable = map.GetMobilesInRange(center, 2);
                    foreach (Mobile m in mobileEable)
                    {
                        if (m.Paralyzed && Utility.RandomDouble() < chance)
                        {
                            m.Paralyzed = false;
                            m.FixedParticles(0x376A, 9, 32, 5030, EffectLayer.Waist);
                        }
                    }
                    mobileEable.Free();

                    Effects.PlaySound(center, map, 0x201);
                }
            }

            this.FinishSequence();
        }

        public class InternalTarget : Target
        {
            private readonly DispelFieldSpell m_Owner;

            public InternalTarget(DispelFieldSpell owner)
                : base(12, true, TargetFlags.None) // 광역 시전을 위해 지면 타겟팅 허용
            {
                this.m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is IPoint3D)
                {
                    this.m_Owner.Target((IPoint3D)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                this.m_Owner.FinishSequence();
            }
        }
    }
}