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