using System;
using Server;
using Server.Mobiles;

namespace Server.Misc
{
    public static class MalasDungeon
    {
        public static void Setup()
        {
            Map map = Map.Malas;
            string prefix = "Malas Dungeons";

            // ========================================================================
            // [Doom] 둠 (언데드 및 심연 테마) - 최고 난이도
            // ========================================================================
			/*
            DungeonZone doomEnt = new($"{prefix} Doom Entrance", map, 50000, null, TimeSpan.FromHours(2));
            doomEnt.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BoneDemon), typeof(Ravager)]);
            DungeonManager.Zones[doomEnt.ZoneId] = doomEnt;

            DungeonZone doomTunnel = new($"{prefix} Doom Tunnel", map, 80000, typeof(DevourerOfSouls), TimeSpan.FromHours(6));
            doomTunnel.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Ravager), typeof(WandererOfTheVoid)]);
            doomTunnel.SetSpawnProfile(DungeonDepth.Deep, [typeof(DevourerOfSouls)]);
            DungeonManager.Zones[doomTunnel.ZoneId] = doomTunnel;

            DungeonZone doomInside = new($"{prefix} Doom Inside", map, 150000, typeof(AbysmalHorror), TimeSpan.FromHours(12));
            doomInside.SetSpawnProfile(DungeonDepth.Entrance, [typeof(FleshGolem), typeof(GoreFiend)]);
            doomInside.SetSpawnProfile(DungeonDepth.Middle, [typeof(ShadowKnight), typeof(MoundOfMaggots)]);
            doomInside.SetSpawnProfile(DungeonDepth.Deep, [typeof(AbysmalHorror), typeof(DarknightCreeper)]);
            DungeonManager.Zones[doomInside.ZoneId] = doomInside;

            DungeonZone doomGuardian = new($"{prefix} Doom Guardian's Room", map, 200000, typeof(DarkFather), TimeSpan.FromHours(24));
            doomGuardian.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BoneKnight), typeof(SkeletalDragon)]);
            doomGuardian.SetSpawnProfile(DungeonDepth.Deep, [typeof(DemonKnight)]); 
            DungeonManager.Zones[doomGuardian.ZoneId] = doomGuardian;

            DungeonZone doomGauntlet = new($"{prefix} Doom Gauntlet", map, 180000, typeof(SystemDungeonBoss), TimeSpan.FromHours(18));
            doomGauntlet.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Impaler), typeof(DarknightCreeper)]);
            doomGauntlet.SetSpawnProfile(DungeonDepth.Middle, [typeof(AbysmalHorror), typeof(FleshRenderer)]);
            doomGauntlet.SetSpawnProfile(DungeonDepth.Deep, [typeof(DemonKnight), typeof(ShadowKnight)]);
            DungeonManager.Zones[doomGauntlet.ZoneId] = doomGauntlet;

            DungeonZone doomLamp = new($"{prefix} Doom Lamp Room", map, 100000, typeof(PoisonElemental), TimeSpan.FromHours(8));
            doomLamp.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Wraith), typeof(Spectre)]);
            doomLamp.SetSpawnProfile(DungeonDepth.Deep, [typeof(PoisonElemental)]);
            DungeonManager.Zones[doomLamp.ZoneId] = doomLamp;
			*/
            // ========================================================================
            // [Labyrinth] 미궁 (미노타우르스 및 맹독 생물 테마)
            // ========================================================================
            DungeonZone labyrinth = new($"{prefix} Labyrinth", map, 150000, typeof(Miasma), TimeSpan.FromHours(10));
            labyrinth.SetSpawnProfile(DungeonDepth.Entrance, [typeof(Minotaur), typeof(Reptalon)]);
            labyrinth.SetSpawnProfile(DungeonDepth.Middle, [typeof(MinotaurCaptain), typeof(GoreFiend)]);
            labyrinth.SetSpawnProfile(DungeonDepth.Deep, [typeof(Miasma), typeof(Rend)]);
            DungeonManager.Zones[labyrinth.ZoneId] = labyrinth;

			/*
            // ========================================================================
            // [Bedlam] 베들램 (타락한 언데드 학원 테마)
            // ========================================================================
            DungeonZone bedlam = new($"{prefix} Bedlam", map, 140000, typeof(MonstrousInterredGrizzle), TimeSpan.FromHours(10));
            bedlam.SetSpawnProfile(DungeonDepth.Entrance, [typeof(GoreFiend), typeof(RedDeath)]);
            bedlam.SetSpawnProfile(DungeonDepth.Middle, [typeof(RottingCorpse), typeof(FleshGolem)]);
            bedlam.SetSpawnProfile(DungeonDepth.Deep, [typeof(MonstrousInterredGrizzle), typeof(SirPatrick)]);
            DungeonManager.Zones[bedlam.ZoneId] = bedlam;

            // ========================================================================
            // [The Citadel] 시타델 (검은 기사단 및 트라베스티 테마) - 입구는 토쿠노에 존재
            // ========================================================================
            DungeonZone citadel = new($"{prefix} The Citadel", map, 160000, typeof(Travesty), TimeSpan.FromHours(12));
            citadel.SetSpawnProfile(DungeonDepth.Entrance, [typeof(BlackOrderWarrior), typeof(BlackOrderThief)]);
            citadel.SetSpawnProfile(DungeonDepth.Middle, [typeof(BlackOrderMage), typeof(BlackOrderAssassin)]);
            citadel.SetSpawnProfile(DungeonDepth.Deep, [typeof(Travesty)]); // 변신술사 보스 트라베스티
            DungeonManager.Zones[citadel.ZoneId] = citadel;
			*/
        }
    }
}