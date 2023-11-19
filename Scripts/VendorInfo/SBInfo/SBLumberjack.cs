using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBLumberjack : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBLumberjack()
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
                Add(new GenericBuyInfo(typeof(Log), 10, 20, 0x1BDD, 0));

            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                Add(typeof(Log), 7);
				/*
                Add(typeof(WoodenBox), 7);
                Add(typeof(SmallCrate), 5);
                Add(typeof(MediumCrate), 6);
                Add(typeof(LargeCrate), 7);
                Add(typeof(WoodenChest), 15);
              
                Add(typeof(LargeTable), 10);
                Add(typeof(Nightstand), 7);
                Add(typeof(YewWoodTable), 10);

                Add(typeof(Throne), 24);
                Add(typeof(WoodenThrone), 6);
                Add(typeof(Stool), 6);
                Add(typeof(FootStool), 6);

                Add(typeof(FancyWoodenChairCushion), 12);
                Add(typeof(WoodenChairCushion), 10);
                Add(typeof(WoodenChair), 8);
                Add(typeof(BambooChair), 6);
                Add(typeof(WoodenBench), 6);

                Add(typeof(Saw), 9);
                Add(typeof(Scorp), 6);
                Add(typeof(SmoothingPlane), 6);
                Add(typeof(DrawKnife), 6);
                Add(typeof(Froe), 6);
                Add(typeof(Hammer), 14);
                Add(typeof(Inshave), 6);
                Add(typeof(JointingPlane), 6);
                Add(typeof(MouldingPlane), 6);
                Add(typeof(DovetailSaw), 7);
                Add(typeof(Board), 1);
                Add(typeof(Axle), 1);

                Add(typeof(Club), 13);

                Add(typeof(Lute), 10);
                Add(typeof(LapHarp), 10);
                Add(typeof(Tambourine), 10);
                Add(typeof(Drums), 10);
				*/
            }
        }
    }
}