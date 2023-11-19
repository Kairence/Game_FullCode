using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBSwordWeapon : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBSwordWeapon()
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
                Add(new GenericBuyInfo(typeof(Cutlass), 100, 20, 0x1441, 0));
                Add(new GenericBuyInfo(typeof(Katana), 100, 20, 0x13FF, 0));
                Add(new GenericBuyInfo(typeof(Kryss), 100, 20, 0x1401, 0));
                Add(new GenericBuyInfo(typeof(Broadsword), 100, 20, 0xF5E, 0));
                Add(new GenericBuyInfo(typeof(Longsword), 100, 20, 0xF61, 0));
                Add(new GenericBuyInfo(typeof(ThinLongsword), 100, 20, 0x13B8, 0));
                Add(new GenericBuyInfo(typeof(VikingSword), 100, 20, 0x13B9, 0));
                Add(new GenericBuyInfo(typeof(Scimitar), 100, 20, 0x13B6, 0));

                if (Core.AOS)
                {
                    Add(new GenericBuyInfo(typeof(BoneHarvester), 100, 20, 0x26BB, 0));
                    Add(new GenericBuyInfo(typeof(CrescentBlade), 100, 20, 0x26C1, 0));
                    Add(new GenericBuyInfo(typeof(DoubleBladedStaff), 100, 20, 0x26BF, 0));
                    Add(new GenericBuyInfo(typeof(Lance), 100, 20, 0x26C0, 0));
                    Add(new GenericBuyInfo(typeof(Pike), 100, 20, 0x26BE, 0));
                    Add(new GenericBuyInfo(typeof(Scythe), 100, 20, 0x26BA, 0));
                }
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