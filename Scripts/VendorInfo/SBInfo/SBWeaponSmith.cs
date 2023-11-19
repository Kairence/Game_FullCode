using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles 
{ 
    public class SBWeaponSmith : SBInfo 
    { 
        private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
        public SBWeaponSmith() 
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
                Add(new GenericBuyInfo(typeof(BlackStaff), 100, 20, 0xDF1, 0));
                Add(new GenericBuyInfo(typeof(Club), 100, 20, 0x13B4, 0));
                Add(new GenericBuyInfo(typeof(GnarledStaff), 100, 20, 0x13F8, 0));
                Add(new GenericBuyInfo(typeof(Mace), 100, 20, 0xF5C, 0));
                Add(new GenericBuyInfo(typeof(Maul), 100, 20, 0x143B, 0));
                Add(new GenericBuyInfo(typeof(QuarterStaff), 100, 20, 0xE89, 0));
                Add(new GenericBuyInfo(typeof(ShepherdsCrook), 100, 20, 0xE81, 0));
                Add(new GenericBuyInfo(typeof(SmithHammer), 100, 20, 0x13E3, 0));
                Add(new GenericBuyInfo(typeof(ShortSpear), 100, 20, 0x1403, 0));
                Add(new GenericBuyInfo(typeof(Spear), 100, 20, 0xF62, 0));
                Add(new GenericBuyInfo(typeof(WarHammer), 100, 20, 0x1439, 0));
                Add(new GenericBuyInfo(typeof(WarMace), 100, 20, 0x1407, 0));

                if (Core.AOS)
                {
                    Add(new GenericBuyInfo(typeof(Scepter), 100, 20, 0x26BC, 0));
                    Add(new GenericBuyInfo(typeof(BladedStaff), 100, 20, 0x26BD, 0));
                }

                Add(new GenericBuyInfo(typeof(Hatchet), 100, 20, 0xF44, 0));
                Add(new GenericBuyInfo(typeof(Hatchet), 100, 20, 0xF43, 0));
                Add(new GenericBuyInfo(typeof(WarFork), 100, 20, 0x1405, 0));

                switch ( Utility.Random(3)) 
                { 
                    case 0:
                        {
                            Add(new GenericBuyInfo(typeof(ExecutionersAxe), 50, 20, 0xF45, 0));
                            Add(new GenericBuyInfo(typeof(Bardiche), 60, 20, 0xF4D, 0));
                            Add(new GenericBuyInfo(typeof(BattleAxe), 50, 20, 0xF47, 0));
                            Add(new GenericBuyInfo(typeof(TwoHandedAxe), 50, 20, 0x1443, 0));

                            Add(new GenericBuyInfo(typeof(Bow), 50, 20, 0x13B2, 0));

                            Add(new GenericBuyInfo(typeof(ButcherKnife), 50, 20, 0x13F6, 0));

                            Add(new GenericBuyInfo(typeof(Crossbow), 50, 20, 0xF50, 0));
                            Add(new GenericBuyInfo(typeof(HeavyCrossbow), 55, 20, 0x13FD, 0));

                            Add(new GenericBuyInfo(typeof(Cutlass), 50, 20, 0x1441, 0));
                            Add(new GenericBuyInfo(typeof(Dagger), 50, 20, 0xF52, 0));
                            Add(new GenericBuyInfo(typeof(Halberd), 50, 20, 0x143E, 0));

                            Add(new GenericBuyInfo(typeof(HammerPick), 50, 20, 0x143D, 0));

                            Add(new GenericBuyInfo(typeof(Katana), 50, 20, 0x13FF, 0));
                            Add(new GenericBuyInfo(typeof(Kryss), 50, 20, 0x1401, 0));
                            Add(new GenericBuyInfo(typeof(Broadsword), 50, 20, 0xF5E, 0));
                            Add(new GenericBuyInfo(typeof(Longsword), 55, 20, 0xF61, 0));
                            Add(new GenericBuyInfo(typeof(ThinLongsword), 50, 20, 0x13B8, 0));
                            Add(new GenericBuyInfo(typeof(VikingSword), 55, 20, 0x13B9, 0));

                            Add(new GenericBuyInfo(typeof(Cleaver), 50, 20, 0xEC3, 0));
                            Add(new GenericBuyInfo(typeof(Axe), 50, 20, 0xF49, 0));
                            Add(new GenericBuyInfo(typeof(DoubleAxe), 52, 20, 0xF4B, 0));
                            Add(new GenericBuyInfo(typeof(Pickaxe), 50, 20, 0xE86, 0));

                            Add(new GenericBuyInfo(typeof(Pitchfork), 50, 20, 0xE87, 0));

                            Add(new GenericBuyInfo(typeof(Scimitar), 50, 20, 0x13B6, 0));

                            Add(new GenericBuyInfo(typeof(SkinningKnife), 50, 20, 0xEC4, 0));

                            Add(new GenericBuyInfo(typeof(LargeBattleAxe), 50, 20, 0x13FB, 0));
                            Add(new GenericBuyInfo(typeof(WarAxe), 50, 20, 0x13B0, 0));

                            if (Core.AOS)
                            {
                                Add(new GenericBuyInfo(typeof(BoneHarvester), 50, 20, 0x26BB, 0));
                                Add(new GenericBuyInfo(typeof(CrescentBlade), 50, 20, 0x26C1, 0));
                                Add(new GenericBuyInfo(typeof(DoubleBladedStaff), 50, 20, 0x26BF, 0));
                                Add(new GenericBuyInfo(typeof(Lance), 50, 20, 0x26C0, 0));
                                Add(new GenericBuyInfo(typeof(Pike), 50, 20, 0x26BE, 0));
                                Add(new GenericBuyInfo(typeof(Scythe), 50, 20, 0x26BA, 0));
                                Add(new GenericBuyInfo(typeof(CompositeBow), 50, 20, 0x26C2, 0));
                                Add(new GenericBuyInfo(typeof(RepeatingCrossbow), 57, 20, 0x26C3, 0));
                            }

                            break;
                        }
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