using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles 
{ 
    public class SBWeaver : SBInfo 
    { 
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBWeaver() 
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
                Add(new GenericBuyInfo(typeof(UncutCloth), 10, 20, 0x1764, 0));

                Add(new GenericBuyInfo(typeof(BoltOfCloth), 500, 20, 0xf97, 0));

                Add(new GenericBuyInfo(typeof(DarkYarn), 100, 20, 0xE1D, 0));
                Add(new GenericBuyInfo(typeof(LightYarn), 100, 20, 0xE1E, 0));
                Add(new GenericBuyInfo(typeof(LightYarnUnraveled), 100, 20, 0xE1F, 0));


            }
        }

        public class InternalSellInfo : GenericSellInfo 
        { 
            public InternalSellInfo() 
            { 
                Add(typeof(UncutCloth), 5 ); // 5, 20, 0x1764, 0, true));

                Add(typeof(BoltOfCloth), 5 ); // 150, 20, 0xf97, 0, true));

                Add(typeof(DarkYarn), 5 ); // 30, 20, 0xE1D, 0, true));
                Add(typeof(LightYarn), 5 ); // 30, 20, 0xE1E, 0, true));
                Add(typeof(LightYarnUnraveled), 5 ); // 30, 20, 0xE1F, 0, true));

            }
        }
    }
}