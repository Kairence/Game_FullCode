using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBHealer : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBHealer()
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
                Add(new GenericBuyInfo(typeof(MiniHealPotion), 20, 20, 0xF0C, 0));
				
                Add(new GenericBuyInfo(typeof(LesserHealPotion), 55, 20, 0xF0C, 0));
                Add(new GenericBuyInfo(typeof(HealPotion), 110, 20, 0xF0C, 0));
                Add(new GenericBuyInfo(typeof(GreaterHealPotion), 200, 20, 0xF0C, 0));
                Add(new GenericBuyInfo(typeof(TotalHealPotion), 380, 20, 0xF0C, 0));
				
                Add(new GenericBuyInfo(typeof(Bandage), 10, 20, 0xE21, 0));
                Add(new GenericBuyInfo(typeof(Bloodmoss), 5, 20, 0xF7B, 0));
               // Add(new GenericBuyInfo(typeof(RefreshPotion), 15, 20, 0xF0B, 0, true));
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                Add(typeof(Bandage), 1);
                Add(typeof(MiniHealPotion), 7);
                Add(typeof(LesserHealPotion), 7);
                Add(typeof(HealPotion), 7);
                Add(typeof(GreaterHealPotion), 2);
                Add(typeof(TotalHealPotion), 7);
                Add(typeof(Bloodmoss), 2);
            }
        }
    }
}