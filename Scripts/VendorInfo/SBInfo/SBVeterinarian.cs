using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBVeterinarian : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBVeterinarian()
        {
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
            public InternalBuyInfo()
            {
                Add(new GenericBuyInfo(typeof(Bandage), 10, 20, 0xE21, 0));
                Add(new GenericBuyInfo(typeof(SpidersSilk), 5, 20, 0xF8D, 0));
			}
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                Add(typeof(Bandage), 1);
                Add(typeof(SpidersSilk), 1);
            }
        }
    }
}