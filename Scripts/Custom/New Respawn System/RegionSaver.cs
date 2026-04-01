using System;
using System.Collections.Generic;
using Server;

namespace Server.Misc
{
    // ==============================================================================
    // [Enum] 구역 고유 식별 번호 (6자리 절대 규칙)
    // [100000] 대륙: 1=트라멜, 2=펠루카, 3=일쉐나, 4=말라스, 5=토쿠노, 6=터머
    // [10000]  분류: 1=마을, 2=던전, 3=신전, 4=특수(Internal)
    // [100]    고유이름 (01~99)
    // [1]      세부구역 (00=대표구역, 01~99=세부구역)
    // ==============================================================================
    public enum RegionCode
    {
        None = 0,

        // ==========================================
        // 🏡 트라멜 (1) - 마을 (1)
        // ==========================================
        Trammel_Town_Britain = 110100,
        Trammel_Town_Britain_BlackthornCastle = 110101,
        Trammel_Town_Britain_BlackthornEntrance = 110102,
        Trammel_Town_Britain_BritishCastle = 110103,
        Trammel_Town_Britain_BritishEntrance = 110104,
        Trammel_Town_Britain_Cemetery = 110105,
        Trammel_Town_Britain_Center = 110106,
        Trammel_Town_Britain_Farmlands = 110107,
        Trammel_Town_Britain_Park = 110108,
        Trammel_Town_Britain_Suburbs = 110109,

        Trammel_Town_BuccaneersDen = 110200,
        Trammel_Town_BuccaneersDen_Bathhouse = 110201,
        Trammel_Town_BuccaneersDen_Docks = 110202,
        Trammel_Town_BuccaneersDen_Tunnels = 110203,

        Trammel_Town_Cove = 110300,
        Trammel_Town_Cove_Cemetery = 110301,
        Trammel_Town_Cove_Gates = 110302,
        Trammel_Town_Cove_GuardPost = 110303,
        Trammel_Town_Cove_OrcFort = 110304,

        Trammel_Town_Jhelom = 110400,
        Trammel_Town_Jhelom_Cemetery = 110401,
        Trammel_Town_Jhelom_EastDocks = 110402,
        Trammel_Town_Jhelom_FightingPit = 110403,
        Trammel_Town_Jhelom_MainIsland = 110404,
        Trammel_Town_Jhelom_MediumIsland = 110405,
        Trammel_Town_Jhelom_SmallIsland = 110406,

        Trammel_Town_Magincia = 110500,
        Trammel_Town_Magincia_Bank = 110501,
        Trammel_Town_Magincia_Docks = 110502,
        Trammel_Town_Magincia_Park = 110503,
        Trammel_Town_Magincia_Parliament = 110504,

        Trammel_Town_Minoc = 110600,
        Trammel_Town_Minoc_Bridge = 110601,
        Trammel_Town_Minoc_GypsyCamp = 110602,
        Trammel_Town_Minoc_MiningCamp = 110603,
        Trammel_Town_Minoc_North = 110604,
        Trammel_Town_Minoc_South = 110605,

        Trammel_Town_Moonglow = 110700,
        Trammel_Town_Moonglow_Cemetery = 110701,
        Trammel_Town_Moonglow_Center = 110702,
        Trammel_Town_Moonglow_Docks = 110703,
        Trammel_Town_Moonglow_Telescope = 110704,
        Trammel_Town_Moonglow_Zoo = 110705,

        Trammel_Town_Nujelm = 110800,
        Trammel_Town_Nujelm_Cemetery = 110801,
        Trammel_Town_Nujelm_ChessBoard = 110802,
        Trammel_Town_Nujelm_Docks = 110803,
        Trammel_Town_Nujelm_East = 110804,
        Trammel_Town_Nujelm_North = 110805,
        Trammel_Town_Nujelm_Palace = 110806,
        Trammel_Town_Nujelm_West = 110807,

        Trammel_Town_Haven = 110900,
        Trammel_Town_Haven_OldHaven = 110901,
        Trammel_Town_Haven_OldHavenNorth = 110902,
        Trammel_Town_Haven_NewHaven = 110903,
        Trammel_Town_Haven_NewHavenNorth = 110904,
        Trammel_Town_Haven_Farmlands = 110905,

        Trammel_Town_SerpentsHold = 111000,
        Trammel_Town_SerpentsHold_North = 111001,
        Trammel_Town_SerpentsHold_South = 111002,
        Trammel_Town_SerpentsHold_GuardPost = 111003,

        Trammel_Town_SkaraBrae = 111100,
        Trammel_Town_SkaraBrae_East = 111101,
        Trammel_Town_SkaraBrae_EastDocks = 111102,
        Trammel_Town_SkaraBrae_North = 111103,
        Trammel_Town_SkaraBrae_South = 111104,
        Trammel_Town_SkaraBrae_West = 111105,
        Trammel_Town_SkaraBrae_WestDocks = 111106,

        Trammel_Town_Trinsic = 111200,
        Trammel_Town_Trinsic_Center = 111201,
        Trammel_Town_Trinsic_EastDocks = 111202,
        Trammel_Town_Trinsic_IslandPark = 111203,
        Trammel_Town_Trinsic_North = 111204,
        Trammel_Town_Trinsic_South = 111205,
        Trammel_Town_Trinsic_SouthGate = 111206,
        Trammel_Town_Trinsic_WestGate = 111207,

        Trammel_Town_Vesper = 111300,
        Trammel_Town_Vesper_Cemetery = 111301,
        Trammel_Town_Vesper_Center = 111302,
        Trammel_Town_Vesper_Docks = 111303,
        Trammel_Town_Vesper_East = 111304,
        Trammel_Town_Vesper_North = 111305,

        Trammel_Town_Wind = 111400,
        Trammel_Town_Wind_Caves = 111401,
        Trammel_Town_Wind_East = 111402,
        Trammel_Town_Wind_Park = 111403,
        Trammel_Town_Wind_South = 111404,
        Trammel_Town_Wind_West = 111405,

        Trammel_Town_Yew = 111500,
        Trammel_Town_Yew_Cemetery = 111501,
        Trammel_Town_Yew_Center = 111502,
        Trammel_Town_Yew_CourtsAndPrisons = 111503,
        Trammel_Town_Yew_EmpathAbbey = 111504,
        Trammel_Town_Yew_HiddenCave = 111505,
        Trammel_Town_Yew_OrcFort = 111506,

        Trammel_Town_Delucia = 111600,
        Trammel_Town_Delucia_WatchTower = 111601,
        Trammel_Town_Delucia_Center = 111602,
        Trammel_Town_Delucia_OrcFort = 111603,

        Trammel_Town_Papua = 111700,
        Trammel_Town_Papua_TheJustInn = 111701,
        Trammel_Town_Papua_Center = 111702,
        Trammel_Town_Papua_Docks = 111703,

        Trammel_Town_Heartwood = 111800, 

        // ==========================================
        // 🦇 트라멜 (2) - 던전 (2)
        // ==========================================
        Trammel_Dungeon_BlightedGrove = 120100, 

        Trammel_Dungeon_Covetous = 120200,
        Trammel_Dungeon_Covetous_Level1 = 120201,
        Trammel_Dungeon_Covetous_Level2 = 120202,
        Trammel_Dungeon_Covetous_Level3 = 120203,
        Trammel_Dungeon_Covetous_LakeCave = 120204,
        Trammel_Dungeon_Covetous_TortureChambers = 120205,

        Trammel_Dungeon_Deceit = 120300,
        Trammel_Dungeon_Deceit_Level1 = 120301,
        Trammel_Dungeon_Deceit_Level2 = 120302,
        Trammel_Dungeon_Deceit_Level3 = 120303,
        Trammel_Dungeon_Deceit_Level4 = 120304,

        Trammel_Dungeon_Despise = 120400,
        Trammel_Dungeon_Despise_Entryway = 120401,
        Trammel_Dungeon_Despise_Level1 = 120402,
        Trammel_Dungeon_Despise_Level2 = 120403,
        Trammel_Dungeon_Despise_Level3 = 120404,

        Trammel_Dungeon_Destard = 120500,
        Trammel_Dungeon_Destard_Level1 = 120501,
        Trammel_Dungeon_Destard_Level2 = 120502,
        Trammel_Dungeon_Destard_Level3 = 120503,

        Trammel_Dungeon_Hythloth = 120600,
        Trammel_Dungeon_Hythloth_Level1 = 120601,
        Trammel_Dungeon_Hythloth_Level2 = 120602,
        Trammel_Dungeon_Hythloth_Level3 = 120603,
        Trammel_Dungeon_Hythloth_Level4 = 120604,

        Trammel_Dungeon_Shame = 120700,
        Trammel_Dungeon_Shame_Level1 = 120701,
        Trammel_Dungeon_Shame_Level2 = 120702,
        Trammel_Dungeon_Shame_Level3 = 120703,
        Trammel_Dungeon_Shame_Level4 = 120704,

        Trammel_Dungeon_Wrong = 120800,
        Trammel_Dungeon_Wrong_Level1 = 120801,
        Trammel_Dungeon_Wrong_Level2 = 120802,
        Trammel_Dungeon_Wrong_Level3 = 120803,

        Trammel_Dungeon_Miscellaneous = 120900,
        Trammel_Dungeon_Miscellaneous_HylothFirePit = 120901,
        Trammel_Dungeon_Miscellaneous_YewBritainBrigandCamp = 120902,
        Trammel_Dungeon_Miscellaneous_YewFortOfTheDamned = 120903,

        Trammel_Dungeon_TerathanKeep = 121000,
        Trammel_Dungeon_TerathanKeep_Level1 = 121001,
        Trammel_Dungeon_TerathanKeep_ChampionRoom = 121002,
        Trammel_Dungeon_TerathanKeep_Starroom = 121003,

        Trammel_Dungeon_Fire = 121100,
        Trammel_Dungeon_Fire_BritEntrance = 121101,
        Trammel_Dungeon_Fire_Level1 = 121102,
        Trammel_Dungeon_Fire_Level2 = 121103,

        Trammel_Dungeon_Ice = 121200,
        Trammel_Dungeon_Ice_BritEntrance = 121201, 
        Trammel_Dungeon_Ice_Level1 = 121202,
        Trammel_Dungeon_Ice_RatmanRoom = 121203,
        Trammel_Dungeon_Ice_IceDemonLair = 121204,

        Trammel_Dungeon_OrcCave = 121300,
        Trammel_Dungeon_OrcCave_Level1 = 121301,
        Trammel_Dungeon_OrcCave_Level2 = 121302,
        Trammel_Dungeon_OrcCave_Level3 = 121303,

        Trammel_Dungeon_PaintedCaves = 121400,
        Trammel_Dungeon_PalaceOfParoxysmus = 121500,
        Trammel_Dungeon_PrismOfLight = 121600,
        Trammel_Dungeon_Sanctuary = 121700,

        Trammel_Dungeon_SolenHives = 121800,
        Trammel_Dungeon_SolenHives_CentralArea = 121801,
        Trammel_Dungeon_SolenHives_AreaALevel1 = 121802,
        Trammel_Dungeon_SolenHives_AreaALevel2 = 121803,
        Trammel_Dungeon_SolenHives_AreaBLevel1 = 121804,
        Trammel_Dungeon_SolenHives_AreaBLevel2 = 121805,
        Trammel_Dungeon_SolenHives_AreaCLevel1 = 121806,
        Trammel_Dungeon_SolenHives_AreaCLevel2 = 121807,
        Trammel_Dungeon_SolenHives_AreaDLevel1 = 121808,
        Trammel_Dungeon_SolenHives_AreaDLevel2 = 121809,
        Trammel_Dungeon_SolenHives_AreaE = 121810,

        // ==========================================
        // 🏛️ 트라멜 (3) - 신전 (3)
        // ==========================================
        Trammel_Shrine_Chaos = 130100,
        Trammel_Shrine_Compassion = 130200,
        Trammel_Shrine_Honesty = 130300,
        Trammel_Shrine_Honor = 130400,
        Trammel_Shrine_Humility = 130500,
        Trammel_Shrine_Justice = 130600,
        Trammel_Shrine_Sacrifice = 130700,
        Trammel_Shrine_Spirituality = 130800,
        Trammel_Shrine_Valor = 130900,

        // ==========================================
        // ⚙️ 트라멜 (4) - 특수/내부 (4)
        // ==========================================
        Trammel_Internal_JailCells = 140100,
        Trammel_Internal_JailCells_Cell1 = 140101,
        Trammel_Internal_JailCells_Cell2 = 140102,
        Trammel_Internal_JailCells_Cell3 = 140103,
        Trammel_Internal_JailCells_Cell4 = 140104,
        Trammel_Internal_JailCells_Cell5 = 140105,
        Trammel_Internal_JailCells_Cell6 = 140106,
        Trammel_Internal_JailCells_Cell7 = 140107,
        Trammel_Internal_JailCells_Cell8 = 140108,
        Trammel_Internal_JailCells_Cell9 = 140109,
        Trammel_Internal_JailCells_Cell10 = 140110,

        Trammel_Internal_GreenAcres = 140200,
        
        // ==========================================
        // 💀 [2] 펠루카 (Felucca)
        // ==========================================
        Felucca_Town_Britain = 210100,
        Felucca_Town_Magincia = 210200,
        Felucca_Town_Minoc = 210300,
        Felucca_Town_Moonglow = 210400,
        Felucca_Town_SkaraBrae = 210500,
        Felucca_Town_Trinsic = 210600,
        Felucca_Town_Vesper = 210700,
        Felucca_Town_Yew = 210800,
        Felucca_Town_Jhelom = 210900,
        Felucca_Town_Ocllo = 211000,
        Felucca_Town_SerpentsHold = 211100,
        Felucca_Town_Wind = 211200,
        Felucca_Town_Delucia = 211300,
        Felucca_Town_Papua = 211400,

        Felucca_Dungeon_BlightedGrove = 220100,
        Felucca_Dungeon_Covetous = 220200,
        Felucca_Dungeon_Covetous_Level1 = 220201,
        Felucca_Dungeon_Covetous_Level2 = 220202,
        Felucca_Dungeon_Covetous_Level3 = 220203,
        Felucca_Dungeon_Deceit = 220300,
        Felucca_Dungeon_Deceit_Level1 = 220301,
        Felucca_Dungeon_Deceit_Level2 = 220302,
        Felucca_Dungeon_Deceit_Level3 = 220303,
        Felucca_Dungeon_Deceit_Level4 = 220304,
        Felucca_Dungeon_Despise = 220400,
        Felucca_Dungeon_Despise_Level1 = 220401,
        Felucca_Dungeon_Despise_Level2 = 220402,
        Felucca_Dungeon_Despise_Level3 = 220403,
        Felucca_Dungeon_Destard = 220500,
        Felucca_Dungeon_Destard_Level1 = 220501,
        Felucca_Dungeon_Destard_Level2 = 220502,
        Felucca_Dungeon_Destard_Level3 = 220503,
        Felucca_Dungeon_Hythloth = 220600,
        Felucca_Dungeon_Hythloth_Level1 = 220601,
        Felucca_Dungeon_Hythloth_Level2 = 220602,
        Felucca_Dungeon_Hythloth_Level3 = 220603,
        Felucca_Dungeon_Hythloth_Level4 = 220604,
        Felucca_Dungeon_Shame = 220700,
        Felucca_Dungeon_Shame_Level1 = 220701,
        Felucca_Dungeon_Shame_Level2 = 220702,
        Felucca_Dungeon_Shame_Level3 = 220703,
        Felucca_Dungeon_Shame_Level4 = 220704,
        Felucca_Dungeon_Wrong = 220800,
        Felucca_Dungeon_Wrong_Level1 = 220801,
        Felucca_Dungeon_Wrong_Level2 = 220802,
        Felucca_Dungeon_Wrong_Level3 = 220803,
        Felucca_Dungeon_Khaldun = 220900,
        Felucca_Dungeon_Khaldun_Level1 = 220901,
        
        Felucca_Dungeon_TerathanKeep = 221000,
        Felucca_Dungeon_Fire = 221100,
        Felucca_Dungeon_Ice = 221200,
        Felucca_Dungeon_OrcCave = 221300,
        Felucca_Dungeon_PaintedCaves = 221400,
        Felucca_Dungeon_PalaceOfParoxysmus = 221500,
        Felucca_Dungeon_PrismOfLight = 221600,
        Felucca_Dungeon_Sanctuary = 221700,
        Felucca_Dungeon_SolenHives = 221800,

        Felucca_Shrine_Chaos = 230100,
        Felucca_Shrine_Compassion = 230200,
        Felucca_Shrine_Honesty = 230300,
        Felucca_Shrine_Honor = 230400,
        Felucca_Shrine_Humility = 230500,
        Felucca_Shrine_Justice = 230600,
        Felucca_Shrine_Sacrifice = 230700,
        Felucca_Shrine_Spirituality = 230800,
        Felucca_Shrine_Valor = 230900,

        Felucca_Internal_JailCells = 240100,
        Felucca_Internal_GreenAcres = 240200,

        Felucca_Faction_CouncilOfMages = 250100,
        Felucca_Faction_Minax = 250200,
        Felucca_Faction_Shadowlords = 250300,
        Felucca_Faction_TrueBritannians = 250400,

        // ==========================================
        // 🔮 [3] 일쉐나 (Ilshenar)
        // ==========================================
        Ilshenar_City_AncientCitadel = 310100,
        Ilshenar_City_GargoyleCity = 310200,
        Ilshenar_City_Lakeshire = 310300,
        Ilshenar_City_Mistas = 310400,
        Ilshenar_City_Montor = 310500,
        Ilshenar_City_ReqVolon = 310600,
        Ilshenar_City_SavageCamp = 310700,
        Ilshenar_City_TerortSkitas = 310800,

        Ilshenar_Dungeon_Ankh = 320100,
        Ilshenar_Dungeon_Ankh_Level1 = 320101,
        Ilshenar_Dungeon_Ankh_KirinPassage = 320102,
        Ilshenar_Dungeon_Ankh_SerpentinePassage = 320103,
        Ilshenar_Dungeon_Blood = 320200,
        Ilshenar_Dungeon_Blood_Level1 = 320201,
        Ilshenar_Dungeon_Exodus = 320300,
        Ilshenar_Dungeon_Exodus_Level1 = 320301,
        Ilshenar_Dungeon_Rock = 320400,
        Ilshenar_Dungeon_Rock_Level1 = 320401,
        Ilshenar_Dungeon_Rock_Level2 = 320402,
        Ilshenar_Dungeon_Sorcerers = 320500,
        Ilshenar_Dungeon_Sorcerers_Level1 = 320501,
        Ilshenar_Dungeon_Sorcerers_Level2 = 320502,
        Ilshenar_Dungeon_Sorcerers_Level3 = 320503,
        Ilshenar_Dungeon_Sorcerers_Level4 = 320504,
        Ilshenar_Dungeon_Sorcerers_Level5 = 320505,
        Ilshenar_Dungeon_Spectre = 320600,
        Ilshenar_Dungeon_Spectre_Level1 = 320601,
        Ilshenar_Dungeon_Wisp = 320700,
        Ilshenar_Dungeon_Wisp_Level1 = 320701,
        Ilshenar_Dungeon_Wisp_Level2 = 320702,
        Ilshenar_Dungeon_Wisp_Level3 = 320703,
        Ilshenar_Dungeon_Wisp_Level4 = 320704,
        Ilshenar_Dungeon_Wisp_Level5 = 320705,
        Ilshenar_Dungeon_Wisp_Level6 = 320706,
        Ilshenar_Dungeon_Wisp_Level7 = 320707,
        Ilshenar_Dungeon_Wisp_Level8 = 320708,

        Ilshenar_Dungeon_AncientLair = 320800,
        Ilshenar_Dungeon_AncientLair_Level1 = 320801,
        Ilshenar_Dungeon_LizardPassage = 320900,
        Ilshenar_Dungeon_LizardPassage_Level1 = 320901,
        Ilshenar_Dungeon_LizardPassage_Level2 = 320902,
        Ilshenar_Dungeon_MushroomCave = 321000, 
        Ilshenar_Dungeon_RatCave = 321100,
        Ilshenar_Dungeon_RatCave_Level1 = 321101,
        Ilshenar_Dungeon_RatCave_Level2 = 321102,
        Ilshenar_Dungeon_SpiderCave = 321200,
        Ilshenar_Dungeon_SpiderCave_Level1 = 321201,
        Ilshenar_Dungeon_SpiderCave_Level2 = 321202,
        Ilshenar_Dungeon_SpiderCave_EtherealKeep = 321203,

        Ilshenar_Shrine_Compassion = 330100,
        Ilshenar_Shrine_Honesty = 330200,
        Ilshenar_Shrine_Honor = 330300,
        Ilshenar_Shrine_Humility = 330400,
        Ilshenar_Shrine_Justice = 330500,
        Ilshenar_Shrine_Sacrifice = 330600,
        Ilshenar_Shrine_Spirituality = 330700,
        Ilshenar_Shrine_Valor = 330800,

        // ==========================================
        // 🌌 [4] 말라스 (Malas)
        // ==========================================
        Malas_Town_Luna = 410100,
        Malas_Town_Umbra = 410200,

        Malas_Dungeon_Doom = 420100,
        Malas_Dungeon_Doom_Tunnel = 420101,
        Malas_Dungeon_Doom_Inside = 420102,
        Malas_Dungeon_Doom_Gauntlet = 420103,
        Malas_Dungeon_Doom_GuardiansRoom = 420104,
        Malas_Dungeon_Doom_LampRoom = 420105,
        Malas_Dungeon_Labyrinth = 420200, 
        Malas_Dungeon_Bedlam = 420300,    
        Malas_Dungeon_TheCitadel = 420400, 

        Malas_Site_BrokenMountains = 430100,
        Malas_Site_CorruptedForest = 430200,
        Malas_Site_CrumblingContinent = 430300,
        Malas_Site_CrystalFens = 430400,
        Malas_Site_DivideOfTheAbyss = 430500,
        Malas_Site_DryHighlands = 430600,
        Malas_Site_ForgottenPyramid = 430700,
        Malas_Site_GravewaterLake = 430800,
        Malas_Site_GrimswindRuins = 430900,
        Malas_Site_NorthernCrags = 431000,
        Malas_Site_HansesHostel = 431100,
        
        Malas_Site_OrcFortress1 = 431200,
        Malas_Site_OrcFortress2 = 431300,
        Malas_Site_OrcFortress3 = 431400,
        Malas_Site_OrcFortress4 = 431500,
        Malas_Site_OrcFortress5 = 431600,
        Malas_Site_OrcFortress6 = 431700,

        Malas_Site_Mine1 = 431800,
        Malas_Site_Mine2 = 431900,
        Malas_Site_Mine3 = 432000,
        Malas_Site_Mine4 = 432100,
        Malas_Site_Mine5 = 432200,
        Malas_Site_Mine6 = 432300,
        Malas_Site_Mine7 = 432400,
        Malas_Site_Mine8 = 432500,
        Malas_Site_Mine9 = 432600,

        // ==========================================
        // 🏯 [5] 토쿠노 (Tokuno)
        // ==========================================
        Tokuno_Town_Zento = 510100,

        Tokuno_Dungeon_FanDancersDojo = 520100, 
        Tokuno_Dungeon_YomotsuMines = 520200,   

        Tokuno_Site_MakotoJima = 530100,
        Tokuno_Site_IsamuJima = 530200,
        Tokuno_Site_HomareJima = 530300,

        // ==========================================
        // 🦇 [6] 터머 (Ter Mur)
        // ==========================================
        TerMur_Town_RoyalCity = 610100,
        TerMur_Town_HolyCity = 610200,
        TerMur_Town_Dugan = 610300,

        TerMur_Dungeon_TombOfKings = 620100, 
        TerMur_Dungeon_StygianAbyss = 620200,
        TerMur_Dungeon_StygianAbyss_AbyssalLair = 620201,
        TerMur_Dungeon_StygianAbyss_Cavern = 620202,
        TerMur_Dungeon_StygianAbyss_ClanScratch = 620203,
        TerMur_Dungeon_StygianAbyss_CrimsonVeins = 620204,
        TerMur_Dungeon_StygianAbyss_EnslavedGoblins = 620205,
        TerMur_Dungeon_StygianAbyss_FairyDragonLair = 620206,
        TerMur_Dungeon_StygianAbyss_FireTemple = 620207,
        TerMur_Dungeon_StygianAbyss_Hydra = 620208,
        TerMur_Dungeon_StygianAbyss_LandsOfLich = 620209,
        TerMur_Dungeon_StygianAbyss_LavaCaldera = 620210,
        TerMur_Dungeon_StygianAbyss_MedusaLair = 620211,
        TerMur_Dungeon_StygianAbyss_PassageOfTears = 620212,
        TerMur_Dungeon_StygianAbyss_SecretGarden = 620213,
        TerMur_Dungeon_StygianAbyss_SerpentLair = 620214,
        TerMur_Dungeon_StygianAbyss_SilverSapling = 620215,
        TerMur_Dungeon_StygianAbyss_StygianDragon = 620216,
        TerMur_Dungeon_StygianAbyss_Sutek = 620217,
        TerMur_Dungeon_Underworld = 620300, 

        TerMur_Site_AtollBend = 630100,
        TerMur_Site_ChickenChase = 630200,
        TerMur_Site_CityResidential = 630300,
        TerMur_Site_CoralDesert = 630400,
        TerMur_Site_FishermansReach = 630500,
        TerMur_Site_GatedIsle = 630600,
        TerMur_Site_HighPlain = 630700,
        TerMur_Site_HolyCityIsland = 630800,
        TerMur_Site_KepetchWaste = 630900,
        TerMur_Site_LavaLake = 631000,
        TerMur_Site_LavapitPyramid = 631100,
        TerMur_Site_LostSettlement = 631200,
        TerMur_Site_NorthernSteppes = 631300,
        TerMur_Site_RaptorIsland = 631400,
        TerMur_Site_RoyalPark = 631500,
        TerMur_Site_ShrineOfSingularity = 631600,
        TerMur_Site_SlithValley = 631700,
        TerMur_Site_SpiderIsland = 631800,
        TerMur_Site_SpidersGuarde = 631900,
        TerMur_Site_TalonPoint = 632000,
        TerMur_Site_TreefellowCourse = 632100,
        TerMur_Site_VoidIsle = 632200,
        TerMur_Site_WalledCircus = 632300,
        TerMur_Site_WaterfallPoint = 632400,
    } // <-- RegionCode Enum 종료 지점

    // ==============================================================================
    // 🌟 [추가됨] 클래스 선언 및 RegionBounds 레코드 정의
    // ==============================================================================
   // ==============================================================================
    // 🌟 [추가됨] 클래스 선언 및 RegionBounds 레코드 정의
    // ==============================================================================
    public static class RegionSaver
    {
        // 🌟 Z축 방어 및 층별(Level) Bounding Box 최적화 레코드
        public record RegionBounds(Map Facet, int StartX, int StartY, int EndX, int EndY, int MinZ, int MaxZ, RegionCode Code);

        // ==============================================================================
        // 🌟 [전 대륙 통합 좌표 데이터베이스] - 던전 층(Level)별 독립 영역 압축 완료
        // ==============================================================================
        private static readonly List<RegionBounds> m_Regions =
        [
            // ==========================================
            // 🌲 [1] 트라멜 (Trammel) - 마을
            // ==========================================
            new(Map.Trammel, 1093, 1408, 1740, 1907, -255, 255, RegionCode.Trammel_Town_Britain),
            new(Map.Trammel, 1466, 1375, 1582, 1505, -255, 255, RegionCode.Trammel_Town_Britain_BlackthornCastle),
            new(Map.Trammel, 1472, 1487, 1576, 1520, -255, 255, RegionCode.Trammel_Town_Britain_BlackthornEntrance),
            new(Map.Trammel, 1224, 1552, 1414, 1704, -255, 255, RegionCode.Trammel_Town_Britain_BritishCastle),
            new(Map.Trammel, 1361, 1553, 1414, 1704, -255, 255, RegionCode.Trammel_Town_Britain_BritishEntrance),
            new(Map.Trammel, 1333, 1441, 1472, 1528, -255, 255, RegionCode.Trammel_Town_Britain_Cemetery),
            new(Map.Trammel, 1420, 1550, 1508, 1746, -255, 255, RegionCode.Trammel_Town_Britain_Center),
            new(Map.Trammel, 1100, 1550, 1343, 2004, -255, 255, RegionCode.Trammel_Town_Britain_Farmlands),
            new(Map.Trammel, 1544, 1584, 1630, 1776, -255, 255, RegionCode.Trammel_Town_Britain_Park),
            new(Map.Trammel, 1580, 1510, 1710, 1592, -255, 255, RegionCode.Trammel_Town_Britain_Suburbs),

            new(Map.Trammel, 2592, 2057, 2887, 2303, -255, 255, RegionCode.Trammel_Town_BuccaneersDen),
            new(Map.Trammel, 2655, 2072, 2728, 2104, -255, 255, RegionCode.Trammel_Town_BuccaneersDen_Bathhouse),
            new(Map.Trammel, 2664, 2155, 2852, 2257, -255, 255, RegionCode.Trammel_Town_BuccaneersDen_Docks),
            new(Map.Trammel, 2592, 2057, 2887, 2303, -255, 255, RegionCode.Trammel_Town_BuccaneersDen_Tunnels),

            new(Map.Trammel, 2200, 1110, 2286, 1246, -255, 255, RegionCode.Trammel_Town_Cove),
            new(Map.Trammel, 2422, 1078, 2588, 1125, -255, 255, RegionCode.Trammel_Town_Cove_Cemetery),
            new(Map.Trammel, 2354, 1176, 2755, 1233, -255, 255, RegionCode.Trammel_Town_Cove_Gates),
            new(Map.Trammel, 2208, 1112, 2238, 1160, -255, 255, RegionCode.Trammel_Town_Cove_GuardPost),
            new(Map.Trammel, 2155, 1315, 2225, 1420, -255, 255, RegionCode.Trammel_Town_Cove_OrcFort),

            new(Map.Trammel, 688, 3373, 2100, 4080, -255, 255, RegionCode.Trammel_Town_Jhelom),
            new(Map.Trammel, 1272, 3712, 1300, 3750, -255, 255, RegionCode.Trammel_Town_Jhelom_Cemetery),
            new(Map.Trammel, 1480, 3670, 1515, 3778, -255, 255, RegionCode.Trammel_Town_Jhelom_EastDocks),
            new(Map.Trammel, 1376, 3728, 1424, 3760, -255, 255, RegionCode.Trammel_Town_Jhelom_FightingPit),
            new(Map.Trammel, 1224, 3592, 1533, 4065, -255, 255, RegionCode.Trammel_Town_Jhelom_MainIsland),
            new(Map.Trammel, 1078, 3373, 1243, 3709, -255, 255, RegionCode.Trammel_Town_Jhelom_MediumIsland),
            new(Map.Trammel, 1395, 3957, 1475, 4042, -255, 255, RegionCode.Trammel_Town_Jhelom_SmallIsland),

            new(Map.Trammel, 3624, 2032, 3812, 2303, -255, 255, RegionCode.Trammel_Town_Magincia),
            new(Map.Trammel, 3710, 2121, 3747, 2189, -255, 255, RegionCode.Trammel_Town_Magincia_Bank),
            new(Map.Trammel, 3554, 2132, 3710, 2312, -255, 255, RegionCode.Trammel_Town_Magincia_Docks),
            new(Map.Trammel, 3705, 2047, 3736, 2086, -255, 255, RegionCode.Trammel_Town_Magincia_Park),
            new(Map.Trammel, 3785, 2233, 3808, 2272, -255, 255, RegionCode.Trammel_Town_Magincia_Parliament),

            new(Map.Trammel, 2411, 366, 2628, 690, -255, 255, RegionCode.Trammel_Town_Minoc),
            new(Map.Trammel, 2522, 486, 2551, 509, -255, 255, RegionCode.Trammel_Town_Minoc_Bridge),
            new(Map.Trammel, 2502, 630, 2564, 682, -255, 255, RegionCode.Trammel_Town_Minoc_GypsyCamp),
            new(Map.Trammel, 2563, 515, 2596, 541, -255, 255, RegionCode.Trammel_Town_Minoc_MiningCamp),
            new(Map.Trammel, 2406, 39, 2522, 482, -255, 255, RegionCode.Trammel_Town_Minoc_North),
            new(Map.Trammel, 2463, 552, 2544, 609, -255, 255, RegionCode.Trammel_Town_Minoc_South),

            new(Map.Trammel, 4278, 844, 4726, 1509, -255, 255, RegionCode.Trammel_Town_Moonglow),
            new(Map.Trammel, 4459, 1276, 4556, 1334, -255, 255, RegionCode.Trammel_Town_Moonglow_Cemetery),
            new(Map.Trammel, 4380, 1050, 4492, 1183, -255, 255, RegionCode.Trammel_Town_Moonglow_Center),
            new(Map.Trammel, 4384, 1023, 4427, 1049, -255, 255, RegionCode.Trammel_Town_Moonglow_Docks),
            new(Map.Trammel, 4696, 1110, 4714, 1145, -255, 255, RegionCode.Trammel_Town_Moonglow_Telescope),
            new(Map.Trammel, 4488, 1354, 4577, 1481, -255, 255, RegionCode.Trammel_Town_Moonglow_Zoo),

            new(Map.Trammel, 3475, 1000, 3835, 1435, -255, 255, RegionCode.Trammel_Town_Nujelm),
            new(Map.Trammel, 3505, 1122, 3550, 1160, -255, 255, RegionCode.Trammel_Town_Nujelm_Cemetery),
            new(Map.Trammel, 3720, 1312, 3751, 1369, -255, 255, RegionCode.Trammel_Town_Nujelm_ChessBoard),
            new(Map.Trammel, 3800, 1272, 3807, 1286, -255, 255, RegionCode.Trammel_Town_Nujelm_Docks),
            new(Map.Trammel, 3728, 1184, 3785, 1272, -255, 255, RegionCode.Trammel_Town_Nujelm_East),
            new(Map.Trammel, 3654, 1055, 3700, 1172, -255, 255, RegionCode.Trammel_Town_Nujelm_North),
            new(Map.Trammel, 3668, 1216, 3751, 1336, -255, 255, RegionCode.Trammel_Town_Nujelm_Palace),
            new(Map.Trammel, 3512, 1170, 3630, 1285, -255, 255, RegionCode.Trammel_Town_Nujelm_West),

            new(Map.Trammel, 3314, 2345, 3814, 3095, -255, 255, RegionCode.Trammel_Town_Haven),
            new(Map.Trammel, 3314, 2345, 3814, 3095, -255, 255, RegionCode.Trammel_Town_Haven_OldHaven),
            new(Map.Trammel, 3589, 2443, 3704, 2543, -255, 255, RegionCode.Trammel_Town_Haven_OldHavenNorth),
            new(Map.Trammel, 3408, 2480, 3543, 2782, -255, 255, RegionCode.Trammel_Town_Haven_NewHaven),
            new(Map.Trammel, 3415, 2488, 3537, 2531, -255, 255, RegionCode.Trammel_Town_Haven_NewHavenNorth),
            new(Map.Trammel, 3350, 2567, 3850, 3429, -255, 255, RegionCode.Trammel_Town_Haven_Farmlands),

            new(Map.Trammel, 2868, 3324, 3073, 3519, -255, 255, RegionCode.Trammel_Town_SerpentsHold),
            new(Map.Trammel, 3008, 3450, 3022, 3464, -255, 255, RegionCode.Trammel_Town_SerpentsHold_North),
            new(Map.Trammel, 2884, 3537, 2897, 3550, -255, 255, RegionCode.Trammel_Town_SerpentsHold_South),

            new(Map.Trammel, 538, 2107, 688, 2297, -255, 255, RegionCode.Trammel_Town_SkaraBrae),
            new(Map.Trammel, 816, 2251, 832, 2289, -255, 255, RegionCode.Trammel_Town_SkaraBrae_East),
            new(Map.Trammel, 796, 2152, 832, 2176, -255, 255, RegionCode.Trammel_Town_SkaraBrae_North),
            new(Map.Trammel, 816, 2344, 851, 2368, -255, 255, RegionCode.Trammel_Town_SkaraBrae_South),
            new(Map.Trammel, 552, 2062, 650, 2192, -255, 255, RegionCode.Trammel_Town_SkaraBrae_West),
            new(Map.Trammel, 592, 2232, 624, 2256, -255, 255, RegionCode.Trammel_Town_SkaraBrae_WestDocks),

            new(Map.Trammel, 1796, 2636, 2117, 2954, -255, 255, RegionCode.Trammel_Town_Trinsic),
            new(Map.Trammel, 1923, 2786, 1942, 2808, -255, 255, RegionCode.Trammel_Town_Trinsic_Center),
            new(Map.Trammel, 2024, 2784, 2040, 2813, -255, 255, RegionCode.Trammel_Town_Trinsic_EastDocks),
            new(Map.Trammel, 1823, 2943, 1834, 2954, -255, 255, RegionCode.Trammel_Town_Trinsic_South),
            new(Map.Trammel, 1834, 2728, 1856, 2744, -255, 255, RegionCode.Trammel_Town_Trinsic_WestGate),

            new(Map.Trammel, 2728, 598, 3065, 1054, -255, 255, RegionCode.Trammel_Town_Vesper),
            new(Map.Trammel, 2892, 901, 2908, 920, -255, 255, RegionCode.Trammel_Town_Vesper_Cemetery),
            new(Map.Trammel, 2908, 904, 2916, 912, -255, 255, RegionCode.Trammel_Town_Vesper_Center),
            new(Map.Trammel, 2952, 864, 2976, 896, -255, 255, RegionCode.Trammel_Town_Vesper_Docks),
            new(Map.Trammel, 2710, 952, 2792, 1054, -255, 255, RegionCode.Trammel_Town_Vesper_East),

            new(Map.Trammel, 5132, 3, 5366, 204, -255, 255, RegionCode.Trammel_Town_Wind),
            new(Map.Trammel, 5303, 28, 5319, 42, -255, 255, RegionCode.Trammel_Town_Wind_East),
            new(Map.Trammel, 5159, 15, 5184, 40, -255, 255, RegionCode.Trammel_Town_Wind_Park),
            new(Map.Trammel, 5206, 159, 5228, 183, -255, 255, RegionCode.Trammel_Town_Wind_South),

            new(Map.Trammel, 92, 656, 756, 1261, -255, 255, RegionCode.Trammel_Town_Yew),
            new(Map.Trammel, 560, 1168, 686, 1248, -255, 255, RegionCode.Trammel_Town_Yew_Cemetery),
            new(Map.Trammel, 368, 1088, 582, 1208, -255, 255, RegionCode.Trammel_Town_Yew_Center),
            new(Map.Trammel, 600, 741, 780, 950, -255, 255, RegionCode.Trammel_Town_Yew_EmpathAbbey),

            new(Map.Trammel, 5123, 3930, 5315, 4084, -255, 255, RegionCode.Trammel_Town_Delucia),
            new(Map.Trammel, 5194, 4053, 5204, 4073, -255, 255, RegionCode.Trammel_Town_Delucia_Center),

            new(Map.Trammel, 5639, 3095, 5851, 3318, -255, 255, RegionCode.Trammel_Town_Papua),
            new(Map.Trammel, 5757, 3150, 5781, 3174, -255, 255, RegionCode.Trammel_Town_Papua_TheJustInn),
            new(Map.Trammel, 6466, 73, 7168, 549, -255, 255, RegionCode.Trammel_Town_Heartwood),

            // ==========================================
            // 🦇 [2] 트라멜 (Trammel) - 층(Level)별 던전 분리
            // ==========================================
            new(Map.Trammel, 6440, 820, 6600, 970, -255, 255, RegionCode.Trammel_Dungeon_BlightedGrove),

            // Covetous: 층별 독립 분리
            new(Map.Trammel, 5376, 1840, 5511, 1944, -255, 255, RegionCode.Trammel_Dungeon_Covetous_Level1),
            new(Map.Trammel, 5376, 1952, 5633, 2048, -255, 255, RegionCode.Trammel_Dungeon_Covetous_Level2),
            new(Map.Trammel, 5533, 1822, 5630, 1925, -255, 255, RegionCode.Trammel_Dungeon_Covetous_Level3),
            new(Map.Trammel, 5395, 1790, 5488, 1835, -255, 255, RegionCode.Trammel_Dungeon_Covetous_LakeCave),
            new(Map.Trammel, 5497, 1798, 5555, 1818, -255, 255, RegionCode.Trammel_Dungeon_Covetous_TortureChambers),

            // Deceit: 층별 독립 분리
            new(Map.Trammel, 5120, 512, 5247, 639, -255, 255, RegionCode.Trammel_Dungeon_Deceit_Level1),
            new(Map.Trammel, 5248, 512, 5375, 639, -255, 255, RegionCode.Trammel_Dungeon_Deceit_Level2),
            new(Map.Trammel, 5120, 640, 5247, 767, -255, 255, RegionCode.Trammel_Dungeon_Deceit_Level3),
            new(Map.Trammel, 5248, 640, 5375, 767, -255, 255, RegionCode.Trammel_Dungeon_Deceit_Level4),

            // Despise: 층별 독립 분리
            new(Map.Trammel, 5376, 512, 5515, 636, -255, 255, RegionCode.Trammel_Dungeon_Despise_Level1),
            new(Map.Trammel, 5376, 650, 5535, 766, -255, 255, RegionCode.Trammel_Dungeon_Despise_Level2),
            new(Map.Trammel, 5376, 770, 5631, 1023, -255, 255, RegionCode.Trammel_Dungeon_Despise_Level3),

            // Destard: 층별 독립 분리
            new(Map.Trammel, 5120, 768, 5247, 895, -255, 255, RegionCode.Trammel_Dungeon_Destard_Level1),
            new(Map.Trammel, 5248, 768, 5375, 895, -255, 255, RegionCode.Trammel_Dungeon_Destard_Level2),
            new(Map.Trammel, 5120, 896, 5375, 1023, -255, 255, RegionCode.Trammel_Dungeon_Destard_Level3),

            // Hythloth: 층별 독립 분리 (이전 병합 오류 해결)
            new(Map.Trammel, 5888, 0, 6015, 127, -255, 255, RegionCode.Trammel_Dungeon_Hythloth_Level1),
            new(Map.Trammel, 6016, 0, 6143, 127, -255, 255, RegionCode.Trammel_Dungeon_Hythloth_Level2),
            new(Map.Trammel, 5888, 128, 6015, 255, -255, 255, RegionCode.Trammel_Dungeon_Hythloth_Level3),
            new(Map.Trammel, 6016, 128, 6143, 255, -255, 255, RegionCode.Trammel_Dungeon_Hythloth_Level4),

            // Shame: 층별 독립 분리
            new(Map.Trammel, 5376, 0, 5495, 127, -255, 255, RegionCode.Trammel_Dungeon_Shame_Level1),
            new(Map.Trammel, 5496, 0, 5631, 127, -255, 255, RegionCode.Trammel_Dungeon_Shame_Level2),
            new(Map.Trammel, 5376, 128, 5631, 255, -255, 255, RegionCode.Trammel_Dungeon_Shame_Level3),
            new(Map.Trammel, 5632, 0, 5887, 127, -255, 255, RegionCode.Trammel_Dungeon_Shame_Level4),

            // Wrong: 층별 독립 분리
            new(Map.Trammel, 5632, 512, 5887, 620, -255, 255, RegionCode.Trammel_Dungeon_Wrong_Level1),
            new(Map.Trammel, 5632, 621, 5887, 720, -255, 255, RegionCode.Trammel_Dungeon_Wrong_Level2),
            new(Map.Trammel, 5632, 721, 5887, 1023, -255, 255, RegionCode.Trammel_Dungeon_Wrong_Level3),

            // 특수 던전들
            new(Map.Trammel, 5381, 1284, 5628, 1509, -255, 255, RegionCode.Felucca_Dungeon_Khaldun),
            new(Map.Trammel, 5120, 1530, 5481, 3167, -255, 255, RegionCode.Trammel_Dungeon_TerathanKeep),
            new(Map.Trammel, 2960, 1285, 6564, 3432, -255, 255, RegionCode.Trammel_Dungeon_Fire),
            new(Map.Trammel, 4529, 130, 5888, 2408, -255, 255, RegionCode.Trammel_Dungeon_Ice),
            new(Map.Trammel, 5127, 1283, 5373, 2046, -255, 255, RegionCode.Trammel_Dungeon_OrcCave),
            new(Map.Trammel, 6240, 860, 6310, 920, -255, 255, RegionCode.Trammel_Dungeon_PaintedCaves),
            new(Map.Trammel, 5620, 311, 6561, 3042, -255, 255, RegionCode.Trammel_Dungeon_PalaceOfParoxysmus),
            new(Map.Trammel, 6400, 0, 6621, 255, -255, 255, RegionCode.Trammel_Dungeon_PrismOfLight),
            new(Map.Trammel, 759, 4, 6399, 1697, -255, 255, RegionCode.Trammel_Dungeon_Sanctuary),
            new(Map.Trammel, 1595, 550, 5935, 2039, -255, 255, RegionCode.Trammel_Dungeon_SolenHives),

            // 신전 및 특수
            new(Map.Trammel, 1456, 840, 1460, 847, -255, 255, RegionCode.Trammel_Shrine_Chaos),
            new(Map.Trammel, 1851, 867, 1865, 881, -255, 255, RegionCode.Trammel_Shrine_Compassion),
            new(Map.Trammel, 4209, 560, 4216, 568, -255, 255, RegionCode.Trammel_Shrine_Honesty),
            new(Map.Trammel, 1721, 3525, 1729, 3531, -255, 255, RegionCode.Trammel_Shrine_Honor),
            new(Map.Trammel, 4038, 3303, 4279, 3703, -255, 255, RegionCode.Trammel_Shrine_Humility),
            new(Map.Trammel, 1297, 629, 1306, 638, -255, 255, RegionCode.Trammel_Shrine_Justice),
            new(Map.Trammel, 3352, 286, 3358, 293, -255, 255, RegionCode.Trammel_Shrine_Sacrifice),
            new(Map.Trammel, 1302, 2331, 1609, 2496, -255, 255, RegionCode.Trammel_Shrine_Spirituality),
            new(Map.Trammel, 2488, 3928, 2497, 3939, -255, 255, RegionCode.Trammel_Shrine_Valor),
            new(Map.Trammel, 5271, 1159, 5312, 1192, -255, 255, RegionCode.Trammel_Internal_JailCells),
            new(Map.Trammel, 5376, 512, 6143, 1279, -255, 255, RegionCode.Trammel_Internal_GreenAcres),

            // ==========================================
            // 💀 [2] 펠루카 (Felucca) - 마을 (트라멜과 면적 동일)
            // ==========================================
            new(Map.Felucca, 1093, 1408, 1740, 1907, -255, 255, RegionCode.Felucca_Town_Britain),
            new(Map.Felucca, 3554, 2032, 3812, 2312, -255, 255, RegionCode.Felucca_Town_Magincia),
            new(Map.Felucca, 2406, 39, 2628, 690, -255, 255, RegionCode.Felucca_Town_Minoc),
            new(Map.Felucca, 4278, 844, 4726, 1509, -255, 255, RegionCode.Felucca_Town_Moonglow),
            new(Map.Felucca, 538, 2062, 851, 2368, -255, 255, RegionCode.Felucca_Town_SkaraBrae),
            new(Map.Felucca, 1796, 2636, 2117, 2954, -255, 255, RegionCode.Felucca_Town_Trinsic),
            new(Map.Felucca, 2728, 598, 3065, 1013, -255, 255, RegionCode.Felucca_Town_Vesper),
            new(Map.Felucca, 92, 656, 780, 1261, -255, 255, RegionCode.Felucca_Town_Yew),
            new(Map.Felucca, 688, 3373, 2100, 4080, -255, 255, RegionCode.Felucca_Town_Jhelom),
            new(Map.Felucca, 3587, 2456, 3768, 2712, -255, 255, RegionCode.Felucca_Town_Ocllo),
            new(Map.Felucca, 2868, 3324, 3073, 3519, -255, 255, RegionCode.Felucca_Town_SerpentsHold),
            new(Map.Felucca, 5132, 3, 5366, 204, -255, 255, RegionCode.Felucca_Town_Wind),
            new(Map.Felucca, 5123, 3930, 5315, 4084, -255, 255, RegionCode.Felucca_Town_Delucia),
            new(Map.Felucca, 5639, 3095, 5851, 3318, -255, 255, RegionCode.Felucca_Town_Papua),

            // ==========================================
            // 🦇 [2] 펠루카 (Felucca) - 층(Level)별 던전 분리
            // ==========================================
            new(Map.Felucca, 6440, 820, 6600, 970, -255, 255, RegionCode.Felucca_Dungeon_BlightedGrove),

            // Covetous: 층별 독립 분리
            new(Map.Felucca, 5376, 1840, 5511, 1944, -255, 255, RegionCode.Felucca_Dungeon_Covetous_Level1),
            new(Map.Felucca, 5376, 1952, 5633, 2048, -255, 255, RegionCode.Felucca_Dungeon_Covetous_Level2),
            new(Map.Felucca, 5533, 1822, 5630, 1925, -255, 255, RegionCode.Felucca_Dungeon_Covetous_Level3),

            // Deceit: 층별 독립 분리
            new(Map.Felucca, 5120, 512, 5247, 639, -255, 255, RegionCode.Felucca_Dungeon_Deceit_Level1),
            new(Map.Felucca, 5248, 512, 5375, 639, -255, 255, RegionCode.Felucca_Dungeon_Deceit_Level2),
            new(Map.Felucca, 5120, 640, 5247, 767, -255, 255, RegionCode.Felucca_Dungeon_Deceit_Level3),
            new(Map.Felucca, 5248, 640, 5375, 767, -255, 255, RegionCode.Felucca_Dungeon_Deceit_Level4),

            // Despise: 층별 독립 분리
            new(Map.Felucca, 5376, 512, 5515, 636, -255, 255, RegionCode.Felucca_Dungeon_Despise_Level1),
            new(Map.Felucca, 5376, 650, 5535, 766, -255, 255, RegionCode.Felucca_Dungeon_Despise_Level2),
            new(Map.Felucca, 5376, 770, 5631, 1023, -255, 255, RegionCode.Felucca_Dungeon_Despise_Level3),

            // Destard: 층별 독립 분리
            new(Map.Felucca, 5120, 768, 5247, 895, -255, 255, RegionCode.Felucca_Dungeon_Destard_Level1),
            new(Map.Felucca, 5248, 768, 5375, 895, -255, 255, RegionCode.Felucca_Dungeon_Destard_Level2),
            new(Map.Felucca, 5120, 896, 5375, 1023, -255, 255, RegionCode.Felucca_Dungeon_Destard_Level3),

            // Hythloth: 층별 독립 분리
            new(Map.Felucca, 5888, 0, 6015, 127, -255, 255, RegionCode.Felucca_Dungeon_Hythloth_Level1),
            new(Map.Felucca, 6016, 0, 6143, 127, -255, 255, RegionCode.Felucca_Dungeon_Hythloth_Level2),
            new(Map.Felucca, 5888, 128, 6015, 255, -255, 255, RegionCode.Felucca_Dungeon_Hythloth_Level3),
            new(Map.Felucca, 6016, 128, 6143, 255, -255, 255, RegionCode.Felucca_Dungeon_Hythloth_Level4),

            // Shame: 층별 독립 분리
            new(Map.Felucca, 5376, 0, 5495, 127, -255, 255, RegionCode.Felucca_Dungeon_Shame_Level1),
            new(Map.Felucca, 5496, 0, 5631, 127, -255, 255, RegionCode.Felucca_Dungeon_Shame_Level2),
            new(Map.Felucca, 5376, 128, 5631, 255, -255, 255, RegionCode.Felucca_Dungeon_Shame_Level3),
            new(Map.Felucca, 5632, 0, 5887, 127, -255, 255, RegionCode.Felucca_Dungeon_Shame_Level4),

            // Wrong: 층별 독립 분리
            new(Map.Felucca, 5632, 512, 5887, 620, -255, 255, RegionCode.Felucca_Dungeon_Wrong_Level1),
            new(Map.Felucca, 5632, 621, 5887, 720, -255, 255, RegionCode.Felucca_Dungeon_Wrong_Level2),
            new(Map.Felucca, 5632, 721, 5887, 1023, -255, 255, RegionCode.Felucca_Dungeon_Wrong_Level3),

            // 특수 던전
            new(Map.Felucca, 5381, 1284, 5628, 1509, -255, 255, RegionCode.Felucca_Dungeon_Khaldun),
            new(Map.Felucca, 5120, 1530, 5481, 3167, -255, 255, RegionCode.Felucca_Dungeon_TerathanKeep),
            new(Map.Felucca, 2960, 1281, 6564, 3432, -255, 255, RegionCode.Felucca_Dungeon_Fire),
            new(Map.Felucca, 4529, 130, 5888, 2408, -255, 255, RegionCode.Felucca_Dungeon_Ice),
            new(Map.Felucca, 5127, 1283, 5373, 2046, -255, 255, RegionCode.Felucca_Dungeon_OrcCave),
            new(Map.Felucca, 6240, 860, 6310, 920, -255, 255, RegionCode.Felucca_Dungeon_PaintedCaves),
            new(Map.Felucca, 5620, 311, 6561, 3042, -255, 255, RegionCode.Felucca_Dungeon_PalaceOfParoxysmus),
            new(Map.Felucca, 6400, 0, 6621, 255, -255, 255, RegionCode.Felucca_Dungeon_PrismOfLight),
            new(Map.Felucca, 759, 4, 6399, 1697, -255, 255, RegionCode.Felucca_Dungeon_Sanctuary),
            new(Map.Felucca, 1595, 550, 5935, 2039, -255, 255, RegionCode.Felucca_Dungeon_SolenHives),

            new(Map.Felucca, 1456, 840, 1460, 847, -255, 255, RegionCode.Felucca_Shrine_Chaos),
            new(Map.Felucca, 1713, 867, 1865, 1080, -255, 255, RegionCode.Felucca_Shrine_Compassion),
            new(Map.Felucca, 4209, 560, 4216, 568, -255, 255, RegionCode.Felucca_Shrine_Honesty),
            new(Map.Felucca, 1721, 3525, 1729, 3531, -255, 255, RegionCode.Felucca_Shrine_Honor),
            new(Map.Felucca, 4270, 3694, 4279, 3703, -255, 255, RegionCode.Felucca_Shrine_Humility),
            new(Map.Felucca, 1297, 629, 1306, 638, -255, 255, RegionCode.Felucca_Shrine_Justice),
            new(Map.Felucca, 3352, 286, 3358, 293, -255, 255, RegionCode.Felucca_Shrine_Sacrifice),
            new(Map.Felucca, 1590, 2485, 1609, 2496, -255, 255, RegionCode.Felucca_Shrine_Spirituality),
            new(Map.Felucca, 2488, 3928, 2497, 3939, -255, 255, RegionCode.Felucca_Shrine_Valor),
            new(Map.Felucca, 5271, 1159, 5312, 1192, -255, 255, RegionCode.Felucca_Internal_JailCells),
            new(Map.Felucca, 5376, 512, 6143, 1279, -255, 255, RegionCode.Felucca_Internal_GreenAcres),
            new(Map.Felucca, 3804, 1260, 3824, 1280, -255, 255, RegionCode.Felucca_Faction_CouncilOfMages),
            new(Map.Felucca, 2680, 3030, 2740, 3100, -255, 255, RegionCode.Felucca_Faction_Minax),

            // ==========================================
            // 🔮 [3] 일쉐나 (Ilshenar) - 최외곽선 병합 적용
            // ==========================================
            new(Map.Ilshenar, 1448, 496, 1632, 640, -255, 255, RegionCode.Ilshenar_City_AncientCitadel),
            new(Map.Ilshenar, 736, 480, 950, 750, -255, 255, RegionCode.Ilshenar_City_GargoyleCity),
            new(Map.Ilshenar, 1144, 1072, 1264, 1200, -255, 255, RegionCode.Ilshenar_City_Lakeshire),
            new(Map.Ilshenar, 744, 984, 912, 1176, -255, 255, RegionCode.Ilshenar_City_Mistas),
            new(Map.Ilshenar, 1520, 144, 1768, 496, -255, 255, RegionCode.Ilshenar_City_Montor),
            new(Map.Ilshenar, 1328, 1008, 1408, 1120, -255, 255, RegionCode.Ilshenar_City_ReqVolon),
            new(Map.Ilshenar, 1048, 616, 1304, 792, -255, 255, RegionCode.Ilshenar_City_SavageCamp),
            new(Map.Ilshenar, 472, 336, 624, 464, -255, 255, RegionCode.Ilshenar_City_TerortSkitas),

            new(Map.Ilshenar, 0, 1152, 584, 1592, -255, 255, RegionCode.Ilshenar_Dungeon_Ankh),
            new(Map.Ilshenar, 0, 800, 192, 1200, -255, 255, RegionCode.Ilshenar_Dungeon_Ankh_KirinPassage),
            new(Map.Ilshenar, 368, 1488, 560, 1592, -255, 255, RegionCode.Ilshenar_Dungeon_Ankh_SerpentinePassage),

            new(Map.Ilshenar, 1736, 808, 2200, 1240, -255, 255, RegionCode.Ilshenar_Dungeon_Blood),
            new(Map.Ilshenar, 800, 16, 2080, 880, -255, 255, RegionCode.Ilshenar_Dungeon_Exodus),
            new(Map.Ilshenar, 1724, 8, 2248, 576, -255, 255, RegionCode.Ilshenar_Dungeon_Rock),
            
            new(Map.Ilshenar, 48, 0, 488, 152, -255, 255, RegionCode.Ilshenar_Dungeon_Sorcerers),
            new(Map.Ilshenar, 1232, 936, 2254, 1280, -255, 255, RegionCode.Ilshenar_Dungeon_Spectre),
            new(Map.Ilshenar, 616, 1256, 1024, 1584, -255, 255, RegionCode.Ilshenar_Dungeon_Wisp),
            new(Map.Ilshenar, 24, 232, 1088, 768, -255, 255, RegionCode.Ilshenar_Dungeon_AncientLair),
            new(Map.Ilshenar, 256, 1280, 336, 1592, -255, 255, RegionCode.Ilshenar_Dungeon_LizardPassage),
            new(Map.Ilshenar, 1303, 1312, 1496, 1568, -255, 255, RegionCode.Ilshenar_Dungeon_MushroomCave),
            new(Map.Ilshenar, 1024, 1152, 1368, 1584, -255, 255, RegionCode.Ilshenar_Dungeon_RatCave),
            new(Map.Ilshenar, 1400, 792, 1864, 1072, -255, 255, RegionCode.Ilshenar_Dungeon_SpiderCave),

            new(Map.Ilshenar, 1048, 304, 1440, 592, -255, 255, RegionCode.Ilshenar_Shrine_Compassion),
            new(Map.Ilshenar, 712, 1344, 736, 1376, -255, 255, RegionCode.Ilshenar_Shrine_Honesty),
            new(Map.Ilshenar, 616, 654, 760, 848, -255, 255, RegionCode.Ilshenar_Shrine_Honor),
            new(Map.Ilshenar, 272, 1008, 296, 1024, -255, 255, RegionCode.Ilshenar_Shrine_Humility),
            new(Map.Ilshenar, 1160, 1280, 1192, 1296, -255, 255, RegionCode.Ilshenar_Shrine_Sacrifice),
            new(Map.Ilshenar, 1520, 1336, 1536, 1352, -255, 255, RegionCode.Ilshenar_Shrine_Spirituality),
            new(Map.Ilshenar, 512, 200, 544, 232, -255, 255, RegionCode.Ilshenar_Shrine_Valor),

            // ==========================================
            // 🌌 [4] 말라스 (Malas) - 최외곽선 병합 적용
            // ==========================================
            new(Map.Malas, 919, 490, 1036, 652, -255, 255, RegionCode.Malas_Town_Luna),
            new(Map.Malas, 1960, 1265, 2106, 1419, -255, 255, RegionCode.Malas_Town_Umbra),
            new(Map.Malas, 256, 0, 512, 560, -255, 255, RegionCode.Malas_Dungeon_Doom),
            new(Map.Malas, 328, 690, 512, 835, -255, 255, RegionCode.Malas_Dungeon_Doom_Gauntlet),
            new(Map.Malas, 1533, 849, 2047, 2047, -255, 255, RegionCode.Malas_Dungeon_Labyrinth),
            new(Map.Malas, 79, 1590, 210, 1690, -255, 255, RegionCode.Malas_Dungeon_Bedlam),
            new(Map.Malas, 65, 1865, 195, 1990, -255, 255, RegionCode.Malas_Dungeon_TheCitadel),

            new(Map.Malas, 1980, 800, 2080, 900, -255, 255, RegionCode.Malas_Site_BrokenMountains),
            new(Map.Malas, 1381, 1527, 1863, 1789, -255, 255, RegionCode.Malas_Site_GravewaterLake),
            new(Map.Malas, 1976, 101, 2448, 420, -255, 255, RegionCode.Malas_Site_GrimswindRuins),
            new(Map.Malas, 79, 1663, 101, 1685, -255, 255, RegionCode.Malas_Site_CrumblingContinent),
            new(Map.Malas, 1049, 1420, 1065, 1445, -255, 255, RegionCode.Malas_Site_HansesHostel),
            new(Map.Malas, 1295, 1189, 1395, 1294, -255, 255, RegionCode.Malas_Site_OrcFortress1),
            new(Map.Malas, 2082, 574, 2150, 650, -255, 255, RegionCode.Malas_Site_Mine1),
            new(Map.Malas, 2026, 345, 2144, 396, -255, 255, RegionCode.Malas_Site_Mine2),
            new(Map.Malas, 1176, 509, 1219, 522, -255, 255, RegionCode.Malas_Site_Mine9),

            // ==========================================
            // 🏯 [5] 토쿠노 (Tokuno) - 최외곽선 병합 적용
            // ==========================================
            new(Map.Tokuno, 650, 1192, 816, 1400, -255, 255, RegionCode.Tokuno_Town_Zento),
            new(Map.Tokuno, 40, 194, 210, 720, -255, 255, RegionCode.Tokuno_Dungeon_FanDancersDojo),
            new(Map.Tokuno, 0, 0, 129, 129, -255, 255, RegionCode.Tokuno_Dungeon_YomotsuMines),
            new(Map.Tokuno, 674, 1203, 699, 1219, -255, 255, RegionCode.Tokuno_Site_MakotoJima),
            new(Map.Tokuno, 1167, 996, 1171, 1000, -255, 255, RegionCode.Tokuno_Site_IsamuJima),
            new(Map.Tokuno, 267, 361, 351, 632, -255, 255, RegionCode.Tokuno_Site_HomareJima),

            // ==========================================
            // 🦇 [6] 터머 (Ter Mur) - 최외곽선 병합 적용
            // ==========================================
            new(Map.TerMur, 624, 3296, 927, 3583, -255, 255, RegionCode.TerMur_Town_RoyalCity),
            new(Map.TerMur, 922, 3838, 1071, 4003, -255, 255, RegionCode.TerMur_Town_HolyCity),
            new(Map.TerMur, 1087, 1127, 1096, 1133, -42, 255, RegionCode.TerMur_Town_Dugan),

            new(Map.TerMur, 0, 14, 489, 260, -255, 255, RegionCode.TerMur_Dungeon_TombOfKings),
            new(Map.TerMur, 413, 135, 1076, 983, -255, 255, RegionCode.TerMur_Dungeon_StygianAbyss),
            new(Map.TerMur, 898, 808, 1280, 1231, -255, 255, RegionCode.TerMur_Dungeon_Underworld),
            
            new(Map.TerMur, 1027, 3311, 1224, 3514, -255, 255, RegionCode.TerMur_Site_AtollBend),
            new(Map.TerMur, 448, 3352, 613, 3480, -255, 255, RegionCode.TerMur_Site_ChickenChase),
            new(Map.TerMur, 547, 2835, 745, 3102, -255, 255, RegionCode.TerMur_Site_FishermansReach),
            new(Map.TerMur, 603, 3836, 815, 4043, -255, 255, RegionCode.TerMur_Site_GatedIsle),
            new(Map.TerMur, 748, 2843, 981, 3050, -255, 255, RegionCode.TerMur_Site_HighPlain),
            new(Map.TerMur, 356, 3143, 523, 3224, -255, 255, RegionCode.TerMur_Site_KepetchWaste),
            new(Map.TerMur, 454, 3708, 648, 3957, -255, 255, RegionCode.TerMur_Site_LostSettlement),
            new(Map.TerMur, 694, 3020, 954, 3148, -255, 255, RegionCode.TerMur_Site_NorthernSteppes),
            new(Map.TerMur, 712, 3692, 972, 3923, -255, 255, RegionCode.TerMur_Site_RaptorIsland),
            new(Map.TerMur, 632, 3161, 990, 3412, -255, 255, RegionCode.TerMur_Site_RoyalPark),
            new(Map.TerMur, 1028, 3263, 1221, 3404, -255, 255, RegionCode.TerMur_Site_SlithValley),
            new(Map.TerMur, 1063, 3695, 1181, 3779, -255, 255, RegionCode.TerMur_Site_SpiderIsland),
            new(Map.TerMur, 629, 3750, 730, 3885, -255, 255, RegionCode.TerMur_Site_TalonPoint),
            new(Map.TerMur, 291, 3207, 445, 3354, -255, 255, RegionCode.TerMur_Site_WalledCircus),
            new(Map.TerMur, 435, 2270, 731, 2926, -255, 255, RegionCode.TerMur_Site_WaterfallPoint),
            new(Map.TerMur, 978, 3786, 1018, 3838, -255, 255, RegionCode.TerMur_Site_ShrineOfSingularity)
        ];

        // ==============================================================================
        // 🌟 [핵심 엔진] 맵과 X, Y, Z 좌표를 넣으면 즉시 RegionCode(Enum)를 반환합니다.
        // ==============================================================================
        public static RegionCode GetRegionCode(Map map, int x, int y, int z)
        {
            if (map == null || map == Map.Internal) return RegionCode.None;
            if (x <= 5 || y <= 5 || x >= map.Width - 5 || y >= map.Height - 5) return RegionCode.None;

            // 좌표 및 Z축 매칭 판독 (순수 정수 연산으로 속도 저하 0)
            foreach (var bounds in m_Regions)
            {
                if (bounds.Facet == map && 
                    x >= bounds.StartX && x <= bounds.EndX && 
                    y >= bounds.StartY && y <= bounds.EndY &&
                    z >= bounds.MinZ && z <= bounds.MaxZ)
                {
                    return bounds.Code;
                }
            }

            return RegionCode.None;
        }

        // ==============================================================================
        // 🌟 [핵심 엔진] 맵과 X, Y, Z 좌표를 넣으면 Major(대표)와 Minor(세부) 구역을 동시에 반환!
        // ==============================================================================
        public static (RegionCode Major, RegionCode Minor) GetRegionCodes(Map map, int x, int y, int z)
        {
            if (map == null || map == Map.Internal) return (RegionCode.None, RegionCode.None);
            if (x <= 5 || y <= 5 || x >= map.Width - 5 || y >= map.Height - 5) return (RegionCode.None, RegionCode.None);

            RegionCode major = RegionCode.None;
            RegionCode minor = RegionCode.None;

            // 1. 단 한 번의 루프로 겹쳐진 상자를 찾습니다.
            foreach (var bounds in m_Regions)
            {
                if (bounds.Facet == map && 
                    x >= bounds.StartX && x <= bounds.EndX && 
                    y >= bounds.StartY && y <= bounds.EndY &&
                    z >= bounds.MinZ && z <= bounds.MaxZ)
                {
                    // 끝자리가 00으로 떨어지면 Major, 아니면 Minor로 분류
                    if ((int)bounds.Code % 100 == 0)
                    {
                        if (major == RegionCode.None) major = bounds.Code;
                    }
                    else
                    {
                        if (minor == RegionCode.None) minor = bounds.Code;
                    }

                    // Major와 Minor를 모두 찾았다면 연산 낭비 없이 즉시 루프 종료!
                    if (major != RegionCode.None && minor != RegionCode.None)
                        break;
                }
            }

            // 2. 🌟 [6자리 규칙의 마법] 
            // 세부 구역 번호를 100으로 나눈 뒤 다시 100을 곱하면 자동으로 Major 번호가 유추됩니다.
            if (minor != RegionCode.None && major == RegionCode.None)
            {
                major = (RegionCode)(((int)minor / 100) * 100);
            }

            return (major, minor);
        }

        // ==============================================================================
        // 💡 [편의성 헬퍼] 특정 구역 소속인지 물어볼 때 쓰는 초고속 판별기
        // ==============================================================================
        public static bool IsBelongTo(RegionCode currentTarget, RegionCode checkMajor)
        {
            if (currentTarget == RegionCode.None || checkMajor == RegionCode.None) return false;
            return ((int)currentTarget / 100) * 100 == (int)checkMajor;
        }
    }
}