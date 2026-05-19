using System;
using System.Collections.Generic;
using Server;
using Server.Mobiles;
using System.Linq;
namespace Server.Misc
{
    public class VendorNode : Item
    {
        private List<Mobile> m_Spawned = new List<Mobile>();
        public List<string> SpawnTypes { get; set; } = new List<string>();
        private InternalTimer m_Timer;
		public string VendorName;
        // [핵심] 기존의 string ZoneId를 완전히 삭제하고 int TownID로 교체
        [CommandProperty(AccessLevel.GameMaster)]
        public int TownID { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxCount { get; set; } = 1;

        [CommandProperty(AccessLevel.GameMaster)]
        public int HomeRange { get; set; } = 5;

		// VendorNode.cs 내부 상단
		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsActive { get; set; } = true;

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan MinDelay { get; set; } = TimeSpan.FromMinutes(5.0);

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
            StartTimer(); 
        }

        // [자동화] GM이 노드를 이동시키거나 맵을 바꿀 때 TownID 자동 갱신
        public override void OnLocationChange(Point3D oldLocation)
        {
            base.OnLocationChange(oldLocation);
            TownID = TownNumber.GetID(this.Location, this.Map);
        }

        public override void OnMapChange()
        {
            base.OnMapChange();
            TownID = TownNumber.GetID(this.Location, this.Map);
        }

        public void StartTimer()
        {
            if (m_Timer != null) m_Timer.Stop();

            TimeSpan delay = TimeSpan.FromSeconds(Utility.RandomMinMax((int)MinDelay.TotalSeconds, (int)MaxDelay.TotalSeconds));
            m_Timer = new InternalTimer(this, delay);
            m_Timer.Start();
        }
		// [추가] 마을 구역 이탈 체크 및 강제 복귀 로직
		public void CheckBoundaries()
		{
			if (m_Spawned == null || m_Spawned.Count == 0) return;

			// 현재 살아있는 상인들 중 마을 ID가 바뀐 녀석들을 찾음
			foreach (var m in m_Spawned.Where(v => v != null && !v.Deleted))
			{
				// 상인의 현재 좌표가 인식하는 마을 ID 추출
				int currentLocTownID = TownNumber.GetID(m.Location, m.Map);

				// 노드의 TownID와 현재 위치의 TownID가 다르면 마을을 벗어난 것임
				if (currentLocTownID != this.TownID)
				{
					// 노드 위치로 즉시 복귀
					m.MoveToWorld(this.Location, this.Map);
					
					if (m is BaseVendor bv)
					{
						bv.Say("구역을 벗어나서 상점으로 복귀합니다."); // 안내 문구 (선택 사항)
					}
				}
			
			}
		}
        public void DoTimerTick()
        {
			CheckBoundaries(); // 마을 이탈 체크 추가
            Respawn();
            StartTimer(); 
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel < AccessLevel.GameMaster) return;
            from.SendGump(new VendorNodeGump(this)); // Gump도 나중에 int 기반으로 수정 필요
        }
		public void ClearSpawned()
		{
			if (m_Spawned == null) return;

			for (int i = m_Spawned.Count - 1; i >= 0; i--)
			{
				Mobile m = m_Spawned[i];
				if (m != null && !m.Deleted) 
					m.Delete();
			}
			m_Spawned.Clear();
		}
		public void Respawn()
		{
			// 리스폰 기능이 꺼져있으면 작동하지 않음
			if (!IsActive) return;

			m_Spawned.RemoveAll(m => m == null || m.Deleted);

			while (m_Spawned.Count < MaxCount)
			{
				if (SpawnTypes.Count == 0) break;

				string typeName = SpawnTypes[Utility.Random(SpawnTypes.Count)];
				Type type = ScriptCompiler.FindTypeByName(typeName);

				if (type != null)
				{
					try 
					{
						Mobile m = (Mobile)Activator.CreateInstance(type);
						
						if (!string.IsNullOrEmpty(this.VendorName)) 
							m.Name = this.VendorName;

						// --- [수정] 랜덤 좌표 계산 로직 ---
						Point3D spawnLoc = this.Location;
						Map map = this.Map;

						// HomeRange가 0보다 크면 주변 랜덤 좌표를 찾음
						if (HomeRange > 0 && map != null)
						{
							for (int i = 0; i < 10; i++) // 적절한 위치를 찾기 위해 최대 10번 시도
							{
								int x = X + Utility.RandomMinMax(-HomeRange, HomeRange);
								int y = Y + Utility.RandomMinMax(-HomeRange, HomeRange);
								int z = map.GetAverageZ(x, y); // 지면 높이 계산

								if (map.CanSpawnMobile(x, y, z)) // 해당 위치에 소환 가능한지 체크
								{
									spawnLoc = new Point3D(x, y, z);
									break;
								}
							}
						}
						// ----------------------------------

						// 월드에 랜덤 위치로 배치
						m.MoveToWorld(spawnLoc, map);

						if (m is BaseCreature bc) 
						{ 
							// 상인의 집(Home)은 노드 위치로 고정하여 멀리 도망가지 않게 함
							bc.Home = this.Location; 
							bc.RangeHome = this.HomeRange; 
						}

						m_Spawned.Add(m);
					} 
					catch { break; }
				}
				else 
				{
					Console.WriteLine($"[VendorNode Error] '{typeName}' 타입을 찾을 수 없으므로 명단에서 자동 삭제합니다.");
					SpawnTypes.Remove(typeName); // 서버에 없는 타입이면 노드의 소환 명단에서 아예 지워버림
					continue; // break 하지 않고 명단에 남은 다른 애들이라도 소환하도록 계속 진행
				}
			}
		}
        public override void OnDelete()
        {
            if (m_Timer != null) m_Timer.Stop();

            if (m_Spawned != null)
            {
                for (int i = m_Spawned.Count - 1; i >= 0; i--)
                {
                    Mobile m = m_Spawned[i];
                    if (m != null && !m.Deleted) m.Delete();
                }
                m_Spawned.Clear();
            }
            base.OnDelete();
        }

		public int ClearSpawnedVenders()
		{
			int count = m_Spawned.Count;
			for (int i = m_Spawned.Count - 1; i >= 0; i--)
			{
				if (!m_Spawned[i].Deleted) m_Spawned[i].Delete();
			}
			m_Spawned.Clear();
			return count;
		}

        public VendorNode(Serial serial) : base(serial) { }

        // [완전 단순화] 구버전 호환용 코드 전면 철거
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1); // [수정] Version 0 -> 1

            writer.Write(IsActive); // [핵심] 리스폰 방지 스위치 상태 저장

            writer.Write(TownID); 
            writer.Write(MaxCount);
            writer.Write(HomeRange);
            writer.Write(MinDelay);
            writer.Write(MaxDelay);

            writer.Write(SpawnTypes.Count);
            foreach (string s in SpawnTypes) writer.Write(s);

            writer.WriteMobileList(m_Spawned, true);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            
            if (version >= 1)
            {
                IsActive = reader.ReadBool(); // [핵심] 정지 상태 복구
            }

            TownID = reader.ReadInt();
            MaxCount = reader.ReadInt();
            HomeRange = reader.ReadInt();
            MinDelay = reader.ReadTimeSpan();
            MaxDelay = reader.ReadTimeSpan();

            SpawnTypes.Clear();
            int count = reader.ReadInt();
            for (int i = 0; i < count; i++) SpawnTypes.Add(reader.ReadString());

            m_Spawned = reader.ReadStrongMobileList();

            if (TownID == 0) TownID = TownNumber.GetID(this.Location, this.Map);

            StartTimer(); 
        }

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
				if (m_Node == null || m_Node.Deleted || !NewSpawnManager.IsMapActive(m_Node.Map)) return;
                if (m_Node != null && !m_Node.Deleted) m_Node.DoTimerTick();
            }
        }
    }
}
