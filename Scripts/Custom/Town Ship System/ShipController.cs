using System;
using Server.Gumps;
using Server.Items;
using System.Collections;
using Server.ContextMenus;
using System.Collections.Generic; 
namespace Server.Mobiles
{
	[CorpseName( "a humans corpse" )]
	public class ShipController : BaseCreature
	{
        public virtual bool IsInvulnerable{ get{ return true; } }
		private int m_ShipCheck;
		public int ShipTarget = 0;
		[CommandProperty( AccessLevel.GameMaster )]
		public int ShipCheck
		{
			get{ return m_ShipCheck;}
			set{ m_ShipCheck = value; InvalidateProperties();}
		}
		[Constructable]
		public ShipController() : base( AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2 )
		{
			if ( this.Female = Utility.RandomBool() )
			{
				Body = 0x191;
				Name = NameList.RandomName( "female" );
			}
			else
			{
				Body = 0x190;
				Name = NameList.RandomName( "male" );
			}

			//Title = "- 양배추, 순무";
			
			Direction = Direction.Down;
			Frozen = true;

			AddItem( new Server.Items.WideBrimHat( Utility.RandomNeutralHue() ) );
			AddItem( new ThighBoots() );
			AddItem( new Doublet() );
			AddItem( new LongPants() );

			Hue = Utility.RandomSkinHue();
			RangeHome = 0;
			LocationCheck();
		}
		
		private void LocationCheck()
		{
			if( m_ShipCheck == 0 )
			{
				ShipLocation( this.Location.X, this.Location.Y );
				Timer.DelayCall( TimeSpan.FromSeconds( 10 ), new TimerCallback( LocationCheck ) );
			}
		}

		int[] townname = { 1041189, 1041183, 1041181, 1041186, 1041188, 1078098, 1041184, 1041180, 1041177, 1041178 };
		//string[] townname = {"브리튼", "부케니어스 덴", "젤롬", "마진시아", "문글로우", "뉴헤븐", "서펜트 홀드", "스카라 브레", "트린식", "베스퍼"};
		
		public ShipController( Serial serial ) : base( serial )
		{
		}
        public override void GetContextMenuEntries( Mobile from, List<ContextMenuEntry> list ) 
        { 
            base.GetContextMenuEntries( from, list );
			switch ( m_ShipCheck )
			{
				case 1:
				{
					list.Add(new ShipEntry( from, this, 6, 1500, townname[2] ) );
					list.Add(new ShipEntry( from, this, 10, 1000, townname[6] ) );
					list.Add(new ShipEntry( from, this, 12, 1000, townname[8] ) );
					break;
				}
				case 2:
				{
					list.Add(new ShipEntry( from, this, 3, 750, townname[1] ) );
					list.Add(new ShipEntry( from, this, 7, 1000, townname[4] ) );
					list.Add(new ShipEntry( from, this, 13, 1000, townname[9] ) );
					break;
				}
				case 3:
				{
					list.Add(new ShipEntry( from, this, 2, 750, townname[0] ) );
					list.Add(new ShipEntry( from, this, 7, 500, townname[4] ) );
					list.Add(new ShipEntry( from, this, 13, 750, townname[9] ) );
					break;
				}
				case 4:
				{
					list.Add(new ShipEntry( from, this, 11, 1000, townname[7] ) );
					break;
				}
				case 5:
				{
					list.Add(new ShipEntry( from, this, 9, 1500, townname[5] ) );
					list.Add(new ShipEntry( from, this, 10, 1000, townname[6] ) );
					list.Add(new ShipEntry( from, this, 13, 2500, townname[9] ) );
					break;
				}
				case 6:
				{
					list.Add(new ShipEntry( from, this, 1, 1500, townname[0] ) );
					list.Add(new ShipEntry( from, this, 12, 1000, townname[8] ) );
					break;
				}
				case 7:
				{
					list.Add(new ShipEntry( from, this, 2, 1000, townname[0] ) );
					list.Add(new ShipEntry( from, this, 3, 500, townname[1] ) );
					list.Add(new ShipEntry( from, this, 8, 500, townname[4] ) );
					list.Add(new ShipEntry( from, this, 9, 250, townname[5] ) );
					list.Add(new ShipEntry( from, this, 12, 1000, townname[8] ) );
					list.Add(new ShipEntry( from, this, 13, 1000, townname[9] ) );
					break;
				}
				case 8:
				{
					list.Add(new ShipEntry( from, this, 7, 500, townname[3] ) );
					list.Add(new ShipEntry( from, this, 13, 1000, townname[9] ) );
					break;
				}
				case 9:
				{
					list.Add(new ShipEntry( from, this, 5, 1500, townname[2] ) );
					list.Add(new ShipEntry( from, this, 7, 250, townname[3] ) );
					break;
				}
				case 10:
				{
					list.Add(new ShipEntry( from, this, 1, 1000, townname[0] ) );
					list.Add(new ShipEntry( from, this, 5, 1000, townname[2] ) );
					break;
				}
				case 11:
				{
					list.Add(new ShipEntry( from, this, 4, 1000, townname[2] ) );
					break;
				}
				case 12:
				{
					list.Add(new ShipEntry( from, this, 1, 1000, townname[0] ) );
					list.Add(new ShipEntry( from, this, 6, 1000, townname[2] ) );
					list.Add(new ShipEntry( from, this, 7, 1000, townname[3] ) );
					list.Add(new ShipEntry( from, this, 13, 1500, townname[9] ) );
					break;
				}
				case 13:
				{
					list.Add(new ShipEntry( from, this, 2, 1000, townname[0] ) );
					list.Add(new ShipEntry( from, this, 3, 750, townname[1] ) );
					list.Add(new ShipEntry( from, this, 6, 2500, townname[2] ) );
					list.Add(new ShipEntry( from, this, 7, 1000, townname[3] ) );
					list.Add(new ShipEntry( from, this, 8, 1000, townname[4] ) );
					list.Add(new ShipEntry( from, this, 12, 1500, townname[8] ) );
					break;
				}
			}
	    }
		
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
			writer.Write( (int) m_ShipCheck );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			m_ShipCheck = reader.ReadInt();
		}

		public class ShipEntry : ContextMenuEntry
		{
			private Mobile m_Mobile;
			private ShipController m_Ship;
			private int m_Gold;
			private int m_Town;
			
			public ShipEntry( Mobile from, ShipController ship, int town, int gold, int name ) : base( name )
			{
				m_Mobile = from;
				m_Ship = ship;
				m_Gold = gold;
				m_Town = town;
			}

			public override void OnClick()
			{
				if( m_Mobile is PlayerMobile )
				{
					string talk = "이 배를 타기 위해서는 "+ m_Gold.ToString() + "골드가 필요합니다";
					m_Ship.Say( talk );
					PlayerMobile pm = m_Mobile as PlayerMobile;
					pm.ShipUse = m_Gold;
					m_Ship.ShipTarget = m_Town;
				}
				else
					return;
			}
		}

		public override bool OnDragDrop( Mobile from, Item dropped )
		{
			if( m_ShipCheck == 0 )
				Say( "선박 세팅중입니다." );
			else if( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;

				if( !( dropped is Gold ) )
					this.SayTo( from, "골드만 사용 가능합니다." );
				else if( pm.ShipUse == 0 )
					this.SayTo( from, "이동할 곳을 먼저 선택하세요.");
				else if( pm.ShipUse > dropped.Amount )
					this.SayTo( from, "돈이 부족합니다." );
				else
				{
					if( dropped.Amount > pm.ShipUse )
						from.AddToBackpack( new Gold( dropped.Amount - pm.ShipUse ) );
						
					pm.TimerList[68] = ( pm.ShipUse / 250 ) * 600;
					pm.ShipCheck = this.ShipTarget;
					dropped.Delete();
					BaseCreature.TeleportPets(from, new Point3D( 6994, 1534, 18 ), Map.Trammel, true);
					pm.MoveToWorld( new Point3D( 6994, 1534, 18 ), Map.Trammel );
					return true;
				}
			}
			return false;
		}

		private void ShipLocation( int x, int y )
		{
			if( x == 1449 && y == 1766 )
				m_ShipCheck = 1;
			else if( x == 1487 && y == 1760 )
				m_ShipCheck = 2;
			else if( x == 2751 && y == 2159 )
				m_ShipCheck = 3;
			else if( x == 1132 && y == 3690 )
				m_ShipCheck = 4;
			else if( x == 1519 && y == 3990 )
				m_ShipCheck = 5;
			else if( x == 1497 && y == 3708 )
				m_ShipCheck = 6;
			else if( x == 3708 && y == 2295 )
				m_ShipCheck = 7;
			else if( x == 4425 && y == 1025 )
				m_ShipCheck = 8;
			else if( x == 3520 && y == 2594 ) //
				m_ShipCheck = 9;
			else if( x == 2941 && y == 3412 ) //
				m_ShipCheck = 10;
			else if( x == 643 && y == 2243 ) //
				m_ShipCheck = 11;
			else if( x == 2085 && y == 2855 ) //
				m_ShipCheck = 12;
			else if( x == 3045 && y == 828 ) //
				m_ShipCheck = 13;

			Title = ShipTitle( m_ShipCheck );
		}

		private string ShipTitle( int ship )
		{
			switch ( ship )
			{
				case 1 : return "- 브리튼 서쪽 부두"; break;
				case 2 : return "- 브리튼 동쪽 부두"; break;
				case 3 : return "- 부케니어스 덴 부두"; break;
				case 4 : return "- 젤롬 북쪽 부두"; break;
				case 5 : return "- 젤롬 남쪽 부두"; break;
				case 6 : return "- 젤롬 중앙 부두"; break;
				case 7 : return "- 마진시아 부두"; break;
				case 8 : return "- 문글로우 부두"; break;
				case 9 : return "- 뉴 헤븐 부두"; break;
				case 10 : return "- 서펜트 홀드 부두"; break;
				case 11 : return "- 스카라 브레 부두"; break;
				case 12 : return "- 트린식 부두"; break;
				case 13 : return "- 베스퍼 부두"; break;
				default : return "";
			}
		}
	}
}
				


				


