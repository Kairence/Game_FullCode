using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Commands;

namespace Server.Misc
{
    public static class RetailMarketEngine
    {
        public static void Initialize()
        {
            // 전역 경매장 검색 명령어 등록
            CommandSystem.Register("경매", AccessLevel.Player, new CommandEventHandler(OnMarketSearch));
        }

        [Usage("경매 <아이템이름>")]
        private static void OnMarketSearch(CommandEventArgs e)
        {
            string searchWord = e.ArgString.Trim().ToLower();
            
            if (string.IsNullOrEmpty(searchWord))
            {
                e.Mobile.SendMessage(0x35, "사용법: [경매 <찾을아이템이름>");
                return;
            }

            // 전역 검색 결과 추출 (튜플 리스트 사용)
            var results = SearchGlobalMarket(searchWord);

            if (results.Count == 0)
            {
                e.Mobile.SendMessage(33, $"'{searchWord}'에 해당하는 매물이 현재 등록되어 있지 않습니다.");
                return;
            }

            // 검색 결과 리스트 출력 (차후 전역 검색용 Gump로 확장 가능)
            e.Mobile.SendMessage(68, $"--- '{searchWord}' 검색 결과 ({results.Count}건) ---");
            foreach (var res in results.Take(10)) // 최대 10개까지만 채팅창 표시
            {
                e.Mobile.SendMessage(0x481, $"[{res.VendorName}] {res.ItemName} - 개당 {res.Price:N0} GP (재고: {res.Stock})");
            }
        }

        // 모든 RetailVendor를 뒤져서 검색어와 일치하는 매물을 찾아냅니다.
        public static List<(string VendorName, string ItemName, int Price, int Stock, RetailVendor VendorInstance)> SearchGlobalMarket(string query)
        {
            var list = new List<(string, string, int, int, RetailVendor)>();

            foreach (var vendor in RetailVendor.RetailVendors)
            {
                if (vendor == null || vendor.Deleted) continue;

                var matches = vendor.MarketItems.Where(m => 
                    m.RealItem != null && 
                    !m.RealItem.Deleted && 
                    (m.RealItem.Name != null && m.RealItem.Name.ToLower().Contains(query) || 
                     m.RealItem.ItemData.Name.ToLower().Contains(query))
                );

                foreach (var m in matches)
                {
                    list.Add((
                        vendor.Name, 
                        m.RealItem.Name ?? m.RealItem.ItemData.Name, 
                        m.PricePerUnit, 
                        m.RealItem.Amount,
                        vendor
                    ));
                }
            }

            // 가격 낮은 순으로 정렬하여 반환
            return list.OrderBy(x => x.Item3).ToList();
        }
    }
}