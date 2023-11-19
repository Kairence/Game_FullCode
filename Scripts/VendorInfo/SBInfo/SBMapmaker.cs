using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBMapmaker : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBMapmaker()
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
                Add(new GenericBuyInfo(typeof(BlankMap), 20, 40, 0x14EC, 0));
                Add(new GenericBuyInfo(typeof(MapmakersPen), 100, 20, 0x0FBF, 0));
				
                for (int i = 0; i < PresetMapEntry.Table.Length; ++i)
                    Add(new PresetMapBuyInfo(PresetMapEntry.Table[i], Utility.RandomMinMax(20, 30), 20));
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
				Add(typeof(BlankMap), 5 );
				Add(typeof(MapmakersPen), 5 );
                //TODO: Buy back maps that the mapmaker sells!!!
            }
        }
    }
}