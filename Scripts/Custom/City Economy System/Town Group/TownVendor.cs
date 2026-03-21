using System;
using System.Collections.Generic;
using System.Reflection;
using Server;
using Server.Items;
using Server.Misc;
using System.Linq;

namespace Server.Mobiles
{
    public class TownVendor : BaseVendor
    {
        protected string m_TownName; 
        protected List<SBInfo> m_SBInfos = new List<SBInfo>(); 
        protected List<TownInventoryEntry> m_CurrentEntries = new List<TownInventoryEntry>();

        protected bool m_IsTrading = false;

        [CommandProperty(AccessLevel.GameMaster)]
        public string TownName { get { return m_TownName; } set { m_TownName = value; } }

        protected override List<SBInfo> SBInfos => m_SBInfos;

        [Constructable]
        public TownVendor(string townName) : base("the merchant") 
        { 
            m_TownName = townName; 
            Timer.DelayCall(TimeSpan.FromSeconds(1.0), () => {
                if (!string.IsNullOrEmpty(m_TownName) && m_TownName != "Private")
                {
                    if (this.m_CurrentEntries == null || this.m_CurrentEntries.Count == 0)
                        this.SetInventory(this.Name, TownInventoryData.GetSetupData(m_TownName));
                }
            });
        }

        public TownVendor(Serial serial) : base(serial) { }

        public virtual void SetInventory(string vendorName, List<TownInventoryEntry> entries)
        {
            this.Name = vendorName;
            this.m_TownName = this.TownName;
            m_CurrentEntries = entries; 
            NewVendorSystem.ClearBuyInfoCache(this);
            LoadSBInfo(); 
        }

        // [★ 핵심 1] 판매 장부를 코어 엔진 기본값이 아닌, '마을 경제 연동 장부'로 강제 고정합니다!
        public override IShopSellInfo[] GetSellInfo()
        {
            if (m_SBInfos.Count > 0 && m_SBInfos[0] is TownSBInfo tsbi)
            {
                return new IShopSellInfo[] { tsbi.SellInfo };
            }
            return base.GetSellInfo();
        }

        public override IBuyItemInfo[] GetBuyInfo()
        {
            IBuyItemInfo[] info = base.GetBuyInfo();
            if (info != null)
            {
                SyncEconomy(info); // 조건 없이 무조건 실행!
            }
            return info;
        }

        protected virtual void SyncEconomy(IBuyItemInfo[] info)
        {
            if (info == null || string.IsNullOrEmpty(m_TownName) || !TownEconomyManager.Towns.TryGetValue(m_TownName, out var town)) 
                return;

            foreach (IBuyItemInfo bii in info)
            {
                if (bii is GenericBuyInfo gbi)
                {
                    if (town.Warehouse.TryGetValue(gbi.Type, out var invItem))
                    {
                        // [★ 핵심 2] 가격(Price)은 거래 중이든 아니든 무조건 동기화시켜서 100gp 버그 원천 차단!
                        gbi.Price = town.GetPrice(gbi.Type, town.PriceMultiplier); 
                        
                        // 단, 수량(Amount)은 거래 중이 아닐 때만 500개로 갱신 (엔진 결제 차감 보호)
                        if (!m_IsTrading)
                        {
                            gbi.Amount = Math.Min(500, invItem.Stock); 
                        }
                    }
                }
            }
        }
        
        public override bool OnSellItems(Mobile seller, List<SellItemResponse> list)
        {
            if (string.IsNullOrEmpty(m_TownName) || !TownEconomyManager.Towns.TryGetValue(m_TownName, out var town))
                return base.OnSellItems(seller, list);

            m_IsTrading = true; 
            try
            {
                Dictionary<Item, int> preSellAmounts = new Dictionary<Item, int>();
                foreach (SellItemResponse res in list)
                {
                    if (res.Item != null) preSellAmounts[res.Item] = res.Item.Amount;
                }

                // 위에서 수정한 InternalSellInfo의 철벽 검증을 거친 후 실제 판매 진행
                bool success = base.OnSellItems(seller, list);

                if (success)
                {
                    long expectedPayout = 0;
                    int totalSoldCount = 0;

                    foreach (var kvp in preSellAmounts)
                    {
                        Item item = kvp.Key;
                        int oldAmount = kvp.Value;
                        int newAmount = item.Deleted ? 0 : item.Amount;
                        int soldAmount = oldAmount - newAmount;

                        if (soldAmount > 0)
                        {
                            Type type = item.GetType();
                            
                            // [★ 픽스] IsSellable을 통과한 '취급 품목'이므로 무조건 Warehouse에 존재합니다. 수량만 쏙 더해줍니다.
                            if (town.Warehouse.TryGetValue(type, out var invItem))
                            {
                                invItem.Stock += soldAmount;
                                
                                int buyPrice = Math.Max(1, town.GetPrice(type, town.PriceMultiplier) / 2);
                                expectedPayout += (buyPrice * soldAmount);
                                totalSoldCount += soldAmount;
                            }
                        }
                    }

                    if (expectedPayout > 0)
                    {
                        town.Wealth -= expectedPayout;
                        if (town.Wealth < 0) town.Wealth = 0;
                    }
                }
                return success;
            }
            finally
            {
                m_IsTrading = false; 
            }
        }

        public override bool OnBuyItems(Mobile buyer, List<BuyItemResponse> list)
        {
            if (string.IsNullOrEmpty(m_TownName) || !TownEconomyManager.Towns.TryGetValue(m_TownName, out var town))
                return base.OnBuyItems(buyer, list);

            m_IsTrading = true; 
            try
            {
                Dictionary<GenericBuyInfo, (int amt, int tot, int exactPrice)> preStock = new Dictionary<GenericBuyInfo, (int, int, int)>();
                var buyInfos = this.GetBuyInfo();
                if (buyInfos != null)
                {
                    foreach (var bii in buyInfos)
                        if (bii is GenericBuyInfo gbi) preStock[gbi] = (gbi.Amount, gbi.TotalBought, gbi.Price);
                }

                bool success = base.OnBuyItems(buyer, list);

                if (success)
                {
                    long expectedGold = 0;
                    int totalBoughtCount = 0;

                    foreach (var kvp in preStock)
                    {
                        GenericBuyInfo gbi = kvp.Key;
                        int oldAmount = kvp.Value.amt;
                        int oldTotalBought = kvp.Value.tot;
                        int receiptPrice = kvp.Value.exactPrice; 
                        
                        int boughtAmount = Math.Max(oldAmount - gbi.Amount, gbi.TotalBought - oldTotalBought);

                        if (boughtAmount > 0)
                        {
                            if (town.Warehouse.TryGetValue(gbi.Type, out var invItem))
                            {
                                invItem.Stock -= boughtAmount;
                                if (invItem.Stock < 0) invItem.Stock = 0;
                            }
                            expectedGold += (receiptPrice * boughtAmount);
                            totalBoughtCount += boughtAmount;
                        }
                    }

                    if (expectedGold > 0)
                    {
                        town.Wealth += expectedGold; 
                    }
                }
                return success;
            }
            finally
            {
                m_IsTrading = false;
            }
        }

        public virtual void ClearAndReload()
        {
            try {
                FieldInfo buyField = typeof(BaseVendor).GetField("m_BuyInfo", BindingFlags.Instance | BindingFlags.NonPublic);
                if (buyField != null) buyField.SetValue(this, null); 
            } catch { }
        }

        public override void InitSBInfo() 
        { 
            if (m_CurrentEntries != null && m_CurrentEntries.Count > 0)
                m_SBInfos.Add(new TownSBInfo(m_TownName, m_CurrentEntries));
            else
                m_SBInfos.Add(new TownSBInfo("Empty", new List<TownInventoryEntry>()));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)2);
            writer.Write(m_TownName);
            
            if (m_CurrentEntries == null) m_CurrentEntries = new List<TownInventoryEntry>();
            writer.Write(m_CurrentEntries.Count);
            foreach (var entry in m_CurrentEntries)
            {
                writer.Write(entry.ItemType.FullName);
                writer.Write(entry.InitialStock);
                writer.Write(entry.BasePrice);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader); 
            int version = reader.ReadInt();
            m_TownName = reader.ReadString();

            if (version >= 2)
            {
                int count = reader.ReadInt();
                m_CurrentEntries = new List<TownInventoryEntry>();
                for (int i = 0; i < count; i++)
                {
                    string typeName = reader.ReadString();
                    int stock = reader.ReadInt();
                    int price = reader.ReadInt();
                    
                    Type t = ScriptCompiler.FindTypeByFullName(typeName);
                    if (t != null)
                        m_CurrentEntries.Add(new TownInventoryEntry(t, stock, price));
                }
            }

            LoadSBInfo(); 

            Timer.DelayCall(TimeSpan.FromSeconds(2.0), () => {
                if (!string.IsNullOrEmpty(m_TownName) && m_TownName != "Private")
                    this.SetInventory(this.Name, m_CurrentEntries);
            });
        }
    }

// [★ 완벽 복원된 마을 연동형 상점 장부]
    public class TownSBInfo : SBInfo
    {
        private string m_TownName;
        private List<GenericBuyInfo> m_BuyInfo = new List<GenericBuyInfo>();
        private InternalSellInfo m_SellInfo; 

        public override IShopSellInfo SellInfo => m_SellInfo;
        public override List<GenericBuyInfo> BuyInfo => m_BuyInfo;

        public TownSBInfo(string townName, List<TownInventoryEntry> entries)
        {
            m_TownName = townName;
            
            // [★ 핵심 1] 상인에게 "너는 이 물건(entries)들만 취급해!"라고 전용 리스트를 쥐여줍니다.
            m_SellInfo = new InternalSellInfo(townName, entries); 

            foreach (var entry in entries)
            {
                int itemID = 0x14F0; 
                try
                {
                    Item tempItem = (Item)Activator.CreateInstance(entry.ItemType);
                    if (tempItem != null)
                    {
                        itemID = tempItem.ItemID;
                        tempItem.Delete(); 
                    }
                }
                catch { }

                GenericBuyInfo gbi = new GenericBuyInfo(entry.ItemType, entry.BasePrice, entry.InitialStock, itemID, 0);
                m_BuyInfo.Add(gbi);
            }
        }

        public class InternalSellInfo : IShopSellInfo
        {
            private string m_Town;
            private HashSet<Type> m_MyVendorTypes = new HashSet<Type>(); // 이 NPC만의 고유 취급 품목

            public InternalSellInfo(string town, List<TownInventoryEntry> entries) 
            { 
                m_Town = town; 
                
                // [★ 핵심 2] 이 상인이 파는 물건의 '종류(Type)'만 따로 추출해서 기억해둡니다.
                foreach(var entry in entries)
                {
                    m_MyVendorTypes.Add(entry.ItemType);
                }
            }

            public int GetSellPriceFor(Item item, BaseVendor v)
            {
                // 거대한 마을 창고가 아니라, "내 전용 품목(m_MyVendorTypes)"에 있는지 확인!
                if (m_MyVendorTypes.Contains(item.GetType()) && TownEconomyManager.Towns.TryGetValue(m_Town, out var town))
                {
                    int basePrice = town.GetPrice(item.GetType(), town.PriceMultiplier);
                    return Math.Max(1, basePrice / 2); 
                }
                return 0;
            }

            public int GetBuyPriceFor(Item item, BaseVendor v) => GetSellPriceFor(item, v);
            public string GetNameFor(Item item) => item.Name ?? item.GetType().Name;
            
            public bool IsSellable(Item item) 
            {
                if (item.QuestItem) return false;
                
                // [★ 핵심 3] 내가 파는 물건과 똑같은 종류만 유저에게서 사들입니다.
                return m_MyVendorTypes.Contains(item.GetType()); 
            }
            
            public bool IsResellable(Item item) => IsSellable(item);
            
            // [★ 핵심 4] 엔진에게 "나는 딱 요 리스트에 있는 물건만 매입합니다" 라고 정확히 제출!
            public Type[] Types => m_MyVendorTypes.ToArray();

            public int GetBuyPriceFor(Item item) => GetBuyPriceFor(item, null);
            public int GetSellPriceFor(Item item) => GetSellPriceFor(item, null);
        }
    }
}