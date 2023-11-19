using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBRanger : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBRanger()
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
                Add(new GenericBuyInfo(typeof(Hides), 10, 40, 0x1079, 0)); 				
                Add(new GenericBuyInfo(typeof(Bandage), 11, 20, 0xE21, 0));
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                Add(typeof(Bolt), 5 ); // 5, Utility.Random(30, 60), 5 ); // 0x1BFB, 0, true));
                Add(typeof(Arrow), 5 ); // 5, Utility.Random(30, 60), 5 ); // 0xF3F, 0, true));
                Add(typeof(Feather), 5 ); // 1, Utility.Random(30, 60), 5 ); // 0x1BD1, 0, true));
                Add(typeof(Shaft), 5 ); // 4, Utility.Random(30, 60), 5 ); // 0x1BD4, 0, true));
                Add(typeof(Hides), 5 ); // 6, 40, 0x1079, 0, true)); 				
                Add(typeof(Bandage), 5 ); // 11, 20, 0xE21, 0, true));
            }
        }
    }
}