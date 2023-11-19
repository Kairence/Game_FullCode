using System; 
using System.Collections;
using Server.Mobiles; 

namespace Server.Items
{ 
	public class Seeding : Item
	{ 
		private static Mobile m_Sower;
		public Timer thisTimer;
		private Point3D m_Location;
		private Map m_Map;
		private Item m_Seed;
		private int m_GrawUp;
		private int m_Crop;

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Sower{ get{ return m_Sower; } set{ m_Sower = value; } }
		
		[CommandProperty( AccessLevel.GameMaster )]
		public int GrawUp{ get{ return m_GrawUp; } set{ m_GrawUp = value; } }

		[CommandProperty( AccessLevel.GameMaster )]
		public int Crop{ get{ return m_Crop; } set{ m_Crop = value; } }

		[Constructable] 
		public Seeding( Mobile sower, Item seed, Point3D location, Map map ) : base( 0xCB5 ) 
		{ 
			Movable = false;
			m_Location = location;
			m_Map = map;
			m_Seed = seed;
			m_Sower = sower;
			
			init( m_Seed );
		}

		[Constructable]
		public Seeding( int amount ) : base( 0xCB5 )
		{
		}
		
		[Constructable]
		public Seeding() : this( 1 )
		{
		}
		
		public void init( Item plant )
		{
			if( plant is CabbageSeed )
			{
				m_Crop = 1;
				ItemID = 0xC61;
				Name = "성장중인 양배추";
			}
			else if( plant is CarrotSeed )
			{
				m_Crop = 2;
				ItemID = 0xC69;
				Name = "성장중인 당근";
			}
			else if( plant is CornSeed )
			{
				m_Crop = 3;
				ItemID = 0xC7E;
				Name = "성장중인 옥수수";
			}
			else if( plant is CottonSeed )
			{
				m_Crop = 4;
				ItemID = Utility.RandomList( 0xC53, 0xC54 );
				Name = "성장중인 목화";
			}
			else if( plant is LettuceSeed )
			{
				m_Crop = 5;
				ItemID = 0xC61;
				Name = "성장중인 상추";
			}
			else if( plant is OnionSeed )
			{
				m_Crop = 6;
				ItemID = 0xC69;
				Name = "성장중인 양파";
			}
			else if( plant is PumpkinSeed )
			{
				m_Crop = 7;
				ItemID = 0xC6B;
				Name = "성장중인 호박";
			}
			else if( plant is TurnipSeed )
			{
				m_Crop = 8;
				ItemID = 0xC61;
			}
			else if( plant is WheatSeed )
			{
				m_Crop = 9;
				ItemID = Utility.RandomList( 0xC55, 0xC56, 0xC57, 0xC59 );
				Name = "성장중인 밀";
			}
			
			if( m_Crop != 0 )
			{
				thisTimer = new GrowTimer( this, m_Crop ); 
				thisTimer.Start(); 
			}
		}

		public override void OnDoubleClick( Mobile from ) 
		{
			if( m_Crop == 0 )
			{
				from.SendMessage( "잘못된 작물입니다." );
				this.Delete();
			}
			else if( GrawUp < 200 )
				from.SendMessage( "아직 성장중입니다!" );
			else
				HarvestBegin( from, m_Crop );
		}

		private void HarvestBegin( Mobile from, int crop )
		{
			if ( m_Sower == null || m_Sower.Deleted ) 
				m_Sower = from;

			if ( from.AccessLevel < AccessLevel.GameMaster && m_Sower != from ) 
				return;

			if ( from.Mounted )
			{
				from.SendMessage( "말을 탄 상태에선 작물을 수확할 수 없습니다!" ); 
				return; 
			}
			if ( from.InRange( this.GetWorldLocation(), 2 ) && from == m_Sower ) 
			{
				from.Direction = from.GetDirectionTo( this );
				from.Animate( from.Mounted ? 29:32, 5, 1, true, false, 0 ); 

				int pick = PicksCheck( from );

				if( m_Crop == 1 )
				{
					Cabbage harvest = new Cabbage( pick ); 
					from.AddToBackpack( harvest );
				}
				else if( m_Crop == 2 )
				{
					Carrot harvest = new Carrot( pick ); 
					from.AddToBackpack( harvest );
				}
				else if( m_Crop == 3 )
				{
					Corn harvest = new Corn( pick ); 
					from.AddToBackpack( harvest );
				}
				else if( m_Crop == 4 )
				{
					Cotton harvest = new Cotton( pick ); 
					from.AddToBackpack( harvest );
				}
				else if( m_Crop == 5 )
				{
					Lettuce harvest = new Lettuce( pick ); 
					from.AddToBackpack( harvest );
				}
				else if( m_Crop == 6 )
				{
					Onion harvest = new Onion( pick ); 
					from.AddToBackpack( harvest );
				}
				else if( m_Crop == 7 )
				{
					Pumpkin harvest = new Pumpkin( pick ); 
					from.AddToBackpack( harvest );
				}
				else if( m_Crop == 8 )
				{
					Turnip harvest = new Turnip( pick ); 
					from.AddToBackpack( harvest );
				}
				else if( m_Crop == 9 )
				{
					Wheat harvest = new Wheat( pick ); 
					from.AddToBackpack( harvest );
				}

				if( pick != 0 )
					from.SendMessage( "당신은 {0}개를 수확합니다!", pick ); 
				else
					from.SendMessage( "흉년입니다..." ); 
				thisTimer.Stop();
				this.Delete();
			}
			else 
				from.SendMessage( "더 가까이 붙어야 합니다!" ); 
		}
		public int PicksCheck( Mobile from )
		{
			int result = Utility.Random( 3, 5 );
			double Event = 1.0;
			Event ev = new Event();
			if ( ev.ServerEvent == 1 )
				result += Utility.Random( 1, 2 );
			
			int roll = Utility.Random( 1000 );

			if ( roll < 300 )
				result = (int)( result * Utility.RandomMinMax( 0, 25 ) * 0.01 );
			else if( roll > 940 )
				result = (int)( result * Utility.RandomMinMax( 110, 125 ) * 0.01 );

			if ( result < 0 )
				result = 0;

			return result;
		}		
		public class GrowTimer : Timer 
		{
			private Item m_Seeding;
			private int m_Crop;
			public GrowTimer( Item seeding, int crop ) : base( TimeSpan.FromMinutes( 10 ), TimeSpan.FromMinutes( 10 ) ) 
			{
				m_Seeding = seeding;
				m_Crop = crop;
			} 

			protected override void OnTick() 
			{
				((Seeding)m_Seeding).GrawUp += Utility.RandomMinMax( 1, 2 );
				if( ((Seeding)m_Seeding).GrawUp >= 500 )
				{
					((Item)m_Seeding).Delete();
					Stop();
				}
				if( ((Seeding)m_Seeding).GrawUp >= 200 )
				{
					if( m_Crop == 1 )
					{
						((Item)m_Seeding).ItemID = 0xC7C;
						((Item)m_Seeding).Name = "성장 완료된 양배추"; 
					}
					else if( m_Crop == 2 )
					{
						((Item)m_Seeding).ItemID = 0xC76;
						((Item)m_Seeding).Name = "성장 완료된 당근"; 
					}
					else if( m_Crop == 3 )
					{
						((Item)m_Seeding).ItemID = 0xC7D;
						((Item)m_Seeding).Name = "성장 완료된 옥수수"; 
					}
					else if( m_Crop == 4 )
					{
						((Item)m_Seeding).ItemID = Utility.RandomList( 0xC4F, 0xC50 );
						((Item)m_Seeding).Name = "성장 완료된 목화"; 
					}
					else if( m_Crop == 5 )
					{
						((Item)m_Seeding).ItemID = 0xC70;
						((Item)m_Seeding).Name = "성장 완료된 상추"; 
					}
					else if( m_Crop == 6 )
					{
						((Item)m_Seeding).ItemID = 0xC6F;
						((Item)m_Seeding).Name = "성장 완료된 양파"; 
					}
					else if( m_Crop == 7 )
					{
						((Item)m_Seeding).ItemID = 0xC6A;
						((Item)m_Seeding).Name = "성장 완료된 호박"; 
					}
					else if( m_Crop == 8 )
					{
						((Item)m_Seeding).ItemID = 0xC62;
						((Item)m_Seeding).Name = "성장 완료된 순무"; 
					}
					else if( m_Crop == 9 )
					{
						((Item)m_Seeding).ItemID = Utility.RandomList( 0xC58, 0xC5A, 0xC5B );
						((Item)m_Seeding).Name = "성장 완료된 밀"; 
					}
				}
			} 
		} 		
		
		public Seeding( Serial serial ) : base( serial ) 
		{ 
		} 

		public override void Serialize( GenericWriter writer ) 
		{ 
			base.Serialize( writer ); 
			writer.Write( (int) 1 );
			writer.Write( m_Crop );
			writer.Write( m_Sower );
			writer.Write( m_GrawUp );
		} 

		public override void Deserialize( GenericReader reader ) 
		{
			base.Deserialize( reader ); 
			int version = reader.ReadInt(); 
			switch ( version )
			{
				case 1: 
				{
					m_Crop = reader.ReadInt();
					goto case 0;
				}
				case 0:
				{
					m_Sower = reader.ReadMobile();
					m_GrawUp = reader.ReadInt();
					break;
				}
			}

			init( this );
		} 
	}
}
