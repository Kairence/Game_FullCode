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
					ItemCreate( item, rank, equip.PlayerConstructed, pm, equip.PrefixOption[99], equip.SuffixOption[99], true );
				}
			}
		}
		
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
		
		public static void ItemIdentified( Mobile from, int skillvalue, Item item, bool wandcheck = false, bool armslore = false )
		{
			if( item is IDWand )
			{
				if( !wandcheck && !armslore )
				{
					IDWand checkitem = item as IDWand;
					int save = (int)(from.Skills.ItemID.Value * 10 ) / 200;
					if( save < 5 && IdentifiedSuccess( save, skillvalue ) )
						save++;
					if( save > 0 )
						checkitem.SaveSkill = save + 3;
					from.SendMessage("당신은 {0}의 감정이 가능한 아이템 완드를 제작하였습니다.", ItemRankName(save + 3));
					from.CheckSkill( SkillName.ItemID, 1000 + save * 2500 );
				}
				else
				{
					from.SendMessage("아이템 완드로 아이템 완드 제작은 불가능합니다.");
					return;
				}					
			}
			else if( item is BaseWeapon )
			{
				BaseWeapon checkitem = item as BaseWeapon;
				if( checkitem.Identified )
				{
					from.SendMessage("이미 감정된 아이템입니다.");
				}
				else
				{
					int rank = (int)checkitem.ItemPower;
					if( rank > 1 )
						rank -= 3;
					if( armslore && !checkitem.PlayerConstructed )
					{
						from.SendMessage("제작 장비만 감정할 수 있습니다...");
						return;
					}
					if( !wandcheck )
					{
						if( !armslore )
							from.CheckSkill( SkillName.ItemID, 100 + rank * 500 );
						else
							from.CheckSkill( SkillName.ArmsLore, 100 + rank * 500 );
					}
					
					if( IdentifiedSuccess( rank, skillvalue ) )
					{
						Good_Effect(from);
						from.SendMessage("아이템 감정에 성공합니다!");
						checkitem.Identified = true;
					}
					else
					{
						from.SendMessage("아이템 감정에 실패합니다...");
						if( checkitem.MaxHitPoints <= 1 )
							checkitem.Delete();
						else
						{
							checkitem.MaxHitPoints--;
							if( checkitem.HitPoints > checkitem.MaxHitPoints )
								checkitem.HitPoints = checkitem.MaxHitPoints;
						}
					}
				}
			}
			else if( item is BaseArmor )
			{
				BaseArmor checkitem = item as BaseArmor;
				if( checkitem.Identified )
				{
					from.SendMessage("이미 감정된 아이템입니다.");
				}
				else
				{
					int rank = (int)checkitem.ItemPower;
					if( rank > 1 )
						rank -= 3;
					if( armslore && !checkitem.PlayerConstructed )
					{
						from.SendMessage("제작 장비만 감정할 수 있습니다...");
						return;
					}
					if( !wandcheck )
					{
						if( !armslore )
							from.CheckSkill( SkillName.ItemID, 100 + rank * 500 );
						else
							from.CheckSkill( SkillName.ArmsLore, 100 + rank * 500 );
					}
					if( IdentifiedSuccess( rank, skillvalue ) )
					{
						Good_Effect(from);
						from.SendMessage("아이템 감정에 성공합니다!");
						checkitem.Identified = true;
					}
					else
					{
						from.SendMessage("아이템 감정에 실패합니다...");
						if( checkitem.MaxHitPoints <= 1 )
							checkitem.Delete();
						else
						{
							checkitem.MaxHitPoints--;
							if( checkitem.HitPoints > checkitem.MaxHitPoints )
								checkitem.HitPoints = checkitem.MaxHitPoints;
						}
					}
				}
			}
			else if( item is BaseClothing )
			{
				BaseClothing checkitem = item as BaseClothing;
				if( checkitem.Identified )
				{
					from.SendMessage("이미 감정된 아이템입니다.");
				}
				else
				{
					int rank = (int)checkitem.ItemPower;
					if( rank > 1 )
						rank -= 3;
					if( armslore && !checkitem.PlayerConstructed )
					{
						from.SendMessage("제작 장비만 감정할 수 있습니다...");
						return;
					}
					if( !wandcheck )
					{
						if( !armslore )
							from.CheckSkill( SkillName.ItemID, 100 + rank * 500 );
						else
							from.CheckSkill( SkillName.ArmsLore, 100 + rank * 500 );
					}
					if( IdentifiedSuccess( rank, skillvalue ) )
					{
						Good_Effect(from);
						from.SendMessage("아이템 감정에 성공합니다!");
						checkitem.Identified = true;
					}
					else
					{
						from.SendMessage("아이템 감정에 실패합니다...");
						if( checkitem.MaxHitPoints <= 1 )
							checkitem.Delete();
						else
						{
							checkitem.MaxHitPoints--;
							if( checkitem.HitPoints > checkitem.MaxHitPoints )
								checkitem.HitPoints = checkitem.MaxHitPoints;
						}
					}
				}
			}
			else if( item is BaseJewel )
			{
				BaseJewel checkitem = item as BaseJewel;
				if( checkitem.Identified )
				{
					from.SendMessage("이미 감정된 아이템입니다.");
				}
				else
				{
					int rank = (int)checkitem.ItemPower;
					if( rank > 1 )
						rank -= 3;
					if( armslore && !checkitem.PlayerConstructed )
					{
						from.SendMessage("제작 장비만 감정할 수 있습니다...");
						return;
					}
					if( !wandcheck )
					{
						if( !armslore )
							from.CheckSkill( SkillName.ItemID, 100 + rank * 500 );
						else
							from.CheckSkill( SkillName.ArmsLore, 100 + rank * 500 );
					}
					if( IdentifiedSuccess( rank, skillvalue ) )
					{
						Good_Effect(from);
						from.SendMessage("아이템 감정에 성공합니다!");
						checkitem.Identified = true;
					}
					else
					{
						from.SendMessage("아이템 감정에 실패합니다...");
						if( checkitem.MaxHitPoints <= 1 )
							checkitem.Delete();
						else
						{
							checkitem.MaxHitPoints--;
							if( checkitem.HitPoints > checkitem.MaxHitPoints )
								checkitem.HitPoints = checkitem.MaxHitPoints;
						}
					}
				}
			}			
			else if( item is Spellbook )
			{
				Spellbook checkitem = item as Spellbook;
				if( checkitem.Identified )
				{
					from.SendMessage("이미 감정된 아이템입니다.");
				}
				else
				{
					int rank = (int)checkitem.ItemPower;
					if( rank > 1 )
						rank -= 3;
					if( armslore && !checkitem.PlayerConstructed )
					{
						from.SendMessage("제작 장비만 감정할 수 있습니다...");
						return;
					}
					if( !wandcheck )
					{
						if( !armslore )
							from.CheckSkill( SkillName.ItemID, 100 + rank * 500 );
						else
							from.CheckSkill( SkillName.ArmsLore, 100 + rank * 500 );
					}
					if( IdentifiedSuccess( rank, skillvalue ) )
					{
						Good_Effect(from);
						from.SendMessage("아이템 감정에 성공합니다!");
						checkitem.Identified = true;
					}
					else
					{
						from.SendMessage("아이템 감정에 실패합니다...");
						if( checkitem.MaxHitPoints <= 1 )
							checkitem.Delete();
						else
						{
							checkitem.MaxHitPoints--;
							if( checkitem.HitPoints > checkitem.MaxHitPoints )
								checkitem.HitPoints = checkitem.MaxHitPoints;
						}
					}
				}
			}
			else if( item is Container )
			{
				Container pack = item as Container;
				if( pack != null )
				{
					int success = 0;
					int count = 0;
					List<BaseWeapon> weapon = pack.FindItemsByType<BaseWeapon>();
					for( int i = weapon.Count -1; i >=0; i--)
					{
						count++;
						if( weapon[i].Identified )
							continue;
						int rank = (int)weapon[i].ItemPower;
						if( rank > 1 )
							rank -= 3;
						if( armslore && !weapon[i].PlayerConstructed )
						{
							continue;
						}
						if( !wandcheck )
						{
							if( !armslore )
								from.CheckSkill( SkillName.ItemID, 100 + rank * 500 );
							else
								from.CheckSkill( SkillName.ArmsLore, 100 + rank * 500 );
						}
						if( IdentifiedSuccess( rank, skillvalue ) )
						{
							weapon[i].Identified = true;
							success++;
						}
						else
						{
							if( weapon[i].MaxHitPoints <= 1 )
								weapon[i].Delete();
							else
							{
								weapon[i].MaxHitPoints--;
								if( weapon[i].HitPoints > weapon[i].MaxHitPoints )
									weapon[i].HitPoints = weapon[i].MaxHitPoints;
							}
						}
					}
					List<BaseArmor> armor = pack.FindItemsByType<BaseArmor>();
					for( int i = armor.Count -1; i >=0; i--)
					{
						count++;
						if( armor[i].Identified )
							continue;
						int rank = (int)armor[i].ItemPower;
						if( rank > 1 )
							rank -= 3;
						if( armslore && !armor[i].PlayerConstructed )
						{
							continue;
						}
						if( !wandcheck )
						{
							if( !armslore )
								from.CheckSkill( SkillName.ItemID, 100 + rank * 500 );
							else
								from.CheckSkill( SkillName.ArmsLore, 100 + rank * 500 );
						}
						if( IdentifiedSuccess( rank, skillvalue ) )
						{
							armor[i].Identified = true;
							success++;
						}
						else
						{
							if( armor[i].MaxHitPoints <= 1 )
								armor[i].Delete();
							else
							{
								armor[i].MaxHitPoints--;
								if( armor[i].HitPoints > armor[i].MaxHitPoints )
									armor[i].HitPoints = armor[i].MaxHitPoints;
							}
						}
					}
					List<BaseClothing> clothing = pack.FindItemsByType<BaseClothing>();
					for( int i = clothing.Count -1; i >=0; i--)
					{
						count++;
						if( clothing[i].Identified )
							continue;
						int rank = (int)clothing[i].ItemPower;
						if( rank > 1 )
							rank -= 3;
						if( armslore && !clothing[i].PlayerConstructed )
						{
							continue;
						}
						if( !wandcheck )
						{
							if( !armslore )
								from.CheckSkill( SkillName.ItemID, 100 + rank * 500 );
							else
								from.CheckSkill( SkillName.ArmsLore, 100 + rank * 500 );
						}
						if( IdentifiedSuccess( rank, skillvalue ) )
						{
							clothing[i].Identified = true;
							success++;
						}
						else
						{
							if( clothing[i].MaxHitPoints <= 1 )
								clothing[i].Delete();
							else
							{
								clothing[i].MaxHitPoints--;
								if( clothing[i].HitPoints > clothing[i].MaxHitPoints )
									clothing[i].HitPoints = clothing[i].MaxHitPoints;
							}
						}
					}
					List<BaseJewel> jewel = pack.FindItemsByType<BaseJewel>();
					for( int i = jewel.Count -1; i >=0; i--)
					{
						count++;
						if( jewel[i].Identified )
							continue;
						int rank = (int)jewel[i].ItemPower;
						if( rank > 1 )
							rank -= 3;
						if( armslore && !jewel[i].PlayerConstructed )
						{
							continue;
						}
						if( !wandcheck )
						{
							if( !armslore )
								from.CheckSkill( SkillName.ItemID, 100 + rank * 500 );
							else
								from.CheckSkill( SkillName.ArmsLore, 100 + rank * 500 );
						}
						if( IdentifiedSuccess( rank, skillvalue ) )
						{
							jewel[i].Identified = true;
							success++;
						}
						else
						{
							if( jewel[i].MaxHitPoints <= 1 )
								jewel[i].Delete();
							else
							{
								jewel[i].MaxHitPoints--;
								if( jewel[i].HitPoints > jewel[i].MaxHitPoints )
									jewel[i].HitPoints = jewel[i].MaxHitPoints;
							}
						}
					}
					List<Spellbook> spellbook = pack.FindItemsByType<Spellbook>();
					for( int i = spellbook.Count -1; i >=0; i--)
					{
						count++;
						if( spellbook[i].Identified )
							continue;
						int rank = (int)spellbook[i].ItemPower;
						if( rank > 1 )
							rank -= 3;
						if( armslore && !spellbook[i].PlayerConstructed )
						{
							continue;
						}
						if( !wandcheck )
						{
							if( !armslore )
								from.CheckSkill( SkillName.ItemID, 100 + rank * 500 );
							else
								from.CheckSkill( SkillName.ArmsLore, 100 + rank * 500 );
						}
						if( IdentifiedSuccess( rank, skillvalue ) )
						{
							spellbook[i].Identified = true;
							success++;
						}
						else
						{
							if( spellbook[i].MaxHitPoints <= 1 )
								spellbook[i].Delete();
							else
							{
								spellbook[i].MaxHitPoints--;
								if( spellbook[i].HitPoints > spellbook[i].MaxHitPoints )
									spellbook[i].HitPoints = spellbook[i].MaxHitPoints;
							}
						}
					}
					if( count > 0 )
						from.SendMessage("당신은 총 {0}개의 아이템 중에 {1}개를 성공했습니다!", count, success );
				}				
			}
		}
		
		
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

		//슬레이어 데미지 계산
		public static double MonsterTierSlayerDamage(BaseCreature from )
		{
			if( from.Boss )
				return 0;
			else if( from.Grade == 7 )
				return 0.1;
			else if( from.Grade == 6 )
				return 0.25;
			else if( from.Grade < 1 )
				return 0.5;
			else
				return 1.0;
		}
		//슬레이어 데미지 계산
		public static double GetSlayerDamageScalar(Mobile attacker, Mobile defender)
		{
			double scalar = 1.0;
			
            int slayer_Damage = SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.HumanoidDamage);
			
			if( defender is BaseCreature )
			{
				BaseCreature bc = defender as BaseCreature;
				if( slayer_Damage > 0 )
				{
					if( Util.SlayerCheck(SlayerName.Repond, defender) )
						scalar += slayer_Damage * 0.001 * MonsterTierSlayerDamage(bc);
				}
				slayer_Damage = SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.UndeadDamage);
				if( slayer_Damage > 0 )
				{
					if( Util.SlayerCheck(SlayerName.Silver, defender) )
						scalar += slayer_Damage * 0.001 * MonsterTierSlayerDamage(bc);
				}
				slayer_Damage = SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.ElementalDamage);
				if( slayer_Damage > 0 )
				{
					if( Util.SlayerCheck(SlayerName.ElementalBan, defender) )
						scalar += slayer_Damage * 0.001 * MonsterTierSlayerDamage(bc);
				}
				slayer_Damage = SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.AbyssDamage);
				if( slayer_Damage > 0 )
				{
					if( Util.SlayerCheck(SlayerName.Exorcism, defender) )
						scalar += slayer_Damage * 0.001 * MonsterTierSlayerDamage(bc);
				}
				slayer_Damage = SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.ArachnidDamage);
				if( slayer_Damage > 0 )
				{
					if( Util.SlayerCheck(SlayerName.ArachnidDoom, defender) )
						scalar += slayer_Damage * 0.001 * MonsterTierSlayerDamage(bc);
				}
				slayer_Damage = SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.ReptilianDamage);
				if( slayer_Damage > 0 )
				{
					if( Util.SlayerCheck(SlayerName.ReptilianDeath, defender) )
						scalar += slayer_Damage * 0.001 * MonsterTierSlayerDamage(bc);
				}
				slayer_Damage = SAAbsorptionAttributes.GetValue(attacker, SAAbsorptionAttribute.FeyDamage);
				if( slayer_Damage > 0 )
				{
					if( Util.SlayerCheck(SlayerName.Fey, defender) )
						scalar += slayer_Damage * 0.001 * MonsterTierSlayerDamage(bc);
				}
			}
			return scalar;
		}		
		public static int[,] MonsterLandTier =
		{
			//	1티어		엘리트		치프
			{	940,		990,		1000	}, //Trammel
			{	0,			800,		900		}, //Felucca
			{	940,		990,		1000	}, //Ilshenar
		};

		public static void GradeBonus(BaseCreature bc)
		{
			switch(bc.Grade)
			{
				case 2:
				{
					bc.HitsMaxSeed *= 2;

					bc.RawStr *= 110;
					bc.RawStr /= 100;

					bc.RawDex *= 110;
					bc.RawDex /= 100;

					bc.RawInt *= 110;
					bc.RawInt /= 100;
					break;
				}
				case 3:
				{
					bc.HitsMaxSeed *= 120;
					bc.HitsMaxSeed /= 100;

					bc.RawStr *= 110;
					bc.RawStr /= 100;

					bc.RawDex *= 110;
					bc.RawDex /= 100;

					bc.RawInt *= 150;
					bc.RawInt /= 100;
					break;
				}
				case 4:
				{
					bc.HitsMaxSeed *= 120;
					bc.HitsMaxSeed /= 100;

					bc.RawStr *= 150;
					bc.RawStr /= 100;

					bc.RawDex *= 110;
					bc.RawDex /= 100;

					bc.RawInt *= 110;
					bc.RawInt /= 100;
					break;
				}
				case 5:
				{
					bc.HitsMaxSeed *= 120;
					bc.HitsMaxSeed /= 100;

					bc.RawStr *= 110;
					bc.RawStr /= 100;

					bc.RawDex *= 150;
					bc.RawDex /= 100;

					bc.RawInt *= 110;
					bc.RawInt /= 100;
					break;
				}
				case 6:
				{
					bc.HitsMaxSeed *= 250;
					bc.HitsMaxSeed /= 100;

					bc.RawStr *= 175;
					bc.RawStr /= 100;

					bc.RawDex *= 175;
					bc.RawDex /= 100;

					bc.RawInt *= 175;
					bc.RawInt /= 100;
					break;
				}
				case 7:
				{
					bc.HitsMaxSeed *= 500;
					bc.HitsMaxSeed /= 100;

					bc.RawStr *= 300;
					bc.RawStr /= 100;

					bc.RawDex *= 300;
					bc.RawDex /= 100;

					bc.RawInt *= 300;
					bc.RawInt /= 100;
					for (int i = 0; i < bc.Skills.Length; i++)
					{
						Skill skill = (Skill)bc.Skills[i];

						if (skill.Base > 0.0)
							skill.Base *= ( 120 );
					}
					break;
				}
			}
			
			
			
			bc.Hits = bc.HitsMax;
			bc.Stam = bc.StamMax;
			bc.Mana = bc.ManaMax;
		}
		
		public static void GradeCreate(BaseCreature bc, Point3D loocation, Map m)
		{
			if( bc.Grade > 0 )
				return;
			
			if( bc.Boss )
			{
				bc.Grade = 1;
				return;
			}
			int dice = Utility.RandomMinMax( 1, 1000 );
			int Landcheck = 0;
			if( m == Map.Felucca )
				Landcheck = 1;
				
			if( dice >= MonsterLandTier[Landcheck, 2] )
				bc.Grade = 7;
			else if ( dice >= MonsterLandTier[Landcheck, 1] )
				bc.Grade = 6;
			else if ( dice >= MonsterLandTier[Landcheck, 0] )
			{
				int subdice = Utility.RandomMinMax( 1, 4 );
				switch( subdice )
				{
					case 1:
						bc.Grade = 5;
						break;
					case 2:
						bc.Grade = 4;
						break;
					case 3:
						bc.Grade = 3;
						break;
					case 4:
						bc.Grade = 2;
						break;
				}
			}
			else
				bc.Grade = 1;
		
			int strbonus = 0;
			int dexbonus = 0;
			int intbonus = 0;
			int hitsbonus = 0;
			int skillbonus = 0;
			//int expbonus = 0;

			GradeBonus(bc);
		}
		
		public static int MonsterGrade( int grade )
		{
			switch( grade )
			{
				case 1:
					break;
				case 2:
					grade = 2;
					break;
				case 3:
					grade = 2;
					break;
				case 4:
					grade = 2;
					break;
				case 5:
					grade = 2;
					break;
				case 6:
					grade = 3;
					break;
				case 7:
					grade = 4;
					break;
			}
			return grade;
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
		
		public static int ItemRankMaker( double luck )
		{
			int rank = 1;
			for( int i = 3; i >= 0; --i )
			{
				if( Utility.RandomDouble() < ( ItemRankList[i] + luck * ItemRankLuckBonus[i] ) )
				{
					return i + 2;
				}
			}
			return rank;
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


		#region 유물 설정

		#endregion
		
		
		#region 신규 옵션 코드
		//강화 성공 확률
		public static readonly int[,,] NewItemPowerUpgrade = new int[,,]
		{
			//확률, 일반재료, 일반상승치, 희귀, 희귀, 영웅, 영웅, 서사, 서사, 전설, 전설, 신화, 신화
			{{ 	9000, 3, 500, 7, 600, 12, 800, 20, 1000, 30, 1400, 45, 1900 }, //1강
			{	9000, 3, 100, 7, 120, 12, 150, 20, 200, 30, 275, 45, 375	}},
			{{ 	8500, 7, 600, 12, 700, 20, 900, 30, 1200, 45, 1700, 60, 2300 }, //2강
			{	8500, 7, 110, 12, 132, 20, 165, 30, 220, 45, 303, 60, 413	}},
			{{	8000, 12, 700, 20, 800, 30, 1100, 45, 1400, 60, 1900, 80, 2600 }, //3강
			{	8000, 12, 120, 20, 144, 30, 180, 45, 240, 60, 330, 80, 450	}},
			{{ 	7500, 20, 800, 30, 1000, 45, 1200, 60, 1600, 80, 2200, 100, 3000 }, //4강
			{	7500, 20, 135, 30, 162, 45, 203, 60, 270, 80, 371, 100, 506	}},
			{{ 	7000, 30, 900, 45, 1100, 60, 1400, 80, 1800, 100, 2500, 120, 3400 }, //5강
			{	7000, 30, 150, 45, 180, 60, 225, 80, 300, 100, 413, 120, 563 }},
			{{ 	6500, 45, 1100, 60, 1300, 80, 1700, 100, 2200, 120, 3000, 140, 4100 }, //6강
			{	6500, 45, 170, 60, 204, 80, 255, 100, 340, 120, 468, 140, 638}},
			{{ 	6000, 60, 1300, 80, 1600, 100, 2000, 120, 2600, 140, 3600, 160, 4900 }, //7강
			{	6000, 60, 200, 80, 240, 100, 300, 120, 400, 140, 550, 160, 750	}},
			{{ 	5500, 80, 1500, 100, 1900, 120, 2300, 140, 3000, 160, 4100, 180, 5600 }, //8강
			{	5500, 80, 250, 100, 300, 120, 375, 140, 500, 160, 688, 180, 938	}},
			{{ 	5000, 100, 1800, 120, 2200, 140, 2700, 160, 3600, 180, 5000, 200, 6800 }, //9강
			{	5000, 100, 300, 120, 360, 140, 450, 160, 600, 180, 825, 200, 1125}},
			{{ 	4000, 120, 2200, 140, 2600, 160, 3300, 180, 4400, 200, 6100, 225, 8300 }, //10강
			{	4000, 120, 400, 140, 480, 160, 600, 180, 800, 200, 1100, 225, 1500	}},
			{{ 	3000, 140, 2600, 160, 3100, 180, 3900, 200, 5200, 225, 7200, 250, 9800 }, //11강
			{	3000, 140, 500, 160, 600, 180, 750, 200, 1000, 225, 1375, 250, 1875 }},
			{{ 	2000, 160, 3000, 180, 3600, 200, 4500, 225, 6000, 250, 8300, 275, 11300 }, //12강
			{	2000, 160, 650, 180, 780, 200, 975, 225, 1300, 250, 1788, 275, 2438	}},
			{{ 	1500, 180, 3500, 200, 4200, 225, 5300, 250, 7000, 275, 9600, 300, 13100 }, //13강
			{	1500, 180, 900, 200, 1080, 225, 1350, 250, 1800, 275, 2475, 300, 3375 }},
			{{ 	1200, 200, 4000, 225, 4800, 250, 6000, 275, 8000, 300, 11000, 350, 15000 }, //14강
			{	1200, 200, 1300, 225, 1560, 250, 1950, 275, 2600, 300, 3575, 350, 4875 }},
			{{ 	1000, 225, 5000, 250, 6000, 275, 7500, 300, 10000, 350, 13800, 400, 18800 }, //15강
			{	1000, 225, 2000, 250, 2400, 275, 3000, 300, 4000, 350, 5500, 400, 7500 }},
			{{ 	800, 250, 6000, 275, 7200, 300, 9000, 350, 12000, 400, 16500, 500, 22500 }, //16강
			{	800, 250, 3200, 275, 3840, 300, 4800, 350, 6400, 400, 8800, 500, 12000	}},
			{{ 	600, 275, 7000, 300, 8400, 350, 10500, 400, 14000, 500, 19300, 600, 26300 }, //17강
			{	600, 275, 5000, 300, 6000, 350, 7500, 400, 10000, 500, 13750, 600, 18750 }},
			{{ 	4000, 300, 8000, 350, 9600, 400, 12000, 500, 16000, 600, 22000, 700, 30000 }, //18강
			{	4000, 300, 7500, 350, 9000, 400, 11250, 500, 15000, 600, 20625, 700, 28125 }},
			{{ 	200, 350, 9000, 400, 10800, 500, 13500, 600, 18000, 700, 24800, 800, 33800 }, //19강
			{	200, 350, 12000, 400, 14400, 500, 18000, 600, 24000, 700, 33000, 800, 45000 }},
			{{ 	100, 400, 10000, 500, 12000, 600, 15000, 700, 20000, 800, 27500, 1000, 37500 }, //20강
			{	100, 400, 20000, 500, 24000, 600, 30000, 700, 40000, 800, 55000, 1000, 75000}}
		};

		public static int[] NewItemPowerOption = new int[]
		{
			//물리 데미지 증가, 화염 데미지 증가, 냉기 데미지 증가, 독 데미지 증가, 에너지 데미지 증가, 혼돈 데미지 증가(102), 신성 데미지 증가(103), 무기 뎀감(104), 마법 뎀감(105), 기절 시간 감소(106)
			22, 23, 24, 25, 26, 102, 103, 104, 105, 106
		};
		
		public void UpgradeMessage(Mobile from, bool success, int rank)
		{
			int failcheck = success ? 0 : 1;
			int color = success ? 1165 : 1166;
			if( rank < 9 + failcheck )
			{
				if( success )
					from.SendMessage("{0}강화에 성공하였습니다!!!", rank );
				else
					from.SendMessage("{0}강화에 실패하였습니다...", rank );
			}
			else
			{
				string casting = from.Name + "님이 " + rank.ToString() + "강화에 ";
				if( success )
				{
					casting += "성공하셨습니다!!!";
				}
				else
				{
					casting += "실패하셨습니다...";
				}
				World.Broadcast( color, true, casting );
			}
		}
		
		public static bool NewItemPowerChance(int upgrade)
		{
			int dice = Utility.RandomMinMax(1, 10000);
			if( dice <= NewItemPowerUpgrade[upgrade + 1, 0, 0] )
				return true;
			else
				return false;
		}
		
		public static void NewItemPowerMake(Item equip, int scroll)
		{
			//접두 3 ~ 10 : 강화 종류
			//접미 3 ~ 10 : 강화 레벨
			if( equip is IEquipOption )
			{
				IEquipOption item = equip as IEquipOption;
				int check = NewEquipNumber(equip);
				int itemline = NewItemLine(check);
				item.SuffixOption[3 + scroll] = NewItemPowerUpgrade[ item.PrefixOption[3 + scroll], itemline, (item.SuffixOption[1] + 1) * 2 ];
				int optioncheck = NewItemPowerOption[itemline == 0 ? scroll : scroll + 7];
				int itemvalue = item.SuffixOption[3 + scroll];
				if( item.PrefixOption[3 + scroll] > 0 )
				{
					itemvalue = NewItemPowerUpgrade[ item.PrefixOption[3 + scroll] + 1, itemline, ( (scroll + 1) * 2) ] - item.SuffixOption[3 + scroll];
				}
				item.PrefixOption[3 + scroll]++;
				NewEquipOptionList( equip, optioncheck, itemvalue, 0 );
				//Console.WriteLine("강화 번호 : {0}, optioncheck : {1}, itemvalue : {2}, rank : {3}", 3 + scroll, optioncheck, itemvalue, item.SuffixOption[1] );
			}
		}
		
        //아이템 옵션 설정
		public static int OPLPercentCheck(int number, int step = 1)
		{
			int check = 0;
			if( number >= 1080585 && number <= 1080597 )
				check = step;
			else if( number >= 1080600 && number <= 1080609 )
				check = step;
			else if( number >= 1080615 && number <= 1080624 )
				check = step;
			else if( number >= 1080629 && number <= 1080640 )
				check = step;
			else if( number >= 1080651 && number <= 1080654 )
				check = step;
			return check;
			
		}
			//1080578부터 시작
        public static readonly int[,,] NewEquipOption = new int[,,]
		{
			//	이름,			Score, 	Min, 	Max
			{{ 	1080578,	 	2,		10,		1500},	//0 힘 증가
			{	1080578,	 	1,		1,		300	},
			{	1080578,	 	1,		2,		600	}},	
			{{ 	1080579,	 	2,		10,		1500},	//1 민첩성 증가
			{	1080579,	 	1,		1,		300	},
			{	1080579,	 	1,		2,		600	}},	
			{{ 	1080580,	 	2,		10,		1500},	//2 지능 증가
			{	1080580,	 	1,		1,		300	},
			{	1080580,	 	1,		2,		600	}},	
			{{ 	1080581,	 	2,		1000,	150000},	//3 운 증가
			{	1080581,	 	1,		100,	30000},
			{	1080581,	 	1,		200,	60000}},	
			{{ 	1080582,	 	1,		500,	150000 },	//4 체력 증가
			{	1080582,	 	1,		100,	30000},
			{	1080582,	 	1,		200,	60000 }},	
			{{ 	1080583,	 	1,		500,	150000	},	//5 기력 증가
			{	1080583,	 	1,		100,	30000	},
			{	1080583,	 	1,		200,	60000	}},	
			{{ 	1080584,	 	1,		500,	150000	},	//6 마나 증가
			{	1080584,	 	1,		100,	30000	},
			{	1080584,	 	1,		200,	60000	}},	
			{{ 	1080585,	 	1,		50,		15000},		//7 피해 증가%
			{	1080585,	 	4,		10,		750 },
			{	1080585,	 	2,		10,		1500 }},	
			{{ 	1080586,	 	1,		50,		15000},	//8 주문 피해 증가%
			{	1080586,	 	4,		10,		750 },
			{	1080586,	 	2,		10,		1500 }},	
			{{ 	1080587,	 	0,		200,	250},	//9 관통 피해 증가%
			{	1080587,	 	0,		60,		70 },
			{	1080587,	 	0,		100,	150 }},	
			{{ 	1080588,	 	0,		200,	250},	//10 충격 피해 증가%
			{	1080588,	 	0,		60,		70 },
			{	1080588,	 	0,		100,	150 }},	
			{{ 	1080589,	 	0,		200,	250},	//11 출혈 피해 증가%
			{	1080589,	 	0,		60,		70 },
			{	1080589,	 	0,		100,	150 }},	
			{{ 	1080590,	 	2,		100,	15000},	//12 물리 저항력%
			{	1080590,	 	5,		100,	6000},
			{	1080590,	 	5,		100,	6000}},	
			{{ 	1080591,	 	2,		100,	15000},	//13 화염 저항력%
			{	1080591,	 	5,		100,	6000},
			{	1080591,	 	5,		100,	6000}},	
			{{ 	1080592,	 	2,		100,	15000},	//14 냉기 저항력%
			{	1080592,	 	5,		100,	6000},
			{	1080592,	 	5,		1002,	6000}},	
			{{ 	1080593,	 	2,		100,	15000},	//15 독 저항력%
			{	1080593,	 	5,		100,	6000},
			{	1080593,	 	5,		100,	6000}},	
			{{ 	1080594,	 	2,		100,	15000},	//16 에너지 저항력%
			{	1080594,	 	5,		100,	6000},
			{	1080594,	 	5,		100,	6000}},	
			{{ 	1080595,	 	1,		100,	30000},	//17 명중 확률 증가%
			{	1080595,	 	20,		100,	1500},
			{	1080595,	 	5,		100,	6000 }},	
			{{ 	1080596,	 	100,	100,	30000},	//18 방어 확률 증가%
			{	1080596,	 	20,		100,	1500},
			{	1080596,	 	50,		100,	6000}},	
			{{ 	1080597,		1,		1,		300	},	//19 체력 회복
			{	1080597,	 	2,		1,		150 },
			{	1080597,	 	1,		1,		300 }},	
			{{ 	1080598,		1,		1,		300	},	//20 기력 회복
			{	1080598,	 	2,		1,		150 },
			{	1080598,	 	1,		1,		300 }},	
			{{ 	1080599,		1,		1,		300	},	//21 마나 회복
			{	1080599,	 	2,		1,		150 },
			{	1080599,	 	1,		1,		300 }},	
			{{ 	1080600,	 	0,		200,	250 },	//22 물리 피해 증가%
			{	1080600,	 	0,		60,		70 },
			{	1080600,	 	0,		100,	1500 }},	
			{{ 	1080601,	 	0,		200,	2500},	//23 화염 피해 증가%
			{	1080601,	 	0,		60,		70 },
			{	1080601,	 	0,		100,	1500 }},	
			{{ 	1080602,	 	0,		200,	2500},	//24 냉기 피해 증가%
			{	1080602,	 	0,		60,		70},
			{	1080602,	 	0,		100,	150 }},	
			{{ 	1080603,	 	0,		200,	250},	//25 독 피해 증가%
			{	1080603,	 	0,		60,		70 },
			{	1080603,	 	0,		100,	150 }},	
			{{ 	1080604,	 	0,		200,	250},	//26 에너지 피해 증가%
			{	1080604,	 	0,		60,		70 },
			{	1080604,	 	0,		100,	150 }},	
			{{ 	1080605,	 	0,		200,	250},	//27 광역 물리 피해 증가%
			{	1080605,	 	0,		60,		70 },
			{	1080605,	 	0,		100,	150 }},	
			{{ 	1080606,	 	0,		200,	2500},	//28 광역 화염 피해 증가%
			{	1080606,	 	0,		60,		70 },
			{	1080606,	 	0,		100,	150 }},	
			{{ 	1080607,	 	0,		200,	250},	//29 광역 냉기 피해 증가%
			{	1080607,	 	0,		60,		70 },
			{	1080607,	 	0,		100,	150 }},	
			{{ 	1080608,	 	0,		200,	250},	//30 광역 독 피해 증가%
			{	1080608,	 	0,		60,		70 },
			{	1080608,	 	0,		100,	150 }},	
			{{ 	1080609,	 	0,		200,	250},	//31 광역 에너지 피해 증가%
			{	1080609,	 	0,		60,		70 },
			{	1080609,	 	0,		100,	150 }},	
			{{ 	1080610,		1,		100,	30000},	//32 물리 피해 증가
			{	1080610,	 	10,		1,		3000},
			{	1080610,	 	5,		1,		6000}},	
			{{ 	1080611,		1,		100,	30000},	//33 화염 피해 증가
			{	1080611,	 	10,		1,		3000},
			{	1080611,	 	5,		1,		6000}},	
			{{ 	1080612,		1,		100,	30000},	//34 냉기 피해 증가
			{	1080612,	 	10,		1,		3000},
			{	1080612,	 	5,		1,		6000}},	
			{{ 	1080613,		1,		100,	30000},	//35 독 피해 증가
			{	1080613,	 	10,		1,		3000},
			{	1080613,	 	5,		1,		6000}},	
			{{ 	1080614,		1,		100,	30000},	//36 에너지 피해 증가
			{	1080614,	 	10,		1,		3000},
			{	1080614,	 	5,		1,		6000}},	
			{{ 	1080615,		1,		1,		300	},	//37 체력 흡수%
			{	1080615,	 	1,		1,		300	},
			{	1080615,	 	2,		10,		1500}},	
			{{ 	1080616,		1,		1,		300	},	//38 기력 흡수%
			{	1080616,	 	1,		1,		300	},
			{	1080616,	 	2,		10,		1500}},
			{{ 	1080617,		1,		1,		300	},	//39 마나 흡수%
			{	1080617,	 	1,		1,		300	},
			{	1080617,	 	2,		10,		1500}},
			{{ 	1080618,	 	3,		100,	10000},	//40 공격 속도 증가%
			{	1080618,	 	6,		10,		500},
			{	1080618,	 	15,		10,		2000 }},	
			{{ 	1080619,	 	3,		100,	10000},	//41 시전 속도 증가%
			{	1080619,	 	6,		10,		500},
			{	1080619,	 	15,		10,		2000 }},
			{{ 	1080620,		3,		10,		1000},	//42 물리 치명타 확률 증가%
			{	1080620,	 	6,		10,		500	},
			{	1080620,	 	5,		10,		600	}},	
			{{ 	1080621,		3,		10,		1000},	//43 마법 치명타 확률 증가%
			{	1080621,	 	6,		10,		500	},
			{	1080621,	 	5,		10,		600	}},	
			{{ 	1080622,		2,		100,	15000},	//44 물리 치명타 피해 증가%
			{	1080622,	 	2,		10,		1500},
			{	1080622,	 	1,		10,		3000}},	
			{{ 	1080623,		2,		100,	15000},	//45 마법 치명타 피해 증가%
			{	1080623,	 	2,		10,		1500},
			{	1080623,	 	1,		10,		3000}},	
			{{ 	1080624,		1,		50,		15000},	//46 치유량 증가%
			{	1080624,	 	4,		10,		750},
			{	1080624,	 	2,		10,		1500}},	
			{{ 	1080625,		1,		100,	30000},	//47 치유량 증가
			{	1080625,	 	5,		100,	6000},
			{	1080625,	 	2,		100,	15000}},	
			{{ 	1080626,		0,		3,		4	},	//48 관통 피해 증가
			{	1080626,	 	0,		3,		4	},
			{	1080626,	 	0,		2,		3	}},	
			{{ 	1080627,		0,		3,		4	},	//49 충격 피해 증가
			{	1080627,	 	0,		3,		4	},
			{	1080627,	 	0,		2,		3	}},	
			{{ 	1080628,		0,		3,		4	},	//50 출혈 피해 증가
			{	1080628,	 	0,		3,		4	},
			{	1080628,	 	0,		2,		3	}},	
			{{ 	1080629,		5,		100,	6000},	//51 금화 획득 증가
			{	1080629,	 	10,		100,	3000},
			{	1080629,	 	5,		100,	6000}},	
			{{ 	1080630,		3,		100,	10000},	//52 마법 화살 공격%
			{	1080630,	 	0,		60,		70 },
			{	1080630,	 	0,		100,	150}},	
			{{ 	1080631,		3,		100,	10000},	//53 체력 손상 공격%
			{	1080631,	 	0,		60,		70 },
			{	1080631,	 	0,		100,	150 }},	
			{{ 	1080632,		3,		100,	10000},	//54 화염구 공격%
			{	1080632,	 	0,		60,		700 },
			{	1080632,	 	0,		100,	1500 }},	
			{{ 	1080633,		3,		100,	10000},	//55 번개 공격%
			{	1080633,	 	0,		60,		70 },
			{	1080633,	 	0,		100,	150 }},	
			{{ 	1080634,		6,		100,	5000},	//56 영장류 피해 증가%
			{	1080634,	 	30,		100,	1000},
			{	1080634,	 	2,		10,		1500}},	
			{{ 	1080635,		6,		100,	5000},	//57 언데드 피해 증가%
			{	1080635,	 	30,		100,	1000},
			{	1080635,	 	2,		10,		1500}},
			{{ 	1080636,		6,		100,	5000},	//58 정령 피해 증가%
			{	1080636,	 	30,		100,	1000},
			{	1080636,	 	2,		10,		1500}},
			{{ 	1080637,		6,		100,	5000},	//59 곤충 피해 증가%
			{	1080637,	 	30,		100,	1000},
			{	1080637,	 	2,		10,		1500}},
			{{ 	1080638,		6,		100,	5000},	//60 파충류 피해 증가%
			{	1080638,	 	30,		100,	1000},
			{	1080638,	 	2,		10,		1500}},
			{{ 	1080639,		6,		100,	5000},	//61 악마 피해 증가%
			{	1080639,	 	30,		100,	1000},
			{	1080639,	 	2,		10,		1500}},
			{{ 	1080640,		6,		10,		5000},	//62 요정 피해 증가%
			{	1080640,	 	30,		10,		1000},
			{	1080640,	 	2,		1,		1500}},
			{{ 	1,	 			30,		100,	1000},	//63 해부학 스킬 증가%
			{	1,	 			6,		10,		500},
			{	1,	 			15,		100,	2000}},	
			{{ 	2,	 			30,		100,	1000},	//64 동물지식 스킬 증가%
			{	2,	 			6,		10,		500},
			{	2,	 			15,		100,	2000}},	
			{{ 	5,	 			30,		100,	1000},	//65 방패술 스킬 증가%
			{	5,	 			6,		10,		500},
			{	5,	 			15,		100,	2000}},	
			{{ 	9,	 			30,		100,	1000},	//66 평화연주 스킬 증가%
			{	9,	 			6,		10,		500},
			{	9,	 			15,		100,	2000}},	
			{{ 	14,	 			30,		100,	1000},	//67 은신감지 스킬 증가%
			{	14,	 			6,		10,		500},
			{	14,	 			15,		100,	2000}},	
			{{ 	15,	 			30,		100,	1000},	//68 불협화음 스킬 증가%
			{	15,	 			6,		10,		500},
			{	15,	 			15,		100,	2000}},	
			{{ 	16,	 			30,		100,	1000},	//69 지능평가 스킬 증가%
			{	16,	 			6,		10,		500},
			{	16,	 			15,		100,	2000}},	
			{{ 	17,	 			30,		100,	1000},	//70 회복술 스킬 증가%
			{	17,	 			6,		10,		500},
			{	17,	 			15,		100,	2000}},	
			{{ 	19,	 			30,		100,	1000},	//71 법의학 스킬 증가%
			{	19,	 			6,		10,		500},
			{	19,	 			15,		100,	2000}},	
			{{ 	20,	 			30,		100,	1000},	//72 목동술 스킬 증가%
			{	20,	 			6,		10,		500},
			{	20,	 			15,		100,	2000}},	
			{{ 	21,	 			30,		100,	1000},	//73 은신 스킬 증가%
			{	21,	 			6,		10,		500},
			{	21,	 			15,		100,	2000}},	
			{{ 	22,	 			30,		100,	1000},	//74 도발연주 스킬 증가%
			{	22,	 			6,		10,		500},
			{	22,	 			15,		100,	2000}},	
			{{ 	25,	 			30,		100,	1000},	//75 마법학 스킬 증가%
			{	25,	 			6,		10,		500},
			{	25,	 			15,		100,	2000}},	
			{{ 	26,	 			30,		100,	1000},	//76 마법저항 스킬 증가%
			{	26,	 			6,		10,		500},
			{	26,	 			15,		100,	2000}},	
			{{ 	27,	 			30,		100,	1000},	//77 전술 스킬 증가%
			{	27,	 			6,		10,		500	},
			{	27,	 			15,		100,	2000}},	
			{{ 	28,	 			30,		100,	1000},	//78 훔쳐보기 스킬 증가%
			{	28,	 			6,		10,		500	},
			{	28,	 			15,		100,	2000}},	
			{{ 	29,	 			30,		100,	1000},	//79 음악연주 스킬 증가%
			{	29,	 			6,		10,		500	},
			{	29,	 			15,		100,	2000}},	
			{{ 	30,	 			30,		100,	1000},	//80 포이즈닝 스킬 증가%
			{	30,	 			6,		10,		500	},
			{	30,	 			15,		100,	2000}},	
			{{ 	31,	 			30,		100,	1000},	//81 궁술 스킬 증가%
			{	31,	 			6,		10,		500	},
			{	31,	 			15,		100,	2000}},	
			{{ 	32,	 			30,		100,	1000},	//82 영혼대화 스킬 증가%
			{	32,	 			6,		10,		500	},
			{	32,	 			15,		100,	2000}},	
			{{ 	33,	 			30,		100,	1000},	//83 훔치기 스킬 증가%
			{	33,	 			6,		10,		500	},
			{	33,	 			15,		100,	2000}},	
			{{ 	35,	 			30,		100,	1000},	//84 길들이기 스킬 증가%
			{	35,	 			6,		10,		500	},
			{	35,	 			15,		100,	2000}},	
			{{ 	38,	 			30,		100,	1000},	//85 추적하기 스킬 증가%
			{	38,	 			6,		10,		500	},
			{	38,	 			15,		100,	2000}},	
			{{ 	39,	 			30,		100,	1000},	//86 수의학 스킬 증가%
			{	39,	 			6,		10,		500	},
			{	39,	 			15,		100,	2000}},	
			{{ 	40,	 			30,		100,	1000},	//87 검술 스킬 증가%
			{	40,	 			6,		10,		500	},
			{	40,	 			15,		100,	2000}},	
			{{ 	41,	 			30,		100,	1000},	//88 둔기술 스킬 증가%
			{	41,	 			6,		10,		500	},
			{	41,	 			15,		100,	2000}},	
			{{ 	42,	 			30,		100,	1000},	//89 펜싱술 스킬 증가%
			{	42,	 			6,		10,		500	},
			{	42,	 			15,		100,	2000}},	
			{{ 	46,	 			30,		100,	1000},	//90 명상 스킬 증가%
			{	46,	 			6,		10,		500	},
			{	46,	 			15,		100,	2000}},	
			{{ 	47,	 			30,		100,	1000},	//91 은신이동 스킬 증가%
			{	47,	 			6,		10,		500	},
			{	47,	 			15,		100,	2000}},	
			{{ 	49,	 			30,		100,	1000},	//92 강령술 스킬 증가%
			{	49,	 			6,		10,		500},
			{	49,	 			15,		100,	2000}},	
			{{ 	50,	 			30,		100,	1000},	//93 집중 스킬 증가%
			{	50,	 			6,		10,		500	},
			{	50,	 			15,		100,	2000}},	
			{{ 	51,	 			30,		1000,	1000},	//94 기사도 스킬 증가%
			{	51,	 			6,		10,		500	},
			{	51,	 			15,		100,	2000}},	
			{{ 	52,	 			30,		100,	1000},	//95 무사도 스킬 증가%
			{	52,	 			6,		10,		500	},
			{	52,	 			15,		100,	2000}},	
			{{ 	53,	 			30,		100,	1000},	//96 암술 스킬 증가%
			{	53,	 			6,		10,		500},
			{	53,	 			15,		100,	2000}},	
			{{ 	54,	 			30,		100,	1000},	//97 주문 조합 스킬 증가%
			{	54,	 			6,		10,		500	},
			{	54,	 			15,		100,	2000}},	
			{{ 	55,	 			30,		100,	1000},	//98 신비술 스킬 증가%
			{	55,	 			6,		10,		500	},
			{	55,	 			15,		100,	2000}},	
			{{ 	57,	 			30,		100,	1000},	//99 던지기 스킬 증가%
			{	57,	 			6,		10,		500	},
			{	57,	 			15,		100,	2000}},
			{{ 	1080651,		3,		100,	10000},	//100 무기 공격 반사%
			{	1080651,		15,		100,	2000},
			{	1080651,		30,		500,	5000}},
			{{ 	1080652,		10,		10,		3000},	//101 전투 경험치%
			{	1080652,	 	1,		1,		300	},
			{	1080652,	 	30,		10,		1000}},
			{{ 	1080653,		0,		10,		3000},	//102 혼돈 피해%
			{	1080653,	 	0,		1,		300	},
			{	1080653,	 	0,		10,		1000}},
			{{ 	1080654,		0,		10,		3000},	//103 신성 경험치%
			{	1080654,	 	0,		1,		300	},
			{	1080654,	 	0,		10,		1000}},
			{{ 	1080655,		0,		10,		3000},	//104 무기 데미지 감소
			{	1080655,	 	0,		1,		300	},
			{	1080655,	 	0,		10,		1000}},
			{{ 	1080656,		0,		10,		3000},	//105 마법 데미지 감소
			{	1080656,	 	0,		1,		300	},
			{	1080656,	 	0,		10,		1000}},
			{{ 	1080657,		0,		10,		3000},	//106 기절 시간 감소
			{	1080657,	 	0,		1,		300	},
			{	1080657,	 	0,		10,		1000}},
			{{ 	1080658,		0,		10,		3000},	//107 혼돈 피해
			{	1080658,	 	0,		1,		300	},
			{	1080658,	 	0,		10,		1000}},
			{{ 	1080659,		0,		10,		3000},	//108 신성 피해
			{	1080659,	 	0,		1,		300	},
			{	1080659,	 	30,		10,		1000}}
		};
		
		public static int[,] NewSelectGemOption =
		{
			{ 	38,	38, 38, 38, 38, 38, 38, 38, 38, 39,	100, 8, 6, 	6, 	32,	32,	58,	17,	17,	21,	39,	46,	47 	},	//별무늬 사파이어
			{	5, 	1, 	1, 	20, 18, 5, 	1, 	1, 	1, 	5,	15, 35,	20,	15, 35, 1, 	1, 	1,	20,	5,	1,	15,	35	},  //에메랄드
			{	6, 	2, 	2, 	21, 46, 6, 	2, 	2, 	2, 	8,	14, 34, 21, 14, 35, 7, 	62,	2, 	21,	6,	2,	14,	34	},  //사파이어
			{	4, 	0, 	37, 19, 12, 4, 	1, 	1, 	1, 	4,	13, 33, 19, 13, 33, 0,	0, 	0, 	19,	4,	0,	13,	33	},  //루비
			{	17, 7, 	7, 	32, 33, 35, 34, 7, 	7, 	36,	16, 36, 5, 	16,	36,	5,	56,	17,	17,	19,	37,	16,	36	},  //황수정
			{	3, 	3, 	3, 	3, 	3, 	3, 	3, 	3, 	3, 	3,	3,	3,	3,	3,	3,	3,	61,	3,	3,	3,	3,	3,	3	},  //자수정
			{	44, 44, 44, 44, 44, 44, 44, 44, 44, 46,	46,	47,	4,	4,	4,	4,	60,	7,	46,	20,	38,	12,	32	},  //전기석
			{	51, 51, 51, 51, 51, 51, 51, 51, 51, 51,	51,	51,	51,	51,	51,	51,	59,	51,	51,	51,	51,	51,	51	},  //호박
			{	12, 12, 12, 12, 12, 12, 12, 12, 12, 46,	12,	18,	12,	12,	12,	12,	57,	12,	18,	100,100, 100, 100	}  //다이아몬드
		};
		
		public static int NewEquipNumber(Item equip)
		{
			int check = -1;
			if( equip is IEquipOption )
			{
				IEquipOption item = equip as IEquipOption;
				if( item is BaseWeapon )
				{
					BaseWeapon newmake = item as BaseWeapon;
					check = WeaponList(newmake);
				}
				
				else if( item is BaseArmor )
				{
					BaseArmor newmake = item as BaseArmor;
					check = ArmorList(newmake);
				}
				else if( item is BaseClothing )
				{
					BaseClothing newmake = item as BaseClothing;
					if( !(newmake.Layer == Layer.Neck || newmake.Layer == Layer.Gloves || newmake.Layer == Layer.Arms || newmake.Layer == Layer.Helm || newmake.Layer == Layer.Pants || newmake.Layer == Layer.InnerTorso ) )
					{
						check = -1;
					}
					else
						check = 11;
				}
				else if( item is BaseJewel )
				{
					BaseJewel newmake = item as BaseJewel;
					check = JewelList(newmake, true);					

				}
				else if( item is Spellbook )
				{
					check = 9;
				}
			}

			return check;
			
		}
		
		public static int NewItemLine(int check)
		{
			int itemline = 0;
			if( check >= 10 && check <= 18 )
				itemline = 1;
			else if( check >= 19 )
				itemline = 2;
			return itemline;
		}
		
		//재련 코드
		public static void NewUseGem(Item equip, int gem)
		{
			//접두 31 ~ 40 : 1 ~ 10 재련 리스트
			//접미 31 ~ 40 : 1 ~ 10 재련 저장값
			//접미 2 : 재련 최대값

			if( equip is IEquipOption )
			{
				IEquipOption item = equip as IEquipOption;
				int check = NewEquipNumber(equip);
				int skilluse = 0;
				if( gem == -1 )
				{
					int count = 0;
					for(int i = 0; i < 10; ++i)
					{
						if(item.PrefixOption[31 + i] != -1 )
						{
							count++;
							skilluse = NewEquipOptionList( equip, item.PrefixOption[31 + i], item.SuffixOption[31 + i] * -1, 0 );
							item.PrefixOption[31 + i] = -1;
							item.SuffixOption[31 + i] = -1;
						}
						else
							break;
					}
				}
				else if( item.SuffixOption[2] > 0 )
				{
					int itemline = NewItemLine(check);
					item.SuffixOption[2]--;
					for( int i = 0; i < 10; ++i )
					{
						if( item.PrefixOption[31 + i] == -1 )
						{
							item.PrefixOption[31 + i] = NewSelectGemOption[gem, check];
							if( itemline == 1 && item.PrefixOption[31 + i] >= 56 && item.PrefixOption[31 + i] <= 62 )
								item.SuffixOption[31 + i] = 200;
							else
								item.SuffixOption[31 + i] = NewEquipOption[item.PrefixOption[i + 31],	itemline, 	3] / 6;
							skilluse = NewEquipOptionList( equip, item.PrefixOption[ i + 31], item.SuffixOption[ i + 31], skilluse);
							break;
						}
					}
				}
			}
		}
		
		//재료 보너스
        public static readonly int[,,] NewResourceOption = new int[,,]
		{
			{{ 	1081002,	5,	25000,	20, 50,		-1,	-1,	-1,	-1	},	//철 무기
			{	1081023,	12,	500,	5, 	7500, 	-1, -1, -1, -1},  //방어구
			{	1081044,	 		5,	20000,				-1, -1,	-1,	-1,	-1,	-1}},	//악세사리

			{{ 	1081003,	7,	2000,	40, 2000,	-1,	-1,	-1,	-1	},	//구리 무기
			{	1081024,	 		15,	500,	4, 	7500, 	-1, -1, -1, -1},  //방어구
			{	1081045,	 		4,	20000,				-1, -1,	-1,	-1,	-1,	-1}},	//악세사리

			{{ 	1081004,	4,	50000,				-1, -1,	-1,	-1,	-1,	-1},	//청동 무기
			{	1081025,	 		12,	300, 13, 100, 16, 400, 40, 100},  //방어구
			{	1081046,	 		7,	1000,				-1, -1,	-1,	-1,	-1,	-1	}},	//악세사리

			{{ 	1081005,	3,	50000,				-1, -1,	-1,	-1,	-1,	-1	},	// 금 무기
			{	1081026,	 		3,	10000,				-1, -1, -1, -1, -1, -1},  //방어구
			{	1081047,	 		51,	2000,				-1, -1,	-1,	-1,	-1,	-1	}},	//악세사리

			{{ 	1081006,	7,	500,				-1, -1,	-1,	-1,	-1,	-1	},	//무기
			{	1081027,	 		13,	200,				-1, -1, -1, -1, -1, -1},  //방어구
			{	1081048,	 		7,	100,				-1, -1,	-1,	-1,	-1,	-1}},	//악세사리

			{{ 	1081007,	15,	1000,				-1, -1,	-1,	-1,	-1,	-1	},	//무기
			{	1081028,	 		7,	300,					-1, -1, -1, -1, -1, -1},  //방어구
			{	1081049,	 		40,	800,				-1, -1,	-1,	-1,	-1,	-1	}},	//악세사리

			{{ 	1081008,	38,	100,				-1, -1,	-1,	-1,	-1,	-1	},	//무기
			{	1081029,	 		12,	300,	14, 400, 	16, 100, 40, 100},  //방어구
			{	1081050,	 		53,	1000,				-1, -1,	-1,	-1,	-1,	-1}},	//악세사리 잉갓 끝


			//가죽
			{{ 	1081009,	6,		25000,	21, 50,				-1,	-1,	-1,	-1	},	//무기
			{	1081030,	 		18,		500,				-1, -1, -1, -1, -1, -1	},  //방어구
			{	1081051,	 		6, 		20000,				-1,	-1,	-1, -1, -1, -1	}},	//악세사리
			
			{{ 	1081010,						-1, -1, -1,	-1,	-1,	-1, -1, -1	},	//무기
			{	1081031,	 		47,	10,	6, 500, 					-1, -1, -1, -1	},  //방어구
			{	1081052,	 		47, 2000, 			-1,	-1,	-1,	-1, -1, -1	}},	//악세사리
			
			{{ 	1081011,	8,		2000,		41, 2000,		-1,	-1,	-1,	-1	},	//무기
			{	1081032,	 		41,		100,		8, 	100, 		-1, -1, -1, -1	},  //방어구
			{	1081053,	 		8, 		1000, 					-1,	-1,	-1,	-1, -1, -1	}},	//악세사리
			
			{{ 	1081012,	46,		1000,		4, 25000,	6,	50000,	-1,	-1	},	//무기
			{	1081033,	 							-1,	-1,	-1, -1, -1, -1, -1, -1	},  //방어구
			{	1081054,	 		3, 		20000, 		-1,	-1,	-1,	-1, -1, -1	}},	//악세사리
			
			{{ 	1081013,	3,		50000,		8, 500,	-1,	-1,	-1,	-1	},	//무기
			{	1081034,	 		12,		200,	3, 100, 			-1, -1, -1, -1	},  //방어구
			{	1081055,	 		8, 		100, 		-1,	-1,	-1,	-1, -1, -1	}},	//악세사리
			
			{{ 	1081014,	13,		1000,				-1, -1,	-1,	-1,	-1,	-1	},	//무기
			{	1081035,	 		42,	200,					-1, -1, -1, -1, -1, -1	},  //방어구
			{	1081056,	 		41, 800, 					-1,	-1,	-1,	-1, -1, -1	}},	//악세사리
			
			{{ 	1081015,	39,	100,					-1, -1,	-1,	-1,	-1,	-1	},	//무기
			{	1081036,	 		45,	500,					-1, -1, -1, -1, -1, -1	},  //방어구
			{	1081057,	 		45, 1000, 					-1,	-1,	-1,	-1, -1, -1	}},	//악세사리 가죽 끝
			
			
			//나무
			{{ 	1081016,	4,	25000,	19, 50,					-1,	-1,	-1,	-1	},	//무기
			{	1081037,	 		4,	10000,					-1, -1, -1, -1, -1, -1	},  //방어구
			{	0,	 							-1, -1, -1,	-1,	-1,	-1, -1, -1	}},	//악세사리
			
			{{ 	1081017,	3,	50000,					-1, -1,	-1,	-1,	-1,	-1	},	//무기
			{	1081038,	 		3,	10000,					-1, -1, -1, -1, -1, -1	},  //방어구
			{	0,	 							-1, -1, -1,	-1,	-1,	-1, -1, -1	}},	//악세사리
			
			{{ 	1081018,		40,	2000,	44, 2000,			-1,	-1,	-1,	-1	},	//무기
			{	1081039,	 			14,	300,	15, 100, 16, 600, 100, 2500	},  //방어구
			{	0,	 							-1, -1, -1,	-1,	-1,	-1, -1, -1	}},	//악세사리
			
			{{ 	1081019,	17,		5000,	7, 2500,			-1,	-1,	-1,	-1	},	//무기
			{	1081040,	 		12,		400,	46, 200, 			-1, -1, -1, -1	},  //방어구
			{	0,	 							-1, -1, -1,	-1,	-1,	-1, -1, -1	}},	//악세사리
			
			{{ 	1081020,	44,		500,				-1, -1,	-1,	-1,	-1,	-1	},	//무기
			{	1081041,	 		20,		5, 					-1, -1, -1, -1, -1, -1	},  //방어구
			{	0,	 							-1, -1, -1,	-1,	-1,	-1, -1, -1}},	//악세사리
			
			{{ 	1081021,	4,		10000,				-1, -1,	-1,	-1,	-1,	-1	},	//무기
			{	1081042,	 		19,		50,	4, 2000,	 			-1, -1, -1, -1	},  //방어구
			{	0,	 							-1, -1, -1,	-1,	-1,	-1, -1, -1	}},	//악세사리
			
			{{ 	1081022,	37,		100,				-1, -1,	-1,	-1,	-1,	-1	},	//무기
			{	1081043,	 		12,		200,	14, 700, 15, 100, 21, 25},  //방어구
			{	0,	 							-1, -1, -1,	-1,	-1,	-1, -1, -1	}}	//악세사리 나무 끝


		};
		#endregion

		#region 공통 제작 코드
        private static readonly int[,,] EquipOption = new int[,,]
		{
			//1080578부터 시작
			//	이름,			Min1, 	Max1, 	Min2, 	Max2, 	Min3, 	Max3, 	Min4, 	Max4, 	Min5, 	Max5, 	Min6,	Max6, 	Min7, 	Max7
			{{ 	1080578,	 	5,		15,		20,		30,		35,		50, 	55, 	70,		75,		100, 	125, 	135,	150,	200	},	//0 힘 증가
			{	1080578,	 	1,		5,		6,		10,		11,		16, 	17, 	24,		25,		33,		34,		44,		45,		66	},
			{	1080578,	 	5,		10,		11,		15,		16,		25, 	26, 	35,		36,		50,		51,		70,		71,		100	}},	
			{{ 	1080579,	 	5,		15,		20,		30,		35,		50, 	55, 	70,		75,		100, 	125, 	135,	150,	200	},	//1 민첩성 증가
			{	1080579,	 	1,		5,		6,		10,		11,		16, 	17, 	24,		25,		33,		34,		44,		45,		66	},
			{	1080579,	 	5,		10,		11,		15,		16,		25, 	26, 	35,		36,		50,		51,		70,		71,		100	}},	
			{{ 	1080580,	 	5,		15,		20,		30,		35,		50, 	55, 	70,		75,		100, 	125, 	135,	150,	200	},	//2 지능 증가
			{	1080580,	 	1,		5,		6,		10,		11,		16, 	17, 	24,		25,		33,		34,		44,		45,		66	},
			{	1080580,	 	5,		10,		11,		15,		16,		25, 	26, 	35,		36,		50,		51,		70,		71,		100	}},	
			{{ 	1080581,	 	5,		15,		20,		30,		35,		50, 	55, 	70,		75,		100, 	125, 	135,	150,	200	},	//3 운 증가
			{	1080581,	 	1,		5,		6,		10,		11,		16, 	17, 	24,		25,		33,		34,		44,		45,		66	},
			{	1080581,	 	5,		10,		11,		15,		16,		25, 	26, 	35,		36,		50,		51,		70,		71,		100	}},	
			{{ 	1080582,	 	50,		150,	200,	300,	350,	500, 	550, 	700,	750,	1000, 	1250, 	1350,	1500,	2000},	//4 체력 증가
			{	1080582,	 	10,		50,		60,		100,	110,	160, 	170, 	240,	250,	330,	340,	440,	450,	660	},
			{	1080582,	 	50,		100,	110,	150,	160,	250, 	260, 	350,	360,	500,	510,	700,	710,	1000}},	
			{{ 	1080583,	 	10,		30,		40,		60,		70,		100, 	110, 	140,	150,	200, 	220, 	270,	300,	400	},	//5 기력 증가
			{	1080583,	 	1,		10,		11,		20,		21,		32,		33, 	49,		50,		66,		67,		88,		89,		132	},
			{	1080583,	 	10,		20,		21,		31,		32,		51, 	52, 	71,		72,		100,	101,	140,	141,	200	}},	
			{{ 	1080584,	 	10,		30,		40,		60,		70,		100, 	110, 	140,	150,	200, 	220, 	270,	300,	400	},	//6 마나 증가
			{	1080584,	 	1,		10,		11,		20,		21,		32,		33, 	49,		50,		66,		67,		88,		89,		132	},
			{	1080584,	 	10,		20,		21,		31,		32,		51, 	52, 	71,		72,		100,	101,	140,	141,	200	}},	
			{{ 	1080585,	 	100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//7 피해 증가%
			{	1080585,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080585,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080586,	 	100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//8 주문 피해 증가%
			{	1080586,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080586,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080587,	 	100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//9 관통 피해 증가%
			{	1080587,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080587,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080588,	 	100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//10 충격 피해 증가%
			{	1080588,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080588,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080589,	 	100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//11 출혈 피해 증가%
			{	1080589,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080589,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080590,	 	1,		4,		5,		10,		11,		15, 	16, 	25,		26,		50,		51,		65,		66,		80	},	//12 물리 저항력%
			{	1080590,	 	1,		2,		3,		5,		6,		7, 		8, 		12,		13,		25,		26,		33,		34,		50	},
			{	1080590,	 	1,		2,		3,		5,		6,		7, 		8, 		12,		13,		25,		26,		33,		34,		50	}},	
			{{ 	1080591,	 	1,		4,		5,		10,		11,		15, 	16, 	25,		26,		50,		51,		65,		66,		80	},	//13 화염 저항력%
			{	1080591,	 	1,		2,		3,		5,		6,		7, 		8, 		12,		13,		25,		26,		33,		34,		50	},
			{	1080591,	 	1,		2,		3,		5,		6,		7, 		8, 		12,		13,		25,		26,		33,		34,		50	}},	
			{{ 	1080592,	 	1,		4,		5,		10,		11,		15, 	16, 	25,		26,		50,		51,		65,		66,		80	},	//14 냉기 저항력%
			{	1080592,	 	1,		2,		3,		5,		6,		7, 		8, 		12,		13,		25,		26,		33,		34,		50	},
			{	1080592,	 	1,		2,		3,		5,		6,		7, 		8, 		12,		13,		25,		26,		33,		34,		50	}},	
			{{ 	1080593,	 	1,		4,		5,		10,		11,		15, 	16, 	25,		26,		50,		51,		65,		66,		80	},	//15 독 저항력%
			{	1080593,	 	1,		2,		3,		5,		6,		7, 		8, 		12,		13,		25,		26,		33,		34,		50	},
			{	1080593,	 	1,		2,		3,		5,		6,		7, 		8, 		12,		13,		25,		26,		33,		34,		50	}},	
			{{ 	1080594,	 	1,		4,		5,		10,		11,		15, 	16, 	25,		26,		50,		51,		65,		66,		80	},	//16 에너지 저항력%
			{	1080594,	 	1,		2,		3,		5,		6,		7, 		8, 		12,		13,		25,		26,		33,		34,		50	},
			{	1080594,	 	1,		2,		3,		5,		6,		7, 		8, 		12,		13,		25,		26,		33,		34,		50	}},	
			{{ 	1080595,	 	100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//17 명중 확률 증가%
			{	1080595,	 	20,		40,		50,		70,		80,		110, 	120, 	150,	160,	200,	310,	350,	360,	550 },
			{	1080595,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080596,	 	100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//18 방어 확률 증가%
			{	1080596,	 	20,		40,		50,		70,		80,		110, 	120, 	150,	160,	200,	310,	350,	360,	550 },
			{	1080596,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080597,		1,		3,		4,		6,		7,		10, 	11, 	16,		17,		25,		30,		42,		45,		60	},	//19 체력 회복
			{	1080597,	 	1,		2,		3,		4,		5,		7,	 	8, 		10,		11,		15,		16,		22,		23,		30 },
			{	1080597,	 	1,		2,		3,		4,		5,		7,	 	8, 		10,		11,		15,		16,		24,		27,		40 }},	
			{{ 	1080598,		1,		3,		4,		6,		7,		10, 	11, 	16,		17,		25,		30,		42,		45,		60	},	//20 기력 회복
			{	1080598,	 	1,		2,		3,		4,		5,		7,	 	8, 		10,		11,		15,		16,		22,		23,		30 },
			{	1080598,	 	1,		2,		3,		4,		5,		7,	 	8, 		10,		11,		15,		16,		24,		27,		40 }},	
			{{ 	1080599,		1,		3,		4,		6,		7,		10, 	11, 	16,		17,		25,		30,		42,		45,		60	},	//21 마나 회복
			{	1080599,	 	1,		2,		3,		4,		5,		7,	 	8, 		10,		11,		15,		16,		22,		23,		30 },
			{	1080599,	 	1,		2,		3,		4,		5,		7,	 	8, 		10,		11,		15,		16,		24,		27,		40 }},	
			{{ 	1080600,	 	100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//22 물리 피해 증가%
			{	1080600,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080600,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080601,	 	100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//23 화염 피해 증가%
			{	1080601,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080601,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080602,	 	100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//24 냉기 피해 증가%
			{	1080602,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080602,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080603,	 	100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//25 독 피해 증가%
			{	1080603,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080603,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080604,	 	100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//26 에너지 피해 증가%
			{	1080604,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080604,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080605,	 	100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//27 광역 물리 피해 증가%
			{	1080605,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080605,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080606,	 	100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//28 광역 화염 피해 증가%
			{	1080606,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080606,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080607,	 	100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//29 광역 냉기 피해 증가%
			{	1080607,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080607,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080608,	 	100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//30 광역 독 피해 증가%
			{	1080608,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080608,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080609,	 	100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//31 광역 에너지 피해 증가%
			{	1080609,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080609,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080610,		1,		3,		4,		6,		7,		10, 	11, 	16,		17,		25,		30,		42,		45,		60	},	//32 물리 피해 증가
			{	1080610,	 	1,		2,		3,		4,		5,		7,	 	8, 		10,		11,		15,		16,		22,		23,		30 	},
			{	1080610,	 	1,		2,		3,		5,		6,		8,	 	9, 		13,		14,		20,		21,		30,		31,		45	}},	
			{{ 	1080611,		1,		3,		4,		6,		7,		10, 	11, 	16,		17,		25,		30,		42,		45,		60	},	//33 화염 피해 증가
			{	1080611,	 	1,		2,		3,		4,		5,		7,	 	8, 		10,		11,		15,		16,		22,		23,		30 	},
			{	1080611,	 	1,		2,		3,		5,		6,		8,	 	9, 		13,		14,		20,		21,		30,		31,		45	}},	
			{{ 	1080612,		1,		3,		4,		6,		7,		10, 	11, 	16,		17,		25,		30,		42,		45,		60	},	//34 냉기 피해 증가
			{	1080612,	 	1,		2,		3,		4,		5,		7,	 	8, 		10,		11,		15,		16,		22,		23,		30 	},
			{	1080612,	 	1,		2,		3,		5,		6,		8,	 	9, 		13,		14,		20,		21,		30,		31,		45	}},	
			{{ 	1080613,		1,		3,		4,		6,		7,		10, 	11, 	16,		17,		25,		30,		42,		45,		60	},	//35 독 피해 증가
			{	1080613,	 	1,		2,		3,		4,		5,		7,	 	8, 		10,		11,		15,		16,		22,		23,		30 	},
			{	1080613,	 	1,		2,		3,		5,		6,		8,	 	9, 		13,		14,		20,		21,		30,		31,		45	}},	
			{{ 	1080614,		1,		3,		4,		6,		7,		10, 	11, 	16,		17,		25,		30,		42,		45,		60	},	//36 에너지 피해 증가
			{	1080614,	 	1,		2,		3,		4,		5,		7,	 	8, 		10,		11,		15,		16,		22,		23,		30 	},
			{	1080614,	 	1,		2,		3,		5,		6,		8,	 	9, 		13,		14,		20,		21,		30,		31,		45	}},	
			{{ 	1080615,		3,		5,		7,		10,		15,		19, 	22, 	28,		35,		50,		55,		60,		65,		75	},	//37 체력 흡수%
			{	1080615,	 	1,		1,		2,		2,		3,		3, 		4, 		4,		5,		5,		6,		6,		7,		7	},
			{	1080615,	 	1,		3,		4,		7,		8,		13, 	14, 	20,		21,		30,		31,		40,		41,		55	}},	
			{{ 	1080616,		3,		5,		7,		10,		15,		19, 	22, 	28,		35,		50,		55,		60,		65,		75	},	//38 기력 흡수%
			{	1080616,	 	1,		1,		2,		2,		3,		3, 		4, 		4,		5,		5,		6,		6,		7,		7	},
			{	1080616,	 	1,		3,		4,		7,		8,		13, 	14, 	20,		21,		30,		31,		40,		41,		55	}},	
			{{ 	1080617,		3,		5,		7,		10,		15,		19, 	22, 	28,		35,		50,		55,		60,		65,		75	},	//39 마나 흡수%
			{	1080617,	 	1,		1,		2,		2,		3,		3, 		4, 		4,		5,		5,		6,		6,		7,		7	},
			{	1080617,	 	1,		3,		4,		7,		8,		13, 	14, 	20,		21,		30,		31,		40,		41,		55	}},	
			{{ 	1080618,	 	100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//40 공격 속도 증가%
			{	1080618,	 	20,		40,		50,		70,		80,		110, 	120, 	150,	160,	200,	310,	350,	360,	550 },
			{	1080618,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080619,	 	100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//41 시전 속도 증가%
			{	1080619,	 	20,		40,		50,		70,		80,		110, 	120, 	150,	160,	200,	310,	350,	360,	550 },
			{	1080619,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080620,		10,		30,		40,		60,		70,		100, 	110, 	160,	170,	250,	300,	420,	450,	600	},	//42 물리 치명타 확률 증가%
			{	1080620,	 	1,		7,		8,		12,		13,		20, 	21, 	33,		34,		50,		51,		65,		66,		99	},
			{	1080620,	 	5,		15,		16,		25,		26,		40, 	41, 	60,		61,		100,	101,	120,	121,	150	}},	
			{{ 	1080621,		10,		30,		40,		60,		70,		100, 	110, 	160,	170,	250,	300,	420,	450,	600	},	//43 마법 치명타 확률 증가%
			{	1080621,	 	1,		7,		8,		12,		13,		20, 	21, 	33,		34,		50,		51,		65,		66,		99	},
			{	1080621,	 	5,		15,		16,		25,		26,		40, 	41, 	60,		61,		100,	101,	120,	121,	150	}},	
			{{ 	1080622,		100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//44 물리 치명타 피해 증가%
			{	1080622,	 	20,		40,		50,		70,		80,		110, 	120, 	150,	160,	200,	310,	350,	360,	550	},
			{	1080622,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000}},	
			{{ 	1080623,		100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//45 마법 치명타 피해 증가%
			{	1080623,	 	20,		40,		50,		70,		80,		110, 	120, 	150,	160,	200,	310,	350,	360,	550	},
			{	1080623,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000}},	
			{{ 	1080624,		100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//46 치유량 증가%
			{	1080624,	 	20,		40,		50,		70,		80,		110, 	120, 	150,	160,	200,	310,	350,	360,	550	},
			{	1080624,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000}},	
			{{ 	1080625,		1,		3,		4,		6,		7,		10, 	11, 	16,		17,		25,		30,		42,		45,		60	},	//47 치유량 증가
			{	1080625,	 	1,		3,		4,		6,		7,		10, 	11, 	16,		17,		25,		30,		42,		45,		60	},
			{	1080625,	 	1,		2,		3,		5,		6,		8,	 	9, 		13,		14,		20,		21,		30,		31,		45	}},	
			{{ 	1080626,		1,		3,		4,		6,		7,		10, 	11, 	16,		17,		25,		30,		42,		45,		60	},	//48 관통 피해 증가
			{	1080626,	 	1,		3,		4,		6,		7,		10, 	11, 	16,		17,		25,		30,		42,		45,		60	},
			{	1080626,	 	1,		2,		3,		5,		6,		8,	 	9, 		13,		14,		20,		21,		30,		31,		45	}},	
			{{ 	1080627,		1,		3,		4,		6,		7,		10, 	11, 	16,		17,		25,		30,		42,		45,		60	},	//49 충격 피해 증가
			{	1080627,	 	1,		3,		4,		6,		7,		10, 	11, 	16,		17,		25,		30,		42,		45,		60	},
			{	1080627,	 	1,		2,		3,		5,		6,		8,	 	9, 		13,		14,		20,		21,		30,		31,		45	}},	
			{{ 	1080628,		1,		3,		4,		6,		7,		10, 	11, 	16,		17,		25,		30,		42,		45,		60	},	//50 출혈 피해 증가
			{	1080628,	 	1,		3,		4,		6,		7,		10, 	11, 	16,		17,		25,		30,		42,		45,		60	},
			{	1080628,	 	1,		2,		3,		5,		6,		8,	 	9, 		13,		14,		20,		21,		30,		31,		45	}},	
			{{ 	1080629,		10,		30,		40,		60,		70,		100, 	110, 	160,	170,	250,	300,	420,	450,	600	},	//51 금화 획득 증가
			{	1080629,	 	1,		5,		6,		9,		10,		15, 	16, 	23,		24,		40,		41,		51,		52,		75	},
			{	1080629,	 	5,		15,		16,		25,		26,		40, 	41, 	60,		61,		100,	101,	120,	121,	150	}},	
			{{ 	1080630,		100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//52 마법 화살 공격%
			{	1080630,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080630,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080631,		100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//53 체력 손상 공격%
			{	1080631,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080631,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080632,		100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//54 화염구 공격%
			{	1080632,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080632,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080633,		100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//55 번개 공격%
			{	1080633,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080633,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080634,		100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//56 영장류 피해 증가%
			{	1080634,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080634,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080635,		100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//57 언데드 피해 증가%
			{	1080635,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080635,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080636,		100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//58 정령 피해 증가%
			{	1080636,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080636,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080637,		100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//59 곤충 피해 증가%
			{	1080637,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080637,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080638,		100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//60 파충류 피해 증가%
			{	1080638,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080638,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080639,		100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//61 악마 피해 증가%
			{	1080639,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080639,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1080640,		100,	200,	250,	350,	400,	550, 	600, 	750,	800,	1000,	1250,	1400,	1450,	2000},	//62 요정 피해 증가%
			{	1080640,	 	30,		60,		70,		110,	120,	180, 	190, 	250,	260,	350,	360,	480,	490,	750 },
			{	1080640,	 	50,		100,	150,	200,	250,	300, 	350, 	400,	450,	500,	550,	750,	800,	1000 }},	
			{{ 	1,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//63 해부학 스킬 증가%
			{	1,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	1,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	2,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//64 동물지식 스킬 증가%
			{	2,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	2,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	5,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//65 방패술 스킬 증가%
			{	5,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	5,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	9,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//66 평화연주 스킬 증가%
			{	9,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	9,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	14,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//67 은신감지 스킬 증가%
			{	14,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	14,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	15,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//68 불협화음 스킬 증가%
			{	15,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	15,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	16,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//69 지능평가 스킬 증가%
			{	16,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	16,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	17,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//70 회복술 스킬 증가%
			{	17,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	17,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	19,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//71 법의학 스킬 증가%
			{	19,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	19,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	20,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//72 목동술 스킬 증가%
			{	20,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	20,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	21,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//73 은신 스킬 증가%
			{	21,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	21,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	22,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//74 도발연주 스킬 증가%
			{	22,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	22,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	25,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//75 마법학 스킬 증가%
			{	25,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	25,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	26,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//76 마법저항 스킬 증가%
			{	26,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	26,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	27,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//77 전술 스킬 증가%
			{	27,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	27,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	28,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//78 훔쳐보기 스킬 증가%
			{	28,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	28,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	29,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//79 음악연주 스킬 증가%
			{	29,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	29,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	30,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//80 포이즈닝 스킬 증가%
			{	30,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	30,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	31,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//81 궁술 스킬 증가%
			{	31,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	31,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	32,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//82 영혼대화 스킬 증가%
			{	32,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	32,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	33,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//83 훔치기 스킬 증가%
			{	33,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	33,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	35,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//84 길들이기 스킬 증가%
			{	35,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	35,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	38,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//85 추적하기 스킬 증가%
			{	38,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	38,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	39,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//86 수의학 스킬 증가%
			{	39,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	39,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	40,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//87 검술 스킬 증가%
			{	40,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	40,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	41,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//88 둔기술 스킬 증가%
			{	41,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	41,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	42,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//89 펜싱술 스킬 증가%
			{	42,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	42,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	46,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//90 명상 스킬 증가%
			{	46,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	46,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	47,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//91 은신이동 스킬 증가%
			{	47,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	47,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	49,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//92 강령술 스킬 증가%
			{	49,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	49,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	50,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//93 집중 스킬 증가%
			{	50,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	50,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	51,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//94 기사도 스킬 증가%
			{	51,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	51,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	52,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//95 무사도 스킬 증가%
			{	52,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	52,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	53,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//96 암술 스킬 증가%
			{	53,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	53,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	54,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//97 주문 조합 스킬 증가%
			{	54,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	54,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	55,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//98 신비술 스킬 증가%
			{	55,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	55,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}},	
			{{ 	57,	 			10,		20,		30,		40,		50,		70, 	80, 	100,	110,	150,	160,	250,	300,	500	},	//99 던지기 스킬 증가%
			{	57,	 			1,		5,		6,		9,		10,		15, 	16, 	20,		21,		30,		31,		38,		39,		50	},
			{	57,	 			25,		45,		50,		70,		75,		95, 	100, 	130,	131,	200,	201,	375,	376,	625	}}
		};

		private static readonly int[][] EquipOptionType = new int[][]
		{
			new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 32, 33, 34, 35, 36, 40, 41, 42, 43, 44, 45, 46, 47, 51, 53, 56, 57, 58, 59, 60, 61, 62, 63, 65, 69, 70, 71, 73, 75, 76, 77, 82, 87, 90, 92, 93, 94, 95, 96 },
			new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 32, 33, 34, 35, 36, 40, 41, 42, 43, 44, 45, 46, 47, 51, 53, 56, 57, 58, 59, 60, 61, 62, 63, 69, 70, 71, 75, 76, 77, 82, 87, 90, 92, 93, 94, 95, 96 },
			new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 12, 13, 14, 15, 16, 17, 18, 19, 20, 32, 33, 34, 35, 36, 40, 42, 44, 51, 53, 56, 57, 58, 59, 60, 61, 62, 63, 70, 73, 77, 87, 92, 93, 95, 96 },
			new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 32, 33, 34, 35, 36, 40, 41, 42, 43, 44, 45, 46, 47, 51, 53, 54, 56, 57, 58, 59, 60, 61, 62, 63, 65, 69, 70, 71, 75, 76, 77, 88, 90, 93, 94, 95, 96 },
			new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 32, 33, 34, 35, 36, 40, 41, 42, 43, 44, 45, 46, 47, 51, 53, 54, 56, 57, 58, 59, 60, 61, 62, 63, 66, 68, 69, 70, 71, 74, 75, 76, 77, 79, 88, 90, 93, 94, 95, 96, 97, 98 },
			new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 12, 13, 14, 15, 16, 17, 18, 19, 20, 32, 33, 34, 35, 36, 40, 42, 44, 51, 53, 56, 57, 58, 59, 60, 61, 62, 63, 65, 67, 70, 73, 77, 78, 80, 83, 85, 89, 91, 93, 95, 96 },
			new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 12, 13, 14, 15, 16, 17, 18, 19, 20, 32, 33, 34, 35, 36, 40, 42, 44, 51, 53, 55, 56, 57, 58, 59, 60, 61, 62, 63, 70, 77, 89, 93, 95, 96 },
			new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 12, 13, 14, 15, 16, 17, 18, 19, 20, 32, 33, 34, 35, 36, 40, 42, 44, 46, 47, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 72, 77, 81, 84, 86, 93, 95, 96 },
			new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 12, 13, 14, 15, 16, 17, 18, 19, 20, 32, 33, 34, 35, 36, 40, 42, 44, 46, 47, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 77, 81, 93, 95, 96, 99 }, //여기까지 무기(8)
			new int[] { 0, 1, 2, 3, 4, 5, 6, 8, 13, 14, 15, 16, 18, 21, 33, 34, 35, 36, 39, 41, 43, 45, 46, 47, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 64, 66, 68, 69, 74, 75, 76, 79, 84, 90 }, //여기는 마법책(9)
			new int[] { 0, 1, 2, 3, 4, 5, 6, 12, 13, 14, 15, 16, 18, 19, 20, 21, 46, 47, 51, 63, 64, 65, 66, 68, 69, 70, 71, 72, 74, 75, 76, 77, 79, 82, 84, 86, 87, 88, 89, 90, 92, 93, 94, 97 },
			new int[] { 0, 1, 2, 3, 4, 5, 6, 8, 14, 15, 16, 21, 33, 34, 35, 36, 41, 43, 45, 46, 47, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 89, 90, 91, 92, 93, 96, 97, 99 },
			new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 12, 14, 15, 16, 17, 20, 21, 32, 33, 34, 35, 36, 40, 41, 42, 43, 44, 45, 51, 52, 56, 57, 58, 59, 60, 61, 62, 63, 69, 73, 77, 78, 80, 81, 83, 85, 87, 89, 90, 91, 92, 93, 95, 96, 97, 99 },
			new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 12, 13, 14, 15, 16, 17, 18, 19, 20, 32, 40, 42, 44, 48, 49, 51, 53, 54, 56, 57, 58, 59, 60, 61, 62, 63, 65, 70, 71, 77, 81, 87, 88, 89, 93, 94, 95, 96, 99 },
			new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 17, 19, 20, 21, 32, 33, 34, 35, 36, 40, 41, 42, 43, 44, 45, 51, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 95, 96, 97, 99 },
			new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 12, 13, 14, 15, 16, 17, 18, 19, 20, 32, 40, 42, 44, 51, 52, 54, 56, 57, 58, 59, 60, 61, 62, 63, 65, 70, 77, 81, 87, 88, 89, 93, 94, 95, 99 },
			new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 12, 13, 14, 15, 16, 17, 18, 19, 20, 32, 40, 42, 44, 51, 55, 56, 57, 58, 59, 60, 61, 62, 63, 65, 70, 77, 87, 88, 89, 93, 94, 95 },
			new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 12, 13, 14, 15, 16, 18, 19, 21, 32, 40, 42, 44, 46, 47, 51, 53, 56, 57, 58, 59, 60, 61, 62, 63, 65, 70, 76, 77, 87, 88, 89, 90, 93, 94, 95 },
			new int[] { 0, 1, 2, 3, 4, 5, 6, 12, 13, 14, 15, 16, 18, 21, 34, 35, 36, 39, 41, 43, 45, 46, 47, 51, 53, 55, 66, 67, 68, 70, 72, 73, 74, 76, 78, 84, 86, 88, 90 }, //여기까지 방어구(18)
			new int[] { 4, 5, 12, 13, 14, 15, 16, 18, 19, 20, 40, 42, 44, 46, 51},
			new int[] { 0, 1, 3, 4, 5, 7, 17, 19, 20, 32, 37, 38, 40, 42, 44, 51 },
			new int[] { 3, 4, 5, 12, 13, 14, 15, 16, 17, 18, 19, 20, 46, 47, 51 },
			new int[] { 0, 1, 2, 3, 4, 5, 6, 12, 13, 14, 15, 16, 42, 44, 51  }, //여기까지 전투 악세(22)			
			new int[] { 3, 4, 6, 13, 14, 15, 16, 19, 21, 33, 41, 43, 45, 46, 51},
			new int[] { 2, 3, 6, 8, 21, 33, 34, 35, 36, 39, 41, 43, 45, 47, 51 },
			new int[] { 3, 4, 6, 13, 14, 15, 16, 18, 19, 21, 43, 45, 46, 47, 51 },
			new int[] { 0, 1, 2, 3, 4, 6, 34, 35, 36, 41, 43, 45, 46, 47, 51  }, //여기까지 마법 악세(26)

		};
		
		public static bool ItemOption_ToIntCheck(int PrefixOption)
		{
			if( PrefixOption == 1060485 || PrefixOption == 1060409 || PrefixOption == 1060432 || PrefixOption == 1060436 || PrefixOption == 1060431 || PrefixOption == 1060484 || PrefixOption == 1060439 || PrefixOption == 1060448 || PrefixOption == 1060447 || PrefixOption == 1060445 || PrefixOption == 1060449 || PrefixOption == 1060446 || PrefixOption == 1063492	|| PrefixOption == 1063493 || PrefixOption == 1063494 || PrefixOption == 1063495 || PrefixOption == 1063496	|| PrefixOption == 1063502 || PrefixOption == 1063503 || PrefixOption == 1063504 || PrefixOption == 1063521 )
				return true;
			if( ( PrefixOption >= 1080578 && PrefixOption <= 1080584 ) || ( PrefixOption >= 1080590 && PrefixOption <= 1080594 ) || ( PrefixOption >= 1080610 && PrefixOption <= 1080614 ) || ( PrefixOption >= 1080625 && PrefixOption <= 1080628 ) )
				return true;
			else
				return false;
		}
		
				
		public static int[,] SelectGemOption = new int[4, 9]
		{
			//별무늬 사파이어, 에메랄드, 사파이어, 루비, 황수정, 자수정, 전기석, 호박, 다이아몬드
			{ 2,  5,  6,  4,  0,  3,  1,  51, 18 }, //팔찌
			{ 39, 25, 24, 23, 37, 38, 26, 51, 22 }, //반지
			{ 21, 15, 14, 13, 19, 20, 16, 51, 12 }, //목걸이
			{ 62, 56, 57, 46, 58, 60, 59, 51, 61 } //귀걸이					
		};
		#endregion
		
		#region 신코드 전역 함수
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
		#endregion
		#region 신규 장비 리스트
		public static int WeaponList( BaseWeapon newmake )
		{
			int check = -1;
			if( newmake.Skill is SkillName.Swords )
			{
				if( newmake is BaseAxe )
					check = 2;
				else if( newmake.Layer == Layer.TwoHanded )
					check = 1;
				else if( newmake.Layer == Layer.OneHanded )
					check = 0;
			}
			else if( newmake.Skill is SkillName.Macing )
			{
				if( newmake.Layer == Layer.TwoHanded )
					check = 4;
				else if( newmake.Layer == Layer.OneHanded )
					check = 3;
			}
			else if( newmake.Skill is SkillName.Fencing )
			{
				if( newmake.Layer == Layer.TwoHanded )
					check = 6;
				else if( newmake.Layer == Layer.OneHanded )
					check = 5;
			}
			else if( newmake is BaseRanged )
			{
				if( ((BaseRanged)newmake).AmmoType == typeof(Bolt) )
					check = 8;
				else if( ((BaseRanged)newmake).AmmoType == typeof(Arrow) )
					check = 7;
			}
			return check;
		}
		
		public static int ArmorList( BaseArmor newmake )
		{
			/*
			아머타입 10 : 방패 //탱커
			아머타입 11 : 천옷 //법사, 힐러
			아머타입 12 : 가죽 //도적
			아머타입 13 : 스텃 //아쳐
			아머타입 14 : 뼈 //서포터
			아머타입 15 : 링, 투구, 스톤 //근접 딜러
			아머타입 16 : 체인, 스톤 //근접 딜러
			아머타입 17 : 플레이트 //탱커
			아머타입 18 : 나무 //서포터
			*/
			int check = 1 + (int)newmake.MaterialType;

			if (newmake is BaseShield)
				check = 0;
			else if (check >= 5 && check <= 7)
				check = 2;
			else if (newmake is Helmet || newmake is Bascinet || newmake is CloseHelm || newmake is NorseHelm)
				check = 5;
			else if (check == 8)
				check = 5;
			else if (check == 9 || check == 13)
				check = 6;
			else if (check == 10)
				check = 7;
			else if (check == 12)
				check = 8;
			check += 10;			
			
			return check;
		}
		
		
		public static int JewelList(BaseJewel newmake, bool gemcheck = false)
		{
			/*
			악세타입 19 : 팔찌 
			아머타입 20 : 반지 
			아머타입 21 : 목걸이 
			아머타입 22 : 귀걸이 
			*/
			int check = 19;

			if( newmake.Layer == Layer.Ring )
				check = 20;
			else if( newmake.Layer == Layer.Neck )
				check = 21;
			else if( newmake.Layer == Layer.Earrings )
				check = 22;
			
			if( !gemcheck )
			{
				if( newmake is SilverEarrings || newmake is SilverRing || newmake is SilverBracelet || newmake is SilverNecklace )
					check += 4;
			}
			
			return check;
			
		}
		
		#endregion

		public static int NewEquipOptionList( Item equip, int itemoption, int itemvalue, int skilluse )
		{
			if( equip is IEquipOption )
			{
				IEquipOption item = equip as IEquipOption;
				//옵션 지정 코드
				AosAttributes primary = item.Attributes;
				AosWeaponAttributes weapon = item.WeaponAttributes;
				SAAbsorptionAttributes absorp = item.AbsorptionAttributes;
				ExtendedWeaponAttributes exweapon = item.ExtendedWeaponAttributes;
				AosSkillBonuses skill = item.SkillBonuses;
				AosArmorAttributes armor = item.ArmorAttributes;
				switch( itemoption )
				{
					case 0: //힘 증가
					{
						primary.BonusStr += itemvalue;
						break;
					}
					case 1: //민첩 증가
					{
						primary.BonusDex += itemvalue;
						break;
					}
					case 2: //지능 증가
					{
						primary.BonusInt += itemvalue;
						break;
					}
					case 3: //운 증가
					{
						primary.Luck += itemvalue;
						break;
					}
					case 4: //체력 증가
					{
						primary.BonusHits += itemvalue;
						break;
					}
					case 5: //기력 증가
					{
						primary.BonusStam += itemvalue;
						break;
					}
					case 6: //마나 증가
					{
						primary.BonusMana += itemvalue;
						break;
					}
					case 7: //물리 피해 증가%
					{
						primary.WeaponDamage += itemvalue;
						break;
					}
					case 8: //주문 피해 증가%
					{
						primary.SpellDamage += itemvalue;
						break;
					}
					case 9: //관통 피해 증가%
					{
						absorp.ResonancePierce += itemvalue;
						break;
					}
					case 10: //충격 피해 증가%
					{
						absorp.ResonanceKinetic += itemvalue;
						break;
					}
					case 11: //출혈 피해 증가%
					{
						absorp.ResonanceBleed += itemvalue;
						break;
					}
					case 12: //물리 저항%
					{
						weapon.ResistPhysicalBonus += itemvalue;
						break;
					}
					case 13: //화염 저항%
					{
						weapon.ResistFireBonus += itemvalue;
						break;
					}
					case 14: //냉기 저항%
					{
						weapon.ResistColdBonus += itemvalue;
						break;
					}
					case 15: //독 저항%
					{
						weapon.ResistPoisonBonus += itemvalue;
						break;
					}
					case 16: //에너지 저항%
					{
						weapon.ResistEnergyBonus += itemvalue;
						break;
					}
					case 17: //명중률 증가%
					{
						primary.AttackChance += itemvalue;
						break;
					}
					case 18: //방어율 증가%
					{
						primary.DefendChance += itemvalue;
						break;
					}
					case 19: //체력 회복
					{
						primary.RegenHits += itemvalue;
						break;
					}
					case 20: //기력 회복
					{
						primary.RegenStam += itemvalue;
						break;
					}
					case 21: //마나 회복
					{
						primary.RegenMana += itemvalue;
						break;
					}
					case 22: //물리 피해 증가%
					{
						primary.BalancedWeapon += itemvalue;
						break;
					}
					case 23: //화염 피해 증가%
					{
						absorp.ResonanceFire += itemvalue;
						break;
					}
					case 24: //냉기 피해 증가%
					{
						absorp.ResonanceCold += itemvalue;
						break;
					}
					case 25: //독 피해 증가%
					{
						absorp.ResonancePoison += itemvalue;
						break;
					}
					case 26: //에너지 피해 증가%
					{
						absorp.ResonanceEnergy += itemvalue;
						break;
					}
					case 27: //광역 물리 피해 증가%
					{
						weapon.HitPhysicalArea += itemvalue;
						break;
					}
					case 28: //광역 화염 피해 증가%
					{
						weapon.HitFireArea += itemvalue;
						break;
					}
					case 29: //광역 냉기 피해 증가%
					{
						weapon.HitColdArea += itemvalue;
						break;
					}
					case 30: //광역 독 피해 증가%
					{
						weapon.HitPoisonArea += itemvalue;
						break;
					}
					case 31: //광역 에너지 피해 증가%
					{
						weapon.HitEnergyArea += itemvalue;
						break;
					}
					case 32: //물리 피해 증가
					{
						absorp.EaterDamage += itemvalue;
						break;
					}
					case 33: //화염 피해 증가
					{
						absorp.EaterFire += itemvalue;
						break;
					}
					case 34: //냉기 피해 증가
					{
						absorp.EaterCold += itemvalue;
						break;
					}
					case 35: //독 피해 증가
					{
						absorp.EaterPoison += itemvalue;
						break;
					}
					case 36: //에너지 피해 증가
					{
						absorp.EaterEnergy += itemvalue;
						break;
					}
					case 37: //체력 흡수
					{
						weapon.HitLeechHits += itemvalue;
						break;
					}
					case 38: //기력 흡수
					{
						weapon.HitLeechStam += itemvalue;
						break;
					}
					case 39: //마나 흡수
					{
						weapon.HitLeechMana += itemvalue;
						break;
					}
					case 40: //공격 속도 증가
					{
						primary.WeaponSpeed += itemvalue;
						break;
					}
					case 41: // 시전 속도 증가
					{
						primary.CastSpeed += itemvalue;
						break;
					}
					case 42: //물리 치명타 확률 증가
					{
						primary.WeaponCritical += itemvalue;
						break;
					}
					case 43: //마법 치명타 확률 증가
					{
						primary.CastRecovery += itemvalue;
						break;
					}
					case 44: //물리 치명타 피해 증가
					{
						primary.Brittle += itemvalue;
						break;
					}
					case 45: //마법 치명타 피해 증가
					{
						primary.SpellChanneling += itemvalue;
						break;
					}
					case 46: //치유량 증가%
					{
						primary.EnhancePotions += itemvalue;
						break;
					}
					case 47: //치유량 증가
					{
						primary.HealBonus += itemvalue;
						break;
					}
					case 48: //관통 피해 증가
					{
						absorp.EaterPierce += itemvalue;
						break;
					}
					case 49: //충격 피해 증가
					{
						absorp.EaterKinetic += itemvalue;
						break;
					}
					case 50: //출혈 피해 증가
					{
						absorp.EaterBleed += itemvalue;
						break;
					}
					case 51: //금화 획득 증가%
					{
						primary.NightSight += itemvalue;
						break;
					}
					case 52: //화염 화살 공격%
					{
						weapon.HitMagicArrow += itemvalue;
						break;
					}
					case 53: //체력 손상 공격%
					{
						weapon.HitHarm += itemvalue;
						break;
					}
					case 54: //화염구 공격%
					{
						weapon.HitFireball += itemvalue;
						break;
					}
					case 55: //번개 공격%
					{
						weapon.HitLightning += itemvalue;
						break;
					}
					case 56: //영장류 피해 증가%
					{
						absorp.HumanoidDamage += itemvalue;
						break;
					}
					case 57: //언데드 피해 증가%
					{
						absorp.UndeadDamage += itemvalue;
						break;
					}
					case 58: //정령 피해 증가%
					{
						absorp.ElementalDamage += itemvalue;
						break;
					}
					case 59: //곤충 피래 증가%
					{
						absorp.ArachnidDamage += itemvalue;
						break;
					}
					case 60: //파충류 피해 증가%
					{
						absorp.ReptilianDamage += itemvalue;
						break;
					}
					case 61: //악마 피해 증가%
					{
						absorp.AbyssDamage += itemvalue;
						break;
					}
					case 62: //요정 피해 증가%
					{
						absorp.FeyDamage += itemvalue;
						break;
					}
					case 63: //해부학 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Anatomy, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 64: //동물지식 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.AnimalLore, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 65: //방패술 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Parry, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 66: //평화연주 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Peacemaking, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 67: //은신감지 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.DetectHidden, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 68: //불협화음 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Discordance, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 69: //지능평가 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.EvalInt, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 70: //회복술 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Healing, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 71: //법의학 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Forensics, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 72: //목동술 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Herding, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 73: //은신 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Hiding, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 74: //도발연주 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Provocation, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 75: //마법학 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Magery, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 76: //마법저항 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.MagicResist, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 77: //전술 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Tactics, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 78: //훔쳐보기 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Snooping, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 79: //음악연주 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Musicianship, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 80: //포이즈닝 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Poisoning, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 81: //궁술 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Archery, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 82: //영혼대화 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.SpiritSpeak, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 83: //훔치기 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Stealing, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 84: //길들이기 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.AnimalTaming, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 85: //반사신경 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Tracking, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 86: //수의학 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Veterinary, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 87: //검술 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Swords, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 88: //둔기술 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Macing, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 89: //펜싱 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Fencing, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 90: //명상 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Meditation, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 91: //은신이동 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Stealth, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 92: //강령술 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Necromancy, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 93: //집중 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Focus, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 94: //기사도 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Chivalry, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 95: //무사도 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Bushido, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 96: //암술 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Ninjitsu, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 97: //주문조합 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Spellweaving, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 98: //신비술 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Mysticism, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 99: //던지기 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Throwing, (double)itemvalue * 0.01 );
						skilluse++;
						break;
					}
					case 100: //무기 공격 반사%
					{
						primary.ReflectPhysical += itemvalue;
						break;
					}
					case 101: //전투 경험치%
					{
						primary.LowerAmmoCost += itemvalue;
						break;
					}
					case 102: //혼돈 피해%
					{
						exweapon.ChaosDamage += itemvalue;
						break;
					}
					case 103: //신성 피해%
					{
						exweapon.DirectDamage += itemvalue;
						break;
					}
					case 104: //무기 데미지 감소
					{
						armor.WeaponDefense += itemvalue;
						break;
					}
					case 105: //마법 데미지 감소
					{
						armor.MagicDefense += itemvalue;
						break;
					}
					case 106: //기절 시간 감소
					{
						armor.StunDefense += itemvalue;
						break;
					}
					case 107: //혼돈 피해
					{
						exweapon.ChaosPlus += itemvalue;
						break;
					}
					case 108: //신성 피해
					{
						exweapon.DirectPlus += itemvalue;
						break;
					}
				}
			}
			return skilluse;
		}
		
		public static void NewEquipOptionCreate( Item equip, int rank, bool onlyone = false, bool artifact = false )
		{
			if( equip is IEquipOption )
			{
				IEquipOption item = equip as IEquipOption;
				int check = -1;
				int equipLine = 0;
				int skilluse = 0;
				
				item.SuffixOption[2] = NewUpgradeOptionStock[rank];

				if( !artifact && rank <= 0 )
					return;

				#region 장비 옵션 설정
				if( artifact )
				{
					item.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), item.SuffixOption[1] + 3);
				}
				else
				{
					check = NewEquipNumber(equip);
					equipLine = NewItemLine(check);
				}
				
				/*
				else if( item is BaseWeapon )
				{
					BaseWeapon newmake = item as BaseWeapon;
					check = WeaponList(newmake);
				}
				
				else if( item is BaseArmor )
				{
					BaseArmor newmake = item as BaseArmor;
					check = ArmorList(newmake);
					equipLine = 1;
				}
				else if( item is BaseClothing )
				{
					BaseClothing newmake = item as BaseClothing;
					if( !(newmake.Layer == Layer.Neck || newmake.Layer == Layer.Gloves || newmake.Layer == Layer.Arms || newmake.Layer == Layer.Helm || newmake.Layer == Layer.Pants || newmake.Layer == Layer.InnerTorso ) )
					{
						return;
					}

					check = 11;
					equipLine = 1;
				}
				else if( item is BaseJewel )
				{
					BaseJewel newmake = item as BaseJewel;
					check = JewelList(newmake);					
					equipLine = 2;
				}
				
				else if( item is Spellbook )
				{
					check = 9;
				}
				*/
				#endregion				

				if( artifact || check != -1 )
				{
				
					//신규 아이템 제작 코드
					/*
					아이템 접두, 접미 체크
					접두 : 아이템 기본 옵션(티어 등)
					접미 : 아이템 옵션 저장소
					
					접두 0 : 신규 아이템 체크 유무. 기본 값 100
					접미 0 : 옵션 갯수
					접두 1 : 아이템 세부 내구도(10000 => 내구도 1 하락)
					접미 1 : 유물 레벨 / 랭크 레벨
					접두 2 : 옵션 최대값
					접미 2 : 재련 값
					
					접두 3 ~ 10 : 강화 종류
					접미 3 ~ 10 : 강화 레벨
					
					접두 11 ~ 30 : 1 ~ 20 옵션 리스트. 랜덤은 11 ~ 12만 사용
					접미 11 ~ 30 : 1 ~ 20 옵션 저장값. 랜덤은 11 ~ 12만 사용
					
					접두 31 ~ 40 : 1 ~ 10 재련 리스트
					접미 31 ~ 40 : 1 ~ 10 재련 저장값
					
					접두 41 : 목록 string 값
					접미 41 : 최대 옵션 값
					접두 42 ~ 45 : 1 ~ 4 재료 리스트
					접미 42 ~ 45 : 1 ~ 4 재료 저장값
					*/
					item.PrefixOption[0] = 100;
					
					for( int i = 0; i < 10; ++i)
					{
						item.PrefixOption[i + 31] = -1;
						item.SuffixOption[i + 31] = -1;
					}
					
					if( !artifact )
					{
						item.SuffixOption[0] = onlyone ? 1 : 2;
						for( int i = 0; i < 20; ++i)
						{
							item.PrefixOption[11 + i] = -1;
						}
						item.PrefixOption[2] = NewRandomOptionStock[rank] / item.SuffixOption[0];
						//Console.WriteLine("item.PrefixOption[2] : {0}, NewRandomOptionStock[rank] : {1}", item.PrefixOption[2], NewRandomOptionStock[rank] );
					}
					
					//Console.WriteLine("item.SuffixOption[0] : {0}", item.SuffixOption[0]);

					
					//접두 2, 접두 11 ~ 30, 접미 11 ~ 30 구현 코드
					#region 옵션 설정 코드
					//접두 41 ~ 45, 접미 41 ~ 45 구현 코드
					//Console.WriteLine("(int)item.Resource : {0}", (int)item.Resource);
					int resourceuse = UseResourceNumber((int)item.Resource);
					//Console.WriteLine("resourceuse : {0}", resourceuse);
					
					if( check >= 23 && check <= 26 )
					{
						resourceuse += 7;
					}

					item.PrefixOption[41] = NewResourceOption[resourceuse, equipLine, 0];
					for( int i = 1; i < 5; ++i )
					{
						item.PrefixOption[41 + i] = NewResourceOption[resourceuse, equipLine, ( i * 2) -1];
						item.SuffixOption[41 + i] = NewResourceOption[resourceuse, equipLine, i * 2];
						if( item.PrefixOption[41 + i] != - 1 )
						{
							skilluse = NewEquipOptionList( equip, item.PrefixOption[ i + 41], item.SuffixOption[ i + 41], skilluse);
						}
					}


					if( item.SuffixOption[0] != 0 )
					{
						//재료 옵션
						//랜덤 옵션
						for( int i = 0; i < item.SuffixOption[0]; ++i)
						{
							if( !artifact )
							{
								item.PrefixOption[i + 11] = EquipOptionType[check][Utility.RandomMinMax(0, EquipOptionType[check].GetLength(0) -1)];

								if( i != 0 )
								{
									while(UniqueNumberCheck(item.PrefixOption[11], item.PrefixOption[12]))
									{
										item.PrefixOption[12] = EquipOptionType[check][Utility.RandomMinMax(0, EquipOptionType[check].GetLength(0) -1)];
									}
								}

								int score = NewEquipOption[item.PrefixOption[i + 11], equipLine, 1];
								
								if( score == 0 )
								{
									Console.WriteLine("Score Zero!!!. Count : {0}, item.PrefixOption[i + 11] : {1}", i + 1, item.PrefixOption[i + 11] );
									return;
								}
								
								int optionvalue = NewEquipOption[item.PrefixOption[i + 11],equipLine, 2];
								
								item.SuffixOption[i + 11] = item.PrefixOption[2] / score;
								if( item.SuffixOption[i + 11] * score < item.PrefixOption[2] )
								{
									item.SuffixOption[i + 11]++;
								}
								item.SuffixOption[i + 11] *= optionvalue;
								
								//Console.WriteLine("Item Number : {0}, Item Option : {1}, Item Value : {2}", i + 11, item.PrefixOption[i + 11], item.SuffixOption[i + 11] );
							}
							skilluse = NewEquipOptionList( equip, item.PrefixOption[ i + 11], item.SuffixOption[ i + 11], skilluse);
						}
					}

					#endregion
					item.SuffixOption[2] = NewUpgradeOptionStock[rank];
				}
			}
		}

		public static void NewItemDrop(Item make, Item newmake, PlayerMobile pm)
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
		
		public static Item Artifact_Select(Item item, int rank )
		{
			//유물 체크
			List <Type> artifactitemSelect = new List <Type>();
			Item artifactitem = null;
			switch(rank)
			{
				/*
				case 5:
				{
					for( int i = 0; i < Artifact_5Tier.Length; ++i )
					{
						if( Artifact_Search( item.GetType().BaseType, Artifact_5Tier[i].GetType().BaseType.BaseType )
						{
							artifactitemSelect.Add( Artifact_5Tier[i] );
						}
					}
					if( artifactitemSelect.Count > 0 )
					{
						artifactitem = (Item)Activator.CreateInstance(artifactitemSelect[Utility.Random(artifactitemSelect)]);
						break;
					}
					else
					{
						goto case 4:
					}
				}
				case 4:
				{
					for( int i = 0; i < Artifact_4Tier.Length; ++i )
					{
						if( Artifact_Search( item.GetType().BaseType, Artifact_4Tier[i].GetType().BaseType.BaseType )
						{
							artifactitemSelect.Add( Artifact_4Tier[i] );
						}
					}
					if( artifactitemSelect.Count > 0 )
					{
						artifactitem = (Item)Activator.CreateInstance(artifactitemSelect[Utility.Random(artifactitemSelect)]);
						break;
					}
					else
					{
						goto case 3:
					}
				}
				case 3:
				{
					for( int i = 0; i < Artifact_3Tier.Length; ++i )
					{
						if( Artifact_Search( item.GetType().BaseType, Artifact_3Tier[i].GetType().BaseType.BaseType )
						{
							artifactitemSelect.Add( Artifact_3Tier[i] );
						}
					}
					if( artifactitemSelect.Count > 0 )
					{
						artifactitem = (Item)Activator.CreateInstance(artifactitemSelect[Utility.Random(artifactitemSelect)]);
						break;
					}
					else
					{
						goto case 2:
					}
				}
				case 2:
				{
					for( int i = 0; i < Artifact_2Tier.Length; ++i )
					{
						if( Artifact_Search( item.GetType().BaseType, Artifact_2Tier[i].GetType().BaseType.BaseType )
						{
							artifactitemSelect.Add( Artifact_2Tier[i] );
						}
					}
					if( artifactitemSelect.Count > 0 )
					{
						artifactitem = (Item)Activator.CreateInstance(artifactitemSelect[Utility.Random(artifactitemSelect)]);
						break;
					}
					else
					{
						goto case 1:
					}
				}
				*/

				case 5:
					goto case 4;
				case 4:
					goto case 3;
				case 3:
					goto case 2;
				case 2:
					goto case 1;
				case 1:
				{
					for( int i = 0; i < Artifact_1Tier.Length; ++i )
					{
						if( item.GetType() == Artifact_1Tier[i].BaseType )
						{
							artifactitemSelect.Add( Artifact_1Tier[i] );
						}
					}
					if( artifactitemSelect.Count > 0 )
					{
						artifactitem = (Item)Activator.CreateInstance(artifactitemSelect[Utility.Random(artifactitemSelect.Count)]);
					}
					break;
				}
				
			}
			return artifactitem;
		}
		
		public static Type[] Artifact_1Tier = 
		{
			typeof( AdventurersMachete ), typeof( SilverEtchedMace ), typeof( Luckblade ), typeof( RubyMace ), typeof( TrueSpellblade ), typeof( EmeraldMace ), typeof( ArcanistsWildStaff ), typeof( AncientWildStaff ), typeof( IcySpellblade ), 
			typeof( FierySpellblade ), typeof( SpellbladeOfDefense ), typeof( TrueAssassinSpike ), typeof( ChargedAssassinSpike ), typeof( MagekillerAssassinSpike ), typeof( MagekillerLeafblade ), typeof( TrueLeafblade ), typeof( WoundingAssassinSpike ), typeof( LeafbladeOfEase ), typeof( ButchersWarCleaver ), 
			typeof( KnightsWarCleaver ), typeof( OrcishMachete ), typeof( SerratedWarCleaver ), typeof( TrueWarCleaver ), typeof( DiseasedMachete ), typeof( MacheteOfDefense ), typeof( MagesRuneBlade ), typeof( RuneBladeOfKnowledge ), typeof( Runesabre ), typeof( OrcishBow ), 
			typeof( DemonForks ), typeof( DragonNunchaku ), typeof( PeasantsBokuto ), typeof( PilferedDancerFans ), typeof( TomeOfEnlightenment ), typeof( TheDestroyer ), typeof( HanzosBow ), typeof( Exiler ), typeof( HailstormHuman ), typeof( AssassinsShortbow ), 
			typeof( AxeOfAbandon ), typeof( AxesOfFury ), typeof( BarbedLongbow ), typeof( BladeOfBattle ), typeof( CorruptedRuneBlade ), typeof( DarkglowScimitar ), typeof( EternalGuardianStaff ), typeof( HolySword ), typeof( IcyScimitar ), typeof( JadeWarAxe ), 
			typeof( LongbowOfMight ), typeof( MysticalShortbow ), typeof( PhantomStaff ), typeof( RangersShortbow ), typeof( SlayerLongbow ), typeof( ResonantStaffofEnlightenment ), typeof( RunedDriftwoodBow ), typeof( SingingAxe ), typeof( WindOfCorruption )
		};
				
		
		public static void NewItemCreate( Item item, int rank, PlayerMobile pm = null, bool artifact = false, bool Enhance = false )
		{
			bool onlyone = false;
			if( artifact )
			{
				Item artifactitem = Artifact_Select(item, rank);
				if( artifactitem != null )
				{
					if( item is BaseWeapon )
					{
						BaseWeapon make = item as BaseWeapon;
						if( make.PlayerConstructed && artifactitem is BaseWeapon )
						{
							BaseWeapon newmake = artifactitem as BaseWeapon;
							newmake.Quality = make.Quality;
							newmake.MaxHitPoints = make.MaxHitPoints;
							newmake.HitPoints = make.HitPoints;
							newmake.Crafter = make.Crafter;
							newmake.PlayerConstructed = make.PlayerConstructed;
							newmake.Hue = make.Hue;
							newmake.Resource = make.Resource;
						}
					}
					
					NewEquipOptionCreate( artifactitem, rank, onlyone, artifact );
					artifactitem.Map = item.Map;
					artifactitem.Location = item.Location;
					NewItemDrop(item, artifactitem, pm);
					
					item.Delete();
					return;
				}
			}
			#region 무기 설정
			if( item is BaseWeapon )
			{
				BaseWeapon make = item as BaseWeapon;

				if( make.Resource == CraftResource.Agapite || make.Resource == CraftResource.Heartwood || make.Resource == CraftResource.HornedLeather )
				{
					onlyone = true;
				}
				
				if( Enhance && rank <= 5 )
				{
					BaseWeapon newmake = Activator.CreateInstance(make.GetType()) as BaseWeapon;
					NewItemDrop(make, newmake, pm);
					rank = EnhanceCreate( rank );

					if( rank > 0 )
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);

					//옵션 전이
					newmake.Quality = make.Quality;
					newmake.MaxHitPoints = make.MaxHitPoints;
					newmake.HitPoints = make.HitPoints;
					newmake.Crafter = make.Crafter;
					newmake.EngravedText = make.EngravedText;
					newmake.PlayerConstructed = make.PlayerConstructed;
					newmake.Owner = make.Owner;
					newmake.Identified = make.Identified;
					newmake.IsImbued = make.IsImbued;
					newmake.Location = make.Location;
					newmake.ItemPower = make.ItemPower;
					newmake.Hue = make.Hue;
					newmake.Resource = make.Resource;
					make.Delete();
					NewEquipOptionCreate( newmake, rank, onlyone, artifact );
				}
				else
				{
					if( rank > 0 )
					{
						make.Identified = false;
						//rank = LegendAndMysticCheck( make, rank );
						NewEquipOptionCreate( make, rank, onlyone, artifact );
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);
					}
				}
			}
			#endregion
			#region 아머 설정
			else if( item is BaseArmor )
			{
				BaseArmor make = item as BaseArmor;

				if( make.Resource == CraftResource.Agapite || make.Resource == CraftResource.Heartwood || make.Resource == CraftResource.SpinedLeather )
				{
					onlyone = true;
				}

				if( Enhance && rank <= 5 )
				{
					BaseArmor newmake = Activator.CreateInstance(make.GetType()) as BaseArmor;
					NewItemDrop(make, newmake, pm);

					rank = EnhanceCreate( rank );
					if( rank > 0 )
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);

					//옵션 전이
					newmake.Quality = make.Quality;
					newmake.MaxHitPoints = make.MaxHitPoints;
					newmake.HitPoints = make.HitPoints;
					newmake.Crafter = make.Crafter;
					newmake.EngravedText = make.EngravedText;
					newmake.PlayerConstructed = make.PlayerConstructed;
					newmake.Owner = make.Owner;
					newmake.Identified = make.Identified;
					newmake.IsImbued = make.IsImbued;
					newmake.Location = make.Location;
					newmake.ItemPower = make.ItemPower;
					newmake.Hue = make.Hue;
					newmake.Resource = make.Resource;
					make.Delete();
					NewEquipOptionCreate( newmake, rank, onlyone, artifact );
				}
				else
				{
					make.Identified = false;

					if( rank > 0 )
					{
						make.Identified = false;
						//rank = LegendAndMysticCheck( make, rank );
						NewEquipOptionCreate( make, rank, onlyone, artifact );
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);
					}
				}
			}
			#endregion
			#region 천 설정
			else if( item is BaseClothing )
			{
				BaseClothing make = item as BaseClothing;
				if( Enhance && rank <= 5 )
				{
					BaseClothing newmake = Activator.CreateInstance(make.GetType()) as BaseClothing;
					NewItemDrop(make, newmake, pm);

					rank = EnhanceCreate( rank );
					if( rank > 0 )
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);

					//옵션 전이
					newmake.Quality = make.Quality;
					newmake.MaxHitPoints = make.MaxHitPoints;
					newmake.HitPoints = make.HitPoints;
					newmake.Crafter = make.Crafter;
					newmake.EngravedText = make.EngravedText;
					newmake.PlayerConstructed = make.PlayerConstructed;
					newmake.Owner = make.Owner;
					newmake.Identified = make.Identified;
					newmake.IsImbued = make.IsImbued;
					newmake.Location = make.Location;
					newmake.ItemPower = make.ItemPower;
					newmake.Hue = make.Hue;
					newmake.Resource = make.Resource;
					make.Delete();
					NewEquipOptionCreate( newmake, rank, onlyone, artifact );
				}
				else
				{
					if( rank > 0 )
					{
						make.Identified = false;
						if( !( make.Layer == Layer.Neck || make.Layer == Layer.Gloves || make.Layer == Layer.Arms || make.Layer == Layer.Helm || make.Layer == Layer.Pants || make.Layer == Layer.InnerTorso ) )
						{
							make.Identified = true;
							rank = 0;
						}
						//rank = LegendAndMysticCheck( make, rank );
						NewEquipOptionCreate( make, rank, onlyone, artifact );
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);
					}
				}
			}
			#endregion
			else if( item is BaseJewel )
			{
				BaseJewel make = item as BaseJewel;
				
				if( make.Resource == CraftResource.Bronze )
				{
					onlyone = true;
				}

				if( make.GemType != GemType.None )
				{
					int check = 0;
					make.PrefixOption[79] = 1;
					if( make.Layer == Layer.Ring )
						check = 1;
					else if( make.Layer == Layer.Neck )
						check = 2;
					else if( make.Layer == Layer.Earrings )
						check = 3;
					
					make.PrefixOption[81] = SelectGemOption[check, (int)make.GemType - 1];
				}
				
				if( Enhance && rank <= 5 )
				{
					BaseJewel newmake = Activator.CreateInstance(make.GetType()) as BaseJewel;
					NewItemDrop(make, newmake, pm);

					make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank);
					rank = EnhanceCreate( rank );
					if( rank > 0 )
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);

					//옵션 전이
					newmake.Quality = make.Quality;
					newmake.MaxHitPoints = make.MaxHitPoints;
					newmake.HitPoints = make.HitPoints;
					newmake.Crafter = make.Crafter;
					newmake.PlayerConstructed = make.PlayerConstructed;
					newmake.Owner = make.Owner;
					newmake.Identified = make.Identified;
					newmake.IsImbued = make.IsImbued;
					newmake.Location = make.Location;
					newmake.ItemPower = make.ItemPower;
					newmake.Hue = make.Hue;
					newmake.GemType = make.GemType;
					newmake.Resource = make.Resource;
					if( rank > 0 )
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);
					make.Delete();
					NewEquipOptionCreate( newmake, rank, onlyone, artifact );
				}
				else
				{
					if( rank > 0 )
					{
						make.Identified = false;
						//rank = LegendAndMysticCheck( make, rank );
						NewEquipOptionCreate( make, rank, onlyone, artifact );
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);
					}
				}
			}
			else if( item is Spellbook )
			{
				Spellbook make = item as Spellbook;

				if( Enhance && rank <= 5 )
				{
					Spellbook newmake = Activator.CreateInstance(make.GetType()) as Spellbook;
					NewItemDrop(make, newmake, pm);

					make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank);
					rank = EnhanceCreate( rank );
					if( rank > 0 )
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);

					//옵션 전이
					newmake.Quality = make.Quality;
					newmake.MaxHitPoints = make.MaxHitPoints;
					newmake.HitPoints = make.HitPoints;
					newmake.Crafter = make.Crafter;
					newmake.EngravedText = make.EngravedText;
					newmake.PlayerConstructed = make.PlayerConstructed;
					newmake.Owner = make.Owner;
					newmake.Identified = make.Identified;
					newmake.Location = make.Location;
					newmake.ItemPower = make.ItemPower;
					newmake.Hue = make.Hue;
					newmake.Resource = make.Resource;

					make.Delete();
					NewEquipOptionCreate( newmake, rank, onlyone, artifact );
				}
				else
				{
					if( rank > 0 )
					{
						make.Identified = false;
						//rank = LegendAndMysticCheck( make, rank );
						NewEquipOptionCreate( make, rank, onlyone, artifact );
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);
					}
				}
			}
		}
		
		#region 구 코드
		public static void EquipOptionCreate( Item equip, int rank, int tier, int uniqueoption, int pickup = -1 )
		{
			if( equip is IEquipOption )
			{
				IEquipOption item = equip as IEquipOption;
				int check = -1;
				bool optionPass = false;
				#region item type check
				#region Weapon
				if( item is BaseWeapon )
				{
					BaseWeapon newmake = item as BaseWeapon;
					if( newmake.Skill is SkillName.Swords )
					{
						if( newmake is BaseAxe )
							check = 2;
						else if( newmake.Layer == Layer.TwoHanded )
							check = 1;
						else if( newmake.Layer == Layer.OneHanded )
							check = 0;
					}
					else if( newmake.Skill is SkillName.Macing )
					{
						if( newmake.Layer == Layer.TwoHanded )
							check = 4;
						else if( newmake.Layer == Layer.OneHanded )
							check = 3;
					}
					else if( newmake.Skill is SkillName.Fencing )
					{
						if( newmake.Layer == Layer.TwoHanded )
							check = 6;
						else if( newmake.Layer == Layer.OneHanded )
							check = 5;
					}
					else if( newmake is BaseRanged )
					{
						if( ((BaseRanged)newmake).AmmoType == typeof(Bolt) )
							check = 8;
						else if( ((BaseRanged)newmake).AmmoType == typeof(Arrow) )
							check = 7;
					}
				}
				#endregion
				#region Armor
				else if( item is BaseArmor )
				{
					BaseArmor newmake = item as BaseArmor;
					/*
					아머타입 10 : 방패 //탱커
					아머타입 11 : 천옷 //법사, 힐러
					아머타입 12 : 가죽 //도적
					아머타입 13 : 스텃 //아쳐
					아머타입 14 : 뼈 //서포터
					아머타입 15 : 링, 투구, 스톤 //근접 딜러
					아머타입 16 : 체인, 스톤 //근접 딜러
					아머타입 17 : 플레이트 //탱커
					아머타입 18 : 나무 //서포터
					*/

					check = 1 + (int)newmake.MaterialType;
					if (newmake is BaseShield)
						check = 0;
					else if (check >= 5 && check <= 7)
						check = 2;
					else if (newmake is Helmet || newmake is Bascinet || newmake is CloseHelm || newmake is NorseHelm)
						check = 5;
					else if (check == 8)
						check = 5;
					else if (check == 9 || check == 13)
						check = 6;
					else if (check == 10)
						check = 7;
					else if (check == 12)
						check = 8;
					check += 10;
				}
				#endregion
				#region Clothing
				else if( item is BaseClothing )
				{
					BaseClothing newmake = item as BaseClothing;
					check = 11;
					if( newmake.Layer == Layer.Neck || newmake.Layer == Layer.Gloves || newmake.Layer == Layer.Arms || newmake.Layer == Layer.Helm || newmake.Layer == Layer.Pants || newmake.Layer == Layer.InnerTorso )
					{
						newmake.Identified = false;
					}
					else
					{
						if( item.PrefixOption[80] == 0 )
							newmake.Identified = true;
						optionPass = true;
					}
				}				
				#endregion
				#region Jewel
				else if(item is BaseJewel )
				{
					BaseJewel newmake = item as BaseJewel;
					/*
					악세타입 19 : 팔찌 
					아머타입 20 : 반지 
					아머타입 21 : 목걸이 
					아머타입 22 : 귀걸이 
					*/
					check = 19;
					if( newmake.Layer == Layer.Ring )
						check = 20;
					else if( newmake.Layer == Layer.Neck )
						check = 21;
					else if( newmake.Layer == Layer.Earrings )
						check = 22;
				
				}
				#endregion
				#region Spellbook
				else if( item is Spellbook )
				{
					check = 9;
				}
				#endregion
				#endregion

				int equipLine = -1;
				if( item is BaseWeapon || item is Spellbook )
					equipLine = 0;
				else if( item is BaseArmor || item is BaseClothing )
					equipLine = 1;
				else if( item is BaseJewel )
					equipLine = 2;
				
				if( check != -1 && equipLine != -1 )
				{
					AosAttributes primary = item.Attributes;
					AosWeaponAttributes weapon = item.WeaponAttributes;
					SAAbsorptionAttributes absorp = item.AbsorptionAttributes;
					ExtendedWeaponAttributes exweapon = item.ExtendedWeaponAttributes;
					AosSkillBonuses skill = item.SkillBonuses;
					AosArmorAttributes armor = item.ArmorAttributes;

					int loop = 0;
					
					if( !optionPass || item.PrefixOption[80] > 0 )
					{
						if( optionPass && item.PrefixOption[80] > 0 )
						{
							loop = item.PrefixOption[80];
							optionPass = false;
						}
						else
						{
							loop = OptionLoop();
							if( uniqueoption == 0 && !item.PlayerConstructed )
								loop++;

							if( item.PrefixOption[80] == 0 )
								loop++;
						}
					}

					int[] optionList = new int[loop];
					if( loop > 0 )
					{
						item.PrefixOption[0] = loop;
						item.PrefixOption[99] = tier;
						item.PrefixOption[98] = 1;
						for( int i = 0; i < loop; i++ )
						{
							optionList[i] = -1;
						}

						int doublecheck = 0;

						//신규
						if( item.PrefixOption[79] == 1 || item.PrefixOption[80] > 0 )
						{
							if( item.PrefixOption[79] == 1 )
								doublecheck = 1;
							else if( loop <= item.PrefixOption[80] )
							{
								//loop = item.PrefixOption[80];
								doublecheck = item.PrefixOption[80];
							}
							else if( loop > item.PrefixOption[80] )
								doublecheck = item.PrefixOption[80] + 2;

							for( int i = 0; i < doublecheck; ++i)
							{
								optionList[i] = item.PrefixOption[81 + i];
							}
						}
						#region Magic Option
						while( doublecheck < loop )
						{
							int dice = Utility.RandomMinMax(0, EquipOptionType[check].GetLength(0) -1);
							bool result = true;
							for( int j = 0; j < doublecheck; j++)
							{
								if( optionList[j] == EquipOptionType[check][dice] )
									result = false;
							}
							if ( result )
							{
								optionList[doublecheck] = EquipOptionType[check][dice];
								doublecheck++;
							}
						}
						Array.Sort(optionList);
					}
					int skilluse = 0;

					for( int i = 0; i < loop; i++ )
					{
						double mindice = EquipOption[optionList[i], equipLine, tier * 2 + 1];
						double maxdice = EquipOption[optionList[i], equipLine, tier * 2 + 2];
						
						double dice = NewItemDice( EquipOption[optionList[i], equipLine, tier * 2 + 1], EquipOption[optionList[i], equipLine, tier * 2 + 2] );
						
						if( rank > 1 && rank < 6 )
						{
							dice *= RankDice[rank - 2];
							mindice *= RankDice[rank - 2];
							maxdice *= RankDice[rank - 2];
						}
						
						item.PrefixOption[i * 4 + 1] = EquipOption[optionList[i], equipLine, 0];
						item.PrefixOption[i * 4 + 2] = (int)mindice;
						item.PrefixOption[i * 4 + 3] = (int)maxdice;
						item.PrefixOption[i * 4 + 4] = (int)dice;
					
						switch( optionList[i] )
						{
							case 0: //힘 증가
							{
								primary.BonusStr += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 1: //민첩 증가
							{
								primary.BonusDex += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 2: //지능 증가
							{
								primary.BonusInt += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 3: //운 증가
							{
								primary.Luck += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 4: //체력 증가
							{
								primary.BonusHits += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 5: //기력 증가
							{
								primary.BonusStam += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 6: //마나 증가
							{
								primary.BonusMana += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 7: //물리 피해 증가%
							{
								primary.WeaponDamage += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 8: //주문 피해 증가%
							{
								primary.SpellDamage += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 9: //관통 피해 증가%
							{
								absorp.ResonancePierce += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 10: //충격 피해 증가%
							{
								absorp.ResonanceKinetic += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 11: //출혈 피해 증가%
							{
								absorp.ResonanceBleed += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 12: //물리 저항%
							{
								weapon.ResistPhysicalBonus += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 13: //화염 저항%
							{
								weapon.ResistFireBonus += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 14: //냉기 저항%
							{
								weapon.ResistColdBonus += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 15: //독 저항%
							{
								weapon.ResistPoisonBonus += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 16: //에너지 저항%
							{
								weapon.ResistEnergyBonus += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 17: //명중률 증가%
							{
								primary.AttackChance += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 18: //방어율 증가%
							{
								primary.DefendChance += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 19: //체력 회복
							{
								primary.RegenHits += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 20: //기력 회복
							{
								primary.RegenStam += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 21: //마나 회복
							{
								primary.RegenMana += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 22: //물리 피해 증가%
							{
								primary.BalancedWeapon += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 23: //화염 피해 증가%
							{
								absorp.ResonanceFire += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 24: //냉기 피해 증가%
							{
								absorp.ResonanceCold += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 25: //독 피해 증가%
							{
								absorp.ResonancePoison += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 26: //에너지 피해 증가%
							{
								absorp.ResonanceEnergy += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 27: //광역 물리 피해 증가%
							{
								weapon.HitPhysicalArea += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 28: //광역 화염 피해 증가%
							{
								weapon.HitFireArea += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 29: //광역 냉기 피해 증가%
							{
								weapon.HitColdArea += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 30: //광역 독 피해 증가%
							{
								weapon.HitPoisonArea += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 31: //광역 에너지 피해 증가%
							{
								weapon.HitEnergyArea += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 32: //물리 피해 증가
							{
								absorp.EaterDamage += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 33: //화염 피해 증가
							{
								absorp.EaterFire += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 34: //냉기 피해 증가
							{
								absorp.EaterCold += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 35: //독 피해 증가
							{
								absorp.EaterPoison += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 36: //에너지 피해 증가
							{
								absorp.EaterEnergy += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 37: //체력 흡수
							{
								weapon.HitLeechHits += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 38: //기력 흡수
							{
								weapon.HitLeechStam += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 39: //마나 흡수
							{
								weapon.HitLeechMana += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 40: //공격 속도 증가
							{
								primary.WeaponSpeed += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 41: // 시전 속도 증가
							{
								primary.CastSpeed += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 42: //물리 치명타 확률 증가
							{
								primary.WeaponCritical += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 43: //마법 치명타 확률 증가
							{
								primary.CastRecovery += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 44: //물리 치명타 피해 증가
							{
								primary.Brittle += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 45: //마법 치명타 피해 증가
							{
								primary.SpellChanneling += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 46: //치유량 증가%
							{
								primary.EnhancePotions += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 47: //치유량 증가
							{
								primary.HealBonus += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 48: //관통 피해 증가
							{
								absorp.EaterPierce += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 49: //충격 피해 증가
							{
								absorp.EaterKinetic += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 50: //출혈 피해 증가
							{
								absorp.EaterBleed += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 51: //금화 획득 증가%
							{
								primary.NightSight += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 52: //화염 화살 공격%
							{
								weapon.HitMagicArrow += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 53: //체력 손상 공격%
							{
								weapon.HitHarm += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 54: //화염구 공격%
							{
								weapon.HitFireball += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 55: //번개 공격%
							{
								weapon.HitLightning += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 56: //영장류 피해 증가%
							{
								absorp.HumanoidDamage += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 57: //언데드 피해 증가%
							{
								absorp.UndeadDamage += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 58: //정령 피해 증가%
							{
								absorp.ElementalDamage += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 59: //곤충 피해 증가%
							{
								absorp.AbyssDamage += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 60: //파충류 피해 증가%
							{
								absorp.ArachnidDamage += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 61: //악마 피해 증가%
							{
								absorp.ReptilianDamage += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 62: //요정 피해 증가%
							{
								absorp.FeyDamage += item.PrefixOption[i * 4 + 4];
								break;
							}
							case 63: //해부학 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Anatomy, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 64: //동물지식 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.AnimalLore, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 65: //방패술 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Parry, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 66: //평화연주 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Peacemaking, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 67: //은신감지 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.DetectHidden, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 68: //불협화음 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Discordance, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 69: //지능평가 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.EvalInt, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 70: //회복술 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Healing, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 71: //법의학 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Forensics, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 72: //목동술 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Herding, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 73: //은신 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Hiding, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 74: //도발연주 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Provocation, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 75: //마법학 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Magery, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 76: //마법저항 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.MagicResist, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 77: //전술 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Tactics, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 78: //훔쳐보기 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Snooping, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 79: //음악연주 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Musicianship, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 80: //포이즈닝 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Poisoning, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 81: //궁술 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Archery, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 82: //영혼대화 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.SpiritSpeak, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 83: //훔치기 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Stealing, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 84: //길들이기 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.AnimalTaming, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 85: //반사신경 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Tracking, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 86: //수의학 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Veterinary, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 87: //검술 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Swords, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 88: //둔기술 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Macing, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 89: //펜싱 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Fencing, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 90: //명상 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Meditation, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 91: //은신이동 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Stealth, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 92: //강령술 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Necromancy, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 93: //집중 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Focus, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 94: //기사도 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Chivalry, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 95: //무사도 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Bushido, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 96: //암술 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Ninjitsu, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 97: //주문조합 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Spellweaving, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 98: //신비술 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Mysticism, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
							case 99: //던지기 스킬 증가%
							{
								skill.SetValues(skilluse, SkillName.Throwing, (double)(item.PrefixOption[i * 4 + 4] * 0.1 ));
								skilluse++;
								break;
							}
						}
					}
					#endregion

					#region Unique Option
					if( uniqueoption > 0 || item.PlayerConstructed )
					{
						item.SuffixOption[98] = 1;

						if( uniqueoption > 0 )
						{
							item.SuffixOption[99] = uniqueoption;
							switch ( uniqueoption )
							{
								case 1:
								{
									primary.RegenStam += 3;
									break;
								}
								case 2:
								{
									primary.RegenHits += 3;
									break;
								}
								case 3:
								{
									primary.BonusDex += 6;
									break;
								}
								case 4:
								{
									primary.BonusInt += 10;
									break;
								}
								case 5:
								{
									primary.BonusStam += 8;
									break;
								}
								case 6:
								{
									primary.BonusHits += 5;
									primary.Luck += 5;
									primary.NightSight += 20;
									break;
								}
								case 7:
								{
									primary.WeaponSpeed += 150;
									skill.SetValues(8, SkillName.Swords, 1.5);
									primary.BonusStr += 10;
									break;
								}
								case 8:
								{
									primary.WeaponSpeed += 250;
									skill.SetValues(8, SkillName.Swords, 2.5);
									primary.BonusStr += 20;
									break;
								}						
								case 9:
								{
									absorp.ResonancePoison += 70;
									break;
								}
								case 10:
								{
									absorp.ResonancePoison += 90;
									break;
								}
								case 11:
								{
									absorp.ResonancePoison += 130;
									break;
								}
								case 12:
								{
									skill.SetValues(8, SkillName.Hiding, 2.0);
									break;
								}
								case 13:
								{
									absorp.EaterBleed += 15;
									weapon.HitLeechStam += 5;
									break;
								}
								case 14:
								{
									absorp.ResonancePoison += 500;
									weapon.HitCurse += 250;
									primary.AttackChance += 500;
									break;
								}
								case 15:
								{
									primary.WeaponSpeed += 60;
									break;
								}
								case 16:
								{
									weapon.HitLeechHits += 5;
									break;
								}
								case 17:
								{
									weapon.ResistPhysicalBonus += 5;
									break;
								}
								case 18:
								{
									primary.EnhancePotions += 60;
									break;
								}
								case 19:
								{
									absorp.SoulChargeKinetic += 140;
									break;
								}
								case 20:
								{
									primary.HealBonus += 3;
									break;
								}
								case 21:
								{
									primary.BonusHits += 6;
									break;
								}
								case 22:
								{
									primary.BonusMana += 3;
									break;
								}
								case 23:
								{
									primary.BonusHits += 12;
									break;
								}
								case 24:
								{
									primary.SpellDamage += 450;
									primary.BonusInt += 400;
									primary.BonusMana += 500;
									break;
								}
								case 25:
								{
									primary.EnhancePotions += 110;
									break;
								}
								case 26:
								{
									absorp.ResonanceKinetic += 200;
									absorp.EaterKinetic += 3;
									weapon.HitHarm += 330;
									break;
								}
								case 27:
								{
									primary.Luck += 9;
									break;
								}
								case 28:
								{
									armor.PierceResist += 8;
									break;
								}
								case 29:
								{
									absorp.ResonancePierce += 250;
									absorp.EaterPierce += 5;
									primary.BonusDex += 30;
									break;
								}
								case 30:
								{
									primary.RegenHits += 9;
									break;
								}
								case 31:
								{
									absorp.ResonanceKinetic += 500;
									primary.WeaponDamage += 500;
									primary.Brittle += 500;
									break;
								}
								case 32:
								{
									absorp.ResonanceKinetic += 450;
									primary.WeaponDamage += 400;
									primary.WeaponSpeed += 300;
									break;
								}
								case 33:
								{
									primary.BonusStr += 50;
									primary.BonusDex += 30;
									absorp.EaterDamage += 10;
									break;
								}
								case 34:
								{
									primary.BonusStr += 50;
									break;
								}
								case 35:
								{
									primary.BonusHits += 22;
									break;
								}
								case 36:
								{
									primary.CastSpeed += 90;
									break;
								}
								case 37:
								{
									primary.HealBonus += 7;
									break;
								}
								case 38:
								{
									primary.LowerAmmoCost += 80;
									primary.WeaponSpeed += 350;
									primary.WeaponDamage += 350;
									break;
								}
								case 39:
								{
									primary.LowerAmmoCost += 80;
									primary.CastSpeed += 350;
									primary.SpellDamage += 350;
									break;
								}
								case 40:
								{
									primary.WeaponSpeed += 600;
									primary.BonusDex += 60;
									primary.WeaponDamage += 100;
									break;
								}
								case 41:
								{
									skill.SetValues(8, SkillName.Anatomy, 0.9);
									break;
								}
								case 42:					
								{
									primary.WeaponDamage += 150;
									primary.BonusStam += 20;
									break;
								}
								case 43:					
								{
									primary.SpellDamage += 150;
									primary.BonusMana += 20;
									break;
								}
								case 44:
								{
									skill.SetValues(8, SkillName.Anatomy, 5.0);
									break;
								}
								case 45:
								{
									weapon.HitCurse += 100;
									break;
								}
								case 46:
								{
									weapon.HitDispel += 150;
									primary.BonusDex += 60;
									primary.WeaponDamage += 100;
									break;
								}
								case 47:					
								{
									primary.CastRecovery += 200;
									primary.SpellChanneling += 300;
									break;
								}
								case 48:
								{
									skill.SetValues(8, SkillName.Necromancy, 2.5);
									break;
								}
								case 49:
								{
									primary.WeaponDamage += 750;
									absorp.UndeadDamage += 1500;
									primary.NightSight += 200;
									break;
								}
								case 50:
								{
									skill.SetValues(8, SkillName.Fencing, 2.5);
									absorp.ResonancePierce += 200;
									absorp.ResonancePoison += 200;
									break;
								}
								case 51:
								{
									absorp.ResonancePierce += 70;
									break;
								}
								case 52:
								{
									absorp.ResonanceFire += 70;
									break;
								}
								case 53:
								{
									absorp.ResonanceCold += 70;
									break;
								}
								case 54:
								{
									absorp.ResonanceEnergy += 70;
									break;
								}
								case 55:
								{
									primary.SpellDamage += 70;
									break;
								}
								case 56:
								{
									primary.SpellDamage += 250;
									break;
								}
								case 57:
								{
									absorp.ResonancePoison += 660;
									absorp.EaterPoison += 33;
									skill.SetValues(8, SkillName.Poisoning, 18.8);
									break;
								}
								case 58:
								{
									primary.BonusHits+= 200;
									primary.RegenHits += 28;
									weapon.HitLeechHits += 15;
									break;
								}
								case 59:
								{
									primary.SpellDamage += 500;
									break;
								}
								case 60:
								{
									primary.Luck += 5;
									break;
								}
								case 61:
								{
									skill.SetValues(8, SkillName.AnimalTaming, 0.6);
									break;
								}
								case 62:					
								{
									primary.Luck += 8;
									skill.SetValues(8, SkillName.AnimalTaming, 2.0);
									break;
								}
								case 63:
								{
									primary.Luck += 11;
									break;
								}
								case 64:
								{
									skill.SetValues(8, SkillName.AnimalTaming, 4.0);
									primary.Luck += 25;
									primary.NightSight += 50;
									break;
								}
								case 65:
								{
									absorp.ResonancePoison += 70;
									break;
								}
								case 66:
								{
									primary.WeaponDamage += 70;
									break;
								}
								case 67:
								{
									weapon.HitLeechHits += 15;
									break;
								}
								case 68:
								{
									primary.BonusMana += 50;
									break;
								}
								case 69:
								{
									primary.BonusMana += 20;
									break;
								}
								case 70:
								{
									primary.Luck += 35;
									break;
								}
								case 71:
								{
									primary.Luck += 40;
									break;
								}
								case 72:
								{
									skill.SetValues(8, SkillName.MagicResist, 10.0);
									weapon.ResistColdBonus += 15;
									break;
								}
								case 73:
								{
									absorp.ResonanceKinetic += 480;
									absorp.EaterKinetic += 9;
									weapon.HitLightning += 330;
									break;
								}
								case 74:
								{
									primary.WeaponSpeed += 100;
									break;
								}
								case 75:
								{
									primary.BonusStam += 8;
									break;
								}
								case 76:
								{
									primary.BonusStr += 44;
									primary.BonusDex += 38;
									primary.AttackChance += 300;
									break;
								}
								case 77:
								{
									primary.BonusInt += 46;
									primary.BonusHits += 120;
									primary.SpellDamage += 600;
									break;
								}
								case 78:
								{
									primary.BonusHits += 160;
									break;
								}
								case 79:
								{
									primary.BonusInt += 14;
									break;
								}
								case 80:
								{
									primary.AttackChance += 220;
									primary.WeaponCritical += 85;
									primary.Brittle += 625;
									break;
								}
								case 81:
								{
									primary.BonusHits += 222;
									break;
								}
								case 82:
								{
									primary.BonusDex += 31;
									break;
								}
							}
						}
						else if( !optionPass )
						{
							switch ( item.Resource )
							{
								case CraftResource.Iron:
								{
									if( item is BaseWeapon )
									{
										primary.LowerAmmoCost += 100;
										primary.BonusStr += 5 * ( tier + 1);
										primary.BonusDex += 5 * ( tier + 1);
									}
									else if( item is BaseArmor )
									{
										primary.LowerAmmoCost += 20;
									}
									else if( item is BaseJewel )
									{
										primary.LowerAmmoCost += 100;
										primary.BonusHits += 50 * ( tier + 1);
									}
									break;
								}
								case CraftResource.Copper:
								{
									if( item is BaseWeapon )
									{
										weapon.LowerStatReq += 200;
										primary.BonusDex += 5 * ( tier + 1);
										primary.WeaponSpeed += 50 * ( tier + 1);
									}
									else if( item is BaseArmor )
									{
										weapon.LowerStatReq += 200;
									}
									else if( item is BaseJewel )
									{
										primary.EnhancePotions += 50 * ( tier + 1);
										primary.BonusMana += 20 * ( tier + 1);
									}
									break;
								}
								case CraftResource.Bronze:
								{
									if( item is BaseWeapon )
									{
										exweapon.LightningBonus += 500;
										primary.RegenHits += 5 * ( tier + 1);
										primary.WeaponDamage += 50 * ( tier + 1);
									}
									else if( item is BaseArmor )
									{
										primary.WeaponDamage += 10 * ( tier + 1);
									}
									else if( item is BaseJewel )
									{
										absorp.ResonancePierce += 50 * ( tier + 1);
										absorp.ResonanceKinetic += 50 * ( tier + 1);
									}
									break;
								}
								case CraftResource.Gold:
								{
									if( item is BaseWeapon )
									{
										primary.Luck += 40;
										primary.BonusHits += 50 * ( tier + 1);
										primary.NightSight += 20 * ( tier + 1);
									}
									else if( item is BaseArmor )
									{
										primary.Luck += 10;
									}
									else if( item is BaseJewel )
									{
										primary.Luck += 40;
										primary.NightSight = 20 * ( tier + 1);
									}
									break;
								}
								case CraftResource.Agapite:
								{
									if( item is BaseWeapon )
									{
										BaseWeapon newmake = item as BaseWeapon;
										weapon.BloodDrinker += 1;
										absorp.ResonanceFire += 25 * ( tier + 1);
										newmake.AosElementDamages.Fire = 10 * ( tier + 1 );
									}
									else if( item is BaseArmor )
									{
										absorp.ResonanceFire += 5 * ( tier + 1);
									}
									else if( item is BaseJewel )
									{
										absorp.ResonanceFire += 50 * ( tier + 1);
										exweapon.HitExplosion += 50 * ( tier + 1);
									}
									break;
								}
								case CraftResource.Verite:
								{
									if( item is BaseWeapon )
									{
										BaseWeapon newmake = item as BaseWeapon;
										exweapon.InfectionBonus += 500;
										primary.Brittle += 125 * ( tier + 1);
										newmake.AosElementDamages.Poison = 10 * ( tier + 1 );
									}
									else if( item is BaseArmor )
									{
										primary.Brittle += 25 * ( tier + 1);
									}
									else if( item is BaseJewel )
									{
										absorp.ResonancePoison += 50 * ( tier + 1);
										exweapon.Bane += 50 * ( tier + 1);
									}
									break;
								}
								case CraftResource.Valorite:
								{
									if( item is BaseWeapon )
									{
										weapon.MageWeapon += 200;
										weapon.UseBestSkill += 50;
										weapon.HitLeechHits += 5 * ( tier + 1);
									}
									else if( item is BaseArmor )
									{
										weapon.HitLeechHits += tier + 1;
									}
									else if( item is BaseJewel )
									{
										absorp.ResonanceCold += 50 * ( tier + 1);
										exweapon.Freezing += 50 * ( tier + 1);
									}
									break;
								}
								case CraftResource.RegularWood:
								{
									if( item is BaseWeapon )
									{
										primary.LowerAmmoCost += 100;
										primary.BonusDex += 5 * ( tier + 1);
										primary.BonusStam += 20 * ( tier + 1);
									}
									else if( item is BaseArmor )
									{
										primary.LowerAmmoCost += 20;
									}
									break;
								}
								case CraftResource.OakWood:
								{
									if( item is BaseWeapon )
									{
										primary.WeaponDamage += 50 * ( tier + 1);
										primary.Luck += 40;
										primary.NightSight += 20 * ( tier + 1);
									}
									else if( item is BaseArmor )
									{
										weapon.LowerStatReq += 200;
									}
									break;
								}
								case CraftResource.AshWood:
								{
									if( item is BaseWeapon )
									{
										weapon.LowerStatReq += 200;
										absorp.HumanoidDamage += 50 * ( tier + 1);
										primary.WeaponSpeed += 50 * ( tier + 1);
									}
									else if( item is BaseArmor )
									{
										primary.NightSight += 4 * ( tier + 1);
									}
									break;
								}
								case CraftResource.YewWood:
								{
									if( item is BaseWeapon )
									{
										primary.AttackChance += 50 * ( tier + 1 );
										primary.WeaponDamage += 50 * ( tier + 1);
										primary.BonusHits += 50 * ( tier + 1);
									}
									else if( item is BaseArmor )
									{
										primary.AttackChance += 10 * ( tier + 1 );
									}
									break;
								}
								case CraftResource.Heartwood:
								{
									if( item is BaseWeapon )
									{
										primary.RegenStam += 5 * ( tier + 1);
										weapon.MageWeapon += 200;
										primary.Brittle += 125 * ( tier + 1);
									}
									else if( item is BaseArmor )
									{
										primary.RegenStam += tier + 1;
									}
									break;
								}
								case CraftResource.Bloodwood:
								{
									if( item is BaseWeapon )
									{
										absorp.SoulChargeKinetic += 500; //회복량%
										primary.RegenHits += 5 * ( tier + 1);
										primary.BonusHits += 50 * ( tier + 1);
									}
									else if( item is BaseArmor )
									{
										primary.RegenHits += tier + 1;
									}
									break;
								}
								case CraftResource.Frostwood:
								{
									if( item is BaseWeapon )
									{
										BaseWeapon newmake = item as BaseWeapon;
										weapon.UseBestSkill += 50;
										absorp.ResonanceCold += 25 * ( tier + 1);
										newmake.AosElementDamages.Cold = 10 * ( tier + 1 );
									}
									else if( item is BaseArmor )
									{
										absorp.ResonanceCold += 5 * ( tier + 1);
									}
									break;
								}
								case CraftResource.RegularLeather:
								{
									if( item is BaseArmor || item is BaseClothing )
										primary.LowerAmmoCost += 20;
									else if ( item is Spellbook )
									{
										primary.LowerAmmoCost += 100;
										primary.BonusInt += 5 * ( tier + 1);
										primary.BonusMana += 20 * ( tier + 1);
									}
									break;
								}
								case CraftResource.DernedLeather:
								{
									if( item is BaseArmor || item is BaseClothing )
										primary.BonusMana += 4 * ( tier + 1);
									else if ( item is Spellbook )
									{
										weapon.LowerStatReq += 200;
										primary.BonusHits += 50 * ( tier + 1);
										primary.BonusMana += 20 * ( tier + 1);
									}
									break;
								}
								case CraftResource.RatnedLeather:
								{
									if( item is BaseArmor || item is BaseClothing )
										primary.SpellDamage += 10 * ( tier + 1);
									else if ( item is Spellbook )
									{
										primary.BonusInt += 5 * ( tier + 1);
										primary.CastSpeed += 50 * ( tier + 1);
										primary.SpellDamage += 50 * ( tier + 1);
									}
									break;
								}
								case CraftResource.SernedLeather:
								{
									if( item is BaseArmor || item is BaseClothing )
										primary.EnhancePotions += 10 * ( tier + 1 );
									else if ( item is Spellbook )
									{
										absorp.CastingFocus += 100; //피해의 10%만큼 마나 회복
										primary.EnhancePotions += 50 * ( tier + 1 );
										primary.RegenMana += 5 * ( tier + 1);
									}
									break;
								}
								case CraftResource.SpinedLeather:
								{
									if( item is BaseArmor || item is BaseClothing )
										primary.CastSpeed += 10 * ( tier + 1);
									else if ( item is Spellbook )
									{
										primary.Luck += 40;
										primary.NightSight += 20 * ( tier + 1);
										primary.BonusMana += 20 * ( tier + 1);
										
									}
									break;
								}
								case CraftResource.HornedLeather:
								{
									if( item is BaseArmor || item is BaseClothing )
										primary.SpellChanneling += 25 * ( tier + 1);
									else if ( item is Spellbook )
									{
										weapon.MageWeapon += 200;
										weapon.UseBestSkill += 50;
										primary.SpellChanneling += 125 * ( tier + 1);
									}
									break;
								}
								case CraftResource.BarbedLeather:
								{
									if( item is BaseArmor || item is BaseClothing )
										weapon.HitLeechMana += tier + 1;
									else if ( item is Spellbook )
									{
										weapon.HitCurse += 200;
										primary.CastSpeed += 50 * ( tier + 1);
										weapon.HitLeechMana += 5 * ( tier + 1);
									}
									break;
								}
								/*
								case CraftResource.Mythril: 1079978
								{








								}
								case CraftResource.Obsidian: 1072846
								{

								}
								*/

								
							}
						}
					}
					#endregion
				}
				equip.InvalidateProperties();

			}
		}

		public static double[] Enhance_RankUpgrade =
		{
			100.0, 20.0, 2.5, 0.1, 0.001, 0.000
		};
		
		public static int EnhanceCreate( int rank )
		{
			if( rank > 5 )
				return rank;
			
			if( Utility.RandomDouble() < ( Enhance_RankUpgrade[rank] * 0.01 ) )
				rank++;
				
			return rank;
		}		
		
		public static void ItemCreate( Item item, int rank, bool craft, PlayerMobile pm = null, int tier = 0, int uniqueoption = 0, bool Enhance = false )
		{
			#region 무기 설정
			if( item is BaseWeapon )
			{
				BaseWeapon make = item as BaseWeapon;

				if( Enhance && rank <= 5 )
				{
					BaseWeapon newmake = Activator.CreateInstance(make.GetType()) as BaseWeapon;
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
					rank = EnhanceCreate( rank );
					if( rank > 0 )
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);

					//옵션 전이
					newmake.Quality = make.Quality;
					newmake.MaxHitPoints = make.MaxHitPoints;
					newmake.HitPoints = make.HitPoints;
					newmake.Crafter = make.Crafter;
					newmake.EngravedText = make.EngravedText;
					newmake.PlayerConstructed = make.PlayerConstructed;
					newmake.Owner = make.Owner;
					newmake.Identified = make.Identified;
					newmake.IsImbued = make.IsImbued;
					newmake.Location = make.Location;
					newmake.ItemPower = make.ItemPower;
					newmake.Hue = make.Hue;
					newmake.Resource = make.Resource;
					make.Delete();
					EquipOptionCreate( newmake, rank, tier, uniqueoption );
				}
				else
				{
					if( rank > 0 )
					{
						make.Identified = false;
						//rank = LegendAndMysticCheck( make, rank );
						EquipOptionCreate( make, rank, tier, uniqueoption );
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);
					}
				}
			}
			#endregion
			#region 아머 설정
			else if( item is BaseArmor )
			{
				BaseArmor make = item as BaseArmor;
				if( Enhance && rank <= 5 )
				{
					BaseArmor newmake = Activator.CreateInstance(make.GetType()) as BaseArmor;
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

					rank = EnhanceCreate( rank );
					if( rank > 0 )
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);

					//옵션 전이
					newmake.Quality = make.Quality;
					newmake.MaxHitPoints = make.MaxHitPoints;
					newmake.HitPoints = make.HitPoints;
					newmake.Crafter = make.Crafter;
					newmake.EngravedText = make.EngravedText;
					newmake.PlayerConstructed = make.PlayerConstructed;
					newmake.Owner = make.Owner;
					newmake.Identified = make.Identified;
					newmake.IsImbued = make.IsImbued;
					newmake.Location = make.Location;
					newmake.ItemPower = make.ItemPower;
					newmake.Hue = make.Hue;
					newmake.Resource = make.Resource;
					make.Delete();
					EquipOptionCreate( newmake, rank, tier, uniqueoption );
				}
				else
				{
					make.Identified = false;

					if( rank > 0 )
					{
						make.Identified = false;
						//rank = LegendAndMysticCheck( make, rank );
						EquipOptionCreate( make, rank, tier, uniqueoption );
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);
					}
				}
			}
			#endregion
			#region 천 설정
			else if( item is BaseClothing )
			{
				BaseClothing make = item as BaseClothing;
				if( Enhance && rank <= 5 )
				{
					BaseClothing newmake = Activator.CreateInstance(make.GetType()) as BaseClothing;
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

					rank = EnhanceCreate( rank );
					if( rank > 0 )
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);

					//옵션 전이
					newmake.Quality = make.Quality;
					newmake.MaxHitPoints = make.MaxHitPoints;
					newmake.HitPoints = make.HitPoints;
					newmake.Crafter = make.Crafter;
					newmake.EngravedText = make.EngravedText;
					newmake.PlayerConstructed = make.PlayerConstructed;
					newmake.Owner = make.Owner;
					newmake.Identified = make.Identified;
					newmake.IsImbued = make.IsImbued;
					newmake.Location = make.Location;
					newmake.ItemPower = make.ItemPower;
					newmake.Hue = make.Hue;
					newmake.Resource = make.Resource;
					make.Delete();
					EquipOptionCreate( newmake, rank, tier, uniqueoption );
				}
				else
				{
					if( rank > 0 )
					{
						make.Identified = false;
						if( !( make.Layer == Layer.Neck || make.Layer == Layer.Gloves || make.Layer == Layer.Arms || make.Layer == Layer.Helm || make.Layer == Layer.Pants || make.Layer == Layer.InnerTorso ) && uniqueoption == 0 && make.PrefixOption[80] == 0 )
						{
							make.Identified = true;
							rank = 0;
							tier = 0;
						}
						//rank = LegendAndMysticCheck( make, rank );
						EquipOptionCreate( make, rank, tier, uniqueoption );
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);
					}
				}
			}
			#endregion
			else if( item is BaseJewel )
			{
				BaseJewel make = item as BaseJewel;
				
				if( make.GemType != GemType.None )
				{
					int check = 0;
					make.PrefixOption[79] = 1;
					if( make.Layer == Layer.Ring )
						check = 1;
					else if( make.Layer == Layer.Neck )
						check = 2;
					else if( make.Layer == Layer.Earrings )
						check = 3;
					
					make.PrefixOption[81] = SelectGemOption[check, (int)make.GemType - 1];
				}
				
				if( Enhance && rank <= 5 )
				{
					BaseJewel newmake = Activator.CreateInstance(make.GetType()) as BaseJewel;
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

					make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank);
					rank = EnhanceCreate( rank );
					if( rank > 0 )
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);

					//옵션 전이
					newmake.Quality = make.Quality;
					newmake.MaxHitPoints = make.MaxHitPoints;
					newmake.HitPoints = make.HitPoints;
					newmake.Crafter = make.Crafter;
					newmake.PlayerConstructed = make.PlayerConstructed;
					newmake.Owner = make.Owner;
					newmake.Identified = make.Identified;
					newmake.IsImbued = make.IsImbued;
					newmake.Location = make.Location;
					newmake.ItemPower = make.ItemPower;
					newmake.Hue = make.Hue;
					newmake.GemType = make.GemType;
					newmake.Resource = make.Resource;
					if( rank > 0 )
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);
					make.Delete();
					EquipOptionCreate( newmake, rank, tier, uniqueoption );
				}
				else
				{
					if( rank > 0 )
					{
						make.Identified = false;
						//rank = LegendAndMysticCheck( make, rank );
						EquipOptionCreate( make, rank, tier, uniqueoption );
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);
					}
				}
			}
			else if( item is Spellbook )
			{
				Spellbook make = item as Spellbook;

				if( Enhance && rank <= 5 )
				{
					Spellbook newmake = Activator.CreateInstance(make.GetType()) as Spellbook;
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

					make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank);
					rank = EnhanceCreate( rank );
					if( rank > 0 )
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);

					//옵션 전이
					newmake.Quality = make.Quality;
					newmake.MaxHitPoints = make.MaxHitPoints;
					newmake.HitPoints = make.HitPoints;
					newmake.Crafter = make.Crafter;
					newmake.EngravedText = make.EngravedText;
					newmake.PlayerConstructed = make.PlayerConstructed;
					newmake.Owner = make.Owner;
					newmake.Identified = make.Identified;
					newmake.Location = make.Location;
					newmake.ItemPower = make.ItemPower;
					newmake.Hue = make.Hue;
					newmake.Resource = make.Resource;

					make.Delete();
					EquipOptionCreate( newmake, rank, tier, uniqueoption );
				}
				else
				{
					if( rank > 0 )
					{
						make.Identified = false;
						//rank = LegendAndMysticCheck( make, rank );
						EquipOptionCreate( make, rank, tier, uniqueoption );
						make.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank + 3);
					}
				}
			}
		}
		#endregion

		/*
		public static void CraftRank( Mobile from, Item craftitem, int resource, int excep )
		{
			resource += 400;
			int armslore = (int)from.Skills.ArmsLore.Value * 5;
			if( armslore >= 100 )
				armslore += 150;
			int min = resource + armslore + excep;
			int max = resource * 2 + armslore;
			if( min > max )
			{
				int delta = max;
				max = min;
				min = delta;
			}
			int quality = Utility.RandomMinMax( min, max );

			if( craftitem is BaseWeapon )
			{
				BaseWeapon hidden = craftitem as BaseWeapon;
				hidden.HiddenRank = quality;
			}
			else if( craftitem is BaseArmor )
			{
				BaseArmor hidden = craftitem as BaseArmor;
				hidden.HiddenRank = quality;
			}

			int rank = quality / 1000;
			quality -= rank * 1000;

			if( quality > 0 && Utility.Random( quality ) > 1000 )
				rank++;

			if( rank > 5 )
				rank = 5;

			from.CheckSkill( SkillName.ArmsLore, 200 + rank * 50 );

			if( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				ItemCreate( craftitem, rank, true, pm );
			}
		}
		*/
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
		
		//보핑주금빨 -> 녹파보금핑
		public static string ItemRank( int name )
		{
			string colorname = "{0}\t{1}";
			switch( name )
			{
				case 4 : colorname = "<basefont color=#00A000>{0}\t{1}<basefont color=#FFFFFF>";
				break;
				case 5 : colorname = "<basefont color=#68D5ED>{0}\t{1}<basefont color=#FFFFFF>";
				break;
				case 6 : colorname = "<basefont color=#B36BFF>{0}\t{1}<basefont color=#FFFFFF>";
				break;
				case 7 : colorname = "<basefont color=#FFB400>{0}\t{1}<basefont color=#FFFFFF>";
				break;
				case 8 : colorname = "<basefont color=#FF0090>{0}\t{1}<basefont color=#FFFFFF>";
				break;
			}
			return colorname;
		}

		public static string OneItemRank( int name )
		{
			string colorname = "";
			switch( name )
			{
				case 4 : colorname = "<basefont color=#00A000>{0}\t#{1}\t{2}<basefont color=#FFFFFF>";
				break;
				case 5 : colorname = "<basefont color=#68D5ED>{0}\t#{1}\t{2}<basefont color=#FFFFFF>";
				break;
				case 6 : colorname = "<basefont color=#B36BFF>{0}\t#{1}\t{2}<basefont color=#FFFFFF>";
				break;
				case 7 : colorname = "<basefont color=#FFB400>{0}\t#{1}\t{2}<basefont color=#FFFFFF>";
				break;
				case 8 : colorname = "<basefont color=#FF0090>{0}\t#{1}\t{2}<basefont color=#FFFFFF>";
				break;
			}
			return colorname;
		}

		public static string AllItemRank( int name )
		{
			string colorname = "";
			switch( name )
			{
				case 4 : colorname = "<basefont color=#00A000>{0}\t#{1}\t#{2}\t{3}<basefont color=#FFFFFF>";
				break;
				case 5 : colorname = "<basefont color=#68D5ED>{0}\t#{1}\t#{2}\t{3}<basefont color=#FFFFFF>";
				break;
				case 6 : colorname = "<basefont color=#B36BFF>{0}\t#{1}\t#{2}\t{3}<basefont color=#FFFFFF>";
				break;
				case 7 : colorname = "<basefont color=#FFB400>{0}\t#{1}\t#{2}\t{3}<basefont color=#FFFFFF>";
				break;
				case 8 : colorname = "<basefont color=#FF0090>{0}\t#{1}\t#{2}\t{3}<basefont color=#FFFFFF>";
				break;
			}
			return colorname;
		}

		public static string OreItemRank( int name )
		{
			string colorname = "{0}\t#{1}\t{2}";
			switch( name )
			{
				case 4 : colorname = "<basefont color=#00A000>{0}\t#{1}\t{2}<basefont color=#FFFFFF>";
				break;
				case 5 : colorname = "<basefont color=#68D5ED>{0}\t#{1}\t{2}<basefont color=#FFFFFF>";
				break;
				case 6 : colorname = "<basefont color=#B36BFF>{0}\t#{1}\t{2}<basefont color=#FFFFFF>";
				break;
				case 7 : colorname = "<basefont color=#FFB400>{0}\t#{1}\t{2}<basefont color=#FFFFFF>";
				break;
				case 8 : colorname = "<basefont color=#FF0090>{0}\t#{1}\t{2}<basefont color=#FFFFFF>";
				break;
			}
			return colorname;
		}

		public static string OreOneItemRank( int name )
		{
			string colorname = "";
			switch( name )
			{
				case 4 : colorname = "<basefont color=#00A000>{0}\t#{1}\t#{2}\t{3}<basefont color=#FFFFFF>";
				break;
				case 5 : colorname = "<basefont color=#68D5ED>{0}\t#{1}\t#{2}\t{3}<basefont color=#FFFFFF>";
				break;
				case 6 : colorname = "<basefont color=#B36BFF>{0}\t#{1}\t#{2}\t{3}<basefont color=#FFFFFF>";
				break;
				case 7 : colorname = "<basefont color=#FFB400>{0}\t#{1}\t#{2}\t{3}<basefont color=#FFFFFF>";
				break;
				case 8 : colorname = "<basefont color=#FF0090>{0}\t#{1}\t#{2}\t{3}<basefont color=#FFFFFF>";
				break;
			}
			return colorname;
		}

		public static string OreAllItemRank( int name )
		{
			string colorname = "";
			switch( name )
			{
				case 4 : colorname = "<basefont color=#00A000>{0}\t#{1}\t#{2}\t#{3}\t{4}<basefont color=#FFFFFF>";
				break;
				case 5 : colorname = "<basefont color=#68D5ED>{0}\t#{1}\t#{2}\t#{3}\t{4}<basefont color=#FFFFFF>";
				break;
				case 6 : colorname = "<basefont color=#B36BFF>{0}\t#{1}\t#{2}\t#{3}\t{4}<basefont color=#FFFFFF>";
				break;
				case 7 : colorname = "<basefont color=#FFB400>{0}\t#{1}\t#{2}\t#{3}\t{4}<basefont color=#FFFFFF>";
				break;
				case 8 : colorname = "<basefont color=#FF0090>{0}\t#{1}\t#{2}\t#{3}\t{4}<basefont color=#FFFFFF>";
				break;
			}
			return colorname;
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

		//슬레이어 몬스터 체크
		public static bool SlayerCheck(SlayerName name, Mobile defender)
		{
            SlayerEntry atkSlayer = SlayerGroup.GetEntryByName(name);
			if( atkSlayer.Slays(defender) )
			{
				defender.FixedEffect(0x37B9, 10, 5);
				return true;
			}
			return false;
		}

		public static int[] SlayerCheck(Mobile target)
		{
			int[] set_array = { -1, -1 };
			int count = 0;

			SlayerEntry atkSlayer = SlayerGroup.GetEntryByName(SlayerName.Repond);
			if( atkSlayer.Slays(target) )
			{
				set_array[count] = 0;
				count++;
			}
			atkSlayer = SlayerGroup.GetEntryByName(SlayerName.Silver);
			if( atkSlayer.Slays(target) )
			{
				set_array[count] = 1;
				count++;
			}
			atkSlayer = SlayerGroup.GetEntryByName(SlayerName.ElementalBan);
			if( count < 2 && atkSlayer.Slays(target) )
			{
				set_array[count] = 2;
				count++;
			}
			atkSlayer = SlayerGroup.GetEntryByName(SlayerName.Exorcism);
			if( count < 2 && atkSlayer.Slays(target) )
			{
				set_array[count] = 3;
				count++;
			}
			atkSlayer = SlayerGroup.GetEntryByName(SlayerName.ArachnidDoom);
			if( count < 2 && atkSlayer.Slays(target) )
			{
				set_array[count] = 4;
				count++;
			}
			atkSlayer = SlayerGroup.GetEntryByName(SlayerName.ReptilianDeath);
			if( count < 2 && atkSlayer.Slays(target) )
			{
				set_array[count] = 5;
				count++;
			}
			atkSlayer = SlayerGroup.GetEntryByName(SlayerName.Fey);
			if( count < 2 && atkSlayer.Slays(target) )
			{
				set_array[count] = 6;
				count++;
			}
			return set_array;
		}

		public static int ExpHarvestBonus( PlayerMobile pm, int maxchance )
		{
			maxchance *= 100 + pm.GoldPoint[3];
			maxchance /= 100;
			return maxchance;
		}

		public static int HealCheck( Mobile from, Mobile to, int skillbonus, int heal )
		{
			//실버 포인트 15 치유량, 16 회복량%
			//AOS EnhancePotions : 치유량%, HealBonus : 치유량+, SoulChargeKinetic : 회복량%, SoulCharge : 회복량+
			int Percent = AosAttributes.GetValue(from, AosAttribute.EnhancePotions) + SAAbsorptionAttributes.GetValue(to, SAAbsorptionAttribute.SoulChargeKinetic) + skillbonus;
			int Plus = AosAttributes.GetValue(from, AosAttribute.HealBonus) + AosArmorAttributes.GetValue(to, AosArmorAttribute.SoulCharge );
			if( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				Percent += pm.SilverPoint[15] * 50;
			}
			if( to is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				Percent += pm.SilverPoint[16] * 50;
			}
			//총합 계산
			heal *= 1000 + Percent;
			heal /= 1000;
			heal += Plus;
			
			if( to.Hits + heal > to.HitsMax )
				heal = to.HitsMax - to.Hits;
			
            Map map = to.Map;

			int range = 20;
			
            IPooledEnumerable eable = to.Map.GetMobilesInRange(to.Location, range);
            //List<Mobile> AggroCheck = new List<Mobile>();

            foreach (Mobile m in eable)
            {
                if (m is BaseCreature)
                {
					BaseCreature bc = m as BaseCreature;
					if( bc != null && bc.SummonMaster == null && bc.ControlMaster == null && bc.AggroMobile != null )
					{
						for( int i = 0; i < bc.AggroMobile.Length; ++i )
						{
							if( bc.AggroMobile[i] == to )
							{
								double aggrobonus = 1;
								double aggrominus = 1;
								if( from is PlayerMobile )
								{
									PlayerMobile pm = from as PlayerMobile;
									aggrobonus += pm.SilverPoint[3] * 0.005;
									aggrominus -= pm.SilverPoint[4] * 0.0025;
								}							
								int realaggro = (int)( ( heal * 100 * aggrobonus / 100 ) / aggrominus);
								bc.AggroScore[i] += realaggro;
								break;
							}
						}
					}
                    //if (CanBeHarmful(m, false) && first.CanBeHarmful(m, false))
                    //    AggroCheck.Add(m);
                }
            }
            eable.Free();

            //Mobile t = null;

            //if (possibles.Count > 0)
            //    t = possibles[Utility.Random(possibles.Count)];

            //ColUtility.Free(possibles);

			
			return heal;
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
	}
}
