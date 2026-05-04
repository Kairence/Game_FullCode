using System;
using System.Collections.Generic;
using Server;
using Server.Items;

namespace Server.Misc
{
    public static class PhysicalStorageEngine
    {
        // ------------------------------------------------------------------------------
        // 1. 가구 물리 배치 (인테리어 맵핑 연동)
        // ------------------------------------------------------------------------------
        public static void PlaceFurniture(VirtualHouse house, Item furniture)
        {
            if (house == null || house.Interior == null || furniture == null)
            {
                furniture?.Delete();
                return;
            }

            // NpcHouseChest 같은 귀중품/수납함은 가능하면 2층(인덱스 1) 이상의 안전한 곳에 우선 배치
            int targetFloor = 0;
            if (furniture is NpcHouseChest) 
                targetFloor = 1;

            var placement = house.Interior.FindBestPlacementSpot(targetFloor);

            if (placement.Success)
            {
                furniture.MoveToWorld(placement.Location, house.EstateSign.Map);
                furniture.Movable = false; // 락다운 고정 (풍화 정지)

                if (furniture is NpcHouseChest chest)
                {
                    chest.OwnerHouse = house;
                }

                house.Interior.PlacedFurniture.Add(furniture);
                Console.WriteLine(string.Format("[Housing] {0} 가문이 {1}을(를) 물리적으로 배치했습니다. (위치: {2})", house.HouseName, furniture.GetType().Name, placement.Location));
            }
            else
            {
                Console.WriteLine(string.Format("[Housing] {0} 가문에 {1}을(를) 배치할 공간이 없어 파기되었습니다.", house.HouseName, furniture.GetType().Name));
                furniture.Delete();
            }
        }

        // ------------------------------------------------------------------------------
        // 2. 물리 상자에 아이템 수납 (순차적 스캔)
        // ------------------------------------------------------------------------------
        public static bool TryStoreItem(VirtualHouse house, Item itemToStore)
        {
            if (house == null || house.Interior == null || itemToStore == null || itemToStore.Deleted) 
                return false;

            for (int i = 0; i < house.Interior.PlacedFurniture.Count; i++)
            {
                Item furniture = house.Interior.PlacedFurniture[i];
                
                if (furniture is Container container)
                {
                    int currentItems = container.TotalItems;
                    double currentWeight = container.TotalWeight;
                    
                    int maxItems = container.DefaultMaxItems;
                    double maxWeight = container.DefaultMaxWeight;

                    // 해당 상자의 개수 및 무게 한도 체크
                    if (currentItems + 1 <= maxItems && currentWeight + itemToStore.Weight <= maxWeight)
                    {
                        container.DropItem(itemToStore);
                        return true;
                    }
                }
            }

            return false; // 모든 상자가 꽉 찼거나 상자가 없음
        }

        // ------------------------------------------------------------------------------
        // 3. 물리 상자에서 아이템 꺼내기 (소비 및 판매용)
        // ------------------------------------------------------------------------------
        public static Item RetrieveItem(VirtualHouse house, Type itemType, int requiredAmount)
        {
            if (house == null || house.Interior == null) 
                return null;

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
                                return splitItem;
                            }
                        }
                    }
                }
            }

            return null;
        }

        // ------------------------------------------------------------------------------
        // 4. 특정 아이템의 총 물리적 재고량 확인
        // ------------------------------------------------------------------------------
        public static int GetTotalItemCount(VirtualHouse house, Type itemType)
        {
            if (house == null || house.Interior == null) 
                return 0;
            
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