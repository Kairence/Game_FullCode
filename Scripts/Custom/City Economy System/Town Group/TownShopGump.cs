using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Network;
using System.Linq;
using Server.Mobiles;

namespace Server.Misc
{
    public class TownShopGump : Gump
    {
        private TownVendor m_Vendor;
        private TownEconomy m_Town;

        public TownShopGump(Mobile from, TownVendor vendor, TownEconomy town) : base(100, 100)
        {
            m_Vendor = vendor;
            m_Town = town;

            AddPage(0);
            AddBackground(0, 0, 450, 400, 9270);
            AddAlphaRegion(10, 10, 430, 380);
            AddLabel(20, 20, 1152, $"{m_Town.TownName} 마을 통합 상점 (물가: x{m_Town.PriceMultiplier:F1})");

            int y = 60;
            // TownInventoryData에서 정의한 품목만 출력
            foreach (var kvp in m_Town.Warehouse)
            {
                Type type = kvp.Key;
                WarehouseItem info = kvp.Value;
                int price = m_Town.GetPrice(type, m_Town.PriceMultiplier);

                AddLabel(30, y, 1150, $"{type.Name}");
                AddLabel(180, y, 68, $"재고: {info.Stock}");
                AddLabel(280, y, 54, $"{price} gp");

                // [기획 반영] 재고가 500개 이상일 때만 구매 버튼 활성화
                if (info.Stock >= 500)
                    AddButton(380, y, 4005, 4007, y, GumpButtonType.Reply, 0);
                else
                    AddLabel(380, y, 33, "품절");

                y += 25;
            }
        }

        // TownShopGump.cs 의 OnResponse 부분에 들어갈 핵심 로직입니다.
		public override void OnResponse(NetState sender, RelayInfo info)
		{
			Mobile from = sender.Mobile;
			if (info.ButtonID == 0 || from == null) return;

			// 1. 선택한 아이템 정보 추출 (ButtonID를 인덱스로 활용)
			int index = info.ButtonID - 100; // 버튼 ID를 100부터 부여했다고 가정
			var inventoryList = m_Town.Warehouse.Values.ToList();
			
			if (index < 0 || index >= inventoryList.Count) return;
			
			WarehouseItem target = inventoryList[index];
			int price = m_Town.GetPrice(target.ItemType, m_Town.PriceMultiplier);

			// 2. 판매 제한 체크 (기획: 재고 500개 미만 판매 불가)
			if (target.Stock <= 500)
			{
				from.SendMessage(33, "마을 보급품 재고가 부족하여 현재 판매가 중지되었습니다.");
				return;
			}

			// 3. 결제 처리 (가방 및 은행 확인)
			if (!from.Backpack.ConsumeTotal(typeof(Gold), price) && !Banker.Withdraw(from, price))
			{
				from.SendMessage(33, "구매에 필요한 골드가 부족합니다.");
				return;
			}

			// 4. 경제 데이터 반영 (재고 차감 및 마을 부 축적)
			target.Stock--;
			m_Town.Wealth += price;

			// 5. 실제 아이템 생성 및 지급
			try
			{
				Item boughtItem = Activator.CreateInstance(target.ItemType) as Item;
				if (boughtItem != null)
				{
					from.AddToBackpack(boughtItem);
					from.SendMessage(88, $"{boughtItem.Name ?? target.ItemType.Name}을(를) {price}gp에 구매하였습니다.");
					from.PlaySound(0x2E6); // 동전 소리
				}
			}
			catch { from.SendMessage("아이템 생성 중 오류가 발생했습니다."); }

			// 6. UI 갱신 (재고 수치 업데이트를 위해 다시 띄움)
			from.SendGump(new TownShopGump(from, m_Vendor, m_Town));
		}
    }
}