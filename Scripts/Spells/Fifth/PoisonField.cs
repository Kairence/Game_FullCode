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
            230, 9052, false,
            Reagent.BlackPearl, Reagent.Nightshade, Reagent.SpidersSilk);

        public PoisonFieldSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Fourth; // 5서클이나 엔진에 따라 수치 조정 가능

        public override void OnCast()
        {
            int range = (Caster is PoisonElemental) ? 20 : 10;
            this.Caster.Target = new InternalTarget(this, range);
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

                Effects.PlaySound(p, Caster.Map, 0x20B);

                // 기획: 20초 유지
                TimeSpan duration = TimeSpan.FromSeconds(20.0);
                if (Caster is PoisonElemental) duration = TimeSpan.FromSeconds(15.0);

                // --- 3x3 범위 독성 지대 생성 ---
                for (int x = -1; x <= 1; ++x)
                {
                    for (int y = -1; y <= 1; ++y)
                    {
                        Point3D loc = new Point3D(p.X + x, p.Y + y, p.Z);

                        if (SpellHelper.CheckField(loc, Caster.Map) && SpellHelper.CheckWater(loc, Caster.Map))
                        {
                            new InternalItem(0x3915, loc, Caster, Caster.Map, duration, this);
                        }
                    }
                }
            }
            FinishSequence();
        }

        [DispellableField]
        public class InternalItem : Item
        {
            private Mobile m_Caster;
            private PoisonFieldSpell m_Spell;
            private Timer m_Timer;
            private DateTime m_End;
			public Mobile Caster => m_Caster;
			
            public InternalItem(int itemID, Point3D loc, Mobile caster, Map map, TimeSpan duration, PoisonFieldSpell spell) : base(itemID)
            {
                Movable = false;
                Light = LightType.Circle300;
                MoveToWorld(loc, map);

                m_Caster = caster;
                m_Spell = spell;
                
                // Now 사용
                m_End = DateTime.Now + duration;

                // 2초마다 재공격 시도
                m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(2.0), OnTick);
            }

            private void OnTick()
            {
                if (Deleted || m_Caster == null) return;

                // Now 사용
                if (DateTime.Now > m_End)
                {
                    Delete();
                    return;
                }

                IPooledEnumerable eable = Map.GetMobilesInRange(Location, 0);
                foreach (Mobile m in eable)
                {
                    if (m == m_Caster || !m_Caster.CanBeHarmful(m, false))
                        continue;

                    // --- [필드 중첩 데미지 방지] ---
                    // 2초 틱이 지나지 않은 대상은 필터링
                    if (!SpellHelper.CheckFieldDamage(m, typeof(PoisonFieldSpell), TimeSpan.FromSeconds(2.0)))
                        continue;

                    m_Caster.DoHarmful(m);

                    // 1. DPS 데미지 처리 (25~65 + 보너스)
                    int damage = m_Spell.GetNewAosDamage(0, 25, 65, m);
					SpellHelper.Damage(m_Spell, m, damage, 0, 0, 0, 100, 0);

                    // --- 2. 중독 확률 판정 (20% + 보너스 * 0.004%) ---
                    double bonus = SpellHelper.GetMagicValue(m_Caster, 0.004);
                    double applyChance = 0.20 + (bonus * 0.01);

                    if (Utility.RandomDouble() < applyChance)
                    {
                        // --- 3. 독 레벨 결정 (중독술 보너스 계산) ---
                        double poisoningSkill = m_Caster.Skills[SkillName.Poisoning].Value;
                        
                        int baseLevel = (int)(poisoningSkill / 30.0); 
                        double remainder = poisoningSkill % 30.0;
                        
                        // 상승 확률: 50% + (남은 스킬 * 1.5%)
                        double upgradeChance = (50.0 + (remainder * 1.5)) * 0.01;
                        
                        int finalLevel = baseLevel;
                        if (Utility.RandomDouble() < upgradeChance)
                        {
                            finalLevel++;
                        }

                        if (finalLevel >= 0)
                        {
                            m.ApplyPoison(m_Caster, Poison.GetPoison(finalLevel));
                        }
                    }

                    m.PlaySound(0x474);
                }
                eable.Free();
            }

            public override bool OnMoveOver(Mobile m) { OnTick(); return true; }
            public override bool BlocksFit => true;
            public override void OnAfterDelete() { base.OnAfterDelete(); if (m_Timer != null) m_Timer.Stop(); }
            
            public InternalItem(Serial serial) : base(serial) { }
            public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(m_Caster); writer.WriteDeltaTime(m_End); }
            public override void Deserialize(GenericReader reader) 
            { 
                base.Deserialize(reader); 
                reader.ReadInt(); 
                m_Caster = reader.ReadMobile(); 
                m_End = reader.ReadDeltaTime(); 
                m_Spell = new PoisonFieldSpell(m_Caster, null); 
                m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(2.0), OnTick); 
            }
        }

        public class InternalTarget : Target
        {
            private readonly PoisonFieldSpell m_Owner;
            public InternalTarget(PoisonFieldSpell owner, int range) : base(range, true, TargetFlags.None) { m_Owner = owner; }
            protected override void OnTarget(Mobile from, object o) { if (o is IPoint3D) m_Owner.Target((IPoint3D)o); }
            protected override void OnTargetFinish(Mobile from) { m_Owner.FinishSequence(); }
        }
    }
}