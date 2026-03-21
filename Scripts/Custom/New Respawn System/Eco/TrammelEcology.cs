using System;
using Server;
using Server.Mobiles;

namespace Server.Misc
{
    public static class TrammelEcology
    {
        public static void Setup()
        {
            Map map = Map.Trammel;
            string prefixTowns = "Trammel Towns";
            string prefixShrines = "Trammel Shrines";
            string customPrefix = "Trammel Lost Lands";

            // ========================================================================
            // [Britain] 브리튼 (9개 세부 구역)
            // ========================================================================
            EcoZone britBlackthornCastle = new($"{prefixTowns} Britain Blackthorn Castle", map);
            britBlackthornCastle.AddSpecies(typeof(Cat), 5);
            britBlackthornCastle.AddSpecies(typeof(Dog), 2);
            EcosystemManager.Zones[britBlackthornCastle.ZoneId] = britBlackthornCastle;

            EcoZone britBlackthornEnt = new($"{prefixTowns} Britain Blackthorn Entrance", map);
            britBlackthornEnt.AddSpecies(typeof(Dog), 5);
            britBlackthornEnt.AddSpecies(typeof(Bird), 10);
            EcosystemManager.Zones[britBlackthornEnt.ZoneId] = britBlackthornEnt;

            EcoZone britCastle = new($"{prefixTowns} Britain British Castle", map);
            britCastle.AddSpecies(typeof(Cat), 5);
            EcosystemManager.Zones[britCastle.ZoneId] = britCastle;

            EcoZone britCastleEnt = new($"{prefixTowns} Britain British Entrance", map);
            britCastleEnt.AddSpecies(typeof(Horse), 8); // Fame 350
            britCastleEnt.AddSpecies(typeof(Dog), 5);
            EcosystemManager.Zones[britCastleEnt.ZoneId] = britCastleEnt;

            EcoZone britCemetery = new($"{prefixTowns} Britain Cemetery", map);
            britCemetery.AddSpecies(typeof(Skeleton), 25); // Fame 1200
            britCemetery.AddSpecies(typeof(Zombie), 20);   // Fame 800
            britCemetery.AddSpecies(typeof(Spectre), 5);  // Fame 2500
            britCemetery.AddSpecies(typeof(Wraith), 3);   // Fame 4500
            EcosystemManager.Zones[britCemetery.ZoneId] = britCemetery;

            EcoZone britCenter = new($"{prefixTowns} Britain Center", map);
            britCenter.AddSpecies(typeof(Dog), 10);
            britCenter.AddSpecies(typeof(Cat), 10);
            britCenter.AddSpecies(typeof(Bird), 15);
            EcosystemManager.Zones[britCenter.ZoneId] = britCenter;

            EcoZone britFarmlands = new($"{prefixTowns} Britain Farmlands", map);
            britFarmlands.AddSpecies(typeof(Cow), 30);      // Fame 300
            britFarmlands.AddSpecies(typeof(Pig), 20);      // Fame 200
            britFarmlands.AddSpecies(typeof(Sheep), 30);    // Fame 150
            britFarmlands.AddSpecies(typeof(Chicken), 25);  // Fame 150
            britFarmlands.AddSpecies(typeof(Bull), 5);      // Fame 3000
            EcosystemManager.Zones[britFarmlands.ZoneId] = britFarmlands;

            EcoZone britPark = new($"{prefixTowns} Britain Park", map);
            britPark.AddSpecies(typeof(Bird), 25);
            britPark.AddSpecies(typeof(Rabbit), 20);        // Fame 150
            britPark.AddSpecies(typeof(Squirrel), 15);      // Fame 150 (기획서 기준)
            EcosystemManager.Zones[britPark.ZoneId] = britPark;

            EcoZone britSuburbs = new($"{prefixTowns} Britain Suburbs", map);
            britSuburbs.AddSpecies(typeof(GreatHart), 20);  // Fame 400
            britSuburbs.AddSpecies(typeof(Hind), 30);       // Fame 250
            britSuburbs.AddSpecies(typeof(TimberWolf), 10); // Fame 500
            britSuburbs.AddSpecies(typeof(GreyWolf), 8);    // Fame 600
            britSuburbs.AddSpecies(typeof(Boar), 12);       // Fame 350
            EcosystemManager.Zones[britSuburbs.ZoneId] = britSuburbs;


            // ========================================================================
            // [Buccaneers Den] 버커니어스 덴 (3개 세부 구역)
            // ========================================================================
            EcoZone bucsBathhouse = new($"{prefixTowns} Buccaneers Den Bathhouse", map);
            bucsBathhouse.AddSpecies(typeof(Cat), 5);
            EcosystemManager.Zones[bucsBathhouse.ZoneId] = bucsBathhouse;

            EcoZone bucsDocks = new($"{prefixTowns} Buccaneers Den Docks", map);
            bucsDocks.AddSpecies(typeof(GiantRat), 15);     // Fame 150
            bucsDocks.AddSpecies(typeof(Bird), 10);
            bucsDocks.AddSpecies(typeof(Rat), 20);
            EcosystemManager.Zones[bucsDocks.ZoneId] = bucsDocks;

            EcoZone bucsTunnels = new($"{prefixTowns} Buccaneers Den Tunnels", map);
            bucsTunnels.AddSpecies(typeof(Slime), 20);      // Fame 300
            bucsTunnels.AddSpecies(typeof(GiantRat), 15);
            bucsTunnels.AddSpecies(typeof(Snake), 15);      // Fame 300
            EcosystemManager.Zones[bucsTunnels.ZoneId] = bucsTunnels;


            // ========================================================================
            // [Cove] 코브 (4개 세부 구역)
            // ========================================================================
            EcoZone coveCemetery = new($"{prefixTowns} Cove Cemetery", map);
            coveCemetery.AddSpecies(typeof(Skeleton), 15);
            coveCemetery.AddSpecies(typeof(Zombie), 10);
            coveCemetery.AddSpecies(typeof(Spectre), 3);
            EcosystemManager.Zones[coveCemetery.ZoneId] = coveCemetery;

            EcoZone coveGates = new($"{prefixTowns} Cove Gates", map);
            coveGates.AddSpecies(typeof(Dog), 5);
            coveGates.AddSpecies(typeof(Brigand), 8);  // 기획서 Brigand Camp 반영
            EcosystemManager.Zones[coveGates.ZoneId] = coveGates;

            EcoZone coveGuardPost = new($"{prefixTowns} Cove Guard Post", map);
            coveGuardPost.AddSpecies(typeof(Horse), 5);
            EcosystemManager.Zones[coveGuardPost.ZoneId] = coveGuardPost;

            EcoZone coveOrcFort = new($"{prefixTowns} Cove Orc Fort", map);
            coveOrcFort.AddSpecies(typeof(Orc), 35);
            coveOrcFort.AddSpecies(typeof(OrcCaptain), 10);
            coveOrcFort.AddSpecies(typeof(OrcishMage), 5);
            coveOrcFort.AddSpecies(typeof(OrcBrute), 2);
            EcosystemManager.Zones[coveOrcFort.ZoneId] = coveOrcFort;


            // ========================================================================
            // [Heartwood] 하트우드 (1개 구역)
            // ========================================================================
            EcoZone heartwoodEnt = new($"{prefixTowns} Heartwood Entrance", map);
            heartwoodEnt.AddSpecies(typeof(Squirrel), 15);
            heartwoodEnt.AddSpecies(typeof(Bird), 20);
            EcosystemManager.Zones[heartwoodEnt.ZoneId] = heartwoodEnt;


            // ========================================================================
            // [Jhelom] 젤롬 (6개 세부 구역)
            // ========================================================================
            EcoZone jhelomCemetery = new($"{prefixTowns} Jhelom Cemetery", map);
            jhelomCemetery.AddSpecies(typeof(Skeleton), 15);
            jhelomCemetery.AddSpecies(typeof(Zombie), 10);
            EcosystemManager.Zones[jhelomCemetery.ZoneId] = jhelomCemetery;

            EcoZone jhelomEastDocks = new($"{prefixTowns} Jhelom East Docks", map);
            jhelomEastDocks.AddSpecies(typeof(GiantRat), 10);
            jhelomEastDocks.AddSpecies(typeof(Rat), 15);
            EcosystemManager.Zones[jhelomEastDocks.ZoneId] = jhelomEastDocks;

            EcoZone jhelomFightingPit = new($"{prefixTowns} Jhelom Fighting Pit", map);
            jhelomFightingPit.AddSpecies(typeof(Bull), 10);
            jhelomFightingPit.AddSpecies(typeof(GreatHart), 5);
            EcosystemManager.Zones[jhelomFightingPit.ZoneId] = jhelomFightingPit;

            EcoZone jhelomMainIsland = new($"{prefixTowns} Jhelom Main Island", map);
            jhelomMainIsland.AddSpecies(typeof(Bull), 20);
            jhelomMainIsland.AddSpecies(typeof(Cow), 15);
            jhelomMainIsland.AddSpecies(typeof(Panther), 8);  // Fame 800
            jhelomMainIsland.AddSpecies(typeof(Cougar), 8);   // Fame 800
            EcosystemManager.Zones[jhelomMainIsland.ZoneId] = jhelomMainIsland;

            EcoZone jhelomMediumIsland = new($"{prefixTowns} Jhelom Medium Island", map);
            jhelomMediumIsland.AddSpecies(typeof(Pig), 15);
            jhelomMediumIsland.AddSpecies(typeof(Sheep), 20);
            EcosystemManager.Zones[jhelomMediumIsland.ZoneId] = jhelomMediumIsland;

            EcoZone jhelomSmallIsland = new($"{prefixTowns} Jhelom Small Island", map);
            jhelomSmallIsland.AddSpecies(typeof(Rabbit), 30);
            jhelomSmallIsland.AddSpecies(typeof(Bird), 15);
            EcosystemManager.Zones[jhelomSmallIsland.ZoneId] = jhelomSmallIsland;
			
			// ========================================================================
            // [Magincia] 마진시아 (4개 세부 구역)
            // ========================================================================
            EcoZone maginciaBank = new($"{prefixTowns} Magincia Bank", map);
            maginciaBank.AddSpecies(typeof(Dog), 5);
            EcosystemManager.Zones[maginciaBank.ZoneId] = maginciaBank;

            EcoZone maginciaDocks = new($"{prefixTowns} Magincia Docks", map);
            maginciaDocks.AddSpecies(typeof(Cat), 10);
            maginciaDocks.AddSpecies(typeof(Rat), 15);
            EcosystemManager.Zones[maginciaDocks.ZoneId] = maginciaDocks;

            EcoZone maginciaPark = new($"{prefixTowns} Magincia Park", map);
            maginciaPark.AddSpecies(typeof(Bird), 25);
            maginciaPark.AddSpecies(typeof(Rabbit), 20);
            maginciaPark.AddSpecies(typeof(Squirrel), 15);
            EcosystemManager.Zones[maginciaPark.ZoneId] = maginciaPark;

            EcoZone maginciaParliament = new($"{prefixTowns} Magincia Parliament", map);
            maginciaParliament.AddSpecies(typeof(Cat), 5);
            EcosystemManager.Zones[maginciaParliament.ZoneId] = maginciaParliament;


            // ========================================================================
            // [Minoc] 미녹 (5개 세부 구역)
            // ========================================================================
            EcoZone minocBridge = new($"{prefixTowns} Minoc Bridge", map);
            minocBridge.AddSpecies(typeof(Dog), 5);
            EcosystemManager.Zones[minocBridge.ZoneId] = minocBridge;

            EcoZone minocGypsy = new($"{prefixTowns} Minoc Gypsy Camp", map);
            minocGypsy.AddSpecies(typeof(Brigand), 12); // 기획서 반영
            minocGypsy.AddSpecies(typeof(Dog), 5);
            EcosystemManager.Zones[minocGypsy.ZoneId] = minocGypsy;

            EcoZone minocMiningCamp = new($"{prefixTowns} Minoc Mining Camp", map);
            minocMiningCamp.AddSpecies(typeof(PackHorse), 8);
            minocMiningCamp.AddSpecies(typeof(Rat), 20);
            EcosystemManager.Zones[minocMiningCamp.ZoneId] = minocMiningCamp;

            // 미녹 북부 (곰 시너지 핵심 구역 - 코끼리 제외)
            EcoZone minocNorth = new($"{prefixTowns} Minoc North", map);
            minocNorth.AddSpecies(typeof(GrizzlyBear), 10); // Fame 2000
            minocNorth.AddSpecies(typeof(BrownBear), 15);  // Fame 1000
            minocNorth.AddSpecies(typeof(BlackBear), 15);  // Fame 600
            minocNorth.AddSpecies(typeof(MountainGoat), 20);
            minocNorth.AddSpecies(typeof(Eagle), 10);
            EcosystemManager.Zones[minocNorth.ZoneId] = minocNorth;

            EcoZone minocSouth = new($"{prefixTowns} Minoc South", map);
            minocSouth.AddSpecies(typeof(Sheep), 20);
            minocSouth.AddSpecies(typeof(Goat), 15);
            EcosystemManager.Zones[minocSouth.ZoneId] = minocSouth;


            // ========================================================================
            // [Moonglow] 문글로우 (5개 세부 구역)
            // ========================================================================
            EcoZone moonglowCemetery = new($"{prefixTowns} Moonglow Cemetery", map);
            moonglowCemetery.AddSpecies(typeof(Skeleton), 20);
            moonglowCemetery.AddSpecies(typeof(Zombie), 15);
            moonglowCemetery.AddSpecies(typeof(Wraith), 5); // Fame 4500
            EcosystemManager.Zones[moonglowCemetery.ZoneId] = moonglowCemetery;

            EcoZone moonglowCenter = new($"{prefixTowns} Moonglow Center", map);
            moonglowCenter.AddSpecies(typeof(Cat), 10);
            moonglowCenter.AddSpecies(typeof(Dog), 5);
            EcosystemManager.Zones[moonglowCenter.ZoneId] = moonglowCenter;

            EcoZone moonglowDocks = new($"{prefixTowns} Moonglow Docks", map);
            moonglowDocks.AddSpecies(typeof(GiantRat), 15);
            moonglowDocks.AddSpecies(typeof(Rat), 20);
            EcosystemManager.Zones[moonglowDocks.ZoneId] = moonglowDocks;

            EcoZone moonglowTelescope = new($"{prefixTowns} Moonglow Telescope", map);
            moonglowTelescope.AddSpecies(typeof(Bird), 15);
            moonglowTelescope.AddSpecies(typeof(Wisp), 5);
            EcosystemManager.Zones[moonglowTelescope.ZoneId] = moonglowTelescope;

            // 문글로우 동물원 (Elephant 제외, 맹수 위주)
            EcoZone moonglowZoo = new($"{prefixTowns} Moonglow Zoo", map);
            moonglowZoo.AddSpecies(typeof(Panther), 10);
            moonglowZoo.AddSpecies(typeof(SnowLeopard), 10);
            moonglowZoo.AddSpecies(typeof(GrizzlyBear), 5);
            moonglowZoo.AddSpecies(typeof(SilverSteed), 2);
            moonglowZoo.AddSpecies(typeof(WhiteWolf), 10);
            EcosystemManager.Zones[moonglowZoo.ZoneId] = moonglowZoo;


            // ========================================================================
            // [Nujel'm] 누젤름 (7개 세부 구역)
            // ========================================================================
            EcoZone nujelmCemetery = new($"{prefixTowns} Nujel'm Cemetery", map);
            nujelmCemetery.AddSpecies(typeof(Skeleton), 15);
            nujelmCemetery.AddSpecies(typeof(Zombie), 10);
            EcosystemManager.Zones[nujelmCemetery.ZoneId] = nujelmCemetery;

            EcoZone nujelmChessBoard = new($"{prefixTowns} Nujel'm Chess Board", map);
            nujelmChessBoard.AddSpecies(typeof(Cat), 5);
            EcosystemManager.Zones[nujelmChessBoard.ZoneId] = nujelmChessBoard;

            EcoZone nujelmDocks = new($"{prefixTowns} Nujel'm Docks", map);
            nujelmDocks.AddSpecies(typeof(GiantRat), 15);
            EcosystemManager.Zones[nujelmDocks.ZoneId] = nujelmDocks;

            EcoZone nujelmEast = new($"{prefixTowns} Nujel'm East", map);
            nujelmEast.AddSpecies(typeof(Bird), 15);
            EcosystemManager.Zones[nujelmEast.ZoneId] = nujelmEast;

            EcoZone nujelmNorth = new($"{prefixTowns} Nujel'm North", map);
            nujelmNorth.AddSpecies(typeof(Rabbit), 20);
            EcosystemManager.Zones[nujelmNorth.ZoneId] = nujelmNorth;

            EcoZone nujelmPalace = new($"{prefixTowns} Nujel'm Palace", map);
            nujelmPalace.AddSpecies(typeof(Cat), 5);
            EcosystemManager.Zones[nujelmPalace.ZoneId] = nujelmPalace;

            EcoZone nujelmWest = new($"{prefixTowns} Nujel'm West", map);
            nujelmWest.AddSpecies(typeof(Dog), 10);
            nujelmWest.AddSpecies(typeof(DesertOstard), 10); // 기획서 반영
            EcosystemManager.Zones[nujelmWest.ZoneId] = nujelmWest;
			
			// ========================================================================
            // [Haven] 헤이븐 (5개 세부 구역)
            // ========================================================================
            EcoZone oldHaven = new($"{prefixTowns} Haven Old Haven", map);
            oldHaven.AddSpecies(typeof(Zombie), 30);       // Fame 800
            oldHaven.AddSpecies(typeof(Skeleton), 25);     // Fame 1200
            oldHaven.AddSpecies(typeof(BoneKnight), 10);
            oldHaven.AddSpecies(typeof(Spectre), 5);       // Fame 2500 (기획서 반영)
            EcosystemManager.Zones[oldHaven.ZoneId] = oldHaven;

            EcoZone oldHavenNorth = new($"{prefixTowns} Haven Old Haven North", map);
            oldHavenNorth.AddSpecies(typeof(Zombie), 15);
            oldHavenNorth.AddSpecies(typeof(Mongbat), 15);
            oldHavenNorth.AddSpecies(typeof(Rat), 10);
            EcosystemManager.Zones[oldHavenNorth.ZoneId] = oldHavenNorth;

            EcoZone newHaven = new($"{prefixTowns} Haven New Haven", map);
            newHaven.AddSpecies(typeof(Cat), 10);
            newHaven.AddSpecies(typeof(Dog), 10);
            EcosystemManager.Zones[newHaven.ZoneId] = newHaven;

            EcoZone newHavenNorth = new($"{prefixTowns} Haven New Haven North", map);
            newHavenNorth.AddSpecies(typeof(Bird), 20);
            newHavenNorth.AddSpecies(typeof(Rabbit), 20);
            EcosystemManager.Zones[newHavenNorth.ZoneId] = newHavenNorth;

            EcoZone havenFarmlands = new($"{prefixTowns} Haven Farmlands", map);
            havenFarmlands.AddSpecies(typeof(Sheep), 20);
            havenFarmlands.AddSpecies(typeof(Pig), 15);
            havenFarmlands.AddSpecies(typeof(Cow), 15);
            havenFarmlands.AddSpecies(typeof(Chicken), 15);
            EcosystemManager.Zones[havenFarmlands.ZoneId] = havenFarmlands;


            // ========================================================================
            // [Serpents Hold] 서펀츠 홀드 (3개 세부 구역)
            // ========================================================================
            EcoZone serpentsNorth = new($"{prefixTowns} Serpents Hold North", map);
            serpentsNorth.AddSpecies(typeof(Horse), 10);
            serpentsNorth.AddSpecies(typeof(Bird), 10);
            EcosystemManager.Zones[serpentsNorth.ZoneId] = serpentsNorth;

            EcoZone serpentsSouth = new($"{prefixTowns} Serpents Hold South", map);
            serpentsSouth.AddSpecies(typeof(Dog), 10);
            serpentsSouth.AddSpecies(typeof(Cat), 5);
            EcosystemManager.Zones[serpentsSouth.ZoneId] = serpentsSouth;

            EcoZone serpentsGuardPost = new($"{prefixTowns} Serpents Hold Guard Post", map);
            serpentsGuardPost.AddSpecies(typeof(Horse), 5);
            EcosystemManager.Zones[serpentsGuardPost.ZoneId] = serpentsGuardPost;


            // ========================================================================
            // [Skara Brae] 스카라 브라에 (6개 세부 구역)
            // ========================================================================
            EcoZone skaraEast = new($"{prefixTowns} Skara Brae East", map);
            skaraEast.AddSpecies(typeof(Sheep), 25);
            skaraEast.AddSpecies(typeof(Hind), 20);        // Fame 250
            skaraEast.AddSpecies(typeof(GreatHart), 15);   // Fame 400
            skaraEast.AddSpecies(typeof(Boar), 12);        // 기획서 Boar(Fame 350) 반영
            skaraEast.AddSpecies(typeof(Bird), 20);
            EcosystemManager.Zones[skaraEast.ZoneId] = skaraEast;

            EcoZone skaraEastDocks = new($"{prefixTowns} Skara Brae East Docks", map);
            skaraEastDocks.AddSpecies(typeof(Cat), 10);
            EcosystemManager.Zones[skaraEastDocks.ZoneId] = skaraEastDocks;

            EcoZone skaraNorth = new($"{prefixTowns} Skara Brae North", map);
            skaraNorth.AddSpecies(typeof(Bird), 25);
            skaraNorth.AddSpecies(typeof(Eagle), 5);       // Fame 300 (예상)
            EcosystemManager.Zones[skaraNorth.ZoneId] = skaraNorth;

            EcoZone skaraSouth = new($"{prefixTowns} Skara Brae South", map);
            skaraSouth.AddSpecies(typeof(Rabbit), 25);
            skaraSouth.AddSpecies(typeof(Ferret), 10);     // Fame 200
            EcosystemManager.Zones[skaraSouth.ZoneId] = skaraSouth;

            EcoZone skaraWest = new($"{prefixTowns} Skara Brae West", map);
            skaraWest.AddSpecies(typeof(Dog), 10);
            EcosystemManager.Zones[skaraWest.ZoneId] = skaraWest;

            EcoZone skaraWestDocks = new($"{prefixTowns} Skara Brae West Docks", map);
            skaraWestDocks.AddSpecies(typeof(GiantRat), 15);
            EcosystemManager.Zones[skaraWestDocks.ZoneId] = skaraWestDocks;


            // ========================================================================
            // [Trinsic] 트린식 (7개 세부 구역)
            // ========================================================================
            EcoZone trinsicCenter = new($"{prefixTowns} Trinsic Center", map);
            trinsicCenter.AddSpecies(typeof(Dog), 10);
            trinsicCenter.AddSpecies(typeof(Cat), 10);
            EcosystemManager.Zones[trinsicCenter.ZoneId] = trinsicCenter;

            EcoZone trinsicEastDocks = new($"{prefixTowns} Trinsic East Docks", map);
            trinsicEastDocks.AddSpecies(typeof(GiantRat), 15);
            EcosystemManager.Zones[trinsicEastDocks.ZoneId] = trinsicEastDocks;

            EcoZone trinsicPark = new($"{prefixTowns} Trinsic Island Park", map);
            trinsicPark.AddSpecies(typeof(Bird), 30);
            trinsicPark.AddSpecies(typeof(Rabbit), 20);
            trinsicPark.AddSpecies(typeof(Dog), 5);
            EcosystemManager.Zones[trinsicPark.ZoneId] = trinsicPark;

            EcoZone trinsicNorth = new($"{prefixTowns} Trinsic North", map);
            trinsicNorth.AddSpecies(typeof(Horse), 15);    // Fame 350
            trinsicNorth.AddSpecies(typeof(PackHorse), 5);
            EcosystemManager.Zones[trinsicNorth.ZoneId] = trinsicNorth;

            EcoZone trinsicSouth = new($"{prefixTowns} Trinsic South", map);
            trinsicSouth.AddSpecies(typeof(Dog), 10);
            EcosystemManager.Zones[trinsicSouth.ZoneId] = trinsicSouth;

            EcoZone trinsicSouthGate = new($"{prefixTowns} Trinsic South Gate", map);
            trinsicSouthGate.AddSpecies(typeof(Horse), 8);
            EcosystemManager.Zones[trinsicSouthGate.ZoneId] = trinsicSouthGate;

            EcoZone trinsicWestGate = new($"{prefixTowns} Trinsic West Gate", map);
            trinsicWestGate.AddSpecies(typeof(Horse), 8);
            EcosystemManager.Zones[trinsicWestGate.ZoneId] = trinsicWestGate;


            // ========================================================================
            // [Vesper] 베스퍼 (5개 세부 구역)
            // ========================================================================
            EcoZone vesperCemetery = new($"{prefixTowns} Vesper Cemetery", map);
            vesperCemetery.AddSpecies(typeof(Skeleton), 20);
            vesperCemetery.AddSpecies(typeof(Zombie), 15);
            vesperCemetery.AddSpecies(typeof(Wraith), 5);  // Fame 4500
            EcosystemManager.Zones[vesperCemetery.ZoneId] = vesperCemetery;

            EcoZone vesperCenter = new($"{prefixTowns} Vesper Center", map);
            vesperCenter.AddSpecies(typeof(Dog), 10);
            vesperCenter.AddSpecies(typeof(Cat), 5);
            EcosystemManager.Zones[vesperCenter.ZoneId] = vesperCenter;

            EcoZone vesperDocks = new($"{prefixTowns} Vesper Docks", map);
            vesperDocks.AddSpecies(typeof(GiantRat), 15);
            vesperDocks.AddSpecies(typeof(Rat), 15);
            EcosystemManager.Zones[vesperDocks.ZoneId] = vesperDocks;

            EcoZone vesperEast = new($"{prefixTowns} Vesper East", map);
            vesperEast.AddSpecies(typeof(Cat), 10);
            EcosystemManager.Zones[vesperEast.ZoneId] = vesperEast;

            EcoZone vesperNorth = new($"{prefixTowns} Vesper North", map);
            vesperNorth.AddSpecies(typeof(Bird), 20);
            vesperNorth.AddSpecies(typeof(Squirrel), 10);
            EcosystemManager.Zones[vesperNorth.ZoneId] = vesperNorth;
			
			// ========================================================================
            // [Wind] 윈드 (6개 세부 구역)
            // ========================================================================
            EcoZone windCaves = new($"{prefixTowns} Wind Caves", map);
            windCaves.AddSpecies(typeof(Slime), 20);           // Fame 300
            windCaves.AddSpecies(typeof(GiantRat), 15);
            windCaves.AddSpecies(typeof(Rat), 10);
            EcosystemManager.Zones[windCaves.ZoneId] = windCaves;

            EcoZone windEast = new($"{prefixTowns} Wind East", map);
            windEast.AddSpecies(typeof(Bird), 15);
            windEast.AddSpecies(typeof(Wisp), 5);              // 마법 도시 분위기
            EcosystemManager.Zones[windEast.ZoneId] = windEast;

            EcoZone windEntrance = new($"{prefixTowns} Wind Entrance", map);
            windEntrance.AddSpecies(typeof(Snake), 25);        // Fame 300
            windEntrance.AddSpecies(typeof(GiantSerpent), 8);  // Fame 2500
            windEntrance.AddSpecies(typeof(ShadowWisp), 10);   // Fame 500
            EcosystemManager.Zones[windEntrance.ZoneId] = windEntrance;

            EcoZone windPark = new($"{prefixTowns} Wind Park", map);
            windPark.AddSpecies(typeof(Rabbit), 15);
            windPark.AddSpecies(typeof(Squirrel), 10);
            EcosystemManager.Zones[windPark.ZoneId] = windPark;

            EcoZone windSouth = new($"{prefixTowns} Wind South", map);
            windSouth.AddSpecies(typeof(Bird), 15);
            EcosystemManager.Zones[windSouth.ZoneId] = windSouth;

            EcoZone windWest = new($"{prefixTowns} Wind West", map);
            windWest.AddSpecies(typeof(Dog), 5);
            windWest.AddSpecies(typeof(Cat), 5);
            EcosystemManager.Zones[windWest.ZoneId] = windWest;


            // ========================================================================
            // [Yew] 유 (6개 세부 구역 - 곰의 성지)
            // ========================================================================
            EcoZone yewCemetery = new($"{prefixTowns} Yew Cemetery", map);
            yewCemetery.AddSpecies(typeof(Skeleton), 25);
            yewCemetery.AddSpecies(typeof(Zombie), 15);
            yewCemetery.AddSpecies(typeof(Wraith), 5);         // Fame 4500
            EcosystemManager.Zones[yewCemetery.ZoneId] = yewCemetery;

            // 유 중앙 숲 (곰/사슴 시너지 핵심 지점)
            EcoZone yewCenter = new($"{prefixTowns} Yew Center", map);
            yewCenter.AddSpecies(typeof(GreatHart), 35);       // Fame 400
            yewCenter.AddSpecies(typeof(Hind), 45);            // Fame 250
            yewCenter.AddSpecies(typeof(BrownBear), 15);       // Fame 1000
            yewCenter.AddSpecies(typeof(GrizzlyBear), 8);      // Fame 2000
            yewCenter.AddSpecies(typeof(TimberWolf), 15);      // Fame 500
            yewCenter.AddSpecies(typeof(Boar), 12);            // Fame 350
            EcosystemManager.Zones[yewCenter.ZoneId] = yewCenter;

            EcoZone yewCourts = new($"{prefixTowns} Yew Courts and Prisons", map);
            yewCourts.AddSpecies(typeof(Dog), 10);
            yewCourts.AddSpecies(typeof(Brigand), 5);     // 기획서 반영
            EcosystemManager.Zones[yewCourts.ZoneId] = yewCourts;

            EcoZone yewAbbey = new($"{prefixTowns} Yew Empath Abbey", map);
            yewAbbey.AddSpecies(typeof(Bird), 20);
            yewAbbey.AddSpecies(typeof(Rabbit), 10);
            EcosystemManager.Zones[yewAbbey.ZoneId] = yewAbbey;

            EcoZone yewHiddenCave = new($"{prefixTowns} Yew Hidden Cave", map);
            yewHiddenCave.AddSpecies(typeof(GrizzlyBear), 6);
            yewHiddenCave.AddSpecies(typeof(BlackBear), 10);
            yewHiddenCave.AddSpecies(typeof(GiantSpider), 12);
            EcosystemManager.Zones[yewHiddenCave.ZoneId] = yewHiddenCave;

            EcoZone yewOrcFort = new($"{prefixTowns} Yew Orc Fort", map);
            yewOrcFort.AddSpecies(typeof(Orc), 35);
            yewOrcFort.AddSpecies(typeof(OrcCaptain), 10);
            yewOrcFort.AddSpecies(typeof(OrcishMage), 5);
            yewOrcFort.AddSpecies(typeof(OrcBrute), 2);
            EcosystemManager.Zones[yewOrcFort.ZoneId] = yewOrcFort;


            // ========================================================================
            // [Delucia] 델루시아 (3개 세부 구역)
            // ========================================================================
            EcoZone deluciaWatchTower = new($"{prefixTowns} Delucia Watch Tower", map);
            deluciaWatchTower.AddSpecies(typeof(Bird), 10);
            deluciaWatchTower.AddSpecies(typeof(Eagle), 5);
            EcosystemManager.Zones[deluciaWatchTower.ZoneId] = deluciaWatchTower;

            EcoZone deluciaCenter = new($"{prefixTowns} Delucia Center", map);
            deluciaCenter.AddSpecies(typeof(Bull), 25);        // Fame 3000 (시너지 리스폰)
            deluciaCenter.AddSpecies(typeof(Cow), 35);         // Fame 300
            deluciaCenter.AddSpecies(typeof(Chicken), 25);
            deluciaCenter.AddSpecies(typeof(Goat), 20);
            EcosystemManager.Zones[deluciaCenter.ZoneId] = deluciaCenter;

            EcoZone deluciaOrcFort = new($"{prefixTowns} Delucia Orc Fort", map);
            deluciaOrcFort.AddSpecies(typeof(Orc), 25);
            deluciaOrcFort.AddSpecies(typeof(OrcishMage), 8);
            EcosystemManager.Zones[deluciaOrcFort.ZoneId] = deluciaOrcFort;


            // ========================================================================
            // [Papua] 파푸아 (3개 세부 구역)
            // ========================================================================
            EcoZone papuaInn = new($"{prefixTowns} Papua The Just Inn", map);
            papuaInn.AddSpecies(typeof(Cat), 5);
            EcosystemManager.Zones[papuaInn.ZoneId] = papuaInn;

            EcoZone papuaCenter = new($"{prefixTowns} Papua Center", map);
            papuaCenter.AddSpecies(typeof(Alligator), 20);     // 늪지대 포식자
            papuaCenter.AddSpecies(typeof(Snake), 30);
            papuaCenter.AddSpecies(typeof(BullFrog), 15);
            papuaCenter.AddSpecies(typeof(GiantToad), 10);
            EcosystemManager.Zones[papuaCenter.ZoneId] = papuaCenter;

            EcoZone papuaDocks = new($"{prefixTowns} Papua Docks", map);
            papuaDocks.AddSpecies(typeof(Alligator), 8);
            papuaDocks.AddSpecies(typeof(GiantRat), 15);
            EcosystemManager.Zones[papuaDocks.ZoneId] = papuaDocks;


            // ========================================================================
            // [Shrines] 신전 (9개 전체 복원)
            // ========================================================================
            EcoZone shrineChaos = new($"{prefixShrines} Chaos", map);
            shrineChaos.AddSpecies(typeof(Slime), 20);
            shrineChaos.AddSpecies(typeof(ShadowWisp), 10);
            EcosystemManager.Zones[shrineChaos.ZoneId] = shrineChaos;

            EcoZone shrineCompassion = new($"{prefixShrines} Compassion", map);
            shrineCompassion.AddSpecies(typeof(Pixie), 10);
            shrineCompassion.AddSpecies(typeof(Wisp), 5);
            EcosystemManager.Zones[shrineCompassion.ZoneId] = shrineCompassion;

            EcoZone shrineHonesty = new($"{prefixShrines} Honesty", map);
            shrineHonesty.AddSpecies(typeof(Wisp), 15);
            EcosystemManager.Zones[shrineHonesty.ZoneId] = shrineHonesty;

            EcoZone shrineHonor = new($"{prefixShrines} Honor", map);
            shrineHonor.AddSpecies(typeof(Kirin), 3);
            shrineHonor.AddSpecies(typeof(Unicorn), 3);
            EcosystemManager.Zones[shrineHonor.ZoneId] = shrineHonor;

            EcoZone shrineHumility = new($"{prefixShrines} Humility", map);
            shrineHumility.AddSpecies(typeof(Sheep), 20);
            EcosystemManager.Zones[shrineHumility.ZoneId] = shrineHumility;

            EcoZone shrineJustice = new($"{prefixShrines} Justice", map);
            shrineJustice.AddSpecies(typeof(GreatHart), 15);
            shrineJustice.AddSpecies(typeof(Eagle), 10);
            EcosystemManager.Zones[shrineJustice.ZoneId] = shrineJustice;

            EcoZone shrineSacrifice = new($"{prefixShrines} Sacrifice", map);
            shrineSacrifice.AddSpecies(typeof(GreatHart), 15);
            shrineSacrifice.AddSpecies(typeof(Wisp), 10);
            EcosystemManager.Zones[shrineSacrifice.ZoneId] = shrineSacrifice;

            EcoZone shrineSpirituality = new($"{prefixShrines} Spirituality", map);
            shrineSpirituality.AddSpecies(typeof(Wisp), 10);
            shrineSpirituality.AddSpecies(typeof(DarkWisp), 3); // Fame 3500
            EcosystemManager.Zones[shrineSpirituality.ZoneId] = shrineSpirituality;

            EcoZone shrineValor = new($"{prefixShrines} Valor", map);
            shrineValor.AddSpecies(typeof(Drake), 5);
            shrineValor.AddSpecies(typeof(TimberWolf), 10);
            EcosystemManager.Zones[shrineValor.ZoneId] = shrineValor;


            // ========================================================================
            // [CUSTOM] 야외 생태계 (2개 세부 구역)
            // ========================================================================
            EcoZone hoppersBog = new($"{customPrefix} Hopper's Bog", map);
            hoppersBog.AddSpecies(typeof(Alligator), 25);
            hoppersBog.AddSpecies(typeof(GiantToad), 20);
            hoppersBog.AddSpecies(typeof(SilverSerpent), 5);
            hoppersBog.AddSpecies(typeof(SwampDragon), 5);     // 기획서 반영
            EcosystemManager.Zones[hoppersBog.ZoneId] = hoppersBog;

            EcoZone desertOfCompassion = new($"{customPrefix} Desert of Compassion", map);
            desertOfCompassion.AddSpecies(typeof(Scorpion), 30);
            desertOfCompassion.AddSpecies(typeof(Snake), 25);
            desertOfCompassion.AddSpecies(typeof(Orc), 20);
            desertOfCompassion.AddSpecies(typeof(SandVortex), 5); // 기획서 반영
            EcosystemManager.Zones[desertOfCompassion.ZoneId] = desertOfCompassion;
        }
    }
}