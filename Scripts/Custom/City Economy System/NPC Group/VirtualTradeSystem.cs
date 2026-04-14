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
            int spaceLeft = isMerchant ? 5000 : (citizen.House != null ? citizen.House.MaxCapacity - citizen.House.HouseWarehouse.Values.Sum() : 10);
            
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

                        // 🌟 [수정] 3단계 명예 점수 캐싱 반영 (마을 창고 구매 시)
                        if (citizen.House != null && !isMerchant && !isDirectRequest)
                        {
                            citizen.House.AlterWarehouseItem(itemType, buyAmount, -1);
                        }
                        return (true, totalCost);
                    }
                }
            }

            if (!checkVendorFirst) return SearchPlayerVendors(citizen, town, itemType, tolerance.MaxBuyPrice, finalRequestAmount, isDirectRequest);
            return (false, 0);
        }

        // ==============================================================================
        // 🌟 [최적화] 2. [VirtualEconomyAI] 수학적 거리 계산으로 스캔 렉(Freezing) 제거
        // ==============================================================================
        private static (bool Success, int Spent) SearchPlayerVendors(VirtualCitizen citizen, TownEconomy town, Type targetType, int maxPricePerItem, int requiredAmount, bool isDirectRequest)
        {
            var map = town.Facet;
            if (map == null || map == Map.Internal) return (false, 0);

            bool isMerchant = ((int)citizen.JobClass / 100) * 100 == 600;
            int maxCap = citizen.House?.MaxCapacity ?? 10;
            int currentWeight = citizen.House?.HouseWarehouse.Values.Sum() ?? 0;
            int availableSpace = isMerchant ? 1000 : Math.Max(0, maxCap - currentWeight);

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

                            if (isMerchant) ExecuteSell(citizen, town, targetType, maxPricePerItem, buyAmount);
                            else if (citizen.House != null && !isDirectRequest)
                            {
                                // 🌟 [수정] 명예 점수 캐싱 반영
                                int exactScore = FameEconomy.GetFameScore(boughtItem);
                                citizen.House.AlterWarehouseItem(targetType, buyAmount, exactScore);
                            }
                            boughtItem.Delete(); 
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

                            if (isMerchant) ExecuteSell(citizen, town, targetType, maxPricePerItem, item.Amount);
                            else if (citizen.House != null && !isDirectRequest)
                            {
                                // 🌟 [수정] 명예 점수 캐싱 반영 (이름 오류 해결)
                                int exactScore = FameEconomy.GetFameScore(item);
                                citizen.House.AlterWarehouseItem(targetType, item.Amount, exactScore);
                            }
                            item.Delete();
                            return (true, totalCost);
                        }
                    }
                }
            }
            return (false, 0);
        }

        // ==============================================================================
        // 🌟 [VirtualConsumptionAI] 대량 소비 및 비축 시스템
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
                WorkshopTier currentTier = WorkshopEconomy.GetTier(house.HouseWarehouse, profile.Skill);
                if (currentTier < WorkshopTier.Medium)
                {
                    Type neededAddon = GetDesiredAddonForSkill(profile.Skill);
                    if (neededAddon != null) house.TargetStockProfile[neededAddon] = 1;
                }
            }

            int currentItems = house.HouseWarehouse.Values.Sum();
            if (currentItems > house.MaxCapacity * 0.8)
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
                int currentAmount = house.HouseWarehouse.ContainsKey(kvp.Key) ? house.HouseWarehouse[kvp.Key] : 0;
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
                int currentAmount = house.HouseWarehouse.ContainsKey(itemType) ? house.HouseWarehouse[itemType] : 0;
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
						// 🌟 성공했으니 미수급 장부에서 제거하거나 수량 차감
						if (house.UnfulfilledNeeds.ContainsKey(itemType)) house.UnfulfilledNeeds.Remove(itemType);
					}
					else
					{
						// 🌟 [핵심] 구매 실패 시 미수급 장부에 기록! (내일 아침에 게시판에 올림)
						if (!house.UnfulfilledNeeds.ContainsKey(itemType)) house.UnfulfilledNeeds[itemType] = 0;
						house.UnfulfilledNeeds[itemType] += amountToBuy;
					}
				}
            }
        }

		/// <summary>
		/// 미수급 장부를 확인하여 마을 게시판(PartTimeManager)에 실제 퀘스트를 등록합니다.
		/// </summary>
		public static void GenerateAIJobRequests(VirtualHouse house, TownEconomy town)
		{
			if (house == null || house.UnfulfilledNeeds.Count == 0 || town == null) return;

			foreach (var kvp in house.UnfulfilledNeeds.ToList())
			{
				Type itemType = kvp.Key;
				int amount = kvp.Value;

				if (amount <= 0) continue;

				// 1. 보상금 계산 (마을 시세의 1.5배)
				int unitPrice = town.GetPrice(itemType);
				int totalReward = (int)(unitPrice * amount * 1.5);
				
				// 가문 자본이 충분할 때만 발주
				if (house.TotalWealth < totalReward) continue;

				// 2. 제목 및 카테고리 결정
				string itemName = itemType.Name; // LabelList 대신 기본 Name 사용
				string title = $"[개인] {house.HouseName} 가문의 {itemName} 납품 의뢰";
				JobCategory cat = GetCategoryForItem(itemType);
				
				// 3. 중복 의뢰 방지 (이미 게시판에 같은 제목의 글이 있는지 확인)
				bool alreadyPosted = PartTimeManager.ActiveRequests.Any(r => r.TownName == town.TownName && r.Title == title);

				if (!alreadyPosted)
				{
					// 🌟 [연동] 이제 PartTimeManager에 우리가 만든 함수를 호출합니다!
					PartTimeManager.CreateAIRequest(town.TownName, title, cat, itemType, amount, totalReward);
					
					// 보상금을 가문 자산에서 미리 차감 (예약금)
					house.TotalWealth -= totalReward;
					
					Console.WriteLine($"[AIQuest] {house.HouseName} 가문이 {itemName} {amount}개를 {totalReward}gp에 게시판 발주했습니다.");
				}
			}
			// 발주를 마쳤으므로 장부를 비움
			house.UnfulfilledNeeds.Clear();
		}

		private static JobCategory GetCategoryForItem(Type t)
		{
			// 아이템 타입에 따른 게시판 탭 분류
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
        // 3. [PVA 엔진 & 자원 납품] 
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
                            citizen.House.AlterWarehouseItem(targetItem, consumedAmount, -1);
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

        // ==============================================================================
        // 4. [기타 유틸리티 및 무역]
        // ==============================================================================
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
            var supplier = town.Citizens.FirstOrDefault(c => c.House != null && c.House.HouseWarehouse.Any(kvp => IsRareResource(kvp.Key)));
            var noble = town.Citizens.FirstOrDefault(c => c.RankLevel >= NobilityRank.Baron && c.Gold > 10000);

            if (supplier == null || noble == null) return;

            var rareItem = supplier.House.HouseWarehouse.First(kvp => IsRareResource(kvp.Key));
            int marketPrice = town.GetPrice(rareItem.Key) * 5; 

            if (merchant.Gold >= marketPrice && noble.Gold >= (int)(marketPrice * 1.5))
            {
                supplier.House.AlterWarehouseItem(rareItem.Key, -1);
                supplier.Family.SharedWealth += marketPrice;
                merchant.Gold -= marketPrice;

                int sellPrice = (int)(marketPrice * 1.5);
                merchant.Gold += sellPrice;
                noble.Gold -= sellPrice;

                if (noble.House != null) noble.House.AlterWarehouseItem(rareItem.Key, 1);
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

        public static (bool Success, int Profit) ExecuteTradeRoute(VirtualCitizen merchant, TownEconomy currentTown, int baseCapacity)
        {
            if (currentTown == null || TownEconomyManager.Towns.Count < 2) return (false, 0);

            var otherTowns = TownEconomyManager.Towns.Values.Where(t => t.TownID != currentTown.TownID).ToList();
            var targetTown = otherTowns[Utility.Random(otherTowns.Count)];

            var exportCandidates = currentTown.Warehouse.Values.Where(w => w.Stock > w.TargetStock && currentTown.GetPrice(w.ItemType) < w.BasePrice).OrderBy(w => currentTown.GetPrice(w.ItemType)).ToList();
            if (exportCandidates.Count == 0) return (false, 0);

            var exportItem = exportCandidates.First();
            Type itemType = exportItem.ItemType;
            int buyPrice = currentTown.GetPrice(itemType);
            
            double itemID = merchant.Skills.TryGetValue(SkillName.ItemID, out var sk) ? sk : merchant.PrimarySkill;
            int maxAnimals = (itemID >= 80.0 && merchant.Potential >= 2.5) ? 5 : ((itemID >= 50.0 && merchant.Potential >= 1.5) ? 3 : 1);

            int animalCost = 1000; 
            int stableFee = 50;    
            int theoreticalMaxCapacity = baseCapacity + (maxAnimals * 400); 

            int maxAffordable = merchant.Gold / Math.Max(1, buyPrice);
            int amountToTrade = Math.Min(theoreticalMaxCapacity, Math.Min(exportItem.Stock, maxAffordable));
            
            if (amountToTrade <= 0) return (false, 0);

            int animalsNeeded = 0;
            if (amountToTrade > baseCapacity)
            {
                animalsNeeded = Math.Min((int)Math.Ceiling((amountToTrade - baseCapacity) / 400.0), maxAnimals);
                int totalAnimalCost = animalsNeeded * animalCost;
                
                if (merchant.Gold < totalAnimalCost + (buyPrice * amountToTrade))
                {
                    amountToTrade = (merchant.Gold - totalAnimalCost) / Math.Max(1, buyPrice);
                    if (amountToTrade <= 0) return (false, 0);
                }
                merchant.Gold -= totalAnimalCost;
                currentTown.Wealth += totalAnimalCost;
            }

            int totalCost = buyPrice * amountToTrade;
            int sellPrice = 0;
            bool isNewProduct = false;

            if (targetTown.Warehouse.ContainsKey(itemType))
            {
                sellPrice = targetTown.GetPrice(itemType);
                if (sellPrice <= buyPrice) return (false, 0);
            }
            else
            {
                if (targetTown.TownIndex == "C") return (false, 0);
                isNewProduct = true;
                sellPrice = (int)(buyPrice * 1.5);
            }

            merchant.Gold -= totalCost;
            currentTown.Wealth += totalCost;
            currentTown.Warehouse[itemType].Stock -= amountToTrade;

            int totalRevenue = sellPrice * amountToTrade;
            merchant.Gold += totalRevenue;
            targetTown.Wealth -= totalRevenue; 

            if (animalsNeeded > 0)
            {
                var tamer = currentTown.Citizens.FirstOrDefault(c => c.JobClass == NpcJobClass.StableBroker || c.JobClass == NpcJobClass.StableHand || c.JobClass == NpcJobClass.AnimalTamer_Warrior);
                if (tamer != null)
                {
                    int totalFee = animalsNeeded * stableFee;
                    merchant.Gold -= totalFee;
                    tamer.Gold += totalFee; 
                }
                else
                {
                    int refund = (animalsNeeded * animalCost) / 2;
                    merchant.Gold += refund;
                    currentTown.Wealth -= refund; 
                }
            }

            if (isNewProduct) targetTown.Warehouse[itemType] = new WarehouseItem(itemType, amountToTrade, sellPrice, amountToTrade);
            else
            {
                targetTown.Warehouse[itemType].Stock += amountToTrade;
                var targetItem = targetTown.Warehouse[itemType];
                targetItem.TargetStock += Math.Max(1, amountToTrade / 5);
                int currentBase = targetItem.BasePrice;
                targetItem.BasePrice = Math.Max(1, currentBase + (int)((sellPrice - currentBase) * 0.05));
            }

            merchant.Stress = Math.Max(0, merchant.Stress - 20);
            return (true, totalRevenue - totalCost);
        }

        // ==============================================================================
        // 5. [RetailMarketEngine] 글로벌 검색
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