using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles 
{ 
    public class SBHerbalist : SBInfo 
    { 
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBHerbalist() 
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
                Add(new GenericBuyInfo(typeof(Ginseng), 5, 20, 0xF85, 0)); 
                Add(new GenericBuyInfo(typeof(Garlic), 5, 20, 0xF84, 0)); 
                Add(new GenericBuyInfo(typeof(MandrakeRoot), 5, 20, 0xF86, 0)); 
                Add(new GenericBuyInfo(typeof(Nightshade), 5, 20, 0xF88, 0)); 
                //Add(new GenericBuyInfo(typeof(MortarPestle), 8, 20, 0xE9B, 0));
                //Add(new GenericBuyInfo(typeof(Bottle), 5, 20, 0xF0E, 0, true)); 
            }
        }

        public class InternalSellInfo : GenericSellInfo 
        { 
            public InternalSellInfo() 
            { 
 				Add(typeof(MandrakeRoot), 5 );
 				Add(typeof(Garlic), 5 );
 				Add(typeof(Ginseng), 5 );
 				Add(typeof(Nightshade), 5 );
            }
        }
    }
}