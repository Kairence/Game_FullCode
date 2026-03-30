using System;
using Server;
using Server.Items;
using Server.Mobiles; // [추가] PackHorse, PackLlama를 위해 필수
using Server.Multis;  // [추가] BoatDeed 관련

namespace Server.Misc
{
    public static class VirtualJobCore
    {
        // ==============================================================================
        // 🌟 [핵심] 1,100여 개 개별 직업의 정체성을 담은 초정밀 매핑 엔진
        // ==============================================================================
        public static (
            SkillName Skill, 
            NobilityRank MinRank, NobilityRank MaxRank, 
            Type[] Necessities,  // 생존 필수품 (허기 해소)
            Type[] JobMaterials, // 작업 소모품 (도구/재료)
            Type[] Luxuries,     // 사치품 (스트레스 해소)
            Type[] Produces,     // 생산 결과물 
            int BaseQty          // 기본 생산량
        ) GetDeepJobProfile(NpcJobClass job)
        {
            // [교정] 오타 수정: Shepherd, ClothWholesaler, Healer_Master 등
            // [교정] 스킬명: Swords, Macing, Inscribe
            return job switch
            {
                // ==============================================================================
                // 🌾 [100] Peasant: 기초 자원 추출 및 단순 노무 (신분: 평민)
                // ==============================================================================
                NpcJobClass.Pauper => (SkillName.Begging, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Apple)], null, null, null, 0),
                NpcJobClass.Beggar => (SkillName.Begging, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Muffins), typeof(Carrot)], null, null, null, 0),
                NpcJobClass.Laborer => (SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(BreadLoaf)], [typeof(Shoes)], [typeof(Candle)], null, 0),
                NpcJobClass.StreetSweeper => (SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Apple)], [typeof(Boots)], [typeof(Muffins)], [typeof(Bone)], 2),
                NpcJobClass.WaterCarrier => (SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Turnip)], [typeof(Pouch)], [typeof(Sandals)], null, 20),
                NpcJobClass.NightSoilMan => (SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Cabbage)], [typeof(Shovel), typeof(Torch)], [typeof(Candle)], [typeof(FertileDirt)], 5),
                NpcJobClass.GongFarmer => (SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Onion)], [typeof(Shovel), typeof(Boots)], [typeof(Torch)], [typeof(FertileDirt)], 5),
                NpcJobClass.RatCatcher => (SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Apple)], [typeof(Dagger)], [typeof(Bandage)], [typeof(RawRibs)], 3),
                NpcJobClass.ChimneySweep => (SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Pear)], [typeof(Candle), typeof(Bandage)], [typeof(Shoes)], [typeof(GraveDust)], 4),
                NpcJobClass.Lamplighter => (SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Muffins)], [typeof(Torch), typeof(Lantern), typeof(OilFlask)], [typeof(Boots)], null, 0),
                NpcJobClass.LinkBoy => (SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Apple)], [typeof(Torch)], [typeof(Shoes)], null, 0),
                NpcJobClass.GraveDigger_Basic => (SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(BreadLoaf)], [typeof(Shovel), typeof(Torch)], [typeof(BeverageBottle)], [typeof(GraveDust), typeof(Bone)], 5),
                
                // [수정] 요리사(Scullion)가 어부의 TroutRawFishSteak를 사서 TroutFishSteak로 굽도록 변경
                NpcJobClass.Scullion => (SkillName.Cooking, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Muffins)], [typeof(TroutRawFishSteak), typeof(HalfApron), typeof(Skillet)], [typeof(Candle)], [typeof(TroutFishSteak)], 20),
                
                NpcJobClass.GrainFarmer => (SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Apple)], [typeof(Pitchfork)], [typeof(StrawHat)], [typeof(EarOfCorn)], 20),
                NpcJobClass.VegetableFarmer => (SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Pear)], [typeof(Pitchfork)], [typeof(Shoes)], [typeof(Carrot), typeof(Onion), typeof(Cabbage), typeof(Lettuce), typeof(Turnip)], 18),
                NpcJobClass.GourdFarmer => (SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Apple)], [typeof(Shovel)], [typeof(Boots)], [typeof(Pumpkin), typeof(Squash), typeof(GreenGourd), typeof(YellowGourd)], 15),
                NpcJobClass.Orchardist => (SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Muffins)], [typeof(Basket), typeof(Bag)], [typeof(FloppyHat)], [typeof(Apple), typeof(Pear), typeof(Peach)], 25),
                NpcJobClass.CitrusGrower => (SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Muffins)], [typeof(Basket)], [typeof(StrawHat)], [typeof(Lemon), typeof(Lime)], 15),
                NpcJobClass.VineyardWorker => (SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(BreadLoaf)], [typeof(Scissors)], [typeof(Bandana)], [typeof(Grapes)], 20),
                NpcJobClass.BerryPicker => (SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Apple)], [typeof(Pouch)], [typeof(Sandals)], [typeof(ParasiticPlant)], 10),
                NpcJobClass.Herbalist => (SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, 
					[typeof(Pear)], [typeof(Dagger), typeof(Bag)], [typeof(Cap)], 
					[typeof(Garlic), typeof(Ginseng)], 15),
                NpcJobClass.MushroomGatherer => (SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, 
					[typeof(Muffins)], [typeof(Candle), typeof(Pouch)], [typeof(Shoes)], 
					[typeof(MandrakeRoot), typeof(Nightshade), typeof(SpidersSilk)], 8),
                NpcJobClass.Beekeeper => (SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Apple)], [typeof(Torch)], [typeof(HalfApron)], [typeof(JarHoney)], 10),
                
                NpcJobClass.CoastalFisher => (SkillName.Fishing, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Muffins)], [typeof(FishingPole)], [typeof(ThighBoots)], [typeof(Trout)], 15),
                NpcJobClass.DeepSeaFisher_Basic => (SkillName.Fishing, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(BreadLoaf)], [typeof(SpecialFishingNet)], [typeof(TricorneHat)], [typeof(Trout)], 25),
                NpcJobClass.OysterDiver_Basic => (SkillName.Fishing, NobilityRank.Commoner, NobilityRank.Commoner, 
					[typeof(Apple)], [typeof(SkinningKnife)], [typeof(Bandana)], 
					[typeof(BlackPearl)], 5),
                NpcJobClass.SeaweedCollector => (SkillName.Fishing, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Pear)], [typeof(Bag), typeof(Sandals)], [typeof(Candle)], [typeof(FertileDirt)], 8),
                NpcJobClass.BeachComber => (SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Muffins)], [typeof(Spyglass), typeof(Torch)], [typeof(Boots)], [typeof(Bone)], 3),
                NpcJobClass.SaltGatherer => (SkillName.Mining, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Apple)], [typeof(Shovel)], [typeof(Shoes)], [typeof(Sand)], 10), 
                
                NpcJobClass.Shepherd => (SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(BreadLoaf)], [typeof(ShepherdsCrook)], [typeof(StrawHat)], [typeof(Wool)], 15),
                NpcJobClass.Swineherd => (SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Carrot)], [typeof(Cleaver)], [typeof(ShortPants)], [typeof(RawRibs), typeof(Bacon)], 12),
                NpcJobClass.PoultryFarmer => (SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(EarOfCorn)], [typeof(Basket)], [typeof(HalfApron)], [typeof(Eggs), typeof(Feather), typeof(RawBird), typeof(RawChickenLeg)], 18),
                NpcJobClass.CattleDrover => (SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(RawRibs)], [typeof(Whip)], [typeof(Boots)], [typeof(Hides), typeof(RawRibs)], 10),
                NpcJobClass.StableHand => (SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(EarOfCorn)], [typeof(Pitchfork)], [typeof(Shirt)], null, 0),
                NpcJobClass.GooseHerd => (SkillName.Herding, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Muffins)], [typeof(QuarterStaff)], [typeof(Sandals)], [typeof(Feather), typeof(Eggs)], 12),
                
                NpcJobClass.Woodcutter => (SkillName.Lumberjacking, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(BreadLoaf)], [typeof(Axe), typeof(TwoHandedAxe), typeof(Hatchet)], [typeof(Doublet)], [typeof(Log)], 25),
                NpcJobClass.BarkCollector => (SkillName.Lumberjacking, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Muffins)], [typeof(SkinningKnife)], [typeof(Cap)], [typeof(BarkFragment)], 15),
                NpcJobClass.SurfaceMiner => (SkillName.Mining, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Muffins)], [typeof(Pickaxe)], [typeof(Boots)], [typeof(IronOre)], 20),
                NpcJobClass.SandDigger => (SkillName.Mining, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Apple)], [typeof(Shovel), typeof(Bag)], [typeof(Sandals)], [typeof(Sand)], 25),
                NpcJobClass.StoneQuarryman => (SkillName.Mining, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(BreadLoaf)], [typeof(Pickaxe)], [typeof(LeatherGloves)], [typeof(IronOre)], 15),
                NpcJobClass.FlintKnapper => (SkillName.Mining, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Apple)], [typeof(Hammer)], [typeof(Bandana)], [typeof(IronOre)], 5),
                
                NpcJobClass.Trapper => (SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Sausage)], [typeof(SkinningKnife)], [typeof(LeatherCap)], [typeof(Hides), typeof(RawRibs)], 8),
                NpcJobClass.BirdHunter => (SkillName.Archery, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Apple)], [typeof(Bow), typeof(Arrow)], [typeof(TallStrawHat)], [typeof(Feather), typeof(RawBird)], 15),
                NpcJobClass.BigGameHunter => (SkillName.Tactics, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Bacon)], [typeof(Spear), typeof(Bandage)], [typeof(ThighBoots)], [typeof(TigerPelt), typeof(RawRibs), typeof(Bone)], 5),
                NpcJobClass.FeatherPlucker => (SkillName.Camping, NobilityRank.Commoner, NobilityRank.Commoner, 
                    [typeof(Muffins)], [typeof(HalfApron)], [typeof(Candle)], [typeof(Feather)], 20),

                // ==============================================================================
                // 🔨 [200] Producer: 중간재 및 부품 생산 (신분: 평민 ~ 준훈작)
                // ==============================================================================
                NpcJobClass.Smelter => (SkillName.Blacksmith, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(FrenchBread)], [typeof(IronOre), typeof(Tongs)], [typeof(SilverRing)], [typeof(IronIngot)], 15),
                
                // [수정] 대장장이 계열(PigIronWorker)이 철괴를 소모하여 곡괭이(Pickaxe)와 삽(Shovel)도 함께 생산하도록 변경
                NpcJobClass.PigIronWorker => (SkillName.Blacksmith, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(CheesePizza)], [typeof(IronIngot), typeof(SmithHammer)], [typeof(BeverageBottle)], [typeof(PigIron), typeof(Pickaxe), typeof(Shovel)], 15),
                
                NpcJobClass.NailMaker => (SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(BreadLoaf)], [typeof(IronIngot), typeof(Hammer)], [typeof(Shirt)], [typeof(Nails)], 25),
                NpcJobClass.AxleMaker => (SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(Sausage)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(Boots)], [typeof(Axle)], 10),
                NpcJobClass.GearCutter => (SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(Ham)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(SilverBracelet)], [typeof(Gears), typeof(AxleGears)], 10),
                NpcJobClass.SpringMaker => (SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(ApplePie)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(FancyShirt)], [typeof(Springs)], 15),
                NpcJobClass.HingeMaker => (SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(Cookies)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(PlainDress)], [typeof(Hinge)], 20),
                NpcJobClass.ClockPartMaker => (SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(Cake)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(SilverNecklace)], [typeof(ClockParts)], 5),
                NpcJobClass.SextantPartMaker => (SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(JarHoney)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(SilverEarrings)], [typeof(SextantParts)], 5),
                
                NpcJobClass.Weaver => (SkillName.Tailoring, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(FrenchBread)], [typeof(Wool), typeof(Flax), typeof(Cotton)], [typeof(FancyDress)], [typeof(BoltOfCloth), typeof(UncutCloth)], 15),
                NpcJobClass.Spinner => (SkillName.Tailoring, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(Muffins)], [typeof(Wool), typeof(Cotton)], [typeof(PlainDress)], [typeof(SpoolOfThread), typeof(DarkYarn), typeof(LightYarn)], 20),
                NpcJobClass.LeatherTanner => (SkillName.Tailoring, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(RawRibs)], [typeof(Hides), typeof(Scissors), typeof(SewingKit)], [typeof(StuddedGloves)], [typeof(LeatherChest), typeof(LeatherLegs), typeof(LeatherCap), typeof(LeatherGloves)], 8),
                NpcJobClass.Dyer_Producer => (SkillName.Tailoring, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(ApplePie)], [typeof(Dyes), typeof(DyeTub)], [typeof(GoldRing)], [typeof(Shirt), typeof(ShortPants), typeof(PlainDress)], 10),
                
                NpcJobClass.Sawyer => (SkillName.Carpentry, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(BreadLoaf)], [typeof(Log), typeof(Saw)], [typeof(SilverRing)], [typeof(Board)], 25),
                NpcJobClass.ShaftMaker => (SkillName.Carpentry, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(Muffins)], [typeof(Board), typeof(DrawKnife)], [typeof(Bandana)], [typeof(Shaft)], 30),
                NpcJobClass.BarrelMaker_Base => (SkillName.Carpentry, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(Ham)], [typeof(Board), typeof(IronIngot), typeof(DovetailSaw)], [typeof(Boots)], [typeof(Barrel)], 5),
                NpcJobClass.BoxMaker_Base => (SkillName.Carpentry, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(Bacon)], [typeof(Board), typeof(Nails), typeof(JointingPlane)], [typeof(Shoes)], [typeof(WoodenBox), typeof(SmallCrate), typeof(MediumCrate), typeof(LargeCrate), typeof(WoodenChest)], 6),
                
                NpcJobClass.Miller => (SkillName.Cooking, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(Apple)], [typeof(EarOfCorn), typeof(FlourSifter)], [typeof(SilverBracelet)], [typeof(SackFlour), typeof(BowlFlour)], 15),
                NpcJobClass.Butcher_Expert => (SkillName.Cooking, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(RawRibs)], [typeof(RawLambLeg), typeof(RawRibs), typeof(Cleaver), typeof(ButcherKnife)], [typeof(BeverageBottle)], [typeof(Bacon), typeof(Ham), typeof(Sausage)], 20),
                NpcJobClass.PoultryProcessor => (SkillName.Cooking, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(RawBird)], [typeof(RawBird), typeof(ButcherKnife)], [typeof(HalfApron)], [typeof(RawChickenLeg)], 25),
                NpcJobClass.PizzaChef_Producer => (SkillName.Cooking, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(SackFlour)], [typeof(SackFlour), typeof(RollingPin)], [typeof(GoldRing)], [typeof(CheesePizza), typeof(BreadLoaf), typeof(FrenchBread)], 10),
                
                NpcJobClass.GlassBlower => (SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.Baronet, 
                    [typeof(Cake)], [typeof(Sand), typeof(TinkersTools)], [typeof(SilverNecklace)], [typeof(Bottle), typeof(SolventFlask)], 40),
                NpcJobClass.AshProcessor => (SkillName.Alchemy, NobilityRank.Commoner, NobilityRank.Baronet, 
                    [typeof(Muffins)], [typeof(Log), typeof(Torch)], [typeof(Boots)], [typeof(SulfurousAsh)], 20),
                NpcJobClass.BoneGrinder => (SkillName.Alchemy, NobilityRank.Commoner, NobilityRank.Baronet, 
                    [typeof(BreadLoaf)], [typeof(Bone), typeof(MortarPestle)], [typeof(Shoes)], [typeof(GraveDust)], 15),
                NpcJobClass.CandleDipper => (SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.Baronet, 
                    [typeof(ApplePie)], [typeof(JarHoney), typeof(SpoolOfThread)], [typeof(PlainDress)], [typeof(Candle)], 25),
                NpcJobClass.JewelryBaseMaker => (SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.Baronet, 
                    [typeof(FrenchBread)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(SilverRing)], [typeof(GoldRing), typeof(GoldNecklace), typeof(SilverRing), typeof(SilverNecklace)], 5),
                NpcJobClass.BeadMaker => (SkillName.Tinkering, NobilityRank.Commoner, NobilityRank.Baronet, 
                    [typeof(Cookies)], [typeof(IronIngot), typeof(TinkersTools)], [typeof(SilverBracelet)], [typeof(Beads)], 15),

                // ==============================================================================
                // ⚔️ [300] Warrior: 무구 소모 및 전리품 보급 (신분: 기사 ~ 남작)
                // ==============================================================================
                NpcJobClass.Knight => (SkillName.Swords, NobilityRank.Knight, NobilityRank.Baron, 
                    [typeof(FrenchBread), typeof(Ham)], [typeof(PlateChest), typeof(PlateLegs), typeof(MetalKiteShield), typeof(Longsword), typeof(Bandage)], [typeof(GoldRing), typeof(Cloak)], [typeof(Gold), typeof(DragonBlood), typeof(DaemonBone)], 30),
                NpcJobClass.Halberdier => (SkillName.Tactics, NobilityRank.Knight, NobilityRank.Baron, 
                    [typeof(Sausage), typeof(CheesePizza)], [typeof(PlateChest), typeof(Halberd), typeof(Bandage)], [typeof(SilverNecklace)], [typeof(Gold), typeof(DaemonBlood)], 25),
                NpcJobClass.TownGuard => (SkillName.Swords, NobilityRank.Commoner, NobilityRank.SubBaronet, 
                    [typeof(Bacon), typeof(BreadLoaf)], [typeof(ChainChest), typeof(Broadsword), typeof(Bandage)], [typeof(BeverageBottle), typeof(Shoes)], [typeof(Gold), typeof(Hides)], 15),
                NpcJobClass.Duelist => (SkillName.Fencing, NobilityRank.Knight, NobilityRank.Baronet, 
                    [typeof(CookedBird)], [typeof(StuddedChest), typeof(Kryss), typeof(Bandage)], [typeof(GoldEarrings), typeof(FancyShirt)], [typeof(Gold)], 20),
                NpcJobClass.Archer_Expert => (SkillName.Archery, NobilityRank.Knight, NobilityRank.Baronet, 
                    [typeof(Ham), typeof(ApplePie)], [typeof(Bow), typeof(Arrow), typeof(LeatherChest)], [typeof(TricorneHat)], [typeof(Gold), typeof(Feather), typeof(TigerPelt)], 20),
                NpcJobClass.Crossbowman => (SkillName.Archery, NobilityRank.Knight, NobilityRank.Baronet, 
                    [typeof(Sausage)], [typeof(Crossbow), typeof(HeavyCrossbow), typeof(Bolt), typeof(StuddedChest)], [typeof(Boots)], [typeof(Gold), typeof(Bone)], 18),
                NpcJobClass.UndeadHunter => (SkillName.Macing, NobilityRank.Knight, NobilityRank.SubBaron, 
                    [typeof(FrenchBread)], [typeof(Mace), typeof(WarMace), typeof(ChainChest), typeof(Bandage)], [typeof(SilverRing)], [typeof(Gold), typeof(GraveDust), typeof(DaemonBone), typeof(Bone)], 25),
                NpcJobClass.DragonTracker => (SkillName.Tactics, NobilityRank.Knight, NobilityRank.Baron, 
                    [typeof(Ham)], [typeof(Spear), typeof(PlateChest), typeof(GreaterHealPotion)], [typeof(GoldBracelet)], [typeof(Gold), typeof(DragonBlood), typeof(DragonTurtleScute)], 15),

                // ==============================================================================
                // 🧙 [400] Mage: 시약, 스크롤 및 마법 도구 (신분: 준훈작 ~ 자작)
                // ==============================================================================
                NpcJobClass.Wizard => (SkillName.Magery, NobilityRank.SubBaronet, NobilityRank.Viscount, 
                    [typeof(Cake), typeof(FrenchBread)], [typeof(BlackPearl), typeof(Bloodmoss), typeof(BlankScroll), typeof(Spellbook)], [typeof(Sapphire), typeof(Robe)], [typeof(RecallScroll), typeof(FireballScroll), typeof(LightningScroll)], 8),
                NpcJobClass.Archmage => (SkillName.Magery, NobilityRank.Baronet, NobilityRank.Count, 
                    [typeof(Cake), typeof(JarHoney)], [typeof(MandrakeRoot), typeof(SpidersSilk), typeof(SulfurousAsh), typeof(BlankScroll)], [typeof(StarSapphire), typeof(MagicWizardsHat)], [typeof(GateTravelScroll), typeof(EnergyBoltScroll), typeof(ExplosionScroll), typeof(MeteorSwarmScroll)], 5),
                NpcJobClass.Alchemist => (SkillName.Alchemy, NobilityRank.SubBaronet, NobilityRank.Baron, 
                    [typeof(Cookies), typeof(Muffins)], [typeof(Ginseng), typeof(Garlic), typeof(Bottle), typeof(MortarPestle)], [typeof(SilverNecklace)], [typeof(LesserHealPotion), typeof(HealPotion), typeof(GreaterHealPotion), typeof(LesserCurePotion), typeof(CurePotion), typeof(GreaterCurePotion)], 15),
                NpcJobClass.PotionMaker => (SkillName.Alchemy, NobilityRank.SubBaronet, NobilityRank.Baron, 
                    [typeof(ApplePie)], [typeof(Nightshade), typeof(SulfurousAsh), typeof(Bottle), typeof(MortarPestle)], [typeof(SilverRing)], [typeof(LesserPoisonPotion), typeof(PoisonPotion), typeof(GreaterPoisonPotion), typeof(LesserExplosionPotion), typeof(ExplosionPotion), typeof(GreaterExplosionPotion)], 15),
                NpcJobClass.Scribe_Mage => (SkillName.Inscribe, NobilityRank.SubBaronet, NobilityRank.Viscount, 
                    [typeof(CheesePizza)], [typeof(BlankScroll), typeof(ScribesPen), typeof(BlackPearl)], [typeof(Amethyst)], [typeof(Spellbook), typeof(Magerybook), typeof(RecallScroll)], 5),
                NpcJobClass.Necromancer => (SkillName.Magery, NobilityRank.SubBaronet, NobilityRank.Viscount, 
                    [typeof(RawRibs)], [typeof(GraveDust), typeof(BatWing), typeof(DaemonBlood), typeof(BlankScroll)], [typeof(Robe), typeof(SkullCap)], [typeof(NecromancerSpellbook), typeof(PoisonFieldScroll)], 4),

                // ==============================================================================
                // 👑 [500] Noble: 고가 자산 관리 및 하이엔드 소비 (신분: 남작 ~ 후작)
                // ==============================================================================
                NpcJobClass.Mayor => (SkillName.ItemID, NobilityRank.Baron, NobilityRank.Marquis, 
                    [typeof(Cake), typeof(CookedBird)], [typeof(BlankScroll), typeof(ScribesPen)], [typeof(StarSapphire), typeof(Diamond), typeof(GoldBracelet), typeof(Throne)], [typeof(CommissionContractOfEmployment)], 1),
                NpcJobClass.TaxCollector_Noble => (SkillName.ItemID, NobilityRank.Baron, NobilityRank.Marquis, 
                    [typeof(JarHoney)], [typeof(BlankScroll), typeof(ScribesPen)], [typeof(GoldNecklace), typeof(Ruby)], [typeof(VendorRentalContract)], 2),
                NpcJobClass.Aristocrat => (SkillName.ItemID, NobilityRank.Baron, NobilityRank.Marquis, 
                    [typeof(Cake)], [typeof(BlankScroll)], [typeof(Emerald), typeof(Tourmaline), typeof(HairDye), typeof(OrnateElvenChair)], [typeof(ContractOfEmployment)], 1),

                // ==============================================================================
                // 🐫 [600] Merchant: 대량 물류 및 증서 유통 (신분: 기사 ~ 백작)
                // ==============================================================================
                NpcJobClass.CaravanMaster => (SkillName.Camping, NobilityRank.Knight, NobilityRank.Count, 
                    [typeof(FrenchBread), typeof(Ham)], [typeof(PackHorse), typeof(PackLlama), typeof(IronIngot), typeof(Log)], [typeof(GoldRing), typeof(Cloak)], [typeof(CommodityDeed)], 8),
                NpcJobClass.ClothWholesaler => (SkillName.Camping, NobilityRank.Knight, NobilityRank.Count, 
                    [typeof(Sausage)], [typeof(PackHorse), typeof(BoltOfCloth), typeof(SpoolOfThread)], [typeof(TricorneHat)], [typeof(CommodityDeed)], 10),
                NpcJobClass.ArmamentMajor => (SkillName.Camping, NobilityRank.Knight, NobilityRank.Count, 
                    [typeof(Bacon)], [typeof(PackHorse), typeof(Broadsword), typeof(ChainChest)], [typeof(Boots)], [typeof(CommodityDeed)], 5),

                // ==============================================================================
                // ⛪ [700] Religious: 시약, 붕대, 양초 소모 및 구휼 (신분: 평민 ~ 자작)
                // ==============================================================================
                NpcJobClass.Priest => (SkillName.Healing, NobilityRank.Commoner, NobilityRank.Viscount, 
                    [typeof(BreadLoaf)], [typeof(Bandage), typeof(Candle), typeof(Garlic)], [typeof(PlainDress)], [typeof(GraveDust)], 6),
                NpcJobClass.Healer_Master => (SkillName.Healing, NobilityRank.Knight, NobilityRank.Viscount, 
                    [typeof(Apple)], [typeof(Bandage), typeof(Ginseng), typeof(GreaterHealPotion)], [typeof(SilverRing)], [typeof(Bone)], 8),
                NpcJobClass.Gravedigger_Relig => (SkillName.Camping, NobilityRank.Commoner, NobilityRank.Knight, 
                    [typeof(BreadLoaf)], [typeof(Shovel), typeof(Torch)], [typeof(Shoes)], [typeof(GraveDust), typeof(Bone)], 10),

                // ==============================================================================
                // 🎭 [800] Entertainer: 악기 및 유흥/식음료 최종 소모 (신분: 평민 ~ 훈작)
                // ==============================================================================
                NpcJobClass.Bard => (SkillName.Musicianship, NobilityRank.Commoner, NobilityRank.Baronet, 
                    [typeof(CheesePizza)], [typeof(Lute), typeof(LapHarp)], [typeof(FancyShirt), typeof(FeatheredHat)], null, 0),
                NpcJobClass.Drummer => (SkillName.Musicianship, NobilityRank.Commoner, NobilityRank.Baronet, 
                    [typeof(Cookies)], [typeof(Drums), typeof(Tambourine)], [typeof(JesterSuit), typeof(JesterHat)], null, 0),
                NpcJobClass.InnKeeper => (SkillName.Cooking, NobilityRank.Commoner, NobilityRank.Baronet, 
                    [typeof(ApplePie)], [typeof(RawRibs), typeof(SackFlour), typeof(Pitcher)], [typeof(GoldRing)], [typeof(BeverageBottle), typeof(BeverageBottle), typeof(FrenchBread), typeof(Cake)], 30),

                // ==============================================================================
                // ⚓ [900] Maritime: 선박 및 해양 도구 활용 (신분: 평민 ~ 남작)
                // ==============================================================================
                NpcJobClass.Navigator => (SkillName.Cartography, NobilityRank.Knight, NobilityRank.Baron, 
                    [typeof(Trout), typeof(Bacon)], [typeof(Sextant), typeof(BlankMap)], [typeof(Spyglass), typeof(TricorneHat)], [typeof(Trout)], 10),
                NpcJobClass.Shipwright_Master => (SkillName.Carpentry, NobilityRank.Knight, NobilityRank.Baron, 
                    [typeof(FrenchBread)], [typeof(Log), typeof(Board), typeof(Nails)], [typeof(GoldRing)], [typeof(RowBoatDeed)], 1),
                NpcJobClass.DeepSeaFisher => (SkillName.Fishing, NobilityRank.Commoner, NobilityRank.Knight, 
                    [typeof(Trout)], [typeof(SpecialFishingNet)], [typeof(ThighBoots)], [typeof(Trout), typeof(BlackPearl)], 30),

                // ==============================================================================
                // 📚 [1000] Scholar: 지식 자산화 및 행정 기록 (신분: 훈작 ~ 백작)
                // ==============================================================================
                NpcJobClass.Librarian => (SkillName.Inscribe, NobilityRank.Baronet, NobilityRank.Count, 
                    [typeof(Muffins), typeof(Pear)], [typeof(BlankScroll), typeof(ScribesPen)], [typeof(SilverNecklace), typeof(ElvenReadingChair)], [typeof(RedBook), typeof(BlueBook), typeof(TanBook)], 5),
                NpcJobClass.Cartographer_Scholar => (SkillName.Cartography, NobilityRank.Baronet, NobilityRank.Count, 
                    [typeof(CheesePizza)], [typeof(BlankScroll), typeof(MapmakersPen)], [typeof(SilverRing)], [typeof(BlankMap)], 8),

                // ==============================================================================
                // 🥷 [1100] Criminal: 약탈 및 지하 경제 (신분: 평민 ~ 기사)
                // ==============================================================================
                NpcJobClass.Thief => (SkillName.Stealing, NobilityRank.Commoner, NobilityRank.Knight, 
                    [typeof(RawLambLeg), typeof(BeverageBottle)], [typeof(Dagger)], [typeof(Bandana), typeof(SkullCap)], [typeof(Gold), typeof(GoldRing), typeof(SilverRing)], 10),
                NpcJobClass.Assassin => (SkillName.Poisoning, NobilityRank.Commoner, NobilityRank.Knight, 
					[typeof(Sausage)], [typeof(Dagger)], [typeof(Cloak)], 
					[typeof(Bloodmoss), typeof(Nightshade), typeof(Gold)], 10),

                // ==============================================================================
                // 미분류 예외 처리
                // ==============================================================================
                _ => (SkillName.Camping, NobilityRank.Commoner, NobilityRank.Marquis, [typeof(BreadLoaf)], null, null, null, 0)
            };
        }
    }
}