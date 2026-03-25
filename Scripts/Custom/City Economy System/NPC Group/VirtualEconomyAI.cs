using System;
using System.Collections.Generic; // [★ 추가]
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

            var checkedVendors = new HashSet<PlayerVendor>();

            for (int radius = 10; radius <= 100; radius += 10)
            {
                // [★ 수정] using 대신 명시적 Free 호출
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
                            
                            eable.Free(); // 메모리 해제
                            return true;  // 성공 시 즉시 종료
                        }
                    }
                }
                eable.Free(); // 루프 다 돌아도 못 찾으면 해제
            }
            return false;
        }
    }
}
