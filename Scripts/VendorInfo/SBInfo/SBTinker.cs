using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles 
{ 
    public class SBTinker : SBInfo 
    { 
        private readonly List<GenericBuyInfo> m_BuyInfo;
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();

        public SBTinker(BaseVendor owner) 
        {
            m_BuyInfo = new InternalBuyInfo(owner);
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
            public InternalBuyInfo(BaseVendor owner) 
            { 
                Add(new GenericBuyInfo(typeof(SkinningKnife), 100, 20, 0xEC4, 0));
                Add(new GenericBuyInfo(typeof(Clock), 100, 20, 0x104B, 0));
                Add(new GenericBuyInfo(typeof(Nails), 100, 20, 0x102E, 0));
                Add(new GenericBuyInfo(typeof(ClockParts), 100, 20, 0x104F, 0));
                Add(new GenericBuyInfo(typeof(AxleGears), 100, 20, 0x1051, 0));
                Add(new GenericBuyInfo(typeof(Gears), 100, 20, 0x1053, 0));
                Add(new GenericBuyInfo(typeof(Hinge), 100, 20, 0x1055, 0));

                Add(new GenericBuyInfo(typeof(Sextant), 100, 20, 0x1057, 0));
                Add(new GenericBuyInfo(typeof(SextantParts), 100, 20, 0x1059, 0));
                Add(new GenericBuyInfo(typeof(Axle), 100, 20, 0x105B, 0));
                Add(new GenericBuyInfo(typeof(Springs), 100, 20, 0x105D, 0));

                Add(new GenericBuyInfo("1024111", typeof(Key), 15, 20, 0x100F, 0));
                Add(new GenericBuyInfo("1024112", typeof(Key), 15, 20, 0x1010, 0));
                Add(new GenericBuyInfo("1024115", typeof(Key), 15, 20, 0x1013, 0));
                Add(new GenericBuyInfo(typeof(KeyRing), 20, 20, 0x1010, 0));

                Add(new GenericBuyInfo(typeof(TinkersTools), 100, 20, 0x1EBC, 0));
                Add(new GenericBuyInfo(typeof(SewingKit), 100, 20, 0xF9D, 0));

                Add(new GenericBuyInfo(typeof(DrawKnife), 100, 20, 0x10E4, 0));
                Add(new GenericBuyInfo(typeof(Froe), 100, 20, 0x10E5, 0));
                Add(new GenericBuyInfo(typeof(Scorp), 100, 20, 0x10E7, 0));
                Add(new GenericBuyInfo(typeof(Inshave), 100, 20, 0x10E6, 0));

                Add(new GenericBuyInfo(typeof(ButcherKnife), 100, 20, 0x13F6, 0));

                Add(new GenericBuyInfo(typeof(Scissors), 100, 20, 0xF9F, 0));

                Add(new GenericBuyInfo(typeof(Tongs), 100, 14, 0xFBB, 0));

                Add(new GenericBuyInfo(typeof(DovetailSaw), 100, 20, 0x1028, 0));
                Add(new GenericBuyInfo(typeof(Saw), 100, 20, 0x1034, 0));

                Add(new GenericBuyInfo(typeof(Hammer), 100, 20, 0x102A, 0));
                Add(new GenericBuyInfo(typeof(SmithHammer), 100, 20, 0x13E3, 0));
                // TODO: Sledgehammer

                Add(new GenericBuyInfo(typeof(Shovel), 100, 20, 0xF39, 0));

                Add(new GenericBuyInfo(typeof(MouldingPlane), 100, 20, 0x102C, 0));
                Add(new GenericBuyInfo(typeof(JointingPlane), 100, 20, 0x1030, 0));
                Add(new GenericBuyInfo(typeof(SmoothingPlane), 100, 20, 0x1032, 0));

                Add(new GenericBuyInfo(typeof(RollingPin), 100, 20, 0x1043, 0));
                Add(new GenericBuyInfo(typeof(FlourSifter), 100, 20, 0x103E, 0));
                Add(new GenericBuyInfo("1044567", typeof(Skillet), 100, 20, 0x97F, 0));				
				Add(new GenericBuyInfo(typeof(MortarPestle), 100, 10, 0xE9B, 0));
				Add(new GenericBuyInfo(typeof(FishingPole), 100, 20, 0xDC0, 0));
				Add(new GenericBuyInfo(typeof(Hatchet), 100, 20, 0xF43, 0));
				/*
                if (owner != null && owner.Race == Race.Gargoyle)
                {
                    Add(new GenericBuyInfo(typeof(AudChar), 33, 20, 0x403B, 0));
                    Add(new GenericBuyInfo("1080201", typeof(StatuetteEngravingTool), 1253, 20, 0x12B3, 0));
                    Add(new GenericBuyInfo(typeof(BasketWeavingBook), 10625, 20, 0xFBE, 0));
                }
				*/
            }
        }

        public class InternalSellInfo : GenericSellInfo 
        { 
            public InternalSellInfo() 
            { 
                Add(typeof(SkinningKnife), 10);
                Add(typeof(Clock), 10);
                Add(typeof(Nails), 10);
                Add(typeof(Clock), 10);
                Add(typeof(AxleGears), 10);
                Add(typeof(Gears), 10);
                Add(typeof(Hinge), 10);
                Add(typeof(Sextant), 10);
                Add(typeof(SextantParts), 10);
                Add(typeof(Axle), 10);
                Add(typeof(Springs), 10);
				
                Add(typeof(KeyRing), 10);
                Add(typeof(TinkersTools), 10);
                Add(typeof(SewingKit), 10);
                Add(typeof(DrawKnife), 10);
                Add(typeof(Inshave), 10);
				
                Add(typeof(ButcherKnife), 10);
                Add(typeof(Scissors), 10);
                Add(typeof(Tongs), 10);
                Add(typeof(DovetailSaw), 10);
                Add(typeof(Saw), 10);
				
                Add(typeof(Hammer), 10);
                Add(typeof(SmithHammer), 10);
                Add(typeof(Shovel), 10);
                Add(typeof(MouldingPlane), 10);
                Add(typeof(JointingPlane), 10);
                Add(typeof(SmoothingPlane), 10);
                Add(typeof(RollingPin), 10);
                Add(typeof(FlourSifter), 10);
                Add(typeof(Skillet), 10);
                Add(typeof(MortarPestle), 10);
                Add(typeof(FishingPole), 10);
                Add(typeof(Hatchet), 10);
            }
        }
    }
}