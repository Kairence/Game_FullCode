using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles 
{ 
    public class SBVarietyDealer : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBVarietyDealer()
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
                Add(new GenericBuyInfo(typeof(BlackPearl), 5, 40, 0xF7A, 0)); 
                Add(new GenericBuyInfo(typeof(Bloodmoss), 5, 40, 0xF7B, 0)); 
                Add(new GenericBuyInfo(typeof(MandrakeRoot), 5, 40, 0xF86, 0)); 
                Add(new GenericBuyInfo(typeof(Garlic), 5, 40, 0xF84, 0)); 
                Add(new GenericBuyInfo(typeof(Ginseng), 5, 40, 0xF85, 0)); 
                Add(new GenericBuyInfo(typeof(Nightshade), 5, 40, 0xF88, 0)); 
                Add(new GenericBuyInfo(typeof(SpidersSilk), 5, 40, 0xF8D, 0)); 
                Add(new GenericBuyInfo(typeof(SulfurousAsh), 5, 40, 0xF8C, 0));

                Add(new GenericBuyInfo(typeof(BatWing), 5, 40, 0xF78, 0)); 
                Add(new GenericBuyInfo(typeof(GraveDust), 5, 40, 0xF8F, 0)); 
                Add(new GenericBuyInfo(typeof(DaemonBlood), 5, 40, 0xF7D, 0)); 
                Add(new GenericBuyInfo(typeof(NoxCrystal), 5, 40, 0xF7A, 0)); 
                Add(new GenericBuyInfo(typeof(PigIron), 5, 40, 0xF8E, 0)); 
                Add(new GenericBuyInfo(typeof(Bone), 5, 40, 0xF7E, 0)); 
                Add(new GenericBuyInfo(typeof(DragonBlood), 5, 40, 0x4077, 0)); 
                Add(new GenericBuyInfo(typeof(FertileDirt), 5, 40, 0xF81, 0)); 
                Add(new GenericBuyInfo(typeof(DaemonBone), 5, 40, 0xF80, 0)); 
                //Add(new GenericBuyInfo(typeof(BreadLoaf), 7, 10, 0x103B, 0, true));
                //Add(new GenericBuyInfo(typeof(Backpack), 15, 20, 0x9B2, 0));
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
 				Add(typeof(BlackPearl), 5 );
 				Add(typeof(Bloodmoss), 5 );
 				Add(typeof(MandrakeRoot), 5 );
 				Add(typeof(Garlic), 5 );
 				Add(typeof(Ginseng), 5 );
 				Add(typeof(Nightshade), 5 );
 				Add(typeof(SpidersSilk), 5 );
 				Add(typeof(SulfurousAsh), 5 );
				
				Add(typeof(BatWing), 5 );
				Add(typeof(GraveDust), 5 );
				Add(typeof(DaemonBlood), 5 );
				Add(typeof(NoxCrystal), 5 );
				Add(typeof(PigIron), 5 );
				Add(typeof(Bone), 5 );
				Add(typeof(DragonBlood), 5 );
				Add(typeof(FertileDirt), 5 );
				Add(typeof(DaemonBone), 5 );
            }
        }
    }
}