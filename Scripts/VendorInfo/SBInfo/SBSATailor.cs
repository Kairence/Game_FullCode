using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBSATailor : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBSATailor()
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
                Add(new GenericBuyInfo(typeof(Cotton), 102, 20, 0xDF9, 0, true));
                Add(new GenericBuyInfo(typeof(Wool), 62, 20, 0xDF8, 0, true));
                Add(new GenericBuyInfo(typeof(Flax), 102, 20, 0x1A9C, 0, true));
                Add(new GenericBuyInfo(typeof(SpoolOfThread), 18, 20, 0xFA0, 0, true));
                Add(new GenericBuyInfo(typeof(SewingKit), 100, 20, 0xF9D, 0)); 
                Add(new GenericBuyInfo(typeof(Scissors), 100, 20, 0xF9F, 0));
                Add(new GenericBuyInfo(typeof(DyeTub), 100, 20, 0xFAB, 0)); 
                Add(new GenericBuyInfo(typeof(Dyes), 100, 20, 0xFA9, 0)); 
                
                Add(new GenericBuyInfo(typeof(GargishRobe), 100, 20, 0x4000, 0));
                Add(new GenericBuyInfo(typeof(GargishFancyRobe), 100, 20, 0x4002, 0));

                this.Add(new GenericBuyInfo(typeof(FemaleGargishClothArmsArmor), 100, 20, 0x403, 0));
                this.Add(new GenericBuyInfo(typeof(GargishClothArmsArmor), 100, 20, 0x404, 0));
                this.Add(new GenericBuyInfo(typeof(FemaleGargishClothChestArmor), 100, 20, 0x405, 0));
                this.Add(new GenericBuyInfo(typeof(GargishClothChestArmor), 100, 20, 0x406, 0));
                this.Add(new GenericBuyInfo(typeof(FemaleGargishClothLegsArmor), 100, 20, 0x409, 0));
                this.Add(new GenericBuyInfo(typeof(GargishClothLegsArmor), 100, 20, 0x40A, 0));
                this.Add(new GenericBuyInfo(typeof(FemaleGargishClothKiltArmor), 100, 20, 0x407, 0));
                this.Add(new GenericBuyInfo(typeof(GargishClothKiltArmor), 100, 20, 0x408, 0));
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                Add(typeof(Cotton), 51);
                Add(typeof(Wool), 31);
                Add(typeof(Flax), 51);
                Add(typeof(SpoolOfThread), 9);
                Add(typeof(SewingKit), 1);
                Add(typeof(Scissors), 6);
                Add(typeof(DyeTub), 4);
                Add(typeof(Dyes), 4);

                this.Add(typeof(GargishRobe), 16);
                this.Add(typeof(GargishFancyRobe), 23);
                this.Add(typeof(FemaleGargishClothArmsArmor), 30);
                this.Add(typeof(GargishClothArmsArmor), 30);
                this.Add(typeof(FemaleGargishClothChestArmor), 40);
                this.Add(typeof(GargishClothChestArmor), 42);
                this.Add(typeof(FemaleGargishClothLegsArmor), 30);
                this.Add(typeof(GargishClothLegsArmor), 32);
                this.Add(typeof(FemaleGargishClothKiltArmor), 30);
                this.Add(typeof(GargishClothKiltArmor), 32);
            }
        }
    }
}