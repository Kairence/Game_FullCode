using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBThief : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBThief()
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

                //Add( new GenericBuyInfo( typeof( OilFlask ), 8, 20, 0x####, 0 ) );
                Add(new GenericBuyInfo(typeof(Lockpick), 100, 20, 0x14FC, 0));

            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                Add(typeof(Lockpick), 1);
			}
        }
    }
}