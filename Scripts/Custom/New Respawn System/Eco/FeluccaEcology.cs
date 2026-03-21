using System;
using Server;
using Server.Mobiles;

namespace Server.Misc
{
    public static class FeluccaEcology
    {
        public static void Setup()
        {
            Map map = Map.Felucca;
            
            SetupFactions(map);
            SetupTowns(map);
            SetupShrines(map);
            SetupLostLands(map);
        }

        private static void SetupFactions(Map map)
        {
            string prefix = "Felucca Factions";

            // ========================================================================
            // [Factions -> Towns] 팩션 점령 가능 도시들
            // ========================================================================
            EcoZone facBritain = new($"{prefix} Towns Britain", map);
            facBritain.AddSpecies(typeof(Horse), 5);
            facBritain.AddSpecies(typeof(Dog), 5);
            EcosystemManager.Zones[facBritain.ZoneId] = facBritain;

            EcoZone facMagincia = new($"{prefix} Towns Magincia", map);
            facMagincia.AddSpecies(typeof(Cat), 5);
            EcosystemManager.Zones[facMagincia.ZoneId] = facMagincia;

            EcoZone facMinoc = new($"{prefix} Towns Minoc", map);
            facMinoc.AddSpecies(typeof(PackHorse), 5);
            EcosystemManager.Zones[facMinoc.ZoneId] = facMinoc;

            EcoZone facMoonglow = new($"{prefix} Towns Moonglow", map);
            facMoonglow.AddSpecies(typeof(Dog), 5);
            EcosystemManager.Zones[facMoonglow.ZoneId] = facMoonglow;

            EcoZone facSkara = new($"{prefix} Towns Skara Brae", map);
            facSkara.AddSpecies(typeof(Bird), 5);
            EcosystemManager.Zones[facSkara.ZoneId] = facSkara;

            EcoZone facTrinsic = new($"{prefix} Towns Trinsic", map);
            facTrinsic.AddSpecies(typeof(Horse), 5);
            EcosystemManager.Zones[facTrinsic.ZoneId] = facTrinsic;

            EcoZone facVesper = new($"{prefix} Towns Vesper", map);
            facVesper.AddSpecies(typeof(Cat), 5);
            EcosystemManager.Zones[facVesper.ZoneId] = facVesper;

            EcoZone facYew = new($"{prefix} Towns Yew", map);
            facYew.AddSpecies(typeof(TimberWolf), 5);
            EcosystemManager.Zones[facYew.ZoneId] = facYew;

            // ========================================================================
            // [Factions -> Bases] 팩션 본거지 주변 생태계
            // ========================================================================
            EcoZone councilOfMages = new($"{prefix} Council of Mages", map);
            councilOfMages.AddSpecies(typeof(Wisp), 10);
            EcosystemManager.Zones[councilOfMages.ZoneId] = councilOfMages;

            EcoZone minax = new($"{prefix} Minax", map);
            minax.AddSpecies(typeof(HellHound), 15);
            minax.AddSpecies(typeof(Gargoyle), 5);
            EcosystemManager.Zones[minax.ZoneId] = minax;

            EcoZone shadowlords = new($"{prefix} Shadowlords", map);
            shadowlords.AddSpecies(typeof(Wraith), 10);
            shadowlords.AddSpecies(typeof(Shade), 10);
            EcosystemManager.Zones[shadowlords.ZoneId] = shadowlords;

            EcoZone trueBritannians = new($"{prefix} True Britannians", map);
            trueBritannians.AddSpecies(typeof(Horse), 10);
            trueBritannians.AddSpecies(typeof(Bird), 15);
            EcosystemManager.Zones[trueBritannians.ZoneId] = trueBritannians;
        }

        private static void SetupTowns(Map map)
        {
            string prefixTowns = "Felucca Towns";

            // ========================================================================
            // [Britain] 브리튼
            // ========================================================================
            EcoZone britBlackthornCastle = new($"{prefixTowns} Britain Blackthorn Castle", map);
            britBlackthornCastle.AddSpecies(typeof(Cat), 3);
            EcosystemManager.Zones[britBlackthornCastle.ZoneId] = britBlackthornCastle;

            EcoZone britBlackthornEnt = new($"{prefixTowns} Britain Blackthorn Entrance", map);
            britBlackthornEnt.AddSpecies(typeof(Dog), 3);
            EcosystemManager.Zones[britBlackthornEnt.ZoneId] = britBlackthornEnt;

            EcoZone britCastle = new($"{prefixTowns} Britain British Castle", map);
            britCastle.AddSpecies(typeof(Cat), 3);
            EcosystemManager.Zones[britCastle.ZoneId] = britCastle;

            EcoZone britCastleEnt = new($"{prefixTowns} Britain British Entrance", map);
            britCastleEnt.AddSpecies(typeof(Horse), 5);
            EcosystemManager.Zones[britCastleEnt.ZoneId] = britCastleEnt;

            EcoZone britCemetery = new($"{prefixTowns} Britain Cemetery", map);
            britCemetery.AddSpecies(typeof(Skeleton), 25); // 트라멜보다 언데드 많음
            britCemetery.AddSpecies(typeof(Zombie), 20);
            britCemetery.AddSpecies(typeof(Spectre), 5);
            EcosystemManager.Zones[britCemetery.ZoneId] = britCemetery;

            EcoZone britCenter = new($"{prefixTowns} Britain Center", map);
            britCenter.AddSpecies(typeof(Dog), 5);
            britCenter.AddSpecies(typeof(Cat), 5);
            EcosystemManager.Zones[britCenter.ZoneId] = britCenter;

            EcoZone britFarmlands = new($"{prefixTowns} Britain Farmlands", map);
            britFarmlands.AddSpecies(typeof(Cow), 15);
            britFarmlands.AddSpecies(typeof(Pig), 10);
            britFarmlands.AddSpecies(typeof(Sheep), 20);
            EcosystemManager.Zones[britFarmlands.ZoneId] = britFarmlands;

            EcoZone britPark = new($"{prefixTowns} Britain Park", map);
            britPark.AddSpecies(typeof(Bird), 15);
            britPark.AddSpecies(typeof(Rabbit), 10);
            EcosystemManager.Zones[britPark.ZoneId] = britPark;

            EcoZone britSuburbs = new($"{prefixTowns} Britain Suburbs", map);
            britSuburbs.AddSpecies(typeof(GreatHart), 10);
            britSuburbs.AddSpecies(typeof(Hind), 15);
            britSuburbs.AddSpecies(typeof(TimberWolf), 5);
            EcosystemManager.Zones[britSuburbs.ZoneId] = britSuburbs;

            // ========================================================================
            // [Buccaneers Den] 버커니어스 덴
            // ========================================================================
            EcoZone bucsBathhouse = new($"{prefixTowns} Buccaneers Den Bathhouse", map);
            bucsBathhouse.AddSpecies(typeof(Cat), 2);
            EcosystemManager.Zones[bucsBathhouse.ZoneId] = bucsBathhouse;

            EcoZone bucsDocks = new($"{prefixTowns} Buccaneers Den Docks", map);
            bucsDocks.AddSpecies(typeof(GiantRat), 10);
            EcosystemManager.Zones[bucsDocks.ZoneId] = bucsDocks;

            EcoZone bucsTunnels = new($"{prefixTowns} Buccaneers Den Tunnels", map);
            bucsTunnels.AddSpecies(typeof(Slime), 15);
            bucsTunnels.AddSpecies(typeof(GiantRat), 15);
            EcosystemManager.Zones[bucsTunnels.ZoneId] = bucsTunnels;

            // ========================================================================
            // [Cove] 코브
            // ========================================================================
            EcoZone coveCemetery = new($"{prefixTowns} Cove Cemetery", map);
            coveCemetery.AddSpecies(typeof(Skeleton), 15);
            coveCemetery.AddSpecies(typeof(Zombie), 10);
            EcosystemManager.Zones[coveCemetery.ZoneId] = coveCemetery;

            EcoZone coveGates = new($"{prefixTowns} Cove Gates", map);
            coveGates.AddSpecies(typeof(Dog), 3);
            EcosystemManager.Zones[coveGates.ZoneId] = coveGates;

            EcoZone coveGuardPost = new($"{prefixTowns} Cove Guard Post", map);
            coveGuardPost.AddSpecies(typeof(Horse), 2);
            EcosystemManager.Zones[coveGuardPost.ZoneId] = coveGuardPost;

            EcoZone coveOrcFort = new($"{prefixTowns} Cove Orc Fort", map);
            coveOrcFort.AddSpecies(typeof(Orc), 25);
            coveOrcFort.AddSpecies(typeof(OrcCaptain), 5);
            coveOrcFort.AddSpecies(typeof(OrcishMage), 3);
            coveOrcFort.AddSpecies(typeof(OrcBrute), 1);
            EcosystemManager.Zones[coveOrcFort.ZoneId] = coveOrcFort;

            // ========================================================================
            // [Jhelom] 젤롬
            // ========================================================================
            EcoZone jhelomCemetery = new($"{prefixTowns} Jhelom Cemetery", map);
            jhelomCemetery.AddSpecies(typeof(Skeleton), 10);
            jhelomCemetery.AddSpecies(typeof(Zombie), 10);
            EcosystemManager.Zones[jhelomCemetery.ZoneId] = jhelomCemetery;

            EcoZone jhelomEastDocks = new($"{prefixTowns} Jhelom East Docks", map);
            jhelomEastDocks.AddSpecies(typeof(GiantRat), 5);
            EcosystemManager.Zones[jhelomEastDocks.ZoneId] = jhelomEastDocks;

            EcoZone jhelomFightingPit = new($"{prefixTowns} Jhelom Fighting Pit", map);
            jhelomFightingPit.AddSpecies(typeof(Bull), 5); // 투우
            EcosystemManager.Zones[jhelomFightingPit.ZoneId] = jhelomFightingPit;

            EcoZone jhelomMainIsland = new($"{prefixTowns} Jhelom Main Island", map);
            jhelomMainIsland.AddSpecies(typeof(Bull), 15);
            jhelomMainIsland.AddSpecies(typeof(Cow), 15);
            EcosystemManager.Zones[jhelomMainIsland.ZoneId] = jhelomMainIsland;

            EcoZone jhelomMediumIsland = new($"{prefixTowns} Jhelom Medium Island", map);
            jhelomMediumIsland.AddSpecies(typeof(Pig), 10);
            jhelomMediumIsland.AddSpecies(typeof(Sheep), 10);
            EcosystemManager.Zones[jhelomMediumIsland.ZoneId] = jhelomMediumIsland;

            EcoZone jhelomSmallIsland = new($"{prefixTowns} Jhelom Small Island", map);
            jhelomSmallIsland.AddSpecies(typeof(Rabbit), 10);
            EcosystemManager.Zones[jhelomSmallIsland.ZoneId] = jhelomSmallIsland;

            // ========================================================================
            // [Magincia] 마진시아
            // ========================================================================
            EcoZone maginciaBank = new($"{prefixTowns} Magincia Bank", map);
            maginciaBank.AddSpecies(typeof(Dog), 2);
            EcosystemManager.Zones[maginciaBank.ZoneId] = maginciaBank;

            EcoZone maginciaDocks = new($"{prefixTowns} Magincia Docks", map);
            maginciaDocks.AddSpecies(typeof(Cat), 5);
            EcosystemManager.Zones[maginciaDocks.ZoneId] = maginciaDocks;

            EcoZone maginciaPark = new($"{prefixTowns} Magincia Park", map);
            maginciaPark.AddSpecies(typeof(Bird), 15);
            EcosystemManager.Zones[maginciaPark.ZoneId] = maginciaPark;

            EcoZone maginciaParliament = new($"{prefixTowns} Magincia Parliament", map);
            maginciaParliament.AddSpecies(typeof(Cat), 2);
            EcosystemManager.Zones[maginciaParliament.ZoneId] = maginciaParliament;

            // ========================================================================
            // [Minoc] 미녹
            // ========================================================================
            EcoZone minocBridge = new($"{prefixTowns} Minoc Bridge", map);
            minocBridge.AddSpecies(typeof(Dog), 2);
            EcosystemManager.Zones[minocBridge.ZoneId] = minocBridge;

            EcoZone minocGypsyCamp = new($"{prefixTowns} Minoc Gypsy Camp", map);
            minocGypsyCamp.AddSpecies(typeof(Brigand), 10);
            minocGypsyCamp.AddSpecies(typeof(Dog), 3);
            EcosystemManager.Zones[minocGypsyCamp.ZoneId] = minocGypsyCamp;

            EcoZone minocMiningCamp = new($"{prefixTowns} Minoc Mining Camp", map);
            minocMiningCamp.AddSpecies(typeof(PackHorse), 5);
            EcosystemManager.Zones[minocMiningCamp.ZoneId] = minocMiningCamp;

            EcoZone minocNorth = new($"{prefixTowns} Minoc North", map);
            minocNorth.AddSpecies(typeof(MountainGoat), 15);
            minocNorth.AddSpecies(typeof(GrizzlyBear), 3);
            EcosystemManager.Zones[minocNorth.ZoneId] = minocNorth;

            EcoZone minocSouth = new($"{prefixTowns} Minoc South", map);
            minocSouth.AddSpecies(typeof(Sheep), 10);
            EcosystemManager.Zones[minocSouth.ZoneId] = minocSouth;

            // ========================================================================
            // [Moonglow] 문글로우
            // ========================================================================
            EcoZone moonglowCemetery = new($"{prefixTowns} Moonglow Cemetery", map);
            moonglowCemetery.AddSpecies(typeof(Skeleton), 15);
            moonglowCemetery.AddSpecies(typeof(Zombie), 10);
            moonglowCemetery.AddSpecies(typeof(Wraith), 2);
            EcosystemManager.Zones[moonglowCemetery.ZoneId] = moonglowCemetery;

            EcoZone moonglowCenter = new($"{prefixTowns} Moonglow Center", map);
            moonglowCenter.AddSpecies(typeof(Cat), 5);
            EcosystemManager.Zones[moonglowCenter.ZoneId] = moonglowCenter;

            EcoZone moonglowDocks = new($"{prefixTowns} Moonglow Docks", map);
            moonglowDocks.AddSpecies(typeof(GiantRat), 5);
            EcosystemManager.Zones[moonglowDocks.ZoneId] = moonglowDocks;

            EcoZone moonglowTelescope = new($"{prefixTowns} Moonglow Telescope", map);
            moonglowTelescope.AddSpecies(typeof(Bird), 5);
            EcosystemManager.Zones[moonglowTelescope.ZoneId] = moonglowTelescope;

            EcoZone moonglowZoo = new($"{prefixTowns} Moonglow Zoo", map);
            moonglowZoo.AddSpecies(typeof(DreadSpider), 3); // 펠루카는 몬스터 탈출 컨셉
            moonglowZoo.AddSpecies(typeof(HellCat), 5);
            moonglowZoo.AddSpecies(typeof(Panther), 5);
            EcosystemManager.Zones[moonglowZoo.ZoneId] = moonglowZoo;

            // ========================================================================
            // [Nujel'm] 누젤름
            // ========================================================================
            EcoZone nujelmCemetery = new($"{prefixTowns} Nujel'm Cemetery", map);
            nujelmCemetery.AddSpecies(typeof(Skeleton), 10);
            EcosystemManager.Zones[nujelmCemetery.ZoneId] = nujelmCemetery;

            EcoZone nujelmChessBoard = new($"{prefixTowns} Nujel'm Chess Board", map);
            nujelmChessBoard.AddSpecies(typeof(Cat), 2);
            EcosystemManager.Zones[nujelmChessBoard.ZoneId] = nujelmChessBoard;

            EcoZone nujelmDocks = new($"{prefixTowns} Nujel'm Docks", map);
            nujelmDocks.AddSpecies(typeof(GiantRat), 5);
            EcosystemManager.Zones[nujelmDocks.ZoneId] = nujelmDocks;

            EcoZone nujelmEast = new($"{prefixTowns} Nujel'm East", map);
            nujelmEast.AddSpecies(typeof(Bird), 10);
            EcosystemManager.Zones[nujelmEast.ZoneId] = nujelmEast;

            EcoZone nujelmNorth = new($"{prefixTowns} Nujel'm North", map);
            nujelmNorth.AddSpecies(typeof(Rabbit), 5);
            EcosystemManager.Zones[nujelmNorth.ZoneId] = nujelmNorth;

            EcoZone nujelmPalace = new($"{prefixTowns} Nujel'm Palace", map);
            nujelmPalace.AddSpecies(typeof(Cat), 3);
            EcosystemManager.Zones[nujelmPalace.ZoneId] = nujelmPalace;

            EcoZone nujelmWest = new($"{prefixTowns} Nujel'm West", map);
            nujelmWest.AddSpecies(typeof(Dog), 5);
            EcosystemManager.Zones[nujelmWest.ZoneId] = nujelmWest;

            // ========================================================================
            // [Ocllo] 오클로 (트라멜의 헤이븐 위치, 펠루카는 오클로)
            // ========================================================================
            EcoZone oclloDocks = new($"{prefixTowns} Ocllo Docks", map);
            oclloDocks.AddSpecies(typeof(GiantRat), 5);
            EcosystemManager.Zones[oclloDocks.ZoneId] = oclloDocks;

            EcoZone oclloFarmlands = new($"{prefixTowns} Ocllo Farmlands", map);
            oclloFarmlands.AddSpecies(typeof(Sheep), 20);
            oclloFarmlands.AddSpecies(typeof(Cow), 10);
            EcosystemManager.Zones[oclloFarmlands.ZoneId] = oclloFarmlands;

            EcoZone oclloNorth = new($"{prefixTowns} Ocllo North", map);
            oclloNorth.AddSpecies(typeof(GiantRat), 15);
            oclloNorth.AddSpecies(typeof(Snake), 10);
            oclloNorth.AddSpecies(typeof(Mongbat), 10);
            EcosystemManager.Zones[oclloNorth.ZoneId] = oclloNorth;

            // ========================================================================
            // [Serpents Hold] 서펀츠 홀드
            // ========================================================================
            EcoZone serpentsNorth = new($"{prefixTowns} Serpents Hold North", map);
            serpentsNorth.AddSpecies(typeof(Horse), 5);
            EcosystemManager.Zones[serpentsNorth.ZoneId] = serpentsNorth;

            EcoZone serpentsSouth = new($"{prefixTowns} Serpents Hold South", map);
            serpentsSouth.AddSpecies(typeof(Dog), 5);
            EcosystemManager.Zones[serpentsSouth.ZoneId] = serpentsSouth;

            EcoZone serpentsGuardPost = new($"{prefixTowns} Serpents Hold Guard Post", map);
            serpentsGuardPost.AddSpecies(typeof(Horse), 2);
            EcosystemManager.Zones[serpentsGuardPost.ZoneId] = serpentsGuardPost;

            // ========================================================================
            // [Skara Brae] 스카라 브라에
            // ========================================================================
            EcoZone skaraEast = new($"{prefixTowns} Skara Brae East", map);
            skaraEast.AddSpecies(typeof(Sheep), 15);
            skaraEast.AddSpecies(typeof(Hind), 10);
            EcosystemManager.Zones[skaraEast.ZoneId] = skaraEast;

            EcoZone skaraEastDocks = new($"{prefixTowns} Skara Brae East Docks", map);
            skaraEastDocks.AddSpecies(typeof(Cat), 5);
            EcosystemManager.Zones[skaraEastDocks.ZoneId] = skaraEastDocks;

            EcoZone skaraNorth = new($"{prefixTowns} Skara Brae North", map);
            skaraNorth.AddSpecies(typeof(Bird), 15);
            EcosystemManager.Zones[skaraNorth.ZoneId] = skaraNorth;

            EcoZone skaraSouth = new($"{prefixTowns} Skara Brae South", map);
            skaraSouth.AddSpecies(typeof(Rabbit), 10);
            EcosystemManager.Zones[skaraSouth.ZoneId] = skaraSouth;

            EcoZone skaraWest = new($"{prefixTowns} Skara Brae West", map);
            skaraWest.AddSpecies(typeof(Dog), 5);
            EcosystemManager.Zones[skaraWest.ZoneId] = skaraWest;

            EcoZone skaraWestDocks = new($"{prefixTowns} Skara Brae West Docks", map);
            skaraWestDocks.AddSpecies(typeof(GiantRat), 5);
            EcosystemManager.Zones[skaraWestDocks.ZoneId] = skaraWestDocks;

            // ========================================================================
            // [Trinsic] 트린식
            // ========================================================================
            EcoZone trinsicCenter = new($"{prefixTowns} Trinsic Center", map);
            trinsicCenter.AddSpecies(typeof(Dog), 5);
            trinsicCenter.AddSpecies(typeof(Cat), 5);
            EcosystemManager.Zones[trinsicCenter.ZoneId] = trinsicCenter;

            EcoZone trinsicEastDocks = new($"{prefixTowns} Trinsic East Docks", map);
            trinsicEastDocks.AddSpecies(typeof(GiantRat), 5);
            EcosystemManager.Zones[trinsicEastDocks.ZoneId] = trinsicEastDocks;

            EcoZone trinsicIslandPark = new($"{prefixTowns} Trinsic Island Park", map);
            trinsicIslandPark.AddSpecies(typeof(Bird), 15);
            trinsicIslandPark.AddSpecies(typeof(Rabbit), 10);
            EcosystemManager.Zones[trinsicIslandPark.ZoneId] = trinsicIslandPark;

            EcoZone trinsicNorth = new($"{prefixTowns} Trinsic North", map);
            trinsicNorth.AddSpecies(typeof(Horse), 5);
            EcosystemManager.Zones[trinsicNorth.ZoneId] = trinsicNorth;

            EcoZone trinsicSouth = new($"{prefixTowns} Trinsic South", map);
            trinsicSouth.AddSpecies(typeof(Dog), 3);
            EcosystemManager.Zones[trinsicSouth.ZoneId] = trinsicSouth;

            EcoZone trinsicSouthGate = new($"{prefixTowns} Trinsic South Gate", map);
            trinsicSouthGate.AddSpecies(typeof(Horse), 2);
            EcosystemManager.Zones[trinsicSouthGate.ZoneId] = trinsicSouthGate;

            EcoZone trinsicWestGate = new($"{prefixTowns} Trinsic West Gate", map);
            trinsicWestGate.AddSpecies(typeof(Horse), 2);
            EcosystemManager.Zones[trinsicWestGate.ZoneId] = trinsicWestGate;

            // ========================================================================
            // [Vesper] 베스퍼
            // ========================================================================
            EcoZone vesperCemetery = new($"{prefixTowns} Vesper Cemetery", map);
            vesperCemetery.AddSpecies(typeof(Skeleton), 15);
            vesperCemetery.AddSpecies(typeof(Zombie), 10);
            EcosystemManager.Zones[vesperCemetery.ZoneId] = vesperCemetery;

            EcoZone vesperCenter = new($"{prefixTowns} Vesper Center", map);
            vesperCenter.AddSpecies(typeof(Dog), 5);
            EcosystemManager.Zones[vesperCenter.ZoneId] = vesperCenter;

            EcoZone vesperDocks = new($"{prefixTowns} Vesper Docks", map);
            vesperDocks.AddSpecies(typeof(GiantRat), 10);
            EcosystemManager.Zones[vesperDocks.ZoneId] = vesperDocks;

            EcoZone vesperEast = new($"{prefixTowns} Vesper East", map);
            vesperEast.AddSpecies(typeof(Cat), 5);
            EcosystemManager.Zones[vesperEast.ZoneId] = vesperEast;

            EcoZone vesperNorth = new($"{prefixTowns} Vesper North", map);
            vesperNorth.AddSpecies(typeof(Bird), 10);
            EcosystemManager.Zones[vesperNorth.ZoneId] = vesperNorth;

            // ========================================================================
            // [Wind] 윈드
            // ========================================================================
            EcoZone windCaves = new($"{prefixTowns} Wind Caves", map);
            windCaves.AddSpecies(typeof(Slime), 15);
            windCaves.AddSpecies(typeof(GiantRat), 10);
            EcosystemManager.Zones[windCaves.ZoneId] = windCaves;

            EcoZone windEast = new($"{prefixTowns} Wind East", map);
            windEast.AddSpecies(typeof(Bird), 5);
            EcosystemManager.Zones[windEast.ZoneId] = windEast;

            EcoZone windEntrance = new($"{prefixTowns} Wind Entrance", map);
            windEntrance.AddSpecies(typeof(Snake), 20);
            windEntrance.AddSpecies(typeof(GiantSerpent), 5);
            EcosystemManager.Zones[windEntrance.ZoneId] = windEntrance;

            EcoZone windPark = new($"{prefixTowns} Wind Park", map);
            windPark.AddSpecies(typeof(Rabbit), 5);
            EcosystemManager.Zones[windPark.ZoneId] = windPark;

            EcoZone windSouth = new($"{prefixTowns} Wind South", map);
            windSouth.AddSpecies(typeof(Bird), 5);
            EcosystemManager.Zones[windSouth.ZoneId] = windSouth;

            EcoZone windWest = new($"{prefixTowns} Wind West", map);
            windWest.AddSpecies(typeof(Dog), 2);
            EcosystemManager.Zones[windWest.ZoneId] = windWest;

            // ========================================================================
            // [Yew] 유
            // ========================================================================
            EcoZone yewCemetery = new($"{prefixTowns} Yew Cemetery", map);
            yewCemetery.AddSpecies(typeof(Skeleton), 20);
            yewCemetery.AddSpecies(typeof(Zombie), 10);
            yewCemetery.AddSpecies(typeof(Wraith), 5);
            EcosystemManager.Zones[yewCemetery.ZoneId] = yewCemetery;

            EcoZone yewCenter = new($"{prefixTowns} Yew Center", map);
            yewCenter.AddSpecies(typeof(GreatHart), 25);
            yewCenter.AddSpecies(typeof(Hind), 35);
            yewCenter.AddSpecies(typeof(TimberWolf), 20);
            yewCenter.AddSpecies(typeof(BrownBear), 15);
            yewCenter.AddSpecies(typeof(GrizzlyBear), 8);
            EcosystemManager.Zones[yewCenter.ZoneId] = yewCenter;

            EcoZone yewCourts = new($"{prefixTowns} Yew Courts and Prisons", map);
            yewCourts.AddSpecies(typeof(Dog), 5);
            EcosystemManager.Zones[yewCourts.ZoneId] = yewCourts;

            EcoZone yewAbbey = new($"{prefixTowns} Yew Empath Abbey", map);
            yewAbbey.AddSpecies(typeof(Bird), 10);
            EcosystemManager.Zones[yewAbbey.ZoneId] = yewAbbey;

            EcoZone yewHiddenCave = new($"{prefixTowns} Yew Hidden Cave", map);
            yewHiddenCave.AddSpecies(typeof(GrizzlyBear), 3);
            yewHiddenCave.AddSpecies(typeof(GiantSpider), 5);
            EcosystemManager.Zones[yewHiddenCave.ZoneId] = yewHiddenCave;

            EcoZone yewOrcFort = new($"{prefixTowns} Yew Orc Fort", map);
            yewOrcFort.AddSpecies(typeof(Orc), 25);
            yewOrcFort.AddSpecies(typeof(OrcCaptain), 5);
            yewOrcFort.AddSpecies(typeof(OrcishMage), 3);
            yewOrcFort.AddSpecies(typeof(OrcBrute), 1);
            EcosystemManager.Zones[yewOrcFort.ZoneId] = yewOrcFort;

            // ========================================================================
            // [Delucia] 델루시아
            // ========================================================================
            EcoZone deluciaWatchTower = new($"{prefixTowns} Delucia Watch Tower", map);
            deluciaWatchTower.AddSpecies(typeof(Bird), 5);
            EcosystemManager.Zones[deluciaWatchTower.ZoneId] = deluciaWatchTower;

            EcoZone deluciaCenter = new($"{prefixTowns} Delucia Center", map);
            deluciaCenter.AddSpecies(typeof(Bull), 20);
            deluciaCenter.AddSpecies(typeof(Cow), 30);
            deluciaCenter.AddSpecies(typeof(Chicken), 15);
            EcosystemManager.Zones[deluciaCenter.ZoneId] = deluciaCenter;

            EcoZone deluciaOrcFort = new($"{prefixTowns} Delucia Orc Fort", map);
            deluciaOrcFort.AddSpecies(typeof(Orc), 20);
            deluciaOrcFort.AddSpecies(typeof(OrcishMage), 5);
            EcosystemManager.Zones[deluciaOrcFort.ZoneId] = deluciaOrcFort;

            // ========================================================================
            // [Papua] 파푸아
            // ========================================================================
            EcoZone papuaInn = new($"{prefixTowns} Papua The Just Inn", map);
            papuaInn.AddSpecies(typeof(Cat), 2);
            EcosystemManager.Zones[papuaInn.ZoneId] = papuaInn;

            EcoZone papuaCenter = new($"{prefixTowns} Papua Center", map);
            papuaCenter.AddSpecies(typeof(Alligator), 15);
            papuaCenter.AddSpecies(typeof(Snake), 20);
            papuaCenter.AddSpecies(typeof(BullFrog), 10);
            papuaCenter.AddSpecies(typeof(GiantToad), 5);
            EcosystemManager.Zones[papuaCenter.ZoneId] = papuaCenter;

            EcoZone papuaDocks = new($"{prefixTowns} Papua Docks", map);
            papuaDocks.AddSpecies(typeof(Alligator), 5);
            papuaDocks.AddSpecies(typeof(GiantRat), 10);
            EcosystemManager.Zones[papuaDocks.ZoneId] = papuaDocks;
        }

        private static void SetupShrines(Map map)
        {
            string prefixShrines = "Felucca Shrines";

            // 9개 신전 전부
            EcoZone shrineChaos = new($"{prefixShrines} Chaos", map);
            shrineChaos.AddSpecies(typeof(Slime), 15);
            EcosystemManager.Zones[shrineChaos.ZoneId] = shrineChaos;

            EcoZone shrineCompassion = new($"{prefixShrines} Compassion", map);
            shrineCompassion.AddSpecies(typeof(Pixie), 5);
            shrineCompassion.AddSpecies(typeof(Wisp), 3);
            EcosystemManager.Zones[shrineCompassion.ZoneId] = shrineCompassion;

            EcoZone shrineHonesty = new($"{prefixShrines} Honesty", map);
            shrineHonesty.AddSpecies(typeof(Wisp), 5);
            EcosystemManager.Zones[shrineHonesty.ZoneId] = shrineHonesty;

            EcoZone shrineHonor = new($"{prefixShrines} Honor", map);
            shrineHonor.AddSpecies(typeof(Kirin), 2);
            shrineHonor.AddSpecies(typeof(Unicorn), 2);
            EcosystemManager.Zones[shrineHonor.ZoneId] = shrineHonor;

            EcoZone shrineHumility = new($"{prefixShrines} Humility", map);
            shrineHumility.AddSpecies(typeof(Sheep), 10);
            EcosystemManager.Zones[shrineHumility.ZoneId] = shrineHumility;

            EcoZone shrineJustice = new($"{prefixShrines} Justice", map);
            shrineJustice.AddSpecies(typeof(Eagle), 5);
            shrineJustice.AddSpecies(typeof(GreatHart), 5);
            EcosystemManager.Zones[shrineJustice.ZoneId] = shrineJustice;

            EcoZone shrineSacrifice = new($"{prefixShrines} Sacrifice", map);
            shrineSacrifice.AddSpecies(typeof(GreatHart), 5);
            EcosystemManager.Zones[shrineSacrifice.ZoneId] = shrineSacrifice;

            EcoZone shrineSpirituality = new($"{prefixShrines} Spirituality", map);
            shrineSpirituality.AddSpecies(typeof(Wisp), 5);
            EcosystemManager.Zones[shrineSpirituality.ZoneId] = shrineSpirituality;

            EcoZone shrineValor = new($"{prefixShrines} Valor", map);
            shrineValor.AddSpecies(typeof(Drake), 2);
            EcosystemManager.Zones[shrineValor.ZoneId] = shrineValor;
        }

        private static void SetupLostLands(Map map)
        {
            string customPrefix = "Felucca Lost Lands";

            // ========================================================================
            // [CUSTOM / HIDDEN] 야외 생태계 (로스트 랜드의 정글, 사막 등)
            // ========================================================================
            EcoZone hoppersBog = new($"{customPrefix} Hopper's Bog", map);
            hoppersBog.AddSpecies(typeof(Alligator), 20);
            hoppersBog.AddSpecies(typeof(GiantToad), 15);
            hoppersBog.AddSpecies(typeof(SilverSerpent), 5);
            hoppersBog.AddSpecies(typeof(Corpser), 10);
            EcosystemManager.Zones[hoppersBog.ZoneId] = hoppersBog;

            EcoZone desertOfCompassion = new($"{customPrefix} Desert of Compassion", map);
            desertOfCompassion.AddSpecies(typeof(Scorpion), 25);
            desertOfCompassion.AddSpecies(typeof(Snake), 20);
            desertOfCompassion.AddSpecies(typeof(Orc), 15);
            EcosystemManager.Zones[desertOfCompassion.ZoneId] = desertOfCompassion;
        }
    }
}