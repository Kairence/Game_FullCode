using System;
using Server.Targeting;
using Server.Items;
using Server.Mobiles;
using System.Collections.Generic;
using System.Linq;

namespace Server.Spells.Second
{
    public class MagicTrapSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Magic Trap", "In Jux",
            212, 9001, 
            Reagent.Garlic, 
            Reagent.SpidersSilk, 
            Reagent.SulfurousAsh);

        public MagicTrapSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Second;

        // [성공 보장] 매저리 수치와 상관없이 시전 허용
        public override bool CheckCast() { return true; }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237);
                return;
            }

            // 피즐 방지 수동 체크
            if (Caster.Mana < GetMana()) return;
            if (!ConsumeReagents()) return;

            Caster.Mana -= GetMana();
            SpellHelper.Turn(Caster, p);

            ResistanceType[] types = { ResistanceType.Fire, ResistanceType.Cold, ResistanceType.Poison, ResistanceType.Energy };
            ResistanceType chosenType = types[Utility.Random(types.Length)];

            Map map = Caster.Map;
            int min = 20;
            int max = 40;

            int[,] pentagramIDs = new int[3, 3] 
            {
                { 0x0FE7, 0x0FE8, 0x0FEB }, 
                { 0x0FE6, 0x0FEA, 0x0FEE }, 
                { 0x0FE9, 0x0FEC, 0x0FED }  
            };

            for (int x = -1; x <= 1; ++x)
            {
                for (int y = -1; y <= 1; ++y)
                {
                    Point3D loc = new Point3D(p.X + x, p.Y + y, p.Z);
                    
                    // 지형 고도 보정 (몹 발바닥에 맞춤)
                    IPoint3D ip = new Point3D(loc);
                    SpellHelper.GetSurfaceTop(ref ip);
                    loc.Z = ip.Z;

                    // 생성 조건 완화 (몹이 서있어도 생성되도록)
                    int itemID = pentagramIDs[y + 1, x + 1];
                    InternalMagicTrap trap = new InternalMagicTrap(Caster, chosenType, itemID, min, max, this);
                    trap.MoveToWorld(loc, map);
                }
            }

            Effects.PlaySound(p, map, 0x1EF);
            FinishSequence();
        }

        private class InternalMagicTrap : Item
        {
            private Mobile m_Caster;
            private MagicTrapSpell m_Spell;
            private ResistanceType m_Type;
            private Timer m_Timer;
            private DateTime m_EndTime;
            private int m_MinDamage;
            private int m_MaxDamage;

            public InternalMagicTrap(Mobile caster, ResistanceType type, int itemID, int min, int max, MagicTrapSpell spell) : base(itemID)
            {
                m_Caster = caster;
                m_Spell = spell;
                m_Type = type;
                m_MinDamage = min;
                m_MaxDamage = max;
                
                Movable = false;
                Visible = true;
                Hue = GetHue(type);
                
                m_EndTime = DateTime.Now + TimeSpan.FromSeconds(10.0);
                
                // [보정 1] 소환 즉시 발 밑의 적 체크 (선타 보장)
                Timer.DelayCall(TimeSpan.Zero, CheckDamage);
                
                // [보정 2] 반복 체크 주기를 1초로 단축
                m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0), OnTick);
            }

            // [보정 3] 몹이 이 타일을 밟는 순간 즉시 발동 (반응 속도 해결)
            public override bool OnMoveOver(Mobile m)
            {
                CheckDamage();
                return base.OnMoveOver(m);
            }

            private void OnTick()
            {
                if (DateTime.Now > m_EndTime || m_Caster == null || Deleted)
                {
                    Delete();
                    return;
                }

                CheckDamage();
            }

            // 데미지 및 선타 판정 공용 로직
            public void CheckDamage()
            {
                if (Deleted || m_Caster == null || Map == null) return;

                IPooledEnumerable eable = Map.GetMobilesInRange(Location, 0);
                foreach (Mobile m in eable)
                {
                    if (m == m_Caster || !m.Alive || !m_Caster.CanBeHarmful(m, false))
                        continue;

                    // 정확한 타일 일치 확인
                    if (m.X != this.X || m.Y != this.Y)
                        continue;

                    // 필드 데미지 중복 방지 (2초당 1번 타격)
                    if (!SpellHelper.CheckFieldDamage(m, typeof(MagicTrapSpell), TimeSpan.FromSeconds(2.0)))
                        continue;

                    // [선타 판정] 시전자의 유해 행위 등록
                    m_Caster.DoHarmful(m);

                    int damage = m_Spell.GetNewAosDamage(2, m_MinDamage, m_MaxDamage, m);
                    
                    SpellHelper.Damage(m_Spell, m, damage, 0, 
                        m_Type == ResistanceType.Fire ? 100 : 0,
                        m_Type == ResistanceType.Cold ? 100 : 0,
                        m_Type == ResistanceType.Poison ? 100 : 0,
                        m_Type == ResistanceType.Energy ? 100 : 0);
                    
                    m.FixedParticles(0x376A, 9, 32, 5012, GetHue(m_Type), 0, EffectLayer.Waist);
                    m.PlaySound(0x1EF);
                }
                eable.Free();
            }

            private static int GetHue(ResistanceType type)
            {
                switch (type) {
                    case ResistanceType.Fire: return 1258;
                    case ResistanceType.Cold: return 1265;
                    case ResistanceType.Poison: return 1272;
                    case ResistanceType.Energy: return 1276;
                    default: return 0;
                }
            }

            public override void OnAfterDelete() { if (m_Timer != null) m_Timer.Stop(); base.OnAfterDelete(); }
            public InternalMagicTrap(Serial serial) : base(serial) { }
            public override void Serialize(GenericWriter writer) 
            { 
                base.Serialize(writer); 
                writer.Write((int)0);
                writer.Write(m_Caster);
                writer.Write((int)m_Type);
                writer.Write(m_MinDamage);
                writer.Write(m_MaxDamage);
                writer.WriteDeltaTime(m_EndTime);
            }

            public override void Deserialize(GenericReader reader) 
            { 
                base.Deserialize(reader); 
                reader.ReadInt();
                m_Caster = reader.ReadMobile();
                m_Type = (ResistanceType)reader.ReadInt();
                m_MinDamage = reader.ReadInt();
                m_MaxDamage = reader.ReadInt();
                m_EndTime = reader.ReadDeltaTime();
                
                m_Spell = new MagicTrapSpell(m_Caster, null);
                m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0), OnTick);
            }
        }

        private class InternalTarget : Target
        {
            private readonly MagicTrapSpell m_Owner;
            public InternalTarget(MagicTrapSpell owner) : base(12, true, TargetFlags.None) { m_Owner = owner; }
            protected override void OnTarget(Mobile from, object o) { if (o is IPoint3D) m_Owner.Target((IPoint3D)o); }
            protected override void OnTargetFinish(Mobile from) { m_Owner.FinishSequence(); }
        }
    }
}
