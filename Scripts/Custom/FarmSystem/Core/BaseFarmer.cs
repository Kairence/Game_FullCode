using System;
using Server.Gumps;
using Server.Items;
using System.Collections;
using Server.ContextMenus;
using System.Collections.Generic; 
namespace Server.Mobiles
{
	[CorpseName( "a humans corpse" )]
	public class PlayerFarmer : BaseCreature
	{
        public virtual bool IsInvulnerable{ get{ return true; } }
		private int m_FarmCheck;
		[CommandProperty( AccessLevel.GameMaster )]
		public int FarmCheck
		{
			get{ return m_FarmCheck;}
			set{ m_FarmCheck = value; InvalidateProperties();}
		}
		[Constructable]
		public PlayerFarmer() : base( AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2 )
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
			if( m_FarmCheck == 0 )
			{
				FarmerLocation( this.Location.X, this.Location.Y );
				Timer.DelayCall( TimeSpan.FromSeconds( 10 ), new TimerCallback( LocationCheck ) );
			}
		}

		public PlayerFarmer( Serial serial ) : base( serial )
		{
		}
        public override void GetContextMenuEntries( Mobile from, List<ContextMenuEntry> list ) 
        { 
            base.GetContextMenuEntries( from, list ); 
  	        list.Add( new FarmerEntry( from, this, FarmUse( m_FarmCheck ) ) ); 
	    } 

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
			writer.Write( (int) m_FarmCheck );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			m_FarmCheck = reader.ReadInt();
		}

		public class FarmerEntry : ContextMenuEntry
		{
			private Mobile m_Mobile;
			private Mobile m_Farmer;
			private int m_Farm;
			
			public FarmerEntry( Mobile from, Mobile farmer, int farm ) : base( 6146, 3 )
			{
				m_Mobile = from;
				m_Farmer = farmer;
				m_Farm = farm;
			}

			public override void OnClick()
			{
				if( m_Mobile is PlayerMobile )
				{
					string talk = "이 밭을 빌리기 위해서는 "+ m_Farm.ToString()+" 골드와 500점의 활동포인트가 필요합니다. 대여 기일은 3일입니다.";
					m_Farmer.Say( talk );
				}
				else
					return;
			}
		}

		//private int FarmUse = 38400;
		
		private void FarmerLocation( int x, int y )
		{
			if( x == 2214 && y == 1160 )
				m_FarmCheck = 1;
			else if( x == 4567 && y == 1475 )
				m_FarmCheck = 2;
			else if( x == 4567 && y == 1477 )
				m_FarmCheck = 3;
			else if( x == 4418 && y == 1446 )
				m_FarmCheck = 4;
			else if( x == 4632 && y == 1301 )
				m_FarmCheck = 5;
			else if( x == 4632 && y == 1302 )
				m_FarmCheck = 6;
			else if( x == 4635 && y == 1301 )
				m_FarmCheck = 7;
			else if( x == 4635 && y == 1302 )
				m_FarmCheck = 8;
			else if( x == 1208 && y == 1670 ) //
				m_FarmCheck = 9;
			else if( x == 1240 && y == 1710 ) //
				m_FarmCheck = 10;
			else if( x == 1240 && y == 1590 ) //
				m_FarmCheck = 11;
			else if( x == 1168 && y == 1558 ) // 밀
				m_FarmCheck = 12;
			else if( x == 1136 && y == 1606 ) // 밀
				m_FarmCheck = 13;
			else if( x == 1150 && y == 1774 ) // 밀
				m_FarmCheck = 14;
			else if( x == 1214 && y == 1806 ) // 밀
				m_FarmCheck = 15;
			else if( x == 1246 && y == 1870 ) // 밀
				m_FarmCheck = 16;
			else if( x == 574 && y == 1230 ) // 밀
				m_FarmCheck = 17;
			else if( x == 382 && y == 1174 ) // 밀
				m_FarmCheck = 18;
			else if( x == 830 && y == 2150 ) // 밀
				m_FarmCheck = 19;
			else if( x == 830 && y == 2249 ) // 밀
				m_FarmCheck = 20;
			else if( x == 830 && y == 2342 ) // 밀
				m_FarmCheck = 21;
			else if( x == 834 && y == 2342 ) // 밀
				m_FarmCheck = 22;

			Title = FarmTitle( m_FarmCheck );
		}

		private string FarmTitle( int farm )
		{
			switch ( farm )
			{
				case 1 : return "- 모든 작물"; break;
				case 2 : return "- 목화(서)"; break;
				case 3 : return "- 목화(동)"; break;
				case 4 : return "- 밀"; break;
				case 5 : return "- 밀(북서)"; break;
				case 6 : return "- 밀(남서)"; break;
				case 7 : return "- 밀(북동)"; break;
				case 8 : return "- 밀(남동)"; break;
				case 9 : return "- 양배추, 순무"; break;
				case 10 : return "- 당근, 양파"; break;
				case 11 : return "- 순무, 당근"; break;
				case 12 : return "- 밀"; break;
				case 13 : return "- 밀"; break;
				case 14 : return "- 밀"; break;
				case 15 : return "- 밀"; break;
				case 16 : return "- 밀"; break;
				case 17 : return "- 밀"; break;
				case 18 : return "- 밀"; break;
				case 19 : return "- 모든 작물"; break;
				case 20 : return "- 당근, 양파, 양배추, 순무"; break;
				case 21 : return "- 목화"; break;
				case 22 : return "- 호박"; break;
				default : return "";
			}
		}

		private int FarmUse( int farm )
		{
			if( farm == 1 )
				return 10240;
			else if( farm == 2 || farm == 3 ) //문글로우 소형 농장
				return 1800;
			else if( farm == 4 ) //문글로우 중형 농장
				return 3000;
			else if ( farm == 5 || farm == 6 ) //문글로우 소형 농장
				return 1280;
			else if( farm == 7 || farm == 8 ) //문글로우 소형 농장
				return 1008;
			else if( farm >= 9 && farm <= 11 ) //브리튼 중형 농장
				return 38400;
			else if( farm >= 12 && farm <= 16 ) //브리튼 대형 농장
				return 51200;
			else if( farm == 17 ) //유 중형 농장
				return 8960;
			else if( farm == 18 ) //유 대형 농장
				return 19200;
			else if( farm == 19 ) //스카라 브레 대형 농장
				return 51840;
			else if( farm == 20 ) //스카라 브레 소형 농장
				return 20480;
			else if( farm == 21 ) //스카라 브레 중형 농장
				return 14592;
			else if( farm == 22 ) //스카라 브레 소형 농장
				return 9520;
			else
				return 0;
		}
		
		public override bool OnDragDrop( Mobile from, Item dropped )
		{
			if( m_FarmCheck == 0 )
				Say( "농장 세팅중입니다." );
			else if( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;

				if( pm.FarmTime > DateTime.Now )
					Say( "당신은 이미 다른 밭을 대여중이네요." );
				else if( this.FarmTime > DateTime.Now )
					Say( "이미 이 밭은 누군가 사용중입니다." );
				else if( pm.FarmCheck != 0 )
					Say( "당신은 이번 주에 농장 대여를 마쳤습니다." );
				else if( FarmUse( m_FarmCheck ) > dropped.Amount )
					this.SayTo( from, "돈이 부족합니다." );
				else if( pm.PlayerPoint < 500 )
					this.SayTo( from, "활동 포인트가 부족합니다." );
				else
				{
					if ( dropped is Gold )
					{
						if( dropped.Amount > FarmUse( m_FarmCheck ) )
							from.AddToBackpack( new Gold( dropped.Amount - FarmUse( m_FarmCheck ) ) );
							
						this.Say( pm.Name +"님은 3일 동안 이 밭을 사용할 수 있습니다." );
						pm.FarmTime = DateTime.Now + TimeSpan.FromMinutes( 4320 );
						pm.FarmCheck = m_FarmCheck;
						pm.PlayerPoint -= 1000;
						dropped.Delete();
						return true;
					}
					else if ( dropped is BankCheck )
						this.SayTo( from, "수표는 받지 않습니다." );
					else
						this.SayTo( from, "이건 흥미 없어요." );
				}
			}
			return false;
		}
	}
}
				


				


