using System;
using Server;
using Server.Mobiles;

namespace Server.Misc
{
    public static class TokunoDungeon
    {
        public static void Setup()
        {
            Map map = Map.Tokuno;
            string prefix = "Tokuno Islands Dungeons";
            string customPrefix = "Tokuno Islands Hidden Dungeons";

            // ========================================================================
            // [Fan Dancer's Dojo] 팬 댄서 도장 (닌자, 로닌, 서큐버스 테마)
            // ========================================================================
            DungeonZone fanDojo = new($"{prefix} Fan Dancer's Dojo Entrance", map, 100000, typeof(Succubus), TimeSpan.FromHours(8));
            fanDojo.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Ninja), typeof(GiantRat)]);
            fanDojo.SetSpawnProfile(DungeonDepth.Middle, [typeof(EliteNinja), typeof(FanDancer)]);
            fanDojo.SetSpawnProfile(DungeonDepth.Deep, [typeof(Ronin), typeof(Succubus)]);
            DungeonManager.Zones[fanDojo.ZoneId] = fanDojo;

            // ========================================================================
            // [Yomotsu Mines] 요모츠 갱도 (요모츠 일족 및 땅의 정령 테마)
            // ========================================================================
            DungeonZone yomotsuMines = new($"{prefix} Yomotsu Mines Entrance", map, 80000, typeof(YomotsuElder), TimeSpan.FromHours(6));
            yomotsuMines.SetSpawnProfile(DungeonDepth.Entrance, [typeof(YomotsuWarrior), typeof(EarthElemental)]);
            yomotsuMines.SetSpawnProfile(DungeonDepth.Middle, [typeof(YomotsuPriest)]);
            yomotsuMines.SetSpawnProfile(DungeonDepth.Deep, [typeof(YomotsuElder), typeof(FireElemental)]);
            DungeonManager.Zones[yomotsuMines.ZoneId] = yomotsuMines;

            // ========================================================================
            // [CUSTOM / HIDDEN] 슬리핑 드래곤 및 시타델 입구
            // ========================================================================
            
            // Sleeping Dragon (이사무 섬의 거대 챔피언 스폰 구역)
            DungeonZone sleepingDragon = new($"{customPrefix} Sleeping Dragon Champ Spawn", map, 120000, null, TimeSpan.FromHours(12));
            sleepingDragon.SetSpawnProfile(DungeonDepth.Entrance, [typeof(DeathwatchBeetle), typeof(Kappa)]);
            sleepingDragon.SetSpawnProfile(DungeonDepth.Middle, [typeof(LesserHiryu), typeof(RevenantLion)]);
            sleepingDragon.SetSpawnProfile(DungeonDepth.Deep, [typeof(Hiryu), typeof(Oni)]);
            DungeonManager.Zones[sleepingDragon.ZoneId] = sleepingDragon;

            // The Citadel Entrance (시타델 진입로 - 검은 기사단 암살자 출몰지)
            //DungeonZone citadelEnt = new($"{customPrefix} The Citadel Entrance", map, 60000, null, TimeSpan.FromHours(6));
            //citadelEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackOrderWarrior)]);
            //citadelEnt.SetSpawnProfile(DungeonDepth.Deep, [typeof(BlackOrderAssassin), typeof(BlackOrderMage)]);
            //DungeonManager.Zones[citadelEnt.ZoneId] = citadelEnt;
        }
    }
}