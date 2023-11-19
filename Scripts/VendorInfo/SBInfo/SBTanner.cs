using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBTanner : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBTanner()
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
                Add(new GenericBuyInfo(typeof(LeatherGorget), 100, 100, 0x13C7, 0));
                Add(new GenericBuyInfo(typeof(LeatherCap), 100, 100, 0x1DB9, 0));
                Add(new GenericBuyInfo(typeof(LeatherArms), 100, 100, 0x13CD, 0));
                Add(new GenericBuyInfo(typeof(LeatherChest), 100, 100, 0x13CC, 0));
                Add(new GenericBuyInfo(typeof(LeatherLegs), 100, 100, 0x13CB, 0));
                Add(new GenericBuyInfo(typeof(LeatherGloves), 100, 100, 0x13C6, 0));

                Add(new GenericBuyInfo(typeof(StuddedGorget), 100, 100, 0x13D6, 0));
                Add(new GenericBuyInfo(typeof(StuddedArms), 100, 100, 0x13DC, 0));
                Add(new GenericBuyInfo(typeof(StuddedChest), 100, 100, 0x13DB, 0));
                Add(new GenericBuyInfo(typeof(StuddedLegs), 100, 100, 0x13DA, 0));
                Add(new GenericBuyInfo(typeof(StuddedGloves), 100, 100, 0x13D5, 0));

                Add(new GenericBuyInfo(typeof(FemaleStuddedChest), 100, 100, 0x1C02, 0));
                Add(new GenericBuyInfo(typeof(FemalePlateChest), 207, 100, 0x1C04, 0));
                Add(new GenericBuyInfo(typeof(FemaleLeatherChest), 100, 100, 0x1C06, 0));
                Add(new GenericBuyInfo(typeof(LeatherShorts), 100, 100, 0x1C00, 0));
                Add(new GenericBuyInfo(typeof(LeatherSkirt), 100, 100, 0x1C08, 0));
                Add(new GenericBuyInfo(typeof(LeatherBustierArms), 100, 100, 0x1C0A, 0));
                Add(new GenericBuyInfo(typeof(LeatherBustierArms), 100, 100, 0x1C0B, 0));
                Add(new GenericBuyInfo(typeof(StuddedBustierArms), 100, 100, 0x1C0C, 0));
                Add(new GenericBuyInfo(typeof(StuddedBustierArms), 100, 100, 0x1C0D, 0));

                Add(new GenericBuyInfo(typeof(Bone), 10, 100, 0xf7e, 0));
                Add(new GenericBuyInfo(typeof(TigerPelt), 10, 100, 0x9BCC, 0));
                Add(new GenericBuyInfo(typeof(DragonTurtleScute), 10, 100, 0x9BCE, 0));
				Add(new GenericBuyInfo(typeof(Bag), 30, 100, 0xE76, 0));
                Add(new GenericBuyInfo(typeof(Pouch), 30, 100, 0xE79, 0));
                Add(new GenericBuyInfo(typeof(Backpack), 30, 100, 0x9B2, 0));

                Add(new GenericBuyInfo(typeof(SkinningKnife), 100, 100, 0xEC4, 0));

                Add(new GenericBuyInfo("1041279", typeof(TaxidermyKit), 100000, 20, 0x1EBA, 0));
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                Add(typeof(LeatherGorget), 100);
                Add(typeof(LeatherCap), 100);
                Add(typeof(LeatherArms), 100);
                Add(typeof(LeatherChest), 100);
                Add(typeof(LeatherLegs), 100);
                Add(typeof(LeatherGloves), 100);

                Add(typeof(StuddedGorget), 100);
                Add(typeof(StuddedArms), 100);
                Add(typeof(StuddedChest), 100);
                Add(typeof(StuddedLegs), 100);
                Add(typeof(StuddedGloves), 100);

                Add(typeof(FemaleStuddedChest), 100);
                Add(typeof(FemalePlateChest), 207);
                Add(typeof(FemaleLeatherChest), 100);
                Add(typeof(LeatherShorts), 100);
                Add(typeof(LeatherSkirt), 100);
                Add(typeof(LeatherBustierArms), 100);
                Add(typeof(LeatherBustierArms), 100);
                Add(typeof(StuddedBustierArms), 100);
                Add(typeof(StuddedBustierArms), 100);

                Add(typeof(Bag), 30);
                Add(typeof(Pouch), 30);
                Add(typeof(Backpack), 30);
                Add(typeof(Bone), 10);
                Add(typeof(TigerPelt), 10);
                Add(typeof(DragonTurtleScute), 10);

                Add(typeof(SkinningKnife), 100);

                Add(typeof(TaxidermyKit), 100000);

            }
        }
    }
}