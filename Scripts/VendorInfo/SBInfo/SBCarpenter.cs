using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBCarpenter : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBCarpenter()
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
                Add(new GenericBuyInfo(typeof(WoodenBox), 35, 20, 0xE7D, 0));
                Add(new GenericBuyInfo(typeof(SmallCrate), 60, 20, 0x9A9, 0));
                Add(new GenericBuyInfo(typeof(MediumCrate), 80, 20, 0xE3F, 0));
                Add(new GenericBuyInfo(typeof(LargeCrate), 95, 20, 0xE3D, 0));
                Add(new GenericBuyInfo(typeof(WoodenChest), 110, 20, 0xe43, 0));
                Add(new GenericBuyInfo(typeof(Barrel), 200, 20, 0xE77, 0));

                Add(new GenericBuyInfo(typeof(LargeTable), 100, 20, 0xB90, 0));
                Add(new GenericBuyInfo(typeof(Nightstand), 100, 20, 0xB35, 0));
                Add(new GenericBuyInfo(typeof(YewWoodTable), 100, 20, 0xB8F, 0));

                Add(new GenericBuyInfo(typeof(Throne), 50, 20, 0xB33, 0));
                Add(new GenericBuyInfo(typeof(WoodenThrone), 50, 20, 0xB2E, 0));
                Add(new GenericBuyInfo(typeof(Stool), 50, 20, 0xA2A, 0));
                Add(new GenericBuyInfo(typeof(FootStool), 50, 20, 0xB5E, 0));
                Add(new GenericBuyInfo(typeof(WoodenChair), 50, 20, 0xB57, 0));
                Add(new GenericBuyInfo(typeof(WoodenChairCushion), 50, 20, 0xB53, 0));
                Add(new GenericBuyInfo(typeof(FancyWoodenChairCushion), 50, 20, 0xB4F, 0));
                Add(new GenericBuyInfo(typeof(OrnateElvenChair), 50, 20, 0x2DE3, 0));
                Add(new GenericBuyInfo(typeof(BigElvenChair), 50, 20, 0x2DEB, 0));
                Add(new GenericBuyInfo(typeof(ElvenReadingChair), 50, 20, 0x2DF5, 0));
                Add(new GenericBuyInfo(typeof(BambooChair), 50, 20, 0xB5B, 0));
                Add(new GenericBuyInfo(typeof(WoodenBench), 25, 20, 0xB2D, 0));

                Add(new GenericBuyInfo(typeof(BarkFragment), 10, 100, 0x318F, 0));


				
				Add(new GenericBuyInfo(typeof(Nails), 100, 20, 0x102E, 0));
                Add(new GenericBuyInfo(typeof(Axle), 100, 20, 0x105B, 0));
                Add(new GenericBuyInfo(typeof(Board), 5, 20, 0x1BD7, 0, true));
                Add(new GenericBuyInfo(typeof(DrawKnife), 100, 20, 0x10E4, 0));
                Add(new GenericBuyInfo(typeof(Froe), 100, 20, 0x10E5, 0));
                Add(new GenericBuyInfo(typeof(Scorp), 100, 20, 0x10E7, 0));
                Add(new GenericBuyInfo(typeof(Inshave), 100, 20, 0x10E6, 0));
                Add(new GenericBuyInfo(typeof(DovetailSaw), 100, 20, 0x1028, 0));
                Add(new GenericBuyInfo(typeof(Saw), 100, 20, 0x1034, 0));
                Add(new GenericBuyInfo(typeof(Hammer), 100, 20, 0x102A, 0));
                Add(new GenericBuyInfo(typeof(MouldingPlane), 100, 20, 0x102C, 0));
                Add(new GenericBuyInfo(typeof(SmoothingPlane), 100, 20, 0x1032, 0));
                Add(new GenericBuyInfo(typeof(JointingPlane), 100, 20, 0x1030, 0));
				
                Add(new GenericBuyInfo(typeof(Drums), 35, 20, 0xE9C, 0));
                Add(new GenericBuyInfo(typeof(Tambourine), 35, 20, 0xE9D, 0));
                Add(new GenericBuyInfo(typeof(LapHarp), 35, 20, 0xEB2, 0));
                Add(new GenericBuyInfo(typeof(Lute), 35, 20, 0xEB3, 0));
				

                Add(new GenericBuyInfo("1154004", typeof(SolventFlask), 100, 500, 7192, 2969, true));
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                Add(typeof(WoodenBox), 7);
                Add(typeof(SmallCrate), 5);
                Add(typeof(MediumCrate), 6);
                Add(typeof(LargeCrate), 7);
                Add(typeof(WoodenChest), 15);
                Add(typeof(Barrel), 15);
              
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
                Add(typeof(OrnateElvenChair), 6);
                Add(typeof(OrnateElvenChair), 6);
                Add(typeof(BigElvenChair), 6);

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
                Add(typeof(Nails), 1);

                Add(typeof(BarkFragment), 10);

                Add(typeof(Lute), 10);
                Add(typeof(LapHarp), 10);
                Add(typeof(Tambourine), 10);
                Add(typeof(Drums), 10);
                Add(typeof(SolventFlask), 10);

                //Add(typeof(Log), 1);
				
            }
        }
    }
}