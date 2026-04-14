using System;
using System.Collections.Generic;
using System.Linq;
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
        private List<BaseCreature> m_Animals; 

        private static readonly Point3D ControlLoc = new Point3D(-1, 0, 5);
        private static readonly Point3D TroughLoc = new Point3D(-1, 1, 0); 

        [CommandProperty(AccessLevel.GameMaster)]
        public int FarmSize { get { return m_Size; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile MobileOwner { get { return m_Owner; } set { m_Owner = value; } }

        public int[] TileData { get { return m_TileData; } }
        public List<BaseCreature> Animals { get { return m_Animals; } }

        [Constructable]
        public PrivateFarmAddon(Mobile owner, int size)
        {
            m_Owner = owner;
            m_Size = size;
            m_TileData = new int[size * size];
            m_Animals = new List<BaseCreature>(); 

            AddComponent(new FarmControlComponent(), ControlLoc.X, ControlLoc.Y, ControlLoc.Z); 
            AddComponent(new FarmTroughComponent(), TroughLoc.X, TroughLoc.Y, TroughLoc.Z); 
            
            for (int i = 0; i < m_TileData.Length; i++)
            {
                m_TileData[i] = 1; 
                int x = i % m_Size;
                int y = i / m_Size;
                AddComponent(new FarmPloughedComponent(), x, y, 0);
            }
        }

        public void UpdateLayout()
        {
            List<AddonComponent> toRemove = new List<AddonComponent>();
            foreach (AddonComponent comp in Components)
            {
                if (!(comp is FarmControlComponent || comp is FarmTroughComponent)) 
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
                if (m_TileData[i] == 0) continue; 

                int x = i % m_Size;
                int y = i / m_Size;
                Point3D worldLoc = new Point3D(this.X + x, this.Y + y, this.Z);
                
                if (house != null && BaseHouse.FindHouseAt(worldLoc, this.Map, 16) != house)
                {
                    m_TileData[i] = 0; 
                    continue;
                }

                if (m_TileData[i] == 1) AddComponent(new FarmPloughedComponent(), x, y, 0);
                else if (m_TileData[i] == 2) AddComponent(new FarmBeehiveComponent(), x, y, 0);
                else if (m_TileData[i] == 3) AddComponent(new FarmOrchardComponent(), x, y, 0);
            }

            ValidateFarmPool(); 
        }

        public void AssignAnimal(Mobile from, BaseCreature animal)
        {
            if (animal == null || animal.Deleted || animal.IsDeadPet) return;
            if (m_Animals.Contains(animal))
            {
                from.SendMessage("이 동물은 이미 농장 명부에 등록되어 있습니다.");
                return;
            }

            m_Animals.Add(animal);
            animal.Home = this.Location;
            animal.RangeHome = this.FarmSize; 
            from.SendMessage(68, $"{animal.Name}을(를) 농장에 등록했습니다. (현재 {m_Animals.Count}마리)");
        }

        public int GetLivestockCount()
        {
            m_Animals.RemoveAll(bc => bc == null || bc.Deleted || !bc.Alive);
            return m_Animals.Count;
        }

        public void ValidateFarmPool()
        {
            if (this.Map != null && this.Map != Map.Internal)
            {
                string farmKey = $"PrivateFarm_{this.Serial.Value}";
                ResourceKey key = new ResourceKey(this.Map.Name, farmKey, ResourceType.Farming);

                int ploughedCount = m_TileData.Count(t => t == 1);
                int correctCap = Math.Max(1, ploughedCount); 

                if (!ResourceManager.Pools.ContainsKey(key))
                {
                    // 🌟 [수정 핵심] ResourcePool 생성자 파라미터 개수를 ResourceSystem.cs 사양에 맞춤 (12개)
                    RegionCode rCode = RegionSaver.GetRegionCode(this.Map, this.X, this.Y, this.Z);
                    ResourcePool pool = new ResourcePool(
                        this.Map.Name,          // mapName
                        farmKey,                // regionName
                        this.Map,               // map
                        rCode,                  // code
                        this.X,                 // cx
                        this.Y,                 // cy
                        WaterType.River,        // wType
                        ResourceType.Farming,   // type
                        LocationType.Farm_Remote, // locType
                        correctCap,             // max
                        1,                      // size
                        true                    // isPrivate
                    );
                    pool.CurrentCapacity = 0;
                    ResourceManager.Pools[key] = pool;
                }
                else
                {
                    ResourceManager.Pools[key].MaxCapacity = correctCap;
                }
            }
        }

        public override void OnAfterSpawn() { base.OnAfterSpawn(); ValidateFarmPool(); }

        public override void OnDelete()
        {
            if (this.Map != null && this.Map != Map.Internal)
            {
                ResourceKey key = new ResourceKey(this.Map.Name, $"PrivateFarm_{this.Serial.Value}", ResourceType.Farming);
                ResourceManager.Pools.Remove(key); 
            }
            base.OnDelete();
        }

        public PrivateFarmAddon(Serial serial) : base(serial) { }
        
        public override void Serialize(GenericWriter writer) 
        { 
            base.Serialize(writer); 
            writer.Write((int)2); 
            writer.Write(m_Owner); 
            writer.Write(m_Size); 
            writer.Write(m_TileData.Length); 
            for (int i = 0; i < m_TileData.Length; i++) writer.Write(m_TileData[i]); 
            writer.Write(m_Animals.Count);
            for (int i = 0; i < m_Animals.Count; i++) writer.Write(m_Animals[i]);
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
            m_Animals = new List<BaseCreature>();
            if (version >= 2)
            {
                int animalCount = reader.ReadInt();
                for (int i = 0; i < animalCount; i++)
                {
                    BaseCreature bc = reader.ReadMobile() as BaseCreature;
                    if (bc != null) m_Animals.Add(bc);
                }
            }
            Timer.DelayCall(TimeSpan.FromSeconds(1.0), ValidateFarmPool);
        }
    }

    // ==================================================================================
    // 컴포넌트 클래스들
    // ==================================================================================

    public class FarmBeehiveComponent : AddonComponent
    {
        private DateTime m_NextHarvest;
        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextHarvest { get { return m_NextHarvest; } set { m_NextHarvest = value; } }

        public FarmBeehiveComponent() : base(0x091A) { Name = "양봉통"; m_NextHarvest = DateTime.Now; }
        public override void OnDoubleClick(Mobile from)
        {
            if (from.Skills[SkillName.Herding].Base < 50.0) { from.SendMessage("양봉을 관리할 스킬이 부족합니다."); return; }
            if (DateTime.Now < m_NextHarvest) { from.SendMessage("벌들이 아직 꿀을 모으고 있습니다."); return; }
            from.Animate(32, 5, 1, true, false, 0); from.PlaySound(0x0DF);
            from.AddToBackpack(new JarHoney(Utility.RandomMinMax(1, 3)));
            if (Utility.RandomDouble() < 0.5) from.AddToBackpack(new Beeswax());
            from.SendMessage(68, "달콤한 꿀과 밀랍을 수확했습니다.");
            m_NextHarvest = DateTime.Now + TimeSpan.FromHours(12.0);
        }
        public FarmBeehiveComponent(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); writer.Write(m_NextHarvest); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); m_NextHarvest = reader.ReadDateTime(); }
    }

    public class FarmOrchardComponent : AddonComponent
    {
        private DateTime m_NextHarvest;
        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextHarvest { get { return m_NextHarvest; } set { m_NextHarvest = value; } }

        public FarmOrchardComponent() : base(0x0D01) { Name = "과수원 나무"; m_NextHarvest = DateTime.Now + TimeSpan.FromHours(4.0); }
        public override void OnDoubleClick(Mobile from)
        {
            if (from.Skills[SkillName.Herding].Base < 150.0) { from.SendMessage("과수원을 수확할 스킬이 부족합니다."); return; }
            if (DateTime.Now < m_NextHarvest) { from.SendMessage("아직 과일이 덜 익었습니다."); return; }
            from.Animate(32, 5, 1, true, false, 0); from.PlaySound(0x13E); 
            Item fruit = Utility.Random(4) switch { 0 => new Apple(Utility.RandomMinMax(3, 6)), 1 => new Banana(Utility.RandomMinMax(3, 6)), 2 => new Peach(Utility.RandomMinMax(3, 6)), _ => new Pear(Utility.RandomMinMax(3, 6)) };
            from.AddToBackpack(fruit); from.SendMessage(68, "신선한 과일을 수확했습니다.");
            m_NextHarvest = DateTime.Now + TimeSpan.FromHours(24.0);
        }
        public FarmOrchardComponent(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); writer.Write(m_NextHarvest); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); m_NextHarvest = reader.ReadDateTime(); }
    }

    public class FarmTroughComponent : AddonComponent
    {
        private DateTime m_NextFeed;
        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextFeed { get => m_NextFeed; set => m_NextFeed = value; }

        public FarmTroughComponent() : base(0x0B41) { Name = "가축 여물통"; m_NextFeed = DateTime.Now; }
        public override void OnDoubleClick(Mobile from)
        {
            if (!(Addon is PrivateFarmAddon farm) || farm.MobileOwner != from) { from.SendMessage("당신의 농장이 아닙니다."); return; }
            int count = farm.GetLivestockCount();
            if (count == 0) { from.SendMessage("농장 명부에 등록된 가축이 없습니다."); return; }
            if (DateTime.Now < m_NextFeed) { from.SendMessage("가축들이 아직 배가 부릅니다."); return; }

            Item hay = from.Backpack.FindItemByType(typeof(SheafOfHay));
            if (hay == null || hay.Amount < count) { from.SendMessage(33, $"건초(Sheaf of Hay)가 {count}개 이상 필요합니다."); return; }

            hay.Consume(count);
            from.PlaySound(0x05A); from.Animate(32, 5, 1, true, false, 0);
            double skill = from.Skills[SkillName.Herding].Base;

            foreach (var animal in farm.Animals)
            {
                if (animal is Chicken && skill >= 50.0) { from.AddToBackpack(new Eggs(2)); from.AddToBackpack(new Feather(5)); }
                else if (animal is Cow && skill >= 100.0) { from.AddToBackpack(new Bottle(1)); /* 우유 로직 대체 가능 */ }
                else if (animal is Sheep && skill >= 150.0) { from.AddToBackpack(new Wool(3)); }
                else if ((animal is Horse || animal is Llama) && skill >= 200.0)
                {
                    if (Utility.RandomDouble() < 0.001) // 0.1% 에테리얼 잭팟
                    {
                        Item ethy = Utility.RandomBool() ? new EtherealHorse() : new EtherealLlama();
                        if (from.PlaceInBackpack(ethy)) {
                            from.PlaySound(0x0F5);
                            Server.Commands.CommandHandlers.BroadcastMessage(AccessLevel.Player, 1150, $"[농장] {from.Name}님의 농장에서 에테리얼 탈것이 탄생했습니다!");
                        } else ethy.Delete();
                    }
                }
            }
            from.SendMessage(68, "가축 수확 완료!"); m_NextFeed = DateTime.Now + TimeSpan.FromHours(8.0);
        }
        public FarmTroughComponent(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); writer.Write(m_NextFeed); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); m_NextFeed = reader.ReadDateTime(); }
    }

    public class FarmControlComponent : AddonComponent
    {
        public FarmControlComponent() : base(0xBD2) { Name = "농장 관리 말뚝"; }
        public override void OnDoubleClick(Mobile from) { if (Addon is PrivateFarmAddon farm && (from == farm.MobileOwner || from.AccessLevel >= AccessLevel.GameMaster)) from.SendGump(new FarmBuilderGump(farm, 1)); }
        public FarmControlComponent(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }

    public class FarmPloughedComponent : AddonComponent
    {
        public FarmPloughedComponent() : base(0x32C9) { Name = "경작된 밭"; }
        public FarmPloughedComponent(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}