using System;
using System.Collections.Generic;
using Server;
using Server.Items;      
using Server.Mobiles;    
using Server.Engines.Craft; 
using System.Linq;

namespace Server.Misc
{
	// 창고 등급 정의 (공간 점유 및 AI 구매 가이드용)
    public enum StorageTier { None, Small, Medium, Large, Special }

    public static class StorageEconomy
    {
        // 1. [재봉술 영역] 아이템 종류(Variety) 확장 데이터
        // 파우치나 백팩이 많을수록 더 다양한 종류의 생필품을 보관할 수 있습니다.
        private static readonly Dictionary<Type, int> VarietyBonusMap = new()
        {
            // 기본 종류 5종
            { typeof(Pouch), 5 },        // 종류 한도 +5
            { typeof(Bag), 10 },         // 종류 한도 +10
            { typeof(Backpack), 20 }     // 종류 한도 +20. 업그레이드 개념임.
        };

        // 2. [목공술 영역] 아이템 총 수량(Total Count) 확장 데이터
        // 상자가 클수록 쌓아둘 수 있는 물건의 절대적인 양이 늘어납니다.
        private static readonly Dictionary<Type, int> QuantityBonusMap = new()
        {
            { typeof(WoodenBox), 50 },       // 총 수량 +50
            { typeof(SmallCrate), 100 },     // 총 수량 +100
            { typeof(MediumCrate), 200 },    // 총 수량 +200
            { typeof(LargeCrate), 300 },     // 총 수량 +300
            { typeof(WoodenChest), 500 },    // 총 수량 +500
            { typeof(MetalChest), 1000 }     // 총 수량 +1000 (특급 창고용 고급 상자)
        };

        /// <summary>
        /// 가문의 현재 보관함 구성에 따른 창고 제한치를 계산합니다.
        /// </summary>
        /// <returns>(최대 종류 수, 최대 아이템 수량)</returns>
        public static (int MaxTypes, int MaxQuantity) GetStorageLimits(Dictionary<Type, int> warehouse)
        {
            int maxTypes = 5;       // 창고 없음: 기본 5종 (생필품 위주)
            int maxQuantity = 5;    // 창고 없음: 기본 5개 물품

            if (warehouse == null) return (maxTypes, maxQuantity);

            foreach (var kvp in warehouse)
            {
                // 재봉술 아이템 체크: 종류 다양성 확보
                if (VarietyBonusMap.TryGetValue(kvp.Key, out int vBonus))
                {
                    maxTypes += (vBonus * kvp.Value);
                }

                // 목공술 아이템 체크: 수량 한도 확보
                if (QuantityBonusMap.TryGetValue(kvp.Key, out int qBonus))
                {
                    maxQuantity += (qBonus * kvp.Value);
                }
            }

            return (maxTypes, maxQuantity);
        }

        /// <summary>
        /// 창고 등급별 요구 2차원 공간 (Width, Height)을 반환합니다.
        /// </summary>
        public static (int Width, int Height) GetRequiredDimensions(StorageTier tier) => tier switch
        {
            StorageTier.Special => (10, 10), // 특급: 100칸 점유
            StorageTier.Large   => (4, 4),   // 대형: 16칸 점유
            StorageTier.Medium  => (2, 2),   // 중형: 4칸 점유
            StorageTier.Small   => (1, 1),   // 소형: 1칸 점유
            _ => (0, 0)
        };
    }


	public enum WorkshopBonusType { SuccessRate, ExceptionalChance, ResourceSave }
	public enum WorkshopTier { Small, Medium, Large }

    public static class WorkshopEconomy
    {
        /// <summary>
        /// [공방 에드온 마스터 데이터]
        /// 각 에드온이 어떤 기술에 어떤 보너스를 주는지 정의합니다.
        /// </summary>
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
			{ typeof(SmithingPressDeed), (SkillName.Blacksmith, 0.10, WorkshopBonusType.ExceptionalChance) }, // [신규] 프레스 품질 보너스
			{ typeof(StoneAnvilEastDeed), (SkillName.Blacksmith, 0.06, WorkshopBonusType.ExceptionalChance) },
			{ typeof(StoneAnvilSouthDeed), (SkillName.Blacksmith, 0.06, WorkshopBonusType.ExceptionalChance) },

			// --- 3. 재봉술 (Tailoring) 관련 ---
			{ typeof(LoomEastDeed), (SkillName.Tailoring, 0.07, WorkshopBonusType.SuccessRate) },
			{ typeof(LoomSouthDeed), (SkillName.Tailoring, 0.07, WorkshopBonusType.SuccessRate) },
			{ typeof(SpinningwheelEastDeed), (SkillName.Tailoring, 0.05, WorkshopBonusType.ResourceSave) },
			{ typeof(SpinningwheelSouthDeed), (SkillName.Tailoring, 0.05, WorkshopBonusType.ResourceSave) },
			{ typeof(ElvenSpinningwheelEastDeed), (SkillName.Tailoring, 0.06, WorkshopBonusType.ResourceSave) },
			{ typeof(SewingMachineDeed), (SkillName.Tailoring, 0.10, WorkshopBonusType.SuccessRate) }, //
			{ typeof(SewingMachine), (SkillName.Tailoring, 0.10, WorkshopBonusType.SuccessRate) },     //

			// --- 4. 기록술 (Inscription) 관련 ---
			{ typeof(WritingDeskDeed), (SkillName.Inscribe, 0.10, WorkshopBonusType.SuccessRate) },
			{ typeof(WritingTable), (SkillName.Inscribe, 0.05, WorkshopBonusType.SuccessRate) },

			// --- 5. 땜질 (Tinkering) 관련 ---
			{ typeof(TinkerBenchDeed), (SkillName.Tinkering, 0.05, WorkshopBonusType.SuccessRate) },

			// --- 6. 목공 (Carpentry) 관련 ---
			{ typeof(WoodworkersBenchDeed), (SkillName.Carpentry, 0.05, WorkshopBonusType.SuccessRate) },
			{ typeof(SpinningLatheDeed), (SkillName.Carpentry, 0.05, WorkshopBonusType.SuccessRate) }, // [신규] 선반
			{ typeof(RitualTableDeed), (SkillName.Carpentry, 0.10, WorkshopBonusType.ExceptionalChance) }, // 석공술 정점

			// --- 7. 연금술 및 유리세공 (Alchemy/Glassblowing) 관련 ---
			{ typeof(AlchemyStationDeed), (SkillName.Alchemy, 0.10, WorkshopBonusType.SuccessRate) },
			{ typeof(HeatingStand), (SkillName.Alchemy, 0.05, WorkshopBonusType.SuccessRate) }, //

			// --- 8. 활 제작 (Bowcraft) 관련 ---
			{ typeof(FletchingStationDeed), (SkillName.Fletching, 0.10, WorkshopBonusType.SuccessRate) }
		};

        /// <summary>
        /// 특정 기술에 대한 가문의 최종 공방 보너스를 산출합니다. (티어 배율 적용)
        /// </summary>
        public static double GetFinalBonus(Dictionary<Type, int> warehouse, SkillName skill, WorkshopBonusType type)
        {
            if (warehouse == null) return 0.0;

            // 1. 해당 기술과 연관된 에드온들의 기본 보너스 합산 및 개수 파악
            double baseSum = 0.0;
            int addonCount = 0;

            foreach (var kvp in warehouse)
            {
                if (AddonBonusMap.TryGetValue(kvp.Key, out var data) && data.Skill == skill)
                {
                    addonCount += kvp.Value; // 에드온 보유 수량 합산
                    if (data.Type == type)
                    {
                        baseSum += (data.Bonus * kvp.Value);
                    }
                }
            }

            // 2. 전문가님 기획 티어 배율 적용
            // 중형(3개 이상): 1.5배, 대형(6개 이상): 2.0배
            double multiplier = addonCount switch
            {
                >= 6 => 2.0,
                >= 3 => 1.5,
                _ => 1.0
            };

            return baseSum * multiplier;
        }

        /// <summary>
        /// 현재 가문의 특정 기술 공방 등급을 반환합니다.
        /// </summary>
        public static WorkshopTier GetTier(Dictionary<Type, int> warehouse, SkillName skill)
		{
			// warehouse가 null일 경우를 대비해 null 허용 연산자(?.) 및 대체값(??) 사용
			int count = warehouse?.Where(kvp => AddonBonusMap.ContainsKey(kvp.Key) && AddonBonusMap[kvp.Key].Skill == skill)
								  .Sum(kvp => kvp.Value) ?? 0;

			return count switch
			{
				>= 6 => WorkshopTier.Large,
				>= 3 => WorkshopTier.Medium,
				_    => WorkshopTier.Small
			};
		}
		/// <summary>
        /// 공방 등급별 요구 2차원 공간 (Width, Height)을 반환합니다.
        /// </summary>
        public static (int Width, int Height) GetRequiredDimensions(WorkshopTier tier) => tier switch
        {
            WorkshopTier.Large  => (5, 5),   // 대형: 25칸 점유
            WorkshopTier.Medium => (3, 3),   // 중형: 9칸 점유 (수정됨)
            _                   => (2, 2)    // 소형: 4칸 점유
        };
	}
		
	
	// 6개 필수 의류 부위 정의
    public enum ClothSlot { Head, Shirt, Pants, Outer, Footwear, Misc }

    public static class ClothingEconomy //각 부위당 스트레스 체크. 3벌 이하면 각 1점씩 증가(0벌이면 3점. 6부위면 총 스트레스 18점 증가). 한 부위당 4벌 이상이면 스트레스 1점 감소. 최대 한 부위당 2점(5벌)
    {
        // 천 장비 카테고리 맵 (가죽 제외)
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

        /// <summary>
        /// 가문의 의류 다양성에 따른 스트레스 변동 수치를 계산합니다.
        /// </summary>
        public static int CalculateStressChange(FamilyUnit family)
		{
			// 집이 없거나 창고가 없는 경우 방어 코드
			if (family.ParentHouse?.HouseWarehouse == null) return 0;
			
			var warehouse = family.ParentHouse.HouseWarehouse;
			Dictionary<ClothSlot, int> slotVariety = new();
			
			foreach (ClothSlot slot in Enum.GetValues<ClothSlot>()) slotVariety[slot] = 0;

			foreach (var type in warehouse.Keys)
			{
				if (ClothCategoryMap.TryGetValue(type, out ClothSlot slot)) 
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

	public static class FameEconomy //요구 명성당 해당 점수 이상의 가구가 있어야 함
    {
        // 런타임에 9,000명의 NPC가 실제로 조회할 통합 마스터 딕셔너리
        private static readonly Dictionary<Type, double> m_MasterFameData = new();

        #region 직업별 인테리어 데이터 정의 (Skill / 20.0)

        // 1. 연금술 데이터 정의 (첫 번째 단계)
        private static readonly Dictionary<Type, double> m_AlchemyData = new()
        {
            { typeof(Bottle), 0.0 },               // 0.0 / 20 = 0 -> 최소 1점 보정
            { typeof(HairRestylingDeed), 172.5 },  // 172.5 / 20 = 8.6 -> 9점
        };

		private static readonly Dictionary<Type, double> m_BlacksmithyData = new()
		{
			// [비장비 및 장식용 소품]
			// 공식: (BaseMinSkill / 20.0), 최소 1점 보장
			
			// 대형 함포 증서 (Skill 65.0 ~ 70.0) -> 약 3.5점 (반올림 4점)
			// 가문의 무력을 과시하는 장식용 에드온으로 분류 가능
			{ typeof(LightShipCannonDeed), 65.0 }, 
			{ typeof(HeavyShipCannonDeed), 70.0 },

			// 드래곤 마갑 증서 (Skill 172.5) -> 8.6점 (반올림 9점)
			// 가문의 마구간이나 로비에 전시하는 최고급 사치품/장식으로 분류
			{ typeof(DragonBardingDeed), 172.5 }
		};

		private static readonly Dictionary<Type, double> m_FletchingData = new()
		{
			// [비장비 및 장식용 소품]
			// 공식: (BaseMinSkill / 20.0), 최소 1점 보장
			
			// 사과나무 밑동 증서 (Skill 72.5) -> 3.6점 (반올림 4점)
			{ typeof(AppleTrunkDeed), 72.5 }, 

			// 복숭아나무 밑동 증서 (Skill 72.5) -> 3.6점 (반올림 4점)
			{ typeof(PeachTrunkDeed), 72.5 },

			// 벚꽃나무 밑동 증서 (Skill 172.5) -> 8.6점 (반올림 9점)
			// 활 제작에서 만들 수 있는 최고급 조경/인테리어 아이템입니다.
			{ typeof(CherryBlossomTrunkDeed), 172.5 }
		};

		private static readonly Dictionary<Type, double> m_CarpentryData = new()
		{
			// [1점 구간] 기초 가구 및 소품 (Skill 0.0 미만 ~ 17.1 미만)
			{ typeof(FootStool), -14.0 }, 
			{ typeof(Stool), -14.0 }, 
			{ typeof(BambooChair), -4.0 }, 
			{ typeof(WoodenChair), -4.0 },
			{ typeof(SmallStretchedHideEastDeed), 10.0 }, 
			{ typeof(SmallStretchedHideSouthDeed), 10.0 },

			// [1점 후반 ~ 2점 구간] 일반 가구 및 장식 (Skill 17.1 ~ 40.0 미만)
			{ typeof(FancyWoodenChairCushion), 17.1 }, 
			{ typeof(WoodenChairCushion), 17.1 },
			{ typeof(Nightstand), 17.1 },
			{ typeof(WoodenBench), 27.6 }, 
			{ typeof(WoodenThrone), 27.6 },
			//{ typeof(CraftableItemType.DarkWoodenSignHanger), 27.7 }, 
			//{ typeof(CraftableItemType.LightWoodenSignHanger), 27.7 },
			{ typeof(SmallBedSouthDeed), 29.7 }, 
			{ typeof(SmallBedEastDeed), 29.7 },
			{ typeof(MediumStretchedHideEastDeed), 30.0 }, 
			{ typeof(MediumStretchedHideSouthDeed), 30.0 },
			{ typeof(WritingTable), 38.1 }, 
			{ typeof(YewWoodTable), 38.1 },

			// [2점 후반 ~ 3점 구간] 중급 가구 및 소품 (Skill 40.0 ~ 60.0 미만)
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

			// [3점 후반 ~ 4점 구간] 상급 가구 및 예술품 (Skill 60.0 ~ 80.0 미만)
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

			// [4점 후반 ~ 5점] 최고급 품위 유지 아이템 (Skill 80.0 ~ 100.0)
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
			// [벽걸이 지도 및 도표]
			{ typeof(StarChart), 0.0 },               // 0 -> 1점 (최소 점수)
			{ typeof(LocalMap), 10.0 },               // 0.5 -> 1점
			{ typeof(CityMap), 25.0 },                // 1.25 -> 1점
			{ typeof(SeaChart), 35.0 },               // 1.75 -> 2점
			{ typeof(WorldMap), 39.5 },               // 1.97 -> 2점
			{ typeof(EodonianWallMap), 65.0 },        // 3.25 -> 3점
			{ typeof(TatteredWallMapSouth), 90.0 },   // 4.5 -> 5점
			{ typeof(TatteredWallMapEast), 90.0 }     // 4.5 -> 5점
		};

		private static readonly Dictionary<Type, double> m_CookingData = new()
		{
			// [식탁 장식용 품목]
			{ typeof(CoffeeMug), 30.0 },              // 1.5 -> 2점
			{ typeof(BasketOfGreenTeaMug), 30.0 },    // 1.5 -> 2점
			{ typeof(HotCocoaMug), 30.0 },            // 1.5 -> 2점
			{ typeof(ThreeTieredCake), 60.0 }         // 3점 (화려한 장식 효과)
		};

		private static readonly Dictionary<Type, double> m_GlassblowingData = new()
		{
			{ typeof(SmallFlask), 52.5 }, { typeof(LargeFlask), 60.0 },    // 2.6~3점
			{ typeof(AniRedRibbedFlask), 60.0 }, { typeof(FullVialsWRack), 65.0 }, // 3~3.25점
			{ typeof(SpinningHourglass), 75.0 }, { typeof(GargoyleFloorMirror), 75.0 }, // 3.75점
			{ typeof(GargoyleWallMirror), 70.0 }      // 3.5점
		};

		private static readonly Dictionary<Type, double> m_InscriptionData = new()
		{
			{ typeof(Runebook), 45.0 },               // 2.25점
			{ typeof(RecipeBook), 172.5 }             // 8.6점
		};

		private static readonly Dictionary<Type, double> m_MasonryData = new()
		{
			// [석조 장식 및 꽃병]
			{ typeof(Vase), 52.5 },                  // 2.6점
			{ typeof(LargeVase), 52.5 },             // 2.6점
			{ typeof(AnniversaryVaseTall), 60.0 },   // 3.0점
			{ typeof(AnniversaryVaseShort), 60.0 },  // 3.0점
			{ typeof(SmallUrn), 82.0 },              // 4.1점

			// [석조 가구 및 침대]
			{ typeof(StoneChair), 55.0 },            // 2.7점
			{ typeof(MediumStoneTableEastDeed), 65.0 }, // 3.2점
			{ typeof(LargeStoneTableEastDeed), 75.0 },  // 3.7점
			{ typeof(RitualTableDeed), 94.7 },       // 4.7점
			{ typeof(LargeGargoyleBedSouthDeed), 76.0 }, // 3.8점
			{ typeof(GargishCotSouthDeed), 76.0 },   // 3.8점

			// [조각상 및 예술품]
			{ typeof(StatueGargoyleEast), 54.5 },    // 2.7점
			{ typeof(StatueGryphonEast), 54.5 },     // 2.7점
			{ typeof(StatueSouth), 60.0 },           // 3.0점
			{ typeof(StatuePegasusSouth), 70.0 },    // 3.5점
			{ typeof(GargishSculpture), 82.0 },      // 4.1점
			{ typeof(GargoylePainting), 83.0 },      // 4.1점

			// [건축 장식 (Walls & Floors)]
			{ typeof(CraftableHouseItem), 60.0 }     // 3.0점 (벽, 계단, 바닥재 공통)
		};

		private static readonly Dictionary<Type, double> m_TailoringData = new()
		{
			// [바닥재 및 매트류]
			// 공식: (BaseMinSkill / 20.0), 1점 미만은 1점 보정
			
			// 고자 매트 (Skill 55.0) -> 2.75점 (반올림 시 3점)
			{ typeof(GozaMatEastDeed), 55.0 },
			{ typeof(GozaMatSouthDeed), 55.0 },
			{ typeof(SquareGozaMatEastDeed), 55.0 },
			{ typeof(SquareGozaMatSouthDeed), 55.0 },

			// 브로케이드 고자 매트 (Skill 55.0) -> 2.75점
			{ typeof(BrocadeGozaMatEastDeed), 55.0 },
			{ typeof(BrocadeGozaMatSouthDeed), 55.0 },
			{ typeof(BrocadeSquareGozaMatEastDeed), 55.0 },
			{ typeof(BrocadeSquareGozaMatSouthDeed), 55.0 },

			// [창문 및 벽면 장식]
			// 커튼 증서 (Skill 172.5) -> 8.6점 (반올림 시 9점)
			// 재봉술에서 제작 가능한 최고급 인테리어 아이템입니다.
			{ typeof(CurtainsDeed), 172.5 }
		};

		private static readonly Dictionary<Type, double> m_TinkerData = new()
		{
			// [기초 식기 및 잡화] - 1점 보정 구간 (Skill 0.0 ~ 20.0)
			{ typeof(Plate), 0.0 },                  // 1점
			{ typeof(SpoonLeft), 0.0 },              // 1점
			{ typeof(SpoonRight), 0.0 },             // 1점
			{ typeof(ForkLeft), 0.0 },               // 1점
			{ typeof(ForkRight), 0.0 },              // 1점
			{ typeof(KnifeLeft), 0.0 },              // 1점
			{ typeof(KnifeRight), 0.0 },             // 1점
			{ typeof(PewterMug), 10.0 },             // 1점
			{ typeof(Goblet), 10.0 },                // 1점
			{ typeof(Key), 20.0 },                   // 1점

			// [생활 및 장식 소품] (Skill 30.0 ~ 60.0)
			{ typeof(Lantern), 30.0 },               // 1.5점
			{ typeof(Candelabra), 55.0 },            // 2.75점
			{ typeof(Globe), 55.0 },                 // 2.75점
			{ typeof(Scales), 60.0 },                // 3.0점
			{ typeof(Spyglass), 60.0 },              // 3.0점
			{ typeof(HeatingStand), 60.0 },          // 3.0점

			// [고급 조명 및 예술 장식품] (Skill 75.0 ~ 85.0)
			{ typeof(DragonLamp), 75.0 },            // 3.75점
			{ typeof(StainedGlassLamp), 75.0 },      // 3.75점
			{ typeof(TallDoubleLamp), 75.0 },        // 3.75점
			{ typeof(WindChimes), 80.0 },            // 4.0점
			{ typeof(WeatheredBronzeGlobeSculptureDeed), 85.0 }, // 4.25점
			{ typeof(WeatheredBronzeManOnABenchDeed), 85.0 },    // 4.25점
			{ typeof(WeatheredBronzeFairySculptureDeed), 85.0 }, // 4.25점
			{ typeof(WeatheredBronzeArcherDeed), 85.0 },         // 4.25점

			// [최고급 특수 장식]
			{ typeof(MetalLadderDeed), 172.5 }       // 8.6점 (실내 장식용 사다리)
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

			// 1. 리플렉션으로 'Resource' 속성 추출 (장비/비장비 통합)
			var prop = item.GetType().GetProperty("Resource");
			if (prop == null) return null;

			// 2. CraftResource 값(Enum) 획득
			var resValue = prop.GetValue(item);
			if (resValue is CraftResource res && res != CraftResource.None)
			{
				// 3. 유저 제공 참조 코드: ResourceTypes[0]을 통해 실제 원자재 타입 반환
				var info = CraftResources.GetInfo(res);
				if (info?.ResourceTypes != null && info.ResourceTypes.Length > 0)
				{
					return info.ResourceTypes[0]; // 예: typeof(VeriteIngot)
				}
			}

			return null;
		}
		public static int GetFameScore(Item item)
		{
			if (item == null || !m_MasterFameData.TryGetValue(item.GetType(), out double baseSkill))
				return 0;

			double score = Math.Max(1.0, baseSkill / 20.0);

			// 1. 재질 티어 보너스 (이미 검증된 통합 로직)
			Type resourceType = GetResourceItemType(item);
			if (resourceType != null)
			{
				int tier = VirtualTradeAI.GetResourceTierValue(resourceType);
				score *= GetTierMultiplier(tier);
			}

			// 2. 품질 보너스 (통합 체크 함수 호출)
			if (IsExceptional(item))
			{
				score *= 1.5;
			}

			return Math.Clamp((int)Math.Round(score), 1, 10);
		}

		/// <summary>
		/// 장비/비장비 구분 없이 해당 아이템이 'Exceptional' 품질인지 체크합니다.
		/// </summary>
		private static bool IsExceptional(Item item)
		{
			if (item == null) return false;

			// 1. 우선 서버 표준 인터페이스(IQuality)가 있는지 체크
			if (item is IQuality q)
			{
				return q.Quality == ItemQuality.Exceptional;
			}

			// 2. 인터페이스가 없는 일반 아이템(접시 등)은 리플렉션으로 'Quality' 속성을 체크
			// 보통 제작 시스템에서 quality는 int(2) 또는 ItemQuality Enum으로 저장됩니다.
			var prop = item.GetType().GetProperty("Quality");
			if (prop != null)
			{
				object val = prop.GetValue(item);
				
				// 정수형(2 = Exceptional)이거나 Enum 문자열이 Exceptional인 경우 true
				return (val is int i && i == 2) || val?.ToString() == "Exceptional";
			}

			return false;
		}

		private static double GetTierMultiplier(int tier)
		{
			return tier switch { 2 => 1.2, 3 => 1.5, 4 => 2.0, 5 => 2.5, 6 => 2.8, 7 => 3.0, _ => 1.0 };
		}
    }

    // 1. [신규] 족보 및 역사에 남을 선조 기록
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
        
        // [신규] 가계도 추적 및 생존 지표
        public FamilyUnit ParentFamily { get; set; } // 본가(독립 전 가족) 추적용
        public int DailyExpenses { get; set; }       // 이 가족이 하루에 소모하는 고정 생활비
        public bool IsActive { get; set; }           // 가족의 존속 여부 (사망/독립 시 false)

		public VirtualHouse ParentHouse { get; set; }

        // ====================================================================
        // [기획 추가] 부동산 매매 플래그
        // ====================================================================
        public bool IsWillingToSell { get; set; }    // 재정 상태에 따라 땅을 팔 의사가 있는지 여부

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

    // NPC 가문을 위한 순수 독자 시스템
    public class VirtualHouse 
    {
        public string HouseName { get; set; }
        public int Prestige { get; set; } 
        public long TotalWealth { get; set; }
        public NobilityRank PrimaryRank { get; set; }
        public List<FamilyUnit> Families { get; set; }
        
        // [신규] 물류 및 가문 창고 시스템
        public Dictionary<Type, int> HouseWarehouse { get; set; }
        public int MaxCapacity { get; set; }
        public Dictionary<Type, int> TargetStockProfile { get; set; } // 목표 비축량 및 구매 욕구

        // [신규] 역사, 파벌 및 상태 시스템
        public int Generation { get; set; } // 현재 몇 대째인지 추적
        public bool IsActive { get; set; }  // 몰락 여부
        public List<AncestorRecord> AncestorRecords { get; set; } // 선조 업적 기록
        public List<string> RivalHouses { get; set; } // 전쟁/적대 파벌 목록

        // ====================================================================
        // [기획 추가] 지정학적 영토 및 부동산 경제 변수
        // ====================================================================
        public List<int> OwnedTileIndices { get; set; } // 소유 중인 영토 타일 번호들
        public long LandTaxLiability { get; set; }      // 매일 납부해야 할 예상 토지세액
        public long PropertyValue { get; set; }         // 소유한 땅의 현재 시장 가치 합계

        // ====================================================================
        // [신규] 영토 규모별 부속 건물 (Estate Sub-systems)
        // ====================================================================
        public bool HasGarden { get; set; }    // 텃밭 (식량 자급자족)
        public bool HasWorkshop { get; set; }  // 공방 (생산 효율 및 명성 보너스)
        public bool HasBarracks { get; set; }  // 병영 (전쟁 승률 보너스)

        public VirtualHouse(string name, NobilityRank rank)
        {
            HouseName = name;
            PrimaryRank = rank;
            Families = [];
            Prestige = 100;

            HouseWarehouse = [];
            TargetStockProfile = [];
            AncestorRecords = [];
            RivalHouses = [];
            Generation = 1;
            IsActive = true;

            OwnedTileIndices = [];
            LandTaxLiability = 0;
            PropertyValue = 0;

            // [신규] 건물 초기화
            HasGarden = false;
            HasWorkshop = false;
            HasBarracks = false;

            UpdateCapacity();
        }

        // 작위에 따른 창고 최대 한도 자동 갱신 (기획된 세부 스케일 적용)
        // 작위에 따른 창고 최대 한도 자동 갱신
        public void UpdateCapacity()
        {
            MaxCapacity = PrimaryRank switch
            {
                NobilityRank.Commoner => 400,     // [수정] 100 -> 400 (기본 백팩)
                NobilityRank.Knight => 1000,
                NobilityRank.SubBaronet => 2000,
                NobilityRank.Baronet => 3000,
                NobilityRank.SubBaron => 4000,
                NobilityRank.Baron => 5000,
                NobilityRank.Viscount => 6500,
                NobilityRank.Count => 8000,
                NobilityRank.Marquis => 10000,
                _ => 400
            };
        }

        public void OnTick(TownEconomy town)
        {
            // 가문 단위의 순수 경제/정치 활동 연산
        }
    }
}