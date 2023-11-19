using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBSAArmor : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();

        public SBSAArmor()
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
                Add(new GenericBuyInfo(typeof(FemaleGargishPlateArms), 363, 20, 0x307, 0));
                Add(new GenericBuyInfo(typeof(GargishPlateArms), 328, 20, 0x308, 0));
                Add(new GenericBuyInfo(typeof(FemaleGargishPlateChest), 481, 20, 0x309, 0));
                Add(new GenericBuyInfo(typeof(GargishPlateChest), 462, 20, 0x30A, 0));
                Add(new GenericBuyInfo(typeof(FemaleGargishPlateKilt), 338, 20, 0x30B, 0));
                Add(new GenericBuyInfo(typeof(GargishPlateKilt), 370, 20, 0x30C, 0));
                Add(new GenericBuyInfo(typeof(FemaleGargishPlateLegs), 372, 20, 0x30D, 0));
                Add(new GenericBuyInfo(typeof(GargishPlateLegs), 355, 20, 0x30E, 0));

                Add(new GenericBuyInfo(typeof(FemaleGargishStoneArms), 116, 20, 0x283, 0));
                Add(new GenericBuyInfo(typeof(GargishStoneArms), 121, 20, 0x284, 0));
                Add(new GenericBuyInfo(typeof(FemaleGargishStoneChest), 135, 20, 0x285, 0));
                Add(new GenericBuyInfo(typeof(GargishStoneChest), 142, 20, 0x286, 0));
                Add(new GenericBuyInfo(typeof(FemaleGargishStoneKilt), 135, 20, 0x287, 0));
                Add(new GenericBuyInfo(typeof(GargishStoneKilt), 132, 20, 0x288, 0));
                Add(new GenericBuyInfo(typeof(FemaleGargishStoneLegs), 116, 20, 0x289, 0));
                Add(new GenericBuyInfo(typeof(GargishStoneLegs), 113, 20, 0x28A, 0));
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