using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBTailor : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBTailor()
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
                Add(new GenericBuyInfo(typeof(SewingKit), 100, 100, 0xF9D, 0)); 
                Add(new GenericBuyInfo(typeof(Scissors), 100, 100, 0xF9F, 0));
                Add(new GenericBuyInfo(typeof(DyeTub), 100, 100, 0xFAB, 0)); 
                Add(new GenericBuyInfo(typeof(Dyes), 100, 100, 0xFA9, 0)); 

                Add(new GenericBuyInfo(typeof(Shirt), 100, 100, 0x1517, 0));
                Add(new GenericBuyInfo(typeof(ShortPants), 100, 100, 0x152E, 0));
                Add(new GenericBuyInfo(typeof(FancyShirt), 100, 100, 0x1EFD, 0));
                Add(new GenericBuyInfo(typeof(LongPants), 100, 100, 0x1539, 0));
                Add(new GenericBuyInfo(typeof(FancyDress), 100, 100, 0x1EFF, 0));
                Add(new GenericBuyInfo(typeof(PlainDress), 100, 100, 0x1F01, 0));
                Add(new GenericBuyInfo(typeof(Kilt), 100, 100, 0x1537, 0));
                Add(new GenericBuyInfo(typeof(Kilt), 100, 100, 0x1537, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(HalfApron), 100, 100, 0x153b, 0));
                Add(new GenericBuyInfo(typeof(Robe), 100, 100, 0x1F03, 0));
                Add(new GenericBuyInfo(typeof(Cloak), 100, 100, 0x1515, 0));
                Add(new GenericBuyInfo(typeof(Cloak), 100, 100, 0x1515, 0));
                Add(new GenericBuyInfo(typeof(Doublet), 100, 100, 0x1F7B, 0));
                Add(new GenericBuyInfo(typeof(Tunic), 100, 100, 0x1FA1, 0));
                Add(new GenericBuyInfo(typeof(JesterSuit), 100, 100, 0x1F9F, 0));

                Add(new GenericBuyInfo(typeof(JesterHat), 100, 100, 0x171C, 0));
                Add(new GenericBuyInfo(typeof(FloppyHat), 100, 100, 0x1713, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(WideBrimHat), 100, 100, 0x1714, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(Cap), 100, 100, 0x1715, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(TallStrawHat), 100, 100, 0x1716, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(StrawHat), 100, 100, 0x1717, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(WizardsHat), 100, 100, 0x1718, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(LeatherCap), 100, 100, 0x1DB9, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(FeatheredHat), 100, 100, 0x171A, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(TricorneHat), 100, 100, 0x171B, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(Bandana), 100, 100, 0x1540, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(SkullCap), 100, 100, 0x1544, Utility.RandomDyedHue()));

                Add(new GenericBuyInfo(typeof(Bone), 10, 100, 0xf7e, 0));
                Add(new GenericBuyInfo(typeof(BarkFragment), 10, 100, 0x318F, 0));
                Add(new GenericBuyInfo(typeof(TigerPelt), 10, 100, 0x9BCC, 0));
                Add(new GenericBuyInfo(typeof(DragonTurtleScute), 10, 100, 0x9BCE, 0));

                Add(new GenericBuyInfo(typeof(BoltOfCloth), 100, 20, 0xf95, Utility.RandomDyedHue(), true));

                Add(new GenericBuyInfo(typeof(Cloth), 10, 20, 0x1766, Utility.RandomDyedHue(), true));
                Add(new GenericBuyInfo(typeof(UncutCloth), 10, 20, 0x1767, Utility.RandomDyedHue(), true));

                Add(new GenericBuyInfo(typeof(Cotton), 102, 20, 0xDF9, 0, true));
                Add(new GenericBuyInfo(typeof(Wool), 62, 20, 0xDF8, 0, true));
                Add(new GenericBuyInfo(typeof(Flax), 102, 20, 0x1A9C, 0, true));
                Add(new GenericBuyInfo(typeof(SpoolOfThread), 18, 20, 0xFA0, 0, true));
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                Add(typeof(SewingKit), 100); 
                Add(typeof(Scissors), 100);
                Add(typeof(DyeTub), 100); 
                Add(typeof(Dyes), 100); 

                Add(typeof(Shirt), 100);
                Add(typeof(ShortPants), 100);
                Add(typeof(FancyShirt), 100);
                Add(typeof(LongPants), 100);
                Add(typeof(FancyDress), 100);
                Add(typeof(PlainDress), 100);
                Add(typeof(Kilt), 100);
                Add(typeof(Kilt), 100);
                Add(typeof(HalfApron), 100);
                Add(typeof(Robe), 100);
                Add(typeof(Cloak), 100);
                Add(typeof(Cloak), 100);
                Add(typeof(Doublet), 100);
                Add(typeof(Tunic), 100);
                Add(typeof(JesterSuit), 100);

                Add(typeof(JesterHat), 100);
                Add(typeof(FloppyHat), 100);
                Add(typeof(WideBrimHat), 100);
                Add(typeof(Cap), 100);
                Add(typeof(TallStrawHat), 100);
                Add(typeof(StrawHat), 100);
                Add(typeof(WizardsHat), 100);
                Add(typeof(LeatherCap), 100);
                Add(typeof(FeatheredHat), 100);
                Add(typeof(TricorneHat), 100);
                Add(typeof(Bandana), 100);
                Add(typeof(SkullCap), 100);

                Add(typeof(BoltOfCloth), 100);

                Add(typeof(Bone), 10);
                Add(typeof(BarkFragment), 10);
                Add(typeof(TigerPelt), 10);
                Add(typeof(DragonTurtleScute), 10);
                Add(typeof(Cloth), 10);
                Add(typeof(UncutCloth), 10);

                Add(typeof(Cotton), 102);
                Add(typeof(Wool), 62);
                Add(typeof(Flax), 102);
                Add(typeof(SpoolOfThread), 18);
            }
        }
    }
}