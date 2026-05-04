using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Engines.Craft;
using System.Linq;

namespace Server.Misc
{
    // ====================================================================
    // 🌟 [신규] 색자원과 품질을 기억하는 창고 저장용 고속 키(Key)
    // ====================================================================
    public record struct StoredItemKey(Type ItemType, CraftResource Resource, bool IsExceptional);

    // 창고 등급 정의 (공간 점유 및 AI 구매 가이드용)
    public enum StorageTier { None, Small, Medium, Large, Special }

    public static class StorageEconomy
    {
        // 1. [재봉술 영역] 아이템 종류(Variety) 확장 데이터
        private static readonly Dictionary<Type, int> VarietyBonusMap = new()
        {
            { typeof(Pouch), 5 },
            { typeof(Bag), 10 },
            { typeof(Backpack), 20 }
        };

        // 2. [목공술 영역] 아이템 총 수량(Total Count) 확장 데이터
        private static readonly Dictionary<Type, int> QuantityBonusMap = new()
        {
            { typeof(WoodenBox), 50 },
            { typeof(SmallCrate), 100 },
            { typeof(MediumCrate), 200 },
            { typeof(LargeCrate), 300 },
            { typeof(WoodenChest), 500 },
            { typeof(MetalChest), 1000 }
        };

        public static (int MaxTypes, int MaxQuantity) GetStorageLimits(Dictionary<StoredItemKey, int> warehouse)
        {
            int maxTypes = 5;
            int maxQuantity = 5;

            if (warehouse == null) return (maxTypes, maxQuantity);

            foreach (var kvp in warehouse)
            {
                Type type = kvp.Key.ItemType;

                if (VarietyBonusMap.TryGetValue(type, out int vBonus))
                {
                    maxTypes += (vBonus * kvp.Value);
                }

                if (QuantityBonusMap.TryGetValue(type, out int qBonus))
                {
                    maxQuantity += (qBonus * kvp.Value);
                }
            }

            return (maxTypes, maxQuantity);
        }

        public static (int Width, int Height) GetRequiredDimensions(StorageTier tier) => tier switch
        {
            StorageTier.Special => (10, 10),
            StorageTier.Large   => (4, 4),
            StorageTier.Medium  => (2, 2),
            StorageTier.Small   => (1, 1),
            _ => (0, 0)
        };
    }

    public enum WorkshopBonusType { SuccessRate, ExceptionalChance, ResourceSave }
    public enum WorkshopTier { Small, Medium, Large }

    public static class WorkshopEconomy
    {
        public static readonly Dictionary<Type, (SkillName Skill, double Bonus, WorkshopBonusType Type)> AddonBonusMap = new()
        {
            // --- 1. 요리 (Cooking) 관련 ---
            { typeof(StoneOvenEastDeed), (SkillName.Cooking, 0.10, WorkshopBonusType.SuccessRate) },
            { typeof(StoneOvenSouthDeed), (SkillName.Cooking, 0.10, WorkshopBonusType.SuccessRate) },
            { typeof(FlourMillEastDeed), (SkillName.Cooking, 0.05, WorkshopBonusType.ResourceSave) },
            { typeof(FlourMillSouthDeed), (SkillName.Cooking, 0.05, WorkshopBonusType.ResourceSave) },
            { typeof(BBQSmokerDeed), (SkillName.Cooking, 0.10, WorkshopBonusType.SuccessRate) },
            { typeof(ElvenStoveEastDeed), (SkillName.Cooking, 0.07, WorkshopBonusType.SuccessRate) },
            { typeof(ElvenStoveSouthDeed), (SkillName.Cooking, 0.07, WorkshopBonusType.SuccessRate) },

            // --- 2. 대장술 (Blacksmithy) 관련 ---
            { typeof(SmallForgeDeed), (SkillName.Blacksmith, 0.05, WorkshopBonusType.SuccessRate) },
            { typeof(LargeForgeEastDeed), (SkillName.Blacksmith, 0.08, WorkshopBonusType.SuccessRate) },
            { typeof(LargeForgeSouthDeed), (SkillName.Blacksmith, 0.08, WorkshopBonusType.SuccessRate) },
            { typeof(AnvilEastDeed), (SkillName.Blacksmith, 0.05, WorkshopBonusType.ExceptionalChance) },
            { typeof(AnvilSouthDeed), (SkillName.Blacksmith, 0.05, WorkshopBonusType.ExceptionalChance) },
            { typeof(SmithingPressDeed), (SkillName.Blacksmith, 0.10, WorkshopBonusType.ExceptionalChance) },
            { typeof(StoneAnvilEastDeed), (SkillName.Blacksmith, 0.06, WorkshopBonusType.ExceptionalChance) },
            { typeof(StoneAnvilSouthDeed), (SkillName.Blacksmith, 0.06, WorkshopBonusType.ExceptionalChance) },

            // --- 3. 재봉술 (Tailoring) 관련 ---
            { typeof(LoomEastDeed), (SkillName.Tailoring, 0.07, WorkshopBonusType.SuccessRate) },
            { typeof(LoomSouthDeed), (SkillName.Tailoring, 0.07, WorkshopBonusType.SuccessRate) },
            { typeof(SpinningwheelEastDeed), (SkillName.Tailoring, 0.05, WorkshopBonusType.ResourceSave) },
            { typeof(SpinningwheelSouthDeed), (SkillName.Tailoring, 0.05, WorkshopBonusType.ResourceSave) },
            { typeof(ElvenSpinningwheelEastDeed), (SkillName.Tailoring, 0.06, WorkshopBonusType.ResourceSave) },
            { typeof(SewingMachineDeed), (SkillName.Tailoring, 0.10, WorkshopBonusType.SuccessRate) },
            { typeof(SewingMachine), (SkillName.Tailoring, 0.10, WorkshopBonusType.SuccessRate) },

            // --- 4. 기록술 (Inscription) 관련 ---
            { typeof(WritingDeskDeed), (SkillName.Inscribe, 0.10, WorkshopBonusType.SuccessRate) },
            { typeof(WritingTable), (SkillName.Inscribe, 0.05, WorkshopBonusType.SuccessRate) },

            // --- 5. 땜질 (Tinkering) 관련 ---
            { typeof(TinkerBenchDeed), (SkillName.Tinkering, 0.05, WorkshopBonusType.SuccessRate) },

            // --- 6. 목공 (Carpentry) 관련 ---
            { typeof(WoodworkersBenchDeed), (SkillName.Carpentry, 0.05, WorkshopBonusType.SuccessRate) },
            { typeof(SpinningLatheDeed), (SkillName.Carpentry, 0.05, WorkshopBonusType.SuccessRate) },
            { typeof(RitualTableDeed), (SkillName.Carpentry, 0.10, WorkshopBonusType.ExceptionalChance) },

            // --- 7. 연금술 및 유리세공 (Alchemy/Glassblowing) 관련 ---
            { typeof(AlchemyStationDeed), (SkillName.Alchemy, 0.10, WorkshopBonusType.SuccessRate) },
            { typeof(HeatingStand), (SkillName.Alchemy, 0.05, WorkshopBonusType.SuccessRate) },

            // --- 8. 활 제작 (Bowcraft) 관련 ---
            { typeof(FletchingStationDeed), (SkillName.Fletching, 0.10, WorkshopBonusType.SuccessRate) }
        };

        public static double GetFinalBonus(Dictionary<StoredItemKey, int> warehouse, SkillName skill, WorkshopBonusType type)
        {
            if (warehouse == null) return 0.0;

            double baseSum = 0.0;
            int addonCount = 0;

            foreach (var kvp in warehouse)
            {
                if (AddonBonusMap.TryGetValue(kvp.Key.ItemType, out var data) && data.Skill == skill)
                {
                    addonCount += kvp.Value;
                    if (data.Type == type)
                    {
                        baseSum += (data.Bonus * kvp.Value);
                    }
                }
            }

            double multiplier = addonCount switch
            {
                >= 6 => 2.0,
                >= 3 => 1.5,
                _ => 1.0
            };

            return baseSum * multiplier;
        }

        public static WorkshopTier GetTier(Dictionary<StoredItemKey, int> warehouse, SkillName skill)
        {
            int count = warehouse?.Where(kvp => AddonBonusMap.ContainsKey(kvp.Key.ItemType) && AddonBonusMap[kvp.Key.ItemType].Skill == skill)
                                  .Sum(kvp => kvp.Value) ?? 0;

            return count switch
            {
                >= 6 => WorkshopTier.Large,
                >= 3 => WorkshopTier.Medium,
                _    => WorkshopTier.Small
            };
        }

        public static (int Width, int Height) GetRequiredDimensions(WorkshopTier tier) => tier switch
        {
            WorkshopTier.Large  => (5, 5),
            WorkshopTier.Medium => (3, 3),
            _                   => (2, 2)
        };
    }

    public enum ClothSlot { Head, Shirt, Pants, Outer, Footwear, Misc }

    public static class ClothingEconomy
    {
        public static readonly Dictionary<Type, ClothSlot> ClothCategoryMap = new()
        {
            // 1. 머리 (Hats)
            { typeof(ChefsToque), ClothSlot.Head }, { typeof(ClothNinjaHood), ClothSlot.Head },
            { typeof(Kasa), ClothSlot.Head }, { typeof(AssassinsCowl), ClothSlot.Head },
            { typeof(MagesHood), ClothSlot.Head }, { typeof(KrampusMinionHat), ClothSlot.Head },
            { typeof(CowlOfTheMaceAndShield), ClothSlot.Head }, { typeof(MagesHoodOfScholarlyInsight), ClothSlot.Head },

            // 2. 상의 (Shirts)
            { typeof(Shirt), ClothSlot.Shirt }, { typeof(FancyShirt), ClothSlot.Shirt },
            { typeof(Doublet), ClothSlot.Shirt }, { typeof(Surcoat), ClothSlot.Shirt },
            { typeof(FormalShirt), ClothSlot.Shirt }, { typeof(JinBaori), ClothSlot.Shirt },
            { typeof(ClothNinjaJacket), ClothSlot.Shirt }, { typeof(Kamishimo), ClothSlot.Shirt },
            { typeof(ElvenShirt), ClothSlot.Shirt }, { typeof(ElvenDarkShirt), ClothSlot.Shirt },

            // 3. 하의 (Pants/Kilt - 가죽 바지 제외)
            { typeof(Kilt), ClothSlot.Pants }, { typeof(FancyKilt), ClothSlot.Pants },
            { typeof(CheckeredKilt), ClothSlot.Pants }, { typeof(GuildedKilt), ClothSlot.Pants },
            { typeof(Hakama), ClothSlot.Pants }, { typeof(TattsukeHakama), ClothSlot.Pants },
            { typeof(Skirt), ClothSlot.Pants }, { typeof(FurSarong), ClothSlot.Pants },

            // 4. 겉옷 (Outer/Robe/Dress)
            { typeof(PlainDress), ClothSlot.Outer }, { typeof(FancyDress), ClothSlot.Outer },
            { typeof(GildedDress), ClothSlot.Outer }, { typeof(FloweredDress), ClothSlot.Outer },
            { typeof(EveningGown), ClothSlot.Outer }, { typeof(Cloak), ClothSlot.Outer },
            { typeof(Robe), ClothSlot.Outer }, { typeof(FurCape), ClothSlot.Outer },
            { typeof(MaleElvenRobe), ClothSlot.Outer }, { typeof(FemaleElvenRobe), ClothSlot.Outer },

            // 5. 신발 (Footwear - 가죽 신발 제외)
            { typeof(NinjaTabi), ClothSlot.Footwear }, { typeof(SamuraiTabi), ClothSlot.Footwear },
            { typeof(JesterShoes), ClothSlot.Footwear }, { typeof(ElvenBoots), ClothSlot.Footwear },
            { typeof(FurBoots), ClothSlot.Footwear },

            // 6. 기타 (Misc/Sash/Apron)
            { typeof(BodySash), ClothSlot.Misc }, { typeof(GargishSash), ClothSlot.Misc },
            { typeof(HalfApron), ClothSlot.Misc }, { typeof(FullApron), ClothSlot.Misc },
            { typeof(Obi), ClothSlot.Misc }, { typeof(WoodlandBelt), ClothSlot.Misc },
            { typeof(OilCloth), ClothSlot.Misc }
        };

        public static int CalculateStressChange(FamilyUnit family)
        {
            if (family.ParentHouse?.HouseWarehouse == null) return 0;
            
            var warehouse = family.ParentHouse.HouseWarehouse;
            Dictionary<ClothSlot, int> slotVariety = new();
            
            foreach (ClothSlot slot in Enum.GetValues<ClothSlot>()) slotVariety[slot] = 0;

            foreach (var key in warehouse.Keys)
            {
                if (ClothCategoryMap.TryGetValue(key.ItemType, out ClothSlot slot)) 
                    slotVariety[slot]++;
            }

            int totalStressChange = 0;
            foreach (int count in slotVariety.Values)
            {
                totalStressChange += count switch { < 3 => 3, 3 => 0, 4 => -1, _ => -2 };
            }
            return totalStressChange;
        }
    }

    public static class FameEconomy
    {
        private static readonly Dictionary<Type, double> m_MasterFameData = new();

        #region 직업별 인테리어 데이터 정의 (Skill / 20.0)

        // 1. 연금술 데이터 정의 (첫 번째 단계)
        private static readonly Dictionary<Type, double> m_AlchemyData = new()
        {
            { typeof(Bottle), 0.0 },               
            { typeof(HairRestylingDeed), 172.5 },  
        };

        private static readonly Dictionary<Type, double> m_BlacksmithyData = new()
        {
            { typeof(LightShipCannonDeed), 65.0 }, 
            { typeof(HeavyShipCannonDeed), 70.0 },
            { typeof(DragonBardingDeed), 172.5 }
        };

        private static readonly Dictionary<Type, double> m_FletchingData = new()
        {
            { typeof(AppleTrunkDeed), 72.5 }, 
            { typeof(PeachTrunkDeed), 72.5 },
            { typeof(CherryBlossomTrunkDeed), 172.5 }
        };

        private static readonly Dictionary<Type, double> m_CarpentryData = new()
        {
            { typeof(FootStool), -14.0 }, 
            { typeof(Stool), -14.0 }, 
            { typeof(BambooChair), -4.0 }, 
            { typeof(WoodenChair), -4.0 },
            { typeof(SmallStretchedHideEastDeed), 10.0 }, 
            { typeof(SmallStretchedHideSouthDeed), 10.0 },
            { typeof(FancyWoodenChairCushion), 17.1 }, 
            { typeof(WoodenChairCushion), 17.1 },
            { typeof(Nightstand), 17.1 },
            { typeof(WoodenBench), 27.6 }, 
            { typeof(WoodenThrone), 27.6 },
            { typeof(SmallBedSouthDeed), 29.7 }, 
            { typeof(SmallBedEastDeed), 29.7 },
            { typeof(MediumStretchedHideEastDeed), 30.0 }, 
            { typeof(MediumStretchedHideSouthDeed), 30.0 },
            { typeof(WritingTable), 38.1 }, 
            { typeof(YewWoodTable), 38.1 },
            { typeof(PlainWoodenShelfSouthDeed), 40.0 }, 
            { typeof(PlainWoodenShelfEastDeed), 40.0 },
            { typeof(FancyWoodenShelfSouthDeed), 40.0 }, 
            { typeof(FancyWoodenShelfEastDeed), 40.0 },
            { typeof(Throne), 48.6 },
            { typeof(TerMurStyleTable), 50.0 }, 
            { typeof(ParrotPerchAddonDeed), 50.0 },
            { typeof(ShortMusicStandLeft), 53.9 }, 
            { typeof(ShortMusicStandRight), 53.9 },
            { typeof(ElegantLowTable), 55.0 }, 
            { typeof(PlainLowTable), 55.0 },
            { typeof(ElvenPodium), 55.0 }, 
            { typeof(OrnateElvenChair), 55.0 }, 
            { typeof(ElvenReadingChair), 55.0 },
            { typeof(FancyElvenTableSouthDeed), 55.0 }, 
            { typeof(FancyElvenTableEastDeed), 55.0 },
            { typeof(ShojiScreen), 55.0 }, 
            { typeof(BambooScreen), 55.0 },
            { typeof(TallMusicStandLeft), 56.5 }, 
            { typeof(TallMusicStandRight), 56.5 },
            { typeof(LargeTable), 59.2 }, 
            { typeof(RusticBenchSouthDeed), 59.7 }, 
            { typeof(RusticBenchEastDeed), 59.7 },
            { typeof(OrnateElvenTableSouthDeed), 60.0 }, 
            { typeof(OrnateElvenTableEastDeed), 60.0 },
            { typeof(BigElvenChair), 60.0 }, 
            { typeof(TerMurStyleChair), 60.0 }, 
            { typeof(UpholsteredChairDeed), 60.0 },
            { typeof(EasleSouth), 61.8 }, 
            { typeof(EasleEast), 61.8 }, 
            { typeof(EasleNorth), 61.8 },
            { typeof(DressformFront), 63.1 }, 
            { typeof(DressformSide), 63.1 },
            { typeof(GargishBanner), 65.0 }, 
            { typeof(GargishCouchEastDeed), 65.0 }, 
            { typeof(GargishCouchSouthDeed), 65.0 },
            { typeof(LongTableSouthDeed), 65.0 }, 
            { typeof(LongTableEastDeed), 65.0 },
            { typeof(LargeBedSouthDeed), 69.7 }, 
            { typeof(LargeBedEastDeed), 69.7 },
            { typeof(FancyLoveseatSouthDeed), 70.0 }, 
            { typeof(FancyLoveseatEastDeed), 70.0 },
            { typeof(FancyCouchSouthDeed), 70.0 }, 
            { typeof(FancyCouchEastDeed), 70.0 },
            { typeof(PlushLoveseatSouthDeed), 70.0 }, 
            { typeof(PlushLoveseatEastDeed), 70.0 },
            { typeof(CelloDeed), 75.0 }, 
            { typeof(WallMountedBellSouthDeed), 75.0 }, 
            { typeof(WallMountedBellEastDeed), 75.0 },
            { typeof(ElvenLoveseatSouthDeed), 80.0 }, 
            { typeof(ElvenLoveseatEastDeed), 80.0 },
            { typeof(MetalTableSouthDeed), 80.0 }, 
            { typeof(MetalTableEastDeed), 80.0 },
            { typeof(LongMetalTableSouthDeed), 80.0 }, 
            { typeof(LongMetalTableEastDeed), 80.0 },
            { typeof(WoodenTableSouthDeed), 80.0 }, 
            { typeof(WoodenTableEastDeed), 80.0 },
            { typeof(LongWoodenTableSouthDeed), 80.0 }, 
            { typeof(LongWoodenTableEastDeed), 80.0 },
            { typeof(GiantReplicaAcorn), 80.0 },
            { typeof(ArcanistStatueSouthDeed), 85.0 }, 
            { typeof(ArcanistStatueEastDeed), 85.0 },
            { typeof(WarriorStatueSouthDeed), 85.0 }, 
            { typeof(WarriorStatueEastDeed), 85.0 },
            { typeof(SquirrelStatueSouthDeed), 85.0 }, 
            { typeof(SquirrelStatueEastDeed), 85.0 },
            { typeof(TrumpetDeed), 85.0 }, 
            { typeof(CowBellDeed), 85.0 },
            { typeof(PlantTapestrySouthDeed), 85.0 }, 
            { typeof(PlantTapestryEastDeed), 85.0 },
            { typeof(MountedDreadHorn), 90.0 },
            { typeof(TallElvenBedSouthDeed), 94.7 }, 
            { typeof(TallElvenBedEastDeed), 94.7 },
            { typeof(ElvenBedSouthDeed), 94.7 }, 
            { typeof(ElvenBedEastDeed), 94.7 },
            { typeof(SmallDisplayCaseSouthDeed), 95.0 }, 
            { typeof(SmallDisplayCaseEastDeed), 95.0 }
        };

        private static readonly Dictionary<Type, double> m_CartographyData = new()
        {
            { typeof(StarChart), 0.0 },               
            { typeof(LocalMap), 10.0 },               
            { typeof(CityMap), 25.0 },                
            { typeof(SeaChart), 35.0 },               
            { typeof(WorldMap), 39.5 },               
            { typeof(EodonianWallMap), 65.0 },        
            { typeof(TatteredWallMapSouth), 90.0 },   
            { typeof(TatteredWallMapEast), 90.0 }     
        };

        private static readonly Dictionary<Type, double> m_CookingData = new()
        {
            { typeof(CoffeeMug), 30.0 },              
            { typeof(BasketOfGreenTeaMug), 30.0 },    
            { typeof(HotCocoaMug), 30.0 },            
            { typeof(ThreeTieredCake), 60.0 }         
        };

        private static readonly Dictionary<Type, double> m_GlassblowingData = new()
        {
            { typeof(SmallFlask), 52.5 }, { typeof(LargeFlask), 60.0 },    
            { typeof(AniRedRibbedFlask), 60.0 }, { typeof(FullVialsWRack), 65.0 }, 
            { typeof(SpinningHourglass), 75.0 }, { typeof(GargoyleFloorMirror), 75.0 }, 
            { typeof(GargoyleWallMirror), 70.0 }      
        };

        private static readonly Dictionary<Type, double> m_InscriptionData = new()
        {
            { typeof(Runebook), 45.0 },               
            { typeof(RecipeBook), 172.5 }             
        };

        private static readonly Dictionary<Type, double> m_MasonryData = new()
        {
            { typeof(Vase), 52.5 },                  
            { typeof(LargeVase), 52.5 },             
            { typeof(AnniversaryVaseTall), 60.0 },   
            { typeof(AnniversaryVaseShort), 60.0 },  
            { typeof(SmallUrn), 82.0 },              
            { typeof(StoneChair), 55.0 },            
            { typeof(MediumStoneTableEastDeed), 65.0 }, 
            { typeof(LargeStoneTableEastDeed), 75.0 },  
            { typeof(RitualTableDeed), 94.7 },       
            { typeof(LargeGargoyleBedSouthDeed), 76.0 }, 
            { typeof(GargishCotSouthDeed), 76.0 },   
            { typeof(StatueGargoyleEast), 54.5 },    
            { typeof(StatueGryphonEast), 54.5 },     
            { typeof(StatueSouth), 60.0 },           
            { typeof(StatuePegasusSouth), 70.0 },    
            { typeof(GargishSculpture), 82.0 },      
            { typeof(GargoylePainting), 83.0 },      
            { typeof(CraftableHouseItem), 60.0 }     
        };

        private static readonly Dictionary<Type, double> m_TailoringData = new()
        {
            { typeof(GozaMatEastDeed), 55.0 },
            { typeof(GozaMatSouthDeed), 55.0 },
            { typeof(SquareGozaMatEastDeed), 55.0 },
            { typeof(SquareGozaMatSouthDeed), 55.0 },
            { typeof(BrocadeGozaMatEastDeed), 55.0 },
            { typeof(BrocadeGozaMatSouthDeed), 55.0 },
            { typeof(BrocadeSquareGozaMatEastDeed), 55.0 },
            { typeof(BrocadeSquareGozaMatSouthDeed), 55.0 },
            { typeof(CurtainsDeed), 172.5 }
        };

        private static readonly Dictionary<Type, double> m_TinkerData = new()
        {
            { typeof(Plate), 0.0 },                  
            { typeof(SpoonLeft), 0.0 },              
            { typeof(SpoonRight), 0.0 },             
            { typeof(ForkLeft), 0.0 },               
            { typeof(ForkRight), 0.0 },              
            { typeof(KnifeLeft), 0.0 },              
            { typeof(KnifeRight), 0.0 },             
            { typeof(PewterMug), 10.0 },             
            { typeof(Goblet), 10.0 },                
            { typeof(Key), 20.0 },                   
            { typeof(Lantern), 30.0 },               
            { typeof(Candelabra), 55.0 },            
            { typeof(Globe), 55.0 },                 
            { typeof(Scales), 60.0 },                
            { typeof(Spyglass), 60.0 },              
            { typeof(HeatingStand), 60.0 },          
            { typeof(DragonLamp), 75.0 },            
            { typeof(StainedGlassLamp), 75.0 },      
            { typeof(TallDoubleLamp), 75.0 },        
            { typeof(WindChimes), 80.0 },            
            { typeof(WeatheredBronzeGlobeSculptureDeed), 85.0 }, 
            { typeof(WeatheredBronzeManOnABenchDeed), 85.0 },    
            { typeof(WeatheredBronzeFairySculptureDeed), 85.0 }, 
            { typeof(WeatheredBronzeArcherDeed), 85.0 },         
            { typeof(MetalLadderDeed), 172.5 }       
        };

        #endregion

        static FameEconomy()
        {
            var allData = new List<Dictionary<Type, double>> 
            { 
                m_AlchemyData, m_BlacksmithyData, m_FletchingData, m_CarpentryData, 
                m_CartographyData, m_CookingData, m_GlassblowingData, m_InscriptionData, 
                m_MasonryData, m_TinkerData, m_TailoringData 
            };

            foreach (var dict in allData)
            {
                foreach (var kvp in dict)
                    m_MasterFameData[kvp.Key] = kvp.Value;
            }
        }

        private static Type GetResourceItemType(Item item)
        {
            if (item == null) return null;

            var prop = item.GetType().GetProperty("Resource");
            if (prop == null) return null;

            var resValue = prop.GetValue(item);
            if (resValue is CraftResource res && res != CraftResource.None)
            {
                var info = CraftResources.GetInfo(res);
                if (info?.ResourceTypes != null && info.ResourceTypes.Length > 0)
                {
                    return info.ResourceTypes[0]; 
                }
            }

            return null;
        }

        public static int GetFameScore(Item item)
        {
            if (item == null || !m_MasterFameData.TryGetValue(item.GetType(), out double baseSkill))
                return 0;

            double score = Math.Max(1.0, baseSkill / 20.0);

            var prop = item.GetType().GetProperty("Resource");
            if (prop != null)
            {
                var resValue = prop.GetValue(item);
                if (resValue is CraftResource res && res != CraftResource.None)
                {
                    int tier = CraftResources.GetIndex(res) + 1;
                    score *= GetTierMultiplier(tier);
                }
            }

            if (IsExceptional(item))
            {
                score *= 1.5;
            }

            return Math.Clamp((int)Math.Round(score), 1, 10);
        }

        public static int GetBaseFameScore(Type type)
        {
            if (type == null || !m_MasterFameData.TryGetValue(type, out double baseSkill))
                return 0;

            double score = Math.Max(1.0, baseSkill / 20.0);
            return Math.Clamp((int)Math.Round(score), 1, 10);
        }

        private static bool IsExceptional(Item item)
        {
            if (item == null) return false;

            if (item is IQuality q)
            {
                return q.Quality == ItemQuality.Exceptional;
            }

            var prop = item.GetType().GetProperty("Quality");
            if (prop != null)
            {
                object val = prop.GetValue(item);
                return (val is int i && i == 2) || val?.ToString() == "Exceptional";
            }

            return false;
        }

        // 🌟 [수정] 9단계 자원 확장에 따른 명예 보정치 9단계 반영
        public static double GetTierMultiplier(int tier)
        {
            return tier switch 
            { 
                2 => 1.2, 
                3 => 1.5, 
                4 => 2.0, 
                5 => 2.5, 
                6 => 2.8, 
                7 => 3.0, 
                8 => 3.5, 
                9 => 4.0, 
                _ => 1.0 
            };
        }

        // 🌟 [신규] 재질별 일일 풍화/마모도(Wear) 반환 (역배열 적용)
        public static int GetDailyWear(StoredItemKey key)
        {
            Type t = key.ItemType;
            CraftResource res = key.Resource;
            
            // 무등급 자원 (유리, 도자기 등)은 고정 피로도 부여
            if (t == typeof(Bottle) || t == typeof(Pitcher) || t == typeof(Glass) || t == typeof(Vase) || 
                t == typeof(LargeVase) || t == typeof(SmallFlask) || t == typeof(LargeFlask) || 
                t == typeof(AniRedRibbedFlask) || t == typeof(FullVialsWRack) || t == typeof(SpinningHourglass) || 
                t == typeof(GargoyleFloorMirror) || t == typeof(GargoyleWallMirror))
            {
                return 150; // 제일 예민하고 잘 깨짐
            }
                
            if (t == typeof(GozaMatEastDeed) || t == typeof(CurtainsDeed) || t == typeof(PlainDress) || t == typeof(Shirt))
            {
                return 50; // 천 재질
            }

            // 등급 자원 (금속, 나무, 가죽, 생선) - 9단계 역배열 룰
            // 티어가 높을수록 관리하기 힘들어 피로도가 높게(10배수) 쌓임
            int tier = CraftResources.GetIndex(res) + 1; 
            if (tier <= 0 || tier > 9) tier = 1;
            
            int wear = tier * 10; 
            return wear < 10 ? 10 : wear;
        }
    }

    public class AncestorRecord
    {
        public string Name { get; set; }
        public NpcJobClass Job { get; set; }
        public NobilityRank HighestRank { get; set; }
        public int DeathAge { get; set; }
        public string CauseOfDeath { get; set; }

        public AncestorRecord(string name, NpcJobClass job, NobilityRank rank, int age, string cause)
        {
            Name = name;
            Job = job;
            HighestRank = rank;
            DeathAge = age;
            CauseOfDeath = cause;
        }
    }

    public class FamilyUnit
    {
        public VirtualCitizen Father { get; set; }
        public VirtualCitizen Mother { get; set; }
        public List<VirtualCitizen> Children { get; set; }
        public long SharedWealth { get; set; } 
        public int Prestige { get; set; }
        
        public FamilyUnit ParentFamily { get; set; } 
        public int DailyExpenses { get; set; }       
        public bool IsActive { get; set; }           

        public VirtualHouse ParentHouse { get; set; }
        public bool IsWillingToSell { get; set; }    

        public FamilyUnit(VirtualCitizen father, VirtualCitizen mother)
        {
            Father = father;
            Mother = mother;
            Children = [];
            SharedWealth = 0;
            IsActive = true;
            IsWillingToSell = false;
        }
    }

    public class VirtualHouse 
    {
        public string HouseName { get; set; }
        public int Prestige { get; set; } 
        public long TotalWealth { get; set; }
        public NobilityRank PrimaryRank { get; set; }
        public List<FamilyUnit> Families { get; set; }
        
        public int ZoneID { get; set; }
        public Dictionary<string, int> Grudges { get; set; } = new(); 
        public int HousingAmbition { get; set; } 
        public NpcJobClass PrimaryJob => Families.FirstOrDefault(f => f.IsActive && f.Father != null)?.Father.JobClass ?? NpcJobClass.Laborer;
        
        // 🌟 [수정] 색자원 딕셔너리로 전면 교체
        public Dictionary<StoredItemKey, int> HouseWarehouse { get; set; }
        
        // 🌟 [신규] 스택 피로도 통 (Damage Pool)
        public Dictionary<StoredItemKey, int> DamagePools { get; set; } = new();

        public int CurrentFameScore { get; set; } = 0;
        public DateTime LastSocialEventTime { get; set; } = DateTime.MinValue; 
        public bool IsHostingEventTonight { get; set; } = false; 
        public int EventFameBonus { get; set; } = 0; 

        public int MaxCapacity { get; set; }
        public Dictionary<Type, int> TargetStockProfile { get; set; } 

        public int Generation { get; set; } 
        public bool IsActive { get; set; }  
        public List<AncestorRecord> AncestorRecords { get; set; } 
        public List<string> RivalHouses { get; set; } 
        
        public int MultiID { get; set; }
        public VirtualEstateSign EstateSign { get; set; } 

        public List<int> OwnedTileIndices { get; set; } 
        public long LandTaxLiability { get; set; }     
        public long PropertyValue { get; set; }          

        public bool HasGarden { get; set; }    
        public bool HasWorkshop { get; set; }  
        public bool HasBarracks { get; set; }  
        public Dictionary<string, int> PlayerGrudges { get; set; }
        
        public Dictionary<Type, int> UnfulfilledNeeds { get; set; } = new();

		public VirtualHouseInterior Interior { get; set; }
		public int SecurityAlertLevel { get; set; }

        public VirtualHouse(string name, NobilityRank rank)
        {
            HouseName = name;
            PrimaryRank = rank;
            Families = [];
            Prestige = 100;
            
            ZoneID = 0;

            HouseWarehouse = new Dictionary<StoredItemKey, int>();
            DamagePools = new Dictionary<StoredItemKey, int>();
            TargetStockProfile = [];
            AncestorRecords = [];
            RivalHouses = [];
            Generation = 1;
            IsActive = true;

            OwnedTileIndices = [];
            LandTaxLiability = 0;
            PropertyValue = 0;

            HasGarden = false;
            HasWorkshop = false;
            HasBarracks = false;

            PlayerGrudges = new Dictionary<string, int>();
            UnfulfilledNeeds = new Dictionary<Type, int>();

            UpdateCapacity();
        }

        // 🌟 [수정] Record 구조체를 활용한 입출고 (티어/품질 동기화)
        public void AlterWarehouseItem(Type type, CraftResource res, bool isExceptional, int amount, int exactScorePerUnit = -1)
        {
            StoredItemKey key = new StoredItemKey(type, res, isExceptional);

            if (!HouseWarehouse.ContainsKey(key)) HouseWarehouse[key] = 0;
            HouseWarehouse[key] += amount;

            if (HouseWarehouse[key] <= 0) HouseWarehouse.Remove(key);

            int score = exactScorePerUnit >= 0 ? exactScorePerUnit : FameEconomy.GetBaseFameScore(type);
            
            if (exactScorePerUnit < 0)
            {
                int tierIndex = CraftResources.GetIndex(res) + 1;
                score = (int)(score * FameEconomy.GetTierMultiplier(tierIndex));
                if (isExceptional) score = (int)(score * 1.5);
            }

            CurrentFameScore += (score * amount);
            if (CurrentFameScore < 0) CurrentFameScore = 0;
        }

        // 🌟 [신규] 상점이 아닌 집에서 직접 먹고 파괴하는 로직 (소비 절벽 방지용)
        public bool ConsumeFoodOrDrink(bool isFood)
        {
            if (HouseWarehouse == null || HouseWarehouse.Count == 0) return false;

            StoredItemKey targetKey = default;
            bool found = false;

            foreach (var kvp in HouseWarehouse)
            {
                Type t = kvp.Key.ItemType;
                if (isFood && (t.IsSubclassOf(typeof(Food)) || t == typeof(Food)))
                {
                    targetKey = kvp.Key;
                    found = true;
                    break;
                }
                else if (!isFood && (t == typeof(BeverageBottle) || t == typeof(Pitcher) || t.IsSubclassOf(typeof(BaseBeverage))))
                {
                    targetKey = kvp.Key;
                    found = true;
                    break;
                }
            }

            if (found)
            {
                AlterWarehouseItem(targetKey.ItemType, targetKey.Resource, targetKey.IsExceptional, -1, -1);
                return true;
            }
            return false;
        }

        // 🌟 [신규] 매일 저녁 만찬 시 작동하는 다이나믹 마모 시스템 (스택 피로도 O(1) 처리)
        public void ConsumeFameItems()
        {
            if (HouseWarehouse == null || HouseWarehouse.Count == 0) return;

            // 순회 중 컬렉션 수정을 피하기 위해 키 복사
            List<StoredItemKey> keys = new List<StoredItemKey>(HouseWarehouse.Keys);

            foreach (var key in keys)
            {
                if (!HouseWarehouse.ContainsKey(key)) continue;

                int baseFame = FameEconomy.GetBaseFameScore(key.ItemType);
                if (baseFame > 0)
                {
                    int amount = HouseWarehouse[key];
                    int dailyWear = FameEconomy.GetDailyWear(key);
                    
                    // 주사위 굴림 (0.8 ~ 1.2배) 다이나믹 풍화치 적용
                    double roll = 0.8 + (Utility.RandomDouble() * 0.4);
                    int addedDamage = (int)((amount * dailyWear) * roll);

                    if (!DamagePools.ContainsKey(key)) DamagePools[key] = 0;
                    DamagePools[key] += addedDamage;

                    // 임계점 10,000을 넘으면 아이템 파괴 처리
                    int broken = DamagePools[key] / 10000;
                    if (broken > 0)
                    {
                        broken = Math.Min(broken, amount); // 실제 가진 양보다 더 부서질 수 없음
                        DamagePools[key] %= 10000; // 남은 피로도 이월

                        AlterWarehouseItem(key.ItemType, key.Resource, key.IsExceptional, -broken, -1);
                        Console.WriteLine($"[Wear&Tear] {HouseName} 가문의 {key.ItemType.Name}({key.Resource}) {broken}개가 낡거나 깨져서 버려졌습니다.");
                    }
                }
            }
        }

        public void UpdateCapacity()
        {
            if (MultiID <= 0) 
            {
                int tentStorage = 145;
                int tentSecures = 72;
                MaxCapacity = (tentSecures * 125) + tentStorage; 
                return;
            }

            var (storage, secures) = GetExactHouseData(MultiID);
            MaxCapacity = (secures * 125) + storage;

            UpdateFacilityBonuses();
        }

        public (int MaxFamilies, int MaxChildren, int RentFee) GetHousingProfile() => MultiID switch
        {
            0 => (1, 0, 0), 
            0x0064 or 0x0066 or 0x0068 or 0x006A or 0x006C or 0x006E => (1, 1, 0), 
            0x00A0 or 0x00A2 => (2, 2, 100), 
            0x0098 or 0x009A or 0x009C or 0x009E or 0x0074 => (3, 2, 250), 
            0x008C or 0x0096 or 0x0076 or 0x0078 => (5, 3, 500), 
            0x007A or 0x007C or 0x007E => (10, 5, 1000), 
            _ => (1, 1, 0)
        };

        private void UpdateFacilityBonuses()
        {
            HasGarden = MultiID > 0 && OwnedTileIndices.Count >= 10;
            HasWorkshop = MultiID is >= 0x00A0 and <= 0x00A2 || MultiID == 0x0074; 
            HasBarracks = MultiID is 0x007A or 0x007C or 0x007E; 
        }

        private (int Storage, int Secures) GetExactHouseData(int multiID) => multiID switch
        {
            0x0064 or 0x0066 or 0x0068 or 0x006A or 0x006C or 0x006E => (580, 290),
            0x00A0 or 0x00A2 => (800, 400),
            0x0098 or 0x009A or 0x009C or 0x009E => (1100, 550),
            0x008C or 0x0074 or 0x0096 or 0x0076 or 0x0078 => (2119, 1059),
            0x007A => (2119, 1059),
            0x007C => (2625, 1312),
            0x007E => (4076, 2038),
            _ => (580, 290)
        };

        public void OnTick(TownEconomy town)
        {
        }
    }
}