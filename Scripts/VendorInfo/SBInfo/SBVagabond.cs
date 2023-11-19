using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles 
{ 
    public class SBVagabond : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBVagabond()
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
                Add(new GenericBuyInfo(typeof(GoldRing), 100, 20, 0x108A, 0));
                Add(new GenericBuyInfo(typeof(GoldNecklace), 100, 20, 0x1088, 0));
                Add(new GenericBuyInfo(typeof(GoldBracelet), 100, 20, 0x1086, 0));
                Add(new GenericBuyInfo(typeof(GoldEarrings), 100, 20, 0x1087, 0));
                Add(new GenericBuyInfo(typeof(SilverRing), 100, 20, 0x1F09, 0));
                Add(new GenericBuyInfo(typeof(SilverNecklace), 100, 20, 0x1F08, 0));
                Add(new GenericBuyInfo(typeof(SilverBracelet), 100, 20, 0x1F06, 0));
                Add(new GenericBuyInfo(typeof(SilverEarrings), 100, 20, 0x1F07, 0));
                Add(new GenericBuyInfo(typeof(Beads), 20, 20, 0x108B, 0));
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                Add(typeof(Beads), 13);
                Add(typeof(GoldRing), 13);
                Add(typeof(SilverRing), 10);
                Add(typeof(GoldNecklace), 13);
                Add(typeof(SilverNecklace), 10);
                Add(typeof(GoldBracelet), 13);
                Add(typeof(SilverBracelet), 10);
                Add(typeof(GoldEarrings), 13);
                Add(typeof(SilverEarrings), 10);

            }
        }
    }
}