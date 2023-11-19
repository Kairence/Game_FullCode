using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBLeatherArmor : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBLeatherArmor()
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
                Add(new GenericBuyInfo(typeof(LeatherArms), 100, 20, 0x13CD, 0));
                Add(new GenericBuyInfo(typeof(LeatherChest), 101, 20, 0x13CC, 0));
                Add(new GenericBuyInfo(typeof(LeatherGloves), 100, 20, 0x13C6, 0));
                Add(new GenericBuyInfo(typeof(LeatherGorget), 100, 20, 0x13C7, 0));
                Add(new GenericBuyInfo(typeof(LeatherLegs), 100, 20, 0x13cb, 0));
                Add(new GenericBuyInfo(typeof(LeatherCap), 100, 20, 0x1DB9, 0));
                Add(new GenericBuyInfo(typeof(FemaleLeatherChest), 116, 20, 0x1C06, 0));
                Add(new GenericBuyInfo(typeof(LeatherBustierArms), 100, 20, 0x1C0A, 0));
                Add(new GenericBuyInfo(typeof(LeatherShorts), 100, 20, 0x1C00, 0));
                Add(new GenericBuyInfo(typeof(LeatherSkirt), 100, 20, 0x1C08, 0));
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