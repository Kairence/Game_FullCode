using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBBowCrafter : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBBowCrafter()
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
                Add(new GenericBuyInfo(typeof(Bolt), 10, Utility.Random(30, 60), 0x1BFB, 0));
                Add(new GenericBuyInfo(typeof(Arrow), 10, Utility.Random(30, 60), 0xF3F, 0));
                Add(new GenericBuyInfo(typeof(Feather), 5, Utility.Random(30, 60), 0x1BD1, 0));
                Add(new GenericBuyInfo(typeof(Shaft), 5, Utility.Random(30, 60), 0x1BD4, 0));

            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                Add(typeof(Bolt), 1);
                Add(typeof(Arrow), 1);
                Add(typeof(Feather), 1);
                Add(typeof(Shaft), 1);
            }
        }
    }
}