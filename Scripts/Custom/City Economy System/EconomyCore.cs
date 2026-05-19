using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Engines.Craft;

namespace Server.Misc
{
    public enum NpcJobClass
    {
        // --- [100] Peasant: 기초 자원 추출 및 단순 노무 (Raw Materials) ---
        Pauper = 100, Laborer, StreetSweeper, WaterCarrier, NightSoilMan, RatCatcher, Beggar, ChimneySweep, Lamplighter, LinkBoy, GraveDigger_Basic, Scullion, Messenger_Foot, KennelMaid, GongFarmer, GrainFarmer, VegetableFarmer, GourdFarmer, Orchardist, CitrusGrower, VineyardWorker, BerryPicker, Herbalist, MushroomGatherer, Beekeeper, CoastalFisher, DeepSeaFisher_Basic, Crabber, OysterDiver_Basic, SeaweedCollector, BeachComber, SaltGatherer, Mudlarker, Shepherd, Swineherd, PoultryFarmer, CattleDrover, StableHand, DairyWorker, GooseHerd, DonkeyDriver, HorseGroom_Basic, Woodcutter, BarkCollector, ResinGatherer, SurfaceMiner, SandDigger, CharcoalBurner, PeatCutter, StoneQuarryman, FlintKnapper, Trapper, BirdHunter, BigGameHunter, BoneCollector, FeatherPlucker, ApprenticeLaborer, ApprenticeGroom, ApprenticeScullery, FlaxCutter,

        // --- [200] Producer: 중간재 및 부품 대량 생산 (Components & Materials) ---
        Smelter = 200, NailMaker, AxleMaker, GearCutter, SpringMaker, HingeMaker, SextantPartMaker, ClockPartMaker, PigIronWorker, Weaver, Spinner, ThreadMaker, LeatherTanner, ClothUnraveler, Dyer_Producer, Sawyer, ShaftMaker, BarkProcessor, BarrelMaker_Base, BoxMaker_Base, Miller, Butcher_Expert, PoultryProcessor, Vintner_Base, PizzaChef_Producer, OilPresser_Producer, GlassBlower, ReagentRefiner, AshProcessor, BoneGrinder, SilkExtractor, InkProducer, ScrollPresser, MapPresser, FeatherWorker, CandleDipper, GemCutter, JewelryBaseMaker, BeadMaker, Blacksmith, Bowyer, Carpenter_Producer, Tailor, Tinker,

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
        public string Name { get; set; } = "Unknown"; 
        public Container Backpack { get; set; }      
        public NpcJobClass JobClass { get; set; }
        public NpcRank Rank { get; set; }
        public int Gold { get; set; }
        public int Hunger { get; set; }
        public int Stress { get; set; } 
        public double PrimarySkill { get; set; }

        protected Dictionary<EconomyItemKey, int> m_EquipmentUses = new Dictionary<EconomyItemKey, int>();

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

        public void EquipItem(Item item) 
        { 
            if (item is Container c) Backpack = c; 
        }

        public void CheckSkillGain()
        {
            if (PrimarySkill >= 200.0) return;

            double ratio = (200.0 - PrimarySkill) / 200.0;
            double gainChance = Math.Pow(ratio, 3);

            if (Utility.RandomDouble() < gainChance)
            {
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

        protected int GetEffectivePrice(TownEconomy town, EconomyItemKey itemKey)
        {
            Type[] baseResources = [typeof(IronOre), typeof(Log), typeof(Hides), typeof(RawFishSteak), typeof(Fish), typeof(IronIngot), typeof(Board), typeof(Leather)];
            if (baseResources.Contains(itemKey.ItemType)) return 10;
            return Math.Max(1, town.GetPrice(itemKey)); // town.GetPrice도 나중에 Key를 받도록 수정할 예정
        }

		// 🌟 유저님 전용 커스텀 광물 티어 반영 (초반 생략, 후반 미스릴/흑요석 확장)
        public (int Tier, bool CanProcess) GetResourceTier(double skill)
        {
            int tier = skill switch
            {
                < 50.0  => 1, // 티어 1: Iron (기초 광물)
                < 70.0  => 2, // 티어 2: Copper (Dull/Shadow 삭제로 빠른 진입)
                < 90.0  => 3, // 티어 3: Bronze
                < 110.0 => 4, // 티어 4: Gold
                < 130.0 => 5, // 티어 5: Agapite
                < 150.0 => 6, // 티어 6: Verite
                < 170.0 => 7, // 티어 7: Valorite
                < 190.0 => 8, // 티어 8: Mithril (신규 하이엔드 광물)
                _       => 9  // 티어 9: Obsidian (스킬 190 이상 최상위 광물)
            };
            return (tier, true);
        }

        protected (bool Success, int Earnings) TrySellItem(TownEconomy town, EconomyItemKey itemKey, int amount)
        {
            if (itemKey.ItemType == null || amount <= 0) return (false, 0);

            int unitPrice = GetEffectivePrice(town, itemKey);
            int totalPrice = unitPrice * amount;

            this.Gold += totalPrice;
            town.Wealth -= totalPrice; 
            town.SupplyItem(itemKey, amount, totalPrice); // SupplyItem도 나중에 Key를 받도록 수정 예정
            return (true, totalPrice);
        }

        // 🌟 [최적화 완료] 실제로 돈을 지불한 개수만큼 반환
        protected (bool Success, int AmountBought, int TotalCost) TryBuyItem(TownEconomy town, EconomyItemKey itemKey, int requestedAmount)
        {
            if (itemKey.ItemType == null || this is not VirtualCitizen citizen) return (false, 0, 0);

            int unitPrice = GetEffectivePrice(town, itemKey);
            var result = VirtualTradeSystem.ExecutePurchase(citizen, town, itemKey, unitPrice, requestedAmount);

            if (result.Success && result.Spent > 0)
            {
                int actualBought = result.Spent / unitPrice;
                return (true, actualBought, result.Spent); 
            }

            return (false, 0, 0);
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

        protected bool CheckAndUseEquipment(TownEconomy town, EconomyItemKey equipKey, int lossAmount = 1)
        {
            if (equipKey.ItemType == null) return true;

            if (!m_EquipmentUses.ContainsKey(equipKey) || m_EquipmentUses[equipKey] <= 0)
            {
                if (TryBuyItem(town, equipKey, 1).Success)
                {
                    m_EquipmentUses[equipKey] = GetItemDurability(equipKey.ItemType); 
                }
                else return false; 
            }
            
            m_EquipmentUses[equipKey] -= lossAmount; 
            return true;
        }

        // ==============================================================================
        // 🌟 [안전 복구] UO CraftSystem 연동 (보호수준 에러 방지 처리 완료)
        // ==============================================================================
        public static CraftSystem GetCraftSystem(NpcJobClass job)
        {
            // 이름으로 텍스트 매칭을 시도하여 시스템 생성자 접근 보호(CS0122) 에러를 완벽 회피합니다.
            string targetName = job switch {
                NpcJobClass.Blacksmith => "DefBlacksmithy",
                NpcJobClass.Tailor => "DefTailoring",
                NpcJobClass.Carpenter_Producer => "DefCarpentry",
                NpcJobClass.Bowyer => "DefBowcraft",
                NpcJobClass.Alchemist or NpcJobClass.PotionMaker => "DefAlchemy",
                NpcJobClass.Tinker => "DefTinkering",
                NpcJobClass.Scribe_Mage or NpcJobClass.Scribe_Scholar => "DefInscription",
                NpcJobClass.PizzaChef_Producer => "DefCooking",
                NpcJobClass.GlassBlower => "DefGlassblowing",
                _ => null
            };

            if (targetName == null) return null;

            // 서버에 이미 로드된 CraftSystem 리스트를 순회하여 해당 엔진을 찾습니다.
            foreach (CraftSystem sys in CraftSystem.Systems)
            {
                if (sys.GetType().Name == targetName)
                    return sys;
            }
            return null;
        }

        protected Type GetToolForJob(NpcJobClass job)
        {
            return job switch {
                NpcJobClass.Blacksmith => typeof(SmithHammer),
                NpcJobClass.Tailor => typeof(SewingKit),
                NpcJobClass.Carpenter_Producer => typeof(Saw),
                NpcJobClass.Alchemist or NpcJobClass.PotionMaker => typeof(MortarPestle),
                NpcJobClass.Bowyer => typeof(FletcherTools),
                NpcJobClass.PizzaChef_Producer => typeof(RollingPin),
                NpcJobClass.Scribe_Mage or NpcJobClass.Scribe_Scholar => typeof(ScribesPen),
                NpcJobClass.GlassBlower => typeof(Blowpipe),
                NpcJobClass.Tinker => typeof(TinkerTools),
                _ => null
            };
        }

        // CraftSystem에 포함되지 않는 1차 정제 작업(제련, 직조 등)을 위한 예비 레시피
        private (Type Input, int Cost, Type Output, int Yield) GetRefineryRecipe(NpcJobClass job)
        {
            return job switch {
                NpcJobClass.Smelter or NpcJobClass.PigIronWorker => (typeof(IronOre), 1, typeof(IronIngot), 2),
                NpcJobClass.Sawyer or NpcJobClass.BarkProcessor => (typeof(Log), 1, typeof(Board), 2),
                NpcJobClass.Miller => (typeof(WheatSheaf), 1, typeof(SackFlour), 2),
                NpcJobClass.LeatherTanner => (typeof(Hides), 1, typeof(Leather), 1),
                NpcJobClass.Butcher_Expert => (typeof(RawRibs), 1, typeof(Ribs), 1),
                NpcJobClass.Weaver => (typeof(SpoolOfThread), 5, typeof(BoltOfCloth), 1),
                NpcJobClass.Spinner => (typeof(Wool), 1, typeof(SpoolOfThread), 3),
                _ => (null, 0, null, 0)
            };
        }

        // ==============================================================================
        // 🌟 30회 루프 + 진짜 UO 레시피 대량 생산 + LINQ(CS1061) 호환성 수정 완료
        // ==============================================================================
        protected virtual void ProcessJob(TownEconomy town)
        {
            int jobGroup = JobGetGroup(JobClass);
            bool jobSuccess = false; 

            double pFactor = (this is VirtualCitizen vc) ? vc.Potential : 1.0;
            int workCycles = 30; // 30분에 맞춰 30회 분할 연산
            int failedAttempts = 0;

            switch (jobGroup)
            {
                case 100: // [100번대] 기초 채집
                    Type gatherTool = this.JobClass.ToString().Contains("Miner") ? typeof(Pickaxe) : typeof(Axe); 
                    
                    if (CheckAndUseEquipment(town, gatherTool, 1)) 
                    {
                        if (this is VirtualCitizen worker100)
                        {
                            for (int i = 0; i < workCycles; i++)
                            {
                                var result = VirtualTradeSystem.ExecuteHarvestAndSell(worker100, town, 10);
                                if (result.Success) jobSuccess = true;
                            }
                        }
                    }
                    break;

                case 200: 
                case 400: 
                case 1000: // [200, 400, 1000번대] CraftSystem 기반 물품 제작
                    CraftSystem craftSys = GetCraftSystem(this.JobClass);
                    
                    if (craftSys != null)
                    {
                        Type tool = GetToolForJob(this.JobClass);
                        if (CheckAndUseEquipment(town, tool, 1))
                        {
                            // 🌟 [수정] CraftItemCol에서 호환성 에러가 나는 LINQ를 제거하고 안전한 For 루프로 우회 필터링
                            List<CraftItem> validItems = new List<CraftItem>();
                            for (int i = 0; i < craftSys.CraftItems.Count; i++)
                            {
                                CraftItem c = craftSys.CraftItems.GetAt(i);
                                if (c.Resources.Count > 0 && c.Skills.Count > 0 && c.Skills.GetAt(0).MinSkill <= this.PrimarySkill)
                                {
                                    validItems.Add(c);
                                }
                            }

                            if (validItems.Count > 0)
                            {
                                for (int i = 0; i < workCycles; i++)
                                {
                                    if (failedAttempts >= 5) break; 

                                    // 내가 만들 수 있는 물건 중 무작위로 선택 (다양성)
                                    CraftItem targetItem = validItems[Utility.Random(validItems.Count)];
                                    
                                    Type inputMat = targetItem.Resources.GetAt(0).ItemType;
                                    int inputAmount = targetItem.Resources.GetAt(0).Amount;
                                    Type outputItem = targetItem.ItemType;

                                    var buyResult = TryBuyItem(town, inputMat, inputAmount);
                                    
                                    if (buyResult.Success && buyResult.AmountBought >= inputAmount)
                                    {
                                        failedAttempts = 0;
                                        
                                        // 🌟 [수정] 존재하지 않는 MinCraftEffect 대신 자체 확률 산술식 사용
                                        double minSkill = targetItem.Skills.GetAt(0).MinSkill;
                                        double maxSkill = targetItem.Skills.GetAt(0).MaxSkill;
                                        double chance = 0.2 + ((this.PrimarySkill - minSkill) / Math.Max(1.0, maxSkill - minSkill)) * 0.8;
                                        chance = Math.Clamp(chance, 0.2, 1.0); 

                                        if (Utility.RandomDouble() <= chance)
                                        {
                                            int actualYield = (int)(Math.Max(1.0, pFactor));
                                            int laborValue = (int)((this.PrimarySkill / 5.0) * actualYield);
                                            int guaranteedPrice = (int)(buyResult.TotalCost * 1.5) + laborValue;

                                            this.Gold += guaranteedPrice;
                                            town.Wealth -= guaranteedPrice;
                                            
                                            town.SupplyItem(outputItem, actualYield, guaranteedPrice);
                                            jobSuccess = true;
                                        }
                                        else
                                        {
                                            this.Stress = Math.Min(100, this.Stress + 1); 
                                        }
                                    }
                                    else
                                    {
                                        failedAttempts++;
                                        this.Stress = Math.Min(100, this.Stress + 2); 
                                    }
                                }
                            }
                        }
                    }
                    else 
                    {
                        var refinery = GetRefineryRecipe(this.JobClass);
                        if (refinery.Input != null && CheckAndUseEquipment(town, typeof(Tongs), 1))
                        {
                            for (int i = 0; i < workCycles; i++)
                            {
                                if (failedAttempts >= 5) break;
                                var buyResult = TryBuyItem(town, refinery.Input, refinery.Cost);
                                if (buyResult.Success && buyResult.AmountBought >= refinery.Cost)
                                {
                                    failedAttempts = 0;
                                    int actualYield = (int)(refinery.Yield * Math.Max(1.0, pFactor));
                                    int price = (int)(buyResult.TotalCost * 1.5) + (int)(this.PrimarySkill / 5.0);
                                    
                                    this.Gold += price;
                                    town.Wealth -= price;
                                    town.SupplyItem(refinery.Output, actualYield, price);
                                    jobSuccess = true;
                                }
                                else { failedAttempts++; this.Stress = Math.Min(100, this.Stress + 2); }
                            }
                        }
                    }
                    break;

                case 300: // [300번대] 전투직 전리품
                    int targetFame = (int)(this.PrimarySkill * 100 * pFactor); 
                    int durabilityLoss = 1 + (targetFame / 3000); 

                    if (CheckAndUseEquipment(town, typeof(Broadsword), durabilityLoss) && CheckAndUseEquipment(town, typeof(PlateChest), durabilityLoss))
                    {
                        int totalGoldPool = (int)((10 + Utility.RandomMinMax(targetFame / 30, targetFame / 15)) * pFactor);
                        this.Gold += totalGoldPool; 
                        
                        Type[] lootTable = [typeof(Bone), typeof(Hides), typeof(RawRibs)];
                        Type loot = lootTable[Utility.Random(lootTable.Length)];
                        
                        int lootAmount = (int)((2 + (this.PrimarySkill / 15.0)) * pFactor * 10); 
                        TrySellItem(town, loot, Utility.RandomMinMax(lootAmount / 2, lootAmount));
                        jobSuccess = true;
                    }
                    break;

                case 500: // [500번대] 귀족 사치품 소비
                    int salary = (int)(((int)this.Rank * 100 + 100) * pFactor); 
                    if (town.Wealth > salary)
                    {
                        town.Wealth -= salary;
                        this.Gold += salary;
                        TryBuyItem(town, typeof(GoldRing), (int)(2 * pFactor)); 
                        jobSuccess = true;
                    }
                    break;

                case 600: // [600번대] 상인 무역
                    if (CheckAndUseEquipment(town, typeof(PackHorse))) 
                    {
                        if (this is VirtualCitizen merchant600)
                        {
                            int tradeCapacity = (int)(1000 * pFactor);
                            var tradeResult = VirtualTradeSystem.ExecuteTradeRoute(merchant600, town, tradeCapacity);
                            if (tradeResult.Success) jobSuccess = true;
                        }
                    }
                    break;

                case 700: // [700번대] 종교 소모품
                    if (TryBuyItem(town, typeof(Bandage), (int)(2 * pFactor)).Success &&
                        TryBuyItem(town, typeof(Candle), (int)(2 * pFactor)).Success) 
                    {
                        int donation = (int)((Utility.RandomMinMax(10, 30) + (int)this.PrimarySkill) * pFactor);
                        if (town.Wealth >= donation) { town.Wealth -= donation; this.Gold += donation; }
                        TrySellItem(town, typeof(GraveDust), (int)(5 * pFactor));
                        jobSuccess = true;
                    }
                    break;

                case 800: // [800번대] 예술/유흥
                    if (CheckAndUseEquipment(town, typeof(Lute))) 
                    {
                        this.Gold += (int)((Utility.RandomMinMax(15, 40) + (int)(this.PrimarySkill / 2)) * pFactor);
                        TrySellItem(town, typeof(Cake), (int)(5 * pFactor)); 
                        jobSuccess = true;
                    }
                    break;

                case 900: // [900번대] 해양 (채집 유사 30회)
                    if (this is VirtualCitizen maritime900)
                    {
                        for (int i = 0; i < workCycles; i++)
                        {
                            var result = VirtualTradeSystem.ExecuteHarvestAndSell(maritime900, town, 4); 
                            if (result.Success) jobSuccess = true;
                        }
                        TrySellItem(town, typeof(RawFishSteak), (int)((10 + Utility.Random(15)) * pFactor));
                    }
                    break;

                case 1100: // [1100번대] 범죄 (장물 유통)
                    if (CheckAndUseEquipment(town, typeof(Dagger))) 
                    {
                        int stolenGold = (int)((Utility.RandomMinMax(20, 80) + (int)this.PrimarySkill) * pFactor);
                        if (town.Wealth >= stolenGold)
                        {
                            town.Wealth -= stolenGold; 
                            this.Gold += stolenGold;
                            TrySellItem(town, typeof(GoldRing), (int)(2 * pFactor));
                            jobSuccess = true;
                        }
                    }
                    break;
            }

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