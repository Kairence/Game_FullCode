using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Engines.Craft;

namespace Server.Misc
{
    // 이전 답변에서 확정한 12개 직업 2217번까지의 CraftType Enum (생략 없이 전문 사용)
    public enum CraftType : int
    {
        None = 0,
        TotalBlacksmithy = 10,
        Bascinet = 11, CloseHelm = 12, Helmet = 13, NorseHelm = 14, PlateHelm = 15,
        RingmailGloves = 16, RingmailLegs = 17, RingmailArms = 18, RingmailChest = 19,
        ChainCoif = 20, ChainLegs = 21, ChainChest = 22, PlateArms = 23, PlateGloves = 24,
        PlateGorget = 25, PlateLegs = 26, PlateChest = 27, FemalePlateChest = 28,
        Buckler = 29, BronzeShield = 30, HeaterShield = 31, MetalShield = 32,
        MetalKiteShield = 33, WoodenKiteShield = 34, ChaosShield = 35, OrderShield = 36,
        BoneHarvester = 37, Broadsword = 38, Cutlass = 39, Katana = 40, Longsword = 41,
        PaladinSword = 42, Scimitar = 43, ThinLongsword = 44, VikingSword = 45,
        Bardiche = 46, BladedStaff = 47, CrescentBlade = 48, Halberd = 49, Scythe = 50,
        Axe = 51, BattleAxe = 52, DoubleAxe = 53, ExecutionersAxe = 54, LargeBattleAxe = 55,
        OrnateAxe = 56, TwoHandedAxe = 57, Mace = 58, Maul = 59, Scepter = 60,
        WarAxe = 61, WarMace = 62, HammerPick = 63, WarHammer = 64, AssassinSpike = 65,
        Dagger = 66, Lance = 67, Kryss = 68, WarFork = 69, DoubleBladedStaff = 70,
        ElvenSpellblade = 71, Pike = 72, ShortSpear = 73, Spear = 74, MetalKeg = 75,
        DragonBardingDeed = 76, Cannonball = 77, LightCannonball = 78, HeavyCannonball = 79,
        Grapeshot = 80, LightGrapeshot = 81, HeavyGrapeshot = 82, LightShipCannonDeed = 83,
        HeavyShipCannonDeed = 84,

        TotalTailoring = 200,
        CutUpCloth = 201, CombineCloth = 202, PowderCharge = 203, LightPowderCharge = 204,
        HeavyPowderCharge = 205, AbyssalCloth = 206, SkullCap = 207, Bandana = 208,
        FloppyHat = 209, Cap = 210, WideBrimHat = 211, StrawHat = 212, TallStrawHat = 213,
        WizardsHat = 214, Bonnet = 215, FeatheredHat = 216, TricorneHat = 217, JesterHat = 218,
        FlowerGarland = 219, ClothNinjaJacket = 220, Kamishimo = 221, HakamaShita = 222,
        MaleKimono = 223, FemaleKimono = 224, JinBaori = 225, OrcMask = 226, BearMask = 227,
        DeerMask = 228, TribalMask = 229, HornedTribalMask = 230, Doublet = 231, Shirt = 232,
        FancyShirt = 233, Tunic = 234, Surcoat = 235, PlainDress = 236, FancyDress = 237,
        Cloak = 238, Robe = 239, JesterSuit = 240, FurCape = 241, GildedDress = 242,
        FormalShirt = 243, ShortPants = 244, LongPants = 245, Kilt = 246, Skirt = 247,
        FurSarong = 248, Hakama = 249, TattsukeHakama = 250, ElvenShirt = 251, ElvenDarkShirt = 252,
        ElvenPants = 253, MaleElvenRobe = 254, FemaleElvenRobe = 255, WoodlandBelt = 256,
        GuildedKilt = 257, CheckeredKilt = 258, FancyKilt = 259, FloweredDress = 260, EveningGown = 261,
        BodySash = 262, HalfApron = 263, FullApron = 264, Obi = 265, OilCloth = 266,
        GozaMatDeed = 267, SquareGozaMatDeed = 268, BrocadeGozaMatDeed = 269, BrocadeSquareGozaMatDeed = 270, 
        CurtainsDeed = 271, ElvenBoots = 272, FurBoots = 273, NinjaTabi = 274, SamuraiTabi = 275, Sandals = 276, 
        Shoes = 277, Boots = 278, ThighBoots = 279, LeatherGorget = 280, LeatherCap = 281, LeatherGloves = 282, 
        LeatherArms = 283, LeatherLegs = 284, LeatherChest = 285, LeafChest = 286, LeafArms = 287, LeafGloves = 288, 
        LeafLegs = 289, LeafGorget = 290, LeafTonlet = 291, WingedHelm = 292, TigerPeltChest = 293, TigerPeltLegs = 294,
        TigerPeltShorts = 295, TigerPeltHelm = 296, TigerPeltCollar = 297, DragonTurtleHideChest = 298,
        DragonTurtleHideLegs = 299, DragonTurtleHideHelm = 300, DragonTurtleHideArms = 301, StuddedGorget = 302,
        StuddedGloves = 303, StuddedArms = 304, StuddedLegs = 305, StuddedChest = 306, HideChest = 307,
        HidePauldrons = 308, HideGloves = 309, HidePants = 310, HideGorget = 311, VultureHelm = 312,
        LeatherShorts = 313, LeatherSkirt = 314, LeatherBustierArms = 315, StuddedBustierArms = 316,
        FemaleLeatherChest = 317, FemaleStuddedChest = 318, TigerPeltBustier = 319, TigerPeltLongSkirt = 320,
        TigerPeltSkirt = 321, DragonTurtleHideBustier = 322, BoneHelm = 323, BoneGloves = 324,
        BoneArms = 325, BoneLegs = 326, BoneChest = 327, OrcHelm = 328,

        TotalTinkering = 400,
        GoldBracelet = 401, SilverBracelet = 402, GoldRing = 403, SilverRing = 404,
        GoldNecklace = 405, SilverNecklace = 406, GoldEarrings = 407, SilverEarrings = 408,
        Scissors = 409, MortarPestle = 410, Scorp = 411, TinkerTools = 412, Hatchet = 413,
        DrawKnife = 414, SewingKit = 415, Saw = 416, DovetailSaw = 417, Froe = 418,
        Shovel = 419, Hammer = 420, Tongs = 421, SmithHammer = 422, SledgeHammer = 423,
        Inshave = 424, Pickaxe = 425, Lockpick = 426, Skillet = 427, FlourSifter = 428,
        FletcherTools = 429, MapmakersPen = 430, ScribesPen = 431, Clippers = 432,
        MetalContainerEngraver = 433, Pitchfork = 434, Gears = 435, ClockParts = 436,
        BarrelTap = 437, Springs = 438, SextantParts = 439, BarrelHoops = 440, Hinge = 441,
        BolaBall = 442, JeweledFiligree = 443, ButcherKnife = 444, SpoonLeft = 445,
        SpoonRight = 446, Plate = 447, ForkLeft = 448, ForkRight = 449, Cleaver = 450,
        KnifeLeft = 451, KnifeRight = 452, Goblet = 453, PewterMug = 454, SkinningKnife = 455,
        GargishCleaver = 456, GargishButcherKnife = 457, KeyRing = 458, Candelabra = 459,
        Scales = 460, Key = 461, Globe = 462, Spyglass = 463, Lantern = 464, HeatingStand = 465,
        ShojiLantern = 466, PaperLantern = 467, RoundPaperLantern = 468, WindChimes = 469,
        FancyWindChimes = 470, DragonLamp = 471, StainedGlassLamp = 472, TallDoubleLamp = 473,
        CraftableHouseItem = 474, CraftableMetalHouseDoor = 475, WallSafeDeed = 476, KotlPowerCore = 477,
        WeatheredBronzeGlobeSculptureDeed = 478, WeatheredBronzeManOnABenchDeed = 479,
        WeatheredBronzeFairySculptureDeed = 480, WeatheredBronzeArcherDeed = 481, MetalLadderDeed = 482,
        AxleGears = 483, ClockRight = 484, ClockLeft = 485, Sextant = 486, Bola = 487, PotionKegTinker = 488,
        ModifiedClockworkAssembly = 489, HitchingRope = 490, HitchingPost = 491, ArcanicRuneStone = 492,
        VoidOrb = 493, AdvancedTrainingDummyDeed = 494, DistilleryAddonDeed = 495, KotlAutomatonHead = 496,
        PersonalTelescope = 497, DartTrapCraft = 498, PoisonTrapCraft = 499, ExplosionTrapCraft = 500,
        FactionGasTrapDeed = 501, FactionExplosionTrapDeed = 502, FactionSawTrapDeed = 503,
        FactionSpikeTrapDeed = 504, FactionTrapRemovalKit = 505,

        TotalAlchemy = 600,
        MiniHealPotion = 601, MiniRefreshPotion = 602, MiniAgilityPotion = 603,
        MiniCurePotion = 604, MiniPoisonPotion = 605, MiniExplosionPotion = 606,
        NightSightPotion = 607, MiniStrengthPotion = 608, LesserHealPotion = 609,
        LesserRefreshPotion = 610, LesserAgilityPotion = 611, LesserCurePotion = 612,
        LesserPoisonPotion = 613, LesserExplosionPotion = 614, LesserStrengthPotion = 615,
        HealPotion = 616, RefreshPotion = 617, AgilityPotion = 618, CurePotion = 619,
        PoisonPotion = 620, ExplosionPotion = 621, StrengthPotion = 622,
        GreaterHealPotion = 623, GreaterRefreshPotion = 624, GreaterAgilityPotion = 625,
        GreaterCurePotion = 626, GreaterPoisonPotion = 627, GreaterExplosionPotion = 628,
        GreaterStrengthPotion = 629, TotalHealPotion = 630, TotalRefreshPotion = 631,
        TotalAgilityPotion = 632, TotalCurePotion = 633, DeadlyPoisonPotion = 634,
        TotalExplosionPotion = 635, TotalStrengthPotion = 636, Bottle = 637, HairRestylingDeed = 638,
        ElixirOfRebirth = 639, BarrabHemolymphConcentrate = 640, InvisibilityPotion = 641,
        JukariBurnPoiltice = 642, KurakAmbushersEssence = 643, BarakoDraftOfMight = 644,
        UraliTranceTonic = 645, SakkhraProphylaxisPotion = 646, ParasiticPotion = 647,
        DarkglowPotion = 648, ScouringToxin = 649, ConflagrationPotion = 650,
        GreaterConflagrationPotion = 651, ConfusionBlastPotion = 652, GreaterConfusionBlastPotion = 653,
        BlackPowder = 654, Matchcord = 655, FuseCord = 656, SmokeBomb = 657, HoveringWisp = 658,
        NaturalDye = 659, NexusCore = 660, PlantPigment = 661, ColorFixative = 662,
        CrystalGranules = 663, CrystalDust = 664, SoftenedReeds = 665, VialOfVitriol = 666,
        BottleIchor = 667, Potash = 668, GoldDust = 669,

        TotalCarpentry = 800,
        TaxidermyKit = 801, Board = 802, BarkFragment = 803, LustrousHeartwood = 804,
        WoodenBox = 805, SmallCrate = 806, MediumCrate = 807, LargeCrate = 808, WoodenChest = 809,
        PlainWoodenChest = 810, OrnateWoodenChest = 811, GildedWoodenChest = 812, WoodenFootLocker = 813,
        FinishedWoodenChest = 814, BarrelStaves = 815, BarrelLid = 816, Keg = 817, PotionKegCarpentry = 818,
        Armoire = 819, FancyArmoire = 820, CherryArmoire = 821, MapleArmoire = 822, ElegantArmoire = 823,
        PlainArmoire = 824, WoodenArmoire = 825, GargishCouch = 826, ShortCabinet = 827, TallCabinet = 828,
        RedArmoire = 829, ElegantCabinet = 830, FootStool = 831, Stool = 832, StrawChair = 833,
        WoodenChair = 834, VesperStyleChair = 835, TrinsicStyleChair = 836, WoodenBench = 837, Throne = 838,
        MaginciaStyleChair = 839, YewStyleChair = 840, BambooChair = 841, CushionedChair = 842,
        ReadingChair = 843, OrnateChair = 844, FancyChair = 845, GargishChair = 846, StoneChairCarp = 847,
        FleshyChair = 848, GargishStoneChair = 849, LargeStoneTable = 850, StoneTableCarp = 851,
        LargeGargishStoneTable = 852, SmallGargishStoneTable = 853, GargishTable = 854, Table = 855,
        WritingDesk = 856, YewWoodTable = 857, ElegantLowTent = 858, PlainLowTent = 859, OrnateTable = 860,
        HardwoodDesk = 861, Dresser = 862, BedDeed = 863, GargishCotDeed = 864, GargishBedDeed = 865,
        RusticBedDeed = 866, WaterTroughDeed = 867, WoodenTroughDeed = 868, FishingPole = 869, QuarterStaff = 870,
        GnarledStaff = 871, ShepherdsCrook = 872, Tetsubo = 873, Bokuto = 874, NunchakuCarp = 875,
        WallTrophyDeed = 876, EaselDeed = 877, PentagramDeed = 878, AbattoirDeed = 879, DartboardDeed = 880,
        LoomDeed = 881, SpinningWheelDeed = 882, ScreenDeed = 883, StoneAnvilDeed = 884, SoulforgeDeed = 885,
        TrainingDummyDeed = 886, PickpocketDipDeed = 887, ArcaneTableDeed = 888, BakersScaffoldDeed = 889,
        BoneTableDeed = 890, DressingTableDeed = 891, RaisedVanityDeed = 892, SofaDeed = 893,
        PlantGoblinTrophyDeed = 894, MusicStandDeed = 895, BustDeed = 896, ArcheryButteDeed = 897, DartsDeed = 898,
        BallotBox = 899, WoodenShield = 900, CarpentersDevice = 901, WoodworkingTools = 902, Nails = 903,
        Moulding = 904, MillDeed = 905, WaterWheelDeed = 906, StoneOvenDeed = 907, FireplaceDeed = 908,
        BedOfNailsDeed = 909, HauntedMirrorDeed = 910, BoilingCauldronDeed = 911, IronMaidenDeed = 912,
        CreepyPortraitDeed = 913, ElvenPodiumDeed = 914, ElvenForgeDeed = 915, ElvenSpinningWheelDeed = 916,
        ElvenStoveDeed = 917, ElvenLoomDeed = 918, ElvenWashBasinDeed = 919,

        TotalBowFletching = 1000,
        Arrow = 1001, Bolt = 1002, FukiyaDarts = 1003, FletcherToolsBow = 1004, Bow = 1005,
        Crossbow = 1006, HeavyCrossbow = 1007, CompositeBow = 1008, RepeatingCrossbow = 1009,
        Yumi = 1010, ElvenCompositeBow = 1011, MagicalShortbow = 1012, BlightGrippedLongbow = 1013,
        FaerieFire = 1014, SilvanisFeywoodBow = 1015, MischiefMaker = 1016, TheNightReaper = 1017,
        Boomerang = 1018, Cyclone = 1019, SoulGlaive = 1020, Kindling = 1021,

        TotalCooking = 1200,
        SackOfFlour = 1201, Dough = 1202, SweetDough = 1203, CakeMix = 1204, CookieMix = 1205,
        CocoaButter = 1206, RawRibs = 1207, RawLambLeg = 1208, RawBird = 1209, RawChickenLeg = 1210,
        RawFishSteak = 1211, FriedEggs = 1212, CookedBird = 1213, Ribs = 1214, FishSteak = 1215,
        Bacon = 1216, Sausage = 1217, Ham = 1218, ChickenLeg = 1219, LambLeg = 1220, PorkPan = 1221,
        VegetableSoup = 1222, CheesePizza = 1223, FruitPie = 1224, MeatPie = 1225, PumpkinPie = 1226,
        ApplePie = 1227, BakedQuiche = 1228, PeachCobbler = 1229, Muffins = 1230, Cake = 1231,
        Cookies = 1232, Pizza = 1233, FrenchBread = 1234, BreadLoaf = 1235, PanCookies = 1236,
        Nutcracker = 1237, TribalBerry = 1238, Grapes = 1239, SavageOsti = 1240, CreateFoodOsti = 1241,
        Fish = 1242, Chocolate = 1243, GreenTea = 1244, SushiRoll = 1245, SushiPlatter = 1246,
        MisoSoup = 1247, AwaseMisoSoup = 1248, BentoBox = 1249, Wasabi = 1250, ParrotRoast = 1251,
        CocoaLiquor = 1252, WheatWort = 1253, Coffee = 1254, CoffeeMug = 1255, EnchantedApple = 1256,
        GrapesOfWrath = 1257, BBQRoastPig = 1258, BakedHam = 1259,

        TotalInscription = 1400,
        Spellbook = 1401, NecromancerSpellbook = 1402, ChivalryBook = 1403, BushidoBook = 1404,
        NinjitsuBook = 1405, ArcaneManual = 1406, MysticBook = 1407, BardicManual = 1408, MageryBook = 1409,
        BlankScroll = 1410, RecallRune = 1411, Runebook = 1412, BulkOrderBook = 1413, EngravingTool = 1414,
        ScrappersCompendiumInsc = 1415, ReactiveArmorScroll = 1416, ClumsyScroll = 1417, CreateFoodScroll = 1418, 
        FeeblemindScroll = 1419, HealScroll = 1420, MagicArrowScroll = 1421, NightSightScroll = 1422, WeakenScroll = 1423,
        AgilityScroll = 1424, CunningScroll = 1425, CureScroll = 1426, HarmScroll = 1427,
        MagicTrapScroll = 1428, MagicUntrapScroll = 1429, ProtectionScroll = 1430, StrengthScroll = 1431,
        BlessScroll = 1432, FireballScroll = 1433, MagicLockScroll = 1434, PoisonScroll = 1435,
        TelekinesisScroll = 1436, TeleportScroll = 1437, UnlockScroll = 1438, WallOfStoneScroll = 1439,
        ArchCureScroll = 1440, ArchProtectionScroll = 1441, CurseScroll = 1442, FireFieldScroll = 1443,
        GreaterHealScroll = 1444, LightningScroll = 1445, ManaDrainScroll = 1446, RecallScroll = 1447,
        BladeSpiritsScroll = 1448, DispelFieldScroll = 1449, IncognitoScroll = 1450, MagicReflectionScroll = 1451,
        MindBlastScroll = 1452, ParalyzeScroll = 1453, PoisonFieldScroll = 1454, SummonCreatureScroll = 1455,
        DispelScroll = 1456, EnergyBoltScroll = 1457, ExplosionScroll = 1458, InvisibilityScroll = 1459,
        MarkScroll = 1460, MassCurseScroll = 1461, ParalyzeFieldScroll = 1462, RevealScroll = 1463,
        ChainLightningScroll = 1464, EnergyFieldScroll = 1465, FlamestrikeScroll = 1466, GateTravelScroll = 1467,
        ManaVampireScroll = 1468, MassDispelScroll = 1469, MeteorSwarmScroll = 1470, PolymorphScroll = 1471,
        EarthquakeScroll = 1472, EnergyVortexScroll = 1473, ResurrectionScroll = 1474, SummonAirElementalScroll = 1475,
        SummonDaemonScroll = 1476, SummonEarthElementalScroll = 1477, SummonFireElementalScroll = 1478,
        SummonWaterElementalScroll = 1479, AnimateDeadScroll = 1480, BloodOathScroll = 1481, CorpseSkinScroll = 1482, 
        CurseWeaponScroll = 1483, EvilOmenScroll = 1484, HorrificBeastScroll = 1485, LichFormScroll = 1486, MindRotScroll = 1487,
        PainSpikeScroll = 1488, PoisonStrikeScroll = 1489, StrangleScroll = 1490, SummonFamiliarScroll = 1491,
        VampiricEmbraceScroll = 1492, VengefulSpiritScroll = 1493, WitherScroll = 1494, WraithFormScroll = 1495,
        ExorcismScroll = 1496, NetherBoltScroll = 1497, HealingStoneScroll = 1498, PurgeMagicScroll = 1499, EnchantScroll = 1500,
        SleepScroll = 1501, EagleStrikeScroll = 1502, AnimatedWeaponScroll = 1503, StoneFormScroll = 1504,
        SpellTriggerScroll = 1505, MassSleepScroll = 1506, CleansingWindsScroll = 1507, BombardScroll = 1508,
        SpellPlagueScroll = 1509, HailStormScroll = 1510, NetherCycloneScroll = 1511, RisingColossusScroll = 1512,

        TotalCartography = 1600,
        LocalMap = 1601, CityMap = 1602, SeaChart = 1603, WorldMap = 1604, BlankMap = 1605,
        MapmakersPenCart = 1606, WallMapDeed = 1607,

        TotalGlassblowing = 1800,
        EmptyVial = 1801, PotionBottle = 1802, Flask = 1803, Beaker = 1804,
        Jug = 1805, Hourglass = 1806, GlassSword = 1807, GlassStaff = 1808,

        TotalImbuing = 2000,
        MagicalResidue = 2001, EnchantedEssence = 2002, RelicFragment = 2003,

        TotalMasonry = 2200,
        StoneAnvilMason = 2201, StoneTableMason = 2202, StoneChairMason = 2203, StoneVase = 2204,
        StoneStatue = 2205, GargoyleStatue = 2206, PegasusStatue = 2207, DragonStatue = 2208,
        GriffonStatue = 2209, EagleStatue = 2210, BullStatue = 2211, DeerStatue = 2212,
        BustStatue = 2213, GargishStoneTableMason = 2214, GargishStoneChairMason = 2215,
        GargishStoneVase = 2216, StoneFireplaceMason = 2217
    }

    public static class CraftMastery
    {
        public const int MaxLevel = 100;
        public const int LevelOffset = 3000;

        // UI 카테고리 열람을 위한 직업 배열
        public static readonly CraftType[] Categories = new CraftType[]
        {
            CraftType.TotalBlacksmithy, CraftType.TotalTailoring, CraftType.TotalTinkering,
            CraftType.TotalAlchemy, CraftType.TotalCarpentry, CraftType.TotalBowFletching,
            CraftType.TotalCooking, CraftType.TotalInscription, CraftType.TotalCartography,
            CraftType.TotalGlassblowing, CraftType.TotalImbuing, CraftType.TotalMasonry
        };

        // ---------------------------------------------------------
        // 1. 경험치 및 레벨업 로직
        // ---------------------------------------------------------
        public static int GetNextExp(int currentLevel)
        {
            if (currentLevel >= MaxLevel) return 0;
            return (currentLevel + 1) * (currentLevel + 1) * 25;
        }

        public static void AddExp(PlayerMobile pm, CraftType type, int amount = 1)
        {
            if (pm == null || type == CraftType.None) return;

            ProcessExp(pm, type, amount);
            CraftType totalType = GetCategoryTotal(type);
            if (totalType != CraftType.None && totalType != type)
                ProcessExp(pm, totalType, amount);
        }

        private static void ProcessExp(PlayerMobile pm, CraftType type, int amount)
        {
            int idx = (int)type;
            if (idx <= 0 || idx >= 3000) return;

            int currentLevel = pm.CraftPoint[idx + LevelOffset];
            if (currentLevel >= MaxLevel) return;

            pm.CraftPoint[idx] += amount;

            if (pm.CraftPoint[idx] >= GetNextExp(currentLevel))
            {
                pm.CraftPoint[idx + LevelOffset]++;
                int hue = (type == GetCategoryTotal(type)) ? 0x44 : 0x35;
                pm.SendMessage(hue, $"[{GetProductionName(type)}] 제작 숙련도가 {pm.CraftPoint[idx + LevelOffset]} 레벨이 되었습니다!");
                pm.PlaySound(0x214);
            }
        }

        // ---------------------------------------------------------
        // 2. 제작 숙련도 연산용 헬퍼 (차후 CraftSystem 훅에 연동할 부분 - 주석 처리)
        // ---------------------------------------------------------
        
        /*
        // [개별/총합 보너스 예시 템플릿] - 유저님이 원하시는 기획 수치로 채워 넣으시면 됩니다.
        public static double GetExceptionalBonus(PlayerMobile pm, CraftType type)
        {
            // 예: 개별 레벨당 0.2% 익셉 확률 증가
            return 0.0; 
        }

        public static double GetResourceSaveBonus(PlayerMobile pm, CraftType type)
        {
            // 예: 총합 레벨당 0.1% 재료 반환 확률 증가
            return 0.0;
        }

        public static bool CheckMasteryInstantCraft(PlayerMobile pm, CraftType totalType)
        {
            // 예: 총합 100레벨 시 즉시 제작 등
            return false;
        }
        */

        // ---------------------------------------------------------
        // 3. 카테고리 총합 판별 및 이름 변환
        // ---------------------------------------------------------
        public static CraftType GetCategoryTotal(CraftType type)
        {
            int idx = (int)type;
            if (idx >= 10 && idx < 200) return CraftType.TotalBlacksmithy;
            if (idx >= 200 && idx < 400) return CraftType.TotalTailoring;
            if (idx >= 400 && idx < 600) return CraftType.TotalTinkering;
            if (idx >= 600 && idx < 800) return CraftType.TotalAlchemy;
            if (idx >= 800 && idx < 1000) return CraftType.TotalCarpentry;
            if (idx >= 1000 && idx < 1200) return CraftType.TotalBowFletching;
            if (idx >= 1200 && idx < 1400) return CraftType.TotalCooking;
            if (idx >= 1400 && idx < 1600) return CraftType.TotalInscription;
            if (idx >= 1600 && idx < 1800) return CraftType.TotalCartography;
            if (idx >= 1800 && idx < 2000) return CraftType.TotalGlassblowing;
            if (idx >= 2000 && idx < 2200) return CraftType.TotalImbuing;
            if (idx >= 2200 && idx < 2400) return CraftType.TotalMasonry;
            return CraftType.None;
        }

        public static string GetCategoryName(CraftType type)
        {
            return type switch
            {
                CraftType.TotalBlacksmithy => "대장장이",
                CraftType.TotalTailoring => "재봉",
                CraftType.TotalTinkering => "기계공학",
                CraftType.TotalAlchemy => "연금술",
                CraftType.TotalCarpentry => "목공",
                CraftType.TotalBowFletching => "활제작",
                CraftType.TotalCooking => "요리",
                CraftType.TotalInscription => "주문각인",
                CraftType.TotalCartography => "지도제작",
                CraftType.TotalGlassblowing => "유리세공",
                CraftType.TotalImbuing => "마법부여",
                CraftType.TotalMasonry => "석공",
                _ => "알 수 없음"
            };
        }

		// ---------------------------------------------------------
        // 4. 동적 UI 연동을 위한 CraftSystem 헬퍼
        // ---------------------------------------------------------
        public static CraftSystem GetCraftSystem(CraftType category)
        {
            switch (category)
            {
                case CraftType.TotalBlacksmithy: return DefBlacksmithy.CraftSystem;
                case CraftType.TotalTailoring: return DefTailoring.CraftSystem;
                case CraftType.TotalTinkering: return DefTinkering.CraftSystem;
                case CraftType.TotalAlchemy: return DefAlchemy.CraftSystem;
                case CraftType.TotalCarpentry: return DefCarpentry.CraftSystem;
                case CraftType.TotalBowFletching: return DefBowFletching.CraftSystem;
                case CraftType.TotalCooking: return DefCooking.CraftSystem;
                case CraftType.TotalInscription: return DefInscription.CraftSystem;
                case CraftType.TotalCartography: return DefCartography.CraftSystem;
                case CraftType.TotalGlassblowing: return DefGlassblowing.CraftSystem;
                case CraftType.TotalImbuing: return DefImbuing.CraftSystem;
                case CraftType.TotalMasonry: return DefMasonry.CraftSystem;
                default: return null;
            }
        }

        public static CraftType ParseCraftType(Type itemType, CraftType category)
        {
            if (itemType == null) return CraftType.None;
            string name = itemType.Name;
            
            // 이름 중복 회피용 커스텀 매핑 처리
            if (name == "PotionKeg") return category == CraftType.TotalTinkering ? CraftType.PotionKegTinker : CraftType.PotionKegCarpentry;
            if (name == "StoneAnvil") return category == CraftType.TotalMasonry ? CraftType.StoneAnvilMason : CraftType.StoneAnvilDeed;

            if (Enum.TryParse(name, out CraftType result))
                return result;

            return CraftType.None;
        }

        public static string GetProductionName(CraftType type)
        {
            if (type == GetCategoryTotal(type)) return GetCategoryName(type) + " 총합";
            // TODO: 개별 아이템의 한글 명칭은 Cliloc 연동이나 별도 하드코딩 필요
            return type.ToString();
        }
    }
}