using System;
using Server.Items;

namespace Server.Engines.Craft
{
    public enum AlchemyRecipes
    {
        BarrabHemolymphConcentrate = 900,
        JukariBurnPoiltice = 901,
        KurakAmbushersEssence = 902,
        BarakoDraftOfMight = 903,
        UraliTranceTonic = 904,
        SakkhraProphylaxisPotion = 905,
    }

    public class DefAlchemy : CraftSystem
    {
        public override SkillName MainSkill
        {
            get
            {
                return SkillName.Alchemy;
            }
        }

        public override int GumpTitleNumber
        {
            get
            {
                return 1044001;
            }// <CENTER>ALCHEMY MENU</CENTER>
        }

        private static CraftSystem m_CraftSystem;

        public static CraftSystem CraftSystem
        {
            get
            {
                if (m_CraftSystem == null)
                    m_CraftSystem = new DefAlchemy();

                return m_CraftSystem;
            }
        }

        public override double GetChanceAtMin(CraftItem item)
        {
            return 0.0; // 0%
        }

        private DefAlchemy()
            : base(3, 7, 0.5)// base( 1, 1, 3.1 )
        {
        }

        public override int CanCraft(Mobile from, ITool tool, Type itemType)
        {
            int num = 0;

            if (tool == null || tool.Deleted || tool.UsesRemaining <= 0)
                return 1044038; // You have worn out your tool!
            else if (!tool.CheckAccessible(from, ref num))
                return num; // The tool must be on your person to use.
            IPooledEnumerable eable = from.Map.GetItemsInRange(from.Location, 2);

            foreach (Item item in eable)
            {
				if( item is AlchemyStation )
					return 0;
            }			

            return 1044038;
        }

        public override void PlayCraftEffect(Mobile from)
        {
            from.PlaySound(0x242);
        }

        private static readonly Type typeofPotion = typeof(BasePotion);

        public static bool IsPotion(Type type)
        {
            return typeofPotion.IsAssignableFrom(type);
        }
		
        public override int PlayEndingEffect(Mobile from, bool failed, bool lostMaterial, bool toolBroken, int quality, bool makersMark, CraftItem item)
        {
            if (toolBroken)
                from.SendLocalizedMessage(1044038); // You have worn out your tool

			CraftSkillCheck( from, item.ItemType, MainSkill );
            if (failed)
            {
                if (IsPotion(item.ItemType))
                {
                    from.AddToBackpack(new Bottle());
                    return 500287; // You fail to create a useful potion.
                }
                else
                {
                    return 1044043; // You failed to create the item, and some of your materials are lost.
                }
            }
            else
            {
                from.PlaySound(0x240); // Sound of a filling bottle

                if (IsPotion(item.ItemType))
                {
                    if (quality == -1)
                        return 1048136; // You create the potion and pour it into a keg.
                    else
                        return 500279; // You pour the potion into a bottle...
                }
                else
                {
                    return 1044154; // You create the item.
                }
            }
        }

        public override void InitCraftList()
        {
            int index = -1;

            //체력 회복 물약
            //index = AddCraft(typeof(LesserHealPotion), 1116348, 1044543, -25.0, 25.0, typeof(Ginseng), 1044356, 1, 1044364);
            //AddRes(index, typeof(Bottle), 1044529, 1, 500315);
            index = AddCraft(typeof(MiniHealPotion), "체력 회복 물약", "최하급 체력 회복 물약", -25.0, 25.0, typeof(Bloodmoss), 1044354, 1, 1044361);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(LesserHealPotion), "체력 회복 물약", "하급 체력 회복 물약", 0.0, 50.0, typeof(Bloodmoss), 1044354, 2, 1044361);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(HealPotion), "체력 회복 물약", "체력 회복 물약", 25.0, 75.0, typeof(Bloodmoss), 1044354, 3, 1044361);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(GreaterHealPotion), "체력 회복 물약", "상급 체력 회복 물약", 50.0, 100.0, typeof(Bloodmoss), 1044354, 6, 1044361);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(TotalHealPotion), "체력 회복 물약", "최상급 체력 회복 물약", 75.0, 125.0, typeof(Bloodmoss), 1044354, 10, 1044361);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);
			
			//기력 회복 물약
            index = AddCraft(typeof(MiniRefreshPotion), "기력 회복 물약", "최하급 기력 회복 물약", -25, 25.0, typeof(Ginseng), 1044356, 1, 1044361);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(LesserRefreshPotion), "기력 회복 물약", "하급 기력 회복 물약", 0.0, 50.0, typeof(Ginseng), 1044356, 2, 1044361);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(RefreshPotion), "기력 회복 물약", "기력 회복 물약", 25, 75.0, typeof(Ginseng), 1044356, 3, 1044361);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(GreaterRefreshPotion), "기력 회복 물약", "상급 기력 회복 물약", 50.0, 100.0, typeof(Ginseng), 1044356, 6, 1044361);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(TotalRefreshPotion), "기력 회복 물약", "최상급 기력 회복 물약", 75.0, 125.0, typeof(Ginseng), 1044356, 10, 1044361);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

			//마나 회복 물약
            index = AddCraft(typeof(MiniAgilityPotion), "마나 회복 물약", "최하급 마나 회복 물약", -25.0, 25.0, typeof(MandrakeRoot), 1044357, 1, 1044361);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(LesserAgilityPotion), "마나 회복 물약", "하급 마나 회복 물약", 0.0, 50.0, typeof(MandrakeRoot), 1044357, 2, 1044361);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(AgilityPotion), "마나 회복 물약", "마나 회복 물약", 25.0, 75.0, typeof(MandrakeRoot), 1044357, 3, 1044361);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(GreaterAgilityPotion), "마나 회복 물약", "상급 마나 회복 물약", 50.0, 100.0, typeof(MandrakeRoot), 1044357, 6, 1044361);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(TotalAgilityPotion), "마나 회복 물약", "최상급 마나 회복 물약", 75.0, 125.0, typeof(MandrakeRoot), 1044357, 10, 1044361);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

			//해독 물약
            index = AddCraft(typeof(MiniCurePotion), "해독 물약", "최하급 해독 물약", -20.0, 30.0, typeof(Garlic), 1044355, 1, 1044363);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(LesserCurePotion), "해독 물약", "하급 해독 물약", 5.0, 55.0, typeof(Garlic), 1044355, 2, 1044363);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(CurePotion), "해독 물약", "해독 물약", 30.0, 80.0, typeof(Garlic), 1044355, 3, 1044363);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(GreaterCurePotion), "해독 물약", "상급 해독 물약", 55.0, 105.0, typeof(Garlic), 1044355, 6, 1044363);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(TotalCurePotion), "해독 물약", "최상급 해독 물약", 80.0, 130.0, typeof(Garlic), 1044355, 10, 1044363);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            //독 물약
            index = AddCraft(typeof(MiniPoisonPotion), "독 물약", "최하급 독 물약", -10.0, 40.0, typeof(Nightshade), 1044358, 1, 1044366);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(LesserPoisonPotion), "독 물약", "하급 독 물약", 15.0, 65.0, typeof(Nightshade), 1044358, 2, 1044366);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(PoisonPotion), "독 물약", "독 물약", 40.0, 90.0, typeof(Nightshade), 1044358, 3, 1044366);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(GreaterPoisonPotion), "독 물약", "상급 독 물약", 65.0, 115.0, typeof(Nightshade), 1044358, 6, 1044366);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(DeadlyPoisonPotion), "독 물약", "최상급 해독 물약", 90.0, 140.0, typeof(Nightshade), 1044358, 10, 1044366);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            // 폭발 물약
            index = AddCraft(typeof(MiniExplosionPotion), "폭발 물약", "최하급 폭발 물약", -15.0, 35.0, typeof(SulfurousAsh), 1044359, 1, 1044367);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(LesserExplosionPotion), "폭발 물약", "하급 폭발 물약", 10.0, 65.0, typeof(SulfurousAsh), 1044359, 2, 1044367);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(ExplosionPotion), "폭발 물약", "폭발 물약", 35.0, 85.0, typeof(SulfurousAsh), 1044359, 3, 1044367);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(GreaterExplosionPotion), "폭발 물약", "상급 폭발 물약", 60.0, 110.0, typeof(SulfurousAsh), 1044359, 6, 1044367);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(TotalExplosionPotion), "폭발 물약", "최상급 폭발 물약", 85.0, 135.0, typeof(SulfurousAsh), 1044359, 10, 1044367);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            // Enhancement
			//방어 물약
            index = AddCraft(typeof(NightSightPotion), "방어 물약", "최하급 방어 물약", -5.0, 45.0, typeof(SpidersSilk), 1044360, 1, 1044368);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(NightSightPotion), "방어 물약", "하급 방어 물약", 20.0, 70.0, typeof(SpidersSilk), 1044360, 2, 1044368);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(NightSightPotion), "방어 물약", "방어 물약", 45.0, 95.0, typeof(SpidersSilk), 1044360, 3, 1044368);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(NightSightPotion), "방어 물약", "상급 방어 물약", 70.0, 120.0, typeof(SpidersSilk), 1044360, 6, 1044368);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(NightSightPotion), "방어 물약", "최상급 방어 물약", 95.0, 140.0, typeof(SpidersSilk), 1044360, 10, 1044368);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

			//파워 물약
            index = AddCraft(typeof(MiniStrengthPotion), "파워 물약", "최하급 파워 물약", -5.0, 45.0, typeof(BlackPearl), 1044353, 1, 1044365);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(LesserStrengthPotion), "파워 물약", "하급 파워 물약", 20.0, 70.0, typeof(BlackPearl), 1044353, 2, 1044365);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(StrengthPotion), "파워 물약", "파워 물약", 45.0, 95.0, typeof(BlackPearl), 1044353, 3, 1044365);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(GreaterStrengthPotion), "파워 물약", "상급 파워 물약", 70.0, 120.0, typeof(BlackPearl), 1044353, 6, 1044365);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(TotalStrengthPotion), "파워 물약", "최상급 파워 물약", 95.0, 140.0, typeof(BlackPearl), 1044353, 10, 1044365);
            AddRes(index, typeof(Bottle), 1044529, 1, 500315);
			SetUseAllRes(index, true);

            index = AddCraft(typeof(Bottle), 1077435, 1096926, 0.0, 50.0, typeof(Sand), 1124449, 10, 1044627);
			SetUseAllRes(index, true);
			
            index = AddCraft(typeof(HairRestylingDeed), 1077435, 1041061, 172.5, 222.5, typeof(ParasiticPlant), 1032688, 750, 502774);
		
			/*
            if (Core.SA)
            {
                index = AddCraft(typeof(ElixirOfRebirth), 1116348, 1112762, 65.0, 115.0, typeof(MedusaBlood), 1031702, 1, 1044253);
                AddRes(index, typeof(SpidersSilk), 1044360, 3, 1044368);
                AddRes(index, typeof(Bottle), 1044529, 1, 500315);
            }

            if (Core.TOL)
            {
                index = AddCraft(typeof(BarrabHemolymphConcentrate), 1116348, 1156724, 51.0, 151.0, typeof(Bottle), 1044529, 1, 500315);
                AddRes(index, typeof(Ginseng), 1044356, 20, 1044364);
                AddRes(index, typeof(PlantClippings), 1112131, 5, 1044253);
                AddRes(index, typeof(MyrmidexEggsac), 1156725, 5, 1044253);
                AddRecipe(index, (int)AlchemyRecipes.BarrabHemolymphConcentrate);
            }
			*/
            //index = AddCraft(typeof(AgilityPotion), 1116349, 1044540, 15.0, 65.0, typeof(Bloodmoss), 1044354, 1, 1044362);
            //AddRes(index, typeof(Bottle), 1044529, 1, 500315);

            //index = AddCraft(typeof(GreaterAgilityPotion), 1116349, 1044541, 35.0, 85.0, typeof(Bloodmoss), 1044354, 3, 1044362);
            //AddRes(index, typeof(Bottle), 1044529, 1, 500315);


            //index = AddCraft(typeof(StrengthPotion), 1116349, 1044546, 25.0, 75.0, typeof(MandrakeRoot), 1044357, 2, 1044365);
            //AddRes(index, typeof(Bottle), 1044529, 1, 500315);

            //index = AddCraft(typeof(GreaterStrengthPotion), 1116349, 1044547, 45.0, 95.0, typeof(MandrakeRoot), 1044357, 5, 1044365);
            //AddRes(index, typeof(Bottle), 1044529, 1, 500315);

			/*
            if (Core.ML)
            {
                index = AddCraft(typeof(InvisibilityPotion), 1116349, 1074860, 65.0, 115.0, typeof(Bottle), 1044529, 1, 500315);
                AddRes(index, typeof(Bloodmoss), 1044354, 4, 1044362);
                AddRes(index, typeof(Nightshade), 1044358, 3, 1044366);
                AddRecipe(index, (int)TinkerRecipes.InvisibilityPotion);
            }
            
            if (Core.TOL)
            {
                index = AddCraft(typeof(JukariBurnPoiltice), 1116349, 1156726, 51.0, 151.0, typeof(Bottle), 1044529, 1, 500315);
                AddRes(index, typeof(BlackPearl), 1044353, 20, 1044361);
                AddRes(index, typeof(Vanilla), 1080000, 10, 1080008);
                AddRes(index, typeof(LavaBerry), 1156727, 5, 1044253);
                AddRecipe(index, (int)AlchemyRecipes.JukariBurnPoiltice);

                index = AddCraft(typeof(KurakAmbushersEssence), 1116349, 1156728, 51.0, 151.0, typeof(Bottle), 1044529, 1, 500315);
                AddRes(index, typeof(Bloodmoss), 1044354, 20, 1044362);
                AddRes(index, typeof(BlueDiamond), 1032696, 1, 1044253);
                AddRes(index, typeof(TigerPelt), 1156727, 10, 1044253);
                AddRecipe(index, (int)AlchemyRecipes.KurakAmbushersEssence);

                index = AddCraft(typeof(BarakoDraftOfMight), 1116349, 1156729, 51.0, 151.0, typeof(Bottle), 1044529, 1, 500315);
                AddRes(index, typeof(SpidersSilk), 1044360, 20, 1044368);
                AddRes(index, typeof(BaseBeverage), 1022459, 10, 1044253);
                AddRes(index, typeof(PerfectBanana), 1156730, 5, 1044253);
                SetBeverageType(index, BeverageType.Liquor);
                AddRecipe(index, (int)AlchemyRecipes.BarakoDraftOfMight);

                index = AddCraft(typeof(UraliTranceTonic), 1116349, 1156734, 51.0, 151.0, typeof(Bottle), 1044529, 1, 500315);
                AddRes(index, typeof(MandrakeRoot), 1044357, 20, 1044365);
                AddRes(index, typeof(YellowScales), 1156799, 10, 1044253);
                AddRes(index, typeof(RiverMoss), 1156731, 5, 1044253);
                AddRecipe(index, (int)AlchemyRecipes.UraliTranceTonic);

                index = AddCraft(typeof(SakkhraProphylaxisPotion), 1116349, 1156732, 51.0, 151.0, typeof(Bottle), 1044529, 1, 500315);
                AddRes(index, typeof(Nightshade), 1044358, 20, 1044366);
                AddRes(index, typeof(BaseBeverage), 1022503, 10, 1044253);
                AddRes(index, typeof(BlueCorn), 1156733, 5, 1044253);
                SetBeverageType(index, BeverageType.Wine);
                AddRecipe(index, (int)AlchemyRecipes.SakkhraProphylaxisPotion);
            }
			*/
			/*
            if (Core.ML)
            {
                index = AddCraft(typeof(ParasiticPotion), 1116350, 1072942, 65.0, 115.0, typeof(Bottle), 1044529, 1, 500315);
                AddRes(index, typeof(ParasiticPlant), 1073474, 5, 1044253);
                AddRecipe(index, (int)TinkerRecipes.ParasiticPotion);

                index = AddCraft(typeof(DarkglowPotion), 1116350, 1072943, 65.0, 115.0, typeof(Bottle), 1044529, 1, 500315);
                AddRes(index, typeof(LuminescentFungi), 1073475, 5, 1044253);
                AddRecipe(index, (int)TinkerRecipes.DarkglowPotion);

                index = AddCraft(typeof(ScouringToxin), 1116350, 1112292, 75.0, 100.0, typeof(ToxicVenomSac), 1112291, 1, 1044253);
                AddRes(index, typeof(Bottle), 1044529, 1, 500315);
            }

            if (Core.ML)
            {
                index = AddCraft(typeof(ConflagrationPotion), 1116351, 1072096, 55.0, 105.0, typeof(Bottle), 1044529, 1, 500315);
                AddRes(index, typeof(GraveDust), 1023983, 5, 1044253);

                index = AddCraft(typeof(GreaterConflagrationPotion), 1116351, 1072099, 70.0, 120.0, typeof(Bottle), 1044529, 1, 500315);
                AddRes(index, typeof(GraveDust), 1023983, 10, 1044253);

                index = AddCraft(typeof(ConfusionBlastPotion), 1116351, 1072106, 55.0, 105.0, typeof(Bottle), 1044529, 1, 500315);
                AddRes(index, typeof(PigIron), 1023978, 5, 1044253);

                index = AddCraft(typeof(GreaterConfusionBlastPotion), 1116351, 1072109, 70.0, 120.0, typeof(Bottle), 1044529, 1, 500315);
                AddRes(index, typeof(PigIron), 1023978, 10, 1044253);
            }

            if (Core.SA)
            {
                index = AddCraft(typeof(BlackPowder), 1116351, 1095826, 65.0, 115.0, typeof(SulfurousAsh), 1023980, 1, 1044253);
                AddRes(index, typeof(Saltpeter), 1116302, 6, 1044253);
                AddRes(index, typeof(Charcoal), 1116303, 1, 1044253);
                if (Core.EJ) SetUseAllRes(index, true);

                // Removed for Dark Tides Cannon Changes
                if (!Core.EJ)
                {
                    index = AddCraft(typeof(Matchcord), 1116351, 1095184, 25.0, 80.0, typeof(DarkYarn), 1023615, 1, 1044253);
                    AddRes(index, typeof(BaseBeverage), 1024088, 1, 1044253);
                    AddRes(index, typeof(Saltpeter), 1116302, 1, 1044253);
                    AddRes(index, typeof(Potash), 1116319, 1, 1044253);
                }

                index = AddCraft(typeof(FuseCord), 1116351, 1116305, 55.0, 105.0, typeof(DarkYarn), 1023615, 1, 1044253);
                AddRes(index, typeof(BlackPowder), 1095826, 1, 1044253);
                AddRes(index, typeof(Potash), 1116319, 1, 1044253);
                SetNeedWater(index, true);
            }

            // Strange Brew
            if (Core.SE)
            {                
                index = AddCraft(typeof(SmokeBomb), 1116353, 1030248, 90.0, 120.0, typeof(Eggs), 1044477, 1, 1044253);
                AddRes(index, typeof(Ginseng), 1044356, 3, 1044364);
            }

            if (Core.ML)
            {
                index = AddCraft(typeof(HoveringWisp), 1116353, 1072881, 75.0, 125.0, typeof(CapturedEssence), 1032686, 4, 1044253);

                if (!Core.TOL) // Removed at OSI Publish 103
                    AddRecipe(index, (int)TinkerRecipes.HoveringWisp);
            }

            if (Core.SA)
            {
                index = AddCraft(typeof(NaturalDye), 1116353, 1112136, 75.0, 100.0, typeof(PlantPigment), 1112132, 1, 1044253);
                AddRes(index, typeof(ColorFixative), 1112135, 1, 1044253);
                SetItemHue(index, 2101);
                SetRequireResTarget(index);

                index = AddCraft(typeof(NexusCore), 1116353, 1153501, 90.0, 120.0, typeof(MandrakeRoot), 1015013, 10, 1044253);
                AddRes(index, typeof(SpidersSilk), 1015007, 10, 1044253);
                AddRes(index, typeof(DarkSapphire), 1032690, 5, 1044253);
                AddRes(index, typeof(CrushedGlass), 1113351, 5, 1044253);
                ForceNonExceptional(index);
            }

            // Ingrediants
            if (Core.SA)
            {
                // Ingrediants
                index = AddCraft(typeof(PlantPigment), 1044495, 1112132, 33.0, 83.0, typeof(PlantClippings), 1112131, 1, 1044253);
                AddRes(index, typeof(Bottle), 1023854, 1, 1044253);
                SetItemHue(index, 2101);
                SetRequireResTarget(index);

                index = AddCraft(typeof(ColorFixative), 1044495, 1112135, 75.0, 100.0, typeof(SilverSerpentVenom), 1112173, 1, 1044253);
                AddRes(index, typeof(BaseBeverage), 1022503, 1, 1044253);
                SetBeverageType(index, BeverageType.Wine);

                index = AddCraft(typeof(CrystalGranules), 1044495, 1112329, 75.0, 100.0, typeof(ShimmeringCrystals), 1075095, 1, 1044253);
                SetItemHue(index, 2625);

                index = AddCraft(typeof(CrystalDust), 1044495, 1112328, 75.0, 100.0, typeof(CrystallineFragments), 1153988, 4, 1044253);
                SetItemHue(index, 2103);               

                index = AddCraft(typeof(SoftenedReeds), 1044495, 1112249, 75.0, 100.0, typeof(DryReeds), 1112248, 1, 1112250);
                AddRes(index, typeof(ScouringToxin), 1112292, 2, 1112326);
                SetRequireResTarget(index);
                SetRequiresBasketWeaving(index);

                index = AddCraft(typeof(VialOfVitriol), 1044495, 1113331, 90.0, 100.0, typeof(ParasiticPotion), 1072848, 1, 1113754);
                AddRes(index, typeof (Nightshade), 1044358, 30, 1044366);
                AddSkill(index, SkillName.Magery, 75.0, 100.0);

                index = AddCraft(typeof(BottleIchor), 1044495, 1113361, 90.0, 100.0, typeof(DarkglowPotion), 1072849, 1, 1113755);
                AddRes(index, typeof(SpidersSilk), 1044360, 1, 1044368);
                AddSkill(index, SkillName.Magery, 75.0, 100.0);
            }

            if (Core.HS)
            {
                index = AddCraft(typeof(Potash), 1044495, 1116319, 0.0, 50.0, typeof(Board), 1044041, 1, 1044253);

                if (Core.EJ)
                {
                    SetNeedWater(index, true);
                    SetUseAllRes(index, true);
                }
                else
                {
                    AddRes(index, typeof(BaseBeverage), 1024088, 1, 1044253);
                }
            }

            if (Core.SA)
            {
                index = AddCraft(typeof(GoldDust), 1044495, 1153504, 90.0, 120.0, typeof(Gold), 3000083, 1000, 1150747);
                ForceNonExceptional(index);
            }
			*/
        }
    }
}
