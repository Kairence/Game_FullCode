using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
    public static class VirtualTradeAI
    {
        // [1~3 섹션: 심리 연산, 구매, 벤더 탐색 로직은 기존 원본 유지]
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

            // [수정] 일반 시민은 작위(RankLevel)와 잠재력에 비례하여 구매량을 늘립니다. (상인은 도매 스케일 유지)
            int desiredAmount = isDirectRequest 
                ? requestedAmount 
                : (isMerchant 
                    ? (int)(500 * citizen.Potential) 
                    : (int)Math.Max(1, citizen.Potential * (2 + (int)citizen.RankLevel)));
            
			// [수정] 상인은 비좁은 집 창고(MaxCapacity) 대신 '상단 마차(용량 5000)'가 있다고 가정합니다.
            int spaceLeft = isMerchant 
                ? 5000 
                : (citizen.House != null ? citizen.House.MaxCapacity - citizen.House.HouseWarehouse.Values.Sum() : 10);
            
            // 최종 필요 수량 확정
            int finalRequestAmount = isDirectRequest ? desiredAmount : Math.Min(desiredAmount, spaceLeft);

            if (finalRequestAmount <= 0) return (false, 0);

            double vendorFirstChance = citizen.Potential / 4.0;
            bool checkVendorFirst = isDirectRequest || Utility.RandomDouble() < vendorFirstChance || isMerchant;

            if (checkVendorFirst)
            {
                // 벤더 탐색 시 '필요 수량(finalRequestAmount)'을 던져줍니다.
                var vendorResult = SearchPlayerVendors(citizen, town, itemType, tolerance.MaxBuyPrice, finalRequestAmount, isDirectRequest);
                if (vendorResult.Success) return vendorResult;
            }

            // --- 마을 창고 구매 로직 ---
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
                            if (!citizen.House.HouseWarehouse.ContainsKey(itemType))
                                citizen.House.HouseWarehouse[itemType] = 0;
                            citizen.House.HouseWarehouse[itemType] += buyAmount;
                        }
                        return (true, totalCost);
                    }
                }
            }

            if (!checkVendorFirst)
            {
                return SearchPlayerVendors(citizen, town, itemType, tolerance.MaxBuyPrice, finalRequestAmount, isDirectRequest);
            }

            return (false, 0);
        }

		// 파라미터에 'int requiredAmount' 가 추가되었습니다.
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
                foreach (var v in PlayerVendor.PlayerVendors.Where(v => v != null && v.Map == map && !v.Deleted && v.Backpack != null))
                {
                    double dist = Math.Sqrt(Math.Pow(town.Center.X - v.Location.X, 2) + Math.Pow(town.Center.Y - v.Location.Y, 2));
                    allVendors.Add((v, dist, false));
                }
            }

            if (RetailVendor.RetailVendors != null)
            {
                foreach (var v in RetailVendor.RetailVendors.Where(v => v != null && v.Map == map && !v.Deleted))
                {
                    double dist = Math.Sqrt(Math.Pow(town.Center.X - v.Location.X, 2) + Math.Pow(town.Center.Y - v.Location.Y, 2));
                    allVendors.Add((v, dist, true));
                }
            }

            var sortedVendors = allVendors.OrderBy(x => x.Distance).ToList();

            foreach (var vData in sortedVendors)
            {
                double distance = vData.Distance;
                double premiumRate = distance <= 20 ? 1.2 : (distance <= 50 ? 1.0 : 0.7);
                int adjustedMaxPrice = (int)(maxPricePerItem * premiumRate);

                if (vData.IsRetail)
                {
                    RetailVendor vendor = vData.Vendor as RetailVendor;
                    if (vendor == null) continue;

                    var matchingItems = vendor.MarketItems
                        .Where(m => m.RealItem != null && !m.RealItem.Deleted && m.RealItem.GetType() == targetType)
                        .ToList();

                    foreach (var mItem in matchingItems)
                    {
                        if (mItem.PricePerUnit > adjustedMaxPrice) continue;

                        int affordableQty = citizen.Gold / Math.Max(1, mItem.PricePerUnit);
                        
                        // [사재기 방지 핵심] 매대 재고, 남은 공간, 내 예산, 그리고 '필요 수량(requiredAmount)' 중 가장 작은 값 선택!
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
                                if (!citizen.House.HouseWarehouse.ContainsKey(targetType))
                                    citizen.House.HouseWarehouse[targetType] = 0;
                                citizen.House.HouseWarehouse[targetType] += buyAmount;
                            }

                            // AI 인벤토리로 넘어온 분할 아이템만 삭제되므로 원본은 무사합니다.
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
                        foreach (var item in currentContainer.Items.ToArray())
                        {
                            if (item.GetType() == targetType) itemsToCheck.Add(item);
                            else if (item is Container sub) containersToSearch.Enqueue(sub);
                        }
                    }

                    foreach (var item in itemsToCheck)
                    {
                        var vi = vendor.GetVendorItem(item);
                        if (vi == null || vi.Price <= 0) continue;
                        if (!isDirectRequest && item.Amount > availableSpace) continue;

                        int npcBudgetForThisStack = adjustedMaxPrice * item.Amount;

                        if (vi.Price <= npcBudgetForThisStack && vi.Price <= citizen.Gold)
                        {
                            int totalCost = vi.Price;
                            citizen.Gold -= totalCost;
                            vendor.HoldGold += totalCost;

                            if (isMerchant) ExecuteSell(citizen, town, targetType, maxPricePerItem, item.Amount);
                            else if (citizen.House != null && !isDirectRequest)
                            {
                                if (!citizen.House.HouseWarehouse.ContainsKey(targetType))
                                    citizen.House.HouseWarehouse[targetType] = 0;
                                citizen.House.HouseWarehouse[targetType] += item.Amount;
                            }

                            item.Delete();
                            return (true, totalCost);
                        }
                    }
                }
            }
            return (false, 0);
        }
        // 마을의 현재 도매 매입가를 예측하는 헬퍼 함수
        private static int GetEstimatedTownBuyPrice(TownEconomy town, Type itemType)
        {
            int guaranteedPrice = GetPVAGuaranteedPrice(itemType, town);
            int currentStock = town.Warehouse.ContainsKey(itemType) ? town.Warehouse[itemType].Stock : 0;
            int targetStock = town.Warehouse.ContainsKey(itemType) ? town.Warehouse[itemType].TargetStock : 100;

            double wholesaleRate = 0.65;
            if (town.Wealth > 100000) wholesaleRate += 0.10;
            if (targetStock > 0 && (double)currentStock / targetStock < 0.1) wholesaleRate = 0.85;

            return (int)(guaranteedPrice * wholesaleRate);
        }

		// ==============================================================================
        // 4. [최종] 자원 추출 로직: Pool에서 직접 땡겨오기
        // ==============================================================================
		public static (bool Success, int Earnings) ExecuteHarvestAndSell(VirtualCitizen citizen, TownEconomy town, int basePrice)
		{
			// [BioStats 초기화 확인 및 안전한 변수 할당]
			double focus = citizen.Bio != null ? Math.Max(0, citizen.Bio.Focus / 1000000.0) : 0;
			double perception = citizen.Bio != null ? Math.Max(0, citizen.Bio.Perception / 1000000.0) : 0;
			double adaptability = citizen.Bio != null ? Math.Max(0, citizen.Bio.Adaptability / 1000000.0) : 0;

			// 1. [집중(Focus) 반영] 기본 성공 확률 + 집중력에 따른 추가 보정 (최대 +20% 추가)
			double successChance = 0.2 + (0.8 * (citizen.PrimarySkill / 200.0)) + (0.2 * focus);
			if (Utility.RandomDouble() > successChance) return (false, 0);

			// 2. [적응(Adaptability) 반영] 기본 채집량 + 적응력에 따른 수량 뻥튀기 (최대 +50% 증가)
			int baseHarvest = (int)(6 * citizen.Potential);
			int harvestAmount = baseHarvest + (int)(baseHarvest * (0.5 * adaptability));

			if (string.IsNullOrEmpty(citizen.TargetRegionName)) FindWorkPool(citizen, town);
			if (string.IsNullOrEmpty(citizen.TargetRegionName)) return (false, 0);

			ResourceType type = GetResourceTypeByJob(citizen.JobClass);
			ResourceKey key = new ResourceKey(town.Facet.Name, citizen.TargetRegionName, type);

			if (ResourceManager.Pools.TryGetValue(key, out ResourcePool pool) && pool.CurrentCapacity > 0)
			{
				var available = pool.AvailableResources.Keys.ToList();
				Type targetItem = (available.Count > 0) ? available[Utility.Random(available.Count)] : GetDefaultItem(type);

				if (targetItem != null)
				{
					var (npcTier, _) = citizen.GetResourceTier(citizen.PrimarySkill);
					int itemTier = GetResourceTierValue(targetItem);

					// 3. [감각(Perception) 반영] NPC 스킬 티어가 낮아도 감각이 극도로 높으면 강등(Downgrade) 방어
					// 감각 100%일 때 50% 확률로 상위 티어 자원 획득 유지
					bool preventDowngrade = (perception > 0) && (Utility.RandomDouble() < (0.5 * perception));

					if (itemTier > npcTier && !preventDowngrade)
					{
						targetItem = GetDefaultItem(type); 
					}

					int actualHarvest = Math.Min(harvestAmount, pool.CurrentCapacity);
					for (int i = 0; i < actualHarvest; i++) pool.ConsumeResource(targetItem);

					citizen.CheckSkillGain();

					if (IsRareResource(targetItem) && citizen.House != null)
					{
						if (!citizen.House.HouseWarehouse.ContainsKey(targetItem))
							citizen.House.HouseWarehouse[targetItem] = 0;
						citizen.House.HouseWarehouse[targetItem] += actualHarvest;
						return (true, 0);
					}

					return ExecuteSell(citizen, town, targetItem, basePrice, actualHarvest);
				}
			}
			return (false, 0);
		}

		// [추가] 7단계 자원 티어 판별 (정적 메서드)
		public static bool IsRareResource(Type type) => GetResourceTierValue(type) > 1;

		public static int GetResourceTierValue(Type type)
		{
			if (type == null) return 1;
			string name = type.Name.ToLower();

			if (name.Contains("valorite") || name.Contains("frostwood") || name.Contains("barbed") || name.Contains("perchfish")) return 7;
			if (name.Contains("verite") || name.Contains("bloodwood") || name.Contains("horned") || name.Contains("codfish")) return 6;
			if (name.Contains("agapite") || name.Contains("heartwood") || name.Contains("spined") || name.Contains("catfish")) return 5;
			if (name.Contains("gold") || name.Contains("yew") || name.Contains("serned") || name.Contains("cruciancarp")) return 4;
			if (name.Contains("bronze") || name.Contains("ash") || name.Contains("ratned") || name.Contains("shiner")) return 3;
			if (name.Contains("copper") || name.Contains("oak") || name.Contains("derned") || name.Contains("bass")) return 2;

			return 1; // Iron, Log, Hides, Trout 등
		}

		// [신규] 상인이 가문 창고의 색자원을 매입하여 귀족에게 납품하는 중개 무역
        public static void ExecuteRareBrokerage(VirtualCitizen merchant, TownEconomy town)
        {
            // 1. 마을 내 색자원을 보유한 평민 가문 탐색
            var supplier = town.Citizens
                .Where(c => c.House != null && c.House.HouseWarehouse.Any(kvp => IsRareResource(kvp.Key)))
                .FirstOrDefault();

            // 2. 해당 자원을 필요로 하는 귀족 탐색
            var noble = town.Citizens
                .Where(c => c.RankLevel >= NobilityRank.Baron && c.Gold > 10000)
                .FirstOrDefault();

            if (supplier == null || noble == null) return;

            var rareItem = supplier.House.HouseWarehouse.First(kvp => IsRareResource(kvp.Key));
            int marketPrice = town.GetPrice(rareItem.Key) * 5; // 색자원은 시장가 5배 적용

            // [중개] 평민에게 매입 -> 귀족에게 판매
            if (merchant.Gold >= marketPrice && noble.Gold >= (int)(marketPrice * 1.5))
            {
                // 매입
                supplier.House.HouseWarehouse[rareItem.Key]--;
                supplier.Family.SharedWealth += marketPrice;
                merchant.Gold -= marketPrice;

                // 판매 (상인 수익 50% 마진)
                int sellPrice = (int)(marketPrice * 1.5);
                merchant.Gold += sellPrice;
                noble.Gold -= sellPrice;

                if (!noble.House.HouseWarehouse.ContainsKey(rareItem.Key))
                    noble.House.HouseWarehouse[rareItem.Key] = 0;
                
                noble.House.HouseWarehouse[rareItem.Key]++;
            }
        }

        // = [에러 해결 1] 누락된 메서드 추가 =
        private static ResourceType GetResourceTypeByJob(NpcJobClass job)
        {
            int id = (int)job;
            if (id >= 100 && id < 110) return ResourceType.Mining;
            if (id >= 110 && id < 120) return ResourceType.Lumberjacking;
            if (id >= 120 && id < 130) return ResourceType.Fishing;
            return ResourceType.Farming;
        }

		// 마을 근처 또는 잠재력에 따른 원정 작업지 예약 로직
        public static void FindWorkPool(VirtualCitizen citizen, TownEconomy town)
        {
            ResourceType type = GetResourceTypeByJob(citizen.JobClass);
            string townName = TownNumber.GetName(town.TownID);

            // 해당 대륙의 모든 동일 타입 자원 풀 탐색
            var pools = ResourceManager.Pools.Values
                .Where(p => p.MapName == town.Facet.Name && p.Type == type);

            ResourcePool bestPool;

            // [기획 반영] 하이 포텐셜(2.5 이상)은 마을 제약 없이 가장 매장량이 많은 곳으로 원정
            if (citizen.Potential >= 2.5)
            {
                bestPool = pools.OrderByDescending(p => p.CurrentCapacity).FirstOrDefault();
            }
            else
            {
                // 일반 시민은 기존처럼 자기 마을 이름이 포함된 지역(안전지대)만 탐색
                bestPool = pools
                    .Where(p => p.RegionName.Contains(townName))
                    .OrderByDescending(p => p.CurrentCapacity)
                    .FirstOrDefault();
            }

            if (bestPool != null)
            {
                citizen.TargetRegionName = bestPool.RegionName;
            }
        }

        private static Type GetDefaultItem(ResourceType type) => type switch
        {
            ResourceType.Mining => typeof(IronOre),
            ResourceType.Lumberjacking => typeof(Log),
            // [수정] 낚시 산출물 기본값을 세분화된 생선으로 변경
            ResourceType.Fishing => typeof(Trout),
            _ => typeof(WheatSheaf)
        };

		// [3번 기획] 가공품 가치 보존 법칙 (PVA) 산출 엔진
		public static int GetPVAGuaranteedPrice(Type itemType, TownEconomy town)
		{
			// 기본 시장가
			int marketPrice = Math.Max(1, town.GetPrice(itemType));

			// 공식: (원재료 시장가 / 산출량) + 최소 공임
			// 주요 중간재 및 가공품에 대한 최소 마진 보장
			if (itemType == typeof(IronIngot)) return CalculatePVA(typeof(IronOre), town, 2, 4, marketPrice);
			if (itemType == typeof(Board)) return CalculatePVA(typeof(Log), town, 1, 3, marketPrice);
			if (itemType == typeof(SackFlour)) return CalculatePVA(typeof(WheatSheaf), town, 1, 5, marketPrice);
			
			// [물 고갈 사태 방지용 공임 세팅]
			if (itemType == typeof(Bottle)) return CalculatePVA(typeof(Sand), town, 2, 5, marketPrice); // 모래 -> 빈병
			if (itemType == typeof(BeverageBottle)) return CalculatePVA(typeof(Bottle), town, 1, 3, marketPrice); // 빈병 -> 물병
			if (itemType == typeof(Pitcher)) return CalculatePVA(typeof(Board), town, 1, 4, marketPrice); // 나무판자 -> 물통

			// 등록되지 않은 일반/야생 품목은 그대로 시장가 반환
			return marketPrice;
		}

		private static int CalculatePVA(Type rawMaterial, TownEconomy town, int yieldRate, int processingFee, int currentMarketPrice)
		{
			// 원재료의 현재 시장가를 가져옵니다.
			int rawCost = Math.Max(1, town.GetPrice(rawMaterial));
			
			// PVA 공식 적용
			int pvaPrice = (rawCost / yieldRate) + processingFee;
			
			// 현재 시장가와 PVA 보장가 중 '더 높은 가격'을 채택하여 시민의 손해를 절대적으로 방어합니다.
			return Math.Max(currentMarketPrice, pvaPrice);
		}


       // ==============================================================================
        // 5. 생산품 마을 납품 (포텐셜 기반 가치 산정 로직 적용)
        // ==============================================================================
		public static (bool Success, int Earnings) ExecuteSell(VirtualCitizen citizen, TownEconomy town, Type itemType, int basePrice, int amount)
		{
			if (itemType == typeof(Gold))
			{
				citizen.Gold += amount;
				return (true, amount);
			}

			// 시민의 심리적 마지노선(최소 판매 희망가) 산출
			var (_, minSell, _) = GetTradeTolerance(citizen, basePrice);
			
			// [3번 기획] PVA 엔진 보장가 산출 (원가 + 공임)
			int guaranteedPrice = GetPVAGuaranteedPrice(itemType, town);
			
			// ==============================================================================
			// [수정] 시민 전용 판매가 공식: 기본 70% + (포텐셜 0.1당 1% 추가)
			// 예: 포텐셜 1.0 -> 80% / 포텐셜 2.0 -> 90% / 포텐셜 3.0 -> 100%
			// ==============================================================================
			double sellRate = 0.70 + (citizen.Potential * 0.10);
			
			// 포텐셜이 3.0을 초과하더라도 구매가(100%)보다 비싸게 팔아 무한 돈 복사가 생기는 것을 방지
			sellRate = Math.Min(1.0, sellRate); 

			// 최종 판매가 산정
			int townBuyPrice = Math.Max(1, (int)(guaranteedPrice * sellRate)); 
			// ==============================================================================

			// 상인이 제시한 가격이 시민의 최소 희망가보다 높거나 같으면 거래 성사
			if (townBuyPrice >= minSell)
			{
				int totalEarnings = townBuyPrice * amount;
				citizen.Gold += totalEarnings;
				town.Wealth -= totalEarnings; 
				
				if (!town.Warehouse.ContainsKey(itemType))
					town.Warehouse[itemType] = new WarehouseItem(itemType, 0, basePrice, 100);
				
				town.Warehouse[itemType].Stock += amount;
				return (true, totalEarnings);
			}

			// 거래 결렬 시 스트레스 소폭 상승
			citizen.Stress = Math.Min(100, citizen.Stress + 2); 
			return (false, 0);
		}

        // ==============================================================================
        // 6. 글로벌 무역 로직 (상인의 짐말 운용 및 마구간 시스템 연동)
        // ==============================================================================
        public static (bool Success, int Profit) ExecuteTradeRoute(VirtualCitizen merchant, TownEconomy currentTown, int baseCapacity)
        {
            if (currentTown == null || TownEconomyManager.Towns.Count < 2) return (false, 0);

            var otherTowns = TownEconomyManager.Towns.Values.Where(t => t.TownID != currentTown.TownID).ToList();
            var targetTown = otherTowns[Random.Shared.Next(otherTowns.Count)];

            var exportCandidates = currentTown.Warehouse.Values
                .Where(w => w.Stock > w.TargetStock && currentTown.GetPrice(w.ItemType) < w.BasePrice)
                .OrderBy(w => currentTown.GetPrice(w.ItemType))
                .ToList();

            if (exportCandidates.Count == 0) return (false, 0);

            var exportItem = exportCandidates.First();
            Type itemType = exportItem.ItemType;
            int buyPrice = currentTown.GetPrice(itemType);
            
            // -------------------------------------------------------------
            // [상인 등급 판별 및 짐말(Pack Animal) 견적 산출]
            // -------------------------------------------------------------
            double itemID = merchant.Skills.ContainsKey(SkillName.ItemID) ? merchant.Skills[SkillName.ItemID] : merchant.PrimarySkill;
            int maxAnimals = 1; // 초급 1마리
            if (itemID >= 80.0 && merchant.Potential >= 2.5) maxAnimals = 5; // 거상 5마리
            else if (itemID >= 50.0 && merchant.Potential >= 1.5) maxAnimals = 3; // 중견 3마리

            int animalCost = 1000; // 짐말/짐라마 구입비
            int stableFee = 50;    // 동물 조련사 보관료
            int theoreticalMaxCapacity = baseCapacity + (maxAnimals * 400); // 짐말 1마리당 +400개

            int maxAffordable = merchant.Gold / Math.Max(1, buyPrice);
            int amountToTrade = Math.Min(theoreticalMaxCapacity, Math.Min(exportItem.Stock, maxAffordable));
            
            if (amountToTrade <= 0) return (false, 0);

            int animalsNeeded = 0;
            if (amountToTrade > baseCapacity)
            {
                animalsNeeded = (int)Math.Ceiling((amountToTrade - baseCapacity) / 400.0);
                animalsNeeded = Math.Min(animalsNeeded, maxAnimals);
                
                int totalAnimalCost = animalsNeeded * animalCost;
                
                // 짐말 구입비가 모자라면, 수량을 깎아서라도 맞춤
                if (merchant.Gold < totalAnimalCost + (buyPrice * amountToTrade))
                {
                    amountToTrade = (merchant.Gold - totalAnimalCost) / Math.Max(1, buyPrice);
                    if (amountToTrade <= 0) return (false, 0);
                }
                merchant.Gold -= totalAnimalCost;
                currentTown.Wealth += totalAnimalCost;
                Console.WriteLine($"[TradeAI] 상인 '{merchant.Name}'이 짐말 {animalsNeeded}마리를 대여했습니다.");
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

            // -------------------------------------------------------------
            // [실거래 및 마구간 처분 로직]
            // -------------------------------------------------------------
            merchant.Gold -= totalCost;
            currentTown.Wealth += totalCost;
            currentTown.Warehouse[itemType].Stock -= amountToTrade;

            int totalRevenue = sellPrice * amountToTrade;
            merchant.Gold += totalRevenue;
            targetTown.Wealth -= totalRevenue; 

            // 무역 종료 후 짐말 처분
            if (animalsNeeded > 0)
            {
                // 마을에 조련사, 마구간지기, 중개인이 있는지 확인
                var tamer = currentTown.Citizens.FirstOrDefault(c => 
                    c.JobClass == NpcJobClass.StableBroker || 
                    c.JobClass == NpcJobClass.StableHand || 
                    c.JobClass == NpcJobClass.AnimalTamer_Warrior);

                if (tamer != null)
                {
                    // 마구간에 보관료 지불 (정상 보관)
                    int totalFee = animalsNeeded * stableFee;
                    merchant.Gold -= totalFee;
                    tamer.Gold += totalFee; 
                }
                else
                {
                    // 마구간이 없으면 야생에 반값에 처분 (손해)
                    int refund = (animalsNeeded * animalCost) / 2;
                    merchant.Gold += refund;
                    currentTown.Wealth -= refund; // 마을 경제로 흡수
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
    }
}