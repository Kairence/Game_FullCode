using System;
using Server.Misc;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Third
{
    public class WallOfStoneSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Wall of Stone", "In Sanct Ylem",
            227,
            9011,
            false,
            Reagent.Bloodmoss,
            Reagent.Garlic);

        public WallOfStoneSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Third;

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
                SpellHelper.GetSurfaceTop(ref p);

                // --- 1. 방향 결정 로직 ---
                int dx = Caster.Location.X - p.X;
                int dy = Caster.Location.Y - p.Y;
                int rx = (dx - dy) * 44;
                int ry = (dx + dy) * 44;

                bool eastToWest = (rx < 0 && ry < 0) || (rx >= 0 && ry >= 0) ? false : true;

                Effects.PlaySound(p, Caster.Map, 0x1F6);

                // --- 2. 지속 시간 계산 (10초 + 보너스 * 0.004) ---
                double bonus = SpellHelper.GetMagicValue(Caster, 0.002);
                TimeSpan duration = TimeSpan.FromSeconds(10.0 + bonus);

                // --- 3. 1x11 벽 생성 (중앙 기준 좌우 5칸씩) ---
                for (int i = -5; i <= 5; ++i)
                {
                    Point3D loc = new Point3D(eastToWest ? p.X + i : p.X, eastToWest ? p.Y : p.Y + i, p.Z);

                    // 물 위나 다른 필드가 있는 곳이 아니면 생성
                    if (SpellHelper.CheckWater(loc, Caster.Map) && SpellHelper.CheckField(loc, Caster.Map))
                    {
                        Item item = new InternalItem(loc, Caster.Map, Caster, duration);
                        Effects.SendLocationParticles(item, 0x376A, 9, 10, 5025);
                    }
                }
            }

            FinishSequence();
        }

        [DispellableField]
        private class InternalItem : Item
        {
            private Timer m_Timer;
            private DateTime m_End;

            public InternalItem(Point3D loc, Map map, Mobile caster, TimeSpan duration)
                : base(0x82) // 돌 벽 그래픽
            {
                Movable = false;
                MoveToWorld(loc, map);

                if (Deleted) return;

                m_End = DateTime.UtcNow + duration;
                m_Timer = Timer.DelayCall(duration, new TimerCallback(Delete));
            }

            public InternalItem(Serial serial) : base(serial) { }

            public override bool BlocksFit => true;

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);
                writer.Write((int)1);
                writer.WriteDeltaTime(m_End);
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);
                int version = reader.ReadInt();
                m_End = reader.ReadDeltaTime();
                m_Timer = Timer.DelayCall(m_End - DateTime.UtcNow, new TimerCallback(Delete));
            }

            public override void OnAfterDelete()
            {
                base.OnAfterDelete();
                if (m_Timer != null) m_Timer.Stop();
            }
        }

        public class InternalTarget : Target
        {
            private readonly WallOfStoneSpell m_Owner;
            public InternalTarget(WallOfStoneSpell owner)
                : base(12, true, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is IPoint3D) m_Owner.Target((IPoint3D)o);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }
}