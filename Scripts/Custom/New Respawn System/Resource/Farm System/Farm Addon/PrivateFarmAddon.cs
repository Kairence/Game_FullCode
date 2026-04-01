using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Multis;
using Server.Gumps;
using Server.Mobiles;

namespace Server.Misc
{
    public class PrivateFarmAddon : BaseAddon
    {
        private int m_Size; 
        private Mobile m_Owner;
        private int[] m_TileData; 
        
        // ★ [최적화 추가] 가축을 기억할 명부 리스트
        private List<BaseCreature> m_Animals; 

        // [말뚝 위치 고정] 밭 범위(0~3)와 겹치지 않게 -1, 0 좌표에 배치
        private static readonly Point3D ControlLoc = new Point3D(-1, 0, 5);
		private static readonly Point3D TroughLoc = new Point3D(-1, 1, 0); // 🌟 여물통 위치 추가
        [CommandProperty(AccessLevel.GameMaster)]
        public int FarmSize { get { return m_Size; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Owner { get { return m_Owner; } set { m_Owner = value; } }

        public int[] TileData { get { return m_TileData; } }
        
        public List<BaseCreature> Animals { get { return m_Animals; } }

        [Constructable]
        public PrivateFarmAddon(Mobile owner, int size)
        {
            m_Owner = owner;
            m_Size = size;
            m_TileData = new int[size * size];
            m_Animals = new List<BaseCreature>(); // 명부 초기화

            // 1. 말뚝 추가
            AddComponent(new FarmControlComponent(), ControlLoc.X, ControlLoc.Y, ControlLoc.Z); 
            AddComponent(new FarmTroughComponent(), TroughLoc.X, TroughLoc.Y, TroughLoc.Z); // 🌟 추가됨
			
            // ★ [버그 해결 1] 지연 생성(Timer) 삭제하고 생성자에서 즉시 4x4 타일 부착!
            for (int i = 0; i < m_TileData.Length; i++)
            {
                m_TileData[i] = 1; 
                int x = i % m_Size;
                int y = i / m_Size;
                AddComponent(new FarmPloughedComponent(), x, y, 0);
            }
        }

        // ==================================================================================
        // ★ [버그 해결 2] 에드온 증발(연쇄 삭제) 방지 및 실시간 집 경계 체크
        // ==================================================================================
        // PrivateFarmAddon.cs 내부

		public void UpdateLayout()
		{
			List<AddonComponent> toRemove = new List<AddonComponent>();
			foreach (AddonComponent comp in Components)
			{
				// 말뚝(컨트롤러)을 제외한 모든 타일을 일단 싹 지웁니다.
				if (!(comp is FarmControlComponent)) 
					toRemove.Add(comp);
			}

			foreach (AddonComponent comp in toRemove) 
			{ 
				Components.Remove(comp); 
				comp.Addon = null; 
				comp.Delete(); 
			}

			BaseHouse house = BaseHouse.FindHouseAt(this.Location, this.Map, 16);

			for (int i = 0; i < m_TileData.Length; i++)
			{
				// 0은 지우기(맨땅)이므로 아무것도 설치하지 않고 패스
				if (m_TileData[i] == 0) continue; 

				int x = i % m_Size;
				int y = i / m_Size;

				Point3D worldLoc = new Point3D(this.X + x, this.Y + y, this.Z);
				
				// 집 경계선 체크
				if (house != null && BaseHouse.FindHouseAt(worldLoc, this.Map, 16) != house)
				{
					m_TileData[i] = 0; 
					continue;
				}

				// 🌟 [핵심] 저장된 데이터(1, 2, 3)에 따라 서로 다른 독립적인 컴포넌트 부착
				if (m_TileData[i] == 1)
				{
					AddComponent(new FarmPloughedComponent(), x, y, 0); // [100] 밭 타일
				}
				else if (m_TileData[i] == 2)
				{
					AddComponent(new FarmBeehiveComponent(), x, y, 0);  // [50] 양봉통
				}
				else if (m_TileData[i] == 3)
				{
					AddComponent(new FarmOrchardComponent(), x, y, 0);  // [150] 과수원 나무
				}
			}

			ValidateFarmPool(); 
		}

        public bool IsPloughedTile(Point3D loc)
        {
            int relX = loc.X - this.X;
            int relY = loc.Y - this.Y;

            if (relX == -1 && relY == 0) return false; 

            if (relX >= 0 && relX < m_Size && relY >= 0 && relY < m_Size)
            {
                int index = relY * m_Size + relX;
                return m_TileData[index] == 1;
            }

            return false;
        }
		// 🌟 [50] 양봉통 컴포넌트
    public class FarmBeehiveComponent : AddonComponent
    {
        private DateTime m_NextHarvest;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextHarvest { get { return m_NextHarvest; } set { m_NextHarvest = value; } }

        [Constructable]
        public FarmBeehiveComponent() : base(0x091A) // 양봉통 그래픽
        { 
            Name = "양봉통"; 
            m_NextHarvest = DateTime.Now; // 설치 직후 바로 수확 가능하게
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.Skills[SkillName.Herding].Base < 50.0)
            {
                from.SendMessage("양봉을 관리할 스킬이 부족합니다.");
                return;
            }

            if (DateTime.Now < m_NextHarvest)
            {
                TimeSpan ts = m_NextHarvest - DateTime.Now;
                from.SendMessage($"벌들이 아직 꿀을 모으고 있습니다. (약 {ts.Hours}시간 {ts.Minutes}분 남음)");
                return;
            }

            from.Animate(32, 5, 1, true, false, 0);
            from.PlaySound(0x0DF); // 부스럭 소리

            // 꿀(JarHoney)과 밀랍(Beeswax) 지급
            from.AddToBackpack(new JarHoney(Utility.RandomMinMax(1, 3)));
            if (Utility.RandomDouble() < 0.5) from.AddToBackpack(new Beeswax());

            from.SendMessage(68, "달콤한 꿀과 밀랍을 수확했습니다.");
            
            // 다음 수확은 12시간 뒤
            m_NextHarvest = DateTime.Now + TimeSpan.FromHours(12.0);
        }

        public FarmBeehiveComponent(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); writer.Write(m_NextHarvest); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); m_NextHarvest = reader.ReadDateTime(); }
    }

    // 🌟 [150] 과수원 컴포넌트
    public class FarmOrchardComponent : AddonComponent
    {
        private DateTime m_NextHarvest;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextHarvest { get { return m_NextHarvest; } set { m_NextHarvest = value; } }

        [Constructable]
        public FarmOrchardComponent() : base(0x0D01) // 사과나무 그래픽
        { 
            Name = "과수원 나무"; 
            m_NextHarvest = DateTime.Now + TimeSpan.FromHours(4.0); // 묘목 정착 시간
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.Skills[SkillName.Herding].Base < 150.0)
            {
                from.SendMessage("과수원을 수확할 스킬이 부족합니다.");
                return;
            }

            if (DateTime.Now < m_NextHarvest)
            {
                TimeSpan ts = m_NextHarvest - DateTime.Now;
                from.SendMessage($"아직 과일이 덜 익었습니다. (약 {ts.Hours}시간 {ts.Minutes}분 남음)");
                return;
            }

            from.Animate(32, 5, 1, true, false, 0);
            from.PlaySound(0x13E); 

            // 사과, 바나나, 복숭아, 배 중 랜덤 수확
            Item fruit = null;
            switch (Utility.Random(4))
            {
                case 0: fruit = new Apple(Utility.RandomMinMax(3, 6)); break;
                case 1: fruit = new Banana(Utility.RandomMinMax(3, 6)); break;
                case 2: fruit = new Peach(Utility.RandomMinMax(3, 6)); break;
                case 3: fruit = new Pear(Utility.RandomMinMax(3, 6)); break;
            }

            if (fruit != null)
            {
                from.AddToBackpack(fruit);
                from.SendMessage(68, "신선한 과일을 수확했습니다.");
            }

            // 다음 수확은 24시간 뒤
            m_NextHarvest = DateTime.Now + TimeSpan.FromHours(24.0);
        }

        public FarmOrchardComponent(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); writer.Write(m_NextHarvest); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); m_NextHarvest = reader.ReadDateTime(); }
    }
        // =======================================================================
        // ★ [축산 로직 1] 동물을 농장에 귀속시키고 명부에 등록 (O(1) 처리)
        // =======================================================================
        public void AssignAnimal(Mobile from, BaseCreature animal)
        {
            if (animal == null || animal.Deleted || animal.IsDeadPet) return;

            // 이미 명부에 있는 동물인지 체크
            if (m_Animals.Contains(animal))
            {
                from.SendMessage("이 동물은 이미 농장 명부에 등록되어 있습니다.");
                return;
            }

            // 명부에 추가
            m_Animals.Add(animal);

            // 동물의 집(Home)을 에드온의 중심으로 설정하고 이탈 방지
            animal.Home = this.Location;
            animal.RangeHome = this.FarmSize; 
            
            from.SendMessage(68, $"{animal.Name}을(를) 농장에 성공적으로 배치했습니다. (현재 {m_Animals.Count}마리)");
        }

        // =======================================================================
        // ★ [축산 로직 2] 최적화된 가축 카운트 (스캔 없이 리스트만 확인)
        // =======================================================================
        public int GetLivestockCount()
        {
            // 카운트 전, 죽었거나 지워진 동물을 명부에서 솎아내는 정리(Cleanup) 작업
            for (int i = m_Animals.Count - 1; i >= 0; i--)
            {
                BaseCreature bc = m_Animals[i];
                if (bc == null || bc.Deleted || !bc.Alive)
                {
                    m_Animals.RemoveAt(i);
                }
            }

            return m_Animals.Count;
        }
		// ==================================================================================
    // 🌟 [신규] 가축 일괄 관리 및 수확 시스템 (여물통)
    // ==================================================================================
    public class FarmTroughComponent : AddonComponent
    {
        private DateTime m_NextFeed;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextFeed { get => m_NextFeed; set => m_NextFeed = value; }

        public FarmTroughComponent() : base(0x0B41) // 여물통 그래픽
        { 
            Name = "가축 여물통"; 
            m_NextFeed = DateTime.Now; 
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!(Addon is PrivateFarmAddon farm) || farm.Owner != from)
            {
                from.SendMessage("당신의 농장이 아닙니다.");
                return;
            }

            int count = farm.GetLivestockCount();
            if (count == 0)
            {
                from.SendMessage("농장 명부에 등록된 가축이 없습니다.");
                return;
            }

            if (DateTime.Now < m_NextFeed)
            {
                TimeSpan ts = m_NextFeed - DateTime.Now;
                from.SendMessage($"가축들이 아직 배가 부릅니다. (약 {ts.Hours}시간 {ts.Minutes}분 남음)");
                return;
            }

            // 🌟 건초(SheafOfHay) 요구: 1마리당 1개 소모
            Item hay = from.Backpack.FindItemByType(typeof(SheafOfHay));
            if (hay == null || hay.Amount < count)
            {
                from.SendMessage(33, $"가축을 모두 먹이려면 가방에 건초(Sheaf of Hay)가 {count}개 이상 필요합니다.");
                return;
            }

            double skill = from.Skills[SkillName.Herding].Base;
            hay.Consume(count);

            from.PlaySound(0x05A); // 동물 밥 먹는 소리
            from.Animate(32, 5, 1, true, false, 0);

            int eggs = 0, feathers = 0, leather = 0, wool = 0, mountDeeds = 0;
			int etherealJackpots = 0; // 🌟 에테리얼 잭팟 카운트 추가
            bool gotMilk = false;

            // 🌟 가축 티어별 생산 스킬 체크 및 수확량 집계
            foreach (var animal in farm.Animals)
            {
                if (animal is Chicken && skill >= 50.0) { eggs += 2; feathers += 5; }
                else if (animal is Cow && skill >= 100.0) { gotMilk = true; leather += 2; }
                else if (animal is Sheep && skill >= 150.0) { wool += 3; }
                else if ((animal is Horse || animal is Llama) && skill >= 200.0)
                {
                    // 200레벨: 5% 확률로 일반 명마 증서 생산
                    if (Utility.RandomDouble() < 0.05) mountDeeds++;

                    // 🌟 [추가] 0.5%의 극악 확률로 에테리얼 환상종 탄생! (확률은 입맛에 맞게 조절하세요)
                    if (Utility.RandomDouble() < 0.001) etherealJackpots++;
                }
            }

            for (int i = 0; i < etherealJackpots; i++)
            {
                // 말과 라마 중 랜덤으로 에테리얼 탈것 생성
                Item ethy = Utility.RandomBool() ? new EtherealHorse() : new EtherealLlama();
                
                if (from.PlaceInBackpack(ethy))
                {
                    from.PlaySound(0x0F5); // 신비로운 마법 소리
                    from.SendMessage(1150, "세상에! 가축들 사이에서 신비로운 에테리얼의 기운이 발견되었습니다!");
                    
                    // 🌟 서버 전체에 잭팟 공지 (생산직의 위상 떡상)
                    Server.Commands.CommandHandlers.BroadcastMessage(AccessLevel.Player, 1150, 
                        $"[농장 소식] {from.Name}님의 농장에서 환상의 에테리얼 탈것이 탄생했습니다!");
                }
                else
                {
                    ethy.Delete();
                    from.SendMessage(33, "가방이 꽉 차서 에테리얼 탈것을 받을 수 없었습니다!");
                }
            }

            from.SendMessage(68, "가축들에게 먹이를 주고 생산품을 수확했습니다!");
            m_NextFeed = DateTime.Now + TimeSpan.FromHours(8.0); // 8시간 쿨타임
        }

        public FarmTroughComponent(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); writer.Write(m_NextFeed); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); m_NextFeed = reader.ReadDateTime(); }
    }
        public void ValidateFarmPool()
        {
            if (this.Map != null && this.Map != Map.Internal)
            {
                string farmKey = $"PrivateFarm_{this.Serial.Value}";
                ResourceKey key = new ResourceKey(this.Map.Name, farmKey, ResourceType.Farming);

                int ploughedCount = 0;
                for (int i = 0; i < m_TileData.Length; i++)
                {
                    if (m_TileData[i] == 1) ploughedCount++;
                }
                
                int correctCap = Math.Max(1, ploughedCount); 

                if (!ResourceManager.Pools.ContainsKey(key))
                {
                    ResourcePool pool = new ResourcePool(this.Map.Name, farmKey, ResourceType.Farming, LocationType.Farm_Remote, correctCap, 1);
                    pool.CurrentCapacity = 0;
                    ResourceManager.Pools[key] = pool;
                }
                else
                {
                    ResourcePool pool = ResourceManager.Pools[key];
                    if (pool.MaxCapacity != correctCap)
                        pool.MaxCapacity = correctCap;
                }
            }
        }

        public override void OnAfterSpawn()
        {
            base.OnAfterSpawn();
            ValidateFarmPool(); 
        }

        public override void OnDelete()
        {
            if (this.Map != null && this.Map != Map.Internal)
            {
                string farmKey = $"PrivateFarm_{this.Serial.Value}";
                ResourceKey key = new ResourceKey(this.Map.Name, farmKey, ResourceType.Farming);
                if (ResourceManager.Pools.ContainsKey(key))
                    ResourceManager.Pools.Remove(key); 
            }
            base.OnDelete();
        }

        public PrivateFarmAddon(Serial serial) : base(serial) { }
        
        public override void Serialize(GenericWriter writer) 
        { 
            base.Serialize(writer); 
            writer.Write((int)2); // ★ [수정] 버전 업그레이드 (1 -> 2)
            
            writer.Write(m_Owner); 
            writer.Write(m_Size); 
            writer.Write(m_TileData.Length); 
            for (int i = 0; i < m_TileData.Length; i++) writer.Write(m_TileData[i]); 

            // ★ [추가] 동물 명부 저장
            writer.Write(m_Animals.Count);
            for (int i = 0; i < m_Animals.Count; i++)
            {
                writer.Write(m_Animals[i]);
            }
        }
        
        public override void Deserialize(GenericReader reader) 
        { 
            base.Deserialize(reader); 
            int version = reader.ReadInt(); 
            
            m_Owner = reader.ReadMobile(); 
            m_Size = reader.ReadInt(); 
            int len = reader.ReadInt(); 
            m_TileData = new int[len]; 
            for (int i = 0; i < len; i++) m_TileData[i] = reader.ReadInt(); 

            m_Animals = new List<BaseCreature>(); // 리스트 초기화

            // ★ [추가] 버전 2 이상일 때 가축 명부 불러오기
            if (version >= 2)
            {
                int animalCount = reader.ReadInt();
                for (int i = 0; i < animalCount; i++)
                {
                    BaseCreature bc = reader.ReadMobile() as BaseCreature;
                    if (bc != null)
                        m_Animals.Add(bc);
                }
            }

            Timer.DelayCall(TimeSpan.FromSeconds(1.0), ValidateFarmPool);
        }
    }

    // ==================================================================================
    // 서브 클래스들
    // ==================================================================================

    public class FarmControlComponent : AddonComponent
    {
        public FarmControlComponent() : base(0xBD2) { Name = "농장 관리 말뚝"; }
        
        public override void OnDoubleClick(Mobile from)
        {
            if (Addon is PrivateFarmAddon farm && (from == farm.Owner || from.AccessLevel >= AccessLevel.GameMaster))
                from.SendGump(new FarmBuilderGump(farm, 1));
        }

        public FarmControlComponent(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }

    public class FarmPloughedComponent : AddonComponent
    {
        [Constructable]
        public FarmPloughedComponent() : base(0x32C9) 
        { 
            Name = "경작된 밭"; 
            Hue = 0;
        }

        public FarmPloughedComponent(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}
