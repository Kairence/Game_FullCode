using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Regions;
using Server.Spells; // CheckMulti(유저 집 판별)용
using Server.Mobiles;

namespace Server.Misc
{
    public enum EcoAreaType { Town, Forest, Hunting, Special }
    public enum EcoClimateType { Temperate, Arctic, Tropical, Desert, Coastal, Swamp, Volcanic, Void }

    public class EcoNode : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public string ZoneId { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public EcoAreaType AreaType { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public EcoClimateType ClimateType { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SpawnRange { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HomeRange { get; set; }

        // 🌟 [통합 핵심] 이 노드가 관리하는 현재 살아있는 동물들
        private List<Mobile> m_Spawned = new();
        private EcoTimer m_Timer;

        [Constructable]
        public EcoNode() : base(0x11EA)
        {
            Movable = false;
            Visible = false;
            Name = "Ecosystem Spawn Node";
            ZoneId = "Unknown";
            AreaType = EcoAreaType.Forest; 
            ClimateType = EcoClimateType.Temperate;
            SpawnRange = 64; // 128x128 타일의 절반 반경
            HomeRange = 80;

            StartTimer();
        }

        public EcoNode(Serial serial) : base(serial) { }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
                from.SendGump(new EcoNodeGump(from, this));
        }

        private void StartTimer()
        {
            m_Timer?.Stop();
            m_Timer = new EcoTimer(this);
            m_Timer.Start();
        }

        public override void OnDelete()
        {
            m_Timer?.Stop();
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

                if (Map.CanSpawnMobile(rx, ry, rz))
                {
                    if (nodeRegion == Region.Find(testLoc, Map))
                    {
                        if (!SpellHelper.CheckMulti(testLoc, Map))
                        {
                            bool isCrowded = false;
                            IPooledEnumerable eable = Map.GetMobilesInRange(testLoc, 1);
                            foreach (Mobile m in eable) { isCrowded = true; break; }
                            eable.Free();

                            if (!isCrowded) return testLoc;
                        }
                    }
                }
            }
            return null;
        }

        // ==============================================================================
        // 🌟 [핵심] 노드 스스로 호흡하며 자원 체크 및 스폰을 관리하는 주기적 타이머
        // ==============================================================================
        public void DoTick()
        {
            if (Map == null || Map == Map.Internal) return;

            // 1. 죽었거나 길들여진(Tamed) 몹 리스트에서 제거
            m_Spawned.RemoveAll(m => m == null || m.Deleted || !m.Alive || (m is BaseCreature bc && (bc.Controlled || bc.IsStabled)));

            var chunkInfo = EcoGridDatabase.GetChunkAt(Map, X, Y);
            if (!chunkInfo.IsValid) return;

            int maxPop = Math.Clamp(chunkInfo.Data.TanCap / 50, 0, 20);

            // 4. [환경 파괴 연동] 벌목량에 따른 한계치 감소
            string gridName = EcoGridDatabase.GetGridRegionName(Map, X / 128, Y / 128, chunkInfo.Data.Code);
            ResourceKey woodKey = new(Map.Name, gridName, ResourceType.Lumberjacking);

            bool isRecovering = false; // 50% 지점 자가 회복 상태 판별

            if (ResourceManager.Pools.ContainsKey(woodKey))
            {
                var woodPool = ResourceManager.Pools[woodKey];
                if (woodPool.MaxCapacity > 0)
                {
                    double woodRatio = (double)woodPool.CurrentCapacity / woodPool.MaxCapacity;
                    maxPop = (int)(maxPop * woodRatio); 
                    if (woodRatio <= 0.5) isRecovering = true; // 50% 이하면 회복 구간 돌입
                }
            }

            // 5. 스폰 진행 (틱당 1~2마리씩 천천히 자연 생성)
            if (m_Spawned.Count < maxPop)
            {
                int spawnCount = Utility.RandomMinMax(1, 2);
                for (int i = 0; i < spawnCount && m_Spawned.Count < maxPop; i++)
                {
                    Type typeToSpawn = EcoSpawnDatabase.GetRandomSpawn(this);
                    if (typeToSpawn == null) continue;

                    // [기획 연동] 회복 구간(50% 이하)일 경우 초식 9 : 육식 1 강제 조정
                    if (isRecovering)
                    {
                        // 아주 단순하게 확률 10% 미만일 때만 기존 스폰 허용, 나머진 토끼 등 초식 강제 주입
                        if (Utility.RandomDouble() > 0.1)
                        {
                            typeToSpawn = typeof(Rabbit); // 초식 시드용 대표 몹 (원하시는 초식 Type으로 변경 가능)
                        }
                    }

                    Point3D? loc = GetValidSpawnLocation();
                    if (loc.HasValue)
                    {
                        Mobile mob = (Mobile)Activator.CreateInstance(typeToSpawn);
                        if (mob is BaseCreature creature)
                        {
                            creature.Home = Location;
                            creature.RangeHome = HomeRange;
                            creature.Hunger = 100000; // 초기 배고픔 세팅
                        }
                        mob.MoveToWorld(loc.Value, Map);
                        m_Spawned.Add(mob);
                    }
                }
            }

            // 6. [기획 연동] 지하 생태계: 광물 100% 충전 시 1% 확률로 오어 엘리멘탈 스폰
            ResourceKey oreKey = new(Map.Name, gridName, ResourceType.Mining);
            if (AreaType != EcoAreaType.Town && ResourceManager.Pools.ContainsKey(oreKey))
            {
                var orePool = ResourceManager.Pools[oreKey];
                // 매장량이 가득 찼고, 노드 내에 현재 엘리멘탈이 없을 경우 1% 굴림
                if (orePool.CurrentCapacity >= orePool.MaxCapacity && Utility.RandomDouble() < 0.01)
                {
                    bool hasElemental = m_Spawned.Any(m => m != null && m.GetType().Name.Contains("OreElemental"));
                    if (!hasElemental)
                    {
                        Point3D? oreLoc = GetValidSpawnLocation();
                        if (oreLoc.HasValue)
                        {
                            // TODO: 실제 시스템에 맞춰 알맞은 등급의 OreElemental Type을 넣어야 합니다.
                            Mobile elemental = (Mobile)Activator.CreateInstance(typeof(EarthElemental)); 
                            if (elemental is BaseCreature bc) { bc.Home = Location; bc.RangeHome = HomeRange; }
                            elemental.MoveToWorld(oreLoc.Value, Map);
                            m_Spawned.Add(elemental);
                        }
                    }
                }
            }
        }

        // ==============================================================================
        // ⏳ 타이머 클래스 (1~5분 주기로 호흡)
        // ==============================================================================
        private class EcoTimer : Timer
        {
            private EcoNode m_Node;
            public EcoTimer(EcoNode node) : base(TimeSpan.FromMinutes(Utility.RandomMinMax(1, 5)), TimeSpan.FromMinutes(Utility.RandomMinMax(2, 5)))
            {
                m_Node = node;
                Priority = TimerPriority.FiveSeconds;
            }

            protected override void OnTick()
            {
                if (m_Node != null && !m_Node.Deleted)
                    m_Node.DoTick();
                else
                    Stop();
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1); // 버전을 1로 올림
            
            writer.Write(ZoneId ?? string.Empty);
            writer.Write((int)AreaType);
            writer.Write((int)ClimateType);
            writer.Write(SpawnRange);
            writer.Write(HomeRange);

            // 🌟 생성된 몹 목록 저장
            m_Spawned.RemoveAll(m => m == null || m.Deleted);
            writer.Write(m_Spawned.Count);
            foreach (var m in m_Spawned) writer.Write(m);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            
            ZoneId = reader.ReadString();
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

            StartTimer();
        }
    }

    // ========================================================================
    // 생태계 노드 세팅 Gump (기존과 동일하게 유지)
    // ========================================================================
    public class EcoNodeGump : Gump
    {
        private readonly EcoNode m_Node;

        public EcoNodeGump(Mobile from, EcoNode node) : base(100, 100)
        {
            m_Node = node;
            from.CloseGump(typeof(EcoNodeGump));

            AddPage(0);
            AddBackground(0, 0, 450, 400, 9270);
            AddHtml(10, 10, 430, 20, "<CENTER>야외 생태계(Ecosystem) 노드 세팅</CENTER>", false, false);

            AddHtml(20, 50, 100, 20, "현재 생태 구역:", false, false);
            AddLabel(120, 50, 68, node.ZoneId);
            
            // 용도(AreaType) 설정
            AddHtml(20, 90, 100, 20, "생태계 용도:", false, false);
            AddRadio(120, 90, 208, 209, node.AreaType == EcoAreaType.Town, 10); AddLabel(145, 90, 0, "마을");
            AddRadio(220, 90, 208, 209, node.AreaType == EcoAreaType.Forest, 11); AddLabel(245, 90, 0, "벌목/숲");
            AddRadio(320, 90, 208, 209, node.AreaType == EcoAreaType.Hunting, 12); AddLabel(345, 90, 0, "사냥터");
            AddRadio(120, 115, 208, 209, node.AreaType == EcoAreaType.Special, 13); AddLabel(145, 115, 0, "특수 구역");

            // 기후(Climate) 설정
            AddHtml(20, 155, 100, 20, "기후 및 환경:", false, false);
            AddRadio(120, 155, 208, 209, node.ClimateType == EcoClimateType.Temperate, 20); AddLabel(145, 155, 0, "일반/온대");
            AddRadio(220, 155, 208, 209, node.ClimateType == EcoClimateType.Arctic, 21); AddLabel(245, 155, 1152, "설원/북극");
            AddRadio(320, 155, 208, 209, node.ClimateType == EcoClimateType.Tropical, 22); AddLabel(345, 155, 68, "열대/정글");
            AddRadio(120, 180, 208, 209, node.ClimateType == EcoClimateType.Desert, 23); AddLabel(145, 180, 53, "사막");
            AddRadio(220, 180, 208, 209, node.ClimateType == EcoClimateType.Coastal, 24); AddLabel(245, 180, 89, "해안가");
            AddRadio(320, 180, 208, 209, node.ClimateType == EcoClimateType.Swamp, 25); AddLabel(345, 180, 167, "늪지대");
            AddRadio(120, 205, 208, 209, node.ClimateType == EcoClimateType.Volcanic, 26); AddLabel(145, 205, 33, "화산/지하");
            AddRadio(220, 205, 208, 209, node.ClimateType == EcoClimateType.Void, 27); AddLabel(245, 205, 275, "공허/TerMur");

            // 반경 설정
            AddHtml(20, 255, 150, 20, "스폰 탐색 반경:", false, false);
            AddBackground(150, 255, 50, 20, 9300);
            AddTextEntry(150, 255, 50, 20, 0, 30, node.SpawnRange.ToString());

            AddHtml(20, 285, 150, 20, "몬스터 배회 반경:", false, false);
            AddBackground(150, 285, 50, 20, 9300);
            AddTextEntry(150, 285, 50, 20, 0, 31, node.HomeRange.ToString());

            AddButton(180, 340, 2128, 2129, 1, GumpButtonType.Reply, 0); // OK
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Node == null || m_Node.Deleted) return;

            if (info.ButtonID == 1) // OK 버튼
            {
                if (int.TryParse(info.GetTextEntry(30)?.Text, out int sRange)) m_Node.SpawnRange = sRange;
                if (int.TryParse(info.GetTextEntry(31)?.Text, out int hRange)) m_Node.HomeRange = hRange;

                foreach (int switchId in info.Switches)
                {
                    if (switchId >= 10 && switchId <= 13) m_Node.AreaType = (EcoAreaType)(switchId - 10);
                    if (switchId >= 20 && switchId <= 27) m_Node.ClimateType = (EcoClimateType)(switchId - 20);
                }

                sender.Mobile.SendMessage(68, "생태계 노드 설정이 직접 저장되었습니다.");
            }
        }
    }
}