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
using Server.Misc;

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

        public override void OnSingleClick(Mobile from) { base.OnSingleClick(from); }

        public override void OnDoubleClick(Mobile from) { from.SendMessage(0x22, "소매업 상인은 이제 주택의 구인 게시판(인력 시장)을 통해서만 스카웃할 수 있습니다."); from.SendMessage(0x22, "구입하신 디드는 5,000 골드로 환불됩니다."); from.AddToBackpack(new Server.Items.Gold(5000)); this.Delete(); }

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
    // 1. ��ϵ� �������� ������ ��� ����(Wrapper) ������ Ŭ����
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

    // 2. ���� �Ǹ� ��� �ھ� (BaseVendor ������� TownEconomy�� �ڵ� ����)
	public class RetailVendor : BaseVendor, IHouseEmployee 
    {
        public static List<RetailVendor> RetailVendors = [];

        private List<MarketItem> m_MarketItems;
        public List<MarketItem> MarketItems => m_MarketItems;

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Owner { get; set; }

        // AI Employment System Fields
        [CommandProperty(AccessLevel.GameMaster)]
        public Guid EmployeeID { get; set; } = Guid.Empty;

        public NpcJobClass JobClass => NpcJobClass.CaravanMaster;
        public NpcRank EmployeeRank => (NpcRank)EmployeeSkillLevel;
        public Mobile EmployeeMobile => this;
        
        [CommandProperty(AccessLevel.GameMaster)]
        public int DailyWage { get; set; } = 600;
        
        [CommandProperty(AccessLevel.GameMaster)]
        public int UnpaidWages { get; set; } = 0;
        
        [CommandProperty(AccessLevel.GameMaster)]
        public int EmployeeSkillLevel { get; set; } = 0;

        public static void ProcessDailyWages(int gameHour)
        {
            if (gameHour != 0) return; // Only process at midnight (logical 0 / 24)

            for (int i = RetailVendors.Count - 1; i >= 0; i--)
            {
                var vendor = RetailVendors[i];
                if (vendor == null || vendor.Deleted || vendor.EmployeeID == Guid.Empty) continue;

                if (vendor.HoldGold >= vendor.DailyWage)
                {
                    vendor.HoldGold -= vendor.DailyWage;
                }
                else
                {
                    vendor.EmployeeID = Guid.Empty;
                    vendor.EmployeeSkillLevel = 0;
                    if (vendor.Owner != null)
                    {
                        vendor.Owner.SendMessage(33, "[�˸�] �Ҹ� ���� ������(�ϱ� " + vendor.DailyWage + "G)�� �����Ͽ� �˹ٻ��� ����߽��ϴ�!");
                    }
                }
            }
        }


        // [����] �θ� Ŭ������ �����Ƿ� ���� ������ �����մϴ�.
        [CommandProperty(AccessLevel.GameMaster)]
        public int HoldGold { get; set; }

        private readonly List<SBInfo> m_SBInfos = [];
        protected override List<SBInfo> SBInfos => m_SBInfos;
        public override void InitSBInfo() { }

        public RetailVendor() : base("��ȭ��")
        {
            // 1. ������ �������� ���� (50% Ȯ��)
            this.Female = Utility.RandomBool();

            // 2. ������ ���� ���� ���� �̸� ��Ͽ��� ���� ����
            // ���� "human male", "human female" �Ǵ� ������ "male", "female"�� ����մϴ�.
            this.Name = NameList.RandomName(this.Female ? "female" : "male");

            // 3. ���� ���� ����
            this.m_MarketItems = [];
            RetailVendors.Add(this); 
            this.CantWalk = true; // ���ڸ� ����
        }

		// BaseVendor�� �⺻ �޴�(Buy/Sell)�� �����ϰ� �����ϱ� ���� �������̵��մϴ�.
		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            // base.GetContextMenuEntries(from, list); // �����ϰ� �����մϴ�.

            if (from == null || !from.Alive) 
                return;

            // 1. �ʼ� �⺻ ���: ĳ����â(��������) ����
            // Server.ContextMenus.PaperdollEntry�� ���� ǥ�� Ŭ�����Դϴ�.
            list.Add(new PaperdollEntry(this));

            // 2. �����ڿ� �޴�: ���� ���� (���� Ȥ�� GM)
            if (from == this.Owner || from.AccessLevel >= AccessLevel.GameMaster)
            {
                list.Add(new RetailManagementEntry(from, this));
                list.Add(new RelocateVendorEntry(from, this));
            }

            // 3. �մԿ� �޴�: ���� ���� (Ŀ���� ����)
            list.Add(new RetailBrowseEntry(from, this));

            // 4. (���û���) ������ ���Ⱑ �ʿ��ϸ� ���⼭ �߰�, �ʿ� ������ ���� ����
            // list.Add(new AskDestinationEntry(from, this)); 
        }

		// --- ���� Ŭ���� ���� ---
		private class RetailManagementEntry : ContextMenuEntry
		{
			private Mobile m_From;
			private RetailVendor m_Vendor;

			// 4. [CS1729 �ذ�] ������ �μ��� 1��(Cliloc ID)�� ����
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

			// 5. [CS1729 �ذ�] ������ �μ��� 1��(Cliloc ID)�� ����
			public RetailBrowseEntry(Mobile from, RetailVendor vendor) : base(6100) 
			{
				m_From = from;
				m_Vendor = vendor;
			}

			public override void OnClick()
			{
				m_From.SendMessage(0x44, $"{m_Vendor.Name}�� �Ŵ븦 ���캾�ϴ�.");
				// ���� ������ ���� â ȣ���
			}
		}

        public override void OnAfterDelete()
        {
            RetailVendors.Remove(this); 
            base.OnAfterDelete();
        }

        public RetailVendor(Serial serial) : base(serial) { }

        public void FireVendor(Server.Multis.BaseHouse house)
        {
            if (this.EmployeeID != Guid.Empty)
            {
                // Unemployment Compensation (State pays the wage to the AI)
                var nearestTown = Server.Misc.TownEconomyManager.Towns.Values
                    .OrderBy(t => Utility.GetDistanceToSqrt(t.Center, this.Location))
                    .FirstOrDefault(t => t.Facet == this.Map);

                if (nearestTown == null)
                    nearestTown = Server.Misc.TownEconomyManager.Towns.Values.FirstOrDefault(t => t.TownName == "Britain");

                if (nearestTown != null)
                {
                    nearestTown.Wealth = Math.Max(0, nearestTown.Wealth - this.DailyWage);
                }
                
                this.EmployeeID = Guid.Empty;
                this.EmployeeSkillLevel = 0;
            }

            // Move items to moving crate or bank
            if (house != null && house.MovingCrate != null)
            {
                foreach (var mi in this.MarketItems.ToList())
                {
                    if (mi.RealItem != null && !mi.RealItem.Deleted)
                        house.MovingCrate.DropItem(mi.RealItem);
                }
            }
            else if (this.Owner != null && this.Owner.BankBox != null)
            {
                foreach (var mi in this.MarketItems.ToList())
                {
                    if (mi.RealItem != null && !mi.RealItem.Deleted)
                        this.Owner.BankBox.DropItem(mi.RealItem);
                }
            }
            
            if (this.HoldGold > 0)
            {
                if (house != null && house.MovingCrate != null)
                    Server.Mobiles.Banker.Deposit(house.MovingCrate, this.HoldGold);
                else if (this.Owner != null)
                    Server.Mobiles.Banker.Deposit(this.Owner, this.HoldGold);
                    
                this.HoldGold = 0;
            }

            this.Delete();
        }

        public override void OnSingleClick(Mobile from) { base.OnSingleClick(from); }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster || from == Owner)
                from.SendGump(new RetailVendorManagementGump(from, this));
            else
                from.SendGump(new RetailVendorShoppingGump(from, this));
        }

        // ������ ��� �� ���� ���� ����
		public (bool Success, string Message) TryListMarketItem(Mobile seller, Item item, int price)
        {
            if (item == null || item.Deleted) return (false, "�������� �������� �ʽ��ϴ�.");
            if (item is Container) return (false, "������ ���� �Ǹſ� �Ŵ뿡 �ø� �� �����ϴ�.");
            if (item.Layer != Layer.Invalid && item.Layer != Layer.Backpack)
                return (false, "���� ���̰ų� Ư���� ���̾��� �������� ����� �� �����ϴ�.");
            int baseCapacity = 5; switch (EmployeeRank) { case NpcRank.Master: baseCapacity = 30; break; case NpcRank.Expert: baseCapacity = 20; break; case NpcRank.Journeyman: baseCapacity = 10; break; case NpcRank.Novice: baseCapacity = 5; break; } double multiplier = HouseTeamManager.GetEmployeeEfficiencyMultiplier(this); int maxItems = (int)Math.Max(1, baseCapacity * multiplier); if (m_MarketItems.Count >= maxItems) return (false, "이 상인의 직급 한도(" + maxItems + "개)를 초과했습니다. 직급: " + HouseTeamManager.GetJobTitle(this));

            // [�ٽ� ��ġ 1] ���� ���� ���� �ùķ��̼� (������ ���� ��� ����)
            Container pack = this.Backpack;
            if (pack != null)
            {
                // UO ���� �⺻ ������ 400�����Դϴ�.
                int maxWeight = pack.MaxWeight > 0 ? pack.MaxWeight : 400; 
                int currentWeight = pack.TotalWeight;
                int itemTotalWeight = item.TotalWeight;

                if (currentWeight + itemTotalWeight > maxWeight)
                {
                    int availableWeight = maxWeight - currentWeight;
                    if (availableWeight <= 0)
                        return (false, "���� ���԰� �� ���� �� �̻� ����� �� �����ϴ�.");

                    // ������ 1���� ���� ��� (0�� ��� ���)
                    double unitWeight = item.Weight > 0 ? item.Weight : 0.1;
                    int maxAmount = (int)(availableWeight / unitWeight);
                    
                    // ��� �ź� �� �ȳ� �޽��� ���
                    return (false, $"���� �ʰ�! ���� ���� ���Էδ� �ִ� {maxAmount}�������� ����� �� �ֽ��ϴ�.");
                }
            }

            if (price <= 0) return (true, ""); // ���� �ܰ� ����

            // [�ٽ� ��ġ 2] �̸� ����� ���� ���� ����
            // item.Name�� ���� �ǵ帮�� �ʽ��ϴ�. ���� ���¸� �����ؾ� Cliloc�� �۵��մϴ�.

            this.AddToBackpack(item);
            m_MarketItems.Add(new MarketItem(item, price, seller));
            
            return (true, "��� �Ϸ�.");
        }

        // �Ϲ� ���� ���� (���� ���� �Ϸ�)
        public (bool Success, string Message, Item BoughtItem) TryBuyMarketItem(Mobile buyer, MarketItem marketItem, int amount)
        {
            if (marketItem?.RealItem == null || marketItem.RealItem.Deleted)
                return (false, "�������� �ʴ� ��ǰ�Դϴ�.", null);

            if (marketItem.RealItem.Amount < amount)
                return (false, "���� �����մϴ�.", null);

            int totalCost = marketItem.PricePerUnit * amount;

            if (!buyer.Backpack.ConsumeTotal(typeof(Gold), totalCost))
                return (false, "��尡 �����մϴ�.", null);

            // �θ�(BaseVendor)�� �ݰ�� ��Ȯ�� ����
            this.HoldGold += totalCost;

            Item purchasedItem;
            if (marketItem.RealItem.Amount == amount)
            {
                purchasedItem = marketItem.RealItem;
                m_MarketItems.Remove(marketItem);
                // ���濡�� ���� �ʿ� ���� �ٷ� �������� AddToBackpack �ϸ� �̵��˴ϴ�.
            }
            else
            {
                purchasedItem = Mobile.LiftItemDupe(marketItem.RealItem, amount);
                // [�ٽ� ��ġ] ������ �κ� ���� �� ���� ������� �� ��ŭ ���� ����!
                marketItem.RealItem.Amount -= amount;
                marketItem.RealItem.InvalidateProperties();
            }

            buyer.AddToBackpack(purchasedItem);
            return (true, "���Ű� �Ϸ�Ǿ����ϴ�.", purchasedItem);
        }

		// RetailVendor Ŭ���� ���ο� �߰�
		private Item SafeDupe(Item oldItem, int amount)
		{
			try
			{
				// ������ ������ Ÿ���� �� ��ü ����
				Item newItem = (Item)Activator.CreateInstance(oldItem.GetType());

				// ������ �Ӽ� ����
				newItem.Hue = oldItem.Hue;
				newItem.ItemID = oldItem.ItemID;
				newItem.Name = oldItem.Name;
				newItem.LootType = oldItem.LootType;
				newItem.Weight = oldItem.Weight;
				
				// ���� �߿��� ���� ����: ��û���� �� 'amount'��ŭ�� ����
				newItem.Amount = amount;

				return newItem;
			}
			catch
			{
				return null;
			}
		}
        // AI ���� ���� ���� (���� �� ���� ���� ����)
        public Item ExtractItemForAI(MarketItem marketItem, int amount)
		{
			// ������ �����ϰų� �̹� ������ �������̸� �ź�
			if (marketItem?.RealItem == null || marketItem.RealItem.Deleted || marketItem.RealItem.Amount < amount) 
				return null;

			// 1. ���� ���� ��: ������ ��°�� �ѱ�� ����Ʈ���� ����
			if (marketItem.RealItem.Amount == amount)
			{
				Item extracted = marketItem.RealItem;
				m_MarketItems.Remove(marketItem);
				return extracted;
			}
			else
			{
				// 2. �κ� ���� ��: ������ LiftItemDupe ��� �����ϰ� ���� Dupe(����) ȣ��
				Item extracted = SafeDupe(marketItem.RealItem, amount);
				
				if (extracted != null)
				{
					// �纻�� ���������� ��������� ���� ���� ���� ����
					marketItem.RealItem.Amount -= amount;
					marketItem.RealItem.InvalidateProperties();
					
					// ���� ���� ��� ������ ������ 0 ���ϰ� �Ǹ� ����Ʈ���� ���� ���� (����ȭ ����)
					if (marketItem.RealItem.Amount <= 0)
					{
						m_MarketItems.Remove(marketItem);
					}
				}
				else
				{
					// ���� ���� ������ ������ �����ϸ� null�� ��ȯ�Ͽ� �ŷ��� ��ȿȭ (���� ��ȣ)
					return null; 
				}

				return extracted;
			}
		}

		
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1); // Version 1

            writer.Write(HoldGold); // [�߿�] ���ͱ� ����

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
                HoldGold = reader.ReadInt(); // [�߿�] ���ͱ� ����

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
    // 3. �Ǹ���(Owner)�� ���� Gump
	public class RetailVendorManagementGump : Gump
    {
        private RetailVendor m_Vendor;

        public RetailVendorManagementGump(Mobile from, RetailVendor vendor) : base(50, 50)
        {
            m_Vendor = vendor;

            AddPage(0);
            
            // 1. ���� ���: ���� �⺻���̰� �������� ���� ��� (400x500)
            AddBackground(0, 0, 400, 500, 9270);
            
            // 2. ��� Ÿ��Ʋ �� ���м�
            AddLabel(145, 15, 0x480, "�������� ȭ��");
            AddImageTiled(20, 40, 360, 2, 2624); // ���� ��� ������ �Ǽ�

            // 3. ���� ���� ����
            AddLabel(35, 55, 0x34, "���� �Ǹ� ���ͱ�"); 
            AddLabel(35, 75, 0x44, $"{m_Vendor.HoldGold:N0} GP"); // ���ͱ� ��� ����
            
            // ��ȭ �ȱ� ��ư (ǥ�� �Ķ��� ��ư 4005�� ���)
            AddButton(260, 65, 4005, 4007, 1, GumpButtonType.Reply, 0); 
            AddLabel(295, 67, 1152, "��ȭ �ȱ�");

            AddImageTiled(20, 105, 360, 2, 2624); 

            // 4. ���� ���� ����
            // ���ο� ��ǰ ���
            AddButton(35, 120, 4011, 4013, 2, GumpButtonType.Reply, 0); 
            AddLabel(75, 122, 1152, "���ο� ��ǰ ��� (Ÿ��)");

            // ���� ��ġ �̵�
            AddButton(35, 155, 4005, 4007, 3, GumpButtonType.Reply, 0);
            AddLabel(75, 157, 1152, "���� ��ġ �̵�");

            AddImageTiled(20, 190, 360, 2, 2624); 

            // 5. ��ǰ ��� ��� (��� ���� 0x480)
            AddLabel(35, 200, 0x480, "ǰ��");
            AddLabel(185, 200, 0x480, "����");
            AddLabel(250, 200, 0x480, "����");
            AddLabel(330, 200, 0x480, "ȸ��");

            // 6. ���� ��ǰ ����Ʈ (������ Y��ǥ ���)
            int y = 230;
            for (int i = 0; i < m_Vendor.MarketItems.Count; i++)
            {
                var mi = m_Vendor.MarketItems[i];
                if (mi.RealItem == null || mi.RealItem.Deleted) continue;

                Item item = mi.RealItem;

                // [����] ���� ������(Cliloc)�� Ŀ���� �̸� ������ �б� ó��
                if (item.Name != null)
                {
                    // �̸��� ����� ������ (��: [Exceptional] �۾����ũ)
                    string name = item.Name;
                    if (name.Length > 16) name = name.Substring(0, 14) + "..";
                    AddLabel(35, y, 1152, name); // ���� 1152 ���� ����
                }
                else
                {
                    // �̸��� ���� ���� ������ (Ŭ���̾�Ʈ Cliloc ������)
                    // ��Ʈ ũ��/���� HTML ���� ���� �����ϰ� ȣ��. 
                    // Width 140 �������� �� �̸��� �� ĭ�� ħ������ �ʰ� ���.
                    AddHtmlLocalized(35, y, 140, 20, item.LabelNumber, 0x7FFF, false, false);
                }

                AddLabel(190, y, 1152, item.Amount.ToString());
                AddLabel(255, y, 1152, mi.PricePerUnit.ToString());

                // ȸ�� ��ư (������ X ��ư 4017�� ���)
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
                    from.SendMessage(68, $"{m_Vendor.HoldGold:N0} ��带 �����߽��ϴ�.");
                    m_Vendor.HoldGold = 0;
                }
                from.SendGump(new RetailVendorManagementGump(from, m_Vendor));
            }
            else if (info.ButtonID == 2) 
            {
                from.SendMessage(53, "�Ŵ뿡 �ø� �������� �����ϼ���.");
                from.Target = new InternalListTarget(m_Vendor);
            }
			else if (info.ButtonID == 3) // ��ġ �̵� ��ư
			{
				from.SendMessage(0x35, "����� �ű� ���ο� ��ġ�� �����ϼ���.");
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
                    from.SendMessage(53, "��ǰ �ǸŸ� �����ϰ� ȸ���߽��ϴ�.");
                }
                from.SendGump(new RetailVendorManagementGump(from, m_Vendor));
            }
			else if (info.ButtonID >= 100) // [����] ��ǰ ȸ�� ����
            {
                int index = info.ButtonID - 100;
                if (index >= 0 && index < m_Vendor.MarketItems.Count)
                {
                    var mi = m_Vendor.MarketItems[index];
                    
                    if (mi.RealItem != null && !mi.RealItem.Deleted)
                    {
                        // ��� ���濡�� ���� �������� ������ �̵�
                        from.AddToBackpack(mi.RealItem); 
                        from.SendMessage(68, $"{mi.RealItem.Name ?? mi.RealItem.ItemData.Name}��(��) �Ŵ뿡�� ȸ���߽��ϴ�.");
                        
                        // ����Ʈ���� ����
                        m_Vendor.MarketItems.RemoveAt(index);
                    }
                    else
                    {
                        m_Vendor.MarketItems.RemoveAt(index); // �������� ������ ����Ʈ������ ����
                    }
                }
                // â ���ΰ�ħ
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

					// [����] 6�� �μ��� �޴� CheckHold �ñ״�ó ����
					// �μ�: (����, ������, �޽�������, ��������, ����, �߰�����)
					// ������ �μ��� 0�� �־� '������ ���� ����'�� üũ�ϵ��� �մϴ�.
					if (!pack.CheckHold(from, item, false, true, item.Amount, 0))
					{
						from.SendMessage(33, "����� ������ �ʹ� ���̰ų� �������� �ʹ� ���� ���� �� �����ϴ�.");
						from.SendGump(new RetailVendorManagementGump(from, m_Vendor));
						return;
					}

					// 2. �߰����� ���� ���� ��� (�Ҽ��� ���� ����)
					int maxWeight = pack.MaxWeight;
					int currentWeight = pack.TotalWeight;
					// C# 12�� ������ ���� ���� ���
					int itemTotalWeight = (int)Math.Ceiling(item.Weight * item.Amount);

					if (currentWeight + itemTotalWeight > maxWeight)
					{
						int availableWeight = maxWeight - currentWeight;
						double unitWeight = item.Weight > 0 ? item.Weight : 0.1;
						int maxAmount = (int)(availableWeight / unitWeight);
						
						from.SendMessage(33, $"���� �뷮 �ʰ�! ���� ���� ���Էδ� �ִ� {maxAmount}�������� ��� �����մϴ�.");
						from.SendGump(new RetailVendorManagementGump(from, m_Vendor));
						return;
					}

					// 3. �⺻ �Ǹ� ��Ģ üũ (0�� ����)
					var (success, message) = m_Vendor.TryListMarketItem(from, item, 0);

					if (success)
					{
						from.Prompt = new InternalPricePrompt(m_Vendor, item);
						from.SendMessage(53, "�� �������� [���� �Ǹ� ����]�� �Է��ϼ���.");
					}
					else
					{
						from.SendMessage(33, message); 
						from.SendGump(new RetailVendorManagementGump(from, m_Vendor));
					}
				}
				else 
				{
					from.SendMessage(33, "�ڽ��� ���濡 �ִ� �����۸� ����� �� �ֽ��ϴ�.");
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

				// �� ��������, �������� �ٽ� Ȯ�� (����)
				Server.Multis.BaseHouse house = Server.Multis.BaseHouse.FindHouseAt(from);
				if (house == null || !house.IsOwner(from))
				{
					from.SendMessage(33, "�ڽ��� �� ���ηθ� �̵���ų �� �ֽ��ϴ�.");
					return;
				}

				// ������ �������� ��� �ڷ���Ʈ
				m_Vendor.MoveToWorld(new Point3D(p), from.Map);
				from.SendMessage(68, "����� ��ġ�� �Ű���ϴ�.");
				
				// �̵� �� ����â �ٽ� �����ֱ�
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
                else from.SendMessage(33, "��ȿ�� ���ڸ� �Է��ϼ���.");
                
                from.SendGump(new RetailVendorManagementGump(from, m_Vendor));
            }
        }
    }

    // ==============================================================================
    // 4. ������(Customer)�� ���� Gump
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
                
                // ���� 0xFFFFFF ���� �κ��� ���� 1152(���) �� 53(�����)���� ����
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
    // 5. ���� Ȯ�� â
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
            
            // ���� 1152(���) ����
            AddLabel(30, 60, 1152, $"Item: {itemName}");
            AddLabel(30, 85, 1152, $"Price per unit: {mi.PricePerUnit:N0} GP");
            AddLabel(30, 110, 1152, $"Max Stock: {mi.RealItem.Amount}");

            AddLabel(30, 145, 53, "Enter Amount:");
            AddImageTiled(130, 145, 100, 20, 9354);
            AddTextEntry(135, 145, 90, 20, 1152, 1, "1");

            AddButton(60, 190, 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddLabel(95, 192, 68, "Purchase"); // 68 = ���

            AddButton(170, 190, 4017, 4019, 0, GumpButtonType.Reply, 0);
            AddLabel(205, 192, 33, "Cancel"); // 33 = ������
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
                    from.SendMessage(33, "��ȿ�� ������ �Է��ϼ���.");
                }
            }
            
            from.SendGump(new RetailVendorShoppingGump(from, m_Vendor));
        }
    }
public class RelocateVendorEntry : ContextMenuEntry
    {
        private Mobile m_From;
        private RetailVendor m_Vendor;

        public RelocateVendorEntry(Mobile from, RetailVendor vendor) : base(6136, 3) // 6136 = "Move" or similar
        {
            m_From = from;
            m_Vendor = vendor;
        }

        public override void OnClick()
        {
            m_From.SendMessage("알바생을 이동시킬 위치를 클릭하세요.");
            m_From.Target = new RelocateVendorTarget(m_Vendor);
        }
    }

    public class RelocateVendorTarget : Server.Targeting.Target
    {
        private RetailVendor m_Vendor;

        public RelocateVendorTarget(RetailVendor vendor) : base(12, true, Server.Targeting.TargetFlags.None)
        {
            m_Vendor = vendor;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            if (m_Vendor == null || m_Vendor.Deleted) return;

            IPoint3D p = targeted as IPoint3D;
            if (p != null)
            {
                BaseHouse house = BaseHouse.FindHouseAt(new Point3D(p), from.Map, 16);
                if (house != null && house.IsOwner(from))
                {
                    m_Vendor.MoveToWorld(new Point3D(p), from.Map);
                    m_Vendor.Home = m_Vendor.Location;
                    m_Vendor.RangeHome = 0;
                    from.SendMessage("알바생의 위치가 고정되었습니다.");
                }
                else
                {
                    from.SendMessage("집 안의 올바른 위치가 아닙니다.");
                }
            }
        }
    }









}

