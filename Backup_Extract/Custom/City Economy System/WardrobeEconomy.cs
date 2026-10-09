using System;
using System.Collections.Generic;
using Server;
using Server.Items;

namespace Server.Misc
{
    public static class WardrobeEconomy
    {
        // 🌟 옷장(Armoire) 종류별 스탯: (최대 의류 보관량, 명예 점수)
        public static readonly Dictionary<Type, (int MaxClothes, int FameScore)> ArmoireStats = new()
        {
            { typeof(RedArmoire), (300, 1) },
            { typeof(MapleArmoire), (300, 1) },
            { typeof(SimpleElvenArmoire), (400, 2) },
            { typeof(ElegantArmoire), (500, 2) },
            { typeof(CherryArmoire), (500, 2) },
            { typeof(FancyElvenArmoire), (800, 4) }
        };

        // 🌟 시민(NPC)용: 가문 창고에 있는 옷장들을 합산하여 최대 의류 보관 한도 계산
        public static int GetMaxClothesStorage(Dictionary<EconomyItemKey, int> warehouse)
        {
            int maxStorage = 50; // 옷장이 없을 때의 기본 보관량 (바닥이나 궤짝 등에 구겨 넣음)

            if (warehouse == null) return maxStorage;

            foreach (var kvp in warehouse)
            {
                if (ArmoireStats.TryGetValue(kvp.Key.ItemType, out var stats))
                {
                    // 옷장 개수 * 옷장별 수납량
                    maxStorage += (stats.MaxClothes * kvp.Value);
                }
            }

            return maxStorage;
        }

        // 현재 가문이 옷을 더 보관할 수 있는지 검사
        public static bool CanStoreMoreClothes(VirtualHouse house, int amountToAdd = 1)
        {
            if (house?.HouseWarehouse == null) return false;

            int maxCapacity = GetMaxClothesStorage(house.HouseWarehouse);
            int currentClothesCount = 0;

            foreach (var kvp in house.HouseWarehouse)
            {
                if (kvp.Key.ItemType.IsSubclassOf(typeof(BaseClothing)))
                {
                    currentClothesCount += kvp.Value;
                }
            }

            return (currentClothesCount + amountToAdd) <= maxCapacity;
        }

        /* 
        // 🌟 유저(플레이어) 옷장 보관 시 내구도 풍화율 감소 로직
        // (요청에 따라 현재는 구현을 보류하고 구조만 잡아둠)
        public static double GetDecayMultiplier(Item item)
        {
            // TODO: 추후 구현 (옷장 안에 있으면 0.5 반환 등)
            return 1.0; 
        }
        */
    }
}