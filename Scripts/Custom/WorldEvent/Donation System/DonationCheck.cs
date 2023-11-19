using System;
using Server.Items;
using Server.Gumps;
using Server.Network;
using Server.Mobiles;
using Server.Accounting;

namespace Server.Items
{
	/*
		최초 100 유저
	*/
	
	public class DonationCheck : Item 
	{
		private DateTime m_RespawnTime = DateTime.Now;
		
		public DateTime RespawnTime
		{
			get{ return m_RespawnTime;}
			set{ m_RespawnTime = value; InvalidateProperties();}
		}
		
		private string[] m_DonationList = new string[1000];
		public string[] DonationList
		{
			get{ return m_DonationList;}
			set{ m_DonationList = value; InvalidateProperties();}
		}

		public override string DefaultName
		{
			get { return "기부 체크 시스템"; }
		}
		[Constructable]
		public DonationCheck() : base( 0xED4 )
		{
			Movable = false;
			Hue = 1121;
			Name = "기부 체크 시스템";
		}
		
		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( "다음 기부 시간 : {0}", m_RespawnTime.ToString());
			try
			{
				for( int i = 0; i < 10; i++ )
				{
					if( DonationList[i] == "" || DonationList[i] == null )
						break;
					else
					{
						foreach (Account a in Accounts.GetAccounts())
						{
							if( DonationList[i] == a.Username )
							{
								from.SendMessage( "{0}의 기부 포인트 {1}", DonationList[i], a.DonationPoint);
							}
						}
					}
				}
			}
			catch
			{
			}
		}

		public DonationCheck( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( (DateTime) m_RespawnTime );
			
			for( int i = 0; i < 1000; i++ )
			{
				writer.Write( (string) m_DonationList[i] );
			}
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			m_RespawnTime = reader.ReadDateTime();
			
			for( int i = 0; i < 1000; i++ )
			{
				m_DonationList[i] = reader.ReadString();
			}
		}
	}
}
