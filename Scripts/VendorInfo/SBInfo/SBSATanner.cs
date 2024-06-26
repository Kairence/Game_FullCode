using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBSATanner : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBSATanner()
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
                Add(new GenericBuyInfo(typeof(Bag), 20, 20, 0xE76, 0));
                Add(new GenericBuyInfo(typeof(Pouch), 20, 20, 0xE79, 0));
                Add(new GenericBuyInfo(typeof(Backpack), 15, 20, 0x9B2, 0));
                Add(new GenericBuyInfo(typeof(Leather), 10, 20, 0x1081, 0));
                Add(new GenericBuyInfo(typeof(GargishDagger), 100, 20, 0x902, 0));
                Add(new GenericBuyInfo(typeof(TaxidermyKit), 100000, 20, 0x1EBA, 0));

                Add(new GenericBuyInfo(typeof(FemaleGargishLeatherArms), 100, 20, 0x301, 0));
                Add(new GenericBuyInfo(typeof(GargishLeatherArms), 100, 20, 0x302, 0));
                Add(new GenericBuyInfo(typeof(FemaleGargishLeatherChest), 100, 20, 0x303, 0));
                Add(new GenericBuyInfo(typeof(GargishLeatherChest), 100, 20, 0x304, 0));
                Add(new GenericBuyInfo(typeof(FemaleGargishLeatherKilt), 100, 20, 0x310, 0));
                Add(new GenericBuyInfo(typeof(GargishLeatherKilt), 100, 20, 0x311, 0));
                Add(new GenericBuyInfo(typeof(GargishLeatherLegs), 100, 20, 0x305, 0));
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                Add(typeof(Bag), 3);
                Add(typeof(Pouch), 3);
                Add(typeof(Backpack), 7);
                Add(typeof(Leather), 5);
                Add(typeof(GargishDagger), 10);

                Add(typeof(FemaleGargishLeatherArms), 42);
                Add(typeof(GargishLeatherArms), 41);
                Add(typeof(FemaleGargishLeatherChest), 44);
                Add(typeof(GargishLeatherChest), 38);
                Add(typeof(FemaleGargishLeatherKilt), 46);
                Add(typeof(GargishLeatherKilt), 48);
                Add(typeof(GargishLeatherLegs), 34);
            }
        }
    }
}