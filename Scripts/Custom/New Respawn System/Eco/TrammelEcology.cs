using System;
using System.Linq;
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
            // [Britain] 브리튼
            // ========================================================================
            EcoZone britBlackthornCastle = new($"{prefixTowns} Britain Blackthorn Castle", map);
            ApplyTown(britBlackthornCastle);
            britBlackthornCastle.AddSpecies(typeof(Cat), 8);
            EcosystemManager.Zones[britBlackthornCastle.ZoneId] = britBlackthornCastle;

            EcoZone britBlackthornEnt = new($"{prefixTowns} Britain Blackthorn Entrance", map);
            ApplyTown(britBlackthornEnt);
            britBlackthornEnt.AddSpecies(typeof(Dog), 5);
            EcosystemManager.Zones[britBlackthornEnt.ZoneId] = britBlackthornEnt;

            EcoZone britCastle = new($"{prefixTowns} Britain British Castle", map);
            ApplyTown(britCastle);
            EcosystemManager.Zones[britCastle.ZoneId] = britCastle;

            EcoZone britCastleEnt = new($"{prefixTowns} Britain British Entrance", map);
            ApplyTown(britCastleEnt);
            britCastleEnt.AddSpecies(typeof(Horse), 5);
            EcosystemManager.Zones[britCastleEnt.ZoneId] = britCastleEnt;

            EcoZone britCemetery = new($"{prefixTowns} Britain Cemetery", map);
            ApplyCemetery(britCemetery);
            EcosystemManager.Zones[britCemetery.ZoneId] = britCemetery;

            EcoZone britCenter = new($"{prefixTowns} Britain Center", map);
            ApplyTown(britCenter);
            EcosystemManager.Zones[britCenter.ZoneId] = britCenter;

            EcoZone britFarmlands = new($"{prefixTowns} Britain Farmlands", map);
            ApplyFarm(britFarmlands);
            britFarmlands.AddSpecies(typeof(Cow), 5);
            EcosystemManager.Zones[britFarmlands.ZoneId] = britFarmlands;

            EcoZone britPark = new($"{prefixTowns} Britain Park", map);
            ApplyTown(britPark);
            britPark.AddSpecies(typeof(Bird), 5);
            britPark.AddSpecies(typeof(Squirrel), 5); 
            EcosystemManager.Zones[britPark.ZoneId] = britPark;

            EcoZone britSuburbs = new($"{prefixTowns} Britain Suburbs", map);
            ApplyForest(britSuburbs, 2);
            britSuburbs.AddSpecies(typeof(GreatHart), 5);
            EcosystemManager.Zones[britSuburbs.ZoneId] = britSuburbs;

            // ========================================================================
            // [Buccaneers Den] 버커니어스 덴
            // ========================================================================
            EcoZone bucsVampireBathhouse = new($"{prefixTowns} Buccaneers Den VampireBathhouse", map);
            ApplyTown(bucsVampireBathhouse);
            EcosystemManager.Zones[bucsVampireBathhouse.ZoneId] = bucsVampireBathhouse;

            EcoZone bucsDocks = new($"{prefixTowns} Buccaneers Den Docks", map);
            ApplyTown(bucsDocks);
            bucsDocks.AddSpecies(typeof(GiantRat), 8);
            EcosystemManager.Zones[bucsDocks.ZoneId] = bucsDocks;

            EcoZone bucsTunnels = new($"{prefixTowns} Buccaneers Den Tunnels", map);
            ApplySwamp(bucsTunnels);
            bucsTunnels.AddSpecies(typeof(Slime), 8);
            EcosystemManager.Zones[bucsTunnels.ZoneId] = bucsTunnels;

            // ========================================================================
            // [Cove] 코브
            // ========================================================================
            EcoZone coveCemetery = new($"{prefixTowns} Cove Cemetery", map);
            ApplyCemetery(coveCemetery);
            EcosystemManager.Zones[coveCemetery.ZoneId] = coveCemetery;

            EcoZone coveGates = new($"{prefixTowns} Cove Gates", map);
            ApplyTown(coveGates);
            coveGates.AddSpecies(typeof(Brigand), 5); 
            EcosystemManager.Zones[coveGates.ZoneId] = coveGates;

            EcoZone coveGuardPost = new($"{prefixTowns} Cove Guard Post", map);
            ApplyTown(coveGuardPost);
            EcosystemManager.Zones[coveGuardPost.ZoneId] = coveGuardPost;

            EcoZone coveOrcFort = new($"{prefixTowns} Cove Orc Fort", map);
            ApplyOrcFort(coveOrcFort);
            EcosystemManager.Zones[coveOrcFort.ZoneId] = coveOrcFort;

            // ========================================================================
            // [Heartwood] 하트우드
            // ========================================================================
            EcoZone heartwoodEnt = new($"{prefixTowns} Heartwood Entrance", map);
            ApplyForest(heartwoodEnt, 2);
            heartwoodEnt.AddSpecies(typeof(Squirrel), 5);
            EcosystemManager.Zones[heartwoodEnt.ZoneId] = heartwoodEnt;

            // ========================================================================
            // [Jhelom] 젤롬
            // ========================================================================
            EcoZone jhelomCemetery = new($"{prefixTowns} Jhelom Cemetery", map);
            ApplyCemetery(jhelomCemetery);
            EcosystemManager.Zones[jhelomCemetery.ZoneId] = jhelomCemetery;

            EcoZone jhelomEastDocks = new($"{prefixTowns} Jhelom East Docks", map);
            ApplyTown(jhelomEastDocks);
            EcosystemManager.Zones[jhelomEastDocks.ZoneId] = jhelomEastDocks;

            EcoZone jhelomFightingPit = new($"{prefixTowns} Jhelom Fighting Pit", map);
            ApplyFarm(jhelomFightingPit);
            jhelomFightingPit.AddSpecies(typeof(Bull), 10); 
            EcosystemManager.Zones[jhelomFightingPit.ZoneId] = jhelomFightingPit;

            EcoZone jhelomMainIsland = new($"{prefixTowns} Jhelom Main Island", map);
            ApplyFarm(jhelomMainIsland);
            jhelomMainIsland.AddSpecies(typeof(Panther), 4); 
            EcosystemManager.Zones[jhelomMainIsland.ZoneId] = jhelomMainIsland;

            EcoZone jhelomMediumIsland = new($"{prefixTowns} Jhelom Medium Island", map);
            ApplyFarm(jhelomMediumIsland);
            EcosystemManager.Zones[jhelomMediumIsland.ZoneId] = jhelomMediumIsland;

            EcoZone jhelomSmallIsland = new($"{prefixTowns} Jhelom Small Island", map);
            ApplyForest(jhelomSmallIsland, 1);
            jhelomSmallIsland.AddSpecies(typeof(Rabbit), 8);
            EcosystemManager.Zones[jhelomSmallIsland.ZoneId] = jhelomSmallIsland;

            // ========================================================================
            // [Magincia] 마진시아
            // ========================================================================
            EcoZone maginciaBank = new($"{prefixTowns} Magincia Bank", map);
            ApplyTown(maginciaBank);
            EcosystemManager.Zones[maginciaBank.ZoneId] = maginciaBank;

            EcoZone maginciaDocks = new($"{prefixTowns} Magincia Docks", map);
            ApplyTown(maginciaDocks);
            EcosystemManager.Zones[maginciaDocks.ZoneId] = maginciaDocks;

            EcoZone maginciaPark = new($"{prefixTowns} Magincia Park", map);
            ApplyTown(maginciaPark);
            maginciaPark.AddSpecies(typeof(Bird), 8);
            EcosystemManager.Zones[maginciaPark.ZoneId] = maginciaPark;

            EcoZone maginciaParliament = new($"{prefixTowns} Magincia Parliament", map);
            ApplyTown(maginciaParliament);
            EcosystemManager.Zones[maginciaParliament.ZoneId] = maginciaParliament;

            // ========================================================================
            // [Minoc] 미녹
            // ========================================================================
            EcoZone minocBridge = new($"{prefixTowns} Minoc Bridge", map);
            ApplyTown(minocBridge);
            EcosystemManager.Zones[minocBridge.ZoneId] = minocBridge;

            EcoZone minocGypsy = new($"{prefixTowns} Minoc Gypsy Camp", map);
            ApplyTown(minocGypsy);
            minocGypsy.AddSpecies(typeof(Brigand), 6);
            EcosystemManager.Zones[minocGypsy.ZoneId] = minocGypsy;

            EcoZone minocMiningCamp = new($"{prefixTowns} Minoc Mining Camp", map);
            ApplyTown(minocMiningCamp);
            EcosystemManager.Zones[minocMiningCamp.ZoneId] = minocMiningCamp;

            EcoZone minocNorth = new($"{prefixTowns} Minoc North", map);
            ApplyDeepForest(minocNorth, 3); 
            minocNorth.AddSpecies(typeof(GrizzlyBear), 5);
            EcosystemManager.Zones[minocNorth.ZoneId] = minocNorth;

            EcoZone minocSouth = new($"{prefixTowns} Minoc South", map);
            ApplyFarm(minocSouth);
            EcosystemManager.Zones[minocSouth.ZoneId] = minocSouth;

            // ========================================================================
            // [Moonglow] 문글로우
            // ========================================================================
            EcoZone moonglowCemetery = new($"{prefixTowns} Moonglow Cemetery", map);
            ApplyCemetery(moonglowCemetery);
            EcosystemManager.Zones[moonglowCemetery.ZoneId] = moonglowCemetery;

            EcoZone moonglowCenter = new($"{prefixTowns} Moonglow Center", map);
            ApplyTown(moonglowCenter);
            EcosystemManager.Zones[moonglowCenter.ZoneId] = moonglowCenter;

            EcoZone moonglowDocks = new($"{prefixTowns} Moonglow Docks", map);
            ApplyTown(moonglowDocks);
            EcosystemManager.Zones[moonglowDocks.ZoneId] = moonglowDocks;

            EcoZone moonglowTelescope = new($"{prefixTowns} Moonglow Telescope", map);
            ApplyTown(moonglowTelescope);
            moonglowTelescope.AddSpecies(typeof(Wisp), 4); 
            EcosystemManager.Zones[moonglowTelescope.ZoneId] = moonglowTelescope;

            EcoZone moonglowZoo = new($"{prefixTowns} Moonglow Zoo", map);
            ApplyJungle(moonglowZoo);
            moonglowZoo.AddSpecies(typeof(SnowLeopard), 5);
            moonglowZoo.AddSpecies(typeof(SilverSteed), 1);
            moonglowZoo.AddSpecies(typeof(WhiteWolf), 5);
            EcosystemManager.Zones[moonglowZoo.ZoneId] = moonglowZoo;

            // ========================================================================
            // [Nujel'm] 누젤름
            // ========================================================================
            EcoZone nujelmCemetery = new($"{prefixTowns} Nujel'm Cemetery", map);
            ApplyCemetery(nujelmCemetery);
            EcosystemManager.Zones[nujelmCemetery.ZoneId] = nujelmCemetery;

            EcoZone nujelmChessBoard = new($"{prefixTowns} Nujel'm Chess Board", map);
            ApplyTown(nujelmChessBoard);
            EcosystemManager.Zones[nujelmChessBoard.ZoneId] = nujelmChessBoard;

            EcoZone nujelmDocks = new($"{prefixTowns} Nujel'm Docks", map);
            ApplyTown(nujelmDocks);
            EcosystemManager.Zones[nujelmDocks.ZoneId] = nujelmDocks;

            EcoZone nujelmEast = new($"{prefixTowns} Nujel'm East", map);
            ApplyTown(nujelmEast);
            EcosystemManager.Zones[nujelmEast.ZoneId] = nujelmEast;

            EcoZone nujelmNorth = new($"{prefixTowns} Nujel'm North", map);
            ApplyTown(nujelmNorth);
            EcosystemManager.Zones[nujelmNorth.ZoneId] = nujelmNorth;

            EcoZone nujelmPalace = new($"{prefixTowns} Nujel'm Palace", map);
            ApplyTown(nujelmPalace);
            EcosystemManager.Zones[nujelmPalace.ZoneId] = nujelmPalace;

            EcoZone nujelmWest = new($"{prefixTowns} Nujel'm West", map);
            ApplyTown(nujelmWest);
            nujelmWest.AddSpecies(typeof(DesertScorpion), 5); 
            EcosystemManager.Zones[nujelmWest.ZoneId] = nujelmWest;

            // ========================================================================
            // [Haven] 헤이븐
            // ========================================================================
            EcoZone oldHaven = new($"{prefixTowns} Haven Old Haven", map);
            ApplyCemetery(oldHaven);
            EcosystemManager.Zones[oldHaven.ZoneId] = oldHaven;

            EcoZone oldHavenNorth = new($"{prefixTowns} Haven Old Haven North", map);
            ApplyForest(oldHavenNorth, 1);
            oldHavenNorth.AddSpecies(typeof(Zombie), 3); 
            EcosystemManager.Zones[oldHavenNorth.ZoneId] = oldHavenNorth;

            EcoZone newHaven = new($"{prefixTowns} Haven New Haven", map);
            ApplyTown(newHaven);
            EcosystemManager.Zones[newHaven.ZoneId] = newHaven;

            EcoZone newHavenNorth = new($"{prefixTowns} Haven New Haven North", map);
            ApplyTown(newHavenNorth);
            EcosystemManager.Zones[newHavenNorth.ZoneId] = newHavenNorth;

            EcoZone havenFarmlands = new($"{prefixTowns} Haven Farmlands", map);
            ApplyFarm(havenFarmlands);
            EcosystemManager.Zones[havenFarmlands.ZoneId] = havenFarmlands;

            // ========================================================================
            // [Serpents Hold & Skara Brae & Trinsic & Vesper & Wind & Yew]
            // ========================================================================
            EcoZone serpentsNorth = new($"{prefixTowns} Serpents Hold North", map); ApplyTown(serpentsNorth); EcosystemManager.Zones[serpentsNorth.ZoneId] = serpentsNorth;
            EcoZone serpentsSouth = new($"{prefixTowns} Serpents Hold South", map); ApplyTown(serpentsSouth); EcosystemManager.Zones[serpentsSouth.ZoneId] = serpentsSouth;
            EcoZone serpentsGuardPost = new($"{prefixTowns} Serpents Hold Guard Post", map); ApplyTown(serpentsGuardPost); EcosystemManager.Zones[serpentsGuardPost.ZoneId] = serpentsGuardPost;

            EcoZone skaraEast = new($"{prefixTowns} Skara Brae East", map); ApplyForest(skaraEast, 2); skaraEast.AddSpecies(typeof(Sheep), 8); EcosystemManager.Zones[skaraEast.ZoneId] = skaraEast;
            EcoZone skaraEastDocks = new($"{prefixTowns} Skara Brae East Docks", map); ApplyTown(skaraEastDocks); EcosystemManager.Zones[skaraEastDocks.ZoneId] = skaraEastDocks;
            EcoZone skaraNorth = new($"{prefixTowns} Skara Brae North", map); ApplyForest(skaraNorth, 2); skaraNorth.AddSpecies(typeof(Eagle), 4); EcosystemManager.Zones[skaraNorth.ZoneId] = skaraNorth;
            EcoZone skaraSouth = new($"{prefixTowns} Skara Brae South", map); ApplyFarm(skaraSouth); EcosystemManager.Zones[skaraSouth.ZoneId] = skaraSouth;
            EcoZone skaraWest = new($"{prefixTowns} Skara Brae West", map); ApplyTown(skaraWest); EcosystemManager.Zones[skaraWest.ZoneId] = skaraWest;
            EcoZone skaraWestDocks = new($"{prefixTowns} Skara Brae West Docks", map); ApplyTown(skaraWestDocks); EcosystemManager.Zones[skaraWestDocks.ZoneId] = skaraWestDocks;

            EcoZone trinsicCenter = new($"{prefixTowns} Trinsic Center", map); ApplyTown(trinsicCenter); EcosystemManager.Zones[trinsicCenter.ZoneId] = trinsicCenter;
            EcoZone trinsicEastDocks = new($"{prefixTowns} Trinsic East Docks", map); ApplyTown(trinsicEastDocks); EcosystemManager.Zones[trinsicEastDocks.ZoneId] = trinsicEastDocks;
            EcoZone trinsicPark = new($"{prefixTowns} Trinsic Island Park", map); ApplyTown(trinsicPark); trinsicPark.AddSpecies(typeof(Bird), 8); EcosystemManager.Zones[trinsicPark.ZoneId] = trinsicPark;
            EcoZone trinsicNorth = new($"{prefixTowns} Trinsic North", map); ApplyTown(trinsicNorth); trinsicNorth.AddSpecies(typeof(Horse), 5); EcosystemManager.Zones[trinsicNorth.ZoneId] = trinsicNorth;
            EcoZone trinsicSouth = new($"{prefixTowns} Trinsic South", map); ApplyTown(trinsicSouth); EcosystemManager.Zones[trinsicSouth.ZoneId] = trinsicSouth;
            EcoZone trinsicSouthGate = new($"{prefixTowns} Trinsic South Gate", map); ApplyTown(trinsicSouthGate); EcosystemManager.Zones[trinsicSouthGate.ZoneId] = trinsicSouthGate;
            EcoZone trinsicWestGate = new($"{prefixTowns} Trinsic West Gate", map); ApplyTown(trinsicWestGate); EcosystemManager.Zones[trinsicWestGate.ZoneId] = trinsicWestGate;

            EcoZone vesperCemetery = new($"{prefixTowns} Vesper Cemetery", map); ApplyCemetery(vesperCemetery); EcosystemManager.Zones[vesperCemetery.ZoneId] = vesperCemetery;
            EcoZone vesperCenter = new($"{prefixTowns} Vesper Center", map); ApplyTown(vesperCenter); EcosystemManager.Zones[vesperCenter.ZoneId] = vesperCenter;
            EcoZone vesperDocks = new($"{prefixTowns} Vesper Docks", map); ApplyTown(vesperDocks); EcosystemManager.Zones[vesperDocks.ZoneId] = vesperDocks;
            EcoZone vesperEast = new($"{prefixTowns} Vesper East", map); ApplyTown(vesperEast); EcosystemManager.Zones[vesperEast.ZoneId] = vesperEast;
            EcoZone vesperNorth = new($"{prefixTowns} Vesper North", map); ApplyForest(vesperNorth, 2); EcosystemManager.Zones[vesperNorth.ZoneId] = vesperNorth;

            EcoZone windCaves = new($"{prefixTowns} Wind Caves", map); ApplySwamp(windCaves); EcosystemManager.Zones[windCaves.ZoneId] = windCaves;
            EcoZone windEast = new($"{prefixTowns} Wind East", map); ApplyForest(windEast, 2); EcosystemManager.Zones[windEast.ZoneId] = windEast;
            EcoZone windEntrance = new($"{prefixTowns} Wind Entrance", map); ApplyDeepForest(windEntrance, 3); EcosystemManager.Zones[windEntrance.ZoneId] = windEntrance;
            EcoZone windPark = new($"{prefixTowns} Wind Park", map); ApplyTown(windPark); EcosystemManager.Zones[windPark.ZoneId] = windPark;
            EcoZone windSouth = new($"{prefixTowns} Wind South", map); ApplyForest(windSouth, 2); EcosystemManager.Zones[windSouth.ZoneId] = windSouth;
            EcoZone windWest = new($"{prefixTowns} Wind West", map); ApplyTown(windWest); EcosystemManager.Zones[windWest.ZoneId] = windWest;

            EcoZone yewCemetery = new($"{prefixTowns} Yew Cemetery", map); ApplyCemetery(yewCemetery); EcosystemManager.Zones[yewCemetery.ZoneId] = yewCemetery;
            EcoZone yewCenter = new($"{prefixTowns} Yew Center", map); ApplyDeepForest(yewCenter, 3); EcosystemManager.Zones[yewCenter.ZoneId] = yewCenter;
            EcoZone yewCourts = new($"{prefixTowns} Yew Courts and Prisons", map); ApplyTown(yewCourts); EcosystemManager.Zones[yewCourts.ZoneId] = yewCourts;
            EcoZone yewAbbey = new($"{prefixTowns} Yew Empath Abbey", map); ApplyTown(yewAbbey); EcosystemManager.Zones[yewAbbey.ZoneId] = yewAbbey;
            EcoZone yewHiddenCave = new($"{prefixTowns} Yew Hidden Cave", map); ApplyDeepForest(yewHiddenCave, 3); EcosystemManager.Zones[yewHiddenCave.ZoneId] = yewHiddenCave;
            EcoZone yewOrcFort = new($"{prefixTowns} Yew Orc Fort", map); ApplyOrcFort(yewOrcFort); EcosystemManager.Zones[yewOrcFort.ZoneId] = yewOrcFort;

            EcoZone deluciaWatchTower = new($"{prefixTowns} Delucia Watch Tower", map); ApplyTown(deluciaWatchTower); EcosystemManager.Zones[deluciaWatchTower.ZoneId] = deluciaWatchTower;
            EcoZone deluciaCenter = new($"{prefixTowns} Delucia Center", map); ApplyFarm(deluciaCenter); EcosystemManager.Zones[deluciaCenter.ZoneId] = deluciaCenter;
            EcoZone deluciaOrcFort = new($"{prefixTowns} Delucia Orc Fort", map); ApplyOrcFort(deluciaOrcFort); EcosystemManager.Zones[deluciaOrcFort.ZoneId] = deluciaOrcFort;

            EcoZone papuaInn = new($"{prefixTowns} Papua The Just Inn", map); ApplyTown(papuaInn); EcosystemManager.Zones[papuaInn.ZoneId] = papuaInn;
            EcoZone papuaCenter = new($"{prefixTowns} Papua Center", map); ApplySwamp(papuaCenter); EcosystemManager.Zones[papuaCenter.ZoneId] = papuaCenter;
            EcoZone papuaDocks = new($"{prefixTowns} Papua Docks", map); ApplyTown(papuaDocks); EcosystemManager.Zones[papuaDocks.ZoneId] = papuaDocks;

            EcoZone shrineChaos = new($"{prefixShrines} Chaos", map); ApplyShrine(shrineChaos); EcosystemManager.Zones[shrineChaos.ZoneId] = shrineChaos;
            EcoZone shrineCompassion = new($"{prefixShrines} Compassion", map); ApplyShrine(shrineCompassion); EcosystemManager.Zones[shrineCompassion.ZoneId] = shrineCompassion;
            EcoZone shrineHonesty = new($"{prefixShrines} Honesty", map); ApplyShrine(shrineHonesty); EcosystemManager.Zones[shrineHonesty.ZoneId] = shrineHonesty;
            EcoZone shrineHonor = new($"{prefixShrines} Honor", map); ApplyShrine(shrineHonor); EcosystemManager.Zones[shrineHonor.ZoneId] = shrineHonor;
            EcoZone shrineHumility = new($"{prefixShrines} Humility", map); ApplyShrine(shrineHumility); EcosystemManager.Zones[shrineHumility.ZoneId] = shrineHumility;
            EcoZone shrineJustice = new($"{prefixShrines} Justice", map); ApplyShrine(shrineJustice); EcosystemManager.Zones[shrineJustice.ZoneId] = shrineJustice;
            EcoZone shrineSacrifice = new($"{prefixShrines} Sacrifice", map); ApplyShrine(shrineSacrifice); EcosystemManager.Zones[shrineSacrifice.ZoneId] = shrineSacrifice;
            EcoZone shrineSpirituality = new($"{prefixShrines} Spirituality", map); ApplyShrine(shrineSpirituality); EcosystemManager.Zones[shrineSpirituality.ZoneId] = shrineSpirituality;
            EcoZone shrineValor = new($"{prefixShrines} Valor", map); ApplyShrine(shrineValor); EcosystemManager.Zones[shrineValor.ZoneId] = shrineValor;

            EcoZone hoppersBog = new($"{customPrefix} Hopper's Bog", map); ApplySwamp(hoppersBog); EcosystemManager.Zones[hoppersBog.ZoneId] = hoppersBog;
            EcoZone desertOfCompassion = new($"{customPrefix} Desert of Compassion", map); ApplyDesert(desertOfCompassion); EcosystemManager.Zones[desertOfCompassion.ZoneId] = desertOfCompassion;

            // ResourceManager 세이브 로드 완료 후(서버 시작 직후) 야생 벌목 구역 등록
            EventSink.ServerStarted += OnServerStarted;
        }

        private static void OnServerStarted()
        {
            if (ResourceManager.Pools != null)
            {
                var lumberPools = ResourceManager.Pools.Values
                    .Where(p => p.MapName == "Trammel" && p.Type == ResourceType.Lumberjacking)
                    .ToList();

                foreach (var pool in lumberPools)
                {
                    string regName = pool.RegionName;

                    if (string.IsNullOrEmpty(regName) || regName.Contains("Ocean") || regName.Contains("Lost Lands") || regName.Contains("Hopper's Bog") || regName.Contains("Desert of Compassion")) 
                        continue;

                    if (EcosystemManager.Zones.ContainsKey(regName)) continue;

                    EcoZone wildZone = new EcoZone(regName, Map.Trammel);
                    
                    int sizeCat = pool.SizeCategory > 0 ? pool.SizeCategory : 2;

                    if (regName.Contains("Deep") || regName.Contains("Wild")) 
                        ApplyDeepForest(wildZone, sizeCat);
                    else 
                        ApplyForest(wildZone, sizeCat);

                    EcosystemManager.Zones[wildZone.ZoneId] = wildZone;
                    
                    wildZone.CacheNodes();
                }
            }
        }

        // ========================================================================
        // 템플릿(Template) 시스템
        // ========================================================================

        // ========================================================================
        // 템플릿(Template) 시스템: 마을은 매우 한적하게, 농장은 가축 위주로
        // ========================================================================

        private static void ApplyTown(EcoZone zone)
        {
            // 타운/마을: 총합 15마리 내외의 한적하고 평화로운 분위기 구성
            zone.AddSpecies(typeof(Cat), 2); 
            zone.AddSpecies(typeof(Dog), 2);
            zone.AddSpecies(typeof(Bird), 4); 
            zone.AddSpecies(typeof(Chicken), 2);
            zone.AddSpecies(typeof(Pig), 1); 
            zone.AddSpecies(typeof(Cow), 1);
            zone.AddSpecies(typeof(Rabbit), 1); 
            zone.AddSpecies(typeof(Squirrel), 1);
            zone.AddSpecies(typeof(Rat), 1);
            // 슬라임, 뱀, 양, 염소 등 부적절한 동물 전면 삭제
        }

        private static void ApplyFarm(EcoZone zone)
        {
            // 농장/목초지: 양, 젖소, 돼지 등 가축이 주를 이루는 구역 (총합 30마리 내외)
            zone.AddSpecies(typeof(Cow), 6); 
            zone.AddSpecies(typeof(Pig), 5);
            zone.AddSpecies(typeof(Sheep), 6); 
            zone.AddSpecies(typeof(Goat), 4);
            zone.AddSpecies(typeof(Chicken), 6); 
            zone.AddSpecies(typeof(Bull), 2);
            zone.AddSpecies(typeof(Horse), 3); 
            zone.AddSpecies(typeof(PackHorse), 1);
            zone.AddSpecies(typeof(Dog), 2); 
            zone.AddSpecies(typeof(Cat), 2);
            zone.AddSpecies(typeof(Bird), 4); 
            zone.AddSpecies(typeof(Rat), 2);
            // 몬스터 및 야생 맹수 배제
        }
        private static void ApplyForest(EcoZone zone, int sizeCategory)
        {
            int t1 = 4 - sizeCategory; 
            int t2 = sizeCategory;     
            int t3 = sizeCategory == 3 ? 3 : (sizeCategory == 2 ? 2 : 1); // 0배수를 1로 변경하여 최소 스폰 보장

            zone.AddSpecies(typeof(Bird), 15 * t1);
            zone.AddSpecies(typeof(Squirrel), 10 * t1);
            zone.AddSpecies(typeof(Rabbit), 10 * t1);
            zone.AddSpecies(typeof(Hind), 10 * t1);
            zone.AddSpecies(typeof(Ferret), 5 * t1);

            zone.AddSpecies(typeof(GreatHart), 8 * t2);
            zone.AddSpecies(typeof(TimberWolf), 8 * t2);
            zone.AddSpecies(typeof(GreyWolf), 5 * t2);
            zone.AddSpecies(typeof(BlackBear), 5 * t2);
            zone.AddSpecies(typeof(BrownBear), 4 * t2);
            zone.AddSpecies(typeof(Eagle), 5 * t2);
            zone.AddSpecies(typeof(Snake), 8 * t2);
            zone.AddSpecies(typeof(GiantRat), 5 * t2);
            zone.AddSpecies(typeof(Slime), 5 * t2);
            zone.AddSpecies(typeof(Mongbat), 4 * t2);
            zone.AddSpecies(typeof(Corpser), 2 * t2);
            zone.AddSpecies(typeof(Lizardman), 2 * t2);
            zone.AddSpecies(typeof(VampireBat), 5 * t2);

            zone.AddSpecies(typeof(GrizzlyBear), 2 * t3);
            zone.AddSpecies(typeof(GiantSerpent), 2 * t3);
            zone.AddSpecies(typeof(GiantSpider), 4 * t3);
            zone.AddSpecies(typeof(Wisp), 2 * t3);
            zone.AddSpecies(typeof(Panther), 3 * t3);
            zone.AddSpecies(typeof(Cougar), 3 * t3);
        }

        private static void ApplyDeepForest(EcoZone zone, int sizeCategory)
        {
            int t1 = 4 - sizeCategory;
            int t2 = sizeCategory;     
            int t3 = sizeCategory == 3 ? 4 : (sizeCategory == 2 ? 2 : 1);

            zone.AddSpecies(typeof(Hind), 8 * t1);

            zone.AddSpecies(typeof(GreatHart), 10 * t2);
            zone.AddSpecies(typeof(TimberWolf), 5 * t2);
            zone.AddSpecies(typeof(GreyWolf), 8 * t2);
            zone.AddSpecies(typeof(BlackBear), 5 * t2);
            zone.AddSpecies(typeof(BrownBear), 8 * t2);
            zone.AddSpecies(typeof(GiantSpider), 8 * t2);
            zone.AddSpecies(typeof(Panther), 5 * t2);
            zone.AddSpecies(typeof(Cougar), 5 * t2);
            zone.AddSpecies(typeof(Corpser), 5 * t2);
            zone.AddSpecies(typeof(Harpy), 4 * t2);
            zone.AddSpecies(typeof(Lizardman), 4 * t2);

            zone.AddSpecies(typeof(DireWolf), 4 * t3);
            zone.AddSpecies(typeof(GrizzlyBear), 6 * t3);
            zone.AddSpecies(typeof(GiantSerpent), 5 * t3);
            zone.AddSpecies(typeof(Wisp), 4 * t3);
            zone.AddSpecies(typeof(ShadowWisp), 2 * t3);
            zone.AddSpecies(typeof(DarkWisp), 1 * t3); 
            zone.AddSpecies(typeof(Centaur), 2 * t3);
            zone.AddSpecies(typeof(Pixie), 2 * t3);
            zone.AddSpecies(typeof(Unicorn), 1 * t3); 
            zone.AddSpecies(typeof(Kirin), 1 * t3);  
            zone.AddSpecies(typeof(Gargoyle), 2 * t3);
            zone.AddSpecies(typeof(Troll), 2 * t3);
            zone.AddSpecies(typeof(Ettin), 2 * t3);
            zone.AddSpecies(typeof(Orc), 5 * t3);
            zone.AddSpecies(typeof(EarthElemental), 2 * t3);
        }

        private static void ApplyCemetery(EcoZone zone)
        {
            zone.AddSpecies(typeof(Skeleton), 15); zone.AddSpecies(typeof(Zombie), 12);
            zone.AddSpecies(typeof(Ghoul), 8); zone.AddSpecies(typeof(Shade), 5);
            zone.AddSpecies(typeof(Spectre), 5); zone.AddSpecies(typeof(Wraith), 4);
            zone.AddSpecies(typeof(BoneKnight), 2); zone.AddSpecies(typeof(SkeletalKnight), 2);
            zone.AddSpecies(typeof(SkeletalMage), 2); zone.AddSpecies(typeof(BoneMagi), 2);
            zone.AddSpecies(typeof(Mummy), 1); zone.AddSpecies(typeof(Slime), 8);
            zone.AddSpecies(typeof(GiantRat), 8); zone.AddSpecies(typeof(Rat), 5);
            zone.AddSpecies(typeof(VampireBat), 8); zone.AddSpecies(typeof(Snake), 5);
            zone.AddSpecies(typeof(GiantSerpent), 2); zone.AddSpecies(typeof(Mongbat), 5);
            zone.AddSpecies(typeof(HeadlessOne), 2); zone.AddSpecies(typeof(EarthElemental), 1);
            zone.AddSpecies(typeof(Wisp), 2); zone.AddSpecies(typeof(ShadowWisp), 1);
        }

        private static void ApplySwamp(EcoZone zone)
        {
            zone.AddSpecies(typeof(Alligator), 10); zone.AddSpecies(typeof(GiantToad), 8);
            zone.AddSpecies(typeof(BullFrog), 8); zone.AddSpecies(typeof(Slime), 12);
            zone.AddSpecies(typeof(Snake), 10); zone.AddSpecies(typeof(GiantSerpent), 6);
            zone.AddSpecies(typeof(SilverSerpent), 1); zone.AddSpecies(typeof(SwampTentacle), 5);
            zone.AddSpecies(typeof(Bogling), 8); zone.AddSpecies(typeof(Lizardman), 8);
            zone.AddSpecies(typeof(Rat), 8); zone.AddSpecies(typeof(GiantRat), 8);
            zone.AddSpecies(typeof(Corpser), 5); zone.AddSpecies(typeof(WaterElemental), 2);
            zone.AddSpecies(typeof(Mongbat), 5); zone.AddSpecies(typeof(SwampDragon), 1);
            zone.AddSpecies(typeof(Troll), 2); zone.AddSpecies(typeof(VampireBat), 5);
            zone.AddSpecies(typeof(Panther), 2); zone.AddSpecies(typeof(Wisp), 1);
        }

        private static void ApplyDesert(EcoZone zone)
        {
            zone.AddSpecies(typeof(Snake), 12); zone.AddSpecies(typeof(Scorpion), 10);
            zone.AddSpecies(typeof(GiantSerpent), 8); zone.AddSpecies(typeof(DesertOstard), 8);
            zone.AddSpecies(typeof(SandVortex), 4); zone.AddSpecies(typeof(EarthElemental), 5);
            zone.AddSpecies(typeof(Mummy), 2); zone.AddSpecies(typeof(Orc), 5);
            zone.AddSpecies(typeof(OrcCaptain), 1); zone.AddSpecies(typeof(OrcishMage), 1);
            zone.AddSpecies(typeof(Bird), 5); zone.AddSpecies(typeof(Eagle), 4);
            zone.AddSpecies(typeof(Rat), 5); zone.AddSpecies(typeof(GiantRat), 4);
            zone.AddSpecies(typeof(Gargoyle), 2); zone.AddSpecies(typeof(HeadlessOne), 2);
            zone.AddSpecies(typeof(VampireBat), 5); zone.AddSpecies(typeof(Mongbat), 5);
            zone.AddSpecies(typeof(Slime), 2);
        }

        private static void ApplyJungle(EcoZone zone)
        {
            zone.AddSpecies(typeof(Bird), 10); zone.AddSpecies(typeof(Snake), 12);
            zone.AddSpecies(typeof(GiantSerpent), 8); zone.AddSpecies(typeof(SilverSerpent), 2);
            zone.AddSpecies(typeof(Panther), 8); zone.AddSpecies(typeof(Alligator), 8);
            zone.AddSpecies(typeof(Slime), 8); zone.AddSpecies(typeof(GiantSpider), 6);
            zone.AddSpecies(typeof(Wisp), 2); zone.AddSpecies(typeof(Mongbat), 5);
            zone.AddSpecies(typeof(Rat), 5); zone.AddSpecies(typeof(GiantRat), 5);
            zone.AddSpecies(typeof(Corpser), 4); zone.AddSpecies(typeof(Lizardman), 5);
            zone.AddSpecies(typeof(Harpy), 2); zone.AddSpecies(typeof(Troll), 2);
            zone.AddSpecies(typeof(Ettin), 1); zone.AddSpecies(typeof(VampireBat), 5);
        }

        private static void ApplyOrcFort(EcoZone zone)
        {
            zone.AddSpecies(typeof(Orc), 18); zone.AddSpecies(typeof(OrcCaptain), 5);
            zone.AddSpecies(typeof(OrcishMage), 4); zone.AddSpecies(typeof(OrcBrute), 1);
            zone.AddSpecies(typeof(Rat), 8); zone.AddSpecies(typeof(GiantRat), 8);
            zone.AddSpecies(typeof(Slime), 5); zone.AddSpecies(typeof(VampireBat), 5);
            zone.AddSpecies(typeof(Mongbat), 4); zone.AddSpecies(typeof(Snake), 5);
            zone.AddSpecies(typeof(TimberWolf), 2); zone.AddSpecies(typeof(DireWolf), 1);
            zone.AddSpecies(typeof(Troll), 2); zone.AddSpecies(typeof(Ettin), 2);
            zone.AddSpecies(typeof(Pig), 2); zone.AddSpecies(typeof(Dog), 2);
            zone.AddSpecies(typeof(Corpser), 2); zone.AddSpecies(typeof(EarthElemental), 1);
        }

        private static void ApplyShrine(EcoZone zone)
        {
            zone.AddSpecies(typeof(Bird), 15); zone.AddSpecies(typeof(Rabbit), 10);
            zone.AddSpecies(typeof(Squirrel), 10); zone.AddSpecies(typeof(Hind), 8);
            zone.AddSpecies(typeof(GreatHart), 5); zone.AddSpecies(typeof(Horse), 5);
            zone.AddSpecies(typeof(Eagle), 5); zone.AddSpecies(typeof(Wisp), 8);
            zone.AddSpecies(typeof(Pixie), 5); zone.AddSpecies(typeof(Centaur), 2);
            zone.AddSpecies(typeof(Unicorn), 1); zone.AddSpecies(typeof(Kirin), 1);
            zone.AddSpecies(typeof(Dog), 2); zone.AddSpecies(typeof(Cat), 2);
            zone.AddSpecies(typeof(Panther), 1); zone.AddSpecies(typeof(DireWolf), 1);
            zone.AddSpecies(typeof(TimberWolf), 2); zone.AddSpecies(typeof(Slime), 2);
            zone.AddSpecies(typeof(Rat), 2); zone.AddSpecies(typeof(VampireBat), 2);
        }
    }
}