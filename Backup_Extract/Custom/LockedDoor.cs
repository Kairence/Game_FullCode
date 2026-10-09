using System;
using Server;
using Server.Items;

namespace Server.Items
{
    public class LockedDoor : BaseDoor, ILockable
    {
        private int m_LockLevel;
        private int m_MaxLockLevel;
        private int m_RequiredSkill;

        [CommandProperty(AccessLevel.GameMaster)]
        public int LockLevel { get => m_LockLevel; set => m_LockLevel = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxLockLevel { get => m_MaxLockLevel; set => m_MaxLockLevel = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int RequiredSkill { get => m_RequiredSkill; set => m_RequiredSkill = value; }

        public LockedDoor(int closedID, int difficulty) 
            : base(closedID, closedID + 1, 0xF5, 0xF6, new Point3D(-1, 1, 0)) 
        {
            Locked = true;
            m_LockLevel = difficulty;
            m_RequiredSkill = difficulty;
            m_MaxLockLevel = difficulty + 20;
            Movable = false;
        }

        public LockedDoor(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_LockLevel);
            writer.Write(m_MaxLockLevel);
            writer.Write(m_RequiredSkill);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_LockLevel = reader.ReadInt();
            m_MaxLockLevel = reader.ReadInt();
            m_RequiredSkill = reader.ReadInt();
        }
    }
}