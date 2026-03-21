using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Server.Mobiles;

namespace Server.Items
{
    public abstract class BaseTrap : Item, IRevealableItem
    {
        protected double m_Difficulty;
        protected bool m_Detected;
        protected int m_Range; 
        protected List<Item> m_Triggers = new List<Item>();
        private DateTime m_NextTriggerTime;
        private Timer m_OccupancyTimer;
        private bool m_IsRevealedBySkill; // [추가] 스킬로 발견되었는지 여부 플래그

        #region Properties
        [CommandProperty(AccessLevel.GameMaster)]
        public double Difficulty { get => m_Difficulty; set => m_Difficulty = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Range { get => m_Range; set { m_Range = value; if (!Deleted) ResetTriggers(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Detected
        {
            get => m_Detected;
            set { if (m_Detected != value) { m_Detected = value; OnDetectedChanged(); } }
        }

        public virtual TimeSpan PassiveTriggerDelay => TimeSpan.Zero;
        public virtual int PassiveTriggerRange => -1;
        public virtual bool PassivelyTriggered => (PassiveTriggerRange >= 0);
        public virtual TimeSpan ResetDelay => TimeSpan.FromSeconds(5.0);

        public virtual int DetectedItemID => 0x35B6; 
        public virtual int HiddenItemID => 0x35B6;
        #endregion

        public BaseTrap(int itemID) : base(itemID)
        {
            Movable = false;
            Visible = false;
            Name = "발견된 함정";
            m_Range = 1; 

            Timer.DelayCall(TimeSpan.Zero, CreateTriggers);
        }

        public BaseTrap(Serial serial) : base(serial) { }

        #region Helper Methods
        public virtual int GetEffectHue()
        {
            int hue = this.Hue & 0x3FFF;
            return (hue < 2) ? 0 : hue - 1;
        }

        public bool CheckRange(Point3D loc, Point3D oldLoc)
        {
            return CheckRange(loc, oldLoc, 0);
        }

        public bool CheckRange(Point3D loc, Point3D oldLoc, int range)
        {
            return ((Z + 8) >= loc.Z && (loc.Z + 16) > Z) && 
                   Utility.InRange(GetWorldLocation(), loc, range) && 
                   !Utility.InRange(GetWorldLocation(), oldLoc, range);
        }

        public bool CheckRange(Point3D loc, int range)
        {
            return ((Z + 8) >= loc.Z && (loc.Z + 16) > Z) &&
                   Utility.InRange(GetWorldLocation(), loc, range);
        }
        #endregion

        #region Trigger System
        private void CreateTriggers()
        {
            if (Deleted || m_Range <= 0) return;
            for (int x = -m_Range; x <= m_Range; x++)
            {
                for (int y = -m_Range; y <= m_Range; y++)
                {
                    if (x == 0 && y == 0) continue;
                    m_Triggers.Add(new TrapTrigger(this, x, y));
                }
            }
            RefreshComponents();
        }

        private void ResetTriggers()
        {
            foreach (var t in m_Triggers.Where(t => !t.Deleted)) t.Delete();
            m_Triggers.Clear();
            CreateTriggers();
        }

        protected virtual void OnDetectedChanged()
        {
            Visible = m_Detected;
            ItemID = m_Detected ? DetectedItemID : HiddenItemID;
            RefreshComponents();
            InvalidateProperties();

            if (m_Detected)
            {
                Effects.PlaySound(Location, Map, 0x1EF);
                
                // [수정] 스킬로 발견된 것이 아닐 때만 자동 숨김 타이머 작동
                if (!m_IsRevealedBySkill)
                {
                    Timer.DelayCall(ResetDelay, () => { if (!Deleted) Detected = false; });
                }
            }
            else
            {
                // 다시 숨겨질 때 플래그 리셋
                m_IsRevealedBySkill = false;
            }
        }

        public void RefreshComponents()
        {
            foreach (var t in m_Triggers.OfType<TrapTrigger>().Where(t => !t.Deleted))
            {
                t.Visible = m_Detected;
                t.ItemID = m_Detected ? DetectedItemID : HiddenItemID;
                t.Name = this.Name;
                t.InvalidateProperties();
            }
        }

        public override bool HandlesOnMovement => true;

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            base.OnMovement(m, oldLocation);
            if (m.Location == oldLocation || m.AccessLevel > AccessLevel.Player || !m.Alive) return;
            if (CheckRange(m.Location, oldLocation, 0)) CheckAndTrigger(m);
        }

        public virtual void CheckAndTrigger(Mobile m)
        {
            if (m == null || !m.Alive || m.AccessLevel > AccessLevel.Player) return;
            if (DateTime.Now < m_NextTriggerTime) return;

            m_NextTriggerTime = DateTime.Now + TimeSpan.FromSeconds(2.0);
            
            // 밟아서 작동할 때는 스킬 발견 플래그를 꺼야 타이머가 작동함
            m_IsRevealedBySkill = false; 

            if (!Detected) Detected = true;
            OnTrigger(m);

            if (m_OccupancyTimer == null)
                m_OccupancyTimer = Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0), OnOccupancyTick);
        }

        private void OnOccupancyTick()
        {
            if (Deleted) { StopOccupancyTimer(); return; }
            Mobile found = null;
            IPooledEnumerable e = Map.GetMobilesInRange(Location, 0);
            foreach (Mobile m in e) { if (m.Alive && m.AccessLevel <= AccessLevel.Player) { found = m; break; } }
            e.Free();

            if (found == null)
            {
                foreach (var t in m_Triggers.Where(t => !t.Deleted))
                {
                    IPooledEnumerable te = Map.GetMobilesInRange(t.Location, 0);
                    foreach (Mobile m in te) { if (m.Alive && m.AccessLevel <= AccessLevel.Player) { found = m; break; } }
                    te.Free();
                    if (found != null) break;
                }
            }

            if (found != null) { if (DateTime.Now >= m_NextTriggerTime) CheckAndTrigger(found); }
            else { StopOccupancyTimer(); }
        }

        private void StopOccupancyTimer() { if (m_OccupancyTimer != null) { m_OccupancyTimer.Stop(); m_OccupancyTimer = null; } }

        public virtual void OnTrigger(Mobile from) { }
        #endregion

        #region IRevealableItem
        public virtual bool CheckWhenHidden => true;
        
        public virtual bool CheckReveal(Mobile m) 
        { 
            if (m_Detected) return false; 
            return true; // 테스트용 확률
        }

        public virtual void OnRevealed(Mobile m)
        {
            // [핵심] 스킬로 발견되었음을 플래그에 저장
            m_IsRevealedBySkill = true; 

            this.Detected = true;
            m.SendLocalizedMessage(500815);
            Effects.SendLocationParticles(this, 0x376A, 9, 32, 5015);
            Effects.PlaySound(Location, Map, 0x1F0);
        }

        public virtual bool CheckPassiveDetect(Mobile m) 
        { 
            if (m_Detected) return false; 
            return true; // 테스트용 확률
        }
        #endregion

        #region Serialize & Deserialize
        public override void OnAfterDelete()
        {
            base.OnAfterDelete();
            StopOccupancyTimer();
            foreach (var t in m_Triggers.Where(t => !t.Deleted)) t.Delete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(4); // version 업그레이드

            writer.Write(m_IsRevealedBySkill); // [추가] 상태 저장
            writer.Write(m_Range);
            writer.Write(m_Difficulty);
            writer.Write(m_Detected);
            writer.WriteItemList(m_Triggers, true);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            try {
                int version = reader.ReadInt();
                if (version >= 4)
                {
                    m_IsRevealedBySkill = reader.ReadBool();
                    m_Range = reader.ReadInt();
                    m_Difficulty = reader.ReadDouble();
                    m_Detected = reader.ReadBool();
                    m_Triggers = reader.ReadItemList().Cast<Item>().ToList();
                }
                else if (version == 3) {
                    m_Range = reader.ReadInt();
                    m_Difficulty = reader.ReadDouble();
                    m_Detected = reader.ReadBool();
                    m_Triggers = reader.ReadItemList().Cast<Item>().ToList();
                }
            } catch { }

            Timer.DelayCall(TimeSpan.Zero, () => {
                ItemID = m_Detected ? DetectedItemID : HiddenItemID;
                Visible = m_Detected;
                RefreshComponents();
            });
        }
        #endregion
    }

    public class TrapTrigger : Item, IRevealableItem
    {
        private BaseTrap m_Parent;
        private int m_XOff, m_YOff;
		// TrapTrigger 클래스 내부에 추가
		public BaseTrap ParentTrap => m_Parent;

        public TrapTrigger(BaseTrap parent, int x, int y) : base(parent.HiddenItemID)
        {
            m_Parent = parent; m_XOff = x; m_YOff = y;
            Movable = false; Visible = false;
            this.Name = "발견된 함정";
            Timer.DelayCall(TimeSpan.Zero, () => {
                if (m_Parent != null && !m_Parent.Deleted) {
                    Location = new Point3D(m_Parent.X + m_XOff, m_Parent.Y + m_YOff, m_Parent.Z);
                    Map = m_Parent.Map;
                }
            });
        }

        public override bool OnMoveOver(Mobile m) { if (m_Parent != null && !m_Parent.Deleted) m_Parent.CheckAndTrigger(m); return true; }
        public TrapTrigger(Serial serial) : base(serial) { }
        public bool CheckWhenHidden => true;
        public bool CheckReveal(Mobile m) => m_Parent != null && m_Parent.CheckReveal(m);
        public void OnRevealed(Mobile m) { if (m_Parent != null) m_Parent.OnRevealed(m); }
        public bool CheckPassiveDetect(Mobile m) => m_Parent != null && m_Parent.CheckPassiveDetect(m);
        public override void OnDelete() { base.OnDelete(); if (m_Parent != null && !m_Parent.Deleted) m_Parent.Delete(); }
        public override void OnDoubleClick(Mobile from) { if (m_Parent != null && m_Parent.Detected) m_Parent.OnDoubleClick(from); }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); writer.Write(m_Parent); writer.Write(m_XOff); writer.Write(m_YOff); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); m_Parent = reader.ReadItem() as BaseTrap; m_XOff = reader.ReadInt(); m_YOff = reader.ReadInt(); }
    }
}