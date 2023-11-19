using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBPlayerBarkeeper : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBPlayerBarkeeper()
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
                Add(new BeverageBuyInfo(typeof(BeverageBottle), BeverageType.Ale, 10, 20, 0x99F, 0));
                Add(new BeverageBuyInfo(typeof(BeverageBottle), BeverageType.Wine, 10, 20, 0x9C7, 0));
                Add(new BeverageBuyInfo(typeof(BeverageBottle), BeverageType.Liquor, 10, 20, 0x99B, 0));
                Add(new BeverageBuyInfo(typeof(Jug), BeverageType.Cider, 20, 20, 0x9C8, 0));
                Add(new BeverageBuyInfo(typeof(Pitcher), BeverageType.Milk, 20, 20, 0x9F0, 0));
                Add(new BeverageBuyInfo(typeof(Pitcher), BeverageType.Ale, 20, 20, 0x1F95, 0));
                Add(new BeverageBuyInfo(typeof(Pitcher), BeverageType.Cider, 20, 20, 0x1F97, 0));
                Add(new BeverageBuyInfo(typeof(Pitcher), BeverageType.Liquor, 20, 20, 0x1F99, 0));
                Add(new BeverageBuyInfo(typeof(Pitcher), BeverageType.Wine, 20, 20, 0x1F9B, 0));
                Add(new BeverageBuyInfo(typeof(Pitcher), BeverageType.Water, 20, 20, 0x1F9D, 0));

                Add(new GenericBuyInfo(typeof(BreadLoaf), 50, 10, 0x103B, 0));
                Add(new GenericBuyInfo(typeof(CheeseWheel), 25, 10, 0x97E, 0));
                Add(new GenericBuyInfo(typeof(CookedBird), 100, 20, 0x9B7, 0));
                Add(new GenericBuyInfo(typeof(LambLeg), 100, 20, 0x160A, 0));
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
            }
        }
    }
}