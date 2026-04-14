using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Items;

namespace Server.Misc
{
    public static class TownSocietyEngine
    {
        public static void ProcessEveningSocialTick(TownEconomy town) 
        { 
            if (town?.Citizens == null) return;
            
            // 🌟 [추가] 1. 야망이 끓어오르는 가문의 파티 기획 및 사치품 쇼핑
            PlanSocialEvents(town); 

            UpdateSocialStatus(town); 
            ProcessMatching(town); 
            ProcessSocialAmbition(town); 
        }

        public static void ProcessDeepNightLifeCycleTick(TownEconomy town) 
        {
            if (town?.Citizens == null) return;
            ProcessIndependence(town); 
            UpdateFamilies(town); 
            ProcessPhysicalHousingAndInvestment(town); 
            
            var expiredCitizens = town.Citizens.Where(c => c.IsExpired || c.IsStarving || c.IsDehydrated).ToList();
            foreach (var c in expiredCitizens) PerformInheritance(c, town);
        }

        // ====================================================================
        // 🌟 [신규 기획] 사교 파티 및 연회 (명예 펌핑 이벤트)
        // ====================================================================
        private static void PlanSocialEvents(TownEconomy town)
        {
            if (town.Houses == null) return;

            foreach (var house in town.Houses.Where(h => h.IsActive))
            {
                // 1. 이벤트 쿨타임 체크 (현실 시간 기준 2시간 = 게임 시간 약 2달)
                if ((DateTime.Now - house.LastSocialEventTime).TotalHours < 2.0) continue;

                // 2. 개최 조건: 야망이 높거나, 잉여 자본이 많거나, 현재 명성이 깎일 위기일 때
                bool needsFameBoost = house.Families.Any(f => f.IsActive && f.Father != null && f.Father.Fame < GetRequiredFameScore(f.Father.RankLevel));
                bool hasAmbition = house.HousingAmbition > 50;
                bool isRich = house.TotalWealth > 20000;

                if (needsFameBoost || hasAmbition || isRich)
                {
                    // 3. 신분(Rank)에 따른 이벤트 규모 결정
                    bool isNoble = house.PrimaryRank >= NobilityRank.Baron;
                    int eventCost = isNoble ? 10000 : 3000; // 연회는 1만골드, 파티는 3천골드 예산
                    
                    if (house.TotalWealth >= eventCost)
                    {
                        // 예산을 할당하고 파티 준비 모드 돌입
                        house.TotalWealth -= eventCost;
                        ExecuteEventShopping(house, town, isNoble, eventCost);
                    }
                }
            }
        }

        private static void ExecuteEventShopping(VirtualHouse house, TownEconomy town, bool isBanquet, int budget)
        {
            // 4. 이벤트 종류에 따른 쇼핑 리스트 구성
            // 일반 파티(Party): 치즈피자, 통닭, 와인, 폭죽 등
            // 귀족 연회(Banquet): 통돼지구이, 3단 케이크, 최고급 샴페인, 은접시 세트 등
            List<(Type ItemType, int Qty, int EstPrice)> shoppingList = isBanquet ? 
            [
                (typeof(RoastPig), 2, 500),         // 통돼지 바베큐
                (typeof(ThreeTieredCake), 1, 1000), // 3단 케이크
                (typeof(BeverageBottle), 20, 50),   // 고급 와인/샴페인 대체
                (typeof(Plate), 5, 200),            // 은접시 대체 (서버 내 Plate 사용)
                (typeof(Candelabra), 2, 300)        // 촛대 장식
            ] : 
            [
                (typeof(CheesePizza), 5, 100),
                (typeof(CookedBird), 5, 100),
                (typeof(Pitcher), 10, 20),
                (typeof(Candle), 10, 10)
            ];

            int totalSpent = 0;
            var agent = house.Families.FirstOrDefault(f => f.IsActive && f.Father != null)?.Father;
            if (agent == null) return;

            // 잠시 가장의 주머니에 예산을 넣어줌 (쇼핑 엔진 활용을 위해)
            agent.Gold += budget; 

            // 5. 무자비한 사재기 실행
            foreach (var req in shoppingList)
            {
                var result = VirtualTradeSystem.ExecutePurchase(agent, town, req.ItemType, req.EstPrice, req.Qty);
                if (result.Success) totalSpent += result.Spent;
            }

            // 남은 예산은 다시 가문 금고로 반환
            int change = agent.Gold;
            agent.Gold = 0; 
            house.TotalWealth += change; 

            // 6. 성공적으로 파티 물품을 샀다면 이벤트 개최 선언!
            if (totalSpent > (budget * 0.3)) // 예산의 30% 이상 물건을 구했다면 파티 강행
            {
                house.IsHostingEventTonight = true;
                house.LastSocialEventTime = DateTime.Now;
                
                // 보상 세팅: 연회는 +10점, 파티는 +5점 
                house.EventFameBonus = isBanquet ? 10 : 5;
                
                // 물품 소모 처리 (파티가 끝났으므로 샀던 물건들을 가문 창고에서 삭제 = 유저 경제에서 증발)
                foreach (var req in shoppingList)
                {
                    house.AlterWarehouseItem(req.ItemType, -req.Qty); // 샀던 만큼 다시 삭제 (먹어 치움)
                }

                string eventName = isBanquet ? "대연회(Banquet)" : "사교 파티(Party)";
                Console.WriteLine($"[SocialEvent] '{house.HouseName}' 가문이 {totalSpent}gp를 들여 {eventName}를 성대하게 개최했습니다!");
            }
            else
            {
                // 물건을 못 구해서 파티 취소 시 페널티
                house.HousingAmbition = Math.Max(0, house.HousingAmbition - 10);
                agent.Stress = Math.Min(100, agent.Stress + 20);
                Console.WriteLine($"[SocialEvent] '{house.HouseName}' 가문이 파티를 열려 했으나, 마을 상단에 최고급 식재료가 부족하여 취소되었습니다.");
            }
        }

        // ====================================================================
        // 🌟 [최적화 완료] 사회적 갈등 
        // ====================================================================
        private static void ProcessSocialAmbition(TownEconomy town)
        {
            if (town.Houses == null) return;

            var activeHouses = town.Houses.Where(h => h.IsActive).ToList();
            if (activeHouses.Count == 0) return;

            var tierCache = new Dictionary<VirtualHouse, int>();
            foreach (var h in activeHouses)
                tierCache[h] = GetHouseTier(h.MultiID);

            var validRivals = activeHouses
                .Where(h => h.EstateSign != null && !h.HouseName.Contains("kairence", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var house in activeHouses)
            {
                int currentTier = tierCache[house];

                if (house.TotalWealth > (currentTier + 1) * 150000)
                {
                    house.HousingAmbition = Math.Min(100, house.HousingAmbition + 2);
                }

                foreach (var other in validRivals)
                {
                    if (other == house) continue;

                    if (tierCache[other] > currentTier)
                    {
                        string rivalName = other.HouseName;
                        if (!house.Grudges.ContainsKey(rivalName)) house.Grudges[rivalName] = 0;
                        house.Grudges[rivalName] += 1;
                    }
                }
            }
        }

        private static void RegisterGrudgeAt(TownEconomy town, VirtualHouse seeker, int x, int y)
        {
            var chunk = VirtualHousingRegistry.Chunks.FirstOrDefault(c => c.Facet == town.Facet && c.Bounds.Contains(new Point2D(x, y)));
            if (chunk == null) return;

            var occupier = chunk.OccupiedLots.FirstOrDefault(l => l.Footprint.Contains(new Point2D(x, y)));
            if (occupier != null && occupier.HouseName != seeker.HouseName)
            {
                if (occupier.HouseName.Contains("kairence", StringComparison.OrdinalIgnoreCase)) return;

                if (!seeker.Grudges.ContainsKey(occupier.HouseName)) seeker.Grudges[occupier.HouseName] = 0;
                seeker.Grudges[occupier.HouseName] += 15; 
                
                if (seeker.Grudges[occupier.HouseName] > 100)
                    Console.WriteLine($"[Rivalry] '{seeker.HouseName}' 가문이 '{occupier.HouseName}' 가문을 주적으로 선포했습니다!");
            }
        }

       // ====================================================================
        // 💣 [물리적 구역 철거 패치] 장부 직접 조회 및 예외 없이 무조건 폭파
        // ====================================================================
        public static void DemolishEstateArea(VirtualHouse house, TownEconomy town)
        {
            if (house == null) return;

            VirtualEstateSign targetSign = house.EstateSign as VirtualEstateSign;

            // 🌟 [안전 패치 1] EstateSign 변수가 비어있다면, 월드 전체를 뒤져서라도 이 가문의 간판을 찾아냅니다.
            if (targetSign == null || targetSign.Deleted)
            {
                targetSign = World.Items.Values.OfType<VirtualEstateSign>()
                            .FirstOrDefault(s => s.HouseData == house || s.HouseName == $"{house.HouseName}의 가택");
            }

            if (targetSign != null && !targetSign.Deleted)
            {
                Console.WriteLine($"[Demolish] '{house.HouseName}' 가문의 실제 건축물을 철거합니다.");
                targetSign.DestroyEstate(); 
                house.EstateSign = null; 
            }
            else
            {
                Console.WriteLine($"[Demolish] '{house.HouseName}'의 건축물이 이미 월드에 없습니다. 장부만 정리합니다.");
            }

            // 🌟 [안전 패치 2] 청크 구역(Lots)에서 이 가문이 점유하던 땅을 완전히 해제합니다.
            foreach (var c in VirtualHousingRegistry.Chunks)
            {
                if (c.Facet == town.Facet)
                {
                    c.OccupiedLots.RemoveAll(l => l.HouseName == house.HouseName);
                }
            }
        }

        // ====================================================================
        // 🏠 하우징 및 건축 시스템
        // ====================================================================
        private static void ProcessPhysicalHousingAndInvestment(TownEconomy town)
        {
            if (town.Houses == null) return;

            string tName = town.TownName.ToLower();

            // 🌟 [기획 추가] 야망이 없고 집도 짓지 않는 4개의 로컬 마을 명시적 차단
            bool isHousingBanned = tName.Contains("papua") || 
                                   tName.Contains("delucia") || 
                                   tName.Contains("magincia") || 
                                   tName.Contains("sea market");

            string townGrade = TownNumber.GetGrade(town.TownID);
            int attemptLimit = townGrade switch { "S" => 20, "A" => 15, "B" => 10, "C" => 6, _ => 4 };
            int maxConcurrent = townGrade switch { "S" => 12, "A" => 8, "B" => 5, "C" => 3, _ => 2 };
            
            int currentBuilding = town.Houses.Count(h => h.EstateSign != null && !h.EstateSign.IsConstructionFinished);
            
            if (currentBuilding >= maxConcurrent) return;

            var candidates = town.Houses
                .Where(h => h.IsActive && h.TotalWealth >= 35000 && (h.EstateSign == null || h.HousingAmbition >= 100))
                .OrderByDescending(h => 
                {
                    var head = h.Families.FirstOrDefault(f => f.IsActive && f.Father != null)?.Father;
                    NobilityRank rank = head?.RankLevel ?? h.PrimaryRank;
                    double priorityBonus = rank switch { NobilityRank.Commoner => 50000.0, NobilityRank.Knight => 20000.0, _ => 0.0 };
                    return h.TotalWealth + priorityBonus + (h.HousingAmbition * 1000);
                }).Take(attemptLimit).ToList();
            
            int builtCount = 0;
            foreach (var house in candidates) 
            {
                if (currentBuilding + builtCount >= maxConcurrent) break;
                if (!isHousingBanned)
                {
                    var head = house.Families.FirstOrDefault(f => f.IsActive && f.Father != null)?.Father;
                    NobilityRank currentRank = head?.RankLevel ?? house.PrimaryRank;

                    if (house.EstateSign != null)
                    {
                        DemolishEstateArea(house, town);
                        Console.WriteLine($"[Upgrade] '{house.HouseName}' 가문이 더 큰 집을 짓기 위해 기존 집을 철거했습니다!");
                    }

                    var (success, builtMultiID) = StartNewConstruction(town, house, currentRank);
                    if (success) 
                    { 
                        int cost = VirtualEstateSystem.GetBaseMultiPrice(builtMultiID); 
                        house.TotalWealth -= cost; 
                        house.HousingAmbition = 0; 
                        town.Wealth += (int)(cost * 0.1); 
                        builtCount++; 
                    }
                }
            }
        }

        private static (bool Success, int MultiID) StartNewConstruction(TownEconomy town, VirtualHouse house, NobilityRank rank)
        {
            for (int downgrade = 0; downgrade <= 4; downgrade++)
            {
                var (multiID, finalTier) = DetermineHouseType(town, house, rank, downgrade); 
                
                int reqW = 5; 
                int reqH = 5; 
                MultiComponentList mcl = null;

                if (multiID > 0)
                {
                    mcl = MultiData.GetComponents(multiID);
                    if (mcl == null || mcl.List.Length == 0) continue;
                    reqW = mcl.Width + 2; 
                    reqH = mcl.Height + 2;
                }
                else if (finalTier > 0) continue; 

                var (success, chunk, targetSpace) = VirtualHousingRegistry.GetAndLockBestFreeSpace(town.Facet, reqW, reqH, house.HouseName, town.TownID, rank);

                if (success)
                {
                    ExecuteConstructionAt(town, house, targetSpace.X, targetSpace.Y, multiID, mcl, chunk);
                    return (true, multiID);
                }
                else
                {
                    RegisterGrudgeAt(town, house, targetSpace.X, targetSpace.Y);
                }
            }

            return (false, 0);
        }

        // ====================================================================
        // 🏠 물리적 건축 실행 
        // ====================================================================
        private static bool ExecuteConstructionAt(TownEconomy town, VirtualHouse house, int startX, int startY, int targetMultiID, MultiComponentList mcl, EcoGridChunk chunk)
        {
            int buildX = startX + 1;
            int buildY = startY + 1;
            int buildZ = 0; // Z축 초기화

            if (mcl != null)
            {
                // Multi 데이터의 오프셋을 보정하여 실제 배치될 원점(좌상단) 계산
                buildX -= mcl.Min.X;
                buildY -= mcl.Min.Y;

                // 🌟 [핵심 패치] 좌상단(NW)이 아닌 남쪽(South) 계단 입구를 기준으로 Z축 측정!
                // 건물의 가장 남쪽 끝(mcl.Max.Y)과 가로 중앙 부분을 입구로 간주합니다.
                int entranceX = buildX + ((mcl.Max.X + mcl.Min.X) / 2);
                int entranceY = buildY + mcl.Max.Y; 

                // 입구 타일의 지형 높이를 가져와서 집 전체의 베이스 Z축으로 설정합니다.
                buildZ = town.Facet.Tiles.GetLandTile(entranceX, entranceY).Z;
            }
            else
            {
                // 텐트 같은 소형 구조물은 기존처럼 해당 타일 높이 사용
                buildZ = town.Facet.Tiles.GetLandTile(buildX, buildY).Z;
            }

            Point3D buildLoc = new Point3D(buildX, buildY, buildZ);

            house.ZoneID = chunk.ZoneID;

            // 간판(Sign)은 입구 근처에 잘 보이도록 배치 (Z축도 입구 기준이므로 예쁘게 박힙니다)
            Point3D signLoc = (targetMultiID == 0) ? new Point3D(buildLoc.X, buildLoc.Y + 2, buildLoc.Z) : buildLoc;
            var sign = new VirtualEstateSign($"{house.HouseName}의 가택", house, town);
            sign.MoveToWorld(signLoc, town.Facet);
            house.EstateSign = sign;

            Console.WriteLine($"[Housing] {house.HouseName} (Tier {(targetMultiID == 0 ? 0 : "1+")}): {town.Facet.Name} {buildLoc}");

            house.MultiID = targetMultiID; 
            house.UpdateCapacity();

            if (mcl == null || targetMultiID == 0)
            {
                var blueprint = CustomBlueprintManager.TentBlueprint;
                foreach (var tileData in blueprint)
                {
                    int rawID = tileData.ItemID;
                    if ((rawID & 0x3FFF) != 0x0001) 
                    {
                        Static newTile = new Static(rawID) { Movable = false };
                        newTile.MoveToWorld(new Point3D(buildLoc.X + tileData.X, buildLoc.Y + tileData.Y, buildLoc.Z + tileData.Z), town.Facet);
                        sign.AttachedTiles.Add(newTile);
                    }
                }
                
                sign.IsConstructionFinished = true;
                sign.Visible = true;
                sign.ItemID = 0x0BD2; 
            }
            else
            {
                var filteredBlueprint = new List<MultiTileEntry>();
                List<LockedDoor> spawnedDoors = [];
                
                foreach (var c in mcl.List)
                {
                    int itemID = c.m_ItemID & TileData.MaxItemValue;
                    if ((TileData.ItemTable[itemID].Flags & TileFlag.Door) != 0)
                    {
                        // 1. 문 생성
                        LockedDoor door = new LockedDoor(itemID, 50) { Offset = GetDoorOffset(itemID) };
                        door.MoveToWorld(new Point3D(buildLoc.X + c.m_OffsetX, buildLoc.Y + c.m_OffsetY, buildLoc.Z + c.m_OffsetZ), town.Facet);
                        
                        // 🌟 [핵심 패치] 생성된 문을 간판(sign)의 AttachedDoors 리스트에 즉시 등록!!
                        if (sign != null)
                        {
                            sign.AttachedDoors.Add(door);
                        }

                        spawnedDoors.Add(door);
                    }
                    else filteredBlueprint.Add(c);
                }
                for (int i = 0; i < spawnedDoors.Count; i++)
                    for (int j = i + 1; j < spawnedDoors.Count; j++)
                        if (Utility.InRange(spawnedDoors[i].Location, spawnedDoors[j].Location, 1))
                        { spawnedDoors[i].Link = spawnedDoors[j]; spawnedDoors[j].Link = spawnedDoors[i]; }
                
                ConstructionStarter.StartFromMulti(sign, [.. filteredBlueprint]);
            }

            return true;
        }

        private static Point3D GetDoorOffset(int itemID) { int id = itemID & TileData.MaxItemValue; int facing = ((id / 2) - 2) % 8; if (facing < 0) facing += 8; return facing switch { 0 => new Point3D(-1, 1, 0), 1 => new Point3D(1, 1, 0), 2 => new Point3D(-1, 0, 0), 3 => new Point3D(1, -1, 0), 4 => new Point3D(1, 1, 0), 5 => new Point3D(1, -1, 0), 6 => new Point3D(0, 0, 0), 7 => new Point3D(0, -1, 0), _ => new Point3D(-1, 1, 0) }; }
        
        private static int GetHouseTier(int multiID)
        {
            if (multiID == 0) return 0;
            return VirtualEstateSystem.GetBaseMultiPrice(multiID) switch { < 50000 => 1, < 150000 => 2, < 400000 => 3, _ => 4 };
        }

        private static (int MultiID, int Tier) DetermineHouseType(TownEconomy town, VirtualHouse house, NobilityRank rank, int downgradeOffset = 0) 
        { 
            int groupID = ((int)house.PrimaryJob / 100) * 100; 
            int targetTier = house.TotalWealth >= 900000 ? 4 : house.TotalWealth >= 180000 ? 3 : house.TotalWealth >= 90000 ? 2 : 1; 
            
            targetTier = Math.Max(0, targetTier - downgradeOffset);

            int maxTier = rank switch { NobilityRank.Commoner => 2, NobilityRank.Knight or NobilityRank.SubBaronet or NobilityRank.Baronet or NobilityRank.SubBaron => 3, _ => 4 }; 
            int finalTier = Math.Min(targetTier, maxTier); 
            
            if (house.TotalWealth < 5000) finalTier = 0; 
            if (finalTier == 0) return (0, 0); 
            
            int multiID = finalTier switch { 
                1 => groupID switch { 100 or 800 => SelectRandom(0x006E, 0x006A), 200 or 300 or 900 => SelectRandom(0x006C, 0x0064), 400 or 700 => SelectRandom(0x00A0, 0x00A2), 500 or 1000 => 0x0068, _ => 0x0064 }, 
                2 => groupID switch { 100 or 800 => 0x009A, 300 or 900 or 1100 => SelectRandom(0x0098, 0x0074), 400 or 700 or 500 or 1000 => SelectRandom(0x009E, 0x009C), 200 => SelectRandom(0x00A0, 0x00A2), _ => 0x0074 }, 
                3 => groupID switch { 100 or 800 => 0x008C, 200 or 300 or 900 => SelectRandom(0x0074, 0x0078), 400 or 700 or 500 or 1000 => SelectRandom(0x0096, 0x0076), _ => 0x0074 }, 
                4 => groupID switch { 400 or 700 or 900 or 1000 => 0x007C, 200 or 500 => 0x007E, 1100 or 300 => 0x007A, _ => 0x007E }, 
                _ => 0 
            }; 
            return (multiID, finalTier); 
        }

        private static int SelectRandom(params int[] ids) => ids[Utility.Random(ids.Length)];
        private static void UpdateSocialStatus(TownEconomy town) { if (town.Houses == null) return; foreach (var house in town.Houses) { if (house.Families == null) continue; foreach (var family in house.Families) { if (family == null || !family.IsActive) continue; ProcessAgentStatus(family.Father); ProcessAgentStatus(family.Mother); if (family.Children != null) foreach (var child in family.Children.Where(c => c != null && !c.IsChild)) ProcessAgentStatus(child); } ApplyGossip(house); } }
        
        // 🌟 [교체] 틱 낭비 없이 캐싱된 점수만 읽어와서 명성을 +-5점으로 조율 (연회 보너스 추가)
        private static void ProcessAgentStatus(VirtualCitizen agent) 
        { 
            if (agent == null) return; 

            if (agent.House != null)
            {
                int targetScore = GetRequiredFameScore(agent.RankLevel);
                int diff = agent.House.CurrentFameScore - targetScore;

                int fameChange = 0;
                if (diff >= 0) 
                {
                    fameChange = Math.Min(5, 1 + (diff / 10)); 
                }
                else 
                {
                    fameChange = Math.Max(-5, diff); 
                }

                // 🌟 [3단계 기획] 어젯밤 파티/연회를 열었다면 묻지도 따지지도 않고 명성 폭등!
                if (agent.House.IsHostingEventTonight)
                {
                    fameChange += agent.House.EventFameBonus;
                    agent.House.IsHostingEventTonight = false; // 보상 수령 후 플래그 초기화
                    agent.House.EventFameBonus = 0;
                    
                    // 파티를 열었으니 스트레스도 대폭 감소
                    agent.Stress = Math.Max(0, agent.Stress - 30);
                    agent.Satisfaction = 100;
                }

                agent.Fame += fameChange;
                if (agent.Fame < 0) agent.Fame = 0; // 명성은 0 이하로 떨어지지 않음
            }

            CheckRankTransition(agent); 
        }

        // 🌟 [추가] 계급별로 집에 갖춰야 할 최소 명예 점수 목표치
        private static int GetRequiredFameScore(NobilityRank rank) => rank switch 
        { 
            NobilityRank.Commoner => 5,        // 평민: 나무그릇 몇 개면 충분
            NobilityRank.Knight => 15,         // 기사: 퓨터 잔과 기본 식기
            NobilityRank.SubBaronet => 30,     // 준남작
            NobilityRank.Baronet => 50,        // 남작: 화려한 은접시 세트 필요
            NobilityRank.SubBaron => 80, 
            NobilityRank.Baron => 120, 
            NobilityRank.Viscount => 180, 
            NobilityRank.Count => 250, 
            NobilityRank.Marquis => 400,       // 후작: 집안이 금은보화 예술품으로 도배되어야 함
            _ => 0 
        };

        private static void ApplyGossip(VirtualHouse house) { if (house.Families == null || house.Families.Count == 0) return; int fameChange = Utility.RandomMinMax(-50, 50); if (house.Families[0].Father != null && house.Families[0].Father.Karma > 5000 && Utility.RandomDouble() > 0.4) fameChange = Math.Abs(fameChange); foreach (var family in house.Families) { if (family == null || !family.IsActive) continue; if (family.Father != null) { family.Father.Fame += (fameChange / 2); CheckRankTransition(family.Father); } if (family.Mother != null) { family.Mother.Fame += (fameChange / 2); CheckRankTransition(family.Mother); } } }
        private static void CheckRankTransition(VirtualCitizen agent) { int fame = agent.Fame; NobilityRank currentRank = agent.RankLevel; if (currentRank < NobilityRank.Marquis && fame >= GetRequiredFame(currentRank + 1)) { agent.RankLevel = currentRank + 1; agent.Satisfaction = Math.Min(100, agent.Satisfaction + 30); if (agent.House != null) agent.House.Prestige += 20; } else if (currentRank > NobilityRank.Commoner && fame < GetRequiredFame(currentRank) - 1000) { agent.RankLevel = currentRank - 1; agent.Satisfaction = Math.Max(0, agent.Satisfaction - 40); if (agent.House != null) agent.House.Prestige = Math.Max(0, agent.House.Prestige - 15); } }
        private static int GetRequiredFame(NobilityRank rank) => rank switch { NobilityRank.Knight => 3000, NobilityRank.SubBaronet => 6500, NobilityRank.Baronet => 10000, NobilityRank.SubBaron => 14000, NobilityRank.Baron => 18500, NobilityRank.Viscount => 23000, NobilityRank.Count => 27500, NobilityRank.Marquis => 29500, _ => 0 };
        
        private static void ProcessIndependence(TownEconomy town) { 
            if (town.Houses == null) return; 
            foreach (var house in town.Houses.ToList()) { 
                var profile = house.GetHousingProfile();
                var newFamilies = new List<FamilyUnit>(); 
                
                foreach (var family in house.Families.ToList()) { 
                    if (!family.IsActive) continue; 
                    var adults = family.Children.Where(c => c.Age >= 20.0).ToList(); 
                    foreach (var adult in adults) { 
                        family.Children.Remove(adult); 
                        long fund = (long)(family.SharedWealth * 0.15); 
                        family.SharedWealth -= fund; 
                        long tax = (long)(fund * 0.3); 
                        town.Wealth += tax; 
                        adult.Gold += (int)(fund - tax); 
                        
                        var sf = new FamilyUnit(adult, null) { ParentFamily = family }; 
                        adult.Family = sf; 
                        
                        if (house.Families.Count + newFamilies.Count < profile.MaxFamilies)
                        {
                            newFamilies.Add(sf); 
                        }
                        else
                        {
                            var newHouse = new VirtualHouse($"{adult.Name}의 피난처", NobilityRank.Commoner);
                            newHouse.Families.Add(sf);
                            town.Houses.Add(newHouse);
                        }
                    } 
                } 
                house.Families.AddRange(newFamilies); 
            } 
        }

        // ====================================================================
        // 🌟 [매칭 시스템]
        // ====================================================================
        private static void ProcessMatching(TownEconomy town) { 
            var males = town.Citizens.Where(c => c.Gender == Gender.Male && IsSingle(c) && IsEligible(c)).ToList(); 
            var females = town.Citizens.Where(c => c.Gender == Gender.Female && IsSingle(c) && IsEligible(c)).ToList(); 
            
            var femaleAges = females.ToDictionary(f => f, f => f.Age);

            foreach (var m in males) { 
                double mAge = m.Age;
                double limit = m.MaxLifespan.TotalMinutes / VirtualCitizen.GameYearMinutes * 0.2;

                var bride = females.FirstOrDefault(f => Math.Abs(mAge - femaleAges[f]) <= limit); 
                if (bride != null) { 
                    FormFamily(m, bride, town); 
                    females.Remove(bride); 
                    femaleAges.Remove(bride); 
                } 
            } 
        }
        
        private static bool IsSingle(VirtualCitizen c) => c.Family != null && ((c.Gender == Gender.Male && c.Family.Mother == null) || (c.Gender == Gender.Female && c.Family.Father == null));
        private static bool IsEligible(VirtualCitizen c) => (c.Age / (c.MaxLifespan.TotalMinutes / VirtualCitizen.GameYearMinutes)) is >= 0.2 and <= 0.7;
        
        private static void FormFamily(VirtualCitizen m, VirtualCitizen f, TownEconomy town) { 
            var mFam = m.Family; 
            var fFam = f.Family; 
            if (mFam == null || fFam == null) return; 

            VirtualHouse mHouse = m.House;
            VirtualHouse fHouse = f.House;
            VirtualHouse targetHouse = mHouse;
            VirtualHouse houseToSell = null;

            bool mIsOwner = mHouse != null && mHouse.Families.FirstOrDefault() == mFam;
            bool fIsOwner = fHouse != null && fHouse.Families.FirstOrDefault() == fFam;

            if (mIsOwner && fIsOwner && mHouse != fHouse)
            {
                int mTier = GetHouseTier(mHouse.MultiID);
                int fTier = GetHouseTier(fHouse.MultiID);

                if (fTier > mTier || (fTier == mTier && fHouse.Prestige > mHouse.Prestige))
                {
                    targetHouse = fHouse;
                    houseToSell = mHouse;
                }
                else
                {
                    targetHouse = mHouse;
                    houseToSell = fHouse;
                }
                
                int refund = (int)(VirtualEstateSystem.GetBaseMultiPrice(houseToSell.MultiID) * 0.7);
                targetHouse.TotalWealth += refund;
                targetHouse.HousingAmbition = 100; 
                
                DemolishEstateArea(houseToSell, town);
                town.Houses.Remove(houseToSell);

                Console.WriteLine($"[Merger] {houseToSell.HouseName}이(가) 매각/철거되고 {targetHouse.HouseName}에 합병되었습니다!");
            }
            else if (fHouse != null && mHouse == null) 
            {
                targetHouse = fHouse;
            }

            mFam.Mother = f; 
            mFam.SharedWealth += fFam.SharedWealth; 
            fFam.IsActive = false; 
            f.Family = mFam; 
            TransferWealth(m, mFam); 
            TransferWealth(f, mFam); 
            
            if (targetHouse != null) { 
                targetHouse.Prestige += 10; 
                f.House = targetHouse; 
                m.House = targetHouse; 
                if (!targetHouse.Families.Contains(mFam)) targetHouse.Families.Add(mFam);
            } 
        }

        private static void UpdateFamilies(TownEconomy town) { 
            if (town.Houses == null) return; 
            
            foreach (var house in town.Houses.ToList()) { 
                var profile = house.GetHousingProfile();
                var ownerFamily = house.Families.FirstOrDefault(f => f.IsActive);

                foreach (var family in house.Families.ToList()) { 
                    if (family == null || !family.IsActive) continue; 

                    if (family != ownerFamily && ownerFamily != null)
                    {
                        long rentToPay = profile.RentFee;
                        if (family.ParentFamily == ownerFamily) rentToPay = 0; 

                        if (rentToPay > 0)
                        {
                            if (family.SharedWealth >= rentToPay)
                            {
                                family.SharedWealth -= rentToPay;
                                ownerFamily.SharedWealth += rentToPay;
                                house.HousingAmbition += 5; 
                            }
                            else
                            {
                                house.Families.Remove(family);
                                
                                var newHouse = new VirtualHouse($"{family.Father?.Name ?? "무명"}의 피난처", NobilityRank.Commoner);
                                newHouse.Families.Add(family);
                                town.Houses.Add(newHouse);
                                continue; 
                            }
                        }
                    }

                    if (family.Children.Count < profile.MaxChildren && family.Father != null && family.Mother != null) { 
                        double ageRatio = Math.Max(family.Father.Age / (family.Father.MaxLifespan.TotalMinutes / VirtualCitizen.GameYearMinutes), family.Mother.Age / (family.Mother.MaxLifespan.TotalMinutes / VirtualCitizen.GameYearMinutes)); 
                        if (ageRatio <= 0.7 && Utility.RandomDouble() < (0.05 * (1 + (house.Prestige * 0.001)) * (ageRatio > 0.5 ? 0.2 : 1.0) * Math.Min(5.0, 1.0 + ((double)family.SharedWealth / 20000.0)))) 
                            CreateChild(family, house, town); 
                    } 
                    
                    TransferWealth(family.Father, family); 
                    TransferWealth(family.Mother, family); 
                    
                    if (family.SharedWealth > 100) { 
                        long tribute = family.SharedWealth - 100; 
                        family.SharedWealth = 100; 
                        house.TotalWealth += tribute; 
                    } 
                } 
            } 
        }

        private static void TransferWealth(VirtualCitizen c, FamilyUnit f) { if (c != null && c.Gold > 100) { f.SharedWealth += (c.Gold - 100); c.Gold = 100; } }
        private static void CreateChild(FamilyUnit family, VirtualHouse house, TownEconomy town) { var child = new VirtualCitizen(Utility.RandomBool() ? family.Father.JobClass : family.Mother.JobClass, NobilityRank.Commoner, 100) { House = house, Family = family, TargetRegionName = town.TownName }; ApplyGenetics(child, family.Father, family.Mother); family.Children.Add(child); town.Citizens.Add(child); }
        
        // ====================================================================
        // 💀 [사망 및 상속 패치] 멸문(가문원 0명) 시 가옥 완전 철거
        // ====================================================================
        public static void PerformInheritance(VirtualCitizen deceased, TownEconomy town) 
        { 
            // 1. 사망 기록 남기기
            if (deceased.House != null) 
                deceased.House.AncestorRecords.Add(new AncestorRecord(deceased.Name, deceased.JobClass, deceased.RankLevel, (int)deceased.Age, deceased.IsStarving ? "아사" : deceased.IsDehydrated ? "탈수" : "노환")); 
            
            // 2. 유산 분배 (마을 30%, 상속자 70%)
            int legacy = (int)(deceased.Gold * 0.7); 
            town.Wealth += (int)(deceased.Gold * 0.3); 

            // 3. 기존 사망자 제거
            town.Citizens.Remove(deceased); 

            // 🌟 [핵심 패치] 가문에 남은 생존자가 있는지 확인합니다.
            VirtualHouse h = deceased.House;
            if (h != null)
            {
                // 이 사망자가 속했던 가족 그룹(FamilyUnit)에서 이 사람을 지웁니다.
                if (deceased.Family != null)
                {
                    if (deceased.Family.Father == deceased) deceased.Family.Father = null;
                    if (deceased.Family.Mother == deceased) deceased.Family.Mother = null;
                    deceased.Family.Children.Remove(deceased);
                    
                    // 가족 단위에 아무도 안 남았다면 가족 그룹 해체
                    if (deceased.Family.Father == null && deceased.Family.Mother == null && deceased.Family.Children.Count == 0)
                    {
                        deceased.Family.IsActive = false;
                        h.Families.Remove(deceased.Family);
                    }
                }

                // 가문 전체를 통틀어 살아있는(Active) 사람이 1명도 없다면? -> 멸문!
                bool hasSurvivors = h.Families.Any(f => f.IsActive && (f.Father != null || f.Mother != null || f.Children.Count > 0));

                if (!hasSurvivors)
                {
                    // 멸문 시: 가문 장부 삭제 및 월드의 실제 집 강제 철거
                    Console.WriteLine($"[Inheritance] '{h.HouseName}' 가문의 마지막 생존자가 사망하여 멸문되었습니다. 가옥을 철거합니다.");
                    DemolishEstateArea(h, town);
                    town.Houses.Remove(h);
                }
                else
                {
                    // 생존자가 있거나 새로운 상속자(Child)를 생성하여 집을 물려줌
                    var child = new VirtualCitizen(deceased.JobClass, NobilityRank.Commoner, 70) 
                    { 
                        Gold = 5000 + legacy, 
                        BirthTime = DateTime.Now, 
                        MaxLifespan = TimeSpan.FromMinutes(Utility.RandomMinMax(60, 90) * VirtualCitizen.GameYearMinutes), 
                        TargetRegionName = town.TownName 
                    }; 

                    child.House = h; 
                    var sf = new FamilyUnit(child, null); 
                    child.Family = sf; 
                    h.Families.Add(sf); 
                    
                    town.Citizens.Add(child); 
                    Console.WriteLine($"[Inheritance] '{h.HouseName}' 가문의 유산을 새로운 후계자 {child.Name}이(가) 상속받았습니다.");
                }
            }
        }
        
        private static void ApplyGenetics(VirtualCitizen child, VirtualCitizen p1, VirtualCitizen p2) { foreach (SkillName sk in Enum.GetValues(typeof(SkillName))) { double v1 = p1.Skills.ContainsKey(sk) ? p1.Skills[sk] : 0.0; double v2 = p2 != null && p2.Skills.ContainsKey(sk) ? p2.Skills[sk] : 0.0; child.Skills[sk] = ((v1 + v2) / 2.0) * Utility.RandomMinMax(30, 50) / 100.0; } }

        public static int GetHouseMaxTiles(NobilityRank rank) => rank switch { NobilityRank.Commoner => 10, NobilityRank.Knight => 50, NobilityRank.SubBaronet => 100, NobilityRank.Baronet => 150, NobilityRank.SubBaron => 200, NobilityRank.Baron => 300, NobilityRank.Viscount => 500, NobilityRank.Count => 700, NobilityRank.Marquis => 1000, _ => 10 };
    }
}