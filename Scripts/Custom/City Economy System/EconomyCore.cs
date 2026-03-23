using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
	public enum NpcJobClass
	{
		// --- [100] Peasant: 기초 자원 추출 및 단순 노무 (Raw Materials) ---
		Pauper = 100, Laborer, StreetSweeper, WaterCarrier, NightSoilMan, RatCatcher, Beggar, ChimneySweep, Lamplighter, LinkBoy, GraveDigger_Basic, Scullion, Messenger_Foot, KennelMaid, GongFarmer, GrainFarmer, VegetableFarmer, GourdFarmer, Orchardist, CitrusGrower, VineyardWorker, BerryPicker, Herbalist, MushroomGatherer, Beekeeper, CoastalFisher, DeepSeaFisher_Basic, Crabber, OysterDiver_Basic, SeaweedCollector, BeachComber, SaltGatherer, Mudlarker, Shepherd, Swineherd, PoultryFarmer, CattleDrover, StableHand, DairyWorker, GooseHerd, DonkeyDriver, HorseGroom_Basic, Woodcutter, BarkCollector, ResinGatherer, SurfaceMiner, SandDigger, CharcoalBurner, PeatCutter, StoneQuarryman, FlintKnapper, Trapper, BirdHunter, BigGameHunter, BoneCollector, FeatherPlucker, ApprenticeLaborer, ApprenticeGroom, ApprenticeScullery,

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
		public NpcJobClass JobClass { get; set; }
		public NpcRank Rank { get; set; }
		public int Gold { get; set; }
		public int Hunger { get; set; }
		public int Stress { get; set; } // 새로운 욕구: 스트레스 추가

		public VirtualAgent(NpcJobClass job, NpcRank rank) 
		{ 
			JobClass = job; 
			Rank = rank; 
			Gold = CalculateStartingGold(job, rank);
			Hunger = Utility.RandomMinMax(0, 30);
			Stress = Utility.RandomMinMax(0, 20); // 초기 스트레스 설정
		}

		public VirtualAgent(GenericReader reader) 
		{ 
			int version = reader.ReadInt(); 
			JobClass = (NpcJobClass)reader.ReadInt(); 
			Rank = (NpcRank)reader.ReadInt(); 
			Gold = reader.ReadInt(); 
			Hunger = reader.ReadInt();

			if (version >= 1)
			{
				Stress = reader.ReadInt();
			}
		}

		public virtual void Serialize(GenericWriter writer) 
		{ 
			writer.Write(1); // 버전 1로 업그레이드 (Stress 변수 추가 반영)
			writer.Write((int)JobClass); 
			writer.Write((int)Rank); 
			writer.Write(Gold); 
			writer.Write(Hunger);
			writer.Write(Stress);
		}

		public static int CalculateStartingGold(NpcJobClass job, NpcRank rank)
		{
			// 직업 번호를 100으로 나눈 후 100을 곱해 메인 그룹 단위로 변환 (예: 412 -> 400)
			int jobGroup = ((int)job / 100) * 100;

			int baseGold = jobGroup switch 
			{ 
				100 => 100,  // Peasant
				200 => 300,  // Producer
				300 => 500,  // Warrior
				400 => 800,  // Mage
				500 => 2000, // Noble
				600 => 1500, // Merchant
				700 => 700,  // Religious
				800 => 400,  // Entertainer
				900 => 1000, // Maritime
				1000 => 600, // Scholar
				1100 => 200, // Criminal
				_ => 100 
			};

			int rankMultiplier = rank switch 
			{ 
				NpcRank.Novice => 1, 
				NpcRank.Journeyman => 2, 
				NpcRank.Expert => 5, 
				NpcRank.Master => 10, 
				_ => 1 
			};

			return baseGold * rankMultiplier;
		}

		public ItemTag ClassifyItem(Item item)
		{
			// 생존 필수재: 음식, 음료, 가방 등
			if (item is Food || item is BaseBeverage || item is Backpack || item is Pouch || item is Candle || item is Torch) 
				return ItemTag.Essential;
			
			// 사치품: 보석, 악기, 꿀, 케이크, 장신구 등 (스트레스 해소용)
			if (item is BaseJewel || item is BaseInstrument || item is Cake || item is JarHoney) 
				return ItemTag.Luxury;
			
			// 도구 및 장비: 무기, 방어구, 작업 도구, 의류
			if (item is BaseWeapon || item is BaseArmor || item is BaseTool || item is BaseClothing)
				return ItemTag.Tool;

			// 원자재 및 소모품: 시약, 주괴, 목재, 스크롤 등
			if (item is BaseReagent || item is BaseIngot || item is Log || item is BlankScroll)
				return ItemTag.Material;

			return ItemTag.None;
		}
		// ==============================================================================
		// [시간 단위(Hourly) NPC 스케줄 및 경제 로직]
		// ==============================================================================

		// 서버의 시간 관리자(타이머)가 게임 내 시간(currentHour: 0~23)을 던져주며 매 시간 호출
		public virtual void HourlyRoutine(TownEconomy town, int currentHour)
		{
			if (town == null) return;

			// 1. 숨만 쉬어도 매 시간 배고픔이 조금씩 증가 (하루 24시간 기준)
			this.Hunger += Utility.RandomMinMax(1, 3); 

			// ---------------------------------------------------------
			// [식사 판정] - 배고픔이 20 이상이면 언제든 식사 시도
			// ---------------------------------------------------------
			if (this.Hunger >= 20)
			{
				ConsumeEssential(town);
			}

			// ---------------------------------------------------------
			// [근무 시간] - 오전 9시 ~ 오후 6시 (09:00 ~ 18:00)
			// ---------------------------------------------------------
			if (currentHour >= 9 && currentHour < 18)
			{
				// 번아웃(스트레스 90 이상)이 아니면 일함
				if (this.Stress < 90)
				{
					// 매 시간 일하는 건 너무 가혹하므로, 확률적으로 일함(예: 50% 확률로 작업)
					if (Utility.RandomBool()) 
					{
						ProcessJob(town);
					}
				}
				else
				{
					// 번아웃 상태면 일 안 하고 배만 더 고파짐
					this.Hunger += 2; 
				}
			}

			// ---------------------------------------------------------
			// [여가 시간] - 저녁 6시 ~ 밤 11시 (18:00 ~ 23:00)
			// ---------------------------------------------------------
			else if (currentHour >= 18 && currentHour <= 23)
			{
				// 퇴근 후 스트레스가 40 이상 쌓여있다면 유흥/사치품으로 스트레스 해소
				if (this.Stress >= 40)
				{
					// 매 시간 탕진하는 걸 막기 위해 확률적으로 해소
					if (Utility.RandomDouble() < 0.3) // 30% 확률
					{
						RelieveStress(town);
					}
				}
			}

			// ---------------------------------------------------------
			// [수면 시간] - 밤 12시 ~ 아침 8시 (00:00 ~ 08:00)
			// ---------------------------------------------------------
			else
			{
				// 잠을 자면서 스트레스가 자연적으로 조금씩 감소
				this.Stress -= Utility.RandomMinMax(1, 2);
				if (this.Stress < 0) this.Stress = 0;
			}
		}

		// --- 공통 거래 로직 (창고와 상호작용) ---
		protected bool TryBuyItem(TownEconomy town, Type itemType, int amount, out int cost)
		{
			cost = 0;
			if (itemType == null) return false;

			int unitPrice = town.GetPrice(itemType);
			int totalPrice = unitPrice * amount;

			// 내 돈이 충분하고 창고에 재고가 있을 때만 거래 성사
			if (this.Gold >= totalPrice && town.Warehouse[itemType].Stock >= amount)
			{
				this.Gold -= totalPrice;
				town.Wealth += totalPrice;      // 마을 금고로 돈이 들어감
				town.SupplyItem(itemType, -amount, -totalPrice);  // 창고 재고 차감 (구매)
				cost = totalPrice;
				return true;
			}
			return false;
		}

		protected bool TrySellItem(TownEconomy town, Type itemType, int amount, out int earnings)
		{
			earnings = 0;
			if (itemType == null) return false;

			int unitPrice = town.GetPrice(itemType);
			int totalPrice = unitPrice * amount;

			// (옵션) 마을 금고에 돈이 충분한지 체크할 수 있음
			this.Gold += totalPrice;
			town.Wealth -= totalPrice;      // 마을 금고에서 돈이 나감
			town.SupplyItem(itemType, amount, totalPrice);   // 창고 재고 증가 (납품)
			earnings = totalPrice;
			return true;
		}

		// --- 1. 필수 소비 (배고픔 해결) ---
		protected virtual void ConsumeEssential(TownEconomy town)
		{
			Type foodType = EconomyTagHelper.GetItemTypeByTag(town, ItemTag.Food_Basic, random: false);
			
			if (TryBuyItem(town, foodType, 1, out int cost))
			{
				this.Hunger = 0; // 배부름
				this.Stress -= 5; // 뭐라도 먹어서 스트레스 약간 감소
			}
			else
			{
				this.Stress += 15; // 밥을 못 사먹으면 스트레스 폭증
			}
		}

		// --- 2. 스트레스 해소 (사치품 및 유흥) ---
		protected virtual void RelieveStress(TownEconomy town)
		{
			int actionType = Utility.Random(100);

			if (actionType < 40) // 40%: 고급 음식 폭식
			{
				Type luxuryFood = EconomyTagHelper.GetItemTypeByTag(town, ItemTag.Food_Luxury, random: true);
				if (TryBuyItem(town, luxuryFood, 2, out _)) // 2개 충동구매
				{
					this.Stress -= 40;
					this.Hunger = 0;
				}
			}
			else if (actionType < 70) // 30%: 귀금속/사치품 플렉스
			{
				Type jewelry = EconomyTagHelper.GetItemTypeByTag(town, ItemTag.Jewelry, random: true);
				if (TryBuyItem(town, jewelry, 1, out _))
				{
					this.Stress -= 60; // 크게 돈 써서 스트레스 대폭 하락
				}
			}
			else // 30%: 여가/유흥비 탕진
			{
				Type entertainment = EconomyTagHelper.GetItemTypeByTag(town, ItemTag.Entertainment, random: true);
				if (TryBuyItem(town, entertainment, 1, out _))
				{
					this.Stress -= 50;
				}
				else // 물건을 못 샀다면 술값/도박으로 서비스 비용만 날림
				{
					int serviceFee = Utility.RandomMinMax(20, 100);
					if (this.Gold >= serviceFee)
					{
						this.Gold -= serviceFee;
						town.Wealth += serviceFee; 
						this.Stress -= 30; 
					}
				}
			}

			if (this.Stress < 0) this.Stress = 0; // 최소치 보정
		}

		public static int JobGetGroup(NpcJobClass JobClass) => ((int)JobClass / 100) * 100;

		// --- 3. 직업 노동 (구매 및 생산) ---
		protected virtual void ProcessJob(TownEconomy town)
		{
			// 직업 번호를 100단위로 묶어 그룹별로 처리
			int jobGroup = JobGetGroup(JobClass);
			int cost, earnings;

			switch (jobGroup)
			{
				case 100: // [Peasant] 노동자: 도구/식량 소비 -> 원자재 생산
					// 1. 도구(곡괭이, 도끼 등)나 소모품 구매
					Type tool = Utility.RandomBool() ? typeof(Pickaxe) : typeof(Axe);
					TryBuyItem(town, tool, 1, out cost);

					// 2. 원자재 납품 (철광석, 나무, 옥수수, 고철 등)
					Type rawMaterial = Utility.RandomList(typeof(IronOre), typeof(Log), typeof(EarOfCorn), typeof(PigIron));
					TrySellItem(town, rawMaterial, Utility.RandomMinMax(5, 15), out earnings);
					
					this.Stress += Utility.RandomMinMax(10, 20); // 육체 노동 스트레스
					break;

				case 200: // [Producer] 가공직: 원자재 소비 -> 중간재/완제품 생산
					// 1. 원자재 구매 (철광석, 나무, 유황재 등)
					Type materialToBuy = Utility.RandomList(typeof(IronOre), typeof(Log), typeof(SulfurousAsh));
					TryBuyItem(town, materialToBuy, Utility.RandomMinMax(10, 20), out cost);

					// 2. 가공품 납품 (철괴, 널빤지, 빈 병, 못 등)
					Type productToSell = Utility.RandomList(typeof(IronIngot), typeof(Board), typeof(Bottle), typeof(Nails));
					TrySellItem(town, productToSell, Utility.RandomMinMax(5, 10), out earnings);
					
					this.Stress += Utility.RandomMinMax(15, 25);
					break;

				case 300: // [Warrior] 전투직: 장비 소비 -> 전리품 생산
					// 1. 악성 재고 무기 및 방어구 랜덤 구매 (소각)
					Type weapon = EconomyTagHelper.GetItemTypeByTag(town, ItemTag.Weapon_Sword, random: true);
					Type armor = EconomyTagHelper.GetItemTypeByTag(town, ItemTag.Armor_Plate, random: true);
					TryBuyItem(town, weapon, 1, out _);
					TryBuyItem(town, armor, 1, out _);

					// 2. 사냥 부산물 납품
					Type loot = Utility.RandomList(typeof(Bone), typeof(Hides), typeof(DragonBlood));
					TrySellItem(town, loot, Utility.RandomMinMax(2, 8), out earnings);
					
					this.Stress += Utility.RandomMinMax(20, 30); // 전투 스트레스 큼
					break;

				case 400: // [Mage] 마법직: 빈 스크롤/시약 소비 -> 마법 물품 생산
					// 1. 방치된 특정 스크롤이나 시약 대량 구매
					Type blankOrReagent = EconomyTagHelper.GetItemTypeByTag(town, ItemTag.Magic_Scroll, random: false); 
					TryBuyItem(town, blankOrReagent, Utility.RandomMinMax(3, 10), out cost);

					// 2. 포션이나 리콜 룬 등 고부가가치 아이템 납품
					Type magicProduct = Utility.RandomList(typeof(RecallRune), typeof(GreaterHealPotion), typeof(NoxCrystal));
					TrySellItem(town, magicProduct, Utility.RandomMinMax(1, 3), out earnings);
					
					this.Stress += Utility.RandomMinMax(10, 20); // 정신적 스트레스
					break;

				case 500: // [Noble] 귀족: 고급 사치품/서류 소비 -> 공식 문서 납품
					// 1. 고가 악기나 장신구, 행정 서류 소비 (Gold Sink)
					Type nobleLuxury = EconomyTagHelper.GetItemTypeByTag(town, ItemTag.Luxury, random: true);
					TryBuyItem(town, nobleLuxury, 1, out cost);

					// 2. 고용 계약서나 자산 증서 발급 (권한 행사)
					Type officialDoc = Utility.RandomBool() ? typeof(ContractOfEmployment) : typeof(TanBook);
					TrySellItem(town, officialDoc, 1, out earnings);
					
					this.Stress += Utility.RandomMinMax(8, 15); // 꿀빠는 직업이라 스트레스 적음
					break;

				case 600: // [Merchant] 상인: 운송 수단/벌크 소비 -> 상품 증서(Deed) 유통
					// 1. 팩호스나 무작위 대량 물품 구매
					TryBuyItem(town, typeof(PackHorse), 1, out cost);
					Type bulkGoods = EconomyTagHelper.GetItemTypeByTag(town, ItemTag.Material, random: true);
					TryBuyItem(town, bulkGoods, 20, out _);

					// 2. 상품 증서(CommodityDeed)로 묶어서 납품
					TrySellItem(town, typeof(CommodityDeed), Utility.RandomMinMax(5, 10), out earnings);
					
					this.Stress += Utility.RandomMinMax(10, 20);
					break;

				case 700: // [Religious] 종교/치료: 양초/붕대/시약 소비 -> 시체 부산물 납품
					// 1. 양초와 붕대 대량 소비
					TryBuyItem(town, typeof(Candle), Utility.RandomMinMax(5, 10), out _);
					TryBuyItem(town, typeof(Bandage), Utility.RandomMinMax(5, 10), out _);

					// 2. 묘지 관리 및 치료의 결과물 납품
					Type graveLoot = Utility.RandomBool() ? typeof(GraveDust) : typeof(Bone);
					TrySellItem(town, graveLoot, Utility.RandomMinMax(2, 5), out earnings);
					
					this.Stress += Utility.RandomMinMax(15, 20);
					break;

				case 800: // [Entertainer] 예술/유흥: 악기/의류/식음료 맹렬한 소비 (최종 소각장)
					// 1. 넘쳐나는 악기류, 천, 염료 등을 구매해서 연주/공연으로 없애버림
					Type instrument = EconomyTagHelper.GetItemTypeByTag(town, ItemTag.Entertainment, random: true);
					TryBuyItem(town, instrument, 1, out cost);
					TryBuyItem(town, typeof(Dyes), 1, out _);

					// 생산은 거의 없음 (가끔 파이나 쿠키 정도)
					if (Utility.RandomBool())
						TrySellItem(town, typeof(ApplePie), 1, out earnings);
					
					this.Stress += Utility.RandomMinMax(10, 25);
					break;

				case 900: // [Maritime] 해양: 선박/항해 도구 소비 -> 어획물 납품
					// 1. 낚시 그물, 육분의 등 항해/어업 도구 구매
					TryBuyItem(town, typeof(SpecialFishingNet), Utility.RandomMinMax(1, 3), out cost);
					TryBuyItem(town, typeof(Sextant), 1, out _);

					// 2. 대량의 송어나 진주 납품
					Type seaLoot = Utility.RandomList(typeof(Trout), typeof(BlackPearl));
					TrySellItem(town, seaLoot, Utility.RandomMinMax(10, 30), out earnings);
					
					this.Stress += Utility.RandomMinMax(15, 25);
					break;

				case 1000: // [Scholar] 학자: 빈 종이/도구 소비 -> 지식 서적 납품
					// 1. 스크롤, 빈 지도, 펜 소비
					TryBuyItem(town, typeof(BlankScroll), Utility.RandomMinMax(5, 15), out cost);
					TryBuyItem(town, typeof(ScribesPen), 1, out _);

					// 2. 완성된 책 납품
					Type bookToSell = Utility.RandomList(typeof(RedBook), typeof(BlueBook), typeof(BlankMap));
					TrySellItem(town, bookToSell, Utility.RandomMinMax(1, 3), out earnings);
					
					this.Stress += Utility.RandomMinMax(11, 15);
					break;

				case 1100: // [Criminal] 지하 경제: 범죄 도구 소비 -> 장물 재납품
					// 1. 단검, 독포션 등 범죄 도구 구매
					TryBuyItem(town, typeof(Dagger), 1, out cost);

					// 2. 훔친 사치품이나 위조 증서를 창고에 팔아치움 (장물 세탁)
					Type stolenGoods = EconomyTagHelper.GetItemTypeByTag(town, ItemTag.Jewelry, random: true);
					TrySellItem(town, stolenGoods, 1, out earnings);
					
					this.Stress += Utility.RandomMinMax(20, 35); // 걸릴까 봐 스트레스 심함
					break;
			}
			this.Stress -= 10;
		}
	}
}