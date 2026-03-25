using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;
using Server.Items;
using Server.Regions;
using Server.Mobiles; // 해양 몬스터 스폰을 위해 추가

namespace Server.Misc
{
    // [핵심] Key에 ResourceType을 추가해서 광물/벌목/낚시 완벽 분리
    public readonly record struct ResourceKey(string MapName, string RegionName, ResourceType Type);

    public static class ResourceManager
    {
        public static Dictionary<ResourceKey, ResourcePool> Pools { get; private set; } = new();
        public static Dictionary<ResourceType, List<ResourceDef>> Defs { get; private set; } = new();
        private static ResourceTimer m_Timer;

        // [신규] 낚시를 위한 3단계 물 깊이 구분
        public enum WaterType
        {
            River,   // 강/호수 (128x128) - 안전함
            Coastal, // 얕은 바다/해안가 (192x192) - 얕은 위험
            Ocean    // 깊은 바다/심해 (256x256) - 매우 위험 (크라켄 등)
        }

        private readonly record struct TierTemplate(double Min, double Max, int Weight);
        private static readonly TierTemplate[] m_Tiers = {
            new(0.0, 50.0, 500), new(20.0, 70.0, 200), new(40.0, 90.0, 100),
            new(60.0, 110.0, 50), new(80.0, 130.0, 25), new(100.0, 150.0, 10), new(120.0, 170.0, 5)
        };

        public static void Configure()
        {
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
        }

        public static void Initialize()
        {
            SetupDefinitions();
            
            // 1. 육지 자원(광산/숲) 세팅
            RegisterAllRegions(); 

            // ★ 2. [추가된 부분] 전 세계 바다/강 낚시터 미리 세팅 (Pre-load)
            RegisterAllWater();
			
			RegisterFarmingRegions(); // ★ 이 호출이 반드시 있어야 농사가 뜹니다.

            // 3. 1분 타이머 작동
            m_Timer = new ResourceTimer();
            m_Timer.Start();
        }

        private static void SetupDefinitions()
        {
            Defs[ResourceType.Mining] = BuildDefs(new[] { typeof(IronOre), typeof(CopperOre), typeof(BronzeOre), typeof(GoldOre), typeof(AgapiteOre), typeof(VeriteOre), typeof(ValoriteOre) }, LocationType.Mine, 4);
            Defs[ResourceType.Lumberjacking] = BuildDefs(new[] { typeof(Log), typeof(OakLog), typeof(AshLog), typeof(YewLog), typeof(HeartwoodLog), typeof(BloodwoodLog), typeof(FrostwoodLog) }, LocationType.Forest, 4);
            Defs[ResourceType.Fishing] = BuildDefs(new[] { typeof(Fish), typeof(Fish), typeof(Fish), typeof(Fish), typeof(Fish), typeof(BigFish), typeof(BigFish) }, LocationType.DeepSea, 5);
        }

        private static List<ResourceDef> BuildDefs(Type[] types, LocationType specialLoc, int specialStartIndex)
        {
            List<ResourceDef> list = new();
            for (int i = 0; i < types.Length && i < m_Tiers.Length; i++)
                list.Add(new ResourceDef(types[i], m_Tiers[i].Min, m_Tiers[i].Max, i >= specialStartIndex ? specialLoc : LocationType.Normal, m_Tiers[i].Weight));
            return list;
        }

        public static void RegisterPool(string map, string region, ResourceType type, LocationType loc, int max, int size)
        {
            ResourceKey key = new(map, region, type);
            if (!Pools.ContainsKey(key)) 
                Pools[key] = new ResourcePool(map, region, type, loc, max, size);
        }

        // ===================================================================================
        // [리스폰 코어 1] 육지 자원(광산/숲) 자동 등록 및 던전 차단
        // ===================================================================================
        private static void RegisterAllRegions()
        {
            foreach (Region r in Region.Regions)
            {
                if (r.Map == null || r.Map == Map.Internal || string.IsNullOrEmpty(r.Name)) continue;

                string lowerName = r.Name.ToLower();

				// [중요] 농경지 관련 리전은 여기서 절대 등록하지 않음 (생태계/던전 노드 생성 방지)
				if (lowerName.Contains("farm") || lowerName.Contains("field") || lowerName.Contains("wheat") || lowerName.Contains("garden"))
					continue;

                // ★ 던전 구역 완벽 차단! (자원 생성 안 함)
                bool isDungeon = r.IsPartOf(typeof(DungeonRegion)) || 
                                 lowerName.Contains("dungeon") || 
                                 (DungeonManager.Zones != null && DungeonManager.Zones.Keys.Any(k => k.Contains(r.Name) || r.Name.Contains(k)));

                if (isDungeon) continue;

                LocationType locType = LocationType.Normal;

                // 생태계 매니저 연동 확인
                bool isEcosystem = false;
                if (EcosystemManager.Zones != null)
                    isEcosystem = EcosystemManager.Zones.Keys.Any(k => k.Contains(r.Name) || r.Name.Contains(k));

                if (lowerName.Contains("cave") || lowerName.Contains("mine"))
                    locType = LocationType.Mine;
                else if (lowerName.Contains("forest") || lowerName.Contains("woods") || lowerName.Contains("jungle") || isEcosystem)
                    locType = LocationType.Forest;
                
                int baseCapacity = 1000;
                int sizeCategory = 1;
                
                if (r.Area != null && r.Area.Length > 0)
                {
                    int areaSize = r.Area[0].Width * r.Area[0].Height;
                    if (areaSize > 100000) { baseCapacity = 4000; sizeCategory = 3; } 
                    else if (areaSize > 20000) { baseCapacity = 2000; sizeCategory = 2; } 
                }

                // 기후에 맞춰서 한 가지 종류만 생성 (하드코어)
                if (locType == LocationType.Mine)
                    RegisterPool(r.Map.Name, r.Name, ResourceType.Mining, locType, baseCapacity, sizeCategory);
                else if (locType == LocationType.Forest)
                    RegisterPool(r.Map.Name, r.Name, ResourceType.Lumberjacking, locType, baseCapacity, sizeCategory);
            }
        }
		// ===================================================================================
        // [누락 복구] 물 타일 ID 데이터 (울온 엔진 기본값)
        // ===================================================================================
        private static readonly int[] m_WaterTiles = new int[]
        {
            0x00A8, 0x00AB,
            0x0136, 0x0137
        };

        private static readonly int[] m_UndeepWaterTiles = new int[]
        {
            0x1797, 0x179C
        };

        // ===================================================================================
        // [누락 복구] 심해(Deep Water) 타일 판독 함수
        // ===================================================================================
        public static bool ValidateDeepWater(Map map, int x, int y)
        {
            // 맵 경계선(에러) 방어 로직 추가
            if (x < 0 || x >= map.Width || y < 0 || y >= map.Height) return false;

            int tileID = map.Tiles.GetLandTile(x, y).ID;
            bool water = false;

            for (int i = 0; !water && i < m_WaterTiles.Length; i += 2)
                water = (tileID >= m_WaterTiles[i] && tileID <= m_WaterTiles[i + 1]);

            return water;
        }

        // ===================================================================================
        // [누락 복구] 스캐닝 기법을 활용한 3단계 수질(WaterType) 자동 판독 함수
        // ===================================================================================
        public static WaterType GetWaterCategory(Map map, int x, int y)
        {
            // 1. 현재 찌를 던진 곳이 깊은 바다라면 무조건 대양(Ocean)
            if (ValidateDeepWater(map, x, y)) 
            {
                return WaterType.Ocean;
            }

            // 2. 얕은 물일 경우, 반경 12칸을 스캔해서 바다와 이어져 있는지 확인
            int scanRange = 12; 
            for (int dx = -scanRange; dx <= scanRange; dx++)
            {
                for (int dy = -scanRange; dy <= scanRange; dy++)
                {
                    // 주변에 단 한 칸이라도 깊은 바다(Ocean) 타일이 존재한다면 해안가(Coastal)
                    if (ValidateDeepWater(map, x + dx, y + dy))
                    {
                        return WaterType.Coastal;
                    }
                }
            }

            // 3. 주변을 다 뒤져도 깊은 바다가 없다면 100% 내륙의 강/호수(River)
            return WaterType.River;
        }
        // ===================================================================================
        // [리스폰 코어 2] 낚시 청크 3단 분할 이름 생성기
        // ===================================================================================
        public static string GetFishingChunk(Point3D loc, WaterType waterType)
        {
            if (waterType == WaterType.Ocean) return $"Ocean_{loc.X / 256}_{loc.Y / 256}";
            if (waterType == WaterType.Coastal) return $"Coastal_{loc.X / 192}_{loc.Y / 192}";
            return $"River_{loc.X / 128}_{loc.Y / 128}";
        }
		// ===================================================================================
        // [신규] 서버 시작 시 전 세계의 물 타일을 스캔하여 낚시 청크를 미리 생성합니다.
        // ===================================================================================
        private static void RegisterAllWater()
        {
            Map[] maps = { Map.Felucca, Map.Trammel, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur };

            foreach (Map map in maps)
            {
                if (map == null || map == Map.Internal) continue;

                // 64칸 단위로 점프하며 맵 전체를 스캔 (성능 최적화 및 겹침 방지)
                for (int x = 0; x < map.Width; x += 64)
                {
                    for (int y = 0; y < map.Height; y += 64)
                    {
                        int tileID = map.Tiles.GetLandTile(x, y).ID;
                        
                        // 대략적으로 물 타일인지 확인 (깊은 물 or 얕은 물)
                        bool isDeep = ValidateDeepWater(map, x, y);
                        bool isShallow = (tileID >= 0x1797 && tileID <= 0x179C); 

                        if (isDeep || isShallow)
                        {
                            Point3D loc = new Point3D(x, y, 0);

                            // ★ 던전 지역이면 생성 안 하고 패스! (이름이 없는 null 구역 에러 완벽 방어)
                            Region r = Region.Find(loc, map);
                            if (r != null)
                            {
                                // 1. 타입 자체가 던전인 경우
                                if (r.IsPartOf(typeof(DungeonRegion))) continue;

                                // 2. 구역에 이름(r.Name)이 있을 때만 이름 검사 실행
                                if (!string.IsNullOrEmpty(r.Name))
                                {
                                    string lowerName = r.Name.ToLower();
                                    if (lowerName.Contains("dungeon") || 
                                       (DungeonManager.Zones != null && DungeonManager.Zones.Keys.Any(k => k != null && (k.Contains(r.Name) || r.Name.Contains(k)))))
                                    {
                                        continue;
                                    }
                                }
                            }

                            // 주변을 스캔하여 수질(Ocean, Coastal, River) 판독
                            WaterType wType = GetWaterCategory(map, x, y);
                            string chunkName = GetFishingChunk(loc, wType);

                            ResourceKey key = new ResourceKey(map.Name, chunkName, ResourceType.Fishing);

                            // 아직 생성되지 않은 청크라면 풀(Pool) 등록!
                            if (!Pools.ContainsKey(key))
                            {
                                LocationType locType = wType == WaterType.Ocean ? LocationType.DeepSea : LocationType.Normal;
                                int maxCap = wType == WaterType.Ocean ? 4000 : wType == WaterType.Coastal ? 2000 : 1000;
                                int sizeCat = wType == WaterType.Ocean ? 3 : wType == WaterType.Coastal ? 2 : 1;

                                Pools[key] = new ResourcePool(map.Name, chunkName, ResourceType.Fishing, locType, maxCap, sizeCat);
                            }
                        }
                    }
                }
            }
            Console.WriteLine($"[ResourceManager]: 전 세계 바다 및 강 낚시터(Chunk) 스캔 완료!");
        }
        // ===================================================================================
        // [수정] 낚시 시도 (미리 생성된 자원에서 빼오기만 함)
        // ===================================================================================
        public static Type TryGatherFishing(Mobile from, Map map, Point3D loc, double skill)
        {
            if (map == null || map == Map.Internal) return null;

			// 던전 안의 늪/지하수 낚시 원천 차단 (null 에러 방어)
            Region r = Region.Find(loc, map);
            if (r != null)
            {
                bool isDungeon = r.IsPartOf(typeof(DungeonRegion));
                
                if (!isDungeon && !string.IsNullOrEmpty(r.Name))
                {
                    string lowerName = r.Name.ToLower();
                    isDungeon = lowerName.Contains("dungeon") || 
                                (DungeonManager.Zones != null && DungeonManager.Zones.Keys.Any(k => k != null && (k.Contains(r.Name) || r.Name.Contains(k))));
                }

                if (isDungeon)
                {
                    if (from != null) from.SendMessage(33, "이곳의 물은 너무 탁하고 오염되어 물고기가 살 수 없습니다.");
                    return null; 
                }
            }

            // 유저가 찌를 던진 곳의 수질(WaterType)을 판독
            WaterType wType = GetWaterCategory(map, loc.X, loc.Y);
            string chunkName = GetFishingChunk(loc, wType);
            ResourceKey key = new ResourceKey(map.Name, chunkName, ResourceType.Fishing);

            // 미리 깔아둔 낚시터(Pool)가 있는지 확인
            if (!Pools.TryGetValue(key, out ResourcePool pool))
            {
                // 풀이 없다면? 던전 지하수이거나 물이 아닌 곳임.
                if (from != null) from.SendMessage(33, "이곳은 물고기가 살 수 없는 환경입니다.");
                return null;
            }

            if (!pool.CanGather()) 
            { 
                if (from != null) from.SendMessage("이 구역의 물고기가 일시적으로 씨가 말랐습니다."); 
                return null; 
            }

            var possible = pool.AvailableResources.Where(kvp => kvp.Value > 0 && skill >= GetDef(ResourceType.Fishing, kvp.Key)?.MinSkill).ToList();
            if (possible.Count == 0) 
            { 
                if (from != null) from.SendMessage("당신의 낚시 실력으로 잡을 수 있는 물고기가 없습니다."); 
                return null; 
            }

            int total = possible.Sum(x => x.Value);
            int roll = Utility.Random(total);
            int cur = 0;
            foreach (var kvp in possible) 
            { 
                cur += kvp.Value; 
                if (roll < cur) 
                { 
                    pool.ConsumeResource(kvp.Key);
                    return kvp.Key; 
                } 
            }
            return null;
        }

		// [ResourceManager.cs] 시스템 전용 밭 타일 판정 (0x150 ~ 0x15C)
		private static bool IsSystemField(int id)
		{
			// 유저가 나중에 사용할 흙바닥(0x3~0x6)은 여기서 제외됩니다.
			return (id == 9 || ( id >= 0x150 && id <= 0x15C)); 
		}

		public static void RegisterFarmingRegions()
        {
            foreach (Map map in Map.AllMaps)
            {
                if (map == null || map == Map.Internal) continue;

                foreach (Region r in map.Regions.Values)
                {
                    if (string.IsNullOrEmpty(r.Name)) continue;

                    string name = r.Name.ToLower();
                    if (name.Contains("farm") || name.Contains("field") || name.Contains("wheatfield") || name.Contains("garden"))
                    {
                        int furrowCount = 0;
                        foreach (Rectangle3D rect in r.Area) 
                        {
                            for (int x = rect.Start.X; x < rect.End.X; x++)
                            {
                                for (int y = rect.Start.Y; y < rect.End.Y; y++)
                                {
                                    int tileID = map.Tiles.GetLandTile(x, y).ID & 0x3FFF;
                                    if (IsSystemField(tileID)) furrowCount++;
                                }
                            }
                        }

                        if (furrowCount > 0)
                        {
                            // ★ [완벽 복구] 유저님이 맞춰두신 황금 밸런스 (/ 4) 부활
                            int capacity = Math.Max(1, furrowCount / 4); 
                            int sizeCategory = capacity > 30 ? 2 : 1;

                            ResourceKey key = new ResourceKey(map.Name, r.Name, ResourceType.Farming);

                            if (!Pools.ContainsKey(key))
                            {
                                Pools[key] = new ResourcePool(map.Name, r.Name, ResourceType.Farming, LocationType.Farm_Remote, capacity, sizeCategory);
                            }
                            else
                            {
                                // ★ [버그 방어] 만약 세이브 파일 로드 등으로 1000개가 들어있어도, 
                                // 무조건 타일 개수 기반(/ 4)으로 강제 덮어씌웁니다!
                                Pools[key].MaxCapacity = capacity;
                                if (Pools[key].CurrentCapacity > capacity) 
                                    Pools[key].CurrentCapacity = capacity;
                            }
                        }
                    }
                }
            }
        }

		// [추가] 누락된 좌표 추출 헬퍼 (CS0103 해결)
		private static Point3D GetRandomPointInRegion(Region reg)
		{
			if (reg == null || reg.Area == null || reg.Area.Length == 0) return Point3D.Zero;

			// 무작위 구역 선택
			Rectangle3D rect = reg.Area[Utility.Random(reg.Area.Length)];
			
			int x = Utility.RandomMinMax(rect.Start.X, rect.End.X);
			int y = Utility.RandomMinMax(rect.Start.Y, rect.End.Y);
			int z = reg.Map.GetAverageZ(x, y);

			return new Point3D(x, y, z);
		}
		// [ResourceManager.cs] 야생 작물 스폰 가능 여부 체크
		private static bool CanSpawnAt(Map map, Point3D loc)
		{
			// 1. 해당 타일 ID가 시스템 전용 밭이랑(Furrow)인지 확인
			int tileID = map.Tiles.GetLandTile(loc.X, loc.Y).ID & 0x3FFF;
			if (!IsSystemField(tileID)) return false; // 흙바닥(Dirt)이면 스폰하지 않음

			// 2. 이미 작물이 심겨 있는지 체크 (겹침 방지)
			IPooledEnumerable eable = map.GetItemsInRange(loc, 0);
			foreach (Item item in eable)
			{
				if (item is BaseFarmItem) 
				{ 
					eable.Free(); 
					return false; 
				}
			}
			eable.Free();

			return true;
		}
		public static Region GetRegionByName(string name, Map map)
		{
			if (map == null || map == Map.Internal || string.IsNullOrEmpty(name))
				return null;

			// 운영자님 엔진의 Dictionary 구조에 맞춰 Values를 순회합니다.
			foreach (Region r in map.Regions.Values) 
			{
				if (r.Name == name)
					return r;
			}

			return null;
		}

		// [추가] 특정 리전 내 아이템 목록 스캔용 헬퍼 (Line 472 에러 해결)
		public static List<Item> GetItemsInRegion(Map map, Region reg)
		{
			List<Item> items = new List<Item>();
			if (map == null || reg == null) return items;

			// 월드 내 모든 아이템 중 맵과 리전 범위가 일치하는 것만 추출
			foreach (Item item in World.Items.Values)
			{
				if (item.Map == map && reg.Contains(item.Location))
					items.Add(item);
			}
			return items;
		}

		// [수정] 야생 작물 스폰 시 작물 선정 (양배추 고정 탈피!)
		private static Type GetRandomCropForRegion(string regionName)
		{
			string n = regionName.ToLower();

			// 1. 이름이 명시된 전용 리전 처리 (확정적 스폰)
			if (n.Contains("wheat"))   return typeof(Wheat);
			if (n.Contains("carrot"))  return typeof(Carrot);
			if (n.Contains("corn"))    return typeof(Corn);
			if (n.Contains("onion"))   return typeof(Onion);
			if (n.Contains("lettuce")) return typeof(Lettuce);
			if (n.Contains("cotton"))  return typeof(Cotton);
			if (n.Contains("pumpkin")) return typeof(Pumpkin);
			if (n.Contains("turnip"))  return typeof(Turnip);

			// 2. [Camping 컨셉] 야생 구역 (숲, 정글, 일반 정원)
			// 이 구역은 '밭' 타일이 없어도 스폰을 허용하도록 CanSpawnAt과 연동되어야 합니다.
			if (n.Contains("forest") || n.Contains("woods") || n.Contains("jungle") || n.Contains("garden"))
			{
				// 야생에서는 시약의 가치를 높이기 위해 시약 40% : 버섯 60% 비율로 설정
				if (Utility.RandomDouble() < 0.40)
				{
					// 야생에서 주로 발견되는 4대 약초형 시약
					Type[] wildReagents = { 
						typeof(Ginseng), typeof(Garlic), 
						typeof(MandrakeRoot), typeof(Nightshade) 
					};
					return wildReagents[Utility.Random(wildReagents.Length)];
				}
				return typeof(Mushrooms1); 
			}

			// 3. [기본 농경지] 이름에 특정 작물이 없는 일반 Farm / Field
			// 농작물(70%)과 농가 근처에서 자라는 흔한 시약(30%)을 혼합 배정합니다.
			if (Utility.RandomDouble() < 0.30)
			{
				// 농장 근처에서 흔히 수집 가능한 시약 (인삼, 마늘)
				return Utility.RandomBool() ? typeof(Ginseng) : typeof(Garlic);
			}

			// 4. 최종 기본값 (식량 중심의 랜덤 배정)
			Type[] defaultCrops = { 
				typeof(Cabbage), typeof(Carrot), typeof(Onion), 
				typeof(Lettuce), typeof(Wheat), typeof(Pumpkin) 
			};
			
			return defaultCrops[Utility.Random(defaultCrops.Length)];
		}
        // ===================================================================================
        // [수정] 1분마다 회복 및 해양 생태계 (단계별 포식자 스폰 및 개체수 조절)
        // ===================================================================================
		private class ResourceTimer : Timer 
        {
            private int m_TotalTicks = 0; // 틱 카운터는 클래스 변수로 유지

            public ResourceTimer() : base(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0)) 
            { 
                Priority = TimerPriority.OneMinute; 
            }
            
            protected override void OnTick() 
            { 
                m_TotalTicks++; // 1분마다 1회 증가

                foreach (var pool in Pools.Values) 
                {
                    Map map = Map.Parse(pool.MapName);
                    Region reg = GetRegionByName(pool.RegionName, map);
                    int regenAmount = 20; 

                    // --- [1] 벌목: 생태계 활성도에 따른 보너스 ---
                    if (pool.Type == ResourceType.Lumberjacking)
                    {
                        if (EcosystemManager.Zones != null)
                        {
                            var ecoZone = EcosystemManager.Zones.Values.FirstOrDefault(z => z.ZoneId.Contains(pool.RegionName) || pool.RegionName.Contains(z.ZoneId));
                            if (ecoZone != null) regenAmount += ecoZone.SpeciesInfo.Values.Sum(s => s.ActiveAnimals.Count); 
                        }
                    }
                    // --- [2] 낚시: 포식자 스폰 및 심해 보너스 ---
                    else if (pool.Type == ResourceType.Fishing)
                    {
                        if (pool.LocType == LocationType.DeepSea) regenAmount += 40; 
                        double fishRatio = pool.MaxCapacity > 0 ? (double)pool.CurrentCapacity / pool.MaxCapacity : 0;

                        if (fishRatio >= 0.5 && !pool.RegionName.StartsWith("River"))
                        {
                            if (Utility.RandomDouble() < 0.05) SpawnTieredPredator(pool, fishRatio);
                        }
                    }
                    // --- [3] 농사/축산: 가축 증식, 식물 성장 및 교배 ---
                    else if (pool.Type == ResourceType.Farming)
                    {
                        if (reg != null)
                        {
                            // A. 가축 증식 체크 (60분마다)
                            if (m_TotalTicks % 60 == 0)
                            {
                                FarmingSystem.HandleLivestockBreeding(pool.RegionName, map, reg);
                            }

                            // B. 식물 업데이트 (성장 단계 및 동적 교배 체크)
                            UpdateRegionCrops(pool, map, reg); 

                            // C. 야생 작물 자연 스폰 (10% 확률)
                            if (!pool.RegionName.StartsWith("PrivateFarm_") && pool.CurrentCapacity < pool.MaxCapacity && Utility.RandomDouble() < 0.1)
                            {
                                SpawnWildCrop(pool);
                            }
                        }
                        continue; // 농사는 별도의 regenAmount 합산을 하지 않음 (개체수 기반)
                    } 

                    // 일반 자원(광산, 벌목, 낚시) 수량 회복
                    pool.CurrentCapacity = Math.Min(pool.MaxCapacity, pool.CurrentCapacity + regenAmount);
                }
            }

            // --- [헬퍼 메서드: 야생 작물 생성] ---
            private static bool SpawnWildCrop(ResourcePool pool)
            {
                Map map = Map.Parse(pool.MapName);
                Region reg = GetRegionByName(pool.RegionName, map);

                if (reg == null || map == null || map == Map.Internal) return false;

                for (int i = 0; i < 10; i++)
                {
                    Point3D loc = GetRandomPointInRegion(reg); 
                    
                    if (CanSpawnAt(map, loc)) 
                    {
                        Type cropType = GetRandomCropForRegion(pool.RegionName); 
                        BaseFarmItem wildCrop = new BaseFarmItem(null, cropType); 
                        
                        wildCrop.MoveToWorld(loc, map);
                        // 수량은 UpdateRegionCrops에서 물리적으로 다시 계산하므로 여기선 직접 증가 생략 가능
                        return true; 
                    }
                }
                return false;
            }
            
            // --- [헬퍼 메서드: 구역 내 작물 전수 조사] ---
            private void UpdateRegionCrops(ResourcePool pool, Map map, Region reg)
            {
                int physicalCount = 0;
                foreach (Item item in World.Items.Values)
                {
                    if (item is BaseFarmItem crop && crop.Map == map && reg.Contains(item.Location))
                    {
                        physicalCount++;
                        crop.CheckGrowth(); // 성장 단계 체크
                        
                        // [기획 반영] 식물 교배 체크 호출 (현재 틱 전달)
                        FarmingSystem.HandlePlantBreeding(crop, m_TotalTicks);
                    }
                }
                pool.CurrentCapacity = physicalCount; // 실제 월드에 존재하는 개수와 Gump 숫자 동기화
            }

            // --- [헬퍼 메서드: 포식자 스폰 로직] ---
            private void SpawnTieredPredator(ResourcePool pool, double fishRatio)
            {
                if (!pool.RegionName.Contains("_") || pool.Type != ResourceType.Fishing) return;
                try
                {
                    string[] parts = pool.RegionName.Split('_');
                    if (parts.Length < 3) return;

                    string waterType = parts[0];
                    int size = waterType == "Ocean" ? 256 : waterType == "Coastal" ? 192 : 128;
                    int cx = int.Parse(parts[1]) * size + (size / 2);
                    int cy = int.Parse(parts[2]) * size + (size / 2);
                    Map map = Map.Parse(pool.MapName);

                    if (map == null || map == Map.Internal) return;

                    EcoZone ecoZone;
                    if (!EcosystemManager.Zones.TryGetValue(pool.RegionName, out ecoZone))
                    {
                        ecoZone = new EcoZone(pool.RegionName, map);
                        EcosystemManager.Zones[pool.RegionName] = ecoZone;
                    }

                    int serpents = 0, deepSerpents = 0, krakens = 0;
                    List<Mobile> currentMonsters = new List<Mobile>();

                    IPooledEnumerable eable = map.GetMobilesInRange(new Point3D(cx, cy, 0), size / 2);
                    foreach (Mobile m in eable)
                    {
                        if (!m.Alive) continue;
                        if (m is SeaSerpent) { serpents++; currentMonsters.Add(m); }
                        else if (m is DeepSeaSerpent) { deepSerpents++; currentMonsters.Add(m); }
                        else if (m is Kraken) { krakens++; currentMonsters.Add(m); }
                    }
                    eable.Free();

                    int totalMonsters = serpents + deepSerpents + krakens;
                    Type spawnType = null;

                    if (waterType == "Ocean")
                    {
                        if (fishRatio >= 0.9 && krakens == 0) 
                        {
                            spawnType = typeof(Kraken);
                            if (totalMonsters >= 3 && currentMonsters.Count > 0)
                            {
                                var weakTarget = currentMonsters.Find(m => m is SeaSerpent) ?? currentMonsters[0];
                                weakTarget.Delete(); 
                            }
                        }
                        else if (fishRatio >= 0.7 && deepSerpents < 2 && krakens == 0) 
                        {
                            spawnType = typeof(DeepSeaSerpent);
                            if (totalMonsters >= 3 && serpents > 0)
                            {
                                var weakTarget = currentMonsters.Find(m => m is SeaSerpent);
                                if (weakTarget != null) weakTarget.Delete(); 
                                else return; 
                            }
                            else if (totalMonsters >= 3) return; 
                        }
                        else if (fishRatio >= 0.5 && totalMonsters < 3) 
                        {
                            spawnType = typeof(SeaSerpent);
                        }
                    }
                    else if (waterType == "Coastal") 
                    {
                        if (fishRatio >= 0.7 && totalMonsters < 2) 
                            spawnType = typeof(SeaSerpent);
                    }

                    if (spawnType != null)
                    {
                        BaseCreature monster = (BaseCreature)Activator.CreateInstance(spawnType);
                        Point3D spawnLoc = new Point3D(cx, cy, map.GetAverageZ(cx, cy));
                        monster.MoveToWorld(spawnLoc, map);

                        if (!ecoZone.SpeciesInfo.ContainsKey(spawnType))
                        {
                            ecoZone.AddSpecies(spawnType, 3); 
                        }
                        
                        ecoZone.SpeciesInfo[spawnType].ActiveAnimals.Add(monster);

                        int eatenAmount = pool.MaxCapacity / 5;
                        pool.CurrentCapacity -= eatenAmount;
                        if (pool.CurrentCapacity < 0) pool.CurrentCapacity = 0;
                    }
                }
                catch { }
            }
        }

        public static ResourceDef GetDef(ResourceType type, Type itemType) => Defs.GetValueOrDefault(type)?.FirstOrDefault(d => d.ItemType == itemType);

        // ===================================================================================
        // [저장/로드] 세이브 시스템 연동
        // ===================================================================================
        private static void OnSave(WorldSaveEventArgs e)
        {
            string path = Path.Combine(Core.BaseDirectory, "Saves", "ResourceManagement", "ResourcePools.bin");
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryFileWriter writer = new BinaryFileWriter(stream, true);
                writer.Write(1); // 버전을 1로 세팅 (ResourceType 포함)
                writer.Write(Pools.Count);
                foreach (var kvp in Pools) 
                { 
                    writer.Write(kvp.Key.MapName); 
                    writer.Write(kvp.Key.RegionName); 
                    writer.Write((int)kvp.Key.Type);
                    kvp.Value.Serialize(writer); 
                }
                writer.Close();
            }
        }

        private static void OnLoad()
        {
            string path = Path.Combine(Core.BaseDirectory, "Saves", "ResourceManagement", "ResourcePools.bin");
            if (!File.Exists(path)) return;

            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                BinaryFileReader reader = new BinaryFileReader(new BinaryReader(stream));
                int version = reader.ReadInt();
                int count = reader.ReadInt();
                
                for (int i = 0; i < count; i++) 
                {
                    string mName = reader.ReadString();
                    string rName = reader.ReadString();
                    
                    ResourceType type = ResourceType.Mining;
                    if (version >= 1) type = (ResourceType)reader.ReadInt();

                    ResourceKey key = new(mName, rName, type);
                    
                    if (Pools.TryGetValue(key, out var pool)) 
                    {
                        pool.Deserialize(reader);
                    }
                    else 
                    {
                        // ★ [수정] 낚시(지연 생성) 풀을 로드할 때, 이름표를 보고 원래 크기와 기후를 복원!
                        LocationType locType = LocationType.Normal;
                        int maxCap = 1000;
                        int sizeCat = 1;

                        if (type == ResourceType.Fishing)
                        {
                            if (rName.StartsWith("Ocean")) { locType = LocationType.DeepSea; maxCap = 4000; sizeCat = 3; }
                            else if (rName.StartsWith("Coastal")) { locType = LocationType.Normal; maxCap = 2000; sizeCat = 2; }
                            else { locType = LocationType.Normal; maxCap = 1000; sizeCat = 1; } // River
                        }

                        var newPool = new ResourcePool(key.MapName, key.RegionName, type, locType, maxCap, sizeCat);
                        newPool.Deserialize(reader);
                        Pools[key] = newPool;
                    }
                }
                reader.Close();
            }
			RegisterFarmingRegions();
        }
    }
}
