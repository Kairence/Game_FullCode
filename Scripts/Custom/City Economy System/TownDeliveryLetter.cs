using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Items
{
    public class TownDeliveryLetter : Item
    {
        public string DestTownName { get; set; }
        public string DestHouseName { get; set; }
        public DateTime ExpireTime { get; set; }

        private InternalTimer m_Timer;

        [Constructable]
        public TownDeliveryLetter(string townName, string houseName, DateTime expireTime) : base(0x14F0)
        {
            Name = "중요 배달 서신";
            Weight = 1.0;
            Hue = 1153;
            LootType = LootType.Blessed; // 퀘스트 도중 도난/루팅 방지
            
            DestTownName = townName;
            DestHouseName = houseName;
            ExpireTime = expireTime;

            m_Timer = new InternalTimer(this);
            m_Timer.Start();
        }

        public TownDeliveryLetter(Serial serial) : base(serial) { }

        public override void OnDoubleClick(Mobile from)
        {
            string target = string.IsNullOrEmpty(DestHouseName) ? "마을 공용" : DestHouseName;
            from.SendMessage(0x35, $"목적지: {DestTownName} [{target}]");
            
            TimeSpan left = ExpireTime - DateTime.Now;
            if (left.TotalSeconds > 0)
                from.SendMessage(0x35, $"남은 시간: {(int)left.TotalMinutes}분 (기한: {ExpireTime:HH:mm})");
            else
                from.SendMessage(0x22, "이미 기한이 초과된 서신입니다.");
        }

        public override void OnDelete()
        {
            if (m_Timer != null)
            {
                m_Timer.Stop();
                m_Timer = null;
            }
            base.OnDelete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(DestTownName ?? "");
            writer.Write(DestHouseName ?? "");
            writer.Write(ExpireTime);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            DestTownName = reader.ReadString();
            DestHouseName = reader.ReadString();
            ExpireTime = reader.ReadDateTime();

            m_Timer = new InternalTimer(this);
            m_Timer.Start();
        }

        private class InternalTimer : Timer
        {
            private TownDeliveryLetter m_Letter;

            // 10초마다 만료 여부 검사
            public InternalTimer(TownDeliveryLetter letter) : base(TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(10.0))
            {
                m_Letter = letter;
                Priority = TimerPriority.OneSecond;
            }

            protected override void OnTick()
            {
                if (m_Letter == null || m_Letter.Deleted)
                {
                    Stop();
                    return;
                }

                if (DateTime.Now > m_Letter.ExpireTime)
                {
                    if (m_Letter.RootParent is Mobile m)
                    {
                        m.SendMessage(33, "배달 서신의 기한이 초과되어 먼지처럼 파기되었습니다.");
                    }
                    m_Letter.Delete();
                    Stop();
                }
            }
        }
    }
}