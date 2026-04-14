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

            // ========================================================================
            // Blighted Grove (몬데인의 유산)
            // ========================================================================
            DungeonZone blightedGroveEnt = new((RegionCode)220101, map, 85000, null, TimeSpan.FromHours(8));
            blightedGroveEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Bogling), typeof(Corpser)]);
            blightedGroveEnt.SetSpawnProfile(DungeonDepth.Deep, [typeof(BogThing), typeof(Wisp)]);
            DungeonManager.RegisterZone(blightedGroveEnt);

            // ========================================================================
            // Covetous
            // ========================================================================
            DungeonZone covetousEnt = new((RegionCode)220200, map, 25000, null, TimeSpan.FromHours(2));
            covetousEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Bird), typeof(Eagle), typeof(Harpy)]);
            DungeonManager.RegisterZone(covetousEnt);

            DungeonZone covetousL1 = new(RegionCode.Felucca_Dungeon_Covetous_Level1, map, 35000, typeof(Harpy), TimeSpan.FromHours(4));
            covetousL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Harpy)]);
            covetousL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(StoneHarpy)]);
            DungeonManager.RegisterZone(covetousL1);

            DungeonZone covetousL2 = new(RegionCode.Felucca_Dungeon_Covetous_Level2, map, 55000, typeof(Gazer), TimeSpan.FromHours(6));
            covetousL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(StoneHarpy), typeof(Gargoyle)]);
            covetousL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(Gazer)]);
            DungeonManager.RegisterZone(covetousL2);

            DungeonZone covetousL3 = new(RegionCode.Felucca_Dungeon_Covetous_Level3, map, 85000, typeof(ElderGazer), TimeSpan.FromHours(8));
            covetousL3.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Gazer), typeof(PoisonElemental)]);
            covetousL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(ElderGazer), typeof(BloodElemental)]);
            DungeonManager.RegisterZone(covetousL3);

            DungeonZone covetousLake = new((RegionCode)220204, map, 65000, typeof(WaterElemental), TimeSpan.FromHours(6));
            covetousLake.SetSpawnProfile(DungeonDepth.Entrance, [typeof(WaterElemental)]);
            covetousLake.SetSpawnProfile(DungeonDepth.Deep, [typeof(SeaSerpent), typeof(Kraken)]);
            DungeonManager.RegisterZone(covetousLake);

            DungeonZone covetousTorture = new((RegionCode)220205, map, 75000, typeof(Executioner), TimeSpan.FromHours(6));
            covetousTorture.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Skeleton), typeof(Zombie)]);
            covetousTorture.SetSpawnProfile(DungeonDepth.Deep, [typeof(Executioner), typeof(BoneKnight)]);
            DungeonManager.RegisterZone(covetousTorture);

            // ========================================================================
            // Deceit
            // ========================================================================
            DungeonZone deceitEnt = new((RegionCode)220300, map, 25000, null, TimeSpan.FromHours(2));
            deceitEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Skeleton), typeof(Zombie)]);
            DungeonManager.RegisterZone(deceitEnt);

            DungeonZone deceitL1 = new(RegionCode.Felucca_Dungeon_Deceit_Level1, map, 45000, typeof(SkeletalKnight), TimeSpan.FromHours(4));
            deceitL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Skeleton), typeof(Ghoul)]);
            deceitL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(Wraith)]);
            DungeonManager.RegisterZone(deceitL1);

            DungeonZone deceitL2 = new(RegionCode.Felucca_Dungeon_Deceit_Level2, map, 65000, typeof(Lich), TimeSpan.FromHours(6));
            deceitL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BoneKnight), typeof(SkeletalMage)]);
            deceitL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(Mummy)]);
            DungeonManager.RegisterZone(deceitL2);

            DungeonZone deceitL3 = new(RegionCode.Felucca_Dungeon_Deceit_Level3, map, 85000, typeof(LichLord), TimeSpan.FromHours(8));
            deceitL3.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Lich), typeof(RottingCorpse)]);
            deceitL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(PoisonElemental)]);
            DungeonManager.RegisterZone(deceitL3);

            DungeonZone deceitL4 = new(RegionCode.Felucca_Dungeon_Deceit_Level4, map, 130000, typeof(SilverSerpent), TimeSpan.FromHours(10));
            deceitL4.SetSpawnProfile(DungeonDepth.Entrance, [typeof(LichLord), typeof(RottingCorpse)]);
            deceitL4.SetSpawnProfile(DungeonDepth.Deep, [typeof(SilverSerpent), typeof(PoisonElemental)]);
            DungeonManager.RegisterZone(deceitL4);

            // ========================================================================
            // Despise
            // ========================================================================
            DungeonZone despiseEnt = new((RegionCode)220400, map, 25000, null, TimeSpan.FromHours(2));
            despiseEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Lizardman)]);
            DungeonManager.RegisterZone(despiseEnt);

            DungeonZone despiseEntryway = new((RegionCode)220401, map, 30000, null, TimeSpan.FromHours(2));
            despiseEntryway.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Lizardman), typeof(GiantRat)]);
            DungeonManager.RegisterZone(despiseEntryway);

            DungeonZone despiseL1 = new(RegionCode.Felucca_Dungeon_Despise_Level1, map, 35000, typeof(EarthElemental), TimeSpan.FromHours(4));
            despiseL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Lizardman)]);
            despiseL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(EarthElemental)]);
            DungeonManager.RegisterZone(despiseL1);

            DungeonZone despiseL2 = new(RegionCode.Felucca_Dungeon_Despise_Level2, map, 55000, typeof(OgreLord), TimeSpan.FromHours(6));
            despiseL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Ettin), typeof(Ogre)]);
            despiseL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(OgreLord), typeof(Cyclops)]);
            DungeonManager.RegisterZone(despiseL2);

            DungeonZone despiseL3 = new(RegionCode.Felucca_Dungeon_Despise_Level3, map, 85000, typeof(Titan), TimeSpan.FromHours(8));
            despiseL3.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Cyclops), typeof(OgreLord)]);
            despiseL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(Titan)]);
            DungeonManager.RegisterZone(despiseL3);

            // ========================================================================
            // Destard
            // ========================================================================
            DungeonZone destardEnt = new((RegionCode)220500, map, 35000, null, TimeSpan.FromHours(2));
            destardEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(GiantSerpent)]);
            DungeonManager.RegisterZone(destardEnt);

            DungeonZone destardL1 = new(RegionCode.Felucca_Dungeon_Destard_Level1, map, 75000, typeof(Wyvern), TimeSpan.FromHours(6));
            destardL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Drake)]);
            destardL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(Wyvern)]);
            DungeonManager.RegisterZone(destardL1);

            DungeonZone destardL2 = new(RegionCode.Felucca_Dungeon_Destard_Level2, map, 110000, typeof(Dragon), TimeSpan.FromHours(8));
            destardL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Drake), typeof(Wyvern)]);
            destardL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(Dragon)]);
            DungeonManager.RegisterZone(destardL2);

            DungeonZone destardL3 = new(RegionCode.Felucca_Dungeon_Destard_Level3, map, 160000, typeof(AncientWyrm), TimeSpan.FromHours(12));
            destardL3.SetSpawnProfile(DungeonDepth.Entrance, [typeof(GreaterDragon)]);
            destardL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(ShadowWyrm)]);
            DungeonManager.RegisterZone(destardL3);

            // ========================================================================
            // Hythloth
            // ========================================================================
            DungeonZone hythlothEnt = new((RegionCode)220600, map, 45000, null, TimeSpan.FromHours(4));
            hythlothEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Gargoyle), typeof(Imp)]);
            DungeonManager.RegisterZone(hythlothEnt);

            DungeonZone hythlothL1 = new(RegionCode.Felucca_Dungeon_Hythloth_Level1, map, 65000, typeof(HellHound), TimeSpan.FromHours(6));
            hythlothL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Gargoyle)]);
            hythlothL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(HellHound), typeof(Daemon)]);
            DungeonManager.RegisterZone(hythlothL1);

            DungeonZone hythlothL2 = new(RegionCode.Felucca_Dungeon_Hythloth_Level2, map, 85000, typeof(Daemon), TimeSpan.FromHours(8));
            hythlothL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(HellHound)]);
            hythlothL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(Daemon), typeof(Succubus)]);
            DungeonManager.RegisterZone(hythlothL2);

            DungeonZone hythlothL3 = new(RegionCode.Felucca_Dungeon_Hythloth_Level3, map, 110000, typeof(Balron), TimeSpan.FromHours(10));
            hythlothL3.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Daemon), typeof(Succubus)]);
            hythlothL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(Balron)]);
            DungeonManager.RegisterZone(hythlothL3);

            DungeonZone hythlothL4 = new(RegionCode.Felucca_Dungeon_Hythloth_Level4, map, 160000, null, TimeSpan.FromHours(12));
            hythlothL4.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Balron)]);
            hythlothL4.SetSpawnProfile(DungeonDepth.Deep, [typeof(BloodElemental)]);
            DungeonManager.RegisterZone(hythlothL4);

            // ========================================================================
            // Shame
            // ========================================================================
            DungeonZone shameEnt = new((RegionCode)220700, map, 25000, null, TimeSpan.FromHours(2));
            shameEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(EarthElemental)]);
            DungeonManager.RegisterZone(shameEnt);

            DungeonZone shameL1 = new(RegionCode.Felucca_Dungeon_Shame_Level1, map, 45000, typeof(EarthElemental), TimeSpan.FromHours(4));
            shameL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Scorpion)]);
            shameL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(EarthElemental)]);
            DungeonManager.RegisterZone(shameL1);

            DungeonZone shameL2 = new(RegionCode.Felucca_Dungeon_Shame_Level2, map, 65000, typeof(AirElemental), TimeSpan.FromHours(6));
            shameL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(EarthElemental)]);
            shameL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(AirElemental), typeof(WaterElemental)]);
            DungeonManager.RegisterZone(shameL2);

            DungeonZone shameL3 = new(RegionCode.Felucca_Dungeon_Shame_Level3, map, 85000, typeof(PoisonElemental), TimeSpan.FromHours(8));
            shameL3.SetSpawnProfile(DungeonDepth.Entrance, [typeof(WaterElemental), typeof(FireElemental)]);
            shameL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(PoisonElemental)]);
            DungeonManager.RegisterZone(shameL3);

            DungeonZone shameL4 = new(RegionCode.Felucca_Dungeon_Shame_Level4, map, 110000, typeof(BloodElemental), TimeSpan.FromHours(10));
            shameL4.SetSpawnProfile(DungeonDepth.Entrance, [typeof(PoisonElemental)]);
            shameL4.SetSpawnProfile(DungeonDepth.Deep, [typeof(BloodElemental)]);
            DungeonManager.RegisterZone(shameL4);

            // ========================================================================
            // Wrong
            // ========================================================================
            DungeonZone wrongEnt = new((RegionCode)220800, map, 25000, null, TimeSpan.FromHours(2));
            wrongEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Brigand)]);
            DungeonManager.RegisterZone(wrongEnt);

            DungeonZone wrongL1 = new(RegionCode.Felucca_Dungeon_Wrong_Level1, map, 45000, typeof(Executioner), TimeSpan.FromHours(4));
            wrongL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Brigand)]);
            wrongL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(Executioner)]);
            DungeonManager.RegisterZone(wrongL1);

            DungeonZone wrongL2 = new(RegionCode.Felucca_Dungeon_Wrong_Level2, map, 65000, typeof(JukaMage), TimeSpan.FromHours(6));
            wrongL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Executioner)]);
            wrongL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(JukaWarrior), typeof(JukaMage)]);
            DungeonManager.RegisterZone(wrongL2);

            DungeonZone wrongL3 = new(RegionCode.Felucca_Dungeon_Wrong_Level3, map, 85000, typeof(JukaLord), TimeSpan.FromHours(8));
            wrongL3.SetSpawnProfile(DungeonDepth.Entrance, [typeof(JukaMage)]);
            wrongL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(JukaLord)]);
            DungeonManager.RegisterZone(wrongL3);

            // ========================================================================
            // Terathan Keep
            // ========================================================================
            DungeonZone teraEnt = new((RegionCode)221000, map, 35000, null, TimeSpan.FromHours(2));
            teraEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(GiantSpider)]);
            DungeonManager.RegisterZone(teraEnt);

            DungeonZone teraL1 = new((RegionCode)221001, map, 55000, typeof(TerathanWarrior), TimeSpan.FromHours(5));
            teraL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(TerathanDrone)]);
            teraL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(TerathanWarrior)]);
            DungeonManager.RegisterZone(teraL1);

            DungeonZone teraChamp = new((RegionCode)221002, map, 85000, typeof(TerathanAvenger), TimeSpan.FromHours(8));
            teraChamp.SetSpawnProfile(DungeonDepth.Entrance, [typeof(TerathanWarrior)]);
            teraChamp.SetSpawnProfile(DungeonDepth.Deep, [typeof(TerathanAvenger)]);
            DungeonManager.RegisterZone(teraChamp);

            DungeonZone teraStar = new((RegionCode)221003, map, 130000, typeof(TerathanMatriarch), TimeSpan.FromHours(12));
            teraStar.SetSpawnProfile(DungeonDepth.Entrance, [typeof(TerathanAvenger)]);
            teraStar.SetSpawnProfile(DungeonDepth.Deep, [typeof(TerathanMatriarch), typeof(PoisonElemental)]);
            DungeonManager.RegisterZone(teraStar);

            // ========================================================================
            // Fire
            // ========================================================================
            DungeonZone fireEnt = new((RegionCode)221100, map, 35000, null, TimeSpan.FromHours(2));
            fireEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Slime), typeof(HellCat)]);
            DungeonManager.RegisterZone(fireEnt);

            DungeonZone fireBritEnt = new((RegionCode)221101, map, 35000, null, TimeSpan.FromHours(2));
            fireBritEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Slime)]);
            DungeonManager.RegisterZone(fireBritEnt);

            DungeonZone fireL1 = new((RegionCode)221102, map, 65000, typeof(FireElemental), TimeSpan.FromHours(6));
            fireL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(FireGargoyle)]);
            fireL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(FireElemental), typeof(LavaLizard)]);
            DungeonManager.RegisterZone(fireL1);

            DungeonZone fireL2 = new((RegionCode)221103, map, 110000, typeof(Efreet), TimeSpan.FromHours(8));
            fireL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(FireElemental)]);
            fireL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(Efreet)]);
            DungeonManager.RegisterZone(fireL2);

            // ========================================================================
            // Ice
            // ========================================================================
            DungeonZone iceEnt = new((RegionCode)221200, map, 35000, null, TimeSpan.FromHours(2));
            iceEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(SnowLeopard), typeof(PolarBear)]);
            DungeonManager.RegisterZone(iceEnt);

            DungeonZone iceBritEnt = new((RegionCode)221201, map, 35000, null, TimeSpan.FromHours(2));
            iceBritEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(FrostSpider)]);
            DungeonManager.RegisterZone(iceBritEnt);

            DungeonZone iceL1 = new((RegionCode)221202, map, 65000, typeof(SnowElemental), TimeSpan.FromHours(6));
            iceL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(FrostTroll), typeof(FrostSpider)]);
            iceL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(SnowElemental), typeof(IceSnake)]);
            DungeonManager.RegisterZone(iceL1);

            DungeonZone iceRatman = new((RegionCode)221203, map, 45000, typeof(RatmanMage), TimeSpan.FromHours(4));
            iceRatman.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Ratman)]);
            iceRatman.SetSpawnProfile(DungeonDepth.Deep, [typeof(RatmanArcher), typeof(RatmanMage)]);
            DungeonManager.RegisterZone(iceRatman);

            DungeonZone iceDemon = new((RegionCode)221204, map, 110000, typeof(IceFiend), TimeSpan.FromHours(8));
            iceDemon.SetSpawnProfile(DungeonDepth.Entrance, [typeof(SnowElemental)]);
            iceDemon.SetSpawnProfile(DungeonDepth.Deep, [typeof(IceFiend)]);
            DungeonManager.RegisterZone(iceDemon);

            // ========================================================================
            // Orc Cave
            // ========================================================================
            DungeonZone orcEnt = new((RegionCode)221300, map, 25000, null, TimeSpan.FromHours(2));
            orcEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Orc)]);
            DungeonManager.RegisterZone(orcEnt);

            DungeonZone orcL1 = new((RegionCode)221301, map, 35000, typeof(OrcCaptain), TimeSpan.FromHours(4));
            orcL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Orc)]);
            orcL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(OrcCaptain), typeof(OrcishMage)]);
            DungeonManager.RegisterZone(orcL1);

            DungeonZone orcL2 = new((RegionCode)221302, map, 45000, typeof(OrcBrute), TimeSpan.FromHours(4));
            orcL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(OrcCaptain)]);
            orcL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(OrcBrute)]);
            DungeonManager.RegisterZone(orcL2);

            DungeonZone orcL3 = new((RegionCode)221303, map, 55000, null, TimeSpan.FromHours(6));
            orcL3.SetSpawnProfile(DungeonDepth.Entrance, [typeof(OrcishMage)]);
            orcL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(OrcBrute)]);
            DungeonManager.RegisterZone(orcL3);

            // ========================================================================
            // Painted Caves / Paroxysmus / Prism of Light / Sanctuary
            // ========================================================================
            DungeonZone paintedCaves = new((RegionCode)221400, map, 85000, typeof(Troglodyte), TimeSpan.FromHours(6));
            paintedCaves.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Troglodyte)]);
            DungeonManager.RegisterZone(paintedCaves);

            DungeonZone paroxysmus = new((RegionCode)221500, map, 160000, typeof(ChiefParoxysmus), TimeSpan.FromHours(12));
            paroxysmus.SetSpawnProfile(DungeonDepth.Entrance, [typeof(PlagueBeast), typeof(BogThing)]);
            paroxysmus.SetSpawnProfile(DungeonDepth.Deep, [typeof(PoisonElemental)]);
            DungeonManager.RegisterZone(paroxysmus);

            DungeonZone prismLight = new((RegionCode)221600, map, 130000, typeof(ShimmeringEffusion), TimeSpan.FromHours(10));
            prismLight.SetSpawnProfile(DungeonDepth.Entrance, [typeof(CrystalElemental), typeof(Wisp)]);
            DungeonManager.RegisterZone(prismLight);

            DungeonZone sanctuary = new((RegionCode)221700, map, 110000, typeof(Succubus), TimeSpan.FromHours(8));
            sanctuary.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Gargoyle), typeof(Ratman)]);
            sanctuary.SetSpawnProfile(DungeonDepth.Deep, [typeof(Succubus)]);
            DungeonManager.RegisterZone(sanctuary);

            // ========================================================================
            // BlackSolen Hives
            // ========================================================================
            DungeonZone BlackSolenCentral = new((RegionCode)221800, map, 85000, typeof(BlackSolenQueen), TimeSpan.FromHours(8));
            BlackSolenCentral.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWorker)]);
            BlackSolenCentral.SetSpawnProfile(DungeonDepth.Deep, [typeof(BlackSolenWarrior), typeof(BlackSolenQueen)]);
            DungeonManager.RegisterZone(BlackSolenCentral);

            DungeonZone BlackSolenAEnt = new((RegionCode)221801, map, 35000, null, TimeSpan.FromHours(2));
            BlackSolenAEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWorker)]);
            DungeonManager.RegisterZone(BlackSolenAEnt);

            DungeonZone BlackSolenAL1 = new((RegionCode)221802, map, 45000, typeof(BlackSolenWarrior), TimeSpan.FromHours(4));
            BlackSolenAL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWorker)]);
            BlackSolenAL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(BlackSolenWarrior)]);
            DungeonManager.RegisterZone(BlackSolenAL1);

            DungeonZone BlackSolenAL2 = new((RegionCode)221803, map, 55000, typeof(BlackSolenQueen), TimeSpan.FromHours(6));
            BlackSolenAL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWarrior)]);
            BlackSolenAL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(BlackSolenQueen)]);
            DungeonManager.RegisterZone(BlackSolenAL2);

            DungeonZone BlackSolenBEnt = new((RegionCode)221804, map, 35000, null, TimeSpan.FromHours(2));
            BlackSolenBEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWorker)]);
            DungeonManager.RegisterZone(BlackSolenBEnt);

            DungeonZone BlackSolenBL1 = new((RegionCode)221805, map, 45000, typeof(BlackSolenWarrior), TimeSpan.FromHours(4));
            BlackSolenBL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWorker)]);
            BlackSolenBL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(BlackSolenWarrior)]);
            DungeonManager.RegisterZone(BlackSolenBL1);

            DungeonZone BlackSolenBL2 = new((RegionCode)221806, map, 55000, typeof(BlackSolenQueen), TimeSpan.FromHours(6));
            BlackSolenBL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWarrior)]);
            BlackSolenBL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(BlackSolenQueen)]);
            DungeonManager.RegisterZone(BlackSolenBL2);

            DungeonZone BlackSolenCEnt = new((RegionCode)221807, map, 35000, null, TimeSpan.FromHours(2));
            BlackSolenCEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWorker)]);
            DungeonManager.RegisterZone(BlackSolenCEnt);

            DungeonZone BlackSolenCL1 = new((RegionCode)221808, map, 45000, typeof(BlackSolenWarrior), TimeSpan.FromHours(4));
            BlackSolenCL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWorker)]);
            BlackSolenCL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(BlackSolenWarrior)]);
            DungeonManager.RegisterZone(BlackSolenCL1);

            DungeonZone BlackSolenCL2 = new((RegionCode)221809, map, 55000, typeof(BlackSolenQueen), TimeSpan.FromHours(6));
            BlackSolenCL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWarrior)]);
            BlackSolenCL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(BlackSolenQueen)]);
            DungeonManager.RegisterZone(BlackSolenCL2);

            DungeonZone BlackSolenDEnt = new((RegionCode)221810, map, 35000, null, TimeSpan.FromHours(2));
            BlackSolenDEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWorker)]);
            DungeonManager.RegisterZone(BlackSolenDEnt);

            DungeonZone BlackSolenDL1 = new((RegionCode)221811, map, 45000, typeof(BlackSolenWarrior), TimeSpan.FromHours(4));
            BlackSolenDL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWorker)]);
            BlackSolenDL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(BlackSolenWarrior)]);
            DungeonManager.RegisterZone(BlackSolenDL1);

            DungeonZone BlackSolenDL2 = new((RegionCode)221812, map, 55000, typeof(BlackSolenQueen), TimeSpan.FromHours(6));
            BlackSolenDL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWarrior)]);
            BlackSolenDL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(BlackSolenQueen)]);
            DungeonManager.RegisterZone(BlackSolenDL2);

            DungeonZone BlackSolenEEnt = new((RegionCode)221813, map, 35000, null, TimeSpan.FromHours(2));
            BlackSolenEEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackSolenWorker)]);
            DungeonManager.RegisterZone(BlackSolenEEnt);

            // ========================================================================
            // Khaldun
            // ========================================================================
            DungeonZone khaldunEnt1 = new((RegionCode)220901, map, 45000, null, TimeSpan.FromHours(4));
            khaldunEnt1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Skeleton), typeof(Zombie)]);
            DungeonManager.RegisterZone(khaldunEnt1);

            DungeonZone khaldunEnt2 = new((RegionCode)220902, map, 45000, null, TimeSpan.FromHours(4));
            khaldunEnt2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Skeleton), typeof(Zombie)]);
            DungeonManager.RegisterZone(khaldunEnt2);

            DungeonZone khaldunL1 = new(RegionCode.Felucca_Dungeon_Khaldun_Level1, map, 110000, typeof(ShadowKnight), TimeSpan.FromHours(8));
            khaldunL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BoneKnight), typeof(SkeletalMage)]);
            khaldunL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(ShadowKnight), typeof(RottingCorpse)]);
            DungeonManager.RegisterZone(khaldunL1);

            // ========================================================================
            // [CUSTOM / HIDDEN] 비밀 및 로스트랜드 구역
            // ========================================================================
            DungeonZone britainSewers = new((RegionCode)221901, map, 45000, typeof(Executioner), TimeSpan.FromHours(4));
            britainSewers.SetSpawnProfile(DungeonDepth.Entrance, [typeof(GiantRat), typeof(Slime), typeof(BullFrog)]);
            britainSewers.SetSpawnProfile(DungeonDepth.Middle, [typeof(Brigand), typeof(Thief)]);
            britainSewers.SetSpawnProfile(DungeonDepth.Deep, [typeof(Executioner)]);
            DungeonManager.RegisterZone(britainSewers);

            DungeonZone hedgeMaze = new((RegionCode)221902, map, 65000, typeof(Daemon), TimeSpan.FromHours(6));
            hedgeMaze.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Imp), typeof(HeadlessOne)]);
            hedgeMaze.SetSpawnProfile(DungeonDepth.Middle, [typeof(EvilMage)]);
            hedgeMaze.SetSpawnProfile(DungeonDepth.Deep, [typeof(Daemon)]);
            DungeonManager.RegisterZone(hedgeMaze);

            DungeonZone ophidianLair = new((RegionCode)221903, map, 110000, typeof(OphidianMatriarch), TimeSpan.FromHours(10));
            ophidianLair.SetSpawnProfile(DungeonDepth.Entrance, [typeof(OphidianWarrior)]);
            ophidianLair.SetSpawnProfile(DungeonDepth.Middle, [typeof(OphidianMage), typeof(OphidianKnight)]);
            ophidianLair.SetSpawnProfile(DungeonDepth.Deep, [typeof(OphidianMatriarch), typeof(PoisonElemental)]);
            DungeonManager.RegisterZone(ophidianLair);

            DungeonZone deluciaPassage = new((RegionCode)221904, map, 65000, typeof(BloodElemental), TimeSpan.FromHours(6));
            deluciaPassage.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BoneKnight), typeof(Wraith)]);
            deluciaPassage.SetSpawnProfile(DungeonDepth.Deep, [typeof(BloodElemental)]);
            DungeonManager.RegisterZone(deluciaPassage);

            DungeonZone cityOfDead = new((RegionCode)221905, map, 85000, typeof(LichLord), TimeSpan.FromHours(8));
            cityOfDead.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Skeleton), typeof(Zombie), typeof(Mummy)]);
            cityOfDead.SetSpawnProfile(DungeonDepth.Deep, [typeof(Lich), typeof(LichLord)]);
            DungeonManager.RegisterZone(cityOfDead);
        }
    }
}