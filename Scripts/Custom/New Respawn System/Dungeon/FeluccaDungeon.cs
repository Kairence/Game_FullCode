using System;
using Server;
using Server.Mobiles;

namespace Server.Misc
{
    public static class FeluccaDungeon
    {
        public static void Setup()
        {
            Map map = Map.Felucca;
            string prefix = "Felucca Dungeons";
            string customPrefix = "Felucca Hidden Dungeons"; // XML에 없지만 추가된 비밀/로스트 랜드 던전

            // ========================================================================
            // Blighted Grove (몬데인의 유산)
            // ========================================================================
            DungeonZone blightedGroveEnt = new($"{prefix} Blighted Grove Entrance", map, 85000, null, TimeSpan.FromHours(8));
            blightedGroveEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Bogling), typeof(Corpser)]);
            blightedGroveEnt.SetSpawnProfile(DungeonDepth.Deep, [typeof(BogThing), typeof(Wisp)]);
            DungeonManager.Zones[blightedGroveEnt.ZoneId] = blightedGroveEnt;

            // ========================================================================
            // Covetous
            // ========================================================================
            DungeonZone covetousEnt = new($"{prefix} Covetous Entrance", map, 25000, null, TimeSpan.FromHours(2));
            covetousEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Bird), typeof(Eagle), typeof(Harpy)]);
            DungeonManager.Zones[covetousEnt.ZoneId] = covetousEnt;

            DungeonZone covetousL1 = new($"{prefix} Covetous Level 1", map, 35000, typeof(Harpy), TimeSpan.FromHours(4));
            covetousL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Harpy)]);
            covetousL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(StoneHarpy)]);
            DungeonManager.Zones[covetousL1.ZoneId] = covetousL1;

            DungeonZone covetousL2 = new($"{prefix} Covetous Level 2", map, 55000, typeof(Gazer), TimeSpan.FromHours(6));
            covetousL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(StoneHarpy), typeof(Gargoyle)]);
            covetousL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(Gazer)]);
            DungeonManager.Zones[covetousL2.ZoneId] = covetousL2;

            DungeonZone covetousL3 = new($"{prefix} Covetous Level 3", map, 85000, typeof(ElderGazer), TimeSpan.FromHours(8));
            covetousL3.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Gazer), typeof(PoisonElemental)]);
            covetousL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(ElderGazer), typeof(BloodElemental)]);
            DungeonManager.Zones[covetousL3.ZoneId] = covetousL3;

            DungeonZone covetousLake = new($"{prefix} Covetous Lake Cave", map, 65000, typeof(WaterElemental), TimeSpan.FromHours(6));
            covetousLake.SetSpawnProfile(DungeonDepth.Entrance, [typeof(WaterElemental)]);
            covetousLake.SetSpawnProfile(DungeonDepth.Deep, [typeof(SeaSerpent), typeof(Kraken)]);
            DungeonManager.Zones[covetousLake.ZoneId] = covetousLake;

            DungeonZone covetousTorture = new($"{prefix} Covetous Torture Chambers", map, 75000, typeof(Executioner), TimeSpan.FromHours(6));
            covetousTorture.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Skeleton), typeof(Zombie)]);
            covetousTorture.SetSpawnProfile(DungeonDepth.Deep, [typeof(Executioner), typeof(BoneKnight)]);
            DungeonManager.Zones[covetousTorture.ZoneId] = covetousTorture;

            // ========================================================================
            // Deceit
            // ========================================================================
            DungeonZone deceitEnt = new($"{prefix} Deceit Entrance", map, 25000, null, TimeSpan.FromHours(2));
            deceitEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Skeleton), typeof(Zombie)]);
            DungeonManager.Zones[deceitEnt.ZoneId] = deceitEnt;

            DungeonZone deceitL1 = new($"{prefix} Deceit Level 1", map, 45000, typeof(SkeletalKnight), TimeSpan.FromHours(4));
            deceitL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Skeleton), typeof(Ghoul)]);
            deceitL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(Wraith)]);
            DungeonManager.Zones[deceitL1.ZoneId] = deceitL1;

            DungeonZone deceitL2 = new($"{prefix} Deceit Level 2", map, 65000, typeof(Lich), TimeSpan.FromHours(6));
            deceitL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BoneKnight), typeof(SkeletalMage)]);
            deceitL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(Mummy)]);
            DungeonManager.Zones[deceitL2.ZoneId] = deceitL2;

            DungeonZone deceitL3 = new($"{prefix} Deceit Level 3", map, 85000, typeof(LichLord), TimeSpan.FromHours(8));
            deceitL3.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Lich), typeof(RottingCorpse)]);
            deceitL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(PoisonElemental)]);
            DungeonManager.Zones[deceitL3.ZoneId] = deceitL3;

            DungeonZone deceitL4 = new($"{prefix} Deceit Level 4", map, 130000, typeof(SilverSerpent), TimeSpan.FromHours(10));
            deceitL4.SetSpawnProfile(DungeonDepth.Entrance, [typeof(LichLord), typeof(RottingCorpse)]);
            deceitL4.SetSpawnProfile(DungeonDepth.Deep, [typeof(SilverSerpent), typeof(PoisonElemental)]);
            DungeonManager.Zones[deceitL4.ZoneId] = deceitL4;

            // ========================================================================
            // Despise
            // ========================================================================
            DungeonZone despiseEnt = new($"{prefix} Despise Entrance", map, 25000, null, TimeSpan.FromHours(2));
            despiseEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Lizardman)]);
            DungeonManager.Zones[despiseEnt.ZoneId] = despiseEnt;

            DungeonZone despiseEntryway = new($"{prefix} Despise Entryway", map, 30000, null, TimeSpan.FromHours(2));
            despiseEntryway.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Lizardman), typeof(GiantRat)]);
            DungeonManager.Zones[despiseEntryway.ZoneId] = despiseEntryway;

            DungeonZone despiseL1 = new($"{prefix} Despise Level 1", map, 35000, typeof(EarthElemental), TimeSpan.FromHours(4));
            despiseL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Lizardman)]);
            despiseL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(EarthElemental)]);
            DungeonManager.Zones[despiseL1.ZoneId] = despiseL1;

            DungeonZone despiseL2 = new($"{prefix} Despise Level 2", map, 55000, typeof(OgreLord), TimeSpan.FromHours(6));
            despiseL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Ettin), typeof(Ogre)]);
            despiseL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(OgreLord), typeof(Cyclops)]);
            DungeonManager.Zones[despiseL2.ZoneId] = despiseL2;

            DungeonZone despiseL3 = new($"{prefix} Despise Level 3", map, 85000, typeof(Titan), TimeSpan.FromHours(8));
            despiseL3.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Cyclops), typeof(OgreLord)]);
            despiseL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(Titan)]);
            DungeonManager.Zones[despiseL3.ZoneId] = despiseL3;

            // ========================================================================
            // Destard
            // ========================================================================
            DungeonZone destardEnt = new($"{prefix} Destard Entrance", map, 35000, null, TimeSpan.FromHours(2));
            destardEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(GiantSerpent)]);
            DungeonManager.Zones[destardEnt.ZoneId] = destardEnt;

            DungeonZone destardL1 = new($"{prefix} Destard Level 1", map, 75000, typeof(Wyvern), TimeSpan.FromHours(6));
            destardL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Drake)]);
            destardL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(Wyvern)]);
            DungeonManager.Zones[destardL1.ZoneId] = destardL1;

            DungeonZone destardL2 = new($"{prefix} Destard Level 2", map, 110000, typeof(Dragon), TimeSpan.FromHours(8));
            destardL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Drake), typeof(Wyvern)]);
            destardL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(Dragon)]);
            DungeonManager.Zones[destardL2.ZoneId] = destardL2;

            DungeonZone destardL3 = new($"{prefix} Destard Level 3", map, 160000, typeof(AncientWyrm), TimeSpan.FromHours(12));
            destardL3.SetSpawnProfile(DungeonDepth.Entrance, [typeof(GreaterDragon)]);
            destardL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(ShadowWyrm)]);
            DungeonManager.Zones[destardL3.ZoneId] = destardL3;

            // ========================================================================
            // Hythloth
            // ========================================================================
            DungeonZone hythlothEnt = new($"{prefix} Hythloth Entrance", map, 45000, null, TimeSpan.FromHours(4));
            hythlothEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Gargoyle), typeof(Imp)]);
            DungeonManager.Zones[hythlothEnt.ZoneId] = hythlothEnt;

            DungeonZone hythlothL1 = new($"{prefix} Hythloth Level 1", map, 65000, typeof(HellHound), TimeSpan.FromHours(6));
            hythlothL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Gargoyle)]);
            hythlothL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(HellHound), typeof(Daemon)]);
            DungeonManager.Zones[hythlothL1.ZoneId] = hythlothL1;

            DungeonZone hythlothL2 = new($"{prefix} Hythloth Level 2", map, 85000, typeof(Daemon), TimeSpan.FromHours(8));
            hythlothL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(HellHound)]);
            hythlothL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(Daemon), typeof(Succubus)]);
            DungeonManager.Zones[hythlothL2.ZoneId] = hythlothL2;

            DungeonZone hythlothL3 = new($"{prefix} Hythloth Level 3", map, 110000, typeof(Balron), TimeSpan.FromHours(10));
            hythlothL3.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Daemon), typeof(Succubus)]);
            hythlothL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(Balron)]);
            DungeonManager.Zones[hythlothL3.ZoneId] = hythlothL3;

            DungeonZone hythlothL4 = new($"{prefix} Hythloth Level 4", map, 160000, null, TimeSpan.FromHours(12));
            hythlothL4.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Balron)]);
            hythlothL4.SetSpawnProfile(DungeonDepth.Deep, [typeof(BloodElemental)]);
            DungeonManager.Zones[hythlothL4.ZoneId] = hythlothL4;

            // ========================================================================
            // Shame
            // ========================================================================
            DungeonZone shameEnt = new($"{prefix} Shame Entrance", map, 25000, null, TimeSpan.FromHours(2));
            shameEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(EarthElemental)]);
            DungeonManager.Zones[shameEnt.ZoneId] = shameEnt;

            DungeonZone shameL1 = new($"{prefix} Shame Level 1", map, 45000, typeof(EarthElemental), TimeSpan.FromHours(4));
            shameL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Scorpion)]);
            shameL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(EarthElemental)]);
            DungeonManager.Zones[shameL1.ZoneId] = shameL1;

            DungeonZone shameL2 = new($"{prefix} Shame Level 2", map, 65000, typeof(AirElemental), TimeSpan.FromHours(6));
            shameL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(EarthElemental)]);
            shameL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(AirElemental), typeof(WaterElemental)]);
            DungeonManager.Zones[shameL2.ZoneId] = shameL2;

            DungeonZone shameL3 = new($"{prefix} Shame Level 3", map, 85000, typeof(PoisonElemental), TimeSpan.FromHours(8));
            shameL3.SetSpawnProfile(DungeonDepth.Entrance, [typeof(WaterElemental), typeof(FireElemental)]);
            shameL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(PoisonElemental)]);
            DungeonManager.Zones[shameL3.ZoneId] = shameL3;

            DungeonZone shameL4 = new($"{prefix} Shame Level 4", map, 110000, typeof(BloodElemental), TimeSpan.FromHours(10));
            shameL4.SetSpawnProfile(DungeonDepth.Entrance, [typeof(PoisonElemental)]);
            shameL4.SetSpawnProfile(DungeonDepth.Deep, [typeof(BloodElemental)]);
            DungeonManager.Zones[shameL4.ZoneId] = shameL4;

            // ========================================================================
            // Wrong
            // ========================================================================
            DungeonZone wrongEnt = new($"{prefix} Wrong Entrance", map, 25000, null, TimeSpan.FromHours(2));
            wrongEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Brigand)]);
            DungeonManager.Zones[wrongEnt.ZoneId] = wrongEnt;

            DungeonZone wrongL1 = new($"{prefix} Wrong Level 1", map, 45000, typeof(Executioner), TimeSpan.FromHours(4));
            wrongL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Brigand)]);
            wrongL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(Executioner)]);
            DungeonManager.Zones[wrongL1.ZoneId] = wrongL1;

            DungeonZone wrongL2 = new($"{prefix} Wrong Level 2", map, 65000, typeof(JukaMage), TimeSpan.FromHours(6));
            wrongL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Executioner)]);
            wrongL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(JukaWarrior), typeof(JukaMage)]);
            DungeonManager.Zones[wrongL2.ZoneId] = wrongL2;

            DungeonZone wrongL3 = new($"{prefix} Wrong Level 3", map, 85000, typeof(JukaLord), TimeSpan.FromHours(8));
            wrongL3.SetSpawnProfile(DungeonDepth.Entrance, [typeof(JukaMage)]);
            wrongL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(JukaLord)]);
            DungeonManager.Zones[wrongL3.ZoneId] = wrongL3;

            // ========================================================================
            // Miscellaneous
            // ========================================================================
            DungeonZone firePit = new($"{prefix} Miscellaneous Hyloth Fire Pit", map, 65000, typeof(FireElemental), TimeSpan.FromHours(6));
            firePit.SetSpawnProfile(DungeonDepth.Entrance, [typeof(FireGargoyle)]);
            firePit.SetSpawnProfile(DungeonDepth.Deep, [typeof(FireElemental), typeof(HellHound)]);
            DungeonManager.Zones[firePit.ZoneId] = firePit;

            DungeonZone brigandCamp = new($"{prefix} Miscellaneous Yew-Britain Brigand Camp", map, 25000, typeof(Brigand), TimeSpan.FromHours(2));
            brigandCamp.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Brigand)]);
            brigandCamp.SetSpawnProfile(DungeonDepth.Deep, [typeof(Executioner)]);
            DungeonManager.Zones[brigandCamp.ZoneId] = brigandCamp;

            DungeonZone damnedFort = new($"{prefix} Miscellaneous Yew Fort of the Damned", map, 45000, typeof(OrcCaptain), TimeSpan.FromHours(4));
            damnedFort.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Orc)]);
            damnedFort.SetSpawnProfile(DungeonDepth.Deep, [typeof(OrcCaptain), typeof(OrcishMage)]);
            DungeonManager.Zones[damnedFort.ZoneId] = damnedFort;

            // ========================================================================
            // Terathan Keep
            // ========================================================================
            DungeonZone teraEnt = new($"{prefix} Terathan Keep Entrance", map, 35000, null, TimeSpan.FromHours(2));
            teraEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(GiantSpider)]);
            DungeonManager.Zones[teraEnt.ZoneId] = teraEnt;

            DungeonZone teraL1 = new($"{prefix} Terathan Keep Level 1", map, 55000, typeof(TerathanWarrior), TimeSpan.FromHours(5));
            teraL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(TerathanDrone)]);
            teraL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(TerathanWarrior)]);
            DungeonManager.Zones[teraL1.ZoneId] = teraL1;

            DungeonZone teraChamp = new($"{prefix} Terathan Keep Champion Room", map, 85000, typeof(TerathanAvenger), TimeSpan.FromHours(8));
            teraChamp.SetSpawnProfile(DungeonDepth.Entrance, [typeof(TerathanWarrior)]);
            teraChamp.SetSpawnProfile(DungeonDepth.Deep, [typeof(TerathanAvenger)]);
            DungeonManager.Zones[teraChamp.ZoneId] = teraChamp;

            DungeonZone teraStar = new($"{prefix} Terathan Keep Starroom", map, 130000, typeof(TerathanMatriarch), TimeSpan.FromHours(12));
            teraStar.SetSpawnProfile(DungeonDepth.Entrance, [typeof(TerathanAvenger)]);
            teraStar.SetSpawnProfile(DungeonDepth.Deep, [typeof(TerathanMatriarch), typeof(PoisonElemental)]);
            DungeonManager.Zones[teraStar.ZoneId] = teraStar;

            // ========================================================================
            // Fire
            // ========================================================================
            DungeonZone fireEnt = new($"{prefix} Fire Entrance", map, 35000, null, TimeSpan.FromHours(2));
            fireEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Slime), typeof(HellCat)]);
            DungeonManager.Zones[fireEnt.ZoneId] = fireEnt;

            DungeonZone fireBritEnt = new($"{prefix} Fire Brit Entrance", map, 35000, null, TimeSpan.FromHours(2));
            fireBritEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Slime)]);
            DungeonManager.Zones[fireBritEnt.ZoneId] = fireBritEnt;

            DungeonZone fireL1 = new($"{prefix} Fire Level 1", map, 65000, typeof(FireElemental), TimeSpan.FromHours(6));
            fireL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(FireGargoyle)]);
            fireL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(FireElemental), typeof(LavaLizard)]);
            DungeonManager.Zones[fireL1.ZoneId] = fireL1;

            DungeonZone fireL2 = new($"{prefix} Fire Level 2", map, 110000, typeof(Efreet), TimeSpan.FromHours(8));
            fireL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(FireElemental)]);
            fireL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(Efreet)]);
            DungeonManager.Zones[fireL2.ZoneId] = fireL2;

            // ========================================================================
            // Ice
            // ========================================================================
            DungeonZone iceEnt = new($"{prefix} Ice Entrance", map, 35000, null, TimeSpan.FromHours(2));
            iceEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(SnowLeopard), typeof(PolarBear)]);
            DungeonManager.Zones[iceEnt.ZoneId] = iceEnt;

            DungeonZone iceBritEnt = new($"{prefix} Ice Brit Entrance", map, 35000, null, TimeSpan.FromHours(2));
            iceBritEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(FrostSpider)]);
            DungeonManager.Zones[iceBritEnt.ZoneId] = iceBritEnt;

            DungeonZone iceL1 = new($"{prefix} Ice Level 1", map, 65000, typeof(SnowElemental), TimeSpan.FromHours(6));
            iceL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(FrostTroll), typeof(FrostSpider)]);
            iceL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(SnowElemental), typeof(IceSnake)]);
            DungeonManager.Zones[iceL1.ZoneId] = iceL1;

            DungeonZone iceRatman = new($"{prefix} Ice Ratman Room", map, 45000, typeof(RatmanMage), TimeSpan.FromHours(4));
            iceRatman.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Ratman)]);
            iceRatman.SetSpawnProfile(DungeonDepth.Deep, [typeof(RatmanArcher), typeof(RatmanMage)]);
            DungeonManager.Zones[iceRatman.ZoneId] = iceRatman;

            DungeonZone iceDemon = new($"{prefix} Ice Ice Demon Lair", map, 110000, typeof(IceFiend), TimeSpan.FromHours(8));
            iceDemon.SetSpawnProfile(DungeonDepth.Entrance, [typeof(SnowElemental)]);
            iceDemon.SetSpawnProfile(DungeonDepth.Deep, [typeof(IceFiend)]);
            DungeonManager.Zones[iceDemon.ZoneId] = iceDemon;

            // ========================================================================
            // Orc Cave
            // ========================================================================
            DungeonZone orcEnt = new($"{prefix} Orc Cave Entrance", map, 25000, null, TimeSpan.FromHours(2));
            orcEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Orc)]);
            DungeonManager.Zones[orcEnt.ZoneId] = orcEnt;

            DungeonZone orcL1 = new($"{prefix} Orc Cave Level 1", map, 35000, typeof(OrcCaptain), TimeSpan.FromHours(4));
            orcL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Orc)]);
            orcL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(OrcCaptain), typeof(OrcishMage)]);
            DungeonManager.Zones[orcL1.ZoneId] = orcL1;

            DungeonZone orcL2 = new($"{prefix} Orc Cave Level 2", map, 45000, typeof(OrcBrute), TimeSpan.FromHours(4));
            orcL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(OrcCaptain)]);
            orcL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(OrcBrute)]);
            DungeonManager.Zones[orcL2.ZoneId] = orcL2;

            DungeonZone orcL3 = new($"{prefix} Orc Cave Level 3", map, 55000, null, TimeSpan.FromHours(6));
            orcL3.SetSpawnProfile(DungeonDepth.Entrance, [typeof(OrcishMage)]);
            orcL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(OrcBrute)]);
            DungeonManager.Zones[orcL3.ZoneId] = orcL3;

            // ========================================================================
            // Painted Caves
            // ========================================================================
            DungeonZone paintedCaves = new($"{prefix} Painted Caves Entrance", map, 85000, typeof(Troglodyte), TimeSpan.FromHours(6));
            paintedCaves.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Troglodyte)]);
            DungeonManager.Zones[paintedCaves.ZoneId] = paintedCaves;

            // ========================================================================
            // Palace of Paroxysmus
            // ========================================================================
            DungeonZone paroxysmus = new($"{prefix} Palace of Paroxysmus Entrance", map, 160000, typeof(ChiefParoxysmus), TimeSpan.FromHours(12));
            paroxysmus.SetSpawnProfile(DungeonDepth.Entrance, [typeof(PlagueBeast), typeof(BogThing)]);
            paroxysmus.SetSpawnProfile(DungeonDepth.Deep, [typeof(PoisonElemental)]);
            DungeonManager.Zones[paroxysmus.ZoneId] = paroxysmus;

            // ========================================================================
            // Prism of Light
            // ========================================================================
            DungeonZone prismLight = new($"{prefix} Prism of Light Entrance", map, 130000, typeof(ShimmeringEffusion), TimeSpan.FromHours(10));
            prismLight.SetSpawnProfile(DungeonDepth.Entrance, [typeof(CrystalElemental), typeof(Wisp)]);
            DungeonManager.Zones[prismLight.ZoneId] = prismLight;

            // ========================================================================
            // Sanctuary
            // ========================================================================
            DungeonZone sanctuary = new($"{prefix} Sanctuary Entrance", map, 110000, typeof(Succubus), TimeSpan.FromHours(8));
            sanctuary.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Gargoyle), typeof(Ratman)]);
            sanctuary.SetSpawnProfile(DungeonDepth.Deep, [typeof(Succubus)]);
            DungeonManager.Zones[sanctuary.ZoneId] = sanctuary;

            // ========================================================================
            // BlackSolen Hives (전 구역 복원)
            // ========================================================================
            DungeonZone BlackSolenCentral = new($"{prefix} BlackSolen Hives Central Area", map, 85000, typeof(BlackSolenQueen), TimeSpan.FromHours(8));
            BlackSolenCentral.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWorker)]);
            BlackSolenCentral.SetSpawnProfile(DungeonDepth.Deep, [typeof(BlackSolenWarrior), typeof(BlackSolenQueen)]);
            DungeonManager.Zones[BlackSolenCentral.ZoneId] = BlackSolenCentral;

            DungeonZone BlackSolenAEnt = new($"{prefix} BlackSolen Hives Area A Entrance", map, 35000, null, TimeSpan.FromHours(2));
            BlackSolenAEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWorker)]);
            DungeonManager.Zones[BlackSolenAEnt.ZoneId] = BlackSolenAEnt;

            DungeonZone BlackSolenAL1 = new($"{prefix} BlackSolen Hives Area A Level 1", map, 45000, typeof(BlackSolenWarrior), TimeSpan.FromHours(4));
            BlackSolenAL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWorker)]);
            BlackSolenAL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(BlackSolenWarrior)]);
            DungeonManager.Zones[BlackSolenAL1.ZoneId] = BlackSolenAL1;

            DungeonZone BlackSolenAL2 = new($"{prefix} BlackSolen Hives Area A Level 2", map, 55000, typeof(BlackSolenQueen), TimeSpan.FromHours(6));
            BlackSolenAL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWarrior)]);
            BlackSolenAL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(BlackSolenQueen)]);
            DungeonManager.Zones[BlackSolenAL2.ZoneId] = BlackSolenAL2;

            DungeonZone BlackSolenBEnt = new($"{prefix} BlackSolen Hives Area B Entrance", map, 35000, null, TimeSpan.FromHours(2));
            BlackSolenBEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWorker)]);
            DungeonManager.Zones[BlackSolenBEnt.ZoneId] = BlackSolenBEnt;

            DungeonZone BlackSolenBL1 = new($"{prefix} BlackSolen Hives Area B Level 1", map, 45000, typeof(BlackSolenWarrior), TimeSpan.FromHours(4));
            BlackSolenBL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWorker)]);
            BlackSolenBL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(BlackSolenWarrior)]);
            DungeonManager.Zones[BlackSolenBL1.ZoneId] = BlackSolenBL1;

            DungeonZone BlackSolenBL2 = new($"{prefix} BlackSolen Hives Area B Level 2", map, 55000, typeof(BlackSolenQueen), TimeSpan.FromHours(6));
            BlackSolenBL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWarrior)]);
            BlackSolenBL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(BlackSolenQueen)]);
            DungeonManager.Zones[BlackSolenBL2.ZoneId] = BlackSolenBL2;

            DungeonZone BlackSolenCEnt = new($"{prefix} BlackSolen Hives Area C Entrance", map, 35000, null, TimeSpan.FromHours(2));
            BlackSolenCEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWorker)]);
            DungeonManager.Zones[BlackSolenCEnt.ZoneId] = BlackSolenCEnt;

            DungeonZone BlackSolenCL1 = new($"{prefix} BlackSolen Hives Area C Level 1", map, 45000, typeof(BlackSolenWarrior), TimeSpan.FromHours(4));
            BlackSolenCL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWorker)]);
            BlackSolenCL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(BlackSolenWarrior)]);
            DungeonManager.Zones[BlackSolenCL1.ZoneId] = BlackSolenCL1;

            DungeonZone BlackSolenCL2 = new($"{prefix} BlackSolen Hives Area C Level 2", map, 55000, typeof(BlackSolenQueen), TimeSpan.FromHours(6));
            BlackSolenCL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWarrior)]);
            BlackSolenCL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(BlackSolenQueen)]);
            DungeonManager.Zones[BlackSolenCL2.ZoneId] = BlackSolenCL2;

            DungeonZone BlackSolenDEnt = new($"{prefix} BlackSolen Hives Area D Entrance", map, 35000, null, TimeSpan.FromHours(2));
            BlackSolenDEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWorker)]);
            DungeonManager.Zones[BlackSolenDEnt.ZoneId] = BlackSolenDEnt;

            DungeonZone BlackSolenDL1 = new($"{prefix} BlackSolen Hives Area D Level 1", map, 45000, typeof(BlackSolenWarrior), TimeSpan.FromHours(4));
            BlackSolenDL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWorker)]);
            BlackSolenDL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(BlackSolenWarrior)]);
            DungeonManager.Zones[BlackSolenDL1.ZoneId] = BlackSolenDL1;

            DungeonZone BlackSolenDL2 = new($"{prefix} BlackSolen Hives Area D Level 2", map, 55000, typeof(BlackSolenQueen), TimeSpan.FromHours(6));
            BlackSolenDL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWarrior)]);
            BlackSolenDL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(BlackSolenQueen)]);
            DungeonManager.Zones[BlackSolenDL2.ZoneId] = BlackSolenDL2;

            DungeonZone BlackSolenEEnt = new($"{prefix} BlackSolen Hives Area E Entrance", map, 35000, null, TimeSpan.FromHours(2));
            BlackSolenEEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWorker)]);
            DungeonManager.Zones[BlackSolenEEnt.ZoneId] = BlackSolenEEnt;

            // ========================================================================
            // Khaldun
            // ========================================================================
            DungeonZone khaldunEnt1 = new($"{prefix} Khaldun Entrance 1", map, 45000, null, TimeSpan.FromHours(4));
            khaldunEnt1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Skeleton), typeof(Zombie)]);
            DungeonManager.Zones[khaldunEnt1.ZoneId] = khaldunEnt1;

            DungeonZone khaldunEnt2 = new($"{prefix} Khaldun Entrance 2", map, 45000, null, TimeSpan.FromHours(4));
            khaldunEnt2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Skeleton), typeof(Zombie)]);
            DungeonManager.Zones[khaldunEnt2.ZoneId] = khaldunEnt2;

            DungeonZone khaldunL1 = new($"{prefix} Khaldun Level 1", map, 110000, typeof(ShadowKnight), TimeSpan.FromHours(8));
            khaldunL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BoneKnight), typeof(SkeletalMage)]);
            khaldunL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(ShadowKnight), typeof(RottingCorpse)]);
            DungeonManager.Zones[khaldunL1.ZoneId] = khaldunL1;


            // ========================================================================
            // [CUSTOM / HIDDEN] 비밀 및 로스트랜드 구역
            // ========================================================================

            DungeonZone britainSewers = new($"{customPrefix} Britain Sewers", map, 45000, typeof(Executioner), TimeSpan.FromHours(4));
            britainSewers.SetSpawnProfile(DungeonDepth.Entrance, [typeof(GiantRat), typeof(Slime), typeof(BullFrog)]);
            britainSewers.SetSpawnProfile(DungeonDepth.Middle, [typeof(Brigand), typeof(Thief)]);
            britainSewers.SetSpawnProfile(DungeonDepth.Deep, [typeof(Executioner)]);
            DungeonManager.Zones[britainSewers.ZoneId] = britainSewers;

            DungeonZone hedgeMaze = new($"{customPrefix} Hedge Maze", map, 65000, typeof(Daemon), TimeSpan.FromHours(6));
            hedgeMaze.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Imp), typeof(HeadlessOne)]);
            hedgeMaze.SetSpawnProfile(DungeonDepth.Middle, [typeof(EvilMage)]);
            hedgeMaze.SetSpawnProfile(DungeonDepth.Deep, [typeof(Daemon)]);
            DungeonManager.Zones[hedgeMaze.ZoneId] = hedgeMaze;

            DungeonZone ophidianLair = new($"{customPrefix} Ophidian Lair", map, 110000, typeof(OphidianMatriarch), TimeSpan.FromHours(10));
            ophidianLair.SetSpawnProfile(DungeonDepth.Entrance, [typeof(OphidianWarrior)]);
            ophidianLair.SetSpawnProfile(DungeonDepth.Middle, [typeof(OphidianMage), typeof(OphidianKnight)]);
            ophidianLair.SetSpawnProfile(DungeonDepth.Deep, [typeof(OphidianMatriarch), typeof(PoisonElemental)]);
            DungeonManager.Zones[ophidianLair.ZoneId] = ophidianLair;

            DungeonZone deluciaPassage = new($"{customPrefix} Delucia Passage", map, 65000, typeof(BloodElemental), TimeSpan.FromHours(6));
            deluciaPassage.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BoneKnight), typeof(Wraith)]);
            deluciaPassage.SetSpawnProfile(DungeonDepth.Deep, [typeof(BloodElemental)]);
            DungeonManager.Zones[deluciaPassage.ZoneId] = deluciaPassage;

            DungeonZone cityOfDead = new($"{customPrefix} City of the Dead", map, 85000, typeof(LichLord), TimeSpan.FromHours(8));
            cityOfDead.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Skeleton), typeof(Zombie), typeof(Mummy)]);
            cityOfDead.SetSpawnProfile(DungeonDepth.Deep, [typeof(Lich), typeof(LichLord)]);
            DungeonManager.Zones[cityOfDead.ZoneId] = cityOfDead;
        }
    }
}