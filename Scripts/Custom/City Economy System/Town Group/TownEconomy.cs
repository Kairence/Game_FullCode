using System;
using System.Collections.Generic;
using Server;

namespace Server.Misc
{
    // [1] WarehouseItem 정의 (공용 사용)
    public class WarehouseItem
    {
        public Type ItemType { get; set; }
        public int Stock { get; set; }
        public int BasePrice { get; set; }
        public WarehouseItem(Type type, int stock, int price) { ItemType = type; Stock = stock; BasePrice = price; }
    }

    // [2] TownEconomy 정의 (모든 속성 통합)
    public partial class TownEconomy
    {
        public string TownName { get; set; }
        public Point3D Center { get; set; }
        public Map Facet { get; set; }
        public long Wealth { get; set; }
        public long BaseWealth { get; set; }
        public long TaxFund { get; set; }
        public int MaxInventoryCapacity { get; set; } // [★ 추가]
        
        public Dictionary<Type, WarehouseItem> Warehouse { get; set; } = new Dictionary<Type, WarehouseItem>();
        
        public double PriceMultiplier => Math.Clamp((double)Wealth / (BaseWealth > 0 ? BaseWealth : 1), 0.5, 1.5);

        // 인자 4개짜리 생성자 확실히 정의
        public TownEconomy(string name, Point3D center, Map map, long baseWealth)
        {
            TownName = name; Center = center; Facet = map; BaseWealth = baseWealth; Wealth = baseWealth;
            MaxInventoryCapacity = 2000;
        }

        public void SupplyItem(Type type, int amount, int price)
        {
            if (Warehouse.ContainsKey(type)) Warehouse[type].Stock += amount;
            else Warehouse[type] = new WarehouseItem(type, amount, price);
        }

        public int GetPrice(Type type, double multiplier)
        {
            if (Warehouse.TryGetValue(type, out var item)) return (int)(item.BasePrice * multiplier);
            return 100;
        }

		public virtual void Serialize(GenericWriter writer)
		{
			writer.Write((int)2); // 버전을 2로 격상

			writer.Write(TownName);
			writer.Write(MaxInventoryCapacity);
			writer.Write((long)TaxFund);
			
			// [★추가] 경제 지표 저장
			writer.Write((long)Wealth);
			writer.Write((long)BaseWealth);

			writer.Write(Warehouse.Count);
			foreach (var kvp in Warehouse)
			{
				writer.Write(kvp.Key.FullName);
				writer.Write(kvp.Value.Stock);
				writer.Write(kvp.Value.BasePrice);
			}
		}

		public virtual void Deserialize(GenericReader reader)
		{
			int version = reader.ReadInt();

			// version 0, 1, 2 모두 공통인 부분
			this.TownName = reader.ReadString();
			this.MaxInventoryCapacity = reader.ReadInt();
			this.TaxFund = reader.ReadLong();

			// [★ 핵심] Serialize에서 version 2일 때 Wealth와 BaseWealth를 썼으므로 여기서 읽어줘야 함
			if (version >= 2)
			{
				this.Wealth = reader.ReadLong();
				this.BaseWealth = reader.ReadLong();
			}

			// 이제야 순서상 창고 개수가 나올 차례입니다.
			int count = reader.ReadInt();
			for (int i = 0; i < count; i++)
			{
				string typeName = reader.ReadString();
				int stock = reader.ReadInt();
				int price = reader.ReadInt();

				Type type = ScriptCompiler.FindTypeByFullName(typeName);
				if (type != null)
				{
					Warehouse[type] = new WarehouseItem(type, stock, price);
				}
			}
		}
    }
}