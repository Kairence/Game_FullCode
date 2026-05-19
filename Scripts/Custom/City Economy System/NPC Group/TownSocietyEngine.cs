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
            ProcessMigration(town);
            
            var expiredCitizens = town.Citizens.Where(c => c.IsExpired || c.IsStarving || c.IsDehydrated).ToList();
            foreach (var c in expiredCitizens) PerformInheritance(c, town);
        }

        // ====================================================================
        // 🌟 사교 파티 및 연회 (명예 펌핑 이벤트)
        // ====================================================================
        private static void PlanSocialEvents(TownEconomy town)
        {
            if (town.Houses == null) return;

            foreach (var house in town.Houses.Where(h => h.IsActive))
            {
                if ((DateTime.Now - house.LastSocialEventTime).TotalHours < 2.0) continue;

                bool needsFameBoost = house.Families.Any(f => f.IsActive && f.Father != null && f.Father.Fame < GetRequiredFameScore(f.Father.RankLevel));
                bool hasAmbition = house.HousingAmbition > 50;
                bool isRich = house.TotalWealth > 20000;

                if (needsFameBoost || hasAmbition || isRich)
                {
                    bool isNoble = house.PrimaryRank >= NobilityRank.Baron;
                    int eventCost = isNoble ? 10000 : 3000; 
                    
                    if (house.TotalWealth >= eventCost)
                    {
                        house.TotalWealth -= eventCost;
                        ExecuteEventShopping(house, town, isNoble, eventCost);
                    }
                }
            }
        }

        private static void ExecuteEventShopping(VirtualHouse house, TownEconomy town, bool isBanquet, int budget)
        {
            List<(Type ItemType, int Qty, int EstPrice)> shoppingList = isBanquet ? 
            new List<(Type ItemType, int Qty, int EstPrice)>
            {
                (typeof(RoastPig), 2, 500),         
                (typeof(ThreeTieredCake), 1, 1000), 
                (typeof(BeverageBottle), 20, 50),   
                (typeof(Plate), 5, 200),            
                (typeof(Candelabra), 2, 300)        
            } : 
            new List<(Type ItemType, int Qty, int EstPrice)>
            {
                (typeof(CheesePizza), 5, 100),
                (typeof(CookedBird), 5, 100),
                (typeof(Pitcher), 10, 20),
                (typeof(Candle), 10, 10)
            };

            int totalSpent = 0;
            var agent = house.Families.FirstOrDefault(f => f.IsActive && f.Father != null)?.Father;
            if (agent == null) return;

            agent.Gold += budget; 

            foreach (var req in shoppingList)
            {
                var result = VirtualTradeSystem.ExecutePurchase(agent, town, req.ItemType, req.EstPrice, req.Qty);
                if (result.Success) 
                {
                    totalSpent += result.Spent;
                }
                else
                {
                    if (!house.UnfulfilledNeeds.ContainsKey(req.ItemType))
                    {
                        house.UnfulfilledNeeds[req.ItemType] = 0;
                    }
                    house.UnfulfilledNeeds[req.ItemType] += req.Qty;
                }
            }

            int change = agent.Gold;
            agent.Gold = 0; 
            house.TotalWealth += change; 

            if (totalSpent > (budget * 0.3)) 
            {
                house.IsHostingEventTonight = true;
                house.LastSocialEventTime = DateTime.Now;
                house.EventFameBonus = isBanquet ? 10 : 5;
                
                string eventName = isBanquet ? "대연회(Banquet)" : "사교 파티(Party)";
                Console.WriteLine($"[SocialEvent] '{house.HouseName}' 가문이 {totalSpent}gp를 들여 {eventName}를 성대하게 개최했습니다!");
            }
            else
            {
                house.HousingAmbition = Math.Max(0, house.HousingAmbition - 10);
                agent.Stress = Math.Min(100, agent.Stress + 20);
                Console.WriteLine($"[SocialEvent] '{house.HouseName}' 가문이 파티를 열려 했으나, 상단 물품 부족으로 취소되었습니다. (게시판 의뢰용 장부 등록 완료)");
            }
        }

        // ====================================================================
        // 🌟 사회적 갈등 처리
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

        // ====================================================================
        // 💣 물리적 구역 철거 및 텐트(무주택) 페널티 전환
        // ====================================================================
        public static void DemolishEstateArea(VirtualHouse house, TownEconomy town)
        {
            if (house == null) return;

            VirtualEstateSign targetSign = house.EstateSign as VirtualEstateSign;

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

            house.MultiID = 0;
            house.UpdateCapacity();
            // 🌟 텐트라는 단어를 삭제하고 노숙으로 변경
            Console.WriteLine($"[Housing Penalty] '{house.HouseName}' 가문이 길거리에 나앉아 노숙 생활에 돌입합니다. (실면적: 0칸, 모든 공방 가동 중지)");

            foreach (var c in VirtualHousingRegistry.Chunks)
            {
                if (c.Facet == town.Facet)
                {
                    c.OccupiedLots.RemoveAll(l => l.HouseName == house.HouseName);
                }
            }
        }

        // ====================================================================
        // 🏠 하우징, 알박기, 강제 철거 엔진
        // ====================================================================
        public static void ProcessPhysicalHousingAndInvestment(TownEconomy town)
        {
            if (town.Houses == null) return;

            string tName = town.TownName.ToLower();

            bool isHousingBanned = tName.Contains("papua") || 
                                   tName.Contains("delucia") || 
                                   tName.Contains("magincia") || 
                                   tName.Contains("sea market");

            string townGrade = TownNumber.GetGrade(town.TownID);
            int attemptLimit = townGrade switch { "S" => 20, "A" => 15, "B" => 10, "C" => 6, _ => 4 };
            int maxConcurrent = townGrade switch { "S" => 12, "A" => 8, "B" => 5, "C" => 3, _ => 2 };
            
            int currentBuilding = town.Houses.Count(h => h.EstateSign != null && !h.EstateSign.IsConstructionFinished);
            
            if (currentBuilding >= maxConcurrent) return;

            // 🌟 최소 건축 자금을 35000gp(소형 가옥)로 픽스
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
                        Console.WriteLine($"[Upgrade] '{house.HouseName}' 가문이 더 큰 집을 짓기 위해 기존 집을 자진 철거했습니다!");
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
                if (multiID <= 0 || finalTier <= 0) continue; // 🌟 텐트(MultiID 0) 원천 차단
                
                MultiComponentList mcl = MultiData.GetComponents(multiID);
                if (mcl == null || mcl.List.Length == 0) continue;
                
                int reqW = mcl.Width + 2; 
                int reqH = mcl.Height + 2;

                var (success, chunk, targetSpace) = VirtualHousingRegistry.GetAndLockBestFreeSpace(town.Facet, reqW, reqH, house.HouseName, town.TownID, rank);

                if (success)
                {
                    ExecuteConstructionAt(town, house, targetSpace.X, targetSpace.Y, multiID, mcl, chunk);
                    return (true, multiID);
                }
                else if (chunk != null)
                {
                    var occupierLot = chunk.OccupiedLots.FirstOrDefault(l => l.Footprint.Contains(new Point2D(targetSpace.X, targetSpace.Y)));
                    if (occupierLot != null && occupierLot.HouseName != house.HouseName)
                    {
                        if (occupierLot.HouseName.Contains("kairence", StringComparison.OrdinalIgnoreCase)) continue; 

                        VirtualHouse victimHouse = town.Houses.FirstOrDefault(h => h.HouseName == occupierLot.HouseName);
                        if (victimHouse != null && victimHouse.EstateSign != null)
                        {
                            int currentGrudge = house.Grudges.ContainsKey(victimHouse.HouseName) ? house.Grudges[victimHouse.HouseName] : 0;
                            if (currentGrudge < 0) continue; 

                            NobilityRank victimRank = victimHouse.PrimaryRank;
                            int victimMultiPrice = VirtualEstateSystem.GetBaseMultiPrice(victimHouse.MultiID);
                            int buyoutPrice = victimMultiPrice * 3; 
                            int myBuildCost = VirtualEstateSystem.GetBaseMultiPrice(multiID);

                            if ((int)rank >= (int)victimRank + 2 || currentGrudge > 50)
                            {
                                Console.WriteLine($"[Eviction] 권력 남용! '{house.HouseName}'({rank}) 가문이 '{victimHouse.HouseName}'({victimRank})의 부지를 강제로 철거하고 땅을 빼앗았습니다.");
                                DemolishEstateArea(victimHouse, town); 
                                
                                if (!victimHouse.Grudges.ContainsKey(house.HouseName)) victimHouse.Grudges[house.HouseName] = 0;
                                victimHouse.Grudges[house.HouseName] += 100;

                                ExecuteConstructionAt(town, house, targetSpace.X, targetSpace.Y, multiID, mcl, chunk);
                                return (true, multiID);
                            }
                            else if (house.TotalWealth >= (buyoutPrice + myBuildCost))
                            {
                                Console.WriteLine($"[Buyout] 부동산 알박기 매입! '{house.HouseName}' 가문이 '{victimHouse.HouseName}' 가문에게 거액의 보상금 {buyoutPrice}gp를 쥐어주고 땅을 매입했습니다.");
                                house.TotalWealth -= buyoutPrice;
                                victimHouse.TotalWealth += buyoutPrice;
                                DemolishEstateArea(victimHouse, town); 

                                if (!house.Grudges.ContainsKey(victimHouse.HouseName)) house.Grudges[victimHouse.HouseName] = 0;
                                house.Grudges[victimHouse.HouseName] += 30; 
                                
                                if (!victimHouse.Grudges.ContainsKey(house.HouseName)) victimHouse.Grudges[house.HouseName] = 0;
                                victimHouse.Grudges[house.HouseName] -= 50; 

                                ExecuteConstructionAt(town, house, targetSpace.X, targetSpace.Y, multiID, mcl, chunk);
                                return (true, multiID);
                            }
                        }
                    }
                }
            }
            return (false, 0);
        }

        // ====================================================================
        // 🏠 물리적 건축 실행 및 실면적 계산
        // ====================================================================
        private static bool ExecuteConstructionAt(TownEconomy town, VirtualHouse house, int startX, int startY, int targetMultiID, MultiComponentList mcl, EcoGridChunk chunk)
        {
            if (targetMultiID <= 0 || mcl == null || mcl.List.Length == 0) return false; // 🌟 텐트 예외 원천 차단

            int buildX = startX + 1 - mcl.Min.X;
            int buildY = startY + 1 - mcl.Min.Y;

            int entranceX = buildX + ((mcl.Max.X + mcl.Min.X) / 2);
            int entranceY = buildY + mcl.Max.Y; 

            int buildZ = town.Facet.Tiles.GetLandTile(entranceX, entranceY).Z;
            Point3D buildLoc = new Point3D(buildX, buildY, buildZ);
            
            house.ZoneID = chunk.ZoneID;
            house.MultiID = targetMultiID; 
            house.UpdateCapacity();

            var sign = new VirtualEstateSign($"{house.HouseName}의 가택", house, town);
            sign.MoveToWorld(buildLoc, town.Facet);
            house.EstateSign = sign;

            int internalArea = Math.Max(0, (mcl.Width - 2) * (mcl.Height - 2)); 
            Console.WriteLine($"[Housing] '{house.HouseName}' (Tier {GetHouseTier(targetMultiID)}): {town.Facet.Name} {buildLoc} / 실면적: {internalArea}칸 확보 완료.");

            var filteredBlueprint = new List<MultiTileEntry>();
            List<LockedDoor> spawnedDoors = [];
            
            foreach (var c in mcl.List)
            {
                int itemID = c.m_ItemID & TileData.MaxItemValue;
                if ((TileData.ItemTable[itemID].Flags & TileFlag.Door) != 0)
                {
                    LockedDoor door = new LockedDoor(itemID, 50) { Offset = GetDoorOffset(itemID) };
                    door.MoveToWorld(new Point3D(buildLoc.X + c.m_OffsetX, buildLoc.Y + c.m_OffsetY, buildLoc.Z + c.m_OffsetZ), town.Facet);
                    
                    if (sign != null) sign.AttachedDoors.Add(door);

                    spawnedDoors.Add(door);
                }
                else filteredBlueprint.Add(c);
            }
            
            for (int i = 0; i < spawnedDoors.Count; i++)
                for (int j = i + 1; j < spawnedDoors.Count; j++)
                    if (Utility.InRange(spawnedDoors[i].Location, spawnedDoors[j].Location, 1))
                    { spawnedDoors[i].Link = spawnedDoors[j]; spawnedDoors[j].Link = spawnedDoors[i]; }
            
            ConstructionStarter.StartFromMulti(sign, [.. filteredBlueprint]);

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
            
            targetTier = Math.Max(1, targetTier - downgradeOffset); // 🌟 최소 티어를 1(소형집)로 고정

            int maxTier = rank switch { NobilityRank.Commoner => 2, NobilityRank.Knight or NobilityRank.SubBaronet or NobilityRank.Baronet or NobilityRank.SubBaron => 3, _ => 4 }; 
            int finalTier = Math.Min(targetTier, maxTier); 
            
            // 🌟 TotalWealth < 5000 일 때 텐트(0티어)를 주던 로직 삭제

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
        
        private static void ProcessAgentStatus(VirtualCitizen agent) 
        { 
            if (agent == null || agent.House == null) return; 

            int dailyDecay = agent.RankLevel switch 
            { 
                NobilityRank.Commoner => 0,        
                NobilityRank.Knight => 10,         
                NobilityRank.SubBaronet => 30,     
                NobilityRank.Baronet => 50,        
                NobilityRank.SubBaron => 100, 
                NobilityRank.Baron => 200, 
                NobilityRank.Viscount => 400, 
                NobilityRank.Count => 800, 
                NobilityRank.Marquis => 1500,       
                _ => 0 
            };

            int defenseScore = agent.House.CurrentFameScore;
            int diff = defenseScore - dailyDecay;

            if (agent.House.IsHostingEventTonight)
            {
                diff += agent.House.EventFameBonus * 10; 
                agent.House.IsHostingEventTonight = false; 
                agent.House.EventFameBonus = 0;
                
                agent.Stress = Math.Max(0, agent.Stress - 30);
                agent.Satisfaction = 100;
            }

            int fameChange = 0;
            if (diff >= 0) 
            {
                fameChange = Math.Min(10, 1 + (diff / 10));
            }
            else 
            {
                fameChange = Math.Max(-1500, diff); 
            }

            agent.Fame += fameChange;
            if (agent.Fame < 0) agent.Fame = 0; 

            CheckRankTransition(agent); 
        }

        private static int GetRequiredFameScore(NobilityRank rank) => rank switch 
        { 
            NobilityRank.Commoner => 5,        
            NobilityRank.Knight => 15,         
            NobilityRank.SubBaronet => 30,     
            NobilityRank.Baronet => 50,        
            NobilityRank.SubBaron => 80, 
            NobilityRank.Baron => 120, 
            NobilityRank.Viscount => 180, 
            NobilityRank.Count => 250, 
            NobilityRank.Marquis => 400,       
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

        private static void ProcessMatching(TownEconomy town) 
        { 
            var males = town.Citizens.Where(c => c.Gender == Gender.Male && IsSingle(c) && IsEligible(c)).ToList(); 
            var females = town.Citizens.Where(c => c.Gender == Gender.Female && IsSingle(c) && IsEligible(c)).ToList(); 
            
            var femaleAges = females.ToDictionary(f => f, f => f.Age);

            foreach (var m in males) 
            { 
                double mAge = m.Age;
                double limit = m.MaxLifespan.TotalMinutes / VirtualCitizen.GameYearMinutes * 0.2;

                var bride = females.FirstOrDefault(f => 
                {
                    if (Math.Abs(mAge - femaleAges[f]) > limit) return false;

                    // 🌟 [기획 3번: 로미오와 줄리엣] 원한 기반 상성 체크
                    if (m.House != null && f.House != null && m.House != f.House)
                    {
                        int mHatesF = m.House.Grudges.ContainsKey(f.House.HouseName) ? m.House.Grudges[f.House.HouseName] : 0;
                        int fHatesM = f.House.Grudges.ContainsKey(m.House.HouseName) ? f.House.Grudges[m.House.HouseName] : 0;
                        
                        // 양쪽 가문 중 하나라도 철천지 원수(Grudge > 50)라면 정상적인 혼담 파기
                        if (mHatesF > 50 || fHatesM > 50) 
                        {
                            // 단, 5% 확률로 눈이 맞아 사랑의 도피(몰래 결혼) 발생!
                            return Utility.RandomDouble() < 0.05; 
                        }
                    }
                    return true;
                }); 

                if (bride != null) 
                { 
                    FormFamily(m, bride, town); 
                    females.Remove(bride); 
                    femaleAges.Remove(bride); 
                } 
            } 
        }
        
        private static bool IsSingle(VirtualCitizen c) => c.Family != null && ((c.Gender == Gender.Male && c.Family.Mother == null) || (c.Gender == Gender.Female && c.Family.Father == null));
        private static bool IsEligible(VirtualCitizen c) => (c.Age / (c.MaxLifespan.TotalMinutes / VirtualCitizen.GameYearMinutes)) is >= 0.2 and <= 0.7;
        
        private static void FormFamily(VirtualCitizen m, VirtualCitizen f, TownEconomy town) 
        { 
            var mFam = m.Family; 
            var fFam = f.Family; 
            if (mFam == null || fFam == null) return; 

            VirtualHouse mHouse = m.House;
            VirtualHouse fHouse = f.House;
            VirtualHouse targetHouse = mHouse;
            VirtualHouse houseToSell = null;

            // 🌟 [사랑의 도피 판정]
            bool isForbiddenLove = false;
            if (mHouse != null && fHouse != null && mHouse != fHouse)
            {
                int mHatesF = mHouse.Grudges.ContainsKey(fHouse.HouseName) ? mHouse.Grudges[fHouse.HouseName] : 0;
                int fHatesM = fHouse.Grudges.ContainsKey(mHouse.HouseName) ? fHouse.Grudges[mHouse.HouseName] : 0;
                
                if (mHatesF > 50 || fHatesM > 50) isForbiddenLove = true;
            }

            if (isForbiddenLove)
            {
                // 원수 가문끼리 몰래 결혼함 -> 양쪽 가문에서 파문당하고 길거리에 나앉음 (텐트 생활 시작)
                Console.WriteLine($"[Scandal] 맙소사! 원수 지간인 '{mHouse.HouseName}' 가문의 {m.Name}과 '{fHouse.HouseName}' 가문의 {f.Name}이(가) 사랑의 도피를 감행했습니다!");
                
                // 가문의 수치: 부모 가문 명예 및 프레스티지 폭락
                mHouse.Prestige = Math.Max(0, mHouse.Prestige - 50);
                fHouse.Prestige = Math.Max(0, fHouse.Prestige - 50);
                m.Fame = Math.Max(0, m.Fame - 1000);
                f.Fame = Math.Max(0, f.Fame - 1000);
                
                // 양가에서 쫓겨나 아무 지원 없이 독립된 피난처(텐트) 생성
                targetHouse = new VirtualHouse($"{m.Name}과 {f.Name}의 도피처", NobilityRank.Commoner);
                town.Houses.Add(targetHouse);
                
                if (mHouse.Families.Contains(mFam)) mHouse.Families.Remove(mFam);
                if (fHouse.Families.Contains(fFam)) fHouse.Families.Remove(fFam);
            }
            else
            {
                // 기존 정상 합병 및 결혼 로직 (사랑의 도피가 아닐 경우)
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
            }

            mFam.Mother = f; 
            mFam.SharedWealth += fFam.SharedWealth; 
            fFam.IsActive = false; 
            f.Family = mFam; 
            TransferWealth(m, mFam); 
            TransferWealth(f, mFam); 
            
            if (targetHouse != null) 
            { 
                // 정상 결혼에 한해서만 가문에 프레스티지(경사) 보너스 지급
                if (!isForbiddenLove) targetHouse.Prestige += 10; 
                
                f.House = targetHouse; 
                m.House = targetHouse; 
                if (!targetHouse.Families.Contains(mFam)) targetHouse.Families.Add(mFam);
            } 
        }

        private static void UpdateFamilies(TownEconomy town) 
        { 
            if (town.Houses == null) return; 
            
            foreach (var house in town.Houses.ToList()) 
            { 
                var profile = house.GetHousingProfile();
                var ownerFamily = house.Families.FirstOrDefault(f => f.IsActive);

                foreach (var family in house.Families.ToList()) 
                { 
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

                    if (family.Children.Count < profile.MaxChildren && family.Father != null && family.Mother != null) 
                    { 
                        double ageRatio = Math.Max(family.Father.Age / (family.Father.MaxLifespan.TotalMinutes / VirtualCitizen.GameYearMinutes), family.Mother.Age / (family.Mother.MaxLifespan.TotalMinutes / VirtualCitizen.GameYearMinutes)); 
                        if (ageRatio <= 0.7 && Utility.RandomDouble() < (0.05 * (1 + (house.Prestige * 0.001)) * (ageRatio > 0.5 ? 0.2 : 1.0) * Math.Min(5.0, 1.0 + ((double)family.SharedWealth / 20000.0)))) 
                            CreateChild(family, house, town); 
                    } 
                    
                    TransferWealth(family.Father, family); 
                    TransferWealth(family.Mother, family); 
                    
                    if (family.SharedWealth > 100) 
                    { 
                        long tribute = family.SharedWealth - 100; 
                        family.SharedWealth = 100; 
                        house.TotalWealth += tribute;
                        
                        if (Utility.RandomDouble() < 0.05)
                        {
                            int donationScore = (int)(house.TotalWealth * 0.01);
                            if (donationScore > 0)
                            {
                                Server.Misc.FamilySystem.Contribute(house.HouseName, donationScore, Server.Items.FamilyCompType.Wealth, true);
                            }
                        }                        
                    } 
                } 
            } 
        }

        private static void TransferWealth(VirtualCitizen c, FamilyUnit f) { if (c != null && c.Gold > 100) { f.SharedWealth += (c.Gold - 100); c.Gold = 100; } }
        private static void CreateChild(FamilyUnit family, VirtualHouse house, TownEconomy town) { var child = new VirtualCitizen(Utility.RandomBool() ? family.Father.JobClass : family.Mother.JobClass, NobilityRank.Commoner, 100) { House = house, Family = family, TargetRegionName = town.TownName }; ApplyGenetics(child, family.Father, family.Mother); family.Children.Add(child); town.Citizens.Add(child); }
        
        public static void PerformInheritance(VirtualCitizen deceased, TownEconomy town) 
        { 
            if (deceased.House != null) 
                deceased.House.AncestorRecords.Add(new AncestorRecord(deceased.Name, deceased.JobClass, deceased.RankLevel, (int)deceased.Age, deceased.IsStarving ? "아사" : deceased.IsDehydrated ? "탈수" : "노환")); 
            
            int legacy = (int)(deceased.Gold * 0.7); 
            town.Wealth += (int)(deceased.Gold * 0.3); 

            town.Citizens.Remove(deceased); 

            VirtualHouse h = deceased.House;
            if (h != null)
            {
                if (deceased.Family != null)
                {
                    if (deceased.Family.Father == deceased) deceased.Family.Father = null;
                    if (deceased.Family.Mother == deceased) deceased.Family.Mother = null;
                    deceased.Family.Children.Remove(deceased);
                    
                    if (deceased.Family.Father == null && deceased.Family.Mother == null && deceased.Family.Children.Count == 0)
                    {
                        deceased.Family.IsActive = false;
                        h.Families.Remove(deceased.Family);
                    }
                }

                bool hasSurvivors = h.Families.Any(f => f.IsActive && (f.Father != null || f.Mother != null || f.Children.Count > 0));

                if (!hasSurvivors)
                {
                    Console.WriteLine($"[Inheritance] '{h.HouseName}' 가문의 마지막 생존자가 사망하여 멸문되었습니다. 가옥을 철거합니다.");
                    DemolishEstateArea(h, town);
                    town.Houses.Remove(h);
                }
                else
                {
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

        private static void ProcessMigration(TownEconomy town)
        {
            if (town.Houses == null) return;

            var homelessHouses = town.Houses.Where(h => h.IsActive && h.EstateSign == null).ToList();

            foreach (var house in homelessHouses)
            {
                bool wantsToLeave = house.HousingAmbition >= 100 || house.TotalWealth < 1000;
                if (!wantsToLeave) continue;

                var bestDest = TownEconomyManager.Towns.Values
                    .Where(t => t.Facet == town.Facet && t.TownID != town.TownID)
                    .OrderByDescending(t => t.Wealth / Math.Max(1, t.Houses.Count)) 
                    .FirstOrDefault();

                if (bestDest != null)
                {
                    var startCode = RegionSaver.GetRegionCodes(town.Facet, town.Center.X, town.Center.Y, town.Center.Z).Major;
                    var endCode = RegionSaver.GetRegionCodes(bestDest.Facet, bestDest.Center.X, bestDest.Center.Y, bestDest.Center.Z).Major;

                    var plan = VirtualTravelNetwork.CalculateBestRoute(startCode, endCode, (int)house.TotalWealth, false);

                    if (plan.IsPossible)
                    {
                        var familyMembers = new List<VirtualCitizen>();
                        foreach (var f in house.Families.Where(f => f.IsActive))
                        {
                            if (f.Father != null && !f.Father.IsExpired) familyMembers.Add(f.Father);
                            if (f.Mother != null && !f.Mother.IsExpired) familyMembers.Add(f.Mother);
                            familyMembers.AddRange(f.Children.Where(c => !c.IsExpired));
                        }

                        foreach (var member in familyMembers)
                        {
                            var req = PartTimeManager.ActiveRequests.FirstOrDefault(r => r.TownName == town.TownName && r.IsAIAssigned);
                            if (req != null)
                            {
                                req.IsAIAssigned = false;
                                req.CurrentParticipants = Math.Max(0, req.CurrentParticipants - 1);
                            }
                        }

                        house.TotalWealth -= plan.TotalCost;
                        town.Wealth += plan.TotalCost; 

                        town.Houses.Remove(house);
                        bestDest.Houses.Add(house);

                        foreach (var member in familyMembers)
                        {
                            town.Citizens.Remove(member);
                            bestDest.Citizens.Add(member);
                            member.TargetRegionName = bestDest.TownName; 
                        }

                        string reason = house.HousingAmbition >= 100 ? "집터 부족" : "일자리/자금 부족";
                        Console.WriteLine($"[Migration] '{house.HouseName}' 가문이 {reason}으로 인해 {town.TownName}을(를) 등지고 {bestDest.TownName}(으)로 이주했습니다! (여비: {plan.TotalCost}gp)");
                    }
                    else
                    {
                        Console.WriteLine($"[Migration] '{house.HouseName}' 가문이 {town.TownName}을(를) 떠나려 했으나 여비가 부족하여 고립되었습니다.");
                    }
                }
            }
        }
        
        private static void ApplyGenetics(VirtualCitizen child, VirtualCitizen p1, VirtualCitizen p2) { foreach (SkillName sk in Enum.GetValues(typeof(SkillName))) { double v1 = p1.Skills.ContainsKey(sk) ? p1.Skills[sk] : 0.0; double v2 = p2 != null && p2.Skills.ContainsKey(sk) ? p2.Skills[sk] : 0.0; child.Skills[sk] = ((v1 + v2) / 2.0) * Utility.RandomMinMax(30, 50) / 100.0; } }

        public static int GetHouseMaxTiles(NobilityRank rank) => rank switch { NobilityRank.Commoner => 10, NobilityRank.Knight => 50, NobilityRank.SubBaronet => 100, NobilityRank.Baronet => 150, NobilityRank.SubBaron => 200, NobilityRank.Baron => 300, NobilityRank.Viscount => 500, NobilityRank.Count => 700, NobilityRank.Marquis => 1000, _ => 10 };
    }
}