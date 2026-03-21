using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Items;
using Server.Misc;

namespace Server.Mobiles
{
    public class PrivateVendor : TownVendor
    {
        private Dictionary<Type, WarehouseItem> m_PrivateWarehouse = new Dictionary<Type, WarehouseItem>();
        private double m_LogisticsSurcharge = 1.0;

        [CommandProperty(AccessLevel.GameMaster)]
        public double LogisticsSurcharge { get { return m_LogisticsSurcharge; } set { m_LogisticsSurcharge = value; } }

        [Constructable]
        public PrivateVendor() : base("Private") { }
        public PrivateVendor(Serial serial) : base(serial) { }

        public override void SetInventory(string vendorName, List<TownInventoryEntry> entries)
        {
            this.Name = vendorName;
            this.TownName = "Private"; 

            // 부모 클래스의 m_CurrentEntries에 저장하여 InitSBInfo가 정상 작동하게 함
            this.m_CurrentEntries = entries;

            m_PrivateWarehouse.Clear();
            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    m_PrivateWarehouse[entry.ItemType] = new WarehouseItem(entry.ItemType, entry.InitialStock, entry.BasePrice);
                }
            }

            NewVendorSystem.ClearBuyInfoCache(this);
            LoadSBInfo(); // 이 함수가 부모의 InitSBInfo를 호출하여 m_SBInfos를 채움
            
            Console.WriteLine($"[PrivateVendor] {vendorName} spawned with {m_LogisticsSurcharge:F2}x surcharge.");
        }

        protected override void SyncEconomy(IBuyItemInfo[] info)
        {
            if (info == null || m_PrivateWarehouse == null) return;

            foreach (IBuyItemInfo bii in info)
            {
                if (bii is GenericBuyInfo gbi && m_PrivateWarehouse.TryGetValue(gbi.Type, out var invItem))
                {
                    gbi.Price = (int)(invItem.BasePrice * m_LogisticsSurcharge);
                    gbi.Amount = Math.Min(500, invItem.Stock); 
                }
            }
        }

        public override bool OnBuyItems(Mobile buyer, List<BuyItemResponse> list)
        {
            // [영수증 픽스] 엔진 훼손 전 미리 구매 품목 파악
            Dictionary<Type, int> receipt = new Dictionary<Type, int>();
            foreach (BuyItemResponse res in list)
            {
                Item itemOnVendor = this.BuyPack.Items.FirstOrDefault(i => i.Serial == res.Serial);
                if (itemOnVendor != null)
                {
                    Type type = itemOnVendor.GetType();
                    if (receipt.ContainsKey(type)) receipt[type] += res.Amount;
                    else receipt[type] = res.Amount;
                }
            }

            if (!NewVendorSystem.CheckStockBeforeBuy(this, buyer, list, m_PrivateWarehouse))
                return false;

            bool success = base.OnBuyItems(buyer, list);

            if (!success && buyer.AccessLevel >= AccessLevel.GameMaster) success = true;

            if (success)
            {
                // 영수증을 바탕으로 개인 창고에서 차감
                foreach (var kvp in receipt)
                {
                    if (m_PrivateWarehouse.TryGetValue(kvp.Key, out var invItem))
                    {
                        invItem.Stock -= kvp.Value;
                        if (invItem.Stock < 0) invItem.Stock = 0;
                    }
                }
                Timer.DelayCall(TimeSpan.FromSeconds(1.0), RestockCheck);
            }

            return success;
        }

        private void RestockCheck()
        {
            foreach (var inv in m_PrivateWarehouse.Values)
            {
                if (inv.Stock <= 0)
                {
                    inv.Stock = Utility.RandomMinMax(20, 50);
                    this.Say($"Wait a second... I just restocked more {inv.ItemType.Name}.");
                }
            }
        }

        // [★ 핵심 교정] 직렬화 순서 동기화
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer); // 부모(TownVendor)의 Serialize(버전 1, 마을이름) 실행
            writer.Write((int)1);   // PrivateVendor 버전

            writer.Write(m_LogisticsSurcharge);
            writer.Write(m_PrivateWarehouse.Count);
            foreach (var kvp in m_PrivateWarehouse)
            {
                writer.Write(kvp.Key.FullName);
                writer.Write(kvp.Value.Stock);
                writer.Write(kvp.Value.BasePrice);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader); // 부모(TownVendor)의 Deserialize 실행
            int version = reader.ReadInt();

            m_LogisticsSurcharge = reader.ReadDouble();
            int count = reader.ReadInt();
            m_PrivateWarehouse = new Dictionary<Type, WarehouseItem>();
            for (int i = 0; i < count; i++)
            {
                string typeName = reader.ReadString();
                int stock = reader.ReadInt();
                int price = reader.ReadInt();
                Type t = ScriptCompiler.FindTypeByFullName(typeName);
                if (t != null) m_PrivateWarehouse[t] = new WarehouseItem(t, stock, price);
            }
        }
    }
}