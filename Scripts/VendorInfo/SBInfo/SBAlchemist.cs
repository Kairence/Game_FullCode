using System;
using System.Collections.Generic;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class SBAlchemist : SBInfo
    {
        private readonly List<GenericBuyInfo> m_BuyInfo;
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();

        public SBAlchemist(Mobile m)
        {
            if (m != null)
            {
                m_BuyInfo = new InternalBuyInfo(m);
            }
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
            public InternalBuyInfo(Mobile m)
            {
                Add(new GenericBuyInfo(typeof(MiniRefreshPotion), 15, 10, 0xF0B, 0));
                Add(new GenericBuyInfo(typeof(MiniAgilityPotion), 15, 10, 0xF08, 0));
                Add(new GenericBuyInfo(typeof(MiniNightSightPotion), 15, 10, 0xF06, 0));
                Add(new GenericBuyInfo(typeof(MiniHealPotion), 15, 10, 0xF0C, 0));
                Add(new GenericBuyInfo(typeof(MiniStrengthPotion), 15, 10, 0xF09, 0));
                Add(new GenericBuyInfo(typeof(MiniPoisonPotion), 15, 10, 0xF0A, 0));
                Add(new GenericBuyInfo(typeof(MiniCurePotion), 15, 10, 0xF07, 0));
                Add(new GenericBuyInfo(typeof(MiniExplosionPotion), 15, 10, 0xF0D, 0));

				
                Add(new GenericBuyInfo(typeof(LesserRefreshPotion), 25, 10, 0xF0B, 0));
                Add(new GenericBuyInfo(typeof(LesserAgilityPotion), 25, 10, 0xF08, 0));
                Add(new GenericBuyInfo(typeof(LesserNightSightPotion), 25, 10, 0xF06, 0));
                Add(new GenericBuyInfo(typeof(LesserHealPotion), 25, 10, 0xF0C, 0));
                Add(new GenericBuyInfo(typeof(LesserStrengthPotion), 25, 10, 0xF09, 0));
                Add(new GenericBuyInfo(typeof(LesserPoisonPotion), 25, 10, 0xF0A, 0));
                Add(new GenericBuyInfo(typeof(LesserCurePotion), 25, 10, 0xF07, 0));
                Add(new GenericBuyInfo(typeof(LesserExplosionPotion), 25, 10, 0xF0D, 0));

                Add(new GenericBuyInfo(typeof(RefreshPotion), 40, 10, 0xF0B, 0));
                Add(new GenericBuyInfo(typeof(AgilityPotion), 40, 10, 0xF08, 0));
                Add(new GenericBuyInfo(typeof(NightSightPotion), 40, 10, 0xF06, 0));
                Add(new GenericBuyInfo(typeof(HealPotion), 40, 10, 0xF0C, 0));
                Add(new GenericBuyInfo(typeof(StrengthPotion), 40, 10, 0xF09, 0));
                Add(new GenericBuyInfo(typeof(PoisonPotion), 40, 10, 0xF0A, 0));
                Add(new GenericBuyInfo(typeof(CurePotion), 40, 10, 0xF07, 0));
                Add(new GenericBuyInfo(typeof(ExplosionPotion), 40, 10, 0xF0D, 0));

                Add(new GenericBuyInfo(typeof(GreaterRefreshPotion), 75, 10, 0xF0B, 0));
                Add(new GenericBuyInfo(typeof(GreaterAgilityPotion), 75, 10, 0xF08, 0));
                Add(new GenericBuyInfo(typeof(GreaterNightSightPotion), 75, 10, 0xF06, 0));
                Add(new GenericBuyInfo(typeof(GreaterHealPotion), 75, 10, 0xF0C, 0));
                Add(new GenericBuyInfo(typeof(GreaterStrengthPotion), 75, 10, 0xF09, 0));
                Add(new GenericBuyInfo(typeof(GreaterPoisonPotion), 75, 10, 0xF0A, 0));
                Add(new GenericBuyInfo(typeof(GreaterCurePotion), 75, 10, 0xF07, 0));
                Add(new GenericBuyInfo(typeof(GreaterExplosionPotion), 75, 10, 0xF0D, 0));

                Add(new GenericBuyInfo(typeof(TotalRefreshPotion), 140, 10, 0xF0B, 0));
                Add(new GenericBuyInfo(typeof(TotalAgilityPotion), 140, 10, 0xF08, 0));
                Add(new GenericBuyInfo(typeof(TotalNightSightPotion), 140, 10, 0xF06, 0));
                Add(new GenericBuyInfo(typeof(TotalHealPotion), 140, 10, 0xF0C, 0));
                Add(new GenericBuyInfo(typeof(TotalStrengthPotion), 140, 10, 0xF09, 0));
                Add(new GenericBuyInfo(typeof(DeadlyPoisonPotion), 140, 10, 0xF0A, 0));
                Add(new GenericBuyInfo(typeof(TotalCurePotion), 140, 10, 0xF07, 0));
                Add(new GenericBuyInfo(typeof(TotalExplosionPotion), 140, 10, 0xF0D, 0));
				
                Add(new GenericBuyInfo(typeof(Bottle), 10, 100, 0xF0E, 0)); 

                Add(new GenericBuyInfo(typeof(MortarPestle), 50, 10, 0xE9B, 0));

				Add(new GenericBuyInfo(typeof(HairDye), 500, 10, 0xEFF, 0));

                Add(new GenericBuyInfo(typeof(BlackPearl), 8, 20, 0xF7A, 0));
                Add(new GenericBuyInfo(typeof(Bloodmoss), 8, 20, 0xF7B, 0));
                Add(new GenericBuyInfo(typeof(Garlic), 8, 20, 0xF84, 0));
                Add(new GenericBuyInfo(typeof(Ginseng), 8, 20, 0xF85, 0));
                Add(new GenericBuyInfo(typeof(MandrakeRoot), 8, 20, 0xF86, 0));
                Add(new GenericBuyInfo(typeof(Nightshade), 8, 20, 0xF88, 0));
                Add(new GenericBuyInfo(typeof(SpidersSilk), 8, 20, 0xF8D, 0));
                Add(new GenericBuyInfo(typeof(SulfurousAsh), 8, 20, 0xF8C, 0));
                Add(new GenericBuyInfo(typeof(Sand), 10, 20, 0x423A, 0));

                //Add(new GenericBuyInfo(typeof(HeatingStand), 5, 100, 0x1849, 0));
                //Add(new GenericBuyInfo(typeof(SkinTingeingTincture), 1255, 20, 0xEFF, 90));

				/*
                if (m.Map != Map.TerMur)
                {
                    Add(new GenericBuyInfo(typeof(HairDye), 48, 10, 0xEFF, 0));
                }
                else if (m is Zosilem)
                {
                    Add(new GenericBuyInfo(typeof(GlassblowingBook), 10637, 30, 0xFF4, 0));
                    Add(new GenericBuyInfo(typeof(SandMiningBook), 10637, 30, 0xFF4, 0));
                    Add(new GenericBuyInfo(typeof(Blowpipe), 21, 100, 0xE8A, 0x3B9));
                }
				*/
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
				
                Add(typeof(MiniRefreshPotion), 5 ); // 10, 10, 0xF0B, 0, true));
                Add(typeof(MiniAgilityPotion), 5 ); // 10, 10, 0xF08, 0, true));
                Add(typeof(MiniNightSightPotion), 5 ); // 10, 10, 0xF06, 0, true));
                Add(typeof(MiniHealPotion), 5 ); // 10, 10, 0xF0C, 0, true));
                Add(typeof(MiniStrengthPotion), 5 ); // 10, 10, 0xF09, 0, true));
                Add(typeof(MiniPoisonPotion), 5 ); // 10, 10, 0xF0A, 0, true));
                Add(typeof(MiniCurePotion), 5 ); // 10, 10, 0xF07, 0, true));
                Add(typeof(MiniExplosionPotion), 5 ); // 10, 10, 0xF0D, 0, true));

                Add(typeof(LesserRefreshPotion), 5 ); // 30, 10, 0xF0B, 0, true));
                Add(typeof(LesserAgilityPotion), 5 ); // 30, 10, 0xF08, 0, true));
                Add(typeof(LesserNightSightPotion), 5 ); // 30, 10, 0xF06, 0, true));
                Add(typeof(LesserHealPotion), 5 ); // 30, 10, 0xF0C, 0, true));
                Add(typeof(LesserStrengthPotion), 5 ); // 30, 10, 0xF09, 0, true));
                Add(typeof(LesserPoisonPotion), 5 ); // 30, 10, 0xF0A, 0, true));
                Add(typeof(LesserCurePotion), 5 ); // 30, 10, 0xF07, 0, true));
                Add(typeof(LesserExplosionPotion), 5 ); // 30, 10, 0xF0D, 0, true));

                Add(typeof(RefreshPotion), 5 ); // 80, 10, 0xF0B, 0, true));
                Add(typeof(AgilityPotion), 5 ); // 80, 10, 0xF08, 0, true));
                Add(typeof(NightSightPotion), 5 ); // 80, 10, 0xF06, 0, true));
                Add(typeof(HealPotion), 5 ); // 80, 10, 0xF0C, 0, true));
                Add(typeof(StrengthPotion), 5 ); // 80, 10, 0xF09, 0, true));
                Add(typeof(PoisonPotion), 5 ); // 80, 10, 0xF0A, 0, true));
                Add(typeof(CurePotion), 5 ); // 80, 10, 0xF07, 0, true));
                Add(typeof(ExplosionPotion), 5 ); // 80, 10, 0xF0D, 0, true));

                Add(typeof(GreaterRefreshPotion), 5 ); // 155, 10, 0xF0B, 0, true));
                Add(typeof(GreaterAgilityPotion), 5 ); // 155, 10, 0xF08, 0, true));
                Add(typeof(GreaterNightSightPotion), 5 ); // 155, 10, 0xF06, 0, true));
                Add(typeof(GreaterHealPotion), 5 ); // 155, 10, 0xF0C, 0, true));
                Add(typeof(GreaterStrengthPotion), 5 ); // 155, 10, 0xF09, 0, true));
                Add(typeof(GreaterPoisonPotion), 5 ); // 155, 10, 0xF0A, 0, true));
                Add(typeof(GreaterCurePotion), 5 ); // 155, 10, 0xF07, 0, true));
                Add(typeof(GreaterExplosionPotion), 5 ); // 155, 10, 0xF0D, 0, true));

                Add(typeof(TotalRefreshPotion), 5 ); // 305, 10, 0xF0B, 0, true));
                Add(typeof(TotalAgilityPotion), 5 ); // 305, 10, 0xF08, 0, true));
                Add(typeof(TotalNightSightPotion), 5 ); // 305, 10, 0xF06, 0, true));
                Add(typeof(TotalHealPotion), 5 ); // 305, 10, 0xF0C, 0, true));
                Add(typeof(TotalStrengthPotion), 5 ); // 305, 10, 0xF09, 0, true));
                Add(typeof(DeadlyPoisonPotion), 5 ); // 305, 10, 0xF0A, 0, true));
                Add(typeof(TotalCurePotion), 5 ); // 305, 10, 0xF07, 0, true));
                Add(typeof(TotalExplosionPotion), 5 ); // 305, 10, 0xF0D, 0, true));
				
                Add(typeof(Bottle), 5 ); // 5, 100, 0xF0E, 0, true)); 
				
                Add(typeof(MortarPestle), 5);
                Add(typeof(HairDye), 16);
				
                Add(typeof(BlackPearl), 16);
				Add(typeof(Bloodmoss), 16);
				Add(typeof(Garlic), 16);
				Add(typeof(Ginseng), 16);
				Add(typeof(MandrakeRoot), 16);
				Add(typeof(Nightshade), 16);
				Add(typeof(SpidersSilk), 16);
				Add(typeof(SulfurousAsh), 16);
				Add(typeof(Sand), 16);
           }
        }
    }
}