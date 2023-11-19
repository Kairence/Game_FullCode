using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class SBWaiter : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBWaiter()
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

				/*
                Add(new GenericBuyInfo(typeof(WoodenBowlOfCarrots), 3, 20, 0x15F9, 0));
                Add(new GenericBuyInfo(typeof(WoodenBowlOfCorn), 3, 20, 0x15FA, 0));
                Add(new GenericBuyInfo(typeof(WoodenBowlOfLettuce), 3, 20, 0x15FB, 0));
                Add(new GenericBuyInfo(typeof(WoodenBowlOfPeas), 3, 20, 0x15FC, 0));
                Add(new GenericBuyInfo(typeof(EmptyPewterBowl), 2, 20, 0x15FD, 0));
                Add(new GenericBuyInfo(typeof(PewterBowlOfCorn), 3, 20, 0x15FE, 0));
                Add(new GenericBuyInfo(typeof(PewterBowlOfLettuce), 3, 20, 0x15FF, 0));
                Add(new GenericBuyInfo(typeof(PewterBowlOfPeas), 3, 20, 0x1601, 0));
                Add(new GenericBuyInfo(typeof(PewterBowlOfPotatos), 3, 20, 0x1601, 0));
                Add(new GenericBuyInfo(typeof(WoodenBowlOfStew), 3, 20, 0x1604, 0));
                Add(new GenericBuyInfo(typeof(WoodenBowlOfTomatoSoup), 3, 20, 0x1606, 0));

                Add(new GenericBuyInfo(typeof(ApplePie), 7, 20, 0x1041, 0, true)); //OSI just has Pie, not Apple/Fruit/Meat
				*/
				
                Add(new GenericBuyInfo("1016450", typeof(Chessboard), 20, 20, 0xFA6, 0));
                Add(new GenericBuyInfo("1016449", typeof(CheckerBoard), 20, 20, 0xFA6, 0));
                Add(new GenericBuyInfo(typeof(Backgammon), 20, 20, 0xE1C, 0));
                Add(new GenericBuyInfo(typeof(Dices), 20, 20, 0xFA7, 0));
                Add(new GenericBuyInfo("1041243", typeof(ContractOfEmployment), 1252, 20, 0x14F0, 0));
                Add(new GenericBuyInfo("a barkeep contract", typeof(BarkeepContract), 1252, 20, 0x14F0, 0));
                if (Multis.BaseHouse.NewVendorSystem)
                    Add(new GenericBuyInfo("1062332", typeof(VendorRentalContract), 1252, 20, 0x14F0, 0x672));
                /*if ( Map == Tokuno )
                {
                Add( new GenericBuyInfo( typeof( Wasabi ), 2, 20, 0x24E8, 0 ) );
                Add( new GenericBuyInfo( typeof( Wasabi ), 2, 20, 0x24E9, 0 ) );
                Add( new GenericBuyInfo( typeof( BentoBox ), 6, 20, 0x2836, 0 ) );
                Add( new GenericBuyInfo( typeof( BentoBox ), 6, 20, 0x2837, 0 ) );
                Add( new GenericBuyInfo( typeof( GreenTeaBasket ), 2, 20, 0x284B, 0 ) );
                }*/
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