using System;
using System.Collections.Generic;
using Server;
using Server.Items;

namespace Server.Misc
{
    public static class PhysicalStorageEngine
    {
        // 1. 가구 물리 배치
        public static void PlaceFurniture(VirtualHouse house, Item furniture)
        {
            if (house == null || house.Interior == null || furniture == null)
            {
                furniture?.Delete();
                return;
            }

            int targetFloor = 0;
            if (furniture is NpcHouseChest) 
                targetFloor = 1;

            var placement = house.Interior.FindBestPlacementSpot(targetFloor);

            if (placement.Success)
            {
                furniture.MoveToWorld(placement.Location, house.EstateSign.Map);
                furniture.Movable = false; 

                if (furniture is NpcHouseChest chest)
                {
                    chest.OwnerHouse = house;
                }

                house.Interior.PlacedFurniture.RemoveAll(f => f == null || f.Deleted);
                house.Interior.PlacedFurniture.Add(furniture);
                Console.WriteLine($"[Housing] {house.HouseName} 가문이 {furniture.GetType().Name}을(를) 물리적으로 배치했습니다.");
            }
            else
            {
                furniture.Delete();
            }
        }

        // 2. 물리 상자에 아이템 수납 (🌟 스택(겹침) 병합 로직 추가)
        public static bool TryStoreItem(VirtualHouse house, Item itemToStore)
        {
            if (house == null || house.Interior == null || itemToStore == null || itemToStore.Deleted) 
                return false;

            house.Interior.PlacedFurniture.RemoveAll(f => f == null || f.Deleted);

            // 단계 1: 기존에 같은 아이템이 있으면 우선적으로 겹치기 (Stack)
            if (itemToStore.Stackable)
            {
                for (int i = 0; i < house.Interior.PlacedFurniture.Count; i++)
                {
                    if (house.Interior.PlacedFurniture[i] is Container container)
                    {
                        Item[] existingItems = container.FindItemsByType(itemToStore.GetType());
                        foreach (var existing in existingItems)
                        {
                            // 🌟 리소스나 내용물(음료)이 같을 때만 겹치도록 방어 코드
                            bool canMerge = true;
                            if (existing is BaseBeverage bev1 && itemToStore is BaseBeverage bev2 && bev1.Content != bev2.Content) canMerge = false;
                            
                            var resProp1 = existing.GetType().GetProperty("Resource");
                            var resProp2 = itemToStore.GetType().GetProperty("Resource");
                            if (resProp1 != null && resProp2 != null)
                            {
                                if (!resProp1.GetValue(existing).Equals(resProp2.GetValue(itemToStore))) canMerge = false;
                            }

                            if (canMerge && existing.Amount < 60000) // 울온 최대 스택 한도
                            {
                                int spaceLeftInStack = 60000 - existing.Amount;
                                int amountToMove = Math.Min(itemToStore.Amount, spaceLeftInStack);
                                
                                existing.Amount += amountToMove;
                                itemToStore.Amount -= amountToMove;
                                
                                if (itemToStore.Amount <= 0)
                                {
                                    itemToStore.Delete();
                                    return true; // 수납 완벽 종료
                                }
                            }
                        }
                    }
                }
            }

            // 단계 2: 겹치고 남은 아이템(또는 겹쳐지지 않는 아이템)을 빈 공간에 새로 넣기
            for (int i = 0; i < house.Interior.PlacedFurniture.Count; i++)
            {
                if (house.Interior.PlacedFurniture[i] is Container container)
                {
                    int currentItems = container.TotalItems;
                    double currentWeight = container.TotalWeight;
                    
                    int maxItems = container.DefaultMaxItems;
                    double maxWeight = container.DefaultMaxWeight;

                    if (currentItems + 1 <= maxItems && currentWeight + itemToStore.Weight <= maxWeight)
                    {
                        container.DropItem(itemToStore);
                        return true;
                    }
                }
            }

            return false; 
        }

        // 3. 물리 상자에서 아이템 꺼내기
        public static Item RetrieveItem(VirtualHouse house, Type itemType, int requiredAmount)
        {
            if (house == null || house.Interior == null) 
                return null;

            house.Interior.PlacedFurniture.RemoveAll(f => f == null || f.Deleted);

            for (int i = 0; i < house.Interior.PlacedFurniture.Count; i++)
            {
                Item furniture = house.Interior.PlacedFurniture[i];
                
                if (furniture is Container container)
                {
                    Item[] foundItems = container.FindItemsByType(itemType);

                    for (int j = 0; j < foundItems.Length; j++)
                    {
                        Item found = foundItems[j];
                        
                        if (found.Amount >= requiredAmount)
                        {
                            if (found.Amount == requiredAmount)
                            {
                                container.RemoveItem(found);
                                return found;
                            }
                            else
                            {
                                found.Amount -= requiredAmount;
                                
                                Item splitItem = (Item)Activator.CreateInstance(itemType);
                                splitItem.Amount = requiredAmount;
                                splitItem.Hue = found.Hue;
                                
                                var resProp = found.GetType().GetProperty("Resource");
                                if (resProp != null) resProp.SetValue(splitItem, resProp.GetValue(found));
                                
                                return splitItem;
                            }
                        }
                    }
                }
            }

            return null;
        }

        // 4. 특정 아이템의 총 물리적 재고량 확인
        public static int GetTotalItemCount(VirtualHouse house, Type itemType)
        {
            if (house == null || house.Interior == null) 
                return 0;
            
            house.Interior.PlacedFurniture.RemoveAll(f => f == null || f.Deleted);
            
            int total = 0;

            for (int i = 0; i < house.Interior.PlacedFurniture.Count; i++)
            {
                Item furniture = house.Interior.PlacedFurniture[i];
                
                if (furniture is Container container)
                {
                    Item[] foundItems = container.FindItemsByType(itemType);
                    for (int j = 0; j < foundItems.Length; j++)
                    {
                        total += foundItems[j].Amount;
                    }
                }
            }

            return total;
        }
    }
}