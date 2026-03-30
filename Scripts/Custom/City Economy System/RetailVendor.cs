using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Items;
using Server.Gumps;
using Server.Network;
using Server.Prompts;
using Server.Targeting;
using Server.Multis;
using Server.ContextMenus;

namespace Server.Mobiles
{
	public class RetailVendorDeed : Item
    {
        [Constructable]
        public RetailVendorDeed() : base(0x14F0) 
        {
            Name = "a retail vendor contract";
            Weight = 1.0;
            LootType = LootType.Blessed;
        }

        public RetailVendorDeed(Serial serial) : base(serial) { }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // 가방에 있어야 합니다.
                return;
            }

            BaseHouse house = BaseHouse.FindHouseAt(from);
            if (house == null)
            {
                from.SendMessage("밴더는 자신의 집 내부에서만 설치할 수 있습니다.");
                return;
            }
            if (!house.IsOwner(from))
            {
                from.SendLocalizedMessage(501565); // 집 주인만 가능합니다.
                return;
            }

            RetailVendor v = new RetailVendor();
            v.Owner = from;
            v.MoveToWorld(from.Location, from.Map);

            from.SendMessage(68, "리테일 밴더가 성공적으로 설치되었습니다.");
            this.Delete(); 
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); 
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
    // 1. 등록된 아이템의 정보를 담는 래퍼(Wrapper) 데이터 클래스
    public class MarketItem
    {
        public Item RealItem { get; set; }
        public int PricePerUnit { get; set; }
        public DateTime RegisteredTime { get; set; }
        public Mobile Seller { get; set; }

        public MarketItem(Item item, int price, Mobile seller)
        {
            RealItem = item;
            PricePerUnit = price;
            RegisteredTime = DateTime.Now;
            Seller = seller;
        }

        public MarketItem() { } 
    }

    // 2. 낱개 판매 밴더 코어 (BaseVendor 상속으로 TownEconomy와 자동 연동)
	public class RetailVendor : BaseVendor 
    {
        public static List<RetailVendor> RetailVendors = [];

        private List<MarketItem> m_MarketItems;
        public List<MarketItem> MarketItems => m_MarketItems;

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Owner { get; set; }
		
		// [복구] 부모 클래스에 없으므로 직접 변수를 선언합니다.
        [CommandProperty(AccessLevel.GameMaster)]
        public int HoldGold { get; set; }

        private readonly List<SBInfo> m_SBInfos = [];
        protected override List<SBInfo> SBInfos => m_SBInfos;
        public override void InitSBInfo() { }

        public RetailVendor() : base("잡화상")
        {
            // 1. 성별을 랜덤으로 결정 (50% 확률)
            this.Female = Utility.RandomBool();

            // 2. 성별에 맞춰 엔진 내장 이름 목록에서 랜덤 선택
            // 보통 "human male", "human female" 또는 간단히 "male", "female"을 사용합니다.
            this.Name = NameList.RandomName(this.Female ? "female" : "male");

            // 3. 기존 설정 유지
            this.m_MarketItems = [];
            RetailVendors.Add(this); 
            this.CantWalk = true; // 제자리 고정
        }

		// BaseVendor의 기본 메뉴(Buy/Sell)를 정밀하게 제거하기 위해 오버라이드합니다.
		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            // base.GetContextMenuEntries(from, list); // 과감하게 제거합니다.

            if (from == null || !from.Alive) 
                return;

            // 1. 필수 기본 기능: 캐릭터창(종이인형) 열기
            // Server.ContextMenus.PaperdollEntry는 엔진 표준 클래스입니다.
            list.Add(new PaperdollEntry(this));

            // 2. 관리자용 메뉴: 상점 관리 (주인 혹은 GM)
            if (from == this.Owner || from.AccessLevel >= AccessLevel.GameMaster)
            {
                list.Add(new RetailManagementEntry(from, this));
            }

            // 3. 손님용 메뉴: 상점 구경 (커스텀 구입)
            list.Add(new RetailBrowseEntry(from, this));

            // 4. (선택사항) 목적지 묻기가 필요하면 여기서 추가, 필요 없으면 생략 가능
            // list.Add(new AskDestinationEntry(from, this)); 
        }

		// --- 내부 클래스 수정 ---
		private class RetailManagementEntry : ContextMenuEntry
		{
			private Mobile m_From;
			private RetailVendor m_Vendor;

			// 4. [CS1729 해결] 생성자 인수를 1개(Cliloc ID)로 수정
			public RetailManagementEntry(Mobile from, RetailVendor vendor) : base(6103) 
			{
				m_From = from;
				m_Vendor = vendor;
			}

			public override void OnClick()
			{
				m_From.SendGump(new RetailVendorManagementGump(m_From, m_Vendor));
			}
		}

		private class RetailBrowseEntry : ContextMenuEntry
		{
			private Mobile m_From;
			private RetailVendor m_Vendor;

			// 5. [CS1729 해결] 생성자 인수를 1개(Cliloc ID)로 수정
			public RetailBrowseEntry(Mobile from, RetailVendor vendor) : base(6100) 
			{
				m_From = from;
				m_Vendor = vendor;
			}

			public override void OnClick()
			{
				m_From.SendMessage(0x44, $"{m_Vendor.Name}의 매대를 살펴봅니다.");
				// 추후 구현될 구매 창 호출부
			}
		}

        public override void OnAfterDelete()
        {
            RetailVendors.Remove(this); 
            base.OnAfterDelete();
        }

        public RetailVendor(Serial serial) : base(serial) { }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster || from == Owner)
                from.SendGump(new RetailVendorManagementGump(from, this));
            else
                from.SendGump(new RetailVendorShoppingGump(from, this));
        }

        // 아이템 등록 및 검증 통합 로직
		public (bool Success, string Message) TryListMarketItem(Mobile seller, Item item, int price)
        {
            if (item == null || item.Deleted) return (false, "아이템이 존재하지 않습니다.");
            if (item is Container) return (false, "가방은 낱개 판매용 매대에 올릴 수 없습니다.");
            if (item.Layer != Layer.Invalid && item.Layer != Layer.Backpack)
                return (false, "착용 중이거나 특수한 레이어의 아이템은 등록할 수 없습니다.");
            if (m_MarketItems.Count >= 10) return (false, "매대가 가득 찼습니다.");

            // [핵심 패치 1] 가방 무게 사전 시뮬레이션 (엔진의 강제 드랍 방지)
            Container pack = this.Backpack;
            if (pack != null)
            {
                // UO 백팩 기본 제한은 400스톤입니다.
                int maxWeight = pack.MaxWeight > 0 ? pack.MaxWeight : 400; 
                int currentWeight = pack.TotalWeight;
                int itemTotalWeight = item.TotalWeight;

                if (currentWeight + itemTotalWeight > maxWeight)
                {
                    int availableWeight = maxWeight - currentWeight;
                    if (availableWeight <= 0)
                        return (false, "가방 무게가 꽉 차서 더 이상 등록할 수 없습니다.");

                    // 아이템 1개당 무게 계산 (0인 경우 대비)
                    double unitWeight = item.Weight > 0 ? item.Weight : 0.1;
                    int maxAmount = (int)(availableWeight / unitWeight);
                    
                    // 등록 거부 및 안내 메시지 출력
                    return (false, $"무게 초과! 현재 여유 무게로는 최대 {maxAmount}개까지만 등록할 수 있습니다.");
                }
            }

            if (price <= 0) return (true, ""); // 검증 단계 종료

            // [핵심 패치 2] 이름 덮어쓰기 로직 전면 삭제
            // item.Name을 절대 건드리지 않습니다. 순정 상태를 유지해야 Cliloc이 작동합니다.

            this.AddToBackpack(item);
            m_MarketItems.Add(new MarketItem(item, price, seller));
            
            return (true, "등록 완료.");
        }

        // 일반 구매 로직 (버그 수정 완료)
        public (bool Success, string Message, Item BoughtItem) TryBuyMarketItem(Mobile buyer, MarketItem marketItem, int amount)
        {
            if (marketItem?.RealItem == null || marketItem.RealItem.Deleted)
                return (false, "존재하지 않는 상품입니다.", null);

            if (marketItem.RealItem.Amount < amount)
                return (false, "재고가 부족합니다.", null);

            int totalCost = marketItem.PricePerUnit * amount;

            if (!buyer.Backpack.ConsumeTotal(typeof(Gold), totalCost))
                return (false, "골드가 부족합니다.", null);

            // 부모(BaseVendor)의 금고에 정확히 누적
            this.HoldGold += totalCost;

            Item purchasedItem;
            if (marketItem.RealItem.Amount == amount)
            {
                purchasedItem = marketItem.RealItem;
                m_MarketItems.Remove(marketItem);
                // 가방에서 꺼낼 필요 없이 바로 유저에게 AddToBackpack 하면 이동됩니다.
            }
            else
            {
                purchasedItem = Mobile.LiftItemDupe(marketItem.RealItem, amount);
                // [핵심 패치] 유저가 부분 구매 시 원본 덩어리에서 산 만큼 개수 차감!
                marketItem.RealItem.Amount -= amount;
                marketItem.RealItem.InvalidateProperties();
            }

            buyer.AddToBackpack(purchasedItem);
            return (true, "구매가 완료되었습니다.", purchasedItem);
        }

		// RetailVendor 클래스 내부에 추가
		private Item SafeDupe(Item oldItem, int amount)
		{
			try
			{
				// 원본과 동일한 타입의 새 객체 생성
				Item newItem = (Item)Activator.CreateInstance(oldItem.GetType());

				// 물리적 속성 복사
				newItem.Hue = oldItem.Hue;
				newItem.ItemID = oldItem.ItemID;
				newItem.Name = oldItem.Name;
				newItem.LootType = oldItem.LootType;
				newItem.Weight = oldItem.Weight;
				
				// 가장 중요한 수량 설정: 요청받은 딱 'amount'만큼만 설정
				newItem.Amount = amount;

				return newItem;
			}
			catch
			{
				return null;
			}
		}
        // AI 전용 추출 로직 (증발 및 더블 결제 방지)
        public Item ExtractItemForAI(MarketItem marketItem, int amount)
		{
			// 수량이 부족하거나 이미 삭제된 아이템이면 거부
			if (marketItem?.RealItem == null || marketItem.RealItem.Deleted || marketItem.RealItem.Amount < amount) 
				return null;

			// 1. 전량 구매 시: 원본을 통째로 넘기고 리스트에서 삭제
			if (marketItem.RealItem.Amount == amount)
			{
				Item extracted = marketItem.RealItem;
				m_MarketItems.Remove(marketItem);
				return extracted;
			}
			else
			{
				// 2. 부분 구매 시: 문제의 LiftItemDupe 대신 안전하게 직접 Dupe(복제) 호출
				Item extracted = SafeDupe(marketItem.RealItem, amount);
				
				if (extracted != null)
				{
					// 사본이 성공적으로 만들어졌을 때만 원본 수량 차감
					marketItem.RealItem.Amount -= amount;
					marketItem.RealItem.InvalidateProperties();
					
					// 만약 엔진 계산 오차로 수량이 0 이하가 되면 리스트에서 강제 삭제 (유령화 방지)
					if (marketItem.RealItem.Amount <= 0)
					{
						m_MarketItems.Remove(marketItem);
					}
				}
				else
				{
					// 만약 엔진 문제로 복제에 실패하면 null을 반환하여 거래를 무효화 (원본 보호)
					return null; 
				}

				return extracted;
			}
		}

		
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1); // Version 1

            writer.Write(HoldGold); // [중요] 수익금 저장

            writer.Write(Owner);
            writer.Write(m_MarketItems.Count);
            foreach (var m in m_MarketItems)
            {
                writer.Write(m.RealItem);
                writer.Write(m.PricePerUnit);
                writer.Write(m.RegisteredTime);
                writer.Write(m.Seller);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

			if (version >= 1)
                HoldGold = reader.ReadInt(); // [중요] 수익금 복구

            Owner = reader.ReadMobile();
            m_MarketItems = [];
            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                Item item = reader.ReadItem();
                int price = reader.ReadInt();
                DateTime time = reader.ReadDateTime();
                Mobile seller = reader.ReadMobile();

                if (item != null && !item.Deleted)
                    m_MarketItems.Add(new MarketItem(item, price, seller) { RegisteredTime = time });
            }
            RetailVendors.Add(this);
        }
    }
    // 3. 판매자(Owner)용 관리 Gump
	public class RetailVendorManagementGump : Gump
    {
        private RetailVendor m_Vendor;

        public RetailVendorManagementGump(Mobile from, RetailVendor vendor) : base(50, 50)
        {
            m_Vendor = vendor;

            AddPage(0);
            
            // 1. 메인 배경: 가장 기본적이고 안정적인 석재 배경 (400x500)
            AddBackground(0, 0, 400, 500, 9270);
            
            // 2. 상단 타이틀 및 구분선
            AddLabel(145, 15, 0x480, "상점관리 화면");
            AddImageTiled(20, 40, 360, 2, 2624); // 가장 얇고 깨끗한 실선

            // 3. 경제 정보 섹션
            AddLabel(35, 55, 0x34, "현재 판매 수익금"); 
            AddLabel(35, 75, 0x44, $"{m_Vendor.HoldGold:N0} GP"); // 수익금 녹색 강조
            
            // 금화 걷기 버튼 (표준 파란색 버튼 4005번 사용)
            AddButton(260, 65, 4005, 4007, 1, GumpButtonType.Reply, 0); 
            AddLabel(295, 67, 1152, "금화 걷기");

            AddImageTiled(20, 105, 360, 2, 2624); 

            // 4. 관리 도구 섹션
            // 새로운 물품 등록
            AddButton(35, 120, 4011, 4013, 2, GumpButtonType.Reply, 0); 
            AddLabel(75, 122, 1152, "새로운 물품 등록 (타겟)");

            // 벤더 위치 이동
            AddButton(35, 155, 4005, 4007, 3, GumpButtonType.Reply, 0);
            AddLabel(75, 157, 1152, "벤더 위치 이동");

            AddImageTiled(20, 190, 360, 2, 2624); 

            // 5. 물품 목록 헤더 (골드 색상 0x480)
            AddLabel(35, 200, 0x480, "품명");
            AddLabel(185, 200, 0x480, "수량");
            AddLabel(250, 200, 0x480, "가격");
            AddLabel(330, 200, 0x480, "회수");

            // 6. 동적 물품 리스트 (안전한 Y좌표 계산)
            int y = 230;
            for (int i = 0; i < m_Vendor.MarketItems.Count; i++)
            {
                var mi = m_Vendor.MarketItems[i];
                if (mi.RealItem == null || mi.RealItem.Deleted) continue;

                Item item = mi.RealItem;

                // [수정] 순정 아이템(Cliloc)과 커스텀 이름 아이템 분기 처리
                if (item.Name != null)
                {
                    // 이름이 변경된 아이템 (예: [Exceptional] 송어스테이크)
                    string name = item.Name;
                    if (name.Length > 16) name = name.Substring(0, 14) + "..";
                    AddLabel(35, y, 1152, name); // 기존 1152 색상 유지
                }
                else
                {
                    // 이름이 없는 순정 아이템 (클라이언트 Cliloc 렌더링)
                    // 폰트 크기/색상 HTML 조작 없이 순수하게 호출. 
                    // Width 140 제한으로 긴 이름이 옆 칸을 침범하지 않게 방어.
                    AddHtmlLocalized(35, y, 140, 20, item.LabelNumber, 0x7FFF, false, false);
                }

                AddLabel(190, y, 1152, item.Amount.ToString());
                AddLabel(255, y, 1152, mi.PricePerUnit.ToString());

                // 회수 버튼 (빨간색 X 버튼 4017번 사용)
                AddButton(330, y, 4017, 4019, 100 + i, GumpButtonType.Reply, 0);
                
                y += 25;
            }
        }
    

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (info.ButtonID == 1) 
            {
                if (m_Vendor.HoldGold > 0)
                {
                    from.AddToBackpack(new Gold(m_Vendor.HoldGold));
                    from.SendMessage(68, $"{m_Vendor.HoldGold:N0} 골드를 정산했습니다.");
                    m_Vendor.HoldGold = 0;
                }
                from.SendGump(new RetailVendorManagementGump(from, m_Vendor));
            }
            else if (info.ButtonID == 2) 
            {
                from.SendMessage(53, "매대에 올릴 아이템을 선택하세요.");
                from.Target = new InternalListTarget(m_Vendor);
            }
			else if (info.ButtonID == 3) // 위치 이동 버튼
			{
				from.SendMessage(0x35, "밴더를 옮길 새로운 위치를 선택하세요.");
				from.Target = new InternalMoveTarget(m_Vendor);
			}
            else if (info.ButtonID >= 700) 
            {
                int index = info.ButtonID - 700;
                if (index < m_Vendor.MarketItems.Count)
                {
                    var mi = m_Vendor.MarketItems[index];
                    from.AddToBackpack(mi.RealItem); 
                    m_Vendor.MarketItems.RemoveAt(index);
                    from.SendMessage(53, "상품 판매를 중지하고 회수했습니다.");
                }
                from.SendGump(new RetailVendorManagementGump(from, m_Vendor));
            }
			else if (info.ButtonID >= 100) // [수정] 물품 회수 로직
            {
                int index = info.ButtonID - 100;
                if (index >= 0 && index < m_Vendor.MarketItems.Count)
                {
                    var mi = m_Vendor.MarketItems[index];
                    
                    if (mi.RealItem != null && !mi.RealItem.Deleted)
                    {
                        // 밴더 가방에서 유저 가방으로 물리적 이동
                        from.AddToBackpack(mi.RealItem); 
                        from.SendMessage(68, $"{mi.RealItem.Name ?? mi.RealItem.ItemData.Name}을(를) 매대에서 회수했습니다.");
                        
                        // 리스트에서 제거
                        m_Vendor.MarketItems.RemoveAt(index);
                    }
                    else
                    {
                        m_Vendor.MarketItems.RemoveAt(index); // 아이템이 없으면 리스트에서만 삭제
                    }
                }
                // 창 새로고침
                from.SendGump(new RetailVendorManagementGump(from, m_Vendor));
            }
        }

		private class InternalListTarget : Target
		{
			private RetailVendor m_Vendor;
			public InternalListTarget(RetailVendor vendor) : base(1, false, TargetFlags.None) { m_Vendor = vendor; }

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (targeted is Item item && item.IsChildOf(from.Backpack))
				{
					Container pack = m_Vendor.Backpack;
					if (pack == null) 
					{
						pack = new Backpack();
						m_Vendor.AddItem(pack);
					}

					// [수정] 6개 인수를 받는 CheckHold 시그니처 대응
					// 인수: (유저, 아이템, 메시지여부, 차감여부, 수량, 추가무게)
					// 마지막 인수에 0을 넣어 '아이템 본래 무게'만 체크하도록 합니다.
					if (!pack.CheckHold(from, item, false, true, item.Amount, 0))
					{
						from.SendMessage(33, "밴더의 가방이 너무 무겁거나 아이템이 너무 많아 넣을 수 없습니다.");
						from.SendGump(new RetailVendorManagementGump(from, m_Vendor));
						return;
					}

					// 2. 추가적인 정밀 무게 계산 (소수점 무게 보정)
					int maxWeight = pack.MaxWeight;
					int currentWeight = pack.TotalWeight;
					// C# 12의 간결한 수학 연산 사용
					int itemTotalWeight = (int)Math.Ceiling(item.Weight * item.Amount);

					if (currentWeight + itemTotalWeight > maxWeight)
					{
						int availableWeight = maxWeight - currentWeight;
						double unitWeight = item.Weight > 0 ? item.Weight : 0.1;
						int maxAmount = (int)(availableWeight / unitWeight);
						
						from.SendMessage(33, $"가방 용량 초과! 현재 여유 무게로는 최대 {maxAmount}개까지만 등록 가능합니다.");
						from.SendGump(new RetailVendorManagementGump(from, m_Vendor));
						return;
					}

					// 3. 기본 판매 규칙 체크 (0원 검증)
					var (success, message) = m_Vendor.TryListMarketItem(from, item, 0);

					if (success)
					{
						from.Prompt = new InternalPricePrompt(m_Vendor, item);
						from.SendMessage(53, "이 아이템의 [개당 판매 가격]을 입력하세요.");
					}
					else
					{
						from.SendMessage(33, message); 
						from.SendGump(new RetailVendorManagementGump(from, m_Vendor));
					}
				}
				else 
				{
					from.SendMessage(33, "자신의 가방에 있는 아이템만 등록할 수 있습니다.");
					from.SendGump(new RetailVendorManagementGump(from, m_Vendor));
				}
			}
		}
		private class InternalMoveTarget : Target
		{
			private RetailVendor m_Vendor;
			public InternalMoveTarget(RetailVendor vendor) : base(10, true, TargetFlags.None) { m_Vendor = vendor; }

			protected override void OnTarget(Mobile from, object targeted)
			{
				IPoint3D p = targeted as IPoint3D;
				if (p == null) return;

				// 집 내부인지, 주인인지 다시 확인 (보안)
				Server.Multis.BaseHouse house = Server.Multis.BaseHouse.FindHouseAt(from);
				if (house == null || !house.IsOwner(from))
				{
					from.SendMessage(33, "자신의 집 내부로만 이동시킬 수 있습니다.");
					return;
				}

				// 선택한 지점으로 즉시 텔레포트
				m_Vendor.MoveToWorld(new Point3D(p), from.Map);
				from.SendMessage(68, "밴더의 위치를 옮겼습니다.");
				
				// 이동 후 관리창 다시 열어주기
				from.SendGump(new RetailVendorManagementGump(from, m_Vendor));
			}
		}
        private class InternalPricePrompt : Prompt
        {
            private RetailVendor m_Vendor;
            private Item m_Item;

            public InternalPricePrompt(RetailVendor v, Item item) { m_Vendor = v; m_Item = item; }

            public override void OnResponse(Mobile from, string text)
            {
                int price = Utility.ToInt32(text);
                if (price > 0)
                {
                    var result = m_Vendor.TryListMarketItem(from, m_Item, price);
                    from.SendMessage(result.Success ? 68 : 33, result.Message);
                }
                else from.SendMessage(33, "유효한 숫자를 입력하세요.");
                
                from.SendGump(new RetailVendorManagementGump(from, m_Vendor));
            }
        }
    }

    // ==============================================================================
    // 4. 구매자(Customer)용 쇼핑 Gump
    // ==============================================================================
    public class RetailVendorShoppingGump : Gump
    {
        private RetailVendor m_Vendor;

        public RetailVendorShoppingGump(Mobile from, RetailVendor vendor) : base(50, 50)
        {
            m_Vendor = vendor;
            from.CloseGump(typeof(RetailVendorShoppingGump));

            AddPage(0);
            AddBackground(0, 0, 550, 450, 9270);
            AddAlphaRegion(10, 10, 530, 430);
            
            AddHtml(10, 20, 530, 25, $"<CENTER><BASEFONT SIZE='6' COLOR='#FFFFFF'>{vendor.Name} Shop</BASEFONT></CENTER>", false, false);

            AddImageTiled(30, 60, 490, 2, 9354);
            AddLabel(40, 70, 1152, "Item Name");
            AddLabel(250, 70, 1152, "Price (Each)");
            AddLabel(350, 70, 1152, "In Stock");
            AddLabel(450, 70, 1152, "Buy");

            int y = 100;
            for (int i = 0; i < vendor.MarketItems.Count; i++)
            {
                var mi = vendor.MarketItems[i];
                if (mi.RealItem == null || mi.RealItem.Deleted) continue;

                string itemName = mi.RealItem.Name ?? mi.RealItem.ItemData.Name;
                
                // 기존 0xFFFFFF 였던 부분을 전부 1152(흰색) 및 53(노란색)으로 변경
                AddLabel(40, y, 1152, itemName.Length > 25 ? itemName.Substring(0, 22) + "..." : itemName);
                AddLabel(250, y, 53, $"{mi.PricePerUnit:N0} GP");
                AddLabel(350, y, 1152, mi.RealItem.Amount.ToString());

                AddButton(455, y, 4005, 4007, 1000 + i, GumpButtonType.Reply, 0);

                y += 30;
                if (y > 380) break;
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (info.ButtonID >= 1000)
            {
                int index = info.ButtonID - 1000;
                if (index < m_Vendor.MarketItems.Count)
                {
                    var mi = m_Vendor.MarketItems[index];
                    from.SendGump(new MarketBuyConfirmGump(from, m_Vendor, mi));
                }
            }
        }
    }

    // ==============================================================================
    // 5. 수량 확인 창
    // ==============================================================================
    public class MarketBuyConfirmGump : Gump
    {
        private RetailVendor m_Vendor;
        private MarketItem m_Item;

        public MarketBuyConfirmGump(Mobile from, RetailVendor vendor, MarketItem mi) : base(150, 150)
        {
            m_Vendor = vendor;
            m_Item = mi;

            AddPage(0);
            AddBackground(0, 0, 300, 250, 9270);
            
            AddHtml(10, 20, 280, 25, "<CENTER><BASEFONT COLOR='#FFFFFF'>Purchase Confirmation</BASEFONT></CENTER>", false, false);
            
            string itemName = mi.RealItem.Name ?? mi.RealItem.ItemData.Name;
            
            // 색상 1152(흰색) 적용
            AddLabel(30, 60, 1152, $"Item: {itemName}");
            AddLabel(30, 85, 1152, $"Price per unit: {mi.PricePerUnit:N0} GP");
            AddLabel(30, 110, 1152, $"Max Stock: {mi.RealItem.Amount}");

            AddLabel(30, 145, 53, "Enter Amount:");
            AddImageTiled(130, 145, 100, 20, 9354);
            AddTextEntry(135, 145, 90, 20, 1152, 1, "1");

            AddButton(60, 190, 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddLabel(95, 192, 68, "Purchase"); // 68 = 녹색

            AddButton(170, 190, 4017, 4019, 0, GumpButtonType.Reply, 0);
            AddLabel(205, 192, 33, "Cancel"); // 33 = 빨간색
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (info.ButtonID == 2)
            {
                TextRelay entry = info.GetTextEntry(1);
                int amount = entry != null ? Utility.ToInt32(entry.Text) : 0;

                if (amount > 0)
                {
                    var result = m_Vendor.TryBuyMarketItem(from, m_Item, amount);
                    from.SendMessage(result.Success ? 68 : 33, result.Message);
                }
                else
                {
                    from.SendMessage(33, "유효한 수량을 입력하세요.");
                }
            }
            
            from.SendGump(new RetailVendorShoppingGump(from, m_Vendor));
        }
    }
}