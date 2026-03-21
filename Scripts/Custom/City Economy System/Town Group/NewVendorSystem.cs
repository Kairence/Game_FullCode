using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Server;
using Server.Mobiles;
using Server.Items;

namespace Server.Misc
{
    public static class NewVendorSystem
    {
        public static void ClearBuyInfoCache(BaseVendor vendor)
        {
            try
            {
                FieldInfo buyField = typeof(BaseVendor).GetField("m_BuyInfo", BindingFlags.Instance | BindingFlags.NonPublic);
                if (buyField != null) buyField.SetValue(vendor, null);
            }
            catch { }
        }

        public static bool CheckStockBeforeBuy(BaseVendor vendor, Mobile buyer, List<BuyItemResponse> list, Dictionary<Type, WarehouseItem> warehouse)
        {
            // [입구 로그] 이 로그가 안 찍히면 OnBuyItems 진입 자체가 안 된 것임
            Console.WriteLine($"\n[Buy Debug] Start: {buyer.Name} buying from {vendor.Name}");

            if (warehouse == null)
            {
                Console.WriteLine("[Buy Debug] Warehouse is NULL. Proceeding with default logic.");
                return true;
            }

            foreach (BuyItemResponse res in list)
            {
                Item itemOnVendor = vendor.BuyPack.Items.FirstOrDefault(i => i.Serial == res.Serial);

                if (itemOnVendor != null)
                {
                    Type type = itemOnVendor.GetType();
                    if (warehouse.TryGetValue(type, out var invItem))
                    {
                        Console.WriteLine($"[Buy Debug] Item: {type.Name}, Warehouse Stock: {invItem.Stock}, Request: {res.Amount}");
                        if (invItem.Stock < res.Amount)
                        {
                            Console.WriteLine($"[Buy Debug] Result: FAILED (Insufficient Stock)");
                            buyer.SendMessage(33, $"Sorry, we only have {invItem.Stock} in stock.");
                            return false;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"[Buy Debug] Warning: Could not find Serial {res.Serial} in BuyPack.");
                }
            }
            return true;
        }

        public static void DeductStock(BaseVendor vendor, List<BuyItemResponse> list, Dictionary<Type, WarehouseItem> warehouse)
        {
            if (warehouse == null) return;

            foreach (BuyItemResponse res in list)
            {
                Item itemOnVendor = vendor.BuyPack.Items.FirstOrDefault(i => i.Serial == res.Serial);

                if (itemOnVendor != null)
                {
                    Type type = itemOnVendor.GetType();
                    if (warehouse.TryGetValue(type, out var invItem))
                    {
                        invItem.Stock -= res.Amount;
                        if (invItem.Stock < 0) invItem.Stock = 0;
                        Console.WriteLine($"[Buy Debug Success] {type.Name} -{res.Amount} (Remaining: {invItem.Stock})");
                    }
                }
            }
        }
    }
}