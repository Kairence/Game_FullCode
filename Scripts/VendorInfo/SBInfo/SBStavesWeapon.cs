using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBStavesWeapon : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBStavesWeapon()
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
                Add(new GenericBuyInfo(typeof(BlackStaff), 100, 20, 0xDF1, 0));
                Add(new GenericBuyInfo(typeof(GnarledStaff), 100, 20, 0x13F8, 0));
                Add(new GenericBuyInfo(typeof(QuarterStaff), 100, 20, 0xE89, 0));
                Add(new GenericBuyInfo(typeof(ShepherdsCrook), 100, 20, 0xE81, 0));
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