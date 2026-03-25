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
        public void UpdateLayout()
        {
            List<AddonComponent> toRemove = new List<AddonComponent>();
            foreach (AddonComponent comp in Components)
            {
                if (!(comp is FarmControlComponent)) 
                    toRemove.Add(comp);
            }

            foreach (AddonComponent comp in toRemove) 
            { 
                // [핵심] 컴포넌트를 지우기 전에 에드온과의 연결을 끊어줘야 전체 폭파를 막습니다!
                Components.Remove(comp); 
                comp.Addon = null; 
                comp.Delete(); 
            }

            // 현재 이 에드온(말뚝)이 속한 집 정보 가져오기
            BaseHouse house = BaseHouse.FindHouseAt(this.Location, this.Map, 16);

            for (int i = 0; i < m_TileData.Length; i++)
            {
                if (m_TileData[i] == 1) 
                {
                    int x = i % m_Size;
                    int y = i / m_Size;

                    Point3D worldLoc = new Point3D(this.X + x, this.Y + y, this.Z);
                    
                    // ★ [에러 수정] 새로 생성할 타일의 위치가 '같은 집' 소속인지 체크!
                    if (house != null && BaseHouse.FindHouseAt(worldLoc, this.Map, 16) != house)
                    {
                        m_TileData[i] = 0; // 데이터 지워버림
                        continue;
                    }

                    AddComponent(new FarmPloughedComponent(), x, y, 0);
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
