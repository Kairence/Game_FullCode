using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Items;

namespace Server.Misc
{
    // [유저님의 14개 태그 모두 유지]
    public enum ItemTag 
    { 
        None, 
        Food_Basic, Food_Luxury, Jewelry, Entertainment, 
        Weapon_Sword, Armor_Plate, Magic_Scroll, Material, 
        Essential, Tool, Armament, Reagent, Luxury 
    }

    public static class EconomyTagHelper
    {
        // [EconomyCore 전용 호출부] town, tag, random 인자를 모두 처리합니다.
        public static Type GetItemTypeByTag(TownEconomy town, ItemTag tag, bool random)
        {
            if (town == null || town.Warehouse == null) 
                return GetDefaultType(tag);

            // 1. 마을 창고에서 해당 태그에 맞는 모든 아이템 타입을 추출
            List<Type> candidates = town.Warehouse.Keys
                .Where(t => GetItemTag(t) == tag)
                .ToList();

            // 2. 창고에 해당 태그 물품이 있다면 상황에 맞춰 반환
            if (candidates.Count > 0)
            {
                if (random) 
                    return candidates[Utility.Random(candidates.Count)]; // 무작위 선택
                
                return candidates[0]; // 고정 선택 (가장 기본형)
            }

            // 3. 창고가 비어있다면 하드코딩된 기본값이라도 반환 (에러 방지)
            return GetDefaultType(tag);
        }

        // 창고 재고가 없을 때를 대비한 백업 데이터
        private static Type GetDefaultType(ItemTag tag) => tag switch
        {
            ItemTag.Food_Basic    => typeof(BreadLoaf),
            ItemTag.Food_Luxury   => typeof(CookedBird),
            ItemTag.Jewelry       => typeof(GoldRing),
            ItemTag.Entertainment => typeof(Lute),
            ItemTag.Weapon_Sword  => typeof(Longsword),
            ItemTag.Armor_Plate   => typeof(PlateChest),
            ItemTag.Magic_Scroll  => typeof(RecallScroll),
            ItemTag.Material      => typeof(IronIngot),
            ItemTag.Essential     => typeof(Bandage),
            ItemTag.Tool          => typeof(Hammer),
            ItemTag.Armament      => typeof(VikingSword),
            ItemTag.Reagent       => typeof(BlackPearl),
            ItemTag.Luxury        => typeof(StarSapphire),
            _                     => typeof(Gold)
        };

        // [핵심 분류기] 창고의 Type들을 유저님의 14개 태그 중 하나로 매칭합니다.
        public static ItemTag GetItemTag(Type type)
        {
            if (type == null) return ItemTag.None;

            // 식량 분류
            if (typeof(Food).IsAssignableFrom(type))
            {
                if (type == typeof(CookedBird) || type == typeof(RawRibs)) return ItemTag.Food_Luxury;
                return ItemTag.Food_Basic;
            }

            // 귀금속 및 사치품
            if (typeof(BaseJewel).IsAssignableFrom(type)) return ItemTag.Jewelry;
            if (type == typeof(GoldRing) || type == typeof(Necklace)) return ItemTag.Jewelry;
            
            // 엔터테인먼트 (악기)
            if (type.IsSubclassOf(typeof(BaseInstrument))) return ItemTag.Entertainment;

            // 무기/방어구/시약/도구 (상속 관계로 큰 분류 처리)
            if (typeof(BaseWeapon).IsAssignableFrom(type)) return ItemTag.Weapon_Sword;
            if (typeof(BaseArmor).IsAssignableFrom(type)) return ItemTag.Armor_Plate;
            if (typeof(BaseReagent).IsAssignableFrom(type)) return ItemTag.Reagent;
            if (typeof(BaseTool).IsAssignableFrom(type)) return ItemTag.Tool;
            
            // 원자재
            if (typeof(BaseIngot).IsAssignableFrom(type) || typeof(BaseLog).IsAssignableFrom(type))
                return ItemTag.Material;

            // 나머지 범용 분류
            return ItemTag.Essential;
        }
    }
}