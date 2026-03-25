using System;
using Server;
using Server.Mobiles;

namespace Server.Misc
{
    public static class TokunoEcology
    {
        public static void Setup()
        {
            Map map = Map.Tokuno;

            SetupZento(map);
            SetupMakotoJima(map);
            SetupIsamuJima(map);
            SetupHomareJima(map);
        }

        private static void SetupZento(Map map)
        {
            string prefix = "Tokuno Islands Towns Zento";

            // ========================================================================
            // [Zento] 젠토 (안전한 수도 - 문게이트와 은행 분리)
            // ========================================================================
            EcoZone zentoMoongate = new($"{prefix} Zento Moongate", map);
            zentoMoongate.AddSpecies(typeof(Bird), 10);
            zentoMoongate.AddSpecies(typeof(Crane), 5); // 학
            EcosystemManager.Zones[zentoMoongate.ZoneId] = zentoMoongate;

            EcoZone zentoBank = new($"{prefix} Zento Bank", map);
            zentoBank.AddSpecies(typeof(Cat), 5);
            zentoBank.AddSpecies(typeof(Dog), 5);
            EcosystemManager.Zones[zentoBank.ZoneId] = zentoBank;
        }

        private static void SetupMakotoJima(Map map)
        {
            string prefix = "Tokuno Islands Sites Makoto-Jima";

            // ========================================================================
            // [Makoto-Jima] 마코토 섬 (젠토가 있는 평화로운 섬이지만 일부 오염됨)
            // ========================================================================
            EcoZone makotoMoongate = new($"{prefix} Moongate", map);
            makotoMoongate.AddSpecies(typeof(Crane), 10);
            EcosystemManager.Zones[makotoMoongate.ZoneId] = makotoMoongate;

            EcoZone makotoShrine = new($"{prefix} Shrine", map);
            makotoShrine.AddSpecies(typeof(Crane), 10);
            makotoShrine.AddSpecies(typeof(BakeKitsune), 2);
            EcosystemManager.Zones[makotoShrine.ZoneId] = makotoShrine;

            EcoZone makotoWaste = new($"{prefix} The Waste", map); // 사막/황무지
            makotoWaste.AddSpecies(typeof(DeathwatchBeetle), 20); // 사막 벌레
            makotoWaste.AddSpecies(typeof(TsukiWolf), 5);
            makotoWaste.AddSpecies(typeof(RevenantLion), 2);
            EcosystemManager.Zones[makotoWaste.ZoneId] = makotoWaste;
        }

        private static void SetupIsamuJima(Map map)
        {
            string prefix = "Tokuno Islands Sites Isamu-Jima";

            // ========================================================================
            // [Isamu-Jima] 이사무 섬 (험준한 산맥과 용의 계곡)
            // ========================================================================
            EcoZone isamuMoongate = new($"{prefix} Moongate", map);
            isamuMoongate.AddSpecies(typeof(Crane), 5);
            EcosystemManager.Zones[isamuMoongate.ZoneId] = isamuMoongate;

            EcoZone isamuShrine = new($"{prefix} Shrine", map);
            isamuShrine.AddSpecies(typeof(Kirin), 3);
            EcosystemManager.Zones[isamuShrine.ZoneId] = isamuShrine;

            EcoZone lotusLakes = new($"{prefix} Lotus Lakes", map); // 연꽃 호수
            lotusLakes.AddSpecies(typeof(Kappa), 15); // 물가 요괴 카파
            lotusLakes.AddSpecies(typeof(Crane), 10);
            lotusLakes.AddSpecies(typeof(GiantToad), 5);
            EcosystemManager.Zones[lotusLakes.ZoneId] = lotusLakes;

            EcoZone mountSho = new($"{prefix} Mount Sho", map); // 쇼 산맥
            mountSho.AddSpecies(typeof(Oni), 10); // 오니 (도깨비)
            mountSho.AddSpecies(typeof(LesserHiryu), 15);
            mountSho.AddSpecies(typeof(Hiryu), 5);
            EcosystemManager.Zones[mountSho.ZoneId] = mountSho;

            EcoZone dragonValley = new($"{prefix} Dragon Valley", map); // 용의 계곡
            dragonValley.AddSpecies(typeof(LesserHiryu), 20);
            dragonValley.AddSpecies(typeof(Hiryu), 10);
            dragonValley.AddSpecies(typeof(SerpentineDragon), 5);
            EcosystemManager.Zones[dragonValley.ZoneId] = dragonValley;

            EcoZone winterSpur = new($"{prefix} Winter Spur", map); // 겨울 산줄기
            winterSpur.AddSpecies(typeof(LadyOfTheSnow), 10); // 설녀
            winterSpur.AddSpecies(typeof(IceSnake), 15);
            winterSpur.AddSpecies(typeof(SnowElemental), 10);
            EcosystemManager.Zones[winterSpur.ZoneId] = winterSpur;
        }

        private static void SetupHomareJima(Map map)
        {
            string prefix = "Tokuno Islands Sites Homare-Jima";

            // ========================================================================
            // [Homare-Jima] 호마레 섬 (어두운 숲과 사무라이/닌자의 격전지)
            // ========================================================================
            EcoZone homareMoongate = new($"{prefix} Moongate", map);
            homareMoongate.AddSpecies(typeof(Crane), 5);
            EcosystemManager.Zones[homareMoongate.ZoneId] = homareMoongate;

            EcoZone homareShrine = new($"{prefix} Shrine", map);
            homareShrine.AddSpecies(typeof(Wisp), 5);
            homareShrine.AddSpecies(typeof(BakeKitsune), 3);
            EcosystemManager.Zones[homareShrine.ZoneId] = homareShrine;

            EcoZone fieldOfEchoes = new($"{prefix} Field of Echoes", map); // 메아리의 들판
            fieldOfEchoes.AddSpecies(typeof(RevenantLion), 15);
            fieldOfEchoes.AddSpecies(typeof(Ronin), 10);
            fieldOfEchoes.AddSpecies(typeof(Gaman), 15); // 토쿠노 들소
            EcosystemManager.Zones[fieldOfEchoes.ZoneId] = fieldOfEchoes;

            EcoZone craneMarsh = new($"{prefix} Crane Marsh", map); // 학의 늪지대
            craneMarsh.AddSpecies(typeof(Crane), 25);
            craneMarsh.AddSpecies(typeof(Kappa), 10);
            craneMarsh.AddSpecies(typeof(Bogling), 15);
            EcosystemManager.Zones[craneMarsh.ZoneId] = craneMarsh;

            EcoZone bushidoDojo = new($"{prefix} Bushido Dojo", map); // 무사도 도장
            bushidoDojo.AddSpecies(typeof(Samurai), 15);
            bushidoDojo.AddSpecies(typeof(Ronin), 5);
            EcosystemManager.Zones[bushidoDojo.ZoneId] = bushidoDojo;

            EcoZone kitsuneWoods = new($"{prefix} Kitsune Woods", map); // 여우 숲
            kitsuneWoods.AddSpecies(typeof(BakeKitsune), 20); // 구미호
            kitsuneWoods.AddSpecies(typeof(TsukiWolf), 15);
            kitsuneWoods.AddSpecies(typeof(KazeKemono), 10); // 바람 요괴
            EcosystemManager.Zones[kitsuneWoods.ZoneId] = kitsuneWoods;
        }
    }
}
