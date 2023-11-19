using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles 
{ 
    public class SBButcher : SBInfo 
    { 
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBButcher() 
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
                Add(new GenericBuyInfo(typeof(Bacon), 17, 20, 0x979, 0));
                Add(new GenericBuyInfo(typeof(Ham), 26, 20, 0x9C9, 0));
                Add(new GenericBuyInfo(typeof(Sausage), 18, 20, 0x9C0, 0));
                Add(new GenericBuyInfo(typeof(RawChickenLeg), 50, 20, 0x1607, 0));
                Add(new GenericBuyInfo(typeof(RawBird), 100, 20, 0x9B9, 0));
                Add(new GenericBuyInfo(typeof(RawLambLeg), 100, 20, 0x1609, 0));
                Add(new GenericBuyInfo(typeof(RawRibs), 20, 20, 0x9F1, 0));
                Add(new GenericBuyInfo(typeof(Bone), 10, 100, 0xf7e, 0));
				
                Add(new GenericBuyInfo(typeof(ButcherKnife), 100, 20, 0x13F6, 0));
                Add(new GenericBuyInfo(typeof(Cleaver), 100, 20, 0xEC3, 0));
                Add(new GenericBuyInfo(typeof(SkinningKnife), 100, 20, 0xEC4, 0)); 
                Add(new GenericBuyInfo(typeof(Hides), 10, 999, 0x1079, 0, true)); 
				
            }
        }

        public class InternalSellInfo : GenericSellInfo 
        { 
            public InternalSellInfo() 
            { 
				
                Add(typeof(Bacon), 3); 
                Add(typeof(Sausage), 9); 
                Add(typeof(Ham), 13); 
                Add(typeof(RawRibs), 8); 
                Add(typeof(RawLambLeg), 4); 
                Add(typeof(RawChickenLeg), 3); 
                Add(typeof(RawBird), 4); 
                Add(typeof(ButcherKnife), 7); 
                Add(typeof(Cleaver), 7); 
                Add(typeof(SkinningKnife), 7); 
                Add(typeof(Bone), 10);
                Add(typeof(Hides), 2); 
				
            }
        }
    }
}