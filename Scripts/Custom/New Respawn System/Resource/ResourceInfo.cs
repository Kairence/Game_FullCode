using System;
using System.Collections.Generic;
using System.Linq;
using Server.Items; // Ingot, Ore 등의 아이템 참조용

namespace Server.Items // 기존 서버의 네임스페이스에 맞게 조정해 주세요.
{
    public enum CraftResource
    {
        None = 0,
        Iron = 1, Copper, Bronze, Gold, Agapite, Verite, Valorite, Mithril, Obsidian,
        DullCopper = 11, ShadowIron, // 단계가 밀려난 구형 광물들

        RegularLeather = 101, DernedLeather, RatnedLeather, SernedLeather, SpinedLeather, HornedLeather, BarbedLeather, PolarLeather, AbyssalLeather,

        RedScales = 201, YellowScales, BlackScales, GreenScales, WhiteScales, BlueScales,

        RegularWood = 301, OakWood, AshWood, YewWood, Heartwood, Bloodwood, Frostwood, EbonyWood, EthrnalWood,

        Trout = 401, Bass, Shiner, CrucianCarp, CatFish, CodFish, PerchFish, Ferring, Tuna
    }

    public enum CraftResourceType
    {
        None, Metal, Leather, Scales, Wood, Fish // Fish 타입 추가
    }

    public class CraftResourceInfo
    {
        public int Hue { get; }
        public int Number { get; }
        public string Name { get; }
        public CraftAttributeInfo AttributeInfo { get; }
        public CraftResource Resource { get; }
        public CraftResourceType ResourceTypeGroup { get; }
        public Type[] ResourceTypes { get; }

        public CraftResourceInfo(int hue, int number, string name, CraftAttributeInfo attributeInfo, CraftResource resource, CraftResourceType group, params Type[] resourceTypes)
        {
            Hue = hue;
            Number = number;
            Name = name;
            AttributeInfo = attributeInfo;
            Resource = resource;
            ResourceTypeGroup = group;
            ResourceTypes = resourceTypes;

            // 생성과 동시에 Type Table에 자동 등록
            for (int i = 0; i < resourceTypes.Length; ++i)
                CraftResources.RegisterType(resourceTypes[i], resource);
        }
    }

    public static class CraftResources
    {
        // 배열을 버리고 검색 속도 O(1)인 Dictionary 사용
        private static readonly Dictionary<CraftResource, CraftResourceInfo> m_Catalog = new();
        private static readonly Dictionary<Type, CraftResource> m_TypeTable = new();

        static CraftResources()
        {
            // =======================================================
            // 1. Metal (광물)
            // =======================================================
            Add(0x000, 1053109, "Iron", CraftAttributeInfo.Blank, CraftResource.Iron, CraftResourceType.Metal, typeof(IronIngot), typeof(IronOre), typeof(Granite));
            Add(0x96D, 1053106, "Copper", CraftAttributeInfo.Copper, CraftResource.Copper, CraftResourceType.Metal, typeof(CopperIngot), typeof(CopperOre), typeof(CopperGranite));
            Add(0x972, 1053105, "Bronze", CraftAttributeInfo.Bronze, CraftResource.Bronze, CraftResourceType.Metal, typeof(BronzeIngot), typeof(BronzeOre), typeof(BronzeGranite));
            Add(0x8A5, 1053104, "Gold", CraftAttributeInfo.Golden, CraftResource.Gold, CraftResourceType.Metal, typeof(GoldIngot), typeof(GoldOre), typeof(GoldGranite));
            Add(0x979, 1053103, "Agapite", CraftAttributeInfo.Agapite, CraftResource.Agapite, CraftResourceType.Metal, typeof(AgapiteIngot), typeof(AgapiteOre), typeof(AgapiteGranite));
            Add(0x89F, 1053102, "Verite", CraftAttributeInfo.Verite, CraftResource.Verite, CraftResourceType.Metal, typeof(VeriteIngot), typeof(VeriteOre), typeof(VeriteGranite));
            Add(0x8AB, 1053101, "Valorite", CraftAttributeInfo.Valorite, CraftResource.Valorite, CraftResourceType.Metal, typeof(ValoriteIngot), typeof(ValoriteOre), typeof(ValoriteGranite));
            Add(0x481, 1053107, "Mithril", CraftAttributeInfo.Blank, CraftResource.Mithril, CraftResourceType.Metal, typeof(MithrilIngot), typeof(MithrilOre), typeof(MithrilGranite));
            Add(0x001, 1053108, "Obsidian", CraftAttributeInfo.Blank, CraftResource.Obsidian, CraftResourceType.Metal, typeof(ObsidianIngot), typeof(ObsidianOre), typeof(ObsidianGranite));
            
            // 밀려난 구형 광물
            Add(0x973, 0, "Dull Copper", CraftAttributeInfo.DullCopper, CraftResource.DullCopper, CraftResourceType.Metal, typeof(DullCopperIngot), typeof(DullCopperOre), typeof(DullCopperGranite));
            Add(0x966, 0, "Shadow Iron", CraftAttributeInfo.ShadowIron, CraftResource.ShadowIron, CraftResourceType.Metal, typeof(ShadowIronIngot), typeof(ShadowIronOre), typeof(ShadowIronGranite));

            // =======================================================
            // 2. Leather (가죽)
            // =======================================================
            Add(0x000, 1049353, "Normal", CraftAttributeInfo.Blank, CraftResource.RegularLeather, CraftResourceType.Leather, typeof(Leather), typeof(Hides));
            Add(0x283, 1051901, "Derned", CraftAttributeInfo.Derned, CraftResource.DernedLeather, CraftResourceType.Leather, typeof(DernedLeather), typeof(DernedHides));
            Add(0x227, 1051902, "Ratned", CraftAttributeInfo.Ratned, CraftResource.RatnedLeather, CraftResourceType.Leather, typeof(RatnedLeather), typeof(RatnedHides));
            Add(0x1C1, 1051901, "Serned", CraftAttributeInfo.Serned, CraftResource.SernedLeather, CraftResourceType.Leather, typeof(SernedLeather), typeof(SernedHides));
            Add(0x8AC, 1049354, "Spined", CraftAttributeInfo.Spined, CraftResource.SpinedLeather, CraftResourceType.Leather, typeof(SpinedLeather), typeof(SpinedHides));
            Add(0x845, 1049355, "Horned", CraftAttributeInfo.Horned, CraftResource.HornedLeather, CraftResourceType.Leather, typeof(HornedLeather), typeof(HornedHides));
            Add(0x851, 1049356, "Barbed", CraftAttributeInfo.Barbed, CraftResource.BarbedLeather, CraftResourceType.Leather, typeof(BarbedLeather), typeof(BarbedHides));
            Add(0x481, 1051910, "Polar", CraftAttributeInfo.Blank, CraftResource.PolarLeather, CraftResourceType.Leather, typeof(PolarLeather), typeof(PolarHides)); // (White)
            Add(0x001, 1051911, "Abyssal", CraftAttributeInfo.Blank, CraftResource.AbyssalLeather, CraftResourceType.Leather, typeof(AbyssalLeather), typeof(AbyssalHides)); // (Void Black)

            // =======================================================
            // 3. Scales (비늘)
            // =======================================================
            Add(0x66D, 1053129, "Red Scales", CraftAttributeInfo.RedScales, CraftResource.RedScales, CraftResourceType.Scales, typeof(RedScales));
            Add(0x8A8, 1053130, "Yellow Scales", CraftAttributeInfo.YellowScales, CraftResource.YellowScales, CraftResourceType.Scales, typeof(YellowScales));
            Add(0x455, 1053131, "Black Scales", CraftAttributeInfo.BlackScales, CraftResource.BlackScales, CraftResourceType.Scales, typeof(BlackScales));
            Add(0x851, 1053132, "Green Scales", CraftAttributeInfo.GreenScales, CraftResource.GreenScales, CraftResourceType.Scales, typeof(GreenScales));
            Add(0x8FD, 1053133, "White Scales", CraftAttributeInfo.WhiteScales, CraftResource.WhiteScales, CraftResourceType.Scales, typeof(WhiteScales));
            Add(0x8B0, 1053134, "Blue Scales", CraftAttributeInfo.BlueScales, CraftResource.BlueScales, CraftResourceType.Scales, typeof(BlueScales));

            // =======================================================
            // 4. Wood (나무)
            // =======================================================
            Add(0x000, 1011542, "Normal", CraftAttributeInfo.Blank, CraftResource.RegularWood, CraftResourceType.Wood, typeof(Board), typeof(Log));
            Add(0x7DA, 1072533, "Oak", CraftAttributeInfo.OakWood, CraftResource.OakWood, CraftResourceType.Wood, typeof(OakBoard), typeof(OakLog));
            Add(0x4A7, 1072534, "Ash", CraftAttributeInfo.AshWood, CraftResource.AshWood, CraftResourceType.Wood, typeof(AshBoard), typeof(AshLog));
            Add(0x4A8, 1072535, "Yew", CraftAttributeInfo.YewWood, CraftResource.YewWood, CraftResourceType.Wood, typeof(YewBoard), typeof(YewLog));
            Add(0x4A9, 1072536, "Heartwood", CraftAttributeInfo.Heartwood, CraftResource.Heartwood, CraftResourceType.Wood, typeof(HeartwoodBoard), typeof(HeartwoodLog));
            Add(0x4AA, 1072538, "Bloodwood", CraftAttributeInfo.Bloodwood, CraftResource.Bloodwood, CraftResourceType.Wood, typeof(BloodwoodBoard), typeof(BloodwoodLog));
            Add(0x47F, 1072539, "Frostwood", CraftAttributeInfo.Frostwood, CraftResource.Frostwood, CraftResourceType.Wood, typeof(FrostwoodBoard), typeof(FrostwoodLog));
            Add(0x001, 1051916, "Ebony", CraftAttributeInfo.Blank, CraftResource.EbonyWood, CraftResourceType.Wood, typeof(EbonyBoard), typeof(EbonyLog)); // Pure Black
            Add(0x481, 1051917, "Ethrnal", CraftAttributeInfo.Blank, CraftResource.EthrnalWood, CraftResourceType.Wood, typeof(EthrnalBoard), typeof(EthrnalLog));; // Neon Ice

            // =======================================================
            // 5. Fish (어류 - 신규 생태계 연동)
            // =======================================================
            Add(0x000, 1063648, "Trout", CraftAttributeInfo.Blank, CraftResource.Trout, CraftResourceType.Fish, typeof(TroutFishSteak));
            Add(0x282, 1063649, "Bass", CraftAttributeInfo.Blank, CraftResource.Bass, CraftResourceType.Fish, typeof(BassFishSteak));
            Add(0x201, 1063650, "Shiner", CraftAttributeInfo.Blank, CraftResource.Shiner, CraftResourceType.Fish, typeof(ShinerFishSteak));
            Add(0x037, 1063651, "Crucian Carp", CraftAttributeInfo.Blank, CraftResource.CrucianCarp, CraftResourceType.Fish, typeof(CrucianCarpFishSteak));
            Add(0x03F, 1063652, "Cat Fish", CraftAttributeInfo.Blank, CraftResource.CatFish, CraftResourceType.Fish, typeof(CatFishSteak));
            Add(0x01E, 1063653, "Cod Fish", CraftAttributeInfo.Blank, CraftResource.CodFish, CraftResourceType.Fish, typeof(CodFishSteak));
            Add(0x01C, 1063654, "Perch Fish", CraftAttributeInfo.Blank, CraftResource.PerchFish, CraftResourceType.Fish, typeof(PerchFishSteak));
            Add(0x3B5, 1063655, "Ferring", CraftAttributeInfo.Blank, CraftResource.Ferring, CraftResourceType.Fish, typeof(FerringFishSteak));
            Add(0x028, 1063656, "Tuna", CraftAttributeInfo.Blank, CraftResource.Tuna, CraftResourceType.Fish, typeof(TunaFishSteak));
        }

        // Dictionary 맵핑 헬퍼
        private static void Add(int hue, int number, string name, CraftAttributeInfo attr, CraftResource res, CraftResourceType group, params Type[] types)
        {
            m_Catalog[res] = new CraftResourceInfo(hue, number, name, attr, res, group, types);
        }

        public static void RegisterType(Type resourceType, CraftResource resource) => m_TypeTable[resourceType] = resource;

        public static CraftResource GetFromType(Type resourceType) => m_TypeTable.TryGetValue(resourceType, out var res) ? res : CraftResource.None;

        public static CraftResourceInfo GetInfo(CraftResource resource) => m_Catalog.TryGetValue(resource, out var info) ? info : null;

        public static CraftResourceType GetType(CraftResource resource) => GetInfo(resource)?.ResourceTypeGroup ?? CraftResourceType.None;

        public static bool IsStandard(CraftResource resource) => 
            resource == CraftResource.None || resource == CraftResource.Iron || resource == CraftResource.RegularLeather || resource == CraftResource.RegularWood || resource == CraftResource.Trout;

        public static int GetLocalizationNumber(CraftResource resource) => GetInfo(resource)?.Number ?? 0;

        public static int GetHue(CraftResource resource) => GetInfo(resource)?.Hue ?? 0;

        public static string GetName(CraftResource resource) => GetInfo(resource)?.Name ?? string.Empty;

        // [중요 개선] DullCopper(11)처럼 Enum 번호가 꼬여있어도, 해당 카테고리 내에서 0부터 시작하는 정상적인 배열 인덱스를 반환하게 수정
        public static int GetIndex(CraftResource resource)
        {
            CraftResourceType group = GetType(resource);
            if (group == CraftResourceType.None) return 0;

            var list = m_Catalog.Values.Where(x => x.ResourceTypeGroup == group).OrderBy(x => (int)x.Resource).ToList();
            int index = list.FindIndex(x => x.Resource == resource);
            return index >= 0 ? index : 0;
        }

        public static CraftResource GetStart(CraftResource resource)
        {
            switch (GetType(resource))
            {
                case CraftResourceType.Metal: return CraftResource.Iron;
                case CraftResourceType.Leather: return CraftResource.RegularLeather;
                case CraftResourceType.Scales: return CraftResource.RedScales;
                case CraftResourceType.Wood: return CraftResource.RegularWood;
                case CraftResourceType.Fish: return CraftResource.Trout;
            }
            return CraftResource.None;
        }

        // =======================================================
        // 낡은 Runic / OreInfo 호환성 유지 구간
        // =======================================================
        public static CraftResource GetFromOreInfo(OreInfo info)
        {
            if (info.Name.IndexOf("Spined") >= 0) return CraftResource.SpinedLeather;
            else if (info.Name.IndexOf("질긴 가죽") >= 0) return CraftResource.DernedLeather;
            else if (info.Name.IndexOf("거친 가죽") >= 0) return CraftResource.RatnedLeather;
            else if (info.Name.IndexOf("경화 가죽") >= 0) return CraftResource.SernedLeather;
            else if (info.Name.IndexOf("Horned") >= 0) return CraftResource.HornedLeather;
            else if (info.Name.IndexOf("Barbed") >= 0) return CraftResource.BarbedLeather;
            else if (info.Name.IndexOf("Leather") >= 0) return CraftResource.RegularLeather;

            if (info.Level == 0) return CraftResource.Iron;
            else if (info.Level == 1) return CraftResource.DullCopper;
            else if (info.Level == 2) return CraftResource.ShadowIron;
            else if (info.Level == 3) return CraftResource.Copper;
            else if (info.Level == 4) return CraftResource.Bronze;
            else if (info.Level == 5) return CraftResource.Gold;
            else if (info.Level == 6) return CraftResource.Agapite;
            else if (info.Level == 7) return CraftResource.Verite;
            else if (info.Level == 8) return CraftResource.Valorite;

            return CraftResource.None;
        }

        public static CraftResource GetFromOreInfo(OreInfo info, ArmorMaterialType material)
        {
            if (material == ArmorMaterialType.Studded || material == ArmorMaterialType.Leather || material == ArmorMaterialType.Spined ||
                material == ArmorMaterialType.Horned || material == ArmorMaterialType.Barbed)
            {
                if (info.Level == 0) return CraftResource.RegularLeather;
                else if (info.Level == 1) return CraftResource.DernedLeather;
                else if (info.Level == 2) return CraftResource.RatnedLeather;
                else if (info.Level == 3) return CraftResource.SernedLeather;
                else if (info.Level == 4) return CraftResource.SpinedLeather;
                else if (info.Level == 5) return CraftResource.HornedLeather;
                else if (info.Level == 6) return CraftResource.BarbedLeather;

                return CraftResource.None;
            }
            return GetFromOreInfo(info);
        }
    }

    // 낡은 루닉 시스템 지원용 빈껍데기 클래스 (에러 방지용)
    public class OreInfo
    {
        public static readonly OreInfo Iron = new OreInfo(0, 0x000, "Iron");
        public static readonly OreInfo DullCopper = new OreInfo(11, 0x973, "Dull Copper");
        public static readonly OreInfo ShadowIron = new OreInfo(12, 0x966, "Shadow Iron");
        public static readonly OreInfo Copper = new OreInfo(1, 0x96D, "Copper");
        public static readonly OreInfo Bronze = new OreInfo(2, 0x972, "Bronze");
        public static readonly OreInfo Gold = new OreInfo(3, 0x8A5, "Gold");
        public static readonly OreInfo Agapite = new OreInfo(4, 0x979, "Agapite");
        public static readonly OreInfo Verite = new OreInfo(5, 0x89F, "Verite");
        public static readonly OreInfo Valorite = new OreInfo(6, 0x8AB, "Valorite");
        public static readonly OreInfo Mithril = new OreInfo(7, 0x8AB, "Mithril");
        public static readonly OreInfo Obsidian = new OreInfo(8, 0x8AB, "Obsidian");

        public int Level { get; }
        public int Hue { get; }
        public string Name { get; }

        public OreInfo(int level, int hue, string name)
        {
            Level = level;
            Hue = hue;
            Name = name;
        }
    }
	
	
	    public class CraftAttributeInfo
    {
        private int m_WeaponFireDamage;
        private int m_WeaponColdDamage;
        private int m_WeaponPoisonDamage;
        private int m_WeaponEnergyDamage;
        private int m_WeaponChaosDamage;
        private int m_WeaponDirectDamage;
        private int m_WeaponDurability;
        private int m_WeaponLuck;
        private int m_WeaponGoldIncrease;
        private int m_WeaponLowerRequirements;
        private int m_WeaponDamage;
        private int m_WeaponHitChance;
        private int m_WeaponHitLifeLeech;
        private int m_WeaponRegenHits;
        private int m_WeaponSwingSpeed;

        private int m_ArmorPhysicalResist;
        private int m_ArmorFireResist;
        private int m_ArmorColdResist;
        private int m_ArmorPoisonResist;
        private int m_ArmorEnergyResist;
        private int m_ArmorDurability;
        private int m_ArmorLuck;
        private int m_ArmorGoldIncrease;
        private int m_ArmorLowerRequirements;
        private int m_ArmorDamage;
        private int m_ArmorHitChance;
        private int m_ArmorRegenHits;
        private int m_ArmorMage;

        private int m_ShieldPhysicalResist;
        private int m_ShieldFireResist;
        private int m_ShieldColdResist;
        private int m_ShieldPoisonResist;
        private int m_ShieldEnergyResist;
        private int m_ShieldPhysicalRandom;
        private int m_ShieldColdRandom;
        private int m_ShieldSpellChanneling;
        private int m_ShieldLuck;
        private int m_ShieldLowerRequirements;
        private int m_ShieldRegenHits;
        private int m_ShieldBonusDex;
        private int m_ShieldBonusStr;
        private int m_ShieldReflectPhys;
        private int m_SelfRepair;

        private int m_OtherSpellChanneling;
        private int m_OtherLuck;
        private int m_OtherRegenHits;
        private int m_OtherLowerRequirements;

        private int m_RunicMinAttributes;
        private int m_RunicMaxAttributes;
        private int m_RunicMinIntensity;
        private int m_RunicMaxIntensity;
        
        public int WeaponFireDamage { get { return m_WeaponFireDamage; } set { m_WeaponFireDamage = value; } }
        public int WeaponColdDamage { get { return m_WeaponColdDamage; } set { m_WeaponColdDamage = value; } }
        public int WeaponPoisonDamage { get { return m_WeaponPoisonDamage; } set { m_WeaponPoisonDamage = value; } }
        public int WeaponEnergyDamage { get { return m_WeaponEnergyDamage; } set { m_WeaponEnergyDamage = value; } }
        public int WeaponChaosDamage { get { return m_WeaponChaosDamage; } set { m_WeaponChaosDamage = value; } }
        public int WeaponDirectDamage { get { return m_WeaponDirectDamage; } set { m_WeaponDirectDamage = value; } }
        public int WeaponDurability { get { return m_WeaponDurability; } set { m_WeaponDurability = value; } }
        public int WeaponLuck { get { return m_WeaponLuck; } set { m_WeaponLuck = value; } }
        public int WeaponGoldIncrease { get { return m_WeaponGoldIncrease; } set { m_WeaponGoldIncrease = value; } }
        public int WeaponLowerRequirements { get { return m_WeaponLowerRequirements; } set { m_WeaponLowerRequirements = value; } }
        public int WeaponDamage { get { return m_WeaponDamage; } set { m_WeaponDamage = value; } }
        public int WeaponHitChance { get { return m_WeaponHitChance; } set { m_WeaponHitChance = value; } }
        public int WeaponHitLifeLeech { get { return m_WeaponHitLifeLeech; } set { m_WeaponHitLifeLeech = value; } }
        public int WeaponRegenHits { get { return m_WeaponRegenHits; } set { m_WeaponRegenHits = value; } }
        public int WeaponSwingSpeed { get { return m_WeaponSwingSpeed; } set { m_WeaponSwingSpeed = value; } }

        public int ArmorPhysicalResist { get { return m_ArmorPhysicalResist; } set { m_ArmorPhysicalResist = value; } }
        public int ArmorFireResist { get { return m_ArmorFireResist; } set { m_ArmorFireResist = value; } }
        public int ArmorColdResist { get { return m_ArmorColdResist; } set { m_ArmorColdResist = value; } }
        public int ArmorPoisonResist { get { return m_ArmorPoisonResist; } set { m_ArmorPoisonResist = value; } }
        public int ArmorEnergyResist { get { return m_ArmorEnergyResist; } set { m_ArmorEnergyResist = value; } }
        public int ArmorDurability { get { return m_ArmorDurability; } set { m_ArmorDurability = value; } }
        public int ArmorLuck { get { return m_ArmorLuck; } set { m_ArmorLuck = value; } }
        public int ArmorGoldIncrease { get { return m_ArmorGoldIncrease; } set { m_ArmorGoldIncrease = value; } }
        public int ArmorLowerRequirements { get { return m_ArmorLowerRequirements; } set { m_ArmorLowerRequirements = value; } }
        public int ArmorDamage { get { return m_ArmorDamage; } set { m_ArmorDamage = value; } }
        public int ArmorHitChance { get { return m_ArmorHitChance; } set { m_ArmorHitChance = value; } }
        public int ArmorRegenHits { get { return m_ArmorRegenHits; } set { m_ArmorRegenHits = value; } }
        public int ArmorMage { get { return m_ArmorMage; } set { m_ArmorMage = value; } }

        public int ShieldPhysicalResist { get { return m_ShieldPhysicalResist; } set { m_ShieldPhysicalResist = value; } }
        public int ShieldFireResist { get { return m_ShieldFireResist; } set { m_ShieldFireResist = value; } }
        public int ShieldColdResist { get { return m_ShieldColdResist; } set { m_ShieldColdResist = value; } }
        public int ShieldPoisonResist { get { return m_ShieldPoisonResist; } set { m_ShieldPoisonResist = value; } }
        public int ShieldEnergyResist { get { return m_ShieldEnergyResist; } set { m_ShieldEnergyResist = value; } }
        public int ShieldPhysicalRandom { get { return m_ShieldPhysicalRandom; } set { m_ShieldPhysicalRandom = value; } }
        public int ShieldColdRandom { get { return m_ShieldColdRandom; } set { m_ShieldColdRandom = value; } }
        public int ShieldSpellChanneling { get { return m_ShieldSpellChanneling; } set { m_ShieldSpellChanneling = value; } }
        public int ShieldLuck { get { return m_ShieldLuck; } set { m_ShieldLuck = value; } }
        public int ShieldLowerRequirements { get { return m_ShieldLowerRequirements; } set { m_ShieldLowerRequirements = value; } }
        public int ShieldRegenHits { get { return m_ShieldRegenHits; } set { m_ShieldRegenHits = value; } }
        public int ShieldBonusDex { get { return m_ShieldBonusDex; } set { m_ShieldBonusDex = value; } }
        public int ShieldBonusStr { get { return m_ShieldBonusStr; } set { m_ShieldBonusStr = value; } }
        public int ShieldReflectPhys { get { return m_ShieldReflectPhys; } set { m_ShieldReflectPhys = value; } }
        public int ShieldSelfRepair { get { return m_SelfRepair; } set { m_SelfRepair = value; } }

        public int OtherSpellChanneling { get { return m_OtherSpellChanneling; } set { m_OtherSpellChanneling = value; } }
        public int OtherLuck { get { return m_OtherLuck; } set { m_OtherLuck = value; } }
        public int OtherRegenHits { get { return m_OtherRegenHits; } set { m_OtherRegenHits = value; } }
        public int OtherLowerRequirements { get { return m_OtherLowerRequirements; } set { m_OtherLowerRequirements = value; } }

        public int RunicMinAttributes { get { return m_RunicMinAttributes; } set { m_RunicMinAttributes = value; } }
        public int RunicMaxAttributes { get { return m_RunicMaxAttributes; } set { m_RunicMaxAttributes = value; } }
        public int RunicMinIntensity { get { return m_RunicMinIntensity; } set { m_RunicMinIntensity = value; } }
        public int RunicMaxIntensity { get { return m_RunicMaxIntensity; } set { m_RunicMaxIntensity = value; } }

        public CraftAttributeInfo()
        {
        }

        public static readonly CraftAttributeInfo Blank;
        public static readonly CraftAttributeInfo DullCopper, ShadowIron, Copper, Bronze, Golden, Agapite, Verite, Valorite;
        public static readonly CraftAttributeInfo Derned, Ratned, Serned, Spined, Horned, Barbed;
        public static readonly CraftAttributeInfo RedScales, YellowScales, BlackScales, GreenScales, WhiteScales, BlueScales;
        public static readonly CraftAttributeInfo OakWood, AshWood, YewWood, Heartwood, Bloodwood, Frostwood;

        static CraftAttributeInfo()
        {
            Blank = new CraftAttributeInfo();
        }
    }
}
