using System;
using Server.Network;

namespace Server.Network
{
    public sealed class MobileBodySizePacket : Packet
    {
        public MobileBodySizePacket(Mobile m, double bodySize)
            : base(0xBF)
        {
            EnsureCapacity(11);

            m_Stream.Write((short)0x00FE); // SubCommand: Mobile BodySize
            m_Stream.Write((int)m.Serial);
            m_Stream.Write((ushort)Math.Round(bodySize * 1000.0));
        }
    }
}
