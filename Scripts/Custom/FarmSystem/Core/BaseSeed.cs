using System;
using System.Collections;
using Server.Mobiles; 
using Server.Gumps;

namespace Server.Items
{ 
	public class BaseSeed : Item 
	{
		[Constructable]
		public BaseSeed() : this( 1 )
		{
		}

		[Constructable]
		public BaseSeed( int amount ) : base( 0xDCF )
		{
			Stackable = true; 
			Weight = .1; 

			Movable = true; 
			
			//Amount = amount;
		}

		public override void OnDoubleClick( Mobile from ) 
		{ 
			if ( from.Mounted )
			{
				from.SendMessage( "당신은 탈것 위에서 씨앗을 심을 수 없습니다!" ); 
				return; 
			}
			else if ( !IsChildOf( from.Backpack ) ) 
			{ 
				from.SendLocalizedMessage( 1042010 ); //You must have the object in your backpack to use it. 
				return; 
			}
			else
			{
				PlayerMobile pm = from as PlayerMobile;
				if( pm.FarmTime < DateTime.Now )
				{
					from.SendMessage( "밭 대여기간이 아닙니다!" ); 
					return; 
				}
				else if( pm.PlayerPoint < 0 )
				{
					from.SendMessage( "활동 포인트가 없습니다!" ); 
					return; 
				}
				else if( !CropHelper.FarmLocation( from, pm.FarmCheck, this ) )
				{
					//from.SendMessage( "농사 허용 지역이 아닙니다!"); 
					return;
				}
				Point3D m_pnt = from.Location;
				m_pnt.Z += 1;
				Map m_map = from.Map;
				ArrayList cropshere = CropHelper.CheckCrop( m_pnt, m_map, 1 );
				if ( cropshere.Count > 0 )
				{
					from.SendMessage( "여기엔 벌써 작물이 자라고 있습니다." ); 
					return;
				}
				else
				{
					++m_pnt.Z;

					from.Animate( 32, 5, 1, true, false, 0 ); // Bow
					pm.PlayerPoint--;
					from.SendMessage("당신은 작물을 심었습니다."); 
					this.Consume();

					Item seeding = Activator.CreateInstance( typeof( Seeding ), from, this, m_pnt, m_map ) as Item;

					if ( seeding != null )
					{
						seeding.Location = from.Location; 
						seeding.Map = from.Map; 
					}
				}
			}
		}
		public BaseSeed( Serial serial ) : base( serial ) 
		{ 
		} 

		public override void Serialize( GenericWriter writer ) 
		{ 
			base.Serialize( writer ); 
			writer.Write( (int) 0 ); 
		} 

		public override void Deserialize( GenericReader reader ) 
		{ 
			base.Deserialize( reader ); 
			int version = reader.ReadInt(); 
		} 
	} 
}
