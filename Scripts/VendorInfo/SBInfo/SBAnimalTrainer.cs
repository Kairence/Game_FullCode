using System;
using System.Collections.Generic;

namespace Server.Mobiles
{
    public class SBAnimalTrainer : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBAnimalTrainer()
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
                Add(new AnimalBuyInfo(1, typeof(Horse), 100, 10, 204, 0));
                Add(new AnimalBuyInfo(1, typeof(PackLlama), 500, 10, 292, 0));
                Add(new AnimalBuyInfo(1, typeof(PackHorse), 1000, 10, 291, 0));
				/*
                Add(new AnimalBuyInfo(1, typeof(Cat), 132, 10, 201, 0));
                Add(new AnimalBuyInfo(1, typeof(Dog), 170, 10, 217, 0));
                Add(new AnimalBuyInfo(1, typeof(Horse), 550, 10, 204, 0));
                Add(new AnimalBuyInfo(1, typeof(PackHorse), 631, 10, 291, 0));
                Add(new AnimalBuyInfo(1, typeof(PackLlama), 565, 10, 292, 0));
                Add(new AnimalBuyInfo(1, typeof(Rabbit), 106, 10, 205, 0));

                if (!Core.AOS)
                {
                    Add(new AnimalBuyInfo(1, typeof(Eagle), 402, 10, 5, 0));
                    Add(new AnimalBuyInfo(1, typeof(BrownBear), 855, 10, 167, 0));
                    Add(new AnimalBuyInfo(1, typeof(GrizzlyBear), 1767, 10, 212, 0));
                    Add(new AnimalBuyInfo(1, typeof(Panther), 1271, 10, 214, 0));
                    Add(new AnimalBuyInfo(1, typeof(TimberWolf), 768, 10, 225, 0));
                    Add(new AnimalBuyInfo(1, typeof(Rat), 107, 10, 238, 0));
                }
				*/
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