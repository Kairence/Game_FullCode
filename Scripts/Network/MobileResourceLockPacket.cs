using System;
using Server.Network;

namespace Server.Network
{
    public sealed class MobileResourceLockPacket : Packet
    {
        public MobileResourceLockPacket(Mobile m, int woundHits, int buffHitsLock, int woundMana, int buffManaLock, int woundStam, int buffStamLock)
            : base(0xBF)
        {
            EnsureCapacity(21);

            m_Stream.Write((short)0x00FD); // SubCommand: Mobile Resource Locks & Wounds
            m_Stream.Write((int)m.Serial);
            m_Stream.Write((ushort)Math.Clamp(woundHits, 0, ushort.MaxValue));
            m_Stream.Write((ushort)Math.Clamp(buffHitsLock, 0, ushort.MaxValue));
            m_Stream.Write((ushort)Math.Clamp(woundMana, 0, ushort.MaxValue));
            m_Stream.Write((ushort)Math.Clamp(buffManaLock, 0, ushort.MaxValue));
            m_Stream.Write((ushort)Math.Clamp(woundStam, 0, ushort.MaxValue));
            m_Stream.Write((ushort)Math.Clamp(buffStamLock, 0, ushort.MaxValue));
        }
    }
}
