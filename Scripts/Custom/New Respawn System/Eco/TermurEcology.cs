using System;
using Server;
using Server.Mobiles;

namespace Server.Misc
{
    public static class TerMurEcology
    {
        public static void Setup()
        {
            Map map = Map.TerMur;

            SetupTowns(map);
            SetupRegions(map);
            SetupEodon(map); // [Time of Legends] 이오돈 생태계 추가
        }

        private static void SetupTowns(Map map)
        {
            string prefix = "Ter Mur Towns";

            // ========================================================================
            // [Royal City] 로열 시티 (가고일 수도 및 문게이트)
            // ========================================================================
            EcoZone royalCity = new($"{prefix} Royal City", map);
            royalCity.AddSpecies(typeof(Gargoyle), 5); // NPC 가고일 거주지 느낌
            royalCity.AddSpecies(typeof(Dog), 5);
            EcosystemManager.Zones[royalCity.ZoneId] = royalCity;

            EcoZone royalCityMoongate = new($"{prefix} Royal City Royal City Moongate", map);
            royalCityMoongate.AddSpecies(typeof(Bird), 5);
            EcosystemManager.Zones[royalCityMoongate.ZoneId] = royalCityMoongate;

            // ========================================================================
            // [Holy City] 홀리 시티 (신성한 도시)
            // ========================================================================
            EcoZone holyCity = new($"{prefix} Holy City", map);
            holyCity.AddSpecies(typeof(Gargoyle), 5);
            EcosystemManager.Zones[holyCity.ZoneId] = holyCity;

            EcoZone holyCityMoongate = new($"{prefix} Holy City Holy City Moongate", map);
            holyCityMoongate.AddSpecies(typeof(Bird), 5);
            EcosystemManager.Zones[holyCityMoongate.ZoneId] = holyCityMoongate;

            // ========================================================================
            // [Dugan] 두간 (작은 거점)
            // ========================================================================
            EcoZone dugan = new($"{prefix} Dugan Dugan", map);
            dugan.AddSpecies(typeof(Cat), 5);
            EcosystemManager.Zones[dugan.ZoneId] = dugan;
        }

        private static void SetupRegions(Map map)
        {
            string prefix = "Ter Mur Sites Regions";

            // ========================================================================
            // [Regions] 테르 무르 외곽 구역
            // ========================================================================
            EcoZone atollBend = new($"{prefix} Atoll Bend", map);
            atollBend.AddSpecies(typeof(Slith), 10);
            atollBend.AddSpecies(typeof(WaterElemental), 5);
            EcosystemManager.Zones[atollBend.ZoneId] = atollBend;

            EcoZone chickenChase = new($"{prefix} Chicken Chase", map); // 닭 농장/추격전 구역
            chickenChase.AddSpecies(typeof(Chicken), 30);
            chickenChase.AddSpecies(typeof(GiantTurkey), 5);
            EcosystemManager.Zones[chickenChase.ZoneId] = chickenChase;

            EcoZone cityResidential = new($"{prefix} City Residential", map);
            cityResidential.AddSpecies(typeof(Dog), 5);
            cityResidential.AddSpecies(typeof(Cat), 5);
            EcosystemManager.Zones[cityResidential.ZoneId] = cityResidential;

            EcoZone coralDesert = new($"{prefix} Coral Desert", map);
            coralDesert.AddSpecies(typeof(Kepetch), 15);
            coralDesert.AddSpecies(typeof(Slith), 10);
            EcosystemManager.Zones[coralDesert.ZoneId] = coralDesert;

            EcoZone fishermansReach = new($"{prefix} Fishermans Reach", map);
            fishermansReach.AddSpecies(typeof(Alligator), 10);
            fishermansReach.AddSpecies(typeof(WaterElemental), 5);
            EcosystemManager.Zones[fishermansReach.ZoneId] = fishermansReach;

            EcoZone gatedIsle = new($"{prefix} Gated Isle", map);
            gatedIsle.AddSpecies(typeof(Gargoyle), 5);
            EcosystemManager.Zones[gatedIsle.ZoneId] = gatedIsle;

            EcoZone highPlain = new($"{prefix} High Plain", map);
            highPlain.AddSpecies(typeof(HighPlainsBoura), 20); // 테르 무르 특산물 보우라(들소)
            highPlain.AddSpecies(typeof(LowlandBoura), 15);
            EcosystemManager.Zones[highPlain.ZoneId] = highPlain;

            EcoZone holyCityIsland = new($"{prefix} Holy City Island", map);
            holyCityIsland.AddSpecies(typeof(Wisp), 5);
            EcosystemManager.Zones[holyCityIsland.ZoneId] = holyCityIsland;

            EcoZone kepetchWaste = new($"{prefix} Kepetch Waste", map);
            kepetchWaste.AddSpecies(typeof(Kepetch), 25);
            kepetchWaste.AddSpecies(typeof(KepetchAmbusher), 10);
            EcosystemManager.Zones[kepetchWaste.ZoneId] = kepetchWaste;

            EcoZone lavaLake = new($"{prefix} Lava Lake", map);
            lavaLake.AddSpecies(typeof(LavaLizard), 15);
            lavaLake.AddSpecies(typeof(LavaElemental), 10);
            EcosystemManager.Zones[lavaLake.ZoneId] = lavaLake;

            EcoZone lavapitPyramid = new($"{prefix} Lavapit Pyramid", map);
            lavapitPyramid.AddSpecies(typeof(FireGargoyle), 15);
            lavapitPyramid.AddSpecies(typeof(FireDaemon), 5);
            EcosystemManager.Zones[lavapitPyramid.ZoneId] = lavapitPyramid;

            EcoZone lostSettlement = new($"{prefix} Lost Settlement", map);
            lostSettlement.AddSpecies(typeof(UndeadGargoyle), 15);
            lostSettlement.AddSpecies(typeof(GargoyleShade), 10);
            EcosystemManager.Zones[lostSettlement.ZoneId] = lostSettlement;

            EcoZone northernSteppes = new($"{prefix} Northern Steppes", map);
            northernSteppes.AddSpecies(typeof(HighPlainsBoura), 20);
            northernSteppes.AddSpecies(typeof(Slith), 10);
            EcosystemManager.Zones[northernSteppes.ZoneId] = northernSteppes;

            EcoZone raptorIsland = new($"{prefix} Raptor Island", map); // 랩터 아일랜드
            raptorIsland.AddSpecies(typeof(Raptor), 30);
            EcosystemManager.Zones[raptorIsland.ZoneId] = raptorIsland;

            EcoZone royalPark = new($"{prefix} Royal Park", map);
            royalPark.AddSpecies(typeof(Bird), 15);
            royalPark.AddSpecies(typeof(Rabbit), 10);
            EcosystemManager.Zones[royalPark.ZoneId] = royalPark;

            EcoZone shrineSingularity = new($"{prefix} Shrine of Singularity", map);
            shrineSingularity.AddSpecies(typeof(Wisp), 10);
            EcosystemManager.Zones[shrineSingularity.ZoneId] = shrineSingularity;

            EcoZone slithValley = new($"{prefix} Slith Valley", map);
            slithValley.AddSpecies(typeof(Slith), 25);
            slithValley.AddSpecies(typeof(ToxicSlith), 10);
            EcosystemManager.Zones[slithValley.ZoneId] = slithValley;

            EcoZone spiderIsland = new($"{prefix} Spider Island", map);
            spiderIsland.AddSpecies(typeof(GiantSpider), 20);
            spiderIsland.AddSpecies(typeof(TrapdoorSpider), 10);
            EcosystemManager.Zones[spiderIsland.ZoneId] = spiderIsland;

            EcoZone spidersGuarde = new($"{prefix} Spiders Guarde", map);
            spidersGuarde.AddSpecies(typeof(WolfSpider), 15);
            spidersGuarde.AddSpecies(typeof(DreadSpider), 5);
            EcosystemManager.Zones[spidersGuarde.ZoneId] = spidersGuarde;

            EcoZone talonPoint = new($"{prefix} Talon Point", map);
            talonPoint.AddSpecies(typeof(Eagle), 15);
            talonPoint.AddSpecies(typeof(Gargoyle), 5);
            EcosystemManager.Zones[talonPoint.ZoneId] = talonPoint;

            EcoZone treefellowCourse = new($"{prefix} Treefellow Course", map);
            treefellowCourse.AddSpecies(typeof(Treefellow), 15); // 나무정령
            treefellowCourse.AddSpecies(typeof(Wisp), 5);
            EcosystemManager.Zones[treefellowCourse.ZoneId] = treefellowCourse;

            EcoZone voidIsle = new($"{prefix} Void Isle", map); // 보이드 생명체 출몰지
            voidIsle.AddSpecies(typeof(Korpre), 15);
            voidIsle.AddSpecies(typeof(Betrayer), 5);
            EcosystemManager.Zones[voidIsle.ZoneId] = voidIsle;

            EcoZone walledCircus = new($"{prefix} Walled Circus", map);
            walledCircus.AddSpecies(typeof(Mongbat), 15); // 서커스단 폐허의 몽뱃
            EcosystemManager.Zones[walledCircus.ZoneId] = walledCircus;

            EcoZone waterfallPoint = new($"{prefix} Waterfall Point", map);
            waterfallPoint.AddSpecies(typeof(WaterElemental), 10);
            waterfallPoint.AddSpecies(typeof(Slith), 5);
            EcosystemManager.Zones[waterfallPoint.ZoneId] = waterfallPoint;
        }

        private static void SetupEodon(Map map)
        {
            string prefix = "Ter Mur Eodon"; // 이오돈 야외 지역 커스텀 접두사

            // ========================================================================
            // [Eodon] 이오돈 대륙 (Time of Legends) 야외 구역
            // ========================================================================
            
            // 공룡들이 서식하는 방대한 정글 지대
            EcoZone saurianJungle = new($"{prefix} Saurian Jungle", map);
            saurianJungle.AddSpecies(typeof(Allosaurus), 10);
            saurianJungle.AddSpecies(typeof(Dimetrosaur), 15);
            saurianJungle.AddSpecies(typeof(Gallusaurus), 20);
            saurianJungle.AddSpecies(typeof(Saurosaurus), 15);
            EcosystemManager.Zones[saurianJungle.ZoneId] = saurianJungle;

            // 호랑이와 고릴라가 서식하는 열대 밀림
            EcoZone tigerJungle = new($"{prefix} Tiger Jungle", map);
            tigerJungle.AddSpecies(typeof(WildTiger), 20);
            tigerJungle.AddSpecies(typeof(SabertoothedTiger), 15);
            EcosystemManager.Zones[tigerJungle.ZoneId] = tigerJungle;

            // 맹독 뱀과 곤충들이 서식하는 독 늪지대
            EcoZone venomousSwamp = new($"{prefix} Venomous Swamp", map);
            venomousSwamp.AddSpecies(typeof(Najasaurus), 15); // 맹독 코브라형 공룡
            venomousSwamp.AddSpecies(typeof(MyrmidexDrone), 10);
            venomousSwamp.AddSpecies(typeof(GiantToad), 10);
            EcosystemManager.Zones[venomousSwamp.ZoneId] = venomousSwamp;

            // 미르미덱스(개미/곤충 종족) 야외 군락지
            EcoZone myrmidexTerritory = new($"{prefix} Myrmidex Territory", map);
            myrmidexTerritory.AddSpecies(typeof(MyrmidexDrone), 20);
            myrmidexTerritory.AddSpecies(typeof(MyrmidexWarrior), 10);
            EcosystemManager.Zones[myrmidexTerritory.ZoneId] = myrmidexTerritory;

            // 이오돈 화산 지대
            EcoZone eodonVolcano = new($"{prefix} Eodon Volcano", map);
            eodonVolcano.AddSpecies(typeof(FireElemental), 15);
            eodonVolcano.AddSpecies(typeof(LavaLizard), 15);
            eodonVolcano.AddSpecies(typeof(LavaSerpent), 10);
            EcosystemManager.Zones[eodonVolcano.ZoneId] = eodonVolcano;

            // 이오돈 원시 부족 마을 주변 (Kurak, Barako, Urali 등)
            EcoZone tribalVillages = new($"{prefix} Tribal Villages", map);
            tribalVillages.AddSpecies(typeof(SavageRider), 10); // 원시 부족 대체 몹
            tribalVillages.AddSpecies(typeof(SavageShaman), 5);
            tribalVillages.AddSpecies(typeof(Panther), 15);
            EcosystemManager.Zones[tribalVillages.ZoneId] = tribalVillages;
        }
    }
}