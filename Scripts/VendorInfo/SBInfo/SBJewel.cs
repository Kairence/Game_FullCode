using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBJewel : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBJewel()
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
                Add(new GenericBuyInfo(typeof(GoldRing), 100, 20, 0x108A, 0));
                Add(new GenericBuyInfo(typeof(GoldNecklace), 100, 20, 0x1088, 0));
                Add(new GenericBuyInfo(typeof(GoldBracelet), 100, 20, 0x1086, 0));
                Add(new GenericBuyInfo(typeof(GoldEarrings), 100, 20, 0x1087, 0));
                Add(new GenericBuyInfo(typeof(SilverRing), 100, 20, 0x1F09, 0));
                Add(new GenericBuyInfo(typeof(SilverNecklace), 100, 20, 0x1F08, 0));
                Add(new GenericBuyInfo(typeof(SilverBracelet), 100, 20, 0x1F06, 0));
                Add(new GenericBuyInfo(typeof(SilverEarrings), 100, 20, 0x1F07, 0));
				/*
                Add(new GenericBuyInfo("1060740", typeof(BroadcastCrystal), 68, 20, 0x1ED0, 0, new object[] { 500 })); // 500 charges
                Add(new GenericBuyInfo("1060740", typeof(BroadcastCrystal), 131, 20, 0x1ED0, 0, new object[] { 1000 })); // 1000 charges
                Add(new GenericBuyInfo("1060740", typeof(BroadcastCrystal), 256, 20, 0x1ED0, 0, new object[] { 2000 })); // 2000 charges

                Add(new GenericBuyInfo("1060740", typeof(ReceiverCrystal), 6, 20, 0x1ED0, 0));
				*/

                Add(new GenericBuyInfo(typeof(Beads), 20, 20, 0x108B, 0));
                Add(new GenericBuyInfo(typeof(StarSapphire), 100, 20, 0x0F0F, 0));
                Add(new GenericBuyInfo(typeof(Emerald), 100, 20, 0xF10, 0));
                Add(new GenericBuyInfo(typeof(Sapphire), 100, 20, 0xF19, 0));
                Add(new GenericBuyInfo(typeof(Ruby), 100, 20, 0xF13, 0));
                Add(new GenericBuyInfo(typeof(Citrine), 100, 20, 0xF15, 0));
                Add(new GenericBuyInfo(typeof(Amethyst), 100, 20, 0xF16, 0));
                Add(new GenericBuyInfo(typeof(Tourmaline), 100, 20, 0x0F18, 0));
                Add(new GenericBuyInfo(typeof(Amber), 100, 20, 0xF25, 0));
                Add(new GenericBuyInfo(typeof(Diamond), 100, 20, 0xF26, 0));
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                Add(typeof(Amber), 25);
                Add(typeof(Amethyst), 50);
                Add(typeof(Citrine), 25);
                Add(typeof(Diamond), 100);
                Add(typeof(Emerald), 50);
                Add(typeof(Ruby), 37);
                Add(typeof(Sapphire), 50);
                Add(typeof(StarSapphire), 62);
                Add(typeof(Tourmaline), 47);
                Add(typeof(Beads), 13);
                Add(typeof(GoldRing), 13);
                Add(typeof(SilverRing), 10);
                Add(typeof(GoldNecklace), 13);
                Add(typeof(SilverNecklace), 10);
                Add(typeof(GoldBracelet), 13);
                Add(typeof(SilverBracelet), 10);
                Add(typeof(GoldEarrings), 13);
                Add(typeof(SilverEarrings), 10);
            }
        }
    }
}