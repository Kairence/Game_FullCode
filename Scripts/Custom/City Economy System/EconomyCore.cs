using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
    public enum NpcJobClass
    {
        // --- [100] Peasant: 기초 자원 추출 및 단순 노무 (Raw Materials) ---
        Pauper = 100, Laborer, StreetSweeper, WaterCarrier, NightSoilMan, RatCatcher, Beggar, ChimneySweep, Lamplighter, LinkBoy, GraveDigger_Basic, Scullion, Messenger_Foot, KennelMaid, GongFarmer, GrainFarmer, VegetableFarmer, GourdFarmer, Orchardist, CitrusGrower, VineyardWorker, BerryPicker, Herbalist, MushroomGatherer, Beekeeper, CoastalFisher, DeepSeaFisher_Basic, Crabber, OysterDiver_Basic, SeaweedCollector, BeachComber, SaltGatherer, Mudlarker, Shepherd, Swineherd, PoultryFarmer, CattleDrover, StableHand, DairyWorker, GooseHerd, DonkeyDriver, HorseGroom_Basic, Woodcutter, BarkCollector, ResinGatherer, SurfaceMiner, SandDigger, CharcoalBurner, PeatCutter, StoneQuarryman, FlintKnapper, Trapper, BirdHunter, BigGameHunter, BoneCollector, FeatherPlucker, ApprenticeLaborer, ApprenticeGroom, ApprenticeScullery, FlaxCutter,

        // --- [200] Producer: 중간재 및 부품 대량 생산 (Components & Materials) ---
        Smelter = 200, NailMaker, AxleMaker, GearCutter, SpringMaker, HingeMaker, SextantPartMaker, ClockPartMaker, PigIronWorker, Weaver, Spinner, ThreadMaker, LeatherTanner, ClothUnraveler, Dyer_Producer, Sawyer, ShaftMaker, BarkProcessor, BarrelMaker_Base, BoxMaker_Base, Miller, Butcher_Expert, PoultryProcessor, Vintner_Base, PizzaChef_Producer, OilPresser_Producer, GlassBlower, ReagentRefiner, AshProcessor, BoneGrinder, SilkExtractor, InkProducer, ScrollPresser, MapPresser, FeatherWorker, CandleDipper, GemCutter, JewelryBaseMaker, BeadMaker,

        // --- [300] Warrior: 무구 소모 및 전리품 보급 ---
        Knight = 300, Vanguard, Paladin, Halberdier, HeavyInfantry, LordGuard, Lancer, ManAtArms, Scout_Warrior, BorderPatrol, ChainGuard, Enforcer, Crusader, MaceBearer, TownGuard, ShieldMaiden, Duelist, Swashbuckler, Assassin_Warrior, Skirmisher, LightCavalry, Archer_Expert, Crossbowman, HeavyArcher, HorseArcher, SkirmishArcher, Slayer, UndeadHunter, DragonTracker, SnakeHunter, BeastSlayer, Marine_Warrior, Sapper, PackLeader_Warrior, AnimalTamer_Warrior, QuarterMaster_Warrior, Recruit, Militia_Warrior, Squire,

        // --- [400] Mage: 시약, 스크롤 및 마법 도구 소모/조제 ---
        Wizard = 400, Mage_Combat, Evoker, Archmage, BattleSage, Alchemist, PotionMaker, Transmuter, ApprenticeAlchemist, Venomist, Scribe_Mage, Copyist_Mage, MapMaker_Expert, Illuminator_Mage, ArchiveKeeper_Mage, Necromancer, Witch, Exorcist_Mage, BoneOracle, DarkApothecary, Druid, Elementalist, HerbMaster_Mage, ItemIdentifier_Mage, RuneCarver_Mage, StaffMaker_Mage, ScrollBundler,

        // --- [500] Noble: 고가 자산 관리 및 하이엔드 소비 ---
        Mayor = 500, Chancellor, TaxCollector_Noble, Administrator_Noble, Herald_Noble, Aristocrat, Socialite, Patron_Noble, Courtier, Landlord_Noble, Banker_Noble, Treasurer_Noble, EstateManager_Noble, MoneyLender_Noble, HighSheriff, Magistrate, Inquisitor_Noble, Executioner_Noble, Bishop_Noble, LibraryFounder_Noble, ExpeditionSponsor_Noble, Steward_Noble, LadyInWaiting_Noble, RoyalStableMaster_Noble,

        // --- [600] Merchant: 대량 물류 및 증서 유통 ---
        CaravanMaster = 600, StableBroker, PackLeader_Merchant, FodderMerchant, MetalWholesaler, TimberMerchant, HardwareTrader_Merchant, ToolWholesaler, ClothWholesaler, LeatherWholesaler, SilkTrader_Merchant, ArmamentMajor, ShieldTrader_Merchant, WeaponBroker_Merchant, ArcheryWholesaler_Mer, ShipBroker_Merchant, MaritimeTrader, PortAgent_Merchant, Wharfinger_Merchant, FruitExporter, GrainWholesaler_Mer, Vintner_Merchant, EmploymentAgent_Mer, DeedBroker_Merchant, Notary_Merchant,

        // --- [700] Religious: 시약, 붕대, 양초 소모 및 구휼 ---
        Priest = 700, Bishop_Relig, Healer_Master, Surgeon_Relig, PlagueDoctor_Relig, Veterinary_Relig, Monk_Scribe, Illuminator_Relig, Alchemist_Relig, Gravedigger_Relig, Embalmer_Relig, Mortician_Relig, Templar_Relig, CandleLighter_Relig, Sexton_Relig, Acolyte_Relig,

        // --- [800] Entertainer: 악기 및 유흥/식음료 최종 소모 ---
        Bard = 800, Harper, Lutanist, Drummer, Tambourinist, InnKeeper, Barmaid, Cook_Entertainer, Cellarman_Entertainer, Dancer, Acrobat, Juggler, Actor, TourGuide, Equestrian, AnimalShowman, FortuneTeller, StageHand, CostumeHelper,

        // --- [900] Maritime: 선박 증서 및 항해 정밀 도구 활용 ---
        Navigator = 900, ShipCaptain, Helmsman, Hydrographer, Lookout_Maritime, Shipwright_Master, BoatRepairer, InstrumentCalibrator, RiggingSpecialist, MastCarver, HarborMaster, DockWorker_Heavy, Stevedore_Lead, Wharfinger_Maritime, CraneOperator_Port, DeepSeaFisher, Whaler_Maritime, OysterDiver_Expert, NetMender_Port, Marine_Officer, Marine_Soldier, CoastGuard_Patrol, NavalArtillerist, ShipChandler_Master, LighthouseKeeper, MarineSalvager, ApprenticeSailor,

        // --- [1000] Scholar: 지식 자산화 및 행정 기록 ---
        Librarian = 1000, Scribe_Scholar, Copyist_Expert, Illuminator_Scholar, Cartographer_Scholar, Astronomer_Scholar, Archaeologist_Scholar, Naturalist_Expert, Professor_Scholar, Researcher_Scholar, Academician_Scholar, Archivist_Scholar, Lexicographer, Student_Scholar, Notary_Scholar, CensusTaker, HeraldicArtist, TaxAuditor, LegalAdvocate, ChronicleWriter, Technician_Scholar,

        // --- [1100] Criminal: 자원 약탈 및 지하 경제 유동성 공급 ---
        Thief = 1100, Burglar, Cutpurse, Smuggler, Fence_Criminal, BlackMarketeer, Assassin, Poisoner_Expert, Extortionist, Forger_Criminal, Gambler_Criminal, Charlatan_Criminal, Bandit_Highwayman, Poacher_Criminal, Slaver_Underworld, Counterfeiter_Deed, IllicitTinker, Lookout_Criminal, UnderworldEnforcer, ShadowBroker, Recruiter_Criminal
    }

    public enum NpcRank { Novice, Journeyman, Expert, Master }

    public abstract class VirtualAgent
    {
		public string Name { get; set; } = "Unknown"; // 추가
		public Container Backpack { get; set; }      // 추가
        public NpcJobClass JobClass { get; set; }
        public NpcRank Rank { get; set; }
        public int Gold { get; set; }
        public int Hunger { get; set; }
        public int Stress { get; set; } 
        public double PrimarySkill { get; set; }
		public enum NpcRank { Novice, Journeyman, Expert, Master } // 숙련도 등급 유지

        // 장비 사용 횟수(내구도) 관리 딕셔너리
        protected Dictionary<Type, int> m_EquipmentUses = new Dictionary<Type, int>();

        public VirtualAgent(NpcJobClass job, NpcRank rank) 
        { 
            JobClass = job; 
            Rank = rank; 
            Gold = CalculateStartingGold(job, rank);
            Hunger = Utility.RandomMinMax(0, 30);
            Stress = Utility.RandomMinMax(0, 20); 

            PrimarySkill = rank switch 
            {
                NpcRank.Novice => Utility.RandomMinMax(25, 50),
                NpcRank.Journeyman => Utility.RandomMinMax(50, 75),
                NpcRank.Expert => Utility.RandomMinMax(75, 95),
                NpcRank.Master => Utility.RandomMinMax(95, 100),
                _ => 25.0
            };
        }

		// [추가] 아이템 장착 메서드
		public void EquipItem(Item item) 
		{ 
			if (item is Container c) Backpack = c; 
		}

        public void CheckSkillGain()
		{
			if (PrimarySkill >= 200.0) return;

			// 성장 효율 지수 (초보 1.0 -> 마스터 0.0)
			double ratio = (200.0 - PrimarySkill) / 200.0;

			// 1. 상승 확률: 세제곱(Pow 3)을 적용하여 고스킬 구간 난이도를 기하급수적으로 높임
			// 스킬 100일 때 확률: 12.5%, 스킬 180일 때 확률: 0.1%
			double gainChance = Math.Pow(ratio, 3);

			if (Utility.RandomDouble() < gainChance)
			{
				// 2. 상승량: 초보일 땐 최대 0.5, 마스터에 가까우면 최소 0.1로 감소
				double amount = 0.1 + (0.4 * ratio);
				
				PrimarySkill = Math.Min(200.0, Math.Round(PrimarySkill + amount, 1));
			}
		}

        public VirtualAgent(GenericReader reader) 
        { 
            int version = reader.ReadInt(); 
            JobClass = (NpcJobClass)reader.ReadInt(); 
            Rank = (NpcRank)reader.ReadInt(); 
            Gold = reader.ReadInt(); 
            Hunger = reader.ReadInt();

            if (version >= 1) Stress = reader.ReadInt();

            if (version >= 2) PrimarySkill = reader.ReadDouble();
            else
            {
                PrimarySkill = Rank switch 
                {
                    NpcRank.Novice => Utility.RandomMinMax(25, 50),
                    NpcRank.Journeyman => Utility.RandomMinMax(50, 75),
                    NpcRank.Expert => Utility.RandomMinMax(75, 95),
                    NpcRank.Master => Utility.RandomMinMax(95, 100),
                    _ => 25.0
                };
            }
        }

        public virtual void Serialize(GenericWriter writer) 
        { 
            writer.Write(2); 
            writer.Write((int)JobClass); 
            writer.Write((int)Rank); 
            writer.Write(Gold); 
            writer.Write(Hunger);
            writer.Write(Stress);
            writer.Write(PrimarySkill); 
        }

        public static int CalculateStartingGold(NpcJobClass job, NpcRank rank)
        {
            int jobGroup = ((int)job / 100) * 100;
            int baseGold = jobGroup switch 
            { 
                100 => 100, 200 => 300, 300 => 500, 400 => 800, 500 => 2000, 
                600 => 1500, 700 => 700, 800 => 400, 900 => 1000, 1000 => 600, 1100 => 200, _ => 100 
            };

            int rankMultiplier = rank switch 
            { NpcRank.Novice => 1, NpcRank.Journeyman => 2, NpcRank.Expert => 5, NpcRank.Master => 10, _ => 1 };

            return baseGold * rankMultiplier;
        }

        public ItemTag ClassifyItem(Item item)
        {
            if (item is Food || item is BaseBeverage || item is Backpack || item is Pouch || item is Candle || item is Torch) 
                return ItemTag.Essential;
            if (item is BaseJewel || item is BaseInstrument || item is Cake || item is JarHoney) 
                return ItemTag.Luxury;
            if (item is BaseWeapon || item is BaseArmor || item is BaseTool || item is BaseClothing)
                return ItemTag.Tool;
            if (item is BaseReagent || item is BaseIngot || item is Log || item is BlankScroll)
                return ItemTag.Material;

            return ItemTag.None;
        }

        public DateTime LastSurvivalTick { get; set; } = DateTime.Now;

        public virtual void HourlyRoutine(TownEconomy town, int currentHour)
        {
            if (town == null) return;

            int elapsedHours = (int)(DateTime.Now - LastSurvivalTick).TotalHours;
            if (elapsedHours > 0)
            {
                LastSurvivalTick = DateTime.Now;
                this.Hunger += elapsedHours * 15; 
            }

            if (this.Hunger >= 50) ConsumeEssential(town);

            if (currentHour >= 9 && currentHour < 18)
            {
                if (this.Stress < 90)
                {
                    if (Utility.RandomBool()) ProcessJob(town);
                }
                else this.Hunger += 2; 
            }
            else if (currentHour >= 18 && currentHour <= 23)
            {
                if (this.Stress >= 40 && Utility.RandomDouble() < 0.3) RelieveStress(town);
            }
            else this.Stress = Math.Max(0, this.Stress - Utility.RandomMinMax(1, 2));
        }

        // ==============================================================================
        // [신규] 기축 통화 10 GP 고정 함수
        // ==============================================================================
        protected int GetEffectivePrice(TownEconomy town, Type itemType)
        {
            Type[] baseResources = [typeof(IronOre), typeof(Log), typeof(Hides), typeof(RawFishSteak), typeof(Fish), typeof(IronIngot), typeof(Board), typeof(Leather)];
            if (baseResources.Contains(itemType)) return 10;
            return Math.Max(1, town.GetPrice(itemType));
        }

        protected (bool Success, int Earnings) TrySellItem(TownEconomy town, Type itemType, int amount)
        {
            if (itemType == null || amount <= 0) return (false, 0);

            int unitPrice = GetEffectivePrice(town, itemType);
            int totalPrice = unitPrice * amount;

            this.Gold += totalPrice;
            town.Wealth -= totalPrice; 
            town.SupplyItem(itemType, amount, totalPrice); 
            return (true, totalPrice);
        }

        protected virtual void ConsumeEssential(TownEconomy town)
        {
            Type foodType = EconomyTagHelper.GetItemTypeByTag(town, ItemTag.Food_Basic, random: false);
            int foodPrice = Math.Max(1, town.GetPrice(foodType));
            int mealsNeeded = Math.Max(1, this.Hunger / 50);

            var buyResult = TryBuyItem(town, foodType, mealsNeeded);
            
            if (buyResult.Success)
            {
                this.Hunger = Math.Max(0, this.Hunger - (buyResult.AmountBought * 50)); 
                this.Stress = Math.Max(0, this.Stress - (buyResult.AmountBought * 5)); 
            }
            else
            {
                int reliefCost = foodPrice * mealsNeeded;
                if (this.Gold < reliefCost && town.Wealth >= reliefCost && town.Warehouse.ContainsKey(foodType) && town.Warehouse[foodType].Stock >= mealsNeeded)
                {
                    town.Wealth -= reliefCost;
                    town.Warehouse[foodType].Stock -= mealsNeeded;
                    
                    this.Hunger = Math.Max(0, this.Hunger - (mealsNeeded * 50));
                    this.Stress = Math.Max(0, this.Stress - 2); 
                    CheckSkillGain(); 
                    return;
                }
                this.Stress += 15; 
            }
        }

        protected virtual void RelieveStress(TownEconomy town)
        {
            int actionType = Utility.Random(100);

            if (actionType < 40) 
            {
                Type luxuryFood = EconomyTagHelper.GetItemTypeByTag(town, ItemTag.Food_Luxury, random: true);
                var buyResult = TryBuyItem(town, luxuryFood, 2);
                if (buyResult.Success) 
                {
                    this.Stress = Math.Max(0, this.Stress - (buyResult.AmountBought * 20));
                    this.Hunger = Math.Max(0, this.Hunger - (buyResult.AmountBought * 10));
                }
            }
            else if (actionType < 70) 
            {
                Type jewelry = EconomyTagHelper.GetItemTypeByTag(town, ItemTag.Jewelry, random: true);
                if (TryBuyItem(town, jewelry, 1).Success) this.Stress = Math.Max(0, this.Stress - 60); 
            }
            else 
            {
                Type entertainment = EconomyTagHelper.GetItemTypeByTag(town, ItemTag.Entertainment, random: true);
                if (TryBuyItem(town, entertainment, 1).Success) this.Stress = Math.Max(0, this.Stress - 50);
                else 
                {
                    int serviceFee = Utility.RandomMinMax(20, 100);
                    if (this.Gold >= serviceFee)
                    {
                        this.Gold -= serviceFee;
                        town.Wealth += serviceFee; 
                        this.Stress = Math.Max(0, this.Stress - 30); 
                    }
                }
            }
        }

        public static int JobGetGroup(NpcJobClass JobClass) => ((int)JobClass / 100) * 100;

        // ==============================================================================
        // [신규] 내구도 추출 및 장비 소비 헬퍼
        // ==============================================================================
        protected int GetItemDurability(Type type)
        {
            try
            {
                Item tempItem = (Item)Activator.CreateInstance(type);
                int maxUses = 50; 

                if (tempItem is BaseWeapon weapon) maxUses = weapon.InitMaxHits;
                else if (tempItem is BaseArmor armor) maxUses = armor.InitMaxHits;
                else if (tempItem is BaseTool tool) maxUses = tool.UsesRemaining;
                else if (tempItem is BaseHarvestTool harvestTool) maxUses = harvestTool.UsesRemaining;
                
                tempItem.Delete(); 
                
                return maxUses > 0 ? maxUses : 50;
            }
            catch { return 50; }
        }

        protected bool CheckAndUseEquipment(TownEconomy town, Type equipType, int lossAmount = 1)
        {
            if (equipType == null) return true; // 필요 없는 장비 슬롯 통과

            if (!m_EquipmentUses.ContainsKey(equipType) || m_EquipmentUses[equipType] <= 0)
            {
                if (TryBuyItem(town, equipType, 1).Success)
                {
                    m_EquipmentUses[equipType] = GetItemDurability(equipType); 
                }
                else return false; 
            }
            
            m_EquipmentUses[equipType] -= lossAmount; 
            return true;
        }

        // ==============================================================================
        // [핵심] 직업 매핑 프로필 (하드코딩 제거를 위한 설계도)
        // ==============================================================================
        public record JobProfile(Type Equip1, Type Equip2, Type InputMat, params Type[] Outputs);

        protected JobProfile GetJobProfile(NpcJobClass job)
        {
            return job switch
            {
				// [100번대] 기초 채집: 1티어 자원 명칭 동기화
				NpcJobClass.Herbalist 
					=> new JobProfile(typeof(Hoe), null, typeof(Ginseng), [typeof(Garlic)]),
				NpcJobClass.MushroomGatherer 
					=> new JobProfile(typeof(Hoe), null, typeof(MandrakeRoot), [typeof(Nightshade)]),
				NpcJobClass.SeaweedCollector or NpcJobClass.BeachComber 
					=> new JobProfile(typeof(FishingPole), null, typeof(BlackPearl), [typeof(SpidersSilk)]),
				NpcJobClass.CharcoalBurner 
					=> new JobProfile(typeof(Axe), null, typeof(SulfurousAsh), [typeof(Log)]),
				NpcJobClass.SurfaceMiner or NpcJobClass.SandDigger or NpcJobClass.StoneQuarryman or NpcJobClass.FlintKnapper
					=> new JobProfile(typeof(Pickaxe), null, typeof(IronOre), [typeof(IronIngot)]),
				NpcJobClass.Woodcutter or NpcJobClass.BarkCollector or NpcJobClass.ResinGatherer
					=> new JobProfile(typeof(Axe), null, typeof(Log), [typeof(Board)]),

				// [수정] 기초 어부: 1티어 송어 원재료인 RawTroutSteak로 변경
				NpcJobClass.CoastalFisher or NpcJobClass.DeepSeaFisher_Basic or NpcJobClass.Crabber or NpcJobClass.OysterDiver_Basic
					=> new JobProfile(typeof(FishingPole), null, typeof(TroutRawFishSteak), []),

				NpcJobClass.VegetableFarmer or NpcJobClass.GrainFarmer or NpcJobClass.GourdFarmer
					=> new JobProfile(typeof(Hoe), null, typeof(WheatSheaf), [typeof(Cabbage), typeof(Carrot)]),
				NpcJobClass.FlaxCutter or NpcJobClass.GongFarmer
					=> new JobProfile(typeof(Hoe), null, typeof(WheatSheaf), []),
				NpcJobClass.Orchardist or NpcJobClass.CitrusGrower or NpcJobClass.VineyardWorker or NpcJobClass.BerryPicker
					=> new JobProfile(typeof(Hoe), null, typeof(Apple), []),
				NpcJobClass.Shepherd or NpcJobClass.GooseHerd or NpcJobClass.PoultryFarmer
					=> new JobProfile(typeof(ShepherdsCrook), null, typeof(Wool), []),

				// 기초 가죽: 1티어 Hides 반영 확인
				NpcJobClass.Swineherd or NpcJobClass.CattleDrover or NpcJobClass.StableHand or NpcJobClass.HorseGroom_Basic
					=> new JobProfile(typeof(Pitchfork), null, typeof(Hides), [typeof(Leather)]),
				
				NpcJobClass.Trapper or NpcJobClass.BirdHunter or NpcJobClass.BigGameHunter or NpcJobClass.FeatherPlucker
					=> new JobProfile(typeof(SkinningKnife), null, typeof(Feather), []),
				NpcJobClass.Beekeeper => new JobProfile(typeof(Dagger), null, typeof(JarHoney), []),
				NpcJobClass.GraveDigger_Basic or NpcJobClass.BoneCollector or NpcJobClass.PeatCutter 
					=> new JobProfile(typeof(Shovel), null, typeof(Bone), [typeof(GraveDust)]),
				NpcJobClass.WaterCarrier => new JobProfile(typeof(Pitcher), null, typeof(BegWaterPitcher), []), 
				NpcJobClass.DairyWorker => new JobProfile(typeof(Pitcher), null, typeof(CheeseWheel), []), 

				// [200번대] 가공/생산 (기존 유지)
				NpcJobClass.Smelter or NpcJobClass.PigIronWorker => new JobProfile(typeof(Tongs), null, typeof(IronOre), [typeof(IronIngot)]),
				NpcJobClass.NailMaker => new JobProfile(typeof(SmithHammer), null, typeof(IronIngot), [typeof(Nails)]),
				NpcJobClass.AxleMaker or NpcJobClass.GearCutter or NpcJobClass.SextantPartMaker or NpcJobClass.ClockPartMaker 
					=> new JobProfile(typeof(TinkerTools), null, typeof(IronIngot), [typeof(Gears)]),
				NpcJobClass.SpringMaker or NpcJobClass.HingeMaker => new JobProfile(typeof(TinkerTools), null, typeof(IronIngot), [typeof(Springs)]),
				NpcJobClass.Spinner or NpcJobClass.ThreadMaker or NpcJobClass.SilkExtractor 
					=> new JobProfile(typeof(SewingKit), null, typeof(Wool), [typeof(SpoolOfThread)]),
				NpcJobClass.Weaver or NpcJobClass.ClothUnraveler => new JobProfile(typeof(SewingKit), null, typeof(SpoolOfThread), [typeof(BoltOfCloth)]),
				NpcJobClass.LeatherTanner => new JobProfile(typeof(SewingKit), null, typeof(Hides), [typeof(Leather)]),
				NpcJobClass.Dyer_Producer => new JobProfile(typeof(Dyes), null, typeof(Tub), [typeof(DyeTub)]),
				NpcJobClass.Sawyer or NpcJobClass.BarkProcessor or NpcJobClass.ShaftMaker => new JobProfile(typeof(Saw), null, typeof(Board), [typeof(Shaft)]),
				NpcJobClass.BarrelMaker_Base => new JobProfile(typeof(Saw), null, typeof(Board), [typeof(Barrel)]),
				NpcJobClass.BoxMaker_Base => new JobProfile(typeof(Saw), null, typeof(Board), [typeof(WoodenBox)]),
				NpcJobClass.Miller or NpcJobClass.PizzaChef_Producer => new JobProfile(typeof(RollingPin), null, typeof(SackFlour), [typeof(CheesePizza)]), 
				NpcJobClass.Butcher_Expert or NpcJobClass.PoultryProcessor => new JobProfile(typeof(Cleaver), null, typeof(RawRibs), [typeof(Ribs)]),
				NpcJobClass.Vintner_Base => new JobProfile(typeof(Bottle), null, typeof(Apple), [typeof(BottleOfWine)]), 
				NpcJobClass.OilPresser_Producer => new JobProfile(typeof(MortarPestle), null, typeof(WheatSheaf), [typeof(Bottle)]), 
				NpcJobClass.GlassBlower => new JobProfile(typeof(Blowpipe), null, typeof(Sand), [typeof(Bottle)]),
				NpcJobClass.GemCutter or NpcJobClass.JewelryBaseMaker or NpcJobClass.BeadMaker 
					=> new JobProfile(typeof(TinkerTools), null, typeof(Amber), [typeof(BaseJewel)]),
				NpcJobClass.ReagentRefiner or NpcJobClass.AshProcessor or NpcJobClass.BoneGrinder 
					=> new JobProfile(typeof(MortarPestle), null, typeof(SulfurousAsh), [typeof(BlackPearl)]),
				NpcJobClass.InkProducer => new JobProfile(typeof(MortarPestle), null, typeof(BlackPearl), [typeof(Dyes)]), 
				NpcJobClass.ScrollPresser or NpcJobClass.MapPresser => new JobProfile(typeof(ScribesPen), null, typeof(Log), [typeof(BlankScroll)]),
				NpcJobClass.FeatherWorker or NpcJobClass.CandleDipper => new JobProfile(typeof(Scissors), null, typeof(Feather), [typeof(Candle)]),

                // [300번대] 전투직
                NpcJobClass.Paladin or NpcJobClass.Crusader or NpcJobClass.Knight
                    => new JobProfile(typeof(Mace), typeof(PlateChest), null, [typeof(Bone), typeof(GraveDust), typeof(DaemonBone)]),
                NpcJobClass.Archer_Expert or NpcJobClass.Crossbowman or NpcJobClass.SkirmishArcher or NpcJobClass.Scout_Warrior
                    => new JobProfile(typeof(Bow), typeof(LeatherChest), null, [typeof(Hides), typeof(Feather), typeof(RawRibs)]),
                NpcJobClass.BeastSlayer or NpcJobClass.DragonTracker or NpcJobClass.Slayer
                    => new JobProfile(typeof(Halberd), typeof(DragonChest), null, [typeof(DragonBlood), typeof(WhiteScales), typeof(Hides)]), // 수정: DragonScales -> WhiteScales
                NpcJobClass.Assassin_Warrior or NpcJobClass.Duelist or NpcJobClass.Swashbuckler
                    => new JobProfile(typeof(Kryss), typeof(StuddedChest), null, [typeof(SpidersSilk), typeof(Bone)]),

                // [400번대] 마법직
                NpcJobClass.Necromancer or NpcJobClass.BoneOracle or NpcJobClass.Witch
                    => new JobProfile(typeof(MortarPestle), null, typeof(BatWing), [typeof(NecromancerSpellbook), typeof(GraveDust)]),
                NpcJobClass.Alchemist or NpcJobClass.PotionMaker or NpcJobClass.DarkApothecary
                    => new JobProfile(typeof(MortarPestle), null, typeof(Ginseng), [typeof(GreaterHealPotion), typeof(TotalRefreshPotion)]),
                NpcJobClass.Venomist or NpcJobClass.Poisoner_Expert
                    => new JobProfile(typeof(MortarPestle), null, typeof(Nightshade), [typeof(DeadlyPoisonPotion), typeof(GreaterCurePotion)]),
                NpcJobClass.Wizard or NpcJobClass.Archmage or NpcJobClass.Scribe_Mage
                    => new JobProfile(typeof(ScribesPen), null, typeof(BlankScroll), [typeof(FlamestrikeScroll), typeof(GateTravelScroll)]),

                // [600번대] 상인
                NpcJobClass.CaravanMaster or NpcJobClass.MaritimeTrader or NpcJobClass.FruitExporter
                    => new JobProfile(typeof(PackHorse), null, typeof(IronOre), [typeof(CommodityDeed)]),

                // [800번대] 유흥/바드
                NpcJobClass.Lutanist or NpcJobClass.Bard => new JobProfile(typeof(Lute), null, typeof(Dyes), [typeof(ApplePie), typeof(Gold)]),
                NpcJobClass.Drummer => new JobProfile(typeof(Drums), null, typeof(Dyes), [typeof(Cake), typeof(Gold)]), // 수정: Drum -> Drums
                NpcJobClass.Tambourinist => new JobProfile(typeof(Tambourine), null, typeof(Dyes), [typeof(JarHoney), typeof(Gold)]),

                // [900번대] 해양
                NpcJobClass.ShipCaptain or NpcJobClass.DeepSeaFisher or NpcJobClass.Whaler_Maritime
					=> new JobProfile(typeof(Sextant), typeof(SpecialFishingNet), typeof(TroutRawFishSteak), [typeof(BlackPearl)]),

                // [1100번대] 범죄자
                NpcJobClass.Burglar or NpcJobClass.Thief or NpcJobClass.Cutpurse
                    => new JobProfile(typeof(Lockpick), null, null, [typeof(GoldRing), typeof(SilverRing)]),
				NpcJobClass.Smuggler 
					=> new JobProfile(typeof(Dagger), null, typeof(Bloodmoss), [typeof(BlackPearl)]),
                NpcJobClass.BlackMarketeer or NpcJobClass.Assassin
                    => new JobProfile(typeof(Dagger), null, null, [typeof(BlackPearl), typeof(Bloodmoss)]),

                _ => null 
            };
        }

		public (int Tier, bool CanProcess) GetResourceTier(double skill)
		{
			int tier = skill switch
			{
				< 50.0 => 1,
				< 70.0 => 2,
				< 90.0 => 3,
				< 110.0 => 4,
				< 130.0 => 5,
				< 150.0 => 6,
				_ => 7
			};
			return (tier, true);
		}

		// [수정] 통합 구매 로직 (CS0103, CS0841 오류 해결)
		protected (bool Success, int AmountBought, int TotalCost) TryBuyItem(TownEconomy town, Type itemType, int requestedAmount)
		{
			if (itemType == null || this is not VirtualCitizen citizen) return (false, 0, 0);

			// 1. 단가 산출
			int unitPrice = GetEffectivePrice(town, itemType);

			// 2. [핵심 수정] VirtualTradeAI로 requestedAmount(요청 수량)를 정확히 넘겨줍니다!
			var result = VirtualTradeAI.ExecutePurchase(citizen, town, itemType, unitPrice, requestedAmount);

			if (result.Success)
			{
				// [수정] 무조건 1이 아니라, 실제로 요청해서 구매 성공한 수량을 반환합니다.
				return (true, requestedAmount, result.Spent); 
			}

			return (false, 0, 0);
		}

		// [추가] 자원 가치 판단 헬퍼 (VirtualTradeAI의 정적 메서드 활용)
		protected bool IsRareResource(Type type) => VirtualTradeAI.IsRareResource(type);
		protected int GetResourceTierValue(Type type) => VirtualTradeAI.GetResourceTierValue(type);


        // ==============================================================================
        // [완성] ProcessJob: 모든 로직 융합 (프로필 기반 작동 + 안전망)
        // ==============================================================================
        protected virtual void ProcessJob(TownEconomy town)
        {
            int jobGroup = JobGetGroup(JobClass);
            JobProfile profile = GetJobProfile(JobClass);
            bool jobSuccess = false; 

			double pFactor = (this is VirtualCitizen vc) ? vc.Potential : 1.0;

            switch (jobGroup)
            {
                case 100: // [100번대] 잠재력에 따른 생산량 차등 적용
                    if (profile != null && CheckAndUseEquipment(town, profile.Equip1)) 
                    {
                        int harvestAmount = (int)((5 + (this.PrimarySkill / 10.0)) * pFactor);
                        
                        if (profile.Outputs.Length > 0)
                        {
                            int rawProfit = GetEffectivePrice(town, profile.InputMat) * harvestAmount;
                            int refinedProfit = GetEffectivePrice(town, profile.Outputs[0]) * (harvestAmount * 2);

                            if (refinedProfit > rawProfit) TrySellItem(town, profile.Outputs[0], harvestAmount * 2);
                            else TrySellItem(town, profile.InputMat, harvestAmount);
                        }
                        else TrySellItem(town, profile.InputMat, harvestAmount);
                        
                        jobSuccess = true;
                    }
                    break;

                case 200: // [200번대] 일반 제작 + 귀족 하청 로직
                    if (profile != null && profile.Outputs.Length > 0 && CheckAndUseEquipment(town, profile.Equip1))
                    {
                        // 1. 귀족의 희귀 자원 가공 시도 (하청 로직 공간)
                        if (this is VirtualCitizen producer)
                        {
                            var noble = town.Citizens.FirstOrDefault(c => c.RankLevel >= NobilityRank.Baron && c.House?.HouseWarehouse.Any(kvp => IsRareResource(kvp.Key)) == true);
                            if (noble != null)
                            {
                                var (myTier, _) = GetResourceTier(this.PrimarySkill);
                                // 여기서 티어 비교 및 명품 제작 로직 수행 가능
                            }
                        }

                        // 2. 일반 제작 로직 (기존 유지)
                        int craftMultiplier = (int)((1 + (this.PrimarySkill / 20.0)) * pFactor);
                        int buyAmount = 5 * craftMultiplier; 
                        
                        var buyResult = TryBuyItem(town, profile.InputMat, buyAmount);
                        if (buyResult.Success)
                        {
                            int laborValue = (int)((this.PrimarySkill / 2.0) * craftMultiplier);
                            int guaranteedPrice = (int)(buyResult.TotalCost * 1.5) + laborValue;

                            this.Gold += guaranteedPrice;
                            town.Wealth -= guaranteedPrice;
                            town.SupplyItem(profile.Outputs[0], craftMultiplier, guaranteedPrice);
                            jobSuccess = true;
                        }
                    }
                    break;

                case 300: // [300번대] 전투직 잠재력 보정
                    Type weapon = profile?.Equip1 ?? typeof(Broadsword);
                    Type armor = profile?.Equip2 ?? typeof(PlateChest);
                    int targetFame = (int)(this.PrimarySkill * 100 * pFactor); 
                    int durabilityLoss = 1 + (targetFame / 3000); 

                    if (CheckAndUseEquipment(town, weapon, durabilityLoss) && CheckAndUseEquipment(town, armor, durabilityLoss))
                    {
                        int totalGoldPool = (int)((10 + Utility.RandomMinMax(targetFame / 30, targetFame / 15)) * pFactor);
                        this.Gold += totalGoldPool; 
                        
                        Type[] lootTable = (profile != null && profile.Outputs.Length > 0) ? profile.Outputs : [typeof(Bone), typeof(Hides), typeof(Ribs)];
                        Type loot = lootTable[Utility.Random(lootTable.Length)];
                        
                        int lootAmount = (int)((2 + (this.PrimarySkill / 15.0)) * pFactor);
                        TrySellItem(town, loot, Utility.RandomMinMax(lootAmount / 2, lootAmount));
                        jobSuccess = true;
                    }
                    break;

				case 400: // [400번대] 마법직: 잠재력에 비례하여 조제량 상승
                    if (profile != null && CheckAndUseEquipment(town, profile.Equip1)) 
                    {
                        // 잠재력이 높을수록 한 번에 더 많은 시약을 가공
                        int craftAmount = Math.Max(1, (int)((this.PrimarySkill / 20.0) * pFactor)); 
                        if (TryBuyItem(town, profile.InputMat, craftAmount).Success)
                        {
                            Type output = profile.Outputs[Utility.Random(profile.Outputs.Length)];
                            // 생산 결과물 또한 pFactor에 비례하여 증폭
                            TrySellItem(town, output, (int)(craftAmount * 2 * pFactor)); 
                            jobSuccess = true;
                        }
                    }
                    break;

                case 500: // [500번대] 귀족: 잠재력이 높을수록 더 높은 품위 유지비(Salary) 획득
					// Rank가 Novice(0)면 100, Master(3)면 400이 기본값이 됩니다.
					int salary = (int)(((int)this.Rank * 100 + 100) * pFactor); 

					if (town.Wealth > salary)
					{
						town.Wealth -= salary;
						this.Gold += salary;
						
						// 사치품 소비 및 문서 발행 로직...
						Type nobleLuxury = EconomyTagHelper.GetItemTypeByTag(town, ItemTag.Luxury, random: true);
						TryBuyItem(town, nobleLuxury, (int)(2 * pFactor)); 
						
						jobSuccess = true;
					}
					break;

                case 600: // [600번대] 상인: 잠재력에 비례하여 운송 용량(Capacity) 확장
                    Type packAnimal = profile?.Equip1 ?? typeof(PackHorse);
                    if (CheckAndUseEquipment(town, packAnimal)) 
                    {
                        if (this is VirtualCitizen merchant600)
                        {
                            // 무역 용량을 pFactor에 비례하여 결정 (최대 3000)
                            int tradeCapacity = (int)(1000 * pFactor);
                            var tradeResult = VirtualTradeAI.ExecuteTradeRoute(merchant600, town, tradeCapacity);
                            
                            if (tradeResult.Success) jobSuccess = true;
                            else
                            {
                                TryBuyItem(town, profile?.InputMat ?? typeof(IronOre), (int)(10 * pFactor));
                                TrySellItem(town, typeof(CommodityDeed), (int)(5 * pFactor));
                            }
                        }
                    }
                    break;

                case 700: // [700번대] 종교: 잠재력이 높을수록 더 큰 구휼금 획득
                    if (TryBuyItem(town, typeof(Bandage), (int)(2 * pFactor)).Success &&
                        TryBuyItem(town, typeof(Candle), (int)(2 * pFactor)).Success) 
                    {
                        int donation = (int)((Utility.RandomMinMax(10, 30) + (int)this.PrimarySkill) * pFactor);
                        if (town.Wealth >= donation) { town.Wealth -= donation; this.Gold += donation; }
                        Type graveLoot = Utility.RandomBool() ? typeof(GraveDust) : typeof(Bone);
                        TrySellItem(town, graveLoot, (int)(2 * pFactor));
                        jobSuccess = true;
                    }
                    break;

                case 800: // [800번대] 예술/유흥: 잠재력에 비례하여 공연 수익 증대
                    Type instrument = profile?.Equip1 ?? typeof(Lute);
                    if (CheckAndUseEquipment(town, instrument)) 
                    {
                        if (profile?.InputMat != null) TryBuyItem(town, profile.InputMat, 1); 
                        // 공연 수익에 pFactor 적용
                        this.Gold += (int)((Utility.RandomMinMax(15, 40) + (int)(this.PrimarySkill / 2)) * pFactor);
                        
                        if (profile != null && profile.Outputs.Length > 0 && Utility.RandomBool())
                        {
                            Type outFood = profile.Outputs[Utility.Random(profile.Outputs.Length)];
                            TrySellItem(town, outFood, (int)(1 * pFactor)); 
                        }
                        jobSuccess = true;
                    }
                    break;

                case 900: // [900번대] 해양: 잠재력에 비례하여 수확량 및 전리품 증가
                    if (profile != null && CheckAndUseEquipment(town, profile.Equip1) && CheckAndUseEquipment(town, profile.Equip2)) 
                    {
                        if (this is VirtualCitizen maritime900)
                        {
                            // 하위 메서드에서 이미 pFactor를 사용하도록 기획됨
                            VirtualTradeAI.ExecuteHarvestAndSell(maritime900, town, 4); 
                            Type seaLoot = profile.Outputs[Utility.Random(profile.Outputs.Length)];
                            TrySellItem(town, seaLoot, (int)((2 + Utility.Random(4)) * pFactor));
                            jobSuccess = true;
                        }
                    }
                    break;

                case 1000: // [1000번대] 학자: 잠재력에 비례하여 집필 속도 및 수량 증가
                    Type pen = profile?.Equip1 ?? typeof(ScribesPen);
                    if (CheckAndUseEquipment(town, pen)) 
                    {
                        int paperCount = (int)(5 * pFactor);
                        if (TryBuyItem(town, typeof(BlankScroll), paperCount).Success) 
                        {
                            Type[] bookTable = (profile != null && profile.Outputs.Length > 0) ? profile.Outputs : [typeof(RedBook), typeof(BlueBook), typeof(BlankMap), typeof(Spellbook)];
                            Type bookToSell = bookTable[Utility.Random(bookTable.Length)];
                            TrySellItem(town, bookToSell, (int)(Utility.RandomMinMax(1, 3) * pFactor)); 
                            jobSuccess = true;
                        }
                    }
                    break;

                case 1100: // [1100번대] 범죄: 잠재력이 높을수록 지능적으로 더 큰 금액을 탈취
                    Type crimeTool = profile?.Equip1 ?? typeof(Dagger);
                    if (CheckAndUseEquipment(town, crimeTool)) 
                    {
                        // 탈취 금액에 pFactor 직접 곱연산
                        int stolenGold = (int)((Utility.RandomMinMax(20, 80) + (int)this.PrimarySkill) * pFactor);
                        if (town.Wealth >= stolenGold)
                        {
                            town.Wealth -= stolenGold; 
                            this.Gold += stolenGold;
                            
                            Type[] stealTable = (profile != null && profile.Outputs.Length > 0) ? profile.Outputs : [typeof(GoldRing)];
                            Type stolenGoods = stealTable[Utility.Random(stealTable.Length)];
                            // 장물 수량 또한 잠재력에 비례
                            TrySellItem(town, stolenGoods, (int)(1 * pFactor));
                            jobSuccess = true;
                        }
                    }
                    break;
			}

            // ==============================================================================
            // [안전 데이터] 작업 실패 또는 매핑이 안 된 직업의 파산 방지 로직
            // ==============================================================================
            if (jobSuccess)
            {
                this.CheckSkillGain();
            }
            else
            {
                int oddJobWage = 10 + (int)(this.PrimarySkill / 10.0); 
                if (town.Wealth >= oddJobWage)
                {
                    town.Wealth -= oddJobWage;
                    this.Gold += oddJobWage;
                }
                else this.Gold += 10; 
                this.Stress += 5; 
            }

            this.Stress -= 10;
        }
    }
}