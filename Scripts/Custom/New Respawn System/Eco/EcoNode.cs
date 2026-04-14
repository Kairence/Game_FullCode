using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Regions;
using Server.Spells; 
using Server.Mobiles;

namespace Server.Misc
{
    public enum EcoAreaType { Town, Forest, Hunting, Special }
    public enum EcoClimateType { Temperate, Arctic, Tropical, Desert, Coastal, Swamp, Volcanic, Void }

    public class EcoNode : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public RegionCode RCode { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ZoneId => NewSpawnManager.GetDisplayName(RCode);

        [CommandProperty(AccessLevel.GameMaster)]
        public EcoAreaType AreaType { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public EcoClimateType ClimateType { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SpawnRange { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HomeRange { get; set; }

        private List<Mobile> m_Spawned = new();
        private List<EcoSpawnDef> m_CachedSpawnPool;

        [Constructable]
        public EcoNode() : base(0x11EA)
        {
            Movable = false; Visible = false; Name = "Ecosystem Spawn Node";
            RCode = RegionCode.None;
            AreaType = EcoAreaType.Forest; ClimateType = EcoClimateType.Temperate;
            SpawnRange = 64; HomeRange = 80;

            UpdateCache();
        }

        public EcoNode(Serial serial) : base(serial) { }

        public override void OnDoubleClick(Mobile from)
        {
            // EcoNodeGump는 기존대로 사용하시면 됩니다.
        }

        public void UpdateCache()
        {
            m_CachedSpawnPool = EcoSpawnDatabase.GetPoolFor(this);
        }

        public override void OnDelete()
        {
            foreach (var m in m_Spawned.ToList()) { m?.Delete(); }
            base.OnDelete();
        }

        public Point3D? GetValidSpawnLocation()
        {
            if (Map == null || Map == Map.Internal) return null;
            Region nodeRegion = Region.Find(Location, Map);

            for (int i = 0; i < 15; i++)
            {
                int rx = X + Utility.RandomMinMax(-SpawnRange, SpawnRange);
                int ry = Y + Utility.RandomMinMax(-SpawnRange, SpawnRange);
                int rz = Map.GetAverageZ(rx, ry);
                Point3D testLoc = new Point3D(rx, ry, rz);

                if (Map.CanSpawnMobile(rx, ry, rz) && nodeRegion == Region.Find(testLoc, Map) && !SpellHelper.CheckMulti(testLoc, Map))
                {
                    bool isCrowded = false;
                    IPooledEnumerable eable = Map.GetMobilesInRange(testLoc, 1);
                    foreach (Mobile m in eable) { isCrowded = true; break; }
                    eable.Free();

                    if (!isCrowded) return testLoc;
                }
            }
            return null;
        }

        // 🌟 [핵심 추가] 유저 곁에 있거나 전투 중인 몬스터를 판별하여 강제 삭제를 막는 안전장치
        private bool IsSafeFromPredation(Mobile m)
        {
            if (m == null || m.Deleted || !m.Alive) return true;
            if (m.Combatant != null) return true; // 전투(사냥) 중이면 면책

            // 18타일 이내(화면 안팎)에 유저가 존재하면 면책
            foreach (NetState state in NetState.Instances)
            {
                Mobile pm = state.Mobile;
                if (pm != null && pm.Map == m.Map && pm.InRange(m.Location, 18))
                {
                    return true;
                }
            }
            return false;
        }

        // 🌟 [MasterTick 파이프라인] 마스터 틱 엔진이 30분 주기로 호출하는 수동형 틱
        public void DoTick()
        {
            if (Map == null || Map == Map.Internal) return;
            if (!Server.Misc.NewSpawnManager.ActiveMaps.GetValueOrDefault(Map, true)) return;

            m_Spawned.RemoveAll(m => m == null || m.Deleted || !m.Alive || (m is BaseCreature bc && (bc.Controlled || bc.IsStabled)));

            var chunkInfo = EcoGridDatabase.GetChunkAt(Map, X, Y);
            if (!chunkInfo.IsValid) return;

            int maxPop = Math.Clamp(chunkInfo.Data.TanCap / 50, 0, 20);
            
            ResourceKey woodKey = new(Map.Name, chunkInfo.Data.Code.ToString(), ResourceType.Lumberjacking);
            bool isRecovering = false; 

            // ====================================================================
            // 🌟 [이관된 숲 생태계 로직] 자원 시스템에서 가져온 동물 먹이사슬 엔진
            // ====================================================================
            if (AreaType == EcoAreaType.Forest && ResourceManager.Pools.TryGetValue(woodKey, out var woodPool))
            {
                if (woodPool.MaxCapacity > 0)
                {
                    var herbivores = m_Spawned.OfType<BaseCreature>().Where(m => m is Hind || m is GreatHart || m is Rabbit).ToList();
                    var carnivores = m_Spawned.OfType<BaseCreature>().Where(m => m is TimberWolf || m is GrizzlyBear || m is DireWolf).ToList();

                    // 1. 초식동물이 나무 자원을 갉아먹음
                    int herbivoreConsumption = herbivores.Count * (woodPool.MaxCapacity / 200); 
                    int newWoodCapacity = woodPool.CurrentCapacity - herbivoreConsumption;

                    if (newWoodCapacity > 0)
                    {
                        woodPool.CurrentCapacity = newWoodCapacity;
                        foreach (var h in herbivores) h.Hunger = 100000; 
                    }
                    else
                    {
                        // 숲 고갈 (사막화)
                        woodPool.CurrentCapacity = 0;
                        woodPool.DepletionCooldown = DateTime.Now.AddHours(2.0); // 숲 사막화 (2시간 쿨타임)
                        Console.WriteLine($"[생태계] {ZoneId}의 숲이 황폐화되었습니다.");
                        
                        foreach (var h in herbivores) 
                        {
                            h.Hunger -= 40000; 
                            if (h.Hunger <= 0) h.Hunger = 1; // 유저 텔레포트 시 시체밭을 막기 위해 1로 유지
                        }
                    }

                    // 🌟 2. 육식동물이 초식동물을 잡아먹음 (안전 구역 검사 적용)
                    var activeCarnivores = carnivores.Where(c => !IsSafeFromPredation(c)).ToList();
                    var activeHerbivores = herbivores.Where(h => !IsSafeFromPredation(h)).ToList();

                    foreach (var c in carnivores)
                    {
                        if (IsSafeFromPredation(c)) continue; // 안전 구역에 있으면 굶지 않고 면책

                        if (activeHerbivores.Count > 0)
                        {
                            // 사냥감을 찾아서 포식함
                            Mobile prey = activeHerbivores[0];
                            
                            // 시각/청각적 연출: 피가 튀고 살점이 찢기는 소리
                            Effects.SendLocationEffect(prey.Location, prey.Map, 0x3728, 10, 10); 
                            Effects.PlaySound(prey.Location, prey.Map, 0x133); 
                            
                            prey.Delete(); // 초식동물 삭제 (다음 턴에 자연스럽게 채워짐)
                            activeHerbivores.RemoveAt(0);
                            herbivores.Remove(prey as BaseCreature);
                            m_Spawned.Remove(prey);
                            
                            c.Hunger = 100000; // 배부름
                        }
                        else
                        {
                            // 사냥감이 없어 굶주림 발생
                            c.Hunger -= 40000;
                            if (c.Hunger <= 0) c.Hunger = 1; 
                            
                            // 육식동물 아사: 굶주림이 심하면 50% 확률로 대자연으로 소멸
                            if (c.Hunger < 20000 && Utility.RandomDouble() < 0.5)
                            {
                                c.Delete();
                                carnivores.Remove(c);
                                m_Spawned.Remove(c);
                            }
                        }
                    }

                    // 3. 생태계 파괴 (초식이 없고 육식도 없으면, 자율 스폰 로직이 7:3 비율 복구를 돕도록 인구수 비율 조정)
                    if (herbivores.Count > 0 && carnivores.Count == 0)
                    {
                        // 포식자가 멸종하면 초식이 비정상 번식
                        maxPop += 5; 
                    }

                    // 숲의 상태(나무 잔여 비율)에 따라 최대 인구수 제한
                    double woodRatio = (double)woodPool.CurrentCapacity / woodPool.MaxCapacity;
                    maxPop = (int)(maxPop * woodRatio); 
                    if (woodRatio <= 0.5) isRecovering = true; 
                }
            }
            // ====================================================================

            // 🌟 일반/공통 자율 생태계 유지 로직 (결핍된 개체수를 채움)
            if (m_Spawned.Count < maxPop)
            {
                int spawnCount = Utility.RandomMinMax(1, 2);
                for (int i = 0; i < spawnCount && m_Spawned.Count < maxPop; i++)
                {
                    Type typeToSpawn = EcoSpawnDatabase.RollFromPool(m_CachedSpawnPool);
                    if (typeToSpawn == null) continue;

                    if (isRecovering && Utility.RandomDouble() > 0.1) typeToSpawn = typeof(Rabbit); 

                    Point3D? loc = GetValidSpawnLocation();
                    if (loc.HasValue)
                    {
                        Mobile mob = (Mobile)Activator.CreateInstance(typeToSpawn);
                        if (mob is BaseCreature creature)
                        {
                            creature.Home = Location; creature.RangeHome = HomeRange; creature.Hunger = 100000; 
                        }
                        mob.MoveToWorld(loc.Value, Map);
                        m_Spawned.Add(mob);
                    }
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(2); 
            
            writer.Write((int)RCode);
            writer.Write((int)AreaType);
            writer.Write((int)ClimateType);
            writer.Write(SpawnRange);
            writer.Write(HomeRange);

            m_Spawned.RemoveAll(m => m == null || m.Deleted);
            writer.Write(m_Spawned.Count);
            foreach (var m in m_Spawned) writer.Write(m);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            
            if (version >= 2)
            {
                RCode = (RegionCode)reader.ReadInt();
            }
            else
            {
                reader.ReadString(); 
                RCode = RegionCode.None;
            }

            AreaType = (EcoAreaType)reader.ReadInt();
            ClimateType = (EcoClimateType)reader.ReadInt();
            SpawnRange = reader.ReadInt();
            HomeRange = reader.ReadInt();

            if (version >= 1)
            {
                int count = reader.ReadInt();
                for (int i = 0; i < count; i++)
                {
                    Mobile m = reader.ReadMobile();
                    if (m != null && !m.Deleted) m_Spawned.Add(m);
                }
            }

            UpdateCache();
        }
    }
}