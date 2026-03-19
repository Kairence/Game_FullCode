using System;
using Server;
using Server.Mobiles;

namespace Server.Misc
{
    public static class IlshenarDungeon
    {
        public static void Setup()
        {
            Map map = Map.Ilshenar;
            string prefixDungeons = "Ilshenar Dungeons";
            string prefixCaves = "Ilshenar Caves";
            string customPrefix = "Ilshenar Hidden Dungeons"; // XML에 없지만 추가된 ML/비밀 던전

			/*
            // ========================================================================
            // [Ankh] 앙크 던전 (신성/타락 테마)
            // ========================================================================
            DungeonZone ankhEnt = new($"{prefixDungeons} Ankh Entrance", map, 20000, null, TimeSpan.FromHours(2));
            ankhEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Skeleton), typeof(Zombie)]);
            DungeonManager.Zones[ankhEnt.ZoneId] = ankhEnt;

            DungeonZone ankhL1 = new($"{prefixDungeons} Ankh Level 1", map, 60000, typeof(BloodElemental), TimeSpan.FromHours(6));
            ankhL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BoneKnight), typeof(Wraith)]);
            ankhL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(BloodElemental), typeof(PoisonElemental)]);
            DungeonManager.Zones[ankhL1.ZoneId] = ankhL1;

            DungeonZone ankhKirin = new($"{prefixDungeons} Ankh Kirin passage", map, 50000, typeof(Kirin), TimeSpan.FromHours(6));
            ankhKirin.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Wisp), typeof(Pixie)]);
            ankhKirin.SetSpawnProfile(DungeonDepth.Deep, [typeof(Kirin)]);
            DungeonManager.Zones[ankhKirin.ZoneId] = ankhKirin;

            DungeonZone ankhSerpent = new($"{prefixDungeons} Ankh Serpentine Passage", map, 70000, typeof(SilverSerpent), TimeSpan.FromHours(6));
            ankhSerpent.SetSpawnProfile(DungeonDepth.Entrance, [typeof(GiantSerpent), typeof(IceSnake)]);
            ankhSerpent.SetSpawnProfile(DungeonDepth.Deep, [typeof(SilverSerpent), typeof(LavaSerpent)]);
            DungeonManager.Zones[ankhSerpent.ZoneId] = ankhSerpent;

            // ========================================================================
            // [Blood] 블러드 던전 (악마/혈액 테마)
            // ========================================================================
            DungeonZone bloodEnt = new($"{prefixDungeons} Blood Entrance", map, 30000, null, TimeSpan.FromHours(2));
            bloodEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(HellHound)]);
            DungeonManager.Zones[bloodEnt.ZoneId] = bloodEnt;

            DungeonZone bloodL1 = new($"{prefixDungeons} Blood Level 1", map, 130000, typeof(Balron), TimeSpan.FromHours(10));
            bloodL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Succubus), typeof(Demon)]);
            bloodL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(BloodElemental), typeof(Balron)]);
            DungeonManager.Zones[bloodL1.ZoneId] = bloodL1;

            // ========================================================================
            // [Exodus] 엑소더스 (기계/가고일 테마)
            // ========================================================================
            DungeonZone exodusEnt = new($"{prefixDungeons} Exodus Entrance", map, 40000, null, TimeSpan.FromHours(4));
            exodusEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Gargoyle), typeof(ExodusOverseer)]);
            DungeonManager.Zones[exodusEnt.ZoneId] = exodusEnt;

            DungeonZone exodusL1 = new($"{prefixDungeons} Exodus Level 1", map, 150000, typeof(ExodusMinion), TimeSpan.FromHours(12));
            exodusL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Golem), typeof(ExodusOverseer)]);
            exodusL1.SetSpawnProfile(DungeonDepth.Middle, [typeof(Juggernaut)]);
            exodusL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(ExodusMinion)]);
            DungeonManager.Zones[exodusL1.ZoneId] = exodusL1;

            // ========================================================================
            // [Rock] 락 던전 (가고일/바위 테마)
            // ========================================================================
            DungeonZone rockEnt = new($"{prefixDungeons} Rock Entrance", map, 30000, null, TimeSpan.FromHours(2));
            rockEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(StoneGargoyle)]);
            DungeonManager.Zones[rockEnt.ZoneId] = rockEnt;

            DungeonZone rockL1 = new($"{prefixDungeons} Rock Level 1", map, 60000, typeof(EarthElemental), TimeSpan.FromHours(6));
            rockL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(StoneGargoyle)]);
            rockL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(EarthElemental)]);
            DungeonManager.Zones[rockL1.ZoneId] = rockL1;

            DungeonZone rockL2 = new($"{prefixDungeons} Rock Level 2", map, 80000, typeof(SystemDungeonBoss), TimeSpan.FromHours(8));
            rockL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(EarthElemental)]);
            rockL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(Titan), typeof(Cyclops)]);
            DungeonManager.Zones[rockL2.ZoneId] = rockL2;

            // ========================================================================
            // [Sorcerers] 소서러 던전 (마법사/언데드 테마)
            // ========================================================================
            DungeonZone sorcEnt = new($"{prefixDungeons} Sorcerers Entrance", map, 30000, null, TimeSpan.FromHours(2));
            sorcEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(EvilMage)]);
            DungeonManager.Zones[sorcEnt.ZoneId] = sorcEnt;

            DungeonZone sorcL1 = new($"{prefixDungeons} Sorcerers Level 1", map, 40000, typeof(EvilMageLord), TimeSpan.FromHours(4));
            sorcL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(EvilMage)]);
            sorcL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(EvilMageLord)]);
            DungeonManager.Zones[sorcL1.ZoneId] = sorcL1;

            DungeonZone sorcL2 = new($"{prefixDungeons} Sorcerers Level 2", map, 60000, typeof(SkeletalMage), TimeSpan.FromHours(6));
            sorcL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BoneKnight)]);
            sorcL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(SkeletalMage), typeof(Wraith)]);
            DungeonManager.Zones[sorcL2.ZoneId] = sorcL2;

            DungeonZone sorcL3 = new($"{prefixDungeons} Sorcerers Level 3", map, 80000, typeof(Lich), TimeSpan.FromHours(8));
            sorcL3.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Wraith), typeof(Spectre)]);
            sorcL3.SetSpawnProfile(DungeonDepth.Deep, [typeof(Lich)]);
            DungeonManager.Zones[sorcL3.ZoneId] = sorcL3;

            DungeonZone sorcL4 = new($"{prefixDungeons} Sorcerers Level 4", map, 100000, typeof(LichLord), TimeSpan.FromHours(10));
            sorcL4.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Lich)]);
            sorcL4.SetSpawnProfile(DungeonDepth.Deep, [typeof(LichLord)]);
            DungeonManager.Zones[sorcL4.ZoneId] = sorcL4;

            DungeonZone sorcL5 = new($"{prefixDungeons} Sorcerers Level 5", map, 130000, typeof(SystemDungeonBoss), TimeSpan.FromHours(12));
            sorcL5.SetSpawnProfile(DungeonDepth.Entrance, [typeof(LichLord), typeof(PoisonElemental)]);
            sorcL5.SetSpawnProfile(DungeonDepth.Deep, [typeof(BloodElemental)]);
            DungeonManager.Zones[sorcL5.ZoneId] = sorcL5;

            // ========================================================================
            // [Spectre] 스펙터 던전 (유령 테마)
            // ========================================================================
            DungeonZone specEnt = new($"{prefixDungeons} Spectre Entrance", map, 30000, null, TimeSpan.FromHours(2));
            specEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Spectre)]);
            DungeonManager.Zones[specEnt.ZoneId] = specEnt;

            DungeonZone specL1 = new($"{prefixDungeons} Spectre Level 1", map, 70000, typeof(Wraith), TimeSpan.FromHours(6));
            specL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Spectre), typeof(Shade)]);
            specL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(Wraith)]);
            DungeonManager.Zones[specL1.ZoneId] = specL1;

            // ========================================================================
            // [Wisp] 위스프 던전 (빛/미궁 테마)
            // ========================================================================
            DungeonZone wispEnt = new($"{prefixDungeons} Wisp Entrance", map, 20000, null, TimeSpan.FromHours(2));
            wispEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Wisp)]);
            DungeonManager.Zones[wispEnt.ZoneId] = wispEnt;

            // Level 1 ~ 8: 점진적으로 강해지는 심층 구조
            Type[] wispTypes = [typeof(Wisp), typeof(Pixie)];
            Type[] deepWispTypes = [typeof(Wisp), typeof(DarkWisp), typeof(AirElemental)];

            for (int i = 1; i <= 8; i++)
            {
                DungeonZone wispLevel = new($"{prefixDungeons} Wisp Level {i}", map, 30000 + (i * 10000), i == 8 ? typeof(SystemDungeonBoss) : null, TimeSpan.FromHours(2 + i));
                wispLevel.SetSpawnProfile(DungeonDepth.Entrance, wispTypes);
                wispLevel.SetSpawnProfile(DungeonDepth.Deep, i >= 4 ? deepWispTypes : wispTypes);
                DungeonManager.Zones[wispLevel.ZoneId] = wispLevel;
            }

            // ========================================================================
            // [Caves] 기타 동굴 구역
            // ========================================================================
            
            // Ancient Lair
            DungeonZone ancientLairEnt = new($"{prefixCaves} Ancient Lair Entrance", map, 30000, null, TimeSpan.FromHours(2));
            ancientLairEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Drake)]);
            DungeonManager.Zones[ancientLairEnt.ZoneId] = ancientLairEnt;

            DungeonZone ancientLairL1 = new($"{prefixCaves} Ancient Lair Level 1", map, 100000, typeof(AncientWyrm), TimeSpan.FromHours(10));
            ancientLairL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Dragon), typeof(Wyvern)]);
            ancientLairL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(GreaterDragon)]);
            DungeonManager.Zones[ancientLairL1.ZoneId] = ancientLairL1;

            // Lizard Passage
            DungeonZone lizardEnt = new($"{prefixCaves} Lizard Passage Entrance", map, 20000, null, TimeSpan.FromHours(2));
            lizardEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Lizardman)]);
            DungeonManager.Zones[lizardEnt.ZoneId] = lizardEnt;

            DungeonZone lizardL1 = new($"{prefixCaves} Lizard Passage Level 1", map, 40000, typeof(Lizardman), TimeSpan.FromHours(4));
            lizardL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Lizardman)]);
            lizardL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(LavaLizard)]);
            DungeonManager.Zones[lizardL1.ZoneId] = lizardL1;

            DungeonZone lizardL2 = new($"{prefixCaves} Lizard Passage Level 2", map, 60000, typeof(Dragon), TimeSpan.FromHours(6));
            lizardL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(LavaLizard)]);
            lizardL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(Drake), typeof(Wyvern)]);
            DungeonManager.Zones[lizardL2.ZoneId] = lizardL2;

            // Mushroom Cave
            DungeonZone mushroomEnt = new($"{prefixCaves} Mushroom Cave Entrance", map, 20000, null, TimeSpan.FromHours(2));
            mushroomEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Corpser), typeof(Bogling)]);
            DungeonManager.Zones[mushroomEnt.ZoneId] = mushroomEnt;

            // Rat Cave
            DungeonZone ratEnt = new($"{prefixCaves} Rat Cave Entrance", map, 20000, null, TimeSpan.FromHours(2));
            ratEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(GiantRat)]);
            DungeonManager.Zones[ratEnt.ZoneId] = ratEnt;

            DungeonZone ratL1 = new($"{prefixCaves} Rat Cave Level 1", map, 40000, typeof(Ratman), TimeSpan.FromHours(4));
            ratL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(GiantRat), typeof(Ratman)]);
            ratL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(RatmanArcher)]);
            DungeonManager.Zones[ratL1.ZoneId] = ratL1;

            DungeonZone ratL2 = new($"{prefixCaves} Rat Cave Level 2", map, 60000, typeof(RatmanMage), TimeSpan.FromHours(6));
            ratL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(RatmanArcher)]);
            ratL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(RatmanMage)]);
            DungeonManager.Zones[ratL2.ZoneId] = ratL2;

            // Spider Cave
            DungeonZone spiderEnt = new($"{prefixCaves} Spider Cave Entrance", map, 30000, null, TimeSpan.FromHours(2));
            spiderEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(GiantSpider)]);
            DungeonManager.Zones[spiderEnt.ZoneId] = spiderEnt;

            DungeonZone spiderL1 = new($"{prefixCaves} Spider Cave Level 1", map, 50000, typeof(DreadSpider), TimeSpan.FromHours(5));
            spiderL1.SetSpawnProfile(DungeonDepth.Entrance, [typeof(GiantSpider), typeof(TerathanDrone)]);
            spiderL1.SetSpawnProfile(DungeonDepth.Deep, [typeof(DreadSpider)]);
            DungeonManager.Zones[spiderL1.ZoneId] = spiderL1;

            DungeonZone spiderL2 = new($"{prefixCaves} Spider Cave Level 2", map, 70000, typeof(TerathanMatriarch), TimeSpan.FromHours(8));
            spiderL2.SetSpawnProfile(DungeonDepth.Entrance, [typeof(TerathanWarrior)]);
            spiderL2.SetSpawnProfile(DungeonDepth.Deep, [typeof(TerathanAvenger), typeof(PoisonElemental)]);
            DungeonManager.Zones[spiderL2.ZoneId] = spiderL2;

            DungeonZone ethKeep = new($"{prefixCaves} Spider Cave Ethereal Keep", map, 100000, typeof(SystemDungeonBoss), TimeSpan.FromHours(10));
            ethKeep.SetSpawnProfile(DungeonDepth.Entrance, [typeof(EtherealWarrior)]);
            ethKeep.SetSpawnProfile(DungeonDepth.Deep, [typeof(BloodElemental)]);
            DungeonManager.Zones[ethKeep.ZoneId] = ethKeep;

            // ========================================================================
            // [CUSTOM / ML] Twisted Weald (뒤틀린 숲 - 일쉐나 영성 신전 부근)
            // ========================================================================
            DungeonZone twistedWeald = new($"{customPrefix} Twisted Weald", map, 180000, typeof(DreadHorn), TimeSpan.FromHours(12));
            twistedWeald.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Pixie), typeof(DireWolf), typeof(Changeling)]);
            twistedWeald.SetSpawnProfile(DungeonDepth.Middle, [typeof(CuSidhe), typeof(Satyr), typeof(Dryad)]);
            twistedWeald.SetSpawnProfile(DungeonDepth.Deep, [typeof(DreadHorn), typeof(Malefic)]);
            DungeonManager.Zones[twistedWeald.ZoneId] = twistedWeald;
			*/
        }
    }
}