using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Commands;

namespace Server.Misc
{
    public static class VirtualTradeSystem
    {
        public static void Initialize()
        {
            CommandSystem.Register("경매", AccessLevel.Player, new CommandEventHandler(OnMarketSearch));
        }

        // ==============================================================================
        // 🌟 [헬퍼 함수] 아이템의 재질(CraftResource)과 명품(Exceptional) 여부를 안전하게 추출
        // ==============================================================================
        public static (CraftResource Res, bool IsExc) GetResourceAndQuality(Item item)
        {
            CraftResource turnInRes = CraftResource.None;
            bool isExc = false;
            if (item == null) return (turnInRes, isExc);

            var prop = item.GetType().GetProperty("Resource");
            if (prop != null)
            {
                var resVal = prop.GetValue(item);
                if (resVal is CraftResource cr) turnInRes = cr;
            }

            if (item is IQuality q) isExc = (q.Quality == ItemQuality.Exceptional);
            else
            {
                var qProp = item.GetType().GetProperty("Quality");
                if (qProp != null)
                {
                    object val = qProp.GetValue(item);
                    isExc = (val is int i && i == 2) || val?.ToString() == "Exceptional";
                }
            }
            return (turnInRes, isExc);
        }

        // ==============================================================================
        // 1. [VirtualTradeAI] 시민 물가 연산 및 자원 수급
        // ==============================================================================
        public static (int MaxBuyPrice, int MinSellPrice, double Desire) GetTradeTolerance(VirtualCitizen citizen, int basePrice)
        {
            int rankBonus = (int)citizen.RankLevel; 
            double wealthFactor = citizen.Gold / (double)Math.Max(1, basePrice * 5);
            double desire = 1.0 + (rankBonus * 0.3) + Math.Min(4.0, wealthFactor);
            double stressFactor = 1.0 + (citizen.Stress / 100.0); 

            int maxBuy = (int)(basePrice * desire * stressFactor);
            int minSell = (int)(basePrice * (0.6 - (stressFactor * 0.1)));

            return (Math.Max(basePrice, maxBuy), Math.Max(1, minSell), desire);
        }

        public static (bool Success, int Spent) ExecutePurchase(VirtualCitizen citizen, TownEconomy town, Type itemType, int basePrice, int requestedAmount = 0)
        {
            if (itemType == typeof(Gold)) return (true, 0);
            
            var tolerance = GetTradeTolerance(citizen, basePrice);
            bool isMerchant = ((int)citizen.JobClass / 100) * 100 == 600;
            bool isDirectRequest = requestedAmount > 0;

            int desiredAmount = isDirectRequest ? requestedAmount : (isMerchant ? (int)(500 * citizen.Potential) : (int)Math.Max(1, citizen.Potential * (2 + (int)citizen.RankLevel)));
            
            // 🌟 수정: 가상 장부 연산 제거, 실제 깔려있는 물리 가구의 총 용량 계산 (LINQ 배제)
            int spaceLeft = 10; 
            if (isMerchant) 
            {
                spaceLeft = 5000;
            }
            else if (citizen.House != null && citizen.House.Interior != null)
            {
                int maxCap = 0;
                int currentItems = 0;
                for (int i = 0; i < citizen.House.Interior.PlacedFurniture.Count; i++)
                {
                    if (citizen.House.Interior.PlacedFurniture[i] is Container c)
                    {
                        maxCap += c.DefaultMaxItems;
                        currentItems += c.TotalItems;
                    }
                }
                spaceLeft = maxCap - currentItems;
            }
            
            int finalRequestAmount = isDirectRequest ? desiredAmount : Math.Min(desiredAmount, spaceLeft);
            if (finalRequestAmount <= 0) return (false, 0);

            bool checkVendorFirst = isDirectRequest || Utility.RandomDouble() < (citizen.Potential / 4.0) || isMerchant;

            if (checkVendorFirst)
            {
                var vendorResult = SearchPlayerVendors(citizen, town, itemType, tolerance.MaxBuyPrice, finalRequestAmount, isDirectRequest);
                if (vendorResult.Success) return vendorResult;
            }

            if (town.Warehouse.TryGetValue(itemType, out var wItem) && wItem.Stock > 0)
            {
                int townPrice = town.GetPrice(itemType);
                if (townPrice <= tolerance.MaxBuyPrice)
                {
                    int canAfford = citizen.Gold / Math.Max(1, townPrice);
                    int buyAmount = Math.Min(finalRequestAmount, Math.Min(wItem.Stock, canAfford));

                    if (buyAmount > 0)
                    {
                        int totalCost = townPrice * buyAmount;
                        citizen.Gold -= totalCost;
                        town.Wealth += totalCost;
                        wItem.Stock -= buyAmount;

                        if (citizen.House != null && !isMerchant && !isDirectRequest)
                        {
                            // 🌟 수정: 가상 장부 대신 실제 Item 인스턴스를 생성하여 물리적으로 수납 시도
                            Item boughtItem = (Item)Activator.CreateInstance(itemType);
                            boughtItem.Amount = buyAmount;
                            
                            if (boughtItem is BaseContainer)
							{
								// 샀는데 그게 가구(Container)라면 3D 그리드를 찾아 바닥에 내려놓음
								PhysicalStorageEngine.PlaceFurniture(citizen.House, boughtItem);
							}
							else if (!PhysicalStorageEngine.TryStoreItem(citizen.House, boughtItem))
							{
								// 일반 템인데 수납 공간이 없으면 바닥에 버림
								boughtItem.MoveToWorld(citizen.Location, citizen.Map);
							}
                        }
                        return (true, totalCost);
                    }
                }
            }

            if (!checkVendorFirst) return SearchPlayerVendors(citizen, town, itemType, tolerance.MaxBuyPrice, finalRequestAmount, isDirectRequest);
            return (false, 0);
        }

        // ==============================================================================
        // 2. [VirtualEconomyAI] 수학적 거리 계산으로 스캔 렉(Freezing) 제거
        // ==============================================================================
        private static (bool Success, int Spent) SearchPlayerVendors(VirtualCitizen citizen, TownEconomy town, Type targetType, int maxPricePerItem, int requiredAmount, bool isDirectRequest)
        {
            var map = town.Facet;
            if (map == null || map == Map.Internal) return (false, 0);

            bool isMerchant = ((int)citizen.JobClass / 100) * 100 == 600;
            
            // 물리 수납공간 연산
            int availableSpace = 10;
            if (isMerchant) availableSpace = 1000;
            else if (citizen.House != null && citizen.House.Interior != null)
            {
                int maxCap = 0, currentItems = 0;
                for (int i = 0; i < citizen.House.Interior.PlacedFurniture.Count; i++)
                {
                    if (citizen.House.Interior.PlacedFurniture[i] is Container c)
                    {
                        maxCap += c.DefaultMaxItems;
                        currentItems += c.TotalItems;
                    }
                }
                availableSpace = Math.Max(0, maxCap - currentItems);
            }

            var allVendors = new List<(Mobile Vendor, double Distance, bool IsRetail)>();

            if (PlayerVendor.PlayerVendors != null)
            {
                for (int i = 0; i < PlayerVendor.PlayerVendors.Count; i++)
                {
                    var v = PlayerVendor.PlayerVendors[i];
                    if (v == null || v.Map != map || v.Deleted || v.Backpack == null) continue;
                    double dist = Utility.GetDistanceToSqrt(town.Center, v.Location);
                    if (dist <= 100.0) allVendors.Add((v, dist, false));
                }
            }

            if (RetailVendor.RetailVendors != null)
            {
                for (int i = 0; i < RetailVendor.RetailVendors.Count; i++)
                {
                    var v = RetailVendor.RetailVendors[i];
                    if (v == null || v.Map != map || v.Deleted) continue;
                    double dist = Utility.GetDistanceToSqrt(town.Center, v.Location);
                    if (dist <= 100.0) allVendors.Add((v, dist, true));
                }
            }

            allVendors.Sort((a, b) => a.Distance.CompareTo(b.Distance));

            foreach (var vData in allVendors)
            {
                double premiumRate = vData.Distance <= 20 ? 1.2 : (vData.Distance <= 50 ? 1.0 : 0.7);
                int adjustedMaxPrice = (int)(maxPricePerItem * premiumRate);

                if (vData.IsRetail)
                {
                    RetailVendor vendor = vData.Vendor as RetailVendor;
                    if (vendor == null) continue;

                    for (int i = 0; i < vendor.MarketItems.Count; i++)
                    {
                        var mItem = vendor.MarketItems[i];
                        if (mItem.RealItem == null || mItem.RealItem.Deleted || mItem.RealItem.GetType() != targetType) continue;
                        if (mItem.PricePerUnit > adjustedMaxPrice) continue;

                        int affordableQty = citizen.Gold / Math.Max(1, mItem.PricePerUnit);
                        int buyAmount = Math.Min(mItem.RealItem.Amount, Math.Min(availableSpace, Math.Min(affordableQty, requiredAmount)));
                        
                        if (buyAmount <= 0) continue;

                        int totalCost = mItem.PricePerUnit * buyAmount;
                        Item boughtItem = vendor.ExtractItemForAI(mItem, buyAmount);
                        
                        if (boughtItem != null)
                        {
                            citizen.Gold -= totalCost;
                            vendor.HoldGold += totalCost;

                            if (isMerchant) 
                            {
                                ExecuteSell(citizen, town, targetType, maxPricePerItem, buyAmount);
                                boughtItem.Delete(); // 상인은 바로 장부에 팔고 파기
                            }
                            else if (citizen.House != null && !isDirectRequest)
                            {
                                // 🌟 물리적 수납 시스템 적용
                                if (!PhysicalStorageEngine.TryStoreItem(citizen.House, boughtItem))
                                    boughtItem.MoveToWorld(citizen.Location, citizen.Map);
                            }
                            else 
                            {
                                boughtItem.Delete(); // 직접 퀘스트 수급용이면 소모
                            }
                            
                            return (true, totalCost);
                        }
                    }
                }
                else
                {
                    PlayerVendor vendor = vData.Vendor as PlayerVendor;
                    if (vendor == null) continue;

                    var itemsToCheck = new List<Item>();
                    var containersToSearch = new Queue<Container>();
                    containersToSearch.Enqueue(vendor.Backpack);

                    while (containersToSearch.Count > 0)
                    {
                        var currentContainer = containersToSearch.Dequeue();
                        foreach (var item in currentContainer.Items)
                        {
                            if (item.GetType() == targetType) itemsToCheck.Add(item);
                            else if (item is Container sub) containersToSearch.Enqueue(sub);
                        }
                    }

                    foreach (var item in itemsToCheck)
                    {
                        var vi = vendor.GetVendorItem(item);
                        if (vi == null || vi.Price <= 0 || (!isDirectRequest && item.Amount > availableSpace)) continue;

                        int npcBudgetForThisStack = adjustedMaxPrice * item.Amount;

                        if (vi.Price <= npcBudgetForThisStack && vi.Price <= citizen.Gold)
                        {
                            int totalCost = vi.Price;
                            citizen.Gold -= totalCost;
                            vendor.HoldGold += totalCost;

                            if (isMerchant) 
                            {
                                ExecuteSell(citizen, town, targetType, maxPricePerItem, item.Amount);
                                item.Delete();
                            }
                            else if (citizen.House != null && !isDirectRequest)
                            {
                                // 🌟 벤더 상자에서 템을 뽑아내어 물리적으로 내 집에 넣음
                                if (item.Parent is Container parent)
                                    parent.RemoveItem(item);

                                if (!PhysicalStorageEngine.TryStoreItem(citizen.House, item))
                                    item.MoveToWorld(citizen.Location, citizen.Map);
                            }
                            else 
                            {
                                item.Delete();
                            }
                            
                            return (true, totalCost);
                        }
                    }
                }
            }
            return (false, 0);
        }

        // ==============================================================================
        // 3. [VirtualConsumptionAI] 대량 소비 및 비축 시스템
        // ==============================================================================
        public static void UpdateHouseWishlist(VirtualHouse house)
        {
            if (house == null || !house.IsActive || house.Families.Count == 0) return;

            house.TargetStockProfile.Clear();

            int totalMembers = house.Families.Where(f => f.IsActive).Sum(f => (f.Father != null ? 1 : 0) + (f.Mother != null ? 1 : 0) + f.Children.Count);

            house.TargetStockProfile[typeof(BreadLoaf)] = totalMembers * 7;
            house.TargetStockProfile[typeof(BeverageBottle)] = totalMembers * 7;
            house.TargetStockProfile[typeof(Candle)] = 10;
            house.TargetStockProfile[typeof(Bandage)] = totalMembers * 10;

            var profile = VirtualJobCore.GetDeepJobProfile(house.PrimaryJob);
            
            if (profile.JobMaterials != null)
            {
                foreach (Type toolType in profile.JobMaterials)
                {
                    if (toolType.IsSubclassOf(typeof(Item))) 
                        house.TargetStockProfile[toolType] = 2; 
                }
            }

            if (house.MultiID > 0 && house.TotalWealth > 15000)
            {
                // 공방 에드온 필요 여부 (나중에 에드온 물리 스캔으로 수정 가능)
                Type neededAddon = GetDesiredAddonForSkill(profile.Skill);
                if (neededAddon != null) house.TargetStockProfile[neededAddon] = 1;
            }

            // 🌟 수정: 실제 배치된 상자의 용량(80%) 체크
            int maxCapacity = 0;
            int currentItems = 0;
            if (house.Interior != null && house.Interior.PlacedFurniture != null)
            {
                for (int i = 0; i < house.Interior.PlacedFurniture.Count; i++)
                {
                    if (house.Interior.PlacedFurniture[i] is Container c)
                    {
                        maxCapacity += c.DefaultMaxItems;
                        currentItems += c.TotalItems;
                    }
                }
            }

            if (maxCapacity > 0 && currentItems > maxCapacity * 0.8)
            {
                if (house.TotalWealth > 5000) house.TargetStockProfile[typeof(MetalChest)] = 1;
                else house.TargetStockProfile[typeof(WoodenBox)] = 2;
            }
        }

        public static void ProcessHoardingShopping(VirtualCitizen agent, TownEconomy town)
        {
            if (agent.House == null || agent.Gold < 100) return;

            var house = agent.House;
            int totalAmountToBuy = 0;
            
            foreach (var kvp in house.TargetStockProfile)
            {
                // 🌟 수정: 실제 집에 물리적으로 적재된 템 수량 체크
                int currentAmount = PhysicalStorageEngine.GetTotalItemCount(house, kvp.Key);
                if (kvp.Value > currentAmount) totalAmountToBuy += (kvp.Value - currentAmount);
            }

            if (totalAmountToBuy <= 0) return;

            int carryLimit = 50; 
            
            if (totalAmountToBuy > carryLimit)
            {
                int animalsNeeded = (int)Math.Ceiling(totalAmountToBuy / 400.0);
                int animalCost = 500; 
                int totalFee = animalsNeeded * animalCost;
                
                if (agent.Gold >= totalFee)
                {
                    agent.Gold -= totalFee;
                    town.Wealth += totalFee;
                    Console.WriteLine($"[Shopping] {agent.Name}이(가) 대량의 물자({totalAmountToBuy}개)를 비축하기 위해 짐말 {animalsNeeded}마리를 동원했습니다.");
                }
                else totalAmountToBuy = carryLimit; 
            }

            int boughtSoFar = 0;
            var wishlist = house.TargetStockProfile.ToList();

            foreach (var kvp in wishlist)
            {
                if (boughtSoFar >= totalAmountToBuy) break;

                Type itemType = kvp.Key;
                int targetAmount = kvp.Value;
                int currentAmount = PhysicalStorageEngine.GetTotalItemCount(house, itemType);
                int amountNeeded = targetAmount - currentAmount;

                if (amountNeeded > 0)
                {
                    int amountToBuy = Math.Min(amountNeeded, totalAmountToBuy - boughtSoFar);
                    int basePrice = Math.Max(1, town.GetPrice(itemType));
                    
                    var result = ExecutePurchase(agent, town, itemType, basePrice, amountToBuy);
                    if (result.Success)
                    {
                        agent.Satisfaction = Math.Min(100, agent.Satisfaction + 5);
                        boughtSoFar += amountToBuy;
                        if (house.UnfulfilledNeeds.ContainsKey(itemType)) house.UnfulfilledNeeds.Remove(itemType);
                    }
                    else
                    {
                        if (!house.UnfulfilledNeeds.ContainsKey(itemType)) house.UnfulfilledNeeds[itemType] = 0;
                        house.UnfulfilledNeeds[itemType] += amountToBuy;
                    }
                }
            }
        }

        public static void GenerateAIJobRequests(VirtualHouse house, TownEconomy town)
        {
            if (house == null || house.UnfulfilledNeeds == null || house.UnfulfilledNeeds.Count == 0 || town == null) 
                return;

            Type[] keys = new Type[house.UnfulfilledNeeds.Count];
            house.UnfulfilledNeeds.Keys.CopyTo(keys, 0);

            for (int i = 0; i < keys.Length; i++)
            {
                Type itemType = keys[i];
                int amount = house.UnfulfilledNeeds[itemType];

                if (amount <= 0) continue;

                int unitPrice = town.GetPrice(itemType);
                int totalReward = (unitPrice * amount) * 2;
                
                if (house.TotalWealth < totalReward) continue;

                string title = string.Format("[긴급 납품] {0} 가문의 의뢰", house.HouseName);
                JobCategory cat = GetCategoryForItem(itemType);
                
                bool alreadyPosted = false;
                for (int j = 0; j < PartTimeManager.ActiveRequests.Count; j++)
                {
                    TownJobRequest req = PartTimeManager.ActiveRequests[j];
                    if (req.TownName == town.TownName && req.TargetType == itemType && !req.IsFullyBooked && req.IssuerHouse == house)
                    {
                        alreadyPosted = true;
                        break;
                    }
                }

                if (!alreadyPosted)
                {
                    PartTimeManager.CreateAIRequest(town.TownName, title, cat, itemType, amount, totalReward, house);
                    
                    house.TotalWealth -= totalReward;
                    Console.WriteLine(string.Format("[AIQuest] {0} 가문이 {1} {2}개 납품 의뢰를 등록했습니다.", house.HouseName, itemType.Name, amount));
                }
            }
            
            house.UnfulfilledNeeds.Clear();
        }

        private static JobCategory GetCategoryForItem(Type t)
        {
            if (t.IsSubclassOf(typeof(BaseArmor)) || t.IsSubclassOf(typeof(BaseWeapon)) || t.Name.Contains("Deed")) 
                return JobCategory.Crafting;
            
            if (t == typeof(IronOre) || t == typeof(Log) || t == typeof(WheatSheaf)) 
                return JobCategory.Gathering;

            return JobCategory.Menial;
        }

        private static Type GetDesiredAddonForSkill(SkillName skill)
        {
            return skill switch
            {
                SkillName.Blacksmith => typeof(AnvilEastDeed),
                SkillName.Tailoring => typeof(LoomEastDeed),
                SkillName.Cooking => typeof(StoneOvenEastDeed),
                SkillName.Carpentry => typeof(WoodworkersBenchDeed),
                SkillName.Alchemy => typeof(AlchemyStationDeed),
                SkillName.Tinkering => typeof(TinkerBenchDeed),
                SkillName.Fletching => typeof(FletchingStationDeed),
                SkillName.Inscribe => typeof(WritingDeskDeed),
                _ => null
            };
        }

        // ==============================================================================
        // 4. [PVA 엔진 & 자원 납품] 
        // ==============================================================================
        public static int GetPVAGuaranteedPrice(Type itemType, TownEconomy town)
        {
            int marketPrice = Math.Max(1, town.GetPrice(itemType));

            if (itemType == typeof(IronIngot)) return CalculatePVA(typeof(IronOre), town, 2, 4, marketPrice);
            if (itemType == typeof(Board)) return CalculatePVA(typeof(Log), town, 1, 3, marketPrice);
            if (itemType == typeof(SackFlour)) return CalculatePVA(typeof(WheatSheaf), town, 1, 5, marketPrice);
            if (itemType == typeof(Bottle)) return CalculatePVA(typeof(Sand), town, 2, 5, marketPrice); 
            if (itemType == typeof(BeverageBottle)) return CalculatePVA(typeof(Bottle), town, 1, 3, marketPrice); 
            if (itemType == typeof(Pitcher)) return CalculatePVA(typeof(Board), town, 1, 4, marketPrice); 

            return marketPrice;
        }

        private static int CalculatePVA(Type rawMaterial, TownEconomy town, int yieldRate, int processingFee, int currentMarketPrice)
        {
            int rawCost = Math.Max(1, town.GetPrice(rawMaterial));
            int pvaPrice = (rawCost / yieldRate) + processingFee;
            return Math.Max(currentMarketPrice, pvaPrice);
        }

        public static (bool Success, int Earnings) ExecuteHarvestAndSell(VirtualCitizen citizen, TownEconomy town, int basePrice)
        {
            double focus = citizen.Bio != null ? Math.Max(0, citizen.Bio.Focus / 1000000.0) : 0;
            double perception = citizen.Bio != null ? Math.Max(0, citizen.Bio.Perception / 1000000.0) : 0;
            double adaptability = citizen.Bio != null ? Math.Max(0, citizen.Bio.Adaptability / 1000000.0) : 0;

            double successChance = 0.4 + (0.6 * (citizen.PrimarySkill / 200.0)) + (0.1 * focus);
            if (Utility.RandomDouble() > successChance) return (false, 0);

            int baseHarvest = (int)(6 * citizen.Potential);
            int harvestAmount = baseHarvest + (int)(baseHarvest * (0.5 * adaptability));
            
            ResourceType type = GetResourceTypeByJob(citizen.JobClass);
            ResourceKey key = new ResourceKey(town.Facet.Name, citizen.TargetRegionName ?? "", type);

            if (string.IsNullOrEmpty(citizen.TargetRegionName) || !ResourceManager.Pools.ContainsKey(key) || ResourceManager.Pools[key].CurrentCapacity <= 0)
            {
                FindWorkPool(citizen, town);
                key = new ResourceKey(town.Facet.Name, citizen.TargetRegionName ?? "", type);
            }

            if (string.IsNullOrEmpty(citizen.TargetRegionName)) return (false, 0);

            if (ResourceManager.Pools.TryGetValue(key, out ResourcePool pool) && pool.CurrentCapacity > 0)
            {
                var availableKeys = pool.AvailableResources.Where(kvp => kvp.Value > 0).Select(k => k.Key).ToList();
                Type targetItem = (availableKeys.Count > 0) ? availableKeys[Utility.Random(availableKeys.Count)] : GetDefaultItem(type);

                if (targetItem != null)
                {
                    var (npcTier, _) = citizen.GetResourceTier(citizen.PrimarySkill);
                    int itemTier = GetResourceTierValue(targetItem);

                    bool preventDowngrade = (perception > 0) && (Utility.RandomDouble() < (0.5 * perception));
                    if (itemTier > npcTier && !preventDowngrade) targetItem = GetDefaultItem(type); 

                    int actualHarvest = Math.Min(harvestAmount, pool.CurrentCapacity);
                    int consumedAmount = pool.ConsumeResource(targetItem, actualHarvest);

                    if (consumedAmount > 0)
                    {
                        citizen.CheckSkillGain();
                        if (IsRareResource(targetItem) && citizen.House != null)
                        {
                            // 🌟 수정: 희귀 자원을 수집하여 창고에 직접 넣음
                            Item harvested = (Item)Activator.CreateInstance(targetItem);
                            harvested.Amount = consumedAmount;
                            
                            if (!PhysicalStorageEngine.TryStoreItem(citizen.House, harvested))
                                harvested.MoveToWorld(citizen.Location, citizen.Map); // 공간이 없으면 바닥에 버림
                                
                            return (true, 0);
                        }
                        return ExecuteSell(citizen, town, targetItem, basePrice, consumedAmount);
                    }
                }
            }
            return (false, 0);
        }

        public static (bool Success, int Earnings) ExecuteSell(VirtualCitizen citizen, TownEconomy town, Type itemType, int basePrice, int amount)
        {
            if (itemType == typeof(Gold))
            {
                citizen.Gold += amount;
                return (true, amount);
            }

            var (_, minSell, _) = GetTradeTolerance(citizen, basePrice);
            int guaranteedPrice = GetPVAGuaranteedPrice(itemType, town);
            double sellRate = Math.Min(1.0, 0.70 + (citizen.Potential * 0.10));
            int townBuyPrice = Math.Max(1, (int)(guaranteedPrice * sellRate)); 

            if (townBuyPrice >= minSell)
            {
                int totalEarnings = townBuyPrice * amount;
                citizen.Gold += totalEarnings;
                town.Wealth -= totalEarnings; 
                
                if (!town.Warehouse.ContainsKey(itemType)) town.Warehouse[itemType] = new WarehouseItem(itemType, 0, basePrice, 100);
                town.Warehouse[itemType].Stock += amount;
                return (true, totalEarnings);
            }

            citizen.Stress = Math.Min(100, citizen.Stress + 2); 
            return (false, 0);
        }

        public static bool IsRareResource(Type type) => GetResourceTierValue(type) > 1;

        public static int GetResourceTierValue(Type type)
        {
            if (type == null) return 1;
            CraftResource res = CraftResources.GetFromType(type);
            if (res == CraftResource.None) return 1;
            return CraftResources.GetIndex(res) + 1;
        }

        public static void ExecuteRareBrokerage(VirtualCitizen merchant, TownEconomy town)
        {
            // 돈 많은 귀족 찾기
            var noble = town.Citizens.FirstOrDefault(c => c.RankLevel >= NobilityRank.Baron && c.Gold > 10000 && c.House != null);
            if (noble == null) return;

            // 희귀 자원을 물리 상자에 가지고 있는 평민/공급자 찾기
            foreach (var supplier in town.Citizens)
            {
                if (supplier == merchant || supplier.House == null || supplier.House.Interior == null) continue;

                Item rareItemToSell = null;
                for (int i = 0; i < supplier.House.Interior.PlacedFurniture.Count; i++)
                {
                    if (supplier.House.Interior.PlacedFurniture[i] is Container c)
                    {
                        foreach (var item in c.Items)
                        {
                            if (IsRareResource(item.GetType()))
                            {
                                rareItemToSell = item;
                                break;
                            }
                        }
                    }
                    if (rareItemToSell != null) break;
                }

                if (rareItemToSell != null)
                {
                    int marketPrice = town.GetPrice(rareItemToSell.GetType()) * 5; 

                    if (merchant.Gold >= marketPrice && noble.Gold >= (int)(marketPrice * 1.5))
                    {
                        // 🌟 수정: 공급자의 상자에서 물리적으로 아이템 1개를 뽑아냄
                        Item extracted = PhysicalStorageEngine.RetrieveItem(supplier.House, rareItemToSell.GetType(), 1);
                        
                        if (extracted != null)
                        {
                            supplier.Family.SharedWealth += marketPrice;
                            merchant.Gold -= marketPrice;

                            int sellPrice = (int)(marketPrice * 1.5);
                            merchant.Gold += sellPrice;
                            noble.Gold -= sellPrice;

                            // 귀족의 물리 상자에 넣음
                            if (!PhysicalStorageEngine.TryStoreItem(noble.House, extracted))
                            {
                                extracted.MoveToWorld(noble.Location, noble.Map);
                            }
                            return; // 단건 거래 후 종료
                        }
                    }
                }
            }
        }

        public static ResourceType GetResourceTypeByJob(NpcJobClass job)
        {
            string name = job.ToString().ToLower();
            if (name.Contains("miner") || name.Contains("digger") || name.Contains("quarryman") || name.Contains("knapper")) return ResourceType.Mining;
            if (name.Contains("wood") || name.Contains("bark") || name.Contains("resin") || name.Contains("sawyer") || name.Contains("cutter")) return ResourceType.Lumberjacking;
            if (name.Contains("fish") || name.Contains("crab") || name.Contains("oyster") || name.Contains("whaler") || name.Contains("comber") || name.Contains("maritime")) return ResourceType.Fishing;
            if (name.Contains("tanner") || name.Contains("leather") || name.Contains("skinner") || name.Contains("hunter") || name.Contains("trapper") || name.Contains("plucker")) return ResourceType.Tanning;
            return ResourceType.Farming; 
        }

        public static void FindWorkPool(VirtualCitizen citizen, TownEconomy town)
        {
            ResourceType type = GetResourceTypeByJob(citizen.JobClass);
            string townName = TownNumber.GetName(town.TownID).ToLower();

            var pools = ResourceManager.Pools.Values.Where(p => p.MapName == town.Facet.Name && p.Type == type);
            ResourcePool bestPool = null;

            if (citizen.Potential >= 2.5) bestPool = pools.OrderByDescending(p => p.CurrentCapacity).FirstOrDefault();
            else
            {
                if (type == ResourceType.Fishing) bestPool = pools.Where(p => !p.RegionName.StartsWith("Ocean")).OrderByDescending(p => p.CurrentCapacity).FirstOrDefault();
                else bestPool = pools.Where(p => p.RegionName.ToLower().Contains(townName)).OrderByDescending(p => p.CurrentCapacity).FirstOrDefault();
            }

            citizen.TargetRegionName = bestPool != null ? bestPool.RegionName : ""; 
        }

        private static Type GetDefaultItem(ResourceType type) => type switch
        {
            ResourceType.Mining => typeof(IronOre),
            ResourceType.Lumberjacking => typeof(Log),
            ResourceType.Fishing => typeof(Trout),
            _ => typeof(WheatSheaf)
        };

        // ==============================================================================
        // 🌟 5. 대상단 대륙 무역 & [물가 변동 안전장치]
        // ==============================================================================
        public static (bool Success, int Profit) ExecuteTradeRoute(VirtualCitizen merchant, TownEconomy currentTown, int baseCapacity)
        {
            if (currentTown == null || TownEconomyManager.Towns.Count < 2) return (false, 0);

            int groupID = ((int)merchant.JobClass / 100) * 100;
            bool isLandMerchant = groupID == 300 || groupID == 400 || groupID == 900 || groupID == 1100;
            bool isSeaMerchant = groupID == 800; // 해양 직업군 (어부, 항해사 등)

            if (!isLandMerchant && !isSeaMerchant) return (false, 0);

            var exportCandidates = currentTown.Warehouse.Values
                .Where(w => w.Stock > w.TargetStock * 1.2 && currentTown.GetPrice(w.ItemType) < w.BasePrice)
                .OrderBy(w => currentTown.GetPrice(w.ItemType))
                .ToList();

            if (exportCandidates.Count == 0) return (false, 0);

            var currentRCode = RegionSaver.GetRegionCodes(currentTown.Facet, currentTown.Center.X, currentTown.Center.Y, currentTown.Center.Z).Major;
            
            foreach (var exportItem in exportCandidates)
            {
                Type itemType = exportItem.ItemType;
                int localPrice = currentTown.GetPrice(itemType);
                if (localPrice <= 0) continue;

                var targetTowns = TownEconomyManager.Towns.Values
                    .Where(t => t.TownID != currentTown.TownID && t.Facet == currentTown.Facet)
                    .Where(t => t.Warehouse.ContainsKey(itemType) && t.GetPrice(itemType) > localPrice * 1.2) 
                    .OrderByDescending(t => t.GetPrice(itemType))
                    .ToList();

                foreach (var targetTown in targetTowns)
                {
                    if (isSeaMerchant && !IsCoastalTown(targetTown.TownName)) continue;

                    var targetRCode = RegionSaver.GetRegionCodes(targetTown.Facet, targetTown.Center.X, targetTown.Center.Y, targetTown.Center.Z).Major;
                    
                    var plan = VirtualTravelNetwork.CalculateBestRoute(currentRCode, targetRCode, merchant.Gold, false);
                    if (!plan.IsPossible) continue;

                    int targetPrice = targetTown.GetPrice(itemType);
                    
                    int animalCost = 500;
                    int capacityPerAnimal = 400;
                    
                    double itemID = merchant.Skills.TryGetValue(SkillName.ItemID, out var sk) ? sk : merchant.PrimarySkill;
                    int maxAnimals = (itemID >= 80.0 && merchant.Potential >= 2.5) ? 5 : ((itemID >= 50.0 && merchant.Potential >= 1.5) ? 3 : 1);
                    int theoreticalMaxCapacity = baseCapacity + (maxAnimals * capacityPerAnimal);

                    int maxAffordable = (merchant.Gold - plan.TotalCost) / Math.Max(1, localPrice);
                    int amountToTrade = Math.Min(theoreticalMaxCapacity, Math.Min(exportItem.Stock, maxAffordable));
                    
                    if (amountToTrade < 50) continue; 

                    int animalsNeeded = 0;
                    int totalAnimalCost = 0;
                    
                    if (amountToTrade > baseCapacity && isLandMerchant) 
                    {
                        animalsNeeded = (int)Math.Ceiling((amountToTrade - baseCapacity) / (double)capacityPerAnimal);
                        animalsNeeded = Math.Min(animalsNeeded, maxAnimals);
                        totalAnimalCost = animalsNeeded * animalCost;

                        if (merchant.Gold < plan.TotalCost + totalAnimalCost + (localPrice * amountToTrade))
                        {
                            amountToTrade = (merchant.Gold - plan.TotalCost - totalAnimalCost) / Math.Max(1, localPrice);
                            if (amountToTrade <= 0) continue;
                        }
                    }

                    int expectedProfit = ((targetPrice - localPrice) * amountToTrade) - plan.TotalCost - totalAnimalCost;

                    if (expectedProfit > 0)
                    {
                        int totalPurchaseCost = localPrice * amountToTrade;
                        
                        int totalExpenses = totalPurchaseCost + plan.TotalCost + totalAnimalCost;
                        merchant.Gold -= totalExpenses;
                        currentTown.Wealth += totalExpenses;

                        int totalRevenue = targetPrice * amountToTrade;
                        merchant.Gold += totalRevenue;
                        targetTown.Wealth -= totalRevenue; 

                        currentTown.Warehouse[itemType].Stock -= amountToTrade;
                        var targetItem = targetTown.Warehouse[itemType];
                        
                        bool wasShortage = targetItem.Stock < (targetItem.TargetStock * 0.5);
                        
                        targetItem.Stock += amountToTrade;

                        if (wasShortage && amountToTrade >= 50)
                        {
                            targetItem.TargetStock += Math.Max(1, amountToTrade / 10);

                            int currentBase = targetItem.BasePrice;
                            int priceDelta = targetPrice - currentBase;

                            if (priceDelta > 0)
                            {
                                int increase = Math.Max(1, (int)(priceDelta * 0.03));
                                int maxIncrease = Math.Max(1, (int)(currentBase * 0.10));
                                
                                targetItem.BasePrice += Math.Min(increase, maxIncrease);
                            }
                        }
                        else if (targetItem.Stock > targetItem.TargetStock * 2)
                        {
                            targetItem.BasePrice = Math.Max(1, (int)(targetItem.BasePrice * 0.98));
                        }

                        currentTown.Citizens.Remove(merchant);
                        targetTown.Citizens.Add(merchant);
                        merchant.TargetRegionName = targetTown.TownName;

                        merchant.Stress = Math.Max(0, merchant.Stress - 20);
                        merchant.Satisfaction = 100;

                        string transportMethod = isSeaMerchant ? "상선(Ship)" : (animalsNeeded > 0 ? $"짐말 {animalsNeeded}마리" : "수레");
                        Console.WriteLine($"[Trade] 대상단 '{merchant.Name}'이(가) {transportMethod}를 이끌고 {currentTown.TownName}에서 {targetTown.TownName}로 {itemType.Name} {amountToTrade}개 사재기 무역 성공! (순이익: +{expectedProfit}gp)");
                        
                        return (true, expectedProfit);
                    }
                }
            }
            return (false, 0);
        }

        private static bool IsCoastalTown(string townName)
        {
            string[] coastalTowns = { "Britain", "Skara Brae", "Vesper", "Trinsic", "Moonglow", "Magincia", "Nujel'm", "Jhelom", "Buccaneer's Den", "Serpent's Hold", "Ocllo", "Haven", "Sea Market" };
            return coastalTowns.Any(c => townName.Contains(c, StringComparison.OrdinalIgnoreCase));
        }

        // ==============================================================================
        // 6. [RetailMarketEngine] 글로벌 검색
        // ==============================================================================
        [Usage("경매 <아이템이름>")]
        private static void OnMarketSearch(CommandEventArgs e)	
        {
            string searchWord = e.ArgString.Trim().ToLower();
            if (string.IsNullOrEmpty(searchWord)) { e.Mobile.SendMessage(0x35, "사용법: [경매 <찾을아이템이름>"); return; }

            var list = new List<(string VendorName, string ItemName, int Price, int Stock)>();

            if (RetailVendor.RetailVendors != null)
            {
                for (int i = 0; i < RetailVendor.RetailVendors.Count; i++)
                {
                    var vendor = RetailVendor.RetailVendors[i];
                    if (vendor == null || vendor.Deleted) continue;

                    for (int j = 0; j < vendor.MarketItems.Count; j++)
                    {
                        var m = vendor.MarketItems[j];
                        if (m.RealItem == null || m.RealItem.Deleted) continue;
                        string itemName = (m.RealItem.Name ?? m.RealItem.ItemData.Name).ToLower();
                        
                        if (itemName.Contains(searchWord))
                            list.Add((vendor.Name, m.RealItem.Name ?? m.RealItem.ItemData.Name, m.PricePerUnit, m.RealItem.Amount));
                    }
                }
            }

            if (list.Count == 0) { e.Mobile.SendMessage(33, $"'{searchWord}'에 해당하는 매물이 현재 등록되어 있지 않습니다."); return; }

            e.Mobile.SendMessage(68, $"--- '{searchWord}' 검색 결과 ({list.Count}건) ---");
            int displayCount = Math.Min(list.Count, 10);
            for (int i = 0; i < displayCount; i++)
            {
                var res = list[i];
                e.Mobile.SendMessage(0x481, $"[{res.VendorName}] {res.ItemName} - 개당 {res.Price:N0} GP (재고: {res.Stock})");
            }
        }
    }
}