using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBScribe : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo;
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();

        public SBScribe(Mobile m)
        {
            if (m != null)
            {
                m_BuyInfo = new InternalBuyInfo(m);
            }
        }

        public override IShopSellInfo SellInfo
        {
            get
            {
                return m_SellInfo;
            }
        }
        public override List<GenericBuyInfo> BuyInfo
        {
            get
            {
                return m_BuyInfo;
            }
        }

        public class InternalBuyInfo : List<GenericBuyInfo>
        {
            public InternalBuyInfo(Mobile m)
            {
                Add(new GenericBuyInfo(typeof(BlankScroll), 10, 999, 0x0E34, 0));
                Type[] types = Loot.RegularScrollTypes;

                int circles = 6;

                for (int i = 0; i < circles * 8 && i < types.Length; ++i)
                {
                    int itemID = 0x1F2E + i;

                    if (i == 6)
                        itemID = 0x1F2D;
                    else if (i > 6)
                        --itemID;

                    Add(new GenericBuyInfo(types[i], 10 + ((i / 8) * (i / 8) * 10), 20, itemID, 0, true));
                }
				
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                Add(typeof(BlankScroll), 1);
                Type[] types = Loot.RegularScrollTypes;

                int circles = 6;
                for (int i = 0; i < circles * 8 && i < types.Length; ++i)
                {
                    int itemID = 0x1F2E + i;

                    if (i == 6)
                        itemID = 0x1F2D;
                    else if (i > 6)
                        --itemID;

                    Add(types[i], 12 + ((i / 8) * 10));
                }				
            }
        }
    }
}
