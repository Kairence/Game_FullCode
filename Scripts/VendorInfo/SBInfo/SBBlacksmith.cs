using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles 
{ 
    public class SBBlacksmith : SBInfo 
    { 
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBBlacksmith() 
        { 
        }

        public override IShopSellInfo SellInfo
        {
            get
            {
                return this.m_SellInfo;
            }
        }
        public override List<GenericBuyInfo> BuyInfo
        {
            get
            {
                return this.m_BuyInfo;
            }
        }

        public class InternalBuyInfo : List<GenericBuyInfo> 
        { 
            public InternalBuyInfo() 
            {
                this.Add(new GenericBuyInfo(typeof(IronIngot), 5, 16, 0x1BF2, 0, true));
                this.Add(new GenericBuyInfo(typeof(Tongs), 100, 14, 0xFBB, 0)); 
 
                this.Add(new GenericBuyInfo(typeof(BronzeShield), 100, 20, 0x1B72, 0));
                this.Add(new GenericBuyInfo(typeof(Buckler), 100, 20, 0x1B73, 0));
                this.Add(new GenericBuyInfo(typeof(MetalKiteShield), 123, 20, 0x1B74, 0));
                this.Add(new GenericBuyInfo(typeof(HeaterShield), 231, 20, 0x1B76, 0));
                this.Add(new GenericBuyInfo(typeof(WoodenKiteShield), 70, 20, 0x1B78, 0));
                this.Add(new GenericBuyInfo(typeof(MetalShield), 121, 20, 0x1B7B, 0));

                this.Add(new GenericBuyInfo(typeof(WoodenShield), 100, 20, 0x1B7A, 0));

                this.Add(new GenericBuyInfo(typeof(PlateGorget), 104, 20, 0x1413, 0));
                this.Add(new GenericBuyInfo(typeof(PlateChest), 243, 20, 0x1415, 0));
                this.Add(new GenericBuyInfo(typeof(PlateLegs), 218, 20, 0x1411, 0));
                this.Add(new GenericBuyInfo(typeof(PlateArms), 188, 20, 0x1410, 0));
                this.Add(new GenericBuyInfo(typeof(PlateGloves), 155, 20, 0x1414, 0));

                this.Add(new GenericBuyInfo(typeof(PlateHelm), 100, 20, 0x1412, 0));
                this.Add(new GenericBuyInfo(typeof(CloseHelm), 100, 20, 0x1408, 0));
                this.Add(new GenericBuyInfo(typeof(Helmet), 100, 20, 0x140A, 0));
                this.Add(new GenericBuyInfo(typeof(NorseHelm), 100, 20, 0x140E, 0));
                this.Add(new GenericBuyInfo(typeof(Bascinet), 100, 20, 0x140C, 0));

                this.Add(new GenericBuyInfo(typeof(ChainCoif), 100, 20, 0x13BB, 0));
                this.Add(new GenericBuyInfo(typeof(ChainChest), 143, 20, 0x13BF, 0));
                this.Add(new GenericBuyInfo(typeof(ChainLegs), 149, 20, 0x13BE, 0));

                this.Add(new GenericBuyInfo(typeof(RingmailChest), 121, 20, 0x13ec, 0));
                this.Add(new GenericBuyInfo(typeof(RingmailLegs), 100, 20, 0x13F0, 0));
                this.Add(new GenericBuyInfo(typeof(RingmailArms), 100, 20, 0x13EE, 0));
                this.Add(new GenericBuyInfo(typeof(RingmailGloves), 100, 20, 0x13eb, 0));

                this.Add(new GenericBuyInfo(typeof(ExecutionersAxe), 100, 20, 0xF45, 0));
                this.Add(new GenericBuyInfo(typeof(Bardiche), 100, 20, 0xF4D, 0));
                this.Add(new GenericBuyInfo(typeof(BattleAxe), 100, 20, 0xF47, 0));
                this.Add(new GenericBuyInfo(typeof(TwoHandedAxe), 100, 20, 0x1443, 0));
                this.Add(new GenericBuyInfo(typeof(Bow), 100, 20, 0x13B2, 0));
                this.Add(new GenericBuyInfo(typeof(ButcherKnife), 100, 20, 0x13F6, 0));
                this.Add(new GenericBuyInfo(typeof(Crossbow), 100, 20, 0xF50, 0));
                this.Add(new GenericBuyInfo(typeof(HeavyCrossbow), 100, 20, 0x13FD, 0));
                this.Add(new GenericBuyInfo(typeof(Cutlass), 100, 20, 0x1441, 0));
                this.Add(new GenericBuyInfo(typeof(Dagger), 100, 20, 0xF52, 0));
                this.Add(new GenericBuyInfo(typeof(Halberd), 100, 20, 0x143E, 0));
                this.Add(new GenericBuyInfo(typeof(HammerPick), 100, 20, 0x143D, 0));
                this.Add(new GenericBuyInfo(typeof(Katana), 100, 20, 0x13FF, 0));
                this.Add(new GenericBuyInfo(typeof(Kryss), 100, 20, 0x1401, 0));
                this.Add(new GenericBuyInfo(typeof(Broadsword), 100, 20, 0xF5E, 0));
                this.Add(new GenericBuyInfo(typeof(Longsword), 100, 20, 0xF61, 0));
                this.Add(new GenericBuyInfo(typeof(ThinLongsword), 100, 20, 0x13B8, 0));
                this.Add(new GenericBuyInfo(typeof(VikingSword), 100, 20, 0x13B9, 0));
                this.Add(new GenericBuyInfo(typeof(Cleaver), 100, 20, 0xEC3, 0));
                this.Add(new GenericBuyInfo(typeof(Axe), 100, 20, 0xF49, 0));
                this.Add(new GenericBuyInfo(typeof(DoubleAxe), 100, 20, 0xF4B, 0));
                this.Add(new GenericBuyInfo(typeof(Pickaxe), 100, 20, 0xE86, 0));
                this.Add(new GenericBuyInfo(typeof(Pitchfork), 100, 20, 0xE87, 0));
                this.Add(new GenericBuyInfo(typeof(Scimitar), 100, 20, 0x13B6, 0));
                this.Add(new GenericBuyInfo(typeof(SkinningKnife), 100, 20, 0xEC4, 0));
                this.Add(new GenericBuyInfo(typeof(LargeBattleAxe), 100, 20, 0x13FB, 0));
                this.Add(new GenericBuyInfo(typeof(WarAxe), 100, 20, 0x13B0, 0));

                if (Core.AOS)
                {
                    this.Add(new GenericBuyInfo(typeof(BoneHarvester), 100, 20, 0x26BB, 0));
                    this.Add(new GenericBuyInfo(typeof(CrescentBlade), 100, 20, 0x26C1, 0));
                    this.Add(new GenericBuyInfo(typeof(DoubleBladedStaff), 100, 20, 0x26BF, 0));
                    this.Add(new GenericBuyInfo(typeof(Lance), 100, 20, 0x26C0, 0));
                    this.Add(new GenericBuyInfo(typeof(Pike), 100, 20, 0x26BE, 0));
                    this.Add(new GenericBuyInfo(typeof(Scythe), 100, 20, 0x26BA, 0));
                    this.Add(new GenericBuyInfo(typeof(CompositeBow), 100, 20, 0x26C2, 0));
                    this.Add(new GenericBuyInfo(typeof(RepeatingCrossbow), 100, 20, 0x26C3, 0));
                }

                this.Add(new GenericBuyInfo(typeof(BlackStaff), 100, 20, 0xDF1, 0));
                this.Add(new GenericBuyInfo(typeof(Club), 100, 20, 0x13B4, 0));
                this.Add(new GenericBuyInfo(typeof(GnarledStaff), 100, 20, 0x13F8, 0));
                this.Add(new GenericBuyInfo(typeof(Mace), 100, 20, 0xF5C, 0));
                this.Add(new GenericBuyInfo(typeof(Maul), 100, 20, 0x143B, 0));
                this.Add(new GenericBuyInfo(typeof(QuarterStaff), 100, 20, 0xE89, 0));
                this.Add(new GenericBuyInfo(typeof(ShepherdsCrook), 100, 20, 0xE81, 0));
                this.Add(new GenericBuyInfo(typeof(SmithHammer), 100, 20, 0x13E3, 0));
                this.Add(new GenericBuyInfo(typeof(ShortSpear), 100, 20, 0x1403, 0));
                this.Add(new GenericBuyInfo(typeof(Spear), 100, 20, 0xF62, 0));
                this.Add(new GenericBuyInfo(typeof(WarHammer), 100, 20, 0x1439, 0));
                this.Add(new GenericBuyInfo(typeof(WarMace), 100, 20, 0x1407, 0));

                if (Core.AOS)
                {
                    this.Add(new GenericBuyInfo(typeof(Scepter), 100, 20, 0x26BC, 0));
                    //this.Add(new GenericBuyInfo(typeof(BladedStaff), 100, 20, 0x26BD, 0));
                }
			}
        }

        public class InternalSellInfo : GenericSellInfo 
        { 
            public InternalSellInfo() 
            {
                this.Add(typeof(Tongs), 7); 
                this.Add(typeof(IronIngot), 4); 

                this.Add(typeof(Buckler), 25);
                this.Add(typeof(BronzeShield), 33);
                this.Add(typeof(MetalShield), 60);
                this.Add(typeof(MetalKiteShield), 62);
                this.Add(typeof(HeaterShield), 115);
                this.Add(typeof(WoodenKiteShield), 35);

                this.Add(typeof(WoodenShield), 15);

                this.Add(typeof(PlateArms), 94);
                this.Add(typeof(PlateChest), 121);
                this.Add(typeof(PlateGloves), 72);
                this.Add(typeof(PlateGorget), 52);
                this.Add(typeof(PlateLegs), 109);

                this.Add(typeof(FemalePlateChest), 113);
                this.Add(typeof(FemaleLeatherChest), 18);
                this.Add(typeof(FemaleStuddedChest), 25);
                this.Add(typeof(LeatherShorts), 14);
                this.Add(typeof(LeatherSkirt), 11);
                this.Add(typeof(LeatherBustierArms), 11);
                this.Add(typeof(StuddedBustierArms), 27);

                this.Add(typeof(Bascinet), 9);
                this.Add(typeof(CloseHelm), 9);
                this.Add(typeof(Helmet), 9);
                this.Add(typeof(NorseHelm), 9);
                this.Add(typeof(PlateHelm), 10);

                this.Add(typeof(ChainCoif), 6);
                this.Add(typeof(ChainChest), 71);
                this.Add(typeof(ChainLegs), 74);

                this.Add(typeof(RingmailArms), 42);
                this.Add(typeof(RingmailChest), 60);
                this.Add(typeof(RingmailGloves), 26);
                this.Add(typeof(RingmailLegs), 45);

                this.Add(typeof(BattleAxe), 13);
                this.Add(typeof(DoubleAxe), 26);
                this.Add(typeof(ExecutionersAxe), 15);
                this.Add(typeof(LargeBattleAxe), 16);
                this.Add(typeof(Pickaxe), 11);
                this.Add(typeof(TwoHandedAxe), 16);
                this.Add(typeof(WarAxe), 14);
                this.Add(typeof(Axe), 20);

                this.Add(typeof(Bardiche), 30);
                this.Add(typeof(Halberd), 21);

                this.Add(typeof(ButcherKnife), 7);
                this.Add(typeof(Cleaver), 7);
                this.Add(typeof(Dagger), 10);
                this.Add(typeof(SkinningKnife), 7);

                this.Add(typeof(Club), 8);
                this.Add(typeof(HammerPick), 13);
                this.Add(typeof(Mace), 14);
                this.Add(typeof(Maul), 10);
                this.Add(typeof(WarHammer), 12);
                this.Add(typeof(WarMace), 15);

                this.Add(typeof(HeavyCrossbow), 27);
                this.Add(typeof(Bow), 17);
                this.Add(typeof(Crossbow), 23); 

                if (Core.AOS)
                {
                    this.Add(typeof(CompositeBow), 25);
                    this.Add(typeof(RepeatingCrossbow), 28);
                    this.Add(typeof(Scepter), 20);
                    this.Add(typeof(BladedStaff), 20);
                    this.Add(typeof(Scythe), 19);
                    this.Add(typeof(BoneHarvester), 17);
                    this.Add(typeof(Scepter), 18);
                    this.Add(typeof(BladedStaff), 16);
                    this.Add(typeof(Pike), 19);
                    this.Add(typeof(DoubleBladedStaff), 17);
                    this.Add(typeof(Lance), 17);
                    this.Add(typeof(CrescentBlade), 18);
                }

                this.Add(typeof(Spear), 15);
                this.Add(typeof(Pitchfork), 9);
                this.Add(typeof(ShortSpear), 11);

                this.Add(typeof(BlackStaff), 11);
                this.Add(typeof(GnarledStaff), 8);
                this.Add(typeof(QuarterStaff), 9);
                this.Add(typeof(ShepherdsCrook), 10);

                this.Add(typeof(SmithHammer), 10);

                this.Add(typeof(Broadsword), 17);
                this.Add(typeof(Cutlass), 12);
                this.Add(typeof(Katana), 16);
                this.Add(typeof(Kryss), 16);
                this.Add(typeof(Longsword), 27);
                this.Add(typeof(Scimitar), 18);
                this.Add(typeof(ThinLongsword), 13);
                this.Add(typeof(VikingSword), 27);
				
            }
        }
    }
}