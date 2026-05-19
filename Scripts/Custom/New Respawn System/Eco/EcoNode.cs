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

        public List<Mobile> Spawned => m_Spawned; // 외부 접근용 프로퍼티

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
            // EcoNodeGump 연동
        }

        public void UpdateCache()
        {
            m_CachedSpawnPool = EcoSpawnDatabase.GetPoolFor(this);
        }

        // 🌟 [수정 1] 노드 삭제 시 동반 삭제 및 외부 시스템(Kill Switch)에서 호출할 수 있는 청소 함수 추가
        public override void OnDelete()
        {
            ClearAllSpawns();
            base.OnDelete();
        }

        public int ClearAllSpawns()
        {
            int count = 0;
            if (m_Spawned != null)
            {
                for (int i = m_Spawned.Count - 1; i >= 0; i--)
                {
                    if (m_Spawned[i] != null && !m_Spawned[i].Deleted)
                    {
                        m_Spawned[i].Delete();
                        count++;
                    }
                }
                m_Spawned.Clear();
            }
            return count;
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

        private bool IsSafeFromPredation(Mobile m)
        {
            if (m == null || m.Deleted || !m.Alive) return true;
            if (m.Combatant != null) return true; 

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

        public void DoTick()
        {
            if (Map == null || Map == Map.Internal) return;
            if (!Server.Misc.NewSpawnManager.ActiveMaps.GetValueOrDefault(Map, true)) return;

            // 🌟 [수정 2] 마을(Town) 구역이면 동물/몬스터 스폰 연산을 완전히 차단
            if (this.AreaType == EcoAreaType.Town || ((int)this.RCode / 10000) % 10 == 1)
            {
                if (m_Spawned.Count > 0) ClearAllSpawns(); // 기존에 잘못 소환된 동물이 있다면 즉시 청소
                return;
            }

            m_Spawned.RemoveAll(m => m == null || m.Deleted || !m.Alive || (m is BaseCreature bc && (bc.Controlled || bc.IsStabled)));

            var chunkInfo = EcoGridDatabase.GetChunkAt(Map, X, Y);
            if (!chunkInfo.IsValid) return;

            int maxPop = Math.Clamp(chunkInfo.Data.TanCap / 50, 0, 20);
            
            ResourceKey woodKey = new(Map.Name, chunkInfo.Data.Code.ToString(), ResourceType.Lumberjacking);
            bool isRecovering = false; 

            // ====================================================================
            // 숲 생태계 로직 (먹이사슬 엔진)
            // ====================================================================
            if (AreaType == EcoAreaType.Forest && ResourceManager.Pools.TryGetValue(woodKey, out var woodPool))
            {
                if (woodPool.MaxCapacity > 0)
                {
                    var herbivores = m_Spawned.OfType<BaseCreature>().Where(m => m is Hind || m is GreatHart || m is Rabbit).ToList();
                    var carnivores = m_Spawned.OfType<BaseCreature>().Where(m => m is TimberWolf || m is GrizzlyBear || m is DireWolf).ToList();

                    int herbivoreConsumption = herbivores.Count * (woodPool.MaxCapacity / 200); 
                    int newWoodCapacity = woodPool.CurrentCapacity - herbivoreConsumption;

                    if (newWoodCapacity > 0)
                    {
                        woodPool.CurrentCapacity = newWoodCapacity;
                        foreach (var h in herbivores) h.Hunger = 100000; 
                    }
                    else
                    {
                        woodPool.CurrentCapacity = 0;
                        woodPool.DepletionCooldown = DateTime.Now.AddHours(2.0); 
                        Console.WriteLine($"[생태계] {ZoneId}의 숲이 황폐화되었습니다.");
                        
                        foreach (var h in herbivores) 
                        {
                            h.Hunger -= 40000; 
                            if (h.Hunger <= 0) h.Hunger = 1; 
                        }
                    }

                    var activeCarnivores = carnivores.Where(c => !IsSafeFromPredation(c)).ToList();
                    var activeHerbivores = herbivores.Where(h => !IsSafeFromPredation(h)).ToList();

                    foreach (var c in carnivores)
                    {
                        if (IsSafeFromPredation(c)) continue; 

                        if (activeHerbivores.Count > 0)
                        {
                            Mobile prey = activeHerbivores[0];
                            
                            Effects.SendLocationEffect(prey.Location, prey.Map, 0x3728, 10, 10); 
                            Effects.PlaySound(prey.Location, prey.Map, 0x133); 
                            
                            prey.Delete(); 
                            activeHerbivores.RemoveAt(0);
                            herbivores.Remove(prey as BaseCreature);
                            m_Spawned.Remove(prey);
                            
                            c.Hunger = 100000; 
                        }
                        else
                        {
                            c.Hunger -= 40000;
                            if (c.Hunger <= 0) c.Hunger = 1; 
                            
                            if (c.Hunger < 20000 && Utility.RandomDouble() < 0.5)
                            {
                                c.Delete();
                                carnivores.Remove(c);
                                m_Spawned.Remove(c);
                            }
                        }
                    }

                    if (herbivores.Count > 0 && carnivores.Count == 0)
                    {
                        maxPop += 5; 
                    }

                    double woodRatio = (double)woodPool.CurrentCapacity / woodPool.MaxCapacity;
                    maxPop = (int)(maxPop * woodRatio); 
                    if (woodRatio <= 0.5) isRecovering = true; 
                }
            }

            // ====================================================================
            // 일반/공통 자율 생태계 유지 로직
            // ====================================================================
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