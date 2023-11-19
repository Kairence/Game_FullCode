using System;
using System.Collections.Generic;
using Server.Guilds;
using Server.Items;

namespace Server.Mobiles
{
    public class SBProvisioner : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();

        public SBProvisioner()
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
                Add(new GenericBuyInfo("1060834", typeof(Engines.Plants.PlantBowl), 2, 20, 0x15FD, 0));

                Add(new GenericBuyInfo(typeof(Backpack), 50, 20, 0x9B2, 0));
                Add(new GenericBuyInfo(typeof(Pouch), 30, 20, 0xE79, 0));
                Add(new GenericBuyInfo(typeof(Bag), 30, 20, 0xE76, 0));
				
                Add(new GenericBuyInfo(typeof(Candle), 12, 20, 0xA28, 0));
                Add(new GenericBuyInfo(typeof(Torch), 20, 20, 0xF6B, 0));
                Add(new GenericBuyInfo(typeof(Lantern), 30, 20, 0xA25, 0));
                Add(new GenericBuyInfo(typeof(OilFlask), 10, 20, 0x1C18, 0));

                Add(new GenericBuyInfo(typeof(FloppyHat), 100, 20, 0x1713, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(WideBrimHat), 100, 20, 0x1714, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(Cap), 100, 20, 0x1715, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(TallStrawHat), 100, 20, 0x1716, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(StrawHat), 100, 20, 0x1717, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(WizardsHat), 100, 20, 0x1718, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(LeatherCap), 100, 20, 0x1DB9, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(FeatheredHat), 100, 20, 0x171A, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(TricorneHat), 100, 20, 0x171B, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(Bandana), 100, 20, 0x1540, Utility.RandomDyedHue()));
                Add(new GenericBuyInfo(typeof(SkullCap), 100, 20, 0x1544, Utility.RandomDyedHue()));

                Add(new GenericBuyInfo(typeof(RedBook), 15, 20, 0xFF1, 0));
                Add(new GenericBuyInfo(typeof(BlueBook), 15, 20, 0xFF2, 0));
                Add(new GenericBuyInfo(typeof(TanBook), 15, 20, 0xFF0, 0));

                Add(new GenericBuyInfo(typeof(WoodenBox), 40, 20, 0xE7D, 0));
                Add(new GenericBuyInfo(typeof(Key), 10, 20, 0x100E, 0));

                Add(new GenericBuyInfo(typeof(Bedroll), 10, 20, 0xA59, 0));


                if (Core.AOS)
                    Add(new GenericBuyInfo(typeof(Engines.Mahjong.MahjongGame), 60, 20, 0xFAA, 0));
                Add(new GenericBuyInfo(typeof(Dices), 20, 20, 0xFA7, 0));

                if (Core.AOS)
                {
                    Add(new GenericBuyInfo(typeof(SmallBagBall), 30, 20, 0x2256, 0));
                    Add(new GenericBuyInfo(typeof(LargeBagBall), 30, 20, 0x2257, 0));
                }

				/*
                if (!Guild.NewGuildSystem)
                    Add(new GenericBuyInfo("1041055", typeof(GuildDeed), 12450, 20, 0x14F0, 0));
				*/
                if (Core.ML)
                    Add(new GenericBuyInfo("1079931", typeof(SalvageBag), 100, 20, 0xE76, Utility.RandomBlueHue()));

                if (Core.ML)
                    Add(new GenericBuyInfo("Equip Bag", typeof(EquipBag), 100, 20, 0xE76, Utility.RandomRedHue()));
				
				/*
                if (Core.SA)
                    Add(new GenericBuyInfo("1114770", typeof(SkinTingeingTincture), 1255, 20, 0xEFF, 90));
				*/
			}
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
				Add(typeof(IronOre), 5 );
				
                Add(typeof(Engines.Plants.PlantBowl), 2 );
                Add(typeof(Backpack), 50);
                Add(typeof(Pouch), 30);
                Add(typeof(Bag), 30);
				
                Add(typeof(Candle), 6);
                Add(typeof(Torch), 20);
                Add(typeof(Lantern), 30);
                Add(typeof(OilFlask), 10);

                Add(typeof(FloppyHat), 100);
                Add(typeof(WideBrimHat), 100);
                Add(typeof(Cap), 100);
                Add(typeof(TallStrawHat), 100);
                Add(typeof(StrawHat), 100);
                Add(typeof(WizardsHat), 100);
                Add(typeof(LeatherCap), 100);
                Add(typeof(FeatheredHat), 100);
                Add(typeof(TricorneHat), 100);
                Add(typeof(Bandana), 100);
                Add(typeof(SkullCap), 100);

                Add(typeof(RedBook), 15);
                Add(typeof(BlueBook), 15);
                Add(typeof(TanBook), 15);

                Add(typeof(WoodenBox), 40);
                Add(typeof(Key), 10);

                Add(typeof(Bedroll), 10);


                if (Core.AOS)
                    Add(typeof(Engines.Mahjong.MahjongGame), 60);
                Add(typeof(Dices), 20);

                if (Core.AOS)
                {
                    Add(typeof(SmallBagBall), 30);
                    Add(typeof(LargeBagBall), 30);
                }

				/*
                if (!Guild.NewGuildSystem)
                    Add(new GenericBuyInfo("1041055", typeof(GuildDeed), 12450, 20, 0x14F0, 0));
				*/
                if (Core.ML)
                    Add(typeof(SalvageBag), 100);

                if (Core.ML)
                    Add(typeof(EquipBag), 100);
				

            }
        }
    }
}