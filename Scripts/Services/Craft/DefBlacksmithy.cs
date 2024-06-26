#region References
using System;

using Server.Items;
#endregion

namespace Server.Engines.Craft
{

    #region Mondain's Legacy
    public enum SmithRecipes
    {
        // magical
        TrueSpellblade = 300,
        IcySpellblade = 301,
        FierySpellblade = 302,
        SpellbladeOfDefense = 303,
        TrueAssassinSpike = 304,
        ChargedAssassinSpike = 305,
        MagekillerAssassinSpike = 306,
        WoundingAssassinSpike = 307,
        TrueLeafblade = 308,
        Luckblade = 309,
        MagekillerLeafblade = 310,
        LeafbladeOfEase = 311,
        KnightsWarCleaver = 312,
        ButchersWarCleaver = 313,
        SerratedWarCleaver = 314,
        TrueWarCleaver = 315,
        AdventurersMachete = 316,
        OrcishMachete = 317,
        MacheteOfDefense = 318,
        DiseasedMachete = 319,
        Runesabre = 320,
        MagesRuneBlade = 321,
        RuneBladeOfKnowledge = 322,
        CorruptedRuneBlade = 323,
        TrueRadiantScimitar = 324,
        DarkglowScimitar = 325,
        IcyScimitar = 326,
        TwinklingScimitar = 327,
        GuardianAxe = 328,
        SingingAxe = 329,
        ThunderingAxe = 330,
        HeavyOrnateAxe = 331,
        RubyMace = 332, //good
        EmeraldMace = 333, //good
        SapphireMace = 334, //good
        SilverEtchedMace = 335, //good
        BoneMachete = 336,

        // arties
        RuneCarvingKnife = 350,
        ColdForgedBlade = 351,
        OverseerSunderedBlade = 352,
        LuminousRuneBlade = 353,
        ShardTrasher = 354, //good

        // doom 
        BritchesOfWarding = 355,
        GlovesOfFeudalGrip = 356
    }
    #endregion

    public class DefBlacksmithy : CraftSystem
    {
        public override SkillName MainSkill { get { return SkillName.Blacksmith; } }

        public override int GumpTitleNumber
        {
            get { return 1044002; } // <CENTER>BLACKSMITHY MENU</CENTER>
        }

        private static CraftSystem m_CraftSystem;

        public static CraftSystem CraftSystem { get { return m_CraftSystem ?? (m_CraftSystem = new DefBlacksmithy()); } }

        public override CraftECA ECA { get { return CraftECA.ChanceMinusSixtyToFourtyFive; } }

        public override double GetChanceAtMin(CraftItem item)
        {
            if (item.NameNumber == 1157349 || item.NameNumber == 1157345) // Gloves Of FeudalGrip and Britches Of Warding
                return 0.05; // 5%

            return 0.0; // 0%
        }

        private DefBlacksmithy()
            : base(3, 7, 0.5) // base( 1, 2, 1.7 )
        {
            /*
            base( MinCraftEffect, MaxCraftEffect, Delay )
            MinCraftEffect	: The minimum number of time the mobile will play the craft effect
            MaxCraftEffect	: The maximum number of time the mobile will play the craft effect
            Delay			: The delay between each craft effect
            Example: (3, 6, 1.7) would make the mobile do the PlayCraftEffect override
            function between 3 and 6 time, with a 1.7 second delay each time.
            */
        }

        private static readonly Type typeofAnvil = typeof(AnvilAttribute);
        private static readonly Type typeofForge = typeof(ForgeAttribute);

        public static void CheckAnvilAndForge(Mobile from, int range, out bool anvil, out bool forge)
        {
            anvil = false;
            forge = false;

            Map map = from.Map;

            if (map == null)
            {
                return;
            }

            IPooledEnumerable eable = map.GetItemsInRange(from.Location, range);

            foreach (Item item in eable)
            {
                Type type = item.GetType();

                bool isAnvil = (type.IsDefined(typeofAnvil, false) || item.ItemID == 4015 || item.ItemID == 4016 ||
                                item.ItemID == 0x2DD5 || item.ItemID == 0x2DD6 || (item.ItemID >= 0xA102 && item.ItemID <= 0xA10D));
                bool isForge = (type.IsDefined(typeofForge, false) || item.ItemID == 4017 ||
                                (item.ItemID >= 6522 && item.ItemID <= 6569) || item.ItemID == 0x2DD8) ||
                                item.ItemID == 0xA531 || item.ItemID == 0xA535;

                if (!isAnvil && !isForge)
                {
                    continue;
                }

                if ((from.Z + 16) < item.Z || (item.Z + 16) < from.Z || !from.InLOS(item))
                {
                    continue;
                }

                anvil = anvil || isAnvil;
                forge = forge || isForge;

                if (anvil && forge)
                {
                    break;
                }
            }

            eable.Free();

            for (int x = -range; (!anvil || !forge) && x <= range; ++x)
            {
                for (int y = -range; (!anvil || !forge) && y <= range; ++y)
                {
                    StaticTile[] tiles = map.Tiles.GetStaticTiles(from.X + x, from.Y + y, true);

                    for (int i = 0; (!anvil || !forge) && i < tiles.Length; ++i)
                    {
                        int id = tiles[i].ID;

                        bool isAnvil = (id == 4015 || id == 4016 || id == 0x2DD5 || id == 0x2DD6);
                        bool isForge = (id == 4017 || (id >= 6522 && id <= 6569) || id == 0x2DD8);

                        if (!isAnvil && !isForge)
                        {
                            continue;
                        }

                        if ((from.Z + 16) < tiles[i].Z || (tiles[i].Z + 16) < from.Z ||
                            !from.InLOS(new Point3D(from.X + x, from.Y + y, tiles[i].Z + (tiles[i].Height / 2) + 1)))
                        {
                            continue;
                        }

                        anvil = anvil || isAnvil;
                        forge = forge || isForge;
                    }
                }
            }
			if( !anvil )
				from.SendMessage("모루가 보이지 않습니다.");
			if( !forge )
				from.SendMessage("화로가 보이지 않습니다.");
        }
        public override int CanCraft(Mobile from, ITool tool, Type itemType)
        {
            int num = 0;

            if (tool == null || tool.Deleted || tool.UsesRemaining <= 0)
            {
                return 1044038; // You have worn out your tool!
            }

            if (tool is Item && !BaseTool.CheckTool((Item)tool, from))
            {
                return 1048146; // If you have a tool equipped, you must use that tool.
            }

            else if (!tool.CheckAccessible(from, ref num))
            {
                return num; // The tool must be on your person to use.
            }

            if (tool is AddonToolComponent && from.InRange(((AddonToolComponent)tool).GetWorldLocation(), 2))
            {
                return 0;
            }
			
            IPooledEnumerable eable = from.Map.GetItemsInRange(from.Location, 2);

            foreach (Item item in eable)
            {
				if( item is SmithingPress )
					return 0;
            }			

            bool anvil, forge;
            CheckAnvilAndForge(from, 2, out anvil, out forge);

            if (anvil && forge)
            {
                return 0;
            }

            return 1044267; // You must be near an anvil and a forge to smith items.
        }

        public override void PlayCraftEffect(Mobile from)
        {
            // no animation, instant sound
            //if ( from.Body.Type == BodyType.Human && !from.Mounted )
            //	from.Animate( 9, 5, 1, true, false, 0 );
            //new InternalTimer( from ).Start();
            from.PlaySound(0x2A);
        }

        // Delay to synchronize the sound with the hit on the anvil
        private class InternalTimer : Timer
        {
            private readonly Mobile m_From;

            public InternalTimer(Mobile from)
                : base(TimeSpan.FromSeconds(0.7))
            {
                m_From = from;
            }

            protected override void OnTick()
            {
                m_From.PlaySound(0x2A);
            }
        }

        public override int PlayEndingEffect(
            Mobile from, bool failed, bool lostMaterial, bool toolBroken, int quality, bool makersMark, CraftItem item)
        {
            if (toolBroken)
            {
                from.SendLocalizedMessage(1044038); // You have worn out your tool
            }

			CraftSkillCheck( from, item.ItemType, MainSkill );
            if (failed)
            {
                if (lostMaterial)
                {
                    return 1044043; // You failed to create the item, and some of your materials are lost.
                }

                return 1044157; // You failed to create the item, but no materials were lost.
            }

            if (quality == 0)
            {
                return 502785; // You were barely able to make this item.  It's quality is below average.
            }

            if (makersMark && quality == 2)
            {
                return 1044156; // You create an exceptional quality item and affix your maker's mark.
            }

            if (quality == 2)
            {
                return 1044155; // You create an exceptional quality item.
            }

            return 1044154; // You create the item.
        }

        public override void InitCraftList()
        {
            /*
            Synthax for a SIMPLE craft item
            AddCraft( ObjectType, Group, MinSkill, MaxSkill, ResourceType, Amount, Message )
            ObjectType		: The type of the object you want to add to the build list.
            Group			: The group in wich the object will be showed in the craft menu.
            MinSkill		: The minimum of skill value
            MaxSkill		: The maximum of skill value
            ResourceType	: The type of the resource the mobile need to create the item
            Amount			: The amount of the ResourceType it need to create the item
            Message			: String or Int for Localized.  The message that will be sent to the mobile, if the specified resource is missing.
            Synthax for a COMPLEXE craft item.  A complexe item is an item that need either more than
            only one skill, or more than only one resource.
            Coming soon....
            */

            int index;

            #region Armor
			//갑옷
            AddCraft(typeof(Bascinet), 1131001, 1025132, 8.3, 58.3, typeof(IronIngot), 1044036, 15, 1044037);
            AddCraft(typeof(CloseHelm), 1131001, 1025128, 37.9, 87.9, typeof(IronIngot), 1044036, 15, 1044037);
            AddCraft(typeof(Helmet), 1131001, 1025130, 37.9, 87.9, typeof(IronIngot), 1044036, 15, 1044037);
            AddCraft(typeof(NorseHelm), 1131001, 1025134, 37.9, 87.9, typeof(IronIngot), 1044036, 15, 1044037);
            AddCraft(typeof(PlateHelm), 1131001, 1025138, 62.6, 112.6, typeof(IronIngot), 1044036, 15, 1044037);
			
            AddCraft(typeof(RingmailGloves), 1131001, 1025099, 12.0, 62.0, typeof(IronIngot), 1044036, 10, 1044037);
            AddCraft(typeof(RingmailLegs), 1131001, 1025104, 19.4, 69.4, typeof(IronIngot), 1044036, 16, 1044037);
            AddCraft(typeof(RingmailArms), 1131001, 1025103, 16.9, 66.9, typeof(IronIngot), 1044036, 14, 1044037);
            AddCraft(typeof(RingmailChest), 1131001, 1025100, 21.9, 71.9, typeof(IronIngot), 1044036, 18, 1044037);

            AddCraft(typeof(ChainCoif), 1131001, 1025051, 14.5, 64.5, typeof(IronIngot), 1044036, 10, 1044037);
            AddCraft(typeof(ChainLegs), 1131001, 1025054, 36.7, 86.7, typeof(IronIngot), 1044036, 18, 1044037);
            AddCraft(typeof(ChainChest), 1131001, 1025055, 39.1, 89.1, typeof(IronIngot), 1044036, 20, 1044037);

            AddCraft(typeof(PlateArms), 1131001, 1025136, 66.3, 116.3, typeof(IronIngot), 1044036, 18, 1044037);
            AddCraft(typeof(PlateGloves), 1131001, 1025140, 58.9, 108.9, typeof(IronIngot), 1044036, 12, 1044037);
            AddCraft(typeof(PlateGorget), 1131001, 1025139, 56.4, 106.4, typeof(IronIngot), 1044036, 10, 1044037);
            AddCraft(typeof(PlateLegs), 1131001, 1025137, 68.8, 118.8, typeof(IronIngot), 1044036, 20, 1044037);
            AddCraft(typeof(PlateChest), 1131001, 1046431, 75.0, 125.0, typeof(IronIngot), 1044036, 25, 1044037);
            AddCraft(typeof(FemalePlateChest), 1131001, 1046430, 44.1, 94.1, typeof(IronIngot), 1044036, 20, 1044037);
            #endregion

            #region Shields
            AddCraft(typeof(Buckler), 1131002, 1027027, -25.0, 25.0, typeof(IronIngot), 1044036, 10, 1044037);
            AddCraft(typeof(BronzeShield), 1131002, 1027026, -15.2, 34.8, typeof(IronIngot), 1044036, 12, 1044037);
            AddCraft(typeof(HeaterShield), 1131002, 1027030, 24.3, 74.3, typeof(IronIngot), 1044036, 18, 1044037);
            AddCraft(typeof(MetalShield), 1131002, 1027035, -10.2, 39.8, typeof(IronIngot), 1044036, 14, 1044037);
            AddCraft(typeof(MetalKiteShield), 1131002, 1027028, 4.6, 54.6, typeof(IronIngot), 1044036, 16, 1044037);
            AddCraft(typeof(WoodenKiteShield), 1131002, 1027032, 2.2, 52.2, typeof(IronIngot), 1044036, 8, 1044037);
			AddCraft(typeof(ChaosShield), 1131002, 1027107, 85.0, 135.0, typeof(IronIngot), 1044036, 25, 1044037);
			AddCraft(typeof(OrderShield), 1131002, 1027108, 85.0, 135.0, typeof(IronIngot), 1044036, 25, 1044037);

            #endregion
			
            #region OnehandSword
			AddCraft(typeof(BoneHarvester), 1131003, 1029915, 33.0, 83.0, typeof(IronIngot), 1044036, 10, 1044037);
            AddCraft(typeof(Broadsword), 1131003, 1023934, 35.4, 85.4, typeof(IronIngot), 1044036, 10, 1044037);
            AddCraft(typeof(Cutlass), 1131003, 1025185, 24.3, 74.3, typeof(IronIngot), 1044036, 8, 1044037);
            AddCraft(typeof(Katana), 1131003, 1025119, 44.1, 94.1, typeof(IronIngot), 1044036, 8, 1044037);
            AddCraft(typeof(Longsword), 1131003, 1023937, 28.0, 78.0, typeof(IronIngot), 1044036, 12, 1044037);
            AddCraft(typeof(PaladinSword), 1131003, 1029934, 28.0, 78.0, typeof(IronIngot), 1044036, 25, 1044037);
            AddCraft(typeof(Scimitar), 1131003, 1025046, 31.7, 81.7, typeof(IronIngot), 1044036, 10, 1044037);
            AddCraft(typeof(ThinLongsword), 1131003, 1011412, 40.0, 90.0, typeof(IronIngot), 1044036, 12, 1044037);
            AddCraft(typeof(VikingSword), 1131003, 1025049, 24.3, 74.3, typeof(IronIngot), 1044036, 14, 1044037);
			#endregion

            #region TwohandSword
            AddCraft(typeof(Bardiche), 1131004, 1023917, 31.7, 81.7, typeof(IronIngot), 1044036, 18, 1044037);
			AddCraft(typeof(BladedStaff), 1131004, 1029917, 40.0, 90.0, typeof(IronIngot), 1044036, 12, 1044037);
            AddCraft(typeof(CrescentBlade), 1131004, 1029921, 45.0, 95.0, typeof(IronIngot), 1044036, 14, 1044037);
            AddCraft(typeof(Halberd), 1131004, 1025183, 39.1, 89.1, typeof(IronIngot), 1044036, 20, 1044037);
			AddCraft(typeof(Scythe), 1131004, 1029914, 39.0, 89.0, typeof(IronIngot), 1044036, 14, 1044037);

			#endregion
				
            #region Axe
            AddCraft(typeof(Axe), 1131005, 1023913, 34.2, 84.2, typeof(IronIngot), 1044036, 14, 1044037);
            AddCraft(typeof(BattleAxe), 1131005, 1023911, 30.5, 80.5, typeof(IronIngot), 1044036, 14, 1044037);
            AddCraft(typeof(DoubleAxe), 1131005, 1023915, 29.3, 79.3, typeof(IronIngot), 1044036, 12, 1044037);
            AddCraft(typeof(ExecutionersAxe), 1131005, 1023909, 34.2, 84.2, typeof(IronIngot), 1044036, 14, 1044037);
            AddCraft(typeof(LargeBattleAxe), 1131005, 1025115, 28.0, 78.0, typeof(IronIngot), 1044036, 12, 1044037);
			index = AddCraft(typeof(OrnateAxe), 1131005, 1031572, 70.0, 120.0, typeof(IronIngot), 1044036, 18, 1044037);
            AddCraft(typeof(TwoHandedAxe), 1131005, 1025187, 33.0, 83.0, typeof(IronIngot), 1044036, 16, 1044037);
			#endregion

			#region OnehandMace
            AddCraft(typeof(Mace), 1131006, 1023932, 14.5, 64.5, typeof(IronIngot), 1044036, 7, 1044037);
            AddCraft(typeof(Maul), 1131006, 1025179, 19.4, 69.4, typeof(IronIngot), 1044036, 10, 1044037);

			AddCraft(typeof(Scepter), 1131006, 1029916, 21.4, 71.4, typeof(IronIngot), 1044036, 10, 1044037);
            AddCraft(typeof(WarAxe), 1131006, 1025040, 39.1, 89.1, typeof(IronIngot), 1044036, 16, 1044037);
            AddCraft(typeof(WarMace), 1131006, 1025127, 28.0, 78.0, typeof(IronIngot), 1044036, 14, 1044037);
			#endregion
			
			#region TwohandMace
            AddCraft(typeof(HammerPick), 1131007, 1025181, 34.2, 84.2, typeof(IronIngot), 1044036, 16, 1044037);
            AddCraft(typeof(WarHammer), 1131007, 1025177, 34.2, 84.2, typeof(IronIngot), 1044036, 16, 1044037);
			#endregion
			
			#region OnehandFencing
			index = AddCraft(typeof(AssassinSpike), 1131008, 1031565, 70.0, 120.0, typeof(IronIngot), 1044036, 9, 1044037);
            AddCraft(typeof(Dagger), 1131008, 1023921, -0.4, 49.6, typeof(IronIngot), 1044036, 7, 1044037);
			AddCraft(typeof(Lance), 1131008, 1029920, 48.0, 98.0, typeof(IronIngot), 1044036, 20, 1044037);
            AddCraft(typeof(Kryss), 1131008, 1025121, 36.7, 86.7, typeof(IronIngot), 1044036, 8, 1044037);
            AddCraft(typeof(WarFork), 1131008, 1025125, 42.9, 92.9, typeof(IronIngot), 1044036, 12, 1044037);
			#endregion

			#region TwohandFencing
			AddCraft(typeof(DoubleBladedStaff), 1131009, 1029919, 45.0, 95.0, typeof(IronIngot), 1044036, 16, 1044037);
			index = AddCraft(typeof(ElvenSpellblade), 1131009, 1031564, 70.0, 120.0, typeof(IronIngot), 1044036, 14, 1044037);
			AddCraft(typeof(Pike), 1131009, 1029918, 47.0, 97.0, typeof(IronIngot), 1044036, 12, 1044037);
            AddCraft(typeof(ShortSpear), 1131009, 1025123, 45.3, 95.3, typeof(IronIngot), 1044036, 7, 1044037);
            AddCraft(typeof(Spear), 1131009, 1023938, 49.0, 99.0, typeof(IronIngot), 1044036, 12, 1044037);
			#endregion

            #region Etc
            AddCraft(typeof(MetalKeg), 1131010, 1150675, 85.0, 100.0, typeof(IronIngot), 1044036, 25, 1044253);

			AddCraft(typeof(DragonBardingDeed), 1131010, 1053012, 172.5, 222.5, typeof(IronIngot), 1044036, 750, 1044037);

            if (Core.HS)
            {
                if (Core.EJ)
                {
                    index = AddCraft(typeof(Cannonball), 1131010, 1116029, 10.0, 60.0, typeof(IronIngot), 1044036, 12, 1044037);
                    SetUseAllRes(index, true);
                }
                else
                {
                    AddCraft(typeof(LightCannonball), 1131010, 1116266, 0.0, 50.0, typeof(IronIngot), 1044036, 6, 1044037);
                    AddCraft(typeof(HeavyCannonball), 1131010, 1116267, 10.0, 60.0, typeof(IronIngot), 1044036, 12, 1044037);
                }

                if (Core.EJ)
                {
                    index = AddCraft(typeof(Grapeshot), 1131010, 1116030, 15.0, 70.0, typeof(IronIngot), 1044036, 12, 1044037);
                    AddRes(index, typeof(Cloth), 1131010, 2, 1044287);
                    SetUseAllRes(index, true);
                }
                else
                {
                    index = AddCraft(typeof(LightGrapeshot), 1131010, 1116030, 0.0, 50.0, typeof(IronIngot), 1044036, 6, 1044037);
                    AddRes(index, typeof(Cloth), 1044286, 1, 1044287);

                    index = AddCraft(typeof(HeavyGrapeshot), 1131010, 1116166, 15.0, 70.0, typeof(IronIngot), 1044036, 12, 1044037);
                    AddRes(index, typeof(Cloth), 1044286, 2, 1044287);
                }

                index = AddCraft(typeof(LightShipCannonDeed), 1131010, 1095790, 65.0, 120.0, typeof(IronIngot), 1044036, 900, 1044037);
                AddRes(index, typeof(Board), 1044041, 50, 1044351);
                AddSkill(index, SkillName.Carpentry, 65.0, 100.0);

                index = AddCraft(typeof(HeavyShipCannonDeed), 1131010, 1095794, 70.0, 120.0, typeof(IronIngot), 1044036, 1800, 1044037);
                AddRes(index, typeof(Board), 1044041, 75, 1044351);
                AddSkill(index, SkillName.Carpentry, 70.0, 100.0);
            }

            #endregion

            // Set the overridable material
            SetSubRes(typeof(IronIngot), 1044022);

            // Add every material you want the player to be able to choose from
            // This will override the overridable material
            AddSubRes(typeof(IronIngot), 1044022, 00.0, 1044036, 1044267);
            //AddSubRes(typeof(DullCopperIngot), 1044023, 65.0, 1044036, 1044268);
            //AddSubRes(typeof(ShadowIronIngot), 1044024, 70.0, 1044036, 1044268);
            AddSubRes(typeof(CopperIngot), 1044025, 25.0, 1044036, 1044268);
            AddSubRes(typeof(BronzeIngot), 1044026, 50.0, 1044036, 1044268);
            AddSubRes(typeof(GoldIngot), 1044027, 75.0, 1044036, 1044268);
            AddSubRes(typeof(AgapiteIngot), 1044028, 100.0, 1044036, 1044268);
            AddSubRes(typeof(VeriteIngot), 1044029, 125.0, 1044036, 1044268);
            AddSubRes(typeof(ValoriteIngot), 1044030, 150.0, 1044036, 1044268);

			/*
            SetSubRes2(typeof(RedScales), 1060875);

            AddSubRes2(typeof(RedScales), 1060875, 0.0, 1053137, 1044268);
            AddSubRes2(typeof(YellowScales), 1060876, 0.0, 1053137, 1044268);
            AddSubRes2(typeof(BlackScales), 1060877, 0.0, 1053137, 1044268);
            AddSubRes2(typeof(GreenScales), 1060878, 0.0, 1053137, 1044268);
            AddSubRes2(typeof(WhiteScales), 1060879, 0.0, 1053137, 1044268);
            AddSubRes2(typeof(BlueScales), 1060880, 0.0, 1053137, 1044268);
			*/
            Resmelt = false;
            Repair = true;
            MarkOption = true;
            CanEnhance = true;//Core.AOS;
			CanAlter = false;//Core.SA;
        }
    }

    public class ForgeAttribute : Attribute
    { }

    public class AnvilAttribute : Attribute
    { }
}
