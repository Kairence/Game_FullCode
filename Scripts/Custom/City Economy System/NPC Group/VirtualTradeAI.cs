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

        public static (bool Success, int Spent) ExecutePurchase(VirtualCitizen citizen, TownEconomy town, Type itemType, int basePrice)
        {
            if (itemType == typeof(Gold)) return (true, 0);
            var tolerance = GetTradeTolerance(citizen, basePrice);
            int townPrice = town.GetPrice(itemType); 

            if (town.Warehouse.ContainsKey(itemType) && town.Warehouse[itemType].Stock > 0)
            {
                if (townPrice <= tolerance.MaxBuyPrice && citizen.Gold >= townPrice)
                {
                    citizen.Gold -= townPrice;
                    town.Wealth += townPrice;
                    town.Warehouse[itemType].Stock--;
                    return (true, townPrice);
                }
            }

            var vendorResult = SearchPlayerVendors(citizen, town, itemType, tolerance.MaxBuyPrice);
            if (vendorResult.Success) return vendorResult;

            int stressPenalty = 5 + ((int)citizen.RankLevel * 2) + (int)(tolerance.Desire * 2);
            citizen.Stress = Math.Min(100, citizen.Stress + stressPenalty);
            
            return (false, 0);
        }

        private static (bool Success, int Spent) SearchPlayerVendors(VirtualCitizen citizen, TownEconomy town, Type targetType, int maxPrice)
        {
            var map = town.Facet; 
            if (map == null || map == Map.Internal) return (false, 0);

            int[] searchRadii = [20, 50, 100]; 
            var checkedVendors = new HashSet<PlayerVendor>();

            foreach (int radius in searchRadii)
            {
                var eable = map.GetMobilesInRange(town.Center, radius);
                foreach (var mob in eable)
                {
                    if (mob is PlayerVendor vendor && checkedVendors.Add(vendor))
                    {
                        if (vendor.Backpack == null) continue;
                        foreach (var item in vendor.Backpack.Items.ToArray())
                        {
                            if (item.GetType() == targetType)
                            {
                                var vi = vendor.GetVendorItem(item);
                                if (vi != null && vi.Price <= maxPrice && citizen.Gold >= vi.Price)
                                {
                                    int price = vi.Price;
                                    citizen.Gold -= price;
                                    vendor.HoldGold += price;
                                    item.Delete();
                                    eable.Free();
                                    return (true, price);
                                }
                            }
                        }
                    }
                }
                eable.Free();
            }
            return (false, 0);
        }

		// ==============================================================================
        // 4. [최종] 자원 추출 로직: Pool에서 직접 땡겨오기
        // ==============================================================================
        public static (bool Success, int Earnings) ExecuteHarvestAndSell(VirtualCitizen citizen, TownEconomy town, int basePrice)
		{
			// [1] 생산 성공 확률 계산 (기획 반영: 스킬 0=20%, 200=100%)
			// (0.8 * 비율) + 0.2 기본 확률
			double successChance = 0.2 + (0.8 * (citizen.PrimarySkill / 200.0));
			
			if (Utility.RandomDouble() > successChance)
			{
				// 실패 시 스트레스 약간 증가
				citizen.Stress = Math.Min(100, citizen.Stress + 5);
				return (false, 0); 
			}

			// [2] 생산량 결정: 유저 효율의 20%인 '6개' 고정
			// 현실 1분(게임 6시간 루틴) 동안 채집하는 총량입니다.
			int harvestAmount = 6; 

			// [3] 예약된 작업지(Region) 확인 및 자원 풀 접근
			if (string.IsNullOrEmpty(citizen.TargetRegionName))
			{
				FindWorkPool(citizen, town);
			}

			if (string.IsNullOrEmpty(citizen.TargetRegionName)) return (false, 0);

			ResourceType type = GetResourceTypeByJob(citizen.JobClass);
			ResourceKey key = new ResourceKey(town.Facet.Name, citizen.TargetRegionName, type);

			if (ResourceManager.Pools.TryGetValue(key, out ResourcePool pool) && pool.CurrentCapacity > 0)
			{
				var available = pool.AvailableResources.Keys.ToList();
				Type targetItem = (available.Count > 0) ? available[Utility.Random(available.Count)] : GetDefaultItem(type);

				if (targetItem != null)
				{
					// [4] 실제 월드 자원 소모 (최대 6개)
					int actualHarvest = Math.Min(harvestAmount, pool.CurrentCapacity);
					for (int i = 0; i < actualHarvest; i++) 
					{
						pool.ConsumeResource(targetItem);
					}

					// [5] 스킬 상승 체크 및 마을 창고 납품
					// 납품 시 가격은 town.GetPrice를 통해 동적으로 결정됨
					citizen.CheckSkillGain(); 
					return ExecuteSell(citizen, town, targetItem, basePrice, actualHarvest);
				}
			}

			citizen.Stress = Math.Min(100, citizen.Stress + 2);
			return (false, 0);
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

        // 마을 근처의 적절한 공용 Pool을 찾아 이름표를 예약함
        public static void FindWorkPool(VirtualCitizen citizen, TownEconomy town)
        {
            ResourceType type = GetResourceTypeByJob(citizen.JobClass);
            string townName = TownNumber.GetName(town.TownID); //

            // 이름에 마을명이 포함된 해당 타입의 Pool 검색
            var bestPool = ResourceManager.Pools.Values
                .Where(p => p.MapName == town.Facet.Name && 
                            p.Type == type && 
                            p.RegionName.Contains(townName))
                .OrderByDescending(p => p.CurrentCapacity)
                .FirstOrDefault();

            if (bestPool != null)
            {
                citizen.TargetRegionName = bestPool.RegionName;
            }
        }

        private static Type GetDefaultItem(ResourceType type) => type switch
        {
            ResourceType.Mining => typeof(IronOre),
            ResourceType.Lumberjacking => typeof(Log),
            ResourceType.Fishing => typeof(RawFishSteak),
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
        // 5. 생산품 마을 납품 (기존 로직 유지)
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
			// [4번 기획] 가변 도매가 시스템 (Dynamic Wholesale)
			// ==============================================================================
			// (out 키워드 금지 규칙 적용)
			int currentStock = town.Warehouse.ContainsKey(itemType) ? town.Warehouse[itemType].Stock : 0;
			int targetStock = town.Warehouse.ContainsKey(itemType) ? town.Warehouse[itemType].TargetStock : 100;
			
			double wholesaleRate = 0.65; // 기본 도매율 65% (기존 50% 폐지)

			// 1. 마을 자산 연동 보정 (마을이 부유할수록 매입 여력 상승)
			if (town.Wealth > 100000) wholesaleRate += 0.10;
			else if (town.Wealth > 50000) wholesaleRate += 0.05;

			// 2. 재고 보정 (수요와 공급 법칙)
			if (targetStock > 0)
			{
				double stockRatio = (double)currentStock / targetStock;
				
				if (stockRatio < 0.1) wholesaleRate = 0.85;      // 품귀: 85% 최고가 매입 (생산 폭발적 독려)
				else if (stockRatio > 1.5) wholesaleRate = 0.50; // 과잉: 50% 덤핑 매입 (다른 물품 생산 유도)
			}

			// 최종 도매가 산정: (PVA 보장가 * 가변 도매율)
			int townBuyPrice = Math.Max(1, (int)(guaranteedPrice * wholesaleRate)); 
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
        // 6. [신규] 글로벌 무역 로직 (마을 간 차익 거래 및 시장 기준치 변형)
        // ==============================================================================
        public static (bool Success, int Profit) ExecuteTradeRoute(VirtualCitizen merchant, TownEconomy currentTown, int maxCapacity)
        {
            if (currentTown == null || TownEconomyManager.Towns.Count < 2) return (false, 0);

            // 1. 무역 대상 도시 선정 (현재 내 도시가 아닌 무작위 도시)
            var otherTowns = TownEconomyManager.Towns.Values.Where(t => t.TownID != currentTown.TownID).ToList();
            var targetTown = otherTowns[Random.Shared.Next(otherTowns.Count)];

            // 2. 현재 도시에서 '악성 재고(공급 과잉으로 가격이 폭락한 물건)' 탐색
            var exportCandidates = currentTown.Warehouse.Values
                .Where(w => w.Stock > w.TargetStock && currentTown.GetPrice(w.ItemType) < w.BasePrice)
                .OrderBy(w => currentTown.GetPrice(w.ItemType)) // 제일 싼 것부터 긁어모음
                .ToList();

            if (exportCandidates.Count == 0) return (false, 0);

            // 무역할 아이템 선정 및 구매 견적 산출
            var exportItem = exportCandidates.First();
            Type itemType = exportItem.ItemType;
            int buyPrice = currentTown.GetPrice(itemType);
            
            // 상인의 자금과 [운송 용량(maxCapacity)]에 맞춰 매입량 결정
            int maxAffordable = merchant.Gold / Math.Max(1, buyPrice);
            int amountToTrade = Math.Min(maxCapacity, Math.Min(exportItem.Stock, maxAffordable));
            
            if (amountToTrade <= 0) return (false, 0);
            int totalCost = buyPrice * amountToTrade;

            // 3. 타겟 도시의 시장 조사 (판매가 산출)
            int sellPrice = 0;
            bool isNewProduct = false;

            if (targetTown.Warehouse.ContainsKey(itemType))
			{
				sellPrice = targetTown.GetPrice(itemType);
				if (sellPrice <= buyPrice) return (false, 0);
			}
			else
			{
				// [기획 반영] 대상이 C등급(전초기지)이라면, 취급하지 않는 신규 물품은 절대 사주지 않음 (무역 거부)
				if (targetTown.TownIndex == "C") return (false, 0);

				isNewProduct = true;
				sellPrice = (int)(buyPrice * 1.5);
			}

            // 4. 무역 성사! (현재 도시에서 구매 -> 타겟 도시에 판매)
            
            // [구매 처리]
            merchant.Gold -= totalCost;
            currentTown.Wealth += totalCost;
            currentTown.Warehouse[itemType].Stock -= amountToTrade;

            // [판매 처리]
            int totalRevenue = sellPrice * amountToTrade;
            merchant.Gold += totalRevenue;
            targetTown.Wealth -= totalRevenue; // 타겟 도시의 금고에서 돈이 빠져나감

            if (isNewProduct)
            {
                // 신규 물품 런칭 (들어온 수량을 최초 적정 재고로 세팅)
                targetTown.Warehouse[itemType] = new WarehouseItem(itemType, amountToTrade, sellPrice, amountToTrade);
            }
            else
            {
                targetTown.Warehouse[itemType].Stock += amountToTrade;

                // ==============================================================================
                // [기획 반영] 시장 기준값 변형 (Market Mutation)
                // ==============================================================================
                var targetItem = targetTown.Warehouse[itemType];

                // 1. 시장 확장: 무역로가 뚫리면서 해당 물품에 대한 타겟 도시의 근본적인 수요(TargetStock)가 늘어남
                targetItem.TargetStock += Math.Max(1, amountToTrade / 5);

                // 2. 기준가 동화: 상인이 물건을 지속적으로 공급하면 타겟 도시의 영구적인 기준가가 이번 거래가에 영향을 받아 미세하게(5%) 변함
                int currentBase = targetItem.BasePrice;
                int mutatedBasePrice = currentBase + (int)((sellPrice - currentBase) * 0.05);
                targetItem.BasePrice = Math.Max(1, mutatedBasePrice);
            }

            // 엄청난 이문을 남겼으므로 스트레스 대폭 해소
            merchant.Stress = Math.Max(0, merchant.Stress - 20);
            return (true, totalRevenue - totalCost);
        }
    }
}