using System;
using Server.Items;
using Server.Regions;
using Server.Targeting;

namespace Server.Spells.Third
{
    public class TeleportSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Teleport", "Rel Por",
            215,
            9031,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot);

        public TeleportSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Third;

        // --- 1. 시전 속도 커스텀 (기본 3.0초 - 보너스 * 0.001초) ---
        public override TimeSpan GetCastDelay()
        {
            // 예: 보너스가 2000이면 3.0 - 2.0 = 1.0초 시전
            double bonus = SpellHelper.GetMagicValue(Caster, 0.001);
            double delay = 3.0 - bonus;

            if (delay < 0.5) delay = 0.5; // 최소 시전 시간 0.5초 제한 (안전장치)

            return TimeSpan.FromSeconds(delay);
        }

        public override bool CheckCast()
        {
            if (Factions.Sigil.ExistsOn(this.Caster))
            {
                this.Caster.SendLocalizedMessage(1061632);
                return false;
            }
            else if (Server.Misc.WeightOverloading.IsOverloaded(this.Caster))
            {
                this.Caster.SendLocalizedMessage(502359, "", 0x22);
                return false;
            }

            return SpellHelper.CheckTravel(this.Caster, TravelCheckType.TeleportFrom);
        }

        public override void OnCast()
        {
            this.Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            IPoint3D orig = p;
            Map map = this.Caster.Map;

            SpellHelper.GetSurfaceTop(ref p);

            Point3D from = this.Caster.Location;
            Point3D to = new Point3D(p);

            // --- 2. 11타일 이내 시야 체크 ---
            if (!this.Caster.CanSee(p) || !this.Caster.InRange(to, 11))
            {
                this.Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (Factions.Sigil.ExistsOn(this.Caster))
            {
                this.Caster.SendLocalizedMessage(1061632);
            }
            else if (Server.Misc.WeightOverloading.IsOverloaded(this.Caster))
            {
                this.Caster.SendLocalizedMessage(502359, "", 0x22);
            }
            else if (!SpellHelper.CheckTravel(this.Caster, TravelCheckType.TeleportFrom) || 
                     !SpellHelper.CheckTravel(this.Caster, map, to, TravelCheckType.TeleportTo))
            {
            }
            else if (map == null || !map.CanSpawnMobile(p.X, p.Y, p.Z))
            {
                this.Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if (SpellHelper.CheckMulti(to, map) || Region.Find(to, map).GetRegion(typeof(HouseRegion)) != null)
            {
                this.Caster.SendLocalizedMessage(502829); // Cannot teleport to that spot.
            }
            else if (this.CheckSequence())
            {
                SpellHelper.Turn(this.Caster, orig);

                Mobile m = this.Caster;

                m.Location = to;
                m.ProcessDelta();

                // 이동 이펙트
                Effects.SendLocationParticles(EffectItem.Create(from, m.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);
                Effects.SendLocationParticles(EffectItem.Create(to, m.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 5023);

                m.PlaySound(0x1FE);

                // 필드 마법 위로 텔레포트 시 데미지 처리
                IPooledEnumerable eable = m.GetItemsInRange(0);
                foreach (Item item in eable)
                {
                    if (item is Server.Spells.Fifth.PoisonFieldSpell.InternalItem || item is Server.Spells.Fourth.FireFieldSpell.FireFieldItem)
                        item.OnMoveOver(m);
                }
                eable.Free();
            }

            this.FinishSequence();
        }

        public class InternalTarget : Target
        {
            private readonly TeleportSpell m_Owner;
            public InternalTarget(TeleportSpell owner)
                : base(11, true, TargetFlags.None) // --- 타겟팅 사거리 11로 제한 ---
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
