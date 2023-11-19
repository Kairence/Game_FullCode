using System;
using System.Collections.Generic;
using Server.Items;
using System.Linq;

namespace Server.Mobiles
{
    public class GenericSellInfo : IShopSellInfo
    {
        private readonly Dictionary<Type, int> m_Table = new Dictionary<Type, int>();
        private Type[] m_Types;
        public GenericSellInfo()
        {
        }

        public Type[] Types
        {
            get
            {
                if (m_Types == null)
                {
                    m_Types = new Type[m_Table.Keys.Count];
                    m_Table.Keys.CopyTo(m_Types, 0);
                }

                return m_Types;
            }
        }
        public void Add(Type type, int price)
        {
            m_Table[type] = price;
            m_Types = null;
        }

        public int GetSellPriceFor(Item item)
        {
            return GetSellPriceFor(item, null);
        }
		
        public int GetSellPriceFor(Item item, BaseVendor vendor)
        {
            int price = 0;
            m_Table.TryGetValue(item.GetType(), out price);

            if (vendor != null && BaseVendor.UseVendorEconomy)
            {
                IBuyItemInfo buyInfo = vendor.GetBuyInfo().OfType<GenericBuyInfo>().FirstOrDefault(info => info.EconomyItem && info.Type == item.GetType());

                if (buyInfo != null)
                {
                    int sold = buyInfo.TotalSold;
                    price = (int)((double)buyInfo.Price * .5);

                    return Math.Max(1, price);
                }
            }
			price = Misc.Util.Price(item, price);

            return price;
        }

        public int GetBuyPriceFor(Item item)
        {
            return GetBuyPriceFor(item, null);
        }

        public int GetBuyPriceFor(Item item, BaseVendor vendor)
        {
            return (int)(1.90 * GetSellPriceFor(item, vendor));
        }

        public string GetNameFor(Item item)
        {
            if (item.Name != null)
                return item.Name;
            else
                return item.LabelNumber.ToString();
        }

		/*
        public bool IsSellable(BaseVendor vendor, Item item)
        {
            if (item.QuestItem)
                return false;

			/*
			foreach (var bii in vendor.GetBuyInfo().OfType<GenericBuyInfo>())			
			{
				if ( bii.Type == item.GetType() )			
					return false;
			}
            return IsInList(item.GetType());
        }
		*/
		
        public bool IsSellable(Item item)
        {
            if (item.QuestItem)
                return false;


            return IsInList(item.GetType());
        }
		
		
        public bool IsResellable(Item item)
        {
            if (item.QuestItem)
                return false;
			
            //if ( item is IEquipOption )
			//	return false;

            return IsInList(item.GetType());
        }

        public bool IsInList(Type type)
        {
            return m_Table.ContainsKey(type);
        }
    }
}