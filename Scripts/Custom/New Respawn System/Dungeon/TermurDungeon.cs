using System;
using Server;
using Server.Mobiles;

namespace Server.Misc
{
    public static class TerMurDungeon
    {
        public static void Setup()
        {
            Map map = Map.TerMur;
            string prefix = "Ter Mur Dungeons";
			/*

            // ========================================================================
            // [Tomb of Kings] 왕들의 무덤 (언데드 가고일 테마)
            // ========================================================================
            DungeonZone tombEnt = new($"{prefix} Tomb of Kings Entrance", map, 50000, null, TimeSpan.FromHours(4));
            tombEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(GargoyleShade), typeof(UndeadGargoyle)]);
            DungeonManager.Zones[tombEnt.ZoneId] = tombEnt;

            DungeonZone tombGate = new($"{prefix} Tomb of Kings Gate to Stygian Abyss", map, 80000, typeof(Niporailem), TimeSpan.FromHours(8));
            tombGate.SetSpawnProfile(DungeonDepth.Deep, [typeof(SilverSerpent)]);
            DungeonManager.Zones[tombGate.ZoneId] = tombGate;

            // ========================================================================
            // [Underworld] 언더월드 (고블린, 거미, 언데드 테마)
            // ========================================================================
            DungeonZone underworldEnt = new($"{prefix} Underworld Entrance", map, 100000, typeof(Navrey), TimeSpan.FromHours(10));
            underworldEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(GreenGoblin), typeof(Gremlin)]);
            underworldEnt.SetSpawnProfile(DungeonDepth.Middle, [typeof(Rotworm)]);
            underworldEnt.SetSpawnProfile(DungeonDepth.Deep, [typeof(WolfSpider), typeof(Navrey)]); // 나브레이 나이트아이즈
            DungeonManager.Zones[underworldEnt.ZoneId] = underworldEnt;

            // ========================================================================
            // [Stygian Abyss] 스티지언 어비스 (울티마 최대 규모 던전)
            // ========================================================================
            string abyssPrefix = $"{prefix} Stygian Abyss";

            // 출입구 구역
            DungeonZone abyssExitTomb = new($"{abyssPrefix} Exit to Tomb of Kings", map, 50000, null, TimeSpan.FromHours(4));
            abyssExitTomb.SetSpawnProfile(DungeonDepth.Entrance, [typeof(GargoyleDestroyer)]);
            DungeonManager.Zones[abyssExitTomb.ZoneId] = abyssExitTomb;

            DungeonZone abyssExitUnderworld = new($"{abyssPrefix} Exit to Underworld", map, 50000, null, TimeSpan.FromHours(4));
            abyssExitUnderworld.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Gremlin)]);
            DungeonManager.Zones[abyssExitUnderworld.ZoneId] = abyssExitUnderworld;

            // 메인 네임드 및 기믹 구역
            DungeonZone abyssalLair = new($"{abyssPrefix} Abyssal Lair Entrance", map, 200000, typeof(SlasherOfVeils), TimeSpan.FromHours(24));
            abyssalLair.SetSpawnProfile(DungeonDepth.Deep, [typeof(AbyssalInfernal), typeof(SlasherOfVeils)]);
            DungeonManager.Zones[abyssalLair.ZoneId] = abyssalLair;

            //DungeonZone cavernDiscarded = new($"{abyssPrefix} Cavern of the Discarded", map, 80000, typeof(ClanRibbon), TimeSpan.FromHours(6));
            //cavernDiscarded.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Ratman), typeof(Slime)]);
            //DungeonManager.Zones[cavernDiscarded.ZoneId] = cavernDiscarded;

            //DungeonZone clanScratch = new($"{abyssPrefix} Clan Scratch", map, 70000, typeof(ClanScratch), TimeSpan.FromHours(6));
            //clanScratch.SetSpawnProfile(DungeonDepth.Entrance, [typeof(WolfSpider), typeof(GreenGoblin)]);
            //DungeonManager.Zones[clanScratch.ZoneId] = clanScratch;

            DungeonZone crimsonVeins = new($"{abyssPrefix} Crimson Veins", map, 90000, typeof(FireDaemon), TimeSpan.FromHours(8));
            crimsonVeins.SetSpawnProfile(DungeonDepth.Entrance, [typeof(FireAnt), typeof(LavaLizard)]);
            DungeonManager.Zones[crimsonVeins.ZoneId] = crimsonVeins;

            DungeonZone enslavedGoblins = new($"{abyssPrefix} Enslaved Goblins", map, 50000, typeof(GreenGoblinAlchemist), TimeSpan.FromHours(4));
            enslavedGoblins.SetSpawnProfile(DungeonDepth.Entrance, [typeof(GreenGoblin)]);
            DungeonManager.Zones[enslavedGoblins.ZoneId] = enslavedGoblins;

            DungeonZone fairyDragon = new($"{abyssPrefix} Fairy Dragon Lair", map, 100000, typeof(FairyDragon), TimeSpan.FromHours(8));
            fairyDragon.SetSpawnProfile(DungeonDepth.Entrance, [typeof(FairyDragon), typeof(Wisp)]);
            DungeonManager.Zones[fairyDragon.ZoneId] = fairyDragon;

            DungeonZone fireTemple = new($"{abyssPrefix} Fire Temple Ruins", map, 120000, typeof(FireDaemon), TimeSpan.FromHours(10));
            fireTemple.SetSpawnProfile(DungeonDepth.Entrance, [typeof(FireGargoyle), typeof(LavaSerpent)]);
            fireTemple.SetSpawnProfile(DungeonDepth.Deep, [typeof(FireDaemon)]);
            DungeonManager.Zones[fireTemple.ZoneId] = fireTemple;

            DungeonZone hydra = new($"{abyssPrefix} Hydra", map, 100000, typeof(Hydra), TimeSpan.FromHours(8));
            hydra.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Slith)]);
            hydra.SetSpawnProfile(DungeonDepth.Deep, [typeof(Hydra)]);
            DungeonManager.Zones[hydra.ZoneId] = hydra;

            DungeonZone landsLich = new($"{abyssPrefix} Lands of the Lich", map, 110000, typeof(LichLord), TimeSpan.FromHours(8));
            landsLich.SetSpawnProfile(DungeonDepth.Entrance, [typeof(UndeadGargoyle), typeof(SkeletalDragon)]);
            DungeonManager.Zones[landsLich.ZoneId] = landsLich;

            DungeonZone lavaCaldera = new($"{abyssPrefix} Lava Caldera", map, 90000, typeof(LavaElemental), TimeSpan.FromHours(6));
            lavaCaldera.SetSpawnProfile(DungeonDepth.Entrance, [typeof(LavaElemental), typeof(Slime)]);
            DungeonManager.Zones[lavaCaldera.ZoneId] = lavaCaldera;

            DungeonZone medusa = new($"{abyssPrefix} Medusa's Lair", map, 180000, typeof(Medusa), TimeSpan.FromHours(12));
            medusa.SetSpawnProfile(DungeonDepth.Entrance, [typeof(IronBeetle), typeof(OphidianWarrior)]);
            medusa.SetSpawnProfile(DungeonDepth.Deep, [typeof(Medusa)]);
            DungeonManager.Zones[medusa.ZoneId] = medusa;

            DungeonZone passageTears = new($"{abyssPrefix} Passage of Tears", map, 60000, null, TimeSpan.FromHours(4));
            passageTears.SetSpawnProfile(DungeonDepth.Entrance, [typeof(WaterElemental), typeof(Slime)]);
            DungeonManager.Zones[passageTears.ZoneId] = passageTears;

            DungeonZone secretGarden = new($"{abyssPrefix} Secret Garden", map, 80000, typeof(Pixie), TimeSpan.FromHours(6));
            secretGarden.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Pixie), typeof(Wisp)]);
            DungeonManager.Zones[secretGarden.ZoneId] = secretGarden;

            DungeonZone serpentLair = new($"{abyssPrefix} Serpent Lair", map, 80000, typeof(SilverSerpent), TimeSpan.FromHours(6));
            serpentLair.SetSpawnProfile(DungeonDepth.Entrance, [typeof(GiantSerpent), typeof(SilverSerpent)]);
            DungeonManager.Zones[serpentLair.ZoneId] = serpentLair;

            DungeonZone silverSapling = new($"{abyssPrefix} Silver Sapling", map, 70000, typeof(Wisp), TimeSpan.FromHours(6));
            silverSapling.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Wisp), typeof(FairyDragon)]);
            DungeonManager.Zones[silverSapling.ZoneId] = silverSapling;

            DungeonZone stygianDragon = new($"{abyssPrefix} Stygian Dragon Lair Entrance", map, 220000, typeof(StygianDragon), TimeSpan.FromHours(24));
            stygianDragon.SetSpawnProfile(DungeonDepth.Entrance, [typeof(CrimsonDragon)]);
            stygianDragon.SetSpawnProfile(DungeonDepth.Deep, [typeof(StygianDragon)]);
            DungeonManager.Zones[stygianDragon.ZoneId] = stygianDragon;

            DungeonZone sutek = new($"{abyssPrefix} Sutek the Mage", map, 60000, null, TimeSpan.FromHours(4));
            sutek.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Gargoyle)]); // 퀘스트 및 실험체 구역
            DungeonManager.Zones[sutek.ZoneId] = sutek;

            // ========================================================================
            // [Time of Legends / Eodon] 이오돈 대륙 (테르 무르 맵에 위치)
            // ========================================================================
            string eodonPrefix = "Ter Mur Eodon";

            // 섀도우가드 (Shadowguard) - 미낙스가 숨어있는 성
            DungeonZone shadowguard = new($"{eodonPrefix} Shadowguard", map, 200000, null, TimeSpan.FromHours(18));
            shadowguard.SetSpawnProfile(DungeonDepth.Entrance, [typeof(SilverbackGorilla)]);
            //shadowguard.SetSpawnProfile(DungeonDepth.Deep, [typeof(Minax)]); 
            DungeonManager.Zones[shadowguard.ZoneId] = shadowguard;

            // 미르미덱스 핏 (Myrmidex Pit) - 거대 곤충 던전
            DungeonZone myrmidexPit = new($"{eodonPrefix} Myrmidex Pit", map, 130000, typeof(MyrmidexQueen), TimeSpan.FromHours(10));
            myrmidexPit.SetSpawnProfile(DungeonDepth.Entrance, [typeof(MyrmidexDrone), typeof(MyrmidexWarrior)]);
            myrmidexPit.SetSpawnProfile(DungeonDepth.Deep, [typeof(MyrmidexQueen)]);
            DungeonManager.Zones[myrmidexPit.ZoneId] = myrmidexPit;

            // 거대 용거북 챔피언 스폰 (Dragon Turtle Champ Spawn)
            DungeonZone dragonTurtleChamp = new($"{eodonPrefix} Dragon Turtle Habitat", map, 150000, typeof(DragonTurtle), TimeSpan.FromHours(12));
            dragonTurtleChamp.SetSpawnProfile(DungeonDepth.Entrance, [typeof(DragonTurtleHatchling)]);
            dragonTurtleChamp.SetSpawnProfile(DungeonDepth.Deep, [typeof(DragonTurtle)]);
            DungeonManager.Zones[dragonTurtleChamp.ZoneId] = dragonTurtleChamp;
			*/
        }
    }
}
