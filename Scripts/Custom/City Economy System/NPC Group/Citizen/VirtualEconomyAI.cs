using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
    public static class VirtualEconomyAI
    {
        public static bool TryShopFromPlayerVendor(VirtualAgent agent, TownEconomy town, ItemTag targetCategory, double townPriceM)
        {
            var map = town.Facet;
            if (map == null || map == Map.Internal) return false;

            // [수정] BaseVendor 대신 모든 모바일의 상위 객체인 Mobile로 선언합니다.
            var checkedVendors = new HashSet<Mobile>();

            for (int radius = 10; radius <= 100; radius += 10)
            {
                var eable = map.GetMobilesInRange(town.Center, radius);
                
                foreach (var mob in eable)
                {
                    if (mob is PlayerVendor vendor && checkedVendors.Add(vendor))
                    {
                        if (vendor.Backpack == null) continue;

                        var items = vendor.Backpack.Items.ToArray();
                        foreach (var item in items)
                        {
                            if (agent.ClassifyItem(item) != targetCategory) continue;

                            var vi = vendor.GetVendorItem(item);
                            if (vi == null) continue;

                            int maxPrice = (int)(vi.Price * townPriceM * 1.5);
                            if (vi.Price > maxPrice || vi.Price > agent.Gold) continue;

                            agent.Gold -= vi.Price;
                            vendor.HoldGold += vi.Price;
                            item.Delete();
                            
                            eable.Free(); 
                            return true;  
                        }
                    }
                    else if (mob is RetailVendor rVendor && checkedVendors.Add(rVendor))
                    {
                        foreach (var mItem in rVendor.MarketItems.ToList()) 
                        {
                            if (mItem.RealItem == null || mItem.RealItem.Deleted) continue;
                            if (agent.ClassifyItem(mItem.RealItem) != targetCategory) continue;

                            int maxPrice = (int)(mItem.PricePerUnit * townPriceM * 1.5);
                            if (mItem.PricePerUnit > maxPrice || mItem.PricePerUnit > agent.Gold) continue;

                            Item boughtItem = rVendor.ExtractItemForAI(mItem, 1);
                            
                            if (boughtItem != null)
                            {
                                agent.Gold -= mItem.PricePerUnit;
                                rVendor.HoldGold += mItem.PricePerUnit;
                                boughtItem.Delete(); 

                                eable.Free();
                                return true;
                            }
                        }
                    }
                }
                eable.Free(); 
            }
            return false;
        }
    }
}