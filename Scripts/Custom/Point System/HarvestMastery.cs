using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
    public enum HarvestType : int
    {
        None = 0,
        TotalOre = 10, IronOre = 11, CopperOre = 12, BronzeOre = 13, GoldOre = 14,
        AgapiteOre = 15, VeriteOre = 16, ValoriteOre = 17, MithrilOre = 18, ObsidianOre = 19,
        Sand = 20, Clay = 21,
        TotalGranite = 30, IronGranite = 31, CopperGranite = 32, BronzeGranite = 33, GoldGranite = 34,
        AgapiteGranite = 35, VeriteGranite = 36, ValoriteGranite = 37, MithrilGranite = 38, ObsidianGranite = 39,
        TotalGems = 50, StarSapphire = 51, Emerald = 52, Sapphire = 53, Ruby = 54, Citrine = 55, Amethyst = 56, Tourmaline = 57, Amber = 58, Diamond = 59,
        TotalWood = 70, RegularLog = 71, OakLog = 72, AshLog = 73, YewLog = 74, HeartwoodLog = 75, BloodwoodLog = 76, FrostwoodLog = 77, EbonyLog = 78, EthrnalLog = 79,
        Kindling = 80, Bark = 81, TotalLeather = 90, RegularLeather = 91, DernedLeather = 92, RatnedLeather = 93, SernedLeather = 94, SpinedLeather = 95, HornedLeather = 96, BarbedLeather = 97, PolarLeather = 98, AbyssalLeather = 99,
        TotalScales = 110, RedScales = 111, YellowScales = 112, BlackScales = 113, GreenScales = 114, WhiteScales = 115, BlueScales = 116,
        TotalFish = 130, Trout = 131, Bass = 132, Shiner = 133, CrucianCarp = 134, CatFish = 135, CodFish = 136, PerchFish = 137, Ferring = 138, Tuna = 139,
        TotalReagent = 160, BlackPearl = 161, Bloodmoss = 162, Garlic = 163, Ginseng = 164, MandrakeRoot = 165, Nightshade = 166, SpidersSilk = 167, SulfurousAsh = 168,
        TotalCrop = 200, Cotton = 201, Flax = 202, Wheat = 203, Pumpkin = 209, TotalFruit = 230, Apple = 231, Peach = 232, TotalAnimalProduct = 250, Milk = 251, Eggs = 252, Wool = 254, Honey = 255
    }

    public static class HarvestMastery
    {
        public const int MaxLevel = 100;
        public const int LevelOffset = 300;

        // ---------------------------------------------------------
        // 1. 경험치 및 레벨업 로직
        // ---------------------------------------------------------
        public static int GetNextExp(int currentLevel)
        {
            if (currentLevel >= MaxLevel) return 0;
            return (currentLevel + 1) * (currentLevel + 1) * 25;
        }

        public static void AddExp(PlayerMobile pm, HarvestType type, int amount = 1)
        {
            if (pm == null || type == HarvestType.None) return;

            ProcessExp(pm, type, amount);
            HarvestType totalType = GetCategoryTotal(type);
            if (totalType != HarvestType.None && totalType != type)
                ProcessExp(pm, totalType, amount);
        }

        private static void ProcessExp(PlayerMobile pm, HarvestType type, int amount)
        {
            int idx = (int)type;
            if (idx <= 0 || idx >= 300) return;

            int currentLevel = pm.HarvestPoint[idx + LevelOffset];
            if (currentLevel >= MaxLevel) return;

            pm.HarvestPoint[idx] += amount;

            if (pm.HarvestPoint[idx] >= GetNextExp(currentLevel))
            {
                pm.HarvestPoint[idx + LevelOffset]++;
                int hue = (type == GetCategoryTotal(type)) ? 0x44 : 0x35;
                pm.SendMessage(hue, $"[{GetHarvestName(type)}] 채집 숙련도가 {pm.HarvestPoint[idx + LevelOffset]} 레벨이 되었습니다!");
                pm.PlaySound(0x214);
            }
        }

        // ---------------------------------------------------------
        // 2. 개별 채집 보너스 (해당 자원 레벨 기준)
        // ---------------------------------------------------------

        // [Lv. 1] 정밀 수확: 더블 획득 확률
        public static double GetDoubleYieldChance(PlayerMobile pm, HarvestType type)
        {
            int level = pm.HarvestPoint[(int)type + LevelOffset];
            if (level < 1) return 0.0;
            return level * 0.002; // 레벨당 0.2% (Max 20%)
        }

        // [Lv. 10] 장비 유지: 도구 내구도 보호 확률
        public static double GetDurabilitySaveChance(PlayerMobile pm, HarvestType type)
        {
            int level = pm.HarvestPoint[(int)type + LevelOffset];
            if (level < 10) return 0.0;
            return level * 0.003; // 레벨당 0.3% (Max 30%)
        }

        // [Lv. 25] 신속한 동작: 동작 횟수 1회 감소
        public static int GetCountReduction(PlayerMobile pm, HarvestType type)
        {
            int level = pm.HarvestPoint[(int)type + LevelOffset];
            return (level >= 25) ? 1 : 0;
        }

        // [Lv. 100] 마스터의 육감: 수확량 최대치 고정
        public static bool IsMaximizedYield(PlayerMobile pm, HarvestType type)
        {
            return pm.HarvestPoint[(int)type + LevelOffset] >= 100;
        }

        // ---------------------------------------------------------
        // 3. 총합 채집 보너스 (카테고리 총합 레벨 기준)
        // ---------------------------------------------------------

        public static int GetTotalLevel(PlayerMobile pm, HarvestType type)
        {
            HarvestType totalType = GetCategoryTotal(type);
            if (totalType == HarvestType.None) return 0;
            return pm.HarvestPoint[(int)totalType + LevelOffset];
        }

        // [Lv. 1] 상위 자원 직관: 확률 보너스 (5.0 반환)
        public static double GetHigherTierBonus(int totalLevel)
        {
            if (totalLevel >= 1) return 5.0; 
            return 0.0;
        }

        // [Lv. 10] 부산물 발견: 희귀 아이템 발견 확률 (5%)
        public static double GetByproductChance(int totalLevel)
        {
            if (totalLevel >= 10) return 0.05; 
            return 0.0;
        }

        // [Lv. 25] 기력 효율: 기력 소모량 감소 (기본 5, 25렙당 -1)
        public static int GetStaminaCost(int totalLevel)
        {
            int cost = 5;
            if (totalLevel >= 25)
            {
                cost -= (totalLevel / 25);
            }
            if (cost < 1) cost = 1; // 최소 소모량 1 보장
            return cost;
        }

        // [Lv. 100] 숙련의 찰나: 즉시 채집 완료 (5% 확률)
        public static bool CheckInstantHarvest(int totalLevel)
        {
            if (totalLevel >= 100)
            {
                return Utility.RandomDouble() < 0.05;
            }
            return false;
        }

        // ---------------------------------------------------------
        // 4. 헬퍼 및 이름 변환 (하드코딩)
        // ---------------------------------------------------------
        public static HarvestType GetCategoryTotal(HarvestType type)
        {
            int idx = (int)type;
            if (idx >= 11 && idx <= 29) return HarvestType.TotalOre;
            if (idx >= 31 && idx <= 49) return HarvestType.TotalGranite;
            if (idx >= 51 && idx <= 69) return HarvestType.TotalGems;
            if (idx >= 71 && idx <= 89) return HarvestType.TotalWood;
            if (idx >= 91 && idx <= 109) return HarvestType.TotalLeather;
            if (idx >= 111 && idx <= 129) return HarvestType.TotalScales;
            if (idx >= 131 && idx <= 159) return HarvestType.TotalFish;
            if (idx >= 161 && idx <= 199) return HarvestType.TotalReagent;
            if (idx >= 201 && idx <= 229) return HarvestType.TotalCrop;
            if (idx >= 231 && idx <= 249) return HarvestType.TotalFruit;
            if (idx >= 251 && idx <= 279) return HarvestType.TotalAnimalProduct;
            return HarvestType.None;
        }

        public static HarvestType GetHarvestType(Type itemType)
        {
            if (itemType == null) return HarvestType.None;
            string name = itemType.Name;
            
            // 자주 쓰이는 타입 예외 처리 및 자동 매핑
            if (name == "Log" || name == "Board") return HarvestType.RegularLog;
            if (name == "Leather" || name == "Hides") return HarvestType.RegularLeather;
            if (name == "IronIngot") return HarvestType.IronOre;

            if (Enum.TryParse(name, out HarvestType result)) return result;
            return HarvestType.None;
        }

        public static string GetHarvestName(HarvestType type)
        {
            return type switch
            {
                HarvestType.TotalOre => "광석류 총합", HarvestType.IronOre => "철광석", HarvestType.CopperOre => "구리",
                HarvestType.TotalGranite => "화강암 총합", HarvestType.IronGranite => "일반 화강암",
                HarvestType.TotalWood => "목재류 총합", HarvestType.RegularLog => "일반 나무",
                HarvestType.TotalLeather => "가죽류 총합", HarvestType.RegularLeather => "일반 가죽",
                HarvestType.TotalFish => "어류 총합", HarvestType.Trout => "송어",
                HarvestType.TotalReagent => "시약류 총합", HarvestType.Garlic => "마늘",
                HarvestType.TotalCrop => "농작물 총합", HarvestType.Wheat => "밀",
                HarvestType.TotalAnimalProduct => "생활 채집 총합", HarvestType.Honey => "꿀",
                _ => type.ToString()
            };
        }
    }
}