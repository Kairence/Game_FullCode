using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Commands;

namespace Server.Misc
{
    public enum TradeType { Buy, Sell, Export, Import }
    public enum TraderType { System, NPC, Player }

    public class TradeRecord
    {
        public DateTime Timestamp { get; set; }
        public string TraderName { get; set; }
        public TraderType ActorType { get; set; }
        public TradeType Action { get; set; }
        public EconomyItemKey ItemKey { get; set; }
        public int Amount { get; set; }
        public int UnitPrice { get; set; }
        public int TotalCost => Amount * UnitPrice;

        public TradeRecord(string trader, TraderType actor, TradeType action, EconomyItemKey item, int amount, int unitPrice)
        {
            Timestamp = DateTime.Now;
            TraderName = trader;
            ActorType = actor;
            Action = action;
            ItemKey = item;
            Amount = amount;
            UnitPrice = unitPrice;
        }
    }

    public static class VirtualTradeSystem
    {
        public static Dictionary<int, Queue<TradeRecord>> MasterLedger = new Dictionary<int, Queue<TradeRecord>>();

        public static void Initialize()
        {
            CommandSystem.Register("경매", AccessLevel.Player, new CommandEventHandler(OnMarketSearch));
            CommandSystem.Register("성인강제성장", AccessLevel.Administrator, new CommandEventHandler(OnAgeUpCitizens));
        }

        [Usage("성인강제성장")]
        private static void OnAgeUpCitizens(CommandEventArgs e)
        {
            if (TownEconomyManager.Towns == null) return;
            int count = 0;
            foreach (var town in TownEconomyManager.Towns.Values)
            {
                if (town == null || town.Citizens == null) continue;
                foreach (var citizen in town.Citizens)
                {
                    if (citizen != null && citizen.IsChild)
                    {
                        int adultAge = Utility.RandomMinMax(20, 35);
                        citizen.BirthTime = DateTime.Now - TimeSpan.FromMinutes(adultAge * VirtualCitizen.GameYearMinutes);
                        citizen.Gold += 500; 
                        count++;
                    }
                }
            }
            e.Mobile.SendMessage(68, $"총 {count}명의 영유아가 성인으로 성장하였습니다.");
        }

        public static void LogTrade(TownEconomy town, string traderName, TraderType actor, TradeType action, EconomyItemKey itemKey, int amount, int unitPrice)
        {
            try
            {
                if (town == null || amount <= 0 || MasterLedger == null) return;
                if (!MasterLedger.ContainsKey(town.TownID)) MasterLedger[town.TownID] = new Queue<TradeRecord>();

                var ledger = MasterLedger[town.TownID];
                if (ledger == null) return;
                
                while (ledger.Count >= 1000) ledger.Dequeue();
                ledger.Enqueue(new TradeRecord(traderName ?? "Unknown", actor, action, itemKey, amount, unitPrice));
            }
            catch { }
        }

        public static (CraftResource Res, int SubID, bool IsExc) GetResourceAndQuality(Item item)
        {
            CraftResource turnInRes = CraftResource.None;
            int subID = 0;
            bool isExc = false;
            if (item == null) return (turnInRes, subID, isExc);

            try
            {
                var prop = item.GetType().GetProperty("Resource");
                if (prop != null)
                {
                    var resVal = prop.GetValue(item);
                    if (resVal is CraftResource cr) turnInRes = cr;
                }

                if (item is BaseBeverage bev) subID = (int)bev.Content;

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
            }
            catch { }
            return (turnInRes, subID, isExc);
        }

        public static (int MaxBuyPrice, int MinSellPrice, double Desire) GetTradeTolerance(VirtualCitizen citizen, int basePrice)
        {
            if (citizen == null) return (basePrice, 1, 1.0);
            int rankBonus = (int)citizen.RankLevel; 
            double wealthFactor = citizen.Gold / (double)Math.Max(1, basePrice * 5);
            double desire = 1.0 + (rankBonus * 0.3) + Math.Min(4.0, wealthFactor);
            double stressFactor = 1.0 + (citizen.Stress / 100.0); 

            int maxBuy = (int)(basePrice * desire * stressFactor);
            int minSell = (int)(basePrice * (0.6 - (stressFactor * 0.1)));
            return (Math.Max(basePrice, maxBuy), Math.Max(1, minSell), desire);
        }

        public static (bool Success, int Spent) ExecutePurchase(VirtualCitizen citizen, TownEconomy town, EconomyItemKey itemKey, int basePrice, int requestedAmount = 0, bool isDirectRequest = false)
        {
            try
            {
                if (citizen == null || town == null || itemKey.ItemType == null || itemKey.ItemType == typeof(Gold)) return (false, 0);
                
                var tolerance = GetTradeTolerance(citizen, basePrice);
                bool isMerchant = ((int)citizen.JobClass / 100) * 100 == 600;

                int desiredAmount = requestedAmount > 0 ? requestedAmount : (isMerchant ? (int)(500 * citizen.Potential) : (int)Math.Max(1, citizen.Potential * (2 + (int)citizen.RankLevel)));
                int spaceLeft = 10; 

                if (isMerchant) spaceLeft = 5000;
                else if (citizen.House != null && citizen.House.Interior != null && citizen.House.Interior.PlacedFurniture != null)
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
                    spaceLeft = maxCap - currentItems;
                }
                
                int finalRequestAmount = isDirectRequest ? desiredAmount : Math.Min(desiredAmount, spaceLeft);
                if (finalRequestAmount <= 0) return (false, 0);

                bool checkVendorFirst = isDirectRequest || Utility.RandomDouble() < (citizen.Potential / 4.0) || isMerchant;

                if (checkVendorFirst)
                {
                    var vendorResult = SearchPlayerVendors(citizen, town, itemKey, tolerance.MaxBuyPrice, finalRequestAmount, isDirectRequest);
                    if (vendorResult.Success) return vendorResult;
                }

                if (town.Warehouse != null && town.Warehouse.TryGetValue(itemKey, out var wItem) && wItem != null && wItem.Stock > 0)
                {
                    int townPrice = town.GetPrice(itemKey);
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

                            LogTrade(town, citizen.Name, TraderType.NPC, TradeType.Buy, itemKey, buyAmount, townPrice);

                            if (citizen.House != null && !isMerchant && !isDirectRequest)
                            {
                                Item boughtItem = (Item)Activator.CreateInstance(itemKey.ItemType);
                                if (boughtItem != null)
                                {
                                    for (int k = 0; k < buyAmount; k++)
                                    {
                                        Item bItem = (k == 0) ? boughtItem : (Item)Activator.CreateInstance(itemKey.ItemType);
                                        if (bItem == null) continue;
                                        
                                        if (bItem is BaseBeverage bev2) 
										{
											if (itemKey.SubID != 0) bev2.Content = (BeverageType)itemKey.SubID;
											else bev2.Content = BeverageType.Water; // 지정 안됐으면 기본 물
											
											// 🌟 [패치] 상인이 사기치지 못하게 내용물(Quantity)을 가득 채워줌!
											bev2.Quantity = bev2.MaxQuantity; 
										}
                                        if (itemKey.Resource != CraftResource.None)
                                        {
                                            var resProp = bItem.GetType().GetProperty("Resource");
                                            if (resProp != null) resProp.SetValue(bItem, itemKey.Resource);
                                        }
                                        if (itemKey.IsExceptional && bItem is IQuality q2) q2.Quality = ItemQuality.Exceptional;

                                        if (bItem is BaseContainer) PhysicalStorageEngine.PlaceFurniture(citizen.House, bItem);
                                        else if (!PhysicalStorageEngine.TryStoreItem(citizen.House, bItem)) 
										{
											if (citizen.House.EstateSign != null)
												bItem.MoveToWorld(citizen.House.EstateSign.Location, citizen.House.EstateSign.Map);
											else if (citizen.Map != null && citizen.Map != Map.Internal)
												bItem.MoveToWorld(citizen.Location, citizen.Map); // 간판이 없으면 임시로 발밑에 드랍
											else
												bItem.Delete(); // 시민도 맵에 없으면 템 삭제 (크래시 방지)
										}
                                        if (bItem.Stackable) { bItem.Amount = buyAmount; break; }
                                    }
                                }
                            }
                            return (true, totalCost);
                        }
                    }
                }

                if (!checkVendorFirst) return SearchPlayerVendors(citizen, town, itemKey, tolerance.MaxBuyPrice, finalRequestAmount, isDirectRequest);
            }
            catch (Exception ex) { Console.WriteLine($"[VTS Error - ExecutePurchase] {ex.Message}"); }
            return (false, 0);
        }

        private static (bool Success, int Spent) SearchPlayerVendors(VirtualCitizen citizen, TownEconomy town, EconomyItemKey targetKey, int maxPricePerItem, int requiredAmount, bool isDirectRequest)
        {
            try
            {
                if (citizen == null || town == null || town.Facet == null || targetKey.ItemType == null || town.Facet == Map.Internal) return (false, 0);

                bool isMerchant = ((int)citizen.JobClass / 100) * 100 == 600;
                int availableSpace = isMerchant ? 1000 : 10;
                
                if (!isMerchant && citizen.House != null && citizen.House.Interior != null && citizen.House.Interior.PlacedFurniture != null)
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
                        if (v == null || v.Map != town.Facet || v.Deleted || v.Backpack == null) continue;
                        double dist = Utility.GetDistanceToSqrt(town.Center, v.Location);
                        if (dist <= 100.0) allVendors.Add((v, dist, false));
                    }
                }

                if (RetailVendor.RetailVendors != null)
                {
                    for (int i = 0; i < RetailVendor.RetailVendors.Count; i++)
                    {
                        var v = RetailVendor.RetailVendors[i];
                        if (v == null || v.Map != town.Facet || v.Deleted) continue;
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
                        if (vendor == null || vendor.MarketItems == null) continue;

                        for (int i = 0; i < vendor.MarketItems.Count; i++)
                        {
                            var mItem = vendor.MarketItems[i];
                            if (mItem == null || mItem.RealItem == null || mItem.RealItem.Deleted) continue;

                            var (res, subID, isExc) = GetResourceAndQuality(mItem.RealItem);
                            if (mItem.RealItem.GetType() != targetKey.ItemType || res != targetKey.Resource || isExc != targetKey.IsExceptional || subID != targetKey.SubID) continue;
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
                                LogTrade(town, citizen.Name, TraderType.NPC, TradeType.Buy, targetKey, buyAmount, mItem.PricePerUnit);

                                if (isMerchant) 
                                {
                                    ExecuteSell(citizen, town, targetKey, maxPricePerItem, buyAmount);
                                    boughtItem.Delete(); 
                                }
                                else if (citizen.House != null && !isDirectRequest)
                                {
                                    if (!PhysicalStorageEngine.TryStoreItem(citizen.House, boughtItem))
									{
										if (citizen.House.EstateSign != null)
											boughtItem.MoveToWorld(citizen.House.EstateSign.Location, citizen.House.EstateSign.Map);
										else if (citizen.Map != null && citizen.Map != Map.Internal)
											boughtItem.MoveToWorld(citizen.Location, citizen.Map);
										else
											boughtItem.Delete();
									}
                                }
                                else boughtItem.Delete(); 
                                
                                return (true, totalCost);
                            }
                        }
                    }
                    else
                    {
                        PlayerVendor vendor = vData.Vendor as PlayerVendor;
                        if (vendor == null || vendor.Backpack == null) continue;

                        var itemsToCheck = new List<Item>();
                        var containersToSearch = new Queue<Container>();
                        containersToSearch.Enqueue(vendor.Backpack);

                        while (containersToSearch.Count > 0)
                        {
                            var currentContainer = containersToSearch.Dequeue();
                            if (currentContainer != null && currentContainer.Items != null)
                            {
                                foreach (var item in currentContainer.Items)
                                {
                                    if (item == null) continue;
                                    if (item.GetType() == targetKey.ItemType) itemsToCheck.Add(item);
                                    else if (item is Container sub) containersToSearch.Enqueue(sub);
                                }
                            }
                        }

                        foreach (var item in itemsToCheck)
                        {
                            if (item == null) continue;
                            var (res, subID, isExc) = GetResourceAndQuality(item);
                            if (item.GetType() != targetKey.ItemType || res != targetKey.Resource || isExc != targetKey.IsExceptional || subID != targetKey.SubID) continue;

                            var vi = vendor.GetVendorItem(item);
                            if (vi == null || vi.Price <= 0 || (!isDirectRequest && item.Amount > availableSpace)) continue;

                            int npcBudgetForThisStack = adjustedMaxPrice * item.Amount;

                            if (vi.Price <= npcBudgetForThisStack && vi.Price <= citizen.Gold)
                            {
                                int totalCost = vi.Price;
                                citizen.Gold -= totalCost;
                                vendor.HoldGold += totalCost;

                                int unitPriceForLog = totalCost / Math.Max(1, item.Amount);
                                LogTrade(town, citizen.Name, TraderType.NPC, TradeType.Buy, targetKey, item.Amount, unitPriceForLog);

                                if (isMerchant) 
                                {
                                    ExecuteSell(citizen, town, targetKey, maxPricePerItem, item.Amount);
                                    item.Delete();
                                }
                                else if (citizen.House != null && !isDirectRequest)
                                {
                                    if (item.Parent is Container parent) parent.RemoveItem(item);
                                    // 🌟 무단투기 패치: 여기도 집 앞 대문으로 배송
                                    if (!PhysicalStorageEngine.TryStoreItem(citizen.House, item))
									{
										if (citizen.House.EstateSign != null)
											item.MoveToWorld(citizen.House.EstateSign.Location, citizen.House.EstateSign.Map);
										else if (citizen.Map != null && citizen.Map != Map.Internal)
											item.MoveToWorld(citizen.Location, citizen.Map);
										else
											item.Delete();
									}
                                }
                                else item.Delete(); 
                                
                                return (true, totalCost);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"[VTS Error - SearchPlayerVendors] {ex.Message}"); }
            return (false, 0);
        }
		public static void UpdateHouseWishlist(VirtualHouse house)
        {
            try
            {
                if (house == null || !house.IsActive || house.Families == null || house.Families.Count == 0) return;

                if (house.TargetStockProfile == null) house.TargetStockProfile = new Dictionary<EconomyItemKey, int>();
                house.TargetStockProfile.Clear();

                int totalMembers = house.Families.Where(f => f != null && f.IsActive).Sum(f => (f.Father != null ? 1 : 0) + (f.Mother != null ? 1 : 0) + (f.Children != null ? f.Children.Count : 0));

				house.TargetStockProfile[typeof(BreadLoaf)] = totalMembers * 21;
                house.TargetStockProfile[new EconomyItemKey(typeof(Pitcher), CraftResource.None, (int)BeverageType.Water)] = totalMembers * 4;
                house.TargetStockProfile[typeof(Candle)] = 10;
                house.TargetStockProfile[typeof(Bandage)] = totalMembers * 10;

                var profile = VirtualJobCore.GetDeepJobProfile(house.PrimaryJob);
                if (profile != null && profile.JobMaterials != null)
                {
                    foreach (EconomyItemKey toolKey in profile.JobMaterials)
                    {
                        if (toolKey.ItemType != null && toolKey.ItemType.IsSubclassOf(typeof(Item))) 
                            house.TargetStockProfile[toolKey] = 2; 
                    }
                }

                if (house.MultiID > 0 && house.TotalWealth > 15000)
                {
                    Type neededAddon = GetDesiredAddonForSkill(profile != null ? profile.Skill : SkillName.Alchemy);
                    if (neededAddon != null) house.TargetStockProfile[neededAddon] = 1;
                }

                int maxCapacity = 0, currentItems = 0;
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
            catch (Exception ex) { Console.WriteLine($"[VTS Error - UpdateHouseWishlist] {ex.Message}"); }
        }

        public static void ProcessHoardingShopping(VirtualCitizen agent, TownEconomy town)
        {
            try
            {
                if (agent == null || agent.House == null || agent.Gold < 100 || town == null) return;

                var house = agent.House;
                if (house.TargetStockProfile == null) house.TargetStockProfile = new Dictionary<EconomyItemKey, int>();
                if (house.UnfulfilledNeeds == null) house.UnfulfilledNeeds = new Dictionary<EconomyItemKey, int>();

                int totalAmountToBuy = 0;
                foreach (var kvp in house.TargetStockProfile)
                {
                    if (kvp.Key.ItemType == null) continue;
                    int currentAmount = PhysicalStorageEngine.GetTotalItemCount(house, kvp.Key.ItemType);
                    if (kvp.Value > currentAmount) totalAmountToBuy += (kvp.Value - currentAmount);
                }

                if (totalAmountToBuy <= 0) return;

                int carryLimit = 50; 
                if (totalAmountToBuy > carryLimit)
                {
                    int animalsNeeded = (int)Math.Ceiling(totalAmountToBuy / 400.0);
                    int totalFee = animalsNeeded * 500;
                    if (agent.Gold >= totalFee)
                    {
                        agent.Gold -= totalFee;
                        town.Wealth += totalFee;
                    }
                    else totalAmountToBuy = carryLimit; 
                }

                int boughtSoFar = 0;
                var wishlist = house.TargetStockProfile.ToList();

                foreach (var kvp in wishlist)
                {
                    if (boughtSoFar >= totalAmountToBuy) break;
                    if (kvp.Key.ItemType == null) continue;

                    EconomyItemKey itemKey = kvp.Key;
                    int targetAmount = kvp.Value;
                    int currentAmount = PhysicalStorageEngine.GetTotalItemCount(house, itemKey.ItemType);
                    int amountNeeded = targetAmount - currentAmount;

                    if (amountNeeded > 0)
                    {
                        int amountToBuy = Math.Min(amountNeeded, totalAmountToBuy - boughtSoFar);
                        int basePrice = Math.Max(1, town.GetPrice(itemKey));
                        
                        var result = ExecutePurchase(agent, town, itemKey, basePrice, amountToBuy);
                        if (result.Success)
                        {
                            agent.Satisfaction = Math.Min(100, agent.Satisfaction + 5);
                            boughtSoFar += amountToBuy;
                            if (house.UnfulfilledNeeds.ContainsKey(itemKey)) house.UnfulfilledNeeds.Remove(itemKey);
                        }
                        else
                        {
                            if (!house.UnfulfilledNeeds.ContainsKey(itemKey)) house.UnfulfilledNeeds[itemKey] = 0;
                            house.UnfulfilledNeeds[itemKey] += amountToBuy;
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"[VTS Error - ProcessHoardingShopping] {ex.Message}"); }
        }

        public static void GenerateAIJobRequests(VirtualHouse house, TownEconomy town)
        {
            try
            {
                if (house == null || house.UnfulfilledNeeds == null || house.UnfulfilledNeeds.Count == 0 || town == null || PartTimeManager.ActiveRequests == null) return;

                EconomyItemKey[] keys = new EconomyItemKey[house.UnfulfilledNeeds.Count];
                house.UnfulfilledNeeds.Keys.CopyTo(keys, 0);

                for (int i = 0; i < keys.Length; i++)
                {
                    EconomyItemKey itemKey = keys[i];
                    if (itemKey.ItemType == null) continue;

                    int amount = house.UnfulfilledNeeds[itemKey];
                    if (amount <= 0) continue;

                    int unitPrice = town.GetPrice(itemKey);
                    int totalReward = (unitPrice * amount) * 2;
                    if (house.TotalWealth < totalReward) continue;

                    string title = string.Format("[긴급 납품] {0} 가문의 의뢰", house.HouseName ?? "알 수 없는");
                    JobCategory cat = GetCategoryForItem(itemKey);
                    
                    bool alreadyPosted = false;
                    for (int j = 0; j < PartTimeManager.ActiveRequests.Count; j++)
                    {
                        TownJobRequest req = PartTimeManager.ActiveRequests[j];
                        if (req != null && req.TownName == town.TownName && req.TargetType == itemKey.ItemType && !req.IsFullyBooked && req.IssuerHouse == house)
                        {
                            alreadyPosted = true;
                            break;
                        }
                    }

                    if (!alreadyPosted)
                    {
                        PartTimeManager.CreateAIRequest(town.TownName ?? "", title, cat, itemKey.ItemType, amount, totalReward, house);
                        house.TotalWealth -= totalReward;
                    }
                }
                house.UnfulfilledNeeds.Clear();
            }
            catch (Exception ex) { Console.WriteLine($"[VTS Error - GenerateAIJobRequests] {ex.Message}"); }
        }

        private static JobCategory GetCategoryForItem(EconomyItemKey key)
        {
            Type t = key.ItemType;
            if (t == null) return JobCategory.Menial;
            if (t.IsSubclassOf(typeof(BaseArmor)) || t.IsSubclassOf(typeof(BaseWeapon)) || t.Name.Contains("Deed")) return JobCategory.Crafting;
            if (t == typeof(IronOre) || t == typeof(Log) || t == typeof(WheatSheaf)) return JobCategory.Gathering;
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

        public static int GetPVAGuaranteedPrice(EconomyItemKey itemKey, TownEconomy town)
        {
            if (town == null || itemKey.ItemType == null) return 1;
            int marketPrice = Math.Max(1, town.GetPrice(itemKey));

            try
            {
                if (itemKey.ItemType == typeof(IronIngot)) return CalculatePVA(typeof(IronOre), town, 2, 4, marketPrice);
                if (itemKey.ItemType == typeof(Board)) return CalculatePVA(typeof(Log), town, 1, 3, marketPrice);
                if (itemKey.ItemType == typeof(SackFlour)) return CalculatePVA(typeof(WheatSheaf), town, 1, 5, marketPrice);
                if (itemKey.ItemType == typeof(Bottle)) return CalculatePVA(typeof(Sand), town, 2, 5, marketPrice); 
                if (itemKey.ItemType == typeof(BeverageBottle)) return CalculatePVA(typeof(Bottle), town, 1, 3, marketPrice); 
                if (itemKey.ItemType == typeof(Pitcher)) return CalculatePVA(typeof(Board), town, 1, 4, marketPrice); 
            }
            catch { }
            return marketPrice;
        }

        private static int CalculatePVA(EconomyItemKey rawKey, TownEconomy town, int yieldRate, int processingFee, int currentMarketPrice)
        {
            if (town == null || yieldRate <= 0) return currentMarketPrice;
            int rawCost = Math.Max(1, town.GetPrice(rawKey));
            int pvaPrice = (rawCost / yieldRate) + processingFee;
            return Math.Max(currentMarketPrice, pvaPrice);
        }

        public static (bool Success, int Earnings) ExecuteHarvestAndSell(VirtualCitizen citizen, TownEconomy town, int basePrice)
        {
            try
            {
                if (citizen == null || town == null || town.Facet == null || ResourceManager.Pools == null) return (false, 0);

                double focus = citizen.Bio != null ? Math.Max(0, citizen.Bio.Focus / 1000000.0) : 0;
                double perception = citizen.Bio != null ? Math.Max(0, citizen.Bio.Perception / 1000000.0) : 0;
                double adaptability = citizen.Bio != null ? Math.Max(0, citizen.Bio.Adaptability / 1000000.0) : 0;

                double successChance = 0.4 + (0.6 * (citizen.PrimarySkill / 200.0)) + (0.1 * focus);
                if (Utility.RandomDouble() > successChance) return (false, 0);

                int baseHarvest = (int)(6 * citizen.Potential);
                int harvestAmount = baseHarvest + (int)(baseHarvest * (0.5 * adaptability));
                
                ResourceType type = GetResourceTypeByJob(citizen.JobClass);
                string targetRegion = citizen.TargetRegionName ?? "";
                ResourceKey key = new ResourceKey(town.Facet.Name, targetRegion, type);

                if (string.IsNullOrEmpty(targetRegion) || !ResourceManager.Pools.ContainsKey(key) || ResourceManager.Pools[key] == null || ResourceManager.Pools[key].CurrentCapacity <= 0)
                {
                    FindWorkPool(citizen, town);
                    targetRegion = citizen.TargetRegionName ?? "";
                    key = new ResourceKey(town.Facet.Name, targetRegion, type);
                }

                if (string.IsNullOrEmpty(targetRegion)) return (false, 0);

                if (ResourceManager.Pools.TryGetValue(key, out ResourcePool pool) && pool != null && pool.CurrentCapacity > 0)
                {
                    if (pool.AvailableResources == null) return (false, 0);

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
                                Item harvested = (Item)Activator.CreateInstance(targetItem);
                                if (harvested != null)
                                {
                                    harvested.Amount = consumedAmount;
                                    if (!PhysicalStorageEngine.TryStoreItem(citizen.House, harvested))
                                        harvested.MoveToWorld(citizen.Location, citizen.Map); 
                                    return (true, 0);
                                }
                            }
                            return ExecuteSell(citizen, town, targetItem, basePrice, consumedAmount);
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"[VTS Error - ExecuteHarvestAndSell] {ex.Message}"); }
            return (false, 0);
        }

        public static (bool Success, int Earnings) ExecuteSell(VirtualCitizen citizen, TownEconomy town, EconomyItemKey itemKey, int basePrice, int amount)
        {
            try
            {
                if (citizen == null || town == null || itemKey.ItemType == null) return (false, 0);

                if (itemKey.ItemType == typeof(Gold))
                {
                    citizen.Gold += amount;
                    return (true, amount);
                }

                var (_, minSell, _) = GetTradeTolerance(citizen, basePrice);
                int guaranteedPrice = GetPVAGuaranteedPrice(itemKey, town);
                double sellRate = Math.Min(1.0, 0.70 + (citizen.Potential * 0.10));
                int townBuyPrice = Math.Max(1, (int)(guaranteedPrice * sellRate)); 

                if (townBuyPrice >= minSell)
                {
                    int totalEarnings = townBuyPrice * amount;
                    citizen.Gold += totalEarnings;
                    town.Wealth -= totalEarnings; 
                    
                    if (town.Warehouse == null) town.Warehouse = new Dictionary<EconomyItemKey, WarehouseItem>();
                    if (!town.Warehouse.ContainsKey(itemKey)) town.Warehouse[itemKey] = new WarehouseItem(itemKey, 0, basePrice, 100);
                    
                    var wItem = town.Warehouse[itemKey];
                    if (wItem != null) wItem.Stock += amount;

                    LogTrade(town, citizen.Name, TraderType.NPC, TradeType.Sell, itemKey, amount, townBuyPrice);

                    return (true, totalEarnings);
                }
                citizen.Stress = Math.Min(100, citizen.Stress + 2); 
            }
            catch (Exception ex) { Console.WriteLine($"[VTS Error - ExecuteSell] {ex.Message}"); }
            return (false, 0);
        }
		public static bool IsRareResource(Type type) => GetResourceTierValue(type) > 1;

        public static int GetResourceTierValue(Type type)
        {
            if (type == null) return 1;
            try
            {
                CraftResource res = CraftResources.GetFromType(type);
                if (res == CraftResource.None) return 1;
                return CraftResources.GetIndex(res) + 1;
            }
            catch { return 1; }
        }

        public static void ExecuteRareBrokerage(VirtualCitizen merchant, TownEconomy town)
        {
            try
            {
                if (merchant == null || town == null || town.Citizens == null) return;

                var noble = town.Citizens.FirstOrDefault(c => c != null && c.RankLevel >= NobilityRank.Baron && c.Gold > 10000 && c.House != null);
                if (noble == null) return;

                foreach (var supplier in town.Citizens)
                {
                    if (supplier == null || supplier == merchant || supplier.House == null || supplier.House.Interior == null || supplier.House.Interior.PlacedFurniture == null) continue;

                    Item rareItemToSell = null;
                    for (int i = 0; i < supplier.House.Interior.PlacedFurniture.Count; i++)
                    {
                        if (supplier.House.Interior.PlacedFurniture[i] is Container c && c != null && c.Items != null)
                        {
                            foreach (var item in c.Items)
                            {
                                if (item != null && IsRareResource(item.GetType()))
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
                        EconomyItemKey key = rareItemToSell.GetType(); 
                        int marketPrice = town.GetPrice(key) * 5; 

                        if (merchant.Gold >= marketPrice && noble.Gold >= (int)(marketPrice * 1.5))
                        {
                            Item extracted = PhysicalStorageEngine.RetrieveItem(supplier.House, rareItemToSell.GetType(), 1);
                            if (extracted != null)
                            {
                                if (supplier.Family != null) supplier.Family.SharedWealth += marketPrice;
                                merchant.Gold -= marketPrice;

                                int sellPrice = (int)(marketPrice * 1.5);
                                merchant.Gold += sellPrice;
                                noble.Gold -= sellPrice;

                                LogTrade(town, merchant.Name, TraderType.NPC, TradeType.Buy, key, 1, marketPrice);
                                LogTrade(town, merchant.Name, TraderType.NPC, TradeType.Sell, key, 1, sellPrice);

                                if (!PhysicalStorageEngine.TryStoreItem(noble.House, extracted)) extracted.MoveToWorld(noble.Location, noble.Map);
                                return; 
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"[VTS Error - ExecuteRareBrokerage] {ex.Message}"); }
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
            try
            {
                if (citizen == null || town == null || town.Facet == null || ResourceManager.Pools == null) return;

                ResourceType type = GetResourceTypeByJob(citizen.JobClass);
                string mapName = town.Facet.Name;
                
                string rawTownName = TownNumber.GetName(town.TownID);
                string townName = string.IsNullOrEmpty(rawTownName) ? "" : rawTownName.ToLower();

                // 🌟 [원인 해결] p.CurrentCapacity > 0 조건을 삭제했습니다!
                // 자원이 고갈되었더라도 무조건 구역(RegionName)을 할당해 주어,
                // AI나 틱 엔진이 빈 값("")을 들고 길찾기를 하다가 뻗어버리는 현상을 원천 차단합니다.
                var validPools = ResourceManager.Pools.Values.Where(p => 
                    p != null && p.MapName == mapName && p.Type == type && !p.IsPrivate
                ).ToList();

                bool hasDungeonZones = false;
                try { hasDungeonZones = DungeonManager.Zones != null; } catch { }

                if (hasDungeonZones)
                    validPools = validPools.Where(p => !DungeonManager.Zones.ContainsKey(p.RCode)).ToList();
                else
                    validPools = validPools.Where(p => p.RegionName != null && !p.RegionName.ToLower().Contains("dungeon")).ToList();

                ResourcePool bestPool = null;

                if (validPools.Count > 0)
                {
                    if (type == ResourceType.Fishing)
                    {
                        if (citizen.Potential >= 2.5) bestPool = validPools.OrderByDescending(p => p.CurrentCapacity).FirstOrDefault();
                        else 
                        {
                            var safeWaters = validPools.Where(p => p.RegionName != null && !p.RegionName.StartsWith("Ocean", StringComparison.OrdinalIgnoreCase)).ToList();
                            if (safeWaters.Count == 0) safeWaters = validPools; 
                            bestPool = safeWaters.OrderBy(p => Utility.GetDistanceToSqrt(town.Center, new Point3D(p.CenterX, p.CenterY, 0))).FirstOrDefault();
                        }
                    }
                    else
                    {
                        if (citizen.Potential >= 2.5) bestPool = validPools.OrderByDescending(p => p.CurrentCapacity).FirstOrDefault();
                        else
                        {
                            var townPools = validPools.Where(p => p.RegionName != null && p.RegionName.ToLower().Contains(townName)).ToList();
                            if (townPools.Count == 0) townPools = validPools;
                            bestPool = townPools.OrderBy(p => Utility.GetDistanceToSqrt(town.Center, new Point3D(p.CenterX, p.CenterY, 0))).FirstOrDefault();
                        }
                    }
                }
                
                citizen.TargetRegionName = bestPool != null ? bestPool.RegionName : ""; 
            }
            catch (Exception ex) { Console.WriteLine($"[VTS Error - FindWorkPool] {ex.Message}"); }
        }

        private static Type GetDefaultItem(ResourceType type) => type switch
        {
            ResourceType.Mining => typeof(IronOre),
            ResourceType.Lumberjacking => typeof(Log),
            ResourceType.Fishing => typeof(Fish),
            ResourceType.Tanning => typeof(Hides),
            ResourceType.Farming => typeof(WheatSheaf),
            _ => typeof(WheatSheaf)
        };

        public static (bool Success, int Profit) ExecuteTradeRoute(VirtualCitizen merchant, TownEconomy currentTown, int baseCapacity)
        {
            try
            {
                if (merchant == null || currentTown == null || TownEconomyManager.Towns == null || TownEconomyManager.Towns.Count < 2 || currentTown.Warehouse == null) return (false, 0);

                int groupID = ((int)merchant.JobClass / 100) * 100;
                bool isLandMerchant = groupID == 300 || groupID == 400 || groupID == 900 || groupID == 1100;
                bool isSeaMerchant = groupID == 800; 

                if (!isLandMerchant && !isSeaMerchant) return (false, 0);

                var exportCandidates = currentTown.Warehouse.Values
                    .Where(w => w != null && w.Stock > w.TargetStock * 1.2 && currentTown.GetPrice(w.ItemKey) < w.BasePrice)
                    .OrderBy(w => currentTown.GetPrice(w.ItemKey))
                    .ToList();

                if (exportCandidates.Count == 0) return (false, 0);

                var currentRCode = RegionSaver.GetRegionCodes(currentTown.Facet, currentTown.Center.X, currentTown.Center.Y, currentTown.Center.Z).Major;
                
                foreach (var exportItem in exportCandidates)
                {
                    EconomyItemKey itemKey = exportItem.ItemKey;
                    int localPrice = currentTown.GetPrice(itemKey);
                    if (localPrice <= 0) continue;

                    var targetTowns = TownEconomyManager.Towns.Values
                        .Where(t => t != null && t.TownID != currentTown.TownID && t.Facet == currentTown.Facet && t.Warehouse != null)
                        .Where(t => t.Warehouse.ContainsKey(itemKey) && t.GetPrice(itemKey) > localPrice * 1.2) 
                        .OrderByDescending(t => t.GetPrice(itemKey))
                        .ToList();

                    foreach (var targetTown in targetTowns)
                    {
                        if (isSeaMerchant && targetTown.TownName != null && !IsCoastalTown(targetTown.TownName)) continue;

                        var targetRCode = RegionSaver.GetRegionCodes(targetTown.Facet, targetTown.Center.X, targetTown.Center.Y, targetTown.Center.Z).Major;
                        var plan = VirtualTravelNetwork.CalculateBestRoute(currentRCode, targetRCode, merchant.Gold, false);
                        if (!plan.IsPossible) continue;

                        int targetPrice = targetTown.GetPrice(itemKey);
                        int capacityPerAnimal = 400;
                        
                        double itemID = merchant.Skills != null && merchant.Skills.TryGetValue(SkillName.ItemID, out var sk) ? sk : merchant.PrimarySkill;
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
                            totalAnimalCost = animalsNeeded * 500;

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

                            currentTown.Warehouse[itemKey].Stock -= amountToTrade;
                            LogTrade(currentTown, merchant.Name, TraderType.NPC, TradeType.Export, itemKey, amountToTrade, localPrice);

                            var targetItem = targetTown.Warehouse[itemKey];
                            if (targetItem != null)
                            {
                                bool wasShortage = targetItem.Stock < (targetItem.TargetStock * 0.5);
                                targetItem.Stock += amountToTrade;
                                LogTrade(targetTown, merchant.Name, TraderType.NPC, TradeType.Import, itemKey, amountToTrade, targetPrice);

                                if (wasShortage && amountToTrade >= 50)
                                {
                                    targetItem.TargetStock += Math.Max(1, amountToTrade / 10);
                                    int currentBase = targetItem.BasePrice;
                                    int priceDelta = targetPrice - currentBase;

                                    if (priceDelta > 0)
                                    {
                                        targetItem.BasePrice += Math.Min(Math.Max(1, (int)(priceDelta * 0.03)), Math.Max(1, (int)(currentBase * 0.10)));
                                    }
                                }
                                else if (targetItem.Stock > targetItem.TargetStock * 2)
                                {
                                    targetItem.BasePrice = Math.Max(1, (int)(targetItem.BasePrice * 0.98));
                                }
                            }

                            if (currentTown.Citizens != null) currentTown.Citizens.Remove(merchant);
                            if (targetTown.Citizens != null) targetTown.Citizens.Add(merchant);
                            merchant.TargetRegionName = targetTown.TownName;
                            merchant.Stress = Math.Max(0, merchant.Stress - 20);
                            merchant.Satisfaction = 100;

                            return (true, expectedProfit);
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"[VTS Error - ExecuteTradeRoute] {ex.Message}"); }
            return (false, 0);
        }

        private static bool IsCoastalTown(string townName)
        {
            if (string.IsNullOrEmpty(townName)) return false;
            string[] coastalTowns = { "Britain", "Skara Brae", "Vesper", "Trinsic", "Moonglow", "Magincia", "Nujel'm", "Jhelom", "Buccaneer's Den", "Serpent's Hold", "Ocllo", "Haven", "Sea Market" };
            return coastalTowns.Any(c => townName.Contains(c, StringComparison.OrdinalIgnoreCase));
        }

        [Usage("경매 <아이템이름>")]
        private static void OnMarketSearch(CommandEventArgs e)	
        {
            try
            {
                string searchWord = e.ArgString.Trim().ToLower();
                if (string.IsNullOrEmpty(searchWord)) { e.Mobile.SendMessage(0x35, "사용법: [경매 <찾을아이템이름>"); return; }

                var list = new List<(string VendorName, string ItemName, int Price, int Stock)>();
                if (RetailVendor.RetailVendors != null)
                {
                    for (int i = 0; i < RetailVendor.RetailVendors.Count; i++)
                    {
                        var vendor = RetailVendor.RetailVendors[i];
                        if (vendor == null || vendor.Deleted || vendor.MarketItems == null) continue;

                        for (int j = 0; j < vendor.MarketItems.Count; j++)
                        {
                            var m = vendor.MarketItems[j];
                            if (m == null || m.RealItem == null || m.RealItem.Deleted) continue;
                            string itemName = (m.RealItem.Name ?? m.RealItem.ItemData.Name).ToLower();
                            
                            if (itemName.Contains(searchWord)) list.Add((vendor.Name, m.RealItem.Name ?? m.RealItem.ItemData.Name, m.PricePerUnit, m.RealItem.Amount));
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
            catch { }
        }
        
        public static void ExecuteMarketIntervention(TownEconomy town)
        {
            try
            {
                if (town == null || town.Warehouse == null || town.Warehouse.Count == 0) return;

                foreach (var wItem in town.Warehouse.Values.ToList())
                {
                    if (wItem == null || wItem.ItemKey.ItemType == null) continue;

                    int oldStock = wItem.LastStock;
                    int currentStock = wItem.Stock;
                    wItem.LastStock = currentStock;

                    if (wItem.TargetStock > 0)
                    {
                        if (currentStock >= wItem.TargetStock * 3 && currentStock >= oldStock)
                        {
                            wItem.BasePrice = Math.Max(1, (int)(wItem.BasePrice * 0.9)); 
                            LogTrade(town, "System (Dumping)", TraderType.System, TradeType.Sell, wItem.ItemKey, 0, wItem.BasePrice);
                        }
                        else if (currentStock <= 0)
                        {
                            wItem.BasePrice = Math.Max(1, (int)(wItem.BasePrice * 1.1)); 
                            LogTrade(town, "System (Premium)", TraderType.System, TradeType.Buy, wItem.ItemKey, 0, wItem.BasePrice);
                            
                            if (town.Houses != null && town.Houses.Count > 0 && Utility.RandomDouble() < 0.2)
                            {
                                if (PartTimeManager.ActiveRequests != null)
                                {
                                    JobCategory cat = PartTimeManager.GetCategoryForType(wItem.ItemKey.ItemType);
                                    PartTimeManager.CreateAIRequest(town.TownName ?? "", $"[품귀 현상] {wItem.ItemKey.ItemType.Name} 긴급 조달", cat, wItem.ItemKey.ItemType, 15, wItem.BasePrice * 15 * 2, town.Houses[0]);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"[VTS Error - ExecuteMarketIntervention] {ex.Message}"); }
        }
    }
}