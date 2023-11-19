using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBMage : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBMage()
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
                Add(new GenericBuyInfo(typeof(Spellbook), 100, 10, 0xEFA, 0));
				
                Add(new GenericBuyInfo(typeof(Magerybook), 115, 10, 0x225A, 0));
                Add(new GenericBuyInfo(typeof(NecromancerSpellbook), 115, 10, 0x2253, 0));
                Add(new GenericBuyInfo(typeof(SpellweavingBook), 115, 10, 0x2D50, 0));
                Add(new GenericBuyInfo(typeof(MysticBook), 115, 10, 0x2D9D, 0));
				
                Add(new GenericBuyInfo(typeof(ScribesPen), 100, 10, 0xFBF, 0));

                Add(new GenericBuyInfo("1041072", typeof(MagicWizardsHat), 50, 10, 0x1718, Utility.RandomDyedHue()));

                Add(new GenericBuyInfo(typeof(RecallRune), 15, 10, 0x1F14, 0));

                Add(new GenericBuyInfo(typeof(BlackPearl), 6, 20, 0xF7A, 0));
                Add(new GenericBuyInfo(typeof(Bloodmoss), 6, 20, 0xF7B, 0));
                Add(new GenericBuyInfo(typeof(Garlic), 6, 20, 0xF84, 0));
                Add(new GenericBuyInfo(typeof(Ginseng), 6, 20, 0xF85, 0));
                Add(new GenericBuyInfo(typeof(MandrakeRoot), 6, 20, 0xF86, 0));
                Add(new GenericBuyInfo(typeof(Nightshade), 6, 20, 0xF88, 0));
                Add(new GenericBuyInfo(typeof(SpidersSilk), 6, 20, 0xF8D, 0));
                Add(new GenericBuyInfo(typeof(SulfurousAsh), 6, 20, 0xF8C, 0));

            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
				Add(typeof(Spellbook), 5 );
				Add(typeof(Magerybook), 5 );
				Add(typeof(NecromancerSpellbook), 5 );
				Add(typeof(SpellweavingBook), 5 );
				Add(typeof(MysticBook), 5 );
				Add(typeof(ScribesPen), 5 );
				Add(typeof(MagicWizardsHat), 5 );
				Add(typeof(RecallRune), 5 );
				Add(typeof(BlackPearl), 5 );
				Add(typeof(Bloodmoss), 5 );
				Add(typeof(MandrakeRoot), 5 );
				Add(typeof(Garlic), 5 );
				Add(typeof(Nightshade), 5 );
				Add(typeof(SpidersSilk), 5 );
				Add(typeof(SulfurousAsh), 5 );
				Add(typeof(Ginseng), 5 );
				

				
            }
        }
    }
}