using System;
using Server;
using Server.Mobiles;

namespace Server.Misc
{
    public static class TrammelDungeon
    {
        public static void Setup()
        {
            Map map = Map.Trammel;

            // ========================================================================
            // [Blighted Grove] 블라이티드 그로브 (보스 없음)
            // ========================================================================
            DungeonZone blightedGrove = new(RegionCode.Trammel_Dungeon_BlightedGrove, map, 80000, null, TimeSpan.FromHours(8));
            blightedGrove.SetSpawnProfile(1, [typeof(Bogling), typeof(Corpser)]);
            blightedGrove.SetSpawnProfile(2, [typeof(Corpser)]);
            blightedGrove.SetSpawnProfile(3, [typeof(Wisp)]);
            DungeonManager.RegisterZone(blightedGrove); // 🌟 최적화된 등록 방식

            // ========================================================================
            // [Covetous] 코버투스
            // ========================================================================
            DungeonZone covetousL1 = new(RegionCode.Trammel_Dungeon_Covetous_Level1, map, 110000, typeof(GiantTurkey), TimeSpan.FromHours(4));
            covetousL1.SetSpawnProfile(1, [typeof(Eagle)]);
            covetousL1.SetSpawnProfile(2, [typeof(Mongbat)]);
            covetousL1.SetSpawnProfile(3, [typeof(Turkey)]);
            DungeonManager.RegisterZone(covetousL1);

            DungeonZone covetousL2 = new(RegionCode.Trammel_Dungeon_Covetous_Level2, map, 600000, typeof(GiantDreadSpider), TimeSpan.FromHours(6));
            covetousL2.SetSpawnProfile(1, [typeof(GiantSpider)]);
            covetousL2.SetSpawnProfile(2, [typeof(GiantBlackWidow), typeof(TrapdoorSpider)]);
            covetousL2.SetSpawnProfile(3, [typeof(WolfSpider), typeof(DreadSpider)]);
            DungeonManager.RegisterZone(covetousL2);

            DungeonZone covetousL3 = new(RegionCode.Trammel_Dungeon_Covetous_Level3, map, 2500000, typeof(Lilith), TimeSpan.FromHours(8));
            covetousL3.SetSpawnProfile(1, [typeof(VampireBat)]);
            covetousL3.SetSpawnProfile(2, [typeof(Harpy)]);
            covetousL3.SetSpawnProfile(3, [typeof(StoneHarpy), typeof(Succubus)]);
            DungeonManager.RegisterZone(covetousL3);

            DungeonZone covetousLake = new(RegionCode.Trammel_Dungeon_Covetous_LakeCave, map, 1000000, null, TimeSpan.FromHours(6));
            covetousLake.SetSpawnProfile(1, [typeof(Alligator)]);
            covetousLake.SetSpawnProfile(2, [typeof(WaterElemental)]);
            covetousLake.SetSpawnProfile(3, [typeof(SeaSerpent)]);
            DungeonManager.RegisterZone(covetousLake);

            DungeonZone covetousTorture = new(RegionCode.Trammel_Dungeon_Covetous_TortureChambers, map, 1500000, null, TimeSpan.FromHours(6));
            covetousTorture.SetSpawnProfile(1, [typeof(Skeleton), typeof(Zombie)]);
            covetousTorture.SetSpawnProfile(2, [typeof(Ghoul)]);
            covetousTorture.SetSpawnProfile(3, [typeof(BoneKnight), typeof(Executioner)]);
            DungeonManager.RegisterZone(covetousTorture);

            // ========================================================================
            // [Deceit] 디시트
            // ========================================================================
            DungeonZone deceitL1 = new(RegionCode.Trammel_Dungeon_Deceit_Level1, map, 1125000, typeof(SkeletalMount), TimeSpan.FromHours(4));
            deceitL1.SetSpawnProfile(1, [typeof(BoneMagi)]);
            deceitL1.SetSpawnProfile(2, [typeof(Mummy), typeof(PestilentBandage)]);
            deceitL1.SetSpawnProfile(3, [typeof(RottingCorpse), typeof(BoneKnight)]);
            DungeonManager.RegisterZone(deceitL1);

            DungeonZone deceitL2 = new(RegionCode.Trammel_Dungeon_Deceit_Level2, map, 1875000, typeof(BoneDemon), TimeSpan.FromHours(6));
            deceitL2.SetSpawnProfile(1, [typeof(SkeletalCatStatue), typeof(Ghoul)]);
            deceitL2.SetSpawnProfile(2, [typeof(PatchworkSkeleton), typeof(Shade)]);
            deceitL2.SetSpawnProfile(3, [typeof(SkeletalKnight), typeof(SkeletalMage), typeof(Wraith)]);
            DungeonManager.RegisterZone(deceitL2);

            DungeonZone deceitL3 = new(RegionCode.Trammel_Dungeon_Deceit_Level3, map, 3150000, typeof(AncientLich), TimeSpan.FromHours(8));
            deceitL3.SetSpawnProfile(1, [typeof(SkeletalLich)]);
            deceitL3.SetSpawnProfile(2, [typeof(Lich)]);
            deceitL3.SetSpawnProfile(3, [typeof(Lich)]);
            DungeonManager.RegisterZone(deceitL3);

            DungeonZone deceitL4 = new(RegionCode.Trammel_Dungeon_Deceit_Level4, map, 4050000, typeof(SkeletalDragon), TimeSpan.FromHours(10));
            deceitL4.SetSpawnProfile(1, [typeof(LichLord)]);
            deceitL4.SetSpawnProfile(2, [typeof(LichLord)]);
            deceitL4.SetSpawnProfile(3, [typeof(LichLord)]);
            DungeonManager.RegisterZone(deceitL4);

            // ========================================================================
            // [Despise] 데스파이즈
            // ========================================================================
            DungeonZone despiseL1 = new(RegionCode.Trammel_Dungeon_Despise_Level1, map, 90000, typeof(Reaper), TimeSpan.FromHours(4));
            despiseL1.SetSpawnProfile(1, [typeof(Llama), typeof(Horse)]);
            despiseL1.SetSpawnProfile(2, [typeof(Palomino), typeof(RidableLlama)]);
            despiseL1.SetSpawnProfile(3, [typeof(Bogling), typeof(ForestOstard), typeof(Treefellow), typeof(Corpser)]);
            DungeonManager.RegisterZone(despiseL1);

            DungeonZone despiseL2 = new(RegionCode.Trammel_Dungeon_Despise_Level2, map, 500000, typeof(BloodWorm), TimeSpan.FromHours(6));
            despiseL2.SetSpawnProfile(1, [typeof(HeadlessOne), typeof(Lizardman)]);
            despiseL2.SetSpawnProfile(2, [typeof(LizardmanDefender), typeof(Ettin)]);
            despiseL2.SetSpawnProfile(3, [typeof(Troll), typeof(Cyclops)]);
            DungeonManager.RegisterZone(despiseL2);

            DungeonZone despiseL3 = new(RegionCode.Trammel_Dungeon_Despise_Level3, map, 1500000, typeof(OgreLord), TimeSpan.FromHours(8));
            despiseL3.SetSpawnProfile(1, [typeof(Centaur)]);
            despiseL3.SetSpawnProfile(2, [typeof(Ogre)]);
            despiseL3.SetSpawnProfile(3, [typeof(Ogre), typeof(Centaur)]);
            DungeonManager.RegisterZone(despiseL3);

            // ========================================================================
            // [Destard] 데스타드
            // ========================================================================
            DungeonZone destardL1 = new(RegionCode.Trammel_Dungeon_Destard_Level1, map, 2500000, null, TimeSpan.FromHours(6));
            destardL1.SetSpawnProfile(1, [typeof(LavaSnake), typeof(GiantSerpent)]);
            destardL1.SetSpawnProfile(2, [typeof(DragonWolf), typeof(Drake)]);
            destardL1.SetSpawnProfile(3, [typeof(Wyvern), typeof(Dragon)]);
            DungeonManager.RegisterZone(destardL1);

            DungeonZone destardL2 = new(RegionCode.Trammel_Dungeon_Destard_Level2, map, 5500000, null, TimeSpan.FromHours(8));
            destardL2.SetSpawnProfile(1, [typeof(Drake)]);
            destardL2.SetSpawnProfile(2, [typeof(Dragon), typeof(TsukiWolf)]);
            destardL2.SetSpawnProfile(3, [typeof(ShadowWyrm), typeof(RidableDragon)]);
            DungeonManager.RegisterZone(destardL2);

            DungeonZone destardL3 = new(RegionCode.Trammel_Dungeon_Destard_Level3, map, 7500000, typeof(AncientWyrm), TimeSpan.FromHours(12));
            destardL3.SetSpawnProfile(1, [typeof(Dragon)]);
            destardL3.SetSpawnProfile(2, [typeof(ShadowWyrm)]);
            destardL3.SetSpawnProfile(3, [typeof(GreaterDragon)]);
            DungeonManager.RegisterZone(destardL3);

            // ========================================================================
            // [Hythloth] 히스로스
            // ========================================================================
            DungeonZone hythlothL1 = new(RegionCode.Trammel_Dungeon_Hythloth_Level1, map, 250000, null, TimeSpan.FromHours(4));
            hythlothL1.SetSpawnProfile(1, [typeof(Imp), typeof(Gargoyle)]);
            hythlothL1.SetSpawnProfile(2, [typeof(HellCat)]);
            hythlothL1.SetSpawnProfile(3, [typeof(HellHound), typeof(PredatorHellCat)]);
            DungeonManager.RegisterZone(hythlothL1);

            DungeonZone hythlothL2 = new(RegionCode.Trammel_Dungeon_Hythloth_Level2, map, 3000000, null, TimeSpan.FromHours(6));
            hythlothL2.SetSpawnProfile(1, [typeof(StoneGargoyle)]);
            hythlothL2.SetSpawnProfile(2, [typeof(Daemon)]);
            hythlothL2.SetSpawnProfile(3, [typeof(Daemon)]);
            DungeonManager.RegisterZone(hythlothL2);

            DungeonZone hythlothL3 = new(RegionCode.Trammel_Dungeon_Hythloth_Level3, map, 4500000, null, TimeSpan.FromHours(8));
            hythlothL3.SetSpawnProfile(1, [typeof(Daemon)]);
            hythlothL3.SetSpawnProfile(2, [typeof(HellSteed)]);
            hythlothL3.SetSpawnProfile(3, [typeof(ArchDaemon)]);
            DungeonManager.RegisterZone(hythlothL3);

            DungeonZone hythlothL4 = new(RegionCode.Trammel_Dungeon_Hythloth_Level4, map, 5500000, typeof(Balron), TimeSpan.FromHours(12));
            hythlothL4.SetSpawnProfile(1, [typeof(ArchDaemon)]);
            hythlothL4.SetSpawnProfile(2, [typeof(ArchDaemon)]);
            hythlothL4.SetSpawnProfile(3, [typeof(ArchDaemon)]);
            DungeonManager.RegisterZone(hythlothL4);

            // ========================================================================
            // [Shame] 쉐임
            // ========================================================================
            DungeonZone shameL1 = new(RegionCode.Trammel_Dungeon_Shame_Level1, map, 700000, typeof(EttinLord), TimeSpan.FromHours(4));
            shameL1.SetSpawnProfile(1, [typeof(Scorpion)]);
            shameL1.SetSpawnProfile(2, [typeof(ClockworkScorpion)]);
            shameL1.SetSpawnProfile(3, [typeof(EarthElemental)]);
            DungeonManager.RegisterZone(shameL1);

            DungeonZone shameL2 = new(RegionCode.Trammel_Dungeon_Shame_Level2, map, 900000, null, TimeSpan.FromHours(6));
            shameL2.SetSpawnProfile(1, [typeof(AirElemental)]);
            shameL2.SetSpawnProfile(2, [typeof(WaterElemental)]);
            shameL2.SetSpawnProfile(3, [typeof(FireElemental)]);
            DungeonManager.RegisterZone(shameL2);

            DungeonZone shameL3 = new(RegionCode.Trammel_Dungeon_Shame_Level3, map, 2600000, typeof(Beholder), TimeSpan.FromHours(8));
            shameL3.SetSpawnProfile(1, [typeof(Gazer)]);
            shameL3.SetSpawnProfile(2, [typeof(Gazer)]);
            shameL3.SetSpawnProfile(3, [typeof(ElderGazer)]);
            DungeonManager.RegisterZone(shameL3);

            DungeonZone shameL4 = new(RegionCode.Trammel_Dungeon_Shame_Level4, map, 6000000, null, TimeSpan.FromHours(10));
            shameL4.SetSpawnProfile(1, [typeof(BloodElemental)]);
            shameL4.SetSpawnProfile(2, [typeof(PoisonElemental)]);
            shameL4.SetSpawnProfile(3, [typeof(EnragedColossus)]);
            DungeonManager.RegisterZone(shameL4);

            // ========================================================================
            // [Fire Dungeon] 파이어 던전
            // ========================================================================
            DungeonZone fireL1 = new(RegionCode.Trammel_Dungeon_Fire_Level1, map, 750000, null, TimeSpan.FromHours(6));
            fireL1.SetSpawnProfile(1, [typeof(GrayGoblin), typeof(GreenGoblin)]);
            fireL1.SetSpawnProfile(2, [typeof(EnslavedGrayGoblin), typeof(EnslavedGreenGoblin), typeof(LavaLizard), typeof(UndeadGargoyle)]);
            fireL1.SetSpawnProfile(3, [typeof(EnslavedGargoyle), typeof(Gargoyle), typeof(FireGargoyle), typeof(LavaSerpent)]);
            DungeonManager.RegisterZone(fireL1);

            DungeonZone fireL2 = new(RegionCode.Trammel_Dungeon_Fire_Level2, map, 3500000, null, TimeSpan.FromHours(8));
            fireL2.SetSpawnProfile(1, [typeof(FireBeetle), typeof(LavaElemental)]);
            fireL2.SetSpawnProfile(2, [typeof(FireDrake), typeof(Efreet), typeof(GargoyleEnforcer)]);
            fireL2.SetSpawnProfile(3, [typeof(GargoyleDestroyer), typeof(FireSteed), typeof(FireDaemon), typeof(RedWyrm)]);
			fireL2.AddUnique(typeof(RedWyrm));
			fireL2.AddUnique(typeof(FireSteed));
            DungeonManager.RegisterZone(fireL2);

            // ========================================================================
            // [Ice Dungeon] 아이스 던전
            // ========================================================================
            DungeonZone iceL1 = new(RegionCode.Trammel_Dungeon_Ice_Level1, map, 750000, null, TimeSpan.FromHours(6));
            iceL1.SetSpawnProfile(1, [typeof(FrostOoze), typeof(FrostSpider)]);
            iceL1.SetSpawnProfile(2, [typeof(IceHound), typeof(FrostMite)]);
            iceL1.SetSpawnProfile(3, [typeof(GiantIceWorm), typeof(IceElemental), typeof(SnowElemental)]);
            DungeonManager.RegisterZone(iceL1);

            DungeonZone iceRatman = new(RegionCode.Trammel_Dungeon_Ice_RatmanRoom, map, 225000, null, TimeSpan.FromHours(4));
            iceRatman.SetSpawnProfile(1, [typeof(Ratman)]);
            iceRatman.SetSpawnProfile(2, [typeof(RatmanArcher)]);
            iceRatman.SetSpawnProfile(3, [typeof(RatmanMage)]);
            DungeonManager.RegisterZone(iceRatman);

            DungeonZone iceDemon = new(RegionCode.Trammel_Dungeon_Ice_IceDemonLair, map, 3375000, null, TimeSpan.FromHours(8));
            iceDemon.SetSpawnProfile(1, [typeof(SnowElemental)]);
            iceDemon.SetSpawnProfile(2, [typeof(ArcticOgreLord), typeof(ColdDrake)]);
            iceDemon.SetSpawnProfile(3, [typeof(IceFiend), typeof(WhiteWyrm)]);
			iceDemon.AddUnique(typeof(WhiteWyrm));
            DungeonManager.RegisterZone(iceDemon);

            // ========================================================================
            // [Orc Cave] 오크 동굴
            // ========================================================================
            DungeonZone orcL1 = new(RegionCode.Trammel_Dungeon_OrcCave_Level1, map, 300000, typeof(BogThing), TimeSpan.FromHours(4));
            orcL1.SetSpawnProfile(1, [typeof(OrcChopper)]);
            orcL1.SetSpawnProfile(2, [typeof(OrcBomber), typeof(OrcCaptain)]);
            orcL1.SetSpawnProfile(3, [typeof(OrcishMage)]);
            DungeonManager.RegisterZone(orcL1);

            DungeonZone orcL2 = new(RegionCode.Trammel_Dungeon_OrcCave_Level2, map, 675000, null, TimeSpan.FromHours(6));
            orcL2.SetSpawnProfile(1, [typeof(OrcishLord)]);
            orcL2.SetSpawnProfile(2, [typeof(OrcishLord)]);
            orcL2.SetSpawnProfile(3, [typeof(OrcishLord)]);
            DungeonManager.RegisterZone(orcL2);

            DungeonZone orcL3 = new(RegionCode.Trammel_Dungeon_OrcCave_Level3, map, 3000000, typeof(OrcBrute), TimeSpan.FromHours(8));
            orcL3.SetSpawnProfile(1, [typeof(Titan)]);
            orcL3.SetSpawnProfile(2, [typeof(Titan)]);
            orcL3.SetSpawnProfile(3, [typeof(Titan)]);
            DungeonManager.RegisterZone(orcL3);

            // ========================================================================
            // [Wrong] 롱 던전
            // ========================================================================
            DungeonZone wrongL1 = new(RegionCode.Trammel_Dungeon_Wrong_Level1, map, 1125000, null, TimeSpan.FromHours(4));
            wrongL1.SetSpawnProfile(1, [typeof(Brigand)]);
            wrongL1.SetSpawnProfile(2, [typeof(Golem), typeof(EvilMage)]);
            wrongL1.SetSpawnProfile(3, [typeof(ShadowDragon), typeof(ChaosDragoon)]);
            DungeonManager.RegisterZone(wrongL1);

            DungeonZone wrongL2 = new(RegionCode.Trammel_Dungeon_Wrong_Level2, map, 1875000, typeof(GolemLord), TimeSpan.FromHours(6));
            wrongL2.SetSpawnProfile(1, [typeof(GolemController)]);
            wrongL2.SetSpawnProfile(2, [typeof(Executioner)]);
            wrongL2.SetSpawnProfile(3, [typeof(Executioner)]);
            DungeonManager.RegisterZone(wrongL2);

            DungeonZone wrongL3 = new(RegionCode.Trammel_Dungeon_Wrong_Level3, map, 3375000, typeof(JukaLord), TimeSpan.FromHours(8));
            wrongL3.SetSpawnProfile(1, [typeof(JukaWarrior)]);
            wrongL3.SetSpawnProfile(2, [typeof(JukaMage), typeof(ChaosDragoonElite)]);
            wrongL3.SetSpawnProfile(3, [typeof(EvilMageLord)]);
            DungeonManager.RegisterZone(wrongL3);

            // ========================================================================
            // [Solen Hives] 솔렌 하이브
            // ========================================================================
            DungeonZone solenCentral = new(RegionCode.Trammel_Dungeon_SolenHives_CentralArea, map, 750000, typeof(RedSolenQueen), TimeSpan.FromHours(8));
            solenCentral.SetSpawnProfile(1, [typeof(Beetle), typeof(FireAnt)]);
            solenCentral.SetSpawnProfile(2, [typeof(BlackSolenWorker), typeof(RedSolenWorker)]);
            solenCentral.SetSpawnProfile(3, [typeof(BlackSolenWarrior), typeof(RedSolenWarrior), typeof(AntLion)]);
            DungeonManager.RegisterZone(solenCentral);

            DungeonZone solenBlack = new(RegionCode.Trammel_Dungeon_SolenHives_AreaALevel1, map, 600000, typeof(BlackSolenQueen), TimeSpan.FromHours(6));
            solenBlack.SetSpawnProfile(1, [typeof(BlackSolenWorker)]);
            solenBlack.SetSpawnProfile(2, [typeof(BlackSolenWarrior), typeof(BlackSolenInfiltratorWarrior)]);
            solenBlack.SetSpawnProfile(3, [typeof(BlackSolenInfiltratorQueen)]);
            DungeonManager.RegisterZone(solenBlack);

            DungeonZone solenRed = new(RegionCode.Trammel_Dungeon_SolenHives_AreaBLevel1, map, 600000, typeof(RedSolenQueen), TimeSpan.FromHours(6));
            solenRed.SetSpawnProfile(1, [typeof(RedSolenWorker)]);
            solenRed.SetSpawnProfile(2, [typeof(RedSolenWarrior), typeof(RedSolenInfiltratorWarrior)]);
            solenRed.SetSpawnProfile(3, [typeof(RedSolenInfiltratorQueen)]);
            DungeonManager.RegisterZone(solenRed);

            DungeonZone solenWorker = new(RegionCode.Trammel_Dungeon_SolenHives_AreaCLevel1, map, 225000, null, TimeSpan.FromHours(4));
            solenWorker.SetSpawnProfile(1, [typeof(FireAnt)]);
            solenWorker.SetSpawnProfile(2, [typeof(BlackSolenWorker)]);
            solenWorker.SetSpawnProfile(3, [typeof(RedSolenWorker)]);
            DungeonManager.RegisterZone(solenWorker);

            // ========================================================================
            // [Sewers] 트라멜 하수구
            // ========================================================================
            DungeonZone trammelSewer = new(RegionCode.Trammel_Dungeon_Miscellaneous, map, 150000, typeof(AcidElemental), TimeSpan.FromHours(2));
            trammelSewer.SetSpawnProfile(1, [typeof(Sewerrat)]);
            trammelSewer.SetSpawnProfile(2, [typeof(GiantRat), typeof(BullFrog)]);
            trammelSewer.SetSpawnProfile(3, [typeof(Alligator), typeof(GiantToad)]);
            DungeonManager.RegisterZone(trammelSewer);

            // ========================================================================
            // [Khaldun] 칼둔
            // ========================================================================
            DungeonZone khaldun = new(RegionCode.Felucca_Dungeon_Khaldun, map, 1500000, null, TimeSpan.FromHours(8));
            khaldun.SetSpawnProfile(1, [typeof(Cursed)]);
            khaldun.SetSpawnProfile(2, [typeof(KhaldunZealot)]);
            khaldun.SetSpawnProfile(3, [typeof(SpectralArmour), typeof(KhaldunRevenant), typeof(KhaldunSummoner)]);
            DungeonManager.RegisterZone(khaldun);

            // ========================================================================
            // [Terathan Keep] 테라탄 킵
            // ========================================================================
            DungeonZone terathanKeep = new(RegionCode.Trammel_Dungeon_TerathanKeep, map, 750000, null, TimeSpan.FromHours(6));
            terathanKeep.SetSpawnProfile(1, [typeof(GiantSpider)]);
            terathanKeep.SetSpawnProfile(2, [typeof(TerathanDrone), typeof(TerathanWarrior)]);
            terathanKeep.SetSpawnProfile(3, [typeof(TerathanMatriarch), typeof(TerathanAvenger)]);
            DungeonManager.RegisterZone(terathanKeep);

            // ========================================================================
            // [ML Dungeons] 확장팩 던전 시리즈
            // ========================================================================
            DungeonZone paintedCaves = new(RegionCode.Trammel_Dungeon_PaintedCaves, map, 375000, null, TimeSpan.FromHours(6));
            paintedCaves.SetSpawnProfile(1, [typeof(Troglodyte)]);
            paintedCaves.SetSpawnProfile(2, [typeof(Troglodyte)]);
            paintedCaves.SetSpawnProfile(3, [typeof(Troglodyte)]);
            DungeonManager.RegisterZone(paintedCaves);

            DungeonZone paroxysmus = new(RegionCode.Trammel_Dungeon_PalaceOfParoxysmus, map, 2250000, typeof(ChiefParoxysmus), TimeSpan.FromHours(12));
            paroxysmus.SetSpawnProfile(1, [typeof(PlagueBeast)]);
            paroxysmus.SetSpawnProfile(2, [typeof(PoisonElemental)]);
            paroxysmus.SetSpawnProfile(3, [typeof(PoisonElemental)]);
            DungeonManager.RegisterZone(paroxysmus);

            DungeonZone prismLight = new(RegionCode.Trammel_Dungeon_PrismOfLight, map, 600000, typeof(ShimmeringEffusion), TimeSpan.FromHours(10));
            prismLight.SetSpawnProfile(1, [typeof(Wisp)]);
            prismLight.SetSpawnProfile(2, [typeof(CrystalElemental)]);
            prismLight.SetSpawnProfile(3, [typeof(CrystalElemental)]);
            DungeonManager.RegisterZone(prismLight);

            DungeonZone sanctuary = new(RegionCode.Trammel_Dungeon_Sanctuary, map, 450000, typeof(Succubus), TimeSpan.FromHours(8));
            sanctuary.SetSpawnProfile(1, [typeof(Ratman)]);
            sanctuary.SetSpawnProfile(2, [typeof(Gargoyle)]);
            sanctuary.SetSpawnProfile(3, [typeof(Gargoyle)]);
            DungeonManager.RegisterZone(sanctuary);
        }
    }
}