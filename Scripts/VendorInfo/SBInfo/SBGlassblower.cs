using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBGlassblower : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBGlassblower()
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
                Add(new GenericBuyInfo(typeof(Bottle), 5, 100, 0xF0E, 0)); 

                Add(new GenericBuyInfo(typeof(HeatingStand), 20, 100, 0x1849, 0)); 

                Add(new GenericBuyInfo("Crafting Glass With Glassblowing", typeof(GlassblowingBook), 10637, 30, 0xFF4, 0));
                Add(new GenericBuyInfo("Finding Glass-Quality Sand", typeof(SandMiningBook), 10637, 30, 0xFF4, 0));
                Add(new GenericBuyInfo("1044608", typeof(Blowpipe), 100, 100, 0xE8A, 0x3B9));
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                Add(typeof(BlackPearl), 3); 
                Add(typeof(Bloodmoss), 3); 
                Add(typeof(MandrakeRoot), 2); 
                Add(typeof(Garlic), 2); 
                Add(typeof(Ginseng), 2); 
                Add(typeof(Nightshade), 2); 
                Add(typeof(SpidersSilk), 2); 
                Add(typeof(SulfurousAsh), 2); 
                Add(typeof(Bottle), 3);
                Add(typeof(MortarPestle), 4);

                Add(typeof(NightSightPotion), 7);
                Add(typeof(AgilityPotion), 7);
                Add(typeof(StrengthPotion), 7);
                Add(typeof(RefreshPotion), 7);
                Add(typeof(LesserCurePotion), 7);
                Add(typeof(LesserHealPotion), 7);
                Add(typeof(LesserPoisonPotion), 7);
                Add(typeof(LesserExplosionPotion), 10);

                Add(typeof(GlassblowingBook), 5000);
                Add(typeof(SandMiningBook), 5000);
                Add(typeof(Blowpipe), 10);
            }
        }
    }
}