using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBMaceWeapon : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBMaceWeapon()
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
                Add(new GenericBuyInfo(typeof(HammerPick), 100, 20, 0x143D, 0));
                Add(new GenericBuyInfo(typeof(Club), 100, 20, 0x13B4, 0));
                Add(new GenericBuyInfo(typeof(Mace), 100, 20, 0xF5C, 0));
                Add(new GenericBuyInfo(typeof(Maul), 100, 20, 0x143B, 0));
                Add(new GenericBuyInfo(typeof(WarHammer), 100, 20, 0x1439, 0));
                Add(new GenericBuyInfo(typeof(WarMace), 100, 20, 0x1407, 0));
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