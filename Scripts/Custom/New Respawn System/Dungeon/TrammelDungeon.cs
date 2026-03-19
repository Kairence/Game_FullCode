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
            string prefix = "Trammel Dungeons";

            // ========================================================================
            // [Blighted Grove] 블라이티드 그로브
            // 몹 평균 점수: 약 10점 -> 7,500마리 기준 약 75,000점
            // ========================================================================
            DungeonZone blightedGrove = new($"{prefix} Blighted Grove", map, 80000, null, TimeSpan.FromHours(8));
            blightedGrove.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Bogling), typeof(Corpser)]);
            blightedGrove.SetSpawnProfile(DungeonDepth.Deep, [typeof(BogThing), typeof(Wisp)]);
            DungeonManager.Zones[blightedGrove.ZoneId] = blightedGrove;

            // ========================================================================
            // [Covetous] 코버투스 (1~3층 및 특수 구역)
            // ========================================================================
            
            // 1층 (보스: GiantTurkey / 몹 평균 10~15점)
            // 7,500마리 사냥 목표: 110,000점
            DungeonZone covetousL1 = new($"{prefix} Covetous Level 1", map, 110000, typeof(GiantTurkey), TimeSpan.FromHours(4));
            covetousL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Eagle)]);
            covetousL1.SetSpawnProfile(DungeonDepth.Middle, [typeof(Eagle), typeof(Mongbat), typeof(Turkey)]);
            covetousL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(Turkey), typeof(Mongbat)]);
            DungeonManager.Zones[covetousL1.ZoneId] = covetousL1;

            // 2층 (보스: GiantDreadSpider / 몹 평균 60~100점)
            // 7,500마리 사냥 목표: 600,000점
            DungeonZone covetousL2 = new($"{prefix} Covetous Level 2", map, 600000, typeof(GiantDreadSpider), TimeSpan.FromHours(6));
            covetousL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(GiantSpider)]);
            covetousL2.SetSpawnProfile(DungeonDepth.Middle, [typeof(GiantSpider), typeof(GiantBlackWidow), typeof(TrapdoorSpider)]);
            covetousL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(TrapdoorSpider), typeof(WolfSpider), typeof(DreadSpider), typeof(GiantBlackWidow)]);
            DungeonManager.Zones[covetousL2.ZoneId] = covetousL2;

            // 3층 (보스: Lilith / 몹 평균 200~400점)
            // 7,500마리 사냥 목표: 2,500,000점
            DungeonZone covetousL3 = new($"{prefix} Covetous Level 3", map, 2500000, typeof(Lilith), TimeSpan.FromHours(8));
            covetousL3.SetSpawnProfile(DungeonDepth.Entrance, [typeof(VampireBat)]);
            covetousL3.SetSpawnProfile(DungeonDepth.Middle, [typeof(VampireBat), typeof(Harpy), typeof(StoneHarpy)]);
            covetousL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(StoneHarpy), typeof(Succubus), typeof(Harpy)]);
            DungeonManager.Zones[covetousL3.ZoneId] = covetousL3;

            // 호수 Cave (보스 없음, 순수 사냥터)
            DungeonZone covetousLake = new($"{prefix} Covetous Lake Cave", map, 1000000, null, TimeSpan.FromHours(6));
            covetousLake.SetSpawnProfile(DungeonDepth.Deep, [typeof(WaterElemental), typeof(SeaSerpent), typeof(Alligator)]);
            DungeonManager.Zones[covetousLake.ZoneId] = covetousLake;

            // 고문실 (보스 없음, 순수 사냥터)
            DungeonZone covetousTorture = new($"{prefix} Covetous Torture Chambers", map, 1500000, null, TimeSpan.FromHours(6));
            covetousTorture.SetSpawnProfile(DungeonDepth.Deep, [typeof(Skeleton), typeof(Zombie), typeof(Ghoul), typeof(BoneKnight), typeof(Executioner)]);
            DungeonManager.Zones[covetousTorture.ZoneId] = covetousTorture;

// ========================================================================
            // [1. Deceit] 디시트 (1~4층 / 보스 분리 / 7,500마리 사냥 기준)
            // ========================================================================
            
            // 1층 (보스: SkeletalMount / 몹 평균 점수 약 150점)
            // 7,500마리 사냥 목표: 1,125,000점
            DungeonZone deceitL1 = new($"{prefix} Deceit Level 1", map, 1125000, typeof(SkeletalMount), TimeSpan.FromHours(4));
            deceitL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BoneMagi)]);
            deceitL1.SetSpawnProfile(DungeonDepth.Middle, [typeof(BoneKnight), typeof(BoneMagi), typeof(Mummy), typeof(PestilentBandage)]);
            deceitL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(Mummy), typeof(PestilentBandage), typeof(RottingCorpse)]);
            DungeonManager.Zones[deceitL1.ZoneId] = deceitL1;

            // 2층 (보스: BoneDemon / 몹 평균 점수 약 250점)
            // 7,500마리 사냥 목표: 1,875,000점
            DungeonZone deceitL2 = new($"{prefix} Deceit Level 2", map, 1875000, typeof(BoneDemon), TimeSpan.FromHours(6));
            deceitL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(SkeletalCatStatue)]);
            deceitL2.SetSpawnProfile(DungeonDepth.Middle, [typeof(Ghoul), typeof(PatchworkSkeleton), typeof(Shade), typeof(SkeletalCatStatue)]);
            deceitL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(SkeletalKnight), typeof(SkeletalMage), typeof(Wraith), typeof(Shade)]);
            DungeonManager.Zones[deceitL2.ZoneId] = deceitL2;

            // 3층 (보스: AncientLich / 몹 평균 점수 약 420점)
            // 7,500마리 사냥 목표: 3,150,000점
            DungeonZone deceitL3 = new($"{prefix} Deceit Level 3", map, 3150000, typeof(AncientLich), TimeSpan.FromHours(8));
            deceitL3.SetSpawnProfile(DungeonDepth.Entrance, [typeof(SkeletalLich)]);
            deceitL3.SetSpawnProfile(DungeonDepth.Middle, [typeof(Lich), typeof(SkeletalLich)]);
            deceitL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(Lich)]);
            DungeonManager.Zones[deceitL3.ZoneId] = deceitL3;

            // 4층 (보스: SkeletalDragon / 몹 평균 점수 약 540점)
            // 7,500마리 사냥 목표: 4,050,000점
            DungeonZone deceitL4 = new($"{prefix} Deceit Level 4", map, 4050000, typeof(SkeletalDragon), TimeSpan.FromHours(10));
            deceitL4.SetSpawnProfile(DungeonDepth.Deep, [typeof(LichLord)]); // 리치로드가 유일한 일반몹
            DungeonManager.Zones[deceitL4.ZoneId] = deceitL4;


            // ========================================================================
            // [2. Despise] 데스파이즈 (1~3층 / 보스 분리 / 7,500마리 사냥 기준)
            // ========================================================================
            
            // 1층 (보스: Reaper / 몹 평균 점수 약 10~15점)
            // 7,500마리 사냥 목표: 90,000점
            DungeonZone despiseL1 = new($"{prefix} Despise Level 1", map, 90000, typeof(Reaper), TimeSpan.FromHours(4));
            despiseL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Llama), typeof(Crane)]);
            despiseL1.SetSpawnProfile(DungeonDepth.Middle, [typeof(Palomino), typeof(RidableLlama), typeof(Bogling), typeof(ForestOstard)]);
            despiseL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(ForestOstard), typeof(Corpser), typeof(Treefellow)]);
            DungeonManager.Zones[despiseL1.ZoneId] = despiseL1;

            // 2층 (보스: BloodWorm / 몹 평균 점수 약 60~80점)
            // 7,500마리 사냥 목표: 500,000점
            DungeonZone despiseL2 = new($"{prefix} Despise Level 2", map, 500000, typeof(BloodWorm), TimeSpan.FromHours(6));
            despiseL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(HeadlessOne), typeof(Lizardman)]);
            despiseL2.SetSpawnProfile(DungeonDepth.Middle, [typeof(Lizardman), typeof(LizardmanDefender), typeof(Ettin), typeof(Troll)]);
            despiseL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(Troll), typeof(Cyclops), typeof(LizardmanDefender)]);
            DungeonManager.Zones[despiseL2.ZoneId] = despiseL2;

            // 3층 (보스: OgreLord / 몹 평균 점수 약 200점)
            // 7,500마리 사냥 목표: 1,500,000점
            DungeonZone despiseL3 = new($"{prefix} Despise Level 3", map, 1500000, typeof(OgreLord), TimeSpan.FromHours(8));
            despiseL3.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Centaur)]);
            despiseL3.SetSpawnProfile(DungeonDepth.Middle, [typeof(Centaur), typeof(Ogre)]);
            despiseL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(Ogre), typeof(Centaur)]);
            DungeonManager.Zones[despiseL3.ZoneId] = despiseL3;

// ========================================================================
            // [1. Destard] 데스타드 (드래곤 생태계 / 7,500마리 사냥 기준)
            // ========================================================================
            
            // 1층 (보스 없음 / 몹 평균 점수 약 250~350점)
            // 7,500마리 사냥 목표: 2,500,000점
            DungeonZone destardL1 = new($"{prefix} Destard Level 1", map, 2500000, null, TimeSpan.FromHours(6));
            destardL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(LavaSnake), typeof(GiantSerpent)]);
            destardL1.SetSpawnProfile(DungeonDepth.Middle, [typeof(DragonWolf), typeof(Drake), typeof(Wyvern)]);
            destardL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(Wyvern), typeof(Drake), typeof(Dragon)]);
            DungeonManager.Zones[destardL1.ZoneId] = destardL1;

            // 2층 (보스 없음 / 몹 평균 점수 약 600~800점)
            // 7,500마리 사냥 목표: 5,500,000점
            DungeonZone destardL2 = new($"{prefix} Destard Level 2", map, 5500000, null, TimeSpan.FromHours(8));
            destardL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Drake)]);
            destardL2.SetSpawnProfile(DungeonDepth.Middle, [typeof(Dragon), typeof(TsukiWolf), typeof(ShadowWyrm)]);
            destardL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(ShadowWyrm), typeof(RidableDragon)]);
            DungeonManager.Zones[destardL2.ZoneId] = destardL2;

            // 3층 (보스: AncientWyrm / GreaterDragon 등 마리당 800~1,000점)
            // 7,500마리 사냥 목표: 7,500,000점
            DungeonZone destardL3 = new($"{prefix} Destard Level 3", map, 7500000, typeof(AncientWyrm), TimeSpan.FromHours(12));
            destardL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(GreaterDragon), typeof(ShadowWyrm), typeof(Dragon)]);
            DungeonManager.Zones[destardL3.ZoneId] = destardL3;


            // ========================================================================
            // [2. Hythloth] 히스로스 (악마 군단 / 7,500마리 사냥 기준)
            // ========================================================================
            
            // 1층 (보스 없음 / 몹 평균 점수 약 20~40점)
            DungeonZone hythlothL1 = new($"{prefix} Hythloth Level 1", map, 250000, null, TimeSpan.FromHours(4));
            hythlothL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Imp), typeof(Gargoyle)]);
            hythlothL1.SetSpawnProfile(DungeonDepth.Middle, [typeof(HellCat), typeof(HellHound), typeof(Imp)]);
            hythlothL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(HellHound), typeof(PredatorHellCat)]);
            DungeonManager.Zones[hythlothL1.ZoneId] = hythlothL1;

            // 2층 (보스 없음 / 몹 평균 점수 약 400점)
            DungeonZone hythlothL2 = new($"{prefix} Hythloth Level 2", map, 3000000, null, TimeSpan.FromHours(6));
            hythlothL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(StoneGargoyle)]);
            hythlothL2.SetSpawnProfile(DungeonDepth.Middle, [typeof(StoneGargoyle), typeof(Daemon)]);
            hythlothL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(Daemon)]);
            DungeonManager.Zones[hythlothL2.ZoneId] = hythlothL2;

            // 3층 (보스 없음 / 몹 평균 점수 약 600점)
            DungeonZone hythlothL3 = new($"{prefix} Hythloth Level 3", map, 4500000, null, TimeSpan.FromHours(8));
            hythlothL3.SetSpawnProfile(DungeonDepth.Middle, [typeof(Daemon), typeof(HellSteed), typeof(ArchDaemon)]);
            hythlothL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(HellSteed), typeof(ArchDaemon)]);
            DungeonManager.Zones[hythlothL3.ZoneId] = hythlothL3;

            // 4층 (보스: Balron / ArchDaemon 등 마리당 약 700점)
            DungeonZone hythlothL4 = new($"{prefix} Hythloth Level 4", map, 5500000, typeof(Balron), TimeSpan.FromHours(12));
            hythlothL4.SetSpawnProfile(DungeonDepth.Deep, [typeof(ArchDaemon)]);
            DungeonManager.Zones[hythlothL4.ZoneId] = hythlothL4;


            // ========================================================================
            // [3. Shame] 쉐임 (정령 & 게이저 / 7,500마리 사냥 기준)
            // ========================================================================
            
            // 1층 (보스: EttinLord / 몹 평균 점수 약 80~100점)
            DungeonZone shameL1 = new($"{prefix} Shame Level 1", map, 700000, typeof(EttinLord), TimeSpan.FromHours(4));
            shameL1.SetSpawnProfile(DungeonDepth.Middle, [typeof(EarthElemental), typeof(Scorpion), typeof(ClockworkScorpion)]);
            shameL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(EarthElemental)]);
            DungeonManager.Zones[shameL1.ZoneId] = shameL1;

            // 2층 (보스 없음 / 몹 평균 점수 약 120점)
            DungeonZone shameL2 = new($"{prefix} Shame Level 2", map, 900000, null, TimeSpan.FromHours(6));
            shameL2.SetSpawnProfile(DungeonDepth.Middle, [typeof(AirElemental), typeof(FireElemental), typeof(WaterElemental)]);
            shameL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(FireElemental), typeof(WaterElemental)]);
            DungeonManager.Zones[shameL2.ZoneId] = shameL2;

            // 3층 (보스: Beholder / 몹 평균 점수 약 350점)
            DungeonZone shameL3 = new($"{prefix} Shame Level 3", map, 2600000, typeof(Beholder), TimeSpan.FromHours(8));
            shameL3.SetSpawnProfile(DungeonDepth.Middle, [typeof(Gazer), typeof(ElderGazer)]);
            shameL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(ElderGazer)]);
            DungeonManager.Zones[shameL3.ZoneId] = shameL3;

            // 4층 (보스 없음 / 몹 평균 점수 약 800점)
            DungeonZone shameL4 = new($"{prefix} Shame Level 4", map, 6000000, null, TimeSpan.FromHours(10));
            shameL4.SetSpawnProfile(DungeonDepth.Deep, [typeof(PoisonElemental), typeof(BloodElemental), typeof(EnragedColossus)]);
            DungeonManager.Zones[shameL4.ZoneId] = shameL4;
			
			// ========================================================================
            // [1. Fire Dungeon] 파이어 던전 (7,500마리 사냥 기준)
            // ========================================================================
            
            // 1층 (보스: UndeadGargoyle / 몹 평균 점수 약 100점)
            // 7,500마리 사냥 목표: 750,000점
            DungeonZone fireL1 = new($"{prefix} Fire Level 1", map, 750000, typeof(UndeadGargoyle), TimeSpan.FromHours(6));
            fireL1.SetSpawnProfile(DungeonDepth.Middle, [
                typeof(EnslavedGrayGoblin), typeof(EnslavedGreenGoblin), typeof(GrayGoblin), 
                typeof(GreenGoblin), typeof(EnslavedGargoyle), typeof(Gargoyle), typeof(LavaLizard)
            ]);
            fireL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(FireGargoyle), typeof(LavaSerpent), typeof(Gargoyle)]);
            DungeonManager.Zones[fireL1.ZoneId] = fireL1;

            // 2층 (보스: RedWyrm / 몹 평균 점수 약 400~500점)
            // 7,500마리 사냥 목표: 3,500,000점
            DungeonZone fireL2 = new($"{prefix} Fire Level 2", map, 3500000, typeof(RedWyrm), TimeSpan.FromHours(8));
            fireL2.SetSpawnProfile(DungeonDepth.Middle, [
                typeof(FireBeetle), typeof(LavaElemental), typeof(FireDrake), 
                typeof(Efreet), typeof(GargoyleEnforcer), typeof(GargoyleDestroyer)
            ]);
            fireL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(FireSteed), typeof(FireDaemon), typeof(FireDrake)]);
            DungeonManager.Zones[fireL2.ZoneId] = fireL2;


            // ========================================================================
            // [2. Ice Dungeon] 아이스 던전 (7,500마리 사냥 기준)
            // ========================================================================
            
            // 1층 (보스 없음 / 몹 평균 점수 약 100점)
            DungeonZone iceL1 = new($"{prefix} Ice Level 1", map, 750000, null, TimeSpan.FromHours(6));
            iceL1.SetSpawnProfile(DungeonDepth.Middle, [
                typeof(FrostOoze), typeof(FrostSpider), typeof(IceHound), 
                typeof(FrostMite), typeof(GiantIceWorm), typeof(IceElemental)
            ]);
            iceL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(GiantIceWorm), typeof(IceElemental), typeof(SnowElemental)]);
            DungeonManager.Zones[iceL1.ZoneId] = iceL1;

            // 쥐인간 방 (보스: RatmanMage / 몹 평균 점수 약 30점)
            // 7,500마리 사냥 목표: 225,000점
            DungeonZone iceRatman = new($"{prefix} Ice Ratman Room", map, 225000, typeof(RatmanMage), TimeSpan.FromHours(4));
            iceRatman.SetSpawnProfile(DungeonDepth.Middle, [typeof(Ratman), typeof(RatmanArcher)]);
            iceRatman.SetSpawnProfile(DungeonDepth.Deep, [typeof(RatmanArcher)]);
            DungeonManager.Zones[iceRatman.ZoneId] = iceRatman;

            // 아이스 데몬 은신처 (보스: WhiteWyrm / 몹 평균 점수 약 450점)
            // 7,500마리 사냥 목표: 3,375,000점
            DungeonZone iceDemon = new($"{prefix} Ice Ice Demon Lair", map, 3375000, typeof(WhiteWyrm), TimeSpan.FromHours(8));
            iceDemon.SetSpawnProfile(DungeonDepth.Middle, [typeof(SnowElemental), typeof(ArcticOgreLord), typeof(ColdDrake), typeof(IceFiend)]);
            iceDemon.SetSpawnProfile(DungeonDepth.Deep, [typeof(IceFiend), typeof(ArcticOgreLord)]);
            DungeonManager.Zones[iceDemon.ZoneId] = iceDemon;


            // ========================================================================
            // [3. Orc Cave] 오크 동굴 (7,500마리 사냥 기준)
            // ========================================================================
            
            // 1층 (보스: BogThing / 몹 평균 점수 약 40점)
            DungeonZone orcL1 = new($"{prefix} Orc Cave Level 1", map, 300000, typeof(BogThing), TimeSpan.FromHours(4));
            orcL1.SetSpawnProfile(DungeonDepth.Middle, [typeof(OrcChopper), typeof(OrcBomber), typeof(OrcCaptain), typeof(OrcishMage)]);
            orcL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(OrcCaptain), typeof(OrcishMage)]);
            DungeonManager.Zones[orcL1.ZoneId] = orcL1;

            // 2층 (보스 없음 / 몹 평균 점수 약 90점)
            DungeonZone orcL2 = new($"{prefix} Orc Cave Level 2", map, 675000, null, TimeSpan.FromHours(6));
            orcL2.SetSpawnProfile(DungeonDepth.Middle, [typeof(OrcishLord)]);
            orcL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(OrcishLord)]);
            DungeonManager.Zones[orcL2.ZoneId] = orcL2;

            // 3층 (보스: OrcBrute / 몹 평균 점수 약 400점)
            // 7,500마리 사냥 목표: 3,000,000점
            DungeonZone orcL3 = new($"{prefix} Orc Cave Level 3", map, 3000000, typeof(OrcBrute), TimeSpan.FromHours(8));
            orcL3.SetSpawnProfile(DungeonDepth.Middle, [typeof(Titan)]);
            orcL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(Titan)]);
            DungeonManager.Zones[orcL3.ZoneId] = orcL3;
			
			// ========================================================================
            // [1. Wrong] 롱 던전 (7,500마리 사냥 기준)
            // ========================================================================
            
            // 1층 (보스 없음 / 몹 평균 점수 약 150점)
            // 7,500마리 사냥 목표: 1,125,000점
            DungeonZone wrongL1 = new($"{prefix} Wrong Level 1", map, 1125000, null, TimeSpan.FromHours(4));
            wrongL1.SetSpawnProfile(DungeonDepth.Middle, [typeof(Brigand), typeof(Golem), typeof(EvilMage)]);
            wrongL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(ShadowDragon), typeof(ChaosDragoon)]);
            DungeonManager.Zones[wrongL1.ZoneId] = wrongL1;

            // 2층 (보스: GolemLord / 몹 평균 점수 약 250점)
            // 7,500마리 사냥 목표: 1,875,000점
            DungeonZone wrongL2 = new($"{prefix} Wrong Level 2", map, 1875000, typeof(GolemLord), TimeSpan.FromHours(6));
            wrongL2.SetSpawnProfile(DungeonDepth.Middle, [typeof(GolemController), typeof(Executioner)]);
            wrongL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(Executioner), typeof(GolemController)]);
            DungeonManager.Zones[wrongL2.ZoneId] = wrongL2;

            // 3층 (보스: JukaLord / 몹 평균 점수 약 450점)
            // 7,500마리 사냥 목표: 3,375,000점
            DungeonZone wrongL3 = new($"{prefix} Wrong Level 3", map, 3375000, typeof(JukaLord), TimeSpan.FromHours(8));
            wrongL3.SetSpawnProfile(DungeonDepth.Middle, [typeof(JukaWarrior), typeof(JukaMage), typeof(ChaosDragoonElite)]);
            wrongL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(EvilMageLord), typeof(ChaosDragoonElite)]);
            DungeonManager.Zones[wrongL3.ZoneId] = wrongL3;


            // ========================================================================
            // [2. Solen Hives] 솔렌 하이브 (7,500마리 사냥 기준)
            // ========================================================================
            
            // 솔렌 중앙 구역 (보스: RedSolenQueen / 몹 평균 점수 약 100점)
            // 7,500마리 사냥 목표: 750,000점
            DungeonZone solenCentral = new($"{prefix} Solen Hives Central Area", map, 750000, typeof(RedSolenQueen), TimeSpan.FromHours(8));
            solenCentral.SetSpawnProfile(DungeonDepth.Middle, [typeof(BlackSolenWorker), typeof(RedSolenWorker), typeof(FireAnt), typeof(Beetle)]);
            solenCentral.SetSpawnProfile(DungeonDepth.Deep, [typeof(AntLion), typeof(BlackSolenWarrior), typeof(RedSolenWarrior)]);
            DungeonManager.Zones[solenCentral.ZoneId] = solenCentral;

            // Area A & C (Black Solen 테마 / 보스: BlackSolenQueen / 몹 평균 80점)
            DungeonZone solenBlack = new($"{prefix} Solen Hives Black Area", map, 600000, typeof(BlackSolenQueen), TimeSpan.FromHours(6));
            solenBlack.SetSpawnProfile(DungeonDepth.Middle, [typeof(BlackSolenWorker), typeof(BlackSolenWarrior)]);
            solenBlack.SetSpawnProfile(DungeonDepth.Deep, [typeof(BlackSolenInfiltratorWarrior), typeof(BlackSolenInfiltratorQueen)]);
            DungeonManager.Zones[solenBlack.ZoneId] = solenBlack;

            // Area B & D (Red Solen 테마 / 보스: RedSolenQueen / 몹 평균 80점)
            DungeonZone solenRed = new($"{prefix} Solen Hives Red Area", map, 600000, typeof(RedSolenQueen), TimeSpan.FromHours(6));
            solenRed.SetSpawnProfile(DungeonDepth.Middle, [typeof(RedSolenWorker), typeof(RedSolenWarrior)]);
            solenRed.SetSpawnProfile(DungeonDepth.Deep, [typeof(RedSolenInfiltratorWarrior), typeof(RedSolenInfiltratorQueen)]);
            DungeonManager.Zones[solenRed.ZoneId] = solenRed;

            // Area E (노동자/개미 특화 구역 / 보스 없음 / 몹 평균 30점)
            DungeonZone solenWorker = new($"{prefix} Solen Hives Worker Area", map, 225000, null, TimeSpan.FromHours(4));
            solenWorker.SetSpawnProfile(DungeonDepth.Middle, [typeof(BlackSolenWorker), typeof(RedSolenWorker), typeof(FireAnt)]);
            solenWorker.SetSpawnProfile(DungeonDepth.Deep, [typeof(FireAnt)]);
            DungeonManager.Zones[solenWorker.ZoneId] = solenWorker;


            // ========================================================================
            // [3. Sewer] 트라멜 하수구 (7,500마리 사냥 기준)
            // ========================================================================
            
            // 트라멜 하수구 (보스: AcidElemental / 몹 평균 점수 약 20점)
            // 7,500마리 사냥 목표: 150,000점
            DungeonZone trammelSewer = new($"{prefix} Sewers", map, 150000, typeof(AcidElemental), TimeSpan.FromHours(2));
            trammelSewer.SetSpawnProfile(DungeonDepth.Middle, [typeof(Sewerrat), typeof(GiantRat), typeof(BullFrog), typeof(Alligator)]);
            trammelSewer.SetSpawnProfile(DungeonDepth.Deep, [typeof(GiantToad), typeof(Alligator)]);
            DungeonManager.Zones[trammelSewer.ZoneId] = trammelSewer;
			
			// ========================================================================
            // [1. Khaldun] 칼둔 (7,500마리 사냥 기준)
            // ========================================================================
            
            // 칼둔 심층 (보스: KhaldunSummoner / 몹 평균 점수 약 200점)
            // 7,500마리 사냥 목표: 1,500,000점
            DungeonZone khaldun = new($"{prefix} Khaldun", map, 1500000, typeof(KhaldunSummoner), TimeSpan.FromHours(8));
            khaldun.SetSpawnProfile(DungeonDepth.Middle, [
                typeof(Cursed), typeof(KhaldunZealot), typeof(SpectralArmour)
            ]);
            khaldun.SetSpawnProfile(DungeonDepth.Deep, [
                typeof(SpectralArmour), typeof(KhaldunRevenant)
            ]);
            DungeonManager.Zones[khaldun.ZoneId] = khaldun;


            // ========================================================================
            // [2. ML Dungeons] 확장팩 던전 시리즈 (7,500마리 사냥 기준)
            // ========================================================================
            
            // Painted Caves (페인티드 케이브 / 보스: Troglodyte(임시) / 몹 평균 점수 약 50점)
            // 7,500마리 사냥 목표: 375,000점
            DungeonZone paintedCaves = new($"{prefix} Painted Caves", map, 375000, typeof(Troglodyte), TimeSpan.FromHours(6));
            paintedCaves.SetSpawnProfile(DungeonDepth.Middle, [typeof(Troglodyte)]);
            paintedCaves.SetSpawnProfile(DungeonDepth.Deep, [typeof(Troglodyte)]);
            DungeonManager.Zones[paintedCaves.ZoneId] = paintedCaves;

            // Palace of Paroxysmus (파록시무스의 궁전 / 보스: ChiefParoxysmus / 몹 평균 점수 약 250~300점)
            // 7,500마리 사냥 목표: 2,250,000점
            DungeonZone paroxysmus = new($"{prefix} Palace of Paroxysmus", map, 2250000, typeof(ChiefParoxysmus), TimeSpan.FromHours(12));
            paroxysmus.SetSpawnProfile(DungeonDepth.Middle, [typeof(PlagueBeast), typeof(BogThing)]);
            paroxysmus.SetSpawnProfile(DungeonDepth.Deep, [typeof(BogThing), typeof(PoisonElemental)]);
            DungeonManager.Zones[paroxysmus.ZoneId] = paroxysmus;

            // Prism of Light (프리즘 오브 라이트 / 보스: ShimmeringEffusion / 몹 평균 점수 약 80점)
            // 7,500마리 사냥 목표: 600,000점
            DungeonZone prismLight = new($"{prefix} Prism of Light", map, 600000, typeof(ShimmeringEffusion), TimeSpan.FromHours(10));
            prismLight.SetSpawnProfile(DungeonDepth.Middle, [typeof(Wisp), typeof(CrystalElemental)]);
            prismLight.SetSpawnProfile(DungeonDepth.Deep, [typeof(CrystalElemental)]);
            DungeonManager.Zones[prismLight.ZoneId] = prismLight;

            // Sanctuary (생츄어리 / 보스: Succubus / 몹 평균 점수 약 60점)
            // 7,500마리 사냥 목표: 450,000점
            DungeonZone sanctuary = new($"{prefix} Sanctuary", map, 450000, typeof(Succubus), TimeSpan.FromHours(8));
            sanctuary.SetSpawnProfile(DungeonDepth.Middle, [typeof(Ratman), typeof(Gargoyle)]);
            sanctuary.SetSpawnProfile(DungeonDepth.Deep, [typeof(Gargoyle)]);
            DungeonManager.Zones[sanctuary.ZoneId] = sanctuary;
        }
    }
}