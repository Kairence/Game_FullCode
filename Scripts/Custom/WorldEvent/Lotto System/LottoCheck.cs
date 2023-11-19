using System;
using Server.Items;
using Server.Gumps;
using Server.Network;
using Server.Mobiles;
using Server.Accounting;

namespace Server.Items
{
	public class LottoCheck : Item 
	{
		private DateTime m_RespawnTime = DateTime.Now;
		
		public DateTime RespawnTime
		{
			get{ return m_RespawnTime;}
			set{ m_RespawnTime = value; InvalidateProperties();}
		}
		
		private int m_LottoNumber;
		public int LottoNumber
		{
			get{ return m_LottoNumber;}
			set{ m_LottoNumber = value; InvalidateProperties();}
		}
		
		public override string DefaultName
		{
			get { return "로또 체크 시스템"; }
		}
		[Constructable]
		public LottoCheck() : base( 0xED4 )
		{
			Movable = false;
			Hue = 1123;
			Name = "로또 체크 시스템";
		}
		
		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( "다음 추첨 시간 : {0}", m_RespawnTime.ToString());
			from.SendMessage( "로또 번호 :{0}", m_LottoNumber);
		}

		public LottoCheck( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( (DateTime) m_RespawnTime );
			writer.Write( (int) m_LottoNumber );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			m_RespawnTime = reader.ReadDateTime();
			m_LottoNumber = reader.ReadInt();
		}
	}
}
