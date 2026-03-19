using System;
using Server;
using Server.Mobiles;

namespace Server.Misc
{
    public static class IlshenarEcology
    {
        public static void Setup()
        {
            Map map = Map.Ilshenar;

            SetupCities(map);
            SetupShrines(map);
            SetupHiddenWilds(map);
        }

        private static void SetupCities(Map map)
        {
            string prefix = "Ilshenar Cities";

            // ========================================================================
            // Ancient Citadel (에인션트 시타델 - 주카 족 점령지)
            // ========================================================================
            EcoZone ancientCitadelEnt = new($"{prefix} Ancient Citadel Entrance", map);
            ancientCitadelEnt.AddSpecies(typeof(JukaWarrior), 15);
            ancientCitadelEnt.AddSpecies(typeof(JukaMage), 5);
            ancientCitadelEnt.AddSpecies(typeof(JukaLord), 2);
            EcosystemManager.Zones[ancientCitadelEnt.ZoneId] = ancientCitadelEnt;

            // ========================================================================
            // Gargoyle City (가고일 시티)
            // ========================================================================
            EcoZone gargoyleBank = new($"{prefix} Gargoyle City Bank", map);
            gargoyleBank.AddSpecies(typeof(Gargoyle), 10);
            EcosystemManager.Zones[gargoyleBank.ZoneId] = gargoyleBank;

            EcoZone gargoyleCenter = new($"{prefix} Gargoyle City Central Area", map);
            gargoyleCenter.AddSpecies(typeof(Gargoyle), 15);
            gargoyleCenter.AddSpecies(typeof(StoneGargoyle), 5);
            EcosystemManager.Zones[gargoyleCenter.ZoneId] = gargoyleCenter;

            // ========================================================================
            // Lakeshire (레이크샤이어 - 미어 족 / 숲속 동물)
            // ========================================================================
            EcoZone lakeshireCenter = new($"{prefix} Lakeshire Central Area", map);
            lakeshireCenter.AddSpecies(typeof(GreatHart), 20);
            lakeshireCenter.AddSpecies(typeof(Hind), 25);
            lakeshireCenter.AddSpecies(typeof(BrownBear), 10);
            lakeshireCenter.AddSpecies(typeof(MeerWarrior), 5); // 미어 족 NPC/몹 혼재
            lakeshireCenter.AddSpecies(typeof(MeerMage), 2);
            EcosystemManager.Zones[lakeshireCenter.ZoneId] = lakeshireCenter;

            // ========================================================================
            // Mistas (미스타스 - 파괴된 도시, 야수와 주카)
            // ========================================================================
            EcoZone mistasCenter = new($"{prefix} Mistas Central Area", map);
            mistasCenter.AddSpecies(typeof(JukaWarrior), 10);
            mistasCenter.AddSpecies(typeof(HellCat), 5);
            mistasCenter.AddSpecies(typeof(DireWolf), 10);
            EcosystemManager.Zones[mistasCenter.ZoneId] = mistasCenter;

            // ========================================================================
            // Montor (몬터 - 화산 폭발로 파괴된 도시)
            // ========================================================================
            EcoZone montorCenter = new($"{prefix} Montor Central Area", map);
            montorCenter.AddSpecies(typeof(LavaLizard), 15);
            montorCenter.AddSpecies(typeof(FireGargoyle), 10);
            montorCenter.AddSpecies(typeof(HellHound), 10);
            EcosystemManager.Zones[montorCenter.ZoneId] = montorCenter;

            // ========================================================================
            // Req Volon (렉 볼론 - 폐허)
            // ========================================================================
            EcoZone reqVolonCenter = new($"{prefix} Req Volon Central Area", map);
            reqVolonCenter.AddSpecies(typeof(TimberWolf), 15);
            reqVolonCenter.AddSpecies(typeof(GiantSpider), 10);
            reqVolonCenter.AddSpecies(typeof(Ratman), 10);
            EcosystemManager.Zones[reqVolonCenter.ZoneId] = reqVolonCenter;

            // ========================================================================
            // Savage Camp (새비지 캠프 - 야만인 군락)
            // ========================================================================
            EcoZone savageCamp = new($"{prefix} Savage Camp Central Area", map);
            savageCamp.AddSpecies(typeof(SavageRider), 15);
            savageCamp.AddSpecies(typeof(SavageShaman), 5);
            savageCamp.AddSpecies(typeof(DireWolf), 10);
            savageCamp.AddSpecies(typeof(SavageRidgeback), 10); // 야만인용 릿지백
            EcosystemManager.Zones[savageCamp.ZoneId] = savageCamp;

            // ========================================================================
            // Terort Skitas (테로트 스키타스 - 지식의 신전 부근 폐허)
            // ========================================================================
            EcoZone terortEnt = new($"{prefix} Terort Skitas Entrance", map);
            terortEnt.AddSpecies(typeof(Skeleton), 15);
            terortEnt.AddSpecies(typeof(Zombie), 10);
            terortEnt.AddSpecies(typeof(Ghoul), 5);
            EcosystemManager.Zones[terortEnt.ZoneId] = terortEnt;
        }

        private static void SetupShrines(Map map)
        {
            string prefix = "Ilshenar Shrines";

            // 일쉐나는 Chaos(혼돈) 신전이 존재하지 않으며 8대 미덕 신전만 존재합니다.
            EcoZone shrineCompassion = new($"{prefix} Compassion", map);
            shrineCompassion.AddSpecies(typeof(Pixie), 5);
            shrineCompassion.AddSpecies(typeof(Wisp), 5);
            EcosystemManager.Zones[shrineCompassion.ZoneId] = shrineCompassion;

            EcoZone shrineHonesty = new($"{prefix} Honesty", map);
            shrineHonesty.AddSpecies(typeof(Wisp), 5);
            EcosystemManager.Zones[shrineHonesty.ZoneId] = shrineHonesty;

            EcoZone shrineHonor = new($"{prefix} Honor", map);
            shrineHonor.AddSpecies(typeof(Kirin), 2);
            shrineHonor.AddSpecies(typeof(Unicorn), 2);
            EcosystemManager.Zones[shrineHonor.ZoneId] = shrineHonor;

            EcoZone shrineHumility = new($"{prefix} Humility", map);
            shrineHumility.AddSpecies(typeof(Sheep), 15);
            shrineHumility.AddSpecies(typeof(Rabbit), 10);
            EcosystemManager.Zones[shrineHumility.ZoneId] = shrineHumility;

            EcoZone shrineJustice = new($"{prefix} Justice", map);
            shrineJustice.AddSpecies(typeof(Eagle), 5);
            shrineJustice.AddSpecies(typeof(GreatHart), 5);
            EcosystemManager.Zones[shrineJustice.ZoneId] = shrineJustice;

            EcoZone shrineSacrifice = new($"{prefix} Sacrifice", map);
            shrineSacrifice.AddSpecies(typeof(GreatHart), 5);
            shrineSacrifice.AddSpecies(typeof(TimberWolf), 5);
            EcosystemManager.Zones[shrineSacrifice.ZoneId] = shrineSacrifice;

            EcoZone shrineSpirituality = new($"{prefix} Spirituality", map);
            shrineSpirituality.AddSpecies(typeof(Wisp), 10);
            shrineSpirituality.AddSpecies(typeof(Pixie), 5);
            EcosystemManager.Zones[shrineSpirituality.ZoneId] = shrineSpirituality;

            EcoZone shrineValor = new($"{prefix} Valor", map);
            shrineValor.AddSpecies(typeof(Drake), 2);
            shrineValor.AddSpecies(typeof(Eagle), 5);
            EcosystemManager.Zones[shrineValor.ZoneId] = shrineValor;
        }

        private static void SetupHiddenWilds(Map map)
        {
            string customPrefix = "Ilshenar Hidden Wilds";

            // ========================================================================
            // [CUSTOM / HIDDEN] 일쉐나 야외 대규모 필드 생태계
            // ========================================================================
            
            // 컴패션 사막 (스핑크스와 전갈, 도적들이 서식하는 거대한 사막)
            EcoZone compassionDesert = new($"{customPrefix} Compassion Desert", map);
            compassionDesert.AddSpecies(typeof(Scorpion), 25);
            compassionDesert.AddSpecies(typeof(Snake), 20);
            compassionDesert.AddSpecies(typeof(Brigand), 10);
            EcosystemManager.Zones[compassionDesert.ZoneId] = compassionDesert;

            // 아너 정글 (거미 동굴 앞 열대 우림 지대)
            EcoZone honorJungle = new($"{customPrefix} Honor Jungle", map);
            honorJungle.AddSpecies(typeof(SilverSerpent), 10);
            honorJungle.AddSpecies(typeof(GiantSpider), 15);
            honorJungle.AddSpecies(typeof(Gorilla), 10);
            honorJungle.AddSpecies(typeof(Panther), 10);
            EcosystemManager.Zones[honorJungle.ZoneId] = honorJungle;

            // 가고일 설산 (가고일 시티 북쪽 얼음 지대)
            EcoZone gargoyleMountains = new($"{customPrefix} Snowy Mountains", map);
            gargoyleMountains.AddSpecies(typeof(SnowLeopard), 15);
            gargoyleMountains.AddSpecies(typeof(PolarBear), 10);
            gargoyleMountains.AddSpecies(typeof(FrostTroll), 5);
            EcosystemManager.Zones[gargoyleMountains.ZoneId] = gargoyleMountains;

            // 블러드 평원 (블러드 던전 외부의 핏빛 들판)
            EcoZone bloodPlains = new($"{customPrefix} Plains of Blood", map);
            bloodPlains.AddSpecies(typeof(HellHound), 15);
            bloodPlains.AddSpecies(typeof(Imp), 10);
            bloodPlains.AddSpecies(typeof(Gargoyle), 10);
            EcosystemManager.Zones[bloodPlains.ZoneId] = bloodPlains;

            // 영성의 숲 (미어 족 마을과 영성 신전 부근의 빽빽한 숲)
            EcoZone spiritualityForest = new($"{customPrefix} Spirituality Forest", map);
            spiritualityForest.AddSpecies(typeof(Pixie), 15);
            spiritualityForest.AddSpecies(typeof(Wisp), 10);
            spiritualityForest.AddSpecies(typeof(Unicorn), 2);
            spiritualityForest.AddSpecies(typeof(ShadowWisp), 5); // 숲의 깊고 어두운 부분
            EcosystemManager.Zones[spiritualityForest.ZoneId] = spiritualityForest;
        }
    }
}