using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Network;

namespace Server.Misc
{
    public static class NewDurabilityManager
    {
        public const int SubDurabilityThreshold = 10000;

        // 1. [피격자] 무기/방어구/장신구 타격 로직
        public static void OnWeaponHit(Mobile victim, int damage, int hitLocation)
        {
            if (victim == null || damage <= 0) return;

            Item item = GetEquipmentByLocation(victim, hitLocation);
            if (item != null)
                ApplyDamage(victim, item, damage);
        }

        // 2. [공격자] 무기 및 의복 마모 (공격 시 호출)
        public static void OnAttackerWear(Mobile attacker)
        {
            if (attacker == null) return;

            // A. 무기 마모 (100 ~ 500 증가)
            Item weapon = attacker.Weapon as Item;
            if (weapon != null)
                ApplyDamage(attacker, weapon, Utility.RandomMinMax(100, 500));

            // B. 천옷류 마모 (50 ~ 200 증가)
            ApplyAllClothingWear(attacker, Utility.RandomMinMax(50, 200));
        }

        // 3. [피격자] 천옷류 마모 (피격 시 호출 - 데미지의 10%)
        public static void OnVictimWear(Mobile victim, int damage)
        {
            if (victim == null || damage <= 0) return;

            int wearAmount = (int)(damage * 0.1);
            if (wearAmount < 1) wearAmount = 1;

            ApplyAllClothingWear(victim, wearAmount);
        }

        // [핵심] IEquipOption 인터페이스를 통한 실제 내구도 제어
        private static void ApplyDamage(Mobile owner, Item item, int amount)
        {
            // 인터페이스 캐스팅
            IEquipOption ieo = item as IEquipOption;

            if (ieo == null) 
                return;

            // 서브 내구도 누적
            int currentSub = ieo.PrefixOption[1];
            currentSub += amount;

            if (currentSub >= SubDurabilityThreshold)
            {
                int dropCount = currentSub / SubDurabilityThreshold;
                currentSub %= SubDurabilityThreshold;

                for (int i = 0; i < dropCount; i++)
                {
                    // 인터페이스에 정의된 HitPoints 사용
                    if (ieo.HitPoints > 0)
                    {
                        ieo.HitPoints--;
                    }
                    else if (ieo.MaxHitPoints > 0)
                    {
                        ieo.MaxHitPoints--;
                        // 기존 울온 시스템 메시지 출력
                        owner.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121);
                    }
                }
            }
            ieo.PrefixOption[1] = currentSub;
        }

        private static void ApplyAllClothingWear(Mobile m, int amount)
        {
            Layer[] clothingLayers = { 
                Layer.OuterTorso, Layer.MiddleTorso, Layer.Shirt, 
                Layer.Cloak, Layer.Shoes, Layer.Waist, Layer.OuterLegs, Layer.InnerLegs 
            };

            foreach (Layer layer in clothingLayers)
            {
                Item item = m.FindItemOnLayer(layer);
                
                // 갑옷/방패가 아니면서 IEquipOption을 구현한 의류/스펠북 등 대상
                if (item != null && !(item is BaseArmor) && !(item is BaseShield) && item is IEquipOption)
                {
                    ApplyDamage(m, item, amount);
                }
            }
        }

        public static Item GetEquipmentByLocation(Mobile m, int location)
        {
            switch (location)
            {
                case 0: // Parry
                    Item shield = m.FindItemOnLayer(Layer.TwoHanded);
                    if (shield is BaseShield) return shield;
                    return m.FindItemOnLayer(Layer.OneHanded);
                case 1: return m.FindItemOnLayer(Layer.Helm);
                case 2: return m.FindItemOnLayer(Layer.Neck);
                case 3: return m.FindItemOnLayer(Layer.InnerTorso);
                case 4: return m.FindItemOnLayer(Layer.Arms);
                case 5: return m.FindItemOnLayer(Layer.Gloves);
                case 6: return m.FindItemOnLayer(Layer.Pants);
                case 7: return m.FindItemOnLayer(Layer.Ring);
                case 8: return m.FindItemOnLayer(Layer.Bracelet);
                case 9: return m.FindItemOnLayer(Layer.Earrings);
                case 10: return m.FindItemOnLayer(Layer.Talisman);
                default: return null;
            }
        }
    }
}