using System;
using System.Collections;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Fourth
{
    public class FireFieldSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Fire Field", "In Flam Grav",
            215, 9041, false,
            Reagent.BlackPearl, Reagent.SpidersSilk, Reagent.SulfurousAsh);

        public FireFieldSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Fourth;

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

                Effects.PlaySound(p, Caster.Map, 0x20C);

                // 기획: 10초 지속, 기본 데미지 20~60 (여기에 보너스 합산됨)
                TimeSpan duration = TimeSpan.FromSeconds(10.0);
                int min = 20;
                int max = 60;

                for (int x = -1; x <= 1; ++x)
                {
                    for (int y = -1; y <= 1; ++y)
                    {
                        Point3D loc = new Point3D(p.X + x, p.Y + y, p.Z);
                        
                        if (SpellHelper.CheckField(loc, Caster.Map) && SpellHelper.CheckWater(loc, Caster.Map))
                        {
                            // 보너스 계산을 위해 min, max를 아이템에 전달
                            new FireFieldItem(0x398C, loc, Caster, Caster.Map, duration, min, max, this);
                        }
                    }
                }
            }
            FinishSequence();
        }

        [DispellableField]
        public class FireFieldItem : Item
        {
            private Mobile m_Caster;
            private FireFieldSpell m_Spell;
            private Timer m_Timer;
            private DateTime m_End;
            private int m_MinDamage;
            private int m_MaxDamage;
			public Mobile Caster => m_Caster;
            public FireFieldItem(int itemID, Point3D loc, Mobile caster, Map map, TimeSpan duration, int min, int max, FireFieldSpell spell) : base(itemID)
            {
                Movable = false;
                Light = LightType.Circle300;
                MoveToWorld(loc, map);
                
                m_Caster = caster;
                m_Spell = spell;
                m_MinDamage = min;
                m_MaxDamage = max;
                m_End = DateTime.UtcNow + duration;

                // 2초마다 공격 시도
                m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(2.0), OnTick);
            }

            private void OnTick()
            {
                if (Deleted) return;

                if (DateTime.UtcNow > m_End || m_Caster == null)
                {
                    Delete();
                    return;
                }

                IPooledEnumerable eable = Map.GetMobilesInRange(Location, 0);
                foreach (Mobile m in eable)
                {
                    if (m == m_Caster || !m_Caster.CanBeHarmful(m, false))
                        continue;

                    // 중복 데미지 방지 틱 체크 (2.5초)
                    if (!SpellHelper.CheckFieldDamage(m, typeof(FireFieldSpell), TimeSpan.FromSeconds(2.5)))
                        continue;

                    m_Caster.DoHarmful(m);

                    // [핵심] 시전자의 스펠 인스턴스를 통해 보너스가 포함된 최종 데미지 산출
                    int damage = m_Spell.GetNewAosDamage(4, m_MinDamage, m_MaxDamage, m);
                    
                    // 화염 속성 100% 데미지 적용
                    SpellHelper.Damage(m_Spell, m, damage, 0, 100, 0, 0, 0);

                    m.PlaySound(0x208);
                    m.FixedParticles(0x3709, 10, 30, 5052, EffectLayer.LeftFoot);
                }
                eable.Free();
            }

            public override bool OnMoveOver(Mobile m)
            {
                OnTick();
                return true;
            }

            public override bool BlocksFit => true;

            public override void OnAfterDelete()
            {
                base.OnAfterDelete();
                if (m_Timer != null) m_Timer.Stop();
            }

            // 시리얼라이즈 시 시전자와 데미지 설정값 저장
            public FireFieldItem(Serial serial) : base(serial) { }
            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);
                writer.Write((int)0); // version
                writer.Write(m_Caster);
                writer.Write(m_MinDamage);
                writer.Write(m_MaxDamage);
                writer.WriteDeltaTime(m_End);
            }
            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);
                reader.ReadInt();
                m_Caster = reader.ReadMobile();
                m_MinDamage = reader.ReadInt();
                m_MaxDamage = reader.ReadInt();
                m_End = reader.ReadDeltaTime();
                
                // 역직렬화 시 가상의 스펠 인스턴스 생성 (보너스 계산용)
                m_Spell = new FireFieldSpell(m_Caster, null);
                m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(2.0), OnTick);
            }
        }

        public class InternalTarget : Target
        {
            private readonly FireFieldSpell m_Owner;
            public InternalTarget(FireFieldSpell owner) : base(12, true, TargetFlags.None) { m_Owner = owner; }
            protected override void OnTarget(Mobile from, object o) { if (o is IPoint3D) m_Owner.Target((IPoint3D)o); }
            protected override void OnTargetFinish(Mobile from) { m_Owner.FinishSequence(); }
        }
    }
}
