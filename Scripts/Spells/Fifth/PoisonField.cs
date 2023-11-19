using System;
using System.Collections;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Fifth
{
    public class PoisonFieldSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Poison Field", "In Nox Grav",
            230,
            9052,
            false,
            Reagent.BlackPearl,
            Reagent.Nightshade,
            Reagent.SpidersSilk);
        public PoisonFieldSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Fifth;
            }
        }
        public override void OnCast()
        {
			int range = 10;
			if ( Caster is PoisonElemental )
				range = 20;

            this.Caster.Target = new InternalTarget(this, range);
        }

        public void Target(IPoint3D p)
        {
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (SpellHelper.CheckTown(p, Caster) && SpellHelper.CheckWater(new Point3D(p), Caster.Map) && CheckSequence())
            {
                SpellHelper.Turn(Caster, p);

                SpellHelper.GetSurfaceTop(ref p);

                int dx = Caster.Location.X - p.X;
                int dy = Caster.Location.Y - p.Y;
                int rx = (dx - dy) * 44;
                int ry = (dx + dy) * 44;

                bool eastToWest;

                if (rx >= 0 && ry >= 0)
                {
                    eastToWest = false;
                }
                else if (rx >= 0)
                {
                    eastToWest = true;
                }
                else if (ry >= 0)
                {
                    eastToWest = true;
                }
                else
                {
                    eastToWest = false;
                }

                Effects.PlaySound(p, Caster.Map, 0x20B);
                int itemID = eastToWest ? 0x3915 : 0x3922;

                Point3D pnt = new Point3D(p);
                TimeSpan duration = TimeSpan.FromSeconds(30);
				int range = 1 + (int)(Caster.Skills.Magery.Value / 20 );
				if( Caster is PoisonElemental )
				{
					duration = TimeSpan.FromSeconds(15.0);
					range = 10;
				}
                if (SpellHelper.CheckField(pnt, Caster.Map))
                    new InternalItem(itemID, pnt, Caster, Caster.Map, duration);

				
                for (int i = 1; i <= range; ++i)
                {
                    Timer.DelayCall<int>(TimeSpan.FromMilliseconds(i * 200), index =>
                    {
                        Point3D point = new Point3D(eastToWest ? pnt.X + index : pnt.X, eastToWest ? pnt.Y : pnt.Y + index, pnt.Z);
                        SpellHelper.AdjustField(ref point, Caster.Map, 20, false);

                        if (SpellHelper.CheckField(point, Caster.Map))
                            new InternalItem(itemID, point, Caster, Caster.Map, duration);

                        point = new Point3D(eastToWest ? pnt.X + -index : pnt.X, eastToWest ? pnt.Y : pnt.Y + -index, pnt.Z);
                        SpellHelper.AdjustField(ref point, Caster.Map, 20, false);

                        if (SpellHelper.CheckField(point, Caster.Map))
                            new InternalItem(itemID, point, Caster, Caster.Map, duration);
                    }, i);
                }
            }

            FinishSequence();
        }

        [DispellableField]
        public class InternalItem : Item
        {
            private Timer m_Timer;
            private DateTime m_End;
            private Mobile m_Caster;

            public Mobile Caster { get { return m_Caster; } }

            public InternalItem(int itemID, Point3D loc, Mobile caster, Map map, TimeSpan duration)
                : base(itemID)
            {
                bool canFit = SpellHelper.AdjustField(ref loc, map, 20, false);

                Movable = false;
                Light = LightType.Circle300;

                MoveToWorld(loc, map);
                Effects.SendLocationParticles(EffectItem.Create(loc, map, EffectItem.DefaultDuration), 0x376A, 9, 10, 5029);

                m_Caster = caster;

                m_End = DateTime.UtcNow + duration;

                m_Timer = new InternalTimer(this, caster.InLOS(this), canFit);
                m_Timer.Start();
            }

            public InternalItem(Serial serial)
                : base(serial)
            {
            }

            public override bool BlocksFit
            {
                get
                {
                    return true;
                }
            }
            public override void OnAfterDelete()
            {
                base.OnAfterDelete();

                if (m_Timer != null)
                    m_Timer.Stop();
            }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);

                writer.Write((int)1); // version

                writer.Write(m_Caster);
                writer.WriteDeltaTime(m_End);
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);

                int version = reader.ReadInt();

                switch ( version )
                {
                    case 1:
                        {
                            m_Caster = reader.ReadMobile();

                            goto case 0;
                        }
                    case 0:
                        {
                            m_End = reader.ReadDeltaTime();

                            m_Timer = new InternalTimer(this, true, true);
                            m_Timer.Start();

                            break;
                        }
                }
            }

            public void ApplyPoisonTo(Mobile m)
            {
                if (m_Caster == null)
                    return;

                Poison p;

				if( m is PoisonElemental )
					p = Poison.Lethal;
                else if (Core.AOS)
                {
                    int total = m_Caster.Skills.Magery.Fixed / 500;

					if (total >= 4)
						p = Poison.Lethal;
                    else if (total >= 3)
                        p = Poison.Deadly;
                    else if (total >= 2)
                        p = Poison.Greater;
                    else if (total >= 1)
                        p = Poison.Regular;
                    else
                        p = Poison.Lesser;
                }
                else
                {
                    p = Poison.Regular;
                }

                if (m.ApplyPoison(m_Caster, p) == ApplyPoisonResult.Poisoned)
                    if (SpellHelper.CanRevealCaster(m))
                        m_Caster.RevealingAction();

                if (m is BaseCreature)
                    ((BaseCreature)m).OnHarmfulSpell(m_Caster);
            }

            public override bool OnMoveOver(Mobile m)
            {
                if (Visible && m_Caster != null && (!Core.AOS || m != m_Caster) && SpellHelper.ValidIndirectTarget(m_Caster, m) && m_Caster.CanBeHarmful(m, false))
                {
                    m_Caster.DoHarmful(m);

                    ApplyPoisonTo(m);
                    m.PlaySound(0x474);
                }

                return true;
            }

            private class InternalTimer : Timer
            {
                private static readonly Queue m_Queue = new Queue();
                private readonly InternalItem m_Item;
                private readonly bool m_InLOS;
                private readonly bool m_CanFit;
                public InternalTimer(InternalItem item, bool inLOS, bool canFit)
                    : base(TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1.5))
                {
                    m_Item = item;
                    m_InLOS = inLOS;
                    m_CanFit = canFit;

                    Priority = TimerPriority.FiftyMS;
                }

                protected override void OnTick()
                {
                    if (m_Item.Deleted)
                        return;

                    if (DateTime.UtcNow > m_Item.m_End)
                    {
                        m_Item.Delete();
                        Stop();
                    }
                    else
                    {
                        Map map = m_Item.Map;
                        Mobile caster = m_Item.m_Caster;

                        if (map != null && caster != null)
                        {
                            bool eastToWest = (m_Item.ItemID == 0x3915);
                            IPooledEnumerable eable = map.GetMobilesInBounds(new Rectangle2D(m_Item.X - (eastToWest ? 0 : 1), m_Item.Y - (eastToWest ? 1 : 0), (eastToWest ? 1 : 2), (eastToWest ? 2 : 1)));

                            foreach (Mobile m in eable)
                            {
                                if ((m.Z + 16) > m_Item.Z && (m_Item.Z + 12) > m.Z && (!Core.AOS || m != caster) && SpellHelper.ValidIndirectTarget(caster, m) && caster.CanBeHarmful(m, false))
                                    m_Queue.Enqueue(m);
                            }

                            eable.Free();

                            while (m_Queue.Count > 0)
                            {
                                Mobile m = (Mobile)m_Queue.Dequeue();

                                caster.DoHarmful(m);

                                m_Item.ApplyPoisonTo(m);
                                m.PlaySound(0x474);
                            }
                        }
                    }
                }
            }
        }

        public class InternalTarget : Target
        {
            private readonly PoisonFieldSpell m_Owner;
            public InternalTarget(PoisonFieldSpell owner, int range)
                : base(range, true, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is IPoint3D)
                    m_Owner.Target((IPoint3D)o);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }
}