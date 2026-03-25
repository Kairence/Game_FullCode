using System;
using Server;
using Server.Mobiles;

namespace Server.Misc
{
    public static class MalasEcology
    {
        public static void Setup()
        {
            Map map = Map.Malas;

            SetupTowns(map);
            SetupPointsOfInterest(map);
            SetupMinesAndForts(map);
        }

        private static void SetupTowns(Map map)
        {
            string prefix = "Malas Towns";

            // ========================================================================
            // [Luna] 루나 (팔라딘의 도시) - XML 세부 노드 완벽 반영
            // ========================================================================
            EcoZone lunaMoongate = new($"{prefix} Luna Luna Moongate", map);
            lunaMoongate.AddSpecies(typeof(Bird), 5);
            EcosystemManager.Zones[lunaMoongate.ZoneId] = lunaMoongate;

            EcoZone lunaBank = new($"{prefix} Luna Luna Bank", map);
            lunaBank.AddSpecies(typeof(Dog), 5);
            EcosystemManager.Zones[lunaBank.ZoneId] = lunaBank;

            EcoZone lunaClothiers = new($"{prefix} Luna Clothier's Colors", map);
            lunaClothiers.AddSpecies(typeof(Cat), 2);
            EcosystemManager.Zones[lunaClothiers.ZoneId] = lunaClothiers;

            EcoZone lunaArena = new($"{prefix} Luna Grand Arena", map);
            lunaArena.AddSpecies(typeof(Horse), 5); // 기사들의 말
            EcosystemManager.Zones[lunaArena.ZoneId] = lunaArena;

            EcoZone lunaHardwoods = new($"{prefix} Luna Hardwoods and More", map);
            lunaHardwoods.AddSpecies(typeof(Cat), 2);
            EcosystemManager.Zones[lunaHardwoods.ZoneId] = lunaHardwoods;

            EcoZone lunaPaladin = new($"{prefix} Luna Paladin's Stopover", map);
            lunaPaladin.AddSpecies(typeof(Horse), 10);
            EcosystemManager.Zones[lunaPaladin.ZoneId] = lunaPaladin;

            EcoZone lunaBridle = new($"{prefix} Luna Proud Bridle", map);
            lunaBridle.AddSpecies(typeof(Dog), 3);
            EcosystemManager.Zones[lunaBridle.ZoneId] = lunaBridle;

            EcoZone lunaMarket = new($"{prefix} Luna Open Market", map);
            lunaMarket.AddSpecies(typeof(Dog), 5);
            lunaMarket.AddSpecies(typeof(Cat), 5);
            EcosystemManager.Zones[lunaMarket.ZoneId] = lunaMarket;

            EcoZone lunaSage = new($"{prefix} Luna Sage's Refuge", map);
            lunaSage.AddSpecies(typeof(Bird), 5);
            EcosystemManager.Zones[lunaSage.ZoneId] = lunaSage;

            EcoZone lunaBlades = new($"{prefix} Luna Shining Blades", map);
            lunaBlades.AddSpecies(typeof(Dog), 2);
            EcosystemManager.Zones[lunaBlades.ZoneId] = lunaBlades;

            EcoZone lunaWisdom = new($"{prefix} Luna Shrine of Wisdom", map);
            lunaWisdom.AddSpecies(typeof(Wisp), 3);
            EcosystemManager.Zones[lunaWisdom.ZoneId] = lunaWisdom;

            EcoZone lunaVault = new($"{prefix} Luna Vault of Secrets", map);
            lunaVault.AddSpecies(typeof(Cat), 2);
            EcosystemManager.Zones[lunaVault.ZoneId] = lunaVault;

            // ========================================================================
            // [Umbra] 움브라 (강령술사의 도시)
            // ========================================================================
            EcoZone umbraCity = new($"{prefix} Umbra", map);
            umbraCity.AddSpecies(typeof(GiantRat), 15);
            umbraCity.AddSpecies(typeof(GiantRat), 15);
            umbraCity.AddSpecies(typeof(Skeleton), 10); // 도시 내 해골 배회
            umbraCity.AddSpecies(typeof(Wraith), 5);
            EcosystemManager.Zones[umbraCity.ZoneId] = umbraCity;
        }

        private static void SetupPointsOfInterest(Map map)
        {
            string prefix = "Malas Sites";

            // ========================================================================
            // [Points of Interest] 말라스 특수 야외 구역
            // ========================================================================
            EcoZone brokenMountains = new($"{prefix} Broken Mountains", map);
            brokenMountains.AddSpecies(typeof(EarthElemental), 15);
            brokenMountains.AddSpecies(typeof(MountainGoat), 10);
            EcosystemManager.Zones[brokenMountains.ZoneId] = brokenMountains;

            EcoZone corruptedForest = new($"{prefix} Corrupted Forest", map);
            corruptedForest.AddSpecies(typeof(DireWolf), 20);
            corruptedForest.AddSpecies(typeof(ShadowIronElemental), 10);
            corruptedForest.AddSpecies(typeof(PlagueBeast), 5);
            EcosystemManager.Zones[corruptedForest.ZoneId] = corruptedForest;

            EcoZone crumblingContinent = new($"{prefix} Crumbling Continent", map);
            crumblingContinent.AddSpecies(typeof(Gargoyle), 15);
            crumblingContinent.AddSpecies(typeof(StoneGargoyle), 10);
            EcosystemManager.Zones[crumblingContinent.ZoneId] = crumblingContinent;

            EcoZone crystalFens = new($"{prefix} Crystal Fens", map);
            crystalFens.AddSpecies(typeof(Wisp), 10);
            crystalFens.AddSpecies(typeof(CrystalElemental), 5);
            EcosystemManager.Zones[crystalFens.ZoneId] = crystalFens;

            EcoZone divideOfAbyss = new($"{prefix} Divide of the Abyss", map);
            divideOfAbyss.AddSpecies(typeof(Daemon), 10);
            divideOfAbyss.AddSpecies(typeof(HellHound), 15);
            EcosystemManager.Zones[divideOfAbyss.ZoneId] = divideOfAbyss;

            EcoZone dryHighlands = new($"{prefix} Dry Highlands", map);
            dryHighlands.AddSpecies(typeof(Scorpion), 20);
            dryHighlands.AddSpecies(typeof(Snake), 15);
            EcosystemManager.Zones[dryHighlands.ZoneId] = dryHighlands;

            EcoZone forgottenPyramid = new($"{prefix} Forgotten Pyramid", map);
            forgottenPyramid.AddSpecies(typeof(Mummy), 15);
            forgottenPyramid.AddSpecies(typeof(Skeleton), 20);
            forgottenPyramid.AddSpecies(typeof(Scorpion), 10);
            EcosystemManager.Zones[forgottenPyramid.ZoneId] = forgottenPyramid;

            EcoZone gravewaterLake = new($"{prefix} Gravewater Lake", map);
            gravewaterLake.AddSpecies(typeof(WaterElemental), 15);
            gravewaterLake.AddSpecies(typeof(SeaSerpent), 10);
            gravewaterLake.AddSpecies(typeof(BogThing), 5);
            EcosystemManager.Zones[gravewaterLake.ZoneId] = gravewaterLake;

            EcoZone grimswindRuins = new($"{prefix} Grimswind Ruins", map);
            grimswindRuins.AddSpecies(typeof(Wraith), 15);
            grimswindRuins.AddSpecies(typeof(Spectre), 10);
            grimswindRuins.AddSpecies(typeof(BoneKnight), 5);
            EcosystemManager.Zones[grimswindRuins.ZoneId] = grimswindRuins;

            EcoZone northernCrags = new($"{prefix} Northern Crags", map);
            northernCrags.AddSpecies(typeof(Eagle), 15);
            northernCrags.AddSpecies(typeof(MountainGoat), 15);
            northernCrags.AddSpecies(typeof(GrizzlyBear), 5);
            EcosystemManager.Zones[northernCrags.ZoneId] = northernCrags;

            EcoZone orcFortress = new($"{prefix} Orc Fortress", map);
            orcFortress.AddSpecies(typeof(Orc), 25);
            orcFortress.AddSpecies(typeof(OrcCaptain), 5);
            orcFortress.AddSpecies(typeof(OrcishMage), 5);
            EcosystemManager.Zones[orcFortress.ZoneId] = orcFortress;

            EcoZone hansesHostel = new($"{prefix} Hanse's Hostel", map);
            hansesHostel.AddSpecies(typeof(Cat), 5);
            hansesHostel.AddSpecies(typeof(Dog), 3);
            hansesHostel.AddSpecies(typeof(GiantRat), 5);
            EcosystemManager.Zones[hansesHostel.ZoneId] = hansesHostel;
        }

        private static void SetupMinesAndForts(Map map)
        {
            // ========================================================================
            // [Caves and Mines] 광산 1~9 (정기적으로 슬라임, 박쥐, 정령 출몰)
            // ========================================================================
            string prefixMines = "Malas Caves and Mines";
            for (int i = 1; i <= 9; i++)
            {
                EcoZone mine = new($"{prefixMines} Mine {i}", map);
                mine.AddSpecies(typeof(EarthElemental), 5);
                mine.AddSpecies(typeof(GiantRat), 10);
                mine.AddSpecies(typeof(Slime), 15);
                EcosystemManager.Zones[mine.ZoneId] = mine;
            }

            // ========================================================================
            // [Orc Forts] 오크 전초기지
            // ========================================================================
            string prefixOrcs = "Malas Orc Forts";
            for (int i = 1; i <= 6; i++)
            {
                EcoZone orcFort = new($"{prefixOrcs} Fort {i}", map);
                orcFort.AddSpecies(typeof(Orc), 15);
                orcFort.AddSpecies(typeof(OrcCaptain), 2);
                orcFort.AddSpecies(typeof(OrcishMage), 2);
                EcosystemManager.Zones[orcFort.ZoneId] = orcFort;
            }
        }
    }
}
