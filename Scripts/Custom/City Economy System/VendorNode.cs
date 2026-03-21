using System;
using System.Collections.Generic;
using Server;
using Server.Mobiles;

namespace Server.Misc
{
    public class VendorNode : Item
    {
        private List<Mobile> m_Spawned = new List<Mobile>();
        public List<string> SpawnTypes { get; set; } = new List<string>();

        private InternalTimer m_Timer;

        [CommandProperty(AccessLevel.GameMaster)]
        public string ZoneId { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxCount { get; set; } = 1;

        [CommandProperty(AccessLevel.GameMaster)]
        public int HomeRange { get; set; } = 5;

        // [추가] 스폰 주기 (최소 시간)
        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan MinDelay { get; set; } = TimeSpan.FromMinutes(5.0);

        // [추가] 스폰 주기 (최대 시간)
        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMinutes(10.0);

        [CommandProperty(AccessLevel.GameMaster)]
        public string SpawnList
        {
            get { return string.Join(",", SpawnTypes); }
            set 
            { 
                SpawnTypes.Clear();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    string[] types = value.Split(',');
                    foreach(string t in types) SpawnTypes.Add(t.Trim());
                }
            }
        }

        [Constructable]
        public VendorNode() : base(0x1F14) 
        { 
            Name = "Vendor Node"; 
            Visible = false; 
            Movable = false; 
            StartTimer(); // 노드가 만들어질 때 타이머 시작
        }

        // [추가] 타이머 시작 로직
        public void StartTimer()
        {
            if (m_Timer != null) m_Timer.Stop();

            TimeSpan delay = TimeSpan.FromSeconds(Utility.RandomMinMax((int)MinDelay.TotalSeconds, (int)MaxDelay.TotalSeconds));
            m_Timer = new InternalTimer(this, delay);
            m_Timer.Start();
        }

        public void DoTimerTick()
        {
            Respawn();
            StartTimer(); // 한 번 스폰 후 다음 스폰을 위해 타이머 재시작
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel < AccessLevel.GameMaster) return;

            if (SpawnTypes.Count == 0 || string.IsNullOrEmpty(ZoneId))
            {
                from.SendMessage(33, "[VendorNode] 설정된 스폰 타입(SpawnList)이나 ZoneId가 없습니다! [props 로 설정하세요.");
                return;
            }

            from.SendMessage(89, $"[VendorNode] 강제 스폰을 시도합니다... (목표 수: {MaxCount})");
            Respawn();
            StartTimer(); // 강제 스폰 시 타이머 리셋
        }

        public void Respawn()
        {
            m_Spawned.RemoveAll(m => m == null || m.Deleted);

            int spawnedThisTime = 0;

            while (m_Spawned.Count < MaxCount && SpawnTypes.Count > 0)
            {
                string type = SpawnTypes[Utility.Random(SpawnTypes.Count)];
                
                // 여기서 VendorSpawner에게 상인 제작을 요청합니다.
                Mobile spawned = VendorSpawner.PerformSpawn(type, ZoneId, Location, Map, HomeRange);
                
                if (spawned != null) 
                {
                    m_Spawned.Add(spawned);
                    spawnedThisTime++;
                }
                else break;
            }
            if (spawnedThisTime > 0)
                Console.WriteLine($"[VendorNode] {ZoneId}에 {spawnedThisTime}명의 상인이 스폰되었습니다. (현재 총 {m_Spawned.Count}/{MaxCount}명)");
        }

        public override void OnDelete()
        {
            if (m_Timer != null) m_Timer.Stop();
            base.OnDelete();
        }

        public VendorNode(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)2); // 버전을 2로 격상

            writer.Write(ZoneId);
            writer.Write(MaxCount); // [추가] 이것도 저장해야 로직이 안 꼬입니다.
            writer.Write(HomeRange); // [추가]

            // 상인 리스트 저장
            writer.Write(SpawnTypes.Count);
            foreach (string s in SpawnTypes) writer.Write(s);

            // [핵심] 현재 소환된 상인 목록 저장 (상속받은 Mobile들을 추적)
            writer.WriteMobileList(m_Spawned, true);

            // 시간 관련
            writer.Write(MinDelay);
            writer.Write(MaxDelay);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            
            ZoneId = reader.ReadString();

            if (version >= 2)
            {
                MaxCount = reader.ReadInt();
                HomeRange = reader.ReadInt();
            }

            // [무결성] 리스트를 비우고 읽어야 데이터가 안 꼬입니다.
            SpawnTypes.Clear();
            int count = reader.ReadInt();
            for (int i = 0; i < count; i++) SpawnTypes.Add(reader.ReadString());

            if (version >= 2)
            {
                m_Spawned = reader.ReadStrongMobileList();
            }

            if (version >= 1)
            {
                MinDelay = reader.ReadTimeSpan();
                MaxDelay = reader.ReadTimeSpan();
            }

            // 로드 완료 후 타이머 가동
            StartTimer(); 
        }

        // [추가] 타이머 클래스
        private class InternalTimer : Timer
        {
            private VendorNode m_Node;

            public InternalTimer(VendorNode node, TimeSpan delay) : base(delay)
            {
                m_Node = node;
                Priority = TimerPriority.OneMinute;
            }

            protected override void OnTick()
            {
                if (m_Node != null && !m_Node.Deleted)
                    m_Node.DoTimerTick();
            }
        }
    }
}