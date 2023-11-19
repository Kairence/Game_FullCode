using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBTavernKeeper : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBTavernKeeper()
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
                Add(new BeverageBuyInfo(typeof(BeverageBottle), BeverageType.Ale, 10, 20, 0x99F, 0));
                Add(new BeverageBuyInfo(typeof(BeverageBottle), BeverageType.Wine, 10, 20, 0x9C7, 0));
                Add(new BeverageBuyInfo(typeof(BeverageBottle), BeverageType.Liquor, 10, 20, 0x99B, 0));
                Add(new BeverageBuyInfo(typeof(Jug), BeverageType.Cider, 20, 20, 0x9C8, 0));
                Add(new BeverageBuyInfo(typeof(Pitcher), BeverageType.Milk, 20, 20, 0x9F0, 0));
                Add(new BeverageBuyInfo(typeof(Pitcher), BeverageType.Ale, 20, 20, 0x1F95, 0));
                Add(new BeverageBuyInfo(typeof(Pitcher), BeverageType.Cider, 20, 20, 0x1F97, 0));
                Add(new BeverageBuyInfo(typeof(Pitcher), BeverageType.Liquor, 20, 20, 0x1F99, 0));
                Add(new BeverageBuyInfo(typeof(Pitcher), BeverageType.Wine, 20, 20, 0x1F9B, 0));
                Add(new BeverageBuyInfo(typeof(Pitcher), BeverageType.Water, 20, 20, 0x1F9D, 0));

                Add(new GenericBuyInfo(typeof(BreadLoaf), 50, 10, 0x103B, 0));
                Add(new GenericBuyInfo(typeof(CheeseWheel), 25, 10, 0x97E, 0));
                Add(new GenericBuyInfo(typeof(CookedBird), 100, 20, 0x9B7, 0));
                Add(new GenericBuyInfo(typeof(LambLeg), 100, 20, 0x160A, 0));
                Add(new GenericBuyInfo(typeof(ChickenLeg), 100, 20, 0x1608, 0));
                Add(new GenericBuyInfo(typeof(Ribs), 20, 20, 0x9F2, 0));

                Add(new GenericBuyInfo(typeof(WoodenBowlOfCarrots), 10, 20, 0x15F9, 0));
                Add(new GenericBuyInfo(typeof(WoodenBowlOfCorn), 10, 20, 0x15FA, 0));
                Add(new GenericBuyInfo(typeof(WoodenBowlOfLettuce), 10, 20, 0x15FB, 0));
                Add(new GenericBuyInfo(typeof(WoodenBowlOfPeas), 10, 20, 0x15FC, 0));
                Add(new GenericBuyInfo(typeof(EmptyPewterBowl), 2, 20, 0x15FD, 0));
                Add(new GenericBuyInfo(typeof(PewterBowlOfCorn), 10, 20, 0x15FE, 0));
                Add(new GenericBuyInfo(typeof(PewterBowlOfLettuce), 10, 20, 0x15FF, 0));
                Add(new GenericBuyInfo(typeof(PewterBowlOfPeas), 10, 20, 0x1601, 0));
                Add(new GenericBuyInfo(typeof(PewterBowlOfPotatos), 10, 20, 0x1601, 0));
                Add(new GenericBuyInfo(typeof(WoodenBowlOfStew), 10, 20, 0x1604, 0));
                Add(new GenericBuyInfo(typeof(WoodenBowlOfTomatoSoup), 10, 20, 0x1606, 0));

                //Add(new GenericBuyInfo(typeof(ApplePie), 7, 20, 0x1041, 0)); //OSI just has Pie, not Apple/Fruit/Meat

                Add(new GenericBuyInfo(typeof(Peach), 10, 20, 0x9D2, 0));
                Add(new GenericBuyInfo(typeof(Pear), 10, 20, 0x994, 0));
                Add(new GenericBuyInfo(typeof(Grapes), 10, 20, 0x9D1, 0));
                Add(new GenericBuyInfo(typeof(Apple), 10, 20, 0x9D0, 0));
                Add(new GenericBuyInfo(typeof(Banana), 10, 20, 0x171F, 0));

                Add(new GenericBuyInfo(typeof(Torch), 15, 20, 0xF6B, 0));
                Add(new GenericBuyInfo(typeof(Candle), 5, 20, 0xA28, 0));

                Add(new GenericBuyInfo(typeof(Backpack), 15, 20, 0x9B2, 0));
				
                Add(new GenericBuyInfo("1016450", typeof(Chessboard), 2, 20, 0xFA6, 0));
                Add(new GenericBuyInfo("1016449", typeof(CheckerBoard), 2, 20, 0xFA6, 0));
                Add(new GenericBuyInfo(typeof(Backgammon), 2, 20, 0xE1C, 0));
                Add(new GenericBuyInfo(typeof(Dices), 2, 20, 0xFA7, 0));
                Add(new GenericBuyInfo("1041243", typeof(ContractOfEmployment), 1252, 20, 0x14F0, 0));
                Add(new GenericBuyInfo("a barkeep contract", typeof(BarkeepContract), 1252, 20, 0x14F0, 0));

                if (Multis.BaseHouse.NewVendorSystem)
                    Add(new GenericBuyInfo("1062332", typeof(VendorRentalContract), 1252, 20, 0x14F0, 0x672));
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