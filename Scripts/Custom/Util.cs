using System;
using System.Text;
using Server;
using Server.Mobiles;
using Server.Items;
using Server.Engines.Craft;
using Server.Accounting;
using Server.Engines.VeteranRewards;
using System.Collections.Generic;
using Server.Gumps;
using Server.Network;

namespace Server.Misc
{
	public class Util
	{
		#region Effect
		public static void Good_Effect( Mobile from )
		{
			Effects.PlaySound( from.Location, from.Map, 0x243 );
			Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 6, from.Y - 6, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
			Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 4, from.Y - 6, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
			Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 6, from.Y - 4, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
		}
		
		public static void ItemGet_Effect( Mobile from )
		{
			Effects.PlaySound( from.Location, from.Map, 0x243 );
			Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 6, from.Y - 6, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 1153, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
			Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 4, from.Y - 6, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 1153, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
			Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 6, from.Y - 4, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 1153, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
		}
		public static void HiddenGet_Effect( Mobile from )
		{
			Effects.PlaySound( from.Location, from.Map, 0x243 );
			Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 6, from.Y - 6, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 1166, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
			Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 4, from.Y - 6, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 1166, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
			Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 6, from.Y - 4, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 1166, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
		}		
		
		public static void LevelUp_Effect( Mobile from )
		{
			for( int i = 0; i < 10; ++i)
			{
				Point3D ourLoc = from.Location;

				Point3D startLoc = new Point3D(ourLoc.X, ourLoc.Y, ourLoc.Z + 10);
				Point3D endLoc = new Point3D(startLoc.X + Utility.RandomMinMax(-2, 2), startLoc.Y + Utility.RandomMinMax(-2, 2), startLoc.Z + 32);

				Effects.SendMovingEffect(new Entity(Serial.Zero, startLoc, from.Map), new Entity(Serial.Zero, endLoc, from.Map), 0x36E4, 5, 0, false, false);
				Timer.DelayCall(TimeSpan.FromSeconds(1.0), new TimerStateCallback(FinishLaunch), new object[] { from, endLoc, from.Map });			
			}
		}
		
        private static void FinishLaunch(object state)
        {
            object[] states = (object[])state;

            Mobile from = (Mobile)states[0];
            Point3D endLoc = (Point3D)states[1];
            Map map = (Map)states[2];

            int hue = Utility.Random(40);

            if (hue < 8)
                hue = 0x66D;
            else if (hue < 10)
                hue = 0x482;
            else if (hue < 12)
                hue = 0x47E;
            else if (hue < 16)
                hue = 0x480;
            else if (hue < 20)
                hue = 0x47F;
            else
                hue = 0;

            if (Utility.RandomBool())
                hue = Utility.RandomList(0x47E, 0x47F, 0x480, 0x482, 0x66D);

            int renderMode = Utility.RandomList(0, 2, 3, 4, 5, 7);

            Effects.PlaySound(endLoc, map, Utility.Random(0x11B, 4));
            Effects.SendLocationEffect(endLoc, map, 0x373A + (0x10 * Utility.Random(4)), 16, 10, hue, renderMode);
        }
		

		#endregion
		
		#region Gump AutoTab
		public static int MaxpageCreate(int maxlist, int page, int step )
		{
			int maxpage = maxlist - page * step;
			if( maxpage > step )
				maxpage = step;
			
			return maxpage;
		}		
			
		#endregion
		
		
		#region item identified
		
		public static bool IdentifiedSuccess( int rank, int item_value )
		{
			if( Utility.RandomMinMax( 0, 199 ) - 200 < item_value - rank * 200 )
				return true;
			
			return false;
		}
		
		//유물 조각 수를 읽어서 레벨 및 조각 남은 수 파악하는 알고리즘
		/*
		* 구현은 아래 코드를 이용할 것
            Console.Write("값 : ");

            int total = Convert.ToInt32(Console.ReadLine());
            int piece;
            int level;

            total = Level_Calc(total, out level, out piece);
            Console.WriteLine("레벨 : {0}", level);
            Console.WriteLine("다음 레벨에 필요한 조각 수 :{0} / {1}", total, piece * 40);
            Console.WriteLine("");
		*/
		//total : 유물 조각 수, level ; 유물 조각에 따른 레벨, piece : 다음 레벨에 필요한 조각 수
		
        static int Level_Calc(int total, out int level, out int piece) 
        {
            level = 0;
            piece = 1;
            if (total >= 20020000) // 1001 * 500 * 40
            {
                piece = 0;
                level = 1000;
                return 0;
            }
            else
            {
                for (int i = 1; i < 1000; ++i)
                {
                    if (total >= i * 40)
                    {
                        level++;
                        piece += level;
                        total -= level * 40;
                    }
                    else
                        break;
                }
                return total;
            }
        }
		
		/*
		public static bool TierUpgradeSuccess( int tier, double skillvalue )
		{
			if( Utility.RandomDouble() < ( ( ( skillvalue - ( tier * 40 ) ) * 500 + 1000 ) / ( ( tier + 1 ) * ( tier + 1 ) * ( tier + 1 ) * ( tier + 1 ) ) ) * 0.01 )
				return true;
			else
				return false;
		}
		public static double[] TierUpgradeChance = 
		{ 1, 0.9, 0.7, 0.5, 0.1, 0.01 };
		public static bool TierUpgradeSuccess( int tier )
		{
			if( tier >= 6 )
				return false;
			if( Utility.RandomDouble() <= TierUpgradeChance[tier] )
				return true;
			else
				return false;
		}
		*/
		public static void ItemReOption( Mobile from, Item item, int rank )
		{
			if( item is IEquipOption )
			{
				IEquipOption equip = item as IEquipOption;
				if( rank > 0 )
					rank -= 3;
				
				if( rank == 0 )
					rank = 1;
				if( from is PlayerMobile )
				{
					PlayerMobile pm = from as PlayerMobile;
					//ItemCreate( item, rank, equip.PlayerConstructed, pm, equip.PrefixOption[99], equip.SuffixOption[99], true );
				}
			}
		}
		
		/*
		public static void ItemTierFail( Mobile from, Item item, int tier )
		{
			if( item is IEquipOption )
			{
				IEquipOption equip = item as IEquipOption;
				equip.MaxHitPoints -= tier;
				if( equip.MaxHitPoints < equip.HitPoints )
					equip.HitPoints = equip.MaxHitPoints;
				from.SendMessage("아이템 단계 상승에 실패하셨습니다");
				
				if( equip.MaxHitPoints <= 0 )
				{
					item.Delete();
					from.SendMessage("아이템이 파괴되었습니다!");
				}						
			}
		}
		
		public static void TierUpgrade( Mobile from, Item item )
		{
			if( item is IEquipOption )
			{
				IEquipOption equip = item as IEquipOption;
				int tier = equip.PrefixOption[99] + 1;
				if( tier > 6 )
				{
					from.SendMessage("가장 높은 단계입니다.");
					return;
				}
				else if( TierUpgradeSuccess( tier ) )
				{
					equip.PrefixOption[99]++;
					ItemGet_Effect(from);
					ItemReOption( from, item, (int)equip.ItemPower );
					from.SendMessage("아이템 단계 상승에 성공하였습니다!");
				}
				else
				{
					ItemTierFail( from, item, tier );
				}
			}
		}
		
		public static void TierUpgrade( Mobile from, Item item, int skillvalue)
		{
			if( item is IEquipOption )
			{
				IEquipOption equip = item as IEquipOption;
				int tier = equip.PrefixOption[99] + 1;
				if( tier > 6 )
				{
					from.SendMessage("가장 높은 단계입니다.");
					return;
				}
				if( skillvalue - ( 400 * equip.PrefixOption[99] ) < 0 )
				{
					from.SendMessage("장비학이 낮아 단계를 상승시킬 수 없습니다.");
					return;
				}
				double skill = ( skillvalue - ( 400 * equip.PrefixOption[99] ) ) * 0.025;

				if( skill < 0 )
					skill = 0;
				if( TierUpgradeSuccess( tier, skillvalue ) )
				{
					//아이템 옵션 다시 돌리기
					ItemGet_Effect(from);
					equip.PrefixOption[99]++;
					ItemReOption( from, item, (int)equip.ItemPower );
					from.SendMessage("아이템 단계 상승에 성공하였습니다!");
				}
				else
				{
					ItemTierFail( from, item, tier );
				}
				from.CheckSkill( SkillName.ArmsLore, 200 + tier * 125 );
			}
			else
			{
				from.SendMessage("장비만 단계를 향상시킬 수 있습니다.");
			}
		}
		*/
		
		#endregion
		
		public static string[] HarvestName =
		{
			"오류 아이템",
			"잉갓을",
			"판자를",
			"생선살을",
			"가죽을"
		};
		
		#region Harvest
		public static int HarvestMake( Mobile from, Item harvestitem, double difficulty, SkillName harvestskill )
		{
			int skillcheck = 0;
			int harvestAmount = 0;
			int realAmount = 0;
			if( harvestskill == SkillName.Mining )
			{
				skillcheck = 1;
			}
			else if( harvestskill == SkillName.Lumberjacking )
			{
				skillcheck = 2;
			}
			else if( harvestskill == SkillName.Fishing )
			{
				skillcheck = 3;
			}
			else if( harvestskill == SkillName.TasteID )
			{
				skillcheck = 4;
			}		
			if( skillcheck == 0 )
				return 0;
			PlayerMobile pm = from as PlayerMobile;
			{
				if( from.Hunger < 10 )
				{
					pm.LastObject = null;
					from.SendMessage("당신은 배가 고픕니다."); 
					return 0;
				}
				else if( pm.TimerList[71] == 0 )
				{
					pm.TimerList[71] = 5;
					pm.LastTarget = harvestitem;
				
					double minSkill = difficulty - 50.0;
					double maxSkill = difficulty;

					double skillpoint = minSkill + maxSkill;

					if (minSkill > from.Skills[harvestskill].Value )
					{
						from.SendMessage("당신은 {0} 생성하는데 실패합니다...", HarvestName[skillcheck]);
						return 0;
					}
					harvestAmount = 50 + pm.GoldPoint[5] * 2;
					if( harvestitem.Amount < harvestAmount )
						harvestAmount = harvestitem.Amount;

					if ( 50 + ( from.Skills[harvestskill].Value - difficulty ) * 2 > Utility.Random(100) )
					{
						if( from.Hunger < harvestAmount * 10 )
						{
							harvestAmount = from.Hunger / 10;
						}
						realAmount = harvestAmount;
						
						from.SendMessage("{0} 생성하는데 성공했습니다.", HarvestName[skillcheck]);
					}						
					else
					{
						harvestAmount = 1;
						from.SendMessage("{0} 생성하는데 실패합니다.", HarvestName[skillcheck]);
						realAmount = 0;
					}					
					if( harvestAmount >= harvestitem.Amount )
					{
						pm.Loop = false;
						pm.LastTarget = null;
						harvestitem.Delete();
					}
					else
					{
						harvestitem.Amount -= harvestAmount;
						//Timer.DelayCall(TimeSpan.FromSeconds(0.5), OnDoubleClick, from);
					}
					from.CheckSkill( harvestskill, skillpoint * harvestAmount );
					pm.Getgoldpoint( (int)skillpoint * harvestAmount);
					from.Hunger -= harvestAmount * 10;
					harvestAmount = realAmount;
				}
			}
			return harvestAmount;
		}
		#endregion
		public static int RankCheck( int item )
		{
			if( item >= 100 )
				return 8;
			if( item >= 80 )
				return 7;
			if( item >= 60 )
				return 6;
			if( item >= 40 )
				return 5;
			if( item >= 20 )
				return 4;
			if( item >= 10 )
				return 3;
			if( item >= 5 )
				return 2;
			if( item >= 2 )
				return 1;
			return 0;
		}

		//몬스터 등급 계산
		public static int MonsterTierCalc(BaseCreature bc )
		{
			if( bc.Boss )
				return 5;
			else if( bc.Grade == 7 )
				return 4;
			else if( bc.Grade == 6 )
				return 3;
			else if( bc.Grade > 1 )
				return 2;
			return 1;
		}

		//몬스터 군중제어 회복 시간
		public static double MonsterTierCrowdControlRecovery(BaseCreature from )
		{
			switch( MonsterTierCalc(from) )
			{
				case 5:
					return 0.1;
				case 4:
					return 0.25;
				case 3:
					return 0.34;
				case 2:
					return 0.55;
			}
			return 1.0;
		}
		


		//크리티컬 보정 계산
		public static double MonsterTierCriticalDamage(BaseCreature bc )
		{
			double criticalDamage = 0.0;
			switch(MonsterTierCalc(bc))
			{
				case 5:
				{
					criticalDamage = 1.0;
					break;
				}
				case 4:
				{
					criticalDamage = 0.75;
					break;
				}
				case 3:
				{
					criticalDamage = 0.5;
					break;
				}
				case 2:
				{
					criticalDamage = 0.2;
					break;
				}
			}
			return criticalDamage;
			
		}

		public static int UniqueDice(int count, int max)
		{
			int selectNumber = Utility.RandomMinMax(0, max);
			while(true)
			{
				if( selectNumber != count )
					return selectNumber;
				else
					selectNumber = Utility.RandomMinMax(0, max);
			}
			return selectNumber;
		}
		
		#region 경험치 계산
		public static readonly int MaxLevel = 250;
		public static int Level( int point )
		{
			if( point > MaxLevel * MaxLevel * 10000 )
			{
				point = MaxLevel * MaxLevel * 10000;
				return MaxLevel;
			}
			return (int)( Math.Sqrt(point) / 100 );
		}
		public static int NextLevel( int point )
		{
			return (int)( Math.Pow( ( Level(point) + 1) * 100, 2 ) ) - point;
		}
		#endregion
		public static string GetName(Item item)
		{
			string name;
			if (!string.IsNullOrEmpty(item.Name))
				name = item.Name;
			else
				name = "#" + item.LabelNumber.ToString();		

			return name;			
		}

		public static string GetName(int labelnumber)
		{
			return "#" + labelnumber.ToString();		
		}
	
		public static int MonsterItemGrade(int luckbonus, int MaxBonus)
		{
			double dice = Math.Sqrt(luckbonus) * 0.1 + Math.Pow( Utility.RandomDouble() * 0.1, 7 ) * ( MaxBonus - 1 ) * 10000000;

			if( dice >= 99.9 )
				return 8;
			else if( dice >= 99 )
				return 7;
			else if( dice >= 90 )
				return 6;
			else if( dice >= 75 )
				return 5;
			else if( dice >= 50 )
				return 4;
			else
				return 0;
		}
		
		public static double[] ItemRankList =
		{
			0.5, 0.15, 0.01, 0.0001
		};
		
		public static double[] ItemRankLuckBonus = 
		{
			0.002, 0.0005, 0.00009, 0.0000004
		};
		
		public static int ResourceNumberToNumber( int resource )
		{
			int returnvalue = 0;
			if( resource == 1 )
				returnvalue = 0;
			else if( resource <= 9 )
				returnvalue = resource - 2;
			else if( resource <= 107 )
				returnvalue = resource - 101;
			else if( resource <= 207 )
				returnvalue = 0;
			else if( resource <= 307 )
				returnvalue = resource - 301;
			return returnvalue;
		}
		
		private static int[] ResourceTier =
		{
			0, 50, 125, 225, 350, 500, 675
		};
		
		public static int ItemTierMaker( int fame, int rank, int resource, Mobile from = null )
		{
			if( fame >= 30000 )
				fame = 30000;

			//int tier = Utility.RandomMinMax( fame, fame + 12000 );
			//tier = 1 + tier / 10000;

			double value = fame * 0.0002;
			
			int tier = (int)Utility.RandomMinMax( (double)value - 2, (double)value + 0.2 );
			
			if( tier > 6 )
				tier = 6;

			if( tier < 0 )
				tier = 0;
			
			if( from != null )
				from.CheckSkill( SkillName.ArmsLore, ( 200 + tier * 100 + ResourceTier[resource] ) * 2 );

			return tier;
		}
		

		public static Type[] Monster_1Tier_Artifact =
		{
			typeof( TomeOfEnlightenment ), typeof( PilferedDancerFans ), typeof( PeasantsBokuto ), typeof( DragonNunchaku ), typeof( DemonForks ), 
			typeof( DaimyosHelm ), typeof( BlackLotusHood ), typeof( ArmsOfTacticalExcellence ), typeof( AncientSamuraiDo ), typeof( AncientFarmersKasa )
		};
		
		public static Type[] Monster_2Tier_Artifact =
		{
			typeof( CompassionsEye ), typeof( DespicableQuiver ), typeof( UnforgivenVeil ), typeof( DarkenedSky ), typeof( KasaOfTheRajin ),
			typeof( Stormgrip ), typeof( SwordOfTheStampede ), typeof( SwordsOfProsperity ), typeof( TheHorselord ), typeof( TomeOfLostKnowledge ), 
			typeof( WindsEdge ), typeof( RuneBeetleCarapace )
		};

		/*
		public static int LegendAndMysticCheck( Item item, int rank )
		{
			for( int i = 0; i < m_AllLegendItem.Length; i++ )
			{
				if( item.GetType() == m_AllLegendItem[i] )
				{
					rank = 4;
					break;
				}
			}
			return rank;
		}
		
		public static bool LegendAndMysticMake( Item item, int regionCheck = -1 )
		{
			List <Type> UpgradeItem = new List<Type>();

			if( regionCheck == null )
				regionCheck = m_AllLegendItem;

			for( int i = 0; i < regionCheck.GetLength(0); ++i)
			{
				if( item.GetType() == regionCheck[i].GetType().BaseType )
				{
					UpgradeItem.Add(regionCheck[i]);
				}
			}
			
			if( UpgradeItem.Count > 0 )
			{
				item = Activator.CreateInstance(UpgradeItem[Utility.Random(UpgradeItem.Count)].GetType()) as Item;
				return true;
			}
			
			return false;
		}		
		*/
		public static int RepairSkillCheck( double level )
		{
			int Tier = 0;
			if( level < 50 )
				Tier = 0;
			else if( level < 100 )
				Tier = 1;
			else if( level < 125 )
				Tier = 2;
			else if( level < 150 )
				Tier = 3;
			else if( level < 175 )
				Tier = 4;
			else if( level < 200 )
				Tier = 5;
			else
				Tier = 6;
			return Tier;
		}

        public static double NewItemDice(int minValue, int maxValue)
		{
			double dice = ( minValue + Math.Pow( Utility.RandomDouble() * 0.1, 7 ) * ( maxValue - minValue ) * 10000000 ) * 0.01;
			return dice * 100;
		}

        public static int OptionLoop()
        {
			int loop = 3;
            if (Utility.RandomDouble() < 0.1)
                loop = 4;
			/*
            int loop = 2 * (rank % 4 ) - 1;
			if( rank >= 4 )
				loop = rank--;
			

            return loop;
			*/
			return loop;
        }
		
		//몬스터 아이템 드랍 설정
		#region MonsterItemDrop
		public static readonly Type[,] m_MonsterItemDrop = new[,]
		{
			//몬스터 1052085 시작		일반 아이템, 			희귀 아이템
			{ typeof(Skeleton), 		typeof(Bone), 			typeof(MonsterStatuette)},
			{ typeof(Zombie), 			typeof(FertileDirt), 	typeof(MonsterStatuette)},
			{ typeof(Spectre), 			typeof(GraveDust), 		typeof(AncestralGravestone)},
			{ typeof(Wraith), 			typeof(Nightshade), 	typeof(TombstoneOfTheDamned)},
			{ typeof(Eagle), 			typeof(Feather), 		typeof(CoralTheOwl)},
			{ typeof(Mongbat), 			typeof(BatWing), 		typeof(MonsterStatuette)},
			{ typeof(Turkey), 			typeof(Feather), 		typeof(TurkeyDinner)},
			{ typeof(GiantTurkey), 		typeof(Feather), 		typeof(TurkeyPlatter)},
			{ typeof(GiantSpider), 		typeof(SpidersSilk), 	typeof(MonsterStatuette)},
			{ typeof(GiantBlackWidow), 	typeof(SpidersSilk), 	typeof(DecorativeBlackwidowDeed)},
			{ typeof(DreadSpider), 		typeof(SpidersSilk), 	typeof(DreadSpiderSilk)},
			{ typeof(TrapdoorSpider), 	typeof(SpidersSilk), 	typeof(SpiderCarapace)},
			{ typeof(WolfSpider), 		typeof(SpidersSilk), 	typeof(Web)},
			{ typeof(GiantDreadSpider), typeof(SpidersSilk), 	typeof(DreadSpiderStatuette)},
			{ typeof(Harpy), 			typeof(Feather), 		typeof(JewelryBox)},
			{ typeof(VampireBat), 		typeof(BatWing), 		typeof(WallBlood)},
			{ typeof(StoneHarpy), 		typeof(Feather), 		typeof(EnchantedGraniteCartAddonDeed)},
			{ typeof(Mummy), 			typeof(Bandage), 		typeof(ExcellentIronMaiden)},
			{ typeof(RottingCorpse), 	typeof(Bandage), 		typeof(IronMaidenDeed)},
			{ typeof(Bogling), 			typeof(Engines.Plants.Seed),typeof(DecorativePlant)},
			{ typeof(Corpser), 			typeof(ParasiticPlant),	typeof(PottedCactusDeed)},
			{ typeof(Crane), 			typeof(Feather),		typeof(CraneZooStatuette)},
			{ typeof(Treefellow), 		typeof(BarkFragment),	typeof(TreeStumpDeed)},
			{ typeof(Reaper), 			typeof(MandrakeRoot),	typeof(MonsterStatuette)},
			{ typeof(BogThing), 		typeof(BarkFragment),	typeof(Engines.Plants.SeedBox)},
			{ typeof(Ettin), 			typeof(MiniHealPotion),	typeof(MonsterStatuette)},
			{ typeof(HeadlessOne), 		typeof(MiniHealPotion),	typeof(FlamingHeadDeed)},
			{ typeof(Lizardman), 		typeof(MiniHealPotion),	typeof(MonsterStatuette)},
			{ typeof(LizardmanDefender),typeof(MiniCurePotion),	typeof(RedPoinsettia)},
			{ typeof(Troll),	 		typeof(MiniRefreshPotion),typeof(MonsterStatuette)},
			{ typeof(Cyclops),	 		typeof(LesserHealPotion),typeof(DecoRocks2)},
			{ typeof(Centaur),	 		typeof(Arrow),			typeof(DecorativeBow)},
			{ typeof(Ogre),		 		typeof(MiniRefreshPotion),	typeof(MonsterStatuette)},
			{ typeof(OgreLord),		 	typeof(RefreshPotion),	typeof(MiniHouseDeed)},
			{ typeof(BoneKnight),		typeof(Bone),			typeof(HangingSkeletonDeed)},
			{ typeof(BoneMagi),			typeof(BlackPearl),		typeof(RedPoinsettia)},
			{ typeof(PestilentBandage),	typeof(Bandage),		typeof(WoodenCoffinDeed)},
			{ typeof(SkeletalKnight),	typeof(Bone),			typeof(HangingSwordsDeed)},
			{ typeof(SkeletalMage),		typeof(BlackPearl),		typeof(WhitePoinsettia)},
			{ typeof(SkeletalCat),		typeof(Bone),			typeof(SkeletalCatStatue)},
			{ typeof(PatchworkSkeleton),typeof(Bone),			typeof(SkeletonPortrait)},
			{ typeof(Ghoul),			typeof(PigIron),		typeof(DisturbingPortraitDeed)},
			{ typeof(Shade),			typeof(PigIron),		typeof(CreepyPortraitDeed)},
			{ typeof(BoneDemon),		typeof(DaemonBone),		typeof(BoneTableDeed)},
			{ typeof(SkeletalLich),		typeof(DaemonBlood),	typeof(SkeletalHangmanAddonDeed)},
			{ typeof(Lich),				typeof(NoxCrystal),		typeof(MonsterStatuette)},
			{ typeof(AncientLich),		typeof(PrimalLichDust),	typeof(MonsterStatuette)},
			{ typeof(LichLord),			typeof(NoxCrystal),		typeof(LichPainting)},
			{ typeof(SkeletalDragon),	typeof(DaemonBone),		typeof(Server.Engines.Shadowguard.WitheringBones)},
			{ typeof(Scorpion),			typeof(Nightshade),		typeof(LeatherDyeTub)},
			{ typeof(ClockworkScorpion),typeof(Nightshade),		typeof(RuinedClock)},
			{ typeof(FireElemental),	typeof(SulfurousAsh),	typeof(MonsterStatuette)},
			{ typeof(WaterElemental),	typeof(BlackPearl),		typeof(WaterWheelDeed)},
			{ typeof(AirElemental),		typeof(LesserAgilityPotion),typeof(BrokenFallenChairDeed)},
			{ typeof(Gazer),			typeof(LesserAgilityPotion),typeof(MonsterStatuette)},
			{ typeof(ElderGazer),		typeof(AgilityPotion),	typeof(SuitOfGoldArmorDeed)},
			{ typeof(PoisonElemental),	typeof(PoisonPotion),	typeof(SkullsOnPike)},
			{ typeof(BloodElemental),	typeof(GreaterHealPotion),typeof(BloodyPentagramDeed)},
			{ typeof(Beholder),			typeof(GreaterAgilityPotion),typeof(AnkhOfSacrificeDeed)},
			{ typeof(Sewerrat),			typeof(RatnedHides),	typeof(CheeseSlice)},
			{ typeof(BullFrog),			typeof(DernedHides),	typeof(Items.MusicBox.MusicBoxGears)},
			{ typeof(Alligator),		typeof(SernedHides),	typeof(MonsterStatuette)},
			{ typeof(GiantRat),			typeof(RatnedHides),	typeof(CheeseWedge)},
			{ typeof(GiantToad),		typeof(DernedHides),	typeof(DawnsMusicBox)},
			{ typeof(AcidElemental),	typeof(GreaterAgilityPotion),typeof(AcidProofRope)},
			{ typeof(EarthElemental),	typeof(GreaterAgilityPotion),typeof(MonsterStatuette)},
			{ typeof(BloodWorm),		typeof(Bloodmoss),		typeof(RunebookDyeTub)},
			{ typeof(EvilMage),			typeof(BlankScroll),	typeof(BlackDyeTub)},
			{ typeof(EvilMageLord),		typeof(BlankScroll),	typeof(SpecialDyeTub)},
			{ typeof(Brigand),			typeof(RawRibs),		typeof(WhiteClothDyeTub)},
			{ typeof(ElfBrigand),		typeof(Arrow),			typeof(BlazeDyeTub)},
			{ typeof(Kraken),			typeof(Rope),			typeof(WaterTile)},
			{ typeof(EttinLord), 		typeof(HealPotion),		typeof(WhiteLeatherDyeTub)},
			{ typeof(SkeletalMount),	typeof(Bone),			typeof(ChargerOfTheFallen)},
			{ typeof(Orc),				typeof(BolaBall),		typeof(MonsterStatuette)},
			{ typeof(OrcChopper),		typeof(Shaft),			typeof(FallenLogDeed)},
			{ typeof(OrcishMage),		typeof(BolaBall),		typeof(BrokenBookcaseDeed)},
			{ typeof(OrcCaptain),		typeof(BolaBall),		typeof(DecoBottlesOfLiquor)},
			{ typeof(OrcBomber),		typeof(SulfurousAsh),	typeof(DragonCannonDeed)},
			{ typeof(OrcScout),			typeof(Arrow),			typeof(RecipeScroll)},
			{ typeof(OrcishLord),		typeof(BolaBall),		typeof(RecipeScroll)},
			{ typeof(Titan),			typeof(RoastPig),		typeof(RecipeScroll)}
		};
		
		public static string[] exp_Type_Name =
		{
			"채집", "제작", "전투"
		};
		
		public static void LevelUpEffect(PlayerMobile pm, int getpoint, int exp_Type)
		{
			int savepoint = 0;
			switch(exp_Type)
			{
				case 0:
				{
					if( Level( pm.GoldPoint[0] ) >= MaxLevel )
						return;

					savepoint = pm.GoldPoint[0];
					pm.GoldPoint[0] += getpoint;
					if( pm.HasGump(typeof(GoldPointGump)) )
						pm.SendGump(new GoldPointGump(pm));
					break;
				}
				case 1:
				{
					if( Level( pm.GoldPoint[10] ) >= MaxLevel )
						return;

					savepoint = pm.GoldPoint[10];
					pm.GoldPoint[10] += getpoint;
					if( pm.HasGump(typeof(GoldPointGump)) )
						pm.SendGump(new GoldPointGump(pm));
					break;
				}
				case 2:
				{
					if( Level( pm.SilverPoint[0] ) >= MaxLevel )
						return;

					savepoint = pm.SilverPoint[0];
					pm.SilverPoint[0] += getpoint;
					if( pm.HasGump(typeof(SilverPointGump)) )
						pm.SendGump(new SilverPointGump(pm));
					break;
				}
			}
			if( getpoint + Level( savepoint ) >= NextLevel( savepoint ) )
			{
				if( getpoint + Level( savepoint ) >= MaxLevel )
					pm.DeathCheck = 0;
				LevelUp_Effect(pm);
				pm.SendMessage("레벨이 올랐습니다!");
				pm.ProcessDelta();
				pm.Delta(MobileDelta.Stat);
			}
			pm.SendMessage("{0} 경험치를 {1} 획득합니다!", exp_Type_Name[exp_Type], getpoint );
		}
		
		
		public static void HarvestReward( PlayerMobile pm, int harvestNumber )
		{
			int harvestrank = harvestNumber % 9;
			harvestrank = 50 + harvestrank * 40;
			LevelUpEffect(pm, harvestrank * 1000, 0);
		}
		public static void CraftReward( PlayerMobile pm, int harvestNumber )
		{
			LevelUpEffect(pm, 150000, 1);
		}	
		public static void MonsterFeatReward( PlayerMobile pm, int monsterNumber )
		{
			BaseCreature bc = null;
			bc = MonsterListCheck(monsterNumber);
			if( bc != null )
			{
				LevelUpEffect(pm, bc.Fame * 10, 2);
			}		
		}
		
		public static bool MonsterCheck( BaseCreature mob )
		{
			if( mob.ControlMaster == null && mob.SummonMaster == null )
				return true;
			else
				return false;
		}
		
		public static bool PetStat( BaseCreature bc, int exp, int petStat, int MonsterStat )
		{
			exp += MonsterStat;
			if( exp <= petStat * petStat )
			{
				LevelUp_Effect(bc);
				exp -= petStat * petStat;
				return true;
			}
			return false;
		}
		
		public static BaseCreature MonsterListCheck(int number)
		{
			BaseCreature bc = null;
			try
			{
				bc = Activator.CreateInstance(m_MonsterItemDrop[number, 0]) as BaseCreature;
			}
			catch
			{
			}
			return bc;
		}
		
		public static void RecipeScrollSelect(BaseCreature bc, RecipeScroll item)
		{
			if( bc is OrcScout )
			{
				item.RecipeID = 208;
			}
			if( bc is OrcScout )
			{
				item.RecipeID = 208;
			}
			if( bc is Titan )
			{
				item.RecipeID = 208;
			}
			
		}
		
		public static void MonsterStatuetteSelect(BaseCreature bc, MonsterStatuette item)
		{
			string bcName = bc.GetType().Name;
			for( int i = 0; i < Enum.GetValues(typeof(MonsterStatuetteType)).Length; i++ )
			{
				MonsterStatuetteType monster = (MonsterStatuetteType)i;
				if( bcName == monster.ToString() )
				{
					item.Type = monster;
					break;
				}
			}
		}
		
		public static int MonsterEquipItem(BaseCreature bc)
		{
			int number = 0;
			for( int i = 0; i < m_MonsterItemDrop.GetLength(0); i++)
			{
				if( bc.GetType() == m_MonsterItemDrop[i, 0] )
				{
					number += i + 1;
					break;
				}
			}
			return number;
		}		
		
		public static Type MonsterDropItem(BaseCreature bc)
		{
			Type type = null;
			for( int i = 0; i < m_MonsterItemDrop.GetLength(0); i++)
			{
				if( bc.GetType() == m_MonsterItemDrop[i, 0] )
				{
					type = m_MonsterItemDrop[i, 1];
					break;
				}
			}
			return type;
		}
		public static Type MonsterHiddenDropItem(BaseCreature bc)
		{
			Type type = null;
			for( int i = 0; i < m_MonsterItemDrop.GetLength(0); i++)
			{
				if( bc.GetType() == m_MonsterItemDrop[i, 0] )
				{
					type = m_MonsterItemDrop[i, 2];
					break;
				}
			}
			return type;
		}

		#endregion
		
		public static int QuestTier(PlayerMobile pm, int maxtier)
		{
			int tier = Level( pm.SilverPoint[0] );
			int playerlevel = tier / 35;
			int totalMaxTier = Math.Min( playerlevel, maxtier );
			tier = Utility.RandomMinMax( tier - ( maxtier * 30 ), tier + 15 );
			tier /= 35;
			tier = Math.Min( Math.Max(tier, 0 ), totalMaxTier );
			return tier;
		}

		public static void BroadcastLocalized(int cliloc, string args, int hue)
		{
			foreach (NetState state in NetState.Instances)
			{
				Mobile m = state.Mobile;
				if (m != null)
				{
					m.SendLocalizedMessage(cliloc, args, hue);
				}
			}
		}

		//임의 함수 처리
		public static void NewItemCreate( Item item, int rank, PlayerMobile pm = null, bool artifact = false )
		{
			return;
		}
		public static void NewUseGem(Item equip, int gem)
		{
			return;
		}
        //아이템 옵션 설정
		public static double PercentCalc(int number )
		{
			if( number < 3 )
				return 0.01;
			return 0.0001;
			
		}
		//독 적립 계산식
		public static void PoisonSavingDamage(Mobile from, int saving)
		{
			if( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				pm.PoisonSaving += saving;
			}
			else if( from is BaseCreature )
			{
				BaseCreature bc = from as BaseCreature;
				bc.PoisonSaving += saving;
			}
		}

		//독 저항성 계산식
		public static int PoisonAbsorbDamage(Mobile from)
		{
			int absorbDamage = ExtendedWeaponAttributes.GetValue(from, ExtendedWeaponAttribute.Bane) / 100;
			if( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				//스텟 독 저항성
				absorbDamage += pm.Str * 2;
			}
			absorbDamage += (int)( from.Skills.MagicResist.Value * 80 );
			if( from.Skills.MagicResist.Value >= 100 )
				absorbDamage += 4000;				

			//옵션 독 저항성%
			absorbDamage = (int)( absorbDamage * ( 1 + ExtendedWeaponAttributes.GetValue(from, ExtendedWeaponAttribute.HitSwarm) * 0.0000001 ) );
			
			return absorbDamage;
		}

		//강타 스킬 계산
		public static int SmashCalc(Mobile attacker, Mobile defender, double chanceBonus = 0.0, double damageBonus = 0 )
		{
			int specialDamage = 0;
			if( defender != null )
			{
				BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;

				if( attacker.Skills[SkillName.Bushido].Value >= 100 )
				{
					double smashChance = 0.1;
					if( chanceBonus > 0 )
						smashChance = chanceBonus;
					double smashDamage = 0.1;
					if( damageBonus > 0 )
						smashDamage = damageBonus;
					
					if( atkWeapon.Skill is SkillName.Archery )
						smashChance /= 2;
					if( attacker.Skills[atkWeapon.Skill].Value >= 150 )
					{
						if( atkWeapon.Skill is SkillName.Swords )
						{
							smashChance += 0.05;
							smashDamage = 0.15;
						}
						else if( atkWeapon.Skill is SkillName.Macing )
						{
							smashDamage = 0.2;
						}
						else if( atkWeapon.Skill is SkillName.Fencing )
						{
							smashChance += 0.1;
						}
						else
						{
							smashChance *= 2;
						}
					}
					if( Utility.RandomDouble() < smashChance )
					{
						specialDamage = (int)( defender.Hits * smashDamage );
						if( attacker.Skills[atkWeapon.Skill].Value >= 200 )
							specialDamage = (int)( defender.HitsMax * smashDamage );
						if( defender is BaseCreature )
						{
							BaseCreature bc = defender as BaseCreature;
							if( bc.Boss )
								specialDamage = 0;
							else
							{
								defender.FixedParticles(0x37B9, 1, 4, 0x251D, 0, 0, EffectLayer.Waist); //모맨텀 이펙트
								attacker.PlaySound(0x510);
								specialDamage /= MonsterTierCalc(bc);
							}
						}
					}
				}
			}
			return specialDamage;			
		}
		
		//급소 스킬 계산
		public static int SneakCalc(Mobile attacker, Mobile defender, int damage, double chanceBonus = 0.0, double damageBonus = 0 )
		{
			int specialDamage = 0;
			if( defender != null )
			{
				BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
				BaseShield shieldCheck = attacker.FindItemOnLayer(Layer.TwoHanded) as BaseShield;

				double sneakChance = 0.05;
				if( chanceBonus != 0 )
					sneakChance = chanceBonus;
				double sneakDamage = 1.0;
				if( damageBonus != 0 )
					sneakDamage = damageBonus;
				
				if( shieldCheck != null )
					sneakDamage = 0.5;

				if( defender is BaseCreature )
				{
					if( sneakChance < 1.0 )
					{
						BaseCreature bc = defender as BaseCreature;
						if( bc.Boss )
						{
							if( attacker.Skills[SkillName.Ninjitsu].Value >= 200 )
								sneakChance = 0.01;
							else
								sneakChance = 0.0;
						}
						else
						{
							sneakChance -= Misc.Util.MonsterTierCalc(bc) * 0.01;
						}
					}
					if( attacker.Skills[SkillName.Ninjitsu].Value >= 150 )
					{
						if( atkWeapon.Skill is SkillName.Swords )
						{
							if( shieldCheck != null )
							{
								sneakChance *= 1.25;
								sneakDamage *= 1.25;
							}
							else
							{
								sneakChance *= 1.5;
								sneakDamage *= 1.5;
							}
						}
						else if( atkWeapon.Skill is SkillName.Macing )
						{
							if( shieldCheck != null )
								sneakDamage *= 1.5;
							else
								sneakDamage *= 2;
						}
						else if( atkWeapon.Skill is SkillName.Fencing )
						{
							if( shieldCheck != null )
								sneakChance *= 1.5;
							else
								sneakChance *= 2;
						}
					}
				}

				if( Utility.RandomDouble() < sneakChance )
				{
					specialDamage = (int)( damage * 1 + sneakDamage );
					defender.FixedParticles(0x374A, 1, 17, 0x26BC, EffectLayer.Waist); //DeathStrike 이펙트
					attacker.PlaySound(attacker.Female ? 0x50D : 0x50E);
				}
			}
			return specialDamage;
		}
		
		//마법 보너스 스킬
		public static SkillName[] CastBonusSkill = 
		{
			//1써클
			SkillName.Necromancy,	//둔화
			SkillName.Mysticism,	//음식 만들기
			SkillName.Necromancy,	//정신 쇠약
			SkillName.Chivalry,		//치료
			SkillName.Spellweaving,	//마법 화살
			SkillName.Chivalry,		//야간 시야
			SkillName.Chivalry,		//반응 갑옷
			SkillName.Necromancy,	//약화
			//2써클
			SkillName.Spellweaving,	//민첩
			SkillName.Spellweaving,	//교활
			SkillName.Chivalry,		//치유
			SkillName.Spellweaving,	//체력 손상
			SkillName.Necromancy,	//마법 함정
			SkillName.Mysticism,	//마법 함정제거
			SkillName.Chivalry,		//마법 보호
			SkillName.Mysticism,	//힘
			//3써클
			SkillName.Chivalry,		//축복
			SkillName.Spellweaving,	//화염구
			SkillName.Mysticism,	//마법 자물쇠
			SkillName.Spellweaving,	//독
			SkillName.Spellweaving,	//염동력
			SkillName.Mysticism,	//순간이동
			SkillName.Mysticism,	//잠금 해제
			SkillName.Necromancy,	//오염된 돌 벽
			//4써클
			SkillName.Chivalry,		//대 치유
			SkillName.Chivalry,		//단체 마법 보호
			SkillName.Necromancy,	//저주
			SkillName.Spellweaving,	//화염지대
			SkillName.Chivalry,		//대회복
			SkillName.Spellweaving,	//번개
			SkillName.Necromancy,	//마나 흡수
			SkillName.Mysticism,	//귀환
			//5써클
			SkillName.Necromancy,	//칼날의 정령
			SkillName.Chivalry,		//지역 마법해제
			SkillName.Mysticism,	//변장
			SkillName.Chivalry,		//마법 반사
			SkillName.Necromancy,	//정신 파괴
			SkillName.Necromancy,	//마비
			SkillName.Spellweaving,	//독성 지대
			SkillName.Mysticism,	//짐승 소환
			//6써클
			SkillName.Chivalry,		//마법 해제
			SkillName.Spellweaving,	//에너지 볼트
			SkillName.Spellweaving,	//폭발
			SkillName.Mysticism,	//투명화
			SkillName.Mysticism,	//기록
			SkillName.Necromancy,	//단체 저주
			SkillName.Necromancy,	//마비 지대
			SkillName.Chivalry,		//발각
			//7써클
			SkillName.Spellweaving,	//연속 번개
			SkillName.Spellweaving,	//에너지 지대
			SkillName.Spellweaving,	//화염 강타
			SkillName.Mysticism,	//게이트 여행
			SkillName.Necromancy,	//마나 흡혈
			SkillName.Chivalry,		//집단 마법해제
			SkillName.Spellweaving,	//유성 폭풍
			SkillName.Mysticism,	//변신
			//8써클
			SkillName.Mysticism,	//지진
			SkillName.Necromancy,	//에너지 소용돌이
			SkillName.Chivalry,		//부활
			SkillName.Spellweaving,	//공기의 정령 소환
			SkillName.Necromancy,	//악마 소환
			SkillName.Chivalry,		//땅의 정령 소환
			SkillName.Spellweaving,	//불의 정령 소환
			SkillName.Spellweaving	//물의 정령 소환
		};
		
		//SPM 기력 소모 
		public static int[,] SPMStam =
		{
			{ 500, 0 },		//1  방어구 무시(Armor Ignore), 방어구 관통(Armor Pierce)
			{ 500, 0 },		//2  출혈 공격(Bleed Attack), 힘 화살(Force Arrow)
			{ 1000, 0 },		//3  충격파(Concussion Blow)
			{ 1000, 0 },		//4  파괴 일격(Crushing Blow), 헤드샷(Head Shot, 구 Moving Shot)
			{ 350, 0 },		//5  무장 해제(Disarm)
			{ 350, 0 },		//6  낙마(Dismount)
			{ 250, 0 },		//7  연속 공격(Double Strike)
			{ 250, 0 },		//8  독 바르기(Infecting), 맹독 화살(Serpent Arrow)
			{ 1500, 0 },		//9  급소 가격(Mortal Strike)
			{ 250, 0 },		//10 칼날 매듭(Bladeweave)
			{ 750, 0 },		//11 마비 일격(Paralyzing Blow)
			{ 1000, 0 },		//12 그림자 일격(Shadow Strike)
			{ 500, 0 },			//13 소용돌이 일격(Whirlwind Attack)
			{ 250, 0 },		//14 정신 공격(Psychic Attack)
			{ 750, 0 },		//15 전격 화살(Lightning Arrow)
			{ 100, 0 }			//16 자연의 힘(Force of Nature)
		};
		
		
		public static double[] RankDice =
		{
			1.1, 1.22, 1.35, 1.5
		};
		

		public static int[] NewRandomOptionStock = 
		{
			0, 40, 90, 160, 240, 300
		};
		
		public static int[] NewUpgradeOptionStock = 
		{
			3, 3, 3, 3, 3, 4
		};
		
		public static int UseResourceNumber( int resource )
		{
			int usedresource = resource;
			if( usedresource >= 2 && usedresource <= 9 ) //구리 ~ 벨러. 2 ~ 7
				usedresource -= 2;
			else if( usedresource >= 101 && usedresource <= 107 ) //가죽 ~ 미늘. 8 ~ 14
				usedresource -= 93;
			else if( usedresource >= 301 && usedresource <= 307 ) //가죽 ~ 미늘. 8 ~ 14
				usedresource -= 286;			
			
			usedresource--;
			return usedresource;
		}
		
		public static bool UniqueNumberCheck(int checknumber, int dice )
		{
			if( checknumber == dice )
				return true;
			else
				return false;
		}

		public static void NewItemDrop(Item make, Item newmake, Mobile pm)
		{
			if( make.Parent is Container && pm != null )
			{
				Container cont = make.Parent as Container;
				if (!cont.TryDropItem(pm, newmake, false))
				{
					if(cont != pm.Backpack)
						pm.AddToBackpack(newmake);
					else
						newmake.MoveToWorld(pm.Location, pm.Map);
				}
			}
		}

		public static double[] Enhance_RankUpgrade =
		{
			100.0, 20.0, 2.5, 0.1, 0.001, 0.000
		};
		
		public static int ItemRankPoint( int item )
		{
			int point = 0;
			switch( item )
			{
				case 1 : point = 2;
				break;
				case 2 : point = 5;
				break;
				case 3 : point = 10;
				break;
				case 4 : point = 20;
				break;
				case 5 : point = 40;
				break;
				case 6 : point = 60;
				break;
				case 7 : point = 80;
				break;
				case 8 : point = 100;
				break;
			}
			return point;
		}

		public static double PercentCal( int min, int max )
		{
			double percent = max - min;
			if( percent <= 0 )
				percent = 100;
			else
				percent = 100 / percent;
			return percent;
		}
		
		public static double PercentCal( BaseVendor vendor, double min, double max )
		{
			double percent = max - min;
			if( percent <= 0 )
				percent = 100;
			else
				percent = 100 / percent;
			return percent;
		}
		
		//구입 판매 처리
		public static int Price( Item item, int price )
		{
			/*
			if( item is Gold )
			{
				pm.SendMessage("뭐하는 짓임?");
				return 0;
			}
			*/
			if( item is IEquipOption )
			{
				IEquipOption buyitem = item as IEquipOption;
				//옵션 정의
				if( (int)buyitem.ItemPower == 7 )
					price = 100;
				else if( (int)buyitem.ItemPower == 8 )
					price = 200;
				else if( (int)buyitem.ItemPower <= 6 )
				{
					int count = 1;
					while( buyitem.PrefixOption[count] != 0 && buyitem.PrefixOption[count + 1] != 0 && buyitem.PrefixOption[count + 2] != 0 && buyitem.PrefixOption[count + 3] != 0 && buyitem.PrefixOption[count + 4] != 0 )
					{
						double percent = PercentCal(buyitem.PrefixOption[count + 2], buyitem.PrefixOption[count + 3]) * 0.01;
						percent *= buyitem.PrefixOption[count + 4]; //1 ~ 100원 증가, 최대 800원
						price = (int)percent;
						count += 4;
					}
					if( buyitem.SuffixOption[99] > 0 )
					{
						BaseCreature bc = MonsterListCheck( buyitem.SuffixOption[99] );
						if( bc != null )
							price += bc.Fame / 100; //몬스터 명성 대비 가격. 1 ~ 320원 증가
					}
					else if( buyitem.PlayerConstructed )
						price += 100; //제작품이면 일단 100원 증가
					if( (int)buyitem.ItemPower >= 4 && (int)buyitem.ItemPower <= 6 )
					{
						price *= ( (int)buyitem.ItemPower - 3 ) / 5; //티어당 20%씩 처리
					}
					else
						price /= 10; //그 외 10% 처리
				}
				else
					price = 10;
				//마지막 내구도 체크
				double InitCheck = PercentCal( buyitem.InitMinHits, buyitem.InitMaxHits) * 0.01;
				price *= (int)InitCheck;
			}
			else if( item is BaseInstrument )
			{
				BaseInstrument buyitem = item as BaseInstrument;
				price = buyitem.UsesRemaining / 10;
			}
			else if( item is BaseTool )
			{
				BaseTool buyitem = item as BaseTool;
				price =  buyitem.UsesRemaining / 10;
			}
			else if( item is BaseHarvestTool )
			{
				BaseHarvestTool buyitem = item as BaseHarvestTool;
				price =  buyitem.UsesRemaining / 10;
			}
			else if( item is CraftableFurniture )
			{
				//카펜 가구 체크
				CraftableFurniture buyitem = item as CraftableFurniture;
				CraftItem craftItem = DefCarpentry.CraftSystem.CraftItems.SearchFor(buyitem.GetType() );
				if( craftItem == null )
					return 0;

				CraftRes craftResource = craftItem.Resources.GetAt(0);
				if( craftResource == null || craftResource.Amount < 1 )
					return 0;
				else
				{
					price = craftResource.Amount * 5;
				}
				int mulPrice = 100;
				switch ( buyitem.Resource )
				{
					case CraftResource.OakWood:
						mulPrice = 110;
						break;
					case CraftResource.AshWood:
						mulPrice = 120;
						break;
					case CraftResource.YewWood:
						mulPrice = 140;
						break;
					case CraftResource.Heartwood:
						mulPrice = 180;
						break;
					case CraftResource.Bloodwood:
						mulPrice = 260;
						break;
					case CraftResource.Frostwood:
						mulPrice = 420;
						break;
				}
				price *= mulPrice;
				price /= 100;
			}
			else
			{
				int resourcePrice = 1;
				//보석 체크
				if( item is IGem )
					resourcePrice = 10;
				//물고기 체크
				if( item is Fish )
					resourcePrice = 2;
				//고기 체크
				if( item is RawBird || item is RawLambLeg || item is RawChickenLeg || item is LambLeg || item is CookedBird || item is ChickenLeg )
					resourcePrice = 5;
				if (item is BaseBeverage)
				{
					int price1 = (int)price, price2 = (int)price;

					if (item is Pitcher)
					{
						price1 = 3;
						price2 = 5;
					}
					else if (item is BeverageBottle)
					{
						price1 = 3;
						price2 = 3;
					}
					else if (item is Jug)
					{
						price1 = 6;
						price2 = 6;
					}

					BaseBeverage bev = (BaseBeverage)item;

					if (bev.IsEmpty || bev.Content == BeverageType.Milk)
						price = price1;
					else
						price = price2;
				}				
				
				//알케미 체크
				CraftItem craftItem = DefAlchemy.CraftSystem.CraftItems.SearchFor(item.GetType() );
				if( craftItem != null )
				{
					CraftRes craftResource = craftItem.Resources.GetAt(0);
					if( craftResource.Amount < 1 )
						return 0;
					else
						resourcePrice = craftResource.Amount;
				}
				//카펜 체크
				craftItem = DefCarpentry.CraftSystem.CraftItems.SearchFor(item.GetType() );
				if( craftItem != null )
				{
					CraftRes craftResource = craftItem.Resources.GetAt(0);
					if( craftResource.Amount < 1 )
						return 0;
					else
						resourcePrice = craftResource.Amount;
				}
				//보크
				craftItem = DefBowFletching.CraftSystem.CraftItems.SearchFor(item.GetType() );
				if( craftItem != null )
				{
					CraftRes craftResource = craftItem.Resources.GetAt(0);
					if( craftResource.Amount < 1 )
						return 0;
					else
						resourcePrice = craftResource.Amount;
				}
				//카토
				craftItem = DefCartography.CraftSystem.CraftItems.SearchFor(item.GetType() );
				if( craftItem != null )
				{
					CraftRes craftResource = craftItem.Resources.GetAt(0);
					if( craftResource.Amount < 1 )
						return 0;
					else
						resourcePrice = craftResource.Amount;
				}
				//요리
				craftItem = DefCooking.CraftSystem.CraftItems.SearchFor(item.GetType() );
				if( craftItem != null )
				{
					CraftRes craftResource = craftItem.Resources.GetAt(0);
					if( craftResource.Amount < 1 )
						return 0;
					else
						resourcePrice = craftResource.Amount;
				}
				//테일러
				craftItem = DefTailoring.CraftSystem.CraftItems.SearchFor(item.GetType() );
				if( craftItem != null )
				{
					CraftRes craftResource = craftItem.Resources.GetAt(0);
					if( craftResource.Amount < 1 )
						return 0;
					else
						resourcePrice = craftResource.Amount;
				}
				//팅커
				craftItem = DefTinkering.CraftSystem.CraftItems.SearchFor(item.GetType() );
				if( craftItem != null )
				{
					CraftRes craftResource = craftItem.Resources.GetAt(0);
					if( craftResource.Amount < 1 )
						return 0;
					else
						resourcePrice = craftResource.Amount;
				}
				if( item.Stackable )
				{
					price = item.Amount * resourcePrice * 5; //스택 아이템 모두 5 지피로 구매
				}
				else
				{
					price = resourcePrice * 5; //스택 아이템 모두 5 지피로 구매
				}
			}
			//if( !LastPriceCheck( vendor, buyPrice, pm ) )
			//	return 0;
		
			return price;
		}
		public static int RegionPrice(BaseVendor vendor)
		{
			int regionprice = 1000;
			if( vendor.Region.Name == "Britain" ) //대도시
				regionprice = 8000;
			else if( vendor.Region.Name == "Buccaneer's Den" ) //도둑 도시
			{
				if ( vendor is Thief )
					regionprice = 6000;
				else 
					regionprice = 1000;
			}
			else if( vendor.Region.Name == "Cove" ) //초보자 도시
				regionprice = 1500;
			else if( vendor.Region.Name == "Jhelom" ) //전사의 도시
			{
				if( vendor is Weaponsmith || vendor is Tanner )
				regionprice = 5000;
			}
			else if( vendor.Region.Name == "Magincia" ) //벤더의 도시
				regionprice = 2000;
			else if( vendor.Region.Name == "Minoc" ) //광부의 도시
			{
				if( vendor is Miner || vendor is OreSeller )
					regionprice = 10000;
				else if( vendor is Cook || vendor is Barkeeper )
					regionprice = 3500;
			}
			else if( vendor.Region.Name == "Moonglow" )
				regionprice = 5000;
			else if( vendor.Region.Name == "Nujel'm" )
				regionprice = 5000;
			else if( vendor.Region.Name == "Serpent's Hold" )
				regionprice = 5000;
			else if( vendor.Region.Name == "Skara Brae" ) //동물과 식물의 도시
			{
				
				regionprice = 5000;
			}
			else if( vendor.Region.Name == "Trinsic" )
				regionprice = 5000;
			else if( vendor.Region.Name == "Vesper" )
				regionprice = 5000;
			else if( vendor.Region.Name == "Yew" )
				regionprice = 5000;
			else if( vendor.Region.Name == "New Haven" )
				regionprice = 5000;
			
			if( vendor is AnimalTrainer )
				regionprice /= 100;
			return regionprice;
		}
		public static bool LastPriceCheck( BaseVendor vendor, int Price, Mobile from )
		{
			if( vendor.MyGold < Price )
			{
				from.SendMessage("상인이 가진 돈보다 판매금이 더 많습니다!");
				return false;
			}
			return true;
		}

		public static string NotIdentedItemName( int name )
		{
			return "<basefont color=#AAAAAA>{0}\t{1}<basefont color=#FFFFFF>";
		}



		public static string ItemRankName( int name )
		{
			string colorname = "[ 일반 장비 ]";
			switch( name )
			{
				case 4 : colorname = "[ 희귀 장비 ]";
				break;
				case 5 : colorname = "[ 영웅 장비 ]";
				break;
				case 6 : colorname = "[ 서사 장비 ]";
				break;
				case 7 : colorname = "[ 전설 장비 ]";
				break;
				case 8 : colorname = "[ 신화 장비 ]";
				break;
			}
			return colorname;
		}

		//색상 지정 설정
		#region Color Setting
		public static int RandomColor_Red(bool specialcolor)
		{
			if ( specialcolor && Utility.RandomDouble() < 0.01 )
				return 1166;

			return Utility.RandomMinMax( 23, 41 );
		}

		public static int RandomColor_Blue(bool specialcolor)
		{
			if ( specialcolor && Utility.RandomDouble() < 0.01 )
				return 1154;
				
			return Utility.RandomMinMax( 87, 105 );
		}
		
		public static int RandomColor_Yellow(bool specialcolor)
		{
			if ( specialcolor && Utility.RandomDouble() < 0.01 )
				return 1169;
				
			return Utility.RandomMinMax( 49, 56 );
		}

		public static int RandomColor_Green(bool specialcolor)
		{
			if ( specialcolor && Utility.RandomDouble() < 0.01 )
				return 1167;
				
			return Utility.RandomMinMax( 57, 81 );
		}
		public static int RandomColor_Rare(bool specialcolor)
		{
			if ( specialcolor && Utility.RandomDouble() < 0.01 )
				return Utility.RandomList( 0x657, 0x515, 0x4B1, 0x481, 0x482, 0x455 );

			return Utility.RandomList( 0x97A, 0x978, 0x901, 0x8AC, 0x5A7, 0x527 );
		}

		public static int RandomColor_Legendary(bool specialcolor)
		{
			if ( specialcolor && Utility.RandomDouble() < 0.01 )
				return Utility.RandomList(0x489, 0x480, 0xAAC, 0xAB4, 0xAAF, 0xAB5, 0xAAB);

			return Utility.RandomList(0x483, 0x38C, 0x488, 0x48A, 0x495, 0x48B, 0x486, 0x485, 0x48D, 0x490, 0x48E, 0x491, 0x48F, 0x494, 0x484, 0x497, 0x47F, 0x47E );
		}
		#endregion

		public static void SavingAccountPoint( PlayerMobile pm, int target, int point )
		{
			if( target > 0 )
			{
				Account acc = pm.Account as Account;
				acc.Point[target]++;
				if( Math.Pow( acc.Point[target + 500] + 1, 2 ) <= acc.Point[target] )
				{
					acc.Point[target + 500 ] += point;
					acc.Point[0] += acc.Point[target + 500 ];
					pm.SendMessage("가문 포인트를 {0}점 획득하였습니다.", point);
				}
			}
			if( pm.HasGump(typeof(HarvestGump)) )
				pm.SendGump(new HarvestGump(pm));
			if( pm.HasGump(typeof(CraftingGump)) )
				pm.SendGump(new CraftingGump(pm));
			if( pm.HasGump(typeof(MonsterFeatGump)) )
				pm.SendGump(new MonsterFeatGump(pm));
		}
		
		//생산 업그레이드 확률
		public static int[] upgradechance = { 0, 3000, 1000, 500, 250, 100, 50, 10, 5 };

		//장비포인트 획득
		public static int[] EquipPoint = { 1, 2, 4, 7, 10, 15, 23 };
		
		private static bool EquipMeltingBoolCheck( PlayerMobile pm, int tier, int rank, int named )
		{
			bool melting = false;
			//티어
			melting = pm.EquipMeltingOptionTier[tier];
			if( !melting )
				return false;
			//랭크
			if( rank == 0 && pm.EquipMeltingOptionRank[0] )
				melting = true;
			else if( rank >= 4 && pm.EquipMeltingOptionRank[rank - 3] )
				melting = true;
			else
				return false;
			//고유
			if( named == 100000 && pm.EquipMeltingOptionNamed[0] )
				melting = true;
			else if( named > 0 && pm.EquipMeltingOptionNamed[1] )
				melting = true;
			else if( named == 0 )
				melting = true;
			else
				return false;
			
			return melting;
		}
		
		private static void EquipPointCalc( Account acc, int rank, int tier, int artifact )
		{
			acc.Point[861 + rank] += tier + artifact * 5;
		}
		
		public static void EquipPointReturn( PlayerMobile pm )
		{
			Account acc = pm.Account as Account;
			Container pack = pm.Backpack;
			
			List<BaseWeapon> weapon = new List <BaseWeapon>();
			List<BaseArmor> armor = new List <BaseArmor>();
			List<BaseClothing> clothing = new List <BaseClothing>();
			List<BaseJewel> jewel = new List <BaseJewel>();
			List<Spellbook> spellbook = new List <Spellbook>();

			if( pm.EquipMeltingOptionBag )
			{
				List<Container> container = pack.FindItemsByType<Container>();
				for( int l = container.Count -1; l >=0; l--)
				{
					Container equipbag = container[l];
					if( equipbag is EquipBag )
					{
						EquipBag eb = equipbag as EquipBag;
						if( eb != null )
						{
							weapon = eb.FindItemsByType<BaseWeapon>();
							armor = eb.FindItemsByType<BaseArmor>();
							clothing = eb.FindItemsByType<BaseClothing>();
							jewel = eb.FindItemsByType<BaseJewel>();
							spellbook = eb.FindItemsByType<Spellbook>();
						}
					}
				}
			}
			else
			{
				weapon = pack.FindItemsByType<BaseWeapon>();
				armor = pack.FindItemsByType<BaseArmor>();
				clothing = pack.FindItemsByType<BaseClothing>();
				jewel = pack.FindItemsByType<BaseJewel>();
				spellbook = pack.FindItemsByType<Spellbook>();
			}
			if( weapon.Count > 0 )
			{
				for( int i = weapon.Count -1; i >= 0; --i)
				{
					int tier = weapon[i].PrefixOption[99];
					int rank = (int)weapon[i].ItemPower;
					int named = weapon[i].SuffixOption[99];
					if( weapon[i].PlayerConstructed )
						named = 100000;
					if( weapon[i].LootType != LootType.Blessed && EquipMeltingBoolCheck( pm, tier, rank, named ) )
					{
						int pointrank = rank;
						if( rank >= 4 )
							pointrank -= 4;
						else
							pointrank = 0;
						EquipPointCalc(acc, pointrank, EquipPoint[tier], weapon[i].PrefixOption[80] * 5);
						weapon[i].Delete();
					}
				}
			}
			if( armor.Count > 0 )
			{
				for( int i = armor.Count -1; i >= 0; --i)
				{
					int tier = armor[i].PrefixOption[99];
					int rank = (int)armor[i].ItemPower;
					int named = armor[i].SuffixOption[99];
					if( armor[i].PlayerConstructed )
						named = 100000;
					if( armor[i].LootType != LootType.Blessed && EquipMeltingBoolCheck( pm, tier, rank, named ) )
					{
						int pointrank = rank;
						if( rank >= 4 )
							pointrank -= 4;
						else
							pointrank = 0;
						EquipPointCalc(acc, pointrank, EquipPoint[tier], armor[i].PrefixOption[80] * 5);
						armor[i].Delete();
					}
				}
			}
			if( clothing.Count > 0 )
			{
				for( int i = clothing.Count -1; i >= 0; --i)
				{
					int tier = clothing[i].PrefixOption[99];
					int rank = (int)clothing[i].ItemPower;
					int named = clothing[i].SuffixOption[99];
					if( clothing[i].PlayerConstructed )
						named = 100000;
					if( clothing[i].LootType != LootType.Blessed && EquipMeltingBoolCheck( pm, tier, rank, named ) )
					{
						int pointrank = rank;
						if( rank >= 4 )
							pointrank -= 4;
						else
							pointrank = 0;
						EquipPointCalc(acc, pointrank, EquipPoint[tier], clothing[i].PrefixOption[80] * 5);
						clothing[i].Delete();
					}
				}
			}
			if( jewel.Count > 0 )
			{
				for( int i = jewel.Count -1; i >= 0; --i)
				{
					int tier = jewel[i].PrefixOption[99];
					int rank = (int)jewel[i].ItemPower;
					int named = jewel[i].SuffixOption[99];
					if( jewel[i].PlayerConstructed )
						named = 100000;
					if( jewel[i].LootType != LootType.Blessed && EquipMeltingBoolCheck( pm, tier, rank, named ) )
					{
						int pointrank = rank;
						if( rank >= 4 )
							pointrank -= 4;
						else
							pointrank = 0;
						EquipPointCalc(acc, pointrank, EquipPoint[tier], jewel[i].PrefixOption[80] * 5);
						jewel[i].Delete();
					}
				}
			}
			if( spellbook.Count > 0 )
			{
				for( int i = spellbook.Count -1; i >= 0; --i)
				{
					int tier = spellbook[i].PrefixOption[99];
					int rank = (int)spellbook[i].ItemPower;
					int named = spellbook[i].SuffixOption[99];
					if( spellbook[i].PlayerConstructed )
						named = 100000;
					if( spellbook[i].LootType != LootType.Blessed && EquipMeltingBoolCheck( pm, tier, rank, named ) )
					{
						int pointrank = rank;
						if( rank >= 4 )
							pointrank -= 4;
						else
							pointrank = 0;
						EquipPointCalc(acc, pointrank, EquipPoint[tier], spellbook[i].PrefixOption[80] * 5);
						spellbook[i].Delete();
					}
				}
			}

			pm.SendGump(new EquipMeltingGump(pm));			
		}
		
		public static int MonthCal()
		{
			int monthcheck = 0;
			int year = DateTime.Now.Year;
			int month = DateTime.Now.Month;
			int days = DateTime.Now.Day;
			int daysInMonth = DateTime.DaysInMonth( year, month );
			return daysInMonth - days;
		}
		
		public static int WeekCal()
		{
			int weekcheck = 0;
			switch ( DateTime.Now.DayOfWeek )
			{
				case DayOfWeek.Monday: //월
				weekcheck = 5;
				break;

				case DayOfWeek.Tuesday: //화
				weekcheck = 4;
				break;

				case DayOfWeek.Wednesday: //수
				weekcheck = 3;
				break;

				case DayOfWeek.Thursday: //목
				weekcheck = 2;
				break;

				case DayOfWeek.Friday: //금
				weekcheck = 1;
				break;

				case DayOfWeek.Saturday: //토
				weekcheck = 7;
				break;

				case DayOfWeek.Sunday: //일
				weekcheck = 6;
				break;
			}			
			return weekcheck;
		}
		
		public static string NowTime( long nowtime )
		{
			string time = "";
			int duration = (int)nowtime / 10;
			if( ( duration * 60 * 60 * 24 ) > 0 )
			{
				int day = (duration/60/60/24)%24;
				if( day > 0 )
				{
					time += day.ToString() + "일 ";
					duration -= day * 60 * 60 * 24;
				}
			}
			if( ( duration * 60 * 60 ) > 0 )
			{
				int hour = (duration/60/60)%60;
				if( hour > 0 )
				{
					time += hour.ToString() + "시 ";
					duration -= hour * 60 * 60;
				}
			}
			if( ( duration * 60 ) > 0 )
			{
				int minute = (duration/60)%60;
				if( minute > 0 )
				{
					time += minute.ToString() + "분 ";
					duration -= minute * 60;
				}
			}
			if(  duration > 0 )
				time += duration.ToString() + "초";
			return time;
		}


		public static string TickCal( long oldtime )
		{
			string time = "";
			int duration = (int)(oldtime - Core.TickCount) / 1000;
			if( ( duration * 60 * 60 * 24 ) > 0 )
			{
				int day = (duration/60/60/24)%24;
				if( day > 0 )
				{
					time += day.ToString() + "일 ";
					duration -= day * 60 * 60 * 24;
				}
			}
			if( ( duration * 60 * 60 ) > 0 )
			{
				int hour = (duration/60/60)%60;
				if( hour > 0 )
				{
					time += hour.ToString() + "시 ";
					duration -= hour * 60 * 60;
				}
			}
			if( ( duration * 60 ) > 0 )
			{
				int minute = (duration/60)%60;
				if( minute > 0 )
				{
					time += minute.ToString() + "분 ";
					duration -= minute * 60;
				}
			}
			if(  duration > 0 )
				time += duration.ToString() + "초";
			return time;
		}

		public static string TimeCal( DateTime oldtime, DateTime nowtime )
		{
			TimeSpan timecal = oldtime - nowtime;
			int day = timecal.Days;
			int hour = timecal.Hours;
			int minute = timecal.Minutes;
			int second = timecal.Seconds;
			string time = "";
			if( day > 0 )
				time += day.ToString() + "일 ";
			if( hour > 0 )
				time += hour.ToString() + "시 ";
			if( minute > 0 )
				time += minute.ToString() + "분 ";
			if( second > 0 )
				time += second.ToString() + "초";
			return time;
		}

		public static double RestCal( DateTime oldtime, DateTime nowtime )
		{
			TimeSpan timecal = nowtime - oldtime;
			int time = 0;

			if( timecal.Days > 0 )
				time += timecal.Days * 86400;
			if( timecal.Hours > 0 )
				time += timecal.Hours * 3600;
			if( timecal.Minutes > 0 )
				time += timecal.Minutes * 60;
			if( timecal.Seconds > 0 )
				time += timecal.Seconds;

			double result = time * 0.25;

			return result;
		}

		//경험치 시스템 계산
		public static int PointCal(int point)
		{
			int level = (int)Math.Sqrt(point);
			level /= 100;
			if( level < 0 )
				level = 0;
			else if( level > 150 )
				level = 150;

			return level;
		}

		//경험치 포인트 계산
		public static int PointUsed(int[] point)
		{
			int used_point = 0;
			for( int i = 1; i < point.Length; i++)
			{
				used_point += point[i];
			}
			return used_point;
		}

		public static double AttackSpeedTicks( double speed, int bonus )
		{
			double delayInSeconds = Math.Truncate( ( speed * 10000 / ( 1000 + bonus ) ) ) * 0.1;
			if( delayInSeconds < 0.5 )
				delayInSeconds = 0.5;
			return delayInSeconds;
		}


		public static int ExpHarvestBonus( PlayerMobile pm, int maxchance )
		{
			maxchance *= 100 + pm.GoldPoint[3];
			maxchance /= 100;
			return maxchance;
		}
		
		public static int TimeValue( DateTime oldtime, DateTime nowtime )
		{
			TimeSpan timecal = oldtime - nowtime;
			int time = 0;
			if( timecal.Days > 0 )
				time += timecal.Days * 864000;
			if( timecal.Hours > 0 )
				time += timecal.Hours * 36000;
			if( timecal.Minutes > 0 )
				time += timecal.Minutes * 600;
			if( timecal.Seconds > 0 )
				time += timecal.Seconds * 10;
			return time;
		}

		//피로도 체크
		public static int TiredCheck( PlayerMobile pm, int hunger, int point, int master = 0 )
		{
			return TiredCheck( pm, hunger, (double)point, master );
		}
		public static int TiredCheck( PlayerMobile pm, int hunger, double point, int master = 0 )
		{
			//int doublecheck = ( hunger <= 0 ) ? 2 : 1;
			//double tiredcal = ( point * ( 100 - master ) / 100 ) * doublecheck * 0.1;

			if( pm.Tired > 0 )
			{
				pm.Tired = 0;
				/*
				//point 100, Tired -5
				if( point <= ( pm.Tired + tiredcal ) * -10 )
					point *= 2;
				else
					point += (int)( pm.Tired + tiredcal ) * -10;
				*/
			}
			else
			{
				if( pm.Tired > -100 )
				{
					point *= (int)( 100 + pm.Tired * -1 );
					point /= 100;
					pm.Tired = 0;
				}
				else
				{
					point *= 2;
					pm.Tired += 100;
				}
				
			}
			//pm.SendMessage("피로도 {0} 증가", ( point * ( 100 - master ) / 100 ) * doublecheck );

			if( pm.Hunger < 0 )
				pm.Hunger = 0;

			return (int)point;

		}

		public static double SkillExp_Calc( Mobile from, int skill )
		{
			double maxvalue = 1000;
			double skillvalue = from.Skills[skill].Base;

			if( skillvalue < 10.0 ) // 1000 증가
				maxvalue = 1000 + skillvalue * 100; //00.1 ~ 9.9 스킬 포인트. 1000 ~ 1990
			else if( skillvalue < 20.0 ) // 1500 증가
				maxvalue = 2000 + ( skillvalue - 10.0 ) * 200; //10.0 ~ 19.9 스킬 포인트. 2500 ~ 3985
			else if( skillvalue < 30.0 ) // 2000 증가
				maxvalue = 4000 + ( skillvalue - 20.0 ) * 300; //20.0 ~ 29.9 스킬 포인트. 5000 ~ 6980
			else if( skillvalue < 40.0 ) // 3000 증가
				maxvalue = 7000 + ( skillvalue - 30.0 ) * 500; //30.0 ~ 39.9 스킬 포인트. 9000 ~ 10970
			else if( skillvalue < 50.0 ) // 7000 증가
				maxvalue = 12000 + ( skillvalue - 40.0 ) * 800; //40.0 ~ 49.9 스킬 포인트. 15000 ~ 21930
			else if( skillvalue < 60.0 ) // 10000 증가
				maxvalue = 20000 + ( skillvalue - 50.0 ) * 1000; //50.0 ~ 59.9 스킬 포인트. 27000 ~ 36900
			else if( skillvalue < 70.0 ) // 20000 증가
				maxvalue = 30000 + ( skillvalue - 60.0 ) * 1500; //60.0 ~ 69.9 스킬 포인트. 45000 ~ 64800
			else if( skillvalue < 80.0 ) // 30000 증가
				maxvalue = 45000 + ( skillvalue - 70.0 ) * 2250; //70.0 ~ 79.9 스킬 포인트. 85000 ~ 114700
			else if( skillvalue < 90.0 ) // 65500 증가
				maxvalue = 67500 + ( skillvalue - 80.0 ) * 3250; //80.0 ~ 89.9 스킬 포인트. 200000 ~ 299000
			else if( skillvalue < 100.0 ) // 200000 증가
				maxvalue = 100000 + ( skillvalue - 90.0 ) * 5000; //90.0 ~ 99.9 스킬 포인트. 500000 ~ 995000
			else if( skillvalue < 110.0 ) // 500000 증가
				maxvalue = 150000 + ( skillvalue - 100.0 ) * 7000; //100.0 ~ 104.9 스킬 포인트. 1500000 ~ 498000
			else if( skillvalue < 120.0 ) // 1000000 증가
				maxvalue = 220000 + ( skillvalue - 110.0 ) * 10000; //105.0 ~ 109.9 스킬 포인트. 700000 ~ 945000
			else if( skillvalue < 130.0 ) // 2500000 증가
				maxvalue = 320000 + ( skillvalue - 120.0 ) * 15000; //110.0 ~ 114.9 스킬 포인트. 1500000 ~ 1990000
			else if( skillvalue < 140.0 ) // 8000000 증가
				maxvalue = 470000 + ( skillvalue - 130.0 ) * 21500; //115.0 ~ 119.9 스킬 포인트. 15000000 ~ 16225000
			else if( skillvalue < 150.0 ) // 8000000 증가
				maxvalue = 685000 + ( skillvalue - 140.0 ) * 30000; //115.0 ~ 119.9 스킬 포인트. 15000000 ~ 16225000
			else if( skillvalue < 160.0 ) // 8000000 증가
				maxvalue = 985000 + ( skillvalue - 150.0 ) * 40000; //115.0 ~ 119.9 스킬 포인트. 15000000 ~ 16225000
			else if( skillvalue < 170.0 ) // 8000000 증가
				maxvalue = 1385000 + ( skillvalue - 160.0 ) * 55000; //115.0 ~ 119.9 스킬 포인트. 15000000 ~ 16225000
			else if( skillvalue < 180.0 ) // 8000000 증가
				maxvalue = 1935000 + ( skillvalue - 170.0 ) * 75000; //115.0 ~ 119.9 스킬 포인트. 15000000 ~ 16225000
			else if( skillvalue < 190.0 ) // 8000000 증가
				maxvalue = 2685000 + ( skillvalue - 180.0 ) * 100000; //115.0 ~ 119.9 스킬 포인트. 15000000 ~ 16225000
			else if( skillvalue < 200.0 ) // 8000000 증가
				maxvalue = 3685000 + ( skillvalue - 190.0 ) * 150000; //115.0 ~ 119.9 스킬 포인트. 15000000 ~ 16225000
			else if( skillvalue < 210.0 ) // 8000000 증가
				maxvalue = 5185000 + ( skillvalue - 200.0 ) * 500000; //115.0 ~ 119.9 스킬 포인트. 15000000 ~ 16225000
			else if( skillvalue < 220.0 ) // 8000000 증가
				maxvalue = 10185000 + ( skillvalue - 210.0 ) * 1500000; //115.0 ~ 119.9 스킬 포인트. 15000000 ~ 16225000
			else if( skillvalue < 230.0 ) // 8000000 증가
				maxvalue = 25185000 + ( skillvalue - 220.0 ) * 5000000; //115.0 ~ 119.9 스킬 포인트. 15000000 ~ 16225000
			else if( skillvalue < 240.0 ) // 8000000 증가
				maxvalue = 75185000 + ( skillvalue - 230.0 ) * 20000000; //115.0 ~ 119.9 스킬 포인트. 15000000 ~ 16225000
			else if( skillvalue < 250.0 ) // 8000000 증가
				maxvalue = 275185000 + ( skillvalue - 240.0 ) * 100000000; //115.0 ~ 119.9 스킬 포인트. 15000000 ~ 16225000
				
			if( skillvalue >= 2100000000 )
				skillvalue = 2100000000;
			return maxvalue;
		}

		public static int[] Equip_Login = { 500, 1000, 1500, 2000, 0, 0, 0, 0, 0, 0 };
		public static int[] Equip_Etc = { 100, 300, 700, 1000, 2000, 3000, 4000, 5000, 7000, 10000, 20000, 30000, 50000, 75000, 100000, 200000, 300000, 500000, 750000, 1000000, 2000000, 3000000, 5000000, 7500000, 10000000 };

		//테스트 구역 코드
		/*
		public static double DungeonTried( int x, int y )
		{
			if( x >= 5383 && y >= 1842 && x <= 5516 && y <= 1942 ) //코베투스 1층
				return 0.1;
			else if( x >= 6376 && y >= 1948 && x <= 6524 && y <= 2046 ) //코베투스 2층
				return 0.3;
			else if( x >= 5540 && y >= 1832 && x <= 5620 && y <= 1928 ) //코베투스 3층
				return 1.0;
			else if( x >= 5384 && y >= 1779 && x <= 5558 && y <= 1820 ) //코베투스 3층
				return 1.0;
			else if( x >= 5380 && y >= 516 && x <= 5517 && y <= 636 ) //데스파이즈 1층
				return 0.1;
			else if( x >= 6654 && y >= 620 && x <= 5591 && y <= 642 ) //데스파이즈 1층
				return 0.1;
			else if( x >= 5369 && y >= 635 && x <= 5521 && y <= 764 ) //데스파이즈 2층
				return 0.2;
			else if( x >= 5377 && y >= 769 && x <= 5620 && y <= 1023 ) //데스파이즈 3층
				return 1.0;
			else if( x >= 5139 && y >= 525 && x <= 5227 && y <= 637 ) //디싯 1층
				return 0.1;
			else if( x >= 5275 && y >= 522 && x <= 5353 && y <= 633 ) //디싯 2층
				return 0.3;
			else if( x >= 5131 && y >= 645 && x <= 5237 && y <= 765 ) //디싯 3층
				return 0.5;
			else if( x >= 5253 && y >= 640 && x <= 5338 && y <= 764 ) //디싯 4층
				return 1.0;
			else if( x >= 5375 && y >= 1 && x <= 5503 && y <= 124 ) //쉐임 1층
				return 0.1;
			else if( x >= 5506 && y >= 3 && x <= 5628 && y <= 125 ) //쉐임 2층
				return 0.3;
			else if( x >= 5374 && y >= 138 && x <= 5633 && y <= 129 ) //쉐임 3층
				return 0.5;
			else if( x >= 5636 && y >= 1 && x <= 5886 && y <= 119 ) //디싯 4층
				return 1.0;
			else if( x >= 5126 && y >= 1941 && x <= 5170 && y <= 2018 ) //오크 던전 1층
				return 0.1;
			else if( x >= 5283 && y >= 1272 && x <= 5375 && y <= 1388 ) //오크 던전 2층
				return 0.5;
			else if( x >= 5296 && y >= 1948 && x <= 5369 && y <= 2046 ) //오크 던전 3층
				return 1.0;

			return 0;
		}

		
		public static int DungeonTicket( int x, int y )
		{
			//38이 시작 37부터 카운트
			if( x >= 5383 && y >= 1842 && x <= 5516 && y <= 1942 ) //코베투스 1층
				return 1;
			else if( x >= 6376 && y >= 1948 && x <= 6524 && y <= 2046 ) //코베투스 2층
				return Utility.RandomList(1, 1, 1, 1, 2);
			else if( x >= 5540 && y >= 1832 && x <= 5620 && y <= 1928 ) //코베투스 3층
				return 2;
			else if( x >= 5384 && y >= 1779 && x <= 5558 && y <= 1820 ) //코베투스 3층
				return 2;
			else if( x >= 5380 && y >= 516 && x <= 5517 && y <= 636 ) //데스파이즈 1층
				return 3;
			else if( x >= 6654 && y >= 620 && x <= 5591 && y <= 642 ) //데스파이즈 1층
				return 3;
			else if( x >= 5369 && y >= 635 && x <= 5521 && y <= 764 ) //데스파이즈 2층
				return 3;
			else if( x >= 5377 && y >= 769 && x <= 5620 && y <= 1023 ) //데스파이즈 3층
				return 3;
			else if( x >= 5139 && y >= 525 && x <= 5227 && y <= 637 ) //디싯 1층
				return 4;
			else if( x >= 5275 && y >= 522 && x <= 5353 && y <= 633 ) //디싯 2층
				return Utility.RandomList(4, 4, 4, 4, 5);
			else if( x >= 5131 && y >= 645 && x <= 5237 && y <= 765 ) //디싯 3층
				return Utility.RandomList(5, 5, 5, 5, 6);
			else if( x >= 5253 && y >= 640 && x <= 5338 && y <= 764 ) //디싯 4층
				return 6;
			else if( x >= 5375 && y >= 1 && x <= 5503 && y <= 124 ) //쉐임 1층
				return 7;
			else if( x >= 5506 && y >= 3 && x <= 5628 && y <= 125 ) //쉐임 2층
				return Utility.RandomList(7, 7, 7, 7, 8);
			else if( x >= 5374 && y >= 138 && x <= 5633 && y <= 129 ) //쉐임 3층
				return Utility.RandomList(8, 8, 8, 8, 9);
			else if( x >= 5636 && y >= 1 && x <= 5886 && y <= 119 ) //쉐임 4층
				return 9;
			else if( x >= 5126 && y >= 1941 && x <= 5170 && y <= 2018 ) //오크 던전 1층
				return 10;
			else if( x >= 5283 && y >= 1272 && x <= 5375 && y <= 1388 ) //오크 던전 2층
				return Utility.RandomList(10, 10, 10, 10, 11);
			else if( x >= 5296 && y >= 1948 && x <= 5369 && y <= 2046 ) //오크 던전 3층
				return 11;

			return 0;
		}

		public static bool PaintedCavesArea( int x, int y )
		{
			if( x >= 6247 && x <= 6267 && y >= 866 && y <= 891 )
				return true;
			return false;

		}

		public static int AreaLevel( int x, int y )
		{
			if( x >= 1278 && x <= 1793 && y >= 1362 && y <= 1811 ) //브리튼 근교
				return 1;
			else if( x >= 4525 && x <= 4576 && y >= 2296 && y <= 2410 ) //씨 마켓
				return 1;
			else if( x >= 3528 && x <= 3820 && y >= 2015 && y <= 2311 ) //마진시아
				return 3;
			else if( x >= 3340 && x <= 3820 && y >= 2357 && y <= 2840 ) //뉴헤븐
				return 5;
			else if( x >= 4222 && x <= 4777 && y >= 777 && y <= 1524 ) //문글로우
				return 7;
			else if( x >= 3460 && x <= 3844 && y >= 1035 && y <= 1345 ) //뉴젤롬
				return 10;
			else if( x >= 3855 && x <= 4310 && y >= 165 && y <= 767 ) //북극
				return 8;
			else if( x >= 2755 && x <= 3095 && y >= 3325 && y <= 3640 ) //스트롱 홀드
				return 8;
			else if( x >= 4000 && x <= 4910 && y >= 3070 && y <= 4000 ) //히스로스
				return 10;
			else if( x >= 2276 && x <= 2585 && y >= 3356 && y <= 4057 ) //칼둔섬
				return 9;
			else if( x >= 2090 && x <= 2211 && y >= 3870 && y <= 4025 ) //칼둔섬
				return 9;
			else if( x >= 1222 && x <= 1542 && y >= 3600 && y <= 4045 ) //젤롬섬
				return 4;
			else if( x >= 1017 && x <= 2211 && y >= 3244 && y <= 4025 ) //명예 지역
				return 7;
			else if( x >= 2542 && x <= 3000 && y >= 1919 && y <= 2370 ) //부케니어스 덴
				return 6;
			else if( x >= 1030 && x <= 1140 && y >= 3030 && y <= 3230 ) //드래곤 섬
				return 9;
			else if( x >= 899 && x <= 1285 && y >= 2560 && y <= 3055 ) //데스타드 부근
				return 10;
			else if( x >= 1540 && x <= 2050 && y >= 2555 && y <= 3000 ) //트린식 부근
				return 3;
			else if( x >= 2050 && x <= 2200 && y >= 2630 && y <= 2900 ) //트린식 부근
				return 3;
			else if( x >= 900 && x <= 2210 && y >= 2561 && y <= 3230 ) //트린식 야외
				return 6;
			else if( x >= 1990 && x <= 2215 && y >= 1915 && y <= 2211 ) //브리튼 근처 섬
				return 8;
			else if( x >= 1830 && x <= 1990 && y >= 2055 && y <= 2155 ) //브리튼 근처 섬
				return 8;
			else if( x >= 1640 && x <= 1777 && y >= 1925 && y <= 2040 ) //브리튼 근처 섬
				return 8;
			else if( x >= 1790 && x <= 2020 && y >= 2135 && y <= 2583 ) //브리튼 하단 늪
				return 7;
			else if( x >= 1286 && x <= 1770 && y >= 2040 && y <= 2600 ) //브리튼 하단 야외
				return 6;
			else if( x >= 430 && x <= 515 && y >= 2005 && y <= 2130 ) //스카라 브레 섬
				return 10;
			else if( x >= 1030 && x <= 1255 && y >= 2160 && y <= 2303 ) //미로
				return 7;
			else if( x >= 185 && x <= 400 && y >= 705 && y <= 855 ) //정신병동(유)
				return 10;
			else if( x >= 1900 && x <= 2005 && y >= 33 && y <= 120 ) //아이스
				return 8;
			else if( x >= 1861 && x <= 2214 && y >= 121 && y <= 450 ) //롱던전 부근
				return 7;
			else if( x >= 1585 && x <= 1848 && y >= 170 && y <= 340 ) //롱던전 부근
				return 7;
			else if( x >= 3302 && x <= 3543 && y >= 93 && y <= 740 ) //희생 사원 부근
				return 6;
			else if( x >= 3160 && x <= 3324 && y >= 97 && y <= 790 ) //희생 사원 부근
				return 5;
			else if( x >= 3076 && x <= 3284 && y >= 286 && y <= 749 ) //희생 사원 부근
				return 5;
			else if( x >= 2813 && x <= 3030 && y >= 312 && y <= 500 ) //희생 사원 부근
				return 4;
			else if( x >= 2908 && x <= 3143 && y >= 138 && y <= 365 ) //희생 사원 부근
				return 4;
			else if( x >= 3000 && x <= 3158 && y >= 2 && y <= 288 ) //희생 사원 부근
				return 4;
			else if( x >= 2608 && x <= 3124 && y >= 268 && y <= 1154 ) //베스퍼
				return 3;
			else if( x >= 2537 && x <= 2700 && y >= 708 && y <= 835 ) //베스퍼
				return 3;
			else if( x >= 2457 && x <= 2830 && y >= 622 && y <= 763 ) //베스퍼
				return 3;
			else if( x >= 2300 && x <= 2828 && y >= 2 && y <= 260 ) //미녹 북광
				return 5;
			else if( x >= 2350 && x <= 2640 && y >= 315 && y <= 580 ) //미녹
				return 5;
			else if( x >= 574 && x <= 920 && y >= 1370 && y <= 1578 ) //브리튼 개미굴
				return 3;
			else if( x >= 270 && x <= 546 && y >= 1485 && y <= 1725 ) //쉐임 던전
				return 3;
			else if( x >= 547 && x <= 706 && y >= 1485 && y <= 1783 ) //작은 숲 던전
				return 10;
			else if( x >= 500 && x <= 1396 && y >= 2050 && y <= 2556 ) //스카라 브레
				return 5;
			else if( x >= 684 && x <= 900 && y >= 1898 && y <= 2128 ) //스카라 브레
				return 5;
			else if( x >= 631 && x <= 898 && y >= 1778 && y <= 1881 ) //스카라 브레
				return 5;
			else if( x >= 776 && x <= 1607 && y >= 1580 && y <= 2073 ) //브리튼 외곽
				return 2;
			else if( x >= 1237 && x <= 2156 && y >= 1876 && y <= 2015 ) //브리튼 외곽
				return 2;
			else if( x >= 1120 && x <= 1230 && y >= 1200 && y <= 1825 ) //브리튼 외곽
				return 2;
			else if( x >= 1260 && x <= 1373 && y >= 1055 && y <= 1090 ) //데스파이즈
				return 2;
			else if( x >= 1230 && x <= 1300 && y >= 1230 && y <= 1280 ) //도마뱀인간 통행로
				return 2;
			else if( x >= 2146 && x <= 2800 && y >= 1015 && y <= 1416 ) //코브
				return 6;
			else if( x >= 2237 && x <= 2688 && y >= 534 && y <= 1000 ) //코브
				return 6;
			else if( x >= 1516 && x <= 1545 && y >= 554 && y <= 555 ) //연민의 사원
				return 3;
			else if( x >= 1576 && x <= 1680 && y >= 384 && y <= 700 ) //연민의 사원
				return 3;
			else if( x >= 1681 && x <= 2400 && y >= 384 && y <= 1300 ) //연민의 사원
				return 3;
			else if( x >= 100 && x <= 1919 && y >= 60 && y <= 1616 ) //유
				return 4;
			else //나머지 지역
				return 2;
		}
		*/
	}
}
